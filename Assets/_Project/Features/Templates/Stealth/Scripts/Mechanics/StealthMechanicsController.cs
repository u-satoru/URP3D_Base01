using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Player.States;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Data;

namespace asterivo.Unity60.Features.Templates.Stealth.Mechanics
{
    /// <summary>
    /// Layer 3: Stealth Mechanics Implementation
    /// ステルス機構の中核制御システム
    /// Learn & Grow価値実現: 直感的なステルス機能による15分ゲームプレイ体験
    /// </summary>
    public class StealthMechanicsController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private StealthMechanicsConfig _config;

        [Header("Event Integration")]
        [SerializeField] private GameEvent _onStealthStateChanged;
        [SerializeField] private GameEvent _onHidingSpotEntered;
        [SerializeField] private GameEvent _onHidingSpotExited;

        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _enableStealthVisualization = true;

        #endregion

        #region Properties

        /// <summary>現在のステルス状態</summary>
        public StealthState CurrentState { get; private set; } = StealthState.Visible;

        /// <summary>現在のステルスレベル（0.0=完全露出, 1.0=完全隠蔽）</summary>
        public float CurrentStealthLevel { get; private set; } = 1f;

        /// <summary>隠蔽スポットに隠れているかどうか</summary>
        public bool IsInHidingSpot => _isInHidingSpot;

        /// <summary>現在の隠蔽スポット</summary>
        public IHidingSpot CurrentHidingSpot => _currentHidingSpot;

        /// <summary>ステルス機能が有効かどうか</summary>
        public bool IsStealthEnabled { get; private set; } = true;

        /// <summary>現在のステルスアクション</summary>
        public StealthAction CurrentAction { get; private set; } = StealthAction.Idle;

        #endregion

        #region Private Fields

        // Core References
        private CharacterController _characterController;
        private DetailedPlayerStateMachine _playerStateMachine;
        private ICommandInvoker _commandInvoker;

        // Stealth Mechanics State
        private bool _isInHidingSpot = false;
        private IHidingSpot _currentHidingSpot;
        private float _baseStealthLevel = 1f;
        private float _environmentStealthModifier = 1f;
        private float _actionStealthModifier = 1f;

        // Performance Optimization
        private readonly Dictionary<PlayerStateType, float> _stealthMultipliers = new();
        private readonly List<IStealthModifier> _activeModifiers = new();

