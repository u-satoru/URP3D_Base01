using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Generic game event with typed data payload
    /// Provides strongly typed event communication with data
    /// </summary>
    /// <typeparam name="T">The type of data passed with the event</typeparam>
    [CreateAssetMenu(fileName = "New Generic Game Event", menuName = "asterivo.Unity60/Events/Generic Game Event")]
    public class GameEvent<T> : ScriptableObject
    {
        // Listeners with typed callback
        private readonly HashSet<IGameEventListener<T>> listeners = new HashSet<IGameEventListener<T>>();
        
        // Priority sorted listener list (cached)
        private List<IGameEventListener<T>> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        // Editor debug info
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        #endif

        /// <summary>
        /// Raise the event with typed data
        /// </summary>
        /// <param name="eventData">The data to pass to listeners</param>
        public void Raise(T eventData)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent<{typeof(T).Name}>]</color> '{name}' raised with {listeners.Count} listeners", this);
            }
            #endif
            
            #if UNITY_EDITOR
            listenerCount = listeners.Count;
            #endif
            
            // Sort by priority if needed
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // Execute in reverse order (safe for self-removal)
            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null)
                {
                    sortedListeners[i].OnEventRaised(eventData);
                }
            }
        }
        
        /// <summary>
        /// Raise the event asynchronously (frame-distributed)
        /// </summary>
        /// <param name="eventData">The data to pass to listeners</param>
        public System.Collections.IEnumerator RaiseAsync(T eventData)
        {
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            foreach (var listener in sortedListeners)
            {
                if (listener != null)
                {
                    listener.OnEventRaised(eventData);
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
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T).Name}>] Listener registered: {listener.GetType().Name}", this);
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
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T).Name}>] Listener unregistered: {listener.GetType().Name}", this);
                }
                #endif
            }
        }
        
        /// <summary>
        /// Get the number of registered listeners
        /// </summary>
        public int GetListenerCount() => listeners.Count;
        
        /// <summary>
        /// Check if a listener is registered
        /// </summary>
        public bool HasListener(IGameEventListener<T> listener) => listeners.Contains(listener);
        
        /// <summary>
        /// Clear all listeners
        /// </summary>
        public void ClearListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }
        
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
        /// Editor: Manually raise event with default data
        /// </summary>
        [ContextMenu("Raise Event (Default)")]
        private void RaiseManually()
        {
            T defaultData = default(T);
            if (defaultData == null && typeof(T).IsClass)
            {
                // Try to create a default instance for class types
                try
                {
                    defaultData = System.Activator.CreateInstance<T>();
                }
                catch
                {
                    UnityEngine.Debug.LogWarning($"Cannot create default instance of {typeof(T).Name} for manual event raise");
                    return;
                }
            }
            
            Raise(defaultData);
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
        #endif
    }

}