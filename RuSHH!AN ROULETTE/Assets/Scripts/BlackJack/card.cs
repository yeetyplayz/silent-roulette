using Unity.VisualScripting;
using UnityEngine;

public class card : MonoBehaviour
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
}