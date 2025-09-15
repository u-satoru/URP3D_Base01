using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Strategy.Resources;

namespace asterivo.Unity60.Features.Templates.Strategy.Buildings
{
    /// <summary>
    /// Strategy template building management system
    /// Handles building placement, construction, and management
    /// </summary>
    public class StrategyBuildingManager : MonoBehaviour
    {
        [Header("Building System Settings")]
        [SerializeField] private LayerMask buildableLayerMask = 1;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        [SerializeField] private GameObject buildingPreviewPrefab;

        [Header("Building Configuration")]
        [SerializeField] private BuildingDatabase buildingDatabase;
        [SerializeField] private float buildingSpacing = 2f;
        [SerializeField] private int maxBuildingsPerType = 10;

        // Building state management
        private Dictionary<BuildingType, List<StrategyBuilding>> placedBuildings = new();
        private Dictionary<BuildingType, Queue<StrategyBuilding>> constructionQueue = new();
        
        // Preview system
        private GameObject currentPreview;
        private BuildingType selectedBuildingType;
        private bool isPlacementMode = false;
        private UnityEngine.Camera playerCamera;
        
        // Events
        [Header("Events")]
        [SerializeField] private GameEvent<BuildingPlacedEventData> onBuildingPlaced;
        [SerializeField] private GameEvent<BuildingDestroyedEventData> onBuildingDestroyed;
        [SerializeField] private GameEvent<BuildingConstructionStartedEventData> onConstructionStarted;
        [SerializeField] private GameEvent<BuildingConstructionCompletedEventData> onConstructionCompleted;

        // Services
        private StrategyResourceManager resourceManager;
        private StrategyTemplateConfiguration templateConfig;

        public Dictionary<BuildingType, List<StrategyBuilding>> PlacedBuildings => placedBuildings;
        public bool IsPlacementMode => isPlacementMode;
        public BuildingType SelectedBuildingType => selectedBuildingType;

        private void Awake()
        {
            playerCamera = UnityEngine.Camera.main;
            if (playerCamera == null)
                playerCamera = FindFirstObjectByType<UnityEngine.Camera>();

            InitializeBuildingCollections();
        }

        private void Start()
        {
            resourceManager = ServiceLocator.GetService<StrategyResourceManager>();
            RegisterAsService();
        }

        private void Update()
        {
            if (isPlacementMode)
            {
                UpdateBuildingPreview();
                HandleBuildingPlacement();
            }

            ProcessConstructionQueue();
        }

        public void Initialize(StrategyTemplateConfiguration config)
        {
            templateConfig = config;
            
            if (buildingDatabase == null)
            {
                Debug.LogWarning("[StrategyBuildingManager] Building database not assigned, creating default");
                CreateDefaultBuildingDatabase();
            }

            Debug.Log("[StrategyBuildingManager] Building manager initialized");
        }

        private void InitializeBuildingCollections()
        {
            foreach (BuildingType buildingType in System.Enum.GetValues(typeof(BuildingType)))
            {
                placedBuildings[buildingType] = new List<StrategyBuilding>();
                constructionQueue[buildingType] = new Queue<StrategyBuilding>();
            }
        }

        #region Building Placement

        public bool StartBuildingPlacement(BuildingType buildingType)
        {
            if (!CanPlaceBuilding(buildingType))
            {
                Debug.LogWarning($"[StrategyBuildingManager] Cannot place building: {buildingType}");
                return false;
            }

            selectedBuildingType = buildingType;
            isPlacementMode = true;
            
            CreateBuildingPreview(buildingType);
            
            Debug.Log($"[StrategyBuildingManager] Started placement mode for {buildingType}");
            return true;
        }

        public void CancelBuildingPlacement()
        {
            isPlacementMode = false;
            
            if (currentPreview != null)
            {
                DestroyImmediate(currentPreview);
                currentPreview = null;
            }

            Debug.Log("[StrategyBuildingManager] Placement mode cancelled");
        }

