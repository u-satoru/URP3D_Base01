using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Invokes commands and manages command history for undo/redo functionality
    /// </summary>
    public class CommandInvoker : MonoBehaviour, IGameEventListener<ICommand>
    {
        [Header("Command Events")]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        [Header("State Change Events")]
        [SerializeField] private BoolEventChannelSO onUndoStateChanged;
        [SerializeField] private BoolEventChannelSO onRedoStateChanged;
        
        [Header("Command History")]
        [SerializeField] private int maxHistorySize = 100;
        [SerializeField] private bool enableUndo = true;
        [SerializeField] private bool enableRedo = true;

        [Header("Command Target")]
        [SerializeField] private Component playerHealthComponent;
        private IHealthTarget playerHealth;
        
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        
        private void Start()
        {
            // Initialize health target from component reference
            if (playerHealthComponent != null)
            {
                playerHealth = playerHealthComponent.GetComponent<IHealthTarget>();
                if (playerHealth == null)
                {
                    UnityEngine.Debug.LogError("CommandInvoker: playerHealthComponent does not implement IHealthTarget.");
                }
            }
        }
        
        private void OnEnable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.RegisterListener(this);
            }
        }
        
        private void OnDisable()
        {
            if (onCommandReceived != null)
            {
                onCommandReceived.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// Executes a command directly
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                UnityEngine.Debug.LogWarning("CommandInvoker: Attempted to execute null command");
                return;
            }
            
            command.Execute();
            
            // Add to undo stack if undo is enabled and command supports it
            if (enableUndo && command.CanUndo)
            {
                undoStack.Push(command);
                
                // Limit history size
                while (undoStack.Count > maxHistorySize)
                {
                    var tempStack = new Stack<ICommand>();
                    var items = undoStack.ToArray();
                    
                    // Skip the oldest item
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
                
                // Clear redo stack when a new command is executed
                if (enableRedo)
                {
                    redoStack.Clear();
                }
                
                // Broadcast state changes
                BroadcastHistoryChanges();
            }
        }
        
        /// <summary>
        /// Undoes the last command
        /// </summary>
        public bool Undo()
        {
            if (!enableUndo || undoStack.Count == 0)
                return false;
                
            var command = undoStack.Pop();
            command.Undo();
            if (command.CanUndo)
            {
                if (enableRedo)
                {
                    redoStack.Push(command);
                }
            }
            
            BroadcastHistoryChanges();
            return true;
        }
        
        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        public bool Redo()
        {
            if (!enableRedo || redoStack.Count == 0)
                return false;
                
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
        /// Clears all command history
        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
            BroadcastHistoryChanges();
        }
        
        /// <summary>
        /// Broadcasts the current state of undo/redo stacks to UI and other systems
        /// </summary>
        private void BroadcastHistoryChanges()
        {
            onUndoStateChanged?.Raise(CanUndo);
            onRedoStateChanged?.Raise(CanRedo);
        }
        
        /// <summary>
        /// Event listener implementation
        /// </summary>
        public void OnEventRaised(ICommand value)
        {
            ExecuteCommand(value);
        }

        /// <summary>
        /// Event listener for when an ItemData is used.
        /// Creates and executes commands based on the item's definitions.
        /// </summary>
        public void OnItemUsed(ItemData itemData)
        {
            if (itemData == null)
            {
                UnityEngine.Debug.LogWarning("OnItemUsed called with null ItemData.");
                return;
            }

            foreach (var definition in itemData.commandDefinitions)
            {
                ICommand command = CreateCommandFromDefinition(definition);
                if (command != null)
                {
                    ExecuteCommand(command);
                }
            }
        }

        /// <summary>
        /// Factory method for creating commands from definitions (ドキュメント第4章:481-495行目の実装)
        /// ICommandDefinitionからICommandを生成するファクトリメソッド
        /// </summary>
        private ICommand CreateCommandFromDefinition(ICommandDefinition definition)
        {
            // Ensure we have a target for health-related commands
            if (playerHealth == null)
            {
                UnityEngine.Debug.LogError("CommandInvoker: playerHealth target is not set.");
                return null;
            }

            // Use the definition's factory method directly (ハイブリッドアーキテクチャ)
            var command = definition.CreateCommand(playerHealth);
            
            if (command == null)
            {
                UnityEngine.Debug.LogWarning($"Failed to create command from definition type: {definition.GetType()}");
            }
            
            return command;
        }
        
        // Properties for checking state
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        public int UndoStackCount => undoStack.Count;
        public int RedoStackCount => redoStack.Count;
    }
}