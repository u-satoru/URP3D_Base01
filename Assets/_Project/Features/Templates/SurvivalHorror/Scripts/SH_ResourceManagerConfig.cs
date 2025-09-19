using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート用リソース管理設定
    /// アイテム希少性、スポーン制御、経済バランスを管理
    /// </summary>
    [CreateAssetMenu(fileName = "SH_ResourceManagerConfig", menuName = "Templates/SurvivalHorror/Resource Manager Config")]
    public class SH_ResourceManagerConfig : ScriptableObject
    {
        [Header("Global Resource Settings")]
        [SerializeField] private string configVersion = "1.0.0";
        [SerializeField] private bool enableResourceTracking = true;
        [SerializeField] private bool allowResourceRespawn = false;
        [SerializeField] private float globalScarcityMultiplier = 1.0f;

        [Header("Health Resource Management")]
        [SerializeField] private ResourcePool healthResourcePool;
        [SerializeField] private float healthItemSpawnRate = 0.3f;
        [SerializeField] private int maxHealthItemsInScene = 5;
        [SerializeField] private bool enforceHealthScarcity = true;

        [Header("Ammunition Resource Management")]
        [SerializeField] private ResourcePool ammunitionResourcePool;
        [SerializeField] private float ammunitionSpawnRate = 0.2f;
        [SerializeField] private int maxAmmunitionInScene = 8;
        [SerializeField] private bool enforceAmmunitionScarcity = true;

        [Header("Save Resource Management")]
        [SerializeField] private ResourcePool saveResourcePool;
        [SerializeField] private float saveItemSpawnRate = 0.1f;
        [SerializeField] private int maxSaveItemsInScene = 2;
        [SerializeField] private bool limitSaveResources = true;

        [Header("Key Item Management")]
        [SerializeField] private bool keyItemsAreUnique = true;
        #pragma warning disable 0414
        [SerializeField] private bool preventKeyItemLoss = true;
        #pragma warning restore 0414
        [SerializeField] private float keyItemDiscoveryRate = 1.0f;

        [Header("Dynamic Difficulty Adjustment")]
        [SerializeField] private bool enableDynamicAdjustment = true;
        [SerializeField] private DynamicDifficultySettings dynamicSettings;

        [Header("Consumption Tracking")]
        [SerializeField] private bool trackItemUsage = true;
        [SerializeField] private bool trackPlayerBehavior = true;
        [SerializeField] private float consumptionAnalysisInterval = 60f;

        [Header("Emergency Resource System")]
        [SerializeField] private bool enableEmergencySupply = true;
        [SerializeField] private EmergencyResourceSettings emergencySettings;

        [Header("Events")]
        [SerializeField] private GameEvent<ResourceType> onResourceDepleted;
        [SerializeField] private GameEvent<ResourceType> onResourceReplenished;
        [SerializeField] private GameEvent<float> onScarcityLevelChanged;

        // Runtime Tracking
        private Dictionary<ResourceType, ResourceTracker> resourceTrackers;
        private Dictionary<string, int> itemUsageCounter;
        private float lastAnalysisTime;
        private bool isInitialized = false;

        // Public Properties
        public string ConfigVersion => configVersion;
        public bool EnableResourceTracking => enableResourceTracking;
        public bool AllowResourceRespawn => allowResourceRespawn;
        public float GlobalScarcityMultiplier => globalScarcityMultiplier;
        public bool EnableDynamicAdjustment => enableDynamicAdjustment;

        private void OnEnable()
        {
            InitializeResourceTracking();
        }

        /// <summary>
        /// リソース追跡システムを初期化
        /// </summary>
        public void InitializeResourceTracking()
        {
            if (isInitialized) return;

            resourceTrackers = new Dictionary<ResourceType, ResourceTracker>
            {
                { ResourceType.Health, new ResourceTracker(healthResourcePool, maxHealthItemsInScene) },
                { ResourceType.Ammunition, new ResourceTracker(ammunitionResourcePool, maxAmmunitionInScene) },
                { ResourceType.SaveItem, new ResourceTracker(saveResourcePool, maxSaveItemsInScene) }
            };

            itemUsageCounter = new Dictionary<string, int>();
            lastAnalysisTime = Time.time;
            isInitialized = true;

            Debug.Log("[SH_ResourceManagerConfig] Resource tracking initialized");
        }

        /// <summary>
        /// 指定されたアイテムタイプのスポーン率を計算
        /// </summary>
        public float CalculateSpawnRate(ResourceType resourceType, DifficultyLevel difficulty)
        {
            float baseRate = resourceType switch
            {
                ResourceType.Health => healthItemSpawnRate,
                ResourceType.Ammunition => ammunitionSpawnRate,
                ResourceType.SaveItem => saveItemSpawnRate,
                ResourceType.KeyItem => keyItemDiscoveryRate,
                _ => 1.0f
            };

            float difficultyMultiplier = difficulty switch
            {
                DifficultyLevel.Easy => 1.5f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 0.7f,
                DifficultyLevel.Nightmare => 0.4f,
                _ => 1.0f
            };

            // 動的難易度調整を考慮
            float dynamicMultiplier = enableDynamicAdjustment ?
                CalculateDynamicMultiplier(resourceType) : 1.0f;

            return baseRate * difficultyMultiplier * globalScarcityMultiplier * dynamicMultiplier;
        }

        /// <summary>
        /// アイテムのスポーンを試行
        /// </summary>
        public bool TrySpawnResource(ResourceType resourceType, Vector3 position)
        {
            if (!isInitialized) InitializeResourceTracking();

            var tracker = resourceTrackers[resourceType];

            // スポーン制限チェック
            if (tracker.CurrentCount >= tracker.MaxCount)
            {
                Debug.LogWarning($"[SH_ResourceManagerConfig] Spawn limit reached for {resourceType}");
                return false;
            }

            // スカーシティ制約チェック
            if (!CanSpawnResource(resourceType))
            {
                return false;
            }

            // スポーン実行
            tracker.IncrementCount();
            Debug.Log($"[SH_ResourceManagerConfig] Spawned {resourceType} at {position}");
            return true;
        }

        /// <summary>
        /// リソースの消費を記録
        /// </summary>
        public void RecordResourceConsumption(string itemId, ResourceType resourceType)
        {
            if (!trackItemUsage) return;

            // 使用量カウンター更新
            if (!itemUsageCounter.ContainsKey(itemId))
            {
                itemUsageCounter[itemId] = 0;
            }
            itemUsageCounter[itemId]++;

            // リソーストラッカー更新
            if (resourceTrackers.ContainsKey(resourceType))
            {
                resourceTrackers[resourceType].DecrementCount();
            }

            // 動的調整の判定
            if (enableDynamicAdjustment && Time.time - lastAnalysisTime > consumptionAnalysisInterval)
            {
                AnalyzeConsumptionPatterns();
                lastAnalysisTime = Time.time;
            }

            Debug.Log($"[SH_ResourceManagerConfig] Recorded consumption: {itemId} ({resourceType})");
        }

        /// <summary>
        /// リソース枯渇状態を取得
        /// </summary>
        public ResourceScarcityLevel GetResourceScarcityLevel(ResourceType resourceType)
        {
            if (!resourceTrackers.ContainsKey(resourceType))
                return ResourceScarcityLevel.Abundant;

            var tracker = resourceTrackers[resourceType];
            float ratio = (float)tracker.CurrentCount / tracker.MaxCount;

            return ratio switch
            {
                > 0.7f => ResourceScarcityLevel.Abundant,
                > 0.4f => ResourceScarcityLevel.Moderate,
                > 0.1f => ResourceScarcityLevel.Scarce,
                _ => ResourceScarcityLevel.Critical
            };
        }

        /// <summary>
        /// 緊急リソース供給を実行
        /// </summary>
        public bool TriggerEmergencySupply(ResourceType resourceType)
        {
            if (!enableEmergencySupply) return false;

            var scarcityLevel = GetResourceScarcityLevel(resourceType);
            if (scarcityLevel != ResourceScarcityLevel.Critical)
                return false;

            // 緊急供給条件をチェック
            if (!emergencySettings.CanTriggerEmergencySupply(resourceType))
                return false;

            // 緊急供給実行
            var tracker = resourceTrackers[resourceType];
            int supplyAmount = emergencySettings.GetEmergencySupplyAmount(resourceType);
            tracker.AddResources(supplyAmount);

            onResourceReplenished?.Raise(resourceType);

            Debug.Log($"[SH_ResourceManagerConfig] Emergency supply triggered for {resourceType}: +{supplyAmount}");
            return true;
        }

        /// <summary>
        /// プレイヤー行動に基づく推奨設定を取得
        /// </summary>
        public ResourceRecommendation GetPlayerBehaviorRecommendation()
        {
            if (!trackPlayerBehavior || itemUsageCounter.Count == 0)
                return new ResourceRecommendation();

            var recommendation = new ResourceRecommendation();

            // 消費パターン分析
            int totalHealthUsage = GetCategoryUsage("health");
            int totalAmmunitionUsage = GetCategoryUsage("ammunition");

            if (totalHealthUsage > totalAmmunitionUsage * 2)
            {
                recommendation.suggestedHealthMultiplier = 1.2f;
                recommendation.recommendationReason = "プレイヤーは回復アイテムを多用しています";
            }
            else if (totalAmmunitionUsage > totalHealthUsage * 2)
            {
                recommendation.suggestedAmmunitionMultiplier = 1.2f;
                recommendation.recommendationReason = "プレイヤーは戦闘を頻繁に行っています";
            }

            return recommendation;
        }

        /// <summary>
        /// 設定をリアルタイムで更新
        /// </summary>
        public void UpdateConfiguration(SH_TemplateConfig templateConfig)
        {
            var difficulty = templateConfig.CurrentDifficulty;

            // 難易度に基づく自動調整
            globalScarcityMultiplier = difficulty switch
            {
                DifficultyLevel.Easy => 1.5f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 0.7f,
                DifficultyLevel.Nightmare => 0.4f,
                _ => 1.0f
            };

            Debug.Log($"[SH_ResourceManagerConfig] Configuration updated for {difficulty} difficulty");
        }

        /// <summary>
        /// リソースのスポーン可能性をチェック
        /// </summary>
        private bool CanSpawnResource(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Health => !enforceHealthScarcity || GetResourceScarcityLevel(resourceType) != ResourceScarcityLevel.Critical,
                ResourceType.Ammunition => !enforceAmmunitionScarcity || GetResourceScarcityLevel(resourceType) != ResourceScarcityLevel.Critical,
                ResourceType.SaveItem => !limitSaveResources || GetResourceScarcityLevel(resourceType) != ResourceScarcityLevel.Critical,
                ResourceType.KeyItem => keyItemsAreUnique,
                _ => true
            };
        }

        /// <summary>
        /// 動的難易度調整用乗数を計算
        /// </summary>
        private float CalculateDynamicMultiplier(ResourceType resourceType)
        {
            if (!enableDynamicAdjustment || dynamicSettings == null)
                return 1.0f;

            var scarcityLevel = GetResourceScarcityLevel(resourceType);

            return scarcityLevel switch
            {
                ResourceScarcityLevel.Critical => dynamicSettings.criticalSupplyMultiplier,
                ResourceScarcityLevel.Scarce => dynamicSettings.scarceSupplyMultiplier,
                ResourceScarcityLevel.Moderate => 1.0f,
                ResourceScarcityLevel.Abundant => dynamicSettings.abundantSupplyMultiplier,
                _ => 1.0f
            };
        }

        /// <summary>
        /// 消費パターンを分析
        /// </summary>
        private void AnalyzeConsumptionPatterns()
        {
            foreach (var resourceType in resourceTrackers.Keys)
            {
                var scarcityLevel = GetResourceScarcityLevel(resourceType);

                if (scarcityLevel == ResourceScarcityLevel.Critical)
                {
                    onResourceDepleted?.Raise(resourceType);
                    TriggerEmergencySupply(resourceType);
                }
            }
        }

        private int GetCategoryUsage(string category)
        {
            int total = 0;
            foreach (var kvp in itemUsageCounter)
            {
                if (kvp.Key.Contains(category))
                {
                    total += kvp.Value;
                }
            }
            return total;
        }

        /// <summary>
        /// 設定の整合性を検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (globalScarcityMultiplier < 0)
            {
                Debug.LogError("[SH_ResourceManagerConfig] Global scarcity multiplier cannot be negative");
                isValid = false;
            }

            if (consumptionAnalysisInterval <= 0)
            {
                Debug.LogError("[SH_ResourceManagerConfig] Consumption analysis interval must be positive");
                isValid = false;
            }

            return isValid;
        }

        // エディタ専用デバッグ機能
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/SurvivalHorror/Validate Resource Config")]
        private static void ValidateResourceConfigFromMenu()
        {
            var configs = UnityEngine.Resources.LoadAll<SH_ResourceManagerConfig>("");
            foreach (var config in configs)
            {
                config.ValidateConfiguration();
            }
        }

        [ContextMenu("Reset Resource Counters")]
        private void ResetResourceCounters()
        {
            if (resourceTrackers != null)
            {
                foreach (var tracker in resourceTrackers.Values)
                {
                    tracker.Reset();
                }
            }

            if (itemUsageCounter != null)
            {
                itemUsageCounter.Clear();
            }

            Debug.Log("[SH_ResourceManagerConfig] Resource counters reset");
        }
        #endif
    }

    /// <summary>
    /// リソースタイプ定義
    /// </summary>
    public enum ResourceType
    {
        Health,      // 回復アイテム
        Ammunition,  // 弾薬・武器関連
        SaveItem,    // セーブ関連アイテム
        KeyItem,     // キーアイテム
        Special      // 特殊アイテム
    }

    /// <summary>
    /// リソース希少度レベル
    /// </summary>
    public enum ResourceScarcityLevel
    {
        Abundant,    // 豊富（緑）
        Moderate,    // 普通（黄）
        Scarce,      // 希少（橙）
        Critical     // 危機的（赤）
    }

    /// <summary>
    /// リソースプール設定
    /// </summary>
    [System.Serializable]
    public class ResourcePool
    {
        [Header("Pool Configuration")]
        public string poolName;
        public ResourceType resourceType;
        public int initialCapacity = 10;
        public int maxCapacity = 20;
        public bool allowOverflow = false;

        [Header("Replenishment")]
        public bool enableAutoReplenishment = false;
        public float replenishmentRate = 1.0f;
        public float replenishmentInterval = 30f;

        [Header("Quality Control")]
        public float averageQuality = 1.0f;
        public float qualityVariation = 0.2f;
    }

    /// <summary>
    /// リソーストラッカー（ランタイム使用）
    /// </summary>
    public class ResourceTracker
    {
        private ResourcePool pool;
        private int currentCount;
        private int maxCount;
        private float lastReplenishTime;

        public int CurrentCount => currentCount;
        public int MaxCount => maxCount;
        public float FillRatio => (float)currentCount / maxCount;

        public ResourceTracker(ResourcePool resourcePool, int maxAllowed)
        {
            pool = resourcePool;
            currentCount = resourcePool.initialCapacity;
            maxCount = maxAllowed;
            lastReplenishTime = Time.time;
        }

        public void IncrementCount() => currentCount = Mathf.Min(currentCount + 1, maxCount);
        public void DecrementCount() => currentCount = Mathf.Max(currentCount - 1, 0);
        public void AddResources(int amount) => currentCount = Mathf.Min(currentCount + amount, maxCount);
        public void Reset() => currentCount = pool.initialCapacity;

        public bool TryReplenish()
        {
            if (!pool.enableAutoReplenishment) return false;
            if (Time.time - lastReplenishTime < pool.replenishmentInterval) return false;
            if (currentCount >= maxCount) return false;

            int replenishAmount = Mathf.FloorToInt(pool.replenishmentRate);
            AddResources(replenishAmount);
            lastReplenishTime = Time.time;

            return true;
        }
    }

    /// <summary>
    /// 動的難易度調整設定
    /// </summary>
    [System.Serializable]
    public class DynamicDifficultySettings
    {
        [Header("Supply Multipliers")]
        [Range(0.1f, 3.0f)] public float criticalSupplyMultiplier = 2.0f;
        [Range(0.1f, 2.0f)] public float scarceSupplyMultiplier = 1.5f;
        [Range(0.1f, 1.5f)] public float abundantSupplyMultiplier = 0.8f;

        [Header("Adjustment Sensitivity")]
        [Range(0.1f, 5.0f)] public float adjustmentSensitivity = 1.0f;
        public float maxAdjustmentRate = 0.5f;
        public float adjustmentSmoothness = 2.0f;

        [Header("Player Behavior Tracking")]
        public bool trackDeathCount = true;
        public bool trackHealthUsage = true;
        public bool trackCombatAvoidance = true;
    }

    /// <summary>
    /// 緊急リソース設定
    /// </summary>
    [System.Serializable]
    public class EmergencyResourceSettings
    {
        [Header("Emergency Triggers")]
        public bool enableHealthEmergency = true;
        public bool enableAmmunitionEmergency = true;
        public int maxEmergencySupplies = 3;

        [Header("Supply Amounts")]
        public int healthEmergencyAmount = 2;
        public int ammunitionEmergencyAmount = 5;
        public int saveItemEmergencyAmount = 1;

        [Header("Cooldown")]
        public float emergencyCooldown = 300f; // 5分

        private Dictionary<ResourceType, float> lastEmergencyTime = new Dictionary<ResourceType, float>();

        public bool CanTriggerEmergencySupply(ResourceType resourceType)
        {
            if (!lastEmergencyTime.ContainsKey(resourceType))
            {
                lastEmergencyTime[resourceType] = 0f;
            }

            return Time.time - lastEmergencyTime[resourceType] > emergencyCooldown;
        }

        public int GetEmergencySupplyAmount(ResourceType resourceType)
        {
            lastEmergencyTime[resourceType] = Time.time;

            return resourceType switch
            {
                ResourceType.Health => healthEmergencyAmount,
                ResourceType.Ammunition => ammunitionEmergencyAmount,
                ResourceType.SaveItem => saveItemEmergencyAmount,
                _ => 1
            };
        }
    }

    /// <summary>
    /// プレイヤー行動分析に基づく推奨設定
    /// </summary>
    public class ResourceRecommendation
    {
        public float suggestedHealthMultiplier = 1.0f;
        public float suggestedAmmunitionMultiplier = 1.0f;
        public float suggestedSaveItemMultiplier = 1.0f;
        public string recommendationReason = "プレイヤー行動データが不足しています";
        public float confidenceLevel = 0.5f;
    }
}