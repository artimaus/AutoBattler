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


        private int nextEntityId = 1;
        private HashSet<Entity> entities = new HashSet<Entity>();
        private Queue<Entity> recycledIds = new Queue<Entity>();

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>
        public bool EntityExists(Entity entity)
        {
            return entities.Contains(entity);
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
                entities.Add(id = recycledIds.Dequeue());
            }
            else
            {
                // Use the next available ID
                entities.Add(id = new Entity(entities.Count));
            }
            return id;
        }

        /// <summary>
        /// Destroys an entity
        /// </summary>
        /// <param name="entity">The ID of the entity to destroy</param>
        public void DestroyEntity(ComponentManager _componentManager, Entity entity)
        {
            if (entities.Remove(entity))
            {
                _componentManager.RemoveAllComponents(entity);
                // Add the ID's raw value to the recycled pool
                recycledIds.Enqueue(entity);
             }
         }

        /// <summary>
        /// Gets all active entities
        /// </summary>
        /// <returns>An array of all entity IDs</returns>
        public Entity[] GetAllEntities()
        {
            var result = new Entity[entities.Count];
            entities.CopyTo(result, 0);
            return result;
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