        private void UpdateBuildingPreview()
        {
            if (currentPreview == null || playerCamera == null) return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildableLayerMask))
            {
                Vector3 buildingPosition = GetSnapPosition(hit.point);
                currentPreview.transform.position = buildingPosition;

                // Update preview material based on validity
                bool canPlace = IsValidBuildingPosition(buildingPosition, selectedBuildingType);
                UpdatePreviewMaterial(canPlace);
            }
        }

        private void HandleBuildingPlacement()
        {
            if (Input.GetMouseButtonDown(0)) // Left click to place
            {
                TryPlaceBuilding();
            }
            else if (Input.GetMouseButtonDown(1)) // Right click to cancel
            {
                CancelBuildingPlacement();
            }
        }

        private void TryPlaceBuilding()
        {
            if (currentPreview == null) return;

            Vector3 position = currentPreview.transform.position;
            if (IsValidBuildingPosition(position, selectedBuildingType))
            {
                PlaceBuilding(selectedBuildingType, position);
                CancelBuildingPlacement();
            }
        }

        private bool PlaceBuilding(BuildingType buildingType, Vector3 position)
        {
            var buildingData = GetBuildingData(buildingType);
            if (buildingData == null) return false;

            // Check resources
            if (!HasSufficientResources(buildingData))
            {
                Debug.LogWarning($"[StrategyBuildingManager] Insufficient resources for {buildingType}");
                return false;
            }

            // Create building
            GameObject buildingGO = Instantiate(buildingData.prefab, position, Quaternion.identity);
            var building = buildingGO.GetComponent<StrategyBuilding>();
            
            if (building == null)
            {
                building = buildingGO.AddComponent<StrategyBuilding>();
            }

            building.Initialize(buildingData, this);
            placedBuildings[buildingType].Add(building);

            // Consume resources
            ConsumeResources(buildingData);

            // Start construction
            StartConstruction(building);

            // Raise events
            onBuildingPlaced?.Raise(new BuildingPlacedEventData 
            { 
                buildingType = buildingType, 
                position = position,
                building = building
            });

            Debug.Log($"[StrategyBuildingManager] Placed {buildingType} at {position}");
            return true;
        }

        #endregion

        #region Building Management

        public void DestroyBuilding(StrategyBuilding building)
        {
            if (building == null) return;

            var buildingType = building.BuildingType;
            placedBuildings[buildingType].Remove(building);

            // Raise events
            onBuildingDestroyed?.Raise(new BuildingDestroyedEventData 
            { 
                buildingType = buildingType,
                position = building.transform.position,
                building = building
            });

            Destroy(building.gameObject);
            Debug.Log($"[StrategyBuildingManager] Destroyed {buildingType}");
        }

        public List<StrategyBuilding> GetBuildingsOfType(BuildingType buildingType)
        {
            return placedBuildings.ContainsKey(buildingType) ? placedBuildings[buildingType] : new List<StrategyBuilding>();
        }

        public int GetBuildingCount(BuildingType buildingType)
        {
            return placedBuildings.ContainsKey(buildingType) ? placedBuildings[buildingType].Count : 0;
        }

        public StrategyBuilding GetNearestBuilding(Vector3 position, BuildingType buildingType)
        {
            var buildings = GetBuildingsOfType(buildingType);
            StrategyBuilding nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var building in buildings)
            {
                if (building == null) continue;

                float distance = Vector3.Distance(position, building.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = building;
                }
            }

            return nearest;
        }

        #endregion

        #region Construction System

        private void StartConstruction(StrategyBuilding building)
        {
            constructionQueue[building.BuildingType].Enqueue(building);
            
            onConstructionStarted?.Raise(new BuildingConstructionStartedEventData 
            { 
                buildingType = building.BuildingType,
                building = building
            });
        }

        private void ProcessConstructionQueue()
        {
            foreach (var kvp in constructionQueue)
            {
                var queue = kvp.Value;
                
                while (queue.Count > 0)
                {
                    var building = queue.Peek();
                    
                    if (building == null)
                    {
                        queue.Dequeue();
                        continue;
                    }

                    if (building.IsConstructionComplete())
                    {
                        queue.Dequeue();
                        CompleteConstruction(building);
                    }
                    else
                    {
                        break; // Process one building at a time per type
                    }
                }
            }
        }

        private void CompleteConstruction(StrategyBuilding building)
        {
            building.CompleteConstruction();
            
            onConstructionCompleted?.Raise(new BuildingConstructionCompletedEventData 
            { 
                buildingType = building.BuildingType,
                building = building
            });

            Debug.Log($"[StrategyBuildingManager] Construction completed: {building.BuildingType}");
        }

        #endregion

        #region Helper Methods

        private bool CanPlaceBuilding(BuildingType buildingType)
        {
            var buildingData = GetBuildingData(buildingType);
            if (buildingData == null) return false;

            // Check building limit
            if (GetBuildingCount(buildingType) >= maxBuildingsPerType)
                return false;

            // Check resources
            if (!HasSufficientResources(buildingData))
                return false;

            return true;
        }

        private bool IsValidBuildingPosition(Vector3 position, BuildingType buildingType)
        {
            var buildingData = GetBuildingData(buildingType);
            if (buildingData == null) return false;

            // Check if position is on navmesh
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                return false;

            // Check for overlapping buildings
            Collider[] overlapping = Physics.OverlapSphere(position, buildingData.size, ~buildableLayerMask);
            return overlapping.Length == 0;
        }

        private Vector3 GetSnapPosition(Vector3 worldPosition)
        {
            // Snap to grid
            float snapSize = buildingSpacing;
            float snappedX = Mathf.Round(worldPosition.x / snapSize) * snapSize;
            float snappedZ = Mathf.Round(worldPosition.z / snapSize) * snapSize;
            
            return new Vector3(snappedX, worldPosition.y, snappedZ);
        }

        private void CreateBuildingPreview(BuildingType buildingType)
        {
            var buildingData = GetBuildingData(buildingType);
            if (buildingData == null || buildingData.prefab == null) return;

            currentPreview = Instantiate(buildingData.prefab);
            
            // Make it a preview (disable components, set transparent material, etc.)
            var colliders = currentPreview.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            var renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = validPlacementMaterial;
            }
        }

        private void UpdatePreviewMaterial(bool isValid)
        {
            if (currentPreview == null) return;

            Material materialToUse = isValid ? validPlacementMaterial : invalidPlacementMaterial;
            
            var renderers = currentPreview.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = materialToUse;
            }
        }

        private BuildingData GetBuildingData(BuildingType buildingType)
        {
            return buildingDatabase?.GetBuildingData(buildingType);
        }

        private bool HasSufficientResources(BuildingData buildingData)
        {
            if (resourceManager == null) return true; // Allow if no resource manager

            foreach (var cost in buildingData.resourceCosts)
            {
                if (resourceManager.GetResource(cost.ResourceType) < cost.Amount)
                    return false;
            }
            return true;
        }

        private void ConsumeResources(BuildingData buildingData)
        {
            if (resourceManager == null) return;

            foreach (var cost in buildingData.resourceCosts)
            {
                resourceManager.ConsumeResource(cost.ResourceType, cost.Amount);
            }
        }

        private void CreateDefaultBuildingDatabase()
        {
            // Create basic building database if none exists
            buildingDatabase = ScriptableObject.CreateInstance<BuildingDatabase>();
            
            Debug.Log("[StrategyBuildingManager] Created default building database");
        }

        private void RegisterAsService()
        {
            ServiceLocator.RegisterService<StrategyBuildingManager>(this);
            Debug.Log("[StrategyBuildingManager] Registered as service");
        }

        #endregion
    }

    // Note: BuildingData, BuildingType, and ResourceCost are defined in BuildingDatabase.cs

    // Event data structures
    [System.Serializable]
    public class BuildingPlacedEventData
    {
        public BuildingType buildingType;
        public Vector3 position;
        public StrategyBuilding building;
    }

    [System.Serializable]
    public class BuildingDestroyedEventData
    {
        public BuildingType buildingType;
        public Vector3 position;
        public StrategyBuilding building;
    }

    [System.Serializable]
    public class BuildingConstructionStartedEventData
    {
        public BuildingType buildingType;
        public StrategyBuilding building;
    }

    [System.Serializable]
    public class BuildingConstructionCompletedEventData
    {
        public BuildingType buildingType;
        public StrategyBuilding building;
    }
}