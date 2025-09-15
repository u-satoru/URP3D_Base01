using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.Platformer.Commands;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Collectibles
{
    /// <summary>
    /// プラットフォーマー収集アイテム管理システム
    /// 収集アイテムの生成、管理、効果処理を統合管理
    /// スコア・進捗システムとの完全統合
    /// </summary>
    public class CollectibleManager : MonoBehaviour
    {
        #region Collectible Configuration

        [TabGroup("Collectibles", "Basic Settings")]
        [Title("Collectible Management System", "プラットフォーマー収集アイテム統合管理", TitleAlignments.Centered)]
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
            Coin,           // 基本通貨
            Gem,            // 高価値アイテム
            PowerUp,        // パワーアップアイテム
            Health,         // 体力回復
            Key,            // 特殊キーアイテム
            Star,           // 評価用スターアイテム
            Bonus,          // ボーナスアイテム
            Special         // 特別アイテム
        }

        public enum CollectibleRarity
        {
            Common,         // 一般的
            Uncommon,       // やや珍しい
            Rare,           // 珍しい
            Epic,           // 非常に珍しい
            Legendary       // 伝説的
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
        /// 収集アイテム管理システムの初期化
        /// </summary>
        private void InitializeCollectibleManager()
        {
            LogDebug("[CollectibleManager] Initializing Collectible Management System...");

            try
            {
                // サービス参照の取得
                commandInvoker = ServiceLocator.GetService<ICommandInvoker>();

                // コレクション統計の初期化
                InitializeCollectionStats();

                // スポーンポイントの検証
                ValidateSpawnPoints();

                // 収集タイプの設定確認
                ValidateCollectibleTypes();

                collectionStartTime = Time.time;
                isInitialized = true;

                LogDebug("[CollectibleManager] ✅ Collectible Manager initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[CollectibleManager] ❌ Collectible Manager initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// コレクション統計の初期化
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
        /// スポーンポイントの検証
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
        /// 収集アイテムタイプの検証
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
        /// 収集アイテムシステムのセットアップ
        /// </summary>
        private void SetupCollectibleSystem()
        {
            LogDebug("[CollectibleManager] Setting up Collectible System...");

            // 初期アイテム配置
            SpawnInitialCollectibles();

            // イベントリスナーの登録
            RegisterEventListeners();

            // UI更新
            UpdateUI();

            LogDebug("[CollectibleManager] ✅ Collectible System setup completed");
        }

        /// <summary>
        /// 初期収集アイテムの配置
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
        /// イベントリスナーの登録
        /// </summary>
        private void RegisterEventListeners()
        {
            // プレイヤーイベントのリスナー登録
            // 例: プレイヤー位置更新、プレイヤー死亡等
        }

        #endregion

        #region Update System

        /// <summary>
        /// 収集アイテムシステムの更新
        /// </summary>
        private void UpdateCollectibleSystem()
        {
            // コンボタイマーの更新
            UpdateComboTimer();

            // リスポーン処理
            if (autoRespawn)
            {
                UpdateRespawnSystem();
            }

            // アクティブアイテムの更新
            UpdateActiveCollectibles();

            // 統計情報の更新
            UpdateStatistics();

            // UI更新
            UpdateUI();
        }

        /// <summary>
        /// コンボタイマーの更新
        /// </summary>
        private void UpdateComboTimer()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;

                if (comboTimer <= 0f)
                {
                    // コンボリセット
                    ResetCombo();
                }
            }
        }

        /// <summary>
        /// リスポーンシステムの更新
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
        /// アクティブ収集アイテムの更新
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

                // アニメーション更新
                UpdateCollectibleAnimation(collectible);

                // 自動収集チェック
                CheckAutoCollection(collectible);
            }
        }

        /// <summary>
        /// 収集アイテムのアニメーション更新
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
        /// 自動収集のチェック
        /// </summary>
        private void CheckAutoCollection(Collectible collectible)
        {
            if (!collectible.Config.autoCollect) return;

            // プレイヤーとの距離チェック
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
        /// 統計情報の更新
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
        /// アイテムの収集処理
        /// </summary>
        public bool CollectItem(Collectible collectible)
        {
            if (collectible == null || collectible.IsCollected) return false;

            LogDebug($"[CollectibleManager] Collecting item: {collectible.Config.displayName}");

            // 収集処理
            collectible.IsCollected = true;
            collectedCount++;
            collectionStats[collectible.Config.type]++;

            // スコア処理
            if (enableScoring)
            {
                ProcessScoring(collectible);
            }

            // エフェクト再生
            PlayCollectionEffects(collectible);

            // コマンドパターンでの処理
            if (commandInvoker != null)
            {
                var collectCommand = new CollectItemCommand(collectible, this);
                commandInvoker.ExecuteCommand(collectCommand);
            }

            // アクティブリストから削除
            activeCollectibles.Remove(collectible);
            collectedItems.Add(collectible);

            // スポーンポイント更新
            UpdateSpawnPointStatus(collectible);

            // アイテム非表示/削除
            collectible.gameObject.SetActive(false);

            // イベント発行
            OnItemCollected(collectible);

            LogDebug($"[CollectibleManager] ✅ Item collected: {collectible.Config.displayName}, Score: +{GetItemScore(collectible)}");

            return true;
        }

        /// <summary>
        /// スコア処理
        /// </summary>
        private void ProcessScoring(Collectible collectible)
        {
            int itemScore = GetItemScore(collectible);

            // コンボ処理
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

            // 最終スコア計算
            int finalScore = Mathf.RoundToInt(itemScore * totalComboMultiplier);
            currentScore += finalScore;

            LogDebug($"[CollectibleManager] Score: {itemScore} × {totalComboMultiplier:F1} = {finalScore} (Total: {currentScore})");
        }

        /// <summary>
        /// アイテムスコアの取得
        /// </summary>
        private int GetItemScore(Collectible collectible)
        {
            int baseScore = collectible.Config.scoreValue;

            // レアリティボーナス
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
        /// 収集エフェクトの再生
        /// </summary>
        private void PlayCollectionEffects(Collectible collectible)
        {
            Vector3 effectPosition = collectible.transform.position;

            // パーティクルエフェクト
            if (enableParticleEffects && collectible.Config.collectionEffect != null)
            {
                var effect = Instantiate(collectible.Config.collectionEffect, effectPosition, Quaternion.identity);
                Destroy(effect.gameObject, effectDuration);
            }

            // 音響エフェクト
            if (enableSoundEffects && collectible.Config.collectionSound != null)
            {
                AudioSource.PlayClipAtPoint(collectible.Config.collectionSound, effectPosition);
            }

            // スクリーンエフェクト（UI更新等）
            if (enableScreenEffects)
            {
                // スコア表示等のUI更新
            }
        }

        /// <summary>
        /// スポーンポイント状態の更新
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
        /// コンボのリセット
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
        /// アイテム収集イベント
        /// </summary>
        private void OnItemCollected(Collectible collectible)
        {
            // GameEventの発行
            // UI更新イベント
            // プレイヤー通知イベント
        }

        #endregion

        #region Spawn System

        /// <summary>
        /// 指定位置にアイテムを生成
        /// </summary>
        public bool SpawnCollectibleAtPoint(CollectibleSpawnPoint spawnPoint)
        {
            if (spawnPoint.spawnTransform == null) return false;

            // タイプ設定の取得
            var typeConfig = GetCollectibleConfig(spawnPoint.preferredType);
            if (typeConfig == null || typeConfig.prefab == null) return false;

            // 生成条件のチェック
            if (!CanSpawnAtPoint(spawnPoint)) return false;

            // アイテム生成
            GameObject itemObject = Instantiate(typeConfig.prefab,
                spawnPoint.spawnTransform.position,
                spawnPoint.spawnTransform.rotation);

            var collectible = itemObject.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = itemObject.AddComponent<Collectible>();
            }

            // 設定適用
            collectible.Initialize(typeConfig, spawnPoint.spawnTransform.position);

            // アクティブリストに追加
            activeCollectibles.Add(collectible);

            LogDebug($"[CollectibleManager] Spawned {typeConfig.displayName} at {spawnPoint.spawnTransform.position}");

            return true;
        }

        /// <summary>
        /// 生成可能かチェック
        /// </summary>
        private bool CanSpawnAtPoint(CollectibleSpawnPoint spawnPoint)
        {
            if (!spawnPoint.isActive) return false;

            // プレイヤー距離チェック
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

            // 時間ベース生成チェック
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
        /// 収集アイテム設定の取得
        /// </summary>
        private CollectibleTypeConfig GetCollectibleConfig(CollectibleType type)
        {
            return collectibleTypes.FirstOrDefault(config => config.type == type);
        }

        /// <summary>
        /// ランダムな場所にアイテムを生成
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
        /// 収集統計の取得
        /// </summary>
        public Dictionary<CollectibleType, int> GetCollectionStats()
        {
            return new Dictionary<CollectibleType, int>(collectionStats);
        }

        /// <summary>
        /// 現在のスコアを取得
        /// </summary>
        public int GetCurrentScore()
        {
            return currentScore;
        }

        /// <summary>
        /// 収集完了率を取得
        /// </summary>
        public float GetCompletionPercentage()
        {
            return completionPercentage;
        }

        /// <summary>
        /// 全アイテムの収集をリセット
        /// </summary>
        public void ResetAllCollectibles()
        {
            // 収集済みアイテムの復活
            foreach (var collectible in collectedItems)
            {
                if (collectible != null)
                {
                    collectible.gameObject.SetActive(true);
                    collectible.IsCollected = false;
                    activeCollectibles.Add(collectible);
                }
            }

            // 統計リセット
            collectedCount = 0;
            currentScore = 0;
            ResetCombo();
            InitializeCollectionStats();
            collectedItems.Clear();

            // スポーンポイントリセット
            foreach (var spawnPoint in spawnPoints)
            {
                spawnPoint.hasSpawned = true;
                spawnPoint.respawnTime = 0f;
            }

            LogDebug("[CollectibleManager] All collectibles reset");
        }

        /// <summary>
        /// UI更新
        /// </summary>
        public void UpdateUI()
        {
            // UI更新処理
            // スコア表示、残り個数表示等
        }

        /// <summary>
        /// 目標スコアを設定
        /// PlatformerTemplateManagerから呼び出される
        /// </summary>
        /// <param name="targetScore">設定する目標スコア</param>
        public void SetTargetScore(int targetScore)
        {
            if (targetScore < 0)
            {
                LogDebug("[CollectibleManager] ⚠️ Target score cannot be negative, setting to 0");
                targetScore = 0;
            }

            // 内部的に目標スコアを保存（将来の拡張用）
            // 現在のバージョンでは設定を記録するのみ
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

            // スポーンポイントの可視化
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.spawnTransform == null || !spawnPoint.showGizmos) continue;

                Gizmos.color = spawnPoint.isActive ? spawnPoint.gizmosColor : Color.gray;
                Gizmos.DrawWireSphere(spawnPoint.spawnTransform.position, 0.5f);

                // 自動収集範囲の可視化
                var config = GetCollectibleConfig(spawnPoint.preferredType);
                if (config != null && config.autoCollect)
                {
                    Gizmos.color = new Color(spawnPoint.gizmosColor.r, spawnPoint.gizmosColor.g,
                        spawnPoint.gizmosColor.b, 0.3f);
                    Gizmos.DrawSphere(spawnPoint.spawnTransform.position, config.autoCollectRange);
                }
            }

            // アクティブアイテムの可視化
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

            // シーン内の "CollectibleSpawn" タグが付いたオブジェクトを検索
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