using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{


    public enum ComponentType : ushort
    {
        Unit = 1,
        Commander = 2,
        Form = 4,
        Experience = 8
    }

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
    public struct ComponentId : IEquatable<ComponentId>
    {
        public int Id { get; }
        public ComponentId(int id)
        {
            Id = id;
        }
        public bool Equals(ComponentId other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is ComponentId other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(ComponentId left, ComponentId right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(ComponentId left, ComponentId right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Base interface for all components
    /// </summary>
    public interface IComponentData {}

    /// <summary>
    /// Component manager that stores and retrieves components
    /// </summary>
    public class EntityComponentManager
    {
        public EntityComponentManager() { }

        private BitFlagArray entities; // [maxEntities] true if the entity exists, false if it has been recycled
        private int nextEntityId; // next entity id

        private Entity[] recycledEntities; // [maxEntities / 2] recycled entities
        private int nextRecycledEntityId; // next recycled entity id
        private int firstRecycledEntityId; // first recycled entity id

        private void EncycleEntity(Entity entity)
        {
            recycledEntities[nextRecycledEntityId++] = entity;

            if (nextRecycledEntityId >= recycledEntities.Length)
            {
                nextRecycledEntityId -= recycledEntities.Length;
            }
        }
        private Entity DecycleEntity()
        {
            if (nextRecycledEntityId == firstRecycledEntityId)
            {
                return default;
            }

            Entity entity = recycledEntities[firstRecycledEntityId++];
            if (firstRecycledEntityId >= recycledEntities.Length)
            {
                firstRecycledEntityId -= recycledEntities.Length;
            }

            return entity;
        }

        /// <summary>
        /// Attempts to get an entity by ID
        /// </summary>
        public bool EntityExists(Entity entity)
        {
            return entities[entity.Id];
        }

        /// <summary>
        /// Creates a new entity
        /// </summary>
        /// <returns>The ID of the new entity</returns>
        public Entity CreateEntity()
        {
            Entity id;

            // Check if we have recycled IDs available
            if (firstRecycledEntityId != nextRecycledEntityId)
            {
                id = DecycleEntity();
            }
            else
            {
                // Use the next available ID
                id = new Entity(nextEntityId++);
            }
            entities[id.Id] = true;
            return id;
        }

        /// <summary>
        /// Destroys an entity
        /// </summary>
        /// <param name="entity">The ID of the entity to destroy</param>
        public void DestroyEntity(Entity entity)
        {
            if (EntityExists(entity))
            {
                entities[entity.Id] = false;
                RemoveAllComponents(entity);
                EncycleEntity(entity);
            }
        }

        /// <summary>
        /// Gets all active entities
        /// </summary>
        /// <returns>An array of all entity IDs</returns>
        public HashSet<Entity> GetAllEntities()
        {
            HashSet<Entity> activeEntities = new HashSet<Entity>();
            for (int i = 0; i < nextEntityId; i++)
            {
                if (entities[i])
                {
                    activeEntities.Add(new Entity(i));
                }
            }
            return activeEntities;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Idx(ComponentType type)
        {
            return (int)type << 1;
        }

        private ComponentId[][] recycledComponents; // [types][recycledId]
        private int[] firstRecycledComponentId; // [types]
        private int[] nextRecycledComponentId; // [types]

        private int[] nextComponentId; //[types]

        private UnitComponent[] unitComponents;
        private CommanderComponent[] commanderComponents;
        private FormComponent[] formComponents;
        private ExperienceComponent[] experienceComponents;

        private Entity[][] componentParentEntity; //[types][ComponentId] the first index is the ComponentType and the second is the ComponentId
        private ComponentId[][] entityComponents; //[maxEntities][types] the first index is the Entity and the second is the ComponentType



        private void EncycleComponent(ComponentType type, ComponentId compId)
        {
            recycledComponents[Idx(type)][nextRecycledComponentId[Idx(type)]++] = compId;
            
            if(nextRecycledComponentId[Idx(type)] >= recycledComponents[Idx(type)].Length)
            {
                nextRecycledComponentId[Idx(type)] -= recycledComponents[Idx(type)].Length;
            }
        }
        private ComponentId DecycleComponent(ComponentType type)
        {
            if (firstRecycledComponentId[Idx(type)] == nextRecycledComponentId[Idx(type)])
            {
                return default;
            }

            ComponentId compId = recycledComponents[Idx(type)][firstRecycledComponentId[Idx(type)]++];
            if (firstRecycledComponentId[Idx(type)] >= recycledComponents[Idx(type)].Length)
            {
                firstRecycledComponentId[Idx(type)] -= recycledComponents[Idx(type)].Length;
            }

            return compId;
        }

        private ComponentId LogNewComponent<T>(ComponentType type, T data) where T : struct, IComponentData
        {
            ComponentId compId;
            if (firstRecycledComponentId[Idx(type)] != nextRecycledComponentId[Idx(type)])
            {
                compId = DecycleComponent(type);
            }
            else
            {
                compId = new ComponentId(nextComponentId[Idx(type)]++);
            }
            return LogComponent(compId, data);
        }
        private ComponentId LogComponent<T>(ComponentId compId, T data) where T : struct, IComponentData
        {
            switch (data)
            {
                case UnitComponent compData:
                    unitComponents[compId.Id] = compData;
                    break;
                case CommanderComponent compData:
                    commanderComponents[compId.Id] = compData;
                    break;
                case FormComponent compData:
                    formComponents[compId.Id] = compData;
                    break;
                case ExperienceComponent compData:
                    experienceComponents[compId.Id] = compData;
                    break;
            }
            return compId;
        }
        private void DelogComponent(ComponentType type, ComponentId component)
        {
            switch (type) {
                case ComponentType.Unit:
                    unitComponents[component.Id] = default;
                    break;
                case ComponentType.Commander:
                    commanderComponents[component.Id] = default;
                    break;
                case ComponentType.Form:
                    formComponents[component.Id] = default;
                    break;
                case ComponentType.Experience:
                    experienceComponents[component.Id] = default;
                    break;
            }
            EncycleComponent(type, component);
        }

        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        public ComponentId AttachComponentToEntity<T>(Entity entity, ComponentType type, T data) where T : struct, IComponentData
        {
            ComponentId compId;
            if (HasComponentType(entity, type))
            {
                compId = LogComponent(entityComponents[entity.Id][Idx(type)], data);
            }
            else
            {
                compId = LogNewComponent(type, data);
                componentParentEntity[Idx(type)][compId.Id] = entity;
                entityComponents[entity.Id][Idx(type)] = compId;
            }
            return compId;
        }

        /// <summary>
        /// Removes a component from an entity
        /// </summary>
        public void RemoveComponentFromEntity(Entity entity, ComponentType type)
        {
            if (!HasComponentType(entity, type))
            {
                return;
            }
            ComponentId compId = entityComponents[entity.Id][Idx(type)];
            DelogComponent(type, compId);
            componentParentEntity[Idx(type)][compId.Id] = default;
            entityComponents[entity.Id][Idx(type)] = default;
        }

        /// <summary>
        /// Checks if an entity has a component
        /// </summary>
        public bool HasComponentType(Entity entity, ComponentType type)
        {
            return entityComponents[entity.Id][(ushort)type].Id != default;
        }


        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public HashSet<Entity> GetEntitiesWithComponents(params ComponentType[] types)
        {
            HashSet<Entity> validEntities = new HashSet<Entity>();
            Entity current;
            foreach (ComponentType type in types)
            {
                for (int i = 0; i < nextComponentId[Idx(type)]; i++)
                {
                    current = componentParentEntity[Idx(type)][i];
                    if (type == types[0] && current != default)
                    {
                        validEntities.Add(current);
                    }
                    else if (entityComponents[current.Id][Idx(type)] == default && validEntities.Contains(current))
                    {
                        validEntities.Remove(current);
                    }
                }
            }
            return validEntities;
        }

        public ComponentId[] GetEntityComponents(Entity entity)
        {
            return entityComponents[entity.Id];
        }


        /// <summary>
        /// Removes all components for an entity
        /// </summary>
        public void RemoveAllComponents(Entity entity)
        {
            foreach(ComponentType type in Enum.GetValues(typeof(ComponentType)))
            {
                RemoveComponentFromEntity(entity, type);
            }
        }

        public EntityComponentManager ForkSubManager(Entity[] newEntities)
        {
            EntityComponentManager subManager = new EntityComponentManager();
            foreach (Entity e in newEntities)
            {
                Entity newE = subManager.CreateEntity();
                for (int i = 0; i < entityComponents.Length; i++)
                {
                    ComponentType type = (ComponentType)(1 << i);
                    switch (type)
                    {
                        case ComponentType.Unit:
                            subManager.AttachComponentToEntity(newE, type, unitComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Commander:
                            subManager.AttachComponentToEntity(newE, type, commanderComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Form:
                            subManager.AttachComponentToEntity(newE, type, formComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Experience:
                            subManager.AttachComponentToEntity(newE, type, experienceComponents[entityComponents[e.Id][i].Id]);
                            break;
                    }
                }
            }
            foreach (Entity e in newEntities)
            {
                DestroyEntity(e);
            }
            return subManager;
        }

        public void MergeSubManager(EntityComponentManager subManager)
        {
            Entity[] subEntities = subManager.GetAllEntities().ToArray();
            foreach (Entity e in subEntities)
            {
                Entity mergedE = CreateEntity();
                for (int i = 0; i < entityComponents.Length; i++)
                {
                    ComponentType type = (ComponentType)(1 << i);
                    switch (type)
                    {
                        case ComponentType.Unit:
                            AttachComponentToEntity(mergedE, type, subManager.unitComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Commander:
                            AttachComponentToEntity(mergedE, type, subManager.commanderComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Form:
                            AttachComponentToEntity(mergedE, type, subManager.formComponents[entityComponents[e.Id][i].Id]);
                            break;
                        case ComponentType.Experience:
                            AttachComponentToEntity(mergedE, type, subManager.experienceComponents[entityComponents[e.Id][i].Id]);
                            break;
                    }
                }
            }
            foreach (Entity e in subEntities)
            {
                subManager.DestroyEntity(e);
            }
        }
    }
}
