using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates dealing cards to all players and the dealer.
/// Replaces the random integer system in RoundManager with real card data.
/// One card visible per player at a time — previous card is destroyed on new draw.
///
/// Setup:
///   - Assign playerSlots in seat order (0-3) matching RoundManager.players
///   - Assign dealerSlot
///   - Assign playerCamera (the main camera)
///   - Assign deck
/// </summary>
public class DealingManager : MonoBehaviour
{
    [Header("References")]
    public Deck deck;
    public Camera playerCamera;

    [Header("Slots (assign in seat order 0-3)")]
    public CardSlot[] playerSlots;
    public CardSlot dealerSlot;

    [Header("Timing")]
    [Tooltip("Delay between each card being dealt during the opening hand.")]
    public float dealDelay = 0.4f;

    // Tracks the currently visible card per seat (-1 = dealer)
    private Dictionary<int, GameObject> _activeCards = new Dictionary<int, GameObject>();

    // -----------------------------------------------------------------------
    //  Public API — called by RoundManager
    // -----------------------------------------------------------------------

    /// <summary>
    /// Deal opening hands — 2 cards each to all active players and dealer.
    /// Returns the card value dealt for RoundManager to apply to hand totals.
    /// </summary>
    public IEnumerator DealOpeningHands(PlayerHand[] players, DealerAI dealer, bool useAceAsEleven)
    {
        deck.Shuffle();

        // Deal first card to each player then dealer, then second card each
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (PlayerHand player in players)
            {
                if (player.IsEliminated) continue;
                CardData card = deck.Draw();
                int value = ResolveValue(card, player.handTotal, useAceAsEleven);
                player.AddCard(value);
                yield return StartCoroutine(SpawnCard(card, playerSlots[player.seatIndex], player.seatIndex));
                yield return new WaitForSeconds(dealDelay);
            }

            // Dealer card
            CardData dealerCard = deck.Draw();
            int dealerValue = ResolveValue(dealerCard, dealer.handTotal, useAceAsEleven);
            dealer.handTotal += dealerValue;
            yield return StartCoroutine(SpawnCard(dealerCard, dealerSlot, -1));
            yield return new WaitForSeconds(dealDelay);
        }
    }

    /// <summary>
    /// Deal a single hit card to a player mid-round.
    /// </summary>
    public IEnumerator DealHitCard(PlayerHand player, bool useAceAsEleven)
    {
        CardData card = deck.Draw();
        int value = ResolveValue(card, player.handTotal, useAceAsEleven);
        player.AddCard(value);
        yield return StartCoroutine(SpawnCard(card, playerSlots[player.seatIndex], player.seatIndex));
    }

    /// <summary>
    /// Deal a single hit card to the dealer.
    /// </summary>
    public IEnumerator DealDealerCard(DealerAI dealer, bool useAceAsEleven)
    {
        CardData card = deck.Draw();
        int value = ResolveValue(card, dealer.handTotal, useAceAsEleven);
        dealer.handTotal += value;
        yield return StartCoroutine(SpawnCard(card, dealerSlot, -1));
    }

    /// <summary>
    /// Clear all visible cards from the table.
    /// Called at the end of each round.
    /// </summary>
    public void ClearAllCards()
    {
        foreach (var kvp in _activeCards)
            if (kvp.Value != null) Destroy(kvp.Value);
        _activeCards.Clear();
    }

    // -----------------------------------------------------------------------
    //  Internal
    // -----------------------------------------------------------------------

    IEnumerator SpawnCard(CardData card, CardSlot slot, int seatIndex)
    {
        // Destroy the previous card at this seat
        if (_activeCards.ContainsKey(seatIndex) && _activeCards[seatIndex] != null)
            Destroy(_activeCards[seatIndex]);

        // Instantiate the new card
        GameObject cardObj = Instantiate(card.prefab);

        // Disable physics influence entirely — this card is fully script-driven.
        // Without this, a non-kinematic Rigidbody on the prefab will fight our
        // transform control: gravity pulls it through the table, and collision
        // response overrides the rotation we set from the slot.
        Rigidbody rb = cardObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CardVisual visual = cardObj.AddComponent<CardVisual>();
        _activeCards[seatIndex] = cardObj;

        Debug.Log($"[DealingManager] Dealt {card} to seat {seatIndex}.");

        yield return StartCoroutine(visual.PlayDrop(slot, playerCamera));
    }

    /// <summary>
    /// Resolve the actual integer value of a card given the current hand total.
    /// Handles ace as 11 or 1 depending on bust risk.
    /// </summary>
    int ResolveValue(CardData card, int currentTotal, bool useAceAsEleven)
    {
        int baseValue = card.BaseValue();

        if (card.rank == CardData.Rank.Ace && useAceAsEleven)
            return (currentTotal + 11 <= 21) ? 11 : 1;

        return baseValue;
    }
}