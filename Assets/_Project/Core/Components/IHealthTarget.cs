namespace asterivo.Unity60.Core.Components
{
    /// <summary>
    /// Interface for objects that can receive health-related actions
    /// This allows commands to work with any health system without direct dependencies
    /// </summary>
    public interface IHealthTarget
    {
        /// <summary>
        /// Heals the target by the specified amount
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        void Heal(int amount);
        
        /// <summary>
        /// Damages the target by the specified amount
        /// </summary>
        /// <param name="amount">Amount of damage to deal</param>
        void TakeDamage(int amount);
        
        /// <summary>
        /// Current health of the target
        /// </summary>
        int CurrentHealth { get; }
        
        /// <summary>
        /// Maximum health of the target
        /// </summary>
        int MaxHealth { get; }
    }
}