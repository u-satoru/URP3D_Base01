using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 遘ｻ蜍輔さ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ遘ｻ蜍輔い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 遘ｻ蜍墓婿蜷代→騾溷ｺｦ縺ｮ謖・ｮ・    /// - 遘ｻ蜍輔ち繧､繝暦ｼ域ｭｩ縺阪∬ｵｰ繧翫∝ｿ阪・豁ｩ縺咲ｭ会ｼ峨・邂｡逅・    /// - 遘ｻ蜍募宛邏・ｼ亥慍蠖｢縲・囿螳ｳ迚ｩ遲会ｼ峨・閠・・
    /// - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繝悶Ξ繝ｳ繝・ぅ繝ｳ繧ｰ縺ｨ縺ｮ騾｣謳ｺ
    /// </summary>
    [System.Serializable]
    public class MoveCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 遘ｻ蜍輔・遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum MoveType
        {
            Walk,       // 豁ｩ縺・            Run,        // 襍ｰ繧・ 
            Sprint,     // 繝繝・す繝･
            Sneak,      // 蠢阪・豁ｩ縺・            Strafe      // 讓ｪ豁ｩ縺・        }

        [Header("Movement Parameters")]
        public MoveType moveType = MoveType.Walk;
        public Vector3 direction = Vector3.forward;
        public float speed = 5f;
        public float duration = 1f;

        [Header("Movement Constraints")]
        public bool respectGravity = true;
        public bool checkCollisions = true;
        public LayerMask obstacleLayer = -1;

        [Header("Animation")]
        public bool useRootMotion = false;
        public float blendTime = 0.2f;

        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public MoveCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public MoveCommandDefinition(MoveType type, Vector3 moveDirection, float moveSpeed = 5f)
        {
            moveType = type;
            direction = moveDirection.normalized;
            speed = moveSpeed;
        }

        /// <summary>
        /// 遘ｻ蜍輔さ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (speed <= 0f || duration <= 0f) return false;
            
            // 遘ｻ蜍墓婿蜷代・繝√ぉ繝・け
            if (direction == Vector3.zero) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・AI縺ｮ迥ｶ諷九メ繧ｧ繝・け
                // 萓具ｼ夐ｺｻ逞ｺ迥ｶ諷九√せ繧ｿ繝ｳ迥ｶ諷狗ｭ峨〒縺ｮ遘ｻ蜍穂ｸ榊庄蛻､螳・            }

            return true;
        }

        /// <summary>
        /// 遘ｻ蜍輔さ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new MoveCommand(this, context);
        }
    }

    /// <summary>
    /// MoveCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
    public class MoveCommand : ICommand
    {
        private MoveCommandDefinition definition;
        private object context;
        private bool executed = false;
        private Vector3 originalPosition;

        public MoveCommand(MoveCommandDefinition moveDefinition, object executionContext)
        {
            definition = moveDefinition;
            context = executionContext;
        }

        /// <summary>
        /// 遘ｻ蜍輔さ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 螳溯｡悟燕縺ｮ菴咲ｽｮ繧剃ｿ晏ｭ假ｼ・ndo逕ｨ・・            if (context is MonoBehaviour mono && mono.transform != null)
            {
                originalPosition = mono.transform.position;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.moveType} movement: {definition.direction} direction, {definition.speed} speed");
#endif

            // 螳滄圀縺ｮ遘ｻ蜍募・逅・ｒ縺薙％縺ｫ螳溯｣・            // - Transform謫堺ｽ懊∪縺溘・Rigidbody謫堺ｽ・            // - 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡
            // - 迚ｩ逅・｡晉ｪ√メ繧ｧ繝・け
            // - 繧ｨ繝輔ぉ繧ｯ繝亥・逕・
            executed = true;
        }

        /// <summary>
        /// Undo謫堺ｽ懶ｼ育ｧｻ蜍輔・蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed || context == null) return;

            if (context is MonoBehaviour mono && mono.transform != null)
            {
                mono.transform.position = originalPosition;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Movement undone - returned to original position");
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
