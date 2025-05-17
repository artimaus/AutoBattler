using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AutoBattlerLib; // Reference to your class library

namespace AutoBattler
{
    /// <summary>
    /// UI manager for the WPF application
    /// </summary>
    public class GameViewModel : INotifyPropertyChanged
    {
        private readonly GameManager gameManager;
        private EntityId? selectedEntityId;
        private List<GridPosition> validMoveDestinations = new List<GridPosition>();
        private List<EntityId> validAttackTargets = new List<EntityId>();

        // Properties for binding
        private string statusMessage = "Game ready.";
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private TurnInfo currentTurn;
        public TurnInfo CurrentTurn
        {
            get => currentTurn;
            set
            {
                currentTurn = value;
                OnPropertyChanged(nameof(CurrentTurn));
                OnPropertyChanged(nameof(CurrentFactionText));
            }
        }

        public string CurrentFactionText => currentTurn != null ?
            $"Turn {CurrentTurn.TurnNumber}, Faction {CurrentTurn.CurrentFactionId}'s turn" :
            "No active turn";

        private readonly EventSystem eventSystem;

        public event PropertyChangedEventHandler PropertyChanged;

        public GameViewModel(GameManager gameManager)
        {
            this.gameManager = gameManager;

            // Get the event system from GameManager using reflection since it's likely private
            eventSystem = typeof(GameManager)
                .GetField("eventSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(gameManager) as EventSystem;

            if (eventSystem != null)
            {
                eventSystem.AddEventListener(EventType.UnitMoved, OnUnitMoved);
                eventSystem.AddEventListener(EventType.UnitAttacked, OnUnitAttacked);
                eventSystem.AddEventListener(EventType.UnitDestroyed, OnUnitDestroyed);
                eventSystem.AddEventListener(EventType.TurnChanged, OnTurnChanged);
                eventSystem.AddEventListener(EventType.GameStarted, OnGameStarted);
                eventSystem.AddEventListener(EventType.GameEnded, OnGameEnded);
            }

            // Initialize current turn info
            CurrentTurn = gameManager.GetCurrentTurnInfo();
        }

        /// <summary>
        /// Handles tile click events from the UI
        /// </summary>
        public void HandleTileClick(int x, int y)
        {
            if (!gameManager.IsValidPosition(x, y))
            {
                return;
            }

            // Check if there's a unit at the clicked position
            var clickedUnitId = gameManager.GetUnitAt(x, y);

            // If we have a selected unit
            if (selectedEntityId.HasValue)
            {
                // Check if we're trying to attack
                if (clickedUnitId.Id != -1 && validAttackTargets.Contains(clickedUnitId))
                {
                    var result = gameManager.AttackUnit(selectedEntityId.Value, clickedUnitId);

                    if (result.Success)
                    {
                        StatusMessage = $"Attack successful! Dealt {result.DamageDealt} damage.";

                        if (result.TargetDestroyed)
                        {
                            StatusMessage += " Target destroyed!";
                        }

                        // Update valid attack targets
                        UpdateValidActions();
                    }
                    else
                    {
                        StatusMessage = $"Attack failed: {result.Message}";
                    }

                    return;
                }

                // Check if we're trying to move
                var targetGridPos = new GridPosition(x, y);
                if (validMoveDestinations.Contains(targetGridPos))
                {
                    var result = gameManager.MoveUnit(selectedEntityId.Value, x, y);

                    if (result.Success)
                    {
                        StatusMessage = "Unit moved successfully.";

                        // Update valid moves and targets
                        UpdateValidActions();
                    }
                    else
                    {
                        StatusMessage = $"Movement failed: {result.Message}";
                    }

                    return;
                }

                // If we click on another friendly unit, select it instead
                if (clickedUnitId.Id != -1)
                {
                    try
                    {
                        var clickedFaction = gameManager.GetComponent<FactionComponent>(clickedUnitId);
                        var selectedFaction = gameManager.GetComponent<FactionComponent>(selectedEntityId.Value);

                        if (clickedFaction.FactionId == selectedFaction.FactionId)
                        {
                            SelectEntity(clickedUnitId);
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        // Component not found, ignore
                    }
                }

                // Otherwise, deselect
                selectedEntityId = null;
                validMoveDestinations.Clear();
                validAttackTargets.Clear();

                StatusMessage = "Unit deselected.";
                OnSelectionChanged();
            }
            else
            {
                // If no unit is selected, try to select one
                if (clickedUnitId.Id != -1)
                {
                    try
                    {
                        // Check if this unit belongs to the current faction
                        var factionComponent = gameManager.GetComponent<FactionComponent>(clickedUnitId);
                        var currentTurn = gameManager.GetCurrentTurnInfo();

                        if (factionComponent.FactionId == currentTurn.CurrentFactionId)
                        {
                            SelectEntity(clickedUnitId);
                        }
                        else
                        {
                            StatusMessage = "Cannot select enemy unit.";
                        }
                    }
                    catch (Exception)
                    {
                        StatusMessage = "Not a valid unit.";
                    }
                }
            }
        }

        /// <summary>
        /// Selects an entity and calculates valid moves/targets
        /// </summary>
        private void SelectEntity(EntityId entityId)
        {
            selectedEntityId = entityId;

            try
            {
                // Get unit info
                var unit = gameManager.GetComponent<UnitComponent>(entityId);
                var position = gameManager.GetComponent<PositionComponent>(entityId);

                StatusMessage = $"Selected unit at ({position.X}, {position.Y}) with {unit.Health} health.";

                // Update valid moves and targets
                UpdateValidActions();
                OnSelectionChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error selecting unit: {ex.Message}";
                selectedEntityId = null;
            }
        }

        /// <summary>
        /// Updates the valid moves and attack targets for the selected unit
        /// </summary>
        private void UpdateValidActions()
        {
            if (!selectedEntityId.HasValue)
            {
                validMoveDestinations.Clear();
                validAttackTargets.Clear();
                OnValidActionsChanged();
                return;
            }

            // Get valid moves
            validMoveDestinations = gameManager.GetValidMoveDestinations(selectedEntityId.Value);

            // Get valid attack targets
            validAttackTargets = gameManager.GetValidAttackTargets(selectedEntityId.Value);

            OnValidActionsChanged();

            StatusMessage = $"Unit can move to {validMoveDestinations.Count} positions and attack {validAttackTargets.Count} targets.";
        }

        /// <summary>
        /// Handles the end turn button click
        /// </summary>
        public void EndTurn()
        {
            // Clear selection
            selectedEntityId = null;
            validMoveDestinations.Clear();
            validAttackTargets.Clear();

            // End turn
            gameManager.EndTurn();

            StatusMessage = "Turn ended.";
            OnSelectionChanged();
            OnValidActionsChanged();
        }

        #region Event Handlers

        /// <summary>
        /// Event handler for unit moved events
        /// </summary>
        private void OnUnitMoved(object data)
        {
            // Update UI elements if needed
            Application.Current.Dispatcher.Invoke(() => {
                // The dynamic object might have different properties depending on your implementation
                dynamic eventData = data;
                StatusMessage = $"Unit moved from ({eventData.OldPosition.X}, {eventData.OldPosition.Y}) to ({eventData.NewPosition.X}, {eventData.NewPosition.Y})";
                OnGridStateChanged();
            });
        }

        /// <summary>
        /// Event handler for unit attacked events
        /// </summary>
        private void OnUnitAttacked(object data)
        {
            Application.Current.Dispatcher.Invoke(() => {
                dynamic eventData = data;
                string destroyedText = eventData.Destroyed ? " Unit was destroyed!" : "";
                StatusMessage = $"Unit attacked! Dealt {eventData.Damage} damage.{destroyedText}";
                OnGridStateChanged();
            });
        }

        /// <summary>
        /// Event handler for unit destroyed events
        /// </summary>
        private void OnUnitDestroyed(object data)
        {
            Application.Current.Dispatcher.Invoke(() => {
                OnGridStateChanged();
            });
        }

        /// <summary>
        /// Event handler for turn changed events
        /// </summary>
        private void OnTurnChanged(object data)
        {
            Application.Current.Dispatcher.Invoke(() => {
                if (data is TurnInfo turnInfo)
                {
                    CurrentTurn = turnInfo;
                    StatusMessage = $"Turn {turnInfo.TurnNumber}, Faction {turnInfo.CurrentFactionId}'s turn.";
                }

                // Clear selection when turn changes
                selectedEntityId = null;
                validMoveDestinations.Clear();
                validAttackTargets.Clear();

                OnSelectionChanged();
                OnValidActionsChanged();
            });
        }

        /// <summary>
        /// Event handler for game started events
        /// </summary>
        private void OnGameStarted(object data)
        {
            Application.Current.Dispatcher.Invoke(() => {
                StatusMessage = "Game started!";
                OnGridStateChanged();
            });
        }

        /// <summary>
        /// Event handler for game ended events
        /// </summary>
        private void OnGameEnded(object data)
        {
            Application.Current.Dispatcher.Invoke(() => {
                StatusMessage = "Game ended!";
            });
        }

        #endregion

        #region Event notifications for UI

        /// <summary>
        /// Event for when the grid state has changed
        /// </summary>
        public event EventHandler GridStateChanged;

        protected void OnGridStateChanged()
        {
            GridStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event for when the selection has changed
        /// </summary>
        public event EventHandler SelectionChanged;

        protected void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event for when the valid actions have changed
        /// </summary>
        public event EventHandler ValidActionsChanged;

        protected void OnValidActionsChanged()
        {
            ValidActionsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method to raise property changed events
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// Gets whether a position is a valid move destination
        /// </summary>
        public bool IsValidMoveDestination(int x, int y)
        {
            return validMoveDestinations.Contains(new GridPosition(x, y));
        }

        /// <summary>
        /// Gets whether a unit can be attacked
        /// </summary>
        public bool IsValidAttackTarget(EntityId unitId)
        {
            return validAttackTargets.Contains(unitId);
        }

        /// <summary>
        /// Gets whether a unit is selected
        /// </summary>
        public bool IsUnitSelected(EntityId unitId)
        {
            return selectedEntityId.HasValue && selectedEntityId.Value.Equals(unitId);
        }

        /// <summary>
        /// Gets the terrain type at a specific position
        /// </summary>
        public string GetTerrainTypeName(int x, int y)
        {
            var terrainId = gameManager.GetTerrainAt(x, y);
            if (terrainId.Id != -1)
            {
                try
                {
                    var terrain = gameManager.GetComponent<TerrainComponent>(terrainId);
                    return terrain.Type;
                }
                catch
                {
                    return "unknown";
                }
            }
            return "empty";
        }

        /// <summary>
        /// Gets the unit info at a specific position
        /// </summary>
        public UnitDisplayInfo GetUnitDisplayInfo(int x, int y)
        {
            var unitId = gameManager.GetUnitAt(x, y);
            if (unitId.Id != -1)
            {
                try
                {
                    var unit = gameManager.GetComponent<UnitComponent>(unitId);
                    var sprite = gameManager.GetComponent<SpriteComponent>(unitId);
                    var faction = gameManager.GetComponent<FactionComponent>(unitId);

                    return new UnitDisplayInfo
                    {
                        UnitId = unitId,
                        SpriteId = sprite.SpriteId,
                        Health = unit.Health,
                        MaxHealth = unit.MaxHealth,
                        FactionId = faction.FactionId,
                        HasMoved = unit.MovementRemaining <= 0,
                        HasAttacked = unit.HasAttacked
                    };
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame(string mapData)
        {
            gameManager.InitializeGame(mapData);

            // Update the current turn
            CurrentTurn = gameManager.GetCurrentTurnInfo();

            // Clear selection
            selectedEntityId = null;
            validMoveDestinations.Clear();
            validAttackTargets.Clear();

            OnSelectionChanged();
            OnValidActionsChanged();
            OnGridStateChanged();
        }

        /// <summary>
        /// Saves the current game
        /// </summary>
        public string SaveGame()
        {
            return gameManager.SaveGame();
        }

        /// <summary>
        /// Loads a saved game
        /// </summary>
        public void LoadGame(string saveData)
        {
            gameManager.LoadGame(saveData);

            // Update the current turn
            CurrentTurn = gameManager.GetCurrentTurnInfo();

            // Clear selection
            selectedEntityId = null;
            validMoveDestinations.Clear();
            validAttackTargets.Clear();

            OnSelectionChanged();
            OnValidActionsChanged();
            OnGridStateChanged();
        }

        #endregion
    }

    /// <summary>
    /// Data class for displaying unit information
    /// </summary>
    public class UnitDisplayInfo
    {
        public EntityId UnitId { get; set; }
        public string SpriteId { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int FactionId { get; set; }
        public bool HasMoved { get; set; }
        public bool HasAttacked { get; set; }
    }
}
