using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the roulette phase. The dealer shoots each losing player one by one
/// in resolution order (whoever lost first gets shot first).
/// Each player has their own PlayerRevolver with their own bullet count.
/// The cylinder is respun fresh for each player.
/// </summary>
public class RouletteManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Seconds the human has to accept the shot before it fires automatically.")]
    public float forcedPullTimer = 8f;

    [Tooltip("Pause between each player being shot.")]
    public float timeBetweenShots = 1.5f;

    [Tooltip("How long the red death flash stays on screen.")]
    public float deathFlashDuration = 1.5f;

    [Header("References")]
    public RoundManager roundManager;
    public int humanPlayerIndex = 0;
    public UnityEngine.UI.Image deathFlashOverlay;

    public bool IsResolved { get; private set; } = true;

    private bool _humanAccepted = false;
    private bool _rouletteActive = false;
    private List<PlayerHand> _rouletteQueue = new List<PlayerHand>();

    void Start()
    {
        if (roundManager != null)
            roundManager.OnPlayerSentToRoulette += QueuePlayerForRoulette;
        else
            Debug.LogError("[RouletteManager] No RoundManager assigned.");
    }

    private void QueuePlayerForRoulette(PlayerHand player)
    {
        _rouletteQueue.Add(player);
    }

    public IEnumerator RunRoulettePhase()
    {
        if (_rouletteQueue.Count == 0)
        {
            IsResolved = true;
            yield break;
        }

        IsResolved = false;
        _rouletteActive = true;

        Debug.Log($"[RouletteManager] Roulette phase. Dealer will shoot {_rouletteQueue.Count} player(s).");

        // Shoot each player one by one in resolution order
        foreach (PlayerHand player in _rouletteQueue)
        {
            yield return StartCoroutine(ShootPlayer(player));
            yield return new WaitForSeconds(timeBetweenShots);
        }

        _rouletteQueue = new List<PlayerHand>();
        _rouletteActive = false;
        IsResolved = true;
    }

    IEnumerator ShootPlayer(PlayerHand player)
    {
        PlayerRevolver revolver = player.GetComponent<PlayerRevolver>();
        if (revolver == null)
        {
            Debug.LogWarning($"[RouletteManager] {player.playerName} has no PlayerRevolver component.");
            yield break;
        }

        bool isHuman = player.seatIndex == humanPlayerIndex;

        if (isHuman)
        {
            // Human gets a window to accept the shot manually
            _humanAccepted = false;
            Debug.Log($"[RouletteManager] The dealer is pointing the gun at you. Press F to accept the shot. ({forcedPullTimer}s)");

            float elapsed = 0f;
            while (!_humanAccepted && elapsed < forcedPullTimer)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_humanAccepted)
                Debug.Log("[RouletteManager] The dealer fires.");
            else
                Debug.Log("[RouletteManager] You accepted the shot.");
        }
        else
        {
            // Brief pause before the dealer shoots an AI player
            Debug.Log($"[RouletteManager] Dealer aims at {player.playerName}.");
            yield return new WaitForSeconds(1f);
        }

        // Pull the trigger
        bool died = revolver.PullTrigger();

        if (died)
        {
            Debug.Log($"[RouletteManager] {player.playerName} has been shot. Eliminated.");
            player.Eliminate();

            if (isHuman)
                yield return StartCoroutine(HumanDeathSequence());
        }
        else
        {
            Debug.Log($"[RouletteManager] {player.playerName} survives the shot. The chamber was empty.");
            player.SurviveRoulette();
        }
    }

    /// <summary>Called by DebugInput when the human presses F during roulette.</summary>
    public void HumanAcceptShot()
    {
        if (!_rouletteActive) return;
        _humanAccepted = true;
        Debug.Log("[RouletteManager] Human accepted the shot.");
    }

    IEnumerator HumanDeathSequence()
    {
        Debug.Log("[RouletteManager] You are dead. Game over.");

        if (deathFlashOverlay != null)
        {
            deathFlashOverlay.color = new Color(1f, 0f, 0f, 0.6f);
            float elapsed = 0f;
            while (elapsed < deathFlashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.6f, 0f, elapsed / deathFlashDuration);
                deathFlashOverlay.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }
            deathFlashOverlay.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    public void AddBulletsToSurvivors(PlayerHand[] allPlayers)
    {
        foreach (PlayerHand p in allPlayers)
        {
            if (p.IsEliminated) continue;
            PlayerRevolver revolver = p.GetComponent<PlayerRevolver>();
            if (revolver != null)
                revolver.AddBulletAndReshuffle();
        }
    }
}