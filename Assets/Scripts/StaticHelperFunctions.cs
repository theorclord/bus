using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class StaticHelperFunctions
    {
        public static void ShuffleDeck(List<Card> deck)
        {
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                Card value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
        }

        public static List<Card> ResetCards()
        {
            // initialize deck of cards
            var deck = new List<Card>();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                for (int i = 0; i < 12; i++)
                {
                    deck.Add(
                        new Card()
                        {
                            Suit = s,
                            Value = i + 1
                        });
                }
            }

            // shuffle the deck
            StaticHelperFunctions.ShuffleDeck(deck);

            return deck;
        }
    }
}
