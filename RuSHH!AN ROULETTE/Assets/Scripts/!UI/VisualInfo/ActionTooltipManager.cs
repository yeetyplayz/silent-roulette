using UnityEngine;

/// <summary>
/// Feeds action text to each seat's ActionTooltip.
/// Attach to GameSystems. Assign one ActionTooltip per AI seat
/// in the same order as RoundManager.players.
/// Leave the human player's slot empty (null) — no tooltip for human.
/// </summary>
public class ActionTooltipManager : MonoBehaviour
{
    [Header("Tooltips (assign in seat order, leave human slot empty)")]
    public ActionTooltip[] tooltips;

    [Header("Dealer Tooltip")]
    public ActionTooltip dealerTooltip;

    [Header("References")]
    public RoundManager roundManager;

    /// <summary>
    /// Call this when a player hits.
    /// seatIndex matches RoundManager.players index.
    /// </summary>
    public void OnPlayerHit(int seatIndex, string playerName)
    {
        ShowTooltip(seatIndex, $"{playerName} hits");
    }

    /// <summary>
    /// Call this when a player stands.
    /// </summary>
    public void OnPlayerStand(int seatIndex, string playerName)
    {
        ShowTooltip(seatIndex, $"{playerName} stands");
    }

    /// <summary>
    /// Call this when a player busts.
    /// </summary>
    public void OnPlayerBust(int seatIndex, string playerName)
    {
        ShowTooltip(seatIndex, $"{playerName} busts!");
    }

    public void OnDealerHit()
    {
        if (dealerTooltip != null) dealerTooltip.ShowAction("Dealer hits");
    }

    public void OnDealerStand()
    {
        if (dealerTooltip != null) dealerTooltip.ShowAction("Dealer stands");
    }

    public void OnDealerBust()
    {
        if (dealerTooltip != null) dealerTooltip.ShowAction("Dealer busts!");
    }

    /// <summary>Hide all tooltips — call at round end.</summary>
    public void HideAll()
    {
        foreach (ActionTooltip tooltip in tooltips)
            if (tooltip != null) tooltip.Hide();
        if (dealerTooltip != null) dealerTooltip.Hide();
    }

    void ShowTooltip(int seatIndex, string text)
    {
        Debug.Log($"[ActionTooltip] ShowTooltip called. Seat: {seatIndex}, Text: {text}");
        if (tooltips == null || seatIndex < 0 || seatIndex >= tooltips.Length) return;
        if (tooltips[seatIndex] == null) return;
        tooltips[seatIndex].ShowAction(text);
    }
}