using UnityEngine;

/// <summary>
/// Tracks the hand total and game state for any seat at the table.
/// Attach to every player seat including the human player.
/// </summary>
public class PlayerHand : MonoBehaviour
{
    public enum PlayerState { Active, Surviving, Eliminated }

    [Header("Seat Info")]
    public string playerName = "Player";
    public int seatIndex;               // 0-3, assigned by RoundManager

    [HideInInspector] public int handTotal = 0;
    [HideInInspector] public bool hasStood = false;
    [HideInInspector] public bool isBust = false;
    [HideInInspector] public PlayerState state = PlayerState.Active;

    public bool IsActive => state == PlayerState.Active || state == PlayerState.Surviving;
    public bool IsEliminated => state == PlayerState.Eliminated;
    public bool IsDoneForRound => hasStood || isBust;

    /// <summary>Add a card value to the hand.</summary>
    public void AddCard(int value)
    {
        handTotal += value;
        if (handTotal > 21)
        {
            isBust = true;
            Debug.Log($"[{playerName}] Bust at {handTotal}.");
        }
    }

    /// <summary>Player chooses to stand.</summary>
    public void Stand()
    {
        hasStood = true;
        Debug.Log($"[{playerName}] Stands at {handTotal}.");
    }

    /// <summary>Reset hand for a new round. State (alive/dead) is preserved.</summary>
    public void ResetHand()
    {
        handTotal = 0;
        hasStood = false;
        isBust = false;
    }

    /// <summary>Mark this player as permanently eliminated. They stay seated.</summary>
    public void Eliminate()
    {
        state = PlayerState.Eliminated;
        Debug.Log($"[{playerName}] has been eliminated. Remains at the table.");
        // Trigger death animation here when art is ready.
    }

    /// <summary>Player survived the roulette shot.</summary>
    public void SurviveRoulette()
    {
        state = PlayerState.Surviving;
        Debug.Log($"[{playerName}] survived the roulette shot.");
    }
}