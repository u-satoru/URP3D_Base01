using UnityEngine;

namespace asterivo.Unity60.Features.AI.States
{
    /// <summary>
    /// AIエージェントの各状態が実装すべきインターフェース
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このインターフェースはStateパターンの抽象状態（State）として機能し、
    /// AIエージェントの行動をモジュラー化します。各具体的な状態（Idle、Patrol、Alert等）は
    /// このインターフェースを実装することで、統一されたAI行動システムに組み込まれます。
    /// 
    /// 特徴的な機能：
    /// - 基本的な状態ライフサイクル（Enter、Exit、Update）
    /// - 物理的な接触検出（OnTriggerEnter）
    /// - 視覚による目標検出（OnSightTarget）
    /// - 聴覚による音響検出（OnHearNoise）
    /// 
    /// プレイヤー用IPlayerStateとの違い：
    /// - プレイヤーは明示的な入力処理（HandleInput）を持つのに対し、AIは感覚入力に反応
    /// - FixedUpdateは含まれず、Update内で物理計算も処理
    /// - AIの知覚システム（視覚・聴覚）との統合が前提
    /// 
    /// 実装する状態例：
    /// - AIIdleState: 待機状態
    /// - AIPatrolState: 巡回状態
    /// - AIAlertState: 警戒状態
    /// - AICombatState: 戦闘状態
    /// - AISearchingState: 捜索状態
    /// </remarks>
    public interface IAIState
    {
        /// <summary>
        /// 状態に遷移した際に一度だけ呼び出される初期化処理
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <remarks>
        /// 処理内容例：
        /// - 状態固有の変数の初期化
        /// - アニメーションの設定
        /// - 移動目標の設定
        /// - タイマーのリセット
        /// </remarks>
        void Enter(AIStateMachine stateMachine);
        
        /// <summary>
        /// 状態から他の状態に遷移する際に一度だけ呼び出される終了処理
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <remarks>
        /// 処理内容例：
        /// - 実行中の処理のクリーンアップ
        /// - 一時的なフラグのリセット
        /// - リソースの解放
        /// - 遷移先状態への情報引き継ぎ
        /// </remarks>
        void Exit(AIStateMachine stateMachine);
        
        /// <summary>
        /// 状態がアクティブな間、毎フレーム呼び出される更新処理
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <remarks>
        /// 処理内容例：
        /// - 状態固有のロジック実行
        /// - 移動処理
        /// - 状態遷移条件の評価
        /// - タイマー更新
        /// - アニメーション更新
        /// </remarks>
        void Update(AIStateMachine stateMachine);
        
        /// <summary>
        /// AIエージェントのTriggerColliderが他のColliderに接触した際に呼び出される
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <param name="other">接触したCollider</param>
        /// <remarks>
        /// 用途例：
        /// - プレイヤーとの接触検出
        /// - アイテムやトリガーゾーンとの相互作用
        /// - 環境オブジェクトとの接触による状態変化
        /// 
        /// 注意事項：
        /// - IsTriggerがtrueのColliderでのみ機能
        /// - 物理的な移動処理は別途Update内で実装
        /// </remarks>
        void OnTriggerEnter(AIStateMachine stateMachine, Collider other);
        
        /// <summary>
        /// AIの視覚システムが目標を発見した際に呼び出される
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <param name="target">発見された目標のTransform</param>
        /// <remarks>
        /// 処理内容例：
        /// - 警戒状態への遷移
        /// - 目標の記録と追跡開始
        /// - 他のAIへのアラート通知
        /// - 戦闘状態への準備
        /// 
        /// 連携システム：
        /// - VisibilityCalculatorによる視線判定
        /// - DetectionConfigurationによる検出設定
        /// - ステルスシステムとの統合
        /// </remarks>
        void OnSightTarget(AIStateMachine stateMachine, Transform target);
        
        /// <summary>
        /// AIの聴覚システムが音を検知した際に呼び出される
        /// </summary>
        /// <param name="stateMachine">この状態を管理するAIステートマシン</param>
        /// <param name="noisePosition">音の発生位置</param>
        /// <param name="noiseLevel">音のレベル（大きさ・重要度）</param>
        /// <remarks>
        /// 処理内容例：
        /// - 音源位置への注意・移動
        /// - 警戒レベルの上昇
        /// - 捜索状態への遷移
        /// - 音の種類に応じた反応の分岐
        /// 
        /// 音レベルの一般的な基準：
        /// - 0.1f以下: 環境音（風、水など）→ 通常は無視
        /// - 0.1f-0.5f: 軽微な音（草を踏む、小物を落とす）→ 軽い注意
        /// - 0.5f-1.0f: 明確な音（足音、扉の音）→ 捜索開始
        /// - 1.0f以上: 大きな音（戦闘音、爆発音）→ 即座に警戒
        /// 
        /// 連携システム：
        /// - NPCAuditorySensorによる3D音響検出
        /// - PlayerAudioSystemからの音響イベント
        /// - 環境の音響マスキング機能
        /// </remarks>
        void OnHearNoise(AIStateMachine stateMachine, Vector3 noisePosition, float noiseLevel);
    }
}
