using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ縺励ｃ縺後∩迥ｶ諷九ｒ邂｡逅・☆繧九け繝ｩ繧ｹ縲・    /// </summary>
    public class CrouchingState : IPlayerState
    {
        private float crouchSpeed = 2.5f;
        private float originalHeight;
        private float crouchHeight = 1.2f;
        private Vector2 _moveInput;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ鬮倥＆繧偵＠繧・′縺ｿ迥ｶ諷九↓螟画峩縺励∝ｧｿ蜍｢繧呈峩譁ｰ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = crouchHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = crouchHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
            }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・＠縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ鬮倥＆繧貞・縺ｮ鬮倥＆縺ｫ謌ｻ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                stateMachine.CharacterController.height = originalHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = originalHeight / 2f;
                stateMachine.CharacterController.center = center;
            }
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蜈･蜉帙↓蝓ｺ縺･縺・※繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ遘ｻ蜍輔ｒ蜃ｦ逅・＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.y = 0;

            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection.normalized * crouchSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・＠縲∽ｻ悶・迥ｶ諷九∈縺ｮ驕ｷ遘ｻ繧貞愛譁ｭ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍募・蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝怜・蜉幢ｼ医％縺ｮ迥ｶ諷九〒縺ｯ遶九■荳翫′繧翫↓菴ｿ逕ｨ・峨・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // 繧ｸ繝｣繝ｳ繝怜・蜉帙〒縺励ｃ縺後∩隗｣髯､・育ｫ九■荳翫′繧奇ｼ・            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // 遶九■荳翫′繧翫′蜿ｯ閭ｽ縺ｪ蝣ｴ蜷医∫ｧｻ蜍募・蜉帙↓蠢懊§縺ｦ驕ｩ蛻・↑迥ｶ諷九↓驕ｷ遘ｻ
                    if (moveInput.magnitude > 0.1f)
                    {
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                    return;
                }
            }
            
            // 縺励ｃ縺後∩迥ｶ諷九〒縺ｮ遘ｻ蜍輔・菴朱溽ｧｻ蜍輔→縺励※邯咏ｶ・            // 莉悶・蟋ｿ蜍｢縺ｸ縺ｮ驕ｷ遘ｻ・亥訣蛹舌↑縺ｩ・・            if (Input.GetKeyDown(KeyCode.X))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺檎ｫ九■荳翫′繧後ｋ縺九←縺・°・磯ｭ荳翫↓髫懷ｮｳ迚ｩ縺後↑縺・°・峨ｒ遒ｺ隱阪＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ縲・/param>
        /// <returns>遶九■荳翫′繧後ｋ蝣ｴ蜷医・true縲√◎縺・〒縺ｪ縺・ｴ蜷医・false縲・/returns>
        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
