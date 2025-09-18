using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Running state - High speed movement with stamina consumption
    /// </summary>
    public class RunningState : BasePlayerState
    {
        private float _footstepTimer;
        private float _staminaTimer;

        public override PlayerState StateType => PlayerState.Running;

        public override void Enter()
        {
            _footstepTimer = 0f;
            _staminaTimer = 0f;
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

            // Check if sprint is released or movement is slow
            if (!IsSprintHeld() || moveInput.magnitude < 0.8f)
            {
                _controller.StateMachine.ChangeState(PlayerState.Walking);
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
            ApplyMovement(movement, _playerData.RunSpeed);

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
            // Reset timers
            _footstepTimer = 0f;
            _staminaTimer = 0f;
        }

        /// <summary>
        /// Handle footstep audio timing for running
        /// </summary>
        private void HandleFootsteps()
        {
            _footstepTimer += Time.deltaTime;

            if (_footstepTimer >= _playerData.RunFootstepInterval)
            {
                // Play running footstep sound (louder and faster than walking)
                // This would be handled by the audio service through ServiceLocator
                _footstepTimer = 0f;
            }
        }

        /// <summary>
        /// Handle stamina consumption while running
        /// </summary>
        private void HandleStamina()
        {
            _staminaTimer += Time.deltaTime;

            if (_staminaTimer >= 0.1f) // Update stamina every 0.1 seconds
            {
                // Consume stamina (this would integrate with a stamina system)
                // For now, we'll assume unlimited stamina but this is where
                // integration with player stats would occur
                _staminaTimer = 0f;

                // Example: _controller.ConsumeStamina(_playerData.RunStaminaCost);
            }
        }
    }
}