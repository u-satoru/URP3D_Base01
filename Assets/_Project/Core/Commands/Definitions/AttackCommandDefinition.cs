using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 攻撁E��マンド�E定義、E    /// プレイヤーまた�EAIの攻撁E��クションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 攻撁E��向とダメージ量�E持E��E    /// - 攻撁E��イプ（近接、E��距離、E��法等）�E管琁E    /// - クールダウンとスタミナ消費の老E�E
    /// - 連続攻撁E��コンボ）への対忁E    /// </summary>
    [System.Serializable]
    public class AttackCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 攻撁E�E種類を定義する列挙垁E        /// </summary>
        public enum AttackType
        {
            Light,      // 軽攻撁E            Heavy,      // 強攻撁E            Special,    // 特殊攻撁E            Magic,      // 魔法攻撁E            Ranged      // 遠距離攻撁E        }

        [Header("Attack Parameters")]
        public AttackType attackType = AttackType.Light;
        public Vector3 direction = Vector3.forward;
        public float damage = 10f;
        public float range = 2f;
        public float staminaCost = 15f;

        [Header("Timing")]
        public float executionDelay = 0.1f;
        public float cooldownTime = 1f;

        [Header("Combat Mechanics")]
        public bool canInterruptOthers = false;
        public bool canBeInterrupted = true;
        public int comboIndex = 0;

        /// <summary>
        /// チE��ォルトコンストラクタ
        /// </summary>
        public AttackCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public AttackCommandDefinition(AttackType type, Vector3 attackDirection, float attackDamage = 10f)
        {
            attackType = type;
            direction = attackDirection;
            damage = attackDamage;
        }

        /// <summary>
        /// 攻撁E��マンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (context == null) return false;

            // スタミナチェチE��、クールダウンチェチE��等�Eここで実裁E            // 実際の実裁E��は、�EレイヤーやAIのスチE�Eタスを参照
            
            return damage > 0f && range > 0f;
        }

        /// <summary>
        /// 攻撁E��マンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new AttackCommand(this, context);
        }
    }

    /// <summary>
    /// AttackCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
    public class AttackCommand : ICommand
    {
        private AttackCommandDefinition definition;
        private object context;
        private bool executed = false;

        public AttackCommand(AttackCommandDefinition attackDefinition, object executionContext)
        {
            definition = attackDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 攻撁E��マンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.attackType} attack: {definition.damage} damage, {definition.range} range");
#endif

            // 実際の攻撁E�E琁E��ここに実裁E            // - アニメーション再生
            // - 当たり判宁E            // - ダメージ計箁E            // - エフェクト生戁E
            executed = true;
        }

        /// <summary>
        /// Undo操作（攻撁E�E取り消しは通常不可能�E�E        /// </summary>
        public void Undo()
        {
            // 攻撁E�E通常取り消し不可
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("Attack commands cannot be undone");
#endif
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => false;
    }
}