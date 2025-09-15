using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Strategy.Resources;

namespace asterivo.Unity60.Features.Templates.Strategy.Buildings
{
    /// <summary>
    /// Individual strategy building component
    /// Handles building-specific behavior, construction, and operations
    /// </summary>
    public class StrategyBuilding : MonoBehaviour
    {
        [Header("Building Configuration")]
        [SerializeField] private BuildingData buildingData;
        [SerializeField] private Transform constructionProgressIndicator;
        [SerializeField] private GameObject constructionEffects;
        [SerializeField] private GameObject completionEffects;

        [Header("Building State")]
        [SerializeField] private bool isConstructed = false;
        [SerializeField] private bool isOperational = true;
        [SerializeField] private float currentHealth;
        [SerializeField] private float constructionProgress = 0f;

        // Construction
        private float constructionStartTime;
        private bool isUnderConstruction = false;
        
        // References
        private StrategyBuildingManager buildingManager;
        private StrategyResourceManager resourceManager;

        // Events
        [Header("Events")]
        [SerializeField] private GameEvent<BuildingHealthChangedEventData> onHealthChanged;
        [SerializeField] private GameEvent<BuildingOperationalStateChangedEventData> onOperationalStateChanged;

        public BuildingType BuildingType => buildingData?.buildingType ?? BuildingType.CommandCenter;
        public bool IsConstructed => isConstructed;
        public bool IsOperational => isOperational;
        public float ConstructionProgress => constructionProgress;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => buildingData?.maxHealth ?? 100f;
        public BuildingData Data => buildingData;

        private void Start()
        {
            if (buildingData != null)
            {
                currentHealth = buildingData.maxHealth;
            }

            // Find resource manager
            resourceManager = FindObjectOfType<StrategyResourceManager>();
        }

        private void Update()
        {
            if (isUnderConstruction)
            {
                UpdateConstruction();
            }

            if (isConstructed && isOperational)
            {
                UpdateBuildingOperations();
            }
        }

        public void Initialize(BuildingData data, StrategyBuildingManager manager)
        {
            buildingData = data;
            buildingManager = manager;
            currentHealth = data.maxHealth;
            
            StartConstruction();
            
            Debug.Log($"[StrategyBuilding] Initialized {data.buildingType} building");
        }

        #region Construction System

        private void StartConstruction()
        {
            isUnderConstruction = true;
            isConstructed = false;
            constructionStartTime = Time.time;
            constructionProgress = 0f;

            // Enable construction effects
            if (constructionEffects != null)
                constructionEffects.SetActive(true);

            // Disable building functionality during construction
            SetOperationalState(false);

            UpdateConstructionVisuals();
        }

        private void UpdateConstruction()
        {
            if (buildingData == null) return;

            float elapsedTime = Time.time - constructionStartTime;
            constructionProgress = Mathf.Clamp01(elapsedTime / buildingData.constructionTime);

            UpdateConstructionVisuals();

            if (constructionProgress >= 1f)
            {
                CompleteConstructionInternal();
            }
        }

        public bool IsConstructionComplete()
        {
            return constructionProgress >= 1f;
        }

        public void CompleteConstruction()
        {
            CompleteConstructionInternal();
        }

        private void CompleteConstructionInternal()
        {
            isUnderConstruction = false;
            isConstructed = true;
            constructionProgress = 1f;

            // Disable construction effects
            if (constructionEffects != null)
                constructionEffects.SetActive(false);

            // Enable completion effects
            if (completionEffects != null)
            {
                completionEffects.SetActive(true);
                // Disable after a delay
                Invoke(nameof(DisableCompletionEffects), 2f);
            }

            // Enable building functionality
            SetOperationalState(true);

            UpdateConstructionVisuals();

            Debug.Log($"[StrategyBuilding] Construction completed: {BuildingType}");
        }

        private void DisableCompletionEffects()
        {
            if (completionEffects != null)
                completionEffects.SetActive(false);
        }

