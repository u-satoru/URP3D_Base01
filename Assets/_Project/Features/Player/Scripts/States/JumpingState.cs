
using UnityEngine;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧ｸ繝｣繝ｳ繝礼憾諷九ｒ邂｡逅・・ｽ・ｽ繧九け繝ｩ繧ｹ縲・    /// </summary>
    public class JumpingState : IPlayerState
    {
        private Vector3 _playerVelocity;
        private bool _isGrounded;
        private float _jumpHeight = 1.5f;
        private float _gravityValue = -9.81f;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繝励Ξ繧､繝､繝ｼ縺ｫ荳頑婿蜷托ｿｽE騾溷ｺｦ繧剃ｸ弱∴縺ｦ繧ｸ繝｣繝ｳ繝励＆縺帙∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            _isGrounded = false;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
            
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Standing);
            }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・・ｽ・ｽ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 逹蝨ｰ蛻､螳壹ｒ陦後＞縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            _isGrounded = stateMachine.CharacterController.isGrounded;
            if (_isGrounded && _playerVelocity.y < 0)
            {
                stateMachine.TransitionToState(PlayerStateType.Idle);
            }
        }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 驥榊鴨繧帝←逕ｨ縺励・ｿｽE繝ｬ繧､繝､繝ｼ繧堤ｧｻ蜍輔＆縺帙∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            _playerVelocity.y += _gravityValue * Time.fixedDeltaTime;
            stateMachine.CharacterController.Move(_playerVelocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・・ｽ・ｽ縺ｾ縺吶ゑｼ医％縺ｮ迥ｶ諷九〒縺ｯ蜈･蜉幢ｿｽE辟｡隕悶＆繧後∪縺呻ｼ・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍包ｿｽE蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝暦ｿｽE蜉帙・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            // 繧ｸ繝｣繝ｳ繝嶺ｸｭ縺ｯ莉悶・蜈･蜉帙ｒ蜿励￠莉倥￠縺ｪ縺・
        }
    }
}

