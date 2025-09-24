using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Patterns.ObjectPool;

using asterivo.Unity60.Core.Patterns.Registry;
using Debug = UnityEngine.Debug;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// 新しいコマンドプールマネージャー
    /// Factory + Registry + ObjectPool パターンの統合実装
    /// </summary>
    public class CommandPoolManager
    {
        private readonly ITypeRegistry<IObjectPool<ICommand>> _pools;
        private readonly Dictionary<Type, Func<ICommand>> _factories;
        private readonly Dictionary<Type, PoolConfiguration> _configurations;
        
        // 統計情報とデバッグ
        private readonly Dictionary<Type, CommandStatistics> _statistics;
        private bool _isDebugEnabled;
        
        public CommandPoolManager(bool enableDebug = false)
        {
            _pools = new TypeRegistry<IObjectPool<ICommand>>();
            _factories = new Dictionary<Type, Func<ICommand>>();
            _configurations = new Dictionary<Type, PoolConfiguration>();
            _statistics = new Dictionary<Type, CommandStatistics>();
            _isDebugEnabled = enableDebug;
        }
        
        #region IComponentLifecycle Implementation
        
        public void Initialize()
        {
            // デフォルトファクトリとプールを登録
            RegisterDefaultFactories();
            RegisterDefaultPools();
            PrewarmPools();
            
            if (_isDebugEnabled)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("CommandPoolManager initialized with modern design patterns");
#endif
            }
        }
        
        public void Cleanup()
        {
            ClearAllPools();
            _pools.Clear();
            _factories.Clear();
            _configurations.Clear();
            _statistics.Clear();
        }
        
        #endregion
        
        #region Factory Registration
        
        /// <summary>
        /// コマンドファクトリを登録します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <param name="factory">ファクトリ実装</param>
        /// <summary>
        /// カスタムファクトリ関数を登録します。
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <param name="factoryFunc">ファクトリ関数</param>
        public void RegisterFactory<TCommand>(Func<ICommand> factoryFunc) where TCommand : ICommand
        {
            _factories[typeof(TCommand)] = factoryFunc;
        }
        

        
        #endregion
        
        #region Pool Management
        
        /// <summary>
        /// プール設定を登録します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <param name="config">プール設定</param>
        public void RegisterPoolConfiguration<TCommand>(PoolConfiguration config) where TCommand : class, ICommand, new()
        {
            _configurations[typeof(TCommand)] = config;
            
            // プールが既に存在する場合は再作成
            if (_pools.IsRegistered<TCommand>())
            {
                CreatePoolForType<TCommand>();
            }
        }
        
        /// <summary>
        /// 指定した型のプールを作成します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        private void CreatePoolForType<TCommand>() where TCommand : class, ICommand, new()
        {
            var commandType = typeof(TCommand);
            
            // ファクトリから取得または直接作成
            Func<ICommand> factoryFunc;
            if (_factories.TryGetValue(typeof(TCommand), out var factory))
            {
                factoryFunc = factory;
            }
            else
            {
                // 直接new()を使ってインスタンス生成
                factoryFunc = () => new TCommand();
            }
            
            var config = _configurations.ContainsKey(commandType) 
                ? _configurations[commandType] 
                : PoolConfiguration.Default;
            
            // コールバックを設定
            Action<ICommand> onReturn = (cmd) => {
                if (cmd is IResettableCommand resettable)
                {
                    resettable.Reset();
                }
            };
            
            var pool = new GenericObjectPool<ICommand>(
                factoryFunc, 
                null, // onGet
                onReturn, // onReturn
                config.MaxSize
            );
            _pools.Register(commandType, pool);
        }
        
        #endregion
        
        #region Command Operations
        
        /// <summary>
        /// コマンドを取得します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <returns>コマンドインスタンス</returns>
        public TCommand GetCommand<TCommand>() where TCommand : class, ICommand, new()
        {
            var commandType = typeof(TCommand);
            
            // ファクトリから取得または直接作成
            Func<ICommand> factoryFunc;
            if (_factories.TryGetValue(typeof(TCommand), out var factory))
            {
                factoryFunc = factory;
            }
            else
            {
                // 直接new()を使ってインスタンス生成
                factoryFunc = () => new TCommand();
            }
            
            var config = _configurations.ContainsKey(commandType) 
                ? _configurations[commandType] 
                : PoolConfiguration.Default;
            
            // プールが存在するかチェックし、なければ作成
            if (!_pools.TryGet(commandType, out var pool))
            {
                // コールバックを設定
                Action<ICommand> onReturn = (cmd) => {
                    if (cmd is IResettableCommand resettable)
                    {
                        resettable.Reset();
                    }
                };
                
                pool = new GenericObjectPool<ICommand>(
                    factoryFunc, 
                    null, // onGet
                    onReturn, // onReturn
                    config.MaxSize
                );
                _pools.Register(commandType, pool);
            }
            
            var command = pool.Get();
            
            // 統計更新
            if (!_statistics.TryGetValue(commandType, out var stats))
            {
                stats = new CommandStatistics();
                _statistics[commandType] = stats;
            }
            stats.TotalGets++;
            
            return (TCommand)command;
        }
        
        /// <summary>
        /// コマンドを返却します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <param name="command">返却するコマンド</param>
        public void ReturnCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (command == null) return;
            
            if (_pools.TryGet<TCommand>(out var pool))
            {
                pool.Return(command);
                UpdateStatistics<TCommand>(false);
            }
        }
        
        /// <summary>
        /// 型を指定してコマンドを返却します
        /// </summary>
        /// <param name="command">返却するコマンド</param>
        public void ReturnCommand(ICommand command)
        {
            if (command == null) return;
            
            var commandType = command.GetType();
            if (_pools.TryGet(commandType, out var pool))
            {
                pool.Return(command);
            }
        }
        
        #endregion
        
        #region Statistics and Debug
        
        /// <summary>
        /// 指定した型の統計情報を取得します
        /// </summary>
        /// <typeparam name="TCommand">コマンドの型</typeparam>
        /// <returns>統計情報</returns>
        public CommandStatistics GetStatistics<TCommand>() where TCommand : ICommand
        {
            return _statistics.ContainsKey(typeof(TCommand)) 
                ? _statistics[typeof(TCommand)] 
                : new CommandStatistics();
        }
        
        /// <summary>
        /// 全ての統計情報を取得します
        /// </summary>
        /// <returns>型ごとの統計情報</returns>
        public Dictionary<Type, CommandStatistics> GetAllStatistics()
        {
            return new Dictionary<Type, CommandStatistics>(_statistics);
        }
        
        /// <summary>
        /// デバッグ情報をログに出力します
        /// </summary>
        public void LogDebugInfo()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("=== CommandPoolManager Statistics ===");
            foreach (var kvp in _statistics)
            {
                var type = kvp.Key;
                var stats = kvp.Value;
                var poolStats = _pools.TryGet(type, out var pool) ? pool.GetStatistics() : default;
                
                UnityEngine.Debug.Log($"{type.Name}:\n  Gets: {stats.TotalGets}, Returns: {stats.TotalReturns}\n" +
                         $"  Pool: {poolStats.CurrentInPool}/{poolStats.TotalCreated} (Reuse: {poolStats.ReuseRatio:P1})");
            }
