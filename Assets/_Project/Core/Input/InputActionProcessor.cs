using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Processes Unity Input System actions and converts them to our generic input data structures.
    /// Acts as a bridge between Unity's Input System and our event-driven input architecture.
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class InputActionProcessor : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private bool enableInputLogging = false;
        [SerializeField] private float inputThreshold = 0.1f; // Minimum input value to register
        [SerializeField] private float lookSensitivity = 1f;
        [SerializeField] private bool invertYAxis = false;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runMultiplier = 2f;

        [Header("Combat Settings")]
        [SerializeField] private float aimSensitivity = 0.5f;

        // Component references
        private PlayerInput playerInput;
        private InputManager inputManager;
        private InputActionMap currentActionMap;

        // Input state tracking
        private Vector2 currentMoveInput;
        private Vector2 currentLookInput;
        private bool isRunning;
        private bool isCrouching;
        private bool isJumping;
        private bool isAiming;
        private bool isFiring;
        private bool isReloading;
        private bool isInteracting;

        // UI input state
        private Vector2 currentUINavigate;
        private bool uiSubmit;
        private bool uiCancel;
        private bool uiPause;

        // Input buffer for smooth processing
        private readonly Dictionary<string, float> inputBuffer = new Dictionary<string, float>();
        private readonly Dictionary<string, Vector2> vector2Buffer = new Dictionary<string, Vector2>();

        #region Unity Lifecycle

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("[InputActionProcessor] PlayerInput component not found!");
                enabled = false;
                return;
            }

            InitializeInputBuffers();
        }

        private void Start()
        {
            // Get InputManager service
            inputManager = ServiceLocator.GetService<InputManager>();
            if (inputManager == null)
            {
                Debug.LogError("[InputActionProcessor] InputManager service not found!");
                enabled = false;
                return;
            }

            SetupInputActions();
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }

        private void Update()
        {
            ProcessContinuousInput();
        }

        #endregion

        #region Input Action Callbacks

        // Movement Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            currentMoveInput = context.ReadValue<Vector2>();
            LogInput("Move", currentMoveInput);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            currentLookInput = context.ReadValue<Vector2>();
            if (invertYAxis)
            {
                currentLookInput.y = -currentLookInput.y;
            }
            currentLookInput *= lookSensitivity;
            LogInput("Look", currentLookInput);
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            isRunning = context.ReadValueAsButton();
            LogInput("Run", isRunning);
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isCrouching = !isCrouching; // Toggle crouch
                LogInput("Crouch", isCrouching);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isJumping = true;
                LogInput("Jump", true);
            }
            else if (context.canceled)
            {
                isJumping = false;
            }
        }

        // Combat Actions
        public void OnFire(InputAction.CallbackContext context)
        {
            isFiring = context.ReadValueAsButton();
            LogInput("Fire", isFiring);
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            isAiming = context.ReadValueAsButton();
            LogInput("Aim", isAiming);
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isReloading = true;
                LogInput("Reload", true);
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isInteracting = true;
                LogInput("Interact", true);
            }
        }

        // UI Actions
        public void OnNavigate(InputAction.CallbackContext context)
        {
            currentUINavigate = context.ReadValue<Vector2>();
            LogInput("UI Navigate", currentUINavigate);
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            uiSubmit = context.performed;
            LogInput("UI Submit", uiSubmit);
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            uiCancel = context.performed;
            LogInput("UI Cancel", uiCancel);
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiPause = true;
                LogInput("UI Pause", true);

                // Toggle input context between Gameplay and Pause
                var currentContext = inputManager.GetCurrentInputState().context;
                var newContext = currentContext == InputContext.Pause ? InputContext.Gameplay : InputContext.Pause;
                inputManager.SetInputContext(newContext);
            }
        }

        #endregion

        #region Input Processing

        private void ProcessContinuousInput()
        {
            // Process movement input
            if (currentMoveInput.sqrMagnitude > inputThreshold * inputThreshold ||
                currentLookInput.sqrMagnitude > inputThreshold * inputThreshold ||
                isRunning || isCrouching || isJumping)
            {
                ProcessMovementInput();
            }

            // Process combat input
            if (isFiring || isAiming || isReloading || isInteracting)
            {
                ProcessCombatInput();
            }

            // Process UI input
            if (currentUINavigate.sqrMagnitude > inputThreshold * inputThreshold ||
                uiSubmit || uiCancel || uiPause)
            {
                ProcessUIInput();
            }

            // Reset one-frame inputs
            ResetOneFrameInputs();
        }

        private void ProcessMovementInput()
        {
            var movementData = MovementInputData.Create(
                currentMoveInput,
                currentLookInput,
                isRunning,
                isCrouching,
                isJumping
            );

            inputManager.ProcessMovementInput(movementData);
        }

        private void ProcessCombatInput()
        {
            var aimDirection = isAiming ? currentLookInput * aimSensitivity : Vector2.zero;

            var combatData = CombatInputData.Create(
                isFiring,
                isAiming,
                isReloading,
                isInteracting,
                aimDirection
            );

            inputManager.ProcessCombatInput(combatData);
        }

        private void ProcessUIInput()
        {
            var uiData = UIInputData.Create(
                currentUINavigate,
                uiSubmit,
                uiCancel,
                uiPause
            );

            inputManager.ProcessUIInput(uiData);
        }

        private void ResetOneFrameInputs()
        {
            // Reset inputs that should only be active for one frame
            isJumping = false;
            isReloading = false;
            isInteracting = false;
            uiSubmit = false;
            uiCancel = false;
            uiPause = false;
        }

        #endregion

        #region Setup and Configuration

        private void SetupInputActions()
        {
            if (playerInput.actions == null)
            {
                Debug.LogError("[InputActionProcessor] No input actions asset assigned to PlayerInput!");
                return;
            }

            currentActionMap = playerInput.currentActionMap;
            LogInput("Setup", $"Using action map: {currentActionMap?.name}");
        }

        private void InitializeInputBuffers()
        {
            // Initialize input buffers with default values
            inputBuffer["move"] = 0f;
            inputBuffer["look"] = 0f;
            inputBuffer["run"] = 0f;
            inputBuffer["crouch"] = 0f;
            inputBuffer["jump"] = 0f;
            inputBuffer["fire"] = 0f;
            inputBuffer["aim"] = 0f;
            inputBuffer["reload"] = 0f;
            inputBuffer["interact"] = 0f;

            vector2Buffer["move"] = Vector2.zero;
            vector2Buffer["look"] = Vector2.zero;
            vector2Buffer["navigate"] = Vector2.zero;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Switch to a different input action map
        /// </summary>
        /// <param name="actionMapName">Name of the action map to switch to</param>
        public void SwitchActionMap(string actionMapName)
        {
            if (playerInput.actions.FindActionMap(actionMapName) != null)
            {
                playerInput.SwitchCurrentActionMap(actionMapName);
                currentActionMap = playerInput.currentActionMap;
                LogInput("ActionMap", $"Switched to: {actionMapName}");
            }
            else
            {
                Debug.LogWarning($"[InputActionProcessor] Action map '{actionMapName}' not found!");
            }
        }

        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        /// <param name="enabled">Whether input should be enabled</param>
        public void SetInputEnabled(bool enabled)
        {
            playerInput.enabled = enabled;
            if (inputManager != null)
            {
                inputManager.SetInputEnabled(enabled);
            }
            LogInput("InputEnabled", enabled);
        }

        /// <summary>
        /// Update input sensitivity settings
        /// </summary>
        /// <param name="lookSens">Look sensitivity</param>
        /// <param name="aimSens">Aim sensitivity</param>
        /// <param name="invertY">Invert Y axis</param>
        public void UpdateSensitivitySettings(float lookSens, float aimSens, bool invertY)
        {
            lookSensitivity = lookSens;
            aimSensitivity = aimSens;
            invertYAxis = invertY;
            LogInput("Sensitivity", $"Look: {lookSens}, Aim: {aimSens}, InvertY: {invertY}");
        }

        #endregion

        #region Debug Helpers

        private void LogInput(string actionName, object value)
        {
            if (enableInputLogging)
            {
                Debug.Log($"[InputActionProcessor] {actionName}: {value}");
            }
        }

        /// <summary>
        /// Get current input state for debugging
        /// </summary>
        /// <returns>Formatted debug information</returns>
        public string GetDebugInfo()
        {
            return $"InputActionProcessor Debug Info:\n" +
                   $"Move: {currentMoveInput}\n" +
                   $"Look: {currentLookInput}\n" +
                   $"Running: {isRunning}\n" +
                   $"Crouching: {isCrouching}\n" +
                   $"Jumping: {isJumping}\n" +
                   $"Firing: {isFiring}\n" +
                   $"Aiming: {isAiming}\n" +
                   $"Current Action Map: {currentActionMap?.name}";
        }

        #endregion
    }
}
