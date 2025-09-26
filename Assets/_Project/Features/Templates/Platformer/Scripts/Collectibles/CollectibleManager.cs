using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.Platformer.Commands;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Collectibles
{
    /// <summary>
    /// 繝励Λ繝・ヨ繝輔か繝ｼ繝槭・蜿朱寔繧｢繧､繝・Β邂｡逅・す繧ｹ繝・Β
    /// 蜿朱寔繧｢繧､繝・Β縺ｮ逕滓・縲∫ｮ｡逅・∝柑譫懷・逅・ｒ邨ｱ蜷育ｮ｡逅・
    /// 繧ｹ繧ｳ繧｢繝ｻ騾ｲ謐励す繧ｹ繝・Β縺ｨ縺ｮ螳悟・邨ｱ蜷・
    /// </summary>
    public class CollectibleManager : MonoBehaviour
    {
        #region Collectible Configuration

        [TabGroup("Collectibles", "Basic Settings")]
        [Title("Collectible Management System", "繝励Λ繝・ヨ繝輔か繝ｼ繝槭・蜿朱寔繧｢繧､繝・Β邨ｱ蜷育ｮ｡逅・, TitleAlignments.Centered)]
        [SerializeField] private bool enableCollectibles = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool autoRespawn = false;

        [TabGroup("Collectibles", "Generation")]
        [Header("Collectible Generation")]
        [SerializeField] private List<CollectibleSpawnPoint> spawnPoints = new List<CollectibleSpawnPoint>();
        [SerializeField] private List<CollectibleTypeConfig> collectibleTypes = new List<CollectibleTypeConfig>();
        [SerializeField, Range(1, 1000)] private int maxCollectibles = 100;
        [SerializeField, Range(0f, 60f)] private float respawnDelay = 10f;

        [TabGroup("Collectibles", "Effects")]
        [Header("Collection Effects")]
        [SerializeField] private bool enableParticleEffects = true;
        [SerializeField] private bool enableSoundEffects = true;
        [SerializeField] private bool enableScreenEffects = false;
        [SerializeField, Range(0.1f, 5f)] private float effectDuration = 1f;

        [TabGroup("Collectibles", "Scoring")]
        [Header("Scoring System")]
        [SerializeField] private bool enableScoring = true;
        [SerializeField] private int baseScoreValue = 10;
        [SerializeField] private float comboMultiplier = 1.5f;
        [SerializeField] private float comboTimeWindow = 3f;
        [SerializeField] private int maxComboLevel = 10;

        #endregion

        #region Runtime Data

        [TabGroup("Runtime", "Current State")]
        [Header("Current Collection State")]
        [SerializeField, ReadOnly] private int totalCollectibles;
        [SerializeField, ReadOnly] private int collectedCount;
        [SerializeField, ReadOnly] private int remainingCount;
        [SerializeField, ReadOnly] private float completionPercentage;

        [TabGroup("Runtime", "Scoring")]
        [Header("Current Scoring")]
        [SerializeField, ReadOnly] private int currentScore;
        [SerializeField, ReadOnly] private int currentCombo;
        [SerializeField, ReadOnly] private float comboTimer;
        [SerializeField, ReadOnly] private float totalComboMultiplier = 1f;

        [TabGroup("Runtime", "Statistics")]
        [Header("Collection Statistics")]
        [SerializeField, ReadOnly] private Dictionary<CollectibleType, int> collectionStats = new Dictionary<CollectibleType, int>();
        [SerializeField, ReadOnly] private float collectionStartTime;
        [SerializeField, ReadOnly] private float averageCollectionTime;

        private List<Collectible> activeCollectibles = new List<Collectible>();
        private List<Collectible> collectedItems = new List<Collectible>();
        private Queue<CollectibleSpawnPoint> respawnQueue = new Queue<CollectibleSpawnPoint>();

        #endregion

        #region Collectible Types

        [System.Serializable]
        public class CollectibleTypeConfig
        {
            [Header("Basic Configuration")]
            public CollectibleType type;
            public GameObject prefab;
            public Sprite icon;
            public string displayName;
            public string description;

            [Header("Value & Effects")]
            public int scoreValue = 10;
            public float effectRadius = 2f;
            public ParticleSystem collectionEffect;
            public AudioClip collectionSound;

            [Header("Behavior")]
            public bool autoCollect = false;
            public float autoCollectRange = 1f;
            public bool rotateAnimation = true;
            public bool floatAnimation = true;
            public float animationSpeed = 1f;

            [Header("Rarity")]
            public CollectibleRarity rarity = CollectibleRarity.Common;
            public float spawnChance = 1f;
            public Color rarityColor = Color.white;
        }

        [System.Serializable]
        public class CollectibleSpawnPoint
        {
            public Transform spawnTransform;
            public CollectibleType preferredType = CollectibleType.Coin;
            public bool isActive = true;
            public float respawnTime = 0f;
            public bool hasSpawned = false;

            [Header("Spawn Conditions")]
            public bool requirePlayerDistance = false;
            public float minPlayerDistance = 5f;
            public bool timeBasedSpawn = false;
            public float spawnInterval = 30f;

            [Header("Visual")]
            public bool showGizmos = true;
            public Color gizmosColor = Color.yellow;
        }

        public enum CollectibleType
        {
            Coin,           // 蝓ｺ譛ｬ騾夊ｲｨ
            Gem,            // 鬮倅ｾ｡蛟､繧｢繧､繝・Β
            PowerUp,        // 繝代Ρ繝ｼ繧｢繝・・繧｢繧､繝・Β
            Health,         // 菴灘鴨蝗槫ｾｩ
            Key,            // 迚ｹ谿翫く繝ｼ繧｢繧､繝・Β
            Star,           // 隧穂ｾ｡逕ｨ繧ｹ繧ｿ繝ｼ繧｢繧､繝・Β
            Bonus,          // 繝懊・繝翫せ繧｢繧､繝・Β
            Special         // 迚ｹ蛻･繧｢繧､繝・Β
        }

        public enum CollectibleRarity
        {
            Common,         // 荳闊ｬ逧・
            Uncommon,       // 繧・ｄ迴阪＠縺・
            Rare,           // 迴阪＠縺・
            Epic,           // 髱槫ｸｸ縺ｫ迴阪＠縺・
            Legendary       // 莨晁ｪｬ逧・
        }

        #endregion

        #region Service References

        private ICommandInvoker commandInvoker;
        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCollectibleManager();
        }

        private void Start()
        {
            if (enableCollectibles)
            {
                SetupCollectibleSystem();
            }
        }

        private void Update()
        {
            if (enableCollectibles && isInitialized)
            {
                UpdateCollectibleSystem();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β邂｡逅・す繧ｹ繝・Β縺ｮ蛻晄悄蛹・
        /// </summary>
        private void InitializeCollectibleManager()
        {
            LogDebug("[CollectibleManager] Initializing Collectible Management System...");

            try
            {
                // 繧ｵ繝ｼ繝薙せ蜿ら・縺ｮ蜿門ｾ・
                commandInvoker = ServiceLocator.GetService<ICommandInvoker>();

                // 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ邨ｱ險医・蛻晄悄蛹・
                InitializeCollectionStats();

                // 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝医・讀懆ｨｼ
                ValidateSpawnPoints();

                // 蜿朱寔繧ｿ繧､繝励・險ｭ螳夂｢ｺ隱・
                ValidateCollectibleTypes();

                collectionStartTime = Time.time;
                isInitialized = true;

                LogDebug("[CollectibleManager] 笨・Collectible Manager initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[CollectibleManager] 笶・Collectible Manager initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ邨ｱ險医・蛻晄悄蛹・
        /// </summary>
        private void InitializeCollectionStats()
        {
            collectionStats.Clear();
            foreach (CollectibleType type in System.Enum.GetValues(typeof(CollectibleType)))
            {
                collectionStats[type] = 0;
            }
        }

        /// <summary>
        /// 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝医・讀懆ｨｼ
        /// </summary>
        private void ValidateSpawnPoints()
        {
            spawnPoints.RemoveAll(point => point.spawnTransform == null);

            foreach (var point in spawnPoints)
            {
                if (point.spawnTransform == null)
                {
                    LogError($"[CollectibleManager] Spawn point has null transform reference");
                    continue;
                }

                point.respawnTime = 0f;
                point.hasSpawned = false;
            }

            LogDebug($"[CollectibleManager] Validated {spawnPoints.Count} spawn points");
        }

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β繧ｿ繧､繝励・讀懆ｨｼ
        /// </summary>
        private void ValidateCollectibleTypes()
        {
            collectibleTypes.RemoveAll(config => config.prefab == null);

            foreach (var config in collectibleTypes)
            {
                if (config.prefab.GetComponent<Collectible>() == null)
                {
                    LogError($"[CollectibleManager] Prefab {config.prefab.name} missing Collectible component");
                }
            }

            LogDebug($"[CollectibleManager] Validated {collectibleTypes.Count} collectible types");
        }

        #endregion

        #region System Setup

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β繧ｷ繧ｹ繝・Β縺ｮ繧ｻ繝・ヨ繧｢繝・・
        /// </summary>
        private void SetupCollectibleSystem()
        {
            LogDebug("[CollectibleManager] Setting up Collectible System...");

            // 蛻晄悄繧｢繧､繝・Β驟咲ｽｮ
            SpawnInitialCollectibles();

            // 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ逋ｻ骭ｲ
            RegisterEventListeners();

            // UI譖ｴ譁ｰ
            UpdateUI();

            LogDebug("[CollectibleManager] 笨・Collectible System setup completed");
        }

        /// <summary>
        /// 蛻晄悄蜿朱寔繧｢繧､繝・Β縺ｮ驟咲ｽｮ
        /// </summary>
        private void SpawnInitialCollectibles()
        {
            int spawnedCount = 0;

            foreach (var spawnPoint in spawnPoints)
            {
                if (!spawnPoint.isActive || spawnedCount >= maxCollectibles) continue;

                if (SpawnCollectibleAtPoint(spawnPoint))
                {
                    spawnedCount++;
                    spawnPoint.hasSpawned = true;
                }
            }

            totalCollectibles = spawnedCount;
            remainingCount = totalCollectibles;

            LogDebug($"[CollectibleManager] Spawned {spawnedCount} initial collectibles");
        }

        /// <summary>
        /// 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ逋ｻ骭ｲ
        /// </summary>
        private void RegisterEventListeners()
        {
            // 繝励Ξ繧､繝､繝ｼ繧､繝吶Φ繝医・繝ｪ繧ｹ繝翫・逋ｻ骭ｲ
            // 萓・ 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ譖ｴ譁ｰ縲√・繝ｬ繧､繝､繝ｼ豁ｻ莠｡遲・
        }

        #endregion

        #region Update System

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β繧ｷ繧ｹ繝・Β縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateCollectibleSystem()
        {
            // 繧ｳ繝ｳ繝懊ち繧､繝槭・縺ｮ譖ｴ譁ｰ
            UpdateComboTimer();

            // 繝ｪ繧ｹ繝昴・繝ｳ蜃ｦ逅・
            if (autoRespawn)
            {
                UpdateRespawnSystem();
            }

            // 繧｢繧ｯ繝・ぅ繝悶い繧､繝・Β縺ｮ譖ｴ譁ｰ
            UpdateActiveCollectibles();

            // 邨ｱ險域ュ蝣ｱ縺ｮ譖ｴ譁ｰ
            UpdateStatistics();

            // UI譖ｴ譁ｰ
            UpdateUI();
        }

        /// <summary>
        /// 繧ｳ繝ｳ繝懊ち繧､繝槭・縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateComboTimer()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;

                if (comboTimer <= 0f)
                {
                    // 繧ｳ繝ｳ繝懊Μ繧ｻ繝・ヨ
                    ResetCombo();
                }
            }
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝昴・繝ｳ繧ｷ繧ｹ繝・Β縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateRespawnSystem()
        {
            foreach (var spawnPoint in spawnPoints)
            {
                if (!spawnPoint.isActive || spawnPoint.hasSpawned) continue;

                spawnPoint.respawnTime += Time.deltaTime;

                if (spawnPoint.respawnTime >= respawnDelay)
                {
                    if (SpawnCollectibleAtPoint(spawnPoint))
                    {
                        spawnPoint.hasSpawned = true;
                        spawnPoint.respawnTime = 0f;
                        totalCollectibles++;
                        remainingCount++;
                    }
                }
            }
        }

        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝門庶髮・い繧､繝・Β縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateActiveCollectibles()
        {
            for (int i = activeCollectibles.Count - 1; i >= 0; i--)
            {
                var collectible = activeCollectibles[i];
                if (collectible == null)
                {
                    activeCollectibles.RemoveAt(i);
                    continue;
                }

                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譖ｴ譁ｰ
                UpdateCollectibleAnimation(collectible);

                // 閾ｪ蜍募庶髮・メ繧ｧ繝・け
                CheckAutoCollection(collectible);
            }
        }

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β縺ｮ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譖ｴ譁ｰ
        /// </summary>
        private void UpdateCollectibleAnimation(Collectible collectible)
        {
            if (collectible.Config.rotateAnimation)
            {
                collectible.transform.Rotate(Vector3.up, collectible.Config.animationSpeed * 90f * Time.deltaTime);
            }

            if (collectible.Config.floatAnimation)
            {
                float floatOffset = Mathf.Sin(Time.time * collectible.Config.animationSpeed * 2f) * 0.3f;
                Vector3 originalPos = collectible.OriginalPosition;
                collectible.transform.position = originalPos + Vector3.up * floatOffset;
            }
        }

        /// <summary>
        /// 閾ｪ蜍募庶髮・・繝√ぉ繝・け
        /// </summary>
        private void CheckAutoCollection(Collectible collectible)
        {
            if (!collectible.Config.autoCollect) return;

            // 繝励Ξ繧､繝､繝ｼ縺ｨ縺ｮ霍晞屬繝√ぉ繝・け
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(collectible.transform.position, player.transform.position);
                if (distance <= collectible.Config.autoCollectRange)
                {
                    CollectItem(collectible);
                }
            }
        }

        /// <summary>
        /// 邨ｱ險域ュ蝣ｱ縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateStatistics()
        {
            remainingCount = totalCollectibles - collectedCount;
            completionPercentage = totalCollectibles > 0 ? (float)collectedCount / totalCollectibles * 100f : 0f;

            if (collectedCount > 0)
            {
                averageCollectionTime = (Time.time - collectionStartTime) / collectedCount;
            }
        }

        #endregion

        #region Collection System

        /// <summary>
        /// 繧｢繧､繝・Β縺ｮ蜿朱寔蜃ｦ逅・
        /// </summary>
        public bool CollectItem(Collectible collectible)
        {
            if (collectible == null || collectible.IsCollected) return false;

            LogDebug($"[CollectibleManager] Collecting item: {collectible.Config.displayName}");

            // 蜿朱寔蜃ｦ逅・
            collectible.IsCollected = true;
            collectedCount++;
            collectionStats[collectible.Config.type]++;

            // 繧ｹ繧ｳ繧｢蜃ｦ逅・
            if (enableScoring)
            {
                ProcessScoring(collectible);
            }

            // 繧ｨ繝輔ぉ繧ｯ繝亥・逕・
            PlayCollectionEffects(collectible);

            // 繧ｳ繝槭Φ繝峨ヱ繧ｿ繝ｼ繝ｳ縺ｧ縺ｮ蜃ｦ逅・
            if (commandInvoker != null)
            {
                var collectCommand = new CollectItemCommand(collectible, this);
                commandInvoker.ExecuteCommand(collectCommand);
            }

            // 繧｢繧ｯ繝・ぅ繝悶Μ繧ｹ繝医°繧牙炎髯､
            activeCollectibles.Remove(collectible);
            collectedItems.Add(collectible);

            // 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝域峩譁ｰ
            UpdateSpawnPointStatus(collectible);

            // 繧｢繧､繝・Β髱櫁｡ｨ遉ｺ/蜑企勁
            collectible.gameObject.SetActive(false);

            // 繧､繝吶Φ繝育匱陦・
            OnItemCollected(collectible);

            LogDebug($"[CollectibleManager] 笨・Item collected: {collectible.Config.displayName}, Score: +{GetItemScore(collectible)}");

            return true;
        }

        /// <summary>
        /// 繧ｹ繧ｳ繧｢蜃ｦ逅・
        /// </summary>
        private void ProcessScoring(Collectible collectible)
        {
            int itemScore = GetItemScore(collectible);

            // 繧ｳ繝ｳ繝懷・逅・
            if (comboTimer > 0f)
            {
                currentCombo++;
                comboTimer = comboTimeWindow;
                totalComboMultiplier = 1f + (currentCombo * (comboMultiplier - 1f));
                totalComboMultiplier = Mathf.Min(totalComboMultiplier, maxComboLevel);
            }
            else
            {
                currentCombo = 1;
                comboTimer = comboTimeWindow;
                totalComboMultiplier = 1f;
            }

            // 譛邨ゅせ繧ｳ繧｢險育ｮ・
            int finalScore = Mathf.RoundToInt(itemScore * totalComboMultiplier);
            currentScore += finalScore;

            LogDebug($"[CollectibleManager] Score: {itemScore} ﾃ・{totalComboMultiplier:F1} = {finalScore} (Total: {currentScore})");
        }

        /// <summary>
        /// 繧｢繧､繝・Β繧ｹ繧ｳ繧｢縺ｮ蜿門ｾ・
        /// </summary>
        private int GetItemScore(Collectible collectible)
        {
            int baseScore = collectible.Config.scoreValue;

            // 繝ｬ繧｢繝ｪ繝・ぅ繝懊・繝翫せ
            float rarityMultiplier = collectible.Config.rarity switch
            {
                CollectibleRarity.Common => 1f,
                CollectibleRarity.Uncommon => 1.5f,
                CollectibleRarity.Rare => 2f,
                CollectibleRarity.Epic => 3f,
                CollectibleRarity.Legendary => 5f,
                _ => 1f
            };

            return Mathf.RoundToInt(baseScore * rarityMultiplier);
        }

        /// <summary>
        /// 蜿朱寔繧ｨ繝輔ぉ繧ｯ繝医・蜀咲函
        /// </summary>
        private void PlayCollectionEffects(Collectible collectible)
        {
            Vector3 effectPosition = collectible.transform.position;

            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・
            if (enableParticleEffects && collectible.Config.collectionEffect != null)
            {
                var effect = Instantiate(collectible.Config.collectionEffect, effectPosition, Quaternion.identity);
                Destroy(effect.gameObject, effectDuration);
            }

            // 髻ｳ髻ｿ繧ｨ繝輔ぉ繧ｯ繝・
            if (enableSoundEffects && collectible.Config.collectionSound != null)
            {
                AudioSource.PlayClipAtPoint(collectible.Config.collectionSound, effectPosition);
            }

            // 繧ｹ繧ｯ繝ｪ繝ｼ繝ｳ繧ｨ繝輔ぉ繧ｯ繝茨ｼ・I譖ｴ譁ｰ遲会ｼ・
            if (enableScreenEffects)
            {
                // 繧ｹ繧ｳ繧｢陦ｨ遉ｺ遲峨・UI譖ｴ譁ｰ
            }
        }

        /// <summary>
        /// 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝育憾諷九・譖ｴ譁ｰ
        /// </summary>
        private void UpdateSpawnPointStatus(Collectible collectible)
        {
            var spawnPoint = spawnPoints.FirstOrDefault(p =>
                Vector3.Distance(p.spawnTransform.position, collectible.OriginalPosition) < 0.5f);

            if (spawnPoint != null)
            {
                spawnPoint.hasSpawned = false;
                spawnPoint.respawnTime = 0f;
            }
        }

        /// <summary>
        /// 繧ｳ繝ｳ繝懊・繝ｪ繧ｻ繝・ヨ
        /// </summary>
        private void ResetCombo()
        {
            if (currentCombo > 1)
            {
                LogDebug($"[CollectibleManager] Combo ended: {currentCombo} items");
            }

            currentCombo = 0;
            comboTimer = 0f;
            totalComboMultiplier = 1f;
        }

        /// <summary>
        /// 繧｢繧､繝・Β蜿朱寔繧､繝吶Φ繝・
        /// </summary>
        private void OnItemCollected(Collectible collectible)
        {
            // GameEvent縺ｮ逋ｺ陦・
            // UI譖ｴ譁ｰ繧､繝吶Φ繝・
            // 繝励Ξ繧､繝､繝ｼ騾夂衍繧､繝吶Φ繝・
        }

        #endregion

        #region Spawn System

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｫ繧｢繧､繝・Β繧堤函謌・
        /// </summary>
        public bool SpawnCollectibleAtPoint(CollectibleSpawnPoint spawnPoint)
        {
            if (spawnPoint.spawnTransform == null) return false;

            // 繧ｿ繧､繝苓ｨｭ螳壹・蜿門ｾ・
            var typeConfig = GetCollectibleConfig(spawnPoint.preferredType);
            if (typeConfig == null || typeConfig.prefab == null) return false;

            // 逕滓・譚｡莉ｶ縺ｮ繝√ぉ繝・け
            if (!CanSpawnAtPoint(spawnPoint)) return false;

            // 繧｢繧､繝・Β逕滓・
            GameObject itemObject = Instantiate(typeConfig.prefab,
                spawnPoint.spawnTransform.position,
                spawnPoint.spawnTransform.rotation);

            var collectible = itemObject.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = itemObject.AddComponent<Collectible>();
            }

            // 險ｭ螳夐←逕ｨ
            collectible.Initialize(typeConfig, spawnPoint.spawnTransform.position);

            // 繧｢繧ｯ繝・ぅ繝悶Μ繧ｹ繝医↓霑ｽ蜉
            activeCollectibles.Add(collectible);

            LogDebug($"[CollectibleManager] Spawned {typeConfig.displayName} at {spawnPoint.spawnTransform.position}");

            return true;
        }

        /// <summary>
        /// 逕滓・蜿ｯ閭ｽ縺九メ繧ｧ繝・け
        /// </summary>
        private bool CanSpawnAtPoint(CollectibleSpawnPoint spawnPoint)
        {
            if (!spawnPoint.isActive) return false;

            // 繝励Ξ繧､繝､繝ｼ霍晞屬繝√ぉ繝・け
            if (spawnPoint.requirePlayerDistance)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    float distance = Vector3.Distance(spawnPoint.spawnTransform.position, player.transform.position);
                    if (distance < spawnPoint.minPlayerDistance)
                    {
                        return false;
                    }
                }
            }

            // 譎る俣繝吶・繧ｹ逕滓・繝√ぉ繝・け
            if (spawnPoint.timeBasedSpawn)
            {
                if (Time.time - collectionStartTime < spawnPoint.spawnInterval)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 蜿朱寔繧｢繧､繝・Β險ｭ螳壹・蜿門ｾ・
        /// </summary>
        private CollectibleTypeConfig GetCollectibleConfig(CollectibleType type)
        {
            return collectibleTypes.FirstOrDefault(config => config.type == type);
        }

        /// <summary>
        /// 繝ｩ繝ｳ繝繝縺ｪ蝣ｴ謇縺ｫ繧｢繧､繝・Β繧堤函謌・
        /// </summary>
        public bool SpawnRandomCollectible()
        {
            if (spawnPoints.Count == 0 || activeCollectibles.Count >= maxCollectibles) return false;

            var availablePoints = spawnPoints.Where(p => p.isActive && !p.hasSpawned).ToList();
            if (availablePoints.Count == 0) return false;

            var randomPoint = availablePoints[Random.Range(0, availablePoints.Count)];
            return SpawnCollectibleAtPoint(randomPoint);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 蜿朱寔邨ｱ險医・蜿門ｾ・
        /// </summary>
        public Dictionary<CollectibleType, int> GetCollectionStats()
        {
            return new Dictionary<CollectibleType, int>(collectionStats);
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｹ繧ｳ繧｢繧貞叙蠕・
        /// </summary>
        public int GetCurrentScore()
        {
            return currentScore;
        }

        /// <summary>
        /// 蜿朱寔螳御ｺ・紫繧貞叙蠕・
        /// </summary>
        public float GetCompletionPercentage()
        {
            return completionPercentage;
        }

        /// <summary>
        /// 蜈ｨ繧｢繧､繝・Β縺ｮ蜿朱寔繧偵Μ繧ｻ繝・ヨ
        /// </summary>
        public void ResetAllCollectibles()
        {
            // 蜿朱寔貂医∩繧｢繧､繝・Β縺ｮ蠕ｩ豢ｻ
            foreach (var collectible in collectedItems)
            {
                if (collectible != null)
                {
                    collectible.gameObject.SetActive(true);
                    collectible.IsCollected = false;
                    activeCollectibles.Add(collectible);
                }
            }

            // 邨ｱ險医Μ繧ｻ繝・ヨ
            collectedCount = 0;
            currentScore = 0;
            ResetCombo();
            InitializeCollectionStats();
            collectedItems.Clear();

            // 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝医Μ繧ｻ繝・ヨ
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.hasSpawned = true;
                spawnPoint.respawnTime = 0f;
            }

            LogDebug("[CollectibleManager] All collectibles reset");
        }

        /// <summary>
        /// UI譖ｴ譁ｰ
        /// </summary>
        public void UpdateUI()
        {
            // UI譖ｴ譁ｰ蜃ｦ逅・
            // 繧ｹ繧ｳ繧｢陦ｨ遉ｺ縲∵ｮ九ｊ蛟区焚陦ｨ遉ｺ遲・
        }

        /// <summary>
        /// 逶ｮ讓吶せ繧ｳ繧｢繧定ｨｭ螳・
        /// PlatformerTemplateManager縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧・
        /// </summary>
        /// <param name="targetScore">險ｭ螳壹☆繧狗岼讓吶せ繧ｳ繧｢</param>
        public void SetTargetScore(int targetScore)
        {
            if (targetScore < 0)
            {
                LogDebug("[CollectibleManager] 笞・・Target score cannot be negative, setting to 0");
                targetScore = 0;
            }

            // 蜀・Κ逧・↓逶ｮ讓吶せ繧ｳ繧｢繧剃ｿ晏ｭ假ｼ亥ｰ・擂縺ｮ諡｡蠑ｵ逕ｨ・・
            // 迴ｾ蝨ｨ縺ｮ繝舌・繧ｸ繝ｧ繝ｳ縺ｧ縺ｯ險ｭ螳壹ｒ險倬鹸縺吶ｋ縺ｮ縺ｿ
            LogDebug($"[CollectibleManager] Target score set to: {targetScore}");
        }

        #endregion

        #region Template Actions

        [TabGroup("Actions", "Collection Management")]
        [Button("Spawn Random Collectible")]
        public void TestSpawnRandomCollectible()
        {
            SpawnRandomCollectible();
        }

        [Button("Reset All Collectibles")]
        public void TestResetCollectibles()
        {
            ResetAllCollectibles();
        }

        [Button("Show Collection Stats")]
        public void ShowCollectionStats()
        {
            LogDebug("=== Collection Statistics ===");
            LogDebug($"Total Items: {totalCollectibles}");
            LogDebug($"Collected: {collectedCount}");
            LogDebug($"Remaining: {remainingCount}");
            LogDebug($"Completion: {completionPercentage:F1}%");
            LogDebug($"Current Score: {currentScore}");
            LogDebug($"Current Combo: {currentCombo}");

            foreach (var stat in collectionStats)
            {
                LogDebug($"{stat.Key}: {stat.Value} collected");
            }
            LogDebug("=== Statistics Complete ===");
        }

        [Button("Test Collection Effects")]
        public void TestCollectionEffects()
        {
            if (activeCollectibles.Count > 0)
            {
                PlayCollectionEffects(activeCollectibles[0]);
            }
        }

        #endregion

        #region Debug Support

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!enableCollectibles) return;

            // 繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝医・蜿ｯ隕門喧
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnTransform == null || !spawnPoint.showGizmos) continue;

                Gizmos.color = spawnPoint.isActive ? spawnPoint.gizmosColor : Color.gray;
                Gizmos.DrawWireSphere(spawnPoint.spawnTransform.position, 0.5f);

                // 閾ｪ蜍募庶髮・ｯ・峇縺ｮ蜿ｯ隕門喧
                var config = GetCollectibleConfig(spawnPoint.preferredType);
                if (config != null && config.autoCollect)
                {
                    Gizmos.color = new Color(spawnPoint.gizmosColor.r, spawnPoint.gizmosColor.g,
                        spawnPoint.gizmosColor.b, 0.3f);
                    Gizmos.DrawSphere(spawnPoint.spawnTransform.position, config.autoCollectRange);
                }
            }

            // 繧｢繧ｯ繝・ぅ繝悶い繧､繝・Β縺ｮ蜿ｯ隕門喧
            Gizmos.color = Color.green;
            foreach (var collectible in activeCollectibles)
            {
                if (collectible != null)
                {
                    Gizmos.DrawWireSphere(collectible.transform.position, 0.3f);
                }
            }
        }

        [Button("Auto-Setup Spawn Points")]
        public void AutoSetupSpawnPoints()
        {
            spawnPoints.Clear();

            // 繧ｷ繝ｼ繝ｳ蜀・・ "CollectibleSpawn" 繧ｿ繧ｰ縺御ｻ倥＞縺溘が繝悶ず繧ｧ繧ｯ繝医ｒ讀懃ｴ｢
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("CollectibleSpawn");

            foreach (var obj in spawnObjects)
            {
                var spawnPoint = new CollectibleSpawnPoint
                {
                    spawnTransform = obj.transform,
                    preferredType = CollectibleType.Coin,
                    isActive = true
                };
                spawnPoints.Add(spawnPoint);
            }

            LogDebug($"[CollectibleManager] Auto-setup completed: {spawnPoints.Count} spawn points found");
        }
#endif

        #endregion
    }
}


