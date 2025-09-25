using UnityEngine;
using System;
// using asterivo.Unity60.Core.Components;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace asterivo.Unity60.Core.Combat
{
    /// <summary>
    /// Generic health management component for any game object (player, enemies, destructible objects)
    /// Provides core health functionality with event-driven architecture integration
    /// </summary>
    /// <remarks>
    /// This component serves as the foundation for health management across all game objects.
    /// It integrates with the Command Pattern through DamageCommand and HealCommand for centralized
    /// damage processing and undo functionality.
    ///
    /// Key Features:
    /// - Float-based health values for precise calculations
    /// - C# event system for loose coupling with other systems
    /// - Initialization support for dynamic health configuration
    /// - Dead state management with proper event notification
    ///
    /// Usage Example:
    /// <code>
    /// var healthComponent = GetComponent&lt;HealthComponent&gt;();
    /// healthComponent.Initialize(150f);
    /// healthComponent.OnHealthChanged += (current, max) => UpdateHealthUI(current, max);
    /// healthComponent.OnDied += () => HandleDeath();
    /// </code>
    ///
    /// Integration with Command Pattern:
    /// - DamageCommand.Execute() calls TakeDamage()
    /// - HealCommand.Execute() calls Heal()
    /// - Commands can be undone for testing or special game mechanics
    /// </remarks>
    public class HealthComponent : MonoBehaviour, IHealthTarget
    {
        [Header("Health Configuration")]
        [SerializeField] private float _maxHealth = 100f;

        [Header("Debug Info")]
        [SerializeField, ReadOnly] private float _currentHealth;
        [SerializeField, ReadOnly] private bool _isDead;

        #region Events

        /// <summary>
        /// Fired when health value changes (damage or healing)
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        /// <remarks>
        /// This event is triggered whenever the health value changes, allowing UI components,
        /// audio systems, and other game systems to react to health changes without tight coupling.
        ///
        /// Example usage:
        /// <code>
        /// healthComponent.OnHealthChanged += (current, max) => {
        ///     healthBar.UpdateDisplay(current / max);
        ///     if (current / max < 0.25f) PlayLowHealthWarning();
        /// };
        /// </code>
        /// </remarks>
        public event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Fired when the entity dies (health reaches 0 or below)
        /// </summary>
        /// <remarks>
        /// This event is triggered once when the entity's health reaches zero or below.
        /// It allows death-related systems to react appropriately without requiring
        /// direct references to this component.
        ///
        /// Example usage:
        /// <code>
        /// healthComponent.OnDied += () => {
        ///     PlayDeathAnimation();
        ///     DropLoot();
        ///     UpdateGameState();
        /// };
        /// </code>
        /// </remarks>
        public event Action OnDied;

        #endregion

        #region Properties

        /// <summary>
        /// Current health value (float precision)
        /// </summary>
        public float CurrentHealthFloat => _currentHealth;

        /// <summary>
        /// Maximum health value (float precision)
        /// </summary>
        public float MaxHealthFloat => _maxHealth;

        /// <summary>
        /// Whether the entity is dead (health <= 0)
        /// </summary>
        public bool IsDead => _currentHealth <= 0;

        /// <summary>
        /// Current health as a percentage (0.0 to 1.0)
        /// </summary>
        public float HealthPercentage => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;

        #endregion

        #region IHealthTarget Implementation

        /// <summary>
        /// Current health value as integer (for IHealthTarget interface compatibility)
        /// </summary>
        /// <remarks>
        /// Converts the internal float-based health value to integer for command pattern integration.
        /// Rounded to nearest integer to maintain precision while ensuring compatibility.
        /// </remarks>
        public int CurrentHealth => Mathf.RoundToInt(_currentHealth);

        /// <summary>
        /// Maximum health value as integer (for IHealthTarget interface compatibility)
        /// </summary>
        /// <remarks>
        /// Converts the internal float-based max health value to integer for command pattern integration.
        /// Rounded to nearest integer to maintain precision while ensuring compatibility.
        /// </remarks>
        public int MaxHealth => Mathf.RoundToInt(_maxHealth);

        /// <summary>
        /// Heal this entity (IHealthTarget interface implementation)
        /// </summary>
        /// <param name="amount">Amount of healing to apply (integer, converted to float internally)</param>
        /// <remarks>
        /// IHealthTarget interface implementation that converts integer input to float for internal processing.
        /// This maintains compatibility with existing command system while preserving float precision.
        /// </remarks>
        void IHealthTarget.Heal(int amount)
        {
            Heal((float)amount);
        }

        /// <summary>
        /// Apply damage to this entity (IHealthTarget interface implementation)
        /// </summary>
        /// <param name="amount">Amount of damage to apply (integer, converted to float internally)</param>
        /// <remarks>
        /// IHealthTarget interface implementation that converts integer input to float for internal processing.
        /// This maintains compatibility with existing command system while preserving float precision.
        /// </remarks>
        void IHealthTarget.TakeDamage(int amount)
        {
            TakeDamage((float)amount);
        }

        /// <summary>
        /// Apply elemental damage to this entity (IHealthTarget interface implementation)
        /// </summary>
        /// <param name="amount">Amount of damage to apply (integer, converted to float internally)</param>
        /// <param name="elementType">Type of elemental damage (e.g., "fire", "ice", "physical")</param>
        /// <remarks>
        /// IHealthTarget interface implementation that converts integer input to float for internal processing.
        /// Currently treats all elemental damage as standard damage - can be extended for elemental resistance.
        /// This maintains compatibility with existing command system while preserving float precision.
        /// </remarks>
        void IHealthTarget.TakeDamage(int amount, string elementType)
        {
            // For now, treat all elemental damage as standard damage
            // This can be extended later to support elemental resistances/weaknesses
            TakeDamage((float)amount);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Received {elementType} damage: {amount}", this);
            #endif
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize health component with specific max health value
        /// </summary>
        /// <param name="maxHealth">Maximum health value to set</param>
        /// <remarks>
        /// This method allows dynamic configuration of health values, useful for:
        /// - Different character types with varying health
        /// - Level-based health scaling
        /// - Equipment or buffs that modify max health
        ///
        /// The current health is set to the new max health value.
        /// </remarks>
        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _isDead = false;

            // Notify systems of the initial health state
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Initialized with {maxHealth} max health", this);
            #endif
        }

        /// <summary>
        /// Apply damage to this entity
        /// </summary>
        /// <param name="amount">Amount of damage to apply (positive values reduce health)</param>
        /// <remarks>
        /// This method is typically called by DamageCommand.Execute() as part of the Command Pattern.
        /// It handles:
        /// - Clamping health to minimum value of 0
        /// - Triggering health changed events
        /// - Detecting and handling death state transition
        ///
        /// The method is safe to call multiple times and handles edge cases like:
        /// - Negative damage values (treated as 0 damage)
        /// - Damage applied to already dead entities (ignored)
        /// - Overkill damage (health clamped to 0)
        /// </remarks>
        public void TakeDamage(float amount)
        {
            if (IsDead) return; // Already dead, ignore further damage

            if (amount < 0)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[HealthComponent] Negative damage amount: {amount}. Use Heal() for healing.", this);
                #endif
                return;
            }

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(_currentHealth - amount, 0);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Took {amount} damage. Health: {previousHealth} -> {_currentHealth}", this);
            #endif

            // Always notify of health change
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // Check for death transition
            if (!_isDead && _currentHealth <= 0)
            {
                _isDead = true;
                OnDied?.Invoke();

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[HealthComponent] Entity died", this);
                #endif
            }
        }

        /// <summary>
        /// Heal this entity
        /// </summary>
        /// <param name="amount">Amount of healing to apply (positive values increase health)</param>
        /// <remarks>
        /// This method is typically called by HealCommand.Execute() as part of the Command Pattern.
        /// It handles:
        /// - Clamping health to maximum value
        /// - Triggering health changed events
        /// - Preventing healing of dead entities (optional - can be modified for resurrection mechanics)
        ///
        /// The method is safe to call multiple times and handles edge cases like:
        /// - Negative healing values (treated as 0 healing)
        /// - Healing applied to dead entities (ignored by default)
        /// - Overheal attempts (health clamped to max)
        /// </remarks>
        public void Heal(float amount)
        {
            if (IsDead) return; // Dead entities cannot be healed (modify for resurrection mechanics if needed)

            if (amount < 0)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"[HealthComponent] Negative healing amount: {amount}. Use TakeDamage() for damage.", this);
                #endif
                return;
            }

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Healed {amount}. Health: {previousHealth} -> {_currentHealth}", this);
            #endif

            // Notify of health change
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Reset health to maximum value and clear death state
        /// </summary>
        /// <remarks>
        /// This method is useful for:
        /// - Respawning mechanics
        /// - Level restart functionality
        /// - Testing and debugging
        /// - Resurrection abilities
        ///
        /// It fully restores the entity to a healthy state and notifies all listeners.
        /// </remarks>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Health reset to {_maxHealth}", this);
            #endif
        }

        /// <summary>
        /// Set current health to a specific value
        /// </summary>
        /// <param name="health">Health value to set (clamped between 0 and max health)</param>
        /// <remarks>
        /// This method allows direct health manipulation for special cases like:
        /// - Loading saved game states
        /// - Cheat codes or debug functionality
        /// - Scripted events that set specific health values
        ///
        /// The value is automatically clamped to valid range and all appropriate events are fired.
        /// </remarks>
        public void SetHealth(float health)
        {
            bool wasAlive = !IsDead;
            _currentHealth = Mathf.Clamp(health, 0, _maxHealth);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // Check for death transition
            if (wasAlive && _currentHealth <= 0)
            {
                _isDead = true;
                OnDied?.Invoke();

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[HealthComponent] Entity died from SetHealth", this);
                #endif
            }
            else if (!wasAlive && _currentHealth > 0)
            {
                _isDead = false;

                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[HealthComponent] Entity revived from SetHealth", this);
                #endif
            }

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[HealthComponent] Health set to {_currentHealth}", this);
            #endif
        }

        #endregion

        #region Editor Support

        #if UNITY_EDITOR

        private void OnValidate()
        {
            // Ensure max health is always positive
            if (_maxHealth <= 0)
            {
                _maxHealth = 1f;
                Debug.LogWarning($"[HealthComponent] Max health must be positive. Set to 1.", this);
            }

            // Update current health if it exceeds new max health
            if (_currentHealth > _maxHealth)
            {
                _currentHealth = _maxHealth;
            }

            // Update debug info
            _isDead = _currentHealth <= 0;
        }

        /// <summary>
        /// Editor context menu for testing damage
        /// </summary>
        [ContextMenu("Test Damage (25)")]
        private void TestDamage()
        {
            TakeDamage(25f);
        }

        /// <summary>
        /// Editor context menu for testing healing
        /// </summary>
        [ContextMenu("Test Heal (25)")]
        private void TestHeal()
        {
            Heal(25f);
        }

        /// <summary>
        /// Editor context menu for testing death
        /// </summary>
        [ContextMenu("Test Kill")]
        private void TestKill()
        {
            TakeDamage(_currentHealth);
        }

        /// <summary>
        /// Editor context menu for testing reset
        /// </summary>
        [ContextMenu("Test Reset")]
        private void TestReset()
        {
            ResetHealth();
        }

        #endif

        #endregion
    }
}

/// <summary>
/// ReadOnly attribute for inspector display
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
