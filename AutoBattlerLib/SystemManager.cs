using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBattlerLib
{
    /// <summary>
    /// Base class for all systems
    /// </summary>
    public abstract class System
    {
        protected EntityManager EntityManager { get; }
        protected ComponentManager ComponentManager { get; }
        protected EventSystem EventSystem { get; }

        protected System(EntityManager entityManager, ComponentManager componentManager, EventSystem eventSystem)
        {
            EntityManager = entityManager;
            ComponentManager = componentManager;
            EventSystem = eventSystem;
        }

        /// <summary>
        /// Update the system
        /// </summary>
        /// <param name="deltaTime">Time since last update in seconds</param>
        public abstract void Update(float deltaTime);
    }

    /// <summary>
    /// Manager of all systems
    /// </summary>
    public class SystemManager
    {
        private List<System> systems = new List<System>();
        private Dictionary<Type, System> systemsByType = new Dictionary<Type, System>();

        /// <summary>
        /// Adds a system to be managed
        /// </summary>
        public void AddSystem(System system)
        {
            systems.Add(system);
            systemsByType[system.GetType()] = system;
        }

        /// <summary>
        /// Gets a system by type
        /// </summary>
        public T GetSystem<T>() where T : System
        {
            if (systemsByType.TryGetValue(typeof(T), out var system))
            {
                return (T)system;
            }

            throw new KeyNotFoundException($"No system of type {typeof(T).Name} found");
        }

        /// <summary>
        /// Updates all systems
        /// </summary>
        public void UpdateSystems(float deltaTime)
        {
            foreach (var system in systems)
            {
                system.Update(deltaTime);
            }
        }
    }

    /// <summary>
    /// Types of events that can be dispatched
    /// </summary>
    public enum EventType
    {
        UnitMoved,
        UnitAttacked,
        UnitDestroyed,
        TurnChanged,
        GameStarted,
        GameEnded
    }

    /// <summary>
    /// Event data container
    /// </summary>
    public class GameEvent
    {
        public EventType Type { get; }
        public object Data { get; }

        public GameEvent(EventType type, object data = null)
        {
            Type = type;
            Data = data;
        }
    }

    /// <summary>
    /// System for dispatching and subscribing to events
    /// </summary>
    public class EventSystem
    {
        private Dictionary<EventType, List<Action<object>>> listeners =
            new Dictionary<EventType, List<Action<object>>>();

        /// <summary>
        /// Adds an event listener
        /// </summary>
        public void AddEventListener(EventType type, Action<object> callback)
        {
            if (!listeners.ContainsKey(type))
            {
                listeners[type] = new List<Action<object>>();
            }

            listeners[type].Add(callback);
        }

        /// <summary>
        /// Removes an event listener
        /// </summary>
        public void RemoveEventListener(EventType type, Action<object> callback)
        {
            if (listeners.TryGetValue(type, out var callbacks))
            {
                callbacks.Remove(callback);
            }
        }

        /// <summary>
        /// Dispatches an event to all listeners
        /// </summary>
        public void DispatchEvent(GameEvent evt)
        {
            if (listeners.TryGetValue(evt.Type, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(evt.Data);
                }
            }
        }
    }
}
