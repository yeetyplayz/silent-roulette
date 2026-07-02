using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a single seat's overhead light and emissive cone mesh.
/// Attach to each seat's light GameObject.
/// The light turns on instantly when it's that seat's turn.
/// When the turn ends, it flickers off with a quick double-pulse.
/// </summary>
public class SeatLight : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The HDRP Point or Spot Light component on this seat.")]
    public Light seatLight;

    [Tooltip("The emissive cone mesh renderer above the seat.")]
    public Renderer coneMesh;

    [Header("Light Settings")]
    public float activeIntensity = 3f;
    public Color lightColor = new Color(1f, 0.75f, 0.3f); // warm amber

    [Header("Flicker Off Settings")]
    [Tooltip("Duration of the flicker-off sequence in seconds.")]
    public float flickerDuration = 0.6f;
    [Tooltip("How many quick pulses before the light dies.")]
    public int flickerPulses = 2;

    [Header("Cone Mesh Settings")]
    public float coneActiveAlpha = 0.18f;
    public float coneInactiveAlpha = 0f;

    private Material _coneMaterial;
    private Coroutine _flickerCoroutine;

    void Awake()
    {
        if (seatLight != null)
        {
            seatLight.color = lightColor;
            seatLight.intensity = 0f;
            seatLight.enabled = false;
        }

        if (coneMesh != null)
        {
            // Instance the material so each seat is independent
            _coneMaterial = coneMesh.material;
            SetConeAlpha(coneInactiveAlpha);
        }
    }

    /// <summary>Turn this seat's light on instantly.</summary>
    public void TurnOn()
    {
        if (_flickerCoroutine != null)
        {
            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        if (seatLight != null)
        {
            seatLight.enabled = true;
            seatLight.intensity = activeIntensity;
        }

        SetConeAlpha(coneActiveAlpha);
    }

    /// <summary>Flicker this seat's light off.</summary>
    public void TurnOff()
    {
        if (_flickerCoroutine != null)
            StopCoroutine(_flickerCoroutine);
        _flickerCoroutine = StartCoroutine(FlickerOff());
    }

    IEnumerator FlickerOff()
    {
        float pulseTime = flickerDuration / (flickerPulses * 2f);

        for (int i = 0; i < flickerPulses; i++)
        {
            // Off
            if (seatLight != null) seatLight.intensity = 0f;
            SetConeAlpha(0f);
            yield return new WaitForSeconds(pulseTime);

            // On
            if (seatLight != null) seatLight.intensity = activeIntensity * 0.5f;
            SetConeAlpha(coneActiveAlpha * 0.5f);
            yield return new WaitForSeconds(pulseTime);
        }

        // Fully off
        if (seatLight != null)
        {
            seatLight.intensity = 0f;
            seatLight.enabled = false;
        }
        SetConeAlpha(0f);
        _flickerCoroutine = null;
    }

    void SetConeAlpha(float alpha)
    {
        if (_coneMaterial == null) return;
        Color c = _coneMaterial.color;
        c.a = alpha;
        _coneMaterial.color = c;

        // Also set emissive intensity for HDRP
        if (_coneMaterial.HasProperty("_EmissiveColor"))
        {
            Color emissive = lightColor * (alpha / coneActiveAlpha);
            _coneMaterial.SetColor("_EmissiveColor", emissive);
        }
    }
}