using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// Invokes commands and manages command history for undo/redo functionality
    /// </summary>
    public class CommandInvoker : MonoBehaviour, IGameEventListener<ICommand>
    {
        [Header("Command Events")]
        [SerializeField] private CommandGameEvent onCommandReceived;
        
        [Header("Command History")]
        [SerializeField] private int maxHistorySize = 100;
        [SerializeField] private bool enableUndo = true;
        [SerializeField] private bool enableRedo = true;
        
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        
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
                Debug.LogWarning("CommandInvoker: Attempted to execute null command");
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
            return true;
            
            return false;
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
            
            return true;
        }
        
        /// <summary>
        /// Clears all command history
        /// </summary>
        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
        
        /// <summary>
        /// Event listener implementation
        /// </summary>
        public void OnEventRaised(ICommand value)
        {
            ExecuteCommand(value);
        }
        
        // Properties for checking state
        public bool CanUndo => enableUndo && undoStack.Count > 0;
        public bool CanRedo => enableRedo && redoStack.Count > 0;
        public int UndoStackCount => undoStack.Count;
        public int RedoStackCount => redoStack.Count;
    }
}