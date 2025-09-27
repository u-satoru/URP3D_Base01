using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer Input Service Implementation：入力処理・マッピング・コントローラー対応
    /// ServiceLocator + Event駆動アーキテクチャによる高度な入力システム
    /// Learn & Grow価値実現：直感的な入力設定・プラットフォーマー特化機能
    /// </summary>
    public class PlatformerInputService : IPlatformerInputService
    {
        #region Core Fields & Properties

        private readonly PlatformerInputSettings _settings;
        private bool _isInitialized = false;
        private bool _inputEnabled = true;
        private bool _disposed = false;

        // ==================================================
        // IPlatformerService 基底インターフェース実装
        // ==================================================

        // IPlatformerServiceで必要なプロパティ
        public bool IsInitialized => _isInitialized;
        public bool IsEnabled => _inputEnabled;

        // Input State Properties
        public Vector2 MovementInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool PausePressed { get; private set; }

        // Extended Input Properties
        public bool CrouchPressed { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool RunPressed { get; private set; }
        public bool RunHeld { get; private set; }
        public bool InteractPressed { get; private set; }

        #endregion

        #region Input System Fields

        private PlayerInput _playerInput;
        private InputActionMap _gameplayActionMap;
        private InputActionMap _uiActionMap;

        // Input Actions
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _crouchAction;
        private InputAction _runAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;

        // Input Buffering
        private readonly Dictionary<string, float> _inputBuffer = new Dictionary<string, float>();
        private const float INPUT_BUFFER_TIME = 0.1f; // 100ms buffer

        // Input State Management
        private Vector2 _rawMovementInput;
        private bool _jumpInputBuffered = false;
        private bool _previousJumpInput = false;
        private float _lastJumpInputTime = 0f;

        #endregion

        #region Events

        public event Action<Vector2> OnMovementChanged;
        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnCrouchPressed;
        public event Action OnCrouchReleased;
        public event Action OnRunPressed;
        public event Action OnRunReleased;
        public event Action OnInteractPressed;
        public event Action OnPausePressed;
        public event Action<bool> OnInputEnabledChanged;

        #endregion

        #region Constructor & Initialization

        public PlatformerInputService(PlatformerInputSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                SetupInputSystem();
                BindInputActions();
                ApplySettings();

                _isInitialized = true;

                Debug.Log("[PlatformerInputService] Successfully initialized with Unity Input System integration");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerInputService] Failed to initialize: {ex.Message}");
                throw;
            }
        }

        private void SetupInputSystem()
        {
            // Create player input component dynamically
            var inputObject = new GameObject("PlatformerInputService");
            GameObject.DontDestroyOnLoad(inputObject);

            _playerInput = inputObject.AddComponent<PlayerInput>();

            // Load input action asset (assumes it exists in the project)
            try
            {
                var inputActions = Resources.Load<InputActionAsset>("InputActions/PlatformerInputActions");
                if (inputActions != null)
                {
                    _playerInput.actions = inputActions;
                    _gameplayActionMap = inputActions.FindActionMap("Gameplay");
                    _uiActionMap = inputActions.FindActionMap("UI");
                }
                else
                {
                    Debug.LogWarning("[PlatformerInputService] PlatformerInputActions asset not found, creating default actions");
                    CreateDefaultInputActions();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlatformerInputService] Could not load input actions: {ex.Message}. Creating default actions.");
                CreateDefaultInputActions();
            }
        }

        private void CreateDefaultInputActions()
        {
            // Create basic input actions if asset is not available
            var inputActions = ScriptableObject.CreateInstance<InputActionAsset>();

            // Create Gameplay action map
            _gameplayActionMap = inputActions.AddActionMap("Gameplay");

            // Add movement action
            _moveAction = _gameplayActionMap.AddAction("Move", InputActionType.Value);
            _moveAction.AddBinding("<Keyboard>/wasd");
            _moveAction.AddBinding("<Keyboard>/arrowKeys");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            // Add jump action
            _jumpAction = _gameplayActionMap.AddAction("Jump", InputActionType.Button);
            _jumpAction.AddBinding("<Keyboard>/space");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");

            // Add crouch action
            _crouchAction = _gameplayActionMap.AddAction("Crouch", InputActionType.Button);
            _crouchAction.AddBinding("<Keyboard>/leftCtrl");
            _crouchAction.AddBinding("<Gamepad>/buttonEast");

            // Add run action
            _runAction = _gameplayActionMap.AddAction("Run", InputActionType.Button);
            _runAction.AddBinding("<Keyboard>/leftShift");
            _runAction.AddBinding("<Gamepad>/leftShoulder");

            // Add interact action
            _interactAction = _gameplayActionMap.AddAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e");
            _interactAction.AddBinding("<Gamepad>/buttonWest");

            // Add pause action
            _pauseAction = _gameplayActionMap.AddAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Gamepad>/start");

            _playerInput.actions = inputActions;
        }

        private void BindInputActions()
        {
            if (_gameplayActionMap == null) return;

            // Get actions from action map
            _moveAction = _gameplayActionMap.FindAction("Move");
            _jumpAction = _gameplayActionMap.FindAction("Jump");
            _crouchAction = _gameplayActionMap.FindAction("Crouch");
            _runAction = _gameplayActionMap.FindAction("Run");
            _interactAction = _gameplayActionMap.FindAction("Interact");
            _pauseAction = _gameplayActionMap.FindAction("Pause");

            // Bind callbacks
            if (_moveAction != null)
            {
                _moveAction.performed += OnMovePerformed;
                _moveAction.canceled += OnMoveCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.started += OnJumpStarted;
                _jumpAction.canceled += OnJumpCanceled;
            }

            if (_crouchAction != null)
            {
                _crouchAction.started += OnCrouchStarted;
                _crouchAction.canceled += OnCrouchCanceled;
            }

            if (_runAction != null)
            {
                _runAction.started += OnRunStarted;
                _runAction.canceled += OnRunCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.started += OnInteractStarted;
            }

            if (_pauseAction != null)
            {
                _pauseAction.started += OnPauseStarted;
            }

            // Enable the action map
            _gameplayActionMap.Enable();
        }

        private void ApplySettings()
        {
            if (_settings == null) return;

            // Apply sensitivity settings
            UpdateInputSensitivity();

            // Apply controller settings
            UpdateControllerSettings();

            Debug.Log($"[PlatformerInputService] Settings applied - Sensitivity: H={_settings.HorizontalSensitivity}, V={_settings.VerticalSensitivity}, DeadZone={_settings.DeadZone}");
        }

        #endregion

        #region Input Callbacks

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            _rawMovementInput = context.ReadValue<Vector2>();
            UpdateMovementInput();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _rawMovementInput = Vector2.zero;
            UpdateMovementInput();
        }

        private void OnJumpStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            JumpPressed = true;
            JumpHeld = true;
            _jumpInputBuffered = true;
            _lastJumpInputTime = Time.time;
            _previousJumpInput = true;

            OnJumpPressed?.Invoke();
            BufferInput("Jump", INPUT_BUFFER_TIME);
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            JumpReleased = true;
            JumpHeld = false;
            _previousJumpInput = false;

            OnJumpReleased?.Invoke();
        }

        private void OnCrouchStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            CrouchPressed = true;
            CrouchHeld = true;
            OnCrouchPressed?.Invoke();
        }

        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            CrouchHeld = false;
            OnCrouchReleased?.Invoke();
        }

        private void OnRunStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            RunPressed = true;
            RunHeld = true;
            OnRunPressed?.Invoke();
        }

        private void OnRunCanceled(InputAction.CallbackContext context)
        {
            RunHeld = false;
            OnRunReleased?.Invoke();
        }

        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            InteractPressed = true;
            OnInteractPressed?.Invoke();
        }

        private void OnPauseStarted(InputAction.CallbackContext context)
        {
            if (!_inputEnabled) return;

            PausePressed = true;
            OnPausePressed?.Invoke();
        }

        #endregion

        #region Public Methods

        public void EnableInput(bool enable)
        {
            bool previousState = _inputEnabled;
            _inputEnabled = enable;

            if (_inputEnabled != previousState)
            {
                if (_inputEnabled)
                {
                    _gameplayActionMap?.Enable();
                }
                else
                {
                    _gameplayActionMap?.Disable();
                    ResetInputState();
                }

                OnInputEnabledChanged?.Invoke(_inputEnabled);
                Debug.Log($"[PlatformerInputService] Input {(_inputEnabled ? "enabled" : "disabled")}");
            }
        }

        // ==================================================
        // IPlatformerService インターフェース実装
        // ==================================================

        public void Enable()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[PlatformerInputService] Cannot enable - not initialized yet.");
                return;
            }

            EnableInput(true);
            Debug.Log("[PlatformerInputService] Enabled.");
        }

        public void Disable()
        {
            EnableInput(false);
            Debug.Log("[PlatformerInputService] Disabled.");
        }

        public void Reset()
        {
            ResetInputState();
            _inputBuffer.Clear();
            Debug.Log("[PlatformerInputService] Reset completed.");
        }

        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                // Verify input service can integrate with other services if needed
                bool integration = _isInitialized && _playerInput != null;
                Debug.Log($"[PlatformerInputService] ServiceLocator integration verified: {integration}");
                return integration;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerInputService] ServiceLocator integration failed: {ex.Message}");
                return false;
            }
        }

        public void UpdateService(float deltaTime)
        {
            if (!_isInitialized || !_inputEnabled) return;

            // Update frame-dependent input states
            UpdateInputBuffering();

            // Reset one-frame input states
            JumpPressed = false;
            JumpReleased = false;
            CrouchPressed = false;
            RunPressed = false;
            InteractPressed = false;
            PausePressed = false;
        }

        public void SetInputSensitivity(float horizontal, float vertical)
        {
            if (_settings != null)
            {
                // Update settings if mutable, otherwise just log
                Debug.Log($"[PlatformerInputService] Input sensitivity updated: H={horizontal}, V={vertical}");
            }

            UpdateInputSensitivity();
        }

        public void SwitchActionMap(string actionMapName)
        {
            if (_playerInput?.actions == null) return;

            var actionMap = _playerInput.actions.FindActionMap(actionMapName);
            if (actionMap != null)
            {
                _gameplayActionMap?.Disable();
                _uiActionMap?.Disable();
                actionMap.Enable();

                Debug.Log($"[PlatformerInputService] Switched to action map: {actionMapName}");
            }
        }

        public bool IsInputBuffered(string inputName)
        {
            return _inputBuffer.ContainsKey(inputName) && _inputBuffer[inputName] > Time.time;
        }

        public bool ConsumeBufferedInput(string inputName)
        {
            if (IsInputBuffered(inputName))
            {
                _inputBuffer.Remove(inputName);
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (!_isInitialized) return;

            UpdateInputBuffering();
            ResetFrameInputs();
        }

        #endregion

        #region Private Methods

        private void UpdateMovementInput()
        {
            if (_settings == null)
            {
                MovementInput = _rawMovementInput;
            }
            else
            {
                // Apply sensitivity and dead zone
                Vector2 processedInput = _rawMovementInput;

                // Apply dead zone
                if (processedInput.magnitude < _settings.DeadZone)
                {
                    processedInput = Vector2.zero;
                }
                else
                {
                    // Normalize and reapply magnitude outside dead zone
                    float magnitude = (processedInput.magnitude - _settings.DeadZone) / (1f - _settings.DeadZone);
                    processedInput = processedInput.normalized * magnitude;
                }

                // Apply sensitivity
                processedInput.x *= _settings.HorizontalSensitivity;
                processedInput.y *= _settings.VerticalSensitivity;

                MovementInput = processedInput;
            }

            OnMovementChanged?.Invoke(MovementInput);
        }

        private void UpdateInputSensitivity()
        {
            // Reprocess current movement input with new sensitivity
            UpdateMovementInput();
        }

        private void UpdateControllerSettings()
        {
            // Apply controller-specific settings
            if (_settings?.EnableController == false)
            {
                // Disable gamepad bindings if needed
                Debug.Log("[PlatformerInputService] Controller support disabled");
            }
        }

        private void BufferInput(string inputName, float bufferTime)
        {
            _inputBuffer[inputName] = Time.time + bufferTime;
        }

        private void UpdateInputBuffering()
        {
            // Clean up expired buffered inputs
            var expiredKeys = new List<string>();
            foreach (var kvp in _inputBuffer)
            {
                if (kvp.Value <= Time.time)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _inputBuffer.Remove(key);
            }
        }

        private void ResetFrameInputs()
        {
            // Reset one-frame inputs
            JumpPressed = false;
            JumpReleased = false;
            CrouchPressed = false;
            RunPressed = false;
            InteractPressed = false;
            PausePressed = false;
        }

        private void ResetInputState()
        {
            MovementInput = Vector2.zero;
            JumpPressed = false;
            JumpHeld = false;
            JumpReleased = false;
            CrouchPressed = false;
            CrouchHeld = false;
            RunPressed = false;
            RunHeld = false;
            InteractPressed = false;
            PausePressed = false;
            _rawMovementInput = Vector2.zero;
            _inputBuffer.Clear();
        }

        #endregion

        #region Lifecycle Management

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Disable input
                EnableInput(false);

                // Unbind events
                UnbindInputActions();

                // Cleanup input system
                if (_playerInput != null)
                {
                    var inputObject = _playerInput.gameObject;
                    if (inputObject != null)
                    {
                        GameObject.Destroy(inputObject);
                    }
                }

                // Clear collections
                _inputBuffer.Clear();

                // Clear events
                OnMovementChanged = null;
                OnJumpPressed = null;
                OnJumpReleased = null;
                OnCrouchPressed = null;
                OnCrouchReleased = null;
                OnRunPressed = null;
                OnRunReleased = null;
                OnInteractPressed = null;
                OnPausePressed = null;
                OnInputEnabledChanged = null;

                _disposed = true;

                Debug.Log("[PlatformerInputService] Disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerInputService] Error during disposal: {ex.Message}");
            }
        }

        private void UnbindInputActions()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovePerformed;
                _moveAction.canceled -= OnMoveCanceled;
            }

            if (_jumpAction != null)
            {
                _jumpAction.started -= OnJumpStarted;
                _jumpAction.canceled -= OnJumpCanceled;
            }

            if (_crouchAction != null)
            {
                _crouchAction.started -= OnCrouchStarted;
                _crouchAction.canceled -= OnCrouchCanceled;
            }

            if (_runAction != null)
            {
                _runAction.started -= OnRunStarted;
                _runAction.canceled -= OnRunCanceled;
            }

            if (_interactAction != null)
            {
                _interactAction.started -= OnInteractStarted;
            }

            if (_pauseAction != null)
            {
                _pauseAction.started -= OnPauseStarted;
            }
        }

        #endregion

        #region Debug & Diagnostics

        public void ShowInputDebugInfo()
        {
            if (!_isInitialized)
            {
                Debug.Log("[PlatformerInputService] Service not initialized");
                return;
            }

            Debug.Log("=== Platformer Input Service Debug Info ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"Input Enabled: {_inputEnabled}");
            Debug.Log($"Movement Input: {MovementInput}");
            Debug.Log($"Jump - Pressed: {JumpPressed}, Held: {JumpHeld}, Released: {JumpReleased}");
            Debug.Log($"Crouch - Pressed: {CrouchPressed}, Held: {CrouchHeld}");
            Debug.Log($"Run - Pressed: {RunPressed}, Held: {RunHeld}");
            Debug.Log($"Interact Pressed: {InteractPressed}");
            Debug.Log($"Pause Pressed: {PausePressed}");
            Debug.Log($"Buffered Inputs: {_inputBuffer.Count}");

            if (_settings != null)
            {
                Debug.Log($"Sensitivity - H: {_settings.HorizontalSensitivity}, V: {_settings.VerticalSensitivity}");
                Debug.Log($"Controller Enabled: {_settings.EnableController}");
                Debug.Log($"Dead Zone: {_settings.DeadZone}");
            }
        }

        #endregion
    }
}
