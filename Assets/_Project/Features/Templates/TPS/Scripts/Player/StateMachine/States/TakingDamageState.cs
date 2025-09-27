using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Taking Damage state - Player is reacting to damage taken
    /// </summary>
    public class TakingDamageState : BasePlayerState
    {
        private float _damageReactionTimer;
        private float _damageReactionDuration;
        private float _damageAmount;
        private Vector3 _damageDirection;
        private bool _reactionComplete;

        public override PlayerState StateType => PlayerState.TakingDamage;

        public override void Enter()
        {
            Debug.Log("[TakingDamageState] Taking damage state entered");
            _damageReactionTimer = 0f;
            _reactionComplete = false;
            
            // Get damage reaction duration from player data
            _damageReactionDuration = _playerData?.DamageReactionDuration ?? 0.5f;

            // Reduce movement speed during damage reaction
            _controller.SetMovementMultiplier(0.3f);

            // Start damage reaction animation
            StartDamageReactionAnimation();

            // Play damage sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("player_damage");
            }

            // Apply screen shake through Camera Manager
            var cameraManager = _serviceManager?.GetCameraManager();
            if (cameraManager != null)
            {
                cameraManager.ApplyScreenShake(_damageAmount * 0.1f);
            }

            // Apply knockback effect
            ApplyKnockback();
        }

        public override void Update()
        {
            _damageReactionTimer += Time.deltaTime;

            // Allow limited movement during damage reaction
            Vector2 moveInput = GetMovementInput();
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                // Very slow movement while reacting to damage
                ApplyMovement(movement, _playerData.WalkSpeed * 0.2f);
                
                // Slow rotation during damage reaction
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.2f);
            }

            // Limited look input during damage reaction
            Vector2 lookInput = GetLookInput();
            if (lookInput.magnitude > 0.1f)
            {
                // Reduced sensitivity during damage reaction
                float yRotation = lookInput.x * _playerData.LookSensitivity * 0.3f * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Check if damage reaction is complete
            if (_damageReactionTimer >= _damageReactionDuration && !_reactionComplete)
            {
                CompleteDamageReaction();
            }
        }

        public override void Exit()
        {
            Debug.Log("[TakingDamageState] Taking damage state exited");
            
            // Restore normal movement speed
            _controller.SetMovementMultiplier(1.0f);

            // Stop damage reaction animation if still playing
            StopDamageReactionAnimation();
        }

        /// <summary>
        /// Set damage information for the reaction
        /// </summary>
        public void SetDamageInfo(float damageAmount, Vector3 damageDirection)
        {
            _damageAmount = damageAmount;
            _damageDirection = damageDirection.normalized;
        }

        private void StartDamageReactionAnimation()
        {
            // TODO: Implement damage reaction animation
            // - Play damage reaction animation on character
            // - Direction-based animation (front, back, left, right hit)
            // - Scale animation intensity based on damage amount
            Debug.Log("[TakingDamageState] Damage reaction animation started");
        }

        private void StopDamageReactionAnimation()
        {
            // TODO: Stop damage reaction animation
            Debug.Log("[TakingDamageState] Damage reaction animation stopped");
        }

        private void ApplyKnockback()
        {
            if (_damageDirection != Vector3.zero && _playerData != null)
            {
                // Calculate knockback force based on damage amount
                float knockbackForce = Mathf.Clamp(_damageAmount * 0.1f, 0f, _playerData.MaxKnockbackForce);
                Vector3 knockbackVector = _damageDirection * knockbackForce;

                // Apply knockback to character controller
                _controller.AddForce(knockbackVector);

                Debug.Log($"[TakingDamageState] Applied knockback: {knockbackVector}");
            }
        }

        private void CompleteDamageReaction()
        {
            _reactionComplete = true;
            Debug.Log("[TakingDamageState] Damage reaction completed");

            // TODO: Implement damage reaction completion
            // - Check if player is still alive
            // - Apply any lingering effects
            // - Update UI if needed

            // State machine will handle transition back to appropriate state
        }

        /// <summary>
        /// Check if the damage reaction can be interrupted by input
        /// </summary>
        private bool CanInterruptReaction()
        {
            // Allow interruption after minimum reaction time
            float minReactionTime = _damageReactionDuration * 0.5f;
            return _damageReactionTimer >= minReactionTime;
        }

        /// <summary>
        /// Apply damage reduction effects based on player data
        /// </summary>
        private float ApplyDamageReduction(float rawDamage)
        {
            if (_playerData != null)
            {
                float damageReduction = _playerData.DamageReduction;
                return rawDamage * (1f - damageReduction);
            }
            return rawDamage;
        }
    }
}
