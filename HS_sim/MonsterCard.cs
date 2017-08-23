using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_sim {
    class Monster : BoardObject, Hurtable {
        public string name { get; private set; }
        public int cost { get; private set; }
        public int atk;
        public int maxHp;
        public int hp;

        private Card srcCard;

        private int attackNumber = 0;
        private int maxAttackNumber = 1;

        public Monster(Card card, int atk, int hp) {
            this.srcCard = card;
            this.name = card.name;
            this.cost = card.cost;
            this.atk = atk;
            this.maxHp = hp;
            this.hp = hp;
        }

        public override string GetDescription() {
            return String.Format("{0}/{1}/{2},{3}", cost, atk, hp, name);
        }

        public override Card GetCard() {
            return srcCard;
        }

        public override bool IsDestroyed() {
            return hp <= 0;
        }

        public bool TakeDamage(int damage) {
            hp -= damage;
            return hp <= 0;
        }

        public bool CanAttack() {
            return attackNumber > 0;
        }

        public int DeclareAttack() {
            if (attackNumber > 0) {
                --attackNumber;
                return atk;
            }
            return 0;
        }

        public void RestoreAttackNumber() {
            attackNumber = maxAttackNumber;
        }

        public void ExhaustAttackNumber() {
            attackNumber = 0;
        }
    }

    class MonsterCard : Card {
        public int atk { get; private set; }
        public int hp { get; private set; }

        public MonsterCard(string cardName, int cardCost, int attack, int health)
            : base(cardName, cardCost) {
            atk = attack;
            hp = health;
        }

        public override string GetDescription() {
            return String.Format("{0}/{1}/{2},{3}", cost, atk, hp, name);
        }

        public Monster GetMonster() {
            return new Monster(this, atk, hp);
        }

        protected override ActionChain CreatePrepareChain() {
            return new ActionChain(new CheckCrystalAction(this.cost)).Append(new CheckPlaceMonsterAction(this));
        }

        protected override ActionChain CreateActionChain() {
            return new ActionChain(new ConsumeCrystalAction(this.cost)).Append(new PlaceMonsterAction(this));
        }
    }

    class MCard_AzureDrake : MonsterCard {
        public MCard_AzureDrake() : base("Azure Drake", 5, 4, 4) { }

        protected override ActionChain CreateActionChain() {
            return base.CreateActionChain().Append(new PlayerDrawAction(Action.SELF, 1));
        }
    }

    class MCard_Goblin : MonsterCard {
        public MCard_Goblin() : base("Goblin", 1, 1, 2) { }
    }

    class MCard_MerlocTidehunter : MonsterCard {
        private MonsterCard token = new MonsterCard("Merloc Scout", 1, 1, 1);

        public MCard_MerlocTidehunter() : base("Merloc Tidehunter", 2, 2, 1) { }

        protected override ActionChain CreateActionChain() {
            return base.CreateActionChain().Append(new PlaceMonsterAction(token));
        }
    }
}
