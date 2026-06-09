using UnityEngine;

/// <summary>
/// AI betting personality. Extends PlayerWallet with automatic bet placement.
/// Reacts to the highest visible bet at the table with some randomness.
/// </summary>
public class AIWalletBehaviour : PlayerWallet
{
    [Header("AI Personality")]
    [Tooltip("Base fraction of balance the AI is willing to bet (0.1 = 10%).")]
    [Range(0.05f, 0.5f)]
    public float baseBetFraction = 0.15f;

    [Tooltip("How likely the AI is to follow a high bet at the table (0 = never, 1 = always).")]
    [Range(0f, 1f)]
    public float followHighBetChance = 0.4f;

    [Tooltip("Multiplier applied when following a high bet.")]
    [Range(1f, 3f)]
    public float highBetFollowMultiplier = 1.5f;

    /// <summary>
    /// Automatically decide and place a bet.
    /// highestTableBet is the largest bet visible at the table so far.
    /// </summary>
    public void AutoBet(float minimumBet, float highestTableBet)
    {
        float bet = Balance * baseBetFraction;

        // React to a high bet on the table
        if (highestTableBet > bet && Random.value < followHighBetChance)
        {
            bet = highestTableBet * highBetFollowMultiplier;
            Debug.Log($"[{playerName}] Reacting to high table bet of {highestTableBet}.");
        }

        if (Random.value < 0.05f)
        {
            bet = Balance * Random.Range(0.6f, 1.0f);
            Debug.Log($"[{playerName}] Going big!");
        }

        // Clamp to valid range
        bet = Mathf.Clamp(bet, minimumBet, Balance);

        // Add small random variance so all AIs don't bet identically
        float variance = bet * Random.Range(-0.1f, 0.2f);
        bet = Mathf.Clamp(bet + variance, minimumBet, Balance);

        // Round to nearest whole number for cleanliness
        bet = Mathf.Floor(bet);

        PlaceBet(bet, minimumBet);
    }
}