using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Core Input Service that bridges PlayerInput and GameEvent system
    /// Converts PlayerInput callbacks to GameEvent raises for decoupled communication
    /// </summary>
    public class InputService : MonoBehaviour, IInputManager
    {
        [Header("Event Channels Configuration")]
        [SerializeField] private InputEventChannels inputEventChannels;
        
        [Header("Input System Integration")]
        [SerializeField] private PlayerInput playerInput;
        
        [Header("Debug Options")]
        [SerializeField] private bool enableDebugLogging = false;
        [SerializeField] private bool showInputDebugInfo = false;
        
        // Input state tracking
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool isRunning;
        private bool isCrouching;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            ValidateComponents();
            RegisterToServiceLocator();
            SetupPlayerInputCallbacks();
        }
        
        private void OnDestroy()
        {
            UnregisterFromServiceLocator();
            CleanupPlayerInputCallbacks();
        }
        
        #endregion
        
        #region Service Registration
        
        private void RegisterToServiceLocator()
        {
            ServiceLocator.RegisterService<IInputManager>(this);
            LogDebug("InputService registered to ServiceLocator");
        }
        
        private void UnregisterFromServiceLocator()
        {
            ServiceLocator.UnregisterService<IInputManager>();
            LogDebug("InputService unregistered from ServiceLocator");
        }
        
        #endregion
        
        #region PlayerInput Callback Setup
        
        private void SetupPlayerInputCallbacks()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    Debug.LogError("[InputService] PlayerInput component not found!");
                    return;
                }
            }
            
            // Register callbacks for input events
            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;
            
            playerInput.actions["Look"].performed += OnLook;
            playerInput.actions["Look"].canceled += OnLook;
            
            playerInput.actions["Jump"].performed += OnJump;
            playerInput.actions["Interact"].performed += OnInteract;
            playerInput.actions["Attack"].performed += OnAttack;
            playerInput.actions["Run"].performed += OnRun;
            playerInput.actions["Run"].canceled += OnRun;
            playerInput.actions["Crouch"].performed += OnCrouch;
            playerInput.actions["Crouch"].canceled += OnCrouch;
            
            // Menu and UI inputs
            playerInput.actions["Menu"].performed += OnMenu;
            playerInput.actions["Inventory"].performed += OnInventory;
            
            LogDebug("PlayerInput callbacks setup completed");
        }
        
        private void CleanupPlayerInputCallbacks()
        {
            if (playerInput != null)
            {
                playerInput.actions["Move"].performed -= OnMove;
                playerInput.actions["Move"].canceled -= OnMove;
                playerInput.actions["Look"].performed -= OnLook;
                playerInput.actions["Look"].canceled -= OnLook;
                playerInput.actions["Jump"].performed -= OnJump;
                playerInput.actions["Interact"].performed -= OnInteract;
                playerInput.actions["Attack"].performed -= OnAttack;
                playerInput.actions["Run"].performed -= OnRun;
                playerInput.actions["Run"].canceled -= OnRun;
                playerInput.actions["Crouch"].performed -= OnCrouch;
                playerInput.actions["Crouch"].canceled -= OnCrouch;
                playerInput.actions["Menu"].performed -= OnMenu;
                playerInput.actions["Inventory"].performed -= OnInventory;
                
                LogDebug("PlayerInput callbacks cleaned up");
            }
        }
        
        #endregion
        
        #region Input Callbacks
        
        /// <summary>
        /// Called when movement input is received
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            
            if (inputEventChannels?.OnMoveInput != null)
            {
                inputEventChannels.OnMoveInput.Raise(moveInput);
                LogDebug($"Move input: {moveInput}");
            }
        }
        
        /// <summary>
        /// Called when camera look input is received
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
            
            if (inputEventChannels?.OnLookInput != null)
            {
                inputEventChannels.OnLookInput.Raise(lookInput);
                LogDebug($"Look input: {lookInput}");
            }
        }
        
        /// <summary>
        /// Called when jump input is pressed
        /// </summary>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (inputEventChannels?.OnJumpInputPressed != null)
                {
                    inputEventChannels.OnJumpInputPressed.Raise();
                    LogDebug("Jump input pressed");
                }
            }
        }
        
        /// <summary>
        /// Called when interact input is pressed
        /// </summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (inputEventChannels?.OnInteractInputPressed != null)
                {
                    inputEventChannels.OnInteractInputPressed.Raise();
                    LogDebug("Interact input pressed");
                }
            }
        }
        
        /// <summary>
        /// Called when attack input is pressed
        /// </summary>
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (inputEventChannels?.OnAttackInputPressed != null)
                {
                    inputEventChannels.OnAttackInputPressed.Raise();
                    LogDebug("Attack input pressed");
                }
            }
        }
        
        /// <summary>
        /// Called when run input state changes
        /// </summary>
        public void OnRun(InputAction.CallbackContext context)
        {
            isRunning = context.performed;
            
            if (inputEventChannels?.OnRunInputChanged != null)
            {
                inputEventChannels.OnRunInputChanged.Raise(isRunning);
                LogDebug($"Run input: {isRunning}");
            }
        }
        
        /// <summary>
        /// Called when crouch input state changes
        /// </summary>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            isCrouching = context.performed;
            
            if (inputEventChannels?.OnCrouchInputChanged != null)
            {
                inputEventChannels.OnCrouchInputChanged.Raise(isCrouching);
                LogDebug($"Crouch input: {isCrouching}");
            }
        }
        
        /// <summary>
        /// Called when menu input is pressed
        /// </summary>
        public void OnMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (inputEventChannels?.OnMenuInputPressed != null)
                {
                    inputEventChannels.OnMenuInputPressed.Raise();
                    LogDebug("Menu input pressed");
                }
            }
        }
        
        /// <summary>
        /// Called when inventory input is pressed
        /// </summary>
        public void OnInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (inputEventChannels?.OnInventoryInputPressed != null)
                {
                    inputEventChannels.OnInventoryInputPressed.Raise();
                    LogDebug("Inventory input pressed");
                }
            }
        }
        
        #endregion

        #region IInputManager Implementation

        // Sensitivity settings
        private float mouseSensitivity = 1.0f;
        private float gamepadSensitivity = 1.0f;
        private bool invertYAxis = false;
        private bool inputEnabled = true;

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
            LogDebug($"Mouse sensitivity set to: {sensitivity}");
        }

        public void SetGamepadSensitivity(float sensitivity)
        {
            gamepadSensitivity = sensitivity;
            LogDebug($"Gamepad sensitivity set to: {sensitivity}");
        }

        public void SetInvertYAxis(bool invert)
        {
            invertYAxis = invert;
            LogDebug($"Y-axis inversion set to: {invert}");
        }

        public float GetMouseSensitivity() => mouseSensitivity;

        public float GetGamepadSensitivity() => gamepadSensitivity;

        public bool IsYAxisInverted() => invertYAxis;

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (playerInput != null)
            {
                if (enabled)
                    playerInput.ActivateInput();
                else
                    playerInput.DeactivateInput();

                LogDebug($"Input {(enabled ? "enabled" : "disabled")}");
            }
        }

        public bool IsInputEnabled() => inputEnabled;

        public Vector2 GetMovementInput() => moveInput;

        public Vector2 GetLookInput() => lookInput;

        public bool GetJumpInputDown() => playerInput?.actions["Jump"].WasPressedThisFrame() ?? false;

        public bool GetRunInput() => isRunning;

        public bool GetCrouchInputDown() => playerInput?.actions["Crouch"].WasPressedThisFrame() ?? false;

        public bool GetAimInput() => playerInput?.actions["Aim"]?.IsPressed() ?? false;

        public bool GetFireInputDown() => playerInput?.actions["Attack"].WasPressedThisFrame() ?? false;

        public bool GetFireInput() => playerInput?.actions["Attack"]?.IsPressed() ?? false;

        public bool GetReloadInputDown() => playerInput?.actions["Reload"]?.WasPressedThisFrame() ?? false;

        public bool GetMeleeInputDown() => playerInput?.actions["Melee"]?.WasPressedThisFrame() ?? false;

        public bool GetInteractInputDown() => playerInput?.actions["Interact"].WasPressedThisFrame() ?? false;

        public bool GetPauseInputDown() => playerInput?.actions["Menu"].WasPressedThisFrame() ?? false;

        public bool GetRespawnInputDown() => playerInput?.actions["Respawn"]?.WasPressedThisFrame() ?? false;

        public bool IsJumpPressed() => playerInput?.actions["Jump"]?.IsPressed() ?? false;

        public bool IsSprintHeld() => isRunning;

        public bool IsCrouchPressed() => isCrouching;

        public void SetActionMap(string actionMap)
        {
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap(actionMap);
                LogDebug($"Switched to action map: {actionMap}");
            }
        }

        public void EnableAction(string actionName)
        {
            if (playerInput != null)
            {
                var action = playerInput.actions.FindAction(actionName);
                if (action != null)
                {
                    action.Enable();
                    LogDebug($"Enabled action: {actionName}");
                }
            }
        }

        public void DisableAction(string actionName)
        {
            if (playerInput != null)
            {
                var action = playerInput.actions.FindAction(actionName);
                if (action != null)
                {
                    action.Disable();
                    LogDebug($"Disabled action: {actionName}");
                }
            }
        }

        // Legacy public API for backward compatibility
        public Vector2 GetMoveInput() => moveInput;
        public bool IsRunning() => isRunning;
        public bool IsCrouching() => isCrouching;
        public void SwitchActionMap(string actionMapName) => SetActionMap(actionMapName);
        
        #endregion
        
        #region Validation & Debug
        
        private void ValidateComponents()
        {
            if (inputEventChannels == null)
            {
                Debug.LogError("[InputService] InputEventChannels not assigned! Please assign it in the Inspector.");
            }
            
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
                if (playerInput == null)
                {
                    Debug.LogError("[InputService] PlayerInput component not found! Please add PlayerInput component.");
                }
            }
            
            // Validate required action maps exist
            if (playerInput != null && playerInput.actions != null)
            {
                ValidateInputAction("Move");
                ValidateInputAction("Look");
                ValidateInputAction("Jump");
                ValidateInputAction("Interact");
                ValidateInputAction("Attack");
                ValidateInputAction("Run");
                ValidateInputAction("Crouch");
                ValidateInputAction("Menu");
                ValidateInputAction("Inventory");
            }
        }
        
        private void ValidateInputAction(string actionName)
        {
            if (playerInput.actions.FindAction(actionName) == null)
            {
                Debug.LogWarning($"[InputService] Input action '{actionName}' not found in PlayerInput actions!");
            }
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[InputService] {message}");
            }
        }
        
        #endregion
        
        #region Editor Debug
        
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showInputDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Input Service Debug", new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold });
            GUILayout.Label($"Move: {moveInput}");
            GUILayout.Label($"Look: {lookInput}");
            GUILayout.Label($"Running: {isRunning}");
            GUILayout.Label($"Crouching: {isCrouching}");
            GUILayout.Label($"Service Registered: {ServiceLocator.HasService<IInputManager>()}");
            GUILayout.EndArea();
        }
#endif
        
        #endregion
    }
}