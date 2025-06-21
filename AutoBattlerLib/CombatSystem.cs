using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    public class CombatSystem
    {
        private readonly ComponentManager _componentManager;
        private readonly EntityManager _entityManager;

        public CombatSystem(ComponentManager componentManager, EntityManager entityManager)
        {
            _componentManager = componentManager;
            _entityManager = entityManager;
        }

        // Schedule an entity for action on a battlefield
        public void ScheduleEntityAction(BattlefieldComponent battlefield, Entity combatEntity, Tick tick)
        {
            // Add to battlefield schedule
            if (!battlefield.Schedule.ContainsKey(tick))
                battlefield.Schedule[tick] = new Queue<Entity>();

            battlefield.Schedule[tick].Enqueue(combatEntity);
            battlefield.EntitySchedule[combatEntity.Id] = tick;
        }

        // Get the next tick that has scheduled actions
        public Tick? GetNextActionTick(BattlefieldComponent battlefield)
        {
            if (battlefield?.Schedule.Count > 0)
                return battlefield.Schedule.Keys.First();
            return null;
        }

        // Advance time and get entities to act
        public Entity[] AdvanceToNextTick(BattlefieldComponent battlefield)
        {
            var nextTick = GetNextActionTick(battlefield);
            if (nextTick == null) return new Entity[0];

            battlefield.CurrentTick = nextTick.Value;
            var queue = battlefield.Schedule[nextTick.Value];
            var entities = queue.ToArray();

            // Clean up processed tick
            battlefield.Schedule.Remove(nextTick.Value);
            return entities;
        }
    }
}
