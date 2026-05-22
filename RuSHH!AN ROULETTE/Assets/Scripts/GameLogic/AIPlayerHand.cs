using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsLayers;

/// <summary>
/// AI decision logic for the 3 non-human players.
/// Inherits PlayerHand so it has all hand tracking built in.
/// Each AI has a slightly different risk tolerance to avoid identical behaviour.
/// </summary>
public class AIPlayerHand : PlayerHand
{
    [Header("AI Settings")]
    [Tooltip("The hand value at which this AI will always stand. Randomised slightly per AI.")]
    [Range(14, 19)]
    public int standThreshold = 17;

    [Tooltip("Adds a small random chance to stand early, making each AI feel different.")]
    [Range(0f, 0.3f)]
    public float earlyStandChance = 0.1f;

    /// <summary>
    /// Called by RoundManager when it is this AI's turn.
    /// Returns true if the AI chose to hit, false if it stood.
    /// </summary>
    public bool TakeTurn()
    {
        if (IsDoneForRound) return false;

        // Always hit on low totals
        if (handTotal < 12)
        {
            HitWithRandomCard();
            return true;
        }

        // Stand if at or above threshold
        if (handTotal >= standThreshold)
        {
            Stand();
            return false;
        }

        // Between 12 and threshold — small chance to stand early for variance
        if (Random.value < earlyStandChance)
        {
            Stand();
            return false;
        }

        HitWithRandomCard();
        return true;
    }

    private void HitWithRandomCard()
    {
        int card = Random.Range(1, 12); // 1-11 for prototype
        Debug.Log($"[{playerName}] hits and draws {card}. Total: {handTotal + card}.");
        AddCard(card);
    }
}