using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート用アイテムデータベース
    /// 弾薬、回復アイテム、キーアイテム、特殊アイテムを管理
    /// </summary>
    [CreateAssetMenu(fileName = "SH_ItemDatabase", menuName = "Templates/SurvivalHorror/Item Database")]
    public class SH_ItemDatabase : ScriptableObject
    {
        [Header("Database Configuration")]
        [SerializeField] private string databaseVersion = "1.0.0";
        [SerializeField] private string lastModified;

        [Header("Item Categories")]
        [SerializeField] private List<SH_ItemData> healthItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> weaponItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> ammunitionItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> keyItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> documentItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> specialItems = new List<SH_ItemData>();
        [SerializeField] private List<SH_ItemData> saveItems = new List<SH_ItemData>();

        [Header("Item Rarity Settings")]
        [SerializeField] private ItemRaritySettings commonSettings;
        [SerializeField] private ItemRaritySettings uncommonSettings;
        [SerializeField] private ItemRaritySettings rareSettings;
        [SerializeField] private ItemRaritySettings legendarySettings;

        [Header("Events")]
        [SerializeField] private GameEvent<SH_ItemData> onItemRegistered;
        [SerializeField] private GameEvent<string> onItemRemoved;

        // Runtime Cache
        private Dictionary<string, SH_ItemData> itemCache;
        private Dictionary<ItemCategory, List<SH_ItemData>> categoryCache;
        private bool isCacheInitialized = false;

        public string DatabaseVersion => databaseVersion;
        public IReadOnlyList<SH_ItemData> HealthItems => healthItems;
        public IReadOnlyList<SH_ItemData> WeaponItems => weaponItems;
        public IReadOnlyList<SH_ItemData> AmmunitionItems => ammunitionItems;
        public IReadOnlyList<SH_ItemData> KeyItems => keyItems;
        public IReadOnlyList<SH_ItemData> DocumentItems => documentItems;
        public IReadOnlyList<SH_ItemData> SpecialItems => specialItems;
        public IReadOnlyList<SH_ItemData> SaveItems => saveItems;

        private void OnEnable()
        {
            InitializeCache();
            lastModified = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// アイテムIDから詳細データを取得
        /// </summary>
        public SH_ItemData GetItemById(string itemId)
        {
            if (!isCacheInitialized) InitializeCache();
            return itemCache.GetValueOrDefault(itemId);
        }

        /// <summary>
        /// カテゴリー別アイテムリストを取得
        /// </summary>
        public IReadOnlyList<SH_ItemData> GetItemsByCategory(ItemCategory category)
        {
            if (!isCacheInitialized) InitializeCache();
            return categoryCache.GetValueOrDefault(category, new List<SH_ItemData>());
        }

        /// <summary>
        /// レアリティ別アイテムリストを取得
        /// </summary>
        public IReadOnlyList<SH_ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            if (!isCacheInitialized) InitializeCache();

            return itemCache.Values
                .Where(item => item.Rarity == rarity)
                .ToList();
        }

        /// <summary>
        /// 使用可能なアイテムのみを取得
        /// </summary>
        public IReadOnlyList<SH_ItemData> GetUsableItems()
        {
            if (!isCacheInitialized) InitializeCache();

            return itemCache.Values
                .Where(item => item.IsUsable)
                .ToList();
        }

        /// <summary>
        /// スタック可能なアイテムのみを取得
        /// </summary>
        public IReadOnlyList<SH_ItemData> GetStackableItems()
        {
            if (!isCacheInitialized) InitializeCache();

            return itemCache.Values
                .Where(item => item.IsStackable)
                .ToList();
        }

        /// <summary>
        /// 難易度に応じたアイテム出現率を計算
        /// </summary>
        public float CalculateSpawnRate(SH_ItemData item, DifficultyLevel difficulty)
        {
            if (item == null) return 0f;

            float baseRate = item.BaseSpawnRate;
            float rarityMultiplier = GetRaritySettings(item.Rarity).spawnRateMultiplier;

            float difficultyMultiplier = difficulty switch
            {
                DifficultyLevel.Easy => 1.5f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 0.7f,
                DifficultyLevel.Nightmare => 0.4f,
                _ => 1.0f
            };

            return baseRate * rarityMultiplier * difficultyMultiplier;
        }

        /// <summary>
        /// ランダムアイテムを生成（レアリティ重み付き）
        /// </summary>
        public SH_ItemData GetRandomItem(ItemCategory category = ItemCategory.All)
        {
            var availableItems = category == ItemCategory.All
                ? itemCache.Values.ToList()
                : GetItemsByCategory(category).ToList();

            if (availableItems.Count == 0) return null;

            // レアリティ重み付きランダム選択
            float totalWeight = availableItems.Sum(item => GetRarityWeight(item.Rarity));
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in availableItems)
            {
                currentWeight += GetRarityWeight(item.Rarity);
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }

            return availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
        }

        /// <summary>
        /// アイテムを動的に登録
        /// </summary>
        public bool RegisterItem(SH_ItemData item)
        {
            if (item == null || string.IsNullOrEmpty(item.ItemId)) return false;

            if (itemCache.ContainsKey(item.ItemId))
            {
                Debug.LogWarning($"[SH_ItemDatabase] Item with ID '{item.ItemId}' already exists");
                return false;
            }

            // カテゴリー別リストに追加
            var categoryList = GetCategoryList(item.Category);
            categoryList?.Add(item);

            // キャッシュに追加
            itemCache[item.ItemId] = item;
            if (categoryCache.ContainsKey(item.Category))
            {
                categoryCache[item.Category].Add(item);
            }

            onItemRegistered?.Raise(item);
            Debug.Log($"[SH_ItemDatabase] Registered item: {item.ItemId} ({item.Category})");
            return true;
        }

        /// <summary>
        /// アイテムを削除
        /// </summary>
        public bool RemoveItem(string itemId)
        {
            if (!itemCache.TryGetValue(itemId, out var item)) return false;

            // カテゴリー別リストから削除
            var categoryList = GetCategoryList(item.Category);
            categoryList?.Remove(item);

            // キャッシュから削除
            itemCache.Remove(itemId);
            if (categoryCache.ContainsKey(item.Category))
            {
                categoryCache[item.Category].Remove(item);
            }

            onItemRemoved?.Raise(itemId);
            Debug.Log($"[SH_ItemDatabase] Removed item: {itemId}");
            return true;
        }

        /// <summary>
        /// データベースの統計情報を取得
        /// </summary>
        public DatabaseStatistics GetStatistics()
        {
            if (!isCacheInitialized) InitializeCache();

            return new DatabaseStatistics
            {
                TotalItems = itemCache.Count,
                HealthItemsCount = healthItems.Count,
                WeaponItemsCount = weaponItems.Count,
                AmmunitionItemsCount = ammunitionItems.Count,
                KeyItemsCount = keyItems.Count,
                DocumentItemsCount = documentItems.Count,
                SpecialItemsCount = specialItems.Count,
                SaveItemsCount = saveItems.Count,
                CommonItemsCount = GetItemsByRarity(ItemRarity.Common).Count,
                UncommonItemsCount = GetItemsByRarity(ItemRarity.Uncommon).Count,
                RareItemsCount = GetItemsByRarity(ItemRarity.Rare).Count,
                LegendaryItemsCount = GetItemsByRarity(ItemRarity.Legendary).Count
            };
        }

        /// <summary>
        /// キャッシュを初期化
        /// </summary>
        private void InitializeCache()
        {
            itemCache = new Dictionary<string, SH_ItemData>();
            categoryCache = new Dictionary<ItemCategory, List<SH_ItemData>>();

            // 全アイテムをキャッシュに登録
            RegisterItemsToCache(healthItems, ItemCategory.Health);
            RegisterItemsToCache(weaponItems, ItemCategory.Weapon);
            RegisterItemsToCache(ammunitionItems, ItemCategory.Ammunition);
            RegisterItemsToCache(keyItems, ItemCategory.Key);
            RegisterItemsToCache(documentItems, ItemCategory.Document);
            RegisterItemsToCache(specialItems, ItemCategory.Special);
            RegisterItemsToCache(saveItems, ItemCategory.Save);

            isCacheInitialized = true;
        }

        private void RegisterItemsToCache(List<SH_ItemData> items, ItemCategory category)
        {
            categoryCache[category] = new List<SH_ItemData>();

            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId)) continue;

                if (itemCache.ContainsKey(item.ItemId))
                {
                    Debug.LogError($"[SH_ItemDatabase] Duplicate item ID: {item.ItemId}");
                    continue;
                }

                itemCache[item.ItemId] = item;
                categoryCache[category].Add(item);
            }
        }

        private List<SH_ItemData> GetCategoryList(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Health => healthItems,
                ItemCategory.Weapon => weaponItems,
                ItemCategory.Ammunition => ammunitionItems,
                ItemCategory.Key => keyItems,
                ItemCategory.Document => documentItems,
                ItemCategory.Special => specialItems,
                ItemCategory.Save => saveItems,
                _ => null
            };
        }

        private ItemRaritySettings GetRaritySettings(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonSettings,
                ItemRarity.Uncommon => uncommonSettings,
                ItemRarity.Rare => rareSettings,
                ItemRarity.Legendary => legendarySettings,
                _ => commonSettings
            };
        }

        private float GetRarityWeight(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 50f,
                ItemRarity.Uncommon => 30f,
                ItemRarity.Rare => 15f,
                ItemRarity.Legendary => 5f,
                _ => 1f
            };
        }

        /// <summary>
        /// データベースの整合性を検証
        /// </summary>
        public bool ValidateDatabase()
        {
            bool isValid = true;
            var allItemIds = new HashSet<string>();

            // 重複IDチェック
            foreach (var itemList in new[] { healthItems, weaponItems, ammunitionItems, keyItems, documentItems, specialItems, saveItems })
            {
                foreach (var item in itemList)
                {
                    if (item == null) continue;

                    if (!allItemIds.Add(item.ItemId))
                    {
                        Debug.LogError($"[SH_ItemDatabase] Duplicate item ID found: {item.ItemId}");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        // エディタ専用デバッグ機能
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/SurvivalHorror/Validate Item Database")]
        private static void ValidateItemDatabaseFromMenu()
        {
            var databases = UnityEngine.Resources.LoadAll<SH_ItemDatabase>("");
            foreach (var db in databases)
            {
                db.ValidateDatabase();
            }
        }

        [ContextMenu("Generate Sample Items")]
        private void GenerateSampleItems()
        {
            // 開発用サンプルアイテム生成
            if (healthItems.Count == 0)
            {
                var sampleHealth = ScriptableObject.CreateInstance<SH_ItemData>();
                sampleHealth.Initialize("health_potion", "体力回復薬", "体力を50回復する", ItemCategory.Health, ItemRarity.Common);
                healthItems.Add(sampleHealth);
            }
        }
        #endif
    }

    /// <summary>
    /// データベース統計情報
    /// </summary>
    [System.Serializable]
    public class DatabaseStatistics
    {
        public int TotalItems;
        public int HealthItemsCount;
        public int WeaponItemsCount;
        public int AmmunitionItemsCount;
        public int KeyItemsCount;
        public int DocumentItemsCount;
        public int SpecialItemsCount;
        public int SaveItemsCount;
        public int CommonItemsCount;
        public int UncommonItemsCount;
        public int RareItemsCount;
        public int LegendaryItemsCount;
    }

    /// <summary>
    /// アイテムレアリティ設定
    /// </summary>
    [System.Serializable]
    public class ItemRaritySettings
    {
        [Header("Spawn Settings")]
        [Range(0f, 2f)] public float spawnRateMultiplier = 1.0f;
        [Range(0f, 1f)] public float baseDropChance = 0.1f;

        [Header("Visual Settings")]
        public Color rarityColor = Color.white;
        public Material rarityMaterial;
        public ParticleSystem rarityEffect;

        [Header("Audio Settings")]
        public AudioClip pickupSound;
        public AudioClip discoverySound;
    }
}
