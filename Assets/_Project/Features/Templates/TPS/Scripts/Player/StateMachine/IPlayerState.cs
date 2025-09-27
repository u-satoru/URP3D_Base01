namespace asterivo.Unity60.Features.Templates.TPS.Player.StateMachine
{
    /// <summary>
    /// Interface for all TPS player states
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// Called when entering this state
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        void Update();

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        void Exit();

        /// <summary>
        /// Get the state type
        /// </summary>
        PlayerState StateType { get; }
    }
}
