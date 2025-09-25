using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Character
{
    /// <summary>
    /// Core character movement and physics controller.
    /// Provides generic character movement functionality that can be used by different character types.
    /// Integrates with Command Pattern and Event-Driven Architecture.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public class CharacterMover : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 8f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        [Header("Physics Settings")]
        [SerializeField] private float terminalVelocity = -53f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Events")]
        [SerializeField] private GameEvent<Vector3> onMovementChanged;
        [SerializeField] private GameEvent onLanded;
        [SerializeField] private GameEvent onJumpStarted;
        [SerializeField] private GameEvent<bool> onGroundStateChanged;

        #endregion

        #region Private Fields

        private UnityEngine.CharacterController _characterController;
        private Vector3 _velocity;
        private Vector3 _inputDirection;
        private float _currentSpeed;
        private bool _isGrounded;
        private bool _wasGroundedLastFrame;
        private float _lastGroundedTime;
        private float _jumpBufferTimer;
        private MovementMode _currentMovementMode = MovementMode.Walk;

        // Ground check
        private Transform _groundCheckTransform;

        #endregion

        #region Enums

        /// <summary>
        /// Character movement modes
        /// </summary>
        public enum MovementMode
        {
            Walk,
            Run,
            Crouch
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current velocity of the character
        /// </summary>
        public Vector3 Velocity => _velocity;

        /// <summary>
        /// Current horizontal speed
        /// </summary>
        public float CurrentSpeed => _currentSpeed;

        /// <summary>
        /// Is the character currently grounded
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// Is the character currently falling
        /// </summary>
        public bool IsFalling => _velocity.y < 0 && !_isGrounded;

        /// <summary>
        /// Is the character currently jumping
        /// </summary>
        public bool IsJumping => _velocity.y > 0;

        /// <summary>
        /// Current movement mode
        /// </summary>
        public MovementMode CurrentMovementMode => _currentMovementMode;

        /// <summary>
        /// Character Controller component
        /// </summary>
        public UnityEngine.CharacterController CharacterController => _characterController;

        /// <summary>
        /// Current input direction (normalized)
        /// </summary>
        public Vector3 InputDirection => _inputDirection;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeGroundCheck();
        }

        private void Update()
        {
            UpdateGroundCheck();
            UpdateTimers();
            ApplyGravity();
            ProcessJumpBuffer();
        }

        private void FixedUpdate()
        {
            // Movement is processed in regular Update for input responsiveness
            // Physics calculations are handled here
            UpdatePhysics();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            _characterController = GetComponent<UnityEngine.CharacterController>();

            if (_characterController == null)
            {
                Debug.LogError($"CharacterMover: CharacterController component is required on {gameObject.name}");
            }
        }

        /// <summary>
        /// Initialize ground check system
        /// </summary>
        private void InitializeGroundCheck()
        {
            // Create ground check transform if it doesn't exist
            _groundCheckTransform = transform.Find("GroundCheck");
            if (_groundCheckTransform == null)
            {
                var groundCheckObject = new GameObject("GroundCheck");
                groundCheckObject.transform.SetParent(transform);
                groundCheckObject.transform.localPosition = Vector3.zero;
                _groundCheckTransform = groundCheckObject.transform;
            }
        }

        #endregion

        #region Movement Control

        /// <summary>
        /// Set movement input direction (typically called by Input System or Commands)
        /// </summary>
        /// <param name="direction">Movement direction (normalized)</param>
        public void SetMovementInput(Vector3 direction)
        {
            _inputDirection = direction.normalized;
        }

        /// <summary>
        /// Set movement mode (walk, run, crouch)
        /// </summary>
        /// <param name="mode">Movement mode to set</param>
        public void SetMovementMode(MovementMode mode)
        {
            if (_currentMovementMode != mode)
            {
                _currentMovementMode = mode;

                // Fire movement mode change event if needed
                // onMovementModeChanged?.Raise(mode);
            }
        }

        /// <summary>
        /// Process movement based on current input and mode
        /// </summary>
        public void ProcessMovement()
        {
            if (_characterController == null) return;

            // Calculate target speed based on movement mode
            float targetSpeed = GetTargetSpeed();

            // Apply acceleration/deceleration
            if (_inputDirection.magnitude > 0.1f)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            // Calculate horizontal movement
            Vector3 horizontalMovement = _inputDirection * _currentSpeed;

            // Combine horizontal and vertical movement
            Vector3 movement = new Vector3(horizontalMovement.x, _velocity.y, horizontalMovement.z);

            // Apply movement
            _characterController.Move(movement * Time.deltaTime);

            // Fire movement event
            onMovementChanged?.Raise(movement);
        }

        /// <summary>
        /// Get target speed based on current movement mode
        /// </summary>
        /// <returns>Target speed for current movement mode</returns>
        private float GetTargetSpeed()
        {
            return _currentMovementMode switch
            {
                MovementMode.Walk => walkSpeed,
                MovementMode.Run => runSpeed,
                MovementMode.Crouch => crouchSpeed,
                _ => walkSpeed
            };
        }

        #endregion

        #region Jump Control

        /// <summary>
        /// Attempt to jump (typically called by Input System or Commands)
        /// </summary>
        /// <returns>True if jump was successful</returns>
        public bool TryJump()
        {
            // Check if we can jump (grounded or within coyote time)
            if (_isGrounded || (_lastGroundedTime + coyoteTime > Time.time))
            {
                PerformJump();
                return true;
            }
            else
            {
                // Store jump request for jump buffering
                _jumpBufferTimer = jumpBufferTime;
                return false;
            }
        }

        /// <summary>
        /// Force jump without ground checks (use with caution)
        /// </summary>
        public void ForceJump()
        {
            PerformJump();
        }

        /// <summary>
        /// Perform the actual jump
        /// </summary>
        private void PerformJump()
        {
            float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _velocity.y = jumpVelocity;

            // Fire jump event
            onJumpStarted?.Raise();

            // Reset jump buffer
            _jumpBufferTimer = 0f;
        }

        /// <summary>
        /// Process jump buffering (allows jump input slightly before landing)
        /// </summary>
        private void ProcessJumpBuffer()
        {
            if (_jumpBufferTimer > 0 && _isGrounded)
            {
                PerformJump();
            }
        }

        #endregion

        #region Physics

        /// <summary>
        /// Apply gravity to character
        /// </summary>
        private void ApplyGravity()
        {
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }
            else
            {
                _velocity.y += gravity * Time.deltaTime;

                // Apply terminal velocity
                if (_velocity.y < terminalVelocity)
                {
                    _velocity.y = terminalVelocity;
                }
            }
        }

        /// <summary>
        /// Update physics calculations
        /// </summary>
        private void UpdatePhysics()
        {
            // Process movement in FixedUpdate for consistent physics
            ProcessMovement();
        }

        #endregion

        #region Ground Check

        /// <summary>
        /// Update ground check status
        /// </summary>
        private void UpdateGroundCheck()
        {
            _wasGroundedLastFrame = _isGrounded;

            // Check if grounded using sphere cast
            Vector3 groundCheckPosition = _groundCheckTransform.position;
            _isGrounded = Physics.CheckSphere(groundCheckPosition, groundCheckRadius, groundLayerMask);

            // Update grounded timer
            if (_isGrounded && !_wasGroundedLastFrame)
            {
                // Just landed
                onLanded?.Raise();
            }
            else if (!_isGrounded && _wasGroundedLastFrame)
            {
                // Just left ground
                _lastGroundedTime = Time.time;
            }

            // Fire ground state change event
            if (_isGrounded != _wasGroundedLastFrame)
            {
                onGroundStateChanged?.Raise(_isGrounded);
            }
        }

        #endregion

        #region Timers

        /// <summary>
        /// Update internal timers
        /// </summary>
        private void UpdateTimers()
        {
            if (_jumpBufferTimer > 0)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Stop all movement immediately
        /// </summary>
        public void StopMovement()
        {
            _inputDirection = Vector3.zero;
            _currentSpeed = 0f;
            _velocity = new Vector3(0, _velocity.y, 0);
        }

        /// <summary>
        /// Add external force to character (e.g., from explosions, wind)
        /// </summary>
        /// <param name="force">Force to apply</param>
        public void AddForce(Vector3 force)
        {
            _velocity += force;
        }

        /// <summary>
        /// Set vertical velocity directly (e.g., for special movements)
        /// </summary>
        /// <param name="verticalVelocity">Vertical velocity to set</param>
        public void SetVerticalVelocity(float verticalVelocity)
        {
            _velocity.y = verticalVelocity;
        }

        /// <summary>
        /// Get movement statistics for debugging
        /// </summary>
        /// <returns>String containing movement stats</returns>
        public string GetMovementStats()
        {
            return $"Speed: {_currentSpeed:F2}, Grounded: {_isGrounded}, Mode: {_currentMovementMode}, Velocity: {_velocity}";
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (_groundCheckTransform != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_groundCheckTransform.position, groundCheckRadius);
            }
        }

        #endregion
    }
}
