using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    public enum ComponentType
    {
        Unit,
        Commander,
        Form,
        Proficiencies,
        BodyPart,
        BodySlot,
        Equipment,
        NaturalWeapon,
        Item
    }

    public struct Component : IEquatable<Component>
    {
        public int Id { get; }
        public Component(int id)
        {
            Id = id;
        }
        public bool Equals(Component other)
        {
            return Id == other.Id;
        }
        public override bool Equals(object obj)
        {
            return obj is Component other && Equals(other);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(Component left, Component right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Component left, Component right)
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
    public class ComponentManager
    {
        private int nextComponentId = 1;
        private Queue<Component> recycledIds = new Queue<Component>();
        public Dictionary<Component, Entity> componentEntityLookup = new Dictionary<Component, Entity>();
        public Dictionary<Component, ComponentType> componentTypeLookup = new Dictionary<Component, ComponentType>();
        public Dictionary<ComponentType, Dictionary<Entity, Dictionary<Component, IComponentData>>> components = new
            Dictionary<ComponentType, Dictionary<Entity, Dictionary<Component, IComponentData>>>();

        public Component CreateComponent(ComponentType type)
        {
            Component comp;
            if (recycledIds.Count > 0)
            {
                comp = recycledIds.Dequeue();
            }
            else
            {
                comp = new Component(nextComponentId++);
            }
            componentTypeLookup[comp] = type;
            return comp;
        }

        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        public Component AddNewComponentToEntity(Entity entity, IComponentData data, ComponentType type)
        {
            var compId = CreateComponent(type);

            if (!components[type].ContainsKey(entity))
            {
                components[type][entity] = new Dictionary<Component, IComponentData>();
            }

            components[type][entity].Add(compId, data);
            return compId;
        }

        ///// <summary>
        ///// Adds a component to an entity
        ///// </summary>
        //public bool AddComponentToEntity(Entity entity, KeyValuePair<Component, IComponentData> component)
        //{
        //    if (!TryGetTypeOfComponent(component.Key, out var type))
        //    {
        //        return false;
        //    }

        //    if (!components[type].ContainsKey(entity))
        //    {
        //        components[type][entity] = new Dictionary<Component, IComponentData>();
        //    }

        //    /// Check if the entity already has a component of this type
        //    return components[type][entity].TryAdd(component.Key, component.Value);
        //}

        /// <summary>
        /// Removes a component from an entity
        /// </summary>
        private bool RemoveComponentFromEntity(Entity entity, Component component)
        {
            if (!TryGetTypeOfComponent(component, out var type) || !HasComponentType(entity, type))
            {
                return false;
            }

            return components[type][entity].Remove(component);
        }

        public bool TryGetEntityOfComponent(Component component, out Entity entity)
        {
            if (!componentEntityLookup.TryGetValue(component, out entity))
            {
                return false;
            }
            return true;
        }
        public bool TryGetTypeOfComponent(Component component, out ComponentType type)
        {
            if (!componentTypeLookup.TryGetValue(component, out type))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets a component from an entity
        /// </summary>
        public IComponentData GetComponentData(Component component)
        {
            if (!TryGetTypeOfComponent(component, out var type))
                return default;
            if(!TryGetEntityOfComponent(component, out var entity))
                return default;

            return components[type][entity][component];
        }

        public IComponentData GetComponentData(Component component, ComponentType type)
        {
            if (!TryGetEntityOfComponent(component, out var entity))
                return default;

            return components[type][entity][component];
        }

        public IComponentData GetComponentData(Component component, ComponentType type, Entity entity)
        {
            if (!components[type].ContainsKey(entity))
                return default;

            return components[type][entity][component];
        }

        /// <summary>
        /// Tries to get components from an entity
        /// </summary>
        public bool TryGetComponentDataOfType(Entity entity, ComponentType type, out List<IComponentData> componentList)
        {
            componentList = new List<IComponentData>();
            if (!HasComponentType(entity, type))
            {
                return false;
            }

            componentList = components[type][entity].Values.ToList();

            return (componentList.Count > 0) ? true : false;
         }

        /// <summary>
        /// Checks if an entity has a component
        /// </summary>
        public bool HasComponentType(Entity entity, ComponentType type)
        {
            if (components.ContainsKey(type))
            {
                return components[type].ContainsKey(entity);
            }
            return false;
        }


        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<Entity> GetEntitiesWithComponents(params ComponentType[] types)
        {
            if (types.Length == 0 || !components.TryGetValue(types[0], out var firstTypeEntities))
                return new List<Entity>();

            var result = new HashSet<Entity>(firstTypeEntities.Keys);

            // Filter for entities that have all remaining component types
            for (int i = 1; i < types.Length; i++)
            {
                if (!components.TryGetValue(types[i], out var componentEntities))
                    return new List<Entity>();

                result.IntersectWith(componentEntities.Keys);
            }
            return new List<Entity>(result);
        }

        /// <summary>
        /// Removes all components for an entity
        /// </summary>
        public void RemoveAllComponents(Entity entity)
        {
            foreach (var componentType in components.Keys)
            {
                if (components[componentType].ContainsKey(entity))
                {
                    foreach(var doomedComponent in components[componentType][entity].Keys)
                    {
                        componentEntityLookup.Remove(doomedComponent);
                        componentTypeLookup.Remove(doomedComponent);
                        components[componentType][entity].Remove(doomedComponent);
                    }
                }
                components[componentType].Remove(entity);
            }
        }
    }
}
