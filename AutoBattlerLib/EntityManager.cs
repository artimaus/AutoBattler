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
    enum RelationshipType
    {
        Parent,
        Child
    }


    /// <summary>
    /// Manages entity creation and destruction
    /// </summary>
    public class EntityManager
    {


        private int nextEntityId = 1;
        private Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private Dictionary<Entity, Dictionary<RelationshipType, HashSet<Entity>>> entityRelationships = 
            new Dictionary<Entity, Dictionary<RelationshipType, HashSet<Entity>>>();
        private Queue<int> recycledIds = new Queue<int>();

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>
        public bool EntityExists(int id)
        {
            if (entities.ContainsKey(id))
            {
                return true;
            }
            return false;   
        }

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>
        public bool EntityExists(Entity entity)
        {
            return EntityExists(entity.Id);
        }

        public bool GetEntity(int id, out Entity entity)
        {
            if (entities.TryGetValue(id, out entity))
            {
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

            if (!entityRelationships.ContainsKey(parent))
            {
                entityRelationships.Add(parent, new Dictionary<RelationshipType, HashSet<Entity>>());
            }
            else if (!entityRelationships[parent].ContainsKey(RelationshipType.Child))
            {
                entityRelationships[parent].Add(RelationshipType.Child, new HashSet<Entity>());
            }
            entityRelationships[parent][RelationshipType.Child].Add(child);

            
            if (!entityRelationships.ContainsKey(child))
            {
                entityRelationships.Add(child, new Dictionary<RelationshipType, HashSet<Entity>>());
            }
            else if (!entityRelationships[child].ContainsKey(RelationshipType.Parent))
            {
                entityRelationships[child].Add(RelationshipType.Parent, new HashSet<Entity>());
            }
            entityRelationships[child][RelationshipType.Parent].Add(parent);
        }

        /// <summary>
        /// Destroys an entity
        /// </summary>
        /// <param name="id">The ID of the entity to destroy</param>
        public void DestroyEntity(ComponentManager _componentManager, Entity id)
        {
            if (entities.Remove(id.Id))
            {
                _componentManager.RemoveAllComponents(id);
                // Add the ID's raw value to the recycled pool
                recycledIds.Enqueue(id.Id);

                if (entityRelationships.TryGetValue(id, out var e))
                {
                    foreach (KeyValuePair<RelationshipType, HashSet<Entity>> relationships in e) {
                        foreach (Entity relatedEntity in relationships.Value)
                        {
                            if (relationships.Key == RelationshipType.Parent)
                            {
                                UnpairParentChild(relatedEntity, id);
                            }
                            else if (relationships.Key == RelationshipType.Child)
                            {
                                UnpairParentChild(id, relatedEntity);
                                if (!entityRelationships.TryGetValue(relatedEntity, out var c) || (c.TryGetValue(RelationshipType.Parent, out var ps) && ps.Count < 1))
                                {
                                    DestroyEntity(_componentManager, relatedEntity);
                                }
                            }
                        }
                    }
                    if (e.ContainsKey(RelationshipType.Parent))
                    {
                        foreach (var parentId in e[RelationshipType.Parent])
                        {
                            UnpairParentChild(parentId, id);
                        }
                    }



                    if (e.ContainsKey(RelationshipType.Child))
                    {
                        foreach (var childId in entityRelationships[id][RelationshipType.Child])
                        {
                            if (entityRelationships.TryGetValue(childId, out var c) && c[RelationshipType.Parent].Count < 2)
                            {
                                if () {
                                    DestroyEntity(_componentManager, childId);
                                }
                            }
                            entityRelationships[childId][RelationshipType.Parent].Remove(id.Id);
                        }
                    }
                    entityRelationships[RelationshipType.Child].Remove(id.Id);
                }
            }
        }

        private void UnpairParentChild(Entity parent, Entity child)
        {
            if (!entities.ContainsKey(parent.Id) || !entities.ContainsKey(child.Id))
                throw new ArgumentException("Both parent and child must be valid entities.");

            if (entityRelationships.TryGetValue(parent, out var p) && 
                p.ContainsKey(RelationshipType.Child))
            {
                p[RelationshipType.Child].Remove(child);
            }

            if (entityRelationships.TryGetValue(child, out var c) && 
                c.ContainsKey(RelationshipType.Parent))
            {
                c[RelationshipType.Parent].Remove(parent);
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
            return entityRelationships[RelationshipType.Child][id.Id];
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
