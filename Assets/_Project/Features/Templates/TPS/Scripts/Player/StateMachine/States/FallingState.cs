using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Falling state - Player is airborne and falling
    /// </summary>
    public class FallingState : BasePlayerState
    {
        public override PlayerState StateType => PlayerState.Falling;

        public override void Enter()
        {
            // No specific entry behavior needed
        }

        public override void Update()
        {
            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Allow limited air control while falling
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                Vector3 currentVelocity = _controller.Velocity;

                // Apply air control (reduced movement speed in air)
                currentVelocity.x = movement.x * _playerData.AirControlSpeed;
                currentVelocity.z = movement.z * _playerData.AirControlSpeed;

                _controller.SetVelocity(currentVelocity);

                // Rotate towards movement direction (reduced rotation in air)
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.3f);
            }

            // Handle look input
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Check for high-speed falling (for potential damage calculation)
            if (_controller.Velocity.y < _playerData.FallDamageThreshold)
            {
                // Prepare for potential fall damage on landing
                // This would be handled when transitioning to landing state
            }
        }

        public override void Exit()
        {
            // No specific exit behavior needed
        }
    }
}
