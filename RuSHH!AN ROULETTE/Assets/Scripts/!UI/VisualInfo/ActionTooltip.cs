using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Floating world space tooltip that shows the last action a player took.
/// Fades in with a slight random tilt and stays until replaced.
/// Attach to a World Space Canvas above each player's card slot.
/// One per seat — do NOT attach to the human player's seat.
/// </summary>
public class ActionTooltip : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI actionText;

    [Header("Timing")]
    [Tooltip("Delay in seconds before the tooltip appears after an action.")]
    public float appearDelay = 0.1f;
    public float fadeInDuration = 0.25f;

    [Header("Tilt")]
    [Tooltip("Maximum random tilt in degrees applied on spawn.")]
    public float maxTiltDegrees = 12f;

    [Header("Lifespan")]
    public float displayDuration = 2.5f;
    public float fadeOutDuration = 0.3f;

    [Header("Pop Animation")]
    public float popInScale = 1.2f;  // scale it punches up to before settling at 1
    public float popOutScale = 0.8f;  // scale it shrinks to before disappearing

    private Coroutine _showCoroutine;
    private float _currentTilt = 0f;

    void Start()
    {
        Debug.Log($"[ActionTooltip] Start called. CanvasGroup null: {canvasGroup == null}");
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    void Update()
    {
        // Always face the player camera
        if (playerCamera != null)
            transform.rotation = Quaternion.LookRotation(
                transform.position - playerCamera.transform.position)
                * Quaternion.Euler(0f, 0f, _currentTilt);
    }

    /// <summary>
    /// Show a new action text. Cancels any in-progress fade and restarts.
    /// </summary>
    public void ShowAction(string text)
    {
        if (!gameObject.activeInHierarchy) return;
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = null;
        transform.localScale = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        _showCoroutine = StartCoroutine(AppearRoutine(text));
    }

    /// <summary>Hide the tooltip immediately.</summary>
    public void Hide()
    {
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    IEnumerator AppearRoutine(string text)
    {

        if (canvasGroup == null || actionText == null) yield break;
        Vector3 originalScale = transform.localScale;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(appearDelay);

        if (actionText != null) actionText.text = text;
        _currentTilt = Random.Range(-maxTiltDegrees, maxTiltDegrees);

        // Pop in — punch up to popInScale then settle at 1
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float scale = Mathf.Lerp(0f, popInScale, Mathf.SmoothStep(0f, 1f, t));
            transform.localScale = originalScale * scale;
            if (canvasGroup != null) canvasGroup.alpha = t;
            yield return null;
        }

        // Settle to exactly original scale
        transform.localScale = originalScale;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Hold for display duration
        yield return new WaitForSeconds(displayDuration);

        // Pop out — shrink to popOutScale while fading
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float scale = Mathf.Lerp(1f, popOutScale, t);
            transform.localScale = originalScale * scale;
            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        _showCoroutine = null;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

}