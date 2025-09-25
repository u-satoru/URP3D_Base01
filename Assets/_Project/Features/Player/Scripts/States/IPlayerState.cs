using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜷・憾諷九′螳溯｣・☆縺ｹ縺阪う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｧ縺吶・    /// 繧ｹ繝・・繝医・繧ｷ繝ｳ縺ｯ縲√％縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧剃ｻ九＠縺ｦ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ蛻ｶ蠕｡縺励∪縺吶・    /// </summary>
    public interface IPlayerState
    {
        /// <summary>
        /// 縺薙・迥ｶ諷九↓驕ｷ遘ｻ縺励◆髫帙↓荳蠎ｦ縺縺大他縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蛻晄悄蛹門・逅・ｒ縺薙％縺ｧ陦後＞縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        void Enter(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// 縺薙・迥ｶ諷九°繧我ｻ悶・迥ｶ諷九↓驕ｷ遘ｻ縺吶ｋ髫帙↓荳蠎ｦ縺縺大他縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 邨ゆｺ・・逅・ｒ縺薙％縺ｧ陦後＞縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        void Exit(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// 縺薙・迥ｶ諷九〒縺ゅｋ髢薙∵ｯ弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 荳ｻ縺ｫ譎る俣邨碁℃繧・・蜉帙↓萓晏ｭ倥＠縺ｪ縺・Ο爨憫､ｿ爨輔ｒ蜃ｦ逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        void Update(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// 縺薙・迥ｶ諷九〒縺ゅｋ髢薙∝崋螳壽凾髢馴俣髫斐〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 荳ｻ縺ｫ迚ｩ逅・ｼ皮ｮ鈴未騾｣縺ｮ繝ｭ繧ｸ繝・け繧貞・逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        void FixedUpdate(DetailedPlayerStateMachine stateMachine);

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺九ｉ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍募・蜉帙・繧ｯ繝医Ν縲・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝怜・蜉帙ヵ繝ｩ繧ｰ縲・/param>
        void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput);
    }
}
