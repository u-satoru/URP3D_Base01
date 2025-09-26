using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Platformer.Services;
using asterivo.Unity60.Features.Templates.Platformer.Data;

namespace asterivo.Unity60.Features.Templates.Platformer.Core
{
    /// <summary>
    /// ServiceLocator蝓ｺ逶､縺ｫ繧医ｋPlatformer繝・Φ繝励Ξ繝ｼ繝医し繝ｼ繝薙せ蛻晄悄蛹悶す繧ｹ繝・Β
    /// Learn & Grow萓｡蛟､螳溽樟・・0%蟄ｦ鄙偵さ繧ｹ繝亥炎貂幢ｼ・0譎る俣竊・2譎る俣・・
    /// ServiceLocator + Event鬧・虚繝上う繝悶Μ繝・ラ繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｮ螳溯｣・
    /// </summary>
    public class PlatformerServiceInitializer : MonoBehaviour
    {
        [Header("Platformer Template Configuration")]
        [SerializeField] private PlatformerConfigurationData _configurationData;

        [Header("Service Dependencies")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _enableDebugLogging = true;

        // ServiceLocator遨肴･ｵ豢ｻ逕ｨ・壼・8繧ｵ繝ｼ繝薙せ縺ｮ荳ｭ螟ｮ邂｡逅・
        private PlatformerGameManager _gameManager;
        private PlatformerPhysicsService _physicsService;
        private CollectionService _collectionService;
        private LevelGenerationService _levelGenerationService;
        private CheckpointService _checkpointService;
        // TODO: Implement remaining services
        private PlatformerAudioService _audioService;
        private PlatformerInputService _inputService;
        private PlatformerUIService _uiService;

        // Event鬧・虚騾壻ｿ｡・壹し繝ｼ繝薙せ髢鍋鮪邨仙粋
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
        /// ServiceLocator + Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｫ繧医ｋ谿ｵ髫守噪繧ｵ繝ｼ繝薙せ蛻晄悄蛹・
        /// 險ｭ險亥次蜑・ｼ壻ｾ晏ｭ倬未菫る・ｺ上〒縺ｮ蛻晄悄蛹紋ｿ晁ｨｼ
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
                // Phase 1: Core Infrastructure Services・亥渕逶､繧ｵ繝ｼ繝薙せ・・
                InitializeCoreServices();

                // Phase 2: Gameplay Services・医ご繝ｼ繝繝励Ξ繧､繧ｵ繝ｼ繝薙せ・・
                InitializeGameplayServices();

                // Phase 3: UI & Interaction Services・・I繝ｻ逶ｸ莠剃ｽ懃畑繧ｵ繝ｼ繝薙せ・・
                InitializeUIServices();

                // Phase 4: Event Notification・医う繝吶Φ繝磯夂衍・・
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
        /// Phase 1: 蝓ｺ逶､繧ｵ繝ｼ繝薙せ蛻晄悄蛹厄ｼ・hysics, Audio, Input・・
        /// ServiceLocator遨肴･ｵ豢ｻ逕ｨ・壻ｾ晏ｭ倬未菫ゅ・縺ｪ縺・渕逶､繧ｵ繝ｼ繝薙せ縺九ｉ蛻晄悄蛹・
        /// </summary>
        private void InitializeCoreServices()
        {
            // Physics Service・夂黄逅・ｼ皮ｮ励・驥榊鴨繝ｻ霍ｳ霄榊渕逶､
            _physicsService = new PlatformerPhysicsService(_configurationData.PhysicsSettings);
            ServiceLocator.RegisterService<IPlatformerPhysicsService>(_physicsService);
            RegisterServiceEvent("PlatformerPhysicsService");

            // Audio Service・夐浹髻ｿ繧ｷ繧ｹ繝・Β繝ｻ蜉ｹ譫憺浹繝ｻBGM邂｡逅・
            _audioService = new PlatformerAudioService(_configurationData.AudioSettings);
            ServiceLocator.RegisterService<IPlatformerAudioService>(_audioService);
            RegisterServiceEvent("PlatformerAudioService");

            // Input Service・壼・蜉帛・逅・・繝槭ャ繝斐Φ繧ｰ繝ｻ繧ｳ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ蟇ｾ蠢・
            _inputService = new PlatformerInputService(_configurationData.InputSettings);
            ServiceLocator.RegisterService<IPlatformerInputService>(_inputService);
            RegisterServiceEvent("PlatformerInputService");

            LogDebug("Core Services (Physics, Audio, Input) initialized.");
        }

        /// <summary>
        /// Phase 2: 繧ｲ繝ｼ繝繝励Ξ繧､繧ｵ繝ｼ繝薙せ蛻晄悄蛹厄ｼ・ameManager, Collection, Level, Checkpoint・・
        /// 萓晏ｭ倬未菫ゑｼ啀hase 1繧ｵ繝ｼ繝薙せ鄒､縺ｫ萓晏ｭ・
        /// </summary>
        private void InitializeGameplayServices()
        {
            // Game Manager・壻ｸｭ螟ｮ繧ｲ繝ｼ繝迥ｶ諷狗ｮ｡逅・・繝励Ξ繧､繝､繝ｼ蛻ｶ蠕｡
            _gameManager = new PlatformerGameManager(_configurationData.GameplaySettings);
            ServiceLocator.RegisterService<IPlatformerGameManager>(_gameManager);
            RegisterServiceEvent("PlatformerGameManager");

            // Collection Service・壹い繧､繝・Β蜿朱寔繝ｻ繧ｹ繧ｳ繧｢邂｡逅・・騾ｲ謐苓ｿｽ霍｡
            _collectionService = new CollectionService(_configurationData.CollectionSettings);
            ServiceLocator.RegisterService<ICollectionService>(_collectionService);
            RegisterServiceEvent("CollectionService");

            // Level Generation Service・壹Ξ繝吶Ν逕滓・繝ｻ驟咲ｽｮ繝ｻ蜍慕噪隱ｿ謨ｴ
            _levelGenerationService = new LevelGenerationService(_configurationData.LevelSettings);
            ServiceLocator.RegisterService<ILevelGenerationService>(_levelGenerationService);
            RegisterServiceEvent("LevelGenerationService");

            // Checkpoint Service・壹そ繝ｼ繝悶・繝ｭ繝ｼ繝峨・繝ｪ繧ｹ繝昴・繝ｳ邂｡逅・
            _checkpointService = new CheckpointService(_configurationData.CheckpointSettings);
            ServiceLocator.RegisterService<ICheckpointService>(_checkpointService);
            RegisterServiceEvent("CheckpointService");

            LogDebug("Gameplay Services (GameManager, CollectionService, LevelGenerationService, CheckpointService) initialized. Audio, Input, UI services pending implementation.");
        }

        /// <summary>
        /// Phase 3: UI繝ｻ逶ｸ莠剃ｽ懃畑繧ｵ繝ｼ繝薙せ蛻晄悄蛹厄ｼ・I邂｡逅・ｼ・
        /// 萓晏ｭ倬未菫ゑｼ壼・Phase 1-2繧ｵ繝ｼ繝薙せ鄒､縺ｫ萓晏ｭ・
        /// </summary>
        private void InitializeUIServices()
        {
            // UI Service・壹Θ繝ｼ繧ｶ繝ｼ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繝ｻ繝｡繝九Η繝ｼ繝ｻHUD邂｡逅・
            _uiService = new PlatformerUIService(_configurationData.UISettings);
            ServiceLocator.RegisterService<IPlatformerUIService>(_uiService);
            RegisterServiceEvent("PlatformerUIService");

            LogDebug("UI Services (PlatformerUIService) initialized.");
        }

        /// <summary>
        /// ServiceLocator逋ｻ骭ｲ繧､繝吶Φ繝磯夂衍・哘vent鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε邨ｱ蜷・
        /// </summary>
        private void RegisterServiceEvent(string serviceName)
        {
            _onServiceRegistered?.Raise(serviceName);
            LogDebug($"Service registered: {serviceName}");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ蛻晄悄蛹門ｮ御ｺ・夂衍・壻ｻ悶す繧ｹ繝・Β縺ｸ縺ｮ邨ｱ蜷磯夂衍
        /// </summary>
        private void NotifyServicesInitialized()
        {
            _onServicesInitialized?.Raise();
            LogDebug("Services initialization complete notification sent.");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ邨ゆｺ・・逅・ｼ售erviceLocator隗｣髯､繝ｻ繝ｪ繧ｽ繝ｼ繧ｹ髢区叛
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
                // UI Services shutdown first・井ｾ晏ｭ倬未菫る・・ｼ・
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
        /// 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉幢ｼ夐幕逋ｺ蜉ｹ邇・髄荳・
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[PlatformerServiceInitializer] {message}");
            }
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ蛻晄悄蛹也憾諷狗｢ｺ隱搾ｼ壼､夜Κ繧ｷ繧ｹ繝・Β騾｣謳ｺ逕ｨ
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 迚ｹ螳壹し繝ｼ繝薙せ蜿門ｾ暦ｼ售erviceLocator邨檎罰繧｢繧ｯ繧ｻ繧ｹ縺ｮ繝倥Ν繝代・
        /// </summary>
        public T GetService<T>() where T : class
        {
            return ServiceLocator.GetService<T>();
        }

        /// <summary>
        /// 險ｭ螳壹ョ繝ｼ繧ｿ譖ｴ譁ｰ・壹Λ繝ｳ繧ｿ繧､繝險ｭ螳壼､画峩蟇ｾ蠢・
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
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ險ｺ譁ｭ諠・ｱ・夐幕逋ｺ謾ｯ謠ｴ讖溯・
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
                Debug.Log($"- GameManager: {(ServiceLocator.IsServiceRegistered<IPlatformerGameManager>() ? "笨・ : "笨・)}");
                Debug.Log($"- PhysicsService: {(ServiceLocator.IsServiceRegistered<IPlatformerPhysicsService>() ? "笨・ : "笨・)}");
                Debug.Log($"- CollectionService: {(ServiceLocator.IsServiceRegistered<ICollectionService>() ? "笨・ : "笨・)}");
                Debug.Log($"- LevelGenerationService: {(ServiceLocator.IsServiceRegistered<ILevelGenerationService>() ? "笨・ : "笨・)}");
                Debug.Log($"- CheckpointService: {(ServiceLocator.IsServiceRegistered<ICheckpointService>() ? "笨・ : "笨・)}");
                Debug.Log($"- AudioService: {(ServiceLocator.IsServiceRegistered<IPlatformerAudioService>() ? "笨・ : "笨・)}");
                Debug.Log($"- InputService: {(ServiceLocator.IsServiceRegistered<IPlatformerInputService>() ? "笨・ : "笨・)}");
                Debug.Log($"- UIService: {(ServiceLocator.IsServiceRegistered<IPlatformerUIService>() ? "笨・ : "笨・)}");
            }
        }
#endif
    }
}


