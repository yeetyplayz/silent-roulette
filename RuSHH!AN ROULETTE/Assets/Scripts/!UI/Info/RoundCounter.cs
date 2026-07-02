using UnityEngine;
using TMPro;

/// <summary>
/// Tracks and displays the current round number.
/// Attach to any persistent GameObject.
/// Assign roundText for the betting panel 2D label,
/// and worldRoundText for the world space label near the dealer.
/// Call IncrementRound() from RoundManager at the start of each round.
/// </summary>
public class RoundCounter : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("2D TMP label on the betting panel.")]
    public TextMeshProUGUI roundText;

    [Tooltip("World space TMP label near the dealer.")]
    public TextMeshProUGUI worldRoundText;

    private int _currentRound = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void IncrementRound()
    {
        _currentRound++;
        UpdateDisplay();
    }

    public void ResetRound()
    {
        _currentRound = 0;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        string text = $"Round {_currentRound}";
        if (roundText != null) roundText.text = text;
        if (worldRoundText != null) worldRoundText.text = text;
    }
}