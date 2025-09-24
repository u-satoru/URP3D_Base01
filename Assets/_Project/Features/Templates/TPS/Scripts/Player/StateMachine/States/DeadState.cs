using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Dead state - Player has died and is no longer controllable
    /// </summary>
    public class DeadState : BasePlayerState
    {
        private float _deathTimer;
        private bool _deathAnimationComplete;
        private bool _respawnRequested;

        public override PlayerState StateType => PlayerState.Dead;

        public override void Enter()
        {
            Debug.Log("[DeadState] Dead state entered");
            _deathTimer = 0f;
            _deathAnimationComplete = false;
            _respawnRequested = false;

            // Disable all movement
            _controller.SetMovementMultiplier(0f);
            _controller.enabled = false;

            // Start death animation
            StartDeathAnimation();

            // Play death sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("player_death");
                audioManager.StopBGM(); // Stop background music on death
            }

            // Notify camera system of death
            var cameraManager = _serviceManager?.GetCameraManager();
            if (cameraManager != null)
            {
                cameraManager.OnPlayerDeath();
            }

            // Disable weapon systems
            DisableWeaponSystems();

            // Show death UI
            ShowDeathUI();

            // Trigger death event for other systems
            TriggerDeathEvent();
        }

        public override void Update()
        {
            _deathTimer += Time.deltaTime;

            // Check for respawn input after death animation completes
            if (_deathAnimationComplete && !_respawnRequested)
            {
                if (GetRespawnInput())
                {
                    RequestRespawn();
                }
            }

            // Update death timer for any time-based respawn mechanics
            UpdateDeathTimer();
        }

        public override void Exit()
        {
            Debug.Log("[DeadState] Dead state exited");
            
            // Re-enable character controller
            _controller.enabled = true;
            _controller.SetMovementMultiplier(1.0f);

            // Stop death animation
            StopDeathAnimation();

            // Re-enable weapon systems
            EnableWeaponSystems();

            // Hide death UI
            HideDeathUI();

            // Restore camera to normal behavior
            var cameraManager = _serviceManager?.GetCameraManager();
            if (cameraManager != null)
            {
                cameraManager.OnPlayerRespawn();
            }

            // Resume background music
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.ResumeBGM();
            }
        }

        private void StartDeathAnimation()
        {
            // TODO: Implement death animation
            // - Play death animation on character
            // - Handle ragdoll physics if applicable
            // - Set appropriate animation callbacks
            Debug.Log("[DeadState] Death animation started");
            
            // Simulate death animation completion after delay
            Invoke(nameof(OnDeathAnimationComplete), 2.0f);
        }

        private void StopDeathAnimation()
        {
            // TODO: Stop death animation
            // - Reset character pose
            // - Disable ragdoll if active
            Debug.Log("[DeadState] Death animation stopped");
            
            CancelInvoke(nameof(OnDeathAnimationComplete));
        }

        private void OnDeathAnimationComplete()
        {
            _deathAnimationComplete = true;
            Debug.Log("[DeadState] Death animation completed");
            
            // TODO: Enable respawn UI elements
            // TODO: Start any death-related timers
        }

        private void DisableWeaponSystems()
        {
            // TODO: Disable all weapon-related systems
            // - Stop any active reloading
            // - Disable weapon rendering
            // - Clear weapon input handling
            Debug.Log("[DeadState] Weapon systems disabled");
        }

        private void EnableWeaponSystems()
        {
            // TODO: Re-enable weapon systems on respawn
            // - Restore weapon state
            // - Re-enable weapon rendering
            // - Restore weapon input handling
            Debug.Log("[DeadState] Weapon systems enabled");
        }

        private void ShowDeathUI()
        {
            // TODO: Show death-related UI
            // - Death screen overlay
            // - Respawn button
            // - Death statistics
            // - Game over screen if applicable
            Debug.Log("[DeadState] Death UI shown");
        }

        private void HideDeathUI()
        {
            // TODO: Hide death-related UI
            Debug.Log("[DeadState] Death UI hidden");
        }

        private void TriggerDeathEvent()
        {
            // TODO: Trigger death event for other systems
            // - Update game statistics
            // - Save death data
            // - Notify achievement system
            // - Trigger any death-related game events
            Debug.Log("[DeadState] Death event triggered");
        }

        private bool GetRespawnInput()
        {
            // Get respawn input from Input Manager through ServiceLocator
            var inputManager = _serviceManager?.GetInputManager();
            if (inputManager != null)
            {
                return inputManager.GetRespawnInputDown();
            }

            // Fallback to Unity Input system
            return Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");
        }

        private void RequestRespawn()
        {
            if (_respawnRequested) return;
            
            _respawnRequested = true;
            Debug.Log("[DeadState] Respawn requested");

            // TODO: Implement respawn logic
            // - Check respawn conditions
            // - Find valid respawn point
            // - Reset player health and status
            // - Trigger respawn sequence

            // Play respawn sound
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("player_respawn");
            }

            // State machine will handle transition to respawn state or idle state
        }

        private void UpdateDeathTimer()
        {
            // TODO: Handle any time-based death mechanics
            // - Auto-respawn after certain time
            // - Decay effects
            // - Time-limited respawn windows

            float autoRespawnTime = _playerData?.AutoRespawnTime ?? 0f;
            if (autoRespawnTime > 0f && _deathTimer >= autoRespawnTime && !_respawnRequested)
            {
                Debug.Log("[DeadState] Auto-respawn triggered");
                RequestRespawn();
            }
        }

        /// <summary>
        /// Force respawn (called externally)
        /// </summary>
        public void ForceRespawn()
        {
            if (!_respawnRequested)
            {
                RequestRespawn();
            }
        }

        /// <summary>
        /// Check if respawn is available
        /// </summary>
        public bool CanRespawn()
        {
            return _deathAnimationComplete && !_respawnRequested;
        }
    }
}