using System;

namespace asterivo.Unity60.Features.Templates.Strategy.Resources
{
    /// <summary>
    /// Enum defining different types of resources in the Strategy template
    /// </summary>
    [Serializable]
    public enum ResourceType
    {
        /// <summary>
        /// Wood resource - used for basic construction and unit production
        /// </summary>
        Wood,
        
        /// <summary>
        /// Stone resource - used for advanced buildings and fortifications
        /// </summary>
        Stone,
        
        /// <summary>
        /// Food resource - required for unit maintenance and population growth
        /// </summary>
        Food,
        
        /// <summary>
        /// Gold resource - used for trade, advanced units, and technologies
        /// </summary>
        Gold,

        /// <summary>
        /// Metal resource - used for advanced weapons and machinery
        /// </summary>
        Metal,

        /// <summary>
        /// Population resource - represents available workforce and unit capacity
        /// </summary>
        Population,

        /// <summary>
        /// Energy resource - used for magical abilities and advanced technologies
        /// </summary>
        Energy
    }
}