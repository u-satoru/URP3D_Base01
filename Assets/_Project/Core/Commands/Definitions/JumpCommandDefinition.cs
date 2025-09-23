using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// ジャンプコマンド�E定義、E    /// プレイヤーまた�EAIのジャンプアクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - ジャンプ力と方向�E持E��E    /// - ジャンプタイプ（通常、二段、壁、E��距離等）�E管琁E    /// - 着地判定と着地後�E処琁E    /// - スタミナ消費とクールダウンの老E�E
    /// </summary>
    [System.Serializable]
    public class JumpCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// ジャンプ�E種類を定義する列挙垁E        /// </summary>
        public enum JumpType
        {
            Normal,     // 通常ジャンチE            Double,     // 二段ジャンチE            Wall,       // 壁ジャンチE            Long,       // 長距離ジャンチE            High        // 高ジャンチE        }

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
        /// チE��ォルトコンストラクタ
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
        /// ジャンプコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (jumpForce <= 0f) return false;
            
            // 方向�EクトルのチェチE��
            if (direction == Vector3.zero) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // 地面判定チェチE���E�EequiresGroundedが有効の場合！E                // スタミナチェチE��
                // クールダウンチェチE��
                // 状態異常チェチE���E�麻痺、スタン等！E            }

            return true;
        }

        /// <summary>
        /// ジャンプコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new JumpCommand(this, context);
        }
    }

    /// <summary>
    /// JumpCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// ジャンプコマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 実行前の状態を保存！Endo用�E�E            if (context is MonoBehaviour mono && mono.GetComponent<Rigidbody>() != null)
            {
                var rb = mono.GetComponent<Rigidbody>();
                originalVelocity = rb.linearVelocity;
                // 地面判定�E保存（実際の実裁E��は GroundCheck コンポ�Eネント等を参�E�E�E            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.jumpType} jump: {definition.jumpForce} force, {definition.direction} direction");
#endif

            // 実際のジャンプ�E琁E��ここに実裁E            if (context is MonoBehaviour monoBehaviour && monoBehaviour.GetComponent<Rigidbody>() != null)
            {
                var rb = monoBehaviour.GetComponent<Rigidbody>();
                
                // 垂直速度のリセチE��
                if (definition.resetVerticalVelocity)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                }

                // ジャンプ力の適用
                Vector3 jumpVelocity = definition.direction.normalized * definition.jumpForce;
                
                // 水平ブ�Eスト�E追加
                if (definition.horizontalBoost > 0f)
                {
                    Vector3 horizontalDirection = new Vector3(definition.direction.x, 0f, definition.direction.z).normalized;
                    jumpVelocity += horizontalDirection * definition.horizontalBoost;
                }

                rb.AddForce(jumpVelocity, ForceMode.VelocityChange);

                // アニメーション制御
                // パ�EチE��クルエフェクチE                // サウンドエフェクチE            }

            executed = true;
        }

        /// <summary>
        /// Undo操作（ジャンプ�E取り消し�E�E        /// </summary>
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
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && context != null;
    }
}