using UnityEngine;

/// <summary>
/// Defines a single card's suit, rank, and base value.
/// Assign one entry per card in DealingManager's Inspector array.
/// </summary>
[System.Serializable]
public class CardData
{
    public enum Suit { Harten, Schoppen, Ruiten, Klavers }
    public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public Suit suit;
    public Rank rank;
    public GameObject prefab;

    /// <summary>
    /// Base integer value of the card.
    /// Ace returns 11 by default — hand logic handles reducing to 1 if needed.
    /// </summary>
    public int BaseValue()
    {
        switch (rank)
        {
            case Rank.Ace: return 11;
            case Rank.Jack:
            case Rank.Queen:
            case Rank.King: return 10;
            default: return (int)rank + 2; // Two = 0 + 2, Three = 1 + 2, etc.
        }
    }

    public override string ToString() => $"{rank} of {suit}";
}