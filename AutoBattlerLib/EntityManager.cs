using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Unique identifier for entities
    /// </summary>
    public struct Entity : IEquatable<Entity>
    {
        public int Id { get; }
        public Entity(int id)
        {
            Id = id;
        }
        public bool Equals(Entity other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Entity left, Entity right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Manages entity creation and destruction
    /// </summary>
    public class EntityManager
    {

        private readonly ComponentManager _componentManager;


        private int nextEntityId = 0;
        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private Dictionary<int, HashSet<int>> childrenOfEntity = new Dictionary<int, HashSet<int>>();
        private Dictionary<int, HashSet<int>> parentsOfEntity = new Dictionary<int, HashSet<int>>();
        private Queue<int> recycledIds = new Queue<int>();

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>
        public bool GetEntity(int id, out Entity entity)
        {
            if (entities.ContainsKey(id))
            {
                entity = entities[id];
                return true;
            }
            entity = default;
            return false;
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns>The ID of the new entity</returns>
        public Entity CreateEntity()
        {
            Entity id;
        
            // Check if we have recycled IDs available
            if (recycledIds.Count > 0)
            {
                // Use a recycled ID
                id = new Entity(recycledIds.Dequeue());
            }
            else
            {
                // Use the next available ID
                id = new Entity(nextEntityId++);
            }
        
            entities.Add(id.Id, id);
            return id;
        }


        public void PairParentChild(Entity parent, Entity child)
        {
            if (!entities.ContainsKey(parent.Id) || !entities.ContainsKey(child.Id))
                throw new ArgumentException("Both parent and child must be valid entities.");

            if (!childrenOfEntity.ContainsKey(parent.Id))
            {
                childrenOfEntity[parent.Id] = new HashSet<int>();
            }
            childrenOfEntity[parent.Id].Add(child.Id);

            if (!parentsOfEntity.ContainsKey(child.Id))
            {
                parentsOfEntity[child.Id] = new HashSet<int>();
            }
            parentsOfEntity[child.Id].Add(parent.Id);
        }

        /// <summary>
        /// Destroys an entity
        /// </summary>
        /// <param name="id">The ID of the entity to destroy</param>
        public void DestroyEntity(Entity id)
        {
            if (entities.Remove(id.Id))
            {
                _componentManager.RemoveAllComponents(id);
                parentsOfEntity.Remove(id.Id);
                // Add the ID's raw value to the recycled pool
                recycledIds.Enqueue(id.Id);

                foreach (var childId in childrenOfEntity[id.Id])
                {
                    if (parentsOfEntity[childId].Count < 2)
                    {
                        DestroyEntity(entities[childId]);
                    }
                    else
                    {
                        parentsOfEntity[childId].Remove(id.Id);
                    }
                }
                childrenOfEntity.Remove(id.Id);
            }
        }

        /// <summary>
        /// Gets all active entities
        /// </summary>
        /// <returns>An array of all entity IDs</returns>
        public Entity[] GetAllEntities()
        {
            var result = new Entity[entities.Count];
            entities.Values.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Gets all children of the entity
        /// </summary>
        /// <returns>A hashset of all children entity IDs</returns>
        public HashSet<int> GetChildren(Entity id)
        {
           return childrenOfEntity[id.Id];
        }

        /// <summary>
        /// Gets all children of the entity containing a specified component
        /// </summary>
        /// <returns>A hashset of all relevant children entity IDs</returns>
        public List<int> GetChildrenWithComponent<T>(Entity id) where T : IComponent
        {
            HashSet<int> result = GetChildren(id);
            foreach (var childId in childrenOfEntity[id.Id])
            {
                if (!_componentManager.HasComponent<T>(entities[childId]))
                {
                    result.Remove(childId);
                }
            }
            return new List<int>(result);
        }
        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<int> GetEntitiesWithComponents(params Type[] types)
        {
            if (types.Length == 0)
                return new List<int>();

            // Start with entities that have the first component type
            if (!_componentManager.components.TryGetValue(types[0], out var firstComponentEntities))
                return new List<int>();

            var result = new HashSet<int>(firstComponentEntities.Keys);

            // Filter for entities that have all remaining component types
            for (int i = 1; i < types.Length; i++)
            {
                if (!_componentManager.components.TryGetValue(types[i], out var componentEntities))
                    return new List<int>();

                result.IntersectWith(componentEntities.Keys);
            }

            return new List<int>(result);
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<int> GetEntitiesWithComponents<T1>() where T1 : IComponent
        {
            return GetEntitiesWithComponents(typeof(T1));
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<int> GetEntitiesWithComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent
        {
            return GetEntitiesWithComponents(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<int> GetEntitiesWithComponents<T1, T2, T3>()
            where T1 : IComponent
            where T2 : IComponent
            where T3 : IComponent
        {
            return GetEntitiesWithComponents(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// Checks if an entity exists
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>True if the entity exists</returns>
        public bool Exists(Entity id)
        {
            return entities.ContainsKey(id.Id);
        }
    
        /// <summary>
        /// Gets the number of available recycled IDs
        /// </summary>
        /// <returns>Number of recycled IDs available</returns>
        public int GetRecycledIdCount()
        {
            return recycledIds.Count;
        }
    }
}
