using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 移動コマンドの定義。
    /// プレイヤーまたはAIの移動アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 移動方向と速度の指定
    /// - 移動タイプ（歩き、走り、忍び歩き等）の管理
    /// - 移動制約（地形、障害物等）の考慮
    /// - アニメーションブレンディングとの連携
    /// </summary>
    [System.Serializable]
    public class MoveCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 移動の種類を定義する列挙型
        /// </summary>
        public enum MoveType
        {
            Walk,       // 歩き
            Run,        // 走り  
            Sprint,     // ダッシュ
            Sneak,      // 忍び歩き
            Strafe      // 横歩き
        }

        [Header("Movement Parameters")]
        public MoveType moveType = MoveType.Walk;
        public Vector3 direction = Vector3.forward;
        public float speed = 5f;
        public float duration = 1f;

        [Header("Movement Constraints")]
        public bool respectGravity = true;
        public bool checkCollisions = true;
        public LayerMask obstacleLayer = -1;

        [Header("Animation")]
        public bool useRootMotion = false;
        public float blendTime = 0.2f;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public MoveCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public MoveCommandDefinition(MoveType type, Vector3 moveDirection, float moveSpeed = 5f)
        {
            moveType = type;
            direction = moveDirection.normalized;
            speed = moveSpeed;
        }

        /// <summary>
        /// 移動コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (speed <= 0f || duration <= 0f) return false;
            
            // 移動方向のチェック
            if (direction == Vector3.zero) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // プレイヤーまたはAIの状態チェック
                // 例：麻痺状態、スタン状態等での移動不可判定
            }

            return true;
        }

        /// <summary>
        /// 移動コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new MoveCommand(this, context);
        }
    }

    /// <summary>
    /// MoveCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class MoveCommand : ICommand
    {
        private MoveCommandDefinition definition;
        private object context;
        private bool executed = false;
        private Vector3 originalPosition;

        public MoveCommand(MoveCommandDefinition moveDefinition, object executionContext)
        {
            definition = moveDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 移動コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 実行前の位置を保存（Undo用）
            if (context is MonoBehaviour mono && mono.transform != null)
            {
                originalPosition = mono.transform.position;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.moveType} movement: {definition.direction} direction, {definition.speed} speed");
#endif

            // 実際の移動処理をここに実装
            // - Transform操作またはRigidbody操作
            // - アニメーション制御
            // - 物理衝突チェック
            // - エフェクト再生

            executed = true;
        }

        /// <summary>
        /// Undo操作（移動の取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed || context == null) return;

            if (context is MonoBehaviour mono && mono.transform != null)
            {
                mono.transform.position = originalPosition;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Movement undone - returned to original position");
#endif
            }

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed && context != null;
    }
}