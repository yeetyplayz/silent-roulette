using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central game loop. Manages betting, turn cycling, dealer, roulette, and reward phases.
///
/// Physical seating layout:
///         Dealer
///    1            3
///       0      2
///
/// Anti-clockwise order: 0 -> 1 -> 3 -> 2 -> back to 0
/// </summary>
public class RoundManager : MonoBehaviour
{
    [Header("Players (assign in seat order 0-3)")]
    public PlayerHand[] players;
    public int humanPlayerIndex = 0;

    [Header("Dealer")]
    public DealerAI dealer;

    [Header("Roulette")]
    public RouletteManager rouletteManager;

    [Header("Betting")]
    public BettingManager bettingManager;
    public BettingUI bettingUI;

    private readonly int[] _physicalOrder = { 0, 1, 3, 2 };

    private bool _waitingForHuman;
    private bool _humanTurnActive = false;
    public DealingManager dealingManager;
    public bool useAceAsEleven = true;

    public System.Action<int> OnPlayerTurnStarted;
    public System.Action OnDealerTurnStarted;
    public System.Action<PlayerHand> OnPlayerSentToRoulette;
    public System.Action OnRoundComplete;
    public System.Action<PlayerHand> OnGameWon;

    void Start()
    {
        StartCoroutine(RunGame());
    }

    IEnumerator RunGame()
    {
        while (GetActivePlayers().Count > 1)
        {
            yield return StartCoroutine(RunRound());

            // Wait for roulette to fully resolve
            if (rouletteManager != null)
                yield return new WaitUntil(() => rouletteManager.IsResolved);

            // Add bullets to survivors
            if (rouletteManager != null)
                rouletteManager.AddBulletsToSurvivors(players);

            List<PlayerHand> remaining = GetActivePlayers();
            if (remaining.Count == 1)
            {
                Debug.Log($"[RoundManager] Game over. {remaining[0].playerName} wins!");
                OnGameWon?.Invoke(remaining[0]);
                yield break;
            }
        }
    }

    IEnumerator RunRound()
    {
        Debug.Log("[RoundManager] --- New Round ---");

        // Betting phase — opens UI, locks camera, waits for confirmation
        if (bettingUI != null)
            yield return StartCoroutine(bettingUI.OpenBettingPhase());

        yield return StartCoroutine(dealingManager.DealOpeningHands(players, dealer, useAceAsEleven));

        List<PlayerHand> active = GetActivePlayers();
        int startSeatIndex = Random.Range(0, active.Count);
        Debug.Log($"[RoundManager] Round starts at seat {active[startSeatIndex].seatIndex}.");

        while (!AllPlayersDone() || !dealer.IsDoneForRound)
        {
            active = GetActivePlayers();
            List<PlayerHand> cycleOrder = BuildTurnOrder(active, startSeatIndex);

            foreach (PlayerHand player in cycleOrder)
            {
                if (player.IsDoneForRound) continue;

                OnPlayerTurnStarted?.Invoke(player.seatIndex);

                if (player.seatIndex == humanPlayerIndex)
                {
                    _humanTurnActive = true;
                    _waitingForHuman = true;
                    yield return new WaitUntil(() => !_waitingForHuman);
                }
                else
                {
                    AIPlayerHand ai = player as AIPlayerHand;
                    if (ai != null)
                    {
                        yield return new WaitForSeconds(0.8f);
                        yield return StartCoroutine(ai.TakeTurn());
                    }
                }
            }

            if (!dealer.IsDoneForRound)
            {
                OnDealerTurnStarted?.Invoke();
                yield return new WaitForSeconds(0.8f);
                dealer.TakeOneAction();
            }
        }

        // Resolve hands and collect winners/losers for betting
        List<PlayerHand> winners = new List<PlayerHand>();
        List<PlayerHand> losers = new List<PlayerHand>();
        yield return StartCoroutine(ResolveRound(winners, losers));

        // Resolve bets
        if (bettingManager != null)
            bettingManager.ResolveBets(winners, losers, dealer.isBust);

        ResetAllHands();

        // Roulette phase
        if (rouletteManager != null)
            yield return rouletteManager.StartCoroutine(rouletteManager.RunRoulettePhase());

        OnRoundComplete?.Invoke();
        if (dealingManager != null) dealingManager.ClearAllCards();
    }

