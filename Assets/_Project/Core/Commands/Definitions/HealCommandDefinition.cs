using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ蝗槫ｾｩ繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 菴灘鴨繝ｻ繝槭リ繝ｻ繧ｹ繧ｿ繝溘リ遲峨・蝗槫ｾｩ
    /// - 蝗槫ｾｩ繧ｿ繧､繝暦ｼ育椪髢薙∫ｶ咏ｶ壹∫ｯ・峇・峨・邂｡逅・    /// - 蝗槫ｾｩ繧｢繧､繝・Β繧・せ繧ｭ繝ｫ縺ｨ縺ｮ騾｣謳ｺ
    /// - 蝗槫ｾｩ繧ｨ繝輔ぉ繧ｯ繝医→繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
    /// </summary>
    [System.Serializable]
    public class HealCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 蝗槫ｾｩ縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum HealType
        {
            Instant,        // 迸ｬ髢灘屓蠕ｩ
            Overtime,       // 邯咏ｶ壼屓蠕ｩ
            Area,           // 遽・峇蝗槫ｾｩ
            Percentage,     // 蜑ｲ蜷亥屓蠕ｩ
            Full            // 螳悟・蝗槫ｾｩ
        }

        /// <summary>
        /// 蝗槫ｾｩ蟇ｾ雎｡縺ｮ繝ｪ繧ｽ繝ｼ繧ｹ繧ｿ繧､繝・        /// </summary>
        public enum ResourceType
        {
            Health,         // 菴灘鴨
            Mana,           // 繝槭リ
            Stamina,        // 繧ｹ繧ｿ繝溘リ
            All             // 蜈ｨ縺ｦ
        }

        [Header("Heal Parameters")]
        public HealType healType = HealType.Instant;
        public ResourceType targetResource = ResourceType.Health;
        public float healAmount = 50f;
        public float percentage = 0f; // 蜑ｲ蜷亥屓蠕ｩ譎ゅ↓菴ｿ逕ｨ・・-1・・
        [Header("Overtime Settings")]
        [Tooltip("邯咏ｶ壼屓蠕ｩ譎ゅ・邱冗ｶ咏ｶ壽凾髢・)]
        public float duration = 5f;
        [Tooltip("邯咏ｶ壼屓蠕ｩ譎ゅ・蝗槫ｾｩ髢馴囈")]
        public float tickInterval = 1f;

        [Header("Area Settings")]
        [Tooltip("遽・峇蝗槫ｾｩ譎ゅ・蜉ｹ譫懃ｯ・峇")]
        public float radius = 3f;
        [Tooltip("遽・峇蝗槫ｾｩ縺ｮ蟇ｾ雎｡繝ｬ繧､繝､繝ｼ")]
        public LayerMask targetLayers = -1;
        [Tooltip("閾ｪ蛻・ｂ蝗槫ｾｩ蟇ｾ雎｡縺ｫ蜷ｫ繧縺・)]
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
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public HealCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public HealCommandDefinition(HealType type, ResourceType resource, float amount)
        {
            healType = type;
            targetResource = resource;
            healAmount = amount;
        }

        /// <summary>
        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (healAmount <= 0f && percentage <= 0f) return false;
            
            // 邯咏ｶ壼屓蠕ｩ縺ｮ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (healType == HealType.Overtime)
            {
                if (duration <= 0f || tickInterval <= 0f) return false;
            }

            // 遽・峇蝗槫ｾｩ縺ｮ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (healType == HealType.Area)
            {
                if (radius <= 0f) return false;
            }

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繝槭リ豸郁ｲｻ繝√ぉ繝・け
                // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け
                // 蟇ｾ雎｡縺ｮ蝗槫ｾｩ蜿ｯ閭ｽ諤ｧ繝√ぉ繝・け・域里縺ｫ貅繧ｿ繝ｳ縺ｮ蝣ｴ蜷育ｭ会ｼ・                // 迥ｶ諷狗焚蟶ｸ繝√ぉ繝・け・亥屓蠕ｩ髦ｻ螳ｳ繝・ヰ繝慕ｭ会ｼ・            }

            return true;
        }

        /// <summary>
        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new HealCommand(this, context);
        }
    }

    /// <summary>
    /// HealCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class HealCommand : ICommand
    {
        private HealCommandDefinition definition;
        private object context;
        private bool executed = false;
        private float healedAmount = 0f;
        private bool isActive = false; // 邯咏ｶ壼屓蠕ｩ逕ｨ

        public HealCommand(HealCommandDefinition healDefinition, object executionContext)
        {
            definition = healDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 蝗槫ｾｩ繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
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
        /// 迸ｬ髢灘屓蠕ｩ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteInstantHeal()
        {
            if (context is MonoBehaviour mono)
            {
                healedAmount = ApplyHeal(mono, definition.healAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 邯咏ｶ壼屓蠕ｩ縺ｮ髢句ｧ・        /// </summary>
        private void StartOvertimeHeal()
        {
            isActive = true;
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲，oroutine 縺ｾ縺溘・Update繝ｫ繝ｼ繝励〒螳壽悄逧・↓ApplyHeal繧貞他縺ｳ蜃ｺ縺・            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started overtime heal: {definition.healAmount} over {definition.duration}s");
#endif
        }

        /// <summary>
        /// 遽・峇蝗槫ｾｩ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteAreaHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 遽・峇蜀・・繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ讀懃ｴ｢
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
        /// 蜑ｲ蜷亥屓蠕ｩ縺ｮ螳溯｡・        /// </summary>
        private void ExecutePercentageHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 譛螟ｧ蛟､縺ｮ蜑ｲ蜷医〒蝗槫ｾｩ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ HealthSystem 縺九ｉ譛螟ｧ蛟､繧貞叙蠕暦ｼ・                float percentageAmount = 100f * definition.percentage; // 莉ｮ縺ｮ蛟､
                healedAmount = ApplyHeal(mono, percentageAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 螳悟・蝗槫ｾｩ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteFullHeal()
        {
            if (context is MonoBehaviour mono)
            {
                // 譛螟ｧ蛟､縺ｾ縺ｧ蝗槫ｾｩ・亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ HealthSystem 縺九ｉ譛螟ｧ蛟､繧貞叙蠕暦ｼ・                float fullAmount = 999f; // 莉ｮ縺ｮ蛟､
                healedAmount = ApplyHeal(mono, fullAmount);
                ShowHealEffect(mono);
            }
        }

        /// <summary>
        /// 螳滄圀縺ｮ蝗槫ｾｩ蜃ｦ逅・ｒ驕ｩ逕ｨ
        /// </summary>
        private float ApplyHeal(MonoBehaviour target, float amount)
        {
            if (target == null) return 0f;

            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲？ealthSystem, ManaSystem, StaminaSystem遲峨→縺ｮ騾｣謳ｺ
            float actualHealAmount = amount;

            // 繧ｪ繝ｼ繝舌・繝偵・繝ｫ蛻ｶ髯・            if (!definition.canOverheal)
            {
                // 迴ｾ蝨ｨ蛟､縺ｨ譛螟ｧ蛟､縺九ｉ螳滄圀縺ｮ蝗槫ｾｩ驥上ｒ險育ｮ・                // actualHealAmount = Mathf.Min(amount, maxValue - currentValue);
            }

            // 繝ｪ繧ｽ繝ｼ繧ｹ繧ｿ繧､繝励↓蠢懊§縺溷屓蠕ｩ蜃ｦ逅・            switch (definition.targetResource)
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
                    // 蜈ｨ繝ｪ繧ｽ繝ｼ繧ｹ縺ｮ蝗槫ｾｩ
                    break;
            }

            return actualHealAmount;
        }

        /// <summary>
        /// 蝗槫ｾｩ繧ｨ繝輔ぉ繧ｯ繝医・陦ｨ遉ｺ
        /// </summary>
        private void ShowHealEffect(MonoBehaviour target)
        {
            if (!definition.showHealEffect || target == null) return;

            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            // UI陦ｨ遉ｺ・亥屓蠕ｩ驥上・繝昴ャ繝励い繝・・遲会ｼ・
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Showing heal effect on {target.name}");
#endif
        }

        /// <summary>
        /// 邯咏ｶ壼屓蠕ｩ縺ｮ譖ｴ譁ｰ・亥､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateOvertimeHeal(float deltaTime)
        {
            if (!isActive || definition.healType != HealCommandDefinition.HealType.Overtime) return;

            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲》ickInterval 縺斐→縺ｫ蝗槫ｾｩ蜃ｦ逅・ｒ螳溯｡・            // duration 縺檎ｵ碁℃縺励◆繧臥ｵゆｺ・        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ亥屓蠕ｩ縺ｮ蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed || healedAmount <= 0f) return;

            // 蝗槫ｾｩ縺励◆蛻・□縺代ム繝｡繝ｼ繧ｸ繧剃ｸ弱∴縺ｦ蜈・↓謌ｻ縺・            if (context is MonoBehaviour mono)
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲∝屓蠕ｩ縺励◆蛻・・繝繝｡繝ｼ繧ｸ繧帝←逕ｨ
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Undoing heal: removing {healedAmount} healed amount");
#endif
            }

            // 邯咏ｶ壼屓蠕ｩ縺ｮ蛛懈ｭ｢
            if (isActive)
            {
                isActive = false;
            }

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && healedAmount > 0f;

        /// <summary>
        /// 邯咏ｶ壼屓蠕ｩ縺檎樟蝨ｨ繧｢繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsActive => isActive;
    }
}