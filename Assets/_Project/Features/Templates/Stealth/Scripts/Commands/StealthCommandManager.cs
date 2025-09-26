using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｳ繝槭Φ繝臥ｵｱ蜷育ｮ｡逅・す繧ｹ繝・Β
    /// 蜈ｨ繧ｹ繝・Ν繧ｹ繧ｳ繝槭Φ繝峨・荳ｭ螟ｮ蛻ｶ蠕｡縺ｨServiceLocator邨ｱ蜷・
    /// ObjectPool譛驕ｩ蛹悶↓繧医ｋ鬮俶ｧ閭ｽ繧ｳ繝槭Φ繝牙ｮ溯｡檎腸蠅・
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

        // 繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ邂｡逅・
        private CommandPoolManager _commandPoolManager;
        private IStealthService _stealthService;

        // 繧｢繧ｯ繝・ぅ繝悶さ繝槭Φ繝臥ｮ｡逅・
        private readonly List<StealthAbilityCommand> _activeAbilities = new();
        private readonly Stack<ICommand> _commandHistory = new();
        private readonly Dictionary<string, ICommand> _namedCommands = new();

        // 邨ｱ險域ュ蝣ｱ
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
        /// 繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｮ蛻晄悄蛹・
        /// </summary>
        private void InitializeCommandPools()
        {
            // CommandPoolManager繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・・・ingleton縺ｧ縺ｯ縺ｪ縺・ｼ・
            _commandPoolManager = new CommandPoolManager(_enableDebugLogs);
            _commandPoolManager.Initialize();

            // 繝励・繝ｫ險ｭ螳壹ｒ逋ｻ骭ｲ・医・繝ｼ繝ｫ閾ｪ菴薙・ GetCommand<T>() 譎ゅ↓閾ｪ蜍穂ｽ懈・縺輔ｌ繧具ｼ・
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
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹・
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
        /// 繧ｹ繝・Ν繧ｹ逶ｸ莠剃ｽ懃畑繧ｳ繝槭Φ繝峨・螳溯｡・
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
        /// 繧ｹ繝・Ν繧ｹ遘ｻ蜍輔さ繝槭Φ繝峨・螳溯｡・
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
        /// 繧ｹ繝・Ν繧ｹ閭ｽ蜉帙さ繝槭Φ繝峨・螳溯｡・
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

            // 繧｢繧ｯ繝・ぅ繝冶・蜉帙Μ繧ｹ繝医↓霑ｽ蜉
            if (success && command.IsActive)
            {
                _activeAbilities.Add(command);
            }

            return success;
        }

        /// <summary>
        /// 豎守畑繧ｳ繝槭Φ繝牙ｮ溯｡・
        /// </summary>
        private bool ExecuteCommand(ICommand command, string commandName = null)
        {
            try
            {
                command.Execute();

                // 螻･豁ｴ縺ｫ霑ｽ蜉
                _commandHistory.Push(command);

                // 蜷榊燕莉倥″繧ｳ繝槭Φ繝峨・蝣ｴ蜷医・霎樊嶌縺ｫ霑ｽ蜉
                if (!string.IsNullOrEmpty(commandName))
                {
                    _namedCommands[commandName] = command;
                }

                // 邨ｱ險域峩譁ｰ
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
        /// 譛蠕後・繧ｳ繝槭Φ繝峨ｒUndo縺吶ｋ
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

                // 繧｢繧ｯ繝・ぅ繝冶・蜉帙°繧牙炎髯､・郁・蜉帙さ繝槭Φ繝峨・蝣ｴ蜷茨ｼ・
                if (lastCommand is StealthAbilityCommand abilityCommand)
                {
                    _activeAbilities.Remove(abilityCommand);
                }

                // 繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｫ霑泌唆
                ReturnCommandToPool(lastCommand);

                // 邨ｱ險域峩譁ｰ
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
        /// 蜷榊燕莉倥″繧ｳ繝槭Φ繝峨ｒUndo縺吶ｋ
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

                // 螻･豁ｴ縺ｨ霎樊嶌縺九ｉ蜑企勁
                _namedCommands.Remove(commandName);

                // 繧｢繧ｯ繝・ぅ繝冶・蜉帙°繧牙炎髯､・郁・蜉帙さ繝槭Φ繝峨・蝣ｴ蜷茨ｼ・
                if (command is StealthAbilityCommand abilityCommand)
                {
                    _activeAbilities.Remove(abilityCommand);
                }

                // 繧ｳ繝槭Φ繝峨・繝ｼ繝ｫ縺ｫ霑泌唆
                ReturnCommandToPool(command);

                if (_enableDebugLogs)
                    Debug.Log($"[StealthCommandManager] Undid named command: {commandName}");

                return true;
            }

            Debug.LogWarning($"[StealthCommandManager] Named command cannot be undone: {commandName}");
            return false;
        }

        /// <summary>
        /// 蜈ｨ繧ｳ繝槭Φ繝牙ｱ･豁ｴ繧偵け繝ｪ繧｢
        /// </summary>
        public void ClearCommandHistory()
        {
            // 螻･豁ｴ縺ｮ繧ｳ繝槭Φ繝峨ｒ繝励・繝ｫ縺ｫ霑泌唆
            while (_commandHistory.Count > 0)
            {
                var command = _commandHistory.Pop();
                ReturnCommandToPool(command);
            }

            // 蜷榊燕莉倥″繧ｳ繝槭Φ繝峨ｂ霑泌唆
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
        /// 繧｢繧ｯ繝・ぅ繝悶↑蜈ｨ閭ｽ蜉帙ｒ辟｡蜉ｹ蛹・
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
        /// 繧｢繧ｯ繝・ぅ繝冶・蜉帙・譖ｴ譁ｰ
        /// </summary>
        private void UpdateActiveAbilities()
        {
            for (int i = _activeAbilities.Count - 1; i >= 0; i--)
            {
                var ability = _activeAbilities[i];
                ability.UpdateAbility();

                // 譛滄剞蛻・ｌ縺ｮ閭ｽ蜉帙ｒ蜑企勁
                if (!ability.IsActive)
                {
                    _activeAbilities.RemoveAt(i);
                    ReturnCommandToPool(ability);
                }
            }
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝峨ｒ繝励・繝ｫ縺ｫ霑泌唆
        /// </summary>
        private void ReturnCommandToPool(ICommand command)
        {
            if (command is IResettableCommand resettableCommand)
            {
                _commandPoolManager.ReturnCommand(resettableCommand);
            }
        }

        /// <summary>
        /// 繝・ヰ繝・げ蜈･蜉帛・逅・
        /// </summary>
        private void HandleDebugInput()
        {
            if (!_enableDebugLogs) return;

            // U 繧ｭ繝ｼ縺ｧ譛蠕後・繧ｳ繝槭Φ繝峨ｒUndo
            if (Input.GetKeyDown(KeyCode.U))
            {
                UndoLastCommand();
            }

            // Ctrl+U 縺ｧ蜈ｨ螻･豁ｴ繧ｯ繝ｪ繧｢
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.U))
            {
                ClearCommandHistory();
            }

            // Ctrl+D 縺ｧ蜈ｨ閭ｽ蜉帷┌蜉ｹ蛹・
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            {
                DeactivateAllAbilities();
            }
        }

        #endregion

        #region Public Properties & Methods

        /// <summary>
        /// 螳溯｡後＆繧後◆繧ｳ繝槭Φ繝峨・邱乗焚
        /// </summary>
        public int TotalCommandsExecuted => _totalCommandsExecuted;

        /// <summary>
        /// Undo縺輔ｌ縺溘さ繝槭Φ繝峨・邱乗焚
        /// </summary>
        public int TotalCommandsUndone => _totalCommandsUndone;

        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝悶↑閭ｽ蜉帙・謨ｰ
        /// </summary>
        public int ActiveAbilitiesCount => _activeAbilities.Count;

        /// <summary>
        /// 繧ｳ繝槭Φ繝牙ｱ･豁ｴ縺ｮ謨ｰ
        /// </summary>
        public int CommandHistoryCount => _commandHistory.Count;

        /// <summary>
        /// 譛蠕後・繧ｳ繝槭Φ繝牙ｮ溯｡梧凾蛻ｻ
        /// </summary>
        public float LastCommandExecutionTime => _lastCommandExecutionTime;

        /// <summary>
        /// 蜷榊燕莉倥″繧ｳ繝槭Φ繝峨′蟄伜惠縺吶ｋ縺九メ繧ｧ繝・け
        /// </summary>
        public bool HasNamedCommand(string commandName)
        {
            return _namedCommands.ContainsKey(commandName);
        }

        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝冶・蜉帙Μ繧ｹ繝医・蜿門ｾ暦ｼ郁ｪｭ縺ｿ蜿悶ｊ蟆ら畑・・
        /// </summary>
        public IReadOnlyList<StealthAbilityCommand> GetActiveAbilities()
        {
            return _activeAbilities.AsReadOnly();
        }

        /// <summary>
        /// 繧ｳ繝槭Φ繝臥ｵｱ險域ュ蝣ｱ縺ｮ蜿門ｾ・
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

            // 蜈ｨ邨ｱ險域ュ蝣ｱ繧貞叙蠕励＠縺ｦ邱丞粋蛻ｩ逕ｨ邇・ｒ險育ｮ・
            var allStats = _commandPoolManager.GetAllStatistics();
            if (allStats.Count == 0) return 0.0f;

            int totalGets = 0;
            int totalReturns = 0;

            foreach (var kvp in allStats)
            {
                totalGets += kvp.Value.TotalGets;
                totalReturns += kvp.Value.TotalReturns;
            }

            // 邁｡譏鍋噪縺ｪ蛻ｩ逕ｨ邇・ｨ育ｮ暦ｼ育樟蝨ｨ菴ｿ逕ｨ荳ｭ縺ｮ繧ｳ繝槭Φ繝画焚 / 蜿門ｾ礼ｷ乗焚・・
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
    /// 繧ｳ繝槭Φ繝臥ｵｱ險域ュ蝣ｱ
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


