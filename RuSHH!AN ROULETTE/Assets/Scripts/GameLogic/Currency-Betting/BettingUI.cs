using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the betting panel UI for the human player.
/// Opens automatically at the start of each round.
/// Resolves broke players first, then opens normal betting for everyone else.
/// </summary>
public class BettingUI : MonoBehaviour
{
    [Header("References")]
    public BettingManager bettingManager;
    public BrokePlayerManager brokePlayerManager;
    public BlackjackCameraController cameraController;
    public TablePeekCamera peekCamera;

    [Header("Normal Betting Panel")]
    public GameObject bettingPanel;

    [Header("Human Bet Display")]
    public TextMeshProUGUI currentBetText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI totalPotText;

    [Header("Preset Bet Buttons")]
    public Button addTenButton;
    public Button addFiftyButton;
    public Button addHundredButton;
    public Button addFiveHundredButton;
    public Button clearBetButton;
    public Button confirmBetButton;

    [Header("Custom Input")]
    public Button toggleCustomInputButton;
    public GameObject customInputPanel;
    public TMP_InputField customInputField;
    public Button confirmCustomButton;

    [Header("All Players Bet Display")]
    public TextMeshProUGUI[] playerBetLabels;

    private float _currentBet = 0f;
    private bool _betConfirmed = false;

    public bool BetConfirmed => _betConfirmed;

    public IEnumerator OpenBettingPhase()
    {
        _betConfirmed = false;
        _currentBet = bettingManager.MinimumBet;

        // Unlock cursor and disable camera
        if (cameraController != null) cameraController.SetInputLocked(true);
        if (peekCamera != null) peekCamera.IsBettingPhase = true;

        // Resolve broke players first, one by one
        if (brokePlayerManager != null)
            yield return StartCoroutine(
                brokePlayerManager.ResolveBrokePlayers(
                    bettingManager.wallets,
                    bettingManager.humanWalletIndex));

        // Add this temporarily
        PlayerWallet debugWallet = bettingManager.wallets[bettingManager.humanWalletIndex];
        Debug.Log($"[BettingUI] After broke resolution — IsSpectating: {debugWallet.IsSpectating} IsBroke: {debugWallet.IsBroke}");

        // Skip betting panel if human is spectating
        PlayerWallet humanWallet = bettingManager.wallets[bettingManager.humanWalletIndex];
        PlayerHand humanHand = humanWallet.GetComponent<PlayerHand>();

        if (humanWallet.IsSpectating || (humanHand != null && humanHand.IsEliminated))
        {
            yield return StartCoroutine(bettingManager.PlaceAIBetsStaggered());
        }

        // Skip betting panel if human just took a loan — bet is forced automatically
        if (brokePlayerManager != null && brokePlayerManager.HasDebt(humanWallet))
        {
            yield return StartCoroutine(bettingManager.PlaceAIBetsStaggered());
        }

        // Normal betting phase
        bettingPanel.SetActive(true);
        customInputPanel.SetActive(false);
        InitialisePlayerBetLabels();
        RefreshHumanDisplay();
        yield return StartCoroutine(bettingManager.PlaceAIBetsStaggered());
        RefreshAllPlayerBetLabels();

        yield return new WaitUntil(() => _betConfirmed);

        bettingPanel.SetActive(false);

        Debug.Log($"[BettingUI] Unlocking camera. Controller null: {cameraController == null}");
        if (cameraController != null)
        {
            cameraController.enabled = true;
            cameraController.SetInputLocked(false);
            cameraController.ResetNeutralRotation();
        }
        Debug.Log("[BettingUI] Unlock called.");

        // Lock cursor and re-enable camera
        if (cameraController != null)
        {
            cameraController.enabled = true;
            cameraController.SetInputLocked(false);
            cameraController.ResetNeutralRotation();
        }
        if (peekCamera != null) peekCamera.IsBettingPhase = false;

        Debug.Log($"[BettingUI] Unlocking camera. Controller null: {cameraController == null}");
        if (cameraController != null)
        {
            cameraController.enabled = true;
            cameraController.SetInputLocked(false);
            cameraController.ResetNeutralRotation();
        }
        Debug.Log("[BettingUI] Unlock called.");

        if (cameraController != null)
        {
            cameraController.SetInputLocked(false);
            cameraController.ResetNeutralRotation();
        }

        bettingManager.OnAllBetsPlaced?.Invoke();

    }



