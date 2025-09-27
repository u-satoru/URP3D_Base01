using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート用リソース管理システム
    /// アイテムスポーン、希少性制御、動的バランス調整を実行
    /// </summary>
    public class SH_ResourceManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SH_ResourceManagerConfig config;
        [SerializeField] private SH_ItemDatabase itemDatabase;

        [Header("Spawn Management")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius = 1.0f;
        [SerializeField] private LayerMask spawnLayerMask = -1;
        [SerializeField] private bool enableAutoSpawn = true;
        [SerializeField] private float autoSpawnInterval = 30f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject spawnEffectPrefab;
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private bool showSpawnGizmos = true;

        [Header("Events")]
        [SerializeField] private GameEvent<SH_ItemData> onItemSpawned;
        [SerializeField] private GameEvent<SH_ItemData> onItemCollected;
        [SerializeField] private GameEvent<ResourceType> onResourceCritical;

        // Runtime State
        private Dictionary<string, GameObject> activeItems;
        private Dictionary<ResourceType, List<Vector3>> lastSpawnPositions;
        private Dictionary<ResourceType, float> lastSpawnTimes;
        private Coroutine autoSpawnCoroutine;
        private bool isInitialized = false;

        public int ActiveItemCount => activeItems?.Count ?? 0;
        public bool IsInitialized => isInitialized;

        private void Start()
        {
            if (config != null && itemDatabase != null)
            {
                Initialize(config, itemDatabase);
            }
        }

        /// <summary>
        /// ResourceManagerを初期化
        /// </summary>
        public void Initialize(SH_ResourceManagerConfig resourceConfig, SH_ItemDatabase database)
        {
            if (resourceConfig == null || database == null)
            {
                Debug.LogError("[SH_ResourceManager] Cannot initialize with null parameters");
                return;
            }

            config = resourceConfig;
            itemDatabase = database;

            InitializeDataStructures();
            config.InitializeResourceTracking();

            if (enableAutoSpawn)
            {
                StartAutoSpawnSystem();
            }

            isInitialized = true;
            Debug.Log("[SH_ResourceManager] Resource manager initialized successfully");
        }

        /// <summary>
        /// 指定されたアイテムをスポーン試行
        /// </summary>
        public bool TrySpawnItem(SH_ItemData itemData, Vector3 position, bool forced = false)
        {
            if (!isInitialized || itemData == null)
            {
                Debug.LogWarning("[SH_ResourceManager] Cannot spawn item: manager not initialized or item null");
                return false;
            }

            // リソース制限チェック
            var resourceType = GetResourceType(itemData.Category);
            if (!forced && !config.TrySpawnResource(resourceType, position))
            {
                return false;
            }

            // スポーン位置の妥当性チェック
            Vector3 finalPosition = GetValidSpawnPosition(position);
            if (finalPosition == Vector3.zero)
            {
                Debug.LogWarning($"[SH_ResourceManager] Cannot find valid spawn position for {itemData.ItemName}");
                return false;
            }

            // アイテムインスタンスを生成
            GameObject spawnedItem = CreateItemInstance(itemData, finalPosition);
            if (spawnedItem == null)
            {
                return false;
            }

            // アクティブアイテムとして登録
            string itemId = System.Guid.NewGuid().ToString();
            activeItems[itemId] = spawnedItem;

            // スポーン履歴を記録
            RecordSpawnHistory(resourceType, finalPosition);

            // エフェクトと音響
            PlaySpawnEffects(finalPosition);

            // イベント発行
            onItemSpawned?.Raise(itemData);

            Debug.Log($"[SH_ResourceManager] Spawned {itemData.ItemName} at {finalPosition}");
            return true;
        }

        /// <summary>
        /// カテゴリー指定でランダムアイテムをスポーン
        /// </summary>
        public bool TrySpawnRandomItem(ItemCategory category, Vector3 position)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[SH_ResourceManager] Cannot spawn random item: not initialized");
                return false;
            }

            var randomItem = itemDatabase.GetRandomItem(category);
            if (randomItem == null)
            {
                Debug.LogWarning($"[SH_ResourceManager] No items available for category {category}");
                return false;
            }

            return TrySpawnItem(randomItem, position);
        }

        /// <summary>
        /// アイテム収集を処理
        /// </summary>
        public void OnItemCollected(SH_ItemData itemData, GameObject itemObject)
        {
            if (itemData == null || itemObject == null) return;

            // アクティブアイテムリストから削除
            var itemId = activeItems.FirstOrDefault(kvp => kvp.Value == itemObject).Key;
            if (!string.IsNullOrEmpty(itemId))
            {
                activeItems.Remove(itemId);
            }

            // リソース消費を記録
            var resourceType = GetResourceType(itemData.Category);
            config.RecordResourceConsumption(itemData.ItemId, resourceType);

            // 緊急供給判定
            var scarcityLevel = config.GetResourceScarcityLevel(resourceType);
            if (scarcityLevel == ResourceScarcityLevel.Critical)
            {
                onResourceCritical?.Raise(resourceType);
                config.TriggerEmergencySupply(resourceType);
            }

            onItemCollected?.Raise(itemData);
            Debug.Log($"[SH_ResourceManager] Item collected: {itemData.ItemName}");
        }

        /// <summary>
        /// 指定エリア内の全アイテムを取得
        /// </summary>
        public List<GameObject> GetItemsInArea(Vector3 center, float radius)
        {
            return activeItems.Values
                .Where(item => item != null && Vector3.Distance(item.transform.position, center) <= radius)
                .ToList();
        }

        /// <summary>
        /// リソースタイプ別アイテム数を取得
        /// </summary>
        public int GetActiveItemCount(ResourceType resourceType)
        {
            return activeItems.Values
                .Where(item => item != null)
                .Count(item =>
                {
                    var itemComponent = item.GetComponent<SH_ItemPickup>();
                    return itemComponent != null && GetResourceType(itemComponent.ItemData.Category) == resourceType;
                });
        }

        /// <summary>
        /// 全アイテムをクリア
        /// </summary>
        public void ClearAllItems()
        {
            foreach (var item in activeItems.Values)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }

            activeItems.Clear();
            Debug.Log("[SH_ResourceManager] All items cleared");
        }

        /// <summary>
        /// 緊急リソース供給を実行
        /// </summary>
        public void ExecuteEmergencySupply(ResourceType resourceType, int count = 1)
        {
            var availableSpawnPoints = GetAvailableSpawnPoints();
            if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning("[SH_ResourceManager] No available spawn points for emergency supply");
                return;
            }

            var itemsToSpawn = GetEmergencyItems(resourceType, count);
            int spawned = 0;

            foreach (var item in itemsToSpawn)
            {
                if (spawned >= availableSpawnPoints.Count) break;

                var spawnPoint = availableSpawnPoints[spawned];
                if (TrySpawnItem(item, spawnPoint, true))
                {
                    spawned++;
                }
            }

            Debug.Log($"[SH_ResourceManager] Emergency supply: spawned {spawned} {resourceType} items");
        }

        /// <summary>
        /// リソース統計を取得
        /// </summary>
        public ResourceManagerStatistics GetStatistics()
        {
            var stats = new ResourceManagerStatistics
            {
                TotalActiveItems = activeItems.Count,
                HealthItems = GetActiveItemCount(ResourceType.Health),
                AmmunitionItems = GetActiveItemCount(ResourceType.Ammunition),
                SaveItems = GetActiveItemCount(ResourceType.SaveItem),
                KeyItems = GetActiveItemCount(ResourceType.KeyItem),
                SpecialItems = GetActiveItemCount(ResourceType.Special)
            };

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                stats.ScarcityLevels[resourceType] = config.GetResourceScarcityLevel(resourceType);
            }

            return stats;
        }

        /// <summary>
        /// データ構造を初期化
        /// </summary>
        private void InitializeDataStructures()
        {
            activeItems = new Dictionary<string, GameObject>();
            lastSpawnPositions = new Dictionary<ResourceType, List<Vector3>>();
            lastSpawnTimes = new Dictionary<ResourceType, float>();

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                lastSpawnPositions[resourceType] = new List<Vector3>();
                lastSpawnTimes[resourceType] = 0f;
            }
        }

        /// <summary>
        /// 自動スポーンシステムを開始
        /// </summary>
        private void StartAutoSpawnSystem()
        {
            if (autoSpawnCoroutine != null)
            {
                StopCoroutine(autoSpawnCoroutine);
            }

            autoSpawnCoroutine = StartCoroutine(AutoSpawnLoop());
        }

        /// <summary>
        /// 自動スポーンループ
        /// </summary>
        private IEnumerator AutoSpawnLoop()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(autoSpawnInterval);

                if (!isInitialized) continue;

                PerformAutoSpawn();
            }
        }

        /// <summary>
        /// 自動スポーンを実行
        /// </summary>
        private void PerformAutoSpawn()
        {
            var availableSpawnPoints = GetAvailableSpawnPoints();
            if (availableSpawnPoints.Count == 0) return;

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                var scarcityLevel = config.GetResourceScarcityLevel(resourceType);

                if (scarcityLevel == ResourceScarcityLevel.Moderate || scarcityLevel == ResourceScarcityLevel.Scarce)
                {
                    if (ShouldSpawnResource(resourceType))
                    {
                        var spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
                        TrySpawnRandomItemByResourceType(resourceType, spawnPoint);
                    }
                }
            }
        }

        /// <summary>
        /// リソースタイプに基づくランダムアイテムスポーン
        /// </summary>
        private bool TrySpawnRandomItemByResourceType(ResourceType resourceType, Vector3 position)
        {
            var category = GetCategoryFromResourceType(resourceType);
            return TrySpawnRandomItem(category, position);
        }

        /// <summary>
        /// アイテムインスタンスを作成
        /// </summary>
        private GameObject CreateItemInstance(SH_ItemData itemData, Vector3 position)
        {
            GameObject itemPrefab = itemData.Prefab;

            if (itemPrefab == null)
            {
                // デフォルトアイテムプレハブを作成
                itemPrefab = CreateDefaultItemPrefab(itemData);
            }

            if (itemPrefab == null)
            {
                Debug.LogError($"[SH_ResourceManager] Cannot create item instance for {itemData.ItemName}: no prefab");
                return null;
            }

            GameObject instance = Instantiate(itemPrefab, position, Quaternion.identity);
            instance.name = $"{itemData.ItemName}_Instance";

            // SH_ItemPickupコンポーネントを追加/設定
            var pickupComponent = instance.GetComponent<SH_ItemPickup>();
            if (pickupComponent == null)
            {
                pickupComponent = instance.AddComponent<SH_ItemPickup>();
            }

            pickupComponent.Initialize(itemData, this);

            return instance;
        }

        /// <summary>
        /// デフォルトアイテムプレハブを作成
        /// </summary>
        private GameObject CreateDefaultItemPrefab(SH_ItemData itemData)
        {
            var defaultPrefab = new GameObject($"Default_{itemData.ItemName}");

            // 基本コンポーネントを追加
            var collider = defaultPrefab.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;

            var rigidbody = defaultPrefab.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;

            // アイコンから簡単な視覚表現を作成
            if (itemData.Icon != null)
            {
                var spriteRenderer = defaultPrefab.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = itemData.Icon;
            }

            return defaultPrefab;
        }

        /// <summary>
        /// 有効なスポーン位置を取得
        /// </summary>
        private Vector3 GetValidSpawnPosition(Vector3 preferredPosition)
        {
            // 地面との衝突チェック
            if (Physics.Raycast(preferredPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, spawnLayerMask))
            {
                return hit.point + Vector3.up * 0.1f;
            }

            // 周辺での代替位置検索
            for (int attempts = 0; attempts < 5; attempts++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
                randomOffset.y = 0f;
                Vector3 testPosition = preferredPosition + randomOffset;

                if (Physics.Raycast(testPosition + Vector3.up * 2f, Vector3.down, out hit, 5f, spawnLayerMask))
                {
                    return hit.point + Vector3.up * 0.1f;
                }
            }

            return Vector3.zero; // 有効な位置が見つからない
        }

        /// <summary>
        /// 利用可能なスポーンポイントを取得
        /// </summary>
        private List<Vector3> GetAvailableSpawnPoints()
        {
            var available = new List<Vector3>();

            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint == null) continue;

                bool isOccupied = activeItems.Values.Any(item =>
                    item != null && Vector3.Distance(item.transform.position, spawnPoint.position) < spawnRadius);

                if (!isOccupied)
                {
                    available.Add(spawnPoint.position);
                }
            }

            return available;
        }

        /// <summary>
        /// スポーン履歴を記録
        /// </summary>
        private void RecordSpawnHistory(ResourceType resourceType, Vector3 position)
        {
            lastSpawnPositions[resourceType].Add(position);
            lastSpawnTimes[resourceType] = Time.time;

            // 履歴サイズ制限
            if (lastSpawnPositions[resourceType].Count > 20)
            {
                lastSpawnPositions[resourceType].RemoveAt(0);
            }
        }

        /// <summary>
        /// スポーンエフェクトを再生
        /// </summary>
        private void PlaySpawnEffects(Vector3 position)
        {
            if (spawnEffectPrefab != null)
            {
                var effect = Instantiate(spawnEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, position);
            }
        }

        /// <summary>
        /// リソーススポーン判定
        /// </summary>
        private bool ShouldSpawnResource(ResourceType resourceType)
        {
            float timeSinceLastSpawn = Time.time - lastSpawnTimes[resourceType];
            float minSpawnInterval = 10f; // 最小スポーン間隔

            return timeSinceLastSpawn > minSpawnInterval && Random.Range(0f, 1f) < 0.3f;
        }

        /// <summary>
        /// 緊急アイテムリストを取得
        /// </summary>
        private List<SH_ItemData> GetEmergencyItems(ResourceType resourceType, int count)
        {
            var category = GetCategoryFromResourceType(resourceType);
            var availableItems = itemDatabase.GetItemsByCategory(category).ToList();

            var emergencyItems = new List<SH_ItemData>();
            for (int i = 0; i < count && availableItems.Count > 0; i++)
            {
                var randomItem = availableItems[Random.Range(0, availableItems.Count)];
                emergencyItems.Add(randomItem);
            }

            return emergencyItems;
        }

        /// <summary>
        /// アイテムカテゴリーからリソースタイプを取得
        /// </summary>
        private ResourceType GetResourceType(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Health => ResourceType.Health,
                ItemCategory.Ammunition => ResourceType.Ammunition,
                ItemCategory.Weapon => ResourceType.Ammunition,
                ItemCategory.Key => ResourceType.KeyItem,
                ItemCategory.Save => ResourceType.SaveItem,
                _ => ResourceType.Special
            };
        }

        /// <summary>
        /// リソースタイプからアイテムカテゴリーを取得
        /// </summary>
        private ItemCategory GetCategoryFromResourceType(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Health => ItemCategory.Health,
                ResourceType.Ammunition => ItemCategory.Ammunition,
                ResourceType.KeyItem => ItemCategory.Key,
                ResourceType.SaveItem => ItemCategory.Save,
                _ => ItemCategory.Special
            };
        }

        private void OnDestroy()
        {
            if (autoSpawnCoroutine != null)
            {
                StopCoroutine(autoSpawnCoroutine);
            }
        }

        // Debug Gizmos
        private void OnDrawGizmosSelected()
        {
            if (!showSpawnGizmos || spawnPoints == null) return;

            Gizmos.color = Color.green;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                }
            }
        }

        // Debug GUI
        private void OnGUI()
        {
            if (!Application.isPlaying || !isInitialized) return;

            var stats = GetStatistics();
            var rect = new Rect(Screen.width - 320, 10, 300, 200);
            GUI.Box(rect, "Resource Manager");

            GUILayout.BeginArea(new Rect(Screen.width - 315, 35, 290, 165));
            GUILayout.Label($"Total Items: {stats.TotalActiveItems}");
            GUILayout.Label($"Health: {stats.HealthItems}");
            GUILayout.Label($"Ammunition: {stats.AmmunitionItems}");
            GUILayout.Label($"Save Items: {stats.SaveItems}");
            GUILayout.Label($"Key Items: {stats.KeyItems}");

            if (GUILayout.Button("Emergency Health"))
            {
                ExecuteEmergencySupply(ResourceType.Health, 2);
            }

            if (GUILayout.Button("Clear All Items"))
            {
                ClearAllItems();
            }

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// リソースマネージャー統計情報
    /// </summary>
    [System.Serializable]
    public class ResourceManagerStatistics
    {
        public int TotalActiveItems;
        public int HealthItems;
        public int AmmunitionItems;
        public int SaveItems;
        public int KeyItems;
        public int SpecialItems;
        public Dictionary<ResourceType, ResourceScarcityLevel> ScarcityLevels = new Dictionary<ResourceType, ResourceScarcityLevel>();
    }
}
