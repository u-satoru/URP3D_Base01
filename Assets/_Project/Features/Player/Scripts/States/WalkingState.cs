using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ豁ｩ陦檎憾諷九ｒ邂｡逅・・ｽ・ｽ繧九け繝ｩ繧ｹ縲・    /// </summary>
    public class WalkingState : IPlayerState
    {
        private float walkSpeed = 4f;
        private Vector2 _moveInput;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蟋ｿ蜍｢繧堤ｫ倶ｽ阪↓險ｭ螳壹＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・・ｽ・ｽ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 遘ｻ蜍包ｿｽE蜉帙ｒ繝ｪ繧ｻ繝・・ｽ・ｽ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            _moveInput = Vector2.zero;
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 蜈･蜉帙↓蝓ｺ縺･縺・・ｽ・ｽ繧ｭ繝｣繝ｩ繧ｯ繧ｿ繝ｼ縺ｮ遘ｻ蜍輔ｒ蜃ｦ逅・・ｽ・ｽ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            // NOTE: 縺薙ｌ縺ｯ邁｡逡･蛹悶＆繧後◆遘ｻ蜍募ｮ溯｣・〒縺吶・
            // 譛ｬ譚･縺ｯ繧ｫ繝｡繝ｩ縺ｮ蜷代″繧・溷ｺｦ縲・㍾蜉帙↑縺ｩ繧定・・縺吶ｋ蠢・ｦ√′縺ゅｊ縺ｾ縺吶・
            Vector3 moveDirection = stateMachine.transform.right * _moveInput.x + stateMachine.transform.forward * _moveInput.y;
            moveDirection.y = 0;

            // 謗･蝨ｰ縺励※縺・↑縺・ｴ蜷医・驥榊鴨繧帝←逕ｨ
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            stateMachine.CharacterController.Move(moveDirection.normalized * walkSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・・ｽ・ｽ縲∽ｻ厄ｿｽE迥ｶ諷九∈縺ｮ驕ｷ遘ｻ繧貞愛譁ｭ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍包ｿｽE蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝暦ｿｽE蜉帙・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            if (jumpInput)
            {
                stateMachine.TransitionToState(PlayerStateType.Jumping);
                return;
            }

            if (moveInput.magnitude < 0.1f)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
                return;
            }
            
            // State transitions - 繧ｹ繝励Μ繝ｳ繝茨ｿｽE蜉帙〒Running迥ｶ諷九↓驕ｷ遘ｻ
            var playerController = stateMachine.GetComponent<PlayerController>();
            if (playerController != null && playerController.IsSprintPressed)
            {
                stateMachine.TransitionToState(PlayerStateType.Running);
                return;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                stateMachine.TransitionToState(PlayerStateType.Crouching);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }
            
            if (Input.GetKeyDown(KeyCode.Q) && stateMachine.CoverSystem != null)
            {
                if (stateMachine.CoverSystem.GetAvailableCovers().Count > 0)
                {
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }
        }
    }
}

