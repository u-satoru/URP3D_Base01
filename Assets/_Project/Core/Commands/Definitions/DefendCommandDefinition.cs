using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 髦ｲ蠕｡繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ髦ｲ蠕｡繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｬ繝ｼ繝画婿蜷代→蠑ｷ蠎ｦ縺ｮ謖・ｮ・    /// - 髦ｲ蠕｡繧ｿ繧､繝暦ｼ医ヶ繝ｭ繝・け縲∝屓驕ｿ縲√き繧ｦ繝ｳ繧ｿ繝ｼ遲会ｼ峨・邂｡逅・    /// - 繧ｹ繧ｿ繝溘リ豸郁ｲｻ縺ｨ繝繝｡繝ｼ繧ｸ霆ｽ貂帷紫縺ｮ險育ｮ・    /// - 繝代Μ繧｣繧・き繧ｦ繝ｳ繧ｿ繝ｼ謾ｻ謦・∈縺ｮ蟇ｾ蠢・    /// </summary>
    [System.Serializable]
    public class DefendCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 髦ｲ蠕｡縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum DefendType
        {
            Block,      // 繝悶Ο繝・け・育崟繧・ｭｦ蝎ｨ縺ｧ縺ｮ髦ｲ蠕｡・・            Dodge,      // 蝗樣∩
            Parry,      // 繝代Μ繧｣・亥渚謦・ｼ・            Counter,    // 繧ｫ繧ｦ繝ｳ繧ｿ繝ｼ謾ｻ謦・            Absorb      // 繝繝｡繝ｼ繧ｸ蜷ｸ蜿・        }

        [Header("Defense Parameters")]
        public DefendType defendType = DefendType.Block;
        public Vector3 guardDirection = Vector3.forward;
        public float blockStrength = 0.7f; // 繝繝｡繝ｼ繧ｸ霆ｽ貂帷紫 (0.0-1.0)
        public float guardAngle = 90f; // 髦ｲ蠕｡蜿ｯ閭ｽ隗貞ｺｦ

        [Header("Resource Costs")]
        public float staminaCost = 10f;
        public float staminaDrainRate = 5f; // 邯咏ｶ夐亟蠕｡譎ゅ・豈守ｧ呈ｶ郁ｲｻ

        [Header("Timing Windows")]
        public float activationTime = 0.2f; // 髦ｲ蠕｡髢句ｧ九∪縺ｧ縺ｮ譎る俣
        public float perfectBlockWindow = 0.1f; // 繝代・繝輔ぉ繧ｯ繝医ヶ繝ｭ繝・け縺ｮ遯捺凾髢・        public float parryWindow = 0.15f; // 繝代Μ繧｣縺ｮ遯捺凾髢・
        [Header("Combat Mechanics")]
        public bool allowsPerfectBlock = true;
        public bool canParryProjectiles = false;
        public float counterDamageMultiplier = 1.5f;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public DefendCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public DefendCommandDefinition(DefendType type, Vector3 direction, float strength = 0.7f)
        {
            defendType = type;
            guardDirection = direction;
            blockStrength = Mathf.Clamp01(strength);
        }

        /// <summary>
        /// 髦ｲ蠕｡繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            if (context == null) return false;

            // 繧ｹ繧ｿ繝溘リ繝√ぉ繝・け縲∫憾諷九メ繧ｧ繝・け遲・            return blockStrength > 0f && staminaCost >= 0f;
        }

        /// <summary>
        /// 髦ｲ蠕｡繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new DefendCommand(this, context);
        }
    }

    /// <summary>
    /// DefendCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 髦ｲ蠕｡繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.defendType} defense: {definition.blockStrength * 100}% damage reduction");
#endif

            // 螳滄圀縺ｮ髦ｲ蠕｡蜃ｦ逅・ｒ縺薙％縺ｫ螳溯｣・            // - 髦ｲ蠕｡繝昴・繧ｺ縺ｮ髢句ｧ・            // - 繝繝｡繝ｼ繧ｸ霆ｽ貂帛柑譫懊・驕ｩ逕ｨ
            // - 繧ｹ繧ｿ繝溘リ豸郁ｲｻ
            // - 繧ｨ繝輔ぉ繧ｯ繝育函謌・
            isDefending = true;
            executed = true;
        }

        /// <summary>
        /// 髦ｲ蠕｡縺ｮ邨ゆｺ・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Ending {definition.defendType} defense");
#endif

            // 髦ｲ蠕｡迥ｶ諷九・邨ゆｺ・・逅・            isDefending = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°・磯亟蠕｡邨ゆｺ・→縺励※讖溯・・・        /// </summary>
        public bool CanUndo => executed && isDefending;
    }
}
