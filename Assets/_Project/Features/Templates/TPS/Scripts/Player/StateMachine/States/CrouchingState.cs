using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Crouching state - Reduced movement speed, lower profile, quieter movement
    /// </summary>
    public class CrouchingState : BasePlayerState
    {
        private float _originalControllerHeight;
        private Vector3 _originalControllerCenter;
        private float _footstepTimer;

        public override PlayerState StateType => PlayerState.Crouching;

        public override void Enter()
        {
            // Store original controller dimensions
            _originalControllerHeight = _controller.CharacterController.height;
            _originalControllerCenter = _controller.CharacterController.center;

            // Adjust character controller for crouching
            _controller.CharacterController.height = _playerData.CrouchHeight;
            _controller.CharacterController.center = new Vector3(
                _originalControllerCenter.x,
                _playerData.CrouchHeight / 2f,
                _originalControllerCenter.z
            );

            _footstepTimer = 0f;
        }

public override void Update()
        {
            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Check if crouch is released
            if (!IsCrouchPressed())
            {
                if (moveInput.magnitude > 0.1f)
                {
                    _controller.StateMachine.ChangeState(PlayerState.Walking);
                }
                else
                {
                    _controller.StateMachine.ChangeState(PlayerState.Idle);
                }
                return;
            }

            if (IsJumpPressed() && _controller.IsGrounded)
            {
                _controller.StateMachine.ChangeState(PlayerState.Jumping);
                return;
            }

            // Apply movement using crouch speed from TPSPlayerData
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                ApplyMovement(movement, _playerData.CrouchSpeed);
                
                // Rotate towards movement direction using data from TPSPlayerData
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.5f); // Slower rotation while crouching
            }

            // Handle look input (camera rotation)
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * 0.8f * Time.deltaTime; // Slower camera while crouching
                _controller.transform.Rotate(0, yRotation, 0);
            }
        }

        public override void Exit()
        {
            // Restore original character controller dimensions
            _controller.CharacterController.height = _originalControllerHeight;
            _controller.CharacterController.center = _originalControllerCenter;

            _footstepTimer = 0f;
        }

        /// <summary>
        /// Handle quiet footstep audio for stealth gameplay
        /// </summary>
        private void HandleQuietFootsteps()
        {
            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= _playerData.CrouchFootstepInterval)
            {
                // Play quiet footstep sound with reduced volume
                // This integrates with the stealth audio system
                _footstepTimer = 0f;
            }
        }
    }
}