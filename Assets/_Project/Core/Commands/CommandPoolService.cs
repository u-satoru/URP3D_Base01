using UnityEngine;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// CommandPoolManagerのサービスラッパー
    /// Singletonパターンでアクセス可能
    /// </summary>
    public class CommandPoolService : MonoBehaviour
    {
        [Header("Pool Service Settings")]
        [SerializeField] private bool enableDebugStats = true;
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        private CommandPoolManager _poolManager;
        private static CommandPoolService _instance;
        
        /// <summary>
        /// シングルトンインスタンス（レガシー互換性用）
        /// </summary>
        public static CommandPoolService Instance => _instance;
        
        /// <summary>
        /// CommandPoolManagerへの直接アクセス
        /// </summary>
        public CommandPoolManager PoolManager => _poolManager;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializeService();
                
                if (autoRegisterOnAwake)
                {
                    LogServiceStatus();
                }
                
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeService()
        {
            _poolManager = new CommandPoolManager(enableDebugStats);
            _poolManager.Initialize();
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService initialized with modern CommandPoolManager");
#endif
        }
        
        /// <summary>
        /// サービスを手動で初期化します
        /// </summary>
        public void Initialize()
        {
            // 既にAwakeで初期化済み
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService.Initialize() called");
#endif
        }
        
        /// <summary>
        /// サービスをクリーンアップします
        /// </summary>
        public void Cleanup()
        {
            _poolManager?.Cleanup();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("CommandPoolService cleaned up");
#endif
        }
        
        #region Service Registration
        
        /// <summary>
        /// サービスの状態確認用（デバッグ目的）
        /// </summary>
        public void LogServiceStatus()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"CommandPoolService is active. Pool manager initialized: {_poolManager != null}");
#endif
        }
        
        #endregion
        
        #region Convenient Access Methods
        
        /// <summary>
        /// コマンドを取得します（便利メソッド）
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <returns>コマンドインスタンス</returns>
        public T GetCommand<T>() where T : class, ICommand, new()
        {
            return _poolManager.GetCommand<T>();
        }
        
        /// <summary>
        /// コマンドを返却します（便利メソッド）
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <param name="command">返却するコマンド</param>
        public void ReturnCommand<T>(T command) where T : ICommand
        {
            _poolManager.ReturnCommand(command);
        }
        
        /// <summary>
        /// 統計情報を取得します（便利メソッド）
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <returns>統計情報</returns>
        public CommandStatistics GetStatistics<T>() where T : ICommand
        {
            return _poolManager.GetStatistics<T>();
        }
        
        /// <summary>
        /// デバッグ情報をログに出力します
        /// </summary>
        public void LogDebugInfo()
        {
            _poolManager?.LogDebugInfo();
        }
        
        #endregion
        
        #region Static Access (Legacy Support)
        
        /// <summary>
        /// 静的アクセス用メソッド（レガシー互換性）
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <returns>コマンドインスタンス</returns>
        public static T GetCommandStatic<T>() where T : class, ICommand, new()
        {
            if (_instance != null)
            {
                return _instance.GetCommand<T>();
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("CommandPoolService not available, creating new command instance");
#endif
            return new T();
        }
        
        /// <summary>
        /// 静的アクセス用メソッド（レガシー互換性）
        /// </summary>
        /// <typeparam name="T">コマンドの型</typeparam>
        /// <param name="command">返却するコマンド</param>
        public static void ReturnCommandStatic<T>(T command) where T : ICommand
        {
            if (_instance != null)
            {
                _instance.ReturnCommand(command);
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
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
            }
        }
    }
}