using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Platforms
{
    /// <summary>
    /// プラットフォーマー動的プラットフォーム管理システム
    /// 移動プラットフォーム、回転プラットフォーム、消失プラットフォーム等を統合管理
    /// 物理演算連動によるリアルなプラットフォーム挙動
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        #region Platform Configuration

        [TabGroup("Platforms", "Basic Settings")]
        [Title("Platform Management System", "プラットフォーマー動的プラットフォーム統合管理", TitleAlignments.Centered)]
        [SerializeField] private bool enablePlatforms = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool optimizePerformance = true;

        [TabGroup("Platforms", "Platform Types")]
        [Header("Platform Types Management")]
        [SerializeField] private List<MovingPlatform> movingPlatforms = new List<MovingPlatform>();
        [SerializeField] private List<RotatingPlatform> rotatingPlatforms = new List<RotatingPlatform>();
        [SerializeField] private List<DisappearingPlatform> disappearingPlatforms = new List<DisappearingPlatform>();
        [SerializeField] private List<TriggerPlatform> triggerPlatforms = new List<TriggerPlatform>();

        [TabGroup("Platforms", "Physics")]
        [Header("Physics Configuration")]
        [SerializeField] private LayerMask platformLayer = 1;
        [SerializeField] private PhysicsMaterial platformPhysicsMaterial;
        [SerializeField, Range(0f, 2f)] private float platformFriction = 0.5f;
        [SerializeField, Range(0f, 2f)] private float platformBounciness = 0.1f;

        [TabGroup("Platforms", "Player Interaction")]
        [Header("Player Interaction Settings")]
        [SerializeField] private bool enablePlayerCarrying = true;
        [SerializeField] private bool enablePlayerSticking = false;
        [SerializeField, Range(0f, 20f)] private float carryForce = 10f;
        [SerializeField, Range(0f, 10f)] private float stickThreshold = 5f;

        #endregion

        #region Runtime State

        [TabGroup("Runtime", "Current State")]
        [Header("Runtime Platform State")]
        [SerializeField, ReadOnly] private int activePlatformCount;
        [SerializeField, ReadOnly] private int movingPlatformCount;
        [SerializeField, ReadOnly] private int playersOnPlatforms;
        [SerializeField, ReadOnly] private Transform currentPlayerOnPlatform;

        [TabGroup("Runtime", "Performance")]
        [Header("Performance Statistics")]
        [SerializeField, ReadOnly] private float averageUpdateTime;
        [SerializeField, ReadOnly] private int culledPlatformCount;
        [SerializeField, ReadOnly] private float lastOptimizationTime;

        private Dictionary<PlatformType, List<IPlatform>> platformGroups = new Dictionary<PlatformType, List<IPlatform>>();
        private List<IPlatform> allPlatforms = new List<IPlatform>();
        private List<GameObject> playersOnPlatform = new List<GameObject>();

        #endregion

        #region Platform Types

        [System.Serializable]
        public class MovingPlatform
        {
            [Header("Movement Configuration")]
            public Transform platformTransform;
            public List<Transform> waypoints = new List<Transform>();
            public float moveSpeed = 3f;
            public AnimationCurve movementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            public bool loopMovement = true;
            public bool reverseDirection = false;
            public float pauseAtWaypoint = 0f;

            [Header("Behavior Settings")]
            public bool activateOnPlayerNearby = false;
            public float activationDistance = 5f;
            public bool stopWhenPlayerLeaves = false;
            public bool resetPositionWhenInactive = false;

            [Header("Visual & Audio")]
            public ParticleSystem movementEffect;
            public AudioSource movementSound;
            public bool showGizmos = true;
            public Color gizmosColor = Color.blue;

            [Header("Runtime State")]
            [ReadOnly] public int currentWaypointIndex = 0;
            [ReadOnly] public float movementProgress = 0f;
            [ReadOnly] public bool isMoving = false;
            [ReadOnly] public bool playerNearby = false;
            [ReadOnly] public float pauseTimer = 0f;
        }

        [System.Serializable]
        public class RotatingPlatform
        {
            [Header("Rotation Configuration")]
            public Transform platformTransform;
            public Vector3 rotationAxis = Vector3.up;
            public float rotationSpeed = 90f;
            public bool constantRotation = true;
            public bool reverseDirection = false;

            [Header("Rotation Limits")]
            public bool useRotationLimits = false;
            public float minRotation = -90f;
            public float maxRotation = 90f;
            public float rotationAcceleration = 1f;

            [Header("Trigger Settings")]
            public bool activateOnTrigger = false;
            public float activationDuration = 5f;
            public bool autoReset = true;

            [Header("Visual & Audio")]
            public ParticleSystem rotationEffect;
            public AudioSource rotationSound;
            public bool showGizmos = true;
            public Color gizmosColor = Color.green;

            [Header("Runtime State")]
            [ReadOnly] public float currentRotation = 0f;
            [ReadOnly] public bool isRotating = false;
            [ReadOnly] public bool isActivated = false;
            [ReadOnly] public float activationTimer = 0f;
        }

        [System.Serializable]
        public class DisappearingPlatform
        {
            [Header("Disappearing Configuration")]
            public Transform platformTransform;
            public float disappearDelay = 2f;
            public float reappearDelay = 5f;
            public AnimationCurve disappearCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            public bool triggerOnPlayerStep = true;
            public bool autoReappear = true;

            [Header("Visual Effects")]
            public bool fadeOut = true;
            public bool shrinkOut = false;
            public ParticleSystem disappearEffect;
            public ParticleSystem reappearEffect;
            public AudioSource disappearSound;
            public AudioSource reappearSound;

            [Header("Warning System")]
            public bool showWarning = true;
            public float warningDuration = 1f;
            public Color warningColor = Color.red;
            public float blinkFrequency = 5f;

            [Header("Runtime State")]
            [ReadOnly] public bool isVisible = true;
            [ReadOnly] public bool isTriggered = false;
            [ReadOnly] public float disappearTimer = 0f;
            [ReadOnly] public float reappearTimer = 0f;
            [ReadOnly] public bool isWarning = false;
        }

        [System.Serializable]
        public class TriggerPlatform
        {
            [Header("Trigger Configuration")]
            public Transform platformTransform;
            public TriggerType triggerType = TriggerType.Switch;
            public float triggerDuration = 3f;
            public bool requiresContinuousActivation = false;
            public bool multipleActivations = true;

            [Header("Platform Movement")]
            public Vector3 activatedPosition;
            public Vector3 deactivatedPosition;
            public float movementSpeed = 2f;
            public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            [Header("Trigger Conditions")]
            public LayerMask triggerLayers = 1;
            public string requiredTag = "Player";
            public float triggerCooldown = 0f;

            [Header("Visual & Audio")]
            public Material activatedMaterial;
            public Material deactivatedMaterial;
            public ParticleSystem activationEffect;
            public AudioSource activationSound;

            [Header("Runtime State")]
            [ReadOnly] public bool isActivated = false;
            [ReadOnly] public bool isCooldown = false;
            [ReadOnly] public float cooldownTimer = 0f;
            [ReadOnly] public float activationTimer = 0f;
            [ReadOnly] public int objectsOnPlatform = 0;
        }

        public enum PlatformType
        {
            Moving,
            Rotating,
            Disappearing,
            Trigger,
            Static
        }

        public enum TriggerType
        {
            Switch,       // オン/オフ切り替え
            Pressure,     // 重さセンサー
            Timed,        // タイマー式
            Proximity,    // 近接センサー
            Sequence      // シーケンス式
        }

        #endregion

        #region Interface Definition

        public interface IPlatform
        {
            PlatformType Type { get; }
            Transform Transform { get; }
            bool IsActive { get; set; }
            void UpdatePlatform(float deltaTime);
            void OnPlayerEnter(GameObject player);
            void OnPlayerExit(GameObject player);
            void Activate();
            void Deactivate();
            void Reset();
        }

        #endregion

        #region Service References

        private UnityEngine.Camera playerCamera;
        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializePlatformManager();
        }

        private void Start()
        {
            if (enablePlatforms)
            {
                SetupPlatformSystem();
            }
        }

        private void Update()
        {
            if (enablePlatforms && isInitialized)
            {
                UpdatePlatformSystem();
            }
        }

        private void FixedUpdate()
        {
            if (enablePlatforms && isInitialized)
            {
                FixedUpdatePlatformSystem();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// プラットフォーム管理システムの初期化
        /// </summary>
        private void InitializePlatformManager()
        {
            LogDebug("[PlatformManager] Initializing Platform Management System...");

            try
            {
                // カメラ参照取得
                playerCamera = UnityEngine.Camera.main ?? FindFirstObjectByType<UnityEngine.Camera>();

                // プラットフォームグループの初期化
                InitializePlatformGroups();

                // 全プラットフォームの検証・登録
                ValidateAndRegisterPlatforms();

                // Physics Materialの適用
                ApplyPhysicsMaterials();

                isInitialized = true;
                LogDebug("[PlatformManager] ✅ Platform Manager initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[PlatformManager] ❌ Platform Manager initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// プラットフォームグループの初期化
        /// </summary>
        private void InitializePlatformGroups()
        {
            platformGroups.Clear();
            foreach (PlatformType type in System.Enum.GetValues(typeof(PlatformType)))
            {
                platformGroups[type] = new List<IPlatform>();
            }
        }

        /// <summary>
        /// プラットフォームの検証・登録
        /// </summary>
        private void ValidateAndRegisterPlatforms()
        {
            allPlatforms.Clear();

            // Moving Platformsの検証・登録
            ValidateMovingPlatforms();

            // Rotating Platformsの検証・登録
            ValidateRotatingPlatforms();

            // Disappearing Platformsの検証・登録
            ValidateDisappearingPlatforms();

            // Trigger Platformsの検証・登録
            ValidateTriggerPlatforms();

            activePlatformCount = allPlatforms.Count;
            LogDebug($"[PlatformManager] Total platforms registered: {activePlatformCount}");
        }

        /// <summary>
        /// Moving Platformsの検証
        /// </summary>
        private void ValidateMovingPlatforms()
        {
            movingPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in movingPlatforms)
            {
                if (platform.waypoints.Count < 2)
                {
                    LogError($"[PlatformManager] Moving platform {platform.platformTransform.name} needs at least 2 waypoints");
                    continue;
                }

                // 初期状態設定
                platform.currentWaypointIndex = 0;
                platform.movementProgress = 0f;
                platform.isMoving = !platform.activateOnPlayerNearby;
            }

            movingPlatformCount = movingPlatforms.Count;
        }

        /// <summary>
        /// Rotating Platformsの検証
        /// </summary>
        private void ValidateRotatingPlatforms()
        {
            rotatingPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in rotatingPlatforms)
            {
                // 初期状態設定
                platform.currentRotation = 0f;
                platform.isRotating = platform.constantRotation;
            }
        }

        /// <summary>
        /// Disappearing Platformsの検証
        /// </summary>
        private void ValidateDisappearingPlatforms()
        {
            disappearingPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in disappearingPlatforms)
            {
                // 初期状態設定
                platform.isVisible = true;
                platform.isTriggered = false;
                platform.disappearTimer = 0f;
                platform.reappearTimer = 0f;
            }
        }

        /// <summary>
        /// Trigger Platformsの検証
        /// </summary>
        private void ValidateTriggerPlatforms()
        {
            triggerPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in triggerPlatforms)
            {
                // 初期位置設定
                if (platform.deactivatedPosition == Vector3.zero)
                {
                    platform.deactivatedPosition = platform.platformTransform.position;
                }

                // Colliderの確認・設定
                var collider = platform.platformTransform.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }
            }
        }

        /// <summary>
        /// Physics Materialの適用
        /// </summary>
        private void ApplyPhysicsMaterials()
        {
            if (platformPhysicsMaterial == null)
            {
                // デフォルトPhysics Materialの作成
                platformPhysicsMaterial = new PhysicsMaterial("PlatformMaterial");
                platformPhysicsMaterial.staticFriction = platformFriction;
                platformPhysicsMaterial.dynamicFriction = platformFriction;
                platformPhysicsMaterial.bounciness = platformBounciness;
            }

            // 全プラットフォームにPhysics Materialを適用
            ApplyPhysicsMaterialToAllPlatforms();
        }

        /// <summary>
        /// 全プラットフォームへのPhysics Material適用
        /// </summary>
        private void ApplyPhysicsMaterialToAllPlatforms()
        {
            var allPlatformTransforms = new List<Transform>();

            movingPlatforms.ForEach(p => allPlatformTransforms.Add(p.platformTransform));
            rotatingPlatforms.ForEach(p => allPlatformTransforms.Add(p.platformTransform));
            disappearingPlatforms.ForEach(p => allPlatformTransforms.Add(p.platformTransform));
            triggerPlatforms.ForEach(p => allPlatformTransforms.Add(p.platformTransform));

            foreach (var platformTransform in allPlatformTransforms)
            {
                if (platformTransform == null) continue;

                var collider = platformTransform.GetComponent<Collider>();
                if (collider != null && collider.material != platformPhysicsMaterial)
                {
                    collider.material = platformPhysicsMaterial;
                }
            }
        }

        #endregion

        #region System Setup

        /// <summary>
        /// プラットフォームシステムのセットアップ
        /// </summary>
        private void SetupPlatformSystem()
        {
            LogDebug("[PlatformManager] Setting up Platform System...");

            // イベントリスナーの登録
            RegisterEventListeners();

            // パフォーマンス最適化の初期設定
            if (optimizePerformance)
            {
                InitializePerformanceOptimization();
            }

            LogDebug("[PlatformManager] ✅ Platform System setup completed");
        }

        /// <summary>
        /// イベントリスナーの登録
        /// </summary>
        private void RegisterEventListeners()
        {
            // プレイヤーイベントのリスナー登録
            // 例: プレイヤー位置更新、プレイヤージャンプ等
        }

        /// <summary>
        /// パフォーマンス最適化の初期設定
        /// </summary>
        private void InitializePerformanceOptimization()
        {
            lastOptimizationTime = Time.time;
        }

        #endregion

        #region Update Systems

        /// <summary>
        /// プラットフォームシステムの更新
        /// </summary>
        private void UpdatePlatformSystem()
        {
            float startTime = Time.realtimeSinceStartup;

            // Moving Platformsの更新
            UpdateMovingPlatforms();

            // Rotating Platformsの更新
            UpdateRotatingPlatforms();

            // Disappearing Platformsの更新
            UpdateDisappearingPlatforms();

            // Trigger Platformsの更新
            UpdateTriggerPlatforms();

            // プレイヤー運搬処理
            if (enablePlayerCarrying)
            {
                UpdatePlayerCarrying();
            }

            // パフォーマンス最適化
            if (optimizePerformance && Time.time - lastOptimizationTime > 1f)
            {
                PerformOptimization();
                lastOptimizationTime = Time.time;
            }

            // パフォーマンス統計の更新
            averageUpdateTime = (averageUpdateTime + (Time.realtimeSinceStartup - startTime)) / 2f;
        }

        /// <summary>
        /// Moving Platformsの更新
        /// </summary>
        private void UpdateMovingPlatforms()
        {
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // プレイヤー近接チェック
                if (platform.activateOnPlayerNearby)
                {
                    CheckPlayerProximity(platform);
                }

                // 移動処理
                if (platform.isMoving && platform.waypoints.Count >= 2)
                {
                    UpdatePlatformMovement(platform);
                }
            }
        }

        /// <summary>
        /// プレイヤー近接チェック
        /// </summary>
        private void CheckPlayerProximity(MovingPlatform platform)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(platform.platformTransform.position, player.transform.position);
                bool wasNearby = platform.playerNearby;
                platform.playerNearby = distance <= platform.activationDistance;

                if (!wasNearby && platform.playerNearby)
                {
                    platform.isMoving = true;
                }
                else if (wasNearby && !platform.playerNearby && platform.stopWhenPlayerLeaves)
                {
                    platform.isMoving = false;
                }
            }
        }

        /// <summary>
        /// プラットフォーム移動の更新
        /// </summary>
        private void UpdatePlatformMovement(MovingPlatform platform)
        {
            // 待機時間のチェック
            if (platform.pauseTimer > 0f)
            {
                platform.pauseTimer -= Time.deltaTime;
                return;
            }

            // 現在のウェイポイントと次のウェイポイント
            Transform currentWaypoint = platform.waypoints[platform.currentWaypointIndex];
            int nextIndex = (platform.currentWaypointIndex + 1) % platform.waypoints.Count;
            Transform nextWaypoint = platform.waypoints[nextIndex];

            // 移動進行度の更新
            platform.movementProgress += platform.moveSpeed * Time.deltaTime /
                Vector3.Distance(currentWaypoint.position, nextWaypoint.position);

            // カーブによるスムーズ移動
            float curvedProgress = platform.movementCurve.Evaluate(platform.movementProgress);

            // 位置の計算
            Vector3 newPosition = Vector3.Lerp(currentWaypoint.position, nextWaypoint.position, curvedProgress);
            platform.platformTransform.position = newPosition;

            // ウェイポイント到達チェック
            if (platform.movementProgress >= 1f)
            {
                platform.movementProgress = 0f;
                platform.currentWaypointIndex = nextIndex;

                // 待機時間設定
                if (platform.pauseAtWaypoint > 0f)
                {
                    platform.pauseTimer = platform.pauseAtWaypoint;
                }

                // ループ処理
                if (!platform.loopMovement && platform.currentWaypointIndex == 0)
                {
                    platform.isMoving = false;
                }
            }

            // エフェクト・サウンド再生
            PlayMovementEffects(platform);
        }

        /// <summary>
        /// Rotating Platformsの更新
        /// </summary>
        private void UpdateRotatingPlatforms()
        {
            foreach (var platform in rotatingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 回転処理
                if (platform.isRotating)
                {
                    float rotationAmount = platform.rotationSpeed * Time.deltaTime;
                    if (platform.reverseDirection) rotationAmount = -rotationAmount;

                    // 制限チェック
                    if (platform.useRotationLimits)
                    {
                        platform.currentRotation += rotationAmount;
                        platform.currentRotation = Mathf.Clamp(platform.currentRotation, platform.minRotation, platform.maxRotation);

                        if (platform.currentRotation >= platform.maxRotation || platform.currentRotation <= platform.minRotation)
                        {
                            platform.reverseDirection = !platform.reverseDirection;
                        }
                    }
                    else
                    {
                        platform.currentRotation += rotationAmount;
                    }

                    // 回転適用
                    platform.platformTransform.Rotate(platform.rotationAxis, rotationAmount);
                }

                // アクティベーション処理
                if (platform.isActivated && platform.activationTimer > 0f)
                {
                    platform.activationTimer -= Time.deltaTime;
                    if (platform.activationTimer <= 0f && platform.autoReset)
                    {
                        platform.isActivated = false;
                        platform.isRotating = false;
                    }
                }
            }
        }

        /// <summary>
        /// Disappearing Platformsの更新
        /// </summary>
        private void UpdateDisappearingPlatforms()
        {
            foreach (var platform in disappearingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 警告処理
                if (platform.isWarning)
                {
                    UpdateWarningEffect(platform);
                }

                // 消失処理
                if (platform.isTriggered && platform.isVisible)
                {
                    platform.disappearTimer += Time.deltaTime;

                    if (platform.disappearTimer >= platform.disappearDelay)
                    {
                        DisappearPlatform(platform);
                    }
                }

                // 再出現処理
                if (!platform.isVisible && platform.autoReappear)
                {
                    platform.reappearTimer += Time.deltaTime;

                    if (platform.reappearTimer >= platform.reappearDelay)
                    {
                        ReappearPlatform(platform);
                    }
                }
            }
        }

        /// <summary>
        /// Trigger Platformsの更新
        /// </summary>
        private void UpdateTriggerPlatforms()
        {
            foreach (var platform in triggerPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // クールダウン処理
                if (platform.isCooldown)
                {
                    platform.cooldownTimer -= Time.deltaTime;
                    if (platform.cooldownTimer <= 0f)
                    {
                        platform.isCooldown = false;
                    }
                }

                // アクティベーション時間処理
                if (platform.isActivated && !platform.requiresContinuousActivation)
                {
                    platform.activationTimer -= Time.deltaTime;
                    if (platform.activationTimer <= 0f)
                    {
                        DeactivateTriggerPlatform(platform);
                    }
                }

                // 位置の補間
                Vector3 targetPosition = platform.isActivated ? platform.activatedPosition : platform.deactivatedPosition;
                float currentProgress = platform.movementCurve.Evaluate(
                    Vector3.Distance(platform.platformTransform.position, platform.deactivatedPosition) /
                    Vector3.Distance(platform.activatedPosition, platform.deactivatedPosition));

                platform.platformTransform.position = Vector3.MoveTowards(platform.platformTransform.position,
                    targetPosition, platform.movementSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 物理更新処理
        /// </summary>
        private void FixedUpdatePlatformSystem()
        {
            // 物理演算関連の更新処理
            UpdatePlatformPhysics();
        }

        /// <summary>
        /// プラットフォーム物理演算の更新
        /// </summary>
        private void UpdatePlatformPhysics()
        {
            // プレイヤー運搬物理処理
            // プラットフォーム衝突処理
            // 動的摩擦調整等
        }

        #endregion

        #region Platform Effects

        /// <summary>
        /// 移動エフェクトの再生
        /// </summary>
        private void PlayMovementEffects(MovingPlatform platform)
        {
            // パーティクルエフェクト
            if (platform.movementEffect != null && !platform.movementEffect.isPlaying)
            {
                platform.movementEffect.Play();
            }

            // 移動サウンド
            if (platform.movementSound != null && !platform.movementSound.isPlaying)
            {
                platform.movementSound.Play();
            }
        }

        /// <summary>
        /// 警告エフェクトの更新
        /// </summary>
        private void UpdateWarningEffect(DisappearingPlatform platform)
        {
            var renderer = platform.platformTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                float blinkValue = Mathf.Sin(Time.time * platform.blinkFrequency * Mathf.PI * 2f);
                Color currentColor = Color.Lerp(Color.white, platform.warningColor, (blinkValue + 1f) / 2f);
                renderer.material.color = currentColor;
            }
        }

        /// <summary>
        /// プラットフォームの消失
        /// </summary>
        private void DisappearPlatform(DisappearingPlatform platform)
        {
            platform.isVisible = false;
            platform.isWarning = false;
            platform.reappearTimer = 0f;

            // エフェクト再生
            if (platform.disappearEffect != null)
            {
                platform.disappearEffect.Play();
            }

            if (platform.disappearSound != null)
            {
                platform.disappearSound.Play();
            }

            // フェードアウト処理
            if (platform.fadeOut || platform.shrinkOut)
            {
                StartCoroutine(AnimateDisappear(platform));
            }
            else
            {
                platform.platformTransform.gameObject.SetActive(false);
            }

            LogDebug($"[PlatformManager] Platform disappeared: {platform.platformTransform.name}");
        }

        /// <summary>
        /// プラットフォームの再出現
        /// </summary>
        private void ReappearPlatform(DisappearingPlatform platform)
        {
            platform.isVisible = true;
            platform.isTriggered = false;
            platform.disappearTimer = 0f;

            // エフェクト再生
            if (platform.reappearEffect != null)
            {
                platform.reappearEffect.Play();
            }

            if (platform.reappearSound != null)
            {
                platform.reappearSound.Play();
            }

            // フェードイン処理
            if (platform.fadeOut || platform.shrinkOut)
            {
                StartCoroutine(AnimateReappear(platform));
            }
            else
            {
                platform.platformTransform.gameObject.SetActive(true);
            }

            LogDebug($"[PlatformManager] Platform reappeared: {platform.platformTransform.name}");
        }

        #endregion

        #region Player Interaction

        /// <summary>
        /// プレイヤー運搬処理の更新
        /// </summary>
        private void UpdatePlayerCarrying()
        {
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // プラットフォーム上のプレイヤーをチェック
                CheckPlayersOnPlatform(platform.platformTransform);
            }
        }

        /// <summary>
        /// プラットフォーム上のプレイヤーチェック
        /// </summary>
        private void CheckPlayersOnPlatform(Transform platformTransform)
        {
            Collider platformCollider = platformTransform.GetComponent<Collider>();
            if (platformCollider == null) return;

            // プレイヤーオブジェクトの検索
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (var player in players)
            {
                if (IsPlayerOnPlatform(player, platformCollider))
                {
                    ApplyCarryForce(player, platformTransform);
                }
            }
        }

        /// <summary>
        /// プレイヤーがプラットフォーム上にいるかチェック
        /// </summary>
        private bool IsPlayerOnPlatform(GameObject player, Collider platformCollider)
        {
            // レイキャストによる判定
            Vector3 rayStart = player.transform.position + Vector3.up * 0.1f;
            Vector3 rayDirection = Vector3.down;
            float rayDistance = 0.5f;

            if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, rayDistance))
            {
                return hit.collider == platformCollider;
            }

            return false;
        }

        /// <summary>
        /// プレイヤーに運搬力を適用
        /// </summary>
        private void ApplyCarryForce(GameObject player, Transform platform)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // プラットフォームの移動ベクトルを計算
                // Note: 実際の実装では、プラットフォームの前フレーム位置を記録する必要があります
                Vector3 platformMovement = Vector3.zero; // 簡略化

                playerRb.MovePosition(playerRb.position + platformMovement);
            }
        }

        /// <summary>
        /// トリガープラットフォームのアクティベート
        /// </summary>
        public void ActivateTriggerPlatform(TriggerPlatform platform)
        {
            if (platform.isCooldown) return;

            platform.isActivated = true;
            platform.activationTimer = platform.triggerDuration;

            if (platform.triggerCooldown > 0f)
            {
                platform.isCooldown = true;
                platform.cooldownTimer = platform.triggerCooldown;
            }

            // エフェクト・サウンド再生
            if (platform.activationEffect != null)
            {
                platform.activationEffect.Play();
            }

            if (platform.activationSound != null)
            {
                platform.activationSound.Play();
            }

            // マテリアル変更
            if (platform.activatedMaterial != null)
            {
                var renderer = platform.platformTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = platform.activatedMaterial;
                }
            }

            LogDebug($"[PlatformManager] Trigger platform activated: {platform.platformTransform.name}");
        }

        /// <summary>
        /// トリガープラットフォームのデアクティベート
        /// </summary>
        private void DeactivateTriggerPlatform(TriggerPlatform platform)
        {
            platform.isActivated = false;

            // マテリアル復元
            if (platform.deactivatedMaterial != null)
            {
                var renderer = platform.platformTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = platform.deactivatedMaterial;
                }
            }

            LogDebug($"[PlatformManager] Trigger platform deactivated: {platform.platformTransform.name}");
        }

        #endregion

        #region Performance Optimization

        /// <summary>
        /// パフォーマンス最適化処理
        /// </summary>
        private void PerformOptimization()
        {
            if (playerCamera == null) return;

            int culledCount = 0;

            // 視野外プラットフォームのカリング
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                bool isVisible = IsVisibleFromCamera(platform.platformTransform.position);
                bool wasMoving = platform.isMoving;

                // 視野外の場合、一時停止
                if (!isVisible && wasMoving && !platform.playerNearby)
                {
                    platform.isMoving = false;
                    culledCount++;
                }
                else if (isVisible && !wasMoving && !platform.activateOnPlayerNearby)
                {
                    platform.isMoving = true;
                }
            }

            culledPlatformCount = culledCount;
        }

        /// <summary>
        /// カメラから見えるかどうかの判定
        /// </summary>
        private bool IsVisibleFromCamera(Vector3 position)
        {
            if (playerCamera == null) return true;

            Vector3 screenPoint = playerCamera.WorldToViewportPoint(position);
            return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
        }

        #endregion

        #region Animation Coroutines

        private System.Collections.IEnumerator AnimateDisappear(DisappearingPlatform platform)
        {
            var renderer = platform.platformTransform.GetComponent<Renderer>();
            var originalScale = platform.platformTransform.localScale;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float curveValue = platform.disappearCurve.Evaluate(progress);

                if (platform.fadeOut && renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = curveValue;
                    renderer.material.color = color;
                }

                if (platform.shrinkOut)
                {
                    platform.platformTransform.localScale = originalScale * curveValue;
                }

                yield return null;
            }

            platform.platformTransform.gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator AnimateReappear(DisappearingPlatform platform)
        {
            platform.platformTransform.gameObject.SetActive(true);

            var renderer = platform.platformTransform.GetComponent<Renderer>();
            var originalScale = platform.platformTransform.localScale;
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                if (platform.fadeOut && renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = progress;
                    renderer.material.color = color;
                }

                if (platform.shrinkOut)
                {
                    platform.platformTransform.localScale = originalScale * progress;
                }

                yield return null;
            }

            // 最終状態確保
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1f;
                renderer.material.color = color;
            }
            platform.platformTransform.localScale = originalScale;
        }

        #endregion

        #region Public API

        /// <summary>
        /// 指定タイプのプラットフォーム数を取得
        /// </summary>
        public int GetPlatformCount(PlatformType type)
        {
            return type switch
            {
                PlatformType.Moving => movingPlatforms.Count,
                PlatformType.Rotating => rotatingPlatforms.Count,
                PlatformType.Disappearing => disappearingPlatforms.Count,
                PlatformType.Trigger => triggerPlatforms.Count,
                _ => 0
            };
        }

        /// <summary>
        /// 全プラットフォームのリセット
        /// </summary>
        public void ResetAllPlatforms()
        {
            ResetMovingPlatforms();
            ResetRotatingPlatforms();
            ResetDisappearingPlatforms();
            ResetTriggerPlatforms();

            LogDebug("[PlatformManager] All platforms reset");
        }

        /// <summary>
        /// Moving Platformsのリセット
        /// </summary>
        private void ResetMovingPlatforms()
        {
            foreach (var platform in movingPlatforms)
            {
                platform.currentWaypointIndex = 0;
                platform.movementProgress = 0f;
                platform.pauseTimer = 0f;
                platform.isMoving = !platform.activateOnPlayerNearby;

                if (platform.resetPositionWhenInactive && platform.waypoints.Count > 0)
                {
                    platform.platformTransform.position = platform.waypoints[0].position;
                }
            }
        }

        /// <summary>
        /// Rotating Platformsのリセット
        /// </summary>
        private void ResetRotatingPlatforms()
        {
            foreach (var platform in rotatingPlatforms)
            {
                platform.currentRotation = 0f;
                platform.isRotating = platform.constantRotation;
                platform.isActivated = false;
                platform.activationTimer = 0f;
                platform.reverseDirection = false;
            }
        }

        /// <summary>
        /// Disappearing Platformsのリセット
        /// </summary>
        private void ResetDisappearingPlatforms()
        {
            foreach (var platform in disappearingPlatforms)
            {
                platform.isVisible = true;
                platform.isTriggered = false;
                platform.isWarning = false;
                platform.disappearTimer = 0f;
                platform.reappearTimer = 0f;
                platform.platformTransform.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Trigger Platformsのリセット
        /// </summary>
        private void ResetTriggerPlatforms()
        {
            foreach (var platform in triggerPlatforms)
            {
                platform.isActivated = false;
                platform.isCooldown = false;
                platform.cooldownTimer = 0f;
                platform.activationTimer = 0f;
                platform.objectsOnPlatform = 0;
                platform.platformTransform.position = platform.deactivatedPosition;
            }
        }

        #endregion

        #region Template Actions

        [TabGroup("Actions", "Platform Control")]
        [Button("Reset All Platforms")]
        public void TestResetAllPlatforms()
        {
            ResetAllPlatforms();
        }

        [Button("Activate All Moving Platforms")]
        public void TestActivateMovingPlatforms()
        {
            foreach (var platform in movingPlatforms)
            {
                platform.isMoving = true;
            }
        }

        [Button("Show Platform Statistics")]
        public void ShowPlatformStatistics()
        {
            LogDebug("=== Platform Management Statistics ===");
            LogDebug($"Total Active Platforms: {activePlatformCount}");
            LogDebug($"Moving Platforms: {movingPlatformCount}");
            LogDebug($"Rotating Platforms: {rotatingPlatforms.Count}");
            LogDebug($"Disappearing Platforms: {disappearingPlatforms.Count}");
            LogDebug($"Trigger Platforms: {triggerPlatforms.Count}");
            LogDebug($"Players on Platforms: {playersOnPlatforms}");
            LogDebug($"Average Update Time: {averageUpdateTime * 1000f:F2}ms");
            LogDebug($"Culled Platforms: {culledPlatformCount}");
            LogDebug("=== Statistics Complete ===");
        }

        [Button("Test Platform Effects")]
        public void TestPlatformEffects()
        {
            foreach (var platform in movingPlatforms)
            {
                PlayMovementEffects(platform);
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
            if (!enablePlatforms) return;

            // Moving Platformsの可視化
            DrawMovingPlatformGizmos();

            // Rotating Platformsの可視化
            DrawRotatingPlatformGizmos();

            // Trigger Platformsの可視化
            DrawTriggerPlatformGizmos();
        }

        private void DrawMovingPlatformGizmos()
        {
            foreach (var platform in movingPlatforms)
            {
                if (!platform.showGizmos || platform.platformTransform == null) continue;

                Gizmos.color = platform.gizmosColor;

                // ウェイポイントとパスの表示
                for (int i = 0; i < platform.waypoints.Count; i++)
                {
                    if (platform.waypoints[i] == null) continue;

                    Gizmos.DrawWireCube(platform.waypoints[i].position, Vector3.one * 0.5f);

                    if (i < platform.waypoints.Count - 1)
                    {
                        Gizmos.DrawLine(platform.waypoints[i].position, platform.waypoints[i + 1].position);
                    }
                    else if (platform.loopMovement)
                    {
                        Gizmos.DrawLine(platform.waypoints[i].position, platform.waypoints[0].position);
                    }
                }

                // 現在位置の表示
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(platform.platformTransform.position, Vector3.one * 1f);
            }
        }

        private void DrawRotatingPlatformGizmos()
        {
            foreach (var platform in rotatingPlatforms)
            {
                if (!platform.showGizmos || platform.platformTransform == null) continue;

                Gizmos.color = platform.gizmosColor;
                Gizmos.DrawWireSphere(platform.platformTransform.position, 2f);

                // 回転軸の表示
                Gizmos.DrawRay(platform.platformTransform.position, platform.rotationAxis.normalized * 3f);
            }
        }

        private void DrawTriggerPlatformGizmos()
        {
            foreach (var platform in triggerPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // アクティベート位置
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(platform.activatedPosition, Vector3.one * 0.8f);

                // デアクティベート位置
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(platform.deactivatedPosition, Vector3.one * 0.8f);

                // 移動パス
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(platform.activatedPosition, platform.deactivatedPosition);
            }
        }
#endif

        #endregion
    }
}