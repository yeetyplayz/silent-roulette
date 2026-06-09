using System.Collections.Generic;
using NUnit.Framework.Internal.Builders;
using Unity.VisualScripting;
using UnityEngine;

public class deck : MonoBehaviour
{
    public enum Suit
    {
        Heart,
        Diamond,
        Clubs,
        Spade
    }
    public enum Rank
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }
    public class Card
    {
        public Suit suit;
        public Rank rank;
        public Card(Suit s, Rank r)
        {
            suit = s;
            rank = r;
        }
        public int GetValue()
        {
            if ((int)rank >= 11)
                return 10;
            return (int)rank;
        }
    }
    public class Deck
    {
        private List<Card> cards = new List<Card>();
        public Deck()
        {
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }
        }
        public void Shuffle()
        {
            for (int i = 0; i <cards.Count; i++)
            {
                int randomIndex = Random.Range(i, cards.Count);

                Card temp = cards[i];
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }
        public Card DrawCard()
        {
            Card card = cards[0];
            cards.Remove(card);
            return card;
        }
    }
}