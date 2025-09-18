using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Walking state - Normal movement speed with footstep audio
    /// </summary>
    public class WalkingState : BasePlayerState
    {
        private float _footstepTimer;

        public override PlayerState StateType => PlayerState.Walking;

        public override void Enter()
        {
            _footstepTimer = 0f;
        }

public override void Update()
        {
            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Check for state transitions
            if (moveInput.magnitude < 0.1f)
            {
                _controller.StateMachine.ChangeState(PlayerState.Idle);
                return;
            }

            if (IsSprintHeld() && moveInput.magnitude > 0.8f)
            {
                _controller.StateMachine.ChangeState(PlayerState.Running);
                return;
            }

            if (IsCrouchPressed())
            {
                _controller.StateMachine.ChangeState(PlayerState.Crouching);
                return;
            }

            if (IsJumpPressed() && _controller.IsGrounded)
            {
                _controller.StateMachine.ChangeState(PlayerState.Jumping);
                return;
            }

            // Apply movement using data from TPSPlayerData
            Vector3 movement = GetCameraRelativeMovement(moveInput);
            ApplyMovement(movement, _playerData.WalkSpeed);

            // Rotate towards movement direction using data from TPSPlayerData
            RotateTowardsMovement(movement, _playerData.RotationSpeed);

            // Handle look input (camera rotation)
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }
        }

        public override void Exit()
        {
            // Reset footstep timer
            _footstepTimer = 0f;
        }

        /// <summary>
        /// Handle footstep audio timing
        /// </summary>
        private void HandleFootsteps()
        {
            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= _playerData.WalkFootstepInterval)
            {
                // Play footstep sound through audio manager
                // This would typically be handled by the service locator's audio manager
                _footstepTimer = 0f;
            }
        }
    }
}