using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Factory for creating common entity types
    /// </summary>
    public class EntityFactory
    {
        private EntityManager entityManager;
        private ComponentManager componentManager;
        private GridSystem gridSystem;

        public EntityFactory(EntityManager entityManager, ComponentManager componentManager,
                             GridSystem gridSystem)
        {
            this.entityManager = entityManager;
            this.componentManager = componentManager;
            this.gridSystem = gridSystem;
        }

        /// <summary>
        /// Creates an infantry unit
        /// </summary>
        public EntityId CreateInfantryUnit(int x, int y, int factionId)
        {
            var entity = entityManager.CreateEntity();

            // Add components
            componentManager.AddComponent(entity, new PositionComponent(x, y));
            componentManager.AddComponent(entity, new UnitComponent(
                health: 100,
                attack: 20,
                defense: 10,
                movementRange: 4
            ));
            componentManager.AddComponent(entity, new SpriteComponent("infantry"));
            componentManager.AddComponent(entity, new FactionComponent(factionId, factionId));

            // Place on grid
            gridSystem.PlaceUnitAt(entity, x, y);

            return entity;
        }

        /// <summary>
        /// Creates a tank unit
        /// </summary>
        public EntityId CreateTankUnit(int x, int y, int factionId)
        {
            var entity = entityManager.CreateEntity();

            // Add components
            componentManager.AddComponent(entity, new PositionComponent(x, y));
            componentManager.AddComponent(entity, new UnitComponent(
                health: 200,
                attack: 30,
                defense: 20,
                movementRange: 6
            ));
            componentManager.AddComponent(entity, new SpriteComponent("tank"));
            componentManager.AddComponent(entity, new FactionComponent(factionId, factionId));

            // Place on grid
            gridSystem.PlaceUnitAt(entity, x, y);

            return entity;
        }

        /// <summary>
        /// Creates an artillery unit
        /// </summary>
        public EntityId CreateArtilleryUnit(int x, int y, int factionId)
        {
            var entity = entityManager.CreateEntity();

            // Add components
            componentManager.AddComponent(entity, new PositionComponent(x, y));
            componentManager.AddComponent(entity, new UnitComponent(
                health: 80,
                attack: 40,
                defense: 5,
                movementRange: 3
            ));
            componentManager.AddComponent(entity, new SpriteComponent("artillery"));
            componentManager.AddComponent(entity, new FactionComponent(factionId, factionId));

            // Place on grid
            gridSystem.PlaceUnitAt(entity, x, y);

            return entity;
        }

        /// <summary>
        /// Creates a terrain entity
        /// </summary>
        public EntityId CreateTerrain(int x, int y, string terrainType)
        {
            var entity = entityManager.CreateEntity();

            // Set terrain properties based on type
            int movementCost = 1;
            int defenseBonus = 0;

            switch (terrainType)
            {
                case "mountain":
                    movementCost = 3;
                    defenseBonus = 3;
                    break;
                case "forest":
                    movementCost = 2;
                    defenseBonus = 2;
                    break;
                case "river":
                    movementCost = 2;
                    defenseBonus = 0;
                    break;
                case "road":
                    movementCost = 1;
                    defenseBonus = 0;
                    break;
                case "city":
                    movementCost = 1;
                    defenseBonus = 4;
                    break;
            }

            // Add components
            componentManager.AddComponent(entity, new PositionComponent(x, y));
            componentManager.AddComponent(entity, new SpriteComponent(terrainType));
            componentManager.AddComponent(entity, new TerrainComponent(terrainType, movementCost, defenseBonus));

            // Place on grid
            gridSystem.PlaceTerrainAt(entity, x, y);

            return entity;
        }
    }
}
