using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
// using asterivo.Unity60.Features.Combat.Services;
// using asterivo.Unity60.Features.Combat.Interfaces;
// using asterivo.Unity60.Features.GameManagement.Services;
// using asterivo.Unity60.Features.GameManagement.Interfaces;

namespace asterivo.Unity60.Core.Bootstrap
{
    /// <summary>
    /// ゲーム起動時の初期化を担当するブートストラッパー
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
            // シングルトン管理
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
        /// ゲームサービスの初期化
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

            // ServiceLocatorのクリア（念のため）
            ServiceLocator.Clear();

            // 各サービスの登録と初期化
            RegisterCoreServices();
            RegisterFeatureServices();

            // GameEventBridgeの作成
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
                // TODO: EventManager参照エラーを修正
                // var eventManager = new EventManager();
                // ServiceLocator.Register<IEventManager>(eventManager);

                if (_enableDebugLogs)
                    Debug.Log("[GameBootstrapper] EventManager registration skipped (TODO)");
            }

            // 他のコアサービスもここに追加
            // 例: AudioManager, InputManager, SaveManager など
        }

        /// <summary>
        /// フィーチャーサービスの登録
        /// </summary>
        private void RegisterFeatureServices()
        {
            // TODO: Feature層のサービス登録は後で実装
            // Core層からFeature層への参照は禁止されているため
            // これらのサービスは別の場所で登録する必要がある

            // // CombatServiceの登録
            // var combatService = new CombatService();
            // ServiceLocator.Register<ICombatService>(combatService);
            //
            // if (_enableDebugLogs)
            //     Debug.Log("[GameBootstrapper] Registered CombatService");
            //
            // // GameManagerServiceの登録
            // var gameManagerService = new GameManagerService();
            // ServiceLocator.Register<IGameManager>(gameManagerService);
            //
            // if (_enableDebugLogs)
            //     Debug.Log("[GameBootstrapper] Registered GameManagerService");

            // 将来的にFeature層のサービスをここに追加
            // 例: PlayerManager, AIManager, UIManager など
        }

        /// <summary>
        /// GameEventBridgeの作成
        /// </summary>
        private void CreateGameEventBridge()
        {
            // GameEventBridgeコンポーネントを追加
            var bridgeGO = new GameObject("GameEventBridge");
            bridgeGO.transform.SetParent(transform);
            var bridge = bridgeGO.AddComponent<GameEventBridge>();

            if (_enableDebugLogs)
                Debug.Log("[GameBootstrapper] Created GameEventBridge");
        }

        /// <summary>
        /// クリーンアップ処理
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
        /// 初期化されているかチェック
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 手動初期化（必要に応じて）
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
        /// サービスの取得（便利メソッド）
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
        /// サービスの取得（安全版）
        /// </summary>
        public static bool TryGetService<T>(out T service) where T : IService
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[GameBootstrapper] Not initialized. Service {typeof(T).Name} may not be available.");
            }

            return ServiceLocator.TryGet<T>(out service);
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