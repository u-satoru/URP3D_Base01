using UnityEngine;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Character
{
    /// <summary>
    /// Core character manager implementation that integrates all character control systems.
    /// Implements ICharacterManager for external system integration.
    /// Provides unified access to movement, commands, and state management.
    /// </summary>
    [RequireComponent(typeof(CharacterMover))]
    public class CharacterManager : MonoBehaviour, ICharacterManager
    {
        #region Serialized Fields

        [Header("Event Channels")]
        [SerializeField] private GameEvent<Vector3> onPositionChanged;
        [SerializeField] private GameEvent<Vector3> onVelocityChanged;
        [SerializeField] private GameEvent onLanded;
        [SerializeField] private GameEvent onJumpStarted;
        [SerializeField] private GameEvent<bool> onGroundStateChanged;

        [Header("Configuration")]
        [SerializeField] private bool enableEventNotifications = true;
        [SerializeField] private float positionChangeThreshold = 0.1f;
        [SerializeField] private float velocityChangeThreshold = 0.5f;

        #endregion

        #region Private Fields

        private CharacterMover _characterMover;
        private Vector3 _lastPosition;
        private Vector3 _lastVelocity;
        private bool _wasGrounded;

        // Command pool references for optimization
        private ICommandPoolService _commandPool;

        #endregion

        #region ICharacterManager Properties

        /// <summary>
        /// Current character position
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Current character velocity
        /// </summary>
        public Vector3 Velocity => _characterMover.Velocity;

        /// <summary>
        /// Current horizontal movement speed
        /// </summary>
        public float CurrentSpeed => _characterMover.CurrentSpeed;

        /// <summary>
        /// Is the character currently grounded
        /// </summary>
        public bool IsGrounded => _characterMover.IsGrounded;

        /// <summary>
        /// Is the character currently jumping
        /// </summary>
        public bool IsJumping => _characterMover.IsJumping;

        /// <summary>
        /// Is the character currently falling
        /// </summary>
        public bool IsFalling => _characterMover.IsFalling;

        /// <summary>
        /// Current movement mode
        /// </summary>
        public CharacterMover.MovementMode CurrentMovementMode => _characterMover.CurrentMovementMode;

        #endregion

        #region ICharacterManager Events

        /// <summary>
        /// Event fired when character position changes
        /// </summary>
        public event System.Action<Vector3> OnPositionChanged;

        /// <summary>
        /// Event fired when character velocity changes significantly
        /// </summary>
        public event System.Action<Vector3> OnVelocityChanged;

        /// <summary>
        /// Event fired when character lands on ground
        /// </summary>
        public event System.Action OnLanded;

        /// <summary>
        /// Event fired when character starts jumping
        /// </summary>
        public event System.Action OnJumpStarted;

        /// <summary>
        /// Event fired when character's grounded state changes
        /// </summary>
        public event System.Action<bool> OnGroundStateChanged;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeCommandPool();
            CacheInitialState();
        }

        private void Update()
        {
            if (enableEventNotifications)
            {
                CheckForStateChanges();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            _characterMover = GetComponent<CharacterMover>();

            if (_characterMover == null)
            {
                Debug.LogError($"CharacterManager: CharacterMover component is required on {gameObject.name}");
            }
        }

        /// <summary>
        /// Initialize command pool for optimized command execution
        /// </summary>
        private void InitializeCommandPool()
        {
            _commandPool = ServiceLocator.GetService<ICommandPoolService>();
        }

        /// <summary>
        /// Cache initial state for change detection
        /// </summary>
        private void CacheInitialState()
        {
            _lastPosition = Position;
            _lastVelocity = Velocity;
            _wasGrounded = IsGrounded;
        }

        #endregion

        #region ICharacterManager Movement Control

        /// <summary>
        /// Set movement direction for the character
        /// </summary>
        /// <param name="direction">Normalized movement direction</param>
        public void SetMovementDirection(Vector3 direction)
        {
            _characterMover.SetMovementInput(direction);
        }

        /// <summary>
        /// Set movement mode (walk, run, crouch)
        /// </summary>
        /// <param name="mode">Movement mode to set</param>
        public void SetMovementMode(CharacterMover.MovementMode mode)
        {
            _characterMover.SetMovementMode(mode);
        }

        /// <summary>
        /// Stop all character movement
        /// </summary>
        public void StopMovement()
        {
            _characterMover.StopMovement();
        }

        /// <summary>
        /// Process character input data
        /// </summary>
        /// <param name="inputData">Input data to process</param>
        public void ProcessInput(CharacterInputData inputData)
        {
            // Create and execute input processing command
            var command = CreateInputCommand(inputData);
            command.Execute();

            // Return command to pool for reuse
            if (_commandPool != null)
            {
                _commandPool.ReturnCommand(command);
            }
        }

        #endregion

        #region ICharacterManager Jump Control

        /// <summary>
        /// Attempt to make the character jump
        /// </summary>
        /// <returns>True if jump was successful</returns>
        public bool TryJump()
        {
            return _characterMover.TryJump();
        }

        /// <summary>
        /// Force the character to jump (bypasses ground checks)
        /// </summary>
        public void ForceJump()
        {
            _characterMover.ForceJump();
        }

        #endregion

        #region ICharacterManager Physics

        /// <summary>
        /// Apply external force to the character
        /// </summary>
        /// <param name="force">Force vector to apply</param>
        public void AddForce(Vector3 force)
        {
            _characterMover.AddForce(force);
        }

        /// <summary>
        /// Set vertical velocity directly
        /// </summary>
        /// <param name="verticalVelocity">Vertical velocity to set</param>
        public void SetVerticalVelocity(float verticalVelocity)
        {
            _characterMover.SetVerticalVelocity(verticalVelocity);
        }

        #endregion

        #region ICharacterManager Command Integration

        /// <summary>
        /// Execute a character control command
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command was executed successfully</returns>
        public bool ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                Debug.LogWarning("CharacterManager: Attempted to execute null command");
                return false;
            }

            try
            {
                command.Execute();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CharacterManager: Error executing command {command.GetType().Name}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a movement command for this character
        /// </summary>
        /// <param name="direction">Movement direction</param>
        /// <param name="mode">Movement mode</param>
        /// <returns>Movement command</returns>
        public MoveCharacterCommand CreateMoveCommand(Vector3 direction, CharacterMover.MovementMode mode = CharacterMover.MovementMode.Walk)
        {
            var command = _commandPool?.GetCommand<MoveCharacterCommand>() ?? new MoveCharacterCommand();
            command.Initialize(_characterMover, direction, mode);
            return command;
        }

        /// <summary>
        /// Create a jump command for this character
        /// </summary>
        /// <returns>Jump command</returns>
        public JumpCharacterCommand CreateJumpCommand()
        {
            var command = _commandPool?.GetCommand<JumpCharacterCommand>() ?? new JumpCharacterCommand();
            command.Initialize(_characterMover);
            return command;
        }

        /// <summary>
        /// Create a stop movement command for this character
        /// </summary>
        /// <returns>Stop command</returns>
        public StopCharacterCommand CreateStopCommand()
        {
            var command = _commandPool?.GetCommand<StopCharacterCommand>() ?? new StopCharacterCommand();
            command.Initialize(_characterMover);
            return command;
        }

        /// <summary>
        /// Create a force application command for this character
        /// </summary>
        /// <param name="force">Force to apply</param>
        /// <returns>Force command</returns>
        public ApplyForceCommand CreateForceCommand(Vector3 force)
        {
            var command = _commandPool?.GetCommand<ApplyForceCommand>() ?? new ApplyForceCommand();
            command.Initialize(_characterMover, force);
            return command;
        }

        /// <summary>
        /// Create an input processing command for this character
        /// </summary>
        /// <param name="inputData">Input data to process</param>
        /// <returns>Input processing command</returns>
        public ProcessCharacterInputCommand CreateInputCommand(CharacterInputData inputData)
        {
            var command = _commandPool?.GetCommand<ProcessCharacterInputCommand>() ?? new ProcessCharacterInputCommand();
            command.Initialize(_characterMover, inputData);
            return command;
        }

        #endregion

        #region ICharacterManager Utility

        /// <summary>
        /// Get current movement statistics for debugging
        /// </summary>
        /// <returns>Movement statistics string</returns>
        public string GetMovementStats()
        {
            return _characterMover.GetMovementStats();
        }

        #endregion

        #region Event Management

        /// <summary>
        /// Check for state changes and fire events accordingly
        /// </summary>
        private void CheckForStateChanges()
        {
            // Check position changes
            if (Vector3.Distance(Position, _lastPosition) > positionChangeThreshold)
            {
                FirePositionChanged(Position);
                _lastPosition = Position;
            }

            // Check velocity changes
            if (Vector3.Distance(Velocity, _lastVelocity) > velocityChangeThreshold)
            {
                FireVelocityChanged(Velocity);
                _lastVelocity = Velocity;
            }

            // Check ground state changes
            if (IsGrounded != _wasGrounded)
            {
                if (IsGrounded)
                {
                    FireLanded();
                }

                FireGroundStateChanged(IsGrounded);
                _wasGrounded = IsGrounded;
            }

            // Check jump state (velocity-based detection)
            if (!_wasGrounded && Velocity.y > 0.1f && _lastVelocity.y <= 0.1f)
            {
                FireJumpStarted();
            }
        }

        /// <summary>
        /// Fire position changed event
        /// </summary>
        /// <param name="newPosition">New position</param>
        private void FirePositionChanged(Vector3 newPosition)
        {
            OnPositionChanged?.Invoke(newPosition);
            onPositionChanged?.Raise(newPosition);
        }

        /// <summary>
        /// Fire velocity changed event
        /// </summary>
        /// <param name="newVelocity">New velocity</param>
        private void FireVelocityChanged(Vector3 newVelocity)
        {
            OnVelocityChanged?.Invoke(newVelocity);
            onVelocityChanged?.Raise(newVelocity);
        }

        /// <summary>
        /// Fire landed event
        /// </summary>
        private void FireLanded()
        {
            OnLanded?.Invoke();
            onLanded?.Raise();
        }

        /// <summary>
        /// Fire jump started event
        /// </summary>
        private void FireJumpStarted()
        {
            OnJumpStarted?.Invoke();
            onJumpStarted?.Raise();
        }

        /// <summary>
        /// Fire ground state changed event
        /// </summary>
        /// <param name="isGrounded">New grounded state</param>
        private void FireGroundStateChanged(bool isGrounded)
        {
            OnGroundStateChanged?.Invoke(isGrounded);
            onGroundStateChanged?.Raise(isGrounded);
        }

        #endregion

        #region Debug

        /// <summary>
        /// Enable or disable event notifications
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetEventNotifications(bool enabled)
        {
            enableEventNotifications = enabled;
        }

        private void OnDrawGizmosSelected()
        {
            if (_characterMover != null)
            {
                // Draw movement direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, _characterMover.InputDirection * 2f);

                // Draw velocity
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, _characterMover.Velocity);
            }
        }

        #endregion
    }
}