        private void UpdateConstructionVisuals()
        {
            // Update progress indicator
            if (constructionProgressIndicator != null)
            {
                constructionProgressIndicator.localScale = Vector3.one * constructionProgress;
            }

            // Update building transparency or other visual effects
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color color = renderer.material.color;
                    color.a = isConstructed ? 1f : 0.5f + (constructionProgress * 0.5f);
                    renderer.material.color = color;
                }
            }
        }

        #endregion

        #region Building Operations

        private void UpdateBuildingOperations()
        {
            if (!isOperational || buildingData == null) return;

            // Handle building-specific operations based on type
            switch (buildingData.buildingType)
            {
                case BuildingType.ResourceGenerator:
                    UpdateResourceGeneration();
                    break;
                case BuildingType.Barracks:
                    UpdateUnitProduction();
                    break;
                case BuildingType.DefenseTower:
                    UpdateDefenseOperations();
                    break;
                case BuildingType.ResearchLab:
                    UpdateResearchOperations();
                    break;
            }
        }

        private void UpdateResourceGeneration()
        {
            // Generate resources over time
            if (resourceManager != null && buildingData.resourceGenerationRate > 0)
            {
                float deltaTime = Time.deltaTime;
                float resourceAmount = buildingData.resourceGenerationRate * deltaTime;
                
                // For simplicity, generate the primary resource type
                if (buildingData.generatedResourceType != null)
                {
                    resourceManager.AddResource(buildingData.generatedResourceType.Value, Mathf.RoundToInt(resourceAmount));
                }
            }
        }

        private void UpdateUnitProduction()
        {
            // Handle unit production logic
            // This would integrate with a unit production system
        }

        private void UpdateDefenseOperations()
        {
            // Handle defense tower targeting and shooting
            // This would integrate with a combat system
        }

        private void UpdateResearchOperations()
        {
            // Handle research progression
            // This would integrate with a research system
        }

        #endregion

        #region Health and Damage System

        public void TakeDamage(float damage)
        {
            if (!isConstructed) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);

            onHealthChanged?.Raise(new BuildingHealthChangedEventData
            {
                building = this,
                previousHealth = previousHealth,
                currentHealth = currentHealth,
                maxHealth = MaxHealth,
                damageTaken = damage
            });

            if (currentHealth <= 0)
            {
                DestroyBuilding();
            }

            Debug.Log($"[StrategyBuilding] {BuildingType} took {damage} damage. Health: {currentHealth}/{MaxHealth}");
        }

        public void Repair(float repairAmount)
        {
            if (!isConstructed) return;

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + repairAmount);

            onHealthChanged?.Raise(new BuildingHealthChangedEventData
            {
                building = this,
                previousHealth = oldHealth,
                currentHealth = currentHealth,
                maxHealth = MaxHealth,
                damageTaken = -(currentHealth - oldHealth) // Negative damage = healing
            });

            Debug.Log($"[StrategyBuilding] {BuildingType} repaired by {repairAmount}. Health: {currentHealth}/{MaxHealth}");
        }

        public float GetHealthPercentage()
        {
            return MaxHealth > 0 ? currentHealth / MaxHealth : 0f;
        }

        #endregion

        #region Operational State Management

        public void SetOperationalState(bool operational)
        {
            bool previousState = isOperational;
            isOperational = operational;

            if (previousState != isOperational)
            {
                onOperationalStateChanged?.Raise(new BuildingOperationalStateChangedEventData
                {
                    building = this,
                    previousState = previousState,
                    currentState = isOperational
                });

                Debug.Log($"[StrategyBuilding] {BuildingType} operational state changed to: {isOperational}");
            }
        }

        public void ToggleOperationalState()
        {
            SetOperationalState(!isOperational);
        }

        #endregion

        #region Destruction

        private void DestroyBuilding()
        {
            Debug.Log($"[StrategyBuilding] {BuildingType} destroyed");
            
            // Notify building manager
            if (buildingManager != null)
            {
                buildingManager.DestroyBuilding(this);
            }
            else
            {
                // Fallback if no manager reference
                Destroy(gameObject);
            }
        }

        public void SelfDestruct()
        {
            DestroyBuilding();
        }

        #endregion

        #region Utility Methods

        public Vector3 GetBuildingCenter()
        {
            return transform.position;
        }

        public Bounds GetBuildingBounds()
        {
            var collider = GetComponent<Collider>();
            return collider != null ? collider.bounds : new Bounds(transform.position, Vector3.one);
        }

        public bool IsWithinRange(Vector3 position, float range)
        {
            return Vector3.Distance(transform.position, position) <= range;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (buildingData != null)
            {
                // Draw building range/influence area
                Gizmos.color = isOperational ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, buildingData.operationalRange);
                
                // Draw construction progress
                if (isUnderConstruction)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, 
                        Vector3.one * constructionProgress);
                }
            }
        }
    }

}