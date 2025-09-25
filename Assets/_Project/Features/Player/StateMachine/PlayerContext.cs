using UnityEngine;

namespace asterivo.Unity60.Features.Player.StateMachine
{
    /// <summary>
    /// Context object for player hierarchical state machine.
    /// Contains references to all systems the player states need to access.
    /// </summary>
    [System.Serializable]
    public class PlayerContext
    {
        #region Player References

        /// <summary>
        /// Reference to the player GameObject
        /// </summary>
        public GameObject PlayerGameObject { get; set; }

        /// <summary>
        /// Reference to the player's CharacterController
        /// </summary>
        public CharacterController CharacterController { get; set; }

        /// <summary>
        /// Reference to the player's Rigidbody (if used)
        /// </summary>
        public Rigidbody PlayerRigidbody { get; set; }

        #endregion

        #region System References

        /// <summary>
        /// Reference to input management system
        /// </summary>
        public IInputManager InputManager { get; set; }

        /// <summary>
        /// Reference to audio management system
        /// </summary>
        public IAudioManager AudioManager { get; set; }

        /// <summary>
        /// Reference to physics controller
        /// </summary>
        public IPhysicsController PhysicsController { get; set; }

        /// <summary>
        /// Reference to the player's state machine
        /// </summary>
        public IPlayerStateMachine StateMachine { get; set; }

        #endregion

        #region Player State Properties

        /// <summary>
        /// Current player position
        /// </summary>
        public Vector3 Position => PlayerGameObject?.transform.position ?? Vector3.zero;

        /// <summary>
        /// Current player rotation
        /// </summary>
        public Quaternion Rotation => PlayerGameObject?.transform.rotation ?? Quaternion.identity;

        /// <summary>
        /// Is player currently grounded
        /// </summary>
        public bool IsGrounded { get; set; }

        /// <summary>
        /// Current player velocity
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Player movement speed
        /// </summary>
        public float MovementSpeed { get; set; } = 5f;

        /// <summary>
        /// Player jump force
        /// </summary>
        public float JumpForce { get; set; } = 10f;

        /// <summary>
        /// Current stamina level
        /// </summary>
        public float Stamina { get; set; } = 100f;

        /// <summary>
        /// Maximum stamina
        /// </summary>
        public float MaxStamina { get; set; } = 100f;

        /// <summary>
        /// Is player in combat state
        /// </summary>
        public bool IsInCombat { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if player has sufficient stamina
        /// </summary>
        /// <param name="requiredStamina">Amount of stamina required</param>
        /// <returns>True if player has enough stamina</returns>
        public bool HasStamina(float requiredStamina = 10f)
        {
            return Stamina >= requiredStamina;
        }

        /// <summary>
        /// Consume stamina
        /// </summary>
        /// <param name="amount">Amount of stamina to consume</param>
        public void ConsumeStamina(float amount)
        {
            Stamina = Mathf.Max(0f, Stamina - amount);
        }

        /// <summary>
        /// Restore stamina
        /// </summary>
        /// <param name="amount">Amount of stamina to restore</param>
        public void RestoreStamina(float amount)
        {
            Stamina = Mathf.Min(MaxStamina, Stamina + amount);
        }

        /// <summary>
        /// Set grounded state
        /// </summary>
        /// <param name="grounded">Is player grounded</param>
        public void SetGrounded(bool grounded)
        {
            IsGrounded = grounded;
        }

        #endregion
    }

    #region Interface Definitions

    /// <summary>
    /// Interface for input management system
    /// </summary>
    public interface IInputManager
    {
        Vector2 GetMovementInput();
        bool IsSprintPressed();
        bool IsCrouchPressed();
        bool IsJumpPressed();
        bool IsGlidePressed();
    }

    /// <summary>
    /// Interface for audio management system
    /// </summary>
    public interface IAudioManager
    {
        void SetEnvironmentType(AudioEnvironmentType environmentType);
    }

    /// <summary>
    /// Interface for physics controller
    /// </summary>
    public interface IPhysicsController
    {
        bool IsGrounded();
        float GetVerticalVelocity();
        void EnableGroundedPhysics();
        void EnableAirbornePhysics();
    }

    /// <summary>
    /// Interface for player state machine
    /// </summary>
    public interface IPlayerStateMachine
    {
        void RequestTransition(string targetState);
    }

    /// <summary>
    /// Audio environment types
    /// </summary>
    public enum AudioEnvironmentType
    {
        Grounded,
        Airborne,
        Combat,
        Stealth
    }

    #endregion
}
