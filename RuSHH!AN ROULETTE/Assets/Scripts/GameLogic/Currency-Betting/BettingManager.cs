using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the pot, distributes winnings, applies dealer bust bonus,
/// and deducts debt repayment for broke players who won.
/// </summary>
public class BettingManager : MonoBehaviour
{
    [Header("Settings")]
    public float minimumBet = 10f;
    public float dealerBustMultiplier = 1.5f;

    [Header("References")]
    public PlayerWallet[] wallets;
    public int humanWalletIndex = 0;
    public BrokePlayerManager brokePlayerManager;

    public System.Action OnAllBetsPlaced;
    public System.Action<PlayerWallet> OnPlayerBroke;

    public float MinimumBet => minimumBet;

    public float GetHighestTableBet()
    {
        float highest = 0f;
        foreach (PlayerWallet w in wallets)
            if (w.CurrentBet > highest) highest = w.CurrentBet;
        return highest;
    }

    public IEnumerator PlaceAIBetsStaggered(float delay = 0.8f)
    {
        foreach (PlayerWallet w in wallets)
        {
            if (w is AIWalletBehaviour ai && !w.IsSpectating)
            {
                ai.AutoBet(minimumBet, GetHighestTableBet());
                yield return new WaitForSeconds(delay);
            }
        }
    }

    public void ResolveBets(List<PlayerHand> winners, List<PlayerHand> losers, bool dealerBust)
    {
        // Collect losing pot
        float losingPot = 0f;
        foreach (PlayerHand loser in losers)
        {
            PlayerWallet wallet = GetWalletForPlayer(loser);
            if (wallet == null) continue;
            losingPot += wallet.CurrentBet;
            wallet.DeductBet();
        }

        // Apply dealer bust multiplier
        if (dealerBust && losingPot > 0f)
        {
            float bonus = losingPot * (dealerBustMultiplier - 1f);
            losingPot += bonus;
            Debug.Log($"[BettingManager] Dealer bust! Pot boosted by {bonus}. Total pot: {losingPot}.");
        }

        // Split pot among winners
        if (winners.Count > 0 && losingPot > 0f)
        {
            float share = losingPot / winners.Count;
            foreach (PlayerHand winner in winners)
            {
                PlayerWallet wallet = GetWalletForPlayer(winner);
                if (wallet == null) continue;

                // Deduct debt repayment before awarding winnings
                if (brokePlayerManager != null && brokePlayerManager.HasDebt(wallet))
                    brokePlayerManager.RepayDebt(wallet, share);

                wallet.AddWinnings(share);
            }
        }
        else if (winners.Count > 0)
        {
            Debug.Log("[BettingManager] All players won. No pot to split.");
        }

        // Reset all bets
        foreach (PlayerWallet w in wallets)
            w.ResetBet();

        // Check for broke players
        foreach (PlayerWallet w in wallets)
        {
            if (w.IsBroke && !w.IsSpectating)
            {
                Debug.Log($"[BettingManager] {w.playerName} is broke.");
                OnPlayerBroke?.Invoke(w);
            }
        }
    }

    public float GetTotalPot()
    {
        float total = 0f;
        foreach (PlayerWallet w in wallets)
            total += w.CurrentBet;
        return total;
    }

    PlayerWallet GetWalletForPlayer(PlayerHand hand)
    {
        foreach (PlayerWallet w in wallets)
            if (w.gameObject == hand.gameObject) return w;
        return null;
    }
}