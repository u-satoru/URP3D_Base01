using System.Collections.Generic;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Patterns;
using asterivo.Unity60.Features.Player;
using UnityEngine;

namespace asterivo.Unity60.Features.StateManagement
{
    /// <summary>
    /// StateManagement機能のヘルパークラス
    /// 状態遷移の管理と実行を簡単に行うための便利クラス
    /// </summary>
    public class StateManager : MonoBehaviour, IStateContext
    {
        [SerializeField] private bool isDebugEnabled = false;
        [SerializeField] private PlayerState currentState = PlayerState.Idle;

        private IStateService stateService;
        private IStateHandler currentHandler;
        private readonly List<PlayerState> stateHistory = new List<PlayerState>();

        /// <summary>
        /// デバッグログが有効かどうか
        /// </summary>
        public bool IsDebugEnabled => isDebugEnabled;

        /// <summary>
        /// 現在の状態
        /// </summary>
        public PlayerState CurrentState => currentState;

        /// <summary>
        /// 状態履歴
        /// </summary>
        public IReadOnlyList<PlayerState> StateHistory => stateHistory;

        void Awake()
        {
            // StateServiceを取得またはBootstrapperで初期化
            if (!ServiceLocator.TryGet<IStateService>(out stateService))
            {
                Debug.LogWarning("[StateManager] StateService not found. Initializing StateManagement...");
                StateManagementBootstrapper.Initialize();
                stateService = ServiceLocator.Get<IStateService>();
            }
        }

        void Start()
        {
            // 初期状態の設定
            ChangeState(PlayerState.Idle);
        }

        /// <summary>
        /// 状態を変更する
        /// </summary>
        /// <param name="newState">新しい状態</param>
        public void ChangeState(PlayerState newState)
        {
            if (currentState == newState)
            {
                Log($"State is already {newState}. Skipping transition.");
                return;
            }

            // 現在の状態から退出
            if (currentHandler != null)
            {
                currentHandler.OnExit(this);
                currentHandler = null;
            }

            // 履歴に追加
            stateHistory.Add(currentState);
            if (stateHistory.Count > 10) // 最新10件のみ保持
            {
                stateHistory.RemoveAt(0);
            }

            // 新しい状態に遷移
            currentState = newState;
            currentHandler = stateService.GetHandler((int)newState);

            if (currentHandler != null)
            {
                currentHandler.OnEnter(this);
            }
            else
            {
                Log($"No handler found for state: {newState}");
            }
        }

        /// <summary>
        /// ログメッセージを出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        public void Log(string message)
        {
            if (isDebugEnabled)
            {
                Debug.Log($"[StateManager] {message}");
            }
        }

        /// <summary>
        /// 指定した状態に遷移可能かチェック
        /// </summary>
        /// <param name="targetState">チェックする状態</param>
        /// <returns>遷移可能な場合はtrue</returns>
        public bool CanTransitionTo(PlayerState targetState)
        {
            return stateService.HasHandler((int)targetState);
        }

        /// <summary>
        /// 前の状態に戻る
        /// </summary>
        public void RevertToPreviousState()
        {
            if (stateHistory.Count > 0)
            {
                var previousState = stateHistory[stateHistory.Count - 1];
                stateHistory.RemoveAt(stateHistory.Count - 1);
                ChangeState(previousState);
            }
            else
            {
                Log("No previous state in history");
            }
        }

        void OnDestroy()
        {
            // 状態から退出
            if (currentHandler != null)
            {
                currentHandler.OnExit(this);
                currentHandler = null;
            }
        }
    }
}