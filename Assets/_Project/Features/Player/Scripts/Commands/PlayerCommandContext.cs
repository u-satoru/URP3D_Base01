using UnityEngine;
using asterivo.Unity60.Player.States;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// プレイヤーコマンド実行時に必要なコンテクスト情報を提供するクラス
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このクラスはCommandパターンにおけるコンテクスト（Context）オブジェクトとして機能します。
    /// コマンドの実行に必要なプレイヤー関連の参照を一箇所に集約することで、
    /// 各コマンドクラスの依存関係を明確にし、テスタビリティを向上させます。
    /// 
    /// 提供する機能：
    /// - ステートマシンへの参照（状態遷移制御）
    /// - Transformコンポーネント（位置・回転・スケール操作）
    /// - CharacterControllerコンポーネント（移動・衝突検出）
    /// 
    /// 使用パターン：
    /// 1. 自動構築パターン：ステートマシンから必要な参照を自動取得
    /// 2. 明示指定パターン：個別のコンポーネント参照を明示的に設定
    /// 
    /// 利点：
    /// - コマンドクラスの単一責任原則の維持
    /// - テスト時の依存関係のモック化が容易
    /// - 将来的な拡張時の影響範囲の限定化
    /// </remarks>
    public class PlayerCommandContext
    {
        /// <summary>
        /// プレイヤーの状態管理を行うステートマシンへの参照
        /// </summary>
        /// <value>プレイヤーの現在状態と状態遷移を制御するステートマシン</value>
        public DetailedPlayerStateMachine StateMachine { get; }
        
        /// <summary>
        /// プレイヤーオブジェクトのTransformコンポーネントへの参照
        /// </summary>
        /// <value>位置、回転、スケールの制御に使用するTransform</value>
        public Transform Transform { get; }
        
        /// <summary>
        /// プレイヤーの移動制御を行うCharacterControllerへの参照
        /// </summary>
        /// <value>移動、重力適用、衝突検出に使用するCharacterController</value>
        public CharacterController CharacterController { get; }
        
        /// <summary>
        /// ステートマシンから自動的にコンポーネント参照を取得してコンテクストを構築します
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン</param>
        /// <remarks>
        /// 推奨される標準的な構築方法です。
        /// TransformとCharacterControllerはステートマシンから自動取得されます。
        /// 
        /// 注意事項：
        /// - stateMachineがnullの場合、TransformとCharacterControllerもnullになります
        /// - CharacterControllerコンポーネントが存在しない場合はnullが設定されます
        /// </remarks>
        public PlayerCommandContext(DetailedPlayerStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            Transform = stateMachine?.transform;
            CharacterController = stateMachine?.GetComponent<CharacterController>();
        }
        
        /// <summary>
        /// 各コンポーネント参照を明示的に指定してコンテクストを構築します
        /// </summary>
        /// <param name="stateMachine">プレイヤーのステートマシン</param>
        /// <param name="transform">Transformコンポーネント</param>
        /// <param name="characterController">CharacterControllerコンポーネント</param>
        /// <remarks>
        /// テスト時やモック化が必要な場合に使用される構築方法です。
        /// 各依存関係を個別に制御できるため、単体テストの実装が容易になります。
        /// 
        /// 使用例：
        /// <code>
        /// var mockTransform = CreateMockTransform();
        /// var mockController = CreateMockCharacterController();
        /// var context = new PlayerCommandContext(stateMachine, mockTransform, mockController);
        /// </code>
        /// </remarks>
        public PlayerCommandContext(DetailedPlayerStateMachine stateMachine, 
                                  Transform transform, 
                                  CharacterController characterController)
        {
            StateMachine = stateMachine;
            Transform = transform;
            CharacterController = characterController;
        }
    }
}