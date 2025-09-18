using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Landing state - Brief state when player touches ground after falling/jumping
    /// </summary>
    public class LandingState : BasePlayerState
    {
        private float _landingTimer;
        private bool _hasPlayedLandingSound;

        public override PlayerState StateType => PlayerState.Landing;

        public override void Enter()
        {
            _landingTimer = 0f;
            _hasPlayedLandingSound = false;

            // Calculate landing impact
            float fallSpeed = Mathf.Abs(_controller.Velocity.y);
            HandleLandingImpact(fallSpeed);

            // Reset vertical velocity
            Vector3 velocity = _controller.Velocity;
            velocity.y = 0f;
            _controller.SetVelocity(velocity);
        }

        public override void Update()
        {
            _landingTimer += Time.deltaTime;

            // Short landing state duration
            if (_landingTimer >= _playerData.LandingDuration)
            {
                // Transition to appropriate state based on input
                Vector2 moveInput = GetMovementInput();

                if (moveInput.magnitude > 0.1f)
                {
                    // Player is trying to move, transition to walking
                    // This will be handled by the state machine's automatic transitions
                }
                else
                {
                    // No movement input, transition to idle
                    // This will be handled by the state machine's automatic transitions
                }
            }

            // Handle look input during landing
            Vector2 lookInput = GetLookInput();
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }
        }

        public override void Exit()
        {
            _landingTimer = 0f;
            _hasPlayedLandingSound = false;
        }

        /// <summary>
        /// Handle landing impact effects and potential damage
        /// </summary>
        private void HandleLandingImpact(float fallSpeed)
        {
            if (!_hasPlayedLandingSound)
            {
                // Play landing sound through audio service
                // Volume and sound type based on fall speed
                _hasPlayedLandingSound = true;
            }

            // Check for fall damage
            if (fallSpeed > _playerData.FallDamageThreshold)
            {
                float damage = CalculateFallDamage(fallSpeed);
                if (damage > 0)
                {
                    _controller.TakeDamage(damage);
                }
            }

            // Apply screen shake or other visual effects based on impact
            // This would integrate with camera and effects systems
        }

        /// <summary>
        /// Calculate fall damage based on fall speed
        /// </summary>
        private float CalculateFallDamage(float fallSpeed)
        {
            if (fallSpeed <= _playerData.FallDamageThreshold)
                return 0f;

            // Calculate damage based on excess fall speed
            float excessSpeed = fallSpeed - _playerData.FallDamageThreshold;
            return excessSpeed * _playerData.FallDamageMultiplier;
        }
    }
}