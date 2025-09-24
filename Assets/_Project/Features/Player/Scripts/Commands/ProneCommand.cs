using UnityEngine;
using Debug = UnityEngine.Debug;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Player.Commands
{
    /// <summary>
    /// 匍匐コマンド - プレイヤーの匍匐姿勢変更を実行
    /// </summary>
    public class ProneCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly ProneCommandDefinition _definition;
        private PlayerStateType _previousState;

        public bool CanUndo => true; // 匍匐状態は元に戻すことができる

        public ProneCommand(DetailedPlayerStateMachine stateMachine, ProneCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _definition = definition;
        }

        public void Execute()
        {
            // 現在の状態を保存（Undo用）
            _previousState = _stateMachine.GetCurrentStateType();
            
            if (_definition.toProneState)
            {
                // 匍匐状態に遷移
                _stateMachine.TransitionToState(PlayerStateType.Prone);
                Debug.Log("Player went prone");
            }
            else
            {
                // 立ち上がり状態に遷移
                TransitionFromProneState();
                Debug.Log("Player got up from prone");
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                // 前の状態に戻す
                _stateMachine.TransitionToState(_previousState);
                Debug.Log($"Undoing prone command - returned to {_previousState}");
            }
        }
        
        /// <summary>
        /// 匍匐状態からの適切な遷移状態を決定
        /// </summary>
        private void TransitionFromProneState()
        {
            // 匍匐から立ち上がる場合、通常はしゃがみ状態を経由するのが現実的
            // ただし、プレイヤーの入力や周囲の状況に応じて決定
            
            var playerController = _stateMachine.GetComponent<PlayerController>();
            
            // 周囲に障害物がある場合はしゃがみ状態へ、そうでなければ立ち姿勢へ
            if (CanStandUpDirectly())
            {
                _stateMachine.TransitionToState(PlayerStateType.Idle);
            }
            else
            {
                // 頭上に障害物がある場合はしゃがみ状態へ遷移
                _stateMachine.TransitionToState(PlayerStateType.Crouching);
            }
        }
        
        /// <summary>
        /// 直接立ち上がることができるかチェック
        /// </summary>
        private bool CanStandUpDirectly()
        {
            // 頭上の障害物チェック（簡易実装）
            RaycastHit hit;
            Vector3 origin = _stateMachine.transform.position + Vector3.up * 0.5f; // 腰の高さ
            
            // 立った時の頭の高さまでレイキャスト
            if (Physics.Raycast(origin, Vector3.up, out hit, 1.3f)) // 約1.8m - 0.5m = 1.3m
            {
                // 障害物がある場合は直接立ち上がれない
                return false;
            }
            
            return true;
        }
    }
}
