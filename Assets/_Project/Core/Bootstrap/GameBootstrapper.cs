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
    /// ゲーム起動時の初期化を拁E��するブートストラチE��ー
    /// ServiceLocatorへのサービス登録と初期化を行う
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
            // シングルトン管琁E
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
        /// ゲームサービスの初期匁E
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

            // ServiceLocatorのクリア�E�念のため�E�E
            ServiceLocator.Clear();

            // 吁E��ービスの登録と初期匁E
            RegisterCoreServices();
            RegisterFeatureServices();

            // GameEventBridgeの作�E
            if (_createGameEventBridge)
            {
                CreateGameEventBridge();
            }

            _isInitialized = true;

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Initialization complete");
        }

        /// <summary>
        /// コアサービスの登録
        /// </summary>
        private void RegisterCoreServices()
        {
            // EventManagerの登録
            if (_registerEventManager)
            {
                var eventManager = new EventManager();
                ServiceLocator.Register<IEventManager>(eventManager);

                if (_enableDebugLogs)
                    Debug.Log("[GameBootstrapper] Registered EventManager");
            }

            // 他�Eコアサービスもここに追加
            // 侁E AudioManager, InputManager, SaveManager など
        }

        /// <summary>
        /// フィーチャーサービスの登録
        /// </summary>
        private void RegisterFeatureServices()
        {
            // CombatServiceの登録
            var combatService = new CombatService();
            ServiceLocator.Register<ICombatService>(combatService);

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Registered CombatService");

            // GameManagerServiceの登録
            var gameManagerService = new GameManagerService();
            ServiceLocator.Register<IGameManager>(gameManagerService);

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Registered GameManagerService");

            // 封E��皁E��Feature層のサービスをここに追加
            // 侁E PlayerManager, AIManager, UIManager など
        }

        /// <summary>
        /// GameEventBridgeの作�E
        /// </summary>
        private void CreateGameEventBridge()
        {
            // GameEventBridgeコンポ�Eネントを追加
            var bridgeGO = new GameObject("GameEventBridge");
            bridgeGO.transform.SetParent(transform);
            var bridge = bridgeGO.AddComponent<GameEventBridge>();

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Created GameEventBridge");
        }

        /// <summary>
        /// クリーンアチE�E処琁E
        /// </summary>
        private void Cleanup()
        {
            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Cleaning up...");

            // ServiceLocatorのクリア
            ServiceLocator.Clear();

            _isInitialized = false;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// 初期化されてぁE��かチェチE��
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 手動初期化（忁E��に応じて�E�E
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
        /// サービスの取得（便利メソチE���E�E
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
        /// サービスの取得（安�E版！E
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

