using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class Player {
        private static int flowID = 0;

        public class Face : TargetObject, Hurtable {
            public int hp { get; private set; }
            public string name { get; private set; }

            public bool TakeDamage(int damage) {
                hp -= damage;
                return hp <= 0;
            }

            public string GetDescription() {
                return name;
            }

            public Face(string name, int hp) {
                this.name = name;
                this.hp = hp;
            }
        }

        public class PublicInfo {
            public int currentCrystal;
            public int maximumCrystal;
            public int handNumber;
            public int deckNumber;
            public int cemeteryNumber;
            public List<BoardObject> board;
            public Face face;
            public int ID;
        }

        public int ID { get; private set; }
        public string nickname { get; private set; }
        public int currentCrystal { get; private set; }
        public int maximumCrystal { get; private set; }
        public int hp { get; private set; }

        private List<Card> hand = new List<Card>();
        private List<Card> cemetery = new List<Card>();
        private List<BoardObject> board = new List<BoardObject>();
        private IDeck deck = new TestDeck();

        private Screen screen = new Screen();

        public Player(string playerName, IDeck playerDeck) {
            ID = flowID++;
            nickname = playerName;
            deck = playerDeck;
            currentCrystal = maximumCrystal = 0;
            hp = 5;
        }

        private void InitiateATurn() {
            if (maximumCrystal < 10) ++maximumCrystal;
            currentCrystal = maximumCrystal;
            Draw();
            foreach (BoardObject obj in board) {
                if (obj is Monster) (obj as Monster).RestoreAttackNumber();
            }
        }

        private void EndATurn() {
            foreach (BoardObject obj in board) {
                if (obj is Monster) (obj as Monster).ExhaustAttackNumber();
            }
        }

        public InputHandler PrepareNthCard(Game game, int n, InputHandler success, InputHandler failure) {
            Card c = hand[n];

            return c.Prepare(game, this, new PlayNthCardHandler(n, success), failure);
        }

        public bool TryPlaceMonster(Monster m) {
            if (board.Count >= 5) return false;
            board.Add(m);
            return true;
        }

        public bool CanPlaceCard() {
            return board.Count < 5;
        }

        public bool Pay(int cost) {
            if (cost > currentCrystal) return false;
            currentCrystal -= cost;
            return true;
        }

        public Card RemoveHand(int i) {
            Card c = hand[i];
            hand.RemoveAt(i);
            return c;
        }

        public void PutCemetery(Card c) {
            cemetery.Add(c);
        }

        public bool Draw() {
            Card c = deck.DealACard();
            if (c == null) return false;
            if (hand.Count >= 9) {
                cemetery.Add(c);
            } else {
                hand.Add(c);
            }
            return true;
        }

        public void UpdateFace(Face newFace) {
            if (newFace != null) hp = newFace.hp;
        }

        public void UpdateBoard(int idx, BoardObject obj) {
            if (obj.IsDestroyed()) {
                cemetery.Add(board[idx].GetCard());
                board.RemoveAt(idx);
            } else {
                board[idx] = obj;
            }
        }

        public void UpdateBoard(List<int> idxs, List<BoardObject> objs) {
            for (int i = 0; i < idxs.Count; ++i) {
                board[idxs[i]] = objs[i];
            }
            for (int i = 0; i < board.Count; ++i) {
                if (board[i].IsDestroyed()) {
                    cemetery.Add(board[i].GetCard());
                    board.RemoveAt(i);
                    --i;
                }
            }
        }

        public string[] GetHandDescription() {
            string[] rtn = new string[hand.Count];
            for (int i = 0; i < hand.Count; ++i) {
                rtn[i] = hand[i].GetDescription();
            }
            return rtn;
        }

        public PublicInfo GetPublicInfo() {
            Player.PublicInfo rtn = new Player.PublicInfo();
            
            rtn.currentCrystal = this.currentCrystal;
            rtn.maximumCrystal = this.maximumCrystal;
            rtn.handNumber = hand.Count;
            rtn.deckNumber = deck.GetNumber();
            rtn.cemeteryNumber = cemetery.Count;
            rtn.board = new List<BoardObject>();
            rtn.face = new Face(nickname, hp);
            rtn.ID = this.ID;

            foreach (BoardObject obj in board) {
                rtn.board.Add(obj);
            }

            return rtn;
        }

        public void DoTurn(Game game) {
            InitiateATurn();

            Player.PublicInfo[] info = new Player.PublicInfo[] { GetPublicInfo(), game.GetOppositeInfo(ID) };
            screen.RefreshOpp(game.turnCount, info[1], false);
            screen.RefreshSelf(info[0], false);

            InputHandler handler = new HandHandler(GetHandDescription());

            while (handler != null && !game.IsEnd()) {
                if (handler is ScreenUpdater) (handler as ScreenUpdater).UpdateScreen(game, this, screen);
                screen.Repaint();
                handler = handler.Launch(game, this, screen);
            }

            if (game.IsEnd()) {
                Player.PublicInfo winnerInfo = game.GetWinner();
                info[0] = GetPublicInfo();
                info[1] = game.GetOppositeInfo(ID);
                screen.RefreshOpp(game.turnCount, info[1], false);
                screen.RefreshSelf(info[0], false);
                screen.ShowPopup(new string[] {
                    "Game set!!",
                    String.Format("The winner is {0}", winnerInfo.face.name),
                    (winnerInfo.ID == this.ID) ? "You win!!" : "You lose...."
                });
                screen.Repaint();
            }

            EndATurn();
        }
    }
}
