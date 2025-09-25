using UnityEngine;
using asterivo.Unity60.Core.Services.Interfaces;
// using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Features.Combat.Services;
// using asterivo.Unity60.Features.Combat.Interfaces;
// using asterivo.Unity60.Features.GameManagement.Services;
// using asterivo.Unity60.Features.GameManagement.Interfaces;

namespace asterivo.Unity60.Core.Bootstrap
{
    /// <summary>
    /// 繧ｲ繝ｼ繝襍ｷ蜍墓凾縺ｮ蛻晄悄蛹悶ｒ諡・ｽ薙☆繧九ヶ繝ｼ繝医せ繝医Λ繝・ヱ繝ｼ
    /// ServiceLocator縺ｸ縺ｮ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ縺ｨ蛻晄悄蛹悶ｒ陦後≧
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Bootstrap Settings")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Header("Service Configuration")]
        [SerializeField] private bool _registerEventManager = true;
        [SerializeField] private bool _createGameEventBridge = true;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;

        private static GameBootstrapper _instance;
        private static bool _isInitialized = false;

        #region Unity Lifecycle

        private void Awake()
        {
            // 繧ｷ繝ｳ繧ｰ繝ｫ繝医Φ邂｡逅・
            if (_instance != null && _instance != this)
            {
                if (_enableDebugLogs)
                    Debug.LogWarning("[GameBootstrapper] Duplicate instance detected. Destroying...");
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_initializeOnAwake)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹・
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                if (_enableDebugLogs)
                    Debug.LogWarning("[GameBootstrapper] Already initialized");
                return;
            }

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Starting initialization...");

            // ServiceLocator縺ｮ繧ｯ繝ｪ繧｢・亥ｿｵ縺ｮ縺溘ａ・・
            ServiceLocator.Clear();

            // 蜷・し繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ縺ｨ蛻晄悄蛹・
            RegisterCoreServices();
            RegisterFeatureServices();

            // GameEventBridge縺ｮ菴懈・
            if (_createGameEventBridge)
            {
                CreateGameEventBridge();
            }

            _isInitialized = true;

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Initialization complete");
        }

        /// <summary>
        /// 繧ｳ繧｢繧ｵ繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ
        /// </summary>
        private void RegisterCoreServices()
        {
            // EventManager縺ｮ逋ｻ骭ｲ
            if (_registerEventManager)
            {
                var eventManager = new EventManager();
                ServiceLocator.Register<IEventManager>(eventManager);

                if (_enableDebugLogs)
                    Debug.Log("[GameBootstrapper] Registered EventManager");
            }

            // 莉悶・繧ｳ繧｢繧ｵ繝ｼ繝薙せ繧ゅ％縺薙↓霑ｽ蜉
            // 萓・ AudioManager, InputManager, SaveManager 縺ｪ縺ｩ
        }

        /// <summary>
        /// 繝輔ぅ繝ｼ繝√Ε繝ｼ繧ｵ繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ
        /// </summary>
        private void RegisterFeatureServices()
        {
            // CombatService縺ｮ逋ｻ骭ｲ
            var combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Registered CombatService");

            // GameManagerService縺ｮ逋ｻ骭ｲ
            var gameManagerService = new GameManagerService();
            ServiceLocator.Register<IGameManager>(gameManagerService);

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Registered GameManagerService");

            // 蟆・擂逧・↓Feature螻､縺ｮ繧ｵ繝ｼ繝薙せ繧偵％縺薙↓霑ｽ蜉
            // 萓・ PlayerManager, AIManager, UIManager 縺ｪ縺ｩ
        }

        /// <summary>
        /// GameEventBridge縺ｮ菴懈・
        /// </summary>
        private void CreateGameEventBridge()
        {
            // GameEventBridge繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ霑ｽ蜉
            var bridgeGO = new GameObject("GameEventBridge");
            bridgeGO.transform.SetParent(transform);
            var bridge = bridgeGO.AddComponent<GameEventBridge>();

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Created GameEventBridge");
        }

        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蜃ｦ逅・
        /// </summary>
        private void Cleanup()
        {
            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Cleaning up...");

            // ServiceLocator縺ｮ繧ｯ繝ｪ繧｢
            ServiceLocator.Clear();

            _isInitialized = false;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// 蛻晄悄蛹悶＆繧後※縺・ｋ縺九メ繧ｧ繝・け
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 謇句虚蛻晄悄蛹厄ｼ亥ｿ・ｦ√↓蠢懊§縺ｦ・・
        /// </summary>
        public static void InitializeManually()
        {
            if (_instance != null)
            {
                _instance.Initialize();
            }
            else
            {
                Debug.LogError("[GameBootstrapper] No instance found. Please add GameBootstrapper to the scene.");
            }
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蜿門ｾ暦ｼ井ｾｿ蛻ｩ繝｡繧ｽ繝・ラ・・
        /// </summary>
        public static T GetService<T>() where T : IService
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[GameBootstrapper] Not initialized. Service {typeof(T).Name} may not be available.");
            }

            return ServiceLocator.Get<T>();
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蜿門ｾ暦ｼ亥ｮ牙・迚茨ｼ・
        /// </summary>
        public static bool TryGetService<T>(out T service) where T : IService
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[GameBootstrapper] Not initialized. Service {typeof(T).Name} may not be available.");
            }

            return ServiceLocator.TryGet(out service);
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Game/Initialize Services")]
        private static void InitializeFromMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Services can only be initialized in Play Mode");
                return;
            }

            InitializeManually();
        }

        [UnityEditor.MenuItem("Tools/Game/Clear Services")]
        private static void ClearServicesFromMenu()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Services can only be cleared in Play Mode");
                return;
            }

            ServiceLocator.Clear();
            Debug.Log("All services cleared");
        }
#endif

        #endregion
    }
}

