using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Strategy.Resources
{
    /// <summary>
    /// ストラテジーゲーム用リソース管理システム
    /// 木材、石材、食料、金貨などの資源を統合管理
    /// </summary>
    public class StrategyResourceManager : MonoBehaviour, IService
    {
        [System.Serializable]
        public class ResourceData
        {
            [SerializeField] public ResourceType type;
            [SerializeField] public int currentAmount;
            [SerializeField] public int maxCapacity;
            [SerializeField] public int productionRate; // 1秒あたりの生産量
            [SerializeField] public float lastProductionTime;
            [ShowInInspector, ReadOnly] public float Percentage => maxCapacity > 0 ? (float)currentAmount / maxCapacity : 0f;
        }
        
        [Header("Resource Settings")]
        [SerializeField] private List<ResourceData> resources = new List<ResourceData>();
        [SerializeField] private bool enableAutoProduction = true;
        [SerializeField] private float productionInterval = 1f;
        
        [Header("Starting Resources")]
        [SerializeField] private int startingWood = 100;
        [SerializeField] private int startingStone = 50;
        [SerializeField] private int startingFood = 200;
        [SerializeField] private int startingGold = 100;
        
        [Header("Events")]
        [SerializeField] private GameEvent onResourceChanged;
        [SerializeField] private GameEvent onResourceInsufficientEvent;
        [SerializeField] private GameEvent onResourceCapacityReached;
        [SerializeField] private GameEvent onProductionUpdate;
        
        private Dictionary<ResourceType, ResourceData> resourceDict;
        private float lastProductionTime;
        
        // Properties
        public IReadOnlyDictionary<ResourceType, ResourceData> Resources => resourceDict;
        public bool EnableAutoProduction
        {
            get => enableAutoProduction;
            set => enableAutoProduction = value;
        }
        
        // Events
        public event System.Action<ResourceType, int> OnResourceChanged;
        public event System.Action<ResourceType, int> OnResourceInsufficient;
        public event System.Action<ResourceType> OnResourceCapacityReached;
        
        private void Awake()
        {
            InitializeResources();
            ServiceLocator.RegisterService<StrategyResourceManager>(this);
        }
        
        private void Start()
        {
            SetStartingResources();
            lastProductionTime = Time.time;
        }
        
        private void Update()
        {
            if (enableAutoProduction && Time.time - lastProductionTime >= productionInterval)
            {
                ProcessAutoProduction();
                lastProductionTime = Time.time;
            }
        }
        
        /// <summary>
        /// 外部からの初期化（StrategyTemplateConfiguration用）
        /// </summary>
        public void Initialize(StrategyTemplateConfiguration config)
        {
            InitializeResources();
            SetStartingResources();
            Debug.Log("[StrategyResourceManager] External initialization completed");
        }

        /// <summary>
        /// リソース初期化
        /// </summary>
        private void InitializeResources()
        {
            resourceDict = new Dictionary<ResourceType, ResourceData>();
            
            // デフォルトリソース設定
            if (resources.Count == 0)
            {
                CreateDefaultResources();
            }
            
            foreach (var resourceData in resources)
            {
                resourceDict[resourceData.type] = resourceData;
            }
        }
        
        /// <summary>
        /// デフォルトリソース作成
        /// </summary>
        private void CreateDefaultResources()
        {
            resources.Add(new ResourceData { type = ResourceType.Wood, currentAmount = 0, maxCapacity = 1000, productionRate = 0 });
            resources.Add(new ResourceData { type = ResourceType.Stone, currentAmount = 0, maxCapacity = 1000, productionRate = 0 });
            resources.Add(new ResourceData { type = ResourceType.Food, currentAmount = 0, maxCapacity = 500, productionRate = 0 });
            resources.Add(new ResourceData { type = ResourceType.Gold, currentAmount = 0, maxCapacity = 10000, productionRate = 0 });
            resources.Add(new ResourceData { type = ResourceType.Population, currentAmount = 0, maxCapacity = 100, productionRate = 0 });
            resources.Add(new ResourceData { type = ResourceType.Energy, currentAmount = 0, maxCapacity = 100, productionRate = 0 });
        }
        
        /// <summary>
        /// 初期リソース設定
        /// </summary>
        private void SetStartingResources()
        {
            SetResource(ResourceType.Wood, startingWood);
            SetResource(ResourceType.Stone, startingStone);
            SetResource(ResourceType.Food, startingFood);
            SetResource(ResourceType.Gold, startingGold);
        }
        
        /// <summary>
        /// 自動生産処理
        /// </summary>
        private void ProcessAutoProduction()
        {
            bool hasProduced = false;
            
            foreach (var kvp in resourceDict)
            {
                var resourceData = kvp.Value;
                if (resourceData.productionRate > 0)
                {
                    int producedAmount = resourceData.productionRate;
                    int actualAdded = AddResource(kvp.Key, producedAmount);
                    
                    if (actualAdded > 0)
                    {
                        hasProduced = true;
                    }
                }
            }
            
            if (hasProduced)
            {
                onProductionUpdate?.Raise();
            }
        }
        
        /// <summary>
        /// リソース取得
        /// </summary>
        public int GetResource(ResourceType type)
        {
            return resourceDict.ContainsKey(type) ? resourceDict[type].currentAmount : 0;
        }
        
        /// <summary>
        /// リソース設定
        /// </summary>
        public void SetResource(ResourceType type, int amount)
        {
            if (resourceDict.ContainsKey(type))
            {
                var resourceData = resourceDict[type];
                int oldAmount = resourceData.currentAmount;
                resourceData.currentAmount = Mathf.Clamp(amount, 0, resourceData.maxCapacity);
                
                if (resourceData.currentAmount != oldAmount)
                {
                    OnResourceChanged?.Invoke(type, resourceData.currentAmount);
                    onResourceChanged?.Raise();
                }
            }
        }
        
        /// <summary>
        /// リソース追加
        /// </summary>
        public int AddResource(ResourceType type, int amount)
        {
            if (!resourceDict.ContainsKey(type) || amount <= 0)
                return 0;
            
            var resourceData = resourceDict[type];
            int oldAmount = resourceData.currentAmount;
            int newAmount = Mathf.Min(resourceData.currentAmount + amount, resourceData.maxCapacity);
            int actualAdded = newAmount - resourceData.currentAmount;
            
            resourceData.currentAmount = newAmount;
            
            if (actualAdded > 0)
            {
                OnResourceChanged?.Invoke(type, resourceData.currentAmount);
                onResourceChanged?.Raise();
                
                // 容量到達チェック
                if (resourceData.currentAmount >= resourceData.maxCapacity)
                {
                    OnResourceCapacityReached?.Invoke(type);
                    onResourceCapacityReached?.Raise();
                }
            }
            
            return actualAdded;
        }
        
        /// <summary>
        /// リソース消費
        /// </summary>
        public bool ConsumeResource(ResourceType type, int amount)
        {
            if (!CanAfford(type, amount))
            {
                OnResourceInsufficient?.Invoke(type, amount);
                onResourceInsufficientEvent?.Raise();
                return false;
            }
            
            var resourceData = resourceDict[type];
            resourceData.currentAmount -= amount;
            
            OnResourceChanged?.Invoke(type, resourceData.currentAmount);
            onResourceChanged?.Raise();
            
            return true;
        }
        
        /// <summary>
        /// 複数リソース消費
        /// </summary>
        public bool ConsumeResources(Dictionary<ResourceType, int> costs)
        {
            // まず全部のリソースが足りるかチェック
            foreach (var kvp in costs)
            {
                if (!CanAfford(kvp.Key, kvp.Value))
                {
                    OnResourceInsufficient?.Invoke(kvp.Key, kvp.Value);
                    onResourceInsufficientEvent?.Raise();
                    return false;
                }
            }
            
            // すべて足りる場合は消費実行
            foreach (var kvp in costs)
            {
                ConsumeResource(kvp.Key, kvp.Value);
            }
            
            return true;
        }
        
        /// <summary>
        /// リソース購入可能チェック
        /// </summary>
        public bool CanAfford(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }
        
        /// <summary>
        /// 複数リソース購入可能チェック
        /// </summary>
        public bool CanAfford(Dictionary<ResourceType, int> costs)
        {
            return costs.All(kvp => CanAfford(kvp.Key, kvp.Value));
        }
        
        /// <summary>
        /// 生産レート設定
        /// </summary>
        public void SetProductionRate(ResourceType type, int rate)
        {
            if (resourceDict.ContainsKey(type))
            {
                resourceDict[type].productionRate = Mathf.Max(0, rate);
            }
        }
        
        /// <summary>
        /// 最大容量設定
        /// </summary>
        public void SetMaxCapacity(ResourceType type, int capacity)
        {
            if (resourceDict.ContainsKey(type))
            {
                var resourceData = resourceDict[type];
                resourceData.maxCapacity = Mathf.Max(0, capacity);
                
                // 現在量が容量を超えている場合は調整
                if (resourceData.currentAmount > resourceData.maxCapacity)
                {
                    SetResource(type, resourceData.maxCapacity);
                }
            }
        }
        
        /// <summary>
        /// リソース割合取得
        /// </summary>
        public float GetResourcePercentage(ResourceType type)
        {
            if (!resourceDict.ContainsKey(type))
                return 0f;
                
            var resourceData = resourceDict[type];
            return resourceData.maxCapacity > 0 ? (float)resourceData.currentAmount / resourceData.maxCapacity : 0f;
        }
        
        /// <summary>
        /// リソース情報取得
        /// </summary>
        public ResourceData GetResourceData(ResourceType type)
        {
            return resourceDict.ContainsKey(type) ? resourceDict[type] : null;
        }
        
        /// <summary>
        /// すべてのリソースをリセット
        /// </summary>
        [Button("Reset All Resources")]
        public void ResetAllResources()
        {
            foreach (var resourceData in resourceDict.Values)
            {
                resourceData.currentAmount = 0;
            }
            
            onResourceChanged?.Raise();
        }
        
        /// <summary>
        /// リソース状態のデバッグ出力
        /// </summary>
        [Button("Log Resource Status")]
        public void LogResourceStatus()
        {
            Debug.Log("[StrategyResourceManager] Resource Status:");
            foreach (var kvp in resourceDict)
            {
                var data = kvp.Value;
                Debug.Log($"{kvp.Key}: {data.currentAmount}/{data.maxCapacity} ({data.Percentage:P1}) - Production: {data.productionRate}/sec");
            }
        }
        
        /// <summary>
        /// セーブデータ生成
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            foreach (var kvp in resourceDict)
            {
                saveData[kvp.Key.ToString()] = kvp.Value.currentAmount;
            }
            
            return saveData;
        }
        
        /// <summary>
        /// セーブデータロード
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> saveData)
        {
            foreach (var kvp in saveData)
            {
                if (Enum.TryParse<ResourceType>(kvp.Key, out var resourceType) && kvp.Value is int amount)
                {
                    SetResource(resourceType, amount);
                }
            }
        }
        
        private void OnDestroy()
        {
            ServiceLocator.UnregisterService<StrategyResourceManager>();
        }
        
        #if UNITY_EDITOR
        [Button("Add Test Resources")]
        private void AddTestResources()
        {
            AddResource(ResourceType.Wood, 100);
            AddResource(ResourceType.Stone, 50);
            AddResource(ResourceType.Food, 75);
            AddResource(ResourceType.Gold, 200);
        }
        
        [Button("Consume Test Resources")]
        private void ConsumeTestResources()
        {
            var testCosts = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, 25 },
                { ResourceType.Stone, 10 },
                { ResourceType.Gold, 50 }
            };
            
            bool success = ConsumeResources(testCosts);
            Debug.Log($"[StrategyResourceManager] Test consumption: {(success ? "Success" : "Failed")}");
        }
        #endif
    }
}