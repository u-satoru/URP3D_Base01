using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Platformer.Services;
using asterivo.Unity60.Features.Templates.Platformer.Data;

namespace asterivo.Unity60.Features.Templates.Platformer.Core
{
    /// <summary>
    /// ServiceLocator基盤によるPlatformerテンプレートサービス初期化システム
    /// Learn & Grow価値実現：70%学習コスト削減（40時間→12時間）
    /// ServiceLocator + Event駆動ハイブリッドアーキテクチャの実装
    /// </summary>
    public class PlatformerServiceInitializer : MonoBehaviour
    {
        [Header("Platformer Template Configuration")]
        [SerializeField] private PlatformerConfigurationData _configurationData;

        [Header("Service Dependencies")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _enableDebugLogging = true;

        // ServiceLocator積極活用：全8サービスの中央管理
        private PlatformerGameManager _gameManager;
        private PlatformerPhysicsService _physicsService;
        private CollectionService _collectionService;
        private LevelGenerationService _levelGenerationService;
        private CheckpointService _checkpointService;
        // TODO: Implement remaining services
        private PlatformerAudioService _audioService;
        private PlatformerInputService _inputService;
        private PlatformerUIService _uiService;

        // Event駆動通信：サービス間疎結合
        [Header("Event Channels")]
        [SerializeField] private GameEvent _onServicesInitialized;
        [SerializeField] private GameEvent _onServicesShutdown;
        [SerializeField] private GameEvent<string> _onServiceRegistered;

        private bool _isInitialized = false;

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeServices();
            }
        }

        /// <summary>
        /// ServiceLocator + Event駆動アーキテクチャによる段階的サービス初期化
        /// 設計原則：依存関係順序での初期化保証
        /// </summary>
        public void InitializeServices()
        {
            if (_isInitialized)
            {
                LogDebug("Services already initialized. Skipping.");
                return;
            }

            LogDebug("Starting Platformer Services Initialization...");

            try
            {
                // Phase 1: Core Infrastructure Services（基盤サービス）
                InitializeCoreServices();

                // Phase 2: Gameplay Services（ゲームプレイサービス）
                InitializeGameplayServices();

                // Phase 3: UI & Interaction Services（UI・相互作用サービス）
                InitializeUIServices();

                // Phase 4: Event Notification（イベント通知）
                NotifyServicesInitialized();

                _isInitialized = true;
                LogDebug("All Platformer Services successfully initialized.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize Platformer Services: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Phase 1: 基盤サービス初期化（Physics, Audio, Input）
        /// ServiceLocator積極活用：依存関係のない基盤サービスから初期化
        /// </summary>
        private void InitializeCoreServices()
        {
            // Physics Service：物理演算・重力・跳躍基盤
            _physicsService = new PlatformerPhysicsService(_configurationData.PhysicsSettings);
            ServiceLocator.RegisterService<IPlatformerPhysicsService>(_physicsService);
            RegisterServiceEvent("PlatformerPhysicsService");

            // Audio Service：音響システム・効果音・BGM管理
            _audioService = new PlatformerAudioService(_configurationData.AudioSettings);
            ServiceLocator.RegisterService<IPlatformerAudioService>(_audioService);
            RegisterServiceEvent("PlatformerAudioService");

            // Input Service：入力処理・マッピング・コントローラー対応
            _inputService = new PlatformerInputService(_configurationData.InputSettings);
            ServiceLocator.RegisterService<IPlatformerInputService>(_inputService);
            RegisterServiceEvent("PlatformerInputService");

            LogDebug("Core Services (Physics, Audio, Input) initialized.");
        }

        /// <summary>
        /// Phase 2: ゲームプレイサービス初期化（GameManager, Collection, Level, Checkpoint）
        /// 依存関係：Phase 1サービス群に依存
        /// </summary>
        private void InitializeGameplayServices()
        {
            // Game Manager：中央ゲーム状態管理・プレイヤー制御
            _gameManager = new PlatformerGameManager(_configurationData.GameplaySettings);
            ServiceLocator.RegisterService<IPlatformerGameManager>(_gameManager);
            RegisterServiceEvent("PlatformerGameManager");

            // Collection Service：アイテム収集・スコア管理・進捗追跡
            _collectionService = new CollectionService(_configurationData.CollectionSettings);
            ServiceLocator.RegisterService<ICollectionService>(_collectionService);
            RegisterServiceEvent("CollectionService");

            // Level Generation Service：レベル生成・配置・動的調整
            _levelGenerationService = new LevelGenerationService(_configurationData.LevelSettings);
            ServiceLocator.RegisterService<ILevelGenerationService>(_levelGenerationService);
            RegisterServiceEvent("LevelGenerationService");

            // Checkpoint Service：セーブ・ロード・リスポーン管理
            _checkpointService = new CheckpointService(_configurationData.CheckpointSettings);
            ServiceLocator.RegisterService<ICheckpointService>(_checkpointService);
            RegisterServiceEvent("CheckpointService");

            LogDebug("Gameplay Services (GameManager, CollectionService, LevelGenerationService, CheckpointService) initialized. Audio, Input, UI services pending implementation.");
        }

        /// <summary>
        /// Phase 3: UI・相互作用サービス初期化（UI管理）
        /// 依存関係：全Phase 1-2サービス群に依存
        /// </summary>
        private void InitializeUIServices()
        {
            // UI Service：ユーザーインターフェース・メニュー・HUD管理
            _uiService = new PlatformerUIService(_configurationData.UISettings);
            ServiceLocator.RegisterService<IPlatformerUIService>(_uiService);
            RegisterServiceEvent("PlatformerUIService");

            LogDebug("UI Services (PlatformerUIService) initialized.");
        }

        /// <summary>
        /// ServiceLocator登録イベント通知：Event駆動アーキテクチャ統合
        /// </summary>
        private void RegisterServiceEvent(string serviceName)
        {
            _onServiceRegistered?.Raise(serviceName);
            LogDebug($"Service registered: {serviceName}");
        }

        /// <summary>
        /// サービス初期化完了通知：他システムへの統合通知
        /// </summary>
        private void NotifyServicesInitialized()
        {
            _onServicesInitialized?.Raise();
            LogDebug("Services initialization complete notification sent.");
        }

        /// <summary>
        /// サービス終了処理：ServiceLocator解除・リソース開放
        /// </summary>
        public void ShutdownServices()
        {
            if (!_isInitialized)
            {
                LogDebug("Services not initialized. Nothing to shutdown.");
                return;
            }

            LogDebug("Starting Platformer Services Shutdown...");

            try
            {
                // UI Services shutdown first（依存関係逆順）
                if (_uiService != null)
                {
                    ServiceLocator.UnregisterService<IPlatformerUIService>();
                    _uiService.Dispose();
                    _uiService = null;
                }

                // Gameplay Services shutdown
                ShutdownGameplayServices();

                // Core Services shutdown last
                ShutdownCoreServices();

                _onServicesShutdown?.Raise();
                _isInitialized = false;

                LogDebug("All Platformer Services successfully shutdown.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during services shutdown: {ex.Message}");
            }
        }

        private void ShutdownGameplayServices()
        {
            // Shutdown services in reverse order of initialization
            if (_checkpointService != null)
            {
                ServiceLocator.UnregisterService<ICheckpointService>();
                _checkpointService.Dispose();
                _checkpointService = null;
            }

            if (_levelGenerationService != null)
            {
                ServiceLocator.UnregisterService<ILevelGenerationService>();
                _levelGenerationService.Dispose();
                _levelGenerationService = null;
            }

            if (_collectionService != null)
            {
                ServiceLocator.UnregisterService<ICollectionService>();
                _collectionService.Dispose();
                _collectionService = null;
            }

            if (_gameManager != null)
            {
                ServiceLocator.UnregisterService<IPlatformerGameManager>();
                _gameManager.Dispose();
                _gameManager = null;
            }
        }

        private void ShutdownCoreServices()
        {
            // Input Service shutdown
            if (_inputService != null)
            {
                ServiceLocator.UnregisterService<IPlatformerInputService>();
                _inputService.Dispose();
                _inputService = null;
            }

            if (_audioService != null)
            {
                ServiceLocator.UnregisterService<IPlatformerAudioService>();
                _audioService.Dispose();
                _audioService = null;
            }

            if (_physicsService != null)
            {
                ServiceLocator.UnregisterService<IPlatformerPhysicsService>();
                _physicsService.Dispose();
                _physicsService = null;
            }
        }

        private void OnDestroy()
        {
            ShutdownServices();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_isInitialized && _gameManager != null)
            {
                if (pauseStatus)
                    _gameManager.PauseGame();
                else
                    _gameManager.ResumeGame();
            }
        }

        private void Update()
        {
            // Update input service for input buffering and frame input resets
            if (_isInitialized && _inputService != null)
            {
                _inputService.Update();
            }
        }

        /// <summary>
        /// デバッグログ出力：開発効率向上
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[PlatformerServiceInitializer] {message}");
            }
        }