#endif
        }
        
        #endregion
        
        #region Private Methods
        
        private void RegisterDefaultFactories()
        {
            // よく使用されるコマンドのデフォルトファクトリを登録
            RegisterFactory<DamageCommand>(() => new DamageCommand());
            RegisterFactory<HealCommand>(() => new HealCommand());
        }
        
        private void RegisterDefaultPools()
        {
            // デフォルト設定でプールを作成
            RegisterPoolConfiguration<DamageCommand>(new PoolConfiguration
            {
                MaxSize = 50,
                PrewarmCount = 10
            });
            
            RegisterPoolConfiguration<HealCommand>(new PoolConfiguration
            {
                MaxSize = 30,
                PrewarmCount = 5
            });
        }
        
        private void PrewarmPools()
        {
            foreach (var type in _configurations.Keys)
            {
                // リフレクションを使ってCreatePoolForTypeを呼び出し
                var method = GetType().GetMethod(nameof(CreatePoolForType), 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }
        }
        
        private void ClearAllPools()
        {
            foreach (var pool in _pools.GetValues())
            {
                pool.Clear();
            }
        }
        
        private void OnCommandGet(ICommand command)
        {
            // コマンド取得時の処理（必要に応じてリセットなど）
            if (command is IResettableCommand resettable)
            {
                // 取得時にはリセットしない（Initialize で設定）
            }
        }
        
        private void OnCommandReturn(ICommand command)
        {
            // コマンド返却時の処理（リセット）
            if (command is IResettableCommand resettable)
            {
                resettable.Reset();
            }
        }
        
        private void UpdateStatistics<TCommand>(bool isGet) where TCommand : ICommand
        {
            var commandType = typeof(TCommand);
            if (!_statistics.ContainsKey(commandType))
            {
                _statistics[commandType] = new CommandStatistics();
            }
            
            if (isGet)
                _statistics[commandType].TotalGets++;
            else
                _statistics[commandType].TotalReturns++;
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// プール設定
    /// </summary>
    [Serializable]
    public class PoolConfiguration
    {
        public int MaxSize = 100;
        public int PrewarmCount = 0;
        
        public static PoolConfiguration Default => new PoolConfiguration();
    }
    
    /// <summary>
    /// コマンドの統計情報
    /// </summary>
    public class CommandStatistics
    {
        public int TotalGets;
        public int TotalReturns;
        public float ReuseRatio => TotalGets > 0 ? (float)TotalReturns / TotalGets : 0f;
    }
    

    
    #endregion
}