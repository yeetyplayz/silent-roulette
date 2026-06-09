using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the game over screen when one player remains.
/// Hooks into RoundManager.OnGameWon.
/// Attach to the Canvas. Assign the panel and references in the Inspector.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject gameOverPanel;

    [Header("Labels")]
    public TextMeshProUGUI winnerText;

    [Header("Buttons")]
    public Button restartButton;
    public Button quitButton;

    [Header("References")]
    public RoundManager roundManager;
    public BettingManager bettingManager;
    public BrokePlayerManager brokePlayerManager;
    public BlackjackCameraController cameraController;
    public TablePeekCamera peekCamera;

    void Start()
    {
        gameOverPanel.SetActive(false);

        restartButton.onClick.AddListener(OnRestart);
        quitButton.onClick.AddListener(OnQuit);

        if (roundManager != null)
            roundManager.OnGameWon += ShowGameOver;
        else
            Debug.LogError("[GameOverUI] No RoundManager assigned.");
    }

    void ShowGameOver(PlayerHand winner)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (cameraController != null) cameraController.enabled = false;
        if (peekCamera != null) peekCamera.IsBettingPhase = true;

        winnerText.text = $"{winner.playerName} wins!";
        gameOverPanel.SetActive(true);
    }

    void OnRestart()
    {
        gameOverPanel.SetActive(false);
        StartCoroutine(RestartGame());
    }

    IEnumerator RestartGame()
    {
        // Reset all wallets
        foreach (PlayerWallet wallet in bettingManager.wallets)
        {
            wallet.Balance = wallet.startingBalance;
            wallet.ResetBet();
            wallet.IsSpectating = false;
        }

        // Reset all player hands and revolvers
        foreach (PlayerHand player in roundManager.players)
        {
            player.ResetHand();
            player.state = PlayerHand.PlayerState.Active;

            PlayerRevolver revolver = player.GetComponent<PlayerRevolver>();
            if (revolver != null) revolver.ResetRevolver();
        }

        // Reset broke manager state
        if (brokePlayerManager != null)
            brokePlayerManager.ResetAllState();

        // Reset dealer
        roundManager.dealer.ResetHand();

        // Re-enable camera
        if (cameraController != null) cameraController.enabled = false;
        if (peekCamera != null) peekCamera.IsBettingPhase = false;

        yield return null;
        roundManager.RestartGame();
    }

    void OnQuit()
    {
        Debug.Log("[GameOverUI] Quit selected.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}