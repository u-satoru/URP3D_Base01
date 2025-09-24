using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Components
{
    /// <summary>
    /// Sanity management component for survival horror gameplay.
    /// Manages psychological state and triggers atmospheric changes based on sanity level.
    /// Integrates with Event-Driven Architecture for decoupled system communication.
    /// </summary>
    public class SanityComponent : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Sanity Configuration")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity = 100f;
        [SerializeField] private float sanityDecayRate = 1f;
        [SerializeField] private float criticalSanityThreshold = 20f;
        [SerializeField] private float lowSanityThreshold = 50f;

        [Header("Event Channels")]
        [SerializeField] private GameEvent<float> onSanityChanged;
        [SerializeField] private GameEvent onSanityLow;
        [SerializeField] private GameEvent onSanityCritical;
        [SerializeField] private GameEvent onSanityRestored;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        #endregion

        #region Private Fields

        private bool isLowSanityTriggered = false;
        private bool isCriticalSanityTriggered = false;
        private SanityState currentState = SanityState.Normal;

        #endregion

        #region Properties

        /// <summary>
        /// Current sanity value (0-100)
        /// </summary>
        public float CurrentSanity => currentSanity;

        /// <summary>
        /// Maximum sanity value
        /// </summary>
        public float MaxSanity => maxSanity;

        /// <summary>
        /// Normalized sanity value (0.0-1.0)
        /// </summary>
        public float SanityNormalized => currentSanity / maxSanity;

        /// <summary>
        /// Current sanity state
        /// </summary>
        public SanityState CurrentState => currentState;

        /// <summary>
        /// Is sanity in critical range
        /// </summary>
        public bool IsCritical => currentSanity <= criticalSanityThreshold;

        /// <summary>
        /// Is sanity in low range
        /// </summary>
        public bool IsLow => currentSanity <= lowSanityThreshold;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeSanity();
        }

        private void Update()
        {
            if (sanityDecayRate > 0f)
            {
                DecaySanity(sanityDecayRate * Time.deltaTime);
            }
            
            UpdateSanityState();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize sanity system
        /// </summary>
        private void InitializeSanity()
        {
            currentSanity = Mathf.Clamp(currentSanity, 0f, maxSanity);
            UpdateSanityState();
            FireSanityChanged();

            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Initialized - Current: {currentSanity:F1}, Max: {maxSanity:F1}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Decrease sanity by specified amount
        /// </summary>
        /// <param name="amount">Amount to decrease</param>
        public void DecreaseSanity(float amount)
        {
            if (amount <= 0f) return;

            float previousSanity = currentSanity;
            currentSanity = Mathf.Max(0f, currentSanity - amount);

            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Decreased sanity by {amount:F1} ({previousSanity:F1} -> {currentSanity:F1})");
            }

            FireSanityChanged();
        }

        /// <summary>
        /// Increase sanity by specified amount
        /// </summary>
        /// <param name="amount">Amount to increase</param>
        public void IncreaseSanity(float amount)
        {
            if (amount <= 0f) return;

            float previousSanity = currentSanity;
            currentSanity = Mathf.Min(maxSanity, currentSanity + amount);

            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Increased sanity by {amount:F1} ({previousSanity:F1} -> {currentSanity:F1})");
            }

            FireSanityChanged();
        }

        /// <summary>
        /// Set sanity to specific value
        /// </summary>
        /// <param name="value">New sanity value</param>
        public void SetSanity(float value)
        {
            float previousSanity = currentSanity;
            currentSanity = Mathf.Clamp(value, 0f, maxSanity);

            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Set sanity to {currentSanity:F1} (was {previousSanity:F1})");
            }

            FireSanityChanged();
        }

        /// <summary>
        /// Restore sanity to maximum
        /// </summary>
        public void RestoreSanity()
        {
            SetSanity(maxSanity);
            onSanityRestored?.Raise();

            if (enableDebugLogs)
            {
                Debug.Log("[SanityComponent] Sanity fully restored");
            }
        }

        /// <summary>
        /// Set sanity decay rate
        /// </summary>
        /// <param name="rate">New decay rate per second</param>
        public void SetDecayRate(float rate)
        {
            sanityDecayRate = Mathf.Max(0f, rate);

            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Sanity decay rate set to {sanityDecayRate:F2}/sec");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Apply continuous sanity decay
        /// </summary>
        /// <param name="amount">Amount to decay</param>
        private void DecaySanity(float amount)
        {
            if (currentSanity > 0f)
            {
                currentSanity = Mathf.Max(0f, currentSanity - amount);
                FireSanityChanged();
            }
        }

        /// <summary>
        /// Update sanity state and trigger events
        /// </summary>
        private void UpdateSanityState()
        {
            SanityState previousState = currentState;

            // Determine current state
            if (currentSanity <= criticalSanityThreshold)
            {
                currentState = SanityState.Critical;
            }
            else if (currentSanity <= lowSanityThreshold)
            {
                currentState = SanityState.Low;
            }
            else
            {
                currentState = SanityState.Normal;
            }

            // Trigger state-specific events
            if (currentState != previousState)
            {
                OnSanityStateChanged(previousState, currentState);
            }

            // Handle threshold-based triggers
            if (IsCritical && !isCriticalSanityTriggered)
            {
                isCriticalSanityTriggered = true;
                onSanityCritical?.Raise();

                if (enableDebugLogs)
                {
                    Debug.LogWarning("[SanityComponent] Critical sanity threshold reached!");
                }
            }
            else if (IsLow && !isLowSanityTriggered)
            {
                isLowSanityTriggered = true;
                onSanityLow?.Raise();

                if (enableDebugLogs)
                {
                    Debug.LogWarning("[SanityComponent] Low sanity threshold reached!");
                }
            }

            // Reset triggers when sanity improves
            if (!IsCritical && isCriticalSanityTriggered)
            {
                isCriticalSanityTriggered = false;
            }
            if (!IsLow && isLowSanityTriggered)
            {
                isLowSanityTriggered = false;
            }
        }

        /// <summary>
        /// Handle sanity state changes
        /// </summary>
        /// <param name="previousState">Previous sanity state</param>
        /// <param name="newState">New sanity state</param>
        private void OnSanityStateChanged(SanityState previousState, SanityState newState)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SanityComponent] Sanity state changed: {previousState} -> {newState}");
            }
        }

        /// <summary>
        /// Fire sanity changed event
        /// </summary>
        private void FireSanityChanged()
        {
            onSanityChanged?.Raise(currentSanity);
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get debug information
        /// </summary>
        /// <returns>Debug info string</returns>
        public string GetDebugInfo()
        {
            return $"Sanity: {currentSanity:F1}/{maxSanity:F1} ({SanityNormalized:P1}) | State: {currentState} | Decay: {sanityDecayRate:F2}/sec";
        }

        #endregion
    }

    /// <summary>
    /// Sanity state enumeration
    /// </summary>
    public enum SanityState
    {
        Normal,
        Low,
        Critical
    }
}