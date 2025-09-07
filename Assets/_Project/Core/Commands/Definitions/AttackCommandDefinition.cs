using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 攻撃コマンドの定義。
    /// プレイヤーまたはAIの攻撃アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 攻撃方向とダメージ量の指定
    /// - 攻撃タイプ（近接、遠距離、魔法等）の管理
    /// - クールダウンとスタミナ消費の考慮
    /// - 連続攻撃（コンボ）への対応
    /// </summary>
    [System.Serializable]
    public class AttackCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 攻撃の種類を定義する列挙型
        /// </summary>
        public enum AttackType
        {
            Light,      // 軽攻撃
            Heavy,      // 強攻撃
            Special,    // 特殊攻撃
            Magic,      // 魔法攻撃
            Ranged      // 遠距離攻撃
        }

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
        /// デフォルトコンストラクタ
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
        /// 攻撃コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (context == null) return false;

            // スタミナチェック、クールダウンチェック等はここで実装
            // 実際の実装では、プレイヤーやAIのステータスを参照
            
            return damage > 0f && range > 0f;
        }

        /// <summary>
        /// 攻撃コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new AttackCommand(this, context);
        }
    }

    /// <summary>
    /// AttackCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
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
        /// 攻撃コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.attackType} attack: {definition.damage} damage, {definition.range} range");
#endif

            // 実際の攻撃処理をここに実装
            // - アニメーション再生
            // - 当たり判定
            // - ダメージ計算
            // - エフェクト生成

            executed = true;
        }

        /// <summary>
        /// Undo操作（攻撃の取り消しは通常不可能）
        /// </summary>
        public void Undo()
        {
            // 攻撃は通常取り消し不可
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("Attack commands cannot be undone");
#endif
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => false;
    }
}