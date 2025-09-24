using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Generic game event with three typed data payload parameters
    /// Provides strongly typed event communication with three data arguments
    /// </summary>
    /// <typeparam name="T1">The type of the first data argument</typeparam>
    /// <typeparam name="T2">The type of the second data argument</typeparam>
    /// <typeparam name="T3">The type of the third data argument</typeparam>
    [CreateAssetMenu(fileName = "New Three Args Game Event", menuName = "asterivo.Unity60/Events/Three Args Game Event")]
    public class GameEvent<T1, T2, T3> : ScriptableObject
    {
        // Listeners with typed callback
        private readonly HashSet<IGameEventThreeArgsListener<T1, T2, T3>> listeners = new HashSet<IGameEventThreeArgsListener<T1, T2, T3>>();

        // Priority sorted listener list (cached)
        private List<IGameEventThreeArgsListener<T1, T2, T3>> sortedListeners;
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
        /// Raise the event with three typed data arguments
        /// </summary>
        /// <param name="arg1">The first argument to pass to listeners</param>
        /// <param name="arg2">The second argument to pass to listeners</param>
        /// <param name="arg3">The third argument to pass to listeners</param>
        public void Raise(T1 arg1, T2 arg2, T3 arg3)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>]</color> '{name}' raised with {listeners.Count} listeners", this);
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
                    sortedListeners[i].OnEventRaised(arg1, arg2, arg3);
                }
            }
        }

        /// <summary>
        /// Raise the event asynchronously (frame-distributed)
        /// </summary>
        /// <param name="arg1">The first argument to pass to listeners</param>
        /// <param name="arg2">The second argument to pass to listeners</param>
        /// <param name="arg3">The third argument to pass to listeners</param>
        public System.Collections.IEnumerator RaiseAsync(T1 arg1, T2 arg2, T3 arg3)
        {
            if (isDirty)
            {
                RebuildSortedList();
            }

            foreach (var listener in sortedListeners)
            {
                if (listener != null)
                {
                    listener.OnEventRaised(arg1, arg2, arg3);
                    yield return null; // Next frame
                }
            }
        }

        /// <summary>
        /// Register a listener
        /// </summary>
        public void RegisterListener(IGameEventThreeArgsListener<T1, T2, T3> listener)
        {
            if (listener == null) return;

            if (listeners.Add(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>] Listener registered: {listener.GetType().Name}", this);
                }
                #endif
            }
        }

        /// <summary>
        /// Unregister a listener
        /// </summary>
        public void UnregisterListener(IGameEventThreeArgsListener<T1, T2, T3> listener)
        {
            if (listener == null) return;

            if (listeners.Remove(listener))
            {
                isDirty = true;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"[GameEvent<{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}>] Listener unregistered: {listener.GetType().Name}", this);
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
        public bool HasListener(IGameEventThreeArgsListener<T1, T2, T3> listener) => listeners.Contains(listener);

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
            T3 defaultArg3 = default(T3);

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

            if (defaultArg3 == null && typeof(T3).IsClass)
            {
                try { defaultArg3 = System.Activator.CreateInstance<T3>(); }
                catch { UnityEngine.Debug.LogWarning($"Cannot create default instance of {typeof(T3).Name}"); return; }
            }

            Raise(defaultArg1, defaultArg2, defaultArg3);
        }

        /// <summary>
        /// Log all current listeners
        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' <{typeof(T1).Name}, {typeof(T2).Name}, {typeof(T3).Name}> ===");
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
    /// Interface for objects that listen to three-argument game events
    /// </summary>
    public interface IGameEventThreeArgsListener<T1, T2, T3>
    {
        /// <summary>
        /// Called when the game event is raised
        /// </summary>
        /// <param name="arg1">First argument data</param>
        /// <param name="arg2">Second argument data</param>
        /// <param name="arg3">Third argument data</param>
        void OnEventRaised(T1 arg1, T2 arg2, T3 arg3);

        /// <summary>
        /// Priority for event execution order (higher values execute first)
        /// </summary>
        int Priority { get; }
    }
}