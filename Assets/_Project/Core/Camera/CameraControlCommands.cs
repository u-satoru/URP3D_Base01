using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Camera
{
    /// <summary>
    /// Command to change camera state through the Command Pattern.
    /// Supports Undo functionality for state changes.
    /// </summary>
    public class ChangeCameraStateCommand : IResettableCommand
    {
        #region Private Fields

        private ICameraController cameraController;
        private System.Enum targetState;
        private System.Enum previousState;
        private bool wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (cameraController == null)
            {
                Debug.LogError("ChangeCameraStateCommand: Camera controller is null");
                return;
            }

            previousState = cameraController.CurrentStateType;
            bool success = cameraController.TransitionToState(targetState);

            wasExecuted = success;
            CanUndo = success && previousState != null && !previousState.Equals(targetState);

            if (!success)
            {
                Debug.LogWarning($"ChangeCameraStateCommand: Failed to transition to state {targetState}");
            }
        }

        public void Undo()
        {
            if (!CanUndo || !wasExecuted || previousState == null)
            {
                return;
            }

            cameraController.TransitionToState(previousState);
            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            cameraController = null;
            targetState = null;
            previousState = null;
            wasExecuted = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the camera state change command
        /// </summary>
        /// <param name="controller">Camera controller to operate on</param>
        /// <param name="newState">Target state to transition to</param>
        public void Initialize(ICameraController controller, System.Enum newState)
        {
            cameraController = controller;
            targetState = newState;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = ICameraController, [1] = System.Enum</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("ChangeCameraStateCommand requires 2 parameters: ICameraController and System.Enum");

            var controller = parameters[0] as ICameraController;
            if (controller == null)
                throw new System.ArgumentException("First parameter must be an ICameraController");

            if (!(parameters[1] is System.Enum state))
                throw new System.ArgumentException("Second parameter must be a System.Enum");

            Initialize(controller, state);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the camera controller for this command
        /// </summary>
        public ICameraController CameraController => cameraController;

        /// <summary>
        /// Get the target state for this command
        /// </summary>
        public System.Enum TargetState => targetState;

        /// <summary>
        /// Check if the command was executed successfully
        /// </summary>
        public bool WasExecuted => wasExecuted;

        #endregion
    }

    /// <summary>
    /// Command to set camera target through the Command Pattern.
    /// Supports Undo functionality for target changes.
    /// </summary>
    public class SetCameraTargetCommand : IResettableCommand
    {
        #region Private Fields

        private ICameraController cameraController;
        private Transform newTarget;
        private Transform previousTarget;
        private bool wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (cameraController == null)
            {
                Debug.LogError("SetCameraTargetCommand: Camera controller is null");
                return;
            }

            previousTarget = cameraController.Target;
            cameraController.SetTarget(newTarget);

            wasExecuted = true;
            CanUndo = previousTarget != newTarget;
        }

        public void Undo()
        {
            if (!CanUndo || !wasExecuted)
            {
                return;
            }

            cameraController.SetTarget(previousTarget);
            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            cameraController = null;
            newTarget = null;
            previousTarget = null;
            wasExecuted = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the set camera target command
        /// </summary>
        /// <param name="controller">Camera controller to operate on</param>
        /// <param name="target">New target transform</param>
        public void Initialize(ICameraController controller, Transform target)
        {
            cameraController = controller;
            newTarget = target;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = ICameraController, [1] = Transform</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("SetCameraTargetCommand requires 2 parameters: ICameraController and Transform");

            var controller = parameters[0] as ICameraController;
            if (controller == null)
                throw new System.ArgumentException("First parameter must be an ICameraController");

            var target = parameters[1] as Transform;
            if (target == null)
                throw new System.ArgumentException("Second parameter must be a Transform");

            Initialize(controller, target);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the camera controller for this command
        /// </summary>
        public ICameraController CameraController => cameraController;

        /// <summary>
        /// Get the new target for this command
        /// </summary>
        public Transform NewTarget => newTarget;

        /// <summary>
        /// Check if the command was executed successfully
        /// </summary>
        public bool WasExecuted => wasExecuted;

        #endregion
    }

    /// <summary>
    /// Command to handle camera input through the Command Pattern.
    /// Allows for input recording, playback, and batching.
    /// </summary>
    public class CameraInputCommand : IResettableCommand
    {
        #region Private Fields

        private ICameraController cameraController;
        private CameraInputData inputData;
        private bool wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo => false; // Input commands typically cannot be undone

        public void Execute()
        {
            if (cameraController == null)
            {
                Debug.LogError("CameraInputCommand: Camera controller is null");
                return;
            }

            cameraController.HandleInput(inputData);
            wasExecuted = true;
        }

        public void Undo()
        {
            // Input commands cannot be undone
            Debug.LogWarning("CameraInputCommand: Input commands cannot be undone");
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            cameraController = null;
            inputData = default(CameraInputData);
            wasExecuted = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the camera input command
        /// </summary>
        /// <param name="controller">Camera controller to operate on</param>
        /// <param name="input">Camera input data</param>
        public void Initialize(ICameraController controller, CameraInputData input)
        {
            cameraController = controller;
            inputData = input;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = ICameraController, [1] = CameraInputData</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("CameraInputCommand requires 2 parameters: ICameraController and CameraInputData");

            var controller = parameters[0] as ICameraController;
            if (controller == null)
                throw new System.ArgumentException("First parameter must be an ICameraController");

            if (!(parameters[1] is CameraInputData inputData))
                throw new System.ArgumentException("Second parameter must be CameraInputData");

            Initialize(controller, inputData);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the camera controller for this command
        /// </summary>
        public ICameraController CameraController => cameraController;

        /// <summary>
        /// Get the input data for this command
        /// </summary>
        public CameraInputData InputData => inputData;

        /// <summary>
        /// Check if the command was executed successfully
        /// </summary>
        public bool WasExecuted => wasExecuted;

        #endregion
    }

    /// <summary>
    /// Command to activate or deactivate camera controller through the Command Pattern.
    /// Supports Undo functionality for activation state changes.
    /// </summary>
    public class SetCameraActiveCommand : IResettableCommand
    {
        #region Private Fields

        private ICameraController cameraController;
        private bool targetActiveState;
        private bool previousActiveState;
        private bool wasExecuted;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (cameraController == null)
            {
                Debug.LogError("SetCameraActiveCommand: Camera controller is null");
                return;
            }

            previousActiveState = cameraController.IsActive;
            cameraController.IsActive = targetActiveState;

            wasExecuted = true;
            CanUndo = previousActiveState != targetActiveState;
        }

        public void Undo()
        {
            if (!CanUndo || !wasExecuted)
            {
                return;
            }

            cameraController.IsActive = previousActiveState;
            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            cameraController = null;
            targetActiveState = false;
            previousActiveState = false;
            wasExecuted = false;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the set camera active command
        /// </summary>
        /// <param name="controller">Camera controller to operate on</param>
        /// <param name="active">Target active state</param>
        public void Initialize(ICameraController controller, bool active)
        {
            cameraController = controller;
            targetActiveState = active;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = ICameraController, [1] = bool</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("SetCameraActiveCommand requires 2 parameters: ICameraController and bool");

            var controller = parameters[0] as ICameraController;
            if (controller == null)
                throw new System.ArgumentException("First parameter must be an ICameraController");

            if (!(parameters[1] is bool active))
                throw new System.ArgumentException("Second parameter must be a bool");

            Initialize(controller, active);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get the camera controller for this command
        /// </summary>
        public ICameraController CameraController => cameraController;

        /// <summary>
        /// Get the target active state for this command
        /// </summary>
        public bool TargetActiveState => targetActiveState;

        /// <summary>
        /// Check if the command was executed successfully
        /// </summary>
        public bool WasExecuted => wasExecuted;

        #endregion
    }

    /// <summary>
    /// Composite command for complex camera operations.
    /// Allows executing multiple camera commands in sequence with proper Undo support.
    /// </summary>
    public class CompositeCameraCommand : IResettableCommand
    {
        #region Private Fields

        private readonly System.Collections.Generic.List<ICommand> commands = new();
        private readonly System.Collections.Generic.Stack<ICommand> executedCommands = new();

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            executedCommands.Clear();
            bool allSuccessful = true;

            foreach (var command in commands)
            {
                try
                {
                    command.Execute();
                    executedCommands.Push(command);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CompositeCameraCommand: Error executing command {command.GetType().Name}: {e.Message}");
                    allSuccessful = false;
                    break;
                }
            }

            CanUndo = allSuccessful && executedCommands.Count > 0;
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            while (executedCommands.Count > 0)
            {
                var command = executedCommands.Pop();
                try
                {
                    if (command.CanUndo)
                    {
                        command.Undo();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CompositeCameraCommand: Error undoing command {command.GetType().Name}: {e.Message}");
                }
            }

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            commands.Clear();
            executedCommands.Clear();
            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Interface Implementation

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// For CompositeCameraCommand, this method doesn't take parameters - use AddCommand instead
        /// </summary>
        /// <param name="parameters">No parameters needed for composite command</param>
        public void Initialize(params object[] parameters)
        {
            // CompositeCameraCommand doesn't need initialization parameters
            // Commands are added via AddCommand method
            Reset();
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
                commands.Add(command);
            }
        }

        /// <summary>
        /// Remove a command from the composite
        /// </summary>
        /// <param name="command">Command to remove</param>
        public void RemoveCommand(ICommand command)
        {
            commands.Remove(command);
        }

        /// <summary>
        /// Clear all commands from the composite
        /// </summary>
        public void ClearCommands()
        {
            commands.Clear();
        }

        /// <summary>
        /// Get the number of commands in the composite
        /// </summary>
        public int CommandCount => commands.Count;

        #endregion
    }
}
