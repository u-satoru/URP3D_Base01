using UnityEngine;
using asterivo.Unity60.Features.Templates.Strategy.Resources;

namespace asterivo.Unity60.Features.Templates.Strategy.Buildings
{
    /// <summary>
    /// Event data for building health changes
    /// </summary>
    [System.Serializable]
    public class BuildingHealthChangedEventData
    {
        public StrategyBuilding building;
        public float previousHealth;
        public float currentHealth;
        public float maxHealth;
        public float damageTaken;
        public bool wasDestroyed;
        public Vector3 position;

        public BuildingHealthChangedEventData()
        {
        }

        public BuildingHealthChangedEventData(StrategyBuilding building, float previousHealth, float currentHealth, float maxHealth, float damageTaken = 0f)
        {
            this.building = building;
            this.previousHealth = previousHealth;
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
            this.damageTaken = damageTaken;
            this.wasDestroyed = currentHealth <= 0f;
            this.position = building != null ? building.transform.position : Vector3.zero;
        }

        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsDestroyed => currentHealth <= 0f;
        public bool IsCriticalHealth => HealthPercentage <= 0.25f;
        public bool IsLowHealth => HealthPercentage <= 0.5f;
    }

    /// <summary>
    /// Event data for building operational state changes
    /// </summary>
    [System.Serializable]
    public class BuildingOperationalStateChangedEventData
    {
        public StrategyBuilding building;
        public bool previousState;
        public bool currentState;
        public BuildingOperationalChangeReason reason;
        public Vector3 position;

        public BuildingOperationalStateChangedEventData()
        {
        }

        public BuildingOperationalStateChangedEventData(StrategyBuilding building, bool previousState, bool currentState, BuildingOperationalChangeReason reason = BuildingOperationalChangeReason.Manual)
        {
            this.building = building;
            this.previousState = previousState;
            this.currentState = currentState;
            this.reason = reason;
            this.position = building != null ? building.transform.position : Vector3.zero;
        }

        public bool IsNowOperational => currentState;
        public bool WasOperational => previousState;
        public bool StateChanged => previousState != currentState;
    }

    /// <summary>
    /// Reasons for building operational state changes
    /// </summary>
    public enum BuildingOperationalChangeReason
    {
        Manual,           // Manually toggled by player
        Damaged,          // Disabled due to damage
        ResourceShortage, // Disabled due to lack of resources
        PowerOutage,      // Disabled due to power shortage
        Maintenance,      // Disabled for maintenance
        Upgrade,          // Disabled during upgrade
        Research,         // Disabled due to research requirements
        Enemy            // Disabled by enemy action
    }

    /// <summary>
    /// Event data for building construction events
    /// </summary>
    [System.Serializable]
    public class BuildingConstructionEventData
    {
        public StrategyBuilding building;
        public BuildingConstructionState state;
        public float constructionProgress;
        public float constructionTime;
        public Vector3 position;
        public ResourceCostCollection resourcesSpent;

        public BuildingConstructionEventData()
        {
        }

        public BuildingConstructionEventData(StrategyBuilding building, BuildingConstructionState state, float progress = 0f, float time = 0f)
        {
            this.building = building;
            this.state = state;
            this.constructionProgress = progress;
            this.constructionTime = time;
            this.position = building != null ? building.transform.position : Vector3.zero;
        }

        public bool IsCompleted => state == BuildingConstructionState.Completed;
        public bool IsInProgress => state == BuildingConstructionState.InProgress;
        public bool IsCancelled => state == BuildingConstructionState.Cancelled;
    }

    /// <summary>
    /// Building construction states
    /// </summary>
    public enum BuildingConstructionState
    {
        Planned,       // Building planned but not started
        InProgress,    // Currently under construction
        Completed,     // Construction finished successfully
        Cancelled,     // Construction was cancelled
        Failed,        // Construction failed (e.g., lack of resources)
        Paused         // Construction temporarily paused
    }

    /// <summary>
    /// Event data for building production events (resource generation, unit training, etc.)
    /// </summary>
    [System.Serializable]
    public class BuildingProductionEventData
    {
        public StrategyBuilding building;
        public BuildingProductionType productionType;
        public ResourceType resourceProduced;
        public int amountProduced;
        public Vector3 position;

        public BuildingProductionEventData()
        {
        }

        public BuildingProductionEventData(StrategyBuilding building, BuildingProductionType type, ResourceType resource = ResourceType.Wood, int amount = 0)
        {
            this.building = building;
            this.productionType = type;
            this.resourceProduced = resource;
            this.amountProduced = amount;
            this.position = building != null ? building.transform.position : Vector3.zero;
        }
    }

    /// <summary>
    /// Types of building production
    /// </summary>
    public enum BuildingProductionType
    {
        ResourceGeneration,  // Building produced resources
        UnitTraining,        // Building trained units
        Research,            // Building completed research
        Upgrade,             // Building completed upgrade
        Repair              // Building completed self-repair
    }

    /// <summary>
    /// Event data for building selection events
    /// </summary>
    [System.Serializable]
    public class BuildingSelectionEventData
    {
        public StrategyBuilding building;
        public bool isSelected;
        public bool isMultipleSelection;
        public int totalSelectedBuildings;
        public Vector3 position;

        public BuildingSelectionEventData()
        {
        }

        public BuildingSelectionEventData(StrategyBuilding building, bool selected, bool multiple = false, int total = 1)
        {
            this.building = building;
            this.isSelected = selected;
            this.isMultipleSelection = multiple;
            this.totalSelectedBuildings = total;
            this.position = building != null ? building.transform.position : Vector3.zero;
        }
    }
}