using UnityEngine;

namespace asterivo.Unity60.Player.States
{
    /// <summary>
    /// プレイヤーの待機状態を管理するクラス
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このクラスはStateパターンの具体的な状態（ConcreteState）として実装されています。
    /// プレイヤーが静止している際の基本状態であり、他の全ての状態への遷移の出発点となります。
    /// 
    /// 状態遷移条件：
    /// - 移動入力（moveInput.magnitude > 0.1f）→ Walking状態
    /// - ジャンプ入力（jumpInput == true）→ Jumping状態
    /// 
    /// 状態の特徴：
    /// - アイドル時間の計測（将来的なアニメーション制御や特殊動作のトリガーに使用可能）
    /// - Standing姿勢の設定（ステルス系との連携）
    /// - 最小入力閾値（0.1f）による誤入力フィルタリング
    /// 
    /// 使用例：
    /// ステートマシンがIdleStateに遷移すると、プレイヤーは立位姿勢で静止し、
    /// 入力待ち状態となります。適切な入力が検出されると他の状態に遷移します。
    /// </remarks>
    public class IdleState : IPlayerState
    {
        private float idleTime;
        
        /// <summary>
        /// 状態が開始されたときに呼び出されます。
        /// 待機時間をリセットし、プレイヤーの姿勢を立位に設定します。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            idleTime = 0f;
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        /// <summary>
        /// 状態が終了したときに呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }
        
        /// <summary>
        /// 毎フレーム呼び出されます。
        /// 待機時間をカウントアップします。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            idleTime += Time.deltaTime;
        }
        
        /// <summary>
        /// 固定フレームレートで呼び出されます。
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン。</param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// プレイヤーの入力を処理し、他の状態への遷移を判断します
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン</param>
        /// <param name="moveInput">移動入力（X軸：左右、Y軸：前後）</param>
        /// <param name="jumpInput">ジャンプ入力フラグ</param>
        /// <remarks>
        /// 遷移優先順位：
        /// 1. ジャンプ入力：最優先でJumping状態に遷移（return文で処理終了）
        /// 2. 移動入力：入力強度が閾値（0.1f）を超えた場合、Walking状態に遷移
        /// 
        /// 設計上の考慮事項：
        /// - ジャンプを移動より優先することで、移動しながらのジャンプでも確実にジャンプが実行される
        /// - 0.1fの閾値により、コントローラーのドリフトや微小な入力ノイズを除去
        /// - 状態遷移はTransitionToStateメソッドを通じて行い、適切なEnter/Exit処理を保証
        /// 
        /// 注意事項：
        /// - この状態では移動処理は行わず、状態遷移の判定のみ実行
        /// - 実際の移動処理は遷移先の状態（Walking等）で実装
        /// </remarks>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            if (jumpInput)
            {
                stateMachine.TransitionToState(PlayerStateType.Jumping);
                return;
            }

            if (moveInput.magnitude > 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateType.Walking);
            }
        }
    }
}
