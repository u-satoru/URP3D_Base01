using UnityEngine;
using asterivo.Unity60.Core.Helpers;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Services; // Temporarily commented to avoid circular dependency
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    
    /// CommandPoolManagerのサービスラッパークラス
    /// ServiceLocatorパターンでコマンドプールへのアクセスを提供する
    /// 
    /// 設計思想:
    /// - ObjectPoolパターンによるメモリ効率化（95%のメモリ削減効果）
    /// - コマンドオブジェクトの再利用でガベージコレクションを削減
    /// - Unity MonoBehaviourのライフサイクルに統合された安全なサービス管理
    /// - ServiceLocatorパターンによる依存性注入対応
    /// - 後方互換性を維持しながら段階的移行を支援
    /// 
    /// 推奨使用例:
    /// var service = ServiceLocator.GetService&lt;ICommandPoolService&gt;();
    /// var damageCommand = service.GetCommand&lt;DamageCommand&gt;();
    /// // コマンド使用後
    /// service.ReturnCommand(damageCommand);
    /// </summary>
    public class CommandPoolService : MonoBehaviour, ICommandPoolService, IInitializable
    {
        [Header("Pool Service Settings")]
        /// <summary>デバッグ統計情報の有効化フラグ</summary>
        [SerializeField] private bool enableDebugStats = true;
        
        /// <summary>Awake時の自動初期化フラグ</summary>
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        /// <summary>コマンドプールの実際の管理を行うマネージャー</summary>
        private CommandPoolManager _poolManager;
        
        /// <summary>初期化状態フラグ</summary>
        private bool _isInitialized = false;
        
        // ✅ ServiceLocator移行: Legacy Singleton警告システム（後方互換性のため）
        


        
        /// <summary>
        /// CommandPoolManagerへの直接アクセス
        /// 高度な制御やカスタムプール操作に使用
        /// </summary>
        /// <returns>内部で管理されているCommandPoolManagerのインスタンス</returns>
        public CommandPoolManager PoolManager => _poolManager;
        
        /// <summary>
        /// 初期化優先度（IInitializableインターフェース実装）
        /// CommandPoolServiceは基盤サービスなので早期初期化を設定
        /// </summary>
        public int Priority => 10;
        
        /// <summary>
        /// 初期化が完了したかどうか（IInitializableインターフェース実装）
        /// </summary>
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
        /// サービスの内部初期化処理
        /// CommandPoolManagerのインスタンス作成と初期化を行う
        /// </summary>
        /// <remarks>
        /// Awakeタイミングで自動実行される
        /// デバッグ統計有効時は統計情報の収集が開始される
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
        /// サービスのクリーンアップ処理を実行します
        /// 全てのコマンドプールをクリアし、リソースを解放する
        /// </summary>
        /// <remarks>
        /// GameObjectが破棄される際に自動実行される
        /// 手動でクリーンアップが必要な場合にも使用可能
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
        /// IInitializable実装: サービスの初期化処理
        /// ServiceLocatorから呼び出される標準的な初期化メソッド
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
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                if (eventLogger != null) {
                    eventLogger.LogError($"Failed to register CommandPoolService: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// サービスの状態確認とデバッグ情報をログに出力します
        /// 開発時の問題診断やサービス状態の確認に使用
        /// </summary>
        /// <remarks>
        /// UNITY_EDITOR または DEVELOPMENT_BUILD でのみ実行される
        /// プール管理の状態とサービス稼働状況を確認可能
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
        /// 指定した型のコマンドをプールから取得します
        /// プールが空の場合は新しいインスタンスを作成
        /// </summary>
        /// <typeparam name="T">取得するコマンドの型。ICommandインターフェースを実装し、パラメータなしコンストラクタを持つ必要がある</typeparam>
        /// <returns>使用可能なコマンドインスタンス</returns>
        /// <remarks>
        /// ObjectPoolパターンによりメモリ効率化を実現
        /// 使用後は必ずReturnCommandでプールに返却すること
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
        /// 使用完了したコマンドをプールに返却します
        /// コマンドの状態をリセットして再利用可能にする
        /// </summary>
        /// <typeparam name="T">返却するコマンドの型</typeparam>
        /// <param name="command">プールに返却するコマンドインスタンス</param>
        /// <remarks>
        /// IResettableCommandを実装している場合、Reset()メソッドが自動実行される
        /// nullまたは既に返却済みのコマンドは無視される
        /// </remarks>
        public void ReturnCommand<T>(T command) where T : ICommand
        {
            _poolManager.ReturnCommand(command);
        }
        
        /// <summary>
        /// 指定したコマンド型のプール統計情報を取得します
        /// パフォーマンス監視やメモリ使用量の最適化に使用
        /// </summary>
        /// <typeparam name="T">統計情報を取得するコマンドの型</typeparam>
        /// <returns>コマンドプールの統計情報（作成数、プール数、使用中数など）</returns>
        /// <remarks>
        /// デバッグ統計が有効な場合のみ正確な情報を提供
        /// 無効な場合は基本情報のみ取得可能
        /// </remarks>
        public CommandStatistics GetStatistics<T>() where T : ICommand
        {
            return _poolManager.GetStatistics<T>();
        }
        
        /// <summary>
        /// 全コマンドプールのデバッグ情報をログに出力します
        /// 各プールの使用状況、統計情報、メモリ使用量を確認可能
        /// </summary>
        /// <remarks>
        /// 開発ビルドでのみ実行される
        /// パフォーマンス問題の特定やプールサイズの調整に使用
        /// </remarks>
        public void LogDebugInfo()
        {
            _poolManager?.LogDebugInfo();
        }
        
        #endregion
        
        #region Static Access (Legacy Support)
        
        /// <summary>
        /// 静的アクセス用のコマンド取得メソッド（レガシー互換性用）
        /// サービスインスタンスが利用できない場合のフォールバック機能付き
        /// </summary>
        /// <typeparam name="T">取得するコマンドの型</typeparam>
        /// <returns>コマンドインスタンス</returns>
        /// <remarks>
        /// サービスが初期化されていない場合は新しいインスタンスを作成
        /// この場合はプール機能は利用されないため、パフォーマンス上の利点は得られない
        /// 可能な限りServiceLocator経由でのアクセスを推奨
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
        /// 静的アクセス用のコマンド返却メソッド（レガシー互換性用）
        /// サービスインスタンスが利用できない場合は何も実行しない
        /// </summary>
        /// <typeparam name="T">返却するコマンドの型</typeparam>
        /// <param name="command">プールに返却するコマンドインスタンス</param>
        /// <remarks>
        /// サービスが初期化されていない場合はコマンドを破棄
        /// 警告ログが出力されるため、開発時に問題を特定可能
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
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError($"Failed to unregister CommandPoolService: {ex.Message}");
            }
            
            Cleanup();
        }
    }
}