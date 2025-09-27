using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using asterivo.Unity60.Features.Templates.TPS.Data;
using asterivo.Unity60.Features.Templates.TPS.Services;
using asterivo.Unity60.Features.Templates.TPS.Player.StateMachine;
using asterivo.Unity60.Features.Combat.Interfaces;
using asterivo.Unity60.Features.Combat;
using asterivo.Unity60.Features.Combat.Components;

namespace asterivo.Unity60.Features.Templates.TPS.Player
{
    /// <summary>
    /// TPS Player Health system with ServiceLocator integration
    /// Handles health management, damage, healing, and death/respawn mechanics
    /// Wraps Combat Feature layer's HealthComponent for TPS-specific features
    /// </summary>
    [RequireComponent(typeof(asterivo.Unity60.Features.Combat.Components.HealthComponent))]
    public class TPSPlayerHealth : MonoBehaviour
    {
        [Header("=== Configuration ===")]
        [SerializeField] private TPSPlayerData _playerData;

        [Header("=== Audio Configuration ===")]
        [SerializeField] private AudioClip _playerDamageSound;
        [SerializeField] private AudioClip _playerHealSound;
        [SerializeField] private AudioClip _playerRespawnSound;
        
        [Header("=== Events ===")]
        [Space(5)]
        public UnityEvent<float> OnHealthChanged;
        public UnityEvent<float, Vector3> OnDamageTaken;
        public UnityEvent<float> OnHealed;
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;
        public UnityEvent OnHealthRegenStarted;
        public UnityEvent OnHealthRegenStopped;
        
        [Header("=== Debug Info ===")]
        [Space(5)]
        [SerializeField, ReadOnly] private float _currentHealth;
        [SerializeField, ReadOnly] private float _previousHealth;
        [SerializeField, ReadOnly] private bool _isAlive = true;
        [SerializeField, ReadOnly] private bool _isRegenerating = false;
        [SerializeField, ReadOnly] private float _lastDamageTime;
        
        // Components and services
        private TPSServiceManager _serviceManager;
        private TPSPlayerStateMachine _stateMachine;
        private Coroutine _healthRegenCoroutine;
        private asterivo.Unity60.Features.Combat.Components.HealthComponent _combatHealth;
        
        // Properties (delegating to Combat layer when possible)
        public float CurrentHealth => _combatHealth?.CurrentHealth ?? _currentHealth;
        public float PreviousHealth => _previousHealth;
        public float MaxHealth => _combatHealth?.MaxHealth ?? (_playerData?.MaxHealth ?? 100f);
        public float HealthPercentage => _combatHealth?.GetHealthPercentage() ?? (MaxHealth > 0 ? _currentHealth / MaxHealth : 0f);
        public bool IsAlive => _combatHealth?.IsAlive ?? _isAlive;
        public bool IsDead => !IsAlive;
        public bool IsRegenerating => _isRegenerating;
        public bool CanTakeDamage => _combatHealth?.CanTakeDamage ?? _isAlive;
        public bool IsRecentlyDamaged => _playerData != null && (Time.time - _lastDamageTime) < _playerData.DamageReactionDuration;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeHealth();
        }
        
        private void Start()
        {
            InitializeServices();
        }
        
        private void InitializeComponents()
        {
            _serviceManager = GetComponent<TPSServiceManager>();
            _stateMachine = GetComponent<TPSPlayerStateMachine>();
            _combatHealth = GetComponent<asterivo.Unity60.Features.Combat.Components.HealthComponent>();

            if (_serviceManager == null)
            {
                Debug.LogError("[TPSPlayerHealth] TPSServiceManager component not found!");
            }

            if (_stateMachine == null)
            {
                Debug.LogError("[TPSPlayerHealth] TPSPlayerStateMachine component not found!");
            }

            if (_combatHealth == null)
            {
                _combatHealth = gameObject.AddComponent<asterivo.Unity60.Features.Combat.Components.HealthComponent>();
            }

            ConfigureCombatHealth();
        }

        private void ConfigureCombatHealth()
        {
            if (_combatHealth != null && _playerData != null)
            {
                _combatHealth.SetMaxHealth(_playerData.MaxHealth, true);
            }
        }
        
        private void InitializeServices()
        {
            if (_serviceManager != null)
            {
                _serviceManager.InitializeServices();
            }
        }
        
