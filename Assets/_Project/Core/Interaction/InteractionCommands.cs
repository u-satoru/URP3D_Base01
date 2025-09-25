using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Interaction
{
    /// <summary>
    /// Command to execute an interaction between an interactor and interactable.
    /// Integrates with the Command Pattern and ObjectPool optimization.
    /// </summary>
    public class InteractCommand : IResettableCommand
    {
        #region Private Fields

        private IInteractor _interactor;
        private IInteractable _interactable;
        private bool _interactionExecuted;
        private bool _interactionSuccessful;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_interactor == null || _interactable == null)
            {
                Debug.LogError("InteractCommand: Interactor or Interactable is null");
                return;
            }

            // Store execution state for undo
            _interactionExecuted = true;

            // Attempt interaction
            _interactionSuccessful = _interactable.OnInteract(_interactor);

            // Can only undo if interaction was successful and supports undo
            CanUndo = _interactionSuccessful && SupportsUndo();
        }

        public void Undo()
        {
            if (!CanUndo || !_interactionExecuted)
            {
                return;
            }

            // Perform undo-specific logic
            UndoInteraction();

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _interactor = null;
            _interactable = null;
            _interactionExecuted = false;
            _interactionSuccessful = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the interact command
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction</param>
        /// <param name="interactable">The target interactable</param>
        public void Initialize(IInteractor interactor, IInteractable interactable)
        {
            _interactor = interactor;
            _interactable = interactable;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = IInteractor, [1] = IInteractable</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("InteractCommand requires 2 parameters: IInteractor and IInteractable");

            var interactor = parameters[0] as IInteractor;
            if (interactor == null)
                throw new System.ArgumentException("First parameter must be an IInteractor");

            var interactable = parameters[1] as IInteractable;
            if (interactable == null)
                throw new System.ArgumentException("Second parameter must be an IInteractable");

            Initialize(interactor, interactable);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Check if this interaction supports undo functionality
        /// Override in derived classes to implement specific undo support
        /// </summary>
        /// <returns>True if interaction supports undo</returns>
        protected virtual bool SupportsUndo()
        {
            // Default: interactions don't support undo unless specifically implemented
            return false;
        }

        /// <summary>
        /// Perform undo-specific logic
        /// Override in derived classes to implement specific undo behavior
        /// </summary>
        protected virtual void UndoInteraction()
        {
            // Default: no undo behavior
            Debug.LogWarning("InteractCommand: Undo called but not implemented for this interaction type");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the interactor for this command
        /// </summary>
        public IInteractor Interactor => _interactor;

        /// <summary>
        /// Get the interactable for this command
        /// </summary>
        public IInteractable Interactable => _interactable;

        /// <summary>
        /// Check if the interaction was executed
        /// </summary>
        public bool InteractionExecuted => _interactionExecuted;

        /// <summary>
        /// Check if the interaction was successful
        /// </summary>
        public bool InteractionSuccessful => _interactionSuccessful;

        #endregion
    }

    /// <summary>
    /// Command to enable or disable an interactable object.
    /// Useful for controlling interaction availability through the command system.
    /// </summary>
    public class SetInteractableEnabledCommand : IResettableCommand
    {
        #region Private Fields

        private IInteractable _interactable;
        private bool _newEnabledState;
        private bool _previousEnabledState;
        private bool _wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_interactable == null)
            {
                Debug.LogError("SetInteractableEnabledCommand: Interactable is null");
                return;
            }

            // Store previous state for undo
            _previousEnabledState = _interactable.CanInteract;

            // Apply new state if the interactable supports state changes
            if (_interactable is BaseInteractable baseInteractable)
            {
                baseInteractable.SetInteractionEnabled(_newEnabledState);
                _wasExecuted = true;
                CanUndo = true;
            }
            else
            {
                Debug.LogWarning("SetInteractableEnabledCommand: Interactable does not support state changes");
                _wasExecuted = false;
                CanUndo = false;
            }
        }

        public void Undo()
        {
            if (!CanUndo || !_wasExecuted)
            {
                return;
            }

            // Restore previous state
            if (_interactable is BaseInteractable baseInteractable)
            {
                baseInteractable.SetInteractionEnabled(_previousEnabledState);
            }

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _interactable = null;
            _newEnabledState = false;
            _previousEnabledState = false;
            _wasExecuted = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the set enabled command
        /// </summary>
        /// <param name="interactable">The target interactable</param>
        /// <param name="enabled">The new enabled state</param>
        public void Initialize(IInteractable interactable, bool enabled)
        {
            _interactable = interactable;
            _newEnabledState = enabled;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = IInteractable, [1] = bool</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("SetInteractableEnabledCommand requires 2 parameters: IInteractable and bool");

            var interactable = parameters[0] as IInteractable;
            if (interactable == null)
                throw new System.ArgumentException("First parameter must be an IInteractable");

            if (!(parameters[1] is bool enabled))
                throw new System.ArgumentException("Second parameter must be a bool");

            Initialize(interactable, enabled);
        }

        #endregion
    }

    /// <summary>
    /// Command to update an interactable's properties (prompt, range, priority).
    /// Allows for dynamic modification of interactable characteristics.
    /// </summary>
    public class UpdateInteractablePropertiesCommand : IResettableCommand
    {
        #region Private Fields

        private IInteractable _interactable;
        private string _newPrompt;
        private float _newRange;
        private int _newPriority;

        private string _previousPrompt;
        private float _previousRange;
        private int _previousPriority;

        private bool _updatePrompt;
        private bool _updateRange;
        private bool _updatePriority;
        private bool _wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_interactable == null)
            {
                Debug.LogError("UpdateInteractablePropertiesCommand: Interactable is null");
                return;
            }

            if (!(_interactable is BaseInteractable baseInteractable))
            {
                Debug.LogWarning("UpdateInteractablePropertiesCommand: Interactable does not support property updates");
                return;
            }

            // Store previous state for undo
            _previousPrompt = _interactable.InteractionPrompt;
            _previousRange = _interactable.InteractionRange;
            _previousPriority = _interactable.InteractionPriority;

            // Apply new properties
            if (_updatePrompt)
            {
                baseInteractable.SetInteractionPrompt(_newPrompt);
            }

            if (_updateRange)
            {
                baseInteractable.SetInteractionRange(_newRange);
            }

            if (_updatePriority)
            {
                baseInteractable.SetInteractionPriority(_newPriority);
            }

            _wasExecuted = true;
            CanUndo = true;
        }

        public void Undo()
        {
            if (!CanUndo || !_wasExecuted)
            {
                return;
            }

            if (_interactable is BaseInteractable baseInteractable)
            {
                // Restore previous properties
                if (_updatePrompt)
                {
                    baseInteractable.SetInteractionPrompt(_previousPrompt);
                }

                if (_updateRange)
                {
                    baseInteractable.SetInteractionRange(_previousRange);
                }

                if (_updatePriority)
                {
                    baseInteractable.SetInteractionPriority(_previousPriority);
                }
            }

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _interactable = null;
            _newPrompt = null;
            _newRange = 0f;
            _newPriority = 0;

            _previousPrompt = null;
            _previousRange = 0f;
            _previousPriority = 0;

            _updatePrompt = false;
            _updateRange = false;
            _updatePriority = false;
            _wasExecuted = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the update properties command
        /// </summary>
        /// <param name="interactable">The target interactable</param>
        public void Initialize(IInteractable interactable)
        {
            _interactable = interactable;
            _updatePrompt = false;
            _updateRange = false;
            _updatePriority = false;
        }

        /// <summary>
        /// Set the new interaction prompt
        /// </summary>
        /// <param name="prompt">New prompt text</param>
        public void SetPrompt(string prompt)
        {
            _newPrompt = prompt;
            _updatePrompt = true;
        }

        /// <summary>
        /// Set the new interaction range
        /// </summary>
        /// <param name="range">New interaction range</param>
        public void SetRange(float range)
        {
            _newRange = range;
            _updateRange = true;
        }

        /// <summary>
        /// Set the new interaction priority
        /// </summary>
        /// <param name="priority">New priority value</param>
        public void SetPriority(int priority)
        {
            _newPriority = priority;
            _updatePriority = true;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = IInteractable</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                throw new System.ArgumentException("UpdateInteractablePropertiesCommand requires 1 parameter: IInteractable");

            var interactable = parameters[0] as IInteractable;
            if (interactable == null)
                throw new System.ArgumentException("First parameter must be an IInteractable");

            Initialize(interactable);
        }

        #endregion
    }

    /// <summary>
    /// Command to force an interaction detection update on an interactor.
    /// Useful for triggering immediate detection checks without waiting for the next update cycle.
    /// </summary>
    public class ForceInteractionDetectionCommand : IResettableCommand
    {
        #region Private Fields

        private IInteractor _interactor;

        #endregion

        #region ICommand Implementation

        public bool CanUndo => false; // Detection updates cannot be undone

        public void Execute()
        {
            if (_interactor == null)
            {
                Debug.LogError("ForceInteractionDetectionCommand: Interactor is null");
                return;
            }

            // Force detection update
            _interactor.UpdateInteractionDetection();
        }

        public void Undo()
        {
            // Detection updates cannot be undone
            Debug.LogWarning("ForceInteractionDetectionCommand: Undo is not supported for detection updates");
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _interactor = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the force detection command
        /// </summary>
        /// <param name="interactor">The target interactor</param>
        public void Initialize(IInteractor interactor)
        {
            _interactor = interactor;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = IInteractor</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                throw new System.ArgumentException("ForceInteractionDetectionCommand requires 1 parameter: IInteractor");

            var interactor = parameters[0] as IInteractor;
            if (interactor == null)
                throw new System.ArgumentException("First parameter must be an IInteractor");

            Initialize(interactor);
        }

        #endregion
    }

    /// <summary>
    /// Composite command that handles complex interaction scenarios.
    /// Can execute multiple interaction-related commands in sequence.
    /// </summary>
    public class CompositeInteractionCommand : IResettableCommand
    {
        #region Private Fields

        private readonly System.Collections.Generic.List<ICommand> _commands = new();
        private readonly System.Collections.Generic.Stack<ICommand> _executedCommands = new();

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            _executedCommands.Clear();
            bool allSuccessful = true;

            // Execute all commands in sequence
            foreach (var command in _commands)
            {
                try
                {
                    command.Execute();
                    _executedCommands.Push(command);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CompositeInteractionCommand: Error executing command {command.GetType().Name}: {e.Message}");
                    allSuccessful = false;
                    break;
                }
            }

            // Can undo if all commands were executed successfully and support undo
            CanUndo = allSuccessful && _executedCommands.Count > 0;
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            // Undo commands in reverse order
            while (_executedCommands.Count > 0)
            {
                var command = _executedCommands.Pop();
                try
                {
                    if (command.CanUndo)
                    {
                        command.Undo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CompositeInteractionCommand: Error undoing command {command.GetType().Name}: {e.Message}");
                }
            }

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _commands.Clear();
            _executedCommands.Clear();
            CanUndo = false;
        }

        #endregion

        #region Command Management

        /// <summary>
        /// Add a command to the composite
        /// </summary>
        /// <param name="command">Command to add</param>
        public void AddCommand(ICommand command)
        {
            if (command != null)
            {
                _commands.Add(command);
            }
        }

        /// <summary>
        /// Remove a command from the composite
        /// </summary>
        /// <param name="command">Command to remove</param>
        public void RemoveCommand(ICommand command)
        {
            _commands.Remove(command);
        }

        /// <summary>
        /// Clear all commands from the composite
        /// </summary>
        public void ClearCommands()
        {
            _commands.Clear();
        }

        /// <summary>
        /// Get the number of commands in the composite
        /// </summary>
        public int CommandCount => _commands.Count;

        #endregion

        #region IResettableCommand Interface Implementation

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// For CompositeInteractionCommand, this method doesn't take parameters - use AddCommand instead
        /// </summary>
        /// <param name="parameters">No parameters needed for composite command</param>
        public void Initialize(params object[] parameters)
        {
            // CompositeInteractionCommand doesn't need initialization parameters
            // Commands are added via AddCommand method
            Reset();
        }

        #endregion
    }
}
