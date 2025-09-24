using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Reloading state - Player is reloading weapon
    /// </summary>
    public class ReloadingState : BasePlayerState
    {
        private float _reloadTimer;
        private float _reloadDuration;
        private bool _reloadComplete;

        public override PlayerState StateType => PlayerState.Reloading;

        public override void Enter()
        {
            Debug.Log("[ReloadingState] Reloading state entered");
            _reloadTimer = 0f;
            _reloadComplete = false;
            
            // Get reload duration from player data
            _reloadDuration = _playerData?.ReloadTime ?? 2.0f;

            // Disable shooting during reload
            // Reduce movement speed during reload
            _controller.SetMovementMultiplier(0.7f);

            // Play reload animation
            StartReloadAnimation();

            // Play reload sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("weapon_reload");
            }
        }

        public override void Update()
        {
            _reloadTimer += Time.deltaTime;

            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Allow limited movement during reload
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                // Slower movement while reloading
                ApplyMovement(movement, _playerData.WalkSpeed * 0.5f);
                
                // Rotate towards movement direction (slower while reloading)
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.3f);
            }

            // Handle look input (limited while reloading)
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * 0.5f * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Check if reload is complete
            if (_reloadTimer >= _reloadDuration && !_reloadComplete)
            {
                CompleteReload();
            }

            // Check for reload cancellation (movement interruption)
            if (GetCancelReloadInput())
            {
                CancelReload();
            }
        }

        public override void Exit()
        {
            Debug.Log("[ReloadingState] Reloading state exited");
            
            // Restore normal movement speed
            _controller.SetMovementMultiplier(1.0f);

            // Stop reload animation if still playing
            StopReloadAnimation();
        }

        private void StartReloadAnimation()
        {
            // TODO: Implement reload animation
            // - Play reload animation on weapon
            // - Handle animation events
            // - Sync with reload duration
            Debug.Log("[ReloadingState] Reload animation started");
        }

        private void StopReloadAnimation()
        {
            // TODO: Stop reload animation
            Debug.Log("[ReloadingState] Reload animation stopped");
        }

        private void CompleteReload()
        {
            _reloadComplete = true;
            Debug.Log("[ReloadingState] Reload completed");

            // TODO: Implement reload completion
            // - Refill ammo
            // - Update UI
            // - Play completion sound
            // - Notify weapon system

            // Play reload complete sound
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("weapon_reload_complete");
            }

            // State machine will handle transition back to appropriate state
        }

        private void CancelReload()
        {
            Debug.Log("[ReloadingState] Reload cancelled");
            
            // TODO: Implement reload cancellation
            // - Stop reload animation
            // - Don't refill ammo
            // - Play cancellation sound

            // State machine will handle transition to appropriate state
        }

        private bool GetCancelReloadInput()
        {
            // Check for inputs that should cancel reload
            // Such as sprint, jump, or combat actions
            Vector2 moveInput = GetMovementInput();
            
            // Cancel if trying to sprint (high movement input)
            if (moveInput.magnitude > 0.8f)
            {
                return true;
            }

            // Cancel if trying to jump
            if (GetJumpInput())
            {
                return true;
            }

            // Cancel if trying to melee attack
            if (GetMeleeInput())
            {
                return true;
            }

            return false;
        }

        private bool GetMeleeInput()
        {
            // Get melee input from Input Manager through ServiceLocator
            var inputManager = _serviceManager?.GetInputManager();
            if (inputManager != null)
            {
                return inputManager.GetMeleeInputDown();
            }

            // Fallback to Unity Input system
            return Input.GetButtonDown("Fire2");
        }
    }
}
