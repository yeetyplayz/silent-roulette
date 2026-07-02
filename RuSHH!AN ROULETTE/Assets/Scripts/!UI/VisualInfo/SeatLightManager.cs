using UnityEngine;

/// <summary>
/// Manages which seat light is active based on whose turn it is.
/// Assign one SeatLight per seat in seat order matching RoundManager.players.
/// Subscribe to RoundManager.OnPlayerTurnStarted.
/// </summary>
public class SeatLightManager : MonoBehaviour
{
    [Header("Seat Lights (assign in seat order matching RoundManager.players)")]
    public SeatLight[] seatLights;

    [Header("References")]
    public RoundManager roundManager;

    private int _currentActiveSeat = -1;

    void Start()
    {
        if (roundManager != null)
            roundManager.OnPlayerTurnStarted += OnTurnStarted;

        // All lights off at start
        foreach (SeatLight light in seatLights)
            if (light != null) light.TurnOff();
    }

    void OnDestroy()
    {
        if (roundManager != null)
            roundManager.OnPlayerTurnStarted -= OnTurnStarted;
    }

    void OnTurnStarted(int seatIndex)
    {
        // Flicker off the previous seat's light
        if (_currentActiveSeat >= 0 && _currentActiveSeat < seatLights.Length)
            if (seatLights[_currentActiveSeat] != null)
                seatLights[_currentActiveSeat].TurnOff();

        // Turn on the new seat's light instantly
        if (seatIndex >= 0 && seatIndex < seatLights.Length)
            if (seatLights[seatIndex] != null)
                seatLights[seatIndex].TurnOn();

        _currentActiveSeat = seatIndex;
    }

    /// <summary>Call this at the end of a round to turn all lights off.</summary>
    public void TurnAllOff()
    {
        foreach (SeatLight light in seatLights)
            if (light != null) light.TurnOff();
        _currentActiveSeat = -1;
    }
}