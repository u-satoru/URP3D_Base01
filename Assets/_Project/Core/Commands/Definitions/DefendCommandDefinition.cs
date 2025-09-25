using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 防御コマンドの定義。
    /// プレイヤーまたはAIの防御アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - ガード方向と強度の指定
    /// - 防御タイプ（ブロック、回避、カウンター等）の管理
    /// - スタミナ消費とダメージ軽減率の計算
    /// - パリィやカウンター攻撃への対応
    /// </summary>
    [System.Serializable]
    public class DefendCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 防御の種類を定義する列挙型
        /// </summary>
        public enum DefendType
        {
            Block,      // ブロック（盾や武器での防御）
            Dodge,      // 回避
            Parry,      // パリィ（反撃）
            Counter,    // カウンター攻撃
            Absorb      // ダメージ吸収
        }

        [Header("Defense Parameters")]
        public DefendType defendType = DefendType.Block;
        public Vector3 guardDirection = Vector3.forward;
        public float blockStrength = 0.7f; // ダメージ軽減率 (0.0-1.0)
        public float guardAngle = 90f; // 防御可能角度

        [Header("Resource Costs")]
        public float staminaCost = 10f;
        public float staminaDrainRate = 5f; // 継続防御時の毎秒消費

        [Header("Timing Windows")]
        public float activationTime = 0.2f; // 防御開始までの時間
        public float perfectBlockWindow = 0.1f; // パーフェクトブロックの窓時間
        public float parryWindow = 0.15f; // パリィの窓時間

        [Header("Combat Mechanics")]
        public bool allowsPerfectBlock = true;
        public bool canParryProjectiles = false;
        public float counterDamageMultiplier = 1.5f;

        /// <summary>
        /// デフォルトコンストラクタ
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
        /// 防御コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            if (context == null) return false;

            // スタミナチェック、状態チェック等
            return blockStrength > 0f && staminaCost >= 0f;
        }

        /// <summary>
        /// 防御コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new DefendCommand(this, context);
        }
    }

    /// <summary>
    /// DefendCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
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
        /// 防御コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.defendType} defense: {definition.blockStrength * 100}% damage reduction");
#endif

            // 実際の防御処理をここに実装
            // - 防御ポーズの開始
            // - ダメージ軽減効果の適用
            // - スタミナ消費
            // - エフェクト生成

            isDefending = true;
            executed = true;
        }

        /// <summary>
        /// 防御の終了
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Ending {definition.defendType} defense");
#endif

            // 防御状態の終了処理
            isDefending = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか（防御終了として機能）
        /// </summary>
        public bool CanUndo => executed && isDefending;
    }
}