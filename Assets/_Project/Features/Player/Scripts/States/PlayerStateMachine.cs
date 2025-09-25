using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Player;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの状態管理を行うステートマシン（シンプル版）
    /// DetailedPlayerStateMachine への軽量ラッパーとして機能し、
    /// レガシーコードとの互換性とシンプルなAPIを提供します。
    /// 
    /// アーキテクチャ設計:
    /// - Dictionary&lt;PlayerStateType, IPlayerState&gt; による高速状態管理
    /// - Command System との統合
    /// - Event-Driven Architecture 対応
    /// - レガシーPlayerState enumとの完全互換性
    /// </summary>
    public class PlayerStateMachine : MonoBehaviour
    {
        #region Private Fields
        
        /// <summary>
        /// 詳細ステートマシンへの参照（実際の処理を委譲）
        /// </summary>
        private DetailedPlayerStateMachine detailedStateMachine;
        
        /// <summary>
        /// 状態変更イベント
        /// </summary>
        [SerializeField] private PlayerStateEvent onStateChanged;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 現在の状態タイプを取得します
        /// </summary>
        public PlayerStateType CurrentState => detailedStateMachine?.GetCurrentStateType() ?? PlayerStateType.Idle;
        
        /// <summary>
        /// 前の状態タイプを取得します
        /// </summary>
        public PlayerStateType PreviousState => detailedStateMachine?.GetPreviousStateType() ?? PlayerStateType.Idle;
        
        /// <summary>
        /// レガシー互換: 現在の状態をPlayerState enumで取得
        /// </summary>
        public PlayerState LegacyCurrentState => detailedStateMachine?.GetLegacyCurrentState() ?? PlayerState.Idle;
        
        /// <summary>
        /// レガシー互換: 前の状態をPlayerState enumで取得
        /// </summary>
        public PlayerState LegacyPreviousState => detailedStateMachine?.GetLegacyPreviousState() ?? PlayerState.Idle;
        
        /// <summary>
        /// ステートマシンが有効かどうか
        /// </summary>
        public bool IsEnabled => detailedStateMachine != null && detailedStateMachine.enabled;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // DetailedPlayerStateMachineコンポーネントを取得または追加
            detailedStateMachine = GetComponent<DetailedPlayerStateMachine>();
            if (detailedStateMachine == null)
            {
                detailedStateMachine = gameObject.AddComponent<DetailedPlayerStateMachine>();
            }
        }
        
        private void Start()
        {
            // イベント初期化
            if (onStateChanged != null)
            {
                // 初期状態の通知
                onStateChanged.Raise((int)LegacyCurrentState);
            }
        }
        
        #endregion
        
        #region Public Methods - State Transition
        
        /// <summary>
        /// 指定した状態に遷移します
        /// </summary>
        /// <param name="newState">遷移先の状態</param>
        public void TransitionToState(PlayerStateType newState)
        {
            if (detailedStateMachine == null) return;
            
            PlayerStateType previousState = detailedStateMachine.GetCurrentStateType();
            detailedStateMachine.TransitionToState(newState);
            
            // イベント通知
            if (onStateChanged != null && previousState != newState)
            {
                onStateChanged.Raise((int)detailedStateMachine.GetLegacyCurrentState());
            }
        }
        
        /// <summary>
        /// レガシー互換: PlayerState enumで状態遷移
        /// </summary>
        /// <param name="newState">遷移先のレガシー状態</param>
        public void TransitionToLegacyState(PlayerState newState)
        {
            if (detailedStateMachine == null) return;
            
            detailedStateMachine.TransitionToLegacyState(newState);
            
            // イベント通知
            if (onStateChanged != null)
            {
                onStateChanged.Raise((int)newState);
            }
        }
        
        /// <summary>
        /// 前の状態に戻ります
        /// </summary>
        public void RevertToPreviousState()
        {
            if (detailedStateMachine == null) return;
            
            detailedStateMachine.RevertToLegacyPreviousState();
            
            // イベント通知
            if (onStateChanged != null)
            {
                onStateChanged.Raise((int)detailedStateMachine.GetLegacyCurrentState());
            }
        }
        
        #endregion
        
        #region Public Methods - State Query
        
        /// <summary>
        /// 指定した状態かどうかを確認します
        /// </summary>
        /// <param name="state">確認する状態</param>
        /// <returns>現在の状態が指定した状態と一致するかどうか</returns>
        public bool IsInState(PlayerStateType state)
        {
            return detailedStateMachine?.GetCurrentStateType() == state;
        }
        
        /// <summary>
        /// レガシー互換: 指定したレガシー状態かどうかを確認
        /// </summary>
        /// <param name="state">確認するレガシー状態</param>
        /// <returns>現在の状態が指定した状態と一致するかどうか</returns>
        public bool IsInLegacyState(PlayerState state)
        {
            return detailedStateMachine?.IsInLegacyState(state) ?? false;
        }
        
        /// <summary>
        /// 複数の状態のいずれかに該当するかを確認します
        /// </summary>
        /// <param name="states">確認する状態の配列</param>
        /// <returns>現在の状態が指定した状態のいずれかと一致するかどうか</returns>
        public bool IsInAnyState(params PlayerStateType[] states)
        {
            if (detailedStateMachine == null) return false;
            
            PlayerStateType currentState = detailedStateMachine.GetCurrentStateType();
            foreach (var state in states)
            {
                if (currentState == state)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// レガシー互換: 複数のレガシー状態のいずれかに該当するかを確認
        /// </summary>
        /// <param name="states">確認するレガシー状態の配列</param>
        /// <returns>現在の状態が指定した状態のいずれかと一致するかどうか</returns>
        public bool IsInAnyLegacyState(params PlayerState[] states)
        {
            return detailedStateMachine?.IsInAnyLegacyState(states) ?? false;
        }
        
        #endregion
        
        #region Public Methods - Input Handling
        
        /// <summary>
        /// プレイヤー入力を処理します（簡易版）
        /// </summary>
        /// <param name="moveInput">移動入力</param>
        /// <param name="jumpInput">ジャンプ入力</param>
        public void HandleInput(Vector2 moveInput, bool jumpInput)
        {
            if (detailedStateMachine == null) return;
            
            detailedStateMachine.HandleInput(moveInput, jumpInput);
        }
        
        #endregion
        
        #region Public Methods - System Integration
        
        /// <summary>
        /// ステートマシンを有効化します
        /// </summary>
        public void EnableStateMachine()
        {
            if (detailedStateMachine != null)
            {
                detailedStateMachine.enabled = true;
            }
        }
        
        /// <summary>
        /// ステートマシンを無効化します
        /// </summary>
        public void DisableStateMachine()
        {
            if (detailedStateMachine != null)
            {
                detailedStateMachine.enabled = false;
            }
        }
        
        /// <summary>
        /// 詳細ステートマシンへの直接アクセス（高度な用途向け）
        /// </summary>
        /// <returns>DetailedPlayerStateMachineインスタンス</returns>
        public DetailedPlayerStateMachine GetDetailedStateMachine()
        {
            return detailedStateMachine;
        }
        
        #endregion
        
        #region Debug Support
        
#if UNITY_EDITOR
        /// <summary>
        /// デバッグ情報の表示（エディタ専用）
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void LogCurrentState()
        {
            if (detailedStateMachine != null)
            {
                Debug.Log($"PlayerStateMachine - Current: {CurrentState}, Previous: {PreviousState}, Legacy: {LegacyCurrentState}");
            }
            else
            {
                Debug.LogWarning("PlayerStateMachine - DetailedPlayerStateMachine is not available");
            }
        }
#endif
        
        #endregion
    }
}