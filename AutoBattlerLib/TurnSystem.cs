using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Data class for turn information
    /// </summary>
    public class TurnInfo
    {
        public int TurnNumber { get; }
        public int CurrentFactionId { get; }

        public TurnInfo(int turnNumber, int currentFactionId)
        {
            TurnNumber = turnNumber;
            CurrentFactionId = currentFactionId;
        }
    }

    /// <summary>
    /// System for managing game turns
    /// </summary>
    public class TurnSystem : System
    {
        private List<int> factionIds = new List<int>();
        private int currentFactionIndex = 0;
        private int currentTurnNumber = 1;

        public int CurrentFactionId => factionIds.Count > 0 ? factionIds[currentFactionIndex] : -1;

        public TurnSystem(EntityManager entityManager, ComponentManager componentManager, EventSystem eventSystem)
            : base(entityManager, componentManager, eventSystem)
        {
        }

        /// <summary>
        /// Sets up the factions for the game
        /// </summary>
        public void SetupFactions(IEnumerable<int> factionIds)
        {
            this.factionIds = new List<int>(factionIds);
            currentFactionIndex = 0;
            currentTurnNumber = 1;

            if (this.factionIds.Count > 0)
            {
                EventSystem.DispatchEvent(new GameEvent(EventType.TurnChanged, GetCurrentTurnInfo()));
            }
        }

        /// <summary>
        /// Starts a new turn
        /// </summary>
        public void StartNewTurn()
        {
            if (factionIds.Count == 0)
            {
                return;
            }

            // Reset all units for the current faction
            var units = ComponentManager.GetEntitiesWithComponents<UnitComponent, FactionComponent>();

            foreach (var unitId in units)
            {
                var faction = ComponentManager.GetComponent<FactionComponent>(unitId);

                if (faction.FactionId == CurrentFactionId)
                {
                    var unit = ComponentManager.GetComponent<UnitComponent>(unitId);
                    unit.MovementRemaining = unit.MovementRange;
                    unit.HasAttacked = false;
                }
            }

            // Notify systems of turn change
            EventSystem.DispatchEvent(new GameEvent(EventType.TurnChanged, GetCurrentTurnInfo()));
        }

        /// <summary>
        /// Ends the current turn and moves to the next faction
        /// </summary>
        public void EndTurn()
        {
            if (factionIds.Count == 0)
            {
                return;
            }

            // Move to next faction
            currentFactionIndex = (currentFactionIndex + 1) % factionIds.Count;

            // If we've gone through all factions, increment the turn number
            if (currentFactionIndex == 0)
            {
                currentTurnNumber++;
            }

            // Start new turn for this faction
            StartNewTurn();
        }

        /// <summary>
        /// Gets the current turn information
        /// </summary>
        public TurnInfo GetCurrentTurnInfo()
        {
            return new TurnInfo(currentTurnNumber, CurrentFactionId);
        }

        /// <summary>
        /// Checks if a faction can act in the current turn
        /// </summary>
        public bool CanFactionAct(int factionId)
        {
            return factionId == CurrentFactionId;
        }

        public override void Update(float deltaTime)
        {
            // This system is command-driven, so we don't need to do anything in Update
        }
    }
}
