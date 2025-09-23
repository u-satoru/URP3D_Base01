using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ莨上○・ｽE・ｽ蛹榊倹・臥憾諷九ｒ邂｡逅・・ｽ・ｽ繧九け繝ｩ繧ｹ縲・    /// </summary>
    public class ProneState : IPlayerState
    {
        private float proneSpeed = 1f;
        private float originalHeight;
        private float proneHeight = 0.5f;
        private Vector2 _moveInput;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ鬮倥＆繧剃ｼ上○迥ｶ諷九↓螟画峩縺励∝ｧｿ蜍｢繧呈峩譁ｰ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = proneHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = proneHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Prone);
            }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・・ｽ・ｽ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ鬮倥＆繧抵ｿｽE縺ｮ鬮倥＆縺ｫ謌ｻ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
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
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蜈･蜉帙↓蝓ｺ縺･縺・・ｽ・ｽ繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ遘ｻ蜍輔ｒ蜃ｦ逅・・ｽ・ｽ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
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

            stateMachine.CharacterController.Move(moveDirection.normalized * proneSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・・ｽ・ｽ縲∽ｻ厄ｿｽE迥ｶ諷九∈縺ｮ驕ｷ遘ｻ繧貞愛譁ｭ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍包ｿｽE蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝暦ｿｽE蜉幢ｼ医％縺ｮ迥ｶ諷九〒縺ｯ遶九■荳翫′繧翫↓菴ｿ逕ｨ・ｽE・ｽ縲・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;
            
            // 繧ｸ繝｣繝ｳ繝暦ｿｽE蜉帙〒蛹榊倹隗｣髯､・ｽE・ｽ遶九■荳翫′繧奇ｼ・            if (jumpInput)
            {
                if (CanStandUp(stateMachine))
                {
                    // 逶ｴ謗･遶九■荳翫′繧翫′蜿ｯ閭ｽ縺ｪ蝣ｴ蜷・
                    if (moveInput.magnitude > 0.1f)
                    {
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                }
                else
                {
                    // 髫懷ｮｳ迚ｩ縺後≠繧句ｴ蜷茨ｿｽE縺励ｃ縺後∩迥ｶ諷九∈
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                }
                return;
            }
            
            // 蛹榊倹迥ｶ諷九°繧峨＠繧・′縺ｿ縺ｸ縺ｮ驕ｷ遘ｻ・域ｮｵ髫守噪縺ｪ遶九■荳翫′繧奇ｼ・
            if (Input.GetKeyDown(KeyCode.C))
            {
                stateMachine.TransitionToState(PlayerStateType.Crouching);
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺檎ｫ九■荳翫′繧後ｋ縺九←縺・・ｽ・ｽ・ｽE・ｽ鬆ｭ荳翫↓髫懷ｮｳ迚ｩ縺後↑縺・・ｽ・ｽ・ｽE・ｽ繧堤｢ｺ隱阪＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <returns>遶九■荳翫′繧後ｋ蝣ｴ蜷茨ｿｽEtrue縲√◎縺・・ｽ・ｽ縺ｪ縺・・ｽ・ｽ蜷茨ｿｽEfalse縲・/returns>
        private bool CanStandUp(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = originalHeight - proneHeight;

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺後＠繧・・ｽ・ｽ縺ｿ迥ｶ諷九↓縺ｪ繧後ｋ縺九←縺・・ｽ・ｽ繧堤｢ｺ隱阪＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <returns>縺励ｃ縺後ａ繧句ｴ蜷茨ｿｽEtrue縲√◎縺・・ｽ・ｽ縺ｪ縺・・ｽ・ｽ蜷茨ｿｽEfalse縲・/returns>
        private bool CanCrouch(DetailedPlayerStateMachine stateMachine)
        {
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = 1.2f - proneHeight; // Assuming crouch height is 1.2f

            return !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);
        }
    }
}
