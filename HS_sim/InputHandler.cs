using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    interface InputHandler {
        InputHandler Launch(Game game, Player player, Screen screen);
    }

    interface ScreenUpdater {
        void UpdateScreen(Game game, Player player, Screen screen);
    }

    class CmdHandler : InputHandler {
        InputHandler cache;

        public CmdHandler(InputHandler handler) {
            cache = handler;
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            screen.ShowPrompt();
            string cmd = Console.ReadLine();
            if (cmd == "retire") {
                game.Retire(player.ID);
                return null;
            } else if (cmd == "end") {
                return null;
            }
            screen.HidePrompt();
            return cache;
        }
    }

    class HandHandler : InputHandler, ScreenUpdater {
        private string[] hand;
        private int focus;

        public HandHandler(string[] handDescription) {
            this.hand = handDescription;
            focus = 0;
        }

        public HandHandler(string[] handDescription, int focus) {
            this.hand = handDescription;
            this.focus = focus;
        }

        public void UpdateScreen(Game game, Player player, Screen screen) {
            if (focus >= hand.Length) focus = hand.Length - 1;
            if (focus < 0) focus = 0;
            if (hand.Length > 0) {
                screen.RefreshHand(hand.Length, focus, hand[focus]);
            } else {
                screen.RefreshHand(0, 0, null);
            }
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape) {
                return new CmdHandler(this);
            } else if (keyInfo.Key == ConsoleKey.RightArrow) {
                if (focus < hand.Length - 1) ++focus;
            } else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                if (focus > 0) --focus;
            } else if (keyInfo.Key == ConsoleKey.UpArrow) {
                Player.PublicInfo[] info = new Player.PublicInfo[] { player.GetPublicInfo(), game.GetOppositeInfo(player.ID) };
                if (info[0].board.Count > 0) return new SelfBoardHandler(info[0], focus);
                if (info[1].board.Count > 0) return new OppBoardHandler(info[1], focus);
                return this;
            } else if (keyInfo.Key == ConsoleKey.Enter) {
                return player.PrepareNthCard(game, focus, new RefreshHandler(focus), this);
            }

            return this;
        }
    }

    class SelfBoardHandler : InputHandler, ScreenUpdater {
        private Player.PublicInfo selfInfo;
        private int focus;

        public SelfBoardHandler(Player.PublicInfo selfInfo) {
            this.selfInfo = selfInfo;
            focus = 0;
        }

        public SelfBoardHandler(Player.PublicInfo selfInfo, int preferFocus) {
            this.selfInfo = selfInfo;
            focus = preferFocus;
            if (focus >= selfInfo.board.Count) focus = selfInfo.board.Count - 1;
            if (focus < 0) focus = 0;
        }

        public void UpdateScreen(Game game, Player player, Screen screen) {
            screen.RefreshSelf(selfInfo, false, focus);
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape) {
                return new CmdHandler(this);
            } else if (keyInfo.Key == ConsoleKey.DownArrow) {
                HandHandler rtn = new HandHandler(player.GetHandDescription(), focus);
                focus = -1;
                UpdateScreen(game, player, screen);
                return rtn;
            } else if (keyInfo.Key == ConsoleKey.UpArrow) {
                Player.PublicInfo info = game.GetOppositeInfo(player.ID);
                if (info.board.Count > 0) {
                    OppBoardHandler rtn = new OppBoardHandler(info, focus);
                    focus = -1;
                    UpdateScreen(game, player, screen);
                    return rtn;
                }
            } else if (keyInfo.Key == ConsoleKey.RightArrow) {
                if (focus < selfInfo.board.Count - 1) ++focus;
            } else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                if (focus > 0) --focus;
            } else if (keyInfo.Key == ConsoleKey.Enter) {
                Monster monster = selfInfo.board[focus] as Monster;
                if (monster != null && monster.CanAttack()) {
                    AttackTargetAction attackAction = new AttackTargetAction(Action.SELF | Action.BOARD | (focus & Action.INDEX_MASK), monster);

                    Player.PublicInfo[] info = new Player.PublicInfo[] { player.GetPublicInfo(), game.GetOppositeInfo(player.ID) };
                    return new ChooseTargetHandler(info, attackAction, new ActionChainHandler(attackAction, new RefreshHandler(focus), null), this);
                }
            }

            return this;
        }
    }

    class OppBoardHandler : InputHandler, ScreenUpdater {
        private Player.PublicInfo oppInfo;
        private int focus;

        public OppBoardHandler(Player.PublicInfo oppInfo) {
            this.oppInfo = oppInfo;
            focus = 0;
        }

        public OppBoardHandler(Player.PublicInfo oppInfo, int preferFocus) {
            this.oppInfo = oppInfo;
            focus = preferFocus;
            if (focus >= oppInfo.board.Count) focus = oppInfo.board.Count - 1;
            if (focus < 0) focus = 0;
        }

        public void UpdateScreen(Game game, Player player, Screen screen) {
            screen.RefreshOpp(game.turnCount, oppInfo, false, focus);
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape) {
                return new CmdHandler(this);
            } else if (keyInfo.Key == ConsoleKey.DownArrow) {
                focus = -1;
                UpdateScreen(game, player, screen);
                Player.PublicInfo info = player.GetPublicInfo();
                if (info.board.Count > 0) {
                    return new SelfBoardHandler(info);
                } else {
                    return new HandHandler(player.GetHandDescription());
                }
            } else if (keyInfo.Key == ConsoleKey.RightArrow) {
                if (focus < oppInfo.board.Count - 1) ++focus;
            } else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                if (focus > 0) --focus;
            }

            return this;
        }
    }

    class RefreshHandler : InputHandler {
        int focus;

        public RefreshHandler(int focus) {
            this.focus = focus;
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            Player.PublicInfo[] info = new Player.PublicInfo[] { player.GetPublicInfo(), game.GetOppositeInfo(player.ID) };
            screen.RefreshOpp(game.turnCount, info[1], false);
            screen.RefreshSelf(info[0], false);

            return new HandHandler(player.GetHandDescription(), focus);
        }
    }

    class PlayNthCardHandler : InputHandler {
        InputHandler cache;
        int playIdx;

        public PlayNthCardHandler(int n, InputHandler handler) {
            cache = handler;
            playIdx = n;
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            Card c = player.RemoveHand(playIdx);
            return c.Play(game, player, cache);
        }
    }

    class ActionChainHandler : InputHandler {
        InputHandler success;
        InputHandler failure;
        Action startAction;

        public ActionChainHandler(Action chainHead, InputHandler success, InputHandler failure) {
            this.startAction = chainHead;
            this.success = success;
            this.failure = failure;
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            Action a = startAction;
            while (a != null) {
                InputHandler rtn = a.DoAction(game, player, success, failure);
                if (rtn != success) return rtn;
                a = a.next;
            }
            return success;
        }
    }

    class ChooseTargetHandler : InputHandler, ScreenUpdater {
        private NeedTargetAction action;
        private InputHandler success;
        private InputHandler failure;

        private Player.PublicInfo[] info;

        // 0 for self, 1 for opp
        private List<int>[] indexList = new List<int>[] { new List<int>(), new List<int>() };
        // 2x + i for indexList[i][x]
        private int focusIndex = -1;

        public ChooseTargetHandler(Player.PublicInfo[] info, NeedTargetAction action, InputHandler success, InputHandler failure) {
            this.info = info;
            this.action = action;
            this.success = success;
            this.failure = failure;

            TargetFilter filter = action.filter;
            int[] playerPattern = new int[] { Action.SELF, Action.OPP };
            for (int i = 0; i < 2; ++i) {
                if (filter(playerPattern[i] | Action.FACE, info[i].face)) indexList[i].Add(-1);
                for (int j = 0; j < info[i].board.Count; ++j) {
                    if (filter(playerPattern[i] | Action.BOARD | (j & Action.INDEX_MASK), info[i].board[j])) {
                        indexList[i].Add(j);
                    }
                }
            }
            
            if (indexList[0].Count > 0) {
                focusIndex = 0;
            } else if (indexList[1].Count > 0) {
                focusIndex = 1;
            }
        }

        public void UpdateScreen(Game game, Player player, Screen screen) {
            int trueIdx = indexList[focusIndex & 1][focusIndex >> 1];
            if ((focusIndex & 1) == 0) {
                screen.RefreshSelf(info[0], trueIdx == -1, trueIdx);
            } else {
                screen.RefreshOpp(game.turnCount, info[1], trueIdx == -1, trueIdx);
            }
        }

        private void updateFocusByUpOrDown(int alpha) {
            int trueIdx = indexList[focusIndex & 1][focusIndex >> 1];
            if ((focusIndex & 1) == alpha) {
                if (trueIdx != -1 && indexList[alpha][0] == -1) {
                    focusIndex = alpha;
                }
            } else {
                if (trueIdx != -1 || indexList[1 - alpha].Count == 1) {
                    if (indexList[alpha].Count > 0) {
                        focusIndex = alpha;
                        if (indexList[alpha].Count > 1 && indexList[alpha][0] == -1) focusIndex = 2 + alpha;
                    }
                } else {
                    focusIndex = 3 - alpha;
                }
            }
        }

        public InputHandler Launch(Game game, Player player, Screen screen) {
            if (focusIndex == -1) return failure;
            
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape) {
                screen.RefreshSelf(info[0], false);
                screen.RefreshOpp(game.turnCount, info[1], false);
                return failure;
            } else if (keyInfo.Key == ConsoleKey.DownArrow) {
                int originFocusIndex = focusIndex;
                updateFocusByUpOrDown(0);
                if ((originFocusIndex & 1) == 1 && (focusIndex & 1) == 0) screen.RefreshOpp(game.turnCount, info[1], false);
            } else if (keyInfo.Key == ConsoleKey.UpArrow) {
                int originFocusIndex = focusIndex;
                updateFocusByUpOrDown(1);
                if ((originFocusIndex & 1) == 0 && (focusIndex & 1) == 1) screen.RefreshSelf(info[0], false);
            } else if (keyInfo.Key == ConsoleKey.RightArrow) {
                List<int> list = indexList[focusIndex & 1];
                if (list[focusIndex >> 1] != -1 && (focusIndex >> 1) < list.Count - 1) focusIndex += 2;
            } else if (keyInfo.Key == ConsoleKey.LeftArrow) {
                List<int> list = indexList[focusIndex & 1];
                int idx = focusIndex >> 1;
                if (list[idx] != -1 && idx > 0 && list[idx - 1] != -1) focusIndex -= 2;
            } else if (keyInfo.Key == ConsoleKey.Enter) {
                int trueIdx = indexList[focusIndex & 1][focusIndex >> 1];
                int targetPattern = trueIdx & Action.INDEX_MASK;
                targetPattern |= ((focusIndex & 1) == 0) ? Action.SELF : Action.OPP;
                if (trueIdx == -1) {
                    action.PutTarget(targetPattern | Action.FACE, info[focusIndex & 1].face);
                } else {
                    action.PutTarget(targetPattern | Action.BOARD, info[focusIndex & 1].board[trueIdx]);
                }
                return success;
            }

            return this;
        }
    }
}
