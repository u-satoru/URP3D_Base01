using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Player.States;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// カバー解除コマンド - カバー状態からの離脱を実行
    /// </summary>
    public class ExitCoverCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly ExitCoverCommandDefinition _definition;
        private Vector3 _originalPosition;

        public bool CanUndo => false; // カバー解除は通常元に戻せない（新しいカバーを取る必要がある）

        public ExitCoverCommand(DetailedPlayerStateMachine stateMachine, ExitCoverCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _definition = definition;
        }

        public void Execute()
        {
            if (_stateMachine.GetCurrentStateType() != PlayerStateType.InCover)
            {
                Debug.LogWarning("ExitCoverCommand can only be executed while in cover");
                return;
            }
            
            // 現在の位置を保存
            _originalPosition = _stateMachine.transform.position;
            
            // カバー解除動作を実行
            PerformExitCover();
            
            Debug.Log($"Player exited cover in direction {_definition.exitDirection}");
        }

        public void Undo()
        {
            // カバー解除は通常Undoできない
            Debug.LogWarning("ExitCoverCommand cannot be undone. Player must find new cover.");
        }
        
        /// <summary>
        /// カバー解除動作を実行
        /// </summary>
        private void PerformExitCover()
        {
            // 解除方向に基づいて移動
            Vector3 exitDirection = CalculateExitDirection();
            Vector3 targetPosition = _originalPosition + exitDirection;
            
            // 解除位置への移動
            _stateMachine.transform.position = targetPosition;
            
            // 適切な状態に遷移
            TransitionToExitState();
        }
        
        /// <summary>
        /// 解除方向を計算
        /// </summary>
        private Vector3 CalculateExitDirection()
        {
            float exitDistance = 1f * _definition.exitSpeedMultiplier;
            
            switch (_definition.exitDirection)
            {
                case ExitDirection.Backward:
                    return -_stateMachine.transform.forward * exitDistance;
                case ExitDirection.Left:
                    return -_stateMachine.transform.right * exitDistance;
                case ExitDirection.Right:
                    return _stateMachine.transform.right * exitDistance;
                case ExitDirection.Roll:
                    // ローリングは前方への移動 + 特殊なアニメーション
                    return _stateMachine.transform.forward * exitDistance * 0.5f;
                default:
                    return -_stateMachine.transform.forward * exitDistance;
            }
        }
        
        /// <summary>
        /// カバー解除後の状態遷移
        /// </summary>
        private void TransitionToExitState()
        {
            // 解除方向に基づいて適切な状態に遷移
            switch (_definition.exitDirection)
            {
                case ExitDirection.Roll:
                    // ローリング状態があれば遷移（今回は未実装なのでWalkingに）
                    _stateMachine.TransitionToState(PlayerStateType.Walking);
                    break;
                    
                case ExitDirection.Backward:
                case ExitDirection.Left:
                case ExitDirection.Right:
                default:
                    // 通常の移動状態に遷移
                    _stateMachine.TransitionToState(PlayerStateType.Walking);
                    break;
            }
        }
    }
}