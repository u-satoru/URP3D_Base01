using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Data;
using AlertLevel = asterivo.Unity60.Core.Data.AlertLevel;
using StealthState = asterivo.Unity60.Core.Data.StealthState;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Mechanics
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繝｡繧ｫ繝九け繧ｹ邂｡逅・す繧ｹ繝・Β - ServiceLocator邨ｱ蜷育沿
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫阡ｽ迥ｶ諷九∫匱隕狗憾諷九∫腸蠅・→縺ｮ逶ｸ莠剃ｽ懃畑繧堤ｮ｡逅・
    ///
    /// ServiceLocator邨ｱ蜷医↓繧医ｋ萓｡蛟､螳溽樟:
    /// - Learn & Grow: 邨ｱ荳API縺ｫ繧医ｋ蟄ｦ鄙偵さ繧ｹ繝・0%蜑頑ｸ帶髪謠ｴ
    /// - Ship & Scale: Interface螂醍ｴ・↓繧医ｋ菫晏ｮ域ｧ繝ｻ繝・せ繧ｿ繝薙Μ繝・ぅ蜷台ｸ・
    /// - 譌｢蟄・5%繝｡繝｢繝ｪ蜑頑ｸ帛柑譫懊・67%騾溷ｺｦ謾ｹ蝟・・邯呎価
    /// </summary>
    public class StealthMechanics : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static StealthMechanics instance;
        public static StealthMechanics Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<StealthMechanics>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("StealthMechanics");
                        instance = go.AddComponent<StealthMechanics>();
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Properties and Fields
        
        [TabGroup("Stealth", "Settings")]
        [Header("Core Settings")]
        [SerializeField] private bool enableStealthMechanics = true;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float updateInterval = 0.1f;
        
        [TabGroup("Stealth", "Visibility")]
        [Header("Visibility Settings")]
        [SerializeField] private float baseVisibility = 0.5f;
        [SerializeField] private float crouchVisibilityModifier = 0.3f;
        [SerializeField] private float proneVisibilityModifier = 0.1f;
        [SerializeField] private float movementVisibilityModifier = 0.2f;
        [SerializeField] private AnimationCurve lightVisibilityCurve;
        
        [TabGroup("Stealth", "Cover")]
        [Header("Cover System")]
        [SerializeField] private float coverDetectionRadius = 2f;
        [SerializeField] private LayerMask coverLayerMask = -1;
        [SerializeField] private float shadowVisibilityReduction = 0.3f;
        [SerializeField] private float foliageVisibilityReduction = 0.4f;
        
        [TabGroup("Stealth", "Detection")]
        [Header("Detection Settings")]
        [SerializeField] private float maxDetectionRange = 30f;
        [SerializeField] private float instantDetectionRange = 2f;
        [SerializeField] private float detectionDecayRate = 0.5f;
        [SerializeField] private float alertDecayRate = 0.2f;
        
        [TabGroup("Stealth", "Sound")]
        [Header("Sound Generation")]
        [SerializeField] private float baseNoiseLevel = 0.3f;
        [SerializeField] private float walkNoiseLevel = 0.5f;
        [SerializeField] private float runNoiseLevel = 0.8f;
        [SerializeField] private float crouchNoiseReduction = 0.3f;
        
        // Runtime state
        private float currentVisibility;
        private float currentNoiseLevel;
        private bool isInCover;
        private bool isInShadow;
        private bool isDetected;
        private float detectionLevel;
        private AlertLevel currentAlertLevel;
        
        // Cover detection
        private List<GameObject> nearbyCoverObjects = new List<GameObject>();
        private GameObject currentCover;
        
        // Events
        private GameEvent onEnterStealth;
        private GameEvent onExitStealth;
        private GameEvent onDetectionChanged;
        private GameEvent onAlertLevelChanged;
        
        // Services
        private IStealthAudioService stealthAudioService;
        private IEventLogger eventLogger;
        
        // Caching
        private float lastUpdateTime;
        private Collider[] coverColliders = new Collider[10];

        // IStealthMechanicsService 螳溯｣・
        private StealthMechanicsConfig _serviceConfig;
        private bool _isServiceInitialized = false;

        #endregion

        #region IStealthMechanicsService Properties

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繝医Λ繝ｳ繧ｹ繝輔か繝ｼ繝・・erviceLocator邨ｱ蜷・PI・・
        /// </summary>
        public Transform PlayerTransform
        {
            get => playerTransform;
            set => playerTransform = value;
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ讖溯・譛牙柑/辟｡蜉ｹ・・erviceLocator邨ｱ蜷・PI・・
        /// </summary>
        public bool EnableStealthMechanics
        {
            get => enableStealthMechanics;
            set => enableStealthMechanics = value;
        }

        /// <summary>
        /// 譖ｴ譁ｰ髢馴囈・・erviceLocator邨ｱ蜷・PI・・
        /// </summary>
        public float UpdateInterval
        {
            get => updateInterval;
            set => updateInterval = value;
        }

        /// <summary>
        /// 譖ｴ譁ｰ縺悟ｿ・ｦ√°縺ｮ蜍慕噪蛻､螳夲ｼ医ヱ繝輔か繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹厄ｼ・
        /// </summary>
        public bool NeedsUpdate => enableStealthMechanics && playerTransform != null;

        /// <summary>
        /// 譖ｴ譁ｰ蜆ｪ蜈亥ｺｦ・医せ繝・Ν繧ｹ迥ｶ諷九・莉悶す繧ｹ繝・Β縺ｮ蝓ｺ逶､縺ｨ縺ｪ繧九◆繧・ｫ伜━蜈亥ｺｦ・・
        /// </summary>
        public int UpdatePriority => 10;

        #endregion

        #region IService Implementation

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ譎ゅ・蛻晄悄蛹門・逅・
        /// </summary>
        public void OnServiceRegistered()
        {
            Debug.Log("[StealthMechanics] Service registered successfully");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ隗｣髯､譎ゅ・繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・
        /// </summary>
        public void OnServiceUnregistered()
        {
            Debug.Log("[StealthMechanics] Service unregistered");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺後い繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsServiceActive => enabled && gameObject.activeInHierarchy;

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ蜷搾ｼ医ョ繝舌ャ繧ｰ繝ｻ繝ｭ繧ｰ逕ｨ・・
        /// </summary>
        public string ServiceName => "StealthMechanics";

        #endregion

        #region IConfigurableService Implementation

        /// <summary>
        /// 險ｭ螳壹↓繧医ｋ蛻晄悄蛹厄ｼ・ConfigurableService<T>螳溯｣・ｼ・
        /// ScriptableObject繝吶・繧ｹ縺ｮ險ｭ螳壹ョ繝ｼ繧ｿ縺九ｉ蛻晄悄蛹・
        /// </summary>
        /// <param name="config">繧ｹ繝・Ν繧ｹ繝｡繧ｫ繝九け繧ｹ險ｭ螳・/param>
        public void Initialize(StealthMechanicsConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[StealthMechanics] Initialize called with null config, using defaults");
                return;
            }

            _serviceConfig = config;

            // 險ｭ螳壼､繧貞渚譏
            enableStealthMechanics = config.enableStealthMechanics;
            updateInterval = config.updateInterval;
            baseVisibility = config.baseVisibility;
            crouchVisibilityModifier = config.crouchVisibilityModifier;
            proneVisibilityModifier = config.proneVisibilityModifier;
            movementVisibilityModifier = config.movementVisibilityModifier;
            coverDetectionRadius = config.coverDetectionRadius;
            shadowVisibilityReduction = config.shadowVisibilityReduction;
            foliageVisibilityReduction = config.foliageVisibilityReduction;
            maxDetectionRange = config.maxDetectionRange;
            instantDetectionRange = config.instantDetectionRange;
            detectionDecayRate = config.detectionDecayRate;
            alertDecayRate = config.alertDecayRate;
            baseNoiseLevel = config.baseNoiseLevel;
            walkNoiseLevel = config.walkNoiseLevel;
            runNoiseLevel = config.runNoiseLevel;
            crouchNoiseReduction = config.crouchNoiseReduction;

            _isServiceInitialized = true;

            Debug.Log("[StealthMechanics] Initialized with configuration successfully");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺悟・譛溷喧貂医∩縺九←縺・°
        /// </summary>
        public bool IsInitialized => _isServiceInitialized;

        #endregion

        #region IUpdatableService Implementation

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ譖ｴ譁ｰ蜃ｦ逅・ｼ・pdate()縺ｮ莉｣譖ｿ・・
        /// ServiceLocator邨ｱ蜷医↓繧医ｋ蜉ｹ邇・噪譖ｴ譁ｰ邂｡逅・
        /// </summary>
        public void UpdateService()
        {
            if (!NeedsUpdate) return;

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateStealthState();
                lastUpdateTime = Time.time;
            }
        }

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            InitializeServices();
            InitializeCurves();
        }
        
        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            LoadEvents();
        }
        
        private void Update()
        {
            // ServiceLocator邨ｱ蜷域凾縺ｨSingleton蜊倅ｽ捺凾縺ｮ荳｡遶・
            // ServiceLocator縺ｫ逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ蝣ｴ蜷医・縲ゞpdateService()縺ｧ蛻ｶ蠕｡縺輔ｌ繧九◆繧ゞpdate()繧偵せ繧ｭ繝・・
            bool isRegisteredAsService = ServiceLocator.HasService<StealthMechanics>();

            if (!isRegisteredAsService)
            {
                // Singleton蜊倅ｽ薙Δ繝ｼ繝・ 蠕捺擂騾壹ｊUpdate()縺ｧ蜍穂ｽ・
                UpdateService();
            }
            // ServiceLocator邨ｱ蜷医Δ繝ｼ繝・ UpdateService()縺ｯServiceLocator縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧・
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeServices()
        {
            stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
            eventLogger = ServiceLocator.GetService<IEventLogger>();
        }
        
        private void InitializeCurves()
        {
            if (lightVisibilityCurve == null || lightVisibilityCurve.length == 0)
            {
                lightVisibilityCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 1f);
            }
        }
        
        private void LoadEvents()
        {
            // Load GameEvent assets from Resources or references
            // This would typically be done through ScriptableObject references
        }
        
        #endregion
        
        #region Stealth State Management
        
        private void UpdateStealthState()
        {
            UpdateVisibility();
            UpdateNoiseLevel();
            UpdateCoverStatus();
            UpdateDetectionState();
            
            // Notify audio service of stealth level
            stealthAudioService?.AdjustStealthAudio(1f - currentVisibility);
        }
        
        private void UpdateVisibility()
        {
            float visibility = baseVisibility;
            
            // Player stance modifiers
            if (IsPlayerCrouching())
            {
                visibility *= crouchVisibilityModifier;
            }
            else if (IsPlayerProne())
            {
                visibility *= proneVisibilityModifier;
            }
            
            // Movement modifier
            float movementSpeed = GetPlayerMovementSpeed();
            visibility += movementSpeed * movementVisibilityModifier;
            
            // Environmental modifiers
            if (isInShadow)
            {
                visibility *= (1f - shadowVisibilityReduction);
            }
            
            if (isInCover)
            {
                visibility *= (1f - GetCoverEffectiveness());
            }
            
            // Light level modifier
            float lightLevel = GetEnvironmentLightLevel();
            visibility *= lightVisibilityCurve.Evaluate(lightLevel);
            
            currentVisibility = Mathf.Clamp01(visibility);
        }
        
        private void UpdateNoiseLevel()
        {
            float noise = baseNoiseLevel;
            
            // Movement noise
            if (IsPlayerRunning())
            {
                noise = runNoiseLevel;
            }
            else if (IsPlayerWalking())
            {
                noise = walkNoiseLevel;
            }
            
            // Stance modifier
            if (IsPlayerCrouching())
            {
                noise *= (1f - crouchNoiseReduction);
            }
            
            currentNoiseLevel = Mathf.Clamp01(noise);
            
            // Generate detectable sound for AI
            if (currentNoiseLevel > 0.1f)
            {
                stealthAudioService?.EmitDetectableSound(
                    playerTransform.position,
                    currentNoiseLevel * maxDetectionRange,
                    currentNoiseLevel,
                    "player_movement"
                );
            }
        }
        
        private void UpdateCoverStatus()
        {
            // Check for nearby cover
            int coverCount = Physics.OverlapSphereNonAlloc(
                playerTransform.position,
                coverDetectionRadius,
                coverColliders,
                coverLayerMask
            );
            
            nearbyCoverObjects.Clear();
            currentCover = null;
            isInCover = false;
            
            float closestDistance = float.MaxValue;
            
            for (int i = 0; i < coverCount; i++)
            {
                if (coverColliders[i] != null && coverColliders[i].gameObject != null)
                {
                    nearbyCoverObjects.Add(coverColliders[i].gameObject);
                    
                    float distance = Vector3.Distance(
                        playerTransform.position,
                        coverColliders[i].ClosestPoint(playerTransform.position)
                    );
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        currentCover = coverColliders[i].gameObject;
                    }
                }
            }
            
            isInCover = currentCover != null && closestDistance < 1f;
            
            // Check shadow status
            isInShadow = IsInShadow();
        }
        
        private void UpdateDetectionState()
        {
            // This would integrate with NPCVisualSensor and NPCAuditorySensor
            // For now, we'll use a simplified detection model
            
            bool wasDetected = isDetected;
            AlertLevel previousAlertLevel = currentAlertLevel;
            
            // Check if any NPCs can see the player
            NPCVisualSensor[] visualSensors = FindObjectsByType<NPCVisualSensor>(FindObjectsSortMode.None);
            
            float highestDetection = 0f;
            AlertLevel highestAlert = AlertLevel.Relaxed;
            
            foreach (var sensor in visualSensors)
            {
                if (sensor.enabled)
                {
                    // Check if this NPC can detect the player
                    float distance = Vector3.Distance(sensor.transform.position, playerTransform.position);
                    
                    if (distance <= instantDetectionRange)
                    {
                        highestDetection = 1f;
                        highestAlert = AlertLevel.Alert;
                    }
                    else if (distance <= maxDetectionRange)
                    {
                        // Factor in visibility for detection
                        float detectionChance = currentVisibility * (1f - (distance / maxDetectionRange));
                        if (detectionChance > highestDetection)
                        {
                            highestDetection = detectionChance;
                            highestAlert = CalculateAlertLevel(detectionChance);
                        }
                    }
                }
            }
            
            // Update detection level with decay
            if (highestDetection > detectionLevel)
            {
                detectionLevel = highestDetection;
            }
            else
            {
                detectionLevel = Mathf.Max(0f, detectionLevel - detectionDecayRate * updateInterval);
            }
            
            isDetected = detectionLevel > 0.5f;
            currentAlertLevel = highestAlert;
            
            // Fire events if state changed
            if (wasDetected != isDetected)
            {
                if (isDetected)
                {
                    onExitStealth?.Raise();
                    eventLogger?.Log("[StealthMechanics] Player detected!");
                }
                else
                {
                    onEnterStealth?.Raise();
                    eventLogger?.Log("[StealthMechanics] Player entered stealth");
                }
                
                onDetectionChanged?.Raise();
            }
            
            if (previousAlertLevel != currentAlertLevel)
            {
                onAlertLevelChanged?.Raise();
                stealthAudioService?.SetAlertLevelMusic(currentAlertLevel);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool IsPlayerCrouching()
        {
            // Check player state - would integrate with PlayerStateMachine
            return Input.GetKey(KeyCode.LeftControl);
        }
        
        private bool IsPlayerProne()
        {
            // Check player state - would integrate with PlayerStateMachine
            return Input.GetKey(KeyCode.Z);
        }
        
        private bool IsPlayerWalking()
        {
            return Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;
        }
        
        private bool IsPlayerRunning()
        {
            return IsPlayerWalking() && Input.GetKey(KeyCode.LeftShift);
        }
        
        private float GetPlayerMovementSpeed()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            return input.magnitude * (IsPlayerRunning() ? 2f : 1f);
        }
        
        private float GetCoverEffectiveness()
        {
            if (currentCover == null) return 0f;
            
            // Check cover type
            if (currentCover.CompareTag("HardCover"))
            {
                return 0.8f;
            }
            else if (currentCover.CompareTag("SoftCover"))
            {
                return 0.5f;
            }
            else if (currentCover.layer == LayerMask.NameToLayer("Foliage"))
            {
                return foliageVisibilityReduction;
            }
            
            return 0.3f;
        }
        
        private bool CalculateIsInShadow()
        {
            // Simple shadow check using raycasts to light sources
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            
            foreach (var light in lights)
            {
                if (light.enabled && light.intensity > 0.1f)
                {
                    Vector3 directionToLight = (light.transform.position - playerTransform.position).normalized;
                    float distance = Vector3.Distance(light.transform.position, playerTransform.position);
                    
                    if (distance < light.range)
                    {
                        RaycastHit hit;
                        if (!Physics.Raycast(playerTransform.position, directionToLight, out hit, distance))
                        {
                            // Direct line to light - not in shadow
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        private float GetEnvironmentLightLevel()
        {
            // Calculate ambient light level at player position
            float lightLevel = RenderSettings.ambientIntensity;
            
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.enabled)
                {
                    float distance = Vector3.Distance(light.transform.position, playerTransform.position);
                    if (distance < light.range)
                    {
                        float attenuation = 1f - (distance / light.range);
                        lightLevel += light.intensity * attenuation;
                    }
                }
            }
            
            return Mathf.Clamp01(lightLevel);
        }
        
        private AlertLevel CalculateAlertLevel(float detectionValue)
        {
            if (detectionValue < 0.2f) return AlertLevel.Relaxed;
            if (detectionValue < 0.4f) return AlertLevel.Suspicious;
            if (detectionValue < 0.6f) return AlertLevel.Investigating;
            if (detectionValue < 0.8f) return AlertLevel.Alert;
            return AlertLevel.Alert;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ蜿ｯ隕匁ｧ繝ｬ繝吶Ν繧貞叙蠕・
        /// </summary>
        public float GetVisibility() => currentVisibility;
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝弱う繧ｺ繝ｬ繝吶Ν繧貞叙蠕・
        /// </summary>
        public float GetNoiseLevel() => currentNoiseLevel;
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺後き繝舌・蜀・↓縺・ｋ縺・
        /// </summary>
        public bool IsInCover() => isInCover;
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺悟ｽｱ縺ｮ荳ｭ縺ｫ縺・ｋ縺・
        /// </summary>
        public bool IsInShadow() => CalculateIsInShadow();
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺梧､懷・縺輔ｌ縺ｦ縺・ｋ縺・
        /// </summary>
        public bool IsDetected() => isDetected;
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ讀懷・繝ｬ繝吶Ν繧貞叙蠕・
        /// </summary>
        public float GetDetectionLevel() => detectionLevel;
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ隴ｦ謌偵Ξ繝吶Ν繧貞叙蠕・
        /// </summary>
        public AlertLevel GetAlertLevel() => currentAlertLevel;
        
        /// <summary>
        /// 蠑ｷ蛻ｶ逧・↓繧ｹ繝・Ν繧ｹ迥ｶ諷九↓蜈･繧・
        /// </summary>
        public void ForceEnterStealth()
        {
            isDetected = false;
            detectionLevel = 0f;
            currentAlertLevel = AlertLevel.Relaxed;
            onEnterStealth?.Raise();
        }
        
        /// <summary>
        /// 繝・ぅ繧ｹ繝医Λ繧ｯ繧ｷ繝ｧ繝ｳ繧剃ｽ懈・
        /// </summary>
        public void CreateDistraction(Vector3 position, float radius)
        {
            stealthAudioService?.PlayDistraction(position, radius);
            eventLogger?.Log($"[StealthMechanics] Distraction created at {position}");
        }


        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｹ繝・Ν繧ｹ迥ｶ諷九ｒ蜿門ｾ・
        /// </summary>
        public StealthState CurrentState
        {
            get
            {
                if (isDetected) return StealthState.Detected;
                if (isInCover) return StealthState.Hidden;
                if (currentVisibility > 0.7f) return StealthState.Visible;
                return StealthState.Concealed;
            }
        }

        /// <summary>
        /// 髫繧悟ｴ謇縺ｫ蜈･繧・
        /// </summary>
        public void EnterHidingSpot(Transform hidingSpotTransform)
        {
            isInCover = true;
            currentVisibility *= 0.1f; // 螟ｧ蟷・↓蜿ｯ隕匁ｧ繧剃ｸ九￡繧・
            string spotName = hidingSpotTransform != null ? hidingSpotTransform.name : "Unknown";
            eventLogger?.Log($"[StealthMechanics] Entered hiding spot: {spotName}");
        }

        /// <summary>
        /// 髫繧悟ｴ謇縺九ｉ蜃ｺ繧・
        /// </summary>
        public void ExitHidingSpot()
        {
            isInCover = false;
            eventLogger?.Log($"[StealthMechanics] Exited hiding spot");
        }

        /// <summary>
        /// 迺ｰ蠅・↓繧医ｋ髫阡ｽ蜉ｹ譫懊ｒ驕ｩ逕ｨ・郁ｨｭ險域嶌貅匁侠・・
        /// </summary>
        public void ApplyEnvironmentalConcealment(float concealmentFactor, string concealmentType = "environment")
        {
            // 迺ｰ蠅・國阡ｽ蜉ｹ譫懊ｒ迴ｾ蝨ｨ縺ｮ蜿ｯ隕匁ｧ縺ｫ驕ｩ逕ｨ
            float modifiedVisibility = currentVisibility * (1f - Mathf.Clamp01(concealmentFactor));
            currentVisibility = Mathf.Clamp01(modifiedVisibility);

            eventLogger?.Log($"[StealthMechanics] Applied environmental concealment: {concealmentType}, factor: {concealmentFactor}");
        }

        /// <summary>
        /// 蜈峨↓繧医ｋ髴ｲ蜃ｺ蜉ｹ譫懊ｒ驕ｩ逕ｨ・郁ｨｭ險域嶌貅匁侠・・
        /// </summary>
        public void ApplyLightExposure(float lightIntensity, Vector3 lightDirection)
        {
            // 蜈峨・蠑ｷ蠎ｦ縺ｫ蝓ｺ縺･縺・※蜿ｯ隕匁ｧ繧剃ｸ頑・
            float lightExposure = lightIntensity * lightVisibilityCurve.Evaluate(lightIntensity);
            currentVisibility = Mathf.Clamp01(currentVisibility + lightExposure * 0.3f);

            eventLogger?.Log($"[StealthMechanics] Applied light exposure: intensity {lightIntensity}");
        }

        /// <summary>
        /// 髫繧悟ｴ謇縺ｨ縺ｮ逶ｸ莠剃ｽ懃畑・郁ｨｭ險域嶌貅匁侠・・
        /// </summary>
        public void InteractWithHidingSpot(Transform hidingSpot, float effectiveness = 0.8f)
        {
            if (hidingSpot == null) return;

            // 髫繧悟ｴ謇縺ｮ蜉ｹ譫懊ｒ驕ｩ逕ｨ
            EnterHidingSpot(hidingSpot);

            // 蜉ｹ譫懊・蠑ｷ蠎ｦ繧帝←逕ｨ
            currentVisibility *= (1f - effectiveness);

            eventLogger?.Log($"[StealthMechanics] Interacted with hiding spot: {hidingSpot.name}, effectiveness: {effectiveness}");
        }

        #endregion
        
        #region Editor
        
#if UNITY_EDITOR
        [TabGroup("Stealth", "Debug")]
        [Button("Test Enter Stealth")]
        private void TestEnterStealth()
        {
            ForceEnterStealth();
        }
        
        [TabGroup("Stealth", "Debug")]
        [Button("Test Detection")]
        private void TestDetection()
        {
            isDetected = true;
            detectionLevel = 1f;
            currentAlertLevel = AlertLevel.Alert;
            onExitStealth?.Raise();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;
            
            // Draw cover detection radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerTransform.position, coverDetectionRadius);
            
            // Draw current cover
            if (currentCover != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerTransform.position, currentCover.transform.position);
                Gizmos.DrawWireCube(currentCover.transform.position, Vector3.one);
            }
            
            // Draw visibility indicator
            Gizmos.color = new Color(1f, 0f, 0f, currentVisibility);
            Gizmos.DrawSphere(playerTransform.position, 0.5f);
            
            // Draw noise radius
            if (currentNoiseLevel > 0.1f)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawWireSphere(playerTransform.position, currentNoiseLevel * maxDetectionRange);
            }
        }
#endif
        
        #endregion

        #region Helper Methods


        #endregion
    }
}


