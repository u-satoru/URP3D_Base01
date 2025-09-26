using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Platforms
{
    /// <summary>
    /// 繝励Λ繝・ヨ繝輔か繝ｼ繝槭・蜍慕噪繝励Λ繝・ヨ繝輔か繝ｼ繝邂｡逅・す繧ｹ繝・Β
    /// 遘ｻ蜍輔・繝ｩ繝・ヨ繝輔か繝ｼ繝縲∝屓霆｢繝励Λ繝・ヨ繝輔か繝ｼ繝縲∵ｶ亥､ｱ繝励Λ繝・ヨ繝輔か繝ｼ繝遲峨ｒ邨ｱ蜷育ｮ｡逅・
    /// 迚ｩ逅・ｼ皮ｮ鈴｣蜍輔↓繧医ｋ繝ｪ繧｢繝ｫ縺ｪ繝励Λ繝・ヨ繝輔か繝ｼ繝謖吝虚
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        #region Platform Configuration

        [TabGroup("Platforms", "Basic Settings")]
        [Title("Platform Management System", "繝励Λ繝・ヨ繝輔か繝ｼ繝槭・蜍慕噪繝励Λ繝・ヨ繝輔か繝ｼ繝邨ｱ蜷育ｮ｡逅・, TitleAlignments.Centered)]
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
            Switch,       // 繧ｪ繝ｳ/繧ｪ繝募・繧頑崛縺・
            Pressure,     // 驥阪＆繧ｻ繝ｳ繧ｵ繝ｼ
            Timed,        // 繧ｿ繧､繝槭・蠑・
            Proximity,    // 霑第磁繧ｻ繝ｳ繧ｵ繝ｼ
            Sequence      // 繧ｷ繝ｼ繧ｱ繝ｳ繧ｹ蠑・
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝邂｡逅・す繧ｹ繝・Β縺ｮ蛻晄悄蛹・
        /// </summary>
        private void InitializePlatformManager()
        {
            LogDebug("[PlatformManager] Initializing Platform Management System...");

            try
            {
                // 繧ｫ繝｡繝ｩ蜿ら・蜿門ｾ・
                playerCamera = UnityEngine.Camera.main ?? FindFirstObjectByType<UnityEngine.Camera>();

                // 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｰ繝ｫ繝ｼ繝励・蛻晄悄蛹・
                InitializePlatformGroups();

                // 蜈ｨ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
                ValidateAndRegisterPlatforms();

                // Physics Material縺ｮ驕ｩ逕ｨ
                ApplyPhysicsMaterials();

                isInitialized = true;
                LogDebug("[PlatformManager] 笨・Platform Manager initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[PlatformManager] 笶・Platform Manager initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｰ繝ｫ繝ｼ繝励・蛻晄悄蛹・
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
        /// </summary>
        private void ValidateAndRegisterPlatforms()
        {
            allPlatforms.Clear();

            // Moving Platforms縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
            ValidateMovingPlatforms();

            // Rotating Platforms縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
            ValidateRotatingPlatforms();

            // Disappearing Platforms縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
            ValidateDisappearingPlatforms();

            // Trigger Platforms縺ｮ讀懆ｨｼ繝ｻ逋ｻ骭ｲ
            ValidateTriggerPlatforms();

            activePlatformCount = allPlatforms.Count;
            LogDebug($"[PlatformManager] Total platforms registered: {activePlatformCount}");
        }

        /// <summary>
        /// Moving Platforms縺ｮ讀懆ｨｼ
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

                // 蛻晄悄迥ｶ諷玖ｨｭ螳・
                platform.currentWaypointIndex = 0;
                platform.movementProgress = 0f;
                platform.isMoving = !platform.activateOnPlayerNearby;
            }

            movingPlatformCount = movingPlatforms.Count;
        }

        /// <summary>
        /// Rotating Platforms縺ｮ讀懆ｨｼ
        /// </summary>
        private void ValidateRotatingPlatforms()
        {
            rotatingPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in rotatingPlatforms)
            {
                // 蛻晄悄迥ｶ諷玖ｨｭ螳・
                platform.currentRotation = 0f;
                platform.isRotating = platform.constantRotation;
            }
        }

        /// <summary>
        /// Disappearing Platforms縺ｮ讀懆ｨｼ
        /// </summary>
        private void ValidateDisappearingPlatforms()
        {
            disappearingPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in disappearingPlatforms)
            {
                // 蛻晄悄迥ｶ諷玖ｨｭ螳・
                platform.isVisible = true;
                platform.isTriggered = false;
                platform.disappearTimer = 0f;
                platform.reappearTimer = 0f;
            }
        }

        /// <summary>
        /// Trigger Platforms縺ｮ讀懆ｨｼ
        /// </summary>
        private void ValidateTriggerPlatforms()
        {
            triggerPlatforms.RemoveAll(platform => platform.platformTransform == null);

            foreach (var platform in triggerPlatforms)
            {
                // 蛻晄悄菴咲ｽｮ險ｭ螳・
                if (platform.deactivatedPosition == Vector3.zero)
                {
                    platform.deactivatedPosition = platform.platformTransform.position;
                }

                // Collider縺ｮ遒ｺ隱阪・險ｭ螳・
                var collider = platform.platformTransform.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }
            }
        }

        /// <summary>
        /// Physics Material縺ｮ驕ｩ逕ｨ
        /// </summary>
        private void ApplyPhysicsMaterials()
        {
            if (platformPhysicsMaterial == null)
            {
                // 繝・ヵ繧ｩ繝ｫ繝・hysics Material縺ｮ菴懈・
                platformPhysicsMaterial = new PhysicsMaterial("PlatformMaterial");
                platformPhysicsMaterial.staticFriction = platformFriction;
                platformPhysicsMaterial.dynamicFriction = platformFriction;
                platformPhysicsMaterial.bounciness = platformBounciness;
            }

            // 蜈ｨ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｫPhysics Material繧帝←逕ｨ
            ApplyPhysicsMaterialToAllPlatforms();
        }

        /// <summary>
        /// 蜈ｨ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｸ縺ｮPhysics Material驕ｩ逕ｨ
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｷ繧ｹ繝・Β縺ｮ繧ｻ繝・ヨ繧｢繝・・
        /// </summary>
        private void SetupPlatformSystem()
        {
            LogDebug("[PlatformManager] Setting up Platform System...");

            // 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ逋ｻ骭ｲ
            RegisterEventListeners();

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹悶・蛻晄悄險ｭ螳・
            if (optimizePerformance)
            {
                InitializePerformanceOptimization();
            }

            LogDebug("[PlatformManager] 笨・Platform System setup completed");
        }

        /// <summary>
        /// 繧､繝吶Φ繝医Μ繧ｹ繝翫・縺ｮ逋ｻ骭ｲ
        /// </summary>
        private void RegisterEventListeners()
        {
            // 繝励Ξ繧､繝､繝ｼ繧､繝吶Φ繝医・繝ｪ繧ｹ繝翫・逋ｻ骭ｲ
            // 萓・ 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ譖ｴ譁ｰ縲√・繝ｬ繧､繝､繝ｼ繧ｸ繝｣繝ｳ繝礼ｭ・
        }

        /// <summary>
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹悶・蛻晄悄險ｭ螳・
        /// </summary>
        private void InitializePerformanceOptimization()
        {
            lastOptimizationTime = Time.time;
        }

        #endregion

        #region Update Systems

        /// <summary>
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｷ繧ｹ繝・Β縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdatePlatformSystem()
        {
            float startTime = Time.realtimeSinceStartup;

            // Moving Platforms縺ｮ譖ｴ譁ｰ
            UpdateMovingPlatforms();

            // Rotating Platforms縺ｮ譖ｴ譁ｰ
            UpdateRotatingPlatforms();

            // Disappearing Platforms縺ｮ譖ｴ譁ｰ
            UpdateDisappearingPlatforms();

            // Trigger Platforms縺ｮ譖ｴ譁ｰ
            UpdateTriggerPlatforms();

            // 繝励Ξ繧､繝､繝ｼ驕区成蜃ｦ逅・
            if (enablePlayerCarrying)
            {
                UpdatePlayerCarrying();
            }

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹・
            if (optimizePerformance && Time.time - lastOptimizationTime > 1f)
            {
                PerformOptimization();
                lastOptimizationTime = Time.time;
            }

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨ｱ險医・譖ｴ譁ｰ
            averageUpdateTime = (averageUpdateTime + (Time.realtimeSinceStartup - startTime)) / 2f;
        }

        /// <summary>
        /// Moving Platforms縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateMovingPlatforms()
        {
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 繝励Ξ繧､繝､繝ｼ霑第磁繝√ぉ繝・け
                if (platform.activateOnPlayerNearby)
                {
                    CheckPlayerProximity(platform);
                }

                // 遘ｻ蜍募・逅・
                if (platform.isMoving && platform.waypoints.Count >= 2)
                {
                    UpdatePlatformMovement(platform);
                }
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ霑第磁繝√ぉ繝・け
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝遘ｻ蜍輔・譖ｴ譁ｰ
        /// </summary>
        private void UpdatePlatformMovement(MovingPlatform platform)
        {
            // 蠕・ｩ滓凾髢薙・繝√ぉ繝・け
            if (platform.pauseTimer > 0f)
            {
                platform.pauseTimer -= Time.deltaTime;
                return;
            }

            // 迴ｾ蝨ｨ縺ｮ繧ｦ繧ｧ繧､繝昴う繝ｳ繝医→谺｡縺ｮ繧ｦ繧ｧ繧､繝昴う繝ｳ繝・
            Transform currentWaypoint = platform.waypoints[platform.currentWaypointIndex];
            int nextIndex = (platform.currentWaypointIndex + 1) % platform.waypoints.Count;
            Transform nextWaypoint = platform.waypoints[nextIndex];

            // 遘ｻ蜍暮ｲ陦悟ｺｦ縺ｮ譖ｴ譁ｰ
            platform.movementProgress += platform.moveSpeed * Time.deltaTime /
                Vector3.Distance(currentWaypoint.position, nextWaypoint.position);

            // 繧ｫ繝ｼ繝悶↓繧医ｋ繧ｹ繝繝ｼ繧ｺ遘ｻ蜍・
            float curvedProgress = platform.movementCurve.Evaluate(platform.movementProgress);

            // 菴咲ｽｮ縺ｮ險育ｮ・
            Vector3 newPosition = Vector3.Lerp(currentWaypoint.position, nextWaypoint.position, curvedProgress);
            platform.platformTransform.position = newPosition;

            // 繧ｦ繧ｧ繧､繝昴う繝ｳ繝亥芦驕斐メ繧ｧ繝・け
            if (platform.movementProgress >= 1f)
            {
                platform.movementProgress = 0f;
                platform.currentWaypointIndex = nextIndex;

                // 蠕・ｩ滓凾髢楢ｨｭ螳・
                if (platform.pauseAtWaypoint > 0f)
                {
                    platform.pauseTimer = platform.pauseAtWaypoint;
                }

                // 繝ｫ繝ｼ繝怜・逅・
                if (!platform.loopMovement && platform.currentWaypointIndex == 0)
                {
                    platform.isMoving = false;
                }
            }

            // 繧ｨ繝輔ぉ繧ｯ繝医・繧ｵ繧ｦ繝ｳ繝牙・逕・
            PlayMovementEffects(platform);
        }

        /// <summary>
        /// Rotating Platforms縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateRotatingPlatforms()
        {
            foreach (var platform in rotatingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 蝗櫁ｻ｢蜃ｦ逅・
                if (platform.isRotating)
                {
                    float rotationAmount = platform.rotationSpeed * Time.deltaTime;
                    if (platform.reverseDirection) rotationAmount = -rotationAmount;

                    // 蛻ｶ髯舌メ繧ｧ繝・け
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

                    // 蝗櫁ｻ｢驕ｩ逕ｨ
                    platform.platformTransform.Rotate(platform.rotationAxis, rotationAmount);
                }

                // 繧｢繧ｯ繝・ぅ繝吶・繧ｷ繝ｧ繝ｳ蜃ｦ逅・
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
        /// Disappearing Platforms縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateDisappearingPlatforms()
        {
            foreach (var platform in disappearingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 隴ｦ蜻雁・逅・
                if (platform.isWarning)
                {
                    UpdateWarningEffect(platform);
                }

                // 豸亥､ｱ蜃ｦ逅・
                if (platform.isTriggered && platform.isVisible)
                {
                    platform.disappearTimer += Time.deltaTime;

                    if (platform.disappearTimer >= platform.disappearDelay)
                    {
                        DisappearPlatform(platform);
                    }
                }

                // 蜀榊・迴ｾ蜃ｦ逅・
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
        /// Trigger Platforms縺ｮ譖ｴ譁ｰ
        /// </summary>
        private void UpdateTriggerPlatforms()
        {
            foreach (var platform in triggerPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ蜃ｦ逅・
                if (platform.isCooldown)
                {
                    platform.cooldownTimer -= Time.deltaTime;
                    if (platform.cooldownTimer <= 0f)
                    {
                        platform.isCooldown = false;
                    }
                }

                // 繧｢繧ｯ繝・ぅ繝吶・繧ｷ繝ｧ繝ｳ譎る俣蜃ｦ逅・
                if (platform.isActivated && !platform.requiresContinuousActivation)
                {
                    platform.activationTimer -= Time.deltaTime;
                    if (platform.activationTimer <= 0f)
                    {
                        DeactivateTriggerPlatform(platform);
                    }
                }

                // 菴咲ｽｮ縺ｮ陬憺俣
                Vector3 targetPosition = platform.isActivated ? platform.activatedPosition : platform.deactivatedPosition;
                float currentProgress = platform.movementCurve.Evaluate(
                    Vector3.Distance(platform.platformTransform.position, platform.deactivatedPosition) /
                    Vector3.Distance(platform.activatedPosition, platform.deactivatedPosition));

                platform.platformTransform.position = Vector3.MoveTowards(platform.platformTransform.position,
                    targetPosition, platform.movementSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 迚ｩ逅・峩譁ｰ蜃ｦ逅・
        /// </summary>
        private void FixedUpdatePlatformSystem()
        {
            // 迚ｩ逅・ｼ皮ｮ鈴未騾｣縺ｮ譖ｴ譁ｰ蜃ｦ逅・
            UpdatePlatformPhysics();
        }

        /// <summary>
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝迚ｩ逅・ｼ皮ｮ励・譖ｴ譁ｰ
        /// </summary>
        private void UpdatePlatformPhysics()
        {
            // 繝励Ξ繧､繝､繝ｼ驕区成迚ｩ逅・・逅・
            // 繝励Λ繝・ヨ繝輔か繝ｼ繝陦晉ｪ∝・逅・
            // 蜍慕噪鞫ｩ謫ｦ隱ｿ謨ｴ遲・
        }

        #endregion

        #region Platform Effects

        /// <summary>
        /// 遘ｻ蜍輔お繝輔ぉ繧ｯ繝医・蜀咲函
        /// </summary>
        private void PlayMovementEffects(MovingPlatform platform)
        {
            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・
            if (platform.movementEffect != null && !platform.movementEffect.isPlaying)
            {
                platform.movementEffect.Play();
            }

            // 遘ｻ蜍輔し繧ｦ繝ｳ繝・
            if (platform.movementSound != null && !platform.movementSound.isPlaying)
            {
                platform.movementSound.Play();
            }
        }

        /// <summary>
        /// 隴ｦ蜻翫お繝輔ぉ繧ｯ繝医・譖ｴ譁ｰ
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ豸亥､ｱ
        /// </summary>
        private void DisappearPlatform(DisappearingPlatform platform)
        {
            platform.isVisible = false;
            platform.isWarning = false;
            platform.reappearTimer = 0f;

            // 繧ｨ繝輔ぉ繧ｯ繝亥・逕・
            if (platform.disappearEffect != null)
            {
                platform.disappearEffect.Play();
            }

            if (platform.disappearSound != null)
            {
                platform.disappearSound.Play();
            }

            // 繝輔ぉ繝ｼ繝峨い繧ｦ繝亥・逅・
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ蜀榊・迴ｾ
        /// </summary>
        private void ReappearPlatform(DisappearingPlatform platform)
        {
            platform.isVisible = true;
            platform.isTriggered = false;
            platform.disappearTimer = 0f;

            // 繧ｨ繝輔ぉ繧ｯ繝亥・逕・
            if (platform.reappearEffect != null)
            {
                platform.reappearEffect.Play();
            }

            if (platform.reappearSound != null)
            {
                platform.reappearSound.Play();
            }

            // 繝輔ぉ繝ｼ繝峨う繝ｳ蜃ｦ逅・
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
        /// 繝励Ξ繧､繝､繝ｼ驕区成蜃ｦ逅・・譖ｴ譁ｰ
        /// </summary>
        private void UpdatePlayerCarrying()
        {
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 繝励Λ繝・ヨ繝輔か繝ｼ繝荳翫・繝励Ξ繧､繝､繝ｼ繧偵メ繧ｧ繝・け
                CheckPlayersOnPlatform(platform.platformTransform);
            }
        }

        /// <summary>
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝荳翫・繝励Ξ繧､繝､繝ｼ繝√ぉ繝・け
        /// </summary>
        private void CheckPlayersOnPlatform(Transform platformTransform)
        {
            Collider platformCollider = platformTransform.GetComponent<Collider>();
            if (platformCollider == null) return;

            // 繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝医・讀懃ｴ｢
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
        /// 繝励Ξ繧､繝､繝ｼ縺後・繝ｩ繝・ヨ繝輔か繝ｼ繝荳翫↓縺・ｋ縺九メ繧ｧ繝・け
        /// </summary>
        private bool IsPlayerOnPlatform(GameObject player, Collider platformCollider)
        {
            // 繝ｬ繧､繧ｭ繝｣繧ｹ繝医↓繧医ｋ蛻､螳・
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
        /// 繝励Ξ繧､繝､繝ｼ縺ｫ驕区成蜉帙ｒ驕ｩ逕ｨ
        /// </summary>
        private void ApplyCarryForce(GameObject player, Transform platform)
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // 繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ遘ｻ蜍輔・繧ｯ繝医Ν繧定ｨ育ｮ・
                // Note: 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√・繝ｩ繝・ヨ繝輔か繝ｼ繝縺ｮ蜑阪ヵ繝ｬ繝ｼ繝菴咲ｽｮ繧定ｨ倬鹸縺吶ｋ蠢・ｦ√′縺ゅｊ縺ｾ縺・
                Vector3 platformMovement = Vector3.zero; // 邁｡逡･蛹・

                playerRb.MovePosition(playerRb.position + platformMovement);
            }
        }

        /// <summary>
        /// 繝医Μ繧ｬ繝ｼ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ繧｢繧ｯ繝・ぅ繝吶・繝・
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

            // 繧ｨ繝輔ぉ繧ｯ繝医・繧ｵ繧ｦ繝ｳ繝牙・逕・
            if (platform.activationEffect != null)
            {
                platform.activationEffect.Play();
            }

            if (platform.activationSound != null)
            {
                platform.activationSound.Play();
            }

            // 繝槭ユ繝ｪ繧｢繝ｫ螟画峩
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
        /// 繝医Μ繧ｬ繝ｼ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ繝・い繧ｯ繝・ぅ繝吶・繝・
        /// </summary>
        private void DeactivateTriggerPlatform(TriggerPlatform platform)
        {
            platform.isActivated = false;

            // 繝槭ユ繝ｪ繧｢繝ｫ蠕ｩ蜈・
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
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹門・逅・
        /// </summary>
        private void PerformOptimization()
        {
            if (playerCamera == null) return;

            int culledCount = 0;

            // 隕夜㍽螟悶・繝ｩ繝・ヨ繝輔か繝ｼ繝縺ｮ繧ｫ繝ｪ繝ｳ繧ｰ
            foreach (var platform in movingPlatforms)
            {
                if (platform.platformTransform == null) continue;

                bool isVisible = IsVisibleFromCamera(platform.platformTransform.position);
                bool wasMoving = platform.isMoving;

                // 隕夜㍽螟悶・蝣ｴ蜷医∽ｸ譎ょ●豁｢
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
        /// 繧ｫ繝｡繝ｩ縺九ｉ隕九∴繧九°縺ｩ縺・°縺ｮ蛻､螳・
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

            // 譛邨ら憾諷狗｢ｺ菫・
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
        /// 謖・ｮ壹ち繧､繝励・繝励Λ繝・ヨ繝輔か繝ｼ繝謨ｰ繧貞叙蠕・
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
        /// 蜈ｨ繝励Λ繝・ヨ繝輔か繝ｼ繝縺ｮ繝ｪ繧ｻ繝・ヨ
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
        /// Moving Platforms縺ｮ繝ｪ繧ｻ繝・ヨ
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
        /// Rotating Platforms縺ｮ繝ｪ繧ｻ繝・ヨ
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
        /// Disappearing Platforms縺ｮ繝ｪ繧ｻ繝・ヨ
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
        /// Trigger Platforms縺ｮ繝ｪ繧ｻ繝・ヨ
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

            // Moving Platforms縺ｮ蜿ｯ隕門喧
            DrawMovingPlatformGizmos();

            // Rotating Platforms縺ｮ蜿ｯ隕門喧
            DrawRotatingPlatformGizmos();

            // Trigger Platforms縺ｮ蜿ｯ隕門喧
            DrawTriggerPlatformGizmos();
        }

        private void DrawMovingPlatformGizmos()
        {
            foreach (var platform in movingPlatforms)
            {
                if (!platform.showGizmos || platform.platformTransform == null) continue;

                Gizmos.color = platform.gizmosColor;

                // 繧ｦ繧ｧ繧､繝昴う繝ｳ繝医→繝代せ縺ｮ陦ｨ遉ｺ
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

                // 迴ｾ蝨ｨ菴咲ｽｮ縺ｮ陦ｨ遉ｺ
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

                // 蝗櫁ｻ｢霆ｸ縺ｮ陦ｨ遉ｺ
                Gizmos.DrawRay(platform.platformTransform.position, platform.rotationAxis.normalized * 3f);
            }
        }

        private void DrawTriggerPlatformGizmos()
        {
            foreach (var platform in triggerPlatforms)
            {
                if (platform.platformTransform == null) continue;

                // 繧｢繧ｯ繝・ぅ繝吶・繝井ｽ咲ｽｮ
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(platform.activatedPosition, Vector3.one * 0.8f);

                // 繝・い繧ｯ繝・ぅ繝吶・繝井ｽ咲ｽｮ
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(platform.deactivatedPosition, Vector3.one * 0.8f);

                // 遘ｻ蜍輔ヱ繧ｹ
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(platform.activatedPosition, platform.deactivatedPosition);
            }
        }
#endif

        #endregion
    }
}


