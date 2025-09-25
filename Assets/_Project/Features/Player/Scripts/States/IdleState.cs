using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// プレイヤーの征E��状態を管琁E��るクラス
    /// </summary>
    /// <remarks>
    /// 設計思想�E�E    /// こ�EクラスはStateパターンの具体的な状態！EoncreteState�E�として実裁E��れてぁE��す、E    /// プレイヤーが静止してぁE��際�E基本状態であり、他�E全ての状態への遷移の出発点となります、E    /// 
    /// 状態�E移条件�E�E    /// - 移動�E力！EoveInput.magnitude > 0.1f�E��E Walking状慁E    /// - ジャンプ�E力！EumpInput == true�E��E Jumping状慁E    /// 
    /// 状態�E特徴�E�E    /// - アイドル時間の計測�E�封E��皁E��アニメーション制御めE��殊動作�Eトリガーに使用可能�E�E    /// - Standing姿勢の設定（スチE��ス系との連携�E�E    /// - 最小�E力閾値�E�E.1f�E�による誤入力フィルタリング
    /// 
    /// 使用例！E    /// スチE�Eト�EシンがIdleStateに遷移すると、�Eレイヤーは立位姿勢で静止し、E    /// 入力征E��状態となります。適刁E��入力が検�Eされると他�E状態に遷移します、E    /// </remarks>
    public class IdleState : IPlayerState
    {
        private float idleTime;
        
        /// <summary>
        /// 状態が開始されたときに呼び出されます、E        /// 征E��時間をリセチE��し、�Eレイヤーの姿勢を立位に設定します、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            idleTime = 0f;
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        /// <summary>
        /// 状態が終亁E��たときに呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }
        
        /// <summary>
        /// 毎フレーム呼び出されます、E        /// 征E��時間をカウントアチE�Eします、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            idleTime += Time.deltaTime;
        }
        
        /// <summary>
        /// 固定フレームレートで呼び出されます、E        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン、E/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// プレイヤーの入力を処琁E��、他�E状態への遷移を判断しまぁE        /// </summary>
        /// <param name="stateMachine">プレイヤーのスチE�Eト�Eシン</param>
        /// <param name="moveInput">移動�E力！E軸�E�左右、Y軸�E�前後！E/param>
        /// <param name="jumpInput">ジャンプ�E力フラグ</param>
        /// <remarks>
        /// 遷移優先頁E��！E        /// 1. ジャンプ�E力：最優先でJumping状態に遷移�E�Eeturn斁E��処琁E��亁E��E        /// 2. 移動�E力：�E力強度が閾値�E�E.1f�E�を趁E��た場合、Walking状態に遷移
        /// 
        /// 設計上�E老E�E事頁E��E        /// - ジャンプを移動より優先することで、移動しながらのジャンプでも確実にジャンプが実行される
        /// - 0.1fの閾値により、コントローラーのドリフトめE��小な入力ノイズを除去
        /// - 状態�E移はTransitionToStateメソチE��を通じて行い、E��刁E��Enter/Exit処琁E��保証
        /// 
        /// 注意事頁E��E        /// - こ�E状態では移動�E琁E�E行わず、状態�E移の判定�Eみ実衁E        /// - 実際の移動�E琁E�E遷移先�E状態！Ealking等）で実裁E        /// </remarks>
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
