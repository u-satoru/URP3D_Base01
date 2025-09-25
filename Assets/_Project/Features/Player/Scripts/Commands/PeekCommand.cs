using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Player.Commands
{
    /// <summary>
    /// 覗き見コマンド - カバー状態での覗き見動作を実行
    /// </summary>
    public class PeekCommand : ICommand
    {
        private readonly DetailedPlayerStateMachine _stateMachine;
        private readonly PeekCommandDefinition _definition;
        private Vector3 _originalPosition;
        private bool _isPeeking = false;

        public bool CanUndo => true; // 覗き見は元の位置に戻すことができる

        public PeekCommand(DetailedPlayerStateMachine stateMachine, PeekCommandDefinition definition)
        {
            _stateMachine = stateMachine;
            _definition = definition;
        }

        public void Execute()
        {
            if (_stateMachine.GetCurrentStateType() != PlayerStateType.InCover)
            {
                Debug.LogWarning("PeekCommand can only be executed while in cover");
                return;
            }
            
            // 現在の位置を保存
            _originalPosition = _stateMachine.transform.position;
            
            // 覗き見動作を実行
            PerformPeek();
            _isPeeking = true;
            
            Debug.Log($"Player peeking {_definition.peekDirection} with intensity {_definition.peekIntensity}");
        }

        public void Undo()
        {
            if (CanUndo && _isPeeking)
            {
                // 元の位置に戻す
                _stateMachine.transform.position = _originalPosition;
                _isPeeking = false;
                
                Debug.Log("Returned to original cover position");
            }
        }
        
        /// <summary>
        /// 覗き見動作を実行
        /// </summary>
        private void PerformPeek()
        {
            Vector3 peekOffset = CalculatePeekOffset();
            Vector3 targetPosition = _originalPosition + peekOffset * _definition.peekIntensity;
            
            // 覗き見位置への移動（即座に移動）
            // 実際のゲームでは、アニメーションや段階的な移動を実装することが多い
            _stateMachine.transform.position = targetPosition;
            
            // カメラ角度の調整やアニメーション呼び出しなどもここで行う
            AdjustCameraForPeek();
        }
        
        /// <summary>
        /// 覗き見方向に基づいてオフセットを計算
        /// </summary>
        private Vector3 CalculatePeekOffset()
        {
            float peekDistance = 0.5f; // 覗き見距離
            
            switch (_definition.peekDirection)
            {
                case PeekDirection.Left:
                    return _stateMachine.transform.right * -peekDistance;
                case PeekDirection.Right:
                    return _stateMachine.transform.right * peekDistance;
                case PeekDirection.Up:
                    return Vector3.up * peekDistance;
                case PeekDirection.Down:
                    return Vector3.down * (peekDistance * 0.5f); // 下方向は控えめに
                default:
                    return Vector3.zero;
            }
        }
        
        /// <summary>
        /// 覗き見時のカメラ調整
        /// </summary>
        private void AdjustCameraForPeek()
        {
            // カメラシステムとの統合
            // 実際の実装では、CameraStateMachineやCinemachineとの連携が必要
            
            // 覗き見イベントを発行してカメラシステムに通知
            // onPeekStarted?.Raise(new PeekEventData(_definition.peekDirection, _definition.peekIntensity));
        }
    }
}