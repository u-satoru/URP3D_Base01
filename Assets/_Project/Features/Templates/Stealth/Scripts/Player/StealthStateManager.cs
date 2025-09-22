using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Player.States;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player
{
    /// <summary>
    /// ステルステンプレート用状態管理システム
    /// 通常の状態とステルス強化状態を動的に切り替え
    /// ServiceLocator統合による中央制御とイベント駆動アーキテクチャ
    /// </summary>
    public class StealthStateManager : MonoBehaviour
    {
        [Header("Stealth State Configuration")]
        [SerializeField] private bool _useStealthEnhancedStates = true;
        [SerializeField] private bool _autoEnableStealthMode = true;
        [SerializeField] private float _stealthModeToggleCooldown = 1.0f;

        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;

        // ステルス強化状態の管理
        private Dictionary<PlayerStateType, IPlayerState> _stealthStates;
        private Dictionary<PlayerStateType, IPlayerState> _normalStates;

        // サービス参照
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;

        // 状態管理
        private bool _isStealthModeActive = false;
        private float _lastToggleTime = 0f;

        /// <summary>
        /// ステルス強化状態が有効かどうか
        /// </summary>
        public bool UseStealthEnhancedStates
        {
            get => _useStealthEnhancedStates;
            set
            {
                if (_useStealthEnhancedStates != value)
                {
                    _useStealthEnhancedStates = value;
                    RefreshStateMapping();

                    if (_enableDebugLogs)
                        Debug.Log($"[StealthStateManager] Stealth enhanced states: {(_useStealthEnhancedStates ? "Enabled" : "Disabled")}");
                }
            }
        }

        /// <summary>
        /// 現在ステルスモードが有効かどうか
        /// </summary>
        public bool IsStealthModeActive => _isStealthModeActive;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeStates();
        }

        private void Start()
        {
            InitializeServices();

            if (_autoEnableStealthMode)
            {
                EnableStealthMode();
            }
        }

        private void Update()
        {
            HandleStealthModeToggle();
            UpdateStealthState();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// ステルス状態とノーマル状態の初期化
        /// </summary>
        private void InitializeStates()
        {
            // ステルス強化状態の登録
            _stealthStates = new Dictionary<PlayerStateType, IPlayerState>
            {
                { PlayerStateType.Crouching, new StealthCrouchingState() },
                { PlayerStateType.Prone, new StealthProneState() },
                { PlayerStateType.InCover, new StealthInCoverState() }
            };

            // 通常状態の登録（既存実装への参照）
            _normalStates = new Dictionary<PlayerStateType, IPlayerState>();

            if (_enableDebugLogs)
                Debug.Log($"[StealthStateManager] Initialized {_stealthStates.Count} stealth enhanced states");
        }

        /// <summary>
        /// サービスとコンポーネントの初期化
        /// </summary>
        private void InitializeServices()
        {
            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning("[StealthStateManager] StealthService not found in ServiceLocator");
            }

            // プレイヤーステートマシンの参照を取得
            _playerStateMachine = GetComponent<DetailedPlayerStateMachine>();
            if (_playerStateMachine == null)
            {
                Debug.LogError("[StealthStateManager] DetailedPlayerStateMachine not found on this GameObject");
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// 状態マッピングのリフレッシュ
        /// ステルス強化状態と通常状態の切り替え
        /// </summary>
        private void RefreshStateMapping()
        {
            if (_playerStateMachine == null) return;

            if (_useStealthEnhancedStates && _isStealthModeActive)
            {
                // ステルス強化状態を適用
                foreach (var kvp in _stealthStates)
                {
                    // 実際の状態置き換えは、プレイヤーステートマシンの実装に依存
                    // ここでは概念的な実装を示す
                    RegisterStealthState(kvp.Key, kvp.Value);
                }
            }
            else
            {
                // 通常状態に戻す
                RestoreNormalStates();
            }
        }

        /// <summary>
        /// ステルス状態の登録
        /// </summary>
        private void RegisterStealthState(PlayerStateType stateType, IPlayerState stealthState)
        {
            // プレイヤーステートマシンにステルス強化状態を登録
            // 実装は既存のDetailedPlayerStateMachine.csの構造に依存

            if (_enableDebugLogs)
                Debug.Log($"[StealthStateManager] Registered stealth enhanced state: {stateType}");
        }

        /// <summary>
        /// 通常状態の復元
        /// </summary>
        private void RestoreNormalStates()
        {
            // 通常状態に戻す処理
            // 実装は既存のDetailedPlayerStateMachine.csの構造に依存

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Restored normal states");
        }

        /// <summary>
        /// 現在の状態がステルス関連かどうかを判定
        /// </summary>
        public bool IsCurrentStateStealthRelated()
        {
            if (_playerStateMachine == null) return false;

            PlayerStateType currentState = _playerStateMachine.GetCurrentStateType();
            return _stealthStates.ContainsKey(currentState);
        }

        /// <summary>
        /// 指定された状態にステルス強化版が存在するかチェック
        /// </summary>
        public bool HasStealthEnhancedVersion(PlayerStateType stateType)
        {
            return _stealthStates.ContainsKey(stateType);
        }
        #endregion

        #region Stealth Mode Control
        /// <summary>
        /// ステルスモードの有効化
        /// </summary>
        public void EnableStealthMode()
        {
            if (_isStealthModeActive) return;

            _isStealthModeActive = true;

            // StealthServiceでステルスモードを有効化
            _stealthService?.SetStealthMode(true);

            // 状態マッピングをリフレッシュ
            RefreshStateMapping();

            // ステルス関連UIの表示（将来実装）
            ShowStealthUI();

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Stealth mode enabled");
        }

        /// <summary>
        /// ステルスモードの無効化
        /// </summary>
        public void DisableStealthMode()
        {
            if (!_isStealthModeActive) return;

            _isStealthModeActive = false;

            // StealthServiceでステルスモードを無効化
            _stealthService?.SetStealthMode(false);

            // 通常状態に戻す
            RefreshStateMapping();

            // ステルス関連UIの非表示
            HideStealthUI();

            if (_enableDebugLogs)
                Debug.Log("[StealthStateManager] Stealth mode disabled");
        }

        /// <summary>
        /// ステルスモードの切り替え
        /// </summary>
        public void ToggleStealthMode()
        {
            if (Time.time - _lastToggleTime < _stealthModeToggleCooldown) return;

            if (_isStealthModeActive)
            {
                DisableStealthMode();
            }
            else
            {
                EnableStealthMode();
            }

            _lastToggleTime = Time.time;
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// ステルスモード切り替えの入力処理
        /// </summary>
        private void HandleStealthModeToggle()
        {
            // デフォルトでTab キーでステルスモード切り替え
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleStealthMode();
            }

            // 緊急ステルスモード（Shift+Tab で強制無効化）
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                DisableStealthMode();
            }
        }
        #endregion

        #region State Updates
        /// <summary>
        /// ステルス状態の更新
        /// </summary>
        private void UpdateStealthState()
        {
            if (!_isStealthModeActive || _stealthService == null) return;

            // 現在の状態がステルス関連の場合、追加の処理
            if (IsCurrentStateStealthRelated())
            {
                // ステルス状態特有の更新処理
                UpdateStealthMetrics();
                CheckStealthConditions();
            }
        }

        /// <summary>
        /// ステルス指標の更新
        /// </summary>
        private void UpdateStealthMetrics()
        {
            // ステルス統計の取得と更新
            var stats = _stealthService.GetStealthStatistics();

            // UI更新などの処理（将来実装）
            // UpdateStealthUI(stats);
        }

        /// <summary>
        /// ステルス条件のチェック
        /// </summary>
        private void CheckStealthConditions()
        {
            // 発見リスクの評価
            float visibilityFactor = _stealthService.PlayerVisibilityFactor;
            float noiseLevel = _stealthService.PlayerNoiseLevel;

            // 高リスク状態の警告
            if (visibilityFactor > 0.8f || noiseLevel > 0.7f)
            {
                if (_enableDebugLogs)
                    Debug.Log($"[StealthStateManager] High detection risk - Visibility: {visibilityFactor:F2}, Noise: {noiseLevel:F2}");
            }
        }
        #endregion

        #region UI Integration (Future Implementation)
        /// <summary>
        /// ステルスUIの表示
        /// </summary>
        private void ShowStealthUI()
        {
            // ステルス関連UIの表示処理
            // 将来的に実装予定
        }

        /// <summary>
        /// ステルスUIの非表示
        /// </summary>
        private void HideStealthUI()
        {
            // ステルス関連UIの非表示処理
            // 将来的に実装予定
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 強制的にステルス強化状態への遷移
        /// </summary>
        public void ForceStealthState(PlayerStateType stateType)
        {
            if (!_stealthStates.ContainsKey(stateType))
            {
                Debug.LogWarning($"[StealthStateManager] No stealth enhanced version for state: {stateType}");
                return;
            }

            EnableStealthMode();
            _playerStateMachine?.TransitionToState(stateType);
        }

        /// <summary>
        /// ステルス状態情報の取得
        /// </summary>
        public StealthStateInfo GetCurrentStealthInfo()
        {
            return new StealthStateInfo
            {
                IsStealthModeActive = _isStealthModeActive,
                UseStealthEnhancedStates = _useStealthEnhancedStates,
                CurrentStateIsStealthRelated = IsCurrentStateStealthRelated(),
                VisibilityFactor = _stealthService?.PlayerVisibilityFactor ?? 1.0f,
                NoiseLevel = _stealthService?.PlayerNoiseLevel ?? 0.0f,
                IsConcealed = _stealthService?.IsPlayerConcealed ?? false
            };
        }
        #endregion

        #region Debug
        private void OnGUI()
        {
            if (!_enableDebugLogs || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Stealth State Manager Debug ===");
            GUILayout.Label($"Stealth Mode: {_isStealthModeActive}");
            GUILayout.Label($"Enhanced States: {_useStealthEnhancedStates}");
            GUILayout.Label($"Current State Stealth Related: {IsCurrentStateStealthRelated()}");

            if (_stealthService != null)
            {
                GUILayout.Label($"Visibility: {_stealthService.PlayerVisibilityFactor:F2}");
                GUILayout.Label($"Noise Level: {_stealthService.PlayerNoiseLevel:F2}");
                GUILayout.Label($"Concealed: {_stealthService.IsPlayerConcealed}");
            }

            GUILayout.EndArea();
        }
        #endregion
    }

    /// <summary>
    /// ステルス状態情報構造体
    /// </summary>
    [System.Serializable]
    public struct StealthStateInfo
    {
        public bool IsStealthModeActive;
        public bool UseStealthEnhancedStates;
        public bool CurrentStateIsStealthRelated;
        public float VisibilityFactor;
        public float NoiseLevel;
        public bool IsConcealed;
    }
}