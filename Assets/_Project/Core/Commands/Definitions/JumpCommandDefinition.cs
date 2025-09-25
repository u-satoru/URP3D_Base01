using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ジャンプコマンドの定義。
    /// プレイヤーまたはAIのジャンプアクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - ジャンプ力と方向の指定
    /// - ジャンプタイプ（通常、二段、壁、長距離等）の管理
    /// - 着地判定と着地後の処理
    /// - スタミナ消費とクールダウンの考慮
    /// </summary>
    [System.Serializable]
    public class JumpCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ジャンプの種類を定義する列挙型
        /// </summary>
        public enum JumpType
        {
            Normal,     // 通常ジャンプ
            Double,     // 二段ジャンプ
            Wall,       // 壁ジャンプ
            Long,       // 長距離ジャンプ
            High        // 高ジャンプ
        }

        [Header("Jump Parameters")]
        public JumpType jumpType = JumpType.Normal;
        public float jumpForce = 10f;
        public Vector3 direction = Vector3.up;
        public float horizontalBoost = 0f;

        [Header("Physics")]
        public float gravityScale = 1f;
        public float airControlMultiplier = 0.5f;
        public bool resetVerticalVelocity = true;

        [Header("Constraints")]
        public bool requiresGrounded = true;
        public float staminaCost = 20f;
        public float cooldownTime = 0.5f;

        [Header("Animation")]
        public float jumpAnimationDuration = 0.3f;
        public float landAnimationDuration = 0.2f;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public JumpCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public JumpCommandDefinition(JumpType type, float force, Vector3 jumpDirection = default)
        {
            jumpType = type;
            jumpForce = force;
            direction = jumpDirection == default ? Vector3.up : jumpDirection.normalized;
        }

        /// <summary>
        /// ジャンプコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (jumpForce <= 0f) return false;
            
            // 方向ベクトルのチェック
            if (direction == Vector3.zero) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // 地面判定チェック（requiresGroundedが有効の場合）
                // スタミナチェック
                // クールダウンチェック
                // 状態異常チェック（麻痺、スタン等）
            }

            return true;
        }

        /// <summary>
        /// ジャンプコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new JumpCommand(this, context);
        }
    }

    /// <summary>
    /// JumpCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class JumpCommand : ICommand
    {
        private JumpCommandDefinition definition;
        private object context;
        private bool executed = false;
        private Vector3 originalVelocity;
        private bool wasGrounded;

        public JumpCommand(JumpCommandDefinition jumpDefinition, object executionContext)
        {
            definition = jumpDefinition;
            context = executionContext;
        }

        /// <summary>
        /// ジャンプコマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 実行前の状態を保存（Undo用）
            if (context is MonoBehaviour mono && mono.GetComponent<Rigidbody>() != null)
            {
                var rb = mono.GetComponent<Rigidbody>();
                originalVelocity = rb.linearVelocity;
                // 地面判定の保存（実際の実装では GroundCheck コンポーネント等を参照）
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.jumpType} jump: {definition.jumpForce} force, {definition.direction} direction");
#endif

            // 実際のジャンプ処理をここに実装
            if (context is MonoBehaviour monoBehaviour && monoBehaviour.GetComponent<Rigidbody>() != null)
            {
                var rb = monoBehaviour.GetComponent<Rigidbody>();
                
                // 垂直速度のリセット
                if (definition.resetVerticalVelocity)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                }

                // ジャンプ力の適用
                Vector3 jumpVelocity = definition.direction.normalized * definition.jumpForce;
                
                // 水平ブーストの追加
                if (definition.horizontalBoost > 0f)
                {
                    Vector3 horizontalDirection = new Vector3(definition.direction.x, 0f, definition.direction.z).normalized;
                    jumpVelocity += horizontalDirection * definition.horizontalBoost;
                }

                rb.AddForce(jumpVelocity, ForceMode.VelocityChange);

                // アニメーション制御
                // パーティクルエフェクト
                // サウンドエフェクト
            }

            executed = true;
        }

        /// <summary>
        /// Undo操作（ジャンプの取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed || context == null) return;

            if (context is MonoBehaviour mono && mono.GetComponent<Rigidbody>() != null)
            {
                var rb = mono.GetComponent<Rigidbody>();
                rb.linearVelocity = originalVelocity;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Jump undone - velocity restored");
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