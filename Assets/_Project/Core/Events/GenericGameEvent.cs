using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Generic typed game event channel
    /// Unity 6 optimized version with priority-based listener management
    /// </summary>
    /// <typeparam name="T">The type of data this event carries</typeparam>
    public class GameEvent<T> : ScriptableObject
    {
        // High-performance listener management using HashSet
        private readonly HashSet<IGameEventListener<T>> listeners = new HashSet<IGameEventListener<T>>();

        // Priority-sorted listener list (cached)
        private List<IGameEventListener<T>> sortedListeners;
        private bool isDirty = true;

        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;

        // Editor debug info
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private string lastRaisedValue;
        #endif

        /// <summary>
        /// Raise the event with typed data
        /// </summary>
        /// <param name="value">The value to pass to listeners</param>
        public void Raise(T value)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent<{typeof(T).Name}>]</color> '{name}' raised at {Time.time:F2}s with {listeners.Count} listeners", this);
            }
            #endif
            #if UNITY_EDITOR
            listenerCount = listeners.Count;
            lastRaisedValue = value?.ToString() ?? "null";
            #endif

            // Event logging (simplified version)
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[GameEvent<{typeof(T).Name}>] {name} raised to {listeners.Count} listeners with value: {value}");
            #endif

            // Sort by priority (only when needed)
            if (isDirty)
            {
                RebuildSortedList();
            }

            // Execute in reverse order (safe for self-removal)
            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null)
                {
                    sortedListeners[i].OnEventRaised(value);
                }
            }
        }

        /// <summary>
        /// Raise the event asynchronously (frame-distributed)
        /// </summary>
        /// <param name="value">The value to pass to listeners</param>
        public System.Collections.IEnumerator RaiseAsync(T value)
        {
            if (isDirty)
            {
                RebuildSortedList();
            }

            foreach (var listener in sortedListeners)
            {
                if (listener != null)
                {
                    listener.OnEventRaised(value);
                    yield return null; // Next frame
                }
            }
        }

        /// <summary>
        /// Register a listener
        /// </summary>
        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;

            if (listeners.Add(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=green>[GameEvent<{typeof(T).Name}>]</color> Listener registered to '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// Unregister a listener
        /// </summary>
        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (listener == null) return;

            if (listeners.Remove(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=yellow>[GameEvent<{typeof(T).Name}>]</color> Listener unregistered from '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// Clear all listeners
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }

        /// <summary>
        /// Get the number of active listeners
        /// </summary>
        public int GetListenerCount() => listeners.Count;

        /// <summary>
        /// Check if a listener is registered
        /// </summary>
        public bool HasListener(IGameEventListener<T> listener) => listeners.Contains(listener);

        /// <summary>
        /// Rebuild the sorted listener list
        /// </summary>
        private void RebuildSortedList()
        {
            sortedListeners = listeners
                .Where(l => l != null)
                .OrderByDescending(l => l.Priority)
                .ToList();
            isDirty = false;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor: Manually raise event with default value
        /// </summary>
        [ContextMenu("Raise Event (Default)")]
        private void RaiseManually()
        {
            T defaultValue = default(T);

            // Try to create a default instance for reference types
            if (defaultValue == null && typeof(T).IsClass && typeof(T) != typeof(string))
            {
                try
                {
                    defaultValue = System.Activator.CreateInstance<T>();
                }
                catch
                {
                    UnityEngine.Debug.LogWarning($"Cannot create default instance of {typeof(T).Name}");
                    return;
                }
            }

            Raise(defaultValue);
        }

        /// <summary>
        /// Log all current listeners
        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' <{typeof(T).Name}> ===");
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    var component = listener as Component;
                    if (component != null)
                    {
                        UnityEngine.Debug.Log($"  - {component.gameObject.name}.{listener.GetType().Name} (Priority: {listener.Priority})", component);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"  - {listener.GetType().Name} (Priority: {listener.Priority})");
                    }
                }
            }
        }

        private void OnValidate()
        {
            listenerCount = listeners?.Count ?? 0;
        }
        #endif
    }

    // ===== Concrete typed GameEvent implementations =====
    // These provide Inspector-friendly GameEvent assets for common types

    // Concrete typed GameEvent implementations are in separate files:
    // - Vector2GameEvent.cs (with optimized implementation)
    // - ConcreteGameEvents.cs (for FloatGameEvent, IntGameEvent, BoolGameEvent, StringGameEvent, Vector3GameEvent, GameObjectGameEvent)
}
