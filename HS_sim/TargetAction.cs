using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    interface TargetObject {
        string GetDescription();
    }

    abstract class BoardObject : TargetObject {
        public abstract string GetDescription();
        public abstract Card GetCard();
        public abstract bool IsDestroyed();
    }

    interface Hurtable {
        bool TakeDamage(int damage);
    }


    delegate bool TargetFilter(int pattern, TargetObject obj);
    delegate void TargetFixer(TargetObject target);
    
    class NeedTargetAction : Action {
        protected int targetPattern;
        protected TargetObject target;
        protected TargetFixer fixer;
        public TargetFilter filter { get; private set; }

        public NeedTargetAction(TargetFilter filter, TargetFixer fixer) {
            this.filter = filter;
            this.fixer = fixer;
        }

        public void PutTarget(int targetPattern, TargetObject obj) {
            target = obj;
            this.targetPattern = targetPattern;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            game.UpdateTargetObject(player.ID, targetPattern, target, fixer);
            return success;
        }
    }

    class AllTargetAction : Action {
        protected TargetFilter filter;
        protected TargetFixer fixer;

        public AllTargetAction(TargetFilter filter, TargetFixer fixer) {
            this.filter = filter;
            this.fixer = fixer;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            game.UpdateAllTargetObject(player.ID, filter, fixer);

            return success;
        }
    }

    class ChooseTargetAction : Action {
        private NeedTargetAction needTargetAction;

        public ChooseTargetAction(NeedTargetAction action) {
            this.needTargetAction = action;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            Player.PublicInfo[] info = new Player.PublicInfo[] { player.GetPublicInfo(), game.GetOppositeInfo(player.ID) };
            return new ChooseTargetHandler(info, needTargetAction, new ActionChainHandler(next, success, failure), failure);
        }
    }

    class AttackTargetAction : NeedTargetAction {
        private Monster attacker;
        private int attackerPattern;

        public AttackTargetAction(int attackerPattern, Monster attacker) : base((pattern, obj) => ((pattern & Action.OPP) != 0) && (obj is Hurtable), null) {
            this.attacker = attacker;
            this.attackerPattern = attackerPattern;
        }

        public override InputHandler DoAction(Game game, Player player, InputHandler success, InputHandler failure) {
            game.UpdateTargetObject(player.ID, targetPattern, target, (TargetObject obj) => {
                (obj as Hurtable).TakeDamage(attacker.DeclareAttack());
            });
            game.UpdateTargetObject(player.ID, attackerPattern, attacker, (TargetObject obj) => {
                if (target is Monster) (obj as Hurtable).TakeDamage((target as Monster).atk);
            });

            return success;
        }
    }
}
