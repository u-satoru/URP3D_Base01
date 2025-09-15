using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Features.Templates.Strategy.Resources;

namespace asterivo.Unity60.Features.Templates.Strategy.Buildings
{
    /// <summary>
    /// ScriptableObject database containing all building data for the Strategy template
    /// </summary>
    [CreateAssetMenu(fileName = "StrategyBuildingDatabase", menuName = "Strategy Template/Building Database")]
    public class BuildingDatabase : ScriptableObject
    {
        [Header("Building Database")]
        [SerializeField] private List<BuildingData> buildings = new List<BuildingData>();
        
        [Header("Default Settings")]
        [SerializeField] private float defaultConstructionTime = 5f;
        [SerializeField] private float defaultMaxHealth = 100f;
        [SerializeField] private float defaultOperationalRange = 5f;
        [SerializeField] private float defaultResourceGenerationRate = 1f;

        // Cache for fast lookups
        private Dictionary<BuildingType, BuildingData> buildingLookup;

        private void OnEnable()
        {
            RefreshLookupCache();
        }

        private void OnValidate()
        {
            RefreshLookupCache();
            ValidateBuildingData();
        }

        /// <summary>
        /// Get building data by type
        /// </summary>
        public BuildingData GetBuildingData(BuildingType buildingType)
        {
            if (buildingLookup == null)
                RefreshLookupCache();

            buildingLookup.TryGetValue(buildingType, out BuildingData data);
            return data;
        }

        /// <summary>
        /// Get all building data
        /// </summary>
        public List<BuildingData> GetAllBuildings()
        {
            return new List<BuildingData>(buildings);
        }

        /// <summary>
        /// Check if building type exists in database
        /// </summary>
        public bool HasBuildingType(BuildingType buildingType)
        {
            if (buildingLookup == null)
                RefreshLookupCache();

            return buildingLookup.ContainsKey(buildingType);
        }

        /// <summary>
        /// Get buildings by category or filter
        /// </summary>
        public List<BuildingData> GetBuildingsByCategory(BuildingCategory category)
        {
            List<BuildingData> filteredBuildings = new List<BuildingData>();
            
            foreach (var building in buildings)
            {
                if (building.category == category)
                {
                    filteredBuildings.Add(building);
                }
            }
            
            return filteredBuildings;
        }

        /// <summary>
        /// Get buildings that can be unlocked at a specific tier
        /// </summary>
        public List<BuildingData> GetBuildingsForTier(int tier)
        {
            List<BuildingData> tierBuildings = new List<BuildingData>();
            
            foreach (var building in buildings)
            {
                if (building.requiredTier <= tier)
                {
                    tierBuildings.Add(building);
                }
            }
            
            return tierBuildings;
        }

        private void RefreshLookupCache()
        {
            buildingLookup = new Dictionary<BuildingType, BuildingData>();
            
            foreach (var building in buildings)
            {
                if (building != null && !buildingLookup.ContainsKey(building.buildingType))
                {
                    buildingLookup[building.buildingType] = building;
                }
            }
        }

        private void ValidateBuildingData()
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                var building = buildings[i];
                if (building == null) continue;

                // Auto-fill missing values with defaults
                if (building.constructionTime <= 0)
                    building.constructionTime = defaultConstructionTime;
                
                if (building.maxHealth <= 0)
                    building.maxHealth = defaultMaxHealth;
                
                if (building.operationalRange <= 0)
                    building.operationalRange = defaultOperationalRange;
                
                if (building.resourceGenerationRate <= 0 && building.buildingType == BuildingType.ResourceGenerator)
                    building.resourceGenerationRate = defaultResourceGenerationRate;

                // Validate resource costs
                if (building.resourceCosts == null)
                    building.resourceCosts = new ResourceCost[0];

                // Set default name if empty
                if (string.IsNullOrEmpty(building.name))
                    building.name = building.buildingType.ToString();
            }
        }

        #region Editor Utilities

