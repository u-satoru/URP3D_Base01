using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Commands
{
    /// <summary>
    /// コマンドオブジェクトのプール管理を行うクラス
    /// 頻繁に作成・破棄されるコマンドオブジェクトを再利用してGCを削減します。
    /// </summary>
    public class CommandPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("各コマンドタイプの初期プールサイズ")]
        [SerializeField] private int defaultPoolSize = 10;
        [Tooltip("プールの最大サイズ")]
        [SerializeField] private int maxPoolSize = 100;
        [Tooltip("プール統計を表示する")]
        [SerializeField] private bool showDebugStats = false;
        
        private static CommandPool _instance;
        public static CommandPool Instance => _instance;
        
        private Dictionary<Type, Queue<ICommand>> commandPools;
        private Dictionary<Type, int> poolStats; // デバッグ用統計
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializePools();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializePools()
        {
            commandPools = new Dictionary<Type, Queue<ICommand>>();
            poolStats = new Dictionary<Type, int>();
            
            // よく使用されるコマンドのプールを事前に準備
            PrewarmPool<DamageCommand>(defaultPoolSize);
            PrewarmPool<HealCommand>(defaultPoolSize);
            
            UnityEngine.Debug.Log($"CommandPool initialized with {defaultPoolSize} pre-warmed commands per type");
        }
        
        /// <summary>
        /// 指定したタイプのコマンドをプールから取得します。
        /// プールが空の場合は新しいインスタンスを作成します。
        /// </summary>
        /// <typeparam name="T">取得するコマンドのタイプ</typeparam>
        /// <returns>プールから取得されたまたは新規作成されたコマンド</returns>
        public T GetCommand<T>() where T : ICommand, new()
        {
            Type commandType = typeof(T);
            
            if (!commandPools.ContainsKey(commandType))
            {
                commandPools[commandType] = new Queue<ICommand>();
                poolStats[commandType] = 0;
            }
            
            T command;
            if (commandPools[commandType].Count > 0)
            {
                command = (T)commandPools[commandType].Dequeue();
                if (showDebugStats)
                {
                    poolStats[commandType]++;
                    UnityEngine.Debug.Log($"Retrieved {commandType.Name} from pool (reused {poolStats[commandType]} times)");
                }
            }
            else
            {
                command = new T();
                if (showDebugStats)
                {
                    UnityEngine.Debug.Log($"Created new {commandType.Name} (pool was empty)");
                }
            }
            
            return command;
        }
        
        /// <summary>
        /// 使用済みのコマンドをプールに返却します。
        /// </summary>
        /// <typeparam name="T">返却するコマンドのタイプ</typeparam>
        /// <param name="command">返却するコマンドインスタンス</param>
        public void ReturnCommand<T>(T command) where T : ICommand
        {
            if (command == null) return;
            
            Type commandType = typeof(T);
            
            // リセット可能なコマンドの場合は状態をクリア
            if (command is IResettableCommand resettableCommand)
            {
                resettableCommand.Reset();
            }
            
            if (!commandPools.ContainsKey(commandType))
            {
                commandPools[commandType] = new Queue<ICommand>();
            }
            
            // プールサイズ制限チェック
            if (commandPools[commandType].Count < maxPoolSize)
            {
                commandPools[commandType].Enqueue(command);
                if (showDebugStats)
                {
                    UnityEngine.Debug.Log($"Returned {commandType.Name} to pool (pool size: {commandPools[commandType].Count})");
                }
            }
            else if (showDebugStats)
            {
                UnityEngine.Debug.Log($"Pool for {commandType.Name} is full, discarding command");
            }
        }
        
        /// <summary>
        /// 指定したコマンドタイプのプールを事前に準備します。
        /// </summary>
        /// <typeparam name="T">事前準備するコマンドのタイプ</typeparam>
        /// <param name="count">事前作成する数量</param>
        private void PrewarmPool<T>(int count) where T : ICommand, new()
        {
            for (int i = 0; i < count; i++)
            {
                T command = new T();
                ReturnCommand(command);
            }
        }
        
        /// <summary>
        /// 現在のプール統計情報を取得します（デバッグ用）
        /// </summary>
        /// <returns>プール統計の辞書</returns>
        public Dictionary<Type, int> GetPoolStats()
        {
            var stats = new Dictionary<Type, int>();
            foreach (var kvp in commandPools)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }
        
        /// <summary>
        /// 全てのプールをクリアします。
        /// </summary>
        public void ClearAllPools()
        {
            commandPools.Clear();
            poolStats.Clear();
            UnityEngine.Debug.Log("All command pools cleared");
        }
    }
}