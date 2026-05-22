using UnityEngine;

/// <summary>
/// The dealer entity. Takes one action per cycle — either draws one card or stands.
/// Stands at 17 or above.
/// </summary>
public class DealerAI : MonoBehaviour
{
    [Header("Dealer Settings")]
    public int standThreshold = 17;

    [HideInInspector] public int handTotal = 0;
    [HideInInspector] public bool isBust = false;
    [HideInInspector] public bool hasStood = false;

    public bool IsDoneForRound => isBust || hasStood;

    /// <summary>
    /// Takes exactly one action: draws one card or stands.
    /// Called once per cycle by RoundManager.
    /// </summary>
    public void TakeOneAction()
    {
        if (IsDoneForRound) return;

        if (handTotal >= standThreshold)
        {
            hasStood = true;
            Debug.Log($"[Dealer] Stands at {handTotal}.");
            return;
        }

        int card = Random.Range(1, 12);
        handTotal += card;
        Debug.Log($"[Dealer] Draws {card}. Total: {handTotal}.");

        if (handTotal > 21)
        {
            isBust = true;
            Debug.Log($"[Dealer] Bust at {handTotal}.");
        }
    }

    /// <summary>
    /// Compare dealer hand against a single player.
    /// Returns true if the player wins (safe), false if the player loses (roulette).
    /// </summary>
    public bool PlayerWinsAgainstDealer(PlayerHand player)
    {
        if (player.isBust) return false;
        if (isBust) return true;
        if (player.handTotal >= handTotal) return true;
        return false;
    }

    public void ResetHand()
    {
        handTotal = 0;
        isBust = false;
        hasStood = false;
    }

    public void DealOpeningHand()
    {
        ResetHand();
        for (int i = 0; i < 2; i++)
        {
            int card = Random.Range(1, 12);
            handTotal += card;
            Debug.Log($"[Dealer] Opening card: {card}. Total: {handTotal}.");
        }
    }
}