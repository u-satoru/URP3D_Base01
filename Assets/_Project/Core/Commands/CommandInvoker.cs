using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Components;

using System.Linq;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Core class for the Command pattern implementation.
    /// Manages command execution and command history for Undo/Redo functionality.
    /// </summary>
    public class CommandInvoker : MonoBehaviour, ICommandInvoker, IGameEventListener<object>
    {
        [Header("Command Events")]
        [Tooltip("Event to receive commands to execute")]
        [SerializeField] private CommandGameEvent onCommandReceived;

        [Header("State Change Events")]
        [Tooltip("Event triggered when Undo availability changes")]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [Tooltip("Event triggered when Redo availability changes")]
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;

        [Header("Command History")]
        [Tooltip("Maximum number of command history to keep")]
        [SerializeField] private int maxHistorySize = 100;
        [Tooltip("Enable Undo functionality")]
        [SerializeField] private bool enableUndo = true;
        [Tooltip("Enable Redo functionality")]
        [SerializeField] private bool enableRedo = true;

        [Header("Command Target")]
        [Tooltip("Health component that will be the target of commands")]
        [SerializeField] private Component playerHealthComponent;
        private IHealthTarget playerHealth;

        /// <summary>
        /// Stack to store executed commands for Undo functionality.
        /// </summary>
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        /// <summary>
        /// Stack to store undone commands for Redo functionality.
        /// </summary>
        private Stack<ICommand> redoStack = new Stack<ICommand>();

        /// <summary>
        /// Indicates whether Undo is available.
        /// </summary>
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        /// <summary>
        /// Indicates whether Redo is available.
        /// </summary>
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        /// <summary>
        /// Gets the number of commands currently in the Undo stack.
        /// </summary>
        public int UndoStackCount => undoStack.Count;
        /// <summary>
        /// Gets the number of commands currently in the Redo stack.
        /// </summary>
        public int RedoStackCount => redoStack.Count;

        /// <summary>
        /// Called when the script becomes enabled for the first time.
        /// Initializes the Health target.
        /// </summary>
        private void Start()
        {
            // Register as ICommandInvoker with ServiceLocator
            ServiceLocator.RegisterService<ICommandInvoker>(this);

            // Initialize Health target from component reference
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.GetComponent<IHealthTarget>();
                if (playerHealth == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogError("CommandInvoker: playerHealthComponent does not implement IHealthTarget");
#endif
                }
            }
        }

        /// <summary>
        /// Called when the object becomes enabled.
        /// Registers command reception event listener.
        /// </summary>
        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }

        /// <summary>
        /// Called when the object becomes disabled.
        /// Unregisters command reception event listener.
        /// </summary>
        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Executes the specified command and adds it to Undo history.
        /// </summary>
        /// <param name="command">Command to execute</param>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandInvoker: Attempted to execute null command");
#endif
                return;
            }

            command.Execute();

            // Add to Undo stack if Undo is enabled and command supports Undo
            if (enableUndo && command.CanUndo)
            {
                undoStack.Push(command);

                // Limit history size
                while (undoStack.Count > maxHistorySize)
                {
                    var tempStack = new Stack<ICommand>();
                    var items = undoStack.ToArray();

                    // Remove the oldest item
                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        tempStack.Push(items[i]);
                    }

                    undoStack.Clear();
                    while (tempStack.Count > 0)
                    {
                        undoStack.Push(tempStack.Pop());
                    }
                }

                // Clear Redo stack when a new command is executed
                if (enableRedo)
                {
                    redoStack.Clear();
                }

                BroadcastHistoryChanges();
            }
        }

        /// <summary>
        /// Undoes the last executed command.
        /// </summary>
        /// <returns>true if Undo was successful</returns>
        public bool Undo()
        {
            if (!CanUndo) return false;

            var command = undoStack.Pop();
            command.Undo();

            if (command.CanUndo && enableRedo)
            {
                redoStack.Push(command);
            }

            BroadcastHistoryChanges();
            return true;
        }

        /// <summary>
        /// Re-executes an undone command (Redo).
        /// </summary>
        /// <returns>true if Redo was successful</returns>
        public bool Redo()
        {
            if (!CanRedo) return false;

            var command = redoStack.Pop();
            command.Execute();

            if (enableUndo)
            {
                undoStack.Push(command);
            }

            BroadcastHistoryChanges();
            return true;
        }

        /// <summary>
        /// Clears all command history (Undo/Redo).
        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            BroadcastHistoryChanges();
        }

        /// <summary>
        /// Notifies UI and other systems about Undo/Redo stack state changes.
        /// </summary>
        private void BroadcastHistoryChanges()
        {
            onUndoStateChanged?.Raise(CanUndo);
            onRedoStateChanged?.Raise(CanRedo);
        }

        /// <summary>
        /// Listener method for receiving commands through game events.
        /// </summary>
        /// <param name="value">Received object (casted to ICommand)</param>
        public void OnEventRaised(object value)
        {
            if (value is ICommand command)
            {
                ExecuteCommand(command);
            }
            else
            {
                Debug.LogWarning($"[CommandInvoker] Received object is not ICommand: {value?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// Listener for item usage events.
        /// Creates and executes commands from the command definitions included in item data.
        /// </summary>
        /// <param name="itemData">Used item data</param>
        public void OnItemUsed(ItemData itemData)
        {
            if (itemData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("OnItemUsed was called with null ItemData");
#endif
                return;
            }

            foreach (var definition in itemData.commandDefinitions)
            {
                if (definition is ICommandDefinition commandDefinition)
                {
                    ICommand command = CreateCommandFromDefinition(commandDefinition);
                    if (command != null)
                    {
                        ExecuteCommand(command);
                    }
                }
                else
                {
                    Debug.LogWarning($"[CommandInvoker] ItemData contains invalid type object in commandDefinitions: {definition?.GetType().Name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// Factory method that creates a concrete command from a command definition.
        /// </summary>
        /// <param name="definition">Definition for creating the command</param>
        /// <returns>Created command, or null if creation failed</returns>
        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            if (playerHealth == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogError("CommandInvoker: Command execution target (playerHealth) is not set");
#endif
                return null;
            }

            // Use the definition's factory method directly
            var command = definition.CreateCommand(playerHealth);

            if (command == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Failed to create command from definition type: {definition.GetType()}");
#endif
            }

            return command;
        }
    }
}