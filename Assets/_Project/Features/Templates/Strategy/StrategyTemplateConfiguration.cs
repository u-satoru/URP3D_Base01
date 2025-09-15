using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using System.Collections.Generic;
using asterivo.Unity60.Features.Templates.Strategy.Camera;
using asterivo.Unity60.Features.Templates.Strategy.Units;
using asterivo.Unity60.Features.Templates.Strategy.Resources;
using asterivo.Unity60.Features.Templates.Strategy.Buildings;

namespace asterivo.Unity60.Features.Templates.Strategy
{
    /// <summary>
    /// Strategy（RTS/ストラテジー）ジャンル特化テンプレート設定
    /// リアルタイムストラテジー要素による15分間のゲームプレイ
    /// ユニット管理、リソース管理、建設システムを含む
    /// </summary>
    [CreateAssetMenu(fileName = "StrategyTemplateConfiguration", menuName = "Templates/Strategy/Configuration")]
    public class StrategyTemplateConfiguration : ScriptableObject
    {
        [Header("Strategy Core Settings")]
        [SerializeField] private bool enableRTSCamera = true;
        [SerializeField] private float cameraHeight = 15f;
        [SerializeField] private float cameraAngle = 45f;
        [SerializeField] private float cameraMoveSpeed = 10f;
        [SerializeField] private float cameraZoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 25f;

        [Header("Unit Selection System")]
        [SerializeField] private bool enableUnitSelection = true;
        [SerializeField] private bool enableMultiSelection = true;
        [SerializeField] private int maxSelectedUnits = 20;
        [SerializeField] private LayerMask unitLayerMask = 1;
        [SerializeField] private LayerMask groundLayerMask = 1;

        [Header("Resource Management")]
        [SerializeField] private bool enableResourceSystem = true;
        [SerializeField] private int startingGold = 1000;
        [SerializeField] private int startingWood = 500;
        [SerializeField] private int startingStone = 300;
        [SerializeField] private int startingFood = 200;
        [SerializeField] private int maxPopulation = 100;

        [Header("Building System")]
        [SerializeField] private bool enableBuildingSystem = true;
        [SerializeField] private bool enableBuildingUpgrades = false;
        [SerializeField] private float buildTime = 5f;
        [SerializeField] private bool requireWorkers = true;

        [Header("Combat System")]
        [SerializeField] private bool enableCombatSystem = true;
        [SerializeField] private float baseDamage = 20f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private float attackCooldown = 1.5f;

        [Header("Minimap Settings")]
        [SerializeField] private bool enableMinimap = true;
        [SerializeField] private float minimapSize = 200f;
        [SerializeField] private bool enableMinimapClick = true;
        [SerializeField] private Color playerColor = Color.blue;
        [SerializeField] private Color enemyColor = Color.red;
        [SerializeField] private Color neutralColor = Color.gray;

        [Header("AI Settings")]
        [SerializeField] private bool enableAI = true;
        [SerializeField] private float aiDecisionInterval = 2f;
        [SerializeField] private int aiMaxUnits = 15;
        [SerializeField] private float aiAggression = 0.5f; // 0 = Defensive, 1 = Aggressive

        [Header("15-Minute Learning Objectives")]
        [SerializeField] private List<StrategyLearningObjective> learningObjectives = new List<StrategyLearningObjective>();

        [Header("Unit Types")]
        [SerializeField] private List<StrategyUnitType> availableUnits = new List<StrategyUnitType>();

        [Header("Building Types")]
        [SerializeField] private List<StrategyBuildingType> availableBuildings = new List<StrategyBuildingType>();

        [Header("Audio Settings")]
        [SerializeField] private AudioClip[] unitSelectSounds;
        [SerializeField] private AudioClip[] unitMoveSounds;
        [SerializeField] private AudioClip[] buildingSounds;
        [SerializeField] private AudioClip[] combatSounds;
        [SerializeField] private float masterVolume = 1.0f;

        [Header("Events")]
        [SerializeField] private GameEvent onStrategyGameStarted;
        [SerializeField] private GameEvent onUnitSelected;
        [SerializeField] private GameEvent onUnitMoved;
        [SerializeField] private GameEvent onBuildingPlaced;
        [SerializeField] private GameEvent onResourcesChanged;
        [SerializeField] private GameEvent onObjectiveCompleted;

