using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧ｹ繝励Μ繝ｳ繝茨ｼ医ム繝・す繝･・峨さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ鬮倬溽ｧｻ蜍輔い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｹ繝励Μ繝ｳ繝磯溷ｺｦ縺ｨ邯咏ｶ壽凾髢薙・邂｡逅・    /// - 繧ｹ繧ｿ繝溘リ豸郁ｲｻ繧ｷ繧ｹ繝・Β縺ｨ縺ｮ騾｣謳ｺ
    /// - 繧ｹ繝励Μ繝ｳ繝井ｸｭ縺ｮ蛻ｶ邏・ｼ域婿蜷題ｻ｢謠帛宛髯千ｭ会ｼ・    /// - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｨ繧ｨ繝輔ぉ繧ｯ繝医・蛻ｶ蠕｡
    /// </summary>
    [System.Serializable]
    public class SprintCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝医・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum SprintType
        {
            Burst,      // 遏ｭ霍晞屬辷・匱逧・刈騾・            Sustained,  // 謖∫ｶ夂噪鬮倬溽ｧｻ蜍・            Dodge,      // 蝗樣∩繝繝・す繝･
            Charge      // 遯・ｲ謾ｻ謦・        }

        [Header("Sprint Parameters")]
        public SprintType sprintType = SprintType.Burst;
        public float speedMultiplier = 2f;
        public float maxDuration = 3f;
        public Vector3 direction = Vector3.forward;

        [Header("Stamina System")]
        public float staminaConsumptionRate = 10f; // per second
        public float minimumStaminaRequired = 25f;
        public bool canInterruptOnStaminaDepleted = true;

        [Header("Movement Constraints")]
        public bool lockDirection = false;
        public float directionChangeSpeed = 5f;
        public bool maintainVelocityOnEnd = false;

        [Header("Effects")]
        public float accelerationTime = 0.2f;
        public float decelerationTime = 0.5f;
        public bool showTrailEffect = true;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public SprintCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public SprintCommandDefinition(SprintType type, float multiplier, Vector3 sprintDirection)
        {
            sprintType = type;
            speedMultiplier = multiplier;
            direction = sprintDirection.normalized;
        }

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝医さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (speedMultiplier <= 1f || maxDuration <= 0f) return false;
            
            // 譁ｹ蜷代・繧ｯ繝医Ν縺ｮ繝√ぉ繝・け
            if (direction == Vector3.zero) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧ｹ繧ｿ繝溘リ繝√ぉ繝・け
                // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け
                // 迥ｶ諷狗焚蟶ｸ繝√ぉ繝・け・育夢蜉ｴ縲∬ｲ蛯ｷ遲会ｼ・                // 蝨ｰ蠖｢蛻ｶ邏・メ繧ｧ繝・け・域ｰｴ荳ｭ縲∵･譁憺擇遲峨〒縺ｮ繧ｹ繝励Μ繝ｳ繝亥宛髯撰ｼ・            }

            return true;
        }

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝医さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new SprintCommand(this, context);
        }
    }

    /// <summary>
    /// SprintCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class SprintCommand : ICommand
    {
        private SprintCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isActive = false;
        private float originalSpeed;
        private float currentDuration = 0f;

        public SprintCommand(SprintCommandDefinition sprintDefinition, object executionContext)
        {
            definition = sprintDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝医さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.sprintType} sprint: {definition.speedMultiplier}x speed, {definition.maxDuration}s max duration");
#endif

            // 繧ｹ繝励Μ繝ｳ繝育憾諷九・髢句ｧ・            isActive = true;
            currentDuration = 0f;

            // 螳滄圀縺ｮ繧ｹ繝励Μ繝ｳ繝亥・逅・ｒ縺薙％縺ｫ螳溯｣・            if (context is MonoBehaviour mono)
            {
                // 遘ｻ蜍暮溷ｺｦ縺ｮ菫晏ｭ倥→螟画峩
                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
                // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝磯幕蟋・                // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・
                // 繧ｹ繧ｿ繝溘リ豸郁ｲｻ縺ｮ髢句ｧ具ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ StaminaSystem 縺ｨ縺ｮ騾｣謳ｺ・・                // 邯咏ｶ夂噪縺ｪ譖ｴ譁ｰ蜃ｦ逅・・髢句ｧ具ｼ・oroutine 縺ｾ縺溘・UpdateLoop・・            }

            executed = true;
        }

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝育憾諷九・譖ｴ譁ｰ・亥､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateSprint(float deltaTime)
        {
            if (!isActive) return;

            currentDuration += deltaTime;

            // 繧ｹ繧ｿ繝溘リ豸郁ｲｻ蜃ｦ逅・            float staminaConsumed = definition.staminaConsumptionRate * deltaTime;
            
            // 譛螟ｧ邯咏ｶ壽凾髢薙メ繧ｧ繝・け
            if (currentDuration >= definition.maxDuration)
            {
                EndSprint();
                return;
            }

            // 繧ｹ繧ｿ繝溘リ譫ｯ貂・メ繧ｧ繝・け
            if (definition.canInterruptOnStaminaDepleted)
            {
                // 螳滄圀縺ｮ螳溯｣・〒縺ｯ StaminaSystem 縺九ｉ縺ｮ蛟､繧貞盾辣ｧ
                // if (currentStamina <= 0f) EndSprint();
            }
        }

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝育憾諷九・邨ゆｺ・        /// </summary>
        public void EndSprint()
        {
            if (!isActive) return;

            isActive = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Sprint ended after {currentDuration:F1} seconds");
#endif

            // 騾溷ｺｦ縺ｮ蠕ｩ蜈・ｼ・aintainVelocityOnEnd縺ｫ蠢懊§縺ｦ・・            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
            // 繧ｨ繝輔ぉ繧ｯ繝医・蛛懈ｭ｢
            // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ髢句ｧ・        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医せ繝励Μ繝ｳ繝医・蠑ｷ蛻ｶ蛛懈ｭ｢・・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            EndSprint();

            // 豸郁ｲｻ縺励◆繧ｹ繧ｿ繝溘リ縺ｮ蠕ｩ蜈・ｼ磯Κ蛻・噪・・            // 迥ｶ諷九・螳悟・繝ｪ繧ｻ繝・ヨ

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Sprint command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// 繧ｹ繝励Μ繝ｳ繝医′迴ｾ蝨ｨ繧｢繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsActive => isActive;
    }
}