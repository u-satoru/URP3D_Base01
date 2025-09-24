using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧ｸ繝｣繝ｳ繝励さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ繧ｸ繝｣繝ｳ繝励い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｸ繝｣繝ｳ繝怜鴨縺ｨ譁ｹ蜷代・謖・ｮ・    /// - 繧ｸ繝｣繝ｳ繝励ち繧､繝暦ｼ磯壼ｸｸ縲∽ｺ梧ｮｵ縲∝｣√・聞霍晞屬遲会ｼ峨・邂｡逅・    /// - 逹蝨ｰ蛻､螳壹→逹蝨ｰ蠕後・蜃ｦ逅・    /// - 繧ｹ繧ｿ繝溘リ豸郁ｲｻ縺ｨ繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ縺ｮ閠・・
    /// </summary>
    [System.Serializable]
    public class JumpCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝励・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum JumpType
        {
            Normal,     // 騾壼ｸｸ繧ｸ繝｣繝ｳ繝・            Double,     // 莠梧ｮｵ繧ｸ繝｣繝ｳ繝・            Wall,       // 螢√ず繝｣繝ｳ繝・            Long,       // 髟ｷ霍晞屬繧ｸ繝｣繝ｳ繝・            High        // 鬮倥ず繝｣繝ｳ繝・        }

        [Header("Jump Parameters")]
        public JumpType jumpType = JumpType.Normal;
        public float jumpForce = 10f;
        public Vector3 direction = Vector3.up;
        public float horizontalBoost = 0f;

        [Header("Physics")]
        public float gravityScale = 1f;
        public float airControlMultiplier = 0.5f;
        public bool resetVerticalVelocity = true;

        [Header("Constraints")]
        public bool requiresGrounded = true;
        public float staminaCost = 20f;
        public float cooldownTime = 0.5f;

        [Header("Animation")]
        public float jumpAnimationDuration = 0.3f;
        public float landAnimationDuration = 0.2f;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public JumpCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public JumpCommandDefinition(JumpType type, float force, Vector3 jumpDirection = default)
        {
            jumpType = type;
            jumpForce = force;
            direction = jumpDirection == default ? Vector3.up : jumpDirection.normalized;
        }

        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝励さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (jumpForce <= 0f) return false;
            
            // 譁ｹ蜷代・繧ｯ繝医Ν縺ｮ繝√ぉ繝・け
            if (direction == Vector3.zero) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 蝨ｰ髱｢蛻､螳壹メ繧ｧ繝・け・・equiresGrounded縺梧怏蜉ｹ縺ｮ蝣ｴ蜷茨ｼ・                // 繧ｹ繧ｿ繝溘リ繝√ぉ繝・け
                // 繧ｯ繝ｼ繝ｫ繝繧ｦ繝ｳ繝√ぉ繝・け
                // 迥ｶ諷狗焚蟶ｸ繝√ぉ繝・け・磯ｺｻ逞ｺ縲√せ繧ｿ繝ｳ遲会ｼ・            }

            return true;
        }

        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝励さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new JumpCommand(this, context);
        }
    }

    /// <summary>
    /// JumpCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class JumpCommand : ICommand
    {
        private JumpCommandDefinition definition;
        private object context;
        private bool executed = false;
        private Vector3 originalVelocity;
        private bool wasGrounded;

        public JumpCommand(JumpCommandDefinition jumpDefinition, object executionContext)
        {
            definition = jumpDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 繧ｸ繝｣繝ｳ繝励さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 螳溯｡悟燕縺ｮ迥ｶ諷九ｒ菫晏ｭ假ｼ・ndo逕ｨ・・            if (context is MonoBehaviour mono && mono.GetComponent<Rigidbody>() != null)
            {
                var rb = mono.GetComponent<Rigidbody>();
                originalVelocity = rb.linearVelocity;
                // 蝨ｰ髱｢蛻､螳壹・菫晏ｭ假ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ GroundCheck 繧ｳ繝ｳ繝昴・繝阪Φ繝育ｭ峨ｒ蜿ら・・・            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.jumpType} jump: {definition.jumpForce} force, {definition.direction} direction");
#endif

            // 螳滄圀縺ｮ繧ｸ繝｣繝ｳ繝怜・逅・ｒ縺薙％縺ｫ螳溯｣・            if (context is MonoBehaviour monoBehaviour && monoBehaviour.GetComponent<Rigidbody>() != null)
            {
                var rb = monoBehaviour.GetComponent<Rigidbody>();
                
                // 蝙ら峩騾溷ｺｦ縺ｮ繝ｪ繧ｻ繝・ヨ
                if (definition.resetVerticalVelocity)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                }

                // 繧ｸ繝｣繝ｳ繝怜鴨縺ｮ驕ｩ逕ｨ
                Vector3 jumpVelocity = definition.direction.normalized * definition.jumpForce;
                
                // 豌ｴ蟷ｳ繝悶・繧ｹ繝医・霑ｽ蜉
                if (definition.horizontalBoost > 0f)
                {
                    Vector3 horizontalDirection = new Vector3(definition.direction.x, 0f, definition.direction.z).normalized;
                    jumpVelocity += horizontalDirection * definition.horizontalBoost;
                }

                rb.AddForce(jumpVelocity, ForceMode.VelocityChange);

                // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
                // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・                // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            }

            executed = true;
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ医ず繝｣繝ｳ繝励・蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed || context == null) return;

            if (context is MonoBehaviour mono && mono.GetComponent<Rigidbody>() != null)
            {
                var rb = mono.GetComponent<Rigidbody>();
                rb.linearVelocity = originalVelocity;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Jump undone - velocity restored");
#endif
            }

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && context != null;
    }
}
