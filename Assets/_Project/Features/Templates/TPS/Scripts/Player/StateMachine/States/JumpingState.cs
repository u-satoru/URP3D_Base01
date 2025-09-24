using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Jumping state - Initial jump with upward velocity
    /// </summary>
    public class JumpingState : BasePlayerState
    {
        private bool _hasAppliedJumpForce;

        public override PlayerState StateType => PlayerState.Jumping;

public override void Enter()
        {
            Debug.Log("[JumpingState] Jumping state entered");
            
            // Apply jump force from TPSPlayerData
            Vector3 currentVelocity = _controller.Velocity;
            currentVelocity.y = _playerData.JumpForce;
            _controller.SetVelocity(currentVelocity);
            
            // Play jump sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("player_jump");
            }
            
            // Start jump animation
            StartJumpAnimation();
        }

public override void Update()
        {
            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Allow air movement with reduced control
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                // Apply reduced air control movement using TPSPlayerData
                ApplyMovement(movement, _playerData.WalkSpeed * 0.3f); // 30% air control
                
                // Rotate towards movement direction with reduced control
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.5f);
            }

            // Handle look input (camera rotation)
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Check if we start falling (velocity becomes negative)
            if (_controller.Velocity.y <= 0)
            {
                _controller.StateMachine.ChangeState(PlayerState.Falling);
            }
        }

        public override void Exit()
        {
            _hasAppliedJumpForce = false;
        }

        /// <summary>
        /// Apply initial jump force
        /// </summary>
        private void ApplyJumpForce()
        {
            if (!_hasAppliedJumpForce)
            {
                Vector3 velocity = _controller.Velocity;
                velocity.y = _playerData.JumpForce;
                _controller.SetVelocity(velocity);

                _hasAppliedJumpForce = true;

                // Play jump sound through audio service
                // This would be handled by ServiceLocator's audio manager
            }
        }
    }
}