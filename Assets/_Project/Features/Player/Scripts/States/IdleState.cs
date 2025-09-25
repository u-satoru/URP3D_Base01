using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ蠕・ｩ溽憾諷九ｒ邂｡逅・☆繧九け繝ｩ繧ｹ
    /// </summary>
    /// <remarks>
    /// 險ｭ險域晄Φ・・    /// 縺薙・繧ｯ繝ｩ繧ｹ縺ｯState繝代ち繝ｼ繝ｳ縺ｮ蜈ｷ菴鍋噪縺ｪ迥ｶ諷具ｼ・oncreteState・峨→縺励※螳溯｣・＆繧後※縺・∪縺吶・    /// 繝励Ξ繧､繝､繝ｼ縺碁撕豁｢縺励※縺・ｋ髫帙・蝓ｺ譛ｬ迥ｶ諷九〒縺ゅｊ縲∽ｻ悶・蜈ｨ縺ｦ縺ｮ迥ｶ諷九∈縺ｮ驕ｷ遘ｻ縺ｮ蜃ｺ逋ｺ轤ｹ縺ｨ縺ｪ繧翫∪縺吶・    /// 
    /// 迥ｶ諷矩・遘ｻ譚｡莉ｶ・・    /// - 遘ｻ蜍募・蜉幢ｼ・oveInput.magnitude > 0.1f・俄・ Walking迥ｶ諷・    /// - 繧ｸ繝｣繝ｳ繝怜・蜉幢ｼ・umpInput == true・俄・ Jumping迥ｶ諷・    /// 
    /// 迥ｶ諷九・迚ｹ蠕ｴ・・    /// - 繧｢繧､繝峨Ν譎る俣縺ｮ險域ｸｬ・亥ｰ・擂逧・↑繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡繧・音谿雁虚菴懊・繝医Μ繧ｬ繝ｼ縺ｫ菴ｿ逕ｨ蜿ｯ閭ｽ・・    /// - Standing蟋ｿ蜍｢縺ｮ險ｭ螳夲ｼ医せ繝・Ν繧ｹ邉ｻ縺ｨ縺ｮ騾｣謳ｺ・・    /// - 譛蟆丞・蜉幃明蛟､・・.1f・峨↓繧医ｋ隱､蜈･蜉帙ヵ繧｣繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ
    /// 
    /// 菴ｿ逕ｨ萓具ｼ・    /// 繧ｹ繝・・繝医・繧ｷ繝ｳ縺栗dleState縺ｫ驕ｷ遘ｻ縺吶ｋ縺ｨ縲√・繝ｬ繧､繝､繝ｼ縺ｯ遶倶ｽ榊ｧｿ蜍｢縺ｧ髱呎ｭ｢縺励・    /// 蜈･蜉帛ｾ・■迥ｶ諷九→縺ｪ繧翫∪縺吶る←蛻・↑蜈･蜉帙′讀懷・縺輔ｌ繧九→莉悶・迥ｶ諷九↓驕ｷ遘ｻ縺励∪縺吶・    /// </remarks>
    public class IdleState : IPlayerState
    {
        private float idleTime;
        
        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蠕・ｩ滓凾髢薙ｒ繝ｪ繧ｻ繝・ヨ縺励√・繝ｬ繧､繝､繝ｼ縺ｮ蟋ｿ蜍｢繧堤ｫ倶ｽ阪↓險ｭ螳壹＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            idleTime = 0f;
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }
        
        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・＠縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }
        
        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蠕・ｩ滓凾髢薙ｒ繧ｫ繧ｦ繝ｳ繝医い繝・・縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            idleTime += Time.deltaTime;
        }
        
        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・＠縲∽ｻ悶・迥ｶ諷九∈縺ｮ驕ｷ遘ｻ繧貞愛譁ｭ縺励∪縺・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ</param>
        /// <param name="moveInput">遘ｻ蜍募・蜉幢ｼ・霆ｸ・壼ｷｦ蜿ｳ縲〆霆ｸ・壼燕蠕鯉ｼ・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝怜・蜉帙ヵ繝ｩ繧ｰ</param>
        /// <remarks>
        /// 驕ｷ遘ｻ蜆ｪ蜈磯・ｽ搾ｼ・        /// 1. 繧ｸ繝｣繝ｳ繝怜・蜉幢ｼ壽怙蜆ｪ蜈医〒Jumping迥ｶ諷九↓驕ｷ遘ｻ・・eturn譁・〒蜃ｦ逅・ｵゆｺ・ｼ・        /// 2. 遘ｻ蜍募・蜉幢ｼ壼・蜉帛ｼｷ蠎ｦ縺碁明蛟､・・.1f・峨ｒ雜・∴縺溷ｴ蜷医仝alking迥ｶ諷九↓驕ｷ遘ｻ
        /// 
        /// 險ｭ險井ｸ翫・閠・・莠矩・ｼ・        /// - 繧ｸ繝｣繝ｳ繝励ｒ遘ｻ蜍輔ｈ繧雁━蜈医☆繧九％縺ｨ縺ｧ縲∫ｧｻ蜍輔＠縺ｪ縺後ｉ縺ｮ繧ｸ繝｣繝ｳ繝励〒繧ら｢ｺ螳溘↓繧ｸ繝｣繝ｳ繝励′螳溯｡後＆繧後ｋ
        /// - 0.1f縺ｮ髢ｾ蛟､縺ｫ繧医ｊ縲√さ繝ｳ繝医Ο繝ｼ繝ｩ繝ｼ縺ｮ繝峨Μ繝輔ヨ繧・ｾｮ蟆上↑蜈･蜉帙ヮ繧､繧ｺ繧帝勁蜴ｻ
        /// - 迥ｶ諷矩・遘ｻ縺ｯTransitionToState繝｡繧ｽ繝・ラ繧帝壹§縺ｦ陦後＞縲・←蛻・↑Enter/Exit蜃ｦ逅・ｒ菫晁ｨｼ
        /// 
        /// 豕ｨ諢丈ｺ矩・ｼ・        /// - 縺薙・迥ｶ諷九〒縺ｯ遘ｻ蜍募・逅・・陦後ｏ縺壹∫憾諷矩・遘ｻ縺ｮ蛻､螳壹・縺ｿ螳溯｡・        /// - 螳滄圀縺ｮ遘ｻ蜍募・逅・・驕ｷ遘ｻ蜈医・迥ｶ諷具ｼ・alking遲会ｼ峨〒螳溯｣・        /// </remarks>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            if (jumpInput)
            {
                stateMachine.TransitionToState(PlayerStateType.Jumping);
                return;
            }

            if (moveInput.magnitude > 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateType.Walking);
            }
        }
    }
}

