using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using asterivo.Unity60.Features.Templates.Stealth.AI;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.UI;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// Layer 2: Runtime Management・亥ｮ溯｡梧凾邂｡逅・ｱ､・・
    /// 繧ｹ繝・Ν繧ｹ繝・Φ繝励Ξ繝ｼ繝医・繝｡繧､繝ｳ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ
    /// ServiceLocator + Event鬧・虚縺ｮ繝上う繝悶Μ繝・ラ繧｢繝ｼ繧ｭ繝・け繝√Ε螳溯｣・
    /// Learn & Grow萓｡蛟､螳溽樟縺ｮ荳ｭ螟ｮ蛻ｶ蠕｡繧ｷ繧ｹ繝・Β
    /// </summary>
    public class StealthTemplateManager : MonoBehaviour
    {
        #region Singleton Implementation
        private static StealthTemplateManager _instance;
        public static StealthTemplateManager Instance => _instance;

        [Header("Configuration")]
        [SerializeField] private StealthTemplateConfig _config;
        
        public StealthTemplateConfig Config => _config;
        #endregion

        #region Core System References・・erviceLocator邨ｱ蜷茨ｼ・
        private StealthMechanicsController _mechanicsController;
        private StealthAICoordinator _aiCoordinator;
        private StealthEnvironmentManager _environmentManager;
        private StealthUIManager _uiManager;

        // 譌｢蟄倥が繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β邨ｱ蜷茨ｼ・tealthAudioCoordinator・・
        private asterivo.Unity60.Core.Audio.StealthAudioCoordinator _audioCoordinator;

        // Event System Integration
        private readonly Dictionary<string, GameEvent> _eventChannels = new();

        // Performance Monitoring
        // TODO: Implement StealthPerformanceMonitor for performance monitoring
        // private StealthPerformanceMonitor _performanceMonitor;

        // Learn & Grow System Integration
        // TODO: Implement StealthTutorialSystem and StealthProgressionTracker for Learn & Grow value
        private object _tutorialSystem; // Placeholder until StealthTutorialSystem is implemented
        private object _progressionTracker; // Placeholder until StealthProgressionTracker is implemented
        #endregion

        #region Template State Management
        public bool IsStealthModeEnabled { get; private set; }
        public bool IsInitialized { get; private set; }
        public StealthState CurrentStealthState => _mechanicsController?.CurrentState ?? StealthState.Visible;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeSingleton();
            InitializeSubsystems();
            SetupEventChannels();
        }

        private void Start()
        {
            if (_config != null)
            {
                ApplyConfiguration(_config);
            }
            else
            {
                Debug.LogWarning("StealthTemplateManager: No configuration assigned. Using default settings.");
                LoadDefaultConfiguration();
            }
        }

        private void Update()
        {
            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕厄ｼ域怙驕ｩ蛹悶・縺溘ａ菴朱ｻ蠎ｦ螳溯｡鯉ｼ・
            // TODO: Implement performance monitoring when StealthPerformanceMonitor is available
            // if (Time.frameCount % 60 == 0) // 60繝輔Ξ繝ｼ繝縺斐→
            // {
            //     _performanceMonitor?.UpdatePerformanceMetrics();
            // }
        }

        private void OnDestroy()
        {
            CleanupEventChannels();
        }
        #endregion

        #region Initialization
        private void InitializeSingleton()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("StealthTemplateManager: Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("StealthTemplateManager: Singleton initialized successfully.");
        }

        private void InitializeSubsystems()
        {
            Debug.Log("StealthTemplateManager: Initializing subsystems...");

            // ServiceLocator pattern for subsystem management
            _mechanicsController = GetOrCreateSubsystem<StealthMechanicsController>();
            _aiCoordinator = GetOrCreateSubsystem<StealthAICoordinator>();
            _environmentManager = GetOrCreateSubsystem<StealthEnvironmentManager>();
            _uiManager = GetOrCreateSubsystem<StealthUIManager>();

            // 譌｢蟄倥が繝ｼ繝・ぅ繧ｪ繧ｷ繧ｹ繝・Β邨ｱ蜷・
            _audioCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
            if (_audioCoordinator == null)
            {
                Debug.LogWarning("StealthTemplateManager: StealthAudioCoordinator not found. Audio integration may not work properly.");
            }

            // ServiceLocator邨ｱ蜷・ StealthMechanics 竊・IStealthMechanicsService逋ｻ骭ｲ
            RegisterStealthMechanicsService();

            // Performance monitoring
            // TODO: Initialize StealthPerformanceMonitor when implemented
            // _performanceMonitor = GetOrCreateSubsystem<StealthPerformanceMonitor>();

            // Learn & Grow systems
            // TODO: Initialize StealthTutorialSystem and StealthProgressionTracker when implemented
            // _tutorialSystem = GetOrCreateSubsystem<StealthTutorialSystem>();
            // _progressionTracker = GetOrCreateSubsystem<StealthProgressionTracker>();

            IsInitialized = true;
            Debug.Log("StealthTemplateManager: All subsystems initialized successfully.");
        }

        /// <summary>
        /// ServiceLocator邨ｱ蜷・ StealthMechanics 竊・IStealthMechanicsService逋ｻ骭ｲ
        /// Phase 1: Core Service Integration 縺ｮ譬ｸ蠢・ｮ溯｣・
        /// Learn & Grow萓｡蛟､螳溽樟縺ｮ縺溘ａ縺ｮ邨ｱ荳API謠蝉ｾ・
        /// </summary>
        private void RegisterStealthMechanicsService()
        {
            // StealthMechanics繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ讀懃ｴ｢
            var stealthMechanics = FindFirstObjectByType<StealthMechanics>();

            if (stealthMechanics == null)
            {
                Debug.LogWarning("StealthTemplateManager: StealthMechanics component not found. ServiceLocator registration skipped.");
                return;
            }

            // ServiceLocator縺ｫStealthMechanics縺ｨ縺励※逋ｻ骭ｲ
            try
            {
                ServiceLocator.RegisterService<StealthMechanics>(stealthMechanics);

                // 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ騾夂衍
                stealthMechanics.OnServiceRegistered();

                Debug.Log("StealthTemplateManager: StealthMechanics successfully registered as IStealthMechanicsService in ServiceLocator.");
                Debug.Log($"StealthTemplateManager: Service can be accessed via ServiceLocator.Instance.Get<IStealthMechanicsService>()");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"StealthTemplateManager: Failed to register StealthMechanics service: {ex.Message}");
            }
        }

        private T GetOrCreateSubsystem<T>() where T : Component
        {
            // 縺ｾ縺壼ｭ舌が繝悶ず繧ｧ繧ｯ繝医°繧画､懃ｴ｢
            T component = GetComponentInChildren<T>();
            if (component != null)
            {
                Debug.Log($"StealthTemplateManager: Found existing {typeof(T).Name}");
                return component;
            }

            // 繧ｷ繝ｼ繝ｳ蜈ｨ菴薙°繧画､懃ｴ｢
            component = FindFirstObjectByType<T>();
            if (component != null)
            {
                Debug.Log($"StealthTemplateManager: Found {typeof(T).Name} in scene");
                return component;
            }

            // 譁ｰ隕丈ｽ懈・
            GameObject subsystemObject = new GameObject(typeof(T).Name);
            subsystemObject.transform.SetParent(transform);
            component = subsystemObject.AddComponent<T>();

            Debug.Log($"StealthTemplateManager: Created new {typeof(T).Name}");
            return component;
        }

        private void SetupEventChannels()
        {
            if (_config == null) return;

            // Event Channels registration
            RegisterEventChannel("StealthStateChanged", _config.OnStealthStateChanged);
            RegisterEventChannel("DetectionLevelChanged", _config.OnDetectionLevelChanged);
            RegisterEventChannel("EnvironmentInteraction", _config.OnEnvironmentInteraction);

            Debug.Log("StealthTemplateManager: Event channels setup completed.");
        }

        private void RegisterEventChannel(string channelName, GameEvent gameEvent)
        {
            if (gameEvent != null)
            {
                _eventChannels[channelName] = gameEvent;
                Debug.Log($"StealthTemplateManager: Registered event channel: {channelName}");
            }
        }

        private void CleanupEventChannels()
        {
            _eventChannels.Clear();
            Debug.Log("StealthTemplateManager: Event channels cleaned up.");
        }
        #endregion

        #region Configuration Management
        public void ApplyConfiguration(StealthTemplateConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot apply null configuration");
                return;
            }

            _config = config;

            Debug.Log("StealthTemplateManager: Applying configuration...");

            // 蜷・し繝悶す繧ｹ繝・Β縺ｫ險ｭ螳壹ｒ驕ｩ逕ｨ
            ApplyConfigurationToSubsystems();

            // Event鬧・虚縺ｫ繧医ｋ險ｭ螳壼､画峩騾夂衍
            RaiseStealthConfigurationChangedEvent(config);

            Debug.Log("StealthTemplateManager: Configuration applied successfully.");
        }

        private void ApplyConfigurationToSubsystems()
        {
            // Mechanics Controller
            if (_mechanicsController != null)
            {
                _mechanicsController.ApplyConfiguration(_config.Mechanics);
            }

            // AI Coordinator
            if (_aiCoordinator != null)
            {
                _aiCoordinator.ApplyConfiguration(_config.AIConfiguration);
            }

            // Environment Manager
            if (_environmentManager != null)
            {
                _environmentManager.ApplyConfiguration(_config.Environment);
            }

            // UI Manager
            if (_uiManager != null)
            {
                _uiManager.ApplyConfiguration(_config.UISettings);
            }

            // Audio Coordinator・域里蟄倥す繧ｹ繝・Β邨ｱ蜷茨ｼ・
            if (_audioCoordinator != null)
            {
                // 譌｢蟄倥・StealthAudioCoordinator縺ｫ險ｭ螳壹ｒ驕ｩ逕ｨ
                // 蜈ｷ菴鍋噪縺ｪ驕ｩ逕ｨ譁ｹ豕輔・譌｢蟄倥・螳溯｣・↓萓晏ｭ・
                Debug.Log("StealthTemplateManager: Audio configuration applied via StealthAudioCoordinator");
            }

            // Tutorial System・・earn & Grow萓｡蛟､螳溽樟・・
            if (_tutorialSystem != null)
            {
                // TODO: Implement StealthTutorialSystem.ApplyConfiguration when system is ready
                // _tutorialSystem.ApplyConfiguration(_config.TutorialConfig);
                Debug.Log("StealthTemplateManager: Tutorial system configuration would be applied here");
            }

            // Progression Tracker・・earn & Grow萓｡蛟､螳溽樟・・
            if (_progressionTracker != null)
            {
                // TODO: Implement StealthProgressionTracker.ApplyConfiguration when system is ready
                // _progressionTracker.ApplyConfiguration(_config.ProgressionConfig);
                Debug.Log("StealthTemplateManager: Progression tracker configuration would be applied here");
            }

            // Performance Monitor
            // TODO: Implement performance monitor configuration when available
            // if (_performanceMonitor != null)
            // {
            //     _performanceMonitor.ApplyConfiguration(_config.PerformanceConfig);
            // }
        }

        private void LoadDefaultConfiguration()
        {
            // 繝・ヵ繧ｩ繝ｫ繝郁ｨｭ螳壹・菴懈・
            var defaultConfig = ScriptableObject.CreateInstance<StealthTemplateConfig>();
            defaultConfig.ResetConfiguration();
            
            ApplyConfiguration(defaultConfig);
            
            Debug.Log("StealthTemplateManager: Default configuration loaded.");
        }

        private void RaiseStealthConfigurationChangedEvent(StealthTemplateConfig config)
        {
            // 繧ｫ繧ｹ繧ｿ繝繧､繝吶Φ繝医ョ繝ｼ繧ｿ縺ｮ菴懈・縺ｨEvent逋ｺ陦・
            var configData = new StealthConfigurationEventData
            {
                Config = config,
                Timestamp = Time.time
            };

            // 險ｭ螳壼､画峩繧､繝吶Φ繝医・逋ｺ陦・
            if (_eventChannels.TryGetValue("StealthStateChanged", out GameEvent stealthEvent))
            {
                stealthEvent.Raise();
            }

            Debug.Log("StealthTemplateManager: Configuration change event raised.");
        }
        #endregion

        #region Template-specific API
        public void EnableStealthMode()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot enable stealth mode - system not initialized");
                return;
            }

            IsStealthModeEnabled = true;
            _mechanicsController?.EnableStealthMode();
            
            Debug.Log("StealthTemplateManager: Stealth mode enabled.");
        }

        public void DisableStealthMode()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot disable stealth mode - system not initialized");
                return;
            }

            IsStealthModeEnabled = false;
            _mechanicsController?.DisableStealthMode();

            Debug.Log("StealthTemplateManager: Stealth mode disabled.");
        }
        #endregion

        #region AI System Integration
        public void RegisterNPC(MonoBehaviour npc)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot register NPC - system not initialized");
                return;
            }

            _aiCoordinator?.RegisterNPC(npc);
            Debug.Log($"StealthTemplateManager: NPC registered: {npc}");
        }

        public void UnregisterNPC(MonoBehaviour npc)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot unregister NPC - system not initialized");
                return;
            }

            _aiCoordinator?.UnregisterNPC(npc);
            Debug.Log($"StealthTemplateManager: NPC unregistered: {npc}");
        }
        #endregion

        #region Learn & Grow Value Realization
        public bool IsLearnAndGrowSystemReady()
        {
            // TODO: Implement when StealthTutorialSystem and StealthProgressionTracker are created
            bool tutorialReady = _tutorialSystem != null; // && _tutorialSystem.IsSystemReady();
            bool trackerReady = _progressionTracker != null;

            Debug.Log($"StealthTemplateManager: Learn & Grow system ready check - Tutorial: {tutorialReady}, Tracker: {trackerReady}");
            return tutorialReady && trackerReady;
        }

        public float GetLearningProgress()
        {
            // TODO: Implement when StealthProgressionTracker is created
            // return _progressionTracker?.GetOverallProgress() ?? 0f;
            float defaultProgress = 0f;
            Debug.Log($"StealthTemplateManager: Learning progress (placeholder): {defaultProgress}");
            return defaultProgress;
        }

        public bool IsBasicGameplayReady()
        {
            // TODO: Implement when StealthProgressionTracker is created
            // return _progressionTracker?.IsBasicGameplayReady() ?? false;
            bool defaultReady = _progressionTracker != null;
            Debug.Log($"StealthTemplateManager: Basic gameplay ready (placeholder): {defaultReady}");
            return defaultReady;
        }

        public void StartQuickLearningMode()
        {
            if (_tutorialSystem != null && _config.TutorialConfig.EnableQuickStart)
            {
                // TODO: Implement when StealthTutorialSystem is created
                // _tutorialSystem.StartQuickLearning();
                Debug.Log("StealthTemplateManager: Quick learning mode started (placeholder) - Learn & Grow value realization");
            }
            else
            {
                Debug.LogWarning("StealthTemplateManager: Cannot start quick learning - tutorial system not available or quick start disabled");
            }
        }
        #endregion

        #region Debug & Diagnostics
        [ContextMenu("Apply Default Configuration")]
        public void ApplyDefaultConfigurationDebug()
        {
            LoadDefaultConfiguration();
        }

        [ContextMenu("Log System Status")]
        public void LogSystemStatus()
        {
            Debug.Log($"StealthTemplateManager Status:" +
                     $"\n- Initialized: {IsInitialized}" +
                     $"\n- Stealth Mode: {IsStealthModeEnabled}" +
                     $"\n- Current State: {CurrentStealthState}" +
                     $"\n- Learn & Grow Ready: {IsLearnAndGrowSystemReady()}" +
                     $"\n- Learning Progress: {GetLearningProgress():P}" +
                     $"\n- Basic Gameplay Ready: {IsBasicGameplayReady()}");
        }

        public bool ValidateSystemIntegrity()
        {
            bool isValid = true;

            if (_config == null)
            {
                Debug.LogError("StealthTemplateManager: Configuration is null");
                isValid = false;
            }

            if (_mechanicsController == null)
            {
                Debug.LogError("StealthTemplateManager: MechanicsController is null");
                isValid = false;
            }

            if (_aiCoordinator == null)
            {
                Debug.LogError("StealthTemplateManager: AICoordinator is null");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 讀懷・繧､繝吶Φ繝医ワ繝ｳ繝峨Λ繝ｼ
        /// StealthDetectionEvent縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧・
        /// </summary>
        /// <param name="detectionEvent">讀懷・繧､繝吶Φ繝・/param>
        public void HandleDetectionEvent(StealthDetectionEvent detectionEvent)
        {
            if (detectionEvent == null)
            {
                Debug.LogWarning("StealthTemplateManager: HandleDetectionEvent called with null event");
                return;
            }

            Debug.Log($"StealthTemplateManager: Handling detection event - SuspicionLevel: {detectionEvent.SuspicionLevel}, Position: {detectionEvent.DetectionPosition}");

            // AI Coordinator・・・・・・ｴ・､孖ｸ ・・峡
            if (_aiCoordinator != null)
            {
                _aiCoordinator.OnDetectionEvent(detectionEvent);
            }

            // UI Manager・・・・・・ｴ・､孖ｸ ・・峡
            if (_uiManager != null)
            {
                _uiManager.OnDetectionLevelChanged(detectionEvent.SuspicionLevel);
            }

            // Audio Coordinator・・・・・・ｴ・､孖ｸ ・・峡
            if (_audioCoordinator != null)
            {
                // 逍大ｿ・Ξ繝吶Ν縺ｫ蝓ｺ縺･縺・※驕ｩ蛻・↑繝｡繧ｽ繝・ラ繧貞他縺ｳ蜃ｺ縺・
                if (detectionEvent.SuspicionLevel > 0.5f)
                {
                    _audioCoordinator.OnPlayerExposed();
                }
                else
                {
                    _audioCoordinator.OnPlayerConcealed(1.0f - detectionEvent.SuspicionLevel);
                }
            }
        }

        /// <summary>
        /// ・・・・ｴ・､孖ｸ ・､嵂・・ｨ・・﨑ｸ・､・ｬ
        /// StealthDetectionEvent縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧・
        /// </summary>
        /// <param name="detectionEvent">・・・・ｴ・､孖ｸ</param>
        public void HandleDetectionEventUndo(StealthDetectionEvent detectionEvent)
        {
            if (detectionEvent == null)
            {
                Debug.LogWarning("StealthTemplateManager: HandleDetectionEventUndo called with null event");
                return;
            }

            Debug.Log($"StealthTemplateManager: Undoing detection event - SuspicionLevel: {detectionEvent.SuspicionLevel}");

            // ・・・懍侃奛懍乱 ・､嵂・・ｨ・・奝ｵ・ - 髫阡ｽ迥ｶ諷句ｾｩ蜈・
            if (_uiManager != null)
            {
                // 讀懃衍繝ｬ繝吶Ν繧・縺ｫ謌ｻ縺励※UI譖ｴ譁ｰ
                _uiManager.OnDetectionLevelChanged(0.0f);
            }

            if (_audioCoordinator != null)
            {
                // 繝励Ξ繧､繝､繝ｼ繧帝國阡ｽ迥ｶ諷九↓謌ｻ縺・
                _audioCoordinator.OnPlayerConcealed(1.0f);
            }

            // AI Coordinator縺ｯ譌｢蟄倥Γ繧ｽ繝・ラ縺ｧ隴ｦ謌偵Ξ繝吶Ν繝ｪ繧ｻ繝・ヨ蜃ｦ逅・
            Debug.Log("Detection event undo completed - all systems restored to concealed state");
        }
        #endregion
    }

    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ險ｭ螳壼､画峩繧､繝吶Φ繝育畑繝・・繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public class StealthConfigurationEventData
    {
        public StealthTemplateConfig Config;
        public float Timestamp;
    }
}


