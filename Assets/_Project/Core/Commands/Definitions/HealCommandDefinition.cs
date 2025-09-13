using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 回復コマンドの定義。
    /// プレイヤーまたはAIの回復アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 体力・マナ・スタミナ等の回復
    /// - 回復タイプ（瞬間、継続、範囲）の管理
    /// - 回復アイテムやスキルとの連携
    /// - 回復エフェクトとアニメーション制御
    /// </summary>
    [System.Serializable]
    public class HealCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 回復の種類を定義する列挙型
        /// </summary>
        public enum HealType
        {
            Instant,        // 瞬間回復
            Overtime,       // 継続回復
            Area,           // 範囲回復
            Percentage,     // 割合回復
            Full            // 完全回復
        }

        /// <summary>
        /// 回復対象のリソースタイプ
        /// </summary>
        public enum ResourceType
        {
            Health,         // 体力
            Mana,           // マナ
            Stamina,        // スタミナ
            All             // 全て
        }

        [Header("Heal Parameters")]
        public HealType healType = HealType.Instant;
        public ResourceType targetResource = ResourceType.Health;
        public float healAmount = 50f;
        public float percentage = 0f; // 割合回復時に使用（0-1）

        [Header("Overtime Settings")]
        [Tooltip("継続回復時の総継続時間")]
        public float duration = 5f;
        [Tooltip("継続回復時の回復間隔")]
        public float tickInterval = 1f;

        [Header("Area Settings")]
        [Tooltip("範囲回復時の効果範囲")]
        public float radius = 3f;
        [Tooltip("範囲回復の対象レイヤー")]
        public LayerMask targetLayers = -1;
        [Tooltip("自分も回復対象に含むか")]
        public bool includeSelf = true;

        [Header("Restrictions")]
        public bool canOverheal = false;
        public float cooldownTime = 3f;
        public float manaCost = 15f;

        [Header("Effects")]
        public bool showHealEffect = true;
        public float effectDuration = 2f;
        public Color healEffectColor = Color.green;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public HealCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public HealCommandDefinition(HealType type, ResourceType resource, float amount)
        {
            healType = type;
            targetResource = resource;
            healAmount = amount;
        }

        /// <summary>
        /// 回復コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (healAmount <= 0f && percentage <= 0f) return false;
            
            // 継続回復の場合の追加チェック
            if (healType == HealType.Overtime)
            {
                if (duration <= 0f || tickInterval <= 0f) return false;
            }

            // 範囲回復の場合の追加チェック
            if (healType == HealType.Area)
            {
                if (radius <= 0f) return false;
            }

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // マナ消費チェック
                // クールダウンチェック
                // 対象の回復可能性チェック（既に満タンの場合等）
                // 状態異常チェック（回復阻害デバフ等）
            }

            return true;
        }

        /// <summary>
        /// 回復コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new HealCommand(this, context);
        }
    }

    /// <summary>
    /// HealCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class HealCommand : ICommand
    {
        private HealCommandDefinition definition;
        private object context;
        private bool executed = false;
        private float healedAmount = 0f;
        private bool isActive = false; // 継続回復用

        public HealCommand(HealCommandDefinition healDefinition, object executionContext)
        {
            definition = healDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 回復コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.healType} heal: {definition.healAmount} {definition.targetResource}");
#endif

            switch (definition.healType)
            {
                case HealCommandDefinition.HealType.Instant:
                    ExecuteInstantHeal();
                    break;
                case HealCommandDefinition.HealType.Overtime:
                    StartOvertimeHeal();
                    break;
                case HealCommandDefinition.HealType.Area:
                    ExecuteAreaHeal();
                    break;
                case HealCommandDefinition.HealType.Percentage:
                    ExecutePercentageHeal();
                    break;
                case HealCommandDefinition.HealType.Full:
                    ExecuteFullHeal();
                    break;
            }

            executed = true;
        }

        /// <summary>
        /// 瞬間回復の実行
        /// </summary>
        private void ExecuteInstantHeal()
        {
            if (context is MonoBehaviour mono)
            {
                healedAmount = ApplyHeal(mono, definition.healAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 継続回復の開始
        /// </summary>
        private void StartOvertimeHeal()
        {
            isActive = true;
            // 実際の実装では、Coroutine またはUpdateループで定期的にApplyHealを呼び出す
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started overtime heal: {definition.healAmount} over {definition.duration}s");
#endif
        }

        /// <summary>
        /// 範囲回復の実行
        /// </summary>
        private void ExecuteAreaHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 範囲内のオブジェクトを検索
                Collider[] targets = Physics.OverlapSphere(mono.transform.position, definition.radius, definition.targetLayers);
                
                foreach (var target in targets)
                {
                    if (!definition.includeSelf && target.gameObject == mono.gameObject)
                        continue;

                    ApplyHeal(target.GetComponent<MonoBehaviour>(), definition.healAmount);
                    ShowHealEffect(target.GetComponent<MonoBehaviour>());
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Area heal affected {targets.Length} targets");
#endif
            }
        }

        /// <summary>
        /// 割合回復の実行
        /// </summary>
        private void ExecutePercentageHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 最大値の割合で回復（実際の実装では HealthSystem から最大値を取得）
                float percentageAmount = 100f * definition.percentage; // 仮の値
                healedAmount = ApplyHeal(mono, percentageAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 完全回復の実行
        /// </summary>
        private void ExecuteFullHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 最大値まで回復（実際の実装では HealthSystem から最大値を取得）
                float fullAmount = 999f; // 仮の値
                healedAmount = ApplyHeal(mono, fullAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 実際の回復処理を適用
        /// </summary>
        private float ApplyHeal(MonoBehaviour target, float amount)
        {
            if (target == null) return 0f;

            // 実際の実装では、HealthSystem, ManaSystem, StaminaSystem等との連携
            float actualHealAmount = amount;

            // オーバーヒール制限
            if (!definition.canOverheal)
            {
                // 現在値と最大値から実際の回復量を計算
                // actualHealAmount = Mathf.Min(amount, maxValue - currentValue);
            }

            // リソースタイプに応じた回復処理
            switch (definition.targetResource)
            {
                case HealCommandDefinition.ResourceType.Health:
                    // healthSystem.Heal(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.Mana:
                    // manaSystem.RestoreMana(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.Stamina:
                    // staminaSystem.RestoreStamina(actualHealAmount);
                    break;
                case HealCommandDefinition.ResourceType.All:
                    // 全リソースの回復
                    break;
            }

            return actualHealAmount;
        }

        /// <summary>
        /// 回復エフェクトの表示
        /// </summary>
        private void ShowHealEffect(MonoBehaviour target)
        {
            if (!definition.showHealEffect || target == null) return;

            // パーティクルエフェクト
            // サウンドエフェクト
            // UI表示（回復量のポップアップ等）

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing heal effect on {target.name}");
#endif
        }

        /// <summary>
        /// 継続回復の更新（外部から定期的に呼び出される）
        /// </summary>
        public void UpdateOvertimeHeal(float deltaTime)
        {
            if (!isActive || definition.healType != HealCommandDefinition.HealType.Overtime) return;

            // 実際の実装では、tickInterval ごとに回復処理を実行
            // duration が経過したら終了
        }

        /// <summary>
        /// Undo操作（回復の取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed || healedAmount <= 0f) return;

            // 回復した分だけダメージを与えて元に戻す
            if (context is MonoBehaviour mono)
            {
                // 実際の実装では、回復した分のダメージを適用
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Undoing heal: removing {healedAmount} healed amount");
#endif
            }

            // 継続回復の停止
            if (isActive)
            {
                isActive = false;
            }

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed && healedAmount > 0f;

        /// <summary>
        /// 継続回復が現在アクティブかどうか
        /// </summary>
        public bool IsActive => isActive;
    }
}