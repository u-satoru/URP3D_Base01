using UnityEngine;
// using asterivo.Unity60.Core.Commands;
// using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｫ繝繝｡繝ｼ繧ｸ繧剃ｸ弱∴繧九い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繝繝｡繝ｼ繧ｸ驥上→遞ｮ鬘槭・謖・ｮ・    /// - 繝繝｡繝ｼ繧ｸ繧ｽ繝ｼ繧ｹ縺ｮ邂｡逅・    /// - 迥ｶ諷狗焚蟶ｸ縺ｮ莉倅ｸ・    /// - 繝繝｡繝ｼ繧ｸ霆ｽ貂帛柑譫懊∈縺ｮ蟇ｾ蠢・    /// </summary>
    [System.Serializable]
    public class DamageCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum DamageType
        {
            Physical,   // 迚ｩ逅・ム繝｡繝ｼ繧ｸ
            Fire,       // 轣ｫ轤弱ム繝｡繝ｼ繧ｸ
            Ice,        // 豌ｷ邨舌ム繝｡繝ｼ繧ｸ
            Lightning,  // 髮ｻ謦・ム繝｡繝ｼ繧ｸ
            Poison,     // 豈偵ム繝｡繝ｼ繧ｸ
            Pure        // 邏皮ｲ九ム繝｡繝ｼ繧ｸ・郁ｻｽ貂帑ｸ榊庄・・        }

        [Header("Damage Parameters")]
        public DamageType damageType = DamageType.Physical;
        public float damageAmount = 10f;
        public bool canKill = true;
        public float armorPenetration = 0f;

        [Header("Effects")]
        public bool applyKnockback = false;
        public Vector3 knockbackDirection = Vector3.zero;
        public float knockbackForce = 5f;

        [Header("Status Effects")]
        public bool appliesStatusEffect = false;
        public float statusDuration = 0f;

        [Header("Visual/Audio")]
        public bool showDamageNumbers = true;
        public Color damageColor = Color.red;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public DamageCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public DamageCommandDefinition(float damage, DamageType type = DamageType.Physical)
        {
            damageAmount = damage;
            damageType = type;
        }

        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (damageAmount <= 0f) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繧ｿ繝ｼ繧ｲ繝・ヨ縺ｮ逕溷ｭ倡憾諷九メ繧ｧ繝・け遲・            }

            return true;
        }

        /// <summary>
        /// 繝繝｡繝ｼ繧ｸ繧ｳ繝槭Φ繝峨・繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;
                
            if (context is IHealthTarget target)
            {
                return new DamageCommand(target, Mathf.RoundToInt(damageAmount), damageType.ToString().ToLower());
            }
            
            return null;
        }

        /// <summary>
        /// 螳滄圀縺ｮ繝繝｡繝ｼ繧ｸ驥上ｒ險育ｮ励＠縺ｾ縺呻ｼ磯亟蠕｡蜉帷ｭ峨ｒ閠・・・・        /// </summary>
        public float CalculateActualDamage(float targetDefense = 0f)
        {
            float actualDamage = damageAmount;
            
            // 髦ｲ蠕｡蜉帙↓繧医ｋ霆ｽ貂・            if (armorPenetration < 1f)
            {
                float effectiveDefense = targetDefense * (1f - armorPenetration);
                actualDamage = Mathf.Max(1f, actualDamage - effectiveDefense);
            }
            
            return actualDamage;
        }
    }
}
