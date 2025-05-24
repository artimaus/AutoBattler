using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Base interface for all components
    /// </summary>
    public interface IComponent {}

    /// <summary>
    /// Component manager that stores and retrieves components
    /// </summary>
    public class ComponentManager
    {
        public Dictionary<Type, Dictionary<int, IComponent>> components =
            new Dictionary<Type, Dictionary<int, IComponent>>();

        /// <summary>
        /// Adds a component to an entity
        /// </summary>
        public Entity AddComponent<T>(Entity entityId, T component) where T : IComponent
        {
            var type = typeof(T);

            if (!components.ContainsKey(type))
            {
                components[type] = new Dictionary<int, IComponent>();
            }
            /// Check if the entity already has a component of this type
            components[type][entityId.Id] = component;
            return entityId;
        }

        /// <summary>
        /// Removes a component from an entity
        /// </summary>
        public void RemoveComponent<T>(Entity entityId) where T : IComponent
        {
            var type = typeof(T);

            if (components.ContainsKey(type))
            {
                components[type].Remove(entityId.Id);
            }
        }

        /// <summary>
        /// Gets a component from an entity
        /// </summary>
        public T GetComponent<T>(Entity entityId) where T : IComponent
        {
            var type = typeof(T);
            if (components.TryGetValue(type, out var entityComponents))
            {
                if (entityComponents.TryGetValue(entityId.Id, out var component))
                {
                    return (T)component;
                }
            }
            return default;
        }

        /// <summary>
        /// Tries to get a component from an entity
        /// </summary>
        public bool TryGetComponent<T>(Entity entityId, out T component) where T : IComponent
        {
            var type = typeof(T);
            component = default;

            if (components.TryGetValue(type, out var entityComponents))
            {
                if (entityComponents.TryGetValue(entityId.Id, out var comp))
                {
                    component = (T)comp;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an entity has a component
        /// </summary>
        public bool HasComponent<T>(Entity entityId) where T : IComponent
        {
            var type = typeof(T);

            if (components.TryGetValue(type, out var entityComponents))
            {
                return entityComponents.ContainsKey(entityId.Id);
            }

            return false;
        }

        /// <summary>
        /// Removes all components for an entity
        /// </summary>
        public void RemoveAllComponents(Entity entityId)
        {
            foreach (var componentDict in components.Values)
            {
                componentDict.Remove(entityId.Id);
            }
        }
    }

}
