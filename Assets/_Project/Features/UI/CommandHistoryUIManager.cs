using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.UI
{
    /// <summary>
    /// Manages UI elements related to command history (Undo/Redo buttons)
    /// Demonstrates the event-driven architecture for UI updates
    /// </summary>
    public class CommandHistoryUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button redoButton;
        
        [Header("Event Channels")]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;
        
        private void OnEnable()
        {
            // Subscribe to state change events
            if (onUndoStateChanged != null)
                onUndoStateChanged.OnEventRaised += UpdateUndoButtonState;
                
            if (onRedoStateChanged != null)
                onRedoStateChanged.OnEventRaised += UpdateRedoButtonState;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events to prevent memory leaks
            if (onUndoStateChanged != null)
                onUndoStateChanged.OnEventRaised -= UpdateUndoButtonState;
                
            if (onRedoStateChanged != null)
                onRedoStateChanged.OnEventRaised -= UpdateRedoButtonState;
        }
        
        /// <summary>
        /// Updates the undo button's interactable state
        /// Called via event when CommandInvoker's undo stack changes
        /// </summary>
        public void UpdateUndoButtonState(bool isEnabled)
        {
            if (undoButton != null)
            {
                undoButton.interactable = isEnabled;
                
                // Optional: Change visual appearance based on state
                var colors = undoButton.colors;
                colors.normalColor = isEnabled ? Color.white : Color.gray;
                undoButton.colors = colors;
            }
        }
        
        /// <summary>
        /// Updates the redo button's interactable state
        /// Called via event when CommandInvoker's redo stack changes
        /// </summary>
        public void UpdateRedoButtonState(bool isEnabled)
        {
            if (redoButton != null)
            {
                redoButton.interactable = isEnabled;
                
                // Optional: Change visual appearance based on state
                var colors = redoButton.colors;
                colors.normalColor = isEnabled ? Color.white : Color.gray;
                redoButton.colors = colors;
            }
        }
        
        /// <summary>
        /// Initialize buttons on start (optional)
        /// </summary>
        private void Start()
        {
            // Set initial state (assuming no commands can be undone/redone initially)
            UpdateUndoButtonState(false);
            UpdateRedoButtonState(false);
        }
    }
}