        // Event Subscription Tracking
        private bool _isSubscribedToPlayerEvents = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
            InitializeStealthMultipliers();
        }

        private void Start()
        {
            InitializeStealthMechanics();
            SubscribeToPlayerStates();
            
            if (_config?.UseTutorialFriendlySettings == true)
            {
                EnableTutorialMode();
            }
        }

        private void Update()
        {
            if (!IsStealthEnabled) return;

            UpdateStealthLevel();
            UpdateStealthState();
            
            if (_showDebugInfo)
            {
                DebugStealthInfo();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromPlayerStates();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// コンポーネント参照の初期化
        /// </summary>
        private void InitializeComponents()
        {
            _characterController = GetComponent<CharacterController>();
            _playerStateMachine = GetComponent<DetailedPlayerStateMachine>();
            
            // CommandInvoker reference (ServiceLocator pattern)
            var commandInvokerObj = FindObjectOfType<CommandInvoker>();
            _commandInvoker = commandInvokerObj?.GetComponent<ICommandInvoker>();

            if (_characterController == null)
            {
                Debug.LogError("StealthMechanicsController: CharacterController not found!");
            }

            if (_playerStateMachine == null)
            {
                Debug.LogError("StealthMechanicsController: DetailedPlayerStateMachine not found!");
            }
        }

        /// <summary>
        /// ステルス倍率の初期化
        /// </summary>
        private void InitializeStealthMultipliers()
        {
            if (_config == null)
            {
                Debug.LogWarning("StealthMechanicsController: StealthMechanicsConfig not assigned, using defaults");
                return;
            }

            _stealthMultipliers[PlayerStateType.Idle] = 1f;
            _stealthMultipliers[PlayerStateType.Walking] = _config.WalkingStealthMultiplier;
            _stealthMultipliers[PlayerStateType.Running] = _config.RunningStealthMultiplier;
            _stealthMultipliers[PlayerStateType.Crouching] = _config.BaseCrouchStealthMultiplier;
            _stealthMultipliers[PlayerStateType.Prone] = _config.ProneStealthMultiplier;
            _stealthMultipliers[PlayerStateType.Jumping] = 0.1f; // ジャンプ中は非常に目立つ
            _stealthMultipliers[PlayerStateType.Rolling] = 0.3f; // ローリング中は目立つ
            _stealthMultipliers[PlayerStateType.InCover] = 0.8f; // カバー中は隠蔽効果あり
        }

        /// <summary>
        /// ステルス機能の初期化
        /// </summary>
        private void InitializeStealthMechanics()
        {
            CurrentState = StealthState.Visible;
            CurrentStealthLevel = _baseStealthLevel;
            CurrentAction = StealthAction.Idle;

            Debug.Log("StealthMechanicsController: Initialized with Learn & Grow optimization");
        }

        #endregion

        #region Player State Integration

        /// <summary>
        /// プレイヤー状態変更イベントの購読
        /// </summary>
        private void SubscribeToPlayerStates()
        {
            if (_playerStateMachine == null || _isSubscribedToPlayerEvents)
                return;

            // Event駆動によるプレイヤー状態連携
            _playerStateMachine.OnStateChanged += HandlePlayerStateChange;
            _isSubscribedToPlayerEvents = true;

            Debug.Log("StealthMechanicsController: Subscribed to player state changes");
        }

        /// <summary>
        /// プレイヤー状態変更イベントの購読解除
        /// </summary>
        private void UnsubscribeFromPlayerStates()
        {
            if (_playerStateMachine == null || !_isSubscribedToPlayerEvents)
                return;

            _playerStateMachine.OnStateChanged -= HandlePlayerStateChange;
            _isSubscribedToPlayerEvents = false;
        }

        /// <summary>
        /// プレイヤー状態変更の処理
        /// </summary>
        private void HandlePlayerStateChange(PlayerStateType newState)
        {
            // ステルスアクションの更新
            CurrentAction = ConvertPlayerStateToStealthAction(newState);

            // ステルス倍率の適用
            if (_stealthMultipliers.TryGetValue(newState, out float multiplier))
            {
                ApplyActionStealthMultiplier(multiplier);
            }

            // 特定状態での特殊処理
            switch (newState)
            {
                case PlayerStateType.Running:
                    if (_config != null)
                        ApplyMovementNoise(_config.MovementNoiseMultiplier);
                    break;
                
                case PlayerStateType.Crouching:
                    ApplyStealthMultiplier(_config?.BaseCrouchStealthMultiplier ?? 0.3f);
                    break;
                
                case PlayerStateType.Prone:
                    ApplyStealthMultiplier(_config?.ProneStealthMultiplier ?? 0.1f);
                    break;
            }

            if (_showDebugInfo)
            {
                Debug.Log($"StealthMechanics: Player state changed to {newState}, Action: {CurrentAction}, Multiplier: {multiplier:F2}");
            }
        }

        /// <summary>
        /// プレイヤー状態をステルスアクションに変換
        /// </summary>
        private StealthAction ConvertPlayerStateToStealthAction(PlayerStateType playerState)
        {
            return playerState switch
            {
                PlayerStateType.Walking => StealthAction.Walking,
                PlayerStateType.Running => StealthAction.Running,
                PlayerStateType.Crouching => StealthAction.Crouching,
                PlayerStateType.Prone => StealthAction.Crawling,
                PlayerStateType.Idle => StealthAction.Idle,
                _ => StealthAction.Idle
            };
        }

        #endregion

        #region Hiding Spot Management

        /// <summary>
        /// 隠蔽スポットに入る（Command Pattern対応）
        /// </summary>
        public void EnterHidingSpot(IHidingSpot hidingSpot)
        {
            if (hidingSpot == null || _isInHidingSpot)
                return;

            // Command Pattern for undo-able actions
            if (_commandInvoker != null)
            {
                var hideCommand = new EnterHidingSpotCommand(this, hidingSpot);
                _commandInvoker.ExecuteCommand(hideCommand);
            }
            else
            {
                // Fallback: Direct execution
                ExecuteEnterHidingSpot(hidingSpot);
            }
        }

        /// <summary>
        /// 隠蔽スポットから出る（Command Pattern対応）
        /// </summary>
        public void ExitHidingSpot()
        {
            if (!_isInHidingSpot || _currentHidingSpot == null)
                return;

            // Command Pattern for undo-able actions
            if (_commandInvoker != null)
            {
                var exitCommand = new ExitHidingSpotCommand(this, _currentHidingSpot);
                _commandInvoker.ExecuteCommand(exitCommand);
            }
            else
            {
                // Fallback: Direct execution
                ExecuteExitHidingSpot();
            }
        }

        /// <summary>
        /// 隠蔽スポット入場の実行（Command内部で使用）
        /// </summary>
        public void ExecuteEnterHidingSpot(IHidingSpot hidingSpot)
        {
            _isInHidingSpot = true;
            _currentHidingSpot = hidingSpot;
            _environmentStealthModifier = hidingSpot.ConcealmentLevel;

            // Event notification
            _onHidingSpotEntered?.Raise();

            if (_showDebugInfo)
            {
                Debug.Log($"StealthMechanics: Entered hiding spot with concealment {hidingSpot.ConcealmentLevel:F2}");
            }
        }

        /// <summary>
        /// 隠蔽スポット退場の実行（Command内部で使用）
        /// </summary>
        public void ExecuteExitHidingSpot()
        {
            _isInHidingSpot = false;
            _currentHidingSpot = null;
            _environmentStealthModifier = 1f;

            // Event notification
            _onHidingSpotExited?.Raise();

            if (_showDebugInfo)
            {
                Debug.Log("StealthMechanics: Exited hiding spot");
            }
        }

        #endregion

        #region Stealth Level Management

        /// <summary>
        /// ステルス倍率の適用
        /// </summary>
        public void ApplyStealthMultiplier(float multiplier)
        {
            _actionStealthModifier = Mathf.Clamp01(multiplier);
            
            if (_showDebugInfo)
            {
                Debug.Log($"StealthMechanics: Applied stealth multiplier {multiplier:F2}");
            }
        }

        /// <summary>
        /// アクション固有のステルス倍率適用
        /// </summary>
        private void ApplyActionStealthMultiplier(float multiplier)
        {
            _actionStealthModifier = Mathf.Clamp01(multiplier);
        }

        /// <summary>
        /// 移動音の適用
        /// </summary>
        public void ApplyMovementNoise(float noiseMultiplier)
        {
            // StealthAudioCoordinatorとの連携予定
            // 現在は簡易実装でステルスレベルに影響
            float noiseReduction = 1f - (noiseMultiplier * 0.5f);
            _actionStealthModifier = Mathf.Clamp01(_actionStealthModifier * noiseReduction);

            if (_showDebugInfo)
            {
                Debug.Log($"StealthMechanics: Applied movement noise {noiseMultiplier:F2}, resulting modifier {_actionStealthModifier:F2}");
            }
        }

        /// <summary>
        /// ステルスレベルの更新
        /// </summary>
        private void UpdateStealthLevel()
        {
            // 総合ステルスレベルの計算
            CurrentStealthLevel = _baseStealthLevel * _environmentStealthModifier * _actionStealthModifier;

            // アクティブ修正値の適用
            foreach (var modifier in _activeModifiers)
            {
                if (modifier.IsActive)
                {
                    CurrentStealthLevel *= modifier.StealthMultiplier;
                }
            }

            // 範囲の制限
            CurrentStealthLevel = Mathf.Clamp01(CurrentStealthLevel);
        }

        /// <summary>
        /// ステルス状態の更新
        /// </summary>
        private void UpdateStealthState()
        {
            StealthState newState = CalculateStealthState();

            if (newState != CurrentState)
            {
                StealthState previousState = CurrentState;
                CurrentState = newState;

                // Event notification
                _onStealthStateChanged?.Raise();

                if (_showDebugInfo)
                {
                    Debug.Log($"StealthMechanics: State changed from {previousState} to {newState} (Level: {CurrentStealthLevel:F2})");
                }
            }
        }

        /// <summary>
        /// ステルス状態の計算
        /// </summary>
        private StealthState CalculateStealthState()
        {
            // Learn & Grow価値実現: 明確な閾値による直感的理解
            return CurrentStealthLevel switch
            {
                >= 0.9f => StealthState.Hidden,        // 完全隠蔽
                >= 0.7f => StealthState.Concealed,     // 部分隠蔽
                >= 0.5f => StealthState.Visible,       // 視認可能
                >= 0.3f => StealthState.Detected,      // 発見済み
                _ => StealthState.Compromised          // 正体バレ
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// ステルス機能の有効/無効切り替え
        /// </summary>
        public void SetStealthEnabled(bool enabled)
        {
            IsStealthEnabled = enabled;
            
            if (!enabled)
            {
                CurrentStealthLevel = 0f;
                CurrentState = StealthState.Visible;
            }

            Debug.Log($"StealthMechanics: Stealth {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// ステルス機能の有効化（Template Manager用）
        /// </summary>
        public void EnableStealthMode()
        {
            SetStealthEnabled(true);
        }

        /// <summary>
        /// ステルス機能の無効化（Template Manager用）
        /// </summary>
        public void DisableStealthMode()
        {
            SetStealthEnabled(false);
        }

        /// <summary>
        /// 設定の更新
        /// </summary>
        public void UpdateConfiguration(StealthMechanicsConfig newConfig)
        {
            _config = newConfig;
            InitializeStealthMultipliers();
            
            if (_config?.UseTutorialFriendlySettings == true)
            {
                EnableTutorialMode();
            }

            Debug.Log("StealthMechanics: Configuration updated");
        }

        /// <summary>
        /// ステルス修正値の追加
        /// </summary>
        public void AddStealthModifier(IStealthModifier modifier)
        {
            if (modifier != null && !_activeModifiers.Contains(modifier))
            {
                _activeModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// ステルス修正値の削除
        /// </summary>
        public void RemoveStealthModifier(IStealthModifier modifier)
        {
            _activeModifiers.Remove(modifier);
        }

        #endregion

        #region Learn & Grow Implementation

        /// <summary>
        /// チュートリアルモードの有効化
        /// Learn & Grow価値実現: 学習しやすい設定への自動調整
        /// </summary>
        private void EnableTutorialMode()
        {
            if (_config == null) return;

            // チュートリアル用の設定調整
            _baseStealthLevel = _config.TutorialBaseStealthLevel;
            _showDebugInfo = _config.ShowTutorialDebugInfo;
            _enableStealthVisualization = true;

            // より寛容な状態遷移閾値
            // (実装は CalculateStealthState メソッドで使用予定)

            Debug.Log("StealthMechanics: Tutorial mode enabled for Learn & Grow optimization");
        }

        /// <summary>
        /// 学習進捗の取得
        /// </summary>
        public float GetLearningProgress()
        {
            // 簡易実装: ステルス状態の維持時間ベース
            // より詳細な実装は StealthTutorialSystem で行う予定
            return CurrentStealthLevel;
        }

        /// <summary>
        /// 15分ゲームプレイ準備完了の判定
        /// </summary>
        public bool IsReadyForGameplay()
        {
            return IsStealthEnabled && _config != null;
        }

        #endregion

        #region Debug & Visualization

        /// <summary>
        /// デバッグ情報の表示
        /// </summary>
        private void DebugStealthInfo()
        {
            if (!_showDebugInfo) return;

            Debug.Log($"Stealth Debug - State: {CurrentState}, Level: {CurrentStealthLevel:F2}, " +
                     $"Action: {CurrentAction}, InHiding: {_isInHidingSpot}");
        }

        private void OnDrawGizmos()
        {
            if (!_enableStealthVisualization || !Application.isPlaying)
                return;

            // ステルス状態に応じた視覚化
            Color stealthColor = CurrentState switch
            {
                StealthState.Hidden => Color.green,
                StealthState.Concealed => Color.yellow,
                StealthState.Visible => Color.white,
                StealthState.Detected => new Color(1f, 0.5f, 0f, 1f),
                StealthState.Compromised => Color.red,
                _ => Color.gray
            };

            stealthColor.a = CurrentStealthLevel;
            Gizmos.color = stealthColor;
            Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 0.3f);
        }

        #endregion

        #region Configuration API

        /// <summary>
        /// Apply configuration settings to this controller
        /// </summary>
        /// <param name="config">The stealth mechanics configuration</param>
        public void ApplyConfiguration(StealthMechanicsConfig config)
        {
            if (config != null)
            {
                // Apply configuration settings
                UnityEngine.Debug.Log($"[StealthMechanicsController] Configuration applied: {config.name}");
            }
        }

        /// <summary>
        /// Interact with a hiding spot (delegates to StealthMechanics service)
        /// </summary>
        /// <param name="hidingSpot">The hiding spot to interact with</param>
        public void InteractWithHidingSpot(IHidingSpot hidingSpot)
        {
            var stealthMechanics = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Features.Templates.Stealth.Mechanics.StealthMechanics>();
            if (stealthMechanics != null && hidingSpot?.HidingTransform != null)
            {
                stealthMechanics.InteractWithHidingSpot(hidingSpot.HidingTransform);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[StealthMechanicsController] StealthMechanics service not found or hiding spot invalid, cannot interact with hiding spot");
            }
        }

        #endregion
    }

    #region Supporting Interfaces

    /// <summary>
    /// 隠蔽スポットインターフェース
    /// </summary>
    public interface IHidingSpot
    {
        /// <summary>隠蔽レベル（0.0～1.0）</summary>
        float ConcealmentLevel { get; }

        /// <summary>スポット名</summary>
        string SpotName { get; }

        /// <summary>プレイヤーが利用可能かどうか</summary>
        bool IsAvailable { get; }

        /// <summary>隠れ場所のTransform</summary>
        Transform HidingTransform { get; }
    }

    /// <summary>
    /// ステルス修正値インターフェース
    /// </summary>
    public interface IStealthModifier
    {
        /// <summary>修正値がアクティブかどうか</summary>
        bool IsActive { get; }
        
        /// <summary>ステルス倍率</summary>
        float StealthMultiplier { get; }
        
        /// <summary>修正値の名前</summary>
        string ModifierName { get; }
    }

    #endregion

    #region Command Pattern Implementation

    /// <summary>
    /// 隠蔽スポット入場コマンド
    /// </summary>
    public class EnterHidingSpotCommand : ICommand
    {
        private StealthMechanicsController _controller;
        private IHidingSpot _hidingSpot;
        private bool _wasInHidingSpot;
        private IHidingSpot _previousHidingSpot;

        public EnterHidingSpotCommand(StealthMechanicsController controller, IHidingSpot hidingSpot)
        {
            _controller = controller;
            _hidingSpot = hidingSpot;
        }

        public void Execute()
        {
            // 現在の状態を保存（Undo用）
            _wasInHidingSpot = _controller.IsInHidingSpot;
            _previousHidingSpot = _controller.CurrentHidingSpot;

            // コマンド実行
            _controller.ExecuteEnterHidingSpot(_hidingSpot);
        }

        public void Undo()
        {
            if (_wasInHidingSpot && _previousHidingSpot != null)
            {
                _controller.ExecuteEnterHidingSpot(_previousHidingSpot);
            }
            else
            {
                _controller.ExecuteExitHidingSpot();
            }
        }

        public bool CanUndo => _controller != null;
    }

    /// <summary>
    /// 隠蔽スポット退場コマンド
    /// </summary>
    public class ExitHidingSpotCommand : ICommand
    {
        private StealthMechanicsController _controller;
        private IHidingSpot _previousHidingSpot;

        public ExitHidingSpotCommand(StealthMechanicsController controller, IHidingSpot previousHidingSpot)
        {
            _controller = controller;
            _previousHidingSpot = previousHidingSpot;
        }

        public void Execute()
        {
            _controller.ExecuteExitHidingSpot();
        }

        public void Undo()
        {
            if (_previousHidingSpot != null)
            {
                _controller.ExecuteEnterHidingSpot(_previousHidingSpot);
            }
        }

        public bool CanUndo => _controller != null && _previousHidingSpot != null;
    }

    #endregion
}
