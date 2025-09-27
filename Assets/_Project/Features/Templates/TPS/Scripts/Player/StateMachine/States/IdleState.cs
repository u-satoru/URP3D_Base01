using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Idle state - Player is standing still with minimal input
    /// </summary>
    public class IdleState : BasePlayerState
    {
        public override PlayerState StateType => PlayerState.Idle;

        public override void Enter()
        {
            // Reset velocity when entering idle
            _controller.SetVelocity(Vector3.zero);
        }

        public override void Update()
        {
            // Handle look input for camera rotation
            Vector2 lookInput = GetLookInput();
            if (lookInput.magnitude > 0.1f)
            {
                // Apply look rotation (this would typically be handled by camera system)
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Maintain slight downward velocity to stay grounded
            Vector3 velocity = _controller.Velocity;
            velocity.x = 0;
            velocity.z = 0;
            _controller.SetVelocity(velocity);
        }

        public override void Exit()
        {
            // No specific exit behavior needed for idle state
        }
    }
}
