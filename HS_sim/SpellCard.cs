using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class SpellCard : Card {
        public SpellCard(string cardName, int cardCost) : base(cardName, cardCost) { }

        protected override ActionChain CreatePrepareChain() {
            return new ActionChain(new CheckCrystalAction(this.cost));
        }

        protected override ActionChain CreateActionChain() {
            return new ActionChain(new ConsumeCrystalAction(this.cost)).Append(new PutCemeteryAction(this));
        }
    }

    class SCard_ArcaneIntellect : SpellCard {
        public SCard_ArcaneIntellect() : base("Arcane Intellect", 3) { }

        protected override ActionChain CreateActionChain() {
            return base.CreateActionChain().Append(new PlayerDrawAction(Action.SELF, 2));
        }
    }

    class SCard_DivineSpirit : SpellCard {
        private ChooseTargetAction chooseTargetAction;
        private NeedTargetAction needTargetAction;

        public SCard_DivineSpirit() : base("Divine Spirit", 2) {
            TargetFixer fixer = (TargetObject x) => {
                (x as Monster).maxHp *= 2;
                (x as Monster).hp *= 2;
            };
            needTargetAction = new NeedTargetAction((int pattern, TargetObject x) => x is Monster, fixer);
            chooseTargetAction = new ChooseTargetAction(needTargetAction);
        }

        protected override ActionChain CreatePrepareChain() {
            return base.CreatePrepareChain().Append(chooseTargetAction);
        }

        protected override ActionChain CreateActionChain() {
            return base.CreateActionChain().Append(needTargetAction);
        }
    }

    class SCard_AcraneExplosion : SpellCard {
        protected TargetFilter filter;
        protected TargetFixer fixer;

        public SCard_AcraneExplosion() : base("Acrane Explosion", 2) {
            filter = (int pattern, TargetObject x) => {
                if ((pattern & Action.OPP) == 0) return false;
                if ((pattern & Action.BOARD) == 0) return false;
                return x is Monster;
            };
            fixer = (TargetObject x) => {
                (x as Monster).TakeDamage(1);
            };
        }

        protected override ActionChain CreateActionChain() {
            return base.CreateActionChain().Append(new AllTargetAction(filter, fixer));
        }
    }
}
