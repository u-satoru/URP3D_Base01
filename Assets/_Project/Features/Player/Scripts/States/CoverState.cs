using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    public class CoverState : IPlayerState
    {
        private float coverMoveSpeed = 2f;
        private bool isPeeking = false;
        private Vector2 _moveInput;

        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.TryEnterCover();
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Cover);
            }
        }

        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.ExitCover();
            }
            isPeeking = false;
        }

        public void Update(DetailedPlayerStateMachine stateMachine) { }

        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null || stateMachine.CoverSystem == null) return;

            // カバー中の移動ロジック（覗き見中は移動制限）
            if (!isPeeking && Mathf.Abs(_moveInput.x) > 0.1f)
            {
                Transform transform = stateMachine.transform;
                Vector3 moveDirection = transform.right * _moveInput.x;
                moveDirection.y = 0;

                stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
            }
        }

        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // ジャンプ入力でカバー解除
            if (jumpInput)
            {
                // 後方への標準的なカバー解除
                var exitCoverDefinition = new Commands.ExitCoverCommandDefinition
                {
                    exitDirection = Commands.ExitDirection.Backward,
                    exitSpeedMultiplier = 1f
                };
                var exitCoverCommand = new Commands.ExitCoverCommand(stateMachine, exitCoverDefinition);
                exitCoverCommand.Execute();
                return;
            }
            
            // 左右への覗き見制御
            if (Input.GetKeyDown(KeyCode.Q)) // 左への覗き見開始
            {
                StartPeeking(stateMachine, Commands.PeekDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.E)) // 右への覗き見開始
            {
                StartPeeking(stateMachine, Commands.PeekDirection.Right);
            }
            else if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) // 覗き見終了
            {
                StopPeeking(stateMachine);
            }
            
            // 移動入力によるカバー解除（覗き見中は無効）
            if (!isPeeking && moveInput.magnitude > 0.5f)
            {
                // 移動方向に応じてカバー解除方向を決定
                var exitDirection = Commands.ExitDirection.Backward;
                if (moveInput.x > 0.5f) exitDirection = Commands.ExitDirection.Right;
                else if (moveInput.x < -0.5f) exitDirection = Commands.ExitDirection.Left;
                
                var exitCoverDefinition = new Commands.ExitCoverCommandDefinition
                {
                    exitDirection = exitDirection,
                    exitSpeedMultiplier = 1.2f
                };
                var exitCoverCommand = new Commands.ExitCoverCommand(stateMachine, exitCoverDefinition);
                exitCoverCommand.Execute();
            }
        }
        
        /// <summary>
        /// 覗き見を開始
        /// </summary>
        private void StartPeeking(DetailedPlayerStateMachine stateMachine, Commands.PeekDirection direction)
        {
            if (isPeeking) return; // 既に覗き見中の場合は無視
            
            var peekDefinition = new Commands.PeekCommandDefinition
            {
                peekDirection = direction,
                peekIntensity = 0.3f
            };
            var peekCommand = new Commands.PeekCommand(stateMachine, peekDefinition);
            peekCommand.Execute();
            
            isPeeking = true;
            Debug.Log($"Started peeking {direction}");
        }
        
        /// <summary>
        /// 覗き見を終了
        /// </summary>
        private void StopPeeking(DetailedPlayerStateMachine stateMachine)
        {
            if (!isPeeking) return; // 覗き見中でない場合は無視
            
            isPeeking = false;
            
            // 覗き見位置から元のカバー位置に戻る処理
            // 実際の実装では、PeekCommandのUndoを呼び出すか、
            // カバーシステムに元の位置に戻すよう指示する
            if (stateMachine.CoverSystem != null)
            {
                // CoverSystemに覗き見終了を通知（実装に応じて調整）
                Debug.Log("Stopped peeking - returning to cover position");
            }
        }
        
        /// <summary>
        /// 現在覗き見中かどうかを取得
        /// </summary>
        public bool IsPeeking => isPeeking;
    }
}
