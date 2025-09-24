using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Player.Commands
{
    /// <summary>
    /// プレイヤーをカバー状態から離脱させるコマンドです。
    /// </summary>
    public class ExitCoverCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly ExitCoverCommandDefinition _definition;
        private Vector3 _originalPosition;

        /// <summary>
        /// このコマンドが取り消し可能かどうかを示します。
        /// カバー解除は通常、新しいカバーに入ることでしか「元に戻せない」ため、falseを返します。
        /// </summary>
        public bool CanUndo => false;

        /// <summary>
        /// ExitCoverCommandの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        /// <param name="definition">カバー解除の動作を定義するScriptableObject。</param>
        public ExitCoverCommand(DetailedPlayerStateMachine stateMachine, ExitCoverCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _definition = definition;
        }

        /// <summary>
        /// コマンドを実行し、カバー解除プロセスを開始します。
        /// </summary>
        public void Execute()
        {
            if (_stateMachine.GetCurrentStateType() != PlayerStateType.InCover)
            {
                Debug.LogWarning("ExitCoverCommand can only be executed while in cover");
                return;
            }
            
            _originalPosition = _stateMachine.transform.position;
            
            PerformExitCover();
            
            Debug.Log($"Player exited cover in direction {_definition.exitDirection}");
        }

        /// <summary>
        /// コマンドの取り消し処理。このコマンドではサポートされていません。
        /// </summary>
        public void Undo()
        {
            Debug.LogWarning("ExitCoverCommand cannot be undone. Player must find new cover.");
        }
        
        /// <summary>
        /// カバー解除の具体的な動作（移動と状態遷移）を実行します。
        /// </summary>
        private void PerformExitCover()
        {
            Vector3 exitDirection = CalculateExitDirection();
            Vector3 targetPosition = _originalPosition + exitDirection;
            
            _stateMachine.transform.position = targetPosition;
            
            TransitionToExitState();
        }
        
        /// <summary>
        /// 定義に基づいて、カバーからの離脱方向と距離を計算します。
        /// </summary>
        /// <returns>計算された移動ベクトル。</returns>
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
                    // ローリングは前方への移動として計算
                    return _stateMachine.transform.forward * exitDistance * 0.5f;
                default:
                    return -_stateMachine.transform.forward * exitDistance;
            }
        }
        
        /// <summary>
        /// カバー解除後のプレイヤーの状態を決定し、遷移させます。
        /// </summary>
        private void TransitionToExitState()
        {
            switch (_definition.exitDirection)
            {
                case ExitDirection.Roll:
                    // ローリング状態へ遷移
                    _stateMachine.TransitionToState(PlayerStateType.Rolling);
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