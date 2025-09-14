using UnityEngine;
using asterivo.Unity60.Features.Templates.Platformer.Collectibles;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Collectibles
{
    /// <summary>
    /// 個別の収集アイテムコンポーネント
    /// CollectibleManagerと連携して収集処理を実行
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        #region Configuration

        [TabGroup("Collectible", "Configuration")]
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleManager.CollectibleTypeConfig config;
        [SerializeField] private bool debugMode = false;

        [TabGroup("Collectible", "State")]
        [Header("Current State")]
        [SerializeField, ReadOnly] private bool isCollected = false;
        [SerializeField, ReadOnly] private Vector3 originalPosition;
        [SerializeField, ReadOnly] private float spawnTime;

        #endregion

        #region Properties

        public CollectibleManager.CollectibleTypeConfig Config => config;
        public bool IsCollected { get => isCollected; set => isCollected = value; }
        public Vector3 OriginalPosition => originalPosition;
        public float SpawnTime => spawnTime;

        #endregion

        #region Components

        private Collider itemCollider;
        private Renderer itemRenderer;
        private CollectibleManager manager;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCollectible();
        }

        private void Start()
        {
            SetupCollectible();
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleTriggerEnter(other);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 収集アイテムの初期化
        /// </summary>
        private void InitializeCollectible()
        {
            // コンポーネント取得
            itemCollider = GetComponent<Collider>();
            itemRenderer = GetComponent<Renderer>();

            // Triggerの設定
            if (itemCollider != null)
            {
                itemCollider.isTrigger = true;
            }

            // 初期状態設定
            originalPosition = transform.position;
            spawnTime = Time.time;
            isCollected = false;

            // マネージャーの検索
            manager = FindFirstObjectByType<CollectibleManager>();

            LogDebug($"[Collectible] Initialized: {gameObject.name}");
        }

        /// <summary>
        /// 収集アイテムのセットアップ
        /// </summary>
        private void SetupCollectible()
        {
            // レアリティに応じた色設定
            if (config != null && itemRenderer != null)
            {
                ApplyRarityColor();
            }
        }

        /// <summary>
        /// 外部からの初期化（CollectibleManagerから呼び出し）
        /// </summary>
        public void Initialize(CollectibleManager.CollectibleTypeConfig typeConfig, Vector3 spawnPosition)
        {
            config = typeConfig;
            originalPosition = spawnPosition;
            transform.position = spawnPosition;
            spawnTime = Time.time;
            isCollected = false;

            ApplyRarityColor();

            LogDebug($"[Collectible] Initialized with config: {config.displayName}");
        }

        #endregion

        #region Visual Effects

        /// <summary>
        /// レアリティカラーの適用
        /// </summary>
        private void ApplyRarityColor()
        {
            if (itemRenderer == null || config == null) return;

            // マテリアルカラーの設定
            if (itemRenderer.material != null)
            {
                itemRenderer.material.color = config.rarityColor;
            }
        }

        #endregion

        #region Collection Logic

        /// <summary>
        /// トリガー接触処理
        /// </summary>
        private void HandleTriggerEnter(Collider other)
        {
            if (isCollected) return;

            // プレイヤータグチェック
            if (other.CompareTag("Player"))
            {
                AttemptCollection(other.gameObject);
            }
        }

        /// <summary>
        /// 収集試行
        /// </summary>
        private void AttemptCollection(GameObject player)
        {
            if (manager != null)
            {
                bool collected = manager.CollectItem(this);
                if (collected)
                {
                    LogDebug($"[Collectible] Collected by player: {config.displayName}");
                }
            }
            else
            {
                LogError("[Collectible] CollectibleManager not found!");
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
            // 収集範囲の可視化
            Gizmos.color = isCollected ? Color.gray : Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // 自動収集範囲の可視化
            if (config != null && config.autoCollect)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawSphere(transform.position, config.autoCollectRange);
            }

            // オリジナル位置の表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.1f);
        }
#endif

        #endregion
    }
}