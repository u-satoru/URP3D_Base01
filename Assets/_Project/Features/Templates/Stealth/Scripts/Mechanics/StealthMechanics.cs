using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Data;
using AlertLevel = asterivo.Unity60.Core.Data.AlertLevel;
using asterivo.Unity60.Features.AI.Visual;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Mechanics
{
    /// <summary>
    /// ステルスメカニクス管理システム
    /// プレイヤーの隠蔽状態、発見状態、環境との相互作用を管理
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
                    instance = FindObjectOfType<StealthMechanics>();
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
            if (!enableStealthMechanics || playerTransform == null) return;
            
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateStealthState();
                lastUpdateTime = Time.time;
            }
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
            AlertLevel highestAlert = AlertLevel.Unaware;
            
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
                stealthAudioService?.SetAlertLevelMusic(ConvertToAudioAlertLevel(currentAlertLevel));
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
            if (detectionValue < 0.2f) return AlertLevel.Unaware;
            if (detectionValue < 0.4f) return AlertLevel.Suspicious;
            if (detectionValue < 0.6f) return AlertLevel.Investigating;
            if (detectionValue < 0.8f) return AlertLevel.Alert;
            return AlertLevel.Combat;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 現在の可視性レベルを取得
        /// </summary>
        public float GetVisibility() => currentVisibility;
        
        /// <summary>
        /// 現在のノイズレベルを取得
        /// </summary>
        public float GetNoiseLevel() => currentNoiseLevel;
        
        /// <summary>
        /// プレイヤーがカバー内にいるか
        /// </summary>
        public bool IsInCover() => isInCover;
        
        /// <summary>
        /// プレイヤーが影の中にいるか
        /// </summary>
        public bool IsInShadow() => CalculateIsInShadow();
        
        /// <summary>
        /// プレイヤーが検出されているか
        /// </summary>
        public bool IsDetected() => isDetected;
        
        /// <summary>
        /// 現在の検出レベルを取得
        /// </summary>
        public float GetDetectionLevel() => detectionLevel;
        
        /// <summary>
        /// 現在の警戒レベルを取得
        /// </summary>
        public AlertLevel GetAlertLevel() => currentAlertLevel;
        
        /// <summary>
        /// 強制的にステルス状態に入る
        /// </summary>
        public void ForceEnterStealth()
        {
            isDetected = false;
            detectionLevel = 0f;
            currentAlertLevel = AlertLevel.Unaware;
            onEnterStealth?.Raise();
        }
        
        /// <summary>
        /// ディストラクションを作成
        /// </summary>
        public void CreateDistraction(Vector3 position, float radius)
        {
            stealthAudioService?.PlayDistraction(position, radius);
            eventLogger?.Log($"[StealthMechanics] Distraction created at {position}");
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

        /// <summary>
        /// Data.AlertLevel を Audio.Interfaces.AlertLevel に変換
        /// </summary>
        private asterivo.Unity60.Core.Audio.Interfaces.AlertLevel ConvertToAudioAlertLevel(AlertLevel dataAlertLevel)
        {
            return dataAlertLevel switch
            {
                AlertLevel.Unaware => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.None,
                AlertLevel.Suspicious => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.Low,
                AlertLevel.Investigating => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.Medium,
                AlertLevel.Searching => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.Medium,
                AlertLevel.Alert => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.High,
                AlertLevel.Combat => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.Combat,
                _ => asterivo.Unity60.Core.Audio.Interfaces.AlertLevel.None
            };
        }

        #endregion
    }
}