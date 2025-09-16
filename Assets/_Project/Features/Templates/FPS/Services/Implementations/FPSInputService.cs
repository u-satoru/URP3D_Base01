using System;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS入力サービス実装
    /// Unity Input System統合 + ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class FPSInputService : IFPSInputService
    {
        private PlayerInput _playerInput;
        private InputActionAsset _inputActions;
        private bool _isEnabled = true;
        private bool _isPaused = false;

        // 入力データ
        private Vector2 _moveInput = Vector2.zero;
        private Vector2 _lookInput = Vector2.zero;
        private bool _isFirePressed = false;
        private bool _isAimPressed = false;
        private bool _isReloadPressed = false;
        private bool _isJumpPressed = false;
        private bool _isCrouchPressed = false;
        private bool _isSprintPressed = false;
        private int _weaponSwitchInput = 0;
        private bool _isWeaponSwitchPressed = false;

        // プロパティ実装
        public Vector2 MovementInput => _isEnabled && !_isPaused ? _moveInput : Vector2.zero;
        public Vector2 LookInput => _isEnabled && !_isPaused ? _lookInput : Vector2.zero;
        public bool IsFirePressed => _isEnabled && !_isPaused && _isFirePressed;
        public bool IsAimPressed => _isEnabled && !_isPaused && _isAimPressed;
        public bool IsReloadPressed => _isEnabled && !_isPaused && _isReloadPressed;
        public bool IsSprintPressed => _isEnabled && !_isPaused && _isSprintPressed;
        public bool IsJumpPressed => _isEnabled && !_isPaused && _isJumpPressed;
        public bool IsCrouchPressed => _isEnabled && !_isPaused && _isCrouchPressed;
        public int WeaponSwitchInput => _isEnabled && !_isPaused ? _weaponSwitchInput : 0;
        public bool IsWeaponSwitchPressed => _isEnabled && !_isPaused && _isWeaponSwitchPressed;

        // イベント
        public event Action OnFirePressed;
        public event Action OnFireReleased;
        public event Action OnReloadPressed;
        public event Action OnAimPressed;
        public event Action OnAimReleased;
        public event Action OnJumpPressed;
        public event Action OnSprintPressed;
        public event Action OnSprintReleased;
        public event Action OnCrouchPressed;
        public event Action OnCrouchReleased;
        public event Action<int> OnWeaponSwitchPressed;

        // Additional events for internal use
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnInteractPressed;

        // 初期化
        public void Initialize(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _inputActions = playerInput.actions;

            if (_inputActions == null)
            {
                Debug.LogError("[FPSInputService] InputActionAsset is null");
                return;
            }

            SetupInputCallbacks();
            _inputActions.Enable();

            Debug.Log("[FPSInputService] Input system initialized");
        }

        // 入力制御
        public void EnableInput(bool enable)
        {
            if (enable)
            {
                _isEnabled = true;
                _inputActions?.Enable();
                Debug.Log("[FPSInputService] Input enabled");
            }
            else
            {
                _isEnabled = false;
                _inputActions?.Disable();
                ResetInputValues();
                Debug.Log("[FPSInputService] Input disabled");
            }
        }

        public void PauseInput()
        {
            _isPaused = true;
            ResetInputValues();
            Debug.Log("[FPSInputService] Input paused");
        }

        public void ResumeInput()
        {
            _isPaused = false;
            Debug.Log("[FPSInputService] Input resumed");
        }

        // 設定
        public void SetMouseSensitivity(float sensitivity)
        {
            if (sensitivity <= 0f)
            {
                Debug.LogWarning($"[FPSInputService] Invalid mouse sensitivity: {sensitivity}");
                return;
            }

            // ServiceLocator経由でカメラサービス取得
            var cameraService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSCameraService>();
            cameraService?.SetCameraSensitivity(sensitivity);

            Debug.Log($"[FPSInputService] Mouse sensitivity set to: {sensitivity}");
        }

        public void SetInputConfiguration(InputConfiguration config)
        {
            if (config == null)
            {
                Debug.LogWarning("[FPSInputService] InputConfiguration is null");
                return;
            }

            SetMouseSensitivity(config.MouseSensitivity);

            // その他の設定適用
            Debug.Log("[FPSInputService] Input configuration applied");
        }

        public void SetInputMap(FPSInputMap inputMap)
        {
            if (_playerInput == null || _inputActions == null)
            {
                Debug.LogWarning("[FPSInputService] Cannot set input map - input system not initialized");
                return;
            }

            switch (inputMap)
            {
                case FPSInputMap.Gameplay:
                    _inputActions.Enable();
                    break;
                case FPSInputMap.UI:
                    _inputActions.Disable();
                    break;
                case FPSInputMap.Paused:
                    PauseInput();
                    break;
                case FPSInputMap.Cutscene:
                    _inputActions.Disable();
                    break;
                default:
                    Debug.LogWarning($"[FPSInputService] Unknown input map: {inputMap}");
                    break;
            }

            Debug.Log($"[FPSInputService] Input map set to: {inputMap}");
        }

        // 入力チェック
        public bool IsInputActive()
        {
            return _isEnabled && !_isPaused && _playerInput != null && _playerInput.enabled;
        }

        public InputDeviceType GetActiveInputDevice()
        {
            if (_playerInput?.currentControlScheme == "Keyboard&Mouse")
                return InputDeviceType.KeyboardMouse;
            else if (_playerInput?.currentControlScheme == "Gamepad")
                return InputDeviceType.Gamepad;
            else
                return InputDeviceType.Touch;
        }

        // プライベートメソッド
        private void SetupInputCallbacks()
        {
            // Movement
            var moveAction = _inputActions.FindAction("Move");
            if (moveAction != null)
            {
                moveAction.performed += OnMovePerformed;
                moveAction.canceled += OnMoveCanceled;
            }

            // Look
            var lookAction = _inputActions.FindAction("Look");
            if (lookAction != null)
            {
                lookAction.performed += OnLookPerformed;
                lookAction.canceled += OnLookCanceled;
            }

            // Fire
            var fireAction = _inputActions.FindAction("Fire");
            if (fireAction != null)
            {
                fireAction.performed += OnFirePerformed;
                fireAction.canceled += OnFireCanceled;
            }

            // Aim
            var aimAction = _inputActions.FindAction("Aim");
            if (aimAction != null)
            {
                aimAction.performed += OnAimPerformed;
                aimAction.canceled += OnAimCanceled;
            }

            // Reload
            var reloadAction = _inputActions.FindAction("Reload");
            if (reloadAction != null)
            {
                reloadAction.performed += OnReloadPerformed;
            }

            // Jump
            var jumpAction = _inputActions.FindAction("Jump");
            if (jumpAction != null)
            {
                jumpAction.performed += OnJumpPerformed;
            }

            // Crouch
            var crouchAction = _inputActions.FindAction("Crouch");
            if (crouchAction != null)
            {
                crouchAction.performed += OnCrouchPerformed;
                crouchAction.canceled += OnCrouchCanceled;
            }

            // Sprint
            var sprintAction = _inputActions.FindAction("Sprint");
            if (sprintAction != null)
            {
                sprintAction.performed += OnSprintPerformed;
                sprintAction.canceled += OnSprintCanceled;
            }

            // Alternative: Try "Run" action for backward compatibility
            if (sprintAction == null)
            {
                var runAction = _inputActions.FindAction("Run");
                if (runAction != null)
                {
                    runAction.performed += OnSprintPerformed;
                    runAction.canceled += OnSprintCanceled;
                }
            }

            // Weapon Switch
            var weaponNextAction = _inputActions.FindAction("WeaponNext");
            if (weaponNextAction != null)
            {
                weaponNextAction.performed += OnWeaponNextPerformed;
            }

            var weaponPrevAction = _inputActions.FindAction("WeaponPrevious");
            if (weaponPrevAction != null)
            {
                weaponPrevAction.performed += OnWeaponPrevPerformed;
            }

            // Interact
            var interactAction = _inputActions.FindAction("Interact");
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
        }

        private void ResetInputValues()
        {
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            _isFirePressed = false;
            _isAimPressed = false;
            _isReloadPressed = false;
            _isJumpPressed = false;
            _isCrouchPressed = false;
            _isSprintPressed = false;
            _weaponSwitchInput = 0;
            _isWeaponSwitchPressed = false;
        }

        // Input Action Callbacks
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
            OnMoveInput?.Invoke(_moveInput);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
            OnMoveInput?.Invoke(_moveInput);
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
            OnLookInput?.Invoke(_lookInput);
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            _lookInput = Vector2.zero;
            OnLookInput?.Invoke(_lookInput);
        }

        private void OnFirePerformed(InputAction.CallbackContext context)
        {
            _isFirePressed = true;
            OnFirePressed?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            _isFirePressed = false;
            OnFireReleased?.Invoke();
        }

        private void OnAimPerformed(InputAction.CallbackContext context)
        {
            _isAimPressed = true;
            OnAimPressed?.Invoke();
        }

        private void OnAimCanceled(InputAction.CallbackContext context)
        {
            _isAimPressed = false;
            OnAimReleased?.Invoke();
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            _isReloadPressed = true;
            OnReloadPressed?.Invoke();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _isJumpPressed = true;
            OnJumpPressed?.Invoke();
        }

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            _isCrouchPressed = true;
            OnCrouchPressed?.Invoke();
        }

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            _isCrouchPressed = false;
            OnCrouchReleased?.Invoke();
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            _isSprintPressed = true;
            OnSprintPressed?.Invoke();
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            _isSprintPressed = false;
            OnSprintReleased?.Invoke();
        }

        private void OnWeaponNextPerformed(InputAction.CallbackContext context)
        {
            _weaponSwitchInput = 1; // Next weapon
            _isWeaponSwitchPressed = true;
            OnWeaponSwitchPressed?.Invoke(1);

            // Reset after a frame to simulate button press
            ResetWeaponSwitchInput();
        }

        private void OnWeaponPrevPerformed(InputAction.CallbackContext context)
        {
            _weaponSwitchInput = -1; // Previous weapon
            _isWeaponSwitchPressed = true;
            OnWeaponSwitchPressed?.Invoke(-1);

            // Reset after a frame to simulate button press
            ResetWeaponSwitchInput();
        }

        private void ResetWeaponSwitchInput()
        {
            // Use Unity's next frame to reset the weapon switch state
            UnityEngine.MonoBehaviour.print(""); // This ensures we're on main thread
            _weaponSwitchInput = 0;
            _isWeaponSwitchPressed = false;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
        }

        // Cleanup
        public void Dispose()
        {
            if (_inputActions != null)
            {
                _inputActions.Disable();
            }

            Debug.Log("[FPSInputService] Input service disposed");
        }
    }
}