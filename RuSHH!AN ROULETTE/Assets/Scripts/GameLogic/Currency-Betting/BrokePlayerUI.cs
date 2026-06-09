using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel shown to a broke human player during the betting phase.
/// Shows two options: Spectate or Take a Loan.
/// Has a depleting timer bar — if it runs out, player auto-spectates.
/// </summary>
public class BrokePlayerUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject brokePanel;

    [Header("Labels")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI loanButtonText;
    public TextMeshProUGUI finalChanceWarning; // Red "FINAL CHANCE" text

    [Header("Buttons")]
    public Button spectateButton;
    public Button takeLoanButton;

    [Header("Timer Bar")]
    public Image timerBar; // Fill image — set Fill Method to Horizontal in Inspector
    public float decisionTime = 10f;

    private bool _decided = false;
    private bool _tookLoan = false;

    /// <summary>
    /// Show the broke prompt and wait for the player to decide.
    /// Calls back with true if they took the loan, false if they spectated.
    /// </summary>
    public IEnumerator ShowBrokePrompt(PlayerWallet wallet, int chance, float loanAmount, Action<bool> callback)
    {
        Debug.Log($"brokePanel: {brokePanel}");
        Debug.Log($"promptText: {promptText}");
        Debug.Log($"loanButtonText: {loanButtonText}");
        Debug.Log($"spectateButton: {spectateButton}");
        Debug.Log($"takeLoanButton: {takeLoanButton}");
        Debug.Log($"timerBar: {timerBar}");
        Debug.Log($"finalChanceWarning: {finalChanceWarning}");

        _decided = false;
        _tookLoan = false;

        // Set up UI text
        promptText.text = $"You are broke.\nChance {chance} of 3.";
        loanButtonText.text = $"Take a Loan: ${loanAmount}";

        // Show final chance warning only on chance 3
        if (finalChanceWarning != null)
            finalChanceWarning.gameObject.SetActive(chance == 3);

        // Wire buttons
        spectateButton.onClick.RemoveAllListeners();
        spectateButton.onClick.AddListener(() => { _tookLoan = false; _decided = true; });

        takeLoanButton.onClick.RemoveAllListeners();
        takeLoanButton.onClick.AddListener(() => { _tookLoan = true; _decided = true; });

        brokePanel.SetActive(true);

        // Depleting timer bar
        float elapsed = 0f;
        while (!_decided && elapsed < decisionTime)
        {
            elapsed += Time.deltaTime;
            if (timerBar != null)
                timerBar.fillAmount = 1f - (elapsed / decisionTime);
            yield return null;
        }

        // Timer ran out — auto spectate
        if (!_decided)
        {
            Debug.Log($"[BrokePlayerUI] Timer ran out. {wallet.playerName} auto-spectates.");
            _tookLoan = false;
        }

        brokePanel.SetActive(false);
        callback(_tookLoan);
    }
}