        // Properties
        public float CameraHeight => cameraHeight;
        public float CameraAngle => cameraAngle;
        public float CameraMoveSpeed => cameraMoveSpeed;
        public float CameraZoomSpeed => cameraZoomSpeed;
        public float MinZoom => minZoom;
        public float MaxZoom => maxZoom;
        public LayerMask UnitLayerMask => unitLayerMask;
        public LayerMask GroundLayerMask => groundLayerMask;
        public int MaxSelectedUnits => maxSelectedUnits;
        public bool EnableMultiSelection => enableMultiSelection;
        public bool EnableResourceSystem => enableResourceSystem;
        public bool EnableBuildingSystem => enableBuildingSystem;
        public bool EnableMinimap => enableMinimap;

        /// <summary>
        /// Strategyカメラシステムの初期設定
        /// </summary>
        public void SetupRTSCamera(UnityEngine.Camera mainCamera)
        {
            if (mainCamera == null || !enableRTSCamera) return;

            // RTS Camera Controller
            if (mainCamera.GetComponent<StrategyCameraController>() == null)
            {
                var cameraController = mainCamera.gameObject.AddComponent<StrategyCameraController>();
                // StrategyCameraController initializes automatically in its Awake/Start methods
            }

            // Set initial position and rotation
            mainCamera.transform.position = new Vector3(0, cameraHeight, -cameraHeight * 0.5f);
            mainCamera.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
            mainCamera.orthographic = false; // Perspective for better RTS feel
            mainCamera.fieldOfView = 60f;

            Debug.Log($"[StrategyTemplate] RTS Camera setup completed");
        }

        /// <summary>
        /// ユニット選択システムの設定
        /// </summary>
        public void SetupUnitSelectionSystem()
        {
            if (!enableUnitSelection) return;

            // Find or create Unit Selection Manager
            var selectionManager = FindObjectOfType<StrategyUnitSelectionManager>();
            if (selectionManager == null)
            {
                var managerGO = new GameObject("UnitSelectionManager");
                selectionManager = managerGO.AddComponent<StrategyUnitSelectionManager>();
                selectionManager.Initialize();
            }

            // Setup selection UI
            SetupSelectionUI();

            Debug.Log($"[StrategyTemplate] Unit selection system setup completed with max {maxSelectedUnits} units");
        }

        /// <summary>
        /// 選択システムUIの設定
        /// </summary>
        private void SetupSelectionUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Selection Info Panel
            var selectionPanel = canvas.transform.Find("SelectionInfoPanel");
            if (selectionPanel == null)
            {
                var panelGO = new GameObject("SelectionInfoPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 0f);
                rectTransform.anchorMax = new Vector2(0.4f, 0.25f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0, 0, 0, 0.7f);

                // Unit Info Text
                var infoGO = new GameObject("UnitInfoText");
                infoGO.transform.SetParent(panelGO.transform);

                var infoRect = infoGO.AddComponent<RectTransform>();
                infoRect.anchorMin = Vector2.zero;
                infoRect.anchorMax = Vector2.one;
                infoRect.offsetMin = new Vector2(10, 10);
                infoRect.offsetMax = new Vector2(-10, -10);

                var infoText = infoGO.AddComponent<UnityEngine.UI.Text>();
                infoText.text = "Select units to see information";
                infoText.color = Color.white;
                infoText.fontSize = 14;
                infoText.alignment = TextAnchor.UpperLeft;

                panelGO.SetActive(true);
            }
        }

        /// <summary>
        /// リソース管理システムの設定
        /// </summary>
        public void SetupResourceSystem()
        {
            if (!enableResourceSystem) return;

            // Find or create Resource Manager
            var resourceManager = FindObjectOfType<StrategyResourceManager>();
            if (resourceManager == null)
            {
                var managerGO = new GameObject("ResourceManager");
                resourceManager = managerGO.AddComponent<StrategyResourceManager>();
                resourceManager.Initialize(this);
            }

            // Initialize starting resources
            resourceManager.SetResource(ResourceType.Gold, startingGold);
            resourceManager.SetResource(ResourceType.Wood, startingWood);
            resourceManager.SetResource(ResourceType.Stone, startingStone);
            resourceManager.SetResource(ResourceType.Food, startingFood);

            // Setup resource UI
            SetupResourceUI();

            Debug.Log($"[StrategyTemplate] Resource system setup completed - Gold: {startingGold}, Wood: {startingWood}, Stone: {startingStone}, Food: {startingFood}");
        }

