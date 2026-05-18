using UnityEngine;

/// <summary>
/// Manages the list of player seats and drives snap requests to BlackjackCameraController.
///
/// Setup:
///   1. Attach this to the same GameObject as BlackjackCameraController (or any parent).
///   2. Populate 'seats' in the Inspector with the 3 PlayerSeatTarget objects
///      (the other players — NOT the dealer, because Escape already snaps to neutral/dealer).
///      Suggested order: [0] Left player, [1] Right player, [2] Player across the table.
/// </summary>
public class SeatSnapManager : MonoBehaviour
{
    [Tooltip("Assign your PlayerSeatTarget objects here. Order = key order (1, 2, 3).")]
    public PlayerSeatTarget[] seats;

    // Which seat is currently focused (-1 = none / dealer)
    private int _currentSeatIndex = -1;

    void Start()
    {
        // Auto-assign seat indices if they weren't set in the Inspector
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] != null && seats[i].seatIndex == -1)
                seats[i].seatIndex = i;
        }
    }

    /// <summary>Snap camera to a specific seat by array index (0-based).</summary>
    public void SnapToSeat(int index, BlackjackCameraController cam)
    {
        if (seats == null || index < 0 || index >= seats.Length) return;
        if (seats[index] == null) return;

        _currentSeatIndex = index;
        cam.SnapToPosition(seats[index].transform.position);
        Debug.Log($"[SeatSnap] Looking at: {seats[index].seatLabel}");
    }

    /// <summary>Cycle forward through seats. Wraps around; -1 means return to dealer.</summary>
    public void CycleToNextSeat(BlackjackCameraController cam)
    {
        if (seats == null || seats.Length == 0) return;

        _currentSeatIndex++;

        // After the last seat, go back to neutral (dealer)
        if (_currentSeatIndex >= seats.Length)
        {
            _currentSeatIndex = -1;
            cam.SnapToNeutral();
            Debug.Log("[SeatSnap] Looking at: Dealer (neutral)");
            return;
        }

        SnapToSeat(_currentSeatIndex, cam);
    }

    /// <summary>Returns the currently focused seat, or null if looking at dealer.</summary>
    public PlayerSeatTarget GetCurrentSeat()
    {
        if (_currentSeatIndex < 0 || seats == null || _currentSeatIndex >= seats.Length)
            return null;
        return seats[_currentSeatIndex];
    }
}