        private void InitializeHealth()
        {
            if (_combatHealth != null)
            {
                _combatHealth.ResetHealth();
                _currentHealth = _combatHealth.CurrentHealth;
            }
            else
            {
                _currentHealth = MaxHealth;
            }

            _previousHealth = _currentHealth;
            _isAlive = true;
            _isRegenerating = false;
            _lastDamageTime = 0f;

            // Invoke initial health change event
            OnHealthChanged?.Invoke(_currentHealth);
        }

        /// <summary>
        /// Public initialize method for external initialization
        /// </summary>
        public void Initialize()
        {
            InitializeComponents();
            InitializeServices();
            InitializeHealth();
        }
        
        /// <summary>
        /// Take damage from external source
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageDirection = default)
        {
            if (!CanTakeDamage || damage <= 0f)
                return;

            // Store previous health
            _previousHealth = CurrentHealth;

            // Apply damage reduction
            float finalDamage = CalculateFinalDamage(damage);

            // Create damage info and apply damage through Combat layer
            var damageInfo = DamageInfo.CreateDetailed(
                finalDamage,
                null,
                transform.position,
                damageDirection,
                DamageType.Ranged
            );

            float actualDamage = _combatHealth?.TakeDamage(finalDamage, damageInfo) ?? finalDamage;
            _currentHealth = CurrentHealth;
            _lastDamageTime = Time.time;
            
            Debug.Log($"[TPSPlayerHealth] Took {finalDamage} damage. Health: {_currentHealth}/{MaxHealth}");
            
            // Stop health regeneration
            StopHealthRegeneration();
            
            // Invoke damage events
            OnDamageTaken?.Invoke(finalDamage, damageDirection);
            OnHealthChanged?.Invoke(_currentHealth);
            
            // Play damage sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null && _playerDamageSound != null)
            {
                audioManager.PlaySFX(_playerDamageSound);
            }
            
            // Transition to damage state if alive
            if (IsAlive && CurrentHealth > 0f)
            {
                TransitionToDamageState(actualDamage, damageDirection);

                // Start health regeneration after delay
                StartHealthRegenerationDelayed();
            }
            else if (CurrentHealth <= 0f)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Heal the player
        /// </summary>
        public void Heal(float healAmount)
        {
            if (!IsAlive || healAmount <= 0f)
                return;

            // Store previous health
            _previousHealth = CurrentHealth;

            float oldHealth = CurrentHealth;
            float actualHealAmount = _combatHealth?.Heal(healAmount) ?? 0f;
            _currentHealth = CurrentHealth;
            
            if (actualHealAmount > 0f)
            {
                Debug.Log($"[TPSPlayerHealth] Healed {actualHealAmount}. Health: {_currentHealth}/{MaxHealth}");
                
                OnHealed?.Invoke(actualHealAmount);
                OnHealthChanged?.Invoke(_currentHealth);
                
                // Play heal sound through ServiceLocator
                var audioManager = _serviceManager?.GetAudioManager();
                if (audioManager != null && _playerHealSound != null)
                {
                    audioManager.PlaySFX(_playerHealSound);
                }
            }
        }
        
        /// <summary>
        /// Instantly kill the player
        /// </summary>
        public void Kill()
        {
            if (!IsAlive)
                return;

            _combatHealth?.Kill();
            _currentHealth = 0f;
            Die();
        }
        
        /// <summary>
        /// Respawn the player with full health
        /// </summary>
        public void Respawn()
        {
            if (IsAlive)
                return;

            Debug.Log("[TPSPlayerHealth] Player respawning");

            // Store previous health (should be 0 when dead)
            _previousHealth = CurrentHealth;

            // Restore health through Combat layer
            _combatHealth?.Revive();
            _currentHealth = CurrentHealth;
            _isAlive = true;
            
            // Stop any ongoing coroutines
            StopHealthRegeneration();
            
            // Invoke respawn events
            OnRespawn?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth);
            
            // Play respawn sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null && _playerRespawnSound != null)
            {
                audioManager.PlaySFX(_playerRespawnSound);
            }
            
            // Start health regeneration
            StartHealthRegenerationDelayed();
        }
        
        private float CalculateFinalDamage(float rawDamage)
        {
            if (_playerData == null)
                return rawDamage;
            
            // Apply damage reduction
            float damageReduction = _playerData.DamageReduction;
            return rawDamage * (1f - damageReduction);
        }
        
        private void Die()
        {
            if (!IsAlive && _isAlive == false)
                return;

            Debug.Log("[TPSPlayerHealth] Player died");

            _isAlive = false;
            _currentHealth = 0f;
            
            // Stop health regeneration
            StopHealthRegeneration();
            
            // Invoke death events
            OnDeath?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth);
            
            // Transition to death state
            TransitionToDeathState();
        }
        
        private void TransitionToDamageState(float damageAmount, Vector3 damageDirection)
        {
            if (_stateMachine != null)
            {
                _stateMachine.TransitionToState(PlayerState.TakingDamage);
                
                // Set damage information for the state if it's a TakingDamageState
                var currentState = _stateMachine.CurrentState;
                if (currentState is StateMachine.States.TakingDamageState damageState)
                {
                    damageState.SetDamageInfo(damageAmount, damageDirection);
                }
            }
        }
        
        private void TransitionToDeathState()
        {
            if (_stateMachine != null)
            {
                _stateMachine.TransitionToState(PlayerState.Dead);
            }
        }
        
        private void StartHealthRegenerationDelayed()
        {
            if (_playerData == null || _playerData.HealthRegenRate <= 0f)
                return;
            
            // Stop any existing regeneration
            StopHealthRegeneration();
            
            // Start regeneration after delay
            _healthRegenCoroutine = StartCoroutine(HealthRegenerationCoroutine());
        }
        
        private void StopHealthRegeneration()
        {
            if (_healthRegenCoroutine != null)
            {
                StopCoroutine(_healthRegenCoroutine);
                _healthRegenCoroutine = null;
            }
            
            if (_isRegenerating)
            {
                _isRegenerating = false;
                OnHealthRegenStopped?.Invoke();
            }
        }
        
        private IEnumerator HealthRegenerationCoroutine()
        {
            // Wait for regen delay
            yield return new WaitForSeconds(_playerData.HealthRegenDelay);
            
            _isRegenerating = true;
            OnHealthRegenStarted?.Invoke();
            
            // Regenerate health over time
            while (IsAlive && CurrentHealth < MaxHealth)
            {
                // Check if we took damage recently (stop regen)
                if (Time.time - _lastDamageTime < _playerData.HealthRegenDelay)
                {
                    _isRegenerating = false;
                    OnHealthRegenStopped?.Invoke();
                    yield break;
                }

                // Regenerate health
                float regenAmount = _playerData.HealthRegenRate * Time.deltaTime;
                Heal(regenAmount);

                yield return null;
            }
            
            _isRegenerating = false;
            OnHealthRegenStopped?.Invoke();
        }
        
        /// <summary>
        /// Set player data configuration
        /// </summary>
        public void SetPlayerData(TPSPlayerData playerData)
        {
            _playerData = playerData;

            // Update Combat layer max health
            if (_combatHealth != null && _playerData != null)
            {
                _combatHealth.SetMaxHealth(_playerData.MaxHealth, true);
            }

            // Update current health if needed
            if (IsAlive && CurrentHealth > MaxHealth)
            {
                _currentHealth = MaxHealth;
                OnHealthChanged?.Invoke(CurrentHealth);
            }
        }
        
        /// <summary>
        /// Get current health status for UI
        /// </summary>
        public (float current, float max, float percentage) GetHealthStatus()
        {
            return (CurrentHealth, MaxHealth, HealthPercentage);
        }
        
        private void OnValidate()
        {
            // Ensure health is within valid range when values change in editor
            if (_playerData != null)
            {
                _currentHealth = Mathf.Clamp(_currentHealth, 0f, MaxHealth);
            }
        }
        
        #if UNITY_EDITOR
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            // Draw health bar above player
            if (_playerData != null)
            {
                Vector3 position = transform.position + Vector3.up * 2.5f;
                float barWidth = 2f;
                float barHeight = 0.2f;
                
                // Background
                Gizmos.color = Color.black;
                Gizmos.DrawCube(position, new Vector3(barWidth, barHeight, 0.1f));
                
                // Health bar
                float healthPercentage = HealthPercentage;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                Gizmos.color = healthColor;
                
                Vector3 healthBarSize = new Vector3(barWidth * healthPercentage, barHeight * 0.8f, 0.05f);
                Vector3 healthBarPos = position + Vector3.left * (barWidth * (1f - healthPercentage) * 0.5f);
                Gizmos.DrawCube(healthBarPos, healthBarSize);
            }
        }
        #endif
    }
    
    /// <summary>
    /// ReadOnly attribute for inspector display
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }
    
    #if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endif
}
