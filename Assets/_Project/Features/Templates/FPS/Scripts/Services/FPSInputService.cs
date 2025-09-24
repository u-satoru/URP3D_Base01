using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.FPS.Services;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS Input Service Implementation：一人称シューティング特化の入力管理実装
    /// ServiceLocator + Event駆動アーキテクチャによる統一入力管理システム
    /// 詳細設計書準拠：武器操作・移動・カメラ・インタラクション入力の統合管理
    /// </summary>
    public class FPSInputService : MonoBehaviour, IFPSInputService
    {
        [Header("Input System Integration")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private bool _enableInputLogging = false;

        [Header("Input Sensitivity Settings")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _controllerSensitivity = 3.0f;
        [SerializeField] private float _inputDeadzone = 0.1f;

        [Header("ServiceLocator Integration")]
        [SerializeField] private bool _enableServiceLocatorIntegration = true;
        [SerializeField] private bool _enableDebugLogging = false;

        // Input State Properties
        public bool IsInitialized { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsMovementEnabled { get; private set; }
        public bool IsCombatEnabled { get; private set; }
        public bool IsCameraEnabled { get; private set; }

        // Current Input Values
        public Vector2 MovementInput { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsFirePressed { get; private set; }
        public bool IsFireHeld { get; private set; }
        public bool IsReloadPressed { get; private set; }
        public bool IsAimPressed { get; private set; }
        public bool IsAimHeld { get; private set; }
        public bool IsWeaponSwitchPressed { get; private set; }
        public Vector2 CameraInput { get; private set; }
        public bool IsCameraResetPressed { get; private set; }
        public bool IsInteractPressed { get; private set; }
        public bool IsPausePressed { get; private set; }
        public bool IsInventoryPressed { get; private set; }

        // Input Device Detection
        public bool IsKeyboardActive { get; private set; }
        public bool IsGamepadActive { get; private set; }

        // Input Actions
        private InputAction _moveAction;
        private InputAction _runAction;
        private InputAction _crouchAction;
        private InputAction _jumpAction;
        private InputAction _fireAction;
        private InputAction _reloadAction;
        private InputAction _aimAction;
        private InputAction _weaponSwitchAction;
        private InputAction _cameraAction;
        private InputAction _cameraResetAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;
        private InputAction _inventoryAction;

        // Input Analytics
        private readonly Queue<float> _inputTimestamps = new Queue<float>();
        private float _lastInputTime;
        private int _inputActionsThisSecond;

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            RegisterWithServiceLocator();
            SetupInputActions();
        }

        private void Update()
        {
            UpdateInputAnalytics();
            DetectActiveInputDevice();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region IFPSService Implementation

        public void Initialize()
        {
            if (IsInitialized) return;

            // Initialize input state
            IsMovementEnabled = true;
            IsCombatEnabled = true;
            IsCameraEnabled = true;

            // Get PlayerInput component
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }

            if (_playerInput == null)
            {
                Debug.LogWarning("[FPSInputService] PlayerInput component not found");
                return;
            }

            IsInitialized = true;
            IsEnabled = true;

            if (_enableDebugLogging)
            {
                Debug.Log("[FPSInputService] Service initialized successfully");
            }
        }

        public void Enable()
        {
            IsEnabled = true;
            EnableAllInputActions();
            OnInputEnabled?.Invoke();
        }

        public void Disable()
        {
            IsEnabled = false;
            DisableAllInputActions();
            OnInputDisabled?.Invoke();
        }

        public void Reset()
        {
            ResetInputValues();
            ClearInputHistory();

            if (_enableDebugLogging)
            {
                Debug.Log("[FPSInputService] Service reset completed");
            }
        }

        public bool VerifyServiceLocatorIntegration()
        {
            if (!_enableServiceLocatorIntegration) return true;

            var registeredService = ServiceLocator.GetService<IFPSInputService>();
            bool isRegistered = registeredService == this;

            if (_enableDebugLogging)
            {
                Debug.Log($"[FPSInputService] ServiceLocator integration verified: {isRegistered}");
            }

            return isRegistered;
        }

        public void UpdateService(float deltaTime)
        {
            // フレーム更新が必要な場合はここで実装
            // 入力の平滑化やデッドゾーン処理など
            ProcessInputSmoothing(deltaTime);
        }

        public void Dispose()
        {
            if (_enableServiceLocatorIntegration)
            {
                try
                {
                    ServiceLocator.UnregisterService<IFPSInputService>();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[FPSInputService] Failed to unregister from ServiceLocator: {ex.Message}");
                }
            }

            // Unsubscribe from input actions
            UnsubscribeFromInputActions();

            IsInitialized = false;
            IsEnabled = false;
        }

        #endregion

        #region Input State Control

        public void EnableAllInput()
        {
            EnableMovementInput();
            EnableCombatInput();
            EnableCameraInput();
        }

        public void DisableAllInput()
        {
            DisableMovementInput();
            DisableCombatInput();
            DisableCameraInput();
        }

        public void EnableMovementInput()
        {
            IsMovementEnabled = true;
            _moveAction?.Enable();
            _runAction?.Enable();
            _crouchAction?.Enable();
            _jumpAction?.Enable();
        }

        public void DisableMovementInput()
        {
            IsMovementEnabled = false;
            _moveAction?.Disable();
            _runAction?.Disable();
            _crouchAction?.Disable();
            _jumpAction?.Disable();
        }

        public void EnableCombatInput()
        {
            IsCombatEnabled = true;
            _fireAction?.Enable();
            _reloadAction?.Enable();
            _aimAction?.Enable();
            _weaponSwitchAction?.Enable();
        }

        public void DisableCombatInput()
        {
            IsCombatEnabled = false;
            _fireAction?.Disable();
            _reloadAction?.Disable();
            _aimAction?.Disable();
            _weaponSwitchAction?.Disable();
        }

        public void EnableCameraInput()
        {
            IsCameraEnabled = true;
            _cameraAction?.Enable();
            _cameraResetAction?.Enable();
        }

        public void DisableCameraInput()
        {
            IsCameraEnabled = false;
            _cameraAction?.Disable();
            _cameraResetAction?.Disable();
        }

        #endregion

        #region Input Sensitivity Management

        public void SetMouseSensitivity(float sensitivity)
        {
            _mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10.0f);
        }

        public void SetControllerSensitivity(float sensitivity)
        {
            _controllerSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10.0f);
        }

        public float GetMouseSensitivity()
        {
            return _mouseSensitivity;
        }

        public float GetControllerSensitivity()
        {
            return _controllerSensitivity;
        }

        #endregion

        #region Input Device Detection

        public string GetActiveInputDevice()
        {
            if (IsGamepadActive) return "Gamepad";
            if (IsKeyboardActive) return "Keyboard";
            return "Unknown";
        }

        private void DetectActiveInputDevice()
        {
            var previousKeyboard = IsKeyboardActive;
            var previousGamepad = IsGamepadActive;

            // Detect based on recent input
            IsKeyboardActive = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            IsGamepadActive = Gamepad.current != null && Gamepad.current.allControls.Any(x => x.IsPressed());

            // Fire device change event
            if ((IsKeyboardActive != previousKeyboard) || (IsGamepadActive != previousGamepad))
            {
                OnInputDeviceChanged?.Invoke(GetActiveInputDevice());
            }
        }

        #endregion

        #region Input Analytics

        public void ClearInputHistory()
        {
            _inputTimestamps.Clear();
            _inputActionsThisSecond = 0;
        }

        public float GetInputResponseTime()
        {
            return Time.unscaledTime - _lastInputTime;
        }

        public int GetInputActionsPerSecond()
        {
            return _inputActionsThisSecond;
        }

        private void UpdateInputAnalytics()
        {
            // Update input actions per second
            while (_inputTimestamps.Count > 0 && Time.unscaledTime - _inputTimestamps.Peek() > 1.0f)
            {
                _inputTimestamps.Dequeue();
            }
            _inputActionsThisSecond = _inputTimestamps.Count;
        }

        private void RecordInputAction()
        {
            _lastInputTime = Time.unscaledTime;
            _inputTimestamps.Enqueue(Time.unscaledTime);
        }

        #endregion

        #region Events

        public event Action OnInputEnabled;
        public event Action OnInputDisabled;
        public event Action<string> OnInputDeviceChanged;
        public event Action<Vector2> OnMovementInput;
        public event Action OnRunStarted;
        public event Action OnRunStopped;
        public event Action OnCrouchStarted;
        public event Action OnCrouchStopped;
        public event Action OnJumpPressed;
        public event Action OnFirePressed;
        public event Action OnFireReleased;
        public event Action OnReloadPressed;
        public event Action OnAimStarted;
        public event Action OnAimStopped;
        public event Action OnWeaponSwitchPressed;
        public event Action<Vector2> OnCameraInput;
        public event Action OnCameraReset;
        public event Action OnInteractPressed;
        public event Action OnPausePressed;
        public event Action OnInventoryPressed;

        #endregion

        #region Private Methods

        private void RegisterWithServiceLocator()
        {
            if (_enableServiceLocatorIntegration)
            {
                ServiceLocator.RegisterService<IFPSInputService>(this);

                if (_enableDebugLogging)
                {
                    Debug.Log("[FPSInputService] Registered with ServiceLocator");
                }
            }
        }

        private void SetupInputActions()
        {
            if (_playerInput == null) return;

            var actionMap = _playerInput.actions;

            // Movement Actions
            _moveAction = actionMap.FindAction("Move");
            _runAction = actionMap.FindAction("Run");
            _crouchAction = actionMap.FindAction("Crouch");
            _jumpAction = actionMap.FindAction("Jump");

            // Combat Actions
            _fireAction = actionMap.FindAction("Fire");
            _reloadAction = actionMap.FindAction("Reload");
            _aimAction = actionMap.FindAction("Aim");
            _weaponSwitchAction = actionMap.FindAction("SwitchWeapon");

            // Camera Actions
            _cameraAction = actionMap.FindAction("Look");
            _cameraResetAction = actionMap.FindAction("CameraReset");

            // Interaction Actions
            _interactAction = actionMap.FindAction("Interact");

            // UI Actions
            _pauseAction = actionMap.FindAction("Pause");
            _inventoryAction = actionMap.FindAction("Inventory");

            SubscribeToInputActions();
        }

        private void SubscribeToInputActions()
        {
            // Movement
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_runAction != null)
            {
                _runAction.started += OnRunStarted_Internal;
                _runAction.canceled += OnRunStopped_Internal;
            }

            if (_crouchAction != null)
            {
                _crouchAction.started += OnCrouchStarted_Internal;
                _crouchAction.canceled += OnCrouchStopped_Internal;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJumpPerformed;
            }

            // Combat
            if (_fireAction != null)
            {
                _fireAction.started += OnFireStarted;
                _fireAction.canceled += OnFireCanceled;
            }

            if (_reloadAction != null)
            {
                _reloadAction.performed += OnReloadPerformed;
            }

            if (_aimAction != null)
            {
                _aimAction.started += OnAimStarted_Internal;
                _aimAction.canceled += OnAimStopped_Internal;
            }

            if (_weaponSwitchAction != null)
            {
                _weaponSwitchAction.performed += OnWeaponSwitchPerformed;
            }

            // Camera
            if (_cameraAction != null)
            {
                _cameraAction.performed += OnCameraPerformed;
                _cameraAction.canceled += OnCameraCanceled;
            }

            if (_cameraResetAction != null)
            {
                _cameraResetAction.performed += OnCameraResetPerformed;
            }

            // Interaction
            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
            }

            // UI
            if (_pauseAction != null)
            {
                _pauseAction.performed += OnPausePerformed;
            }

            if (_inventoryAction != null)
            {
                _inventoryAction.performed += OnInventoryPerformed;
            }
        }

        private void UnsubscribeFromInputActions()
        {
            // Movement
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_runAction != null)
            {
                _runAction.started -= OnRunStarted_Internal;
                _runAction.canceled -= OnRunStopped_Internal;
            }

            if (_crouchAction != null)
            {
                _crouchAction.started -= OnCrouchStarted_Internal;
                _crouchAction.canceled -= OnCrouchStopped_Internal;
            }

            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
            }

            // Combat
            if (_fireAction != null)
            {
                _fireAction.started -= OnFireStarted;
                _fireAction.canceled -= OnFireCanceled;
            }

            if (_reloadAction != null)
            {
                _reloadAction.performed -= OnReloadPerformed;
            }

            if (_aimAction != null)
            {
                _aimAction.started -= OnAimStarted_Internal;
                _aimAction.canceled -= OnAimStopped_Internal;
            }

            if (_weaponSwitchAction != null)
            {
                _weaponSwitchAction.performed -= OnWeaponSwitchPerformed;
            }

            // Camera
            if (_cameraAction != null)
            {
                _cameraAction.performed -= OnCameraPerformed;
                _cameraAction.canceled -= OnCameraCanceled;
            }

            if (_cameraResetAction != null)
            {
                _cameraResetAction.performed -= OnCameraResetPerformed;
            }

            // Interaction
            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
            }

            // UI
            if (_pauseAction != null)
            {
                _pauseAction.performed -= OnPausePerformed;
            }

            if (_inventoryAction != null)
            {
                _inventoryAction.performed -= OnInventoryPerformed;
            }
        }

        private void EnableAllInputActions()
        {
            _moveAction?.Enable();
            _runAction?.Enable();
            _crouchAction?.Enable();
            _jumpAction?.Enable();
            _fireAction?.Enable();
            _reloadAction?.Enable();
            _aimAction?.Enable();
            _weaponSwitchAction?.Enable();
            _cameraAction?.Enable();
            _cameraResetAction?.Enable();
            _interactAction?.Enable();
            _pauseAction?.Enable();
            _inventoryAction?.Enable();
        }

        private void DisableAllInputActions()
        {
            _moveAction?.Disable();
            _runAction?.Disable();
            _crouchAction?.Disable();
            _jumpAction?.Disable();
            _fireAction?.Disable();
            _reloadAction?.Disable();
            _aimAction?.Disable();
            _weaponSwitchAction?.Disable();
            _cameraAction?.Disable();
            _cameraResetAction?.Disable();
            _interactAction?.Disable();
            _pauseAction?.Disable();
            _inventoryAction?.Disable();
        }

        private void ResetInputValues()
        {
            MovementInput = Vector2.zero;
            CameraInput = Vector2.zero;
            IsRunning = false;
            IsCrouching = false;
            IsJumping = false;
            IsFirePressed = false;
            IsFireHeld = false;
            IsReloadPressed = false;
            IsAimPressed = false;
            IsAimHeld = false;
            IsWeaponSwitchPressed = false;
            IsCameraResetPressed = false;
            IsInteractPressed = false;
            IsPausePressed = false;
            IsInventoryPressed = false;
        }

        private void ProcessInputSmoothing(float deltaTime)
        {
            // Apply deadzone to movement input
            if (MovementInput.magnitude < _inputDeadzone)
            {
                MovementInput = Vector2.zero;
            }

            // Apply deadzone to camera input
            if (CameraInput.magnitude < _inputDeadzone)
            {
                CameraInput = Vector2.zero;
            }

            // Apply sensitivity to camera input
            float activeSensitivity = IsGamepadActive ? _controllerSensitivity : _mouseSensitivity;
            CameraInput *= activeSensitivity;
        }

        #endregion

        #region Input Action Callbacks

        // Movement Callbacks
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsMovementEnabled) return;

            MovementInput = context.ReadValue<Vector2>();
            RecordInputAction();
            OnMovementInput?.Invoke(MovementInput);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MovementInput = Vector2.zero;
            OnMovementInput?.Invoke(MovementInput);
        }

        private void OnRunStarted_Internal(InputAction.CallbackContext context)
        {
            if (!IsMovementEnabled) return;

            IsRunning = true;
            RecordInputAction();
            OnRunStarted?.Invoke();
        }

        private void OnRunStopped_Internal(InputAction.CallbackContext context)
        {
            IsRunning = false;
            OnRunStopped?.Invoke();
        }

        private void OnCrouchStarted_Internal(InputAction.CallbackContext context)
        {
            if (!IsMovementEnabled) return;

            IsCrouching = true;
            RecordInputAction();
            OnCrouchStarted?.Invoke();
        }

        private void OnCrouchStopped_Internal(InputAction.CallbackContext context)
        {
            IsCrouching = false;
            OnCrouchStopped?.Invoke();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (!IsMovementEnabled) return;

            IsJumping = true;
            RecordInputAction();
            OnJumpPressed?.Invoke();

            // Reset jump state after a frame
            Invoke(nameof(ResetJumpState), 0.1f);
        }

        private void ResetJumpState()
        {
            IsJumping = false;
        }

        // Combat Callbacks
        private void OnFireStarted(InputAction.CallbackContext context)
        {
            if (!IsCombatEnabled) return;

            IsFirePressed = true;
            IsFireHeld = true;
            RecordInputAction();
            OnFirePressed?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext context)
        {
            IsFirePressed = false;
            IsFireHeld = false;
            OnFireReleased?.Invoke();
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            if (!IsCombatEnabled) return;

            IsReloadPressed = true;
            RecordInputAction();
            OnReloadPressed?.Invoke();

            // Reset reload state after a frame
            Invoke(nameof(ResetReloadState), 0.1f);
        }

        private void ResetReloadState()
        {
            IsReloadPressed = false;
        }

        private void OnAimStarted_Internal(InputAction.CallbackContext context)
        {
            if (!IsCombatEnabled) return;

            IsAimPressed = true;
            IsAimHeld = true;
            RecordInputAction();
            OnAimStarted?.Invoke();
        }

        private void OnAimStopped_Internal(InputAction.CallbackContext context)
        {
            IsAimPressed = false;
            IsAimHeld = false;
            OnAimStopped?.Invoke();
        }

        private void OnWeaponSwitchPerformed(InputAction.CallbackContext context)
        {
            if (!IsCombatEnabled) return;

            IsWeaponSwitchPressed = true;
            RecordInputAction();
            OnWeaponSwitchPressed?.Invoke();

            // Reset weapon switch state after a frame
            Invoke(nameof(ResetWeaponSwitchState), 0.1f);
        }

        private void ResetWeaponSwitchState()
        {
            IsWeaponSwitchPressed = false;
        }

        // Camera Callbacks
        private void OnCameraPerformed(InputAction.CallbackContext context)
        {
            if (!IsCameraEnabled) return;

            CameraInput = context.ReadValue<Vector2>();
            RecordInputAction();
            OnCameraInput?.Invoke(CameraInput);
        }

        private void OnCameraCanceled(InputAction.CallbackContext context)
        {
            CameraInput = Vector2.zero;
            OnCameraInput?.Invoke(CameraInput);
        }

        private void OnCameraResetPerformed(InputAction.CallbackContext context)
        {
            if (!IsCameraEnabled) return;

            IsCameraResetPressed = true;
            RecordInputAction();
            OnCameraReset?.Invoke();

            // Reset camera reset state after a frame
            Invoke(nameof(ResetCameraResetState), 0.1f);
        }

        private void ResetCameraResetState()
        {
            IsCameraResetPressed = false;
        }

        // Interaction Callbacks
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            IsInteractPressed = true;
            RecordInputAction();
            OnInteractPressed?.Invoke();

            // Reset interact state after a frame
            Invoke(nameof(ResetInteractState), 0.1f);
        }

        private void ResetInteractState()
        {
            IsInteractPressed = false;
        }

        // UI Callbacks
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            IsPausePressed = true;
            RecordInputAction();
            OnPausePressed?.Invoke();

            // Reset pause state after a frame
            Invoke(nameof(ResetPauseState), 0.1f);
        }

        private void ResetPauseState()
        {
            IsPausePressed = false;
        }

        private void OnInventoryPerformed(InputAction.CallbackContext context)
        {
            IsInventoryPressed = true;
            RecordInputAction();
            OnInventoryPressed?.Invoke();

            // Reset inventory state after a frame
            Invoke(nameof(ResetInventoryState), 0.1f);
        }

        private void ResetInventoryState()
        {
            IsInventoryPressed = false;
        }

        #endregion
    }
}
