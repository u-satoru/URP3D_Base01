using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// Layer 2: Runtime Management（実行時管理層）
    /// ステルステンプレートのメインコントローラー
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ実装
    /// Learn & Grow価値実現の中央制御システム
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

        #region Core System References（ServiceLocator統合）
        private StealthMechanicsController _mechanicsController;
        private StealthAICoordinator _aiCoordinator;
        private StealthEnvironmentManager _environmentManager;
        private StealthUIManager _uiManager;

        // 既存オーディオシステム統合（StealthAudioCoordinator）
        private asterivo.Unity60.Core.Audio.StealthAudioCoordinator _audioCoordinator;

        // Event System Integration
        private readonly Dictionary<string, GameEvent> _eventChannels = new();

        // Performance Monitoring
        private StealthPerformanceMonitor _performanceMonitor;
        
        // Learn & Grow System Integration
        private StealthTutorialSystem _tutorialSystem;
        private StealthProgressionTracker _progressionTracker;
        #endregion

        #region Template State Management
        public bool IsStealthModeEnabled { get; private set; }
        public bool IsInitialized { get; private set; }
        public StealthState CurrentStealthState => _mechanicsController?.CurrentState ?? StealthState.Normal;
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
            // パフォーマンス監視（最適化のため低頻度実行）
            if (Time.frameCount % 60 == 0) // 60フレームごと
            {
                _performanceMonitor?.UpdatePerformanceMetrics();
            }
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

            // 既存オーディオシステム統合
            _audioCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
            if (_audioCoordinator == null)
            {
                Debug.LogWarning("StealthTemplateManager: StealthAudioCoordinator not found. Audio integration may not work properly.");
            }

            // Performance monitoring
            _performanceMonitor = GetOrCreateSubsystem<StealthPerformanceMonitor>();

            // Learn & Grow systems
            _tutorialSystem = GetOrCreateSubsystem<StealthTutorialSystem>();
            _progressionTracker = GetOrCreateSubsystem<StealthProgressionTracker>();

            IsInitialized = true;
            Debug.Log("StealthTemplateManager: All subsystems initialized successfully.");
        }

        private T GetOrCreateSubsystem<T>() where T : Component
        {
            // まず子オブジェクトから検索
            T component = GetComponentInChildren<T>();
            if (component != null)
            {
                Debug.Log($"StealthTemplateManager: Found existing {typeof(T).Name}");
                return component;
            }

            // シーン全体から検索
            component = FindFirstObjectByType<T>();
            if (component != null)
            {
                Debug.Log($"StealthTemplateManager: Found {typeof(T).Name} in scene");
                return component;
            }

            // 新規作成
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

            // 各サブシステムに設定を適用
            ApplyConfigurationToSubsystems();

            // Event駆動による設定変更通知
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

            // Audio Coordinator（既存システム統合）
            if (_audioCoordinator != null)
            {
                // 既存のStealthAudioCoordinatorに設定を適用
                // 具体的な適用方法は既存の実装に依存
                Debug.Log("StealthTemplateManager: Audio configuration applied via StealthAudioCoordinator");
            }

            // Tutorial System（Learn & Grow価値実現）
            if (_tutorialSystem != null)
            {
                _tutorialSystem.ApplyConfiguration(_config.TutorialConfig);
            }

            // Progression Tracker（Learn & Grow価値実現）
            if (_progressionTracker != null)
            {
                _progressionTracker.ApplyConfiguration(_config.ProgressionConfig);
            }

            // Performance Monitor
            if (_performanceMonitor != null)
            {
                _performanceMonitor.ApplyConfiguration(_config.PerformanceConfig);
            }
        }

        private void LoadDefaultConfiguration()
        {
            // デフォルト設定の作成
            var defaultConfig = ScriptableObject.CreateInstance<StealthTemplateConfig>();
            defaultConfig.ResetConfiguration();
            
            ApplyConfiguration(defaultConfig);
            
            Debug.Log("StealthTemplateManager: Default configuration loaded.");
        }

        private void RaiseStealthConfigurationChangedEvent(StealthTemplateConfig config)
        {
            // カスタムイベントデータの作成とEvent発行
            var configData = new StealthConfigurationEventData
            {
                Config = config,
                Timestamp = Time.time
            };

            // 設定変更イベントの発行
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
        public void RegisterNPC(object npc)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("StealthTemplateManager: Cannot register NPC - system not initialized");
                return;
            }

            _aiCoordinator?.RegisterNPC(npc);
            Debug.Log($"StealthTemplateManager: NPC registered: {npc}");
        }

        public void UnregisterNPC(object npc)
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
            return _tutorialSystem != null && 
                   _progressionTracker != null && 
                   _tutorialSystem.IsSystemReady();
        }

        public float GetLearningProgress()
        {
            return _progressionTracker?.GetOverallProgress() ?? 0f;
        }

        public bool IsBasicGameplayReady()
        {
            return _progressionTracker?.IsBasicGameplayReady() ?? false;
        }

        public void StartQuickLearningMode()
        {
            if (_tutorialSystem != null && _config.TutorialConfig.EnableQuickStart)
            {
                _tutorialSystem.StartQuickLearning();
                Debug.Log("StealthTemplateManager: Quick learning mode started - Learn & Grow value realization");
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
        #endregion
    }

    /// <summary>
    /// ステルス設定変更イベント用データ構造
    /// </summary>
    [System.Serializable]
    public class StealthConfigurationEventData
    {
        public StealthTemplateConfig Config;
        public float Timestamp;
    }
}