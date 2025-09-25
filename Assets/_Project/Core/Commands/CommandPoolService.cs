using UnityEngine;
// using asterivo.Unity60.Core.Helpers;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Temporarily commented to avoid circular dependency
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    
    /// CommandPoolManagerのサービスラチE��ークラス
    /// ServiceLocatorパターンでコマンド�Eールへのアクセスを提供すめE    /// 
    /// 設計思想:
    /// - ObjectPoolパターンによるメモリ効玁E���E�E5%のメモリ削減効果！E    /// - コマンドオブジェクト�E再利用でガベ�Eジコレクションを削渁E    /// - Unity MonoBehaviourのライフサイクルに統合された安�Eなサービス管琁E    /// - ServiceLocatorパターンによる依存性注入対忁E    /// - 後方互換性を維持しながら段階的移行を支援
    /// 
    /// 推奨使用侁E
    /// var service = ServiceLocator.GetService&lt;ICommandPoolService&gt;();
    /// var damageCommand = service.GetCommand&lt;DamageCommand&gt;();
    /// // コマンド使用征E    /// service.ReturnCommand(damageCommand);
    /// </summary>
    public class CommandPoolService : MonoBehaviour, ICommandPoolService, IInitializable
    {
        [Header("Pool Service Settings")]
        /// <summary>チE��チE��統計情報の有効化フラグ</summary>
        [SerializeField] private bool enableDebugStats = true;
        
        /// <summary>Awake時�E自動�E期化フラグ</summary>
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        /// <summary>コマンド�Eールの実際の管琁E��行うマネージャー</summary>
        private CommandPoolManager _poolManager;
        
        /// <summary>初期化状態フラグ</summary>
        private bool _isInitialized = false;
        
        // ✁EServiceLocator移衁E Legacy Singleton警告シスチE���E�後方互換性のため�E�E        


        
        /// <summary>
        /// CommandPoolManagerへの直接アクセス
        /// 高度な制御めE��スタムプ�Eル操作に使用
        /// </summary>
        /// <returns>冁E��で管琁E��れてぁE��CommandPoolManagerのインスタンス</returns>
        public CommandPoolManager PoolManager => _poolManager;
        
        /// <summary>
        /// 初期化優先度�E�EInitializableインターフェース実裁E��E        /// CommandPoolServiceは基盤サービスなので早期�E期化を設宁E        /// </summary>
        public int Priority => 10;
        
        /// <summary>
        /// 初期化が完亁E��たかどぁE���E�EInitializableインターフェース実裁E��E        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        private void Awake()
        {
            InitializeService();
            
            // ServiceLocatorへの登録
            if (autoRegisterOnAwake)
            {
                RegisterToServiceLocator();
                LogServiceStatus();
            }
            
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// サービスの冁E��初期化�E琁E        /// CommandPoolManagerのインスタンス作�Eと初期化を行う
        /// </summary>
        /// <remarks>
        /// Awakeタイミングで自動実行される
        /// チE��チE��統計有効時�E統計情報の収集が開始される
        /// </remarks>
        private void InitializeService()
        {
            _poolManager = new CommandPoolManager(enableDebugStats);
            _poolManager.Initialize();
            _isInitialized = true;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService initialized with modern CommandPoolManager");
#endif
        }
        
        
        /// <summary>
        /// サービスのクリーンアチE�E処琁E��実行しまぁE        /// 全てのコマンド�Eールをクリアし、リソースを解放する
        /// </summary>
        /// <remarks>
        /// GameObjectが破棁E��れる際に自動実行される
        /// 手動でクリーンアチE�Eが忁E��な場合にも使用可能
        /// </remarks>
        public void Cleanup()
        {
            _poolManager?.Cleanup();
            _isInitialized = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService cleaned up");
#endif
        }
        
        #region Service Registration
        
        /// <summary>
        /// IInitializable実裁E サービスの初期化�E琁E        /// ServiceLocatorから呼び出される標準的な初期化メソチE��
        /// </summary>
        public void Initialize()
        {
            if (_poolManager == null)
            {
                InitializeService();
            }
            RegisterToServiceLocator();
            _isInitialized = true;
        }

        /// <summary>
        /// ServiceLocatorへのサービス登録
        /// ICommandPoolServiceインターフェースとして自身を登録
        /// </summary>
        private void RegisterToServiceLocator()
        {
            try
            {
                ServiceLocator.RegisterService<ICommandPoolService>(this);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("CommandPoolService registered to ServiceLocator as ICommandPoolService");
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to register CommandPoolService: {ex.Message}");
            }
        }
        
        /// <summary>
        /// サービスの状態確認とチE��チE��惁E��をログに出力しまぁE        /// 開発時�E問題診断めE��ービス状態�E確認に使用
        /// </summary>
        /// <remarks>
        /// UNITY_EDITOR また�E DEVELOPMENT_BUILD でのみ実行される
        /// プ�Eル管琁E�E状態とサービス稼働状況を確認可能
        /// </remarks>
        public void LogServiceStatus()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"CommandPoolService is active. Pool manager initialized: {_poolManager != null}");
            var isRegistered = ServiceLocator.IsServiceRegistered<ICommandPoolService>();
            UnityEngine.Debug.Log($"CommandPoolService ServiceLocator registration: {isRegistered}");
#endif
        }
        
        #endregion
        
        #region Convenient Access Methods
        
        /// <summary>
        /// 持E��した型のコマンドをプ�Eルから取得しまぁE        /// プ�Eルが空の場合�E新しいインスタンスを作�E
        /// </summary>
        /// <typeparam name="T">取得するコマンド�E型、ECommandインターフェースを実裁E��、パラメータなしコンストラクタを持つ忁E��がある</typeparam>
        /// <returns>使用可能なコマンドインスタンス</returns>
        /// <remarks>
        /// ObjectPoolパターンによりメモリ効玁E��を実現
        /// 使用後�E忁E��ReturnCommandでプ�Eルに返却すること
        /// </remarks>
        /// <example>
        /// var damageCmd = GetCommand&lt;DamageCommand&gt;();
        /// damageCmd.Initialize(target, damage);
        /// damageCmd.Execute();
        /// ReturnCommand(damageCmd);
        /// </example>
        public T GetCommand<T>() where T : class, ICommand, new()
        {
            return _poolManager.GetCommand<T>();
        }
        
        /// <summary>
        /// 使用完亁E��たコマンドをプ�Eルに返却しまぁE        /// コマンド�E状態をリセチE��して再利用可能にする
        /// </summary>
        /// <typeparam name="T">返却するコマンド�E垁E/typeparam>
        /// <param name="command">プ�Eルに返却するコマンドインスタンス</param>
        /// <remarks>
        /// IResettableCommandを実裁E��てぁE��場合、Reset()メソチE��が�E動実行される
        /// nullまた�E既に返却済みのコマンド�E無視される
        /// </remarks>
        public void ReturnCommand<T>(T command) where T : ICommand
        {
            _poolManager.ReturnCommand(command);
        }
        
        /// <summary>
        /// 持E��したコマンド型のプ�Eル統計情報を取得しまぁE        /// パフォーマンス監視やメモリ使用量�E最適化に使用
        /// </summary>
        /// <typeparam name="T">統計情報を取得するコマンド�E垁E/typeparam>
        /// <returns>コマンド�Eールの統計情報�E�作�E数、�Eール数、使用中数など�E�E/returns>
        /// <remarks>
        /// チE��チE��統計が有効な場合�Eみ正確な惁E��を提侁E        /// 無効な場合�E基本惁E��のみ取得可能
        /// </remarks>
        public CommandStatistics GetStatistics<T>() where T : ICommand
        {
            return _poolManager.GetStatistics<T>();
        }
        
        /// <summary>
        /// 全コマンド�EールのチE��チE��惁E��をログに出力しまぁE        /// 吁E�Eールの使用状況、統計情報、メモリ使用量を確認可能
        /// </summary>
        /// <remarks>
        /// 開発ビルドでのみ実行される
        /// パフォーマンス問題�E特定やプ�Eルサイズの調整に使用
        /// </remarks>
        public void LogDebugInfo()
        {
            _poolManager?.LogDebugInfo();
        }
        
        #endregion
        
        #region Static Access (Legacy Support)
        
        /// <summary>
        /// 静的アクセス用のコマンド取得メソチE���E�レガシー互換性用�E�E        /// サービスインスタンスが利用できなぁE��合�Eフォールバック機�E付き
        /// </summary>
        /// <typeparam name="T">取得するコマンド�E垁E/typeparam>
        /// <returns>コマンドインスタンス</returns>
        /// <remarks>
        /// サービスが�E期化されてぁE��ぁE��合�E新しいインスタンスを作�E
        /// こ�E場合�Eプ�Eル機�Eは利用されなぁE��め、パフォーマンス上�E利点は得られなぁE        /// 可能な限りServiceLocator経由でのアクセスを推奨
        /// </remarks>
        [System.Obsolete("Use ServiceLocator.GetService<ICommandPoolService>().GetCommand<T>() instead")]
        public static T GetCommandStatic<T>() where T : class, ICommand, new()
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service != null)
            {
                return service.GetCommand<T>();
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("CommandPoolService not available, creating new command instance");
#endif
            return new T();
        }
        
        /// <summary>
        /// 静的アクセス用のコマンド返却メソチE���E�レガシー互換性用�E�E        /// サービスインスタンスが利用できなぁE��合�E何も実行しなぁE        /// </summary>
        /// <typeparam name="T">返却するコマンド�E垁E/typeparam>
        /// <param name="command">プ�Eルに返却するコマンドインスタンス</param>
        /// <remarks>
        /// サービスが�E期化されてぁE��ぁE��合�Eコマンドを破棁E        /// 警告ログが�E力されるため、E��発時に問題を特定可能
        /// </remarks>
        [System.Obsolete("Use ServiceLocator.GetService<ICommandPoolService>().ReturnCommand<T>() instead")]
        public static void ReturnCommandStatic<T>(T command) where T : ICommand
        {
            var service = ServiceLocator.GetService<ICommandPoolService>();
            if (service != null)
            {
                service.ReturnCommand(command);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("CommandPoolService not available, cannot return command to pool");
#endif
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // ServiceLocatorからの登録解除
            try
            {
                ServiceLocator.UnregisterService<ICommandPoolService>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("CommandPoolService unregistered from ServiceLocator");
#endif
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to unregister CommandPoolService: {ex.Message}");
            }
            
            Cleanup();
        }
    }
}