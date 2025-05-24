using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Represents a point on the 2D grid
    /// </summary>
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    /// <summary>
    /// System for managing the game grid
    /// </summary>
    public class GridSystem : System
    {
        public int Width { get; }
        public int Height { get; }

        private Entity[,] terrainGrid;
        private Entity[,] unitGrid;

        public GridSystem(EntityManager entityManager, ComponentManager componentManager,
                          EventSystem eventSystem, int width, int height)
            : base(entityManager, componentManager, eventSystem)
        {
            Width = width;
            Height = height;

            terrainGrid = new Entity[width, height];
            unitGrid = new Entity[width, height];

            // Initialize with default values
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    terrainGrid[x, y] = new Entity(-1); // Invalid ID
                    unitGrid[x, y] = new Entity(-1);    // Invalid ID
                }
            }
        }

        /// <summary>
        /// Places a terrain entity on the grid
        /// </summary>
        public bool PlaceTerrainAt(Entity entityId, int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            // Update entity's position component
            if (!ComponentManager.TryGetComponent<PositionComponent>(entityId, out var position))
            {
                position = new PositionComponent(x, y);
                ComponentManager.AddComponent(entityId, position);
            }
            else
            {
                // If the entity already has a position, update terrain grid at old position
                terrainGrid[position.X, position.Y] = new Entity(-1);

                // Update position component
                position.X = x;
                position.Y = y;
                ComponentManager.AddComponent(entityId, position);
            }

            // Update grid
            terrainGrid[x, y] = entityId;
            return true;
        }

        /// <summary>
        /// Places a unit entity on the grid
        /// </summary>
        public bool PlaceUnitAt(Entity entityId, int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return false;
            }

            // Check if space is occupied
            if (unitGrid[x, y].Id != -1)
            {
                return false;
            }

            // Update entity's position component
            if (!ComponentManager.TryGetComponent<PositionComponent>(entityId, out var position))
            {
                position = new PositionComponent(x, y);
                ComponentManager.AddComponent(entityId, position);
            }
            else
            {
                // If the entity already has a position, update unit grid at old position
                if (IsInBounds(position.X, position.Y))
                {
                    unitGrid[position.X, position.Y] = new Entity(-1);
                }

                // Update position component
                position.X = x;
                position.Y = y;
                ComponentManager.AddComponent(entityId, position);
            }

            // Update grid
            unitGrid[x, y] = entityId;
            return true;
        }

        /// <summary>
        /// Gets the unit at the specified grid position
        /// </summary>
        public Entity GetUnitAt(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return new Entity(-1);
            }

            return unitGrid[x, y];
        }

        /// <summary>
        /// Gets the terrain at the specified grid position
        /// </summary>
        public Entity GetTerrainAt(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return new Entity(-1);
            }

            return terrainGrid[x, y];
        }

        /// <summary>
        /// Removes a unit from the grid
        /// </summary>
        public bool RemoveUnit(Entity entityId)
        {
            if (!ComponentManager.TryGetComponent<PositionComponent>(entityId, out var position))
            {
                return false;
            }

            if (IsInBounds(position.X, position.Y) && unitGrid[position.X, position.Y].Equals(entityId))
            {
                unitGrid[position.X, position.Y] = new Entity(-1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a position is within grid bounds
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// Gets the movement cost for a position
        /// </summary>
        public int GetMovementCost(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return int.MaxValue;
            }

            var terrainEntity = terrainGrid[x, y];

            if (terrainEntity.Id != -1 &&
                ComponentManager.TryGetComponent<TerrainComponent>(terrainEntity, out var terrain))
            {
                return terrain.MovementCost;
            }

            return 1; // Default movement cost
        }

        /// <summary>
        /// Gets the defense bonus for a position
        /// </summary>
        public int GetDefenseBonus(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return 0;
            }

            var terrainEntity = terrainGrid[x, y];

            if (terrainEntity.Id != -1 &&
                ComponentManager.TryGetComponent<TerrainComponent>(terrainEntity, out var terrain))
            {
                return terrain.DefenseBonus;
            }

            return 0; // Default defense bonus
        }

        /// <summary>
        /// Calculates a path between two points (A* algorithm)
        /// </summary>
        public List<GridPosition> FindPath(int startX, int startY, int endX, int endY, int movementLimit = int.MaxValue)
        {
            if (!IsInBounds(startX, startY) || !IsInBounds(endX, endY))
            {
                return new List<GridPosition>();
            }

            var start = new GridPosition(startX, startY);
            var end = new GridPosition(endX, endY);

            // Check for unit at destination
            if (unitGrid[endX, endY].Id != -1)
            {
                return new List<GridPosition>();
            }

            // A* algorithm
            var openSet = new PriorityQueue<GridPosition, int>();
            var closedSet = new HashSet<GridPosition>();
            var cameFrom = new Dictionary<GridPosition, GridPosition>();
            var gScore = new Dictionary<GridPosition, int>();
            var fScore = new Dictionary<GridPosition, int>();

            openSet.Enqueue(start, 0);
            gScore[start] = 0;
            fScore[start] = ManhattanDistance(start, end);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Equals(end))
                {
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                // Consider all neighbors
                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    var unitEntity = unitGrid[neighbor.X, neighbor.Y];
                    if (unitEntity.Id != -1 && !neighbor.Equals(end))
                    {
                        continue; // Can't move through units
                    }

                    var tentativeGScore = gScore[current] + GetMovementCost(neighbor.X, neighbor.Y);

                    if (tentativeGScore > movementLimit)
                    {
                        continue; // Exceeds movement range
                    }

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + ManhattanDistance(neighbor, end);

                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return new List<GridPosition>(); // No path found
        }

        /// <summary>
        /// Gets the valid neighbors for a position
        /// </summary>
        private List<GridPosition> GetNeighbors(GridPosition position)
        {
            var neighbors = new List<GridPosition>();

            // Check all four directions
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int newX = position.X + dx[i];
                int newY = position.Y + dy[i];

                if (IsInBounds(newX, newY))
                {
                    neighbors.Add(new GridPosition(newX, newY));
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Calculates Manhattan distance between two grid positions
        /// </summary>
        private int ManhattanDistance(GridPosition a, GridPosition b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        /// <summary>
        /// Reconstructs a path from the A* algorithm
        /// </summary>
        private List<GridPosition> ReconstructPath(Dictionary<GridPosition, GridPosition> cameFrom, GridPosition current)
        {
            var path = new List<GridPosition> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Update the grid system
        /// </summary>
        public override void Update(float deltaTime)
        {
            // This system is mostly reactive, so we don't need to do much in Update
            // Could add animation or visual effects here
        }

        /// <summary>
        /// Priority queue implementation for A* algorithm
        /// </summary>
        private class PriorityQueue<T, TPriority> where TPriority : IComparable<TPriority>
        {
            private List<(T item, TPriority priority)> elements = new List<(T, TPriority)>();

            public int Count => elements.Count;

            public void Enqueue(T item, TPriority priority)
            {
                elements.Add((item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;

                for (int i = 1; i < elements.Count; i++)
                {
                    if (elements[i].priority.CompareTo(elements[bestIndex].priority) < 0)
                    {
                        bestIndex = i;
                    }
                }

                var bestItem = elements[bestIndex].item;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }
        }
    }
}
