using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Collections;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ迥ｶ諷九ｒ邂｡逅・・ｽ・ｽ繧九け繝ｩ繧ｹ縲・    /// 縺難ｿｽE迥ｶ諷九〒縺ｯ縲・ｿｽE繝ｬ繧､繝､繝ｼ縺ｯ荳螳壽凾髢灘燕譁ｹ縺ｫ遘ｻ蜍輔＠縲∝､夜Κ縺九ｉ縺ｮ蜈･蜉帙ｒ蜿励￠莉倥￠縺ｾ縺帙ｓ縲・    /// </summary>
    public class RollingState : IPlayerState
    {
        private float rollSpeed = 8f;
        private float rollDuration = 1.0f;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ縺ｮ繧ｿ繧､繝橸ｿｽE繧帝幕蟋九＠縲√い繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繧偵ヨ繝ｪ繧ｬ繝ｼ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Entering Rolling State");
#endif
            stateMachine.StartCoroutine(RollTimer(stateMachine));
            
            // 繧｢繝九Γ繝ｼ繧ｿ繝ｼ繧貞叙蠕励＠縺ｦ繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繧帝幕蟋・
            // TODO: DetailedPlayerStateMachine縺ｫAnimator繝励Ο繝代ユ繧｣繧定ｿｽ蜉縺吶ｋ縺九√う繝吶Φ繝育ｵ檎罰縺ｧ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡縺吶ｋ蠢・ｦ√≠繧・
            // Animator animator = stateMachine.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     animator.SetTrigger("Roll");
            //     animator.SetBool("IsRolling", true);
            // }

            // 繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ荳ｭ縺ｯ辟｡謨ｵ譎る俣繧定ｨｭ螳夲ｼ井ｾ具ｼ壹ム繝｡繝ｼ繧ｸ繧貞女縺代↑縺・ｼ・
            // TODO: HealthComponent縺悟ｮ溯｣・＆繧後◆繧画怏蜉ｹ蛹・
            // var healthComponent = stateMachine.GetComponent<HealthComponent>();
            // if (healthComponent != null)
            // {
            //     healthComponent.SetInvulnerable(true, rollDuration);
            // }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・・ｽ・ｽ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Exiting Rolling State");
#endif
            
            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繝輔Λ繧ｰ繧偵Μ繧ｻ繝・ヨ
            // TODO: DetailedPlayerStateMachine縺ｫAnimator繝励Ο繝代ユ繧｣繧定ｿｽ蜉縺吶ｋ縺九√う繝吶Φ繝育ｵ檎罰縺ｧ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛻ｶ蠕｡縺吶ｋ蠢・ｦ√≠繧・
            // Animator animator = stateMachine.GetComponent<Animator>();
            // if (animator != null)
            // {
            //     animator.SetBool("IsRolling", false);
            // }
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ縺ｫ繧医ｋ遘ｻ蜍包ｿｽE逅・・ｽ・ｽ驕ｩ逕ｨ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Vector3 moveDirection = stateMachine.transform.forward * rollSpeed;

            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.deltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection * Time.deltaTime);
        }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶ら黄逅・・ｽ・ｽ邂暦ｿｽE譖ｴ譁ｰ縺ｫ菴ｿ逕ｨ縺輔ｌ縺ｾ縺吶・        /// 縺難ｿｽE迥ｶ諷九〒縺ｯ菴ｿ逕ｨ縺輔ｌ縺ｾ縺帙ｓ縲・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            // Not used
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・・ｽ・ｽ縺ｾ縺吶・        /// 繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ荳ｭ縺ｯ蜈･蜉帙ｒ辟｡隕悶＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍包ｿｽE蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝暦ｿｽE蜉帙・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // Ignore input during roll
        }

        /// <summary>
        /// 繝ｭ繝ｼ繝ｪ繝ｳ繧ｰ縺ｮ邯咏ｶ壽凾髢薙ｒ邂｡逅・・ｽ・ｽ縲∫ｵゆｺ・・ｽ・ｽ縺ｫ豁ｩ陦檎憾諷九↓驕ｷ遘ｻ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        private IEnumerator RollTimer(DetailedPlayerStateMachine stateMachine)
        {
            yield return new WaitForSeconds(rollDuration);
            stateMachine.TransitionToState(PlayerStateType.Walking);
        }
    }
}