#if UNITY_EDITOR
        [ContextMenu("Create Default Buildings")]
        public void CreateDefaultBuildings()
        {
            buildings.Clear();
            
            // Command Center
            buildings.Add(CreateDefaultBuildingData(BuildingType.CommandCenter, BuildingCategory.Core,
                "Main command center", 1, 200f, 0f, 8f, null,
                new ResourceCost[] { new ResourceCost(ResourceType.Metal, 500) }));
            
            // Barracks
            buildings.Add(CreateDefaultBuildingData(BuildingType.Barracks, BuildingCategory.Military,
                "Trains military units", 1, 150f, 0f, 6f, null,
                new ResourceCost[]
                {
                    new ResourceCost(ResourceType.Metal, 200),
                    new ResourceCost(ResourceType.Energy, 100)
                }));
            
            // Factory
            buildings.Add(CreateDefaultBuildingData(BuildingType.Factory, BuildingCategory.Production,
                "Produces advanced units and equipment", 2, 120f, 0f, 5f, null,
                new ResourceCost[]
                {
                    new ResourceCost(ResourceType.Metal, 300),
                    new ResourceCost(ResourceType.Energy, 200)
                }));
            
            // Resource Generator
            buildings.Add(CreateDefaultBuildingData(BuildingType.ResourceGenerator, BuildingCategory.Economy, 
                "Generates resources over time", 1, 100f, 2f, 4f, ResourceType.Metal,
                new ResourceCost[] { new ResourceCost(ResourceType.Metal, 100) }));
            
            // Defense Tower
            buildings.Add(CreateDefaultBuildingData(BuildingType.DefenseTower, BuildingCategory.Defense, 
                "Defends against enemy units", 1, 80f, 0f, 10f, null,
                new ResourceCost[] 
                { 
                    new ResourceCost(ResourceType.Metal, 150),
                    new ResourceCost(ResourceType.Energy, 50)
                }));
            
            // Research Lab
            buildings.Add(CreateDefaultBuildingData(BuildingType.ResearchLab, BuildingCategory.Research, 
                "Researches new technologies", 2, 90f, 0f, 6f, null,
                new ResourceCost[] 
                { 
                    new ResourceCost(ResourceType.Metal, 250),
                    new ResourceCost(ResourceType.Energy, 150)
                }));

            RefreshLookupCache();
            
            Debug.Log("[BuildingDatabase] Created default building data");
        }

        private BuildingData CreateDefaultBuildingData(BuildingType type, BuildingCategory category, 
            string description, int tier, float health, float genRate, float range, 
            ResourceType? generatedResource, ResourceCost[] costs)
        {
            return new BuildingData
            {
                name = type.ToString(),
                buildingType = type,
                category = category,
                description = description,
                requiredTier = tier,
                constructionTime = defaultConstructionTime,
                maxHealth = health,
                operationalRange = range,
                resourceGenerationRate = genRate,
                generatedResourceType = generatedResource,
                resourceCosts = costs ?? new ResourceCost[0],
                size = 2f,
                prefab = null // This needs to be assigned manually in the inspector
            };
        }

        [ContextMenu("Validate All Data")]
        public void ValidateAllData()
        {
            ValidateBuildingData();
            RefreshLookupCache();
            Debug.Log($"[BuildingDatabase] Validated {buildings.Count} building entries");
        }
#endif

        #endregion
    }

    // Enhanced BuildingData with additional properties
    [System.Serializable]
    public class BuildingData
    {
        [Header("Basic Info")]
        public string name;
        public BuildingType buildingType;
        public BuildingCategory category = BuildingCategory.Core;
        public string description;
        
        [Header("Visual")]
        public GameObject prefab;
        public Sprite icon;
        public Material constructionMaterial;
        
        [Header("Construction")]
        public float constructionTime = 5f;
        public float size = 2f;
        public int requiredTier = 1;
        public BuildingType[] prerequisites;
        
        [Header("Gameplay")]
        public float maxHealth = 100f;
        public float operationalRange = 5f;
        public bool canBeDestroyed = true;
        public bool providesPopulation = false;
        public int populationProvided = 0;
        
        [Header("Resources")]
        public ResourceCost[] resourceCosts;
        public float resourceGenerationRate = 0f;
        public ResourceType? generatedResourceType;
        public ResourceCost[] maintenanceCosts;
        
        [Header("Special Abilities")]
        public bool hasSpecialAbility = false;
        public string specialAbilityName;
        public string specialAbilityDescription;
        public float specialAbilityCooldown = 30f;
    }

    public enum BuildingCategory
    {
        Core,        // Command centers, main buildings
        Economy,     // Resource generators, refineries
        Military,    // Barracks, training facilities
        Defense,     // Towers, walls, shields
        Production,  // Factories, workshops
        Research,    // Labs, tech centers
        Support,     // Supply depots, power plants
        Special      // Unique buildings
    }
}