    // -----------------------------------------------------------------------
    //  Button handlers
    // -----------------------------------------------------------------------
    public void OnAddTen() => AdjustBet(10f);
    public void OnAddFifty() => AdjustBet(50f);
    public void OnAddHundred() => AdjustBet(100f);
    public void OnAddFiveHundred() => AdjustBet(500f);

    public void OnClearBet()
    {
        _currentBet = bettingManager.MinimumBet;
        RefreshHumanDisplay();
    }

    public void OnConfirmBet()
    {
        PlayerWallet human = bettingManager.wallets[bettingManager.humanWalletIndex];
        bool valid = human.PlaceBet(_currentBet, bettingManager.MinimumBet);
        if (!valid)
        {
            _currentBet = Mathf.Clamp(_currentBet, bettingManager.MinimumBet, human.Balance);
            human.PlaceBet(_currentBet, bettingManager.MinimumBet);
        }

        RefreshAllPlayerBetLabels();
        _betConfirmed = true;
    }

    public void OnToggleCustomInput()
    {
        customInputPanel.SetActive(!customInputPanel.activeSelf);
    }

    public void OnConfirmCustomInput()
    {
        if (float.TryParse(customInputField.text, out float amount))
        {
            PlayerWallet human = bettingManager.wallets[bettingManager.humanWalletIndex];
            _currentBet = Mathf.Clamp(amount, bettingManager.MinimumBet, human.Balance);
            customInputPanel.SetActive(false);
            RefreshHumanDisplay();
        }
        else
        {
            Debug.LogWarning("[BettingUI] Invalid custom bet input.");
        }
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------
    void AdjustBet(float amount)
    {
        PlayerWallet human = bettingManager.wallets[bettingManager.humanWalletIndex];
        _currentBet = Mathf.Clamp(_currentBet + amount, bettingManager.MinimumBet, human.Balance);
        RefreshHumanDisplay();
    }

    void RefreshHumanDisplay()
    {
        PlayerWallet human = bettingManager.wallets[bettingManager.humanWalletIndex];
        if (currentBetText != null)
            currentBetText.text = $"Bet: ${_currentBet:0}";
        if (balanceText != null)
            balanceText.text = $"Balance: ${human.Balance:0}";
        if (totalPotText != null)
            totalPotText.text = $"Pot: ${bettingManager.GetTotalPot():0}";
    }

    void RefreshAllPlayerBetLabels()
    {
        if (playerBetLabels == null) return;
        for (int i = 0; i < playerBetLabels.Length; i++)
        {
            if (playerBetLabels[i] == null) continue;
            if (i < bettingManager.wallets.Length)
            {
                PlayerWallet w = bettingManager.wallets[i];
                playerBetLabels[i].text = w.IsSpectating
                    ? $"{w.playerName}: Spectating"
                    : w.CurrentBet > 0f
                        ? $"{w.playerName}: ${w.CurrentBet:0}"
                        : $"{w.playerName}: thinking...";
            }
        }
    }
    void InitialisePlayerBetLabels()
    {
        if (playerBetLabels == null) return;
        for (int i = 0; i < playerBetLabels.Length; i++)
        {
            if (playerBetLabels[i] == null) continue;
            if (i < bettingManager.wallets.Length)
            {
                PlayerWallet w = bettingManager.wallets[i];
                playerBetLabels[i].text = w.IsSpectating
                    ? $"{w.playerName}: Spectating"
                    : $"{w.playerName}: thinking...";
            }
        }
    }
}