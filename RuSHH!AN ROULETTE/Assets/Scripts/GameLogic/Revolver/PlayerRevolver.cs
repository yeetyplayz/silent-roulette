using UnityEngine;

/// <summary>
/// Tracks the state of a single player's personal revolver.
/// Each player owns their own cylinder with their own bullet count.
/// Bullets are reshuffled into random chambers every time a new bullet is added.
/// </summary>
public class PlayerRevolver : MonoBehaviour
{
    public const int ChamberCount = 6;

    // How many bullets are currently loaded
    public int BulletCount { get; private set; } = 1;

    // The cylinder — true means a bullet is in that chamber
    private bool[] _chambers = new bool[ChamberCount];

    // Current chamber position
    private int _currentChamber = 0;

    void Awake()
    {
        ShuffleCylinder();
    }

    /// <summary>
    /// Add one bullet and reshuffle the cylinder.
    /// Called at the end of each round this player survives.
    /// </summary>
    public void AddBulletAndReshuffle()
    {
        BulletCount = Mathf.Min(BulletCount + 1, ChamberCount);
        ShuffleCylinder();
        Debug.Log($"[Revolver] Bullet added. {BulletCount}/{ChamberCount} chambers loaded. Cylinder reshuffled.");
    }

    /// <summary>
    /// Pull the trigger. Advances to a random chamber in the reshuffled cylinder.
    /// Returns true if the chamber is live (player dies), false if empty (click).
    /// </summary>
    public bool PullTrigger()
    {
        bool isLive = _chambers[_currentChamber];
        Debug.Log($"[Revolver] Chamber {_currentChamber} — {(isLive ? "LIVE" : "empty")}.");
        // Advance to next chamber for the next pull
        _currentChamber = (_currentChamber + 1) % ChamberCount;
        return isLive;
    }

    /// <summary>
    /// Distribute bullets into random chambers and pick a random start position.
    /// </summary>
    private void ShuffleCylinder()
    {
        // Clear all chambers
        for (int i = 0; i < ChamberCount; i++)
            _chambers[i] = false;

        // Place bullets in random unique chambers
        int placed = 0;
        while (placed < BulletCount)
        {
            int slot = Random.Range(0, ChamberCount);
            if (!_chambers[slot])
            {
                _chambers[slot] = true;
                placed++;
            }
        }

        // Random starting position
        _currentChamber = Random.Range(0, ChamberCount);
        Debug.Log($"[Revolver] Cylinder shuffled. Starting at chamber {_currentChamber}.");
    }
}