using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the roulette phase. All players sent to roulette pull simultaneously.
/// Human player has a time window to pull voluntarily before it fires automatically.
/// Signals RoundManager when the phase is fully complete via IsResolved.
/// </summary>
public class RouletteManager : MonoBehaviour
{
    [Header("Settings")]
    public float forcedPullTimer = 8f;
    public float deathFlashDuration = 1.5f;

    [Header("References")]
    public RoundManager roundManager;
    public int humanPlayerIndex = 0;
    public UnityEngine.UI.Image deathFlashOverlay;

    // RoundManager polls this to know when roulette is done
    public bool IsResolved { get; private set; } = true;

    private bool _humanPulled = false;
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

    /// <summary>
    /// Called by RoundManager after resolve. Runs the roulette phase and
    /// sets IsResolved to false until all pulls are done.
    /// </summary>
    public IEnumerator RunRoulettePhase()
    {
        if (_rouletteQueue.Count == 0)
        {
            IsResolved = true;
            yield break;
        }

        IsResolved = false;
        _rouletteActive = true;

        bool humanInvolved = false;
        foreach (PlayerHand p in _rouletteQueue)
        {
            if (p.seatIndex == humanPlayerIndex)
            {
                humanInvolved = true;
                break;
            }
        }

        Debug.Log($"[RouletteManager] Roulette phase. {_rouletteQueue.Count} player(s) pulling.");

        if (humanInvolved)
        {
            _humanPulled = false;
            Debug.Log($"[RouletteManager] Pull the trigger! ({forcedPullTimer}s before forced pull). Press F.");

            float elapsed = 0f;
            while (!_humanPulled && elapsed < forcedPullTimer)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!_humanPulled)
                Debug.Log("[RouletteManager] Time's up — forced pull.");
        }

        // All queued players pull simultaneously
        foreach (PlayerHand player in _rouletteQueue)
        {
            PlayerRevolver revolver = player.GetComponent<PlayerRevolver>();
            if (revolver == null)
            {
                Debug.LogWarning($"[RouletteManager] {player.playerName} has no PlayerRevolver component.");
                continue;
            }

            bool died = revolver.PullTrigger();

            if (died)
            {
                Debug.Log($"[RouletteManager] {player.playerName} is dead.");
                player.Eliminate();

                if (player.seatIndex == humanPlayerIndex)
                    yield return StartCoroutine(HumanDeathSequence());
            }
            else
            {
                Debug.Log($"[RouletteManager] {player.playerName} survived the pull.");
                player.SurviveRoulette();
            }
        }

        _rouletteQueue = new List<PlayerHand>();
        _rouletteActive = false;
        IsResolved = true;
    }

    public void HumanPullTrigger()
    {
        if (!_rouletteActive) return;
        _humanPulled = true;
        Debug.Log("[RouletteManager] Human pulled the trigger.");
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