using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace asterivo.Unity60.Core.StateMachine.Debug
{
    /// <summary>
    /// Debug information container for hierarchical state machines.
    /// Tracks state transitions, timing information, and provides debugging capabilities.
    /// </summary>
    [System.Serializable]
    public class StateDebugInfo
    {
        [SerializeField] private List<StateTransitionRecord> transitionHistory;
        [SerializeField] private DateTime creationTime;
        [SerializeField] private int totalTransitions;
        [SerializeField] private string currentState;

        /// <summary>
        /// Initialize debug info container
        /// </summary>
        public StateDebugInfo()
        {
            transitionHistory = new List<StateTransitionRecord>();
            creationTime = DateTime.UtcNow;
            totalTransitions = 0;
            currentState = string.Empty;
        }

        /// <summary>
        /// Record a state transition for debugging purposes
        /// </summary>
        /// <param name="transitionType">Type of transition (Enter, Exit, ChildTransition, etc.)</param>
        /// <param name="details">Detailed information about the transition</param>
        /// <param name="timestamp">When the transition occurred</param>
        public void RecordTransition(string transitionType, string details, DateTime timestamp)
        {
            var record = new StateTransitionRecord
            {
                TransitionType = transitionType,
                Details = details,
                Timestamp = timestamp,
                TransitionId = totalTransitions++
            };

            transitionHistory.Add(record);
            currentState = details;

            // Limit history size to prevent memory issues (keep last 100 records)
            if (transitionHistory.Count > 100)
            {
                transitionHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get read-only access to the transition history
        /// </summary>
        /// <returns>Read-only list of transition records</returns>
        public IReadOnlyList<StateTransitionRecord> GetTransitionHistory()
        {
            return transitionHistory.AsReadOnly();
        }

        /// <summary>
        /// Get the total uptime since creation
        /// </summary>
        /// <returns>TimeSpan representing uptime</returns>
        public TimeSpan GetUptime()
        {
            return DateTime.UtcNow - creationTime;
        }

        /// <summary>
        /// Get the total number of transitions recorded
        /// </summary>
        /// <returns>Total transition count</returns>
        public int GetTotalTransitions() => totalTransitions;

        /// <summary>
        /// Get the current state description
        /// </summary>
        /// <returns>Current state string</returns>
        public string GetCurrentState() => currentState;

        /// <summary>
        /// Get creation time
        /// </summary>
        /// <returns>DateTime when this debug info was created</returns>
        public DateTime GetCreationTime() => creationTime;

        /// <summary>
        /// Clear all recorded debug information
        /// </summary>
        public void ClearHistory()
        {
            transitionHistory.Clear();
            totalTransitions = 0;
        }

        /// <summary>
        /// Get recent transitions within specified time window
        /// </summary>
        /// <param name="timeWindow">Time window to look back</param>
        /// <returns>List of recent transitions</returns>
        public List<StateTransitionRecord> GetRecentTransitions(TimeSpan timeWindow)
        {
            var cutoff = DateTime.UtcNow - timeWindow;
            var recentTransitions = new List<StateTransitionRecord>();

            foreach (var record in transitionHistory)
            {
                if (record.Timestamp >= cutoff)
                {
                    recentTransitions.Add(record);
                }
            }

            return recentTransitions;
        }
    }

    /// <summary>
    /// Record of a single state transition for debugging purposes
    /// </summary>
    [System.Serializable]
    public class StateTransitionRecord
    {
        [SerializeField] public string TransitionType;
        [SerializeField] public string Details;
        [SerializeField] public DateTime Timestamp;
        [SerializeField] public int TransitionId;

        /// <summary>
        /// Get formatted string representation of this transition record
        /// </summary>
        /// <returns>Formatted transition information</returns>
        public override string ToString()
        {
            return $"[{TransitionId}] {Timestamp:HH:mm:ss.fff} - {TransitionType}: {Details}";
        }

        /// <summary>
        /// Get duration since this transition occurred
        /// </summary>
        /// <returns>TimeSpan since this transition</returns>
        public TimeSpan GetTimeSinceTransition()
        {
            return DateTime.UtcNow - Timestamp;
        }
    }
}
