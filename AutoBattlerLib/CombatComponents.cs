using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace AutoBattlerLib
{
    public class BattlefieldComponent : IComponent
    {
        ComponentManager _componentManager { get; set; }
        public List<List<List<int>>> BattlefieldTiles { get; set; } = new List<List<List<int>>>();
        public LinkedList<int> CombatantMoveOrder { get; set; } = new LinkedList<int>();

        public LinkedList<int> DeadCombatants { get; set; } = new LinkedList<int>();
        public List<UnitCombatCard> UnitCombatCards { get; set; } = new List<UnitCombatCard>();

        public BattlefieldComponent(ComponentManager componentManager, List<EntityId> aggressorArmy, List<EntityId> defenderArmy)
        {
            _componentManager = componentManager;
            foreach (var unit in defenderArmy)
            {
                UnitCombatCards.Add(new UnitCombatCard(componentManager, unit));
                IterateMoveOrder(UnitCombatCards.Count - 1);
            }
        }

        public void IncrementNextMove(int index)
        {
            UnitCombatCard card = UnitCombatCards[index];
            int cs = card.GetCombatSpeed();
            if (cs <= 0)
            {
                return;
            }

            int tickIncrease = Math.Max(0, (6000 / cs) - Dice.Roll(card.Unit, _componentManager));
            if (tickOfNextMove + tickIncrease >= 6000)
            {
                tickOfNextMove = tickIncrease - 6000;
                roundOfNextMove++;
            }
            else
            {
                tickOfNextMove += tickIncrease;
            }
        }

        public void IterateMoveOrder(int index)
        {
            UnitCombatCards[index].IncrementNextMove();
            LinkedListNode<int> current = CombatantMoveOrder.Last;
            for (int i = 0; i < CombatantMoveOrder.Count && current != null; i++)
            {
                if (UnitCombatCards[current.Value].roundOfNextMove <= UnitCombatCards[index].roundOfNextMove && 
                    UnitCombatCards[current.Value].tickOfNextMove < UnitCombatCards[index].tickOfNextMove)
                {
                    CombatantMoveOrder.AddAfter(current, index);
                }
                current = current.Previous;
            }

        }
    }

    public class BattlefieldTileComponent : IComponent
    {
        public List<EntityId> Occupants { get; set; }

        public BattlefieldTileComponent()
        {
            Occupants = new List<EntityId>();
        }
    }

    public enum Orientation 
    { 
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public class UnitCombatCard
    {
        public EntityId Unit { get; set; }
        public List<IComponent> Body { get; set; } = new List<IComponent>();
        public List<EntityId> Equipment { get; set; } = new List<IComponent>();

        public int CurrentHealth { get; set; }
        public int CurrentExhaustion { get; set; }
        public int tickOfNextMove { get; set; } // each round is 6000 ticks

        public UnitCombatCard(ComponentManager componentManager, EntityId unit)
        {

        }

        public void UpdateAttributes(ComponentManager componentManager)
        {
            var unitComponent = componentManager.GetComponent<UnitComponent>(Unit);
            CurrentForm = unitComponent.currentForm;
            var form = unitComponent.FormIDs[CurrentForm];
            var Attributes = componentManager.GetComponent<AttributesComponent>(form.Form);
            CurrentSize = Attributes.Size;
            CurrentStrength = Attributes.Strength;
            CurrentDexterity = Attributes.Dexterity;
            CurrentAgility = Attributes.Agility;
            CurrentStamina = Attributes.Stamina;
            CurrentToughness = Attributes.Toughness;
            CurrentWill = Attributes.Will;
            CurrentConstitution = Attributes.Constitution;

            int newMaxHealth = (int)(CurrentSize * ((CurrentConstitution / 5.0) + (CurrentToughness / 10.0)));
            int MaxHealthChange = newMaxHealth - CurrentMaxHealth;
            CurrentMaxHealth = newMaxHealth;
            CurrentHealth += MaxHealthChange;

            roundOfNextMove = 0;
            tickOfNextMove = 0;

        }
    }
}
