using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Central manager for the game
    /// </summary>
    public class GameManager
    {
        private EntityManager entityManager;
        private EntityComponentManager componentManager;
        private SystemManager systemManager;
        private EventSystem eventSystem;
        private GridSystem gridSystem;
        private MovementSystem movementSystem;
        private CombatSystem combatSystem;
        private TurnSystem turnSystem;
        private AISystem aiSystem;
        private EntityFactory entityFactory;

        private bool gameInProgress = false;

        public GameManager(int mapWidth = 20, int mapHeight = 15)
        {
            // Initialize core systems
            entityManager = new EntityManager();
            componentManager = new EntityComponentManager();
            systemManager = new SystemManager();
            eventSystem = new EventSystem();

            // Initialize game systems
            gridSystem = new GridSystem(entityManager, componentManager, eventSystem, mapWidth, mapHeight);
            movementSystem = new MovementSystem(entityManager, componentManager, eventSystem, gridSystem);
            combatSystem = new CombatSystem(entityManager, componentManager, eventSystem, gridSystem);
            turnSystem = new TurnSystem(entityManager, componentManager, eventSystem);
            aiSystem = new AISystem(entityManager, componentManager, eventSystem,
                                   turnSystem, movementSystem, combatSystem, gridSystem);

            // Initialize factory
            entityFactory = new EntityFactory(entityManager, componentManager, gridSystem);

            // Register systems
            systemManager.AddSystem(gridSystem);
            systemManager.AddSystem(movementSystem);
            systemManager.AddSystem(combatSystem);
            systemManager.AddSystem(turnSystem);
            systemManager.AddSystem(aiSystem);

            // Listen for game-ending events
            eventSystem.AddEventListener(EventType.GameEnded, (data) => gameInProgress = false);
        }

        /// <summary>
        /// Initializes a new game with the provided map data
        /// </summary>
        public void InitializeGame(string mapData)
        {
            // Parse map data (would need a proper format, using JSON for example)
            var mapConfig = JsonSerializer.Deserialize<MapConfiguration>(mapData);

            // Setup factions
            turnSystem.SetupFactions(mapConfig.FactionIds);

            // Create terrain
            for (int x = 0; x < gridSystem.Width; x++)
            {
                for (int y = 0; y < gridSystem.Height; y++)
                {
                    string terrainType = mapConfig.TerrainData[y * gridSystem.Width + x];
                    if (terrainType != "empty")
                    {
                        entityFactory.CreateTerrain(x, y, terrainType);
                    }
                }
            }

            // Create units
            foreach (var unitData in mapConfig.UnitData)
            {
                switch (unitData.Type)
                {
                    case "infantry":
                        entityFactory.CreateInfantryUnit(unitData.X, unitData.Y, unitData.FactionId);
                        break;
                    case "tank":
                        entityFactory.CreateTankUnit(unitData.X, unitData.Y, unitData.FactionId);
                        break;
                    case "artillery":
                        entityFactory.CreateArtilleryUnit(unitData.X, unitData.Y, unitData.FactionId);
                        break;
                }
            }

            // Set AI-controlled factions
            foreach (var factionId in mapConfig.AIFactions)
            {
                aiSystem.AddAIControlledFaction(factionId);
            }

            // Start game
            gameInProgress = true;
            eventSystem.DispatchEvent(new GameEvent(EventType.GameStarted, null));

            // Start first turn
            turnSystem.StartNewTurn();
        }

        /// <summary>
        /// Updates the game state
        /// </summary>
        public void Update(float deltaTime)
        {
            if (gameInProgress)
            {
                systemManager.UpdateSystems(deltaTime);
            }
        }

        /// <summary>
        /// Moves a unit to a new position
        /// </summary>
        public MovementResult MoveUnit(Entity unitId, int x, int y)
        {
            if (!gameInProgress)
            {
                return new MovementResult(false, "Game is not in progress");
            }

            // Check if it's this unit's faction's turn
            if (componentManager.TryGetComponent<FactionComponent>(unitId, out var faction))
            {
                if (!turnSystem.CanFactionAct(faction.FactionId))
                {
                    return new MovementResult(false, "Not this faction's turn");
                }
            }
            else
            {
                return new MovementResult(false, "Entity is not a unit with a faction");
            }

            return movementSystem.MoveUnit(unitId, x, y);
        }

        /// <summary>
        /// Performs an attack between two units
        /// </summary>
        public CombatResult AttackUnit(Entity attackerId, Entity defenderId)
        {
            if (!gameInProgress)
            {
                return new CombatResult(false, "Game is not in progress");
            }

            // Check if it's the attacker's faction's turn
            if (componentManager.TryGetComponent<FactionComponent>(attackerId, out var faction))
            {
                if (!turnSystem.CanFactionAct(faction.FactionId))
                {
                    return new CombatResult(false, "Not this faction's turn");
                }
            }
            else
            {
                return new CombatResult(false, "Attacker is not a unit with a faction");
            }

            return combatSystem.Attack(attackerId, defenderId);
        }

        /// <summary>
        /// Ends the current turn
        /// </summary>
        public void EndTurn()
        {
            if (gameInProgress)
            {
                turnSystem.EndTurn();
            }
        }

        /// <summary>
        /// Gets information about the current turn
        /// </summary>
        public TurnInfo GetCurrentTurnInfo()
        {
            return turnSystem.GetCurrentTurnInfo();
        }

        /// <summary>
        /// Gets all valid movement destinations for a unit
        /// </summary>
        public List<GridPosition> GetValidMoveDestinations(Entity unitId)
        {
            return movementSystem.GetValidMoveDestinations(unitId);
        }

        /// <summary>
        /// Gets all valid attack targets for a unit
        /// </summary>
        public List<Entity> GetValidAttackTargets(Entity unitId)
        {
            return combatSystem.GetValidAttackTargets(unitId);
        }

        /// <summary>
        /// Saves the current game state to a string
        /// </summary>
        public string SaveGame()
        {
            var saveData = new GameSaveData
            {
                Entities = new List<EntitySaveData>()
            };

            foreach (var entityId in entityManager.GetAllEntities())
            {
                var entityData = new EntitySaveData
                {
                    Id = entityId.Id,
                    Components = new Dictionary<string, object>()
                };

                // Save components
                if (componentManager.TryGetComponent<PositionComponent>(entityId, out var position))
                {
                    entityData.Components["Position"] = position;
                }

                if (componentManager.TryGetComponent<UnitComponent>(entityId, out var unit))
                {
                    entityData.Components["Unit"] = unit;
                }

                if (componentManager.TryGetComponent<SpriteComponent>(entityId, out var sprite))
                {
                    entityData.Components["Sprite"] = sprite;
                }

                if (componentManager.TryGetComponent<FactionComponent>(entityId, out var faction))
                {
                    entityData.Components["Faction"] = faction;
                }

                if (componentManager.TryGetComponent<TerrainComponent>(entityId, out var terrain))
                {
                    entityData.Components["Terrain"] = terrain;
                }

                saveData.Entities.Add(entityData);
            }

            // Save turn information
            saveData.CurrentTurn = GetCurrentTurnInfo();

            // Serialize to JSON
            return JsonSerializer.Serialize(saveData);
        }

        /// <summary>
        /// Loads a game state from a string
        /// </summary>
        public void LoadGame(string saveData)
        {
            // Deserialize from JSON
            var gameData = JsonSerializer.Deserialize<GameSaveData>(saveData);

            // Clear existing entities
            foreach (var entityId in entityManager.GetAllEntities())
            {
                entityManager.DestroyEntity(entityId);
            }

            // Recreate entities
            foreach (var entityData in gameData.Entities)
            {
                // Create entity with specific ID (this would require extending EntityManager)
                var entityId = new Entity(entityData.Id);

                // Add components
                foreach (var componentPair in entityData.Components)
                {
                    switch (componentPair.Key)
                    {
                        case "Position":
                            var position = (PositionComponent)componentPair.Value;
                            componentManager.AddComponent(entityId, position);
                            break;
                        case "Unit":
                            var unit = (UnitComponent)componentPair.Value;
                            componentManager.AddComponent(entityId, unit);
                            break;
                        case "Sprite":
                            var sprite = (SpriteComponent)componentPair.Value;
                            componentManager.AddComponent(entityId, sprite);
                            break;
                        case "Faction":
                            var faction = (FactionComponent)componentPair.Value;
                            componentManager.AddComponent(entityId, faction);
                            break;
                        case "Terrain":
                            var terrain = (TerrainComponent)componentPair.Value;
                            componentManager.AddComponent(entityId, terrain);
                            break;
                    }
                }

                // Place on grid if it has a position
                if (componentManager.TryGetComponent<PositionComponent>(entityId, out var pos))
                {
                    if (componentManager.HasComponent<TerrainComponent>(entityId))
                    {
                        gridSystem.PlaceTerrainAt(entityId, pos.X, pos.Y);
                    }
                    else if (componentManager.HasComponent<UnitComponent>(entityId))
                    {
                        gridSystem.PlaceUnitAt(entityId, pos.X, pos.Y);
                    }
                }
            }

            // Restore turn state
            var factionIds = new HashSet<int>();

            // Collect all faction IDs
            foreach (var entityId in entityManager.GetAllEntities())
            {
                if (componentManager.TryGetComponent<FactionComponent>(entityId, out var faction))
                {
                    factionIds.Add(faction.FactionId);
                }
            }

            turnSystem.SetupFactions(factionIds);

            // Mark the game as in progress
            gameInProgress = true;
            eventSystem.DispatchEvent(new GameEvent(EventType.GameStarted, null));
        }

        /// <summary>
        /// Gets the unit at a specific position
        /// </summary>
        public Entity GetUnitAt(int x, int y)
        {
            return gridSystem.GetUnitAt(x, y);
        }

        /// <summary>
        /// Gets the terrain at a specific position
        /// </summary>
        public Entity GetTerrainAt(int x, int y)
        {
            return gridSystem.GetTerrainAt(x, y);
        }

        /// <summary>
        /// Checks if a position is valid
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return gridSystem.IsInBounds(x, y);
        }

        /// <summary>
        /// Gets the component of the specified type for an entity
        /// </summary>
        public T GetComponent<T>(Entity entityId) where T : IComponentData
        {
            return componentManager.GetComponent<T>(entityId);
        }
    }

    /// <summary>
    /// Data class for map configuration
    /// </summary>
    public class MapConfiguration
    {
        public List<int> FactionIds { get; set; }
        public List<int> AIFactions { get; set; }
        public string[] TerrainData { get; set; }
        public List<UnitData> UnitData { get; set; }
    }

    /// <summary>
    /// Data class for unit configuration
    /// </summary>
    public class UnitData
    {
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int FactionId { get; set; }
    }

    /// <summary>
    /// Data class for saving game state
    /// </summary>
    public class GameSaveData
    {
        public List<EntitySaveData> Entities { get; set; }
        public TurnInfo CurrentTurn { get; set; }
    }

    /// <summary>
    /// Data class for saving entity data
    /// </summary>
    public class EntitySaveData
    {
        public int Id { get; set; }
        public Dictionary<string, object> Components { get; set; }
    }
}
