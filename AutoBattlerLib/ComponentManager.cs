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
        Form,
        BodyPart,
        LeaderStat,
        Resistance,
        Attributes,
        Skill,
        Item,
        StatusEffect
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
        public Dictionary<Component, IComponentData> components = new Dictionary<Component, IComponentData>();
        public Dictionary<Component, ComponentType> componentTypes = new Dictionary<Component, ComponentType>();
        public Dictionary<ComponentType, Dictionary<Entity, HashSet<Component>>> entityComponents = new
            Dictionary<ComponentType, Dictionary<Entity, HashSet<Component>>>();


        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        public bool AddEntityComponent(Entity entity, Component component)
        {
            if (!TryGetTypeOfComponent(component, out var type))
            {
                return false;
            }

            if (!entityComponents.ContainsKey(type))
            {
                entityComponents.Add(type, new Dictionary<Entity, HashSet<Component>>());
            }

            if (!entityComponents[type].ContainsKey(entity))
            {
                entityComponents[type][entity] = new HashSet<Component>();
            }

            /// Check if the entity already has a component of this type
            return entityComponents[type][entity].Add(component);
        }

        /// <summary>
        /// Removes a component from an entity
        /// </summary>
        private bool RemoveEntityComponent(Entity entity, Component component)
        {
            if (!TryGetTypeOfComponent(component, out var type) || !HasComponentType(entity, type))
            {
                return false;
            }

            return entityComponents[type][entity].Remove(component);
        }

        public bool TryGetTypeOfComponent(Component component, out ComponentType type)
        {
            if (!componentTypes.TryGetValue(component, out type) || !components.ContainsKey(component))
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

            return components[component];
        }

        public IComponentData GetComponentData(Component component, ComponentType type)
        {
            if (!components.ContainsKey(component))
            {
                return default;
            }

            return components[component];
        }

        /// <summary>
        /// Tries to get components from an entity
        /// </summary>
        public bool TryGetComponents(Entity entity, ComponentType type, out List<IComponentData> componentList)
        {
            componentList = new List<IComponentData>();
            if (!HasComponentType(entity, type))
            {
                return false;
            }

            foreach (Component component in entityComponents[type][entity])
            {
                componentList.Add(GetComponentData(component, type));
            }
            return (componentList.Count > 0) ? true : false;
         }

        /// <summary>
        /// Checks if an entity has a component
        /// </summary>
        public bool HasComponentType(Entity entity, ComponentType type)
        {
            if (entityComponents.ContainsKey(type))
            {
                return entityComponents[type].ContainsKey(entity);
            }
            return false;
        }


        /// <summary>
        /// Gets all entities that have all of the specified component types
        /// </summary>
        public List<Entity> GetEntitiesWithComponents(params ComponentType[] types)
        {
            if (types.Length == 0 || !entityComponents.TryGetValue(types[0], out var firstTypeEntities))
                return new List<Entity>();

            var result = new HashSet<Entity>(firstTypeEntities.Keys);

            // Filter for entities that have all remaining component types
            for (int i = 1; i < types.Length; i++)
            {
                if (!entityComponents.TryGetValue(types[i], out var componentEntities))
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
            foreach (var componentType in entityComponents)
            {
                if (componentType.Value.ContainsKey(entity))
                {
                    foreach(var doomedComponent in componentType.Value[entity])
                    {
                        components.Remove(doomedComponent);
                    }
                }
                componentType.Value.Remove(entity);
            }
        }
    }
}
