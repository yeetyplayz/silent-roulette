using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks each player's loan/chance state.
/// Handles automatic top-up, debt repayment on win, and spectator state.
/// Attach to the same GameObject as BettingManager.
/// </summary>
public class BrokePlayerManager : MonoBehaviour
{
    [Header("Loan Amounts (Chance 1, 2, 3)")]
    public float[] loanAmounts = { 500f, 250f, 100f };

    [Header("References")]
    public BettingManager bettingManager;
    public BrokePlayerUI brokePlayerUI;

    // Tracks each player's current chance (0 = not in debt, 1/2/3 = on that chance)
    private Dictionary<PlayerWallet, int> _chanceTracker = new Dictionary<PlayerWallet, int>();
    // Tracks how much debt each player currently owes
    private Dictionary<PlayerWallet, float> _debtTracker = new Dictionary<PlayerWallet, float>();
    // Tracks spectating players
    private HashSet<PlayerWallet> _spectators = new HashSet<PlayerWallet>();

    public bool IsSpectating(PlayerWallet wallet) => _spectators.Contains(wallet);
    public int GetChance(PlayerWallet wallet) => _chanceTracker.ContainsKey(wallet) ? _chanceTracker[wallet] : 0;
    public bool IsOnFinalChance(PlayerWallet wallet) => GetChance(wallet) == 3;
    public float GetCurrentLoanAmount(PlayerWallet wallet)
    {
        int chance = GetChance(wallet);
        if (chance <= 0 || chance > loanAmounts.Length) return 0f;
        return loanAmounts[chance - 1];
    }

    /// <summary>
    /// Run the broke player resolution for all broke players one by one.
    /// Called by BettingUI at the start of the betting phase.
    /// </summary>
    public IEnumerator ResolveBrokePlayers(PlayerWallet[] wallets, int humanWalletIndex)
    {
        foreach (PlayerWallet wallet in wallets)
            Debug.Log($"[BrokeCheck] {wallet.playerName} balance: {wallet.Balance} broke: {wallet.IsBroke} spectating: {IsSpectating(wallet)}");
        foreach (PlayerWallet wallet in wallets)
        {
            if (!wallet.IsBroke) continue;
            if (IsSpectating(wallet)) continue;

            // Don't prompt eliminated players
            PlayerHand hand = wallet.GetComponent<PlayerHand>();
            if (hand != null && hand.IsEliminated) continue;

            // Increment chance counter
            if (!_chanceTracker.ContainsKey(wallet))
                _chanceTracker[wallet] = 0;
            _chanceTracker[wallet]++;

            int chance = _chanceTracker[wallet];

            // Out of chances — eliminate
            if (chance > loanAmounts.Length)
            {
                Debug.Log($"[BrokePlayerManager] {wallet.playerName} has used all chances. Eliminated.");
                PlayerHand eliminatedHand = wallet.GetComponent<PlayerHand>();
                if (hand != null) eliminatedHand.Eliminate();
                _chanceTracker[wallet] = 0;
                continue;
            }

            bool isHuman = System.Array.IndexOf(wallets, wallet) == humanWalletIndex;

            if (isHuman)
            {
                // Show broke UI and wait for decision
                bool tookLoan = false;
                yield return StartCoroutine(
                    brokePlayerUI.ShowBrokePrompt(wallet, chance, loanAmounts[chance - 1],
                    result => tookLoan = result));

                if (tookLoan)
                    ApplyLoan(wallet, loanAmounts[chance - 1]);
                else
                    MakeSpectator(wallet);
            }
            else
            {
                // AI always takes the loan
                yield return new WaitForSeconds(0.5f);
                ApplyLoan(wallet, loanAmounts[chance - 1]);
                Debug.Log($"[BrokePlayerManager] {wallet.playerName} (AI) takes loan of ${loanAmounts[chance - 1]}.");
            }
        }
    }

    /// <summary>
    /// Apply loan top-up to a player's wallet.
    /// </summary>
    public void ApplyLoan(PlayerWallet wallet, float amount)
    {
        _debtTracker[wallet] = amount;
        wallet.AddWinnings(amount);
        Debug.Log($"[BrokePlayerManager] {wallet.playerName} topped up with ${amount}. Debt: ${amount}.");
    }

    /// <summary>
    /// Called after a broke player wins. Deducts half the loan from winnings.
    /// </summary>
    public void RepayDebt(PlayerWallet wallet, float winnings)
    {
        if (!_debtTracker.ContainsKey(wallet) || _debtTracker[wallet] <= 0f) return;

        float debt = _debtTracker[wallet];
        float repayment = Mathf.Min(debt / 2f, winnings);
        wallet.Balance -= repayment;
        _debtTracker[wallet] = 0f;
        _chanceTracker[wallet] = 0; // Reset chances on win

        Debug.Log($"[BrokePlayerManager] {wallet.playerName} repaid ${repayment} of debt. Balance: {wallet.Balance}.");
    }

    public bool HasDebt(PlayerWallet wallet) =>
        _debtTracker.ContainsKey(wallet) && _debtTracker[wallet] > 0f;

    void MakeSpectator(PlayerWallet wallet)
    {
        _spectators.Add(wallet);
        wallet.IsSpectating = true;
        _chanceTracker[wallet] = 0;
        Debug.Log($"[BrokePlayerManager] {wallet.playerName} chose to spectate.");
    }

    public void ResetAllState()
    {
        _chanceTracker.Clear();
        _debtTracker.Clear();
        _spectators.Clear();
        Debug.Log("[BrokePlayerManager] All state reset.");
    }
}

