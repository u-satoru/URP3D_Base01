using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Generic game event with two typed data payload parameters
    /// Provides strongly typed event communication with two data arguments
    /// </summary>
    /// <typeparam name="T1">The type of the first data argument</typeparam>
    /// <typeparam name="T2">The type of the second data argument</typeparam>
    [CreateAssetMenu(fileName = "New Two Args Game Event", menuName = "asterivo.Unity60/Events/Two Args Game Event")]
    public class GameEvent<T1, T2> : ScriptableObject
    {
        // Listeners with typed callback
        private readonly HashSet<IGameEventTwoArgsListener<T1, T2>> listeners = new HashSet<IGameEventTwoArgsListener<T1, T2>>();

        // Priority sorted listener list (cached)
        private List<IGameEventTwoArgsListener<T1, T2>> sortedListeners;
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
        /// Raise the event with two typed data arguments
        /// </summary>
        /// <param name="arg1">The first argument to pass to listeners</param>
        /// <param name="arg2">The second argument to pass to listeners</param>
        public void Raise(T1 arg1, T2 arg2)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}>]</color> '{name}' raised with {listeners.Count} listeners", this);
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
                    sortedListeners[i].OnEventRaised(arg1, arg2);
                }
            }
        }

        /// <summary>
        /// Raise the event asynchronously (frame-distributed)
        /// </summary>
        /// <param name="arg1">The first argument to pass to listeners</param>
        /// <param name="arg2">The second argument to pass to listeners</param>
        public System.Collections.IEnumerator RaiseAsync(T1 arg1, T2 arg2)
        {
            if (isDirty)
            {
                RebuildSortedList();
            }

            foreach (var listener in sortedListeners)
            {
                if (listener != null)
                {
                    listener.OnEventRaised(arg1, arg2);
                    yield return null; // Next frame
                }
            }
        }

        /// <summary>
        /// Register a listener
        /// </summary>
        public void RegisterListener(IGameEventTwoArgsListener<T1, T2> listener)
        {
            if (listener == null) return;

            if (listeners.Add(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}>] Listener registered: {listener.GetType().Name}", this);
                }
                #endif
            }
        }

        /// <summary>
        /// Unregister a listener
        /// </summary>
        public void UnregisterListener(IGameEventTwoArgsListener<T1, T2> listener)
        {
            if (listener == null) return;

            if (listeners.Remove(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}>] Listener unregistered: {listener.GetType().Name}", this);
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
        public bool HasListener(IGameEventTwoArgsListener<T1, T2> listener) => listeners.Contains(listener);

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
            T1 defaultArg1 = default(T1);
            T2 defaultArg2 = default(T2);

            if (defaultArg1 == null && typeof(T1).IsClass)
            {
                try { defaultArg1 = System.Activator.CreateInstance<T1>(); }
                catch { UnityEngine.Debug.LogWarning($"Cannot create default instance of {typeof(T1).Name}"); return; }
            }

            if (defaultArg2 == null && typeof(T2).IsClass)
            {
                try { defaultArg2 = System.Activator.CreateInstance<T2>(); }
                catch { UnityEngine.Debug.LogWarning($"Cannot create default instance of {typeof(T2).Name}"); return; }
            }

            Raise(defaultArg1, defaultArg2);
        }

        /// <summary>
        /// Log all current listeners
        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' <{typeof(T1).Name}, {typeof(T2).Name}> ===");
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

    /// <summary>
    /// Interface for objects that listen to two-argument game events
    /// </summary>
    public interface IGameEventTwoArgsListener<T1, T2>
    {
        /// <summary>
        /// Called when the game event is raised
        /// </summary>
        /// <param name="arg1">First argument data</param>
        /// <param name="arg2">Second argument data</param>
        void OnEventRaised(T1 arg1, T2 arg2);

        /// <summary>
        /// Priority for event execution order (higher values execute first)
        /// </summary>
        int Priority { get; }
    }
}
