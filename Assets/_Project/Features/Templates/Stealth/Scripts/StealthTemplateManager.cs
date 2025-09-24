using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
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
            // パフォーマンス監視（最適化のため低頻度実行）
            // TODO: Implement performance monitoring when StealthPerformanceMonitor is available
            // if (Time.frameCount % 60 == 0) // 60フレームごと
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

            // 既存オーディオシステム統合
            _audioCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
            if (_audioCoordinator == null)
            {
                Debug.LogWarning("StealthTemplateManager: StealthAudioCoordinator not found. Audio integration may not work properly.");
            }

            // ServiceLocator統合: StealthMechanics → IStealthMechanicsService登録
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
        /// ServiceLocator統合: StealthMechanics → IStealthMechanicsService登録
        /// Phase 1: Core Service Integration の核心実装
        /// Learn & Grow価値実現のための統一API提供
        /// </summary>
        private void RegisterStealthMechanicsService()
        {
            // StealthMechanicsコンポーネントを検索
            var stealthMechanics = FindFirstObjectByType<StealthMechanics>();

            if (stealthMechanics == null)
            {
                Debug.LogWarning("StealthTemplateManager: StealthMechanics component not found. ServiceLocator registration skipped.");
                return;
            }

            // ServiceLocatorにStealthMechanicsとして登録
            try
            {
                ServiceLocator.RegisterService<StealthMechanics>(stealthMechanics);

                // サービス登録通知
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
                // TODO: Implement StealthTutorialSystem.ApplyConfiguration when system is ready
                // _tutorialSystem.ApplyConfiguration(_config.TutorialConfig);
                Debug.Log("StealthTemplateManager: Tutorial system configuration would be applied here");
            }

            // Progression Tracker（Learn & Grow価値実現）
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
        /// 検出イベントハンドラー
        /// StealthDetectionEventから呼び出される
        /// </summary>
        /// <param name="detectionEvent">検出イベント</param>
        public void HandleDetectionEvent(StealthDetectionEvent detectionEvent)
        {
            if (detectionEvent == null)
            {
                Debug.LogWarning("StealthTemplateManager: HandleDetectionEvent called with null event");
                return;
            }

            Debug.Log($"StealthTemplateManager: Handling detection event - SuspicionLevel: {detectionEvent.SuspicionLevel}, Position: {detectionEvent.DetectionPosition}");

            // AI Coordinator에 검출 이벤트 전달
            if (_aiCoordinator != null)
            {
                _aiCoordinator.OnDetectionEvent(detectionEvent);
            }

            // UI Manager에 검출 이벤트 전달
            if (_uiManager != null)
            {
                _uiManager.OnDetectionLevelChanged(detectionEvent.SuspicionLevel);
            }

            // Audio Coordinator에 검출 이벤트 전달
            if (_audioCoordinator != null)
            {
                // 疑心レベルに基づいて適切なメソッドを呼び出し
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
        /// 검출 이벤트 실행 취소 핸들러
        /// StealthDetectionEventから呼び出される
        /// </summary>
        /// <param name="detectionEvent">검출 이벤트</param>
        public void HandleDetectionEventUndo(StealthDetectionEvent detectionEvent)
        {
            if (detectionEvent == null)
            {
                Debug.LogWarning("StealthTemplateManager: HandleDetectionEventUndo called with null event");
                return;
            }

            Debug.Log($"StealthTemplateManager: Undoing detection event - SuspicionLevel: {detectionEvent.SuspicionLevel}");

            // 각 시스템에 실행 취소 통지 - 隠蔽状態復元
            if (_uiManager != null)
            {
                // 検知レベルを0に戻してUI更新
                _uiManager.OnDetectionLevelChanged(0.0f);
            }

            if (_audioCoordinator != null)
            {
                // プレイヤーを隠蔽状態に戻す
                _audioCoordinator.OnPlayerConcealed(1.0f);
            }

            // AI Coordinatorは既存メソッドで警戒レベルリセット処理
            Debug.Log("Detection event undo completed - all systems restored to concealed state");
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