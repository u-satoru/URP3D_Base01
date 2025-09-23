using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 移動コマンド�E定義、E    /// プレイヤーまた�EAIの移動アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 移動方向と速度の持E��E    /// - 移動タイプ（歩き、走り、忍�E歩き等）�E管琁E    /// - 移動制紁E��地形、E��害物等）�E老E�E
    /// - アニメーションブレンチE��ングとの連携
    /// </summary>
    [System.Serializable]
    public class MoveCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 移動�E種類を定義する列挙垁E        /// </summary>
        public enum MoveType
        {
            Walk,       // 歩ぁE            Run,        // 走めE 
            Sprint,     // ダチE��ュ
            Sneak,      // 忍�E歩ぁE            Strafe      // 横歩ぁE        }

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
        /// チE��ォルトコンストラクタ
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
        /// 移動コマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (speed <= 0f || duration <= 0f) return false;
            
            // 移動方向�EチェチE��
            if (direction == Vector3.zero) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // プレイヤーまた�EAIの状態チェチE��
                // 例：麻痺状態、スタン状態等での移動不可判宁E            }

            return true;
        }

        /// <summary>
        /// 移動コマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new MoveCommand(this, context);
        }
    }

    /// <summary>
    /// MoveCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// 移動コマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 実行前の位置を保存！Endo用�E�E            if (context is MonoBehaviour mono && mono.transform != null)
            {
                originalPosition = mono.transform.position;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.moveType} movement: {definition.direction} direction, {definition.speed} speed");
#endif

            // 実際の移動�E琁E��ここに実裁E            // - Transform操作また�ERigidbody操佁E            // - アニメーション制御
            // - 物琁E��突チェチE��
            // - エフェクト�E甁E
            executed = true;
        }

        /// <summary>
        /// Undo操作（移動�E取り消し�E�E        /// </summary>
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
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && context != null;
    }
}