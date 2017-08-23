using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class ActionChain {
        private Action head;
        private Action tail;

        public ActionChain(Action first) {
            head = first;
            tail = head;
            while (tail.next != null) tail = tail.next;
        }

        public ActionChain Append(Action a) {
            tail.next = a;
            tail = tail.next;
            return this;
        }

        public InputHandler InvokeWithBranch(Game game, Player player, InputHandler success, InputHandler failure) {
            return new ActionChainHandler(head, success, failure);
        }

        public InputHandler InvokeWithFinal(Game game, Player player, InputHandler final) {
            return new ActionChainHandler(head, final, final);
        }
    }

    abstract class Action {
        public static int INDEX_MASK = 0x0F;
        public static int SELF = 0x80;
        public static int OPP = 0x40;
        public static int FACE = 0x20;
        public static int BOARD = 0x10;

        public Action next;

        protected Action() {
            next = null;
        }

        public abstract InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure);
    }

    class CheckCrystalAction : Action {
        private int cost;

        public CheckCrystalAction(int c) {
            cost = c;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            return (player.currentCrystal >= cost) ? success : failure;
        }
    }

    class CheckPlaceMonsterAction : Action {
        private MonsterCard card;

        public CheckPlaceMonsterAction(MonsterCard c) {
            card = c;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            return (player.CanPlaceCard()) ? success : failure;
        }
    }

    class PlaceMonsterAction : Action {
        private MonsterCard card;

        public PlaceMonsterAction(MonsterCard card) : base() {
            this.card = card;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            return player.TryPlaceMonster(card.GetMonster()) ? success : failure;
        }
    }

    class ConsumeCrystalAction : Action {
        private int consumption;

        public ConsumeCrystalAction(int cost) : base() {
            consumption = cost;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            return player.Pay(consumption) ? success : failure;
        }
    }

    class PlayerDrawAction : Action {
        private int drawingPlayer;
        private int drawNumber;

        public PlayerDrawAction(int drawingPlayer, int n) : base() {
            this.drawingPlayer = drawingPlayer;
            drawNumber = n;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            if ((drawingPlayer & SELF) != 0) game.PlayerDraw(player.ID, drawNumber);
            if ((drawingPlayer & OPP) != 0) game.PlayerDraw(game.GetOppositeID(player.ID), drawNumber);
            return success;
        }
    }

    class PutCemeteryAction : Action {
        private Card card;

        public PutCemeteryAction(Card c) : base() {
            this.card = c;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            player.PutCemetery(card);
            return success;
        }
    }
}