    void DealOpeningHands()
    {
        dealer.DealOpeningHand();

        foreach (PlayerHand p in players)
        {
            if (p.IsEliminated) continue;
            p.ResetHand();
            for (int i = 0; i < 2; i++)
            {
                int card = Random.Range(1, 12);
                p.AddCard(card);
            }
            Debug.Log($"[{p.playerName}] Opening hand: {p.handTotal}.");
        }
    }

    public void HumanHit()
    {
        if (!_waitingForHuman || !_humanTurnActive) return;
        _humanTurnActive = false;
        PlayerHand human = players[humanPlayerIndex];
        Debug.Log($"[{human.playerName}] hits.");
        StartCoroutine(dealingManager.DealHitCard(human, useAceAsEleven));
        _waitingForHuman = false;
    }

    public void HumanStand()
    {
        if (!_waitingForHuman || !_humanTurnActive) return;
        _humanTurnActive = false;
        players[humanPlayerIndex].Stand();
        _waitingForHuman = false;
    }

    IEnumerator ResolveRound(List<PlayerHand> winners, List<PlayerHand> losers)
    {
        foreach (PlayerHand player in players)
        {
            if (player.IsEliminated) continue;

            // Skip spectators
            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            if (wallet != null && wallet.IsSpectating) continue;

            bool playerWins = dealer.PlayerWinsAgainstDealer(player);

            if (playerWins)
            {
                Debug.Log($"[{player.playerName}] wins this round (hand: {player.handTotal} vs dealer: {dealer.handTotal}).");
                winners.Add(player);
            }
            else
            {
                Debug.Log($"[{player.playerName}] loses this round — sent to roulette.");
                losers.Add(player);
                OnPlayerSentToRoulette?.Invoke(player);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    void ResetAllHands()
    {
        dealer.ResetHand();
        foreach (PlayerHand p in players)
            if (!p.IsEliminated) p.ResetHand();
    }


    bool AllPlayersDone()
    {
        foreach (PlayerHand p in players)
        {
            if (p.IsEliminated) continue;
            PlayerWallet wallet = p.GetComponent<PlayerWallet>();
            if (wallet != null && wallet.IsSpectating) continue;
            if (!p.IsDoneForRound) return false;
        }
        return true;
    }

    List<PlayerHand> GetActivePlayers()
    {
        List<PlayerHand> active = new List<PlayerHand>();
        foreach (PlayerHand p in players)
        {
            if (p.IsEliminated) continue;
            // Skip spectators
            PlayerWallet wallet = p.GetComponent<PlayerWallet>();
            if (wallet != null && wallet.IsSpectating) continue;
            active.Add(p);
        }
        return active;
    }


    List<PlayerHand> BuildTurnOrder(List<PlayerHand> active, int startSeatIndex)
    {
        int startPos = System.Array.IndexOf(_physicalOrder, active[startSeatIndex].seatIndex);

        List<PlayerHand> order = new List<PlayerHand>();
        int count = _physicalOrder.Length;

        for (int i = 0; i < count; i++)
        {
            int physicalPos = (startPos + i) % count;
            int seatIdx = _physicalOrder[physicalPos];

            foreach (PlayerHand p in active)
            {
                if (p.seatIndex == seatIdx)
                {
                    order.Add(p);
                    break;
                }
            }
        }

        return order;
    }

    public void RestartGame()
    {
        // Reset revolvers
        foreach (PlayerHand player in players)
        {
            PlayerRevolver revolver = player.GetComponent<PlayerRevolver>();
            if (revolver != null) revolver.ResetRevolver();
        }

        Debug.Log("[RoundManager] Restarting game.");
        StartCoroutine(RunGame());
    }
}