        /// <summary>
        /// リソースUIの設定
        /// </summary>
        private void SetupResourceUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Resource Panel
            var resourcePanel = canvas.transform.Find("ResourcePanel");
            if (resourcePanel == null)
            {
                var panelGO = new GameObject("ResourcePanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 0.9f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

                // Resource Text
                var resourceTextGO = new GameObject("ResourceText");
                resourceTextGO.transform.SetParent(panelGO.transform);

                var textRect = resourceTextGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(20, 5);
                textRect.offsetMax = new Vector2(-20, -5);

                var resourceText = resourceTextGO.AddComponent<UnityEngine.UI.Text>();
                resourceText.text = $"Gold: {startingGold}  Wood: {startingWood}  Stone: {startingStone}  Food: {startingFood}  Population: 0/{maxPopulation}";
                resourceText.color = Color.white;
                resourceText.fontSize = 16;
                resourceText.alignment = TextAnchor.MiddleLeft;

                panelGO.SetActive(true);
            }
        }

        /// <summary>
        /// 建設システムの設定
        /// </summary>
        public void SetupBuildingSystem()
        {
            if (!enableBuildingSystem) return;

            // Find or create Building Manager
            var buildingManager = FindObjectOfType<StrategyBuildingManager>();
            if (buildingManager == null)
            {
                var managerGO = new GameObject("BuildingManager");
                buildingManager = managerGO.AddComponent<StrategyBuildingManager>();
                buildingManager.Initialize(this);
            }

            // Initialize default buildings
            InitializeDefaultBuildings();

            // Setup building UI
            SetupBuildingUI();

            Debug.Log($"[StrategyTemplate] Building system setup completed with {availableBuildings.Count} building types");
        }

