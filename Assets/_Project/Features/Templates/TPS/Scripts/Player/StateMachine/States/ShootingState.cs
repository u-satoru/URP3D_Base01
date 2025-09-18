using UnityEngine;

namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine.States
{
    /// <summary>
    /// Shooting state - Player is actively firing weapons
    /// </summary>
    public class ShootingState : BasePlayerState
    {
        private float _shootTimer;
        private bool _isShooting;

        public override PlayerState StateType => PlayerState.Shooting;

        public override void Enter()
        {
            Debug.Log("[ShootingState] Shooting state entered");
            _shootTimer = 0f;
            _isShooting = false;

            // Enable shooting stance
            if (_playerData != null)
            {
                // Reduce movement speed while shooting
                _controller.SetMovementMultiplier(0.5f);
            }
        }

        public override void Update()
        {
            Vector2 moveInput = GetMovementInput();
            Vector2 lookInput = GetLookInput();

            // Handle shooting input
            bool shootInput = GetShootInput();
            if (shootInput && CanShoot())
            {
                PerformShoot();
            }

            // Allow limited movement while shooting
            if (moveInput.magnitude > 0.1f)
            {
                Vector3 movement = GetCameraRelativeMovement(moveInput);
                // Slower movement while shooting
                ApplyMovement(movement, _playerData.WalkSpeed * 0.3f);
                
                // Rotate towards movement direction (slower while shooting)
                RotateTowardsMovement(movement, _playerData.RotationSpeed * 0.5f);
            }

            // Handle look input for aiming
            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _playerData.LookSensitivity * Time.deltaTime;
                _controller.transform.Rotate(0, yRotation, 0);
            }

            // Update shooting timer
            _shootTimer += Time.deltaTime;

            // Check for reload input
            if (GetReloadInput())
            {
                // State machine will handle transition to reloading state
                Debug.Log("[ShootingState] Reload requested");
            }
        }

        public override void Exit()
        {
            Debug.Log("[ShootingState] Shooting state exited");
            _isShooting = false;
            
            // Restore normal movement speed
            _controller.SetMovementMultiplier(1.0f);
        }

        private bool GetShootInput()
        {
            // Get shoot input from Input Manager through ServiceLocator
            var inputManager = _serviceManager?.GetInputManager();
            if (inputManager != null)
            {
                return inputManager.GetFireInput();
            }

            // Fallback to Unity Input system
            return Input.GetButton("Fire1");
        }

        private bool GetReloadInput()
        {
            // Get reload input from Input Manager through ServiceLocator
            var inputManager = _serviceManager?.GetInputManager();
            if (inputManager != null)
            {
                return inputManager.GetReloadInputDown();
            }

            // Fallback to Unity Input system
            return Input.GetKeyDown(KeyCode.R);
        }

        private bool CanShoot()
        {
            if (_playerData == null) return false;
            
            // Check fire rate
            return _shootTimer >= (1f / _playerData.FireRate);
        }

        private void PerformShoot()
        {
            _shootTimer = 0f;
            _isShooting = true;

            // TODO: Implement actual shooting mechanics
            // - Raycast or projectile spawning
            // - Muzzle flash effects
            // - Sound effects through AudioManager
            // - Recoil effects
            // - Ammo consumption

            Debug.Log("[ShootingState] Shot fired");

            // Play shooting sound through ServiceLocator
            var audioManager = _serviceManager?.GetAudioManager();
            if (audioManager != null)
            {
                audioManager.PlaySFX("weapon_fire");
            }

            // Apply recoil to camera through Camera Manager
            var cameraManager = _serviceManager?.GetCameraManager();
            if (cameraManager != null)
            {
                cameraManager.ApplyRecoil(_playerData.RecoilAmount);
            }
        }
    }
}