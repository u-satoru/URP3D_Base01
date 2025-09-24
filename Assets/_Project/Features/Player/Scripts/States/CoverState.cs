using UnityEngine;
using Debug = UnityEngine.Debug;
using asterivo.Unity60.Features.Player.Commands;

namespace asterivo.Unity60.Features.Player.States
{
    /// <summary>
    /// 繝励Ξ繧､繝､繝ｼ縺後き繝撰ｿｽE・ｽE・ｽ迚ｩ髯ｰ縺ｫ髫繧後※縺・・ｽ・ｽ・ｽE・ｽ迥ｶ諷九ｒ邂｡逅・・ｽ・ｽ繧九け繝ｩ繧ｹ縲・    /// </summary>
    public class CoverState : IPlayerState
    {
        private float coverMoveSpeed = 2f;
        private bool isPeeking = false;
        private Vector2 _moveInput;

        /// <summary>
        /// 迥ｶ諷九′髢句ｧ九＆繧後◆縺ｨ縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｫ繝撰ｿｽE繧ｷ繧ｹ繝・・ｽ・ｽ縺ｫ繧ｫ繝撰ｿｽE髢句ｧ九ｒ騾夂衍縺励∝ｧｿ蜍｢繧呈峩譁ｰ縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.TryEnterCover();
            }

            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
            }
        }

        /// <summary>
        /// 迥ｶ諷九′邨ゆｺ・・ｽ・ｽ縺溘→縺阪↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｫ繝撰ｿｽE繧ｷ繧ｹ繝・・ｽ・ｽ縺ｫ繧ｫ繝撰ｿｽE邨ゆｺ・・ｽ・ｽ騾夂衍縺励∪縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CoverSystem != null)
            {
                stateMachine.CoverSystem.ExitCover();
            }
            isPeeking = false;
        }

        /// <summary>
        /// 豈弱ヵ繝ｬ繝ｼ繝蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void Update(DetailedPlayerStateMachine stateMachine) { }

        /// <summary>
        /// 蝗ｺ螳壹ヵ繝ｬ繝ｼ繝繝ｬ繝ｼ繝医〒蜻ｼ縺ｳ蜃ｺ縺輔ｌ縺ｾ縺吶・        /// 繧ｫ繝撰ｿｽE荳ｭ縺ｮ蟾ｦ蜿ｳ遘ｻ蜍輔ｒ蜃ｦ逅・・ｽ・ｽ縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null || stateMachine.CoverSystem == null) return;

            // 繧ｫ繝撰ｿｽE荳ｭ縺ｮ遘ｻ蜍輔Ο繧ｸ繝・・ｽ・ｽ・ｽE・ｽ隕励″隕倶ｸｭ縺ｯ遘ｻ蜍募宛髯撰ｼ・            if (!isPeeking && Mathf.Abs(_moveInput.x) > 0.1f)
            {
                Transform transform = stateMachine.transform;
                Vector3 moveDirection = transform.right * _moveInput.x;
                moveDirection.y = 0;

                stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ縺ｮ蜈･蜉帙ｒ蜃ｦ逅・・ｽ・ｽ縲√き繝撰ｿｽE隗｣髯､繧・・ｽ・ｽ縺崎ｦ九↑縺ｩ縺ｮ繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧貞ｮ溯｡後＠縺ｾ縺吶・        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・ｽE繝茨ｿｽE繧ｷ繝ｳ縲・/param>
        /// <param name="moveInput">遘ｻ蜍包ｿｽE蜉帙・/param>
        /// <param name="jumpInput">繧ｸ繝｣繝ｳ繝暦ｿｽE蜉幢ｼ医％縺ｮ迥ｶ諷九〒縺ｯ繧ｫ繝撰ｿｽE隗｣髯､縺ｫ菴ｿ逕ｨ・ｽE・ｽ縲・/param>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // 繧ｸ繝｣繝ｳ繝暦ｿｽE蜉帙〒繧ｫ繝撰ｿｽE隗｣髯､
            if (jumpInput)
            {
                // 蠕梧婿縺ｸ縺ｮ讓呎ｺ也噪縺ｪ繧ｫ繝撰ｿｽE隗｣髯､
                var exitCoverDefinition = new Commands.ExitCoverCommandDefinition
                {
                    exitDirection = Commands.ExitDirection.Backward,
                    exitSpeedMultiplier = 1f
                };
                var exitCoverCommand = new Commands.ExitCoverCommand(stateMachine, exitCoverDefinition);
                exitCoverCommand.Execute();
                return;
            }
            
            // 蟾ｦ蜿ｳ縺ｸ縺ｮ隕励″隕句宛蠕｡
            if (Input.GetKeyDown(KeyCode.Q)) // 蟾ｦ縺ｸ縺ｮ隕励″隕矩幕蟋・
            {
                StartPeeking(stateMachine, Commands.PeekDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.E)) // 蜿ｳ縺ｸ縺ｮ隕励″隕矩幕蟋・
            {
                StartPeeking(stateMachine, Commands.PeekDirection.Right);
            }
            else if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) // 隕励″隕狗ｵゆｺ・
            {
                StopPeeking(stateMachine);
            }
            
            // 遘ｻ蜍募・蜉帙↓繧医ｋ繧ｫ繝舌・隗｣髯､・郁ｦ励″隕倶ｸｭ縺ｯ辟｡蜉ｹ・・
            if (!isPeeking && moveInput.magnitude > 0.5f)
            {
                // 遘ｻ蜍墓婿蜷代↓蠢懊§縺ｦ繧ｫ繝舌・隗｣髯､譁ｹ蜷代ｒ豎ｺ螳・
                var exitDirection = Commands.ExitDirection.Backward;
                if (moveInput.x > 0.5f) exitDirection = Commands.ExitDirection.Right;
                else if (moveInput.x < -0.5f) exitDirection = Commands.ExitDirection.Left;
                
                var exitCoverDefinition = new Commands.ExitCoverCommandDefinition
                {
                    exitDirection = exitDirection,
                    exitSpeedMultiplier = 1.2f
                };
                var exitCoverCommand = new Commands.ExitCoverCommand(stateMachine, exitCoverDefinition);
                exitCoverCommand.Execute();
            }
        }
        
        /// <summary>
        /// 謖・ｮ壹＆繧後◆譁ｹ蜷代↓隕励″隕九ｒ髢句ｧ九＠縺ｾ縺吶・
        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ</param>
        /// <param name="direction">隕励″隕九☆繧区婿蜷・/param>
        private void StartPeeking(DetailedPlayerStateMachine stateMachine, Commands.PeekDirection direction)
        {
            if (isPeeking) return; // 譌｢縺ｫ隕励″隕倶ｸｭ縺ｮ蝣ｴ蜷医・辟｡隕・
            var peekDefinition = new Commands.PeekCommandDefinition
            {
                peekDirection = direction,
                peekIntensity = 0.3f
            };
            var peekCommand = new Commands.PeekCommand(stateMachine, peekDefinition);
            peekCommand.Execute();
            
            isPeeking = true;
            Debug.Log($"Started peeking {direction}");
        }
        
        /// <summary>
        /// 隕励″隕九ｒ邨ゆｺ・＠縲∝・縺ｮ繧ｫ繝舌・菴咲ｽｮ縺ｫ謌ｻ繧翫∪縺吶・
        /// </summary>
        /// <param name="stateMachine">繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・・繝医・繧ｷ繝ｳ</param>
        private void StopPeeking(DetailedPlayerStateMachine stateMachine)
        {
            if (!isPeeking) return; // 隕励″隕倶ｸｭ縺ｧ縺ｪ縺・ｴ蜷医・辟｡隕・
            isPeeking = false;
            
            // 隕励″隕倶ｽ咲ｽｮ縺九ｉ蜈・・繧ｫ繝舌・菴咲ｽｮ縺ｫ謌ｻ繧句・逅・
            // 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲￣eekCommand縺ｮUndo繧貞他縺ｳ蜃ｺ縺吶°縲・
            // 繧ｫ繝舌・繧ｷ繧ｹ繝・Β縺ｫ蜈・・菴咲ｽｮ縺ｫ謌ｻ縺吶ｈ縺・欠遉ｺ縺吶ｋ
            if (stateMachine.CoverSystem != null)
            {
                // CoverSystem縺ｫ隕励″隕狗ｵゆｺ・ｒ騾夂衍・亥ｮ溯｣・↓蠢懊§縺ｦ隱ｿ謨ｴ・・
                Debug.Log("Stopped peeking - returning to cover position");
            }
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縲√・繝ｬ繧､繝､繝ｼ縺瑚ｦ励″隕倶ｸｭ縺九←縺・°繧貞叙蠕励＠縺ｾ縺吶・
        /// </summary>
        public bool IsPeeking => isPeeking;
    }
}