        /// <summary>
        /// サービス初期化状態確認：外部システム連携用
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 特定サービス取得：ServiceLocator経由アクセスのヘルパー
        /// </summary>
        public T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// 設定データ更新：ランタイム設定変更対応
        /// </summary>
        public void UpdateConfiguration(PlatformerConfigurationData newConfig)
        {
            _configurationData = newConfig;

            if (_isInitialized)
            {
                LogDebug("Configuration updated. Reinitializing services...");
                ShutdownServices();
                InitializeServices();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用診断情報：開発支援機能
        /// </summary>
        [ContextMenu("Diagnostic Info")]
        private void ShowDiagnosticInfo()
        {
            Debug.Log("=== Platformer Service Diagnostic ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"Configuration: {(_configurationData != null ? "Valid" : "Missing")}");

            if (_isInitialized)
            {
                Debug.Log("Registered Services:");
                Debug.Log($"- GameManager: {(ServiceLocator.IsServiceRegistered<IPlatformerGameManager>() ? "✓" : "✗")}");
                Debug.Log($"- PhysicsService: {(ServiceLocator.IsServiceRegistered<IPlatformerPhysicsService>() ? "✓" : "✗")}");
                Debug.Log($"- CollectionService: {(ServiceLocator.IsServiceRegistered<ICollectionService>() ? "✓" : "✗")}");
                Debug.Log($"- LevelGenerationService: {(ServiceLocator.IsServiceRegistered<ILevelGenerationService>() ? "✓" : "✗")}");
                Debug.Log($"- CheckpointService: {(ServiceLocator.IsServiceRegistered<ICheckpointService>() ? "✓" : "✗")}");
                Debug.Log($"- AudioService: {(ServiceLocator.IsServiceRegistered<IPlatformerAudioService>() ? "✓" : "✗")}");
                Debug.Log($"- InputService: {(ServiceLocator.IsServiceRegistered<IPlatformerInputService>() ? "✓" : "✗")}");
                Debug.Log($"- UIService: {(ServiceLocator.IsServiceRegistered<IPlatformerUIService>() ? "✓" : "✗")}");
            }
        }
#endif
    }
}
