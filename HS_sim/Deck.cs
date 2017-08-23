using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    interface IDeck {
        Card DealACard();
        void Shuffle();
        int GetNumber();
    }

    class Deck : IDeck {
        protected List<Card> deck = new List<Card>();

        public void Shuffle() {
            Random rand = new Random();
            for (int i = 0; i < deck.Count; ++i) {
                int toSwap = i + rand.Next() % (deck.Count - i);
                Card tmp = deck[i];
                deck[i] = deck[toSwap];
                deck[toSwap] = tmp;
            }
        }

        public Card DealACard() {
            if (deck.Count == 0) return null;
            Card rtn = deck[0];
            deck.RemoveAt(0);
            return rtn;
        }

        public int GetNumber() {
            return deck.Count;
        }
    }

    class TestDeck : Deck {
        public TestDeck() {
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());
            deck.Add(new MCard_MerlocTidehunter());
            deck.Add(new SCard_AcraneExplosion());
            deck.Add(new SCard_DivineSpirit());
            deck.Add(new SCard_ArcaneIntellect());
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());
            deck.Add(new SCard_ArcaneIntellect());
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());
            deck.Add(new MCard_MerlocTidehunter());
            deck.Add(new SCard_DivineSpirit());
            deck.Add(new SCard_ArcaneIntellect());
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());
            deck.Add(new SCard_ArcaneIntellect());
            deck.Add(new MCard_Goblin());
            deck.Add(new MCard_AzureDrake());

            // Shuffle();
        }
    }
}
