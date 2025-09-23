using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 謾ｻ謦・さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ謾ｻ謦・い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 謾ｻ謦・婿蜷代→繝繝｡繝ｼ繧ｸ驥上・謖・ｮ・    /// - 謾ｻ謦・ち繧､繝暦ｼ郁ｿ第磁縲・□霍晞屬縲・ｭ疲ｳ慕ｭ会ｼ峨・邂｡逅・    /// - 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｨ繧ｹ繧ｿ繝溘リ豸郁ｲｻ縺ｮ閠・・
    /// - 騾｣邯壽判謦・ｼ医さ繝ｳ繝懶ｼ峨∈縺ｮ蟇ｾ蠢・    /// </summary>
    [System.Serializable]
    public class AttackCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 謾ｻ謦・・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum AttackType
        {
            Light,      // 霆ｽ謾ｻ謦・            Heavy,      // 蠑ｷ謾ｻ謦・            Special,    // 迚ｹ谿頑判謦・            Magic,      // 鬲疲ｳ墓判謦・            Ranged      // 驕霍晞屬謾ｻ謦・        }

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
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public AttackCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public AttackCommandDefinition(AttackType type, Vector3 attackDirection, float attackDamage = 10f)
        {
            attackType = type;
            direction = attackDirection;
            damage = attackDamage;
        }

        /// <summary>
        /// 謾ｻ謦・さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (context == null) return false;

            // 繧ｹ繧ｿ繝溘リ繝√ぉ繝・け縲√け繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け遲峨・縺薙％縺ｧ螳溯｣・            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√・繝ｬ繧､繝､繝ｼ繧БI縺ｮ繧ｹ繝・・繧ｿ繧ｹ繧貞盾辣ｧ
            
            return damage > 0f && range > 0f;
        }

        /// <summary>
        /// 謾ｻ謦・さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new AttackCommand(this, context);
        }
    }

    /// <summary>
    /// AttackCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 謾ｻ謦・さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.attackType} attack: {definition.damage} damage, {definition.range} range");
#endif

            // 螳滄圀縺ｮ謾ｻ謦・・逅・ｒ縺薙％縺ｫ螳溯｣・            // - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蜀咲函
            // - 蠖薙◆繧雁愛螳・            // - 繝繝｡繝ｼ繧ｸ險育ｮ・            // - 繧ｨ繝輔ぉ繧ｯ繝育函謌・
            executed = true;
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ域判謦・・蜿悶ｊ豸医＠縺ｯ騾壼ｸｸ荳榊庄閭ｽ・・        /// </summary>
        public void Undo()
        {
            // 謾ｻ謦・・騾壼ｸｸ蜿悶ｊ豸医＠荳榊庄
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.LogWarning("Attack commands cannot be undone");
#endif
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => false;
    }
}