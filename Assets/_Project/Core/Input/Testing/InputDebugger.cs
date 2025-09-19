using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Input.Testing
{
    /// <summary>
    /// Test component for debugging InputService event flow
    /// Listens to InputEventChannels and logs input events to console using IGameEventListener pattern
    /// </summary>
    public class InputDebugger : MonoBehaviour, IGameEventListener, IGameEventListener<Vector2>, IGameEventListener<bool>
    {
        [Header("Event Channels to Monitor")]
        [SerializeField] private InputEventChannels inputEventChannels;
        
        [Header("Debug Options")]
        [SerializeField] private bool logMovementEvents = true;
        [SerializeField] private bool logActionEvents = true;
        [SerializeField] private bool logUIEvents = true;
        [SerializeField] private bool showOnScreenDebug = true;
        [SerializeField] private bool enableDetailedLogging = false;
        
        [Header("On-Screen Debug Settings")]
        [SerializeField] private Vector2 debugUIPosition = new Vector2(10, 200);
        [SerializeField] private Vector2 debugUISize = new Vector2(400, 300);
        
        // Input state tracking for display
        private Vector2 lastMoveInput;
        private Vector2 lastLookInput;
        private bool isRunning;
        private bool isCrouching;
        private string lastActionPressed = "None";
        private float lastActionTime;
        
        // Event counters
        private int moveEventCount;
        private int actionEventCount;
        private int uiEventCount;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (inputEventChannels == null)
            {
                Debug.LogError("[InputDebugger] InputEventChannels not assigned! Please assign it in the Inspector.");
                return;
            }
            
            SubscribeToEvents();
            Debug.Log("[InputDebugger] Started monitoring input events.");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Event Subscription
        
        private void SubscribeToEvents()
        {
            if (inputEventChannels == null) return;

            // Movement Events using proper IGameEventListener pattern
            if (logMovementEvents)
            {
                if (inputEventChannels.OnMoveInput != null)
                    inputEventChannels.OnMoveInput.RegisterListener(this);

                if (inputEventChannels.OnLookInput != null)
                    inputEventChannels.OnLookInput.RegisterListener(this);

                if (inputEventChannels.OnRunInputChanged != null)
                    inputEventChannels.OnRunInputChanged.RegisterListener(this);

                if (inputEventChannels.OnCrouchInputChanged != null)
                    inputEventChannels.OnCrouchInputChanged.RegisterListener(this);
            }

            // Action Events using proper IGameEventListener pattern
            if (logActionEvents)
            {
                if (inputEventChannels.OnJumpInputPressed != null)
                    inputEventChannels.OnJumpInputPressed.RegisterListener(this);

                if (inputEventChannels.OnInteractInputPressed != null)
                    inputEventChannels.OnInteractInputPressed.RegisterListener(this);

                if (inputEventChannels.OnAttackInputPressed != null)
                    inputEventChannels.OnAttackInputPressed.RegisterListener(this);
            }

            // UI Events using proper IGameEventListener pattern
            if (logUIEvents)
            {
                if (inputEventChannels.OnMenuInputPressed != null)
                    inputEventChannels.OnMenuInputPressed.RegisterListener(this);

                if (inputEventChannels.OnInventoryInputPressed != null)
                    inputEventChannels.OnInventoryInputPressed.RegisterListener(this);
            }

            Debug.Log("[InputDebugger] Subscribed to input events successfully using IGameEventListener pattern.");
        }
        
        private void UnsubscribeFromEvents()
        {
            if (inputEventChannels == null) return;

            // Movement Events using proper IGameEventListener pattern
            if (inputEventChannels.OnMoveInput != null)
                inputEventChannels.OnMoveInput.UnregisterListener(this);

            if (inputEventChannels.OnLookInput != null)
                inputEventChannels.OnLookInput.UnregisterListener(this);

            if (inputEventChannels.OnRunInputChanged != null)
                inputEventChannels.OnRunInputChanged.UnregisterListener(this);

            if (inputEventChannels.OnCrouchInputChanged != null)
                inputEventChannels.OnCrouchInputChanged.UnregisterListener(this);

            // Action Events using proper IGameEventListener pattern
            if (inputEventChannels.OnJumpInputPressed != null)
                inputEventChannels.OnJumpInputPressed.UnregisterListener(this);

            if (inputEventChannels.OnInteractInputPressed != null)
                inputEventChannels.OnInteractInputPressed.UnregisterListener(this);

            if (inputEventChannels.OnAttackInputPressed != null)
                inputEventChannels.OnAttackInputPressed.UnregisterListener(this);

            // UI Events using proper IGameEventListener pattern
            if (inputEventChannels.OnMenuInputPressed != null)
                inputEventChannels.OnMenuInputPressed.UnregisterListener(this);

            if (inputEventChannels.OnInventoryInputPressed != null)
                inputEventChannels.OnInventoryInputPressed.UnregisterListener(this);

            Debug.Log("[InputDebugger] Unsubscribed from input events successfully.");
        }

        #endregion

        #region IGameEventListener Implementation

        public void OnEventRaised()
        {
            // Handle parameterless events (action events like Jump, Interact, Attack, Menu, Inventory)
            // Since we can't distinguish which specific action event was triggered from this method,
            // we'll increment the action event counter and log a generic action
            actionEventCount++;
            lastActionPressed = "Generic Action";
            lastActionTime = Time.time;

            if (enableDetailedLogging)
            {
                Debug.Log($"[InputDebugger] Generic Action Event Triggered (Event #{actionEventCount})");
            }
        }

        public void OnEventRaised(Vector2 value)
        {
            // Since we can't distinguish between different Vector2 events in this method,
            // we'll handle both move and look input here
            // We can use the magnitude to make a reasonable guess about the event type

            if (value.sqrMagnitude > 100f) // High magnitude values are likely look input (mouse movement)
            {
                OnLookInputReceived(value);
            }
            else // Lower magnitude values are likely move input (normalized controller/keyboard input)
            {
                OnMoveInputReceived(value);
            }
        }

        public void OnEventRaised(bool value)
        {
            // Since we can't distinguish between different bool events in this method,
            // we'll need to track the last bool event type in our subscription logic
            // For now, we'll route to the most common bool event (running)
            OnRunInputReceived(value);
        }

        #endregion

        #region Event Handlers

        // Movement Event Handlers
        private void OnMoveInputReceived(Vector2 moveInput)
        {
            lastMoveInput = moveInput;
            moveEventCount++;
            
            if (enableDetailedLogging)
            {
                Debug.Log($"[InputDebugger] Move Input: {moveInput} (Event #{moveEventCount})");
            }
        }
        
        private void OnLookInputReceived(Vector2 lookInput)
        {
            lastLookInput = lookInput;
            moveEventCount++;
            
            if (enableDetailedLogging)
            {
                Debug.Log($"[InputDebugger] Look Input: {lookInput} (Event #{moveEventCount})");
            }
        }
        
        private void OnRunInputReceived(bool isRunning)
        {
            this.isRunning = isRunning;
            moveEventCount++;
            lastActionPressed = isRunning ? "Run ON" : "Run OFF";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Run Input: {isRunning}");
        }
        
        private void OnCrouchInputReceived(bool isCrouching)
        {
            this.isCrouching = isCrouching;
            moveEventCount++;
            lastActionPressed = isCrouching ? "Crouch ON" : "Crouch OFF";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Crouch Input: {isCrouching}");
        }
        
        // Action Event Handlers
        private void OnJumpInputReceived()
        {
            actionEventCount++;
            lastActionPressed = "Jump";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Jump Input Pressed (Event #{actionEventCount})");
        }
        
        private void OnInteractInputReceived()
        {
            actionEventCount++;
            lastActionPressed = "Interact";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Interact Input Pressed (Event #{actionEventCount})");
        }
        
        private void OnAttackInputReceived()
        {
            actionEventCount++;
            lastActionPressed = "Attack";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Attack Input Pressed (Event #{actionEventCount})");
        }
        
        // UI Event Handlers
        private void OnMenuInputReceived()
        {
            uiEventCount++;
            lastActionPressed = "Menu";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Menu Input Pressed (Event #{uiEventCount})");
        }
        
        private void OnInventoryInputReceived()
        {
            uiEventCount++;
            lastActionPressed = "Inventory";
            lastActionTime = Time.time;
            
            Debug.Log($"[InputDebugger] Inventory Input Pressed (Event #{uiEventCount})");
        }
        
        #endregion
        
        #region Debug Display
        
        private void OnGUI()
        {
            if (!showOnScreenDebug) return;
            
            GUILayout.BeginArea(new Rect(debugUIPosition.x, debugUIPosition.y, debugUISize.x, debugUISize.y));
            
            // Header
            GUILayout.Label("Input Debugger", new GUIStyle(GUI.skin.label) 
            { 
                fontSize = 16, 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            });
            
            GUILayout.Space(10);
            
            // Event Counters
            GUILayout.Label($"Event Counters:", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            });
            GUILayout.Label($"  Movement Events: {moveEventCount}");
            GUILayout.Label($"  Action Events: {actionEventCount}");
            GUILayout.Label($"  UI Events: {uiEventCount}");
            
            GUILayout.Space(10);
            
            // Current Input States
            GUILayout.Label($"Current Input States:", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            });
            GUILayout.Label($"  Move: {lastMoveInput}");
            GUILayout.Label($"  Look: {lastLookInput}");
            GUILayout.Label($"  Running: {isRunning}");
            GUILayout.Label($"  Crouching: {isCrouching}");
            
            GUILayout.Space(10);
            
            // Last Action
            GUILayout.Label($"Last Action:", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green }
            });
            GUILayout.Label($"  Action: {lastActionPressed}");
            GUILayout.Label($"  Time: {(Time.time - lastActionTime):F2}s ago");
            
            GUILayout.Space(10);
            
            // Service Status
            GUILayout.Label($"Service Status:", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.magenta }
            });
            
            var inputService = ServiceLocator.GetService<IInputManager>();
            bool serviceRegistered = inputService != null;
            GUILayout.Label($"  InputService: {(serviceRegistered ? "✓ Registered" : "✗ Not Found")}");
            GUILayout.Label($"  EventChannels: {(inputEventChannels != null ? "✓ Assigned" : "✗ Missing")}");
            
            // Test Instructions
            GUILayout.Space(10);
            GUILayout.Label($"Test Instructions:", new GUIStyle(GUI.skin.label) 
            { 
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            });
            GUILayout.Label("  • Use WASD for movement");
            GUILayout.Label("  • Use mouse for look");
            GUILayout.Label("  • Press Space to jump");
            GUILayout.Label("  • Press E to interact");
            GUILayout.Label("  • Hold Shift to run");
            GUILayout.Label("  • Hold Ctrl to crouch");
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Public API for Testing
        
        /// <summary>
        /// Get total number of events received
        /// </summary>
        public int GetTotalEventCount()
        {
            return moveEventCount + actionEventCount + uiEventCount;
        }
        
        /// <summary>
        /// Reset all event counters
        /// </summary>
        [ContextMenu("Reset Event Counters")]
        public void ResetEventCounters()
        {
            moveEventCount = 0;
            actionEventCount = 0;
            uiEventCount = 0;
            lastActionPressed = "None";
            lastActionTime = 0;
            
            Debug.Log("[InputDebugger] Event counters reset.");
        }
        
        /// <summary>
        /// Print comprehensive debug report
        /// </summary>
        [ContextMenu("Print Debug Report")]
        public void PrintDebugReport()
        {
            Debug.Log("=== INPUT DEBUGGER REPORT ===");
            Debug.Log($"Movement Events: {moveEventCount}");
            Debug.Log($"Action Events: {actionEventCount}");
            Debug.Log($"UI Events: {uiEventCount}");
            Debug.Log($"Total Events: {GetTotalEventCount()}");
            Debug.Log($"Current Move Input: {lastMoveInput}");
            Debug.Log($"Current Look Input: {lastLookInput}");
            Debug.Log($"Running: {isRunning}");
            Debug.Log($"Crouching: {isCrouching}");
            Debug.Log($"Last Action: {lastActionPressed}");
            Debug.Log($"EventChannels Assigned: {inputEventChannels != null}");
            Debug.Log("============================");
        }
        
        #endregion
    }
}