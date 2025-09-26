using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迚ｹ蛹悶・縺励ｃ縺後∩迥ｶ諷・
    /// 蝓ｺ譛ｬ逧・↑CrouchingState縺ｮ讖溯・縺ｫ蜉縺医※縲ヾtealthService縺ｨ縺ｮ螳悟・邨ｱ蜷医ｒ謠蝉ｾ・
    /// 隕冶ｪ肴ｧ縺ｮ螟ｧ蟷・ｽ惹ｸ九・浹髻ｿ繝ｬ繝吶Ν縺ｮ蜑頑ｸ帙∫ｧｻ蜍暮溷ｺｦ縺ｮ譛驕ｩ蛹・
    /// </summary>
    public class StealthCrouchingState : IPlayerState
    {
        [Header("Stealth Crouching Configuration")]
        [SerializeField] private float crouchSpeed = 1.5f; // 騾壼ｸｸ繧医ｊ譖ｴ縺ｫ菴朱・
        [SerializeField] private float stealthVisibilityReduction = 0.6f; // 60%隕冶ｪ肴ｧ蜑頑ｸ・
        [SerializeField] private float noiseReduction = 0.7f; // 70%鬨帝浹蜑頑ｸ・
        [SerializeField] private float originalHeight;
        [SerializeField] private float crouchHeight = 1.0f; // 騾壼ｸｸ繧医ｊ譖ｴ縺ｫ菴弱￥

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private float _baseNoiseLevel;

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ縺励ｃ縺後∩迥ｶ諷矩幕蟋・
        /// StealthService縺ｨ騾｣謳ｺ縺励∵怙驕ｩ縺ｪ髫阡ｽ蟋ｿ蜍｢繧堤｢ｺ遶・
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // 迚ｩ逅・噪縺ｪ蟋ｿ蜍｢螟画峩
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = crouchHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = crouchHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // 繧ｹ繝・Ν繧ｹ繧ｷ繧ｹ繝・Β邨ｱ蜷・
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // 隕冶ｪ肴ｧ螟ｧ蟷・炎貂・
                _stealthService.UpdatePlayerVisibility(stealthVisibilityReduction);
            }

            // 譌｢蟄牢tealthMovement邨ｱ蜷・
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
                _baseNoiseLevel = stateMachine.StealthMovement.GetNoiseLevel();
            }

            Debug.Log($"[StealthCrouchingState] Stealth crouching activated. Visibility: {stealthVisibilityReduction:F2}, Noise reduction: {noiseReduction:F2}");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ縺励ｃ縺後∩迥ｶ諷狗ｵゆｺ・
        /// 繧ｹ繝・Ν繧ｹ險ｭ螳壹ｒ驕ｩ蛻・↓繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // 迚ｩ逅・噪蟋ｿ蜍｢蠕ｩ蜈・
            if (stateMachine.CharacterController != null)
            {
                stateMachine.CharacterController.height = originalHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = originalHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // 繧ｹ繝・Ν繧ｹ險ｭ螳壼ｾｩ蜈・
            if (_stealthService != null)
            {
                // 蜑阪・迥ｶ諷九′繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝峨〒縺ｪ縺九▲縺溷ｴ蜷医・隗｣髯､
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 騾壼ｸｸ縺ｮ隕冶ｪ肴ｧ縺ｫ謌ｻ縺・
                _stealthService.UpdatePlayerVisibility(1.0f);
            }

            Debug.Log("[StealthCrouchingState] Stealth crouching deactivated");
        }

        /// <summary>
        /// 繝輔Ξ繝ｼ繝譖ｴ譁ｰ
        /// 繧ｹ繝・Ν繧ｹ迥ｶ諷九・邯咏ｶ夂噪逶｣隕・
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            // 繧ｹ繝・Ν繧ｹ繧ｵ繝ｼ繝薙せ縺ｨ縺ｮ邯咏ｶ夂噪蜷梧悄
            if (_stealthService != null)
            {
                // 遘ｻ蜍輔↓繧医ｋ鬨帝浹繝ｬ繝吶Ν隱ｿ謨ｴ
                float currentNoiseLevel = _moveInput.magnitude > 0.1f ?
                    _baseNoiseLevel * noiseReduction : 0.0f;

                _stealthService.UpdatePlayerNoiseLevel(currentNoiseLevel);
            }
        }

        /// <summary>
        /// 迚ｩ逅・峩譁ｰ
        /// 菴朱溘〒諷朱㍾縺ｪ遘ｻ蜍募・逅・
        /// </summary>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            moveDirection.y = 0;

            // 驥榊鴨蜃ｦ逅・
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            // 譖ｴ縺ｫ諷朱㍾縺ｪ遘ｻ蜍包ｼ磯壼ｸｸ縺ｮCrouchingState繧医ｊ菴朱滂ｼ・
            stateMachine.CharacterController.Move(moveDirection.normalized * crouchSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 蜈･蜉帛・逅・
        /// 繧ｹ繝・Ν繧ｹ迚ｹ蛹悶・迥ｶ諷矩・遘ｻ繝ｭ繧ｸ繝・け
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // 繧ｸ繝｣繝ｳ繝怜・蜉帙〒縺励ｃ縺後∩隗｣髯､・域・驥阪↑遶九■荳翫′繧奇ｼ・
            if (jumpInput)
            {
                if (CanStandUpSafely(stateMachine))
                {
                    // 繧ｹ繝・Ν繧ｹ迥ｶ諷九ｒ邯ｭ謖√＠縺ｪ縺後ｉ驕ｷ遘ｻ
                    if (moveInput.magnitude > 0.1f)
                    {
                        // 繧ｹ繝・Ν繧ｹ豁ｩ陦後↓驕ｷ遘ｻ
                        stateMachine.TransitionToState(PlayerStateType.Walking);
                    }
                    else
                    {
                        // 繧ｹ繝・Ν繧ｹ蠕・ｩ溘↓驕ｷ遘ｻ
                        stateMachine.TransitionToState(PlayerStateType.Idle);
                    }
                    return;
                }
                else
                {
                    // 遶九■荳翫′繧後↑縺・ｴ蜷医・隴ｦ蜻奇ｼ医が繝励す繝ｧ繝ｳ・・
                    Debug.Log("[StealthCrouchingState] Cannot stand up safely - obstacle detected");
                }
            }

            // 繧医ｊ豺ｱ縺・せ繝・Ν繧ｹ蟋ｿ蜍｢縺ｸ縺ｮ驕ｷ遘ｻ
            if (Input.GetKeyDown(KeyCode.X))
            {
                stateMachine.TransitionToState(PlayerStateType.Prone);
            }

            // 繧ｫ繝舌・繧｢繧ｯ繧ｷ繝ｧ繝ｳ・磯・阡ｽ迚ｩ縺ｸ縺ｮ遘ｻ蜍包ｼ・
            if (Input.GetKeyDown(KeyCode.C))
            {
                // 霑代￥縺ｮ驕ｮ阡ｽ迚ｩ繧呈爾縺励※繧ｫ繝舌・迥ｶ諷九↓驕ｷ遘ｻ
                if (FindNearestCover(stateMachine) != null)
                {
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }
        }

        /// <summary>
        /// 螳牙・縺ｪ遶九■荳翫′繧雁愛螳・
        /// 鬆ｭ荳翫・髫懷ｮｳ迚ｩ繝√ぉ繝・け縺ｫ蜉縺医※縲∵雰縺ｮ隕也ｷ壹ｂ閠・・
        /// </summary>
        private bool CanStandUpSafely(DetailedPlayerStateMachine stateMachine)
        {
            // 迚ｩ逅・噪髫懷ｮｳ迚ｩ繝√ぉ繝・け
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * crouchHeight;
            float checkDistance = originalHeight - crouchHeight;

            bool physicallyCanStand = !Physics.SphereCast(origin, 0.3f, Vector3.up, out hit, checkDistance);

            // 繧ｹ繝・Ν繧ｹ閠・・: 遶九■荳翫′繧九％縺ｨ縺ｧ逋ｺ隕九Μ繧ｹ繧ｯ縺碁ｫ倥∪繧九°繝√ぉ繝・け
            bool stealthSafe = true;
            if (_stealthService != null)
            {
                // 迴ｾ蝨ｨ縺ｮ隕冶ｪ肴ｧ縺御ｽ弱＞蝣ｴ蜷医・縺ｿ螳牙・縺ｨ縺ｿ縺ｪ縺・
                stealthSafe = _stealthService.PlayerVisibilityFactor < 0.5f;
            }

            return physicallyCanStand && stealthSafe;
        }

        /// <summary>
        /// 譛蟇・ｊ縺ｮ繧ｫ繝舌・繝昴う繝ｳ繝医ｒ謗｢邏｢
        /// </summary>
        private Transform FindNearestCover(DetailedPlayerStateMachine stateMachine)
        {
            // 邁｡譏灘ｮ溯｣・ "Cover"繧ｿ繧ｰ縺ｮ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ謗｢邏｢
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform nearestCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 3.0f) // 3m莉･蜀・・繧ｫ繝舌・
                {
                    nearestDistance = distance;
                    nearestCover = cover.transform;
                }
            }

            return nearestCover;
        }
    }
}


