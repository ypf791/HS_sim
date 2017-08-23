using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class Game {
        private Player[] player;
        private int turnPlayerIdx = 0;
        public int turnCount { get; private set; }
        private Player winner = null;
        private Screen screen = new Screen();

        public Game(Player p0, Player p1) {
            player = new Player[] { p0, p1 };
            turnCount = 0;
        }

        public void Start() {
            Start(player[new Random().Next() % 2].ID);
        }

        public void Start(int playerID) {
            turnPlayerIdx = (playerID == this.player[0].ID) ? 1 : 0;

            for (int j = 0; j < 3; ++j) {
                player[0].Draw();
                player[1].Draw();
            }
        }

        private Player turnPlayer() {
            return player[turnPlayerIdx];
        }

        public void NextTurn() {
            turnPlayerIdx = (turnPlayerIdx + 1) % player.Length;
            ++turnCount;
            
            turnPlayer().DoTurn(this);
        }

        public bool IsEnd() {
            return winner != null;
        }

        public Player.PublicInfo GetWinner() {
            return winner != null ? winner.GetPublicInfo() : null;
        }

        private Player GetPlayer(int playerID, bool wantSelf) {
            return (player[0].ID == playerID) ^ wantSelf ? player[1] : player[0];
        }

        public Player.PublicInfo GetOppositeInfo(int playerID) {
            return GetPlayer(playerID, false).GetPublicInfo();
        }

        public int GetOppositeID(int playerID) {
            return GetPlayer(playerID, false).ID;
        }

        public void Retire(int playerID) {
            winner = GetPlayer(playerID, false);
        }

        public void PlayerDraw(int playerID, int number) {
            foreach (Player p in player) {
                if (p.ID != playerID) continue;
                for (int i = 0; i < number; ++i) {
                    p.Draw();
                }
            }
        }

        public void UpdateTargetObject(int playerID, int targetPattern, TargetObject target, TargetFixer updater) {
            updater(target);

            Player player = GetPlayer(playerID, (targetPattern & Action.SELF) != 0);
            if ((targetPattern & Action.FACE) != 0) {
                Player.Face f = target as Player.Face;
                if (f.hp <= 0) {
                    winner = GetPlayer(player.ID, false);
                }
                player.UpdateFace(f);
            }
            if ((targetPattern & Action.BOARD) != 0) {
                player.UpdateBoard(targetPattern & Action.INDEX_MASK, target as BoardObject);
            }
        }

        public void UpdateAllTargetObject(int playerID, TargetFilter filter, TargetFixer updater) {
            Player.PublicInfo[] info = new Player.PublicInfo[] { player[0].GetPublicInfo(), player[1].GetPublicInfo() };
            int[] playerPattern;
            if (player[0].ID == playerID) playerPattern = new int[] { Action.SELF, Action.OPP };
            else playerPattern = new int[] { Action.OPP, Action.SELF };

            for (int i = 0; i < 2; ++i) {
                int facePattern = playerPattern[i] | Action.FACE;
                Player.Face face = info[i].face;
                if (filter(facePattern, face)) {
                    updater(face);
                    player[i].UpdateFace(face);
                }
                List<int> indices = new List<int>();
                List<BoardObject> objs = new List<BoardObject>();
                for (int j = 0; j < info[i].board.Count; ++j) {
                    int boardPattern = playerPattern[i] | Action.BOARD | (j & Action.INDEX_MASK);
                    BoardObject obj = info[i].board[j];
                    if (filter(boardPattern, obj)) {
                        updater(obj);
                        indices.Add(j);
                        objs.Add(obj);
                    }
                }
                if (indices.Count > 0) player[i].UpdateBoard(indices, objs);
            }
        }
    }
}
