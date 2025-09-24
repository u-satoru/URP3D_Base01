using UnityEngine;
using Debug = UnityEngine.Debug;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Commands.Definitions;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Player.Commands
{
    /// <summary>
    /// しゃがみコマンド - プレイヤーの姿勢変更を実行
    /// </summary>
    public class CrouchCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly CrouchCommandDefinition _definition;
        private PlayerStateType _previousState;

        public bool CanUndo => true; // しゃがみ状態は元に戻すことができる

        public CrouchCommand(DetailedPlayerStateMachine stateMachine, CrouchCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _definition = definition;
        }

        public void Execute()
        {
            // 現在の状態を保存（Undo用）
            _previousState = _stateMachine.GetCurrentStateType();
            
            if (_definition.toggleMode)
            {
                // トグルモードの場合、現在の状態に応じて切り替え
                if (_stateMachine.GetCurrentStateType() == PlayerStateType.Crouching)
                {
                    _stateMachine.TransitionToState(PlayerStateType.Idle);
                    Debug.Log("Player stood up");
                }
                else
                {
                    _stateMachine.TransitionToState(PlayerStateType.Crouching);
                    Debug.Log("Player crouched");
                }
            }
            else
            {
                // 立ち上がり状態に遷移（コンテキストに応じてIdle/Walking）
                TransitionToStandingState();
                Debug.Log("Player stood up");
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                // 前の状態に戻す
                _stateMachine.TransitionToState(_previousState);
                Debug.Log($"Undoing crouch command - returned to {_previousState}");
            }
        }
        
        /// <summary>
        /// 立ち上がり時の適切な状態を決定
        /// </summary>
        private void TransitionToStandingState()
        {
            // PlayerControllerから現在の入力状態を取得して適切な状態に遷移
            var playerController = _stateMachine.GetComponent<PlayerController>();
            
            // 現在移動入力があるかチェック（簡易実装）
            // DetailedPlayerStateMachineに統合されたため、直接状態遷移を実行
            if (playerController != null)
            {
                // 現在の移動状態に基づいて決定（実装に応じて調整）
                _stateMachine.TransitionToState(PlayerStateType.Idle);
            }
            else
            {
                // フォールバック：アイドル状態に遷移
                _stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }
    }
}