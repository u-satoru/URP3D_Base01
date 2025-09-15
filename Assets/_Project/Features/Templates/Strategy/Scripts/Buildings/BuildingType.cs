using System;

namespace asterivo.Unity60.Features.Templates.Strategy.Buildings
{
    /// <summary>
    /// Enum defining different types of buildings in the Strategy template
    /// </summary>
    [Serializable]
    public enum BuildingType
    {
        // Basic Infrastructure
        TownCenter,          // Main building - spawns units, researches tech
        CommandCenter,       // Alternative main building for military operations
        House,               // Increases population capacity
        Warehouse,           // Increases resource storage capacity
        Market,              // Trade and exchange resources
        
        // Resource Buildings
        LumberMill,          // Produces wood
        StoneMine,           // Produces stone
        Farm,                // Produces food
        GoldMine,            // Produces gold
        ResourceGenerator,   // Generic resource production building
        Factory,             // Industrial production building
        
        // Military Buildings
        Barracks,            // Trains basic infantry units
        ArcheryRange,        // Trains ranged units
        Stable,              // Trains cavalry units
        SiegeWorkshop,       // Builds siege equipment
        
        // Defensive Structures
        Wall,                // Defensive barrier
        Tower,               // Defensive tower with ranged attack
        DefenseTower,        // Advanced defensive tower
        Gate,                // Defensive gate in walls
        Castle,              // Advanced defensive structure
        
        // Research & Technology
        University,          // Research center for technologies
        ResearchLab,         // Advanced research laboratory
        Blacksmith,          // Upgrades for military units
        Temple,              // Religious building with unique benefits
        
        // Utility Buildings
        Road,                // Improves unit movement speed
        Bridge,              // Allows crossing water/terrain obstacles
        Dock,                // Naval units and sea trade
        
        // Advanced Buildings
        WizardTower,         // Magic research and spells
        DragonLair,          // Advanced units breeding
        ArtilleryFoundry,    // Advanced siege equipment
        
        // Special Buildings
        Wonder,              // Victory condition building
        Monument,            // Cultural/morale boost building
        
        // Placeholder for mods/expansion
        Custom
    }
}