        /// <summary>
        /// 建設UIの設定
        /// </summary>
        private void SetupBuildingUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Building Panel
            var buildingPanel = canvas.transform.Find("BuildingPanel");
            if (buildingPanel == null)
            {
                var panelGO = new GameObject("BuildingPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.8f, 0.3f);
                rectTransform.anchorMax = new Vector2(1f, 0.9f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                // Building buttons grid
                CreateBuildingGrid(panelGO);

                panelGO.SetActive(true);
            }
        }

        /// <summary>
        /// 建設ボタングリッドの作成
        /// </summary>
        private void CreateBuildingGrid(GameObject parent)
        {
            var gridGO = new GameObject("BuildingGrid");
            gridGO.transform.SetParent(parent.transform);

            var rectTransform = gridGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);

            var gridLayout = gridGO.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(80, 80);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2; // 2 columns

            // Create building buttons (placeholder)
            for (int i = 0; i < 6; i++)
            {
                var buttonGO = new GameObject($"BuildingButton_{i}");
                buttonGO.transform.SetParent(gridGO.transform);

                var button = buttonGO.AddComponent<UnityEngine.UI.Button>();
                var buttonImage = button.GetComponent<UnityEngine.UI.Image>();
                buttonImage.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);

                // Button text
                var textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform);

                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var buttonText = textGO.AddComponent<UnityEngine.UI.Text>();
                buttonText.text = $"Build {i + 1}";
                buttonText.color = Color.white;
                buttonText.fontSize = 10;
                buttonText.alignment = TextAnchor.MiddleCenter;
            }
        }

        /// <summary>
        /// ミニマップシステムの設定
        /// </summary>
        public void SetupMinimapSystem()
        {
            if (!enableMinimap) return;

            // TODO: Implement StrategyMinimapManager
            // Find or create Minimap Manager
            // var minimapManager = FindObjectOfType<StrategyMinimapManager>();
            // if (minimapManager == null)
            // {
            //     var managerGO = new GameObject("MinimapManager");
            //     minimapManager = managerGO.AddComponent<StrategyMinimapManager>();
            //     minimapManager.Initialize(this);
            // }

            // Setup minimap UI
            SetupMinimapUI();

            Debug.Log($"[StrategyTemplate] Minimap system setup completed with size {minimapSize}");
        }

        /// <summary>
        /// ミニマップUIの設定
        /// </summary>
        private void SetupMinimapUI()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Minimap Panel
            var minimapPanel = canvas.transform.Find("MinimapPanel");
            if (minimapPanel == null)
            {
                var panelGO = new GameObject("MinimapPanel");
                panelGO.transform.SetParent(canvas.transform);
                
                var rectTransform = panelGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.anchoredPosition = new Vector2(-minimapSize/2 - 10, -minimapSize/2 - 10);
                rectTransform.sizeDelta = new Vector2(minimapSize, minimapSize);

                var panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
                panelImage.color = new Color(0, 0, 0, 0.8f);

                // Minimap RawImage (for minimap camera)
                var minimapImageGO = new GameObject("MinimapImage");
                minimapImageGO.transform.SetParent(panelGO.transform);

                var imageRect = minimapImageGO.AddComponent<RectTransform>();
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = new Vector2(5, 5);
                imageRect.offsetMax = new Vector2(-5, -5);

                var minimapRawImage = minimapImageGO.AddComponent<UnityEngine.UI.RawImage>();
                minimapRawImage.color = Color.white;

                panelGO.SetActive(true);
            }
        }

        /// <summary>
        /// 学習目標の初期化と追跡
        /// </summary>
        public void InitializeLearningObjectives()
        {
            if (learningObjectives.Count == 0)
            {
                CreateDefaultLearningObjectives();
            }

            foreach (var objective in learningObjectives)
            {
                objective.Initialize();
                Debug.Log($"[StrategyTemplate] Learning objective initialized: {objective.Title}");
            }
        }

        /// <summary>
        /// デフォルト学習目標の作成（15分間の段階的学習）
        /// </summary>
        private void CreateDefaultLearningObjectives()
        {
            learningObjectives.Clear();
            
            // Phase 1: Camera & Basic Controls (0-3 minutes)
            learningObjectives.Add(new StrategyLearningObjective
            {
                Id = "strategy_camera_controls",
                Title = "RTSカメラ操作とUI",
                Description = "カメラ移動、ズーム、UI要素の確認",
                EstimatedTime = 180f,
                Priority = 1
            });

            // Phase 2: Unit Selection & Commands (3-6 minutes)
            learningObjectives.Add(new StrategyLearningObjective
            {
                Id = "strategy_unit_control",
                Title = "ユニット選択と操作",
                Description = "単体・複数選択、移動命令、攻撃命令",
                EstimatedTime = 180f,
                Priority = 2
            });

            // Phase 3: Resource Management (6-9 minutes)
            learningObjectives.Add(new StrategyLearningObjective
            {
                Id = "strategy_resource_management",
                Title = "リソース管理システム",
                Description = "リソース収集、管理、人口制限の理解",
                EstimatedTime = 180f,
                Priority = 3
            });

            // Phase 4: Base Building (9-12 minutes)
            learningObjectives.Add(new StrategyLearningObjective
            {
                Id = "strategy_base_building",
                Title = "基地建設システム",
                Description = "建物配置、生産、基地拡張の基礎",
                EstimatedTime = 180f,
                Priority = 4
            });

            // Phase 5: Strategic Gameplay (12-15 minutes)
            learningObjectives.Add(new StrategyLearningObjective
            {
                Id = "strategy_tactical_play",
                Title = "戦略的ゲームプレイ",
                Description = "総合戦略、AI対戦、勝利条件の達成",
                EstimatedTime = 180f,
                Priority = 5
            });

            Debug.Log($"[StrategyTemplate] Created {learningObjectives.Count} default learning objectives");
        }

        /// <summary>
        /// デフォルト建物タイプの初期化
        /// </summary>
        private void InitializeDefaultBuildings()
        {
            if (availableBuildings.Count == 0)
            {
                availableBuildings.Add(new StrategyBuildingType
                {
                    buildingId = "town_hall",
                    buildingName = "Town Hall",
                    goldCost = 0,
                    woodCost = 0,
                    stoneCost = 0,
                    buildTime = 0f,
                    maxHealth = 500f,
                    canProduce = true
                });

                availableBuildings.Add(new StrategyBuildingType
                {
                    buildingId = "barracks",
                    buildingName = "Barracks",
                    goldCost = 200,
                    woodCost = 150,
                    stoneCost = 0,
                    buildTime = 10f,
                    maxHealth = 300f,
                    canProduce = true
                });

                availableBuildings.Add(new StrategyBuildingType
                {
                    buildingId = "resource_depot",
                    buildingName = "Resource Depot",
                    goldCost = 100,
                    woodCost = 200,
                    stoneCost = 50,
                    buildTime = 8f,
                    maxHealth = 200f,
                    canProduce = false
                });
            }
        }

        /// <summary>
        /// ゲームプレイイベントの発行
        /// </summary>
        public void RaiseGameplayEvent(string eventType)
        {
            switch (eventType.ToLower())
            {
                case "started":
                    onStrategyGameStarted?.Raise();
                    break;
                case "unit_selected":
                    onUnitSelected?.Raise();
                    break;
                case "unit_moved":
                    onUnitMoved?.Raise();
                    break;
                case "building_placed":
                    onBuildingPlaced?.Raise();
                    break;
                case "resources_changed":
                    onResourcesChanged?.Raise();
                    break;
                case "objective_completed":
                    onObjectiveCompleted?.Raise();
                    break;
                default:
                    Debug.LogWarning($"[StrategyTemplate] Unknown gameplay event: {eventType}");
                    break;
            }
        }

        /// <summary>
        /// Strategy設定の検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (cameraHeight <= 0f)
            {
                Debug.LogError("[StrategyTemplate] Camera height must be greater than 0");
                isValid = false;
            }

            if (cameraMoveSpeed <= 0f)
            {
                Debug.LogError("[StrategyTemplate] Camera move speed must be greater than 0");
                isValid = false;
            }

            if (enableUnitSelection && maxSelectedUnits <= 0)
            {
                Debug.LogError("[StrategyTemplate] Max selected units must be greater than 0");
                isValid = false;
            }

            if (enableResourceSystem)
            {
                if (startingGold < 0 || startingWood < 0 || startingStone < 0 || startingFood < 0)
                {
                    Debug.LogError("[StrategyTemplate] Starting resources cannot be negative");
                    isValid = false;
                }

                if (maxPopulation <= 0)
                {
                    Debug.LogError("[StrategyTemplate] Max population must be greater than 0");
                    isValid = false;
                }
            }

            if (enableMinimap && minimapSize <= 0f)
            {
                Debug.LogError("[StrategyTemplate] Minimap size must be greater than 0");
                isValid = false;
            }

            if (learningObjectives.Count == 0)
            {
                Debug.LogWarning("[StrategyTemplate] No learning objectives defined");
            }

            return isValid;
        }
    }

    /// <summary>
    /// Strategy学習目標データ構造
    /// </summary>
    [System.Serializable]
    public class StrategyLearningObjective
    {
        public string Id;
        public string Title;
        [TextArea(2, 4)]
        public string Description;
        public float EstimatedTime;
        public int Priority;
        public bool IsCompleted;
        public float CompletionPercentage;

        public void Initialize()
        {
            IsCompleted = false;
            CompletionPercentage = 0f;
        }

        public void UpdateProgress(float progress)
        {
            CompletionPercentage = Mathf.Clamp01(progress);
            if (CompletionPercentage >= 1.0f && !IsCompleted)
            {
                IsCompleted = true;
                Debug.Log($"[StrategyTemplate] Learning objective completed: {Title}");
            }
        }
    }

    /// <summary>
    /// Strategyユニットタイプデータ構造
    /// </summary>
    [System.Serializable]
    public class StrategyUnitType
    {
        public string unitId;
        public string unitName;
        public int goldCost;
        public int foodCost;
        public float buildTime;
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackRange;
        public bool canAttack;
        public bool canBuild;
    }

    /// <summary>
    /// Strategy建物タイプデータ構造
    /// </summary>
    [System.Serializable]
    public class StrategyBuildingType
    {
        public string buildingId;
        public string buildingName;
        public int goldCost;
        public int woodCost;
        public int stoneCost;
        public float buildTime;
        public float maxHealth;
        public bool canProduce;
        public List<string> producibleUnits = new List<string>();
    }
}