using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the snap-drop animation for a single card.
/// Instantiated by DealingManager, plays the fall, then idles at the slot.
/// </summary>
public class CardVisual : MonoBehaviour
{
    [Header("Drop Settings")]
    public float dropHeight = 3f;   // how high above the slot the card spawns
    public float dropDuration = 0.12f; // how fast it snaps down — keep this snappy

    [Header("Camera Shake")]
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.03f;

    /// <summary>
    /// Play the drop animation onto the given slot.
    /// Camera reference is used for the shake on landing.
    /// </summary>
    public IEnumerator PlayDrop(CardSlot slot, Camera cam)
    {
        // Spawn above the slot
        Vector3 landingPos = slot.transform.position;
        Vector3 startPos = landingPos + Vector3.up * dropHeight;

        transform.position = startPos;
        transform.rotation = slot.transform.rotation * Quaternion.Euler(90f, 0f, 0f);

        // Snap down
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            transform.position = Vector3.Lerp(startPos, landingPos, t);
            yield return null;
        }

        transform.position = landingPos;
        transform.rotation = slot.transform.rotation * Quaternion.Euler(-90f, 0f, 0f);

        // Camera shake on landing
        if (cam != null)
            yield return StartCoroutine(ShakeCamera(cam));
    }

    IEnumerator ShakeCamera(Camera cam)
    {
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            cam.transform.localPosition = new Vector3(
                originalPos.x + x,
                originalPos.y + y,
                originalPos.z);
            yield return null;
        }

        cam.transform.localPosition = originalPos;
    }
}