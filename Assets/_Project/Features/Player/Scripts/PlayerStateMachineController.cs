using UnityEngine;
using Debug = UnityEngine.Debug;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Player;

namespace asterivo.Unity60.Player
{
    /// <summary>
    /// プレイヤーの状態管理を行うステートマシンコントローラー
    /// イベント駆動アーキテクチャによる高レベル状態管理
    /// </summary>
    public class PlayerStateMachineController : MonoBehaviour
    {
        [Header("State Configuration")]
        [SerializeField] private PlayerState currentState = PlayerState.Idle;
        [SerializeField] private PlayerState previousState = PlayerState.Idle;
        
        [Header("Event Channels")]
        [SerializeField] private PlayerStateEvent stateChangeRequestEvent;
        [SerializeField] private PlayerStateEvent stateChangedEvent;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;
        
        public PlayerState CurrentState => currentState;
        public PlayerState PreviousState => previousState;
        
        // 状態変更イベント
        public event System.Action<PlayerState, PlayerState> OnStateChanged;
        
        private void Awake()
        {
            // 初期化
            if (enableDebugLog)
                Debug.Log($"PlayerStateMachineController initialized with state: {currentState}");
        }
        
        // イベントリスナー
        private PlayerStateEventListener stateChangeListener;
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        private void OnDisable()
        {
            UnregisterEventListeners();
        }
        
        private void RegisterEventListeners()
        {
            // PlayerStateEventを使用してenum値で受信
            if (stateChangeRequestEvent != null)
            {
                // 既存のリスナーをチェック
                stateChangeListener = gameObject.GetComponent<PlayerStateEventListener>();
                if (stateChangeListener == null)
                {
                    stateChangeListener = gameObject.AddComponent<PlayerStateEventListener>();
                }
                stateChangeListener.GameEvent = stateChangeRequestEvent;
                stateChangeListener.Response.AddListener(OnStateChangeRequested);
            }
        }
        
        private void UnregisterEventListeners()
        {
            if (stateChangeListener != null)
            {
                stateChangeListener.Response.RemoveListener(OnStateChangeRequested);
                Destroy(stateChangeListener);
                stateChangeListener = null;
            }
        }
        
        /// <summary>
        /// 状態変更リクエストを受信
        /// </summary>
        /// <param name="newState">変更先の状態</param>
        private void OnStateChangeRequested(PlayerState newState)
        {
            ChangeState(newState);
        }
        
        /// <summary>
        /// 状態を変更する
        /// </summary>
        /// <param name="newState">新しい状態</param>
        public void ChangeState(PlayerState newState)
        {
            if (currentState == newState)
                return;
            
            PlayerState oldState = currentState;
            
            // 現在の状態を終了
            ExitState(currentState);
            
            // 状態を更新
            previousState = currentState;
            currentState = newState;
            
            // 新しい状態を開始
            EnterState(currentState);
            
            // イベント通知
            OnStateChanged?.Invoke(oldState, currentState);
            
            // グローバルイベント通知（enum値で送信）
            if (stateChangedEvent != null)
            {
                stateChangedEvent.Raise(currentState);
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"State changed: {oldState} -> {currentState}");
            }
        }
        
