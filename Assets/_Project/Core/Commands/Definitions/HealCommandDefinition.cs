using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 回復コマンド�E定義、E    /// プレイヤーまた�EAIの回復アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 体力・マナ・スタミナ等�E回復
    /// - 回復タイプ（瞬間、継続、篁E���E��E管琁E    /// - 回復アイチE��めE��キルとの連携
    /// - 回復エフェクトとアニメーション制御
    /// </summary>
    [System.Serializable]
    public class HealCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 回復の種類を定義する列挙垁E        /// </summary>
        public enum HealType
        {
            Instant,        // 瞬間回復
            Overtime,       // 継続回復
            Area,           // 篁E��回復
            Percentage,     // 割合回復
            Full            // 完�E回復
        }

        /// <summary>
        /// 回復対象のリソースタイチE        /// </summary>
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
        public float percentage = 0f; // 割合回復時に使用�E�E-1�E�E
        [Header("Overtime Settings")]
        [Tooltip("継続回復時�E総継続時閁E)]
        public float duration = 5f;
        [Tooltip("継続回復時�E回復間隔")]
        public float tickInterval = 1f;

        [Header("Area Settings")]
        [Tooltip("篁E��回復時�E効果篁E��")]
        public float radius = 3f;
        [Tooltip("篁E��回復の対象レイヤー")]
        public LayerMask targetLayers = -1;
        [Tooltip("自刁E��回復対象に含むぁE)]
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
        /// チE��ォルトコンストラクタ
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
        /// 回復コマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (healAmount <= 0f && percentage <= 0f) return false;
            
            // 継続回復の場合�E追加チェチE��
            if (healType == HealType.Overtime)
            {
                if (duration <= 0f || tickInterval <= 0f) return false;
            }

            // 篁E��回復の場合�E追加チェチE��
            if (healType == HealType.Area)
            {
                if (radius <= 0f) return false;
            }

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // マナ消費チェチE��
                // クールダウンチェチE��
                // 対象の回復可能性チェチE���E�既に満タンの場合等！E                // 状態異常チェチE���E�回復阻害チE��フ等！E            }

            return true;
        }

        /// <summary>
        /// 回復コマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new HealCommand(this, context);
        }
    }

    /// <summary>
    /// HealCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// 回復コマンド�E実衁E        /// </summary>
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
        /// 瞬間回復の実衁E        /// </summary>
        private void ExecuteInstantHeal()
        {
            if (context is MonoBehaviour mono)
            {
                healedAmount = ApplyHeal(mono, definition.healAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 継続回復の開姁E        /// </summary>
        private void StartOvertimeHeal()
        {
            isActive = true;
            // 実際の実裁E��は、Coroutine また�EUpdateループで定期皁E��ApplyHealを呼び出ぁE            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started overtime heal: {definition.healAmount} over {definition.duration}s");
#endif
        }

        /// <summary>
        /// 篁E��回復の実衁E        /// </summary>
        private void ExecuteAreaHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 篁E��冁E�Eオブジェクトを検索
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
        /// 割合回復の実衁E        /// </summary>
        private void ExecutePercentageHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 最大値の割合で回復�E�実際の実裁E��は HealthSystem から最大値を取得！E                float percentageAmount = 100f * definition.percentage; // 仮の値
                healedAmount = ApplyHeal(mono, percentageAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 完�E回復の実衁E        /// </summary>
        private void ExecuteFullHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 最大値まで回復�E�実際の実裁E��は HealthSystem から最大値を取得！E                float fullAmount = 999f; // 仮の値
                healedAmount = ApplyHeal(mono, fullAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 実際の回復処琁E��適用
        /// </summary>
        private float ApplyHeal(MonoBehaviour target, float amount)
        {
            if (target == null) return 0f;

            // 実際の実裁E��は、HealthSystem, ManaSystem, StaminaSystem等との連携
            float actualHealAmount = amount;

            // オーバ�Eヒ�Eル制陁E            if (!definition.canOverheal)
            {
                // 現在値と最大値から実際の回復量を計箁E                // actualHealAmount = Mathf.Min(amount, maxValue - currentValue);
            }

            // リソースタイプに応じた回復処琁E            switch (definition.targetResource)
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
        /// 回復エフェクト�E表示
        /// </summary>
        private void ShowHealEffect(MonoBehaviour target)
        {
            if (!definition.showHealEffect || target == null) return;

            // パ�EチE��クルエフェクチE            // サウンドエフェクチE            // UI表示�E�回復量�EポップアチE�E等！E
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing heal effect on {target.name}");
#endif
        }

        /// <summary>
        /// 継続回復の更新�E�外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateOvertimeHeal(float deltaTime)
        {
            if (!isActive || definition.healType != HealCommandDefinition.HealType.Overtime) return;

            // 実際の実裁E��は、tickInterval ごとに回復処琁E��実衁E            // duration が経過したら終亁E        }

        /// <summary>
        /// Undo操作（回復の取り消し�E�E        /// </summary>
        public void Undo()
        {
            if (!executed || healedAmount <= 0f) return;

            // 回復した刁E��けダメージを与えて允E��戻ぁE            if (context is MonoBehaviour mono)
            {
                // 実際の実裁E��は、回復した刁E�Eダメージを適用
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
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && healedAmount > 0f;

        /// <summary>
        /// 継続回復が現在アクチE��ブかどぁE��
        /// </summary>
        public bool IsActive => isActive;
    }
}