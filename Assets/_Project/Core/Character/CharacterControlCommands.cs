using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Character
{
    /// <summary>
    /// Command to control character movement.
    /// Integrates with the Command Pattern and ObjectPool optimization.
    /// </summary>
    public class MoveCharacterCommand : IResettableCommand
    {
        #region Private Fields

        private CharacterMover _characterMover;
        private Vector3 _movementDirection;
        private CharacterMover.MovementMode _movementMode;
        private Vector3 _previousInputDirection;
        private CharacterMover.MovementMode _previousMovementMode;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_characterMover == null)
            {
                Debug.LogError("MoveCharacterCommand: CharacterMover is null");
                return;
            }

            // Store previous state for undo
            _previousInputDirection = _characterMover.InputDirection;
            _previousMovementMode = _characterMover.CurrentMovementMode;

            // Apply new movement
            _characterMover.SetMovementInput(_movementDirection);
            _characterMover.SetMovementMode(_movementMode);

            CanUndo = true;
        }

        public void Undo()
        {
            if (_characterMover == null || !CanUndo)
            {
                return;
            }

            // Restore previous state
            _characterMover.SetMovementInput(_previousInputDirection);
            _characterMover.SetMovementMode(_previousMovementMode);

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _characterMover = null;
            _movementDirection = Vector3.zero;
            _movementMode = CharacterMover.MovementMode.Walk;
            _previousInputDirection = Vector3.zero;
            _previousMovementMode = CharacterMover.MovementMode.Walk;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the move command
        /// </summary>
        /// <param name="characterMover">Target character mover</param>
        /// <param name="direction">Movement direction</param>
        /// <param name="mode">Movement mode</param>
        public void Initialize(CharacterMover characterMover, Vector3 direction, CharacterMover.MovementMode mode = CharacterMover.MovementMode.Walk)
        {
            _characterMover = characterMover;
            _movementDirection = direction;
            _movementMode = mode;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = CharacterMover, [1] = Vector3 direction, [2] = MovementMode (optional)</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("MoveCharacterCommand requires at least 2 parameters: CharacterMover and Vector3");

            var characterMover = parameters[0] as CharacterMover;
            if (characterMover == null)
                throw new System.ArgumentException("First parameter must be a CharacterMover");

            if (!(parameters[1] is Vector3 direction))
                throw new System.ArgumentException("Second parameter must be a Vector3");

            var mode = CharacterMover.MovementMode.Walk;
            if (parameters.Length > 2 && parameters[2] is CharacterMover.MovementMode movementMode)
                mode = movementMode;

            Initialize(characterMover, direction, mode);
        }

        #endregion
    }

    /// <summary>
    /// Command to make character jump.
    /// Integrates with the Command Pattern and ObjectPool optimization.
    /// </summary>
    public class JumpCharacterCommand : IResettableCommand
    {
        #region Private Fields

        private CharacterMover _characterMover;
        private bool _jumpExecuted;
        private float _previousVerticalVelocity;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_characterMover == null)
            {
                Debug.LogError("JumpCharacterCommand: CharacterMover is null");
                return;
            }

            // Store previous state for undo
            _previousVerticalVelocity = _characterMover.Velocity.y;

            // Attempt jump
            _jumpExecuted = _characterMover.TryJump();

            CanUndo = _jumpExecuted; // Can only undo if jump actually happened
        }

        public void Undo()
        {
            if (_characterMover == null || !CanUndo)
            {
                return;
            }

            // Restore previous vertical velocity (this is a bit artificial but demonstrates undo pattern)
            _characterMover.SetVerticalVelocity(_previousVerticalVelocity);

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _characterMover = null;
            _jumpExecuted = false;
            _previousVerticalVelocity = 0f;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the jump command
        /// </summary>
        /// <param name="characterMover">Target character mover</param>
        public void Initialize(CharacterMover characterMover)
        {
            _characterMover = characterMover;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = CharacterMover</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                throw new System.ArgumentException("JumpCharacterCommand requires 1 parameter: CharacterMover");

            var characterMover = parameters[0] as CharacterMover;
            if (characterMover == null)
                throw new System.ArgumentException("First parameter must be a CharacterMover");

            Initialize(characterMover);
        }

        #endregion
    }

    /// <summary>
    /// Command to stop character movement.
    /// Integrates with the Command Pattern and ObjectPool optimization.
    /// </summary>
    public class StopCharacterCommand : IResettableCommand
    {
        #region Private Fields

        private CharacterMover _characterMover;
        private Vector3 _previousInputDirection;
        private float _previousSpeed;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_characterMover == null)
            {
                Debug.LogError("StopCharacterCommand: CharacterMover is null");
                return;
            }

            // Store previous state for undo
            _previousInputDirection = _characterMover.InputDirection;
            _previousSpeed = _characterMover.CurrentSpeed;

            // Stop movement
            _characterMover.StopMovement();

            CanUndo = true;
        }

        public void Undo()
        {
            if (_characterMover == null || !CanUndo)
            {
                return;
            }

            // Restore previous movement
            _characterMover.SetMovementInput(_previousInputDirection);

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _characterMover = null;
            _previousInputDirection = Vector3.zero;
            _previousSpeed = 0f;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the stop command
        /// </summary>
        /// <param name="characterMover">Target character mover</param>
        public void Initialize(CharacterMover characterMover)
        {
            _characterMover = characterMover;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = CharacterMover</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                throw new System.ArgumentException("StopCharacterCommand requires 1 parameter: CharacterMover");

            var characterMover = parameters[0] as CharacterMover;
            if (characterMover == null)
                throw new System.ArgumentException("First parameter must be a CharacterMover");

            Initialize(characterMover);
        }

        #endregion
    }

    /// <summary>
    /// Command to apply external force to character.
    /// Useful for knockback, explosions, wind effects, etc.
    /// </summary>
    public class ApplyForceCommand : IResettableCommand
    {
        #region Private Fields

        private CharacterMover _characterMover;
        private Vector3 _force;
        private Vector3 _previousVelocity;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_characterMover == null)
            {
                Debug.LogError("ApplyForceCommand: CharacterMover is null");
                return;
            }

            // Store previous state for undo
            _previousVelocity = _characterMover.Velocity;

            // Apply force
            _characterMover.AddForce(_force);

            CanUndo = true;
        }

        public void Undo()
        {
            if (_characterMover == null || !CanUndo)
            {
                return;
            }

            // This is a simplified undo - in reality, undoing forces is complex
            // For demonstration purposes, we'll set velocity back to previous state
            _characterMover.SetVerticalVelocity(_previousVelocity.y);

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _characterMover = null;
            _force = Vector3.zero;
            _previousVelocity = Vector3.zero;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the apply force command
        /// </summary>
        /// <param name="characterMover">Target character mover</param>
        /// <param name="force">Force to apply</param>
        public void Initialize(CharacterMover characterMover, Vector3 force)
        {
            _characterMover = characterMover;
            _force = force;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = CharacterMover, [1] = Vector3 force</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("ApplyForceCommand requires 2 parameters: CharacterMover and Vector3");

            var characterMover = parameters[0] as CharacterMover;
            if (characterMover == null)
                throw new System.ArgumentException("First parameter must be a CharacterMover");

            if (!(parameters[1] is Vector3 force))
                throw new System.ArgumentException("Second parameter must be a Vector3");

            Initialize(characterMover, force);
        }

        #endregion
    }

    /// <summary>
    /// Composite command to handle complex character input.
    /// Processes CharacterInputData and executes appropriate movement commands.
    /// </summary>
    public class ProcessCharacterInputCommand : IResettableCommand
    {
        #region Private Fields

        private CharacterMover _characterMover;
        private CharacterInputData _inputData;
        private Vector3 _previousInputDirection;
        private CharacterMover.MovementMode _previousMovementMode;

        #endregion

        #region ICommand Implementation

        public bool CanUndo { get; private set; }

        public void Execute()
        {
            if (_characterMover == null)
            {
                Debug.LogError("ProcessCharacterInputCommand: CharacterMover is null");
                return;
            }

            // Store previous state for undo
            _previousInputDirection = _characterMover.InputDirection;
            _previousMovementMode = _characterMover.CurrentMovementMode;

            // Process movement input
            if (_inputData.HasMovementInput)
            {
                _characterMover.SetMovementInput(_inputData.worldMovementDirection);

                // Determine movement mode based on input
                CharacterMover.MovementMode mode = CharacterMover.MovementMode.Walk;
                if (_inputData.runPressed)
                    mode = CharacterMover.MovementMode.Run;
                else if (_inputData.crouchHeld)
                    mode = CharacterMover.MovementMode.Crouch;

                _characterMover.SetMovementMode(mode);
            }
            else
            {
                // No movement input - stop movement
                _characterMover.SetMovementInput(Vector3.zero);
            }

            // Process jump input
            if (_inputData.jumpPressed)
            {
                _characterMover.TryJump();
            }

            CanUndo = true;
        }

        public void Undo()
        {
            if (_characterMover == null || !CanUndo)
            {
                return;
            }

            // Restore previous state
            _characterMover.SetMovementInput(_previousInputDirection);
            _characterMover.SetMovementMode(_previousMovementMode);

            CanUndo = false;
        }

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _characterMover = null;
            _inputData = CharacterInputData.Empty;
            _previousInputDirection = Vector3.zero;
            _previousMovementMode = CharacterMover.MovementMode.Walk;
            CanUndo = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the process input command
        /// </summary>
        /// <param name="characterMover">Target character mover</param>
        /// <param name="inputData">Input data to process</param>
        public void Initialize(CharacterMover characterMover, CharacterInputData inputData)
        {
            _characterMover = characterMover;
            _inputData = inputData;
        }

        /// <summary>
        /// Initialize the command with parameters (IResettableCommand interface)
        /// </summary>
        /// <param name="parameters">Parameters: [0] = CharacterMover, [1] = CharacterInputData</param>
        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
                throw new System.ArgumentException("ProcessCharacterInputCommand requires 2 parameters: CharacterMover and CharacterInputData");

            var characterMover = parameters[0] as CharacterMover;
            if (characterMover == null)
                throw new System.ArgumentException("First parameter must be a CharacterMover");

            if (!(parameters[1] is CharacterInputData inputData))
                throw new System.ArgumentException("Second parameter must be CharacterInputData");

            Initialize(characterMover, inputData);
        }

        #endregion
    }
}
