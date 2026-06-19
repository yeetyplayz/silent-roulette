using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Holds and manages the 52 card deck.
/// Shuffles at the start of each round and deals one card at a time.
/// Attach to DealingManager's GameObject.
/// </summary>
public class Deck : MonoBehaviour
{
    // Assign all 52 CardData entries in the Inspector
    public CardData[] allCards;

    private List<CardData> _drawPile = new List<CardData>();

    /// <summary>Shuffle a fresh deck ready for a new round.</summary>
    public void Shuffle()
    {
        _drawPile = new List<CardData>(allCards);

        // Fisher-Yates shuffle
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = _drawPile[i];
            _drawPile[i] = _drawPile[j];
            _drawPile[j] = temp;
        }

        Debug.Log($"[Deck] Shuffled. {_drawPile.Count} cards ready.");
    }

    /// <summary>Draw the top card from the pile.</summary>
    public CardData Draw()
    {
        if (_drawPile.Count == 0)
        {
            Debug.LogWarning("[Deck] Draw pile empty — reshuffling.");
            Shuffle();
        }

        CardData card = _drawPile[0];
        _drawPile.RemoveAt(0);
        return card;
    }

    public int CardsRemaining => _drawPile.Count;

    // -----------------------------------------------------------------------
    //  Editor utility — auto-fills allCards from your suit folders
    //  Right click the Deck component header in the Inspector and select
    //  "Auto-Populate Deck From Folders" to run this.
    // -----------------------------------------------------------------------
    [ContextMenu("Auto-Populate Deck From Folders")]
    public void AutoPopulateFromFolders()
    {
#if UNITY_EDITOR
        List<CardData> cards = new List<CardData>();

        // Adjust this path if your folder structure differs
        string basePath = "Assets/Prefabs/Cards";
        string[] suitFolders = { "Harten", "Schoppen", "Ruiten", "Klavers" };

        foreach (string suitName in suitFolders)
        {
            string folderPath = $"{basePath}/{suitName}";
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });

            if (guids.Length == 0)
            {
                Debug.LogWarning($"[Deck] No prefabs found in {folderPath}. Check the path.");
                continue;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                string cardName = prefab.name; // e.g. "HartenA", "Harten10"
                string rankCode = cardName.Substring(suitName.Length);

                CardData.Rank rank;
                switch (rankCode)
                {
                    case "A": rank = CardData.Rank.Ace; break;
                    case "J": rank = CardData.Rank.Jack; break;
                    case "Q": rank = CardData.Rank.Queen; break;
                    case "K": rank = CardData.Rank.King; break;
                    default:
                        if (int.TryParse(rankCode, out int num))
                            rank = (CardData.Rank)(num - 2); // Two = enum index 0
                        else
                        {
                            Debug.LogWarning($"[Deck] Could not parse rank from '{cardName}', skipping.");
                            continue;
                        }
                        break;
                }

                CardData.Suit suit = (CardData.Suit)System.Enum.Parse(typeof(CardData.Suit), suitName);

                cards.Add(new CardData
                {
                    suit = suit,
                    rank = rank,
                    prefab = prefab
                });
            }
        }

        allCards = cards.ToArray();
        Debug.Log($"[Deck] Auto-populated {allCards.Length} cards.");

        EditorUtility.SetDirty(this);
#else
        Debug.LogWarning("[Deck] Auto-populate only works in the editor.");
#endif
    }
}