        /// <summary>
        /// 状態に入る際の処理
        /// </summary>
        /// <param name="state">入る状態</param>
        private void EnterState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle:
                    OnEnterIdle();
                    break;
                case PlayerState.Walking:
                    OnEnterWalking();
                    break;
                case PlayerState.Running:
                    OnEnterRunning();
                    break;
                case PlayerState.Sprinting:
                    OnEnterSprinting();
                    break;
                case PlayerState.Jumping:
                    OnEnterJumping();
                    break;
                case PlayerState.Falling:
                    OnEnterFalling();
                    break;
                case PlayerState.Landing:
                    OnEnterLanding();
                    break;
                case PlayerState.Combat:
                    OnEnterCombat();
                    break;
                case PlayerState.CombatAttacking:
                    OnEnterCombatAttacking();
                    break;
                case PlayerState.Interacting:
                    OnEnterInteracting();
                    break;
                case PlayerState.Dead:
                    OnEnterDead();
                    break;
            }
        }
        
        /// <summary>
        /// 状態から出る際の処理
        /// </summary>
        /// <param name="state">出る状態</param>
        private void ExitState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle:
                    OnExitIdle();
                    break;
                case PlayerState.Walking:
                    OnExitWalking();
                    break;
                case PlayerState.Running:
                    OnExitRunning();
                    break;
                case PlayerState.Sprinting:
                    OnExitSprinting();
                    break;
                case PlayerState.Jumping:
                    OnExitJumping();
                    break;
                case PlayerState.Falling:
                    OnExitFalling();
                    break;
                case PlayerState.Landing:
                    OnExitLanding();
                    break;
                case PlayerState.Combat:
                    OnExitCombat();
                    break;
                case PlayerState.CombatAttacking:
                    OnExitCombatAttacking();
                    break;
                case PlayerState.Interacting:
                    OnExitInteracting();
                    break;
                case PlayerState.Dead:
                    OnExitDead();
                    break;
            }
        }
        
        #region State Enter Methods
        
        private void OnEnterIdle()
        {
            if (enableDebugLog) Debug.Log("Entering Idle state");
        }
        
        private void OnEnterWalking()
        {
            if (enableDebugLog) Debug.Log("Entering Walking state");
        }
        
        private void OnEnterRunning()
        {
            if (enableDebugLog) Debug.Log("Entering Running state");
        }
        
        private void OnEnterSprinting()
        {
            if (enableDebugLog) Debug.Log("Entering Sprinting state");
        }
        
        private void OnEnterJumping()
        {
            if (enableDebugLog) Debug.Log("Entering Jumping state");
        }
        
        private void OnEnterFalling()
        {
            if (enableDebugLog) Debug.Log("Entering Falling state");
        }
        
        private void OnEnterLanding()
        {
            if (enableDebugLog) Debug.Log("Entering Landing state");
        }
        
        private void OnEnterCombat()
        {
            if (enableDebugLog) Debug.Log("Entering Combat state");
        }
        
        private void OnEnterCombatAttacking()
        {
            if (enableDebugLog) Debug.Log("Entering Combat Attacking state");
        }
        
        private void OnEnterInteracting()
        {
            if (enableDebugLog) Debug.Log("Entering Interacting state");
        }
        
        private void OnEnterDead()
        {
            if (enableDebugLog) Debug.Log("Entering Dead state");
        }
        
        #endregion
        
        #region State Exit Methods
        
        private void OnExitIdle()
        {
            if (enableDebugLog) Debug.Log("Exiting Idle state");
        }
        
        private void OnExitWalking()
        {
            if (enableDebugLog) Debug.Log("Exiting Walking state");
        }
        
        private void OnExitRunning()
        {
            if (enableDebugLog) Debug.Log("Exiting Running state");
        }
        
        private void OnExitSprinting()
        {
            if (enableDebugLog) Debug.Log("Exiting Sprinting state");
        }
        
        private void OnExitJumping()
        {
            if (enableDebugLog) Debug.Log("Exiting Jumping state");
        }
        
        private void OnExitFalling()
        {
            if (enableDebugLog) Debug.Log("Exiting Falling state");
        }
        
        private void OnExitLanding()
        {
            if (enableDebugLog) Debug.Log("Exiting Landing state");
        }
        
        private void OnExitCombat()
        {
            if (enableDebugLog) Debug.Log("Exiting Combat state");
        }
        
        private void OnExitCombatAttacking()
        {
            if (enableDebugLog) Debug.Log("Exiting Combat Attacking state");
        }
        
        private void OnExitInteracting()
        {
            if (enableDebugLog) Debug.Log("Exiting Interacting state");
        }
        
        private void OnExitDead()
        {
            if (enableDebugLog) Debug.Log("Exiting Dead state");
        }
        
        #endregion
        
        /// <summary>
        /// 状態が指定した状態かどうかを確認
        /// </summary>
        /// <param name="state">確認する状態</param>
        /// <returns>現在の状態が指定した状態と一致するかどうか</returns>
        public bool IsInState(PlayerState state)
        {
            return currentState == state;
        }
        
        /// <summary>
        /// 現在の状態が指定した状態のいずれかと一致するかを確認
        /// </summary>
        /// <param name="states">確認する状態の配列</param>
        /// <returns>現在の状態が指定した状態のいずれかと一致するかどうか</returns>
        public bool IsInAnyState(params PlayerState[] states)
        {
            foreach (var state in states)
            {
                if (currentState == state)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// 前の状態に戻る
        /// </summary>
        public void RevertToPreviousState()
        {
            ChangeState(previousState);
        }
        
        /// <summary>
        /// 状態遷移（BasePlayerStateから呼び出される）
        /// </summary>
        /// <param name="newState">遷移先の状態</param>
        public void TransitionTo(PlayerState newState)
        {
            ChangeState(newState);
        }
    }
}
