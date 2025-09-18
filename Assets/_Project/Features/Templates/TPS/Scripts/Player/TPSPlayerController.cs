using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.TPS.Services;
using asterivo.Unity60.Features.Templates.TPS.Player.StateMachine;
using asterivo.Unity60.Features.Templates.TPS.Data;

namespace asterivo.Unity60.Features.Templates.TPS.Player
{
    /// <summary>
    /// TPS Player Controller - Central player management with ServiceLocator integration
    /// Implements state machine pattern for player behavior management
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class TPSPlayerController : MonoBehaviour
    {
        [Header("Player Configuration")]
        [SerializeField] private TPSPlayerData _playerData;
        [SerializeField] private Transform _cameraTargetTransform;

        [Header("ServiceLocator Dependencies")]
        [SerializeField] private TPSServiceManager _serviceManager;

        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _playerRunSound;
        [SerializeField] private AudioClip _playerJumpSound;
        [SerializeField] private AudioClip _playerLandSound;
        [SerializeField] private AudioClip _weaponFireSound;
        [SerializeField] private AudioClip _weaponReloadSound;
        [SerializeField] private AudioClip _playerDamageSound;
        [SerializeField] private AudioClip _playerHealSound;

        // Core components
        private CharacterController _characterController;
        private TPSPlayerStateMachine _stateMachine;
        private TPSPlayerHealth _playerHealth;
        private TPSPlayerInventory _playerInventory;

        // ServiceLocator-managed dependencies
        private IInputManager _inputManager;
        private IAudioManager _audioManager;
        private IGameEventManager _eventManager;

        // Player state
        private Vector3 _velocity;
        private bool _isGrounded;
        private float _groundCheckDistance = 0.2f;
        private float _movementMultiplier = 1.0f;

        // Events for player state changes
        [Header("Event Channels")]
        [SerializeField] private GameEvent<Vector3> _playerMovedEvent;
        [SerializeField] private GameEvent<PlayerState> _playerStateChangedEvent;
        [SerializeField] private GameEvent<float> _playerHealthChangedEvent;

        public TPSPlayerData PlayerData => _playerData;
        public CharacterController CharacterController => _characterController;
        public TPSPlayerStateMachine StateMachine => _stateMachine;
        public TPSPlayerHealth Health => _playerHealth;
        public TPSPlayerInventory Inventory => _playerInventory;
        public Vector3 Velocity => _velocity;
        public bool IsGrounded => _isGrounded;
        public Transform CameraTarget => _cameraTargetTransform;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeServices();
            InitializeStateMachine();
            InitializeEventSubscriptions();
        }

        private void Update()
        {
            UpdateGroundCheck();
            _stateMachine?.Update();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
        }

        /// <summary>
        /// Initialize core components
        /// </summary>
/// <summary>
        /// Initialize core components
        /// </summary>
        private void InitializeComponents()
        {
            _characterController = GetComponent<CharacterController>();

            // Initialize health system
            _playerHealth = GetComponent<TPSPlayerHealth>();
            if (_playerHealth == null)
            {
                _playerHealth = gameObject.AddComponent<TPSPlayerHealth>();
            }
            
            // Initialize the health system
            _playerHealth.Initialize();

            // Set player data if available
            if (_playerData != null)
            {
                _playerHealth.SetPlayerData(_playerData);
            }

            // Initialize inventory system (placeholder for future implementation)
            _playerInventory = GetComponent<TPSPlayerInventory>();

            if (_cameraTargetTransform == null)
            {
                // Create default camera target
                GameObject cameraTarget = new GameObject("CameraTarget");
                cameraTarget.transform.SetParent(transform);
                cameraTarget.transform.localPosition = new Vector3(0, 1.6f, 0);
                _cameraTargetTransform = cameraTarget.transform;
            }
        }

        /// <summary>
        /// Initialize ServiceLocator dependencies
        /// </summary>
        private void InitializeServices()
        {
            if (_serviceManager == null)
            {
                _serviceManager = FindObjectOfType<TPSServiceManager>();
                if (_serviceManager == null)
                {
                    Debug.LogError("[TPSPlayerController] TPSServiceManager not found in scene!");
                    return;
                }
            }

            _serviceManager.InitializeServices();

            // Get services from ServiceLocator via TPSServiceManager
            _inputManager = _serviceManager.GetInputManager();
            _audioManager = _serviceManager.GetAudioManager();
            _eventManager = _serviceManager.GetGameEventManager();
        }

        /// <summary>
        /// Initialize state machine system
        /// </summary>
        private void InitializeStateMachine()
        {
            _stateMachine = new TPSPlayerStateMachine();
            _stateMachine.Initialize(this);
        }

        /// <summary>
        /// Initialize event system subscriptions
        /// </summary>
        private void InitializeEventSubscriptions()
        {
            // Subscribe to health events
            _playerHealth?.OnHealthChanged.AddListener(OnHealthChanged);

            // Subscribe to state change events
            _stateMachine?.OnStateChanged.AddListener(OnStateChanged);
        }

