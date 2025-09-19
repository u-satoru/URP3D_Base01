using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Character
{
    /// <summary>
    /// Interface for character management systems.
    /// Provides a standard API for external systems to control character movement.
    /// Integrates with Command Pattern and Event-Driven Architecture.
    /// </summary>
    public interface ICharacterManager
    {
        #region Properties

        /// <summary>
        /// Current character position
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Current character velocity
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Current horizontal movement speed
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Is the character currently grounded
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Is the character currently jumping
        /// </summary>
        bool IsJumping { get; }

        /// <summary>
        /// Is the character currently falling
        /// </summary>
        bool IsFalling { get; }

        /// <summary>
        /// Current movement mode
        /// </summary>
        CharacterMover.MovementMode CurrentMovementMode { get; }

        #endregion

        #region Movement Control

        /// <summary>
        /// Set movement direction for the character
        /// </summary>
        /// <param name="direction">Normalized movement direction</param>
        void SetMovementDirection(Vector3 direction);

        /// <summary>
        /// Set movement mode (walk, run, crouch)
        /// </summary>
        /// <param name="mode">Movement mode to set</param>
        void SetMovementMode(CharacterMover.MovementMode mode);

        /// <summary>
        /// Stop all character movement
        /// </summary>
        void StopMovement();

        /// <summary>
        /// Process character input data
        /// </summary>
        /// <param name="inputData">Input data to process</param>
        void ProcessInput(CharacterInputData inputData);

        #endregion

        #region Jump Control

        /// <summary>
        /// Attempt to make the character jump
        /// </summary>
        /// <returns>True if jump was successful</returns>
        bool TryJump();

        /// <summary>
        /// Force the character to jump (bypasses ground checks)
        /// </summary>
        void ForceJump();

        #endregion

        #region Physics

        /// <summary>
        /// Apply external force to the character
        /// </summary>
        /// <param name="force">Force vector to apply</param>
        void AddForce(Vector3 force);

        /// <summary>
        /// Set vertical velocity directly
        /// </summary>
        /// <param name="verticalVelocity">Vertical velocity to set</param>
        void SetVerticalVelocity(float verticalVelocity);

        #endregion

        #region Command Integration

        /// <summary>
        /// Execute a character control command
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command was executed successfully</returns>
        bool ExecuteCommand(ICommand command);

        /// <summary>
        /// Create a movement command for this character
        /// </summary>
        /// <param name="direction">Movement direction</param>
        /// <param name="mode">Movement mode</param>
        /// <returns>Movement command</returns>
        MoveCharacterCommand CreateMoveCommand(Vector3 direction, CharacterMover.MovementMode mode = CharacterMover.MovementMode.Walk);

        /// <summary>
        /// Create a jump command for this character
        /// </summary>
        /// <returns>Jump command</returns>
        JumpCharacterCommand CreateJumpCommand();

        /// <summary>
        /// Create a stop movement command for this character
        /// </summary>
        /// <returns>Stop command</returns>
        StopCharacterCommand CreateStopCommand();

        /// <summary>
        /// Create a force application command for this character
        /// </summary>
        /// <param name="force">Force to apply</param>
        /// <returns>Force command</returns>
        ApplyForceCommand CreateForceCommand(Vector3 force);

        /// <summary>
        /// Create an input processing command for this character
        /// </summary>
        /// <param name="inputData">Input data to process</param>
        /// <returns>Input processing command</returns>
        ProcessCharacterInputCommand CreateInputCommand(CharacterInputData inputData);

        #endregion

        #region Utility

        /// <summary>
        /// Get current movement statistics for debugging
        /// </summary>
        /// <returns>Movement statistics string</returns>
        string GetMovementStats();

        #endregion

        #region Events

        /// <summary>
        /// Event fired when character position changes
        /// </summary>
        event System.Action<Vector3> OnPositionChanged;

        /// <summary>
        /// Event fired when character velocity changes significantly
        /// </summary>
        event System.Action<Vector3> OnVelocityChanged;

        /// <summary>
        /// Event fired when character lands on ground
        /// </summary>
        event System.Action OnLanded;

        /// <summary>
        /// Event fired when character starts jumping
        /// </summary>
        event System.Action OnJumpStarted;

        /// <summary>
        /// Event fired when character's grounded state changes
        /// </summary>
        event System.Action<bool> OnGroundStateChanged;

        #endregion
    }
}