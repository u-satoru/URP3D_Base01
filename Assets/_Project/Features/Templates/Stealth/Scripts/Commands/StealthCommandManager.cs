using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// ステルスコマンド統合管理システム
    /// 全ステルスコマンドの中央制御とServiceLocator統合
    /// ObjectPool最適化による高性能コマンド実行環境
    /// </summary>
    public class StealthCommandManager : MonoBehaviour
    {
        [Header("Command Pool Configuration")]
        [SerializeField] private int _interactionCommandPoolSize = 20;
        [SerializeField] private int _movementCommandPoolSize = 15;
        [SerializeField] private int _abilityCommandPoolSize = 10;

        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _trackCommandStatistics = true;

        // コマンドプール管理
        private CommandPoolManager _commandPoolManager;
        private IStealthService _stealthService;

        // アクティブコマンド管理
        private readonly List<StealthAbilityCommand> _activeAbilities = new();
        private readonly Stack<ICommand> _commandHistory = new();
        private readonly Dictionary<string, ICommand> _namedCommands = new();

        // 統計情報
        private int _totalCommandsExecuted;
        private int _totalCommandsUndone;
        private float _lastCommandExecutionTime;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCommandPools();
        }

        private void Start()
        {
            InitializeServices();
        }

        private void Update()
        {
            UpdateActiveAbilities();
            HandleDebugInput();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コマンドプールの初期化
        /// </summary>
        private void InitializeCommandPools()
        {
            // CommandPoolManagerインスタンスを作成（Singletonではない）
            _commandPoolManager = new CommandPoolManager(_enableDebugLogs);
            _commandPoolManager.Initialize();

            // プール設定を登録（プール自体は GetCommand<T>() 時に自動作成される）
            var interactionConfig = new PoolConfiguration
            {
                MaxSize = _interactionCommandPoolSize,
                PrewarmCount = _interactionCommandPoolSize / 4
            };
            _commandPoolManager.RegisterPoolConfiguration<StealthInteractionCommand>(interactionConfig);

            var movementConfig = new PoolConfiguration
            {
                MaxSize = _movementCommandPoolSize,
                PrewarmCount = _movementCommandPoolSize / 4
            };
            _commandPoolManager.RegisterPoolConfiguration<StealthMovementCommand>(movementConfig);

            var abilityConfig = new PoolConfiguration
            {
                MaxSize = _abilityCommandPoolSize,
                PrewarmCount = _abilityCommandPoolSize / 4
            };
            _commandPoolManager.RegisterPoolConfiguration<StealthAbilityCommand>(abilityConfig);

            if (_enableDebugLogs)
                Debug.Log("[StealthCommandManager] Command pools initialized with configurations");
        }

        /// <summary>
        /// サービスの初期化
        /// </summary>
        private void InitializeServices()
        {
            _stealthService = ServiceLocator.GetService<IStealthService>();

            if (_stealthService == null)
            {
                Debug.LogWarning("[StealthCommandManager] StealthService not found in ServiceLocator");
            }
        }

        #endregion

        #region Command Execution Methods

        /// <summary>
        /// ステルス相互作用コマンドの実行
        /// </summary>
        public bool ExecuteInteraction(StealthInteractionType interactionType,
                                     GameObject targetObject, float duration = 1.0f, bool requiresStealth = true,
                                     string commandName = null)
        {
            var command = _commandPoolManager.GetCommand<StealthInteractionCommand>();
            if (command == null)
            {
                Debug.LogError("[StealthCommandManager] Failed to get StealthInteractionCommand from pool");
                return false;
            }

            command.Initialize(interactionType, targetObject, duration, requiresStealth);
            return ExecuteCommand(command, commandName);
        }

        /// <summary>
        /// ステルス移動コマンドの実行
        /// </summary>
        public bool ExecuteMovement(StealthMovementCommand.StealthMovementType movementType, Vector3 targetPosition,
                                  float duration = 1.0f, float speedMultiplier = 1.0f, bool maintainStealth = true,
                                  string commandName = null)
        {
            var command = _commandPoolManager.GetCommand<StealthMovementCommand>();
            if (command == null)
            {
                Debug.LogError("[StealthCommandManager] Failed to get StealthMovementCommand from pool");
                return false;
            }

            command.Initialize(movementType, targetPosition, duration, speedMultiplier, maintainStealth);
            return ExecuteCommand(command, commandName);
        }

        /// <summary>
        /// ステルス能力コマンドの実行
        /// </summary>
        public bool ExecuteAbility(StealthAbilityCommand.StealthAbilityType abilityType, float duration = 5.0f,
                                 float intensity = 1.0f, Vector3 targetLocation = default,
                                 GameObject targetObject = null, string commandName = null)
        {
            var command = _commandPoolManager.GetCommand<StealthAbilityCommand>();
            if (command == null)
            {
                Debug.LogError("[StealthCommandManager] Failed to get StealthAbilityCommand from pool");
                return false;
            }

            command.Initialize(abilityType, duration, intensity, targetLocation, targetObject);
            bool success = ExecuteCommand(command, commandName);

            // アクティブ能力リストに追加
            if (success && command.IsActive)
            {
                _activeAbilities.Add(command);
            }

            return success;
        }

        /// <summary>
        /// 汎用コマンド実行
        /// </summary>
        private bool ExecuteCommand(ICommand command, string commandName = null)
        {
            try
            {
                command.Execute();

                // 履歴に追加
                _commandHistory.Push(command);

                // 名前付きコマンドの場合は辞書に追加
                if (!string.IsNullOrEmpty(commandName))
                {
                    _namedCommands[commandName] = command;
                }

                // 統計更新
                if (_trackCommandStatistics)
                {
                    _totalCommandsExecuted++;
                    _lastCommandExecutionTime = Time.time;
                }

                if (_enableDebugLogs)
                    Debug.Log($"[StealthCommandManager] Executed command: {command.GetType().Name}");

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StealthCommandManager] Command execution failed: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Command Management

        /// <summary>
        /// 最後のコマンドをUndoする
        /// </summary>
        public bool UndoLastCommand()
        {
            if (_commandHistory.Count == 0)
            {
                Debug.LogWarning("[StealthCommandManager] No commands to undo");
                return false;
            }

            var lastCommand = _commandHistory.Pop();
            if (lastCommand.CanUndo)
            {
                lastCommand.Undo();

                // アクティブ能力から削除（能力コマンドの場合）
                if (lastCommand is StealthAbilityCommand abilityCommand)
                {
                    _activeAbilities.Remove(abilityCommand);
                }

                // コマンドプールに返却
                ReturnCommandToPool(lastCommand);

                // 統計更新
                if (_trackCommandStatistics)
                {
                    _totalCommandsUndone++;
                }

                if (_enableDebugLogs)
                    Debug.Log($"[StealthCommandManager] Undid command: {lastCommand.GetType().Name}");

                return true;
            }

            Debug.LogWarning("[StealthCommandManager] Last command cannot be undone");
            return false;
        }

        /// <summary>
        /// 名前付きコマンドをUndoする
        /// </summary>
        public bool UndoNamedCommand(string commandName)
        {
            if (!_namedCommands.TryGetValue(commandName, out var command))
            {
                Debug.LogWarning($"[StealthCommandManager] Named command not found: {commandName}");
                return false;
            }

            if (command.CanUndo)
            {
                command.Undo();

                // 履歴と辞書から削除
                _namedCommands.Remove(commandName);

                // アクティブ能力から削除（能力コマンドの場合）
                if (command is StealthAbilityCommand abilityCommand)
                {
                    _activeAbilities.Remove(abilityCommand);
                }

                // コマンドプールに返却
                ReturnCommandToPool(command);

                if (_enableDebugLogs)
                    Debug.Log($"[StealthCommandManager] Undid named command: {commandName}");

                return true;
            }

            Debug.LogWarning($"[StealthCommandManager] Named command cannot be undone: {commandName}");
            return false;
        }

        /// <summary>
        /// 全コマンド履歴をクリア
        /// </summary>
        public void ClearCommandHistory()
        {
            // 履歴のコマンドをプールに返却
            while (_commandHistory.Count > 0)
            {
                var command = _commandHistory.Pop();
                ReturnCommandToPool(command);
            }

            // 名前付きコマンドも返却
            foreach (var command in _namedCommands.Values)
            {
                ReturnCommandToPool(command);
            }

            _namedCommands.Clear();
            _activeAbilities.Clear();

            if (_enableDebugLogs)
                Debug.Log("[StealthCommandManager] Command history cleared");
        }

        /// <summary>
        /// アクティブな全能力を無効化
        /// </summary>
        public void DeactivateAllAbilities()
        {
            foreach (var ability in _activeAbilities)
            {
                ability.ManualDeactivate();
                ReturnCommandToPool(ability);
            }

            _activeAbilities.Clear();

            if (_enableDebugLogs)
                Debug.Log("[StealthCommandManager] All abilities deactivated");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// アクティブ能力の更新
        /// </summary>
        private void UpdateActiveAbilities()
        {
            for (int i = _activeAbilities.Count - 1; i >= 0; i--)
            {
                var ability = _activeAbilities[i];
                ability.UpdateAbility();

                // 期限切れの能力を削除
                if (!ability.IsActive)
                {
                    _activeAbilities.RemoveAt(i);
                    ReturnCommandToPool(ability);
                }
            }
        }

        /// <summary>
        /// コマンドをプールに返却
        /// </summary>
        private void ReturnCommandToPool(ICommand command)
        {
            if (command is IResettableCommand resettableCommand)
            {
                _commandPoolManager.ReturnCommand(resettableCommand);
            }
        }

        /// <summary>
        /// デバッグ入力処理
        /// </summary>
        private void HandleDebugInput()
        {
            if (!_enableDebugLogs) return;

            // U キーで最後のコマンドをUndo
            if (Input.GetKeyDown(KeyCode.U))
            {
                UndoLastCommand();
            }

            // Ctrl+U で全履歴クリア
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
            {
                ClearCommandHistory();
            }

            // Ctrl+D で全能力無効化
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            {
                DeactivateAllAbilities();
            }
        }

        #endregion

        #region Public Properties & Methods

        /// <summary>
        /// 実行されたコマンドの総数
        /// </summary>
        public int TotalCommandsExecuted => _totalCommandsExecuted;

        /// <summary>
        /// Undoされたコマンドの総数
        /// </summary>
        public int TotalCommandsUndone => _totalCommandsUndone;

        /// <summary>
        /// アクティブな能力の数
        /// </summary>
        public int ActiveAbilitiesCount => _activeAbilities.Count;

        /// <summary>
        /// コマンド履歴の数
        /// </summary>
        public int CommandHistoryCount => _commandHistory.Count;

        /// <summary>
        /// 最後のコマンド実行時刻
        /// </summary>
        public float LastCommandExecutionTime => _lastCommandExecutionTime;

        /// <summary>
        /// 名前付きコマンドが存在するかチェック
        /// </summary>
        public bool HasNamedCommand(string commandName)
        {
            return _namedCommands.ContainsKey(commandName);
        }

        /// <summary>
        /// アクティブ能力リストの取得（読み取り専用）
        /// </summary>
        public IReadOnlyList<StealthAbilityCommand> GetActiveAbilities()
        {
            return _activeAbilities.AsReadOnly();
        }

        /// <summary>
        /// コマンド統計情報の取得
        /// </summary>
        public CommandStatistics GetStatistics()
        {
            return new CommandStatistics
            {
                TotalExecuted = _totalCommandsExecuted,
                TotalUndone = _totalCommandsUndone,
                ActiveAbilities = _activeAbilities.Count,
                HistoryCount = _commandHistory.Count,
                LastExecutionTime = _lastCommandExecutionTime,
                PoolUtilization = GetPoolUtilization()
            };
        }

        private float GetPoolUtilization()
        {
            if (_commandPoolManager == null) return 0.0f;

            // 全統計情報を取得して総合利用率を計算
            var allStats = _commandPoolManager.GetAllStatistics();
            if (allStats.Count == 0) return 0.0f;

            int totalGets = 0;
            int totalReturns = 0;

            foreach (var kvp in allStats)
            {
                totalGets += kvp.Value.TotalGets;
                totalReturns += kvp.Value.TotalReturns;
            }

            // 簡易的な利用率計算（現在使用中のコマンド数 / 取得総数）
            int currentlyInUse = totalGets - totalReturns;
            return totalGets > 0 ? currentlyInUse / (float)totalGets : 0.0f;
        }

        #endregion

        #region Debug GUI

        private void OnGUI()
        {
            if (!_enableDebugLogs || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 200, 350, 300));
            GUILayout.Label("=== Stealth Command Manager Debug ===");
            GUILayout.Label($"Commands Executed: {_totalCommandsExecuted}");
            GUILayout.Label($"Commands Undone: {_totalCommandsUndone}");
            GUILayout.Label($"Active Abilities: {_activeAbilities.Count}");
            GUILayout.Label($"History Count: {_commandHistory.Count}");
            GUILayout.Label($"Pool Utilization: {GetPoolUtilization():P}");

            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("U - Undo Last Command");
            GUILayout.Label("Ctrl+U - Clear History");
            GUILayout.Label("Ctrl+D - Deactivate All Abilities");

            if (_activeAbilities.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Active Abilities:");
                foreach (var ability in _activeAbilities)
                {
                    GUILayout.Label($"- {ability.AbilityType}: {ability.RemainingDuration:F1}s");
                }
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// コマンド統計情報
    /// </summary>
    [System.Serializable]
    public struct CommandStatistics
    {
        public int TotalExecuted;
        public int TotalUndone;
        public int ActiveAbilities;
        public int HistoryCount;
        public float LastExecutionTime;
        public float PoolUtilization;
    }
}