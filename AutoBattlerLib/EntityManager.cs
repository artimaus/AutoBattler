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
    public struct EntityId : IEquatable<EntityId>
    {
        public int Id { get; }
        public EntityId(int id)
        {
            Id = id;
        }
        public bool Equals(EntityId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is EntityId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(EntityId left, EntityId right)
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
        private HashSet<EntityId> entities = new HashSet<EntityId>();
        private Dictionary<EntityId, HashSet<EntityId>> childrenOfEntity = new Dictionary<EntityId, HashSet<EntityId>>();
        private Dictionary<EntityId, HashSet<EntityId>> parentsOfEntity = new Dictionary<EntityId, HashSet<EntityId>>();
        private Queue<int> recycledIds = new Queue<int>();

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns>The ID of the new entity</returns>
        public EntityId CreateEntity()
        {
            EntityId id;
        
            // Check if we have recycled IDs available
            if (recycledIds.Count > 0)
            {
                // Use a recycled ID
                id = new EntityId(recycledIds.Dequeue());
            }
            else
            {
                // Use the next available ID
                id = new EntityId(nextEntityId++);
            }
        
            entities.Add(id);
            return id;
        }

        public void PairParentChild(EntityId parent, EntityId child)
        {
            if (!entities.Contains(parent) || !entities.Contains(child))
                throw new ArgumentException("Both parent and child must be valid entities.");

            if (!childrenOfEntity.ContainsKey(parent))
            {
                childrenOfEntity[parent] = new HashSet<EntityId>();
            }
            childrenOfEntity[parent].Add(child);

            if (!parentsOfEntity.ContainsKey(child))
            {
                parentsOfEntity[child] = new HashSet<EntityId>();
            }
            parentsOfEntity[child].Add(parent);
        }

        /// <summary>
        /// Destroys an entity
        /// </summary>
        /// <param name="id">The ID of the entity to destroy</param>
        public void DestroyEntity(EntityId id)
        {
            if (entities.Remove(id))
            {
                _componentManager.RemoveAllComponents(id);
                parentsOfEntity.Remove(id);
                // Add the ID's raw value to the recycled pool
                recycledIds.Enqueue(id.Id);
            }
            foreach (var child in childrenOfEntity[id])
            {
                DestroyEntity(child);
            }
            childrenOfEntity.Remove(id);
        }

        /// <summary>
        /// Gets all active entities
        /// </summary>
        /// <returns>An array of all entity IDs</returns>
        public EntityId[] GetAllEntities()
        {
            var result = new EntityId[entities.Count];
            entities.CopyTo(result);
            return result;
        }

        /// <summary>
        /// Gets all children of the entity
        /// </summary>
        /// <returns>A hashset of all children entity IDs</returns>
        public HashSet<EntityId> GetChildren(EntityId id)
        {
           return new HashSet<EntityId>(childrenOfEntity[id]);
        }

        /// <summary>
        /// Gets all children of the entity containing a specified component
        /// </summary>
        /// <returns>A hashset of all relevant children entity IDs</returns>
        public List<EntityId> GetChildrenWithComponent<T>(EntityId id) where T : IComponent
        {
            HashSet<EntityId> result = GetChildren(id);
            foreach (var child in result)
            {
                if (!_componentManager.HasComponent<T>(child))
                {
                    result.Remove(child);
                }
            }
            return new List<EntityId>(result);
        }
        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<EntityId> GetEntitiesWithComponents(params Type[] types)
        {
            if (types.Length == 0)
                return new List<EntityId>();

            // Start with entities that have the first component type
            if (!_componentManager.components.TryGetValue(types[0], out var firstComponentEntities))
                return new List<EntityId>();

            var result = new HashSet<EntityId>(firstComponentEntities.Keys);

            // Filter for entities that have all remaining component types
            for (int i = 1; i < types.Length; i++)
            {
                if (!_componentManager.components.TryGetValue(types[i], out var componentEntities))
                    return new List<EntityId>();

                result.IntersectWith(componentEntities.Keys);
            }

            return new List<EntityId>(result);
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<EntityId> GetEntitiesWithComponents<T1>() where T1 : IComponent
        {
            return GetEntitiesWithComponents(typeof(T1));
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<EntityId> GetEntitiesWithComponents<T1, T2>()
            where T1 : IComponent
            where T2 : IComponent
        {
            return GetEntitiesWithComponents(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<EntityId> GetEntitiesWithComponents<T1, T2, T3>()
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
        public bool Exists(EntityId id)
        {
            return entities.Contains(id);
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
