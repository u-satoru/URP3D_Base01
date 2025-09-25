using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜷・憾諷九・蝓ｺ蠎輔け繝ｩ繧ｹ縺ｧ縺吶・    /// IPlayerState繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繧貞ｮ溯｣・＠縲∝・騾壹・謖ｯ繧玖・縺・ｄ迥ｶ諷狗ｮ｡逅・ｒ謠蝉ｾ帙＠縺ｾ縺吶・    /// </summary>
    public abstract class BasePlayerState : IPlayerState
    {
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷九↓蜈･縺｣縺ｦ縺九ｉ縺ｮ邨碁℃譎る俣縲・        /// </summary>
        protected float stateTime;

        /// <summary>
        /// 縺薙・迥ｶ諷九↓驕ｷ遘ｻ縺励◆髫帙↓荳蠎ｦ縺縺大他縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 迥ｶ諷九・蛻晄悄蛹門・逅・ｒ陦後＞縲《tateTime繧偵Μ繧ｻ繝・ヨ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public virtual void Enter(DetailedPlayerStateMachine stateMachine)
        {
            stateTime = 0f;
        }

        /// <summary>
        /// 縺薙・迥ｶ諷九°繧我ｻ悶・迥ｶ諷九↓驕ｷ遘ｻ縺吶ｋ髫帙↓荳蠎ｦ縺縺大他縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 迥ｶ諷九・邨ゆｺ・・逅・ｒ陦後＞縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public virtual void Exit(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 縺薙・迥ｶ諷九〒縺ゅｋ髢薙∵ｯ弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// stateTime繧呈峩譁ｰ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public virtual void Update(DetailedPlayerStateMachine stateMachine)
        {
            stateTime += Time.deltaTime;
        }

        /// <summary>
        /// 縺薙・迥ｶ諷九〒縺ゅｋ髢薙∝崋螳壽凾髢馴俣髫斐〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 迚ｩ逅・ｼ皮ｮ鈴未騾｣縺ｮ繝ｭ繧ｸ繝・け繧貞・逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public virtual void FixedUpdate(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺九ｉ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">縺薙・繧ｹ繝・・繝医ｒ邂｡逅・☆繧九せ繝・・繝医・繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍募・蜉帙・繧ｯ繝医Ν縲・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝怜・蜉帙ヵ繝ｩ繧ｰ縲・/param>
        public virtual void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput) { }
    }

    // NOTE: The following states now correctly implement IPlayerState through inheritance,
    // which should resolve the compilation errors. Their internal logic may need
    // to be updated later to use the new command-based input.

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ襍ｰ陦檎憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class SprintingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsSprinting", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsSprinting", false);
        }
    }


    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ關ｽ荳狗憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class FallingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsFalling", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsFalling", false);
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ逹蝨ｰ迥ｶ諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class LandingState : BasePlayerState
    {
        /// <summary>
        /// 逹蝨ｰ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ謖∫ｶ壽凾髢薙・        /// </summary>
        private float landingDuration = 0.3f;

        public override void Update(DetailedPlayerStateMachine stateMachine)
        {
            base.Update(stateMachine);

            if (stateTime >= landingDuration)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ謌ｦ髣倡憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class CombatState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("InCombat", true);
            // animator?.SetLayerWeight(1, 1f);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("InCombat", false);
            // animator?.SetLayerWeight(1, 0f);
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ謌ｦ髣伜ｾ・ｩ溽憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class CombatIdleState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("CombatIdle", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("CombatIdle", false);
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ謌ｦ髣俶判謦・憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class CombatAttackingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧､繝ｳ繧ｿ繝ｩ繧ｯ繝育憾諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class InteractingState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetBool("IsInteracting", true);
        }

        public override void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // animator?.SetBool("IsInteracting", false);
        }
    }

    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ豁ｻ莠｡迥ｶ諷九ｒ邂｡逅・＠縺ｾ縺吶・    /// </summary>
    public class DeadState : BasePlayerState
    {
        public override void Enter(DetailedPlayerStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            // animator?.SetTrigger("Death");
        }
    }
}
