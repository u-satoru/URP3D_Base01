using UnityEngine;

namespace asterivo.Unity60.AI.States
{
    /// <summary>
    /// AIエージェントの待機状態を管理するクラス
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このクラスはAIエージェントが特別な目標や刺激がない時の基本的な待機状態を実装します。
    /// 単純な静止ではなく、環境への注意を払う自然な行動を演出することで、
    /// よりリアルなAIの挙動を実現します。
    /// 
    /// 主な機能：
    /// - 定期的な周囲確認（LookAround）
    /// - パトロールポイントが設定されている場合の自動遷移
    /// - 音響・視覚刺激への反応による状態遷移
    /// 
    /// 状態遷移条件：
    /// - 音レベル > 0.5f → Suspicious状態（音源調査）
    /// - 目標を視認 → 疑念値増加（将来的にAlert状態への遷移準備）
    /// - 5秒経過 + パトロールポイント存在 → Patrol状態
    /// 
    /// 行動パターン：
    /// - NavMeshAgentを停止して定位置で待機
    /// - 3秒間隔で±45度の範囲でランダムに周囲を見回し
    /// - 環境への警戒を維持しつつ、エネルギー消費を最小化
    /// 
    /// 使用例：
    /// 警備員が持ち場で待機している状況、見張りポストでの監視業務など
    /// </remarks>
    public class AIIdleState : IAIState
    {
        /// <summary>周囲確認の経過時間を追跡するタイマー</summary>
        private float idleTimer = 0f;
        
        /// <summary>周囲確認を行う間隔（秒）</summary>
        private float lookAroundInterval = 3f;
        
        /// <summary>
        /// Idle状態に遷移した際の初期化処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <remarks>
        /// 実行内容：
        /// - NavMeshAgentを停止して移動を止める
        /// - アイドルタイマーをリセット
        /// 
        /// この処理により、AIは現在位置で静止し、待機状態に入ります。
        /// </remarks>
        public void Enter(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = true;
            idleTimer = 0f;
        }
        
        /// <summary>
        /// Idle状態から他の状態に遷移する際の終了処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <remarks>
        /// 実行内容：
        /// - NavMeshAgentの停止状態を解除
        /// 
        /// この処理により、次の状態でAIが正常に移動できるようになります。
        /// 特に移動を伴う状態（Patrol、Chase等）への遷移時に重要です。
        /// </remarks>
        public void Exit(AIStateMachine stateMachine)
        {
            stateMachine.NavAgent.isStopped = false;
        }
        
        /// <summary>
        /// Idle状態の毎フレーム更新処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <remarks>
        /// 実行内容：
        /// 1. アイドルタイマーの更新と周囲確認の実行判定
        /// 2. パトロール状態への自動遷移判定
        /// 
        /// 処理の詳細：
        /// - 3秒間隔でLookAroundメソッドを呼び出し、自然な行動を演出
        /// - パトロールポイントが設定されている場合、5秒経過後にPatrol状態に遷移
        /// - これにより、待機とパトロールのバランスの取れたAI行動を実現
        /// 
        /// 設計意図：
        /// AIが完全に静的でなく、適度に動的な行動を取ることで、
        /// プレイヤーに発見されにくい自然な警備パターンを提供します。
        /// </remarks>
        public void Update(AIStateMachine stateMachine)
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= lookAroundInterval)
            {
                LookAround(stateMachine);
                idleTimer = 0f;
            }
            
            if (stateMachine.PatrolPoints != null && stateMachine.PatrolPoints.Length > 0)
            {
                if (stateMachine.StateTimer > 5f)
                {
                    stateMachine.TransitionToState(AIStateMachine.AIStateType.Patrol);
                }
            }
        }
        
        /// <summary>
        /// 物理的な接触イベントの処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <param name="other">接触したCollider</param>
        /// <remarks>
        /// 現在の実装では特別な処理は行いませんが、将来的な拡張に備えて定義されています。
        /// 
        /// 拡張例：
        /// - プレイヤーとの接触による即座の警戒状態遷移
        /// - アイテムや環境オブジェクトとの相互作用
        /// - 特定のトリガーゾーンでの行動変更
        /// </remarks>
        public void OnTriggerEnter(AIStateMachine stateMachine, Collider other)
        {
        }
        
        /// <summary>
        /// 目標を視認した際の処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <param name="target">発見された目標</param>
        /// <remarks>
        /// 実行内容：
        /// 1. 目標をステートマシンに記録
        /// 2. 疑念値を1.0増加
        /// 
        /// 設計意図：
        /// Idle状態では即座にAlert状態に遷移せず、疑念値の蓄積により段階的な反応を実現。
        /// これにより、プレイヤーが一瞬だけ見つかった場合でも即座に戦闘になることを避け、
        /// ステルスゲームとしての緊張感を維持します。
        /// 
        /// 疑念システム：
        /// - 疑念値が閾値を超えると、より高い警戒状態に遷移
        /// - 時間経過により疑念値は自然減少
        /// - 複数回の目撃により確実に発見状態に移行
        /// </remarks>
        public void OnSightTarget(AIStateMachine stateMachine, Transform target)
        {
            stateMachine.SetTarget(target);
            stateMachine.IncreaseSuspicion(1f);
        }
        
        /// <summary>
        /// 音を検知した際の処理
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <param name="noisePosition">音の発生位置</param>
        /// <param name="noiseLevel">音の強度レベル</param>
        /// <remarks>
        /// 実行内容：
        /// - 音レベルが0.5fを超える場合、Suspicious状態に遷移
        /// - 音の発生位置を最後の既知位置として記録
        /// 
        /// 音レベルの判定基準：
        /// - ≤ 0.5f: 軽微な音（無視）
        /// - > 0.5f: 調査が必要な音（疑わしい状態へ遷移）
        /// 
        /// ステルスゲームプレイへの影響：
        /// プレイヤーの足音や物音が一定以上の大きさの場合、
        /// AIが音源を調査しに向かうため、プレイヤーは音に注意を払う必要があります。
        /// これにより、移動ルートや移動方法の戦略性が向上します。
        /// </remarks>
        public void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel)
        {
            if (noiseLevel > 0.5f)
            {
                stateMachine.LastKnownPosition = noisePosition;
                stateMachine.TransitionToState(AIStateMachine.AIStateType.Suspicious);
            }
        }
        
        /// <summary>
        /// 周囲を見回すアニメーション動作
        /// </summary>
        /// <param name="stateMachine">AIステートマシン</param>
        /// <remarks>
        /// 実行内容：
        /// - 現在の向きから±45度の範囲でランダムな方向を選択
        /// - その方向にAIの向きを変更
        /// 
        /// 設計意図：
        /// 1. 自然な警備行動の演出
        /// 2. プレイヤーの発見率向上（異なる角度での視線判定）
        /// 3. 静的すぎるAI行動の改善
        /// 
        /// 角度制限の理由：
        /// - 90度を超える急激な回転は不自然
        /// - ±45度により現実的な「見回し」動作を再現
        /// - プレイヤーにとって予測可能な範囲での動作
        /// 
        /// パフォーマンス考慮：
        /// - 3秒間隔での実行により、過度な処理負荷を回避
        /// - 単純な回転のみで、複雑な移動は行わない
        /// </remarks>
        private void LookAround(AIStateMachine stateMachine)
        {
            float randomAngle = Random.Range(-45f, 45f);
            Vector3 lookDirection = Quaternion.Euler(0, randomAngle, 0) * stateMachine.transform.forward;
            stateMachine.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}