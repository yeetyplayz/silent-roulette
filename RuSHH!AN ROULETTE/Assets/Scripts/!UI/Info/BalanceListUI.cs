using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Hold Tab during gameplay to show a balance list overlay.
/// Shows each player's name and balance, with eliminated/spectating
/// players crossed out. Total pot shown at the top.
/// Attach to the Canvas. Assign the panel and references in the Inspector.
/// </summary>
public class BalanceListUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject balancePanel;

    [Header("References")]
    public RoundManager roundManager;
    public BettingManager bettingManager;

    [Header("Text Fields")]
    public TextMeshProUGUI totalPotText;
    // One entry per player in seat order
    public TextMeshProUGUI[] playerEntries;

    [Header("Settings")]
    public KeyCode holdKey = KeyCode.Tab;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.15f;
    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;
    private bool _isVisible = false;

    void Start()
    {
        _canvasGroup = balancePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = balancePanel.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        balancePanel.SetActive(true); // keep active so CanvasGroup can fade
    }

    void Update()
    {
        bool holding = Input.GetKey(holdKey);

        if (holding && !_isVisible)
        {
            _isVisible = true;
            RefreshList();
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(1f, fadeInDuration));
        }
        else if (!holding && _isVisible)
        {
            _isVisible = false;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(Fade(0f, fadeOutDuration));
        }
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        _canvasGroup.alpha = targetAlpha;
    }

    void RefreshList()
    {
        if (bettingManager == null || roundManager == null) return;

        // Total pot
        if (totalPotText != null)
            totalPotText.text = $"Pot: ${bettingManager.GetTotalPot()}";

        // Player entries
        for (int i = 0; i < playerEntries.Length; i++)
        {
            if (playerEntries[i] == null) continue;
            if (i >= bettingManager.wallets.Length) continue;

            PlayerWallet wallet = bettingManager.wallets[i];
            PlayerHand hand = roundManager.players[i];
            string name = wallet.playerName;
            float balance = wallet.Balance;

            bool isOut = hand.IsEliminated || wallet.IsSpectating;

            if (isOut)
            {
                // Strikethrough via TMP rich text
                playerEntries[i].text = $"<s>{name}: ${balance}</s>";
                playerEntries[i].color = new Color(1f, 1f, 1f, 0.4f);
            }
            else
            {
                playerEntries[i].text = $"{name}: ${balance}";
                playerEntries[i].color = Color.white;
            }
        }
    }
}