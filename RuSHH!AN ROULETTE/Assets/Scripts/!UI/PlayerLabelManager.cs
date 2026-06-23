using UnityEngine;

/// <summary>
/// Feeds live data (hand total, bet, name) to each PlayerInfoLabel every frame.
/// Attach to any persistent GameObject, e.g. RoundManager or GameSystems.
/// Assign one PlayerInfoLabel per seat in order matching RoundManager.players,
/// plus a separate dealerLabel.
/// </summary>
public class PlayerLabelManager : MonoBehaviour
{
    [Header("Player Labels (assign in seat order matching RoundManager.players)")]
    public PlayerInfoLabel[] playerLabels;

    [Header("Dealer Label")]
    public PlayerInfoLabel dealerLabel;

    [Header("References")]
    public RoundManager roundManager;
    public BettingManager bettingManager;
    public DealerAI dealer;

    void Update()
    {
        if (roundManager == null || bettingManager == null) return;

        // Update player labels
        for (int i = 0; i < playerLabels.Length; i++)
        {
            if (playerLabels[i] == null) continue;
            if (i >= roundManager.players.Length) continue;

            PlayerHand player = roundManager.players[i];
            PlayerWallet wallet = bettingManager.wallets[i];

            playerLabels[i].playerName = player.playerName;
            playerLabels[i].handTotal = player.handTotal;
            playerLabels[i].currentBet = wallet.CurrentBet;
            playerLabels[i].isDealer = false;
        }

        // Update dealer label
        if (dealerLabel != null && dealer != null)
        {
            dealerLabel.playerName = "Dealer";
            dealerLabel.handTotal = dealer.handTotal;
            dealerLabel.isDealer = true;
            dealerLabel.alwaysVisible = true;
        }
    }
}