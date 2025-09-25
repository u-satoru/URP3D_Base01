using UnityEngine;
using asterivo.Unity60.Features.Templates.TPS.Player;
using asterivo.Unity60.Features.Templates.TPS.Data;
using asterivo.Unity60.Features.Templates.TPS.Services;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine
{
    /// <summary>
    /// Base class for all TPS player states
    /// Provides common functionality and references
    /// </summary>
    public abstract class BasePlayerState : IPlayerState
    {
        protected TPSPlayerController _controller;
        protected TPSPlayerData _playerData;
        protected TPSServiceManager _serviceManager;

        public abstract PlayerState StateType { get; }

        /// <summary>
        /// Initialize the state with player controller reference
        /// </summary>
        public virtual void Initialize(TPSPlayerController controller)
        {
            _controller = controller;
            _playerData = controller.PlayerData;

            // Get service manager from controller
            _serviceManager = Object.FindObjectOfType<TPSServiceManager>();
            if (_serviceManager == null)
            {
                Debug.LogError("[BasePlayerState] TPSServiceManager not found in scene!");
            }
        }

        /// <summary>
        /// Called when entering this state
        /// </summary>
        public virtual void Enter()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        public virtual void Update()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public virtual void Exit()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Get movement input from controller
        /// </summary>
        protected Vector2 GetMovementInput()
        {
            return _controller.GetMovementInput();
        }

        /// <summary>
        /// Get look input from controller
        /// </summary>
        protected Vector2 GetLookInput()
        {
            return _controller.GetLookInput();
        }

        /// <summary>
        /// Check if jump input is pressed
        /// </summary>
        protected bool IsJumpPressed()
        {
            return _controller.IsJumpPressed();
        }

        /// <summary>
        /// Check if sprint input is held
        /// </summary>
        protected bool IsSprintHeld()
        {
            return _controller.IsSprintHeld();
        }

        /// <summary>
        /// Check if crouch input is pressed
        /// </summary>
        protected bool IsCrouchPressed()
        {
            return _controller.IsCrouchPressed();
        }

        /// <summary>
        /// Apply movement to character controller
        /// </summary>
        protected void ApplyMovement(Vector3 direction, float speed)
        {
            Vector3 movement = direction.normalized * speed;
            movement.y = _controller.Velocity.y; // Preserve vertical velocity
            _controller.SetVelocity(movement);
        }

        /// <summary>
        /// Get camera-relative movement direction
        /// </summary>
        protected Vector3 GetCameraRelativeMovement(Vector2 input)
        {
            if (UnityEngine.Camera.main == null) return Vector3.zero;

            Vector3 forward = UnityEngine.Camera.main.transform.forward;
            Vector3 right = UnityEngine.Camera.main.transform.right;

            // Remove Y component to keep movement horizontal
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        /// <summary>
        /// Rotate player to face movement direction
        /// </summary>
        protected void RotateTowardsMovement(Vector3 direction, float rotationSpeed)
        {
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _controller.transform.rotation = Quaternion.Slerp(
                    _controller.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// Invoke a method with delay (delegates to controller MonoBehaviour)
        /// </summary>
        protected void Invoke(string methodName, float time)
        {
            if (_controller != null)
            {
                _controller.Invoke(methodName, time);
            }
        }

        /// <summary>
        /// Cancel a previously invoked method (delegates to controller MonoBehaviour)
        /// </summary>
        protected void CancelInvoke(string methodName)
        {
            if (_controller != null)
            {
                _controller.CancelInvoke(methodName);
            }
        }

        /// <summary>
        /// Cancel all invoked methods (delegates to controller MonoBehaviour)
        /// </summary>
        protected void CancelInvoke()
        {
            if (_controller != null)
            {
                _controller.CancelInvoke();
            }
        }

        /// <summary>
        /// Helper method to start jump animation
        /// </summary>
        protected void StartJumpAnimation()
        {
            // TODO: Implement jump animation logic
            // This can be expanded to handle Animator triggers or other animation systems
            Debug.Log("[BasePlayerState] Jump animation started");
        }

        /// <summary>
        /// Helper method to get jump input (compatibility method)
        /// </summary>
        protected bool GetJumpInput()
        {
            return IsJumpPressed();
        }
    }
}
