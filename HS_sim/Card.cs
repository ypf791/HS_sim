using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    
    abstract class Card {
        public string name { get; private set; }
        public int cost { get; private set; }

        public Card(string cardName, int cardCost) {
            name = cardName;
            cost = cardCost;
        }

        public virtual string GetDescription() {
            return String.Format("{0},{1}", cost, name);
        }

        private ActionChain prepareChain = null;
        private ActionChain actionChain = null;

        protected abstract ActionChain CreatePrepareChain();
        protected abstract ActionChain CreateActionChain();

        protected ActionChain GetPrepareChain() {
            if (prepareChain == null) prepareChain = CreatePrepareChain();
            return prepareChain;
        }

        protected ActionChain GetActionChain() {
            if (actionChain == null) actionChain = CreateActionChain();
            return actionChain;
        }

        public InputHandler Prepare(Game game, Player player, InputHandler success, InputHandler failure) {
            return GetPrepareChain().InvokeWithBranch(game, player, success, failure);
        }

        public InputHandler Play(Game game, Player player, InputHandler final) {
            return GetActionChain().InvokeWithFinal(game, player, final);
        }
    }
}
