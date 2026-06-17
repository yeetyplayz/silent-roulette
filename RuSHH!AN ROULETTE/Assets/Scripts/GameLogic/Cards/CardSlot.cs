using UnityEngine;

/// <summary>
/// Marks a position on the table where a card lands.
/// Place one on each player seat and one for the dealer.
/// The card will snap down to this transform's position and rotation.
/// </summary>
public class CardSlot : MonoBehaviour
{
    [Tooltip("Label for debugging — e.g. Player 0, Dealer.")]
    public string slotLabel = "Slot";

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(0.1f, 0.01f, 0.14f));
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.05f, slotLabel);
#endif
    }
}