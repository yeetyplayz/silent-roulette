using UnityEngine;
using TMPro;

/// <summary>
/// World Space label that floats above a player's card slot.
/// Fades in when the player camera looks toward it, fades out when looking away.
/// Attach to a Canvas set to World Space render mode, parented to the card slot.
///
/// Setup per seat:
///   - Create a World Space Canvas as a child of the card slot
///   - Set Canvas scale to 0.01, 0.01, 0.01
///   - Add a CanvasGroup component to the Canvas
///   - Add three TMP text objects: nameText, handTotalText, betText
///   - Attach this script to the Canvas
///   - Assign playerCamera, canvasGroup, and the three text fields in Inspector
/// </summary>
public class PlayerInfoLabel : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI handTotalText;
    public TextMeshProUGUI betText;

    [Header("Fade Settings")]
    [Tooltip("Dot product threshold for visibility. 0.9 = tight cone, 0.7 = wider cone.")]
    public float visibilityThreshold = 0.75f;
    public float fadeInSpeed = 4f;
    public float fadeOutSpeed = 6f;

    [Header("Always Visible")]
    [Tooltip("Enable for the dealer label — skips fade logic and stays fully visible.")]
    public bool alwaysVisible = false;

    // Data set by PlayerLabelManager each frame
    [HideInInspector] public string playerName = "";
    [HideInInspector] public int handTotal = 0;
    [HideInInspector] public float currentBet = 0f;
    [HideInInspector] public bool isDealer = false;

    void Update()
    {
        // Always face the player camera
        if (playerCamera != null)
            transform.rotation = Quaternion.LookRotation(
                transform.position - playerCamera.transform.position);

        // Update text content
        if (nameText != null)
            nameText.text = playerName;

        if (!isDealer)
        {
            if (handTotalText != null)
                handTotalText.text = $"Total: {handTotal}";
            if (betText != null)
                betText.text = currentBet > 0 ? $"Bet: ${currentBet}" : "Bet: --";
        }
        else
        {
            if (handTotalText != null)
                handTotalText.text = $"Total: {handTotal}";
            if (betText != null)
                betText.gameObject.SetActive(false);
        }

        // Fade logic
        if (alwaysVisible)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        float targetAlpha = IsLookingAt() ? 1f : 0f;
        float speed = targetAlpha > canvasGroup.alpha ? fadeInSpeed : fadeOutSpeed;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
    }

    bool IsLookingAt()
    {
        if (playerCamera == null) return false;
        Vector3 dirToLabel = (transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(playerCamera.transform.forward, dirToLabel);
        return dot >= visibilityThreshold;
    }
}