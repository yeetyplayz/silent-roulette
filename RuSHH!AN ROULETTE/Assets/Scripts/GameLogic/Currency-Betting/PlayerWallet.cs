using UnityEngine;

/// <summary>
/// Tracks a player's balance and current bet.
/// Attach to every seat including the human player.
/// </summary>
public class PlayerWallet : MonoBehaviour
{
    [Header("Settings")]
    public string playerName = "Player";
    public float startingBalance = 1000f;

    public float Balance { get; set; }
    public float CurrentBet { get; protected set; }
    public bool IsSpectating { get; set; } = false;
    public bool IsBroke => Balance <= 0f;

    void Awake()
    {
        Balance = startingBalance;
    }

    public bool PlaceBet(float amount, float minimumBet)
    {
        if (amount < minimumBet)
        {
            Debug.LogWarning($"[{playerName}] Bet {amount} is below minimum {minimumBet}.");
            return false;
        }
        if (amount > Balance)
        {
            Debug.LogWarning($"[{playerName}] Bet {amount} exceeds balance {Balance}.");
            return false;
        }

        CurrentBet = amount;
        Debug.Log($"[{playerName}] Placed bet: {CurrentBet}. Balance: {Balance}.");
        return true;
    }

    public void DeductBet()
    {
        Balance -= CurrentBet;
        Debug.Log($"[{playerName}] Lost bet {CurrentBet}. Balance: {Balance}.");
    }

    public void AddWinnings(float amount)
    {
        Balance += amount;
        Debug.Log($"[{playerName}] Won {amount}. Balance: {Balance}.");
    }

    public void ResetBet()
    {
        CurrentBet = 0f;
    }
}