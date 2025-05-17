using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    internal class CombatSystem
    {
        private EntityManager _entityManager;
        private ComponentManager _componentManager;
        private int CombatRound = 1;

        public CombatSystem(EntityManager entityManager, ComponentManager componentManager)
        {
            _entityManager = entityManager;
            _componentManager = componentManager;

            
        }

        public void CreateBattle(List<EntityId> aggressorArmy, List<EntityId> defenderArmy)
        {
            EntityId Battle = _entityManager.CreateEntity();
            _componentManager.AddComponent<BattlefieldComponent>(Battle, new BattlefieldComponent(_componentManager, aggressorArmy, defenderArmy));

        }




        /// <summary>
        /// Finds enemy units in surrounding squares of the given unit.
        /// </summary>
        /// <param name="entityManager">The entity manager instance</param>
        /// <param name="componentManager">The component manager instance</param>
        /// <param name="unitEntityId">The entity ID of the unit checking for enemies</param>
        /// <param name="battlefieldEntityId">The entity ID containing the battlefield data</param>
        /// <returns>List of entity IDs of enemy units in surrounding squares</returns>
        public List<EntityId> FindEnemiesInSurroundingSquares(
            EntityManager entityManager,
            ComponentManager componentManager,
            EntityId unitEntityId,
            EntityId battlefieldEntityId)
        {
            List<EntityId> enemyEntities = new List<EntityId>();

            // Verify the entities have required components
            if (!componentManager.HasComponent<UnitComponent>(unitEntityId) ||
                !componentManager.HasComponent<CombatStatusComponent>(unitEntityId) ||
                !componentManager.HasComponent<BattlefieldComponent>(battlefieldEntityId))
            {
                // Early return if entities don't have required components
                return enemyEntities;
            }

            // Get the faction and position of the unit
            var unitComponent = componentManager.GetComponent<UnitComponent>(unitEntityId);
            var combatStatusComponent = componentManager.GetComponent<CombatStatusComponent>(unitEntityId);
            var battlefieldComponent = componentManager.GetComponent<BattlefieldComponent>(battlefieldEntityId);

            EntityId unitFaction = unitComponent.Faction;
            Point gridPosition = combatStatusComponent.CombatPosition;

            // Define the relative positions of the 8 surrounding squares
            Point[] surroundingOffsets = new Point[]
            {
                new Point(-1, -1), // Bottom-left
                new Point(0, -1),  // Bottom
                new Point(1, -1),  // Bottom-right
                new Point(-1, 0),  // Left
                new Point(1, 0),   // Right
                new Point(-1, 1),  // Top-left
                new Point(0, 1),   // Top
                new Point(1, 1)    // Top-right
            };

            // Check each surrounding square
            foreach (Point offset in surroundingOffsets)
            {
                Point adjacentPosition = new Point(gridPosition.X + offset.X, gridPosition.Y + offset.Y);

                // Skip if the position is out of bounds of the battlefield
                if (!IsValidBattlefieldPosition(adjacentPosition, battlefieldComponent))
                    continue;

                // Get the tile entity at this position
                EntityId tileEntityId = battlefieldComponent.BattlefieldTiles[adjacentPosition.Y][adjacentPosition.X];

                // Skip if no tile entity exists
                if (!entityManager.Exists(tileEntityId))
                    continue;

                // Get the BattlefieldTileComponent to access occupants
                if (!componentManager.HasComponent<BattlefieldTileComponent>(tileEntityId))
                    continue;

                var tileComponent = componentManager.GetComponent<BattlefieldTileComponent>(tileEntityId);

                // Process each entity occupying this tile
                foreach (EntityId occupantId in tileComponent.Occupants)
                {
                    // Skip if entity is invalid
                    if (!entityManager.Exists(occupantId))
                        continue;

                    // Check if the entity has a UnitComponent
                    if (componentManager.HasComponent<UnitComponent>(occupantId))
                    {
                        // Get the faction of this entity
                        EntityId entityFaction = componentManager.GetComponent<UnitComponent>(occupantId).Faction;

                        // Add to the list if it's from a different faction
                        if (!entityFaction.Equals(unitFaction))
                        {
                            enemyEntities.Add(occupantId);
                        }
                    }
                }
            }

            return enemyEntities;
        }

        /// <summary>
        /// Selects an enemy for combat engagement based on priority rules:
        /// 1. Prefer mutual combat with unengaged enemies
        /// 2. Fall back to unilateral aggression against enemies already in mutual combat
        /// </summary>
        /// <param name="unitEntityId">The entity ID of the attacking unit</param>
        /// <param name="battlefieldEntityId">The entity ID of the battlefield</param>
        /// <returns>Selected enemy entity ID or null if no suitable enemy found</returns>
        public EntityId? SelectEnemyForCombat(
            EntityId unitEntityId,
            EntityId battlefieldEntityId)
        {
            // Find all surrounding enemies
            List<EntityId> nearbyEnemies = FindEnemiesInSurroundingSquares(
                _entityManager,
                _componentManager,
                unitEntityId,
                battlefieldEntityId);

            if (nearbyEnemies.Count == 0)
                return null; // No enemies nearby

            // Create separate lists for enemies based on engagement status
            List<EntityId> unengagedEnemies = new List<EntityId>();
            List<EntityId> engagedEnemies = new List<EntityId>();

            // Categorize enemies based on whether they're already in mutual combat
            foreach (EntityId enemyId in nearbyEnemies)
            {
                var enemyCombatStatus = _componentManager.GetComponent<CombatStatusComponent>(enemyId);

                if (enemyCombatStatus.IsEngagedInMutualCombat)
                {
                    engagedEnemies.Add(enemyId);
                }
                else
                {
                    unengagedEnemies.Add(enemyId);
                }
            }

            // Select a random enemy from the appropriate list
            Random random = new Random();

            if (unengagedEnemies.Count > 0)
            {
                // Prefer mutual combat with unengaged enemies
                int randomIndex = random.Next(unengagedEnemies.Count);
                return unengagedEnemies[randomIndex];
            }
            else if (engagedEnemies.Count > 0)
            {
                // Fall back to unilateral aggression against already engaged enemies
                int randomIndex = random.Next(engagedEnemies.Count);
                return engagedEnemies[randomIndex];
            }

            // This shouldn't happen since we checked nearbyEnemies.Count earlier,
            // but included for completeness
            return null;
        }

        public void ProcessEngagement(CombatEngagement engagement)
        {
            List<EquipmentPrototype> weapons = GetWeapons(engagement.AttackerCurrentForm(_componentManager));
            for (int i = 0; i < weapons.Count; i++)
            {
                for (int k = 0; k < weapons[i].NumAttacks; k++)
                {
                    if (GetStrikeRoll(engagement.AttackerCurrentForm(_componentManager)) > Math.Max(GetBlockRoll(engagement.DefenderCurrentForm(_componentManager)), GetEvadeRoll(engagement.DefenderCurrentForm(_componentManager))))
                    {

                    }
                    else if (_componentManager.GetComponent<CombatStatusComponent>(engagement.Defender).CombatRound < CombatRound && 
                        (GetParryRoll(engagement.DefenderCurrentForm(_componentManager)) > Math.Max(GetBlockRoll(engagement.AttackerCurrentForm(_componentManager)), GetEvadeRoll(engagement.AttackerCurrentForm(_componentManager)))))
                    {
                        
                    }
                }
            }
        }
        // I have to use the UNITCOMPONENT level entity in order to add XP to proficiecies. This section needs a refactor
        public List<EquipmentPrototype> GetWeapons(EntityId unit)
        {
            List<EquipmentPrototype> weapons = new List<EquipmentPrototype>();
            if (_componentManager.HasComponent<HeadsComponent>(unit))
            {
                foreach (var v in _componentManager.GetComponent<HeadsComponent>(unit).Heads)
                {
                    if (v.EquippedItem.Id != -1)
                    {
                        weapons.Add(_componentManager.GetComponent<EquipmentPrototype>(v.EquippedItem));
                    }
                }
            }
            if (_componentManager.HasComponent<ArmsComponent>(unit))
            {
                foreach (var v in _componentManager.GetComponent<ArmsComponent>(unit).Arms)
                {
                    if (v.EquippedItem.Id != -1)
                    {
                        weapons.Add(_componentManager.GetComponent<EquipmentPrototype>(v.EquippedItem));
                    }
                }
            }
            if (_componentManager.HasComponent<LegsComponent>(unit) && _componentManager.GetComponent<LegsComponent>(unit).EquippedItem.Id != -1)
            {
                weapons.Add(_componentManager.GetComponent<EquipmentPrototype>(_componentManager.GetComponent<LegsComponent>(unit).EquippedItem));
            }
            if (_componentManager.HasComponent<TailsComponent>(unit))
            {
                foreach (var v in _componentManager.GetComponent<TailsComponent>(unit).Tails)
                {
                    if (v.DefaultEquipmentID != -1)
                    {
                        weapons.Add(Prototypes.equipmentPrototypes[v.DefaultEquipmentID]);
                    }
                }
            }
            if (_componentManager.HasComponent<ChestComponent>(unit) && _componentManager.GetComponent<ChestComponent>(unit).EquippedItem.Id != -1)
            {
                weapons.Add(_componentManager.GetComponent<EquipmentPrototype>(_componentManager.GetComponent<ChestComponent>(unit).EquippedItem));
            }
            if (_componentManager.HasComponent<BeastTorsoComponent>(unit) && _componentManager.GetComponent<BeastTorsoComponent>(unit).EquippedItem.Id != -1)
            {
                weapons.Add(_componentManager.GetComponent<EquipmentPrototype>(_componentManager.GetComponent<BeastTorsoComponent>(unit).EquippedItem));
            }
            return weapons;
        }
        public int GetStrikeRoll(EntityId attackerCurrentForm)
        {
            AttributesComponent a = _componentManager.GetComponent<AttributesComponent>(attackerCurrentForm);
            return 0 + DiceSystem.ChooseDieToRoll(attackerCurrentForm, _componentManager);
        }
        public int GetBlockRoll(EntityId defenderCurrentForm)
        {
            AttributesComponent d = _componentManager.GetComponent<AttributesComponent>(defenderCurrentForm);
            return 0 + DiceSystem.ChooseDieToRoll(defenderCurrentForm, _componentManager);
        }
        public int GetEvadeRoll(EntityId defenderCurrentForm)
        {
            AttributesComponent d = _componentManager.GetComponent<AttributesComponent>(defenderCurrentForm);
            return 0 + DiceSystem.ChooseDieToRoll(defenderCurrentForm, _componentManager);
        }

        // Helper method to check if a grid position is valid within the battlefield
        private bool IsValidBattlefieldPosition(Point position, BattlefieldComponent battlefield)
        {
            // Check if position is within battlefield bounds
            return position.Y >= 0 && position.Y < battlefield.BattlefieldTiles.Count &&
                   position.X >= 0 && position.X < battlefield.BattlefieldTiles[position.Y].Count;
        }
    }
}
