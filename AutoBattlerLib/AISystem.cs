using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// System for controlling AI players
    /// </summary>
    public class AISystem : System
    {
        private HashSet<int> aiControlledFactions = new HashSet<int>();
        private TurnSystem turnSystem;
        private MovementSystem movementSystem;
        private CombatSystem combatSystem;
        private GridSystem gridSystem;
        private Random random = new Random();

        public AISystem(EntityManager entityManager, EntityComponentManager componentManager,
                        EventSystem eventSystem, TurnSystem turnSystem,
                        MovementSystem movementSystem, CombatSystem combatSystem,
                        GridSystem gridSystem)
            : base(entityManager, componentManager, eventSystem)
        {
            this.turnSystem = turnSystem;
            this.movementSystem = movementSystem;
            this.combatSystem = combatSystem;
            this.gridSystem = gridSystem;

            // Listen for turn changes
            eventSystem.AddEventListener(EventType.TurnChanged, OnTurnChanged);
        }

        /// <summary>
        /// Adds a faction to be controlled by AI
        /// </summary>
        public void AddAIControlledFaction(int factionId)
        {
            aiControlledFactions.Add(factionId);

            // If it's already this faction's turn, take action
            if (turnSystem.CurrentFactionId == factionId)
            {
                ExecuteAITurn();
            }
        }

        /// <summary>
        /// Removes a faction from AI control
        /// </summary>
        public void RemoveAIControlledFaction(int factionId)
        {
            aiControlledFactions.Remove(factionId);
        }

        /// <summary>
        /// Handles turn change events
        /// </summary>
        private void OnTurnChanged(object data)
        {
            if (data is TurnInfo turnInfo && aiControlledFactions.Contains(turnInfo.CurrentFactionId))
            {
                ExecuteAITurn();
            }
        }

        /// <summary>
        /// Executes an AI turn
        /// </summary>
        private void ExecuteAITurn()
        {
            // Get all units of the current faction
            var factionUnits = GetFactionUnits(turnSystem.CurrentFactionId);

            // Simple AI: For each unit, try to attack nearby enemies or move towards the closest enemy
            foreach (var unitId in factionUnits)
            {
                ProcessUnitAI(unitId);
            }

            // End the turn
            turnSystem.EndTurn();
        }

        /// <summary>
        /// Processes AI for a single unit
        /// </summary>
        private void ProcessUnitAI(Entity unitId)
        {
            // Check for valid attack targets
            var attackTargets = combatSystem.GetValidAttackTargets(unitId);

            if (attackTargets.Count > 0)
            {
                // Attack a random target
                var targetIndex = random.Next(attackTargets.Count);
                combatSystem.Attack(unitId, attackTargets[targetIndex]);
            }
            else
            {
                // No attack targets, try to move
                MoveUnitTowardsEnemy(unitId);
            }
        }

        /// <summary>
        /// Moves a unit towards the closest enemy
        /// </summary>
        private void MoveUnitTowardsEnemy(Entity unitId)
        {
            if (!ComponentManager.TryGetComponent<PositionComponent>(unitId, out var position))
            {
                return;
            }

            // Get all valid move destinations
            var moveDestinations = movementSystem.GetValidMoveDestinations(unitId);

            if (moveDestinations.Count == 0)
            {
                return;
            }

            // Get all enemy units
            var enemyUnits = GetEnemyUnits(turnSystem.CurrentFactionId);

            if (enemyUnits.Count == 0)
            {
                // No enemies, move randomly
                var randomIndex = random.Next(moveDestinations.Count);
                var destination = moveDestinations[randomIndex];
                movementSystem.MoveUnit(unitId, destination.X, destination.Y);
                return;
            }

            // Find the closest enemy
            Entity closestEnemy = enemyUnits[0];
            int closestDistance = int.MaxValue;

            foreach (var enemyId in enemyUnits)
            {
                if (ComponentManager.TryGetComponent<PositionComponent>(enemyId, out var enemyPos))
                {
                    int distance = Math.Abs(position.X - enemyPos.X) + Math.Abs(position.Y - enemyPos.Y);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemyId;
                    }
                }
            }

            // Get the enemy position
            if (!ComponentManager.TryGetComponent<PositionComponent>(closestEnemy, out var targetPos))
            {
                return;
            }

            // Find the move destination that gets us closest to the enemy
            GridPosition bestDestination = moveDestinations[0];
            int bestDistance = int.MaxValue;

            foreach (var destination in moveDestinations)
            {
                int distance = Math.Abs(destination.X - targetPos.X) + Math.Abs(destination.Y - targetPos.Y);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDestination = destination;
                }
            }

            // Move to the best destination
            movementSystem.MoveUnit(unitId, bestDestination.X, bestDestination.Y);
        }

        /// <summary>
        /// Gets all units belonging to a faction
        /// </summary>
        private List<Entity> GetFactionUnits(int factionId)
        {
            var units = ComponentManager.GetEntitiesWithComponents<UnitComponent, FactionComponent>();

            return units.Where(unitId =>
            {
                var faction = ComponentManager.GetComponent<FactionComponent>(unitId);
                return faction.FactionId == factionId;
            }).ToList();
        }

        /// <summary>
        /// Gets all units belonging to enemy factions
        /// </summary>
        private List<Entity> GetEnemyUnits(int factionId)
        {
            var units = ComponentManager.GetEntitiesWithComponents<UnitComponent, FactionComponent>();

            return units.Where(unitId =>
            {
                var faction = ComponentManager.GetComponent<FactionComponent>(unitId);
                return faction.FactionId != factionId;
            }).ToList();
        }

        public override void Update(float deltaTime)
        {
            // This system is event-driven, so we don't need to do anything in Update
        }
    }
}
