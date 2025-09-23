using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 防御コマンド�E定義、E    /// プレイヤーまた�EAIの防御アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - ガード方向と強度の持E��E    /// - 防御タイプ（ブロチE��、回避、カウンター等）�E管琁E    /// - スタミナ消費とダメージ軽減率の計箁E    /// - パリィめE��ウンター攻撁E��の対忁E    /// </summary>
    [System.Serializable]
    public class DefendCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 防御の種類を定義する列挙垁E        /// </summary>
        public enum DefendType
        {
            Block,      // ブロチE���E�盾めE��器での防御�E�E            Dodge,      // 回避
            Parry,      // パリィ�E�反撁E��E            Counter,    // カウンター攻撁E            Absorb      // ダメージ吸叁E        }

        [Header("Defense Parameters")]
        public DefendType defendType = DefendType.Block;
        public Vector3 guardDirection = Vector3.forward;
        public float blockStrength = 0.7f; // ダメージ軽減率 (0.0-1.0)
        public float guardAngle = 90f; // 防御可能角度

        [Header("Resource Costs")]
        public float staminaCost = 10f;
        public float staminaDrainRate = 5f; // 継続防御時�E毎秒消費

        [Header("Timing Windows")]
        public float activationTime = 0.2f; // 防御開始までの時間
        public float perfectBlockWindow = 0.1f; // パ�EフェクトブロチE��の窓時閁E        public float parryWindow = 0.15f; // パリィの窓時閁E
        [Header("Combat Mechanics")]
        public bool allowsPerfectBlock = true;
        public bool canParryProjectiles = false;
        public float counterDamageMultiplier = 1.5f;

        /// <summary>
        /// チE��ォルトコンストラクタ
        /// </summary>
        public DefendCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public DefendCommandDefinition(DefendType type, Vector3 direction, float strength = 0.7f)
        {
            defendType = type;
            guardDirection = direction;
            blockStrength = Mathf.Clamp01(strength);
        }

        /// <summary>
        /// 防御コマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            if (context == null) return false;

            // スタミナチェチE��、状態チェチE��筁E            return blockStrength > 0f && staminaCost >= 0f;
        }

        /// <summary>
        /// 防御コマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new DefendCommand(this, context);
        }
    }

    /// <summary>
    /// DefendCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
    public class DefendCommand : ICommand
    {
        private DefendCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isDefending = false;

        public DefendCommand(DefendCommandDefinition defendDefinition, object executionContext)
        {
            definition = defendDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 防御コマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.defendType} defense: {definition.blockStrength * 100}% damage reduction");
#endif

            // 実際の防御処琁E��ここに実裁E            // - 防御ポ�Eズの開姁E            // - ダメージ軽減効果�E適用
            // - スタミナ消費
            // - エフェクト生戁E
            isDefending = true;
            executed = true;
        }

        /// <summary>
        /// 防御の終亁E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Ending {definition.defendType} defense");
#endif

            // 防御状態�E終亁E�E琁E            isDefending = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE���E�防御終亁E��して機�E�E�E        /// </summary>
        public bool CanUndo => executed && isDefending;
    }
}