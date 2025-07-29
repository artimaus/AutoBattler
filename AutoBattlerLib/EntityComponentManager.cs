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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AutoBattlerLib
{
    public enum ComponentType : ushort
    {
        Unit = 1 << 0,
        Commander = 1 << 1,
        Form = 1 << 2,
        Experience = 1 << 3,
        Coordinates = 1 << 4
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


    public interface IComponentData: IEquatable<IComponentData> { }

    /// <summary>
    /// Component manager that stores and retrieves components
    /// </summary>
    public class EntityComponentManager
    {
        public EntityComponentManager(int maxEntities) {
            entities = new BitFlagArray(maxEntities);
            unitComponents = new BitFlagMap<UnitComponent>(maxEntities);
            commanderComponents = new BitFlagMap<CommanderComponent>(maxEntities);
            formComponents = new BitFlagMap<FormComponent>(maxEntities);
            experienceComponents = new BitFlagMap<ExperienceComponent>(maxEntities);
            coordinatesComponents = new BitFlagMap<CoordinatesComponent>(maxEntities);
            nextEntityId = 1;

            recycledEntities = new BitFlagMap<Entity>(maxEntities);
            entityArchetypes = new BitFlagMap<ushort>(maxEntities);
        }
        private readonly ComponentType[] componentTypes = { ComponentType.Unit, ComponentType.Commander, ComponentType.Form, ComponentType.Experience, ComponentType.Coordinates};
        private BitFlagArray entities; // [maxEntities] true if the entity exists, false if it has been recycled
        private int nextEntityId; // next entity id


        private BitFlagMap<Entity> recycledEntities;
        //private Entity[] recycledEntities; // [maxEntities / 2] recycled entities
        //private int nextRecycledEntityId; // next recycled entity id
        //private int firstRecycledEntityId; // first recycled entity id

        private void EncycleEntity(Entity entity)
        {
            recycledEntities.SetValue(entity.Id, entity);
        }
        private Entity DecycleEntity()
        {
            return recycledEntities.Pop();
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
            if (recycledEntities.CurrentValueNum() > 0)
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
            for (int i = 1; i < nextEntityId; i++)
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

        private BitFlagMap<ushort> entityArchetypes; // [maxEntities] archetype data for each entity
        private BitFlagMap<UnitComponent> unitComponents; // [maxEntities] unit component data
        private BitFlagMap<CommanderComponent> commanderComponents; // [maxEntities] commander component data
        private BitFlagMap<FormComponent> formComponents; // [maxEntities] form component data
        private BitFlagMap<ExperienceComponent> experienceComponents; // [maxEntities] experience component data
        private BitFlagMap<CoordinatesComponent> coordinatesComponents; // [maxEntities] coordinates component data

        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        public void AttachComponentToEntity(Entity entity, UnitComponent data) 
        {
            if (!EntityExists(entity))
            {
                return;
            }
            unitComponents[entity.Id] = data;
            entityArchetypes[entity.Id] |= (ushort)ComponentType.Unit;
        }
        public void AttachComponentToEntity(Entity entity, CommanderComponent data)
        {
            if (!EntityExists(entity))
            {
                return;
            }
            commanderComponents[entity.Id] = data;
            entityArchetypes[entity.Id] |= (ushort)ComponentType.Commander;
        }
        public void AttachComponentToEntity(Entity entity, FormComponent data)
        {
            if (!EntityExists(entity))
            {
                return;
            }
            formComponents[entity.Id] = data;
            entityArchetypes[entity.Id] |= (ushort)ComponentType.Form;
        }
        public void AttachComponentToEntity(Entity entity, ExperienceComponent data)
        {
            if (!EntityExists(entity))
            {
                return;
            }
            experienceComponents[entity.Id] = data;
            entityArchetypes[entity.Id] |= (ushort)ComponentType.Experience;
        }
        public void AttachComponentToEntity(Entity entity, CoordinatesComponent data)
        {
            if (!EntityExists(entity))
            {
                return;
            }
            coordinatesComponents[entity.Id] = data;
            entityArchetypes[entity.Id] |= (ushort)ComponentType.Coordinates;
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
            switch (type)
            {
                case ComponentType.Unit:
                    unitComponents[entity.Id] = default;
                    entityArchetypes[entity.Id] &= (ushort)~ComponentType.Unit;
                    break;
                case ComponentType.Commander:
                    commanderComponents[entity.Id] = default;
                    entityArchetypes[entity.Id] &= (ushort)~ComponentType.Commander;
                    break;
                case ComponentType.Form:
                    formComponents[entity.Id] = default;
                    entityArchetypes[entity.Id] &= (ushort)~ComponentType.Form;
                    break;
                case ComponentType.Experience:
                    experienceComponents[entity.Id] = default;
                    entityArchetypes[entity.Id] &= (ushort)~ComponentType.Experience;
                    break;
                case ComponentType.Coordinates:
                    coordinatesComponents[entity.Id] = default;
                    entityArchetypes[entity.Id] &= (ushort)~ComponentType.Coordinates;
                    break;
            }
        }

        /// <summary>
        /// Checks if an entity has a component
        /// </summary>
        public bool HasComponentType(Entity entity, ComponentType type)
        {
            if (!EntityExists(entity))
            {
                return false;
            }
            return (entityArchetypes[entity.Id] & (ushort)type) != 0;
        }


        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public HashSet<Entity> GetEntitiesWithComponents(params ComponentType[] types)
        {
            if (types.Length < 1)
            {
                return new HashSet<Entity>();
            }

            int[] entityIds = entityArchetypes.GetValidKeys();
            if (entityIds.Length < 1)
            {
                return new HashSet<Entity>();
            }

            HashSet<Entity> validEntities = new HashSet<Entity>();
            ushort archetypeMask = 0;
            foreach (ComponentType type in types)
            {
                archetypeMask |= (ushort)type;
            }
            foreach (int id in entityIds)
            {
                if ((archetypeMask & entityArchetypes[id]) == archetypeMask)
                {
                    validEntities.Add(new Entity(id));
                }
            }
            return validEntities;
        }


        /// <summary>
        /// Removes all components for an entity
        /// </summary>
        public void RemoveAllComponents(Entity entity)
        {
            foreach(ComponentType type in componentTypes)
            {
                if (HasComponentType(entity, type))
                {
                    RemoveComponentFromEntity(entity, type);
                }
            }
        }

        public EntityComponentManager ForkSubManager(int maxEntities, Entity[] forkedEntities)
        {
            EntityComponentManager subManager = new EntityComponentManager(maxEntities);
            int[] eIds = new int[forkedEntities.Length];
            Entity[] cachedNewEntities = new Entity[forkedEntities.Length];
            for (int i = 0; i < forkedEntities.Length; i++)
            {
                cachedNewEntities[i] = subManager.CreateEntity();
                eIds[i] = forkedEntities[i].Id;
            }
            foreach (ComponentType type in componentTypes)
            {
                switch (type)
                {
                    case ComponentType.Unit:
                        UnitComponent[] unitC = unitComponents.GetValues(eIds);
                        for (int i = 0; i < forkedEntities.Length; i++)
                        {
                            subManager.AttachComponentToEntity(cachedNewEntities[i], unitC[i]);
                            unitC[i] = default;
                        }
                        unitComponents.SetValues(eIds, unitC);
                        break;
                    case ComponentType.Commander:
                        CommanderComponent[] commanderC = commanderComponents.GetValues(eIds);
                        for (int i = 0; i < forkedEntities.Length; i++)
                        {
                            subManager.AttachComponentToEntity(cachedNewEntities[i], commanderC[i]);
                            commanderC[i] = default;
                        }
                        commanderComponents.SetValues(eIds, commanderC);
                        break;
                    case ComponentType.Form:
                        FormComponent[] formC = formComponents.GetValues(eIds);
                        for (int i = 0; i < forkedEntities.Length; i++)
                        {
                            subManager.AttachComponentToEntity(cachedNewEntities[i], formC[i]);
                            formC[i] = default;
                        }
                        formComponents.SetValues(eIds, formC);
                        break;
                    case ComponentType.Experience:
                        ExperienceComponent[] experienceC = experienceComponents.GetValues(eIds);
                        for (int i = 0; i < forkedEntities.Length; i++)
                        {
                            subManager.AttachComponentToEntity(cachedNewEntities[i], experienceC[i]);
                            experienceC[i] = default;
                        }
                        experienceComponents.SetValues(eIds, experienceC);
                        break;
                    case ComponentType.Coordinates:
                        CoordinatesComponent[] coordinatesC = coordinatesComponents.GetValues(eIds);
                        for (int i = 0; i < forkedEntities.Length; i++)
                        {
                            subManager.AttachComponentToEntity(cachedNewEntities[i], coordinatesC[i]);
                            coordinatesC[i] = default;
                        }
                        coordinatesComponents.SetValues(eIds, coordinatesC);
                        break;
                }
            }
            foreach (Entity e in forkedEntities)
            {
                entities[e.Id] = false;
                entityArchetypes[e.Id] = 0;
                EncycleEntity(e);
            }
            return subManager;
        }

        public void MergeSubManager(EntityComponentManager subManager)
        {
            Entity[] subEntities = subManager.GetAllEntities().ToArray();
            int[] subIds = new int[subEntities.Length];
            Entity[] cachedNewEntities = new Entity[subEntities.Length];
            for (int i = 0; i < subEntities.Length; i++)
            {
                cachedNewEntities[i] = CreateEntity();
                subIds[i] = subEntities[i].Id;
            }
            foreach (ComponentType type in componentTypes)
            {
                switch (type)
                {
                    case ComponentType.Unit:
                        UnitComponent[] unitC = subManager.unitComponents.GetValues(subIds);
                        for (int i = 0; i < subEntities.Length; i++)
                        {
                            AttachComponentToEntity(cachedNewEntities[i], unitC[i]);
                            unitC[i] = default;
                        }
                        subManager.unitComponents.SetValues(subIds, unitC);
                        break;
                    case ComponentType.Commander:
                        CommanderComponent[] commanderC = subManager.commanderComponents.GetValues(subIds);
                        for (int i = 0; i < subEntities.Length; i++)
                        {
                            AttachComponentToEntity(cachedNewEntities[i], commanderC[i]);
                            commanderC[i] = default;
                        }
                        subManager.commanderComponents.SetValues(subIds, commanderC);
                        break;
                    case ComponentType.Form:
                        FormComponent[] formC = subManager.formComponents.GetValues(subIds);
                        for (int i = 0; i < subEntities.Length; i++)
                        {
                            AttachComponentToEntity(cachedNewEntities[i], formC[i]);
                            formC[i] = default;
                        }
                        subManager.formComponents.SetValues(subIds, formC);
                        break;
                    case ComponentType.Experience:
                        ExperienceComponent[] experienceC = subManager.experienceComponents.GetValues(subIds);
                        for (int i = 0; i < subEntities.Length; i++)
                        {
                            AttachComponentToEntity(cachedNewEntities[i], experienceC[i]);
                            experienceC[i] = default;
                        }
                        subManager.experienceComponents.SetValues(subIds, experienceC);
                        break;
                    case ComponentType.Coordinates:
                        CoordinatesComponent[] coordinatesC = subManager.coordinatesComponents.GetValues(subIds);
                        for (int i = 0; i < subEntities.Length; i++)
                        {
                            AttachComponentToEntity(cachedNewEntities[i], coordinatesC[i]);
                            coordinatesC[i] = default;
                        }
                        subManager.coordinatesComponents.SetValues(subIds, coordinatesC);
                        break;
                }
            }
            foreach (Entity e in subEntities)
            {
                if (subManager.EntityExists(e)) 
                {
                    subManager.entities[e.Id] = false;
                    subManager.entityArchetypes[e.Id] = 0;
                    subManager.EncycleEntity(e);
                }
            }
        }
    }
}