        /// <summary>
        /// Update ground check
        /// </summary>
        private void UpdateGroundCheck()
        {
            Vector3 spherePosition = transform.position + Vector3.down * _groundCheckDistance;
            _isGrounded = Physics.CheckSphere(spherePosition, 0.1f, _playerData.GroundLayerMask);
        }

        /// <summary>
        /// Apply movement to character controller
        /// </summary>
        private void ApplyMovement()
        {
            // Apply gravity if not grounded
            if (!_isGrounded)
            {
                _velocity.y += _playerData.Gravity * Time.fixedDeltaTime;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to stay grounded
            }

            // Apply movement
            _characterController.Move(_velocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Set movement velocity
        /// </summary>
        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        /// <summary>
        /// Add movement velocity
        /// </summary>
        public void AddVelocity(Vector3 velocity)
        {
            _velocity += velocity;
        }

        /// <summary>
        /// Set movement speed multiplier
        /// </summary>
        public void SetMovementMultiplier(float multiplier)
        {
            _movementMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        }

        /// <summary>
        /// Add force to the player
        /// </summary>
        public void AddForce(Vector3 force)
        {
            _velocity += force;
        }

        /// <summary>
        /// Get input from InputManager
        /// </summary>
        public Vector2 GetMovementInput()
        {
            return _inputManager?.GetMovementInput() ?? Vector2.zero;
        }

        /// <summary>
        /// Get look input from InputManager
        /// </summary>
        public Vector2 GetLookInput()
        {
            return _inputManager?.GetLookInput() ?? Vector2.zero;
        }

        /// <summary>
        /// Check if jump input is pressed
        /// </summary>
        public bool IsJumpPressed()
        {
            return _inputManager?.IsJumpPressed() ?? false;
        }

        /// <summary>
        /// Check if sprint input is held
        /// </summary>
        public bool IsSprintHeld()
        {
            return _inputManager?.IsSprintHeld() ?? false;
        }

        /// <summary>
        /// Check if crouch input is pressed
        /// </summary>
        public bool IsCrouchPressed()
        {
            return _inputManager?.IsCrouchPressed() ?? false;
        }

        /// <summary>
        /// Handle health changed event
        /// </summary>
/// <summary>
        /// Handle health changed event
        /// </summary>
        private void OnHealthChanged(float newHealth)
        {
            // Raise event through event system
            _playerHealthChangedEvent?.Raise(newHealth);

            // Audio feedback for health changes
            if (_audioManager != null)
            {
                if (newHealth < _playerHealth.PreviousHealth)
                {
                    if (_playerDamageSound != null)
                        _audioManager.PlaySFX(_playerDamageSound);
                }
                else if (newHealth > _playerHealth.PreviousHealth)
                {
                    if (_playerHealSound != null)
                        _audioManager.PlaySFX(_playerHealSound);
                }
            }
        }

        /// <summary>
        /// Handle state changed event
        /// </summary>
/// <summary>
        /// Handle state changed event
        /// </summary>
        private void OnStateChanged(PlayerState newState)
        {
            // Raise event through event system
            _playerStateChangedEvent?.Raise(newState);

            // Audio feedback for state changes using ServiceLocator
            if (_audioManager != null)
            {
                switch (newState)
                {
                    case PlayerState.Running:
                        if (_playerRunSound != null)
                            _audioManager.PlaySFX(_playerRunSound);
                        break;
                    case PlayerState.Jumping:
                        if (_playerJumpSound != null)
                            _audioManager.PlaySFX(_playerJumpSound);
                        break;
                    case PlayerState.Landing:
                        if (_playerLandSound != null)
                            _audioManager.PlaySFX(_playerLandSound);
                        break;
                    case PlayerState.Shooting:
                        if (_weaponFireSound != null)
                            _audioManager.PlaySFX(_weaponFireSound);
                        break;
                    case PlayerState.Reloading:
                        if (_weaponReloadSound != null)
                            _audioManager.PlaySFX(_weaponReloadSound);
                        break;
                }
            }
        }

        /// <summary>
        /// Take damage
        /// </summary>
/// <summary>
        /// Take damage with optional damage source position for knockback
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageSourcePosition = default)
        {
            if (_playerHealth != null)
            {
                _playerHealth.TakeDamage(damage, damageSourcePosition);
                
                // Trigger damage state if still alive
                if (_playerHealth.CurrentHealth > 0)
                {
                    _stateMachine?.ChangeState(PlayerState.TakingDamage);
                }
                else
                {
                    _stateMachine?.ChangeState(PlayerState.Dead);
                }
            }
        }

        /// <summary>
        /// Heal player
        /// </summary>
        public void Heal(float amount)
        {
            _playerHealth?.Heal(amount);
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged.RemoveListener(OnHealthChanged);

            if (_stateMachine != null)
                _stateMachine.OnStateChanged.RemoveListener(OnStateChanged);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground check sphere
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 spherePosition = transform.position + Vector3.down * _groundCheckDistance;
            Gizmos.DrawWireSphere(spherePosition, 0.1f);
        }
    }
}