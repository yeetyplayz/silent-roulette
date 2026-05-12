using UnityEngine;

/// <summary>
/// Attach this to any GameObject that represents a player's seat position.
/// The camera will snap to look at this object's transform when that seat is selected.
///
/// In your prototype, just create empty GameObjects placed roughly where each
/// player's face/chest would be and attach this component to them.
/// </summary>
public class PlayerSeatTarget : MonoBehaviour
{
    [Tooltip("Display name shown in debug / UI (e.g. 'Left Player', 'Dealer').")]
    public string seatLabel = "Player";

    [Tooltip("Seat index (0 = left, 1 = right, 2 = across). Set automatically by SeatSnapManager if left at -1.")]
    public int seatIndex = -1;

    // Gizmo so you can see seat positions in the Scene view without any art
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
        Gizmos.DrawSphere(transform.position, 0.12f);
        Gizmos.color = Color.white;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, seatLabel);
#endif
    }
}