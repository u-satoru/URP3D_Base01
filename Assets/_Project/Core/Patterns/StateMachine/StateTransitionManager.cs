using System;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Core.StateMachine
{
    /// <summary>
    /// Manages state transition rules and evaluates conditions for state changes.
    /// Supports priority-based rule evaluation and complex transition logic.
    /// </summary>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public class StateTransitionManager<TContext>
    {
        #region Private Fields

        /// <summary>
        /// Dictionary of transition rules organized by source state
        /// </summary>
        private readonly Dictionary<string, List<StateTransitionRule<TContext>>> transitionRules;

        /// <summary>
        /// Current state reference for transition evaluation
        /// </summary>
        private asterivo.Unity60.Core.Patterns.StateMachine.IState<TContext> currentState;

        /// <summary>
        /// Context object for rule evaluation
        /// </summary>
        private readonly TContext context;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize the state transition manager
        /// </summary>
        /// <param name="context">Context object for rule evaluation</param>
        public StateTransitionManager(TContext context)
        {
            this.context = context;
            transitionRules = new Dictionary<string, List<StateTransitionRule<TContext>>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a transition rule between states
        /// </summary>
        /// <param name="fromState">Source state identifier</param>
        /// <param name="toState">Target state identifier</param>
        /// <param name="condition">Condition function that must return true for transition</param>
        /// <param name="priority">Priority of this rule (higher values have higher priority)</param>
        public void AddTransitionRule(string fromState, string toState, 
            System.Func<TContext, bool> condition, int priority = 0)
        {
            if (string.IsNullOrEmpty(fromState) || string.IsNullOrEmpty(toState))
            {
                throw new ArgumentException("State names cannot be null or empty");
            }

            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            // Initialize rule list for this source state if it doesn't exist
            if (!transitionRules.ContainsKey(fromState))
            {
                transitionRules[fromState] = new List<StateTransitionRule<TContext>>();
            }

            var rule = new StateTransitionRule<TContext>
            {
                FromState = fromState,
                ToState = toState,
                Condition = condition,
                Priority = priority
            };

            transitionRules[fromState].Add(rule);

            // Sort by priority (higher priority first)
            transitionRules[fromState].Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>
        /// Remove all transition rules from a specific state
        /// </summary>
        /// <param name="fromState">Source state to remove rules from</param>
        public void RemoveTransitionRulesFrom(string fromState)
        {
            if (transitionRules.ContainsKey(fromState))
            {
                transitionRules.Remove(fromState);
            }
        }

        /// <summary>
        /// Remove a specific transition rule
        /// </summary>
        /// <param name="fromState">Source state</param>
        /// <param name="toState">Target state</param>
        public void RemoveTransitionRule(string fromState, string toState)
        {
            if (transitionRules.ContainsKey(fromState))
            {
                transitionRules[fromState].RemoveAll(rule => rule.ToState == toState);
            }
        }

        /// <summary>
        /// Evaluate all transition rules for the current state and return the target state if any condition is met
        /// </summary>
        /// <param name="currentStateName">Name of the current state</param>
        /// <returns>Target state name if transition should occur, null otherwise</returns>
        public string EvaluateTransitions(string currentStateName)
        {
            if (string.IsNullOrEmpty(currentStateName) || !transitionRules.ContainsKey(currentStateName))
            {
                return null;
            }

            // Evaluate rules in priority order
            foreach (var rule in transitionRules[currentStateName])
            {
                try
                {
                    if (rule.Condition(context))
                    {
                        return rule.ToState;
                    }
                }
                catch (Exception ex)
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError($"StateTransitionManager: Error evaluating transition rule from {rule.FromState} to {rule.ToState}: {ex.Message}");
                    #endif
                }
            }

            return null;
        }

        /// <summary>
        /// Get all possible target states from the current state
        /// </summary>
        /// <param name="fromState">Source state to check</param>
        /// <returns>Collection of possible target states</returns>
        public IReadOnlyCollection<string> GetPossibleTransitions(string fromState)
        {
            if (!transitionRules.ContainsKey(fromState))
            {
                return new List<string>().AsReadOnly();
            }

            return transitionRules[fromState]
                .Select(rule => rule.ToState)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Check if a specific transition is possible (rule exists)
        /// </summary>
        /// <param name="fromState">Source state</param>
        /// <param name="toState">Target state</param>
        /// <returns>True if a transition rule exists</returns>
        public bool HasTransitionRule(string fromState, string toState)
        {
            return transitionRules.ContainsKey(fromState) &&
                   transitionRules[fromState].Any(rule => rule.ToState == toState);
        }

        /// <summary>
        /// Get the number of transition rules for a specific state
        /// </summary>
        /// <param name="fromState">Source state to check</param>
        /// <returns>Number of transition rules</returns>
        public int GetTransitionRuleCount(string fromState)
        {
            return transitionRules.ContainsKey(fromState) ? transitionRules[fromState].Count : 0;
        }

        /// <summary>
        /// Clear all transition rules
        /// </summary>
        public void ClearAllRules()
        {
            transitionRules.Clear();
        }

        /// <summary>
        /// Get all states that have outgoing transition rules
        /// </summary>
        /// <returns>Collection of state names with outgoing rules</returns>
        public IReadOnlyCollection<string> GetStatesWithRules()
        {
            return transitionRules.Keys.ToList().AsReadOnly();
        }

        #endregion
    }

    /// <summary>
    /// Represents a single state transition rule with condition and priority
    /// </summary>
    /// <typeparam name="TContext">The context type that states operate on</typeparam>
    public class StateTransitionRule<TContext>
    {
        /// <summary>
        /// Source state identifier
        /// </summary>
        public string FromState { get; set; }

        /// <summary>
        /// Target state identifier
        /// </summary>
        public string ToState { get; set; }

        /// <summary>
        /// Condition function that must return true for the transition to occur
        /// </summary>
        public System.Func<TContext, bool> Condition { get; set; }

        /// <summary>
        /// Priority of this rule (higher values evaluated first)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// String representation of this transition rule
        /// </summary>
        /// <returns>Formatted rule description</returns>
        public override string ToString()
        {
            return $"{FromState} -> {ToState} (Priority: {Priority})";
        }
    }
}
