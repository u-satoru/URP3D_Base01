using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迚ｹ蛹悶・蛹榊倹迥ｶ諷・
    /// 譛螟ｧ縺ｮ髫阡ｽ蜉ｹ譫懊ｒ謠蝉ｾ帙☆繧区怙繧ゆｽ弱＞蟋ｿ蜍｢
    /// 遘ｻ蜍暮溷ｺｦ縺ｯ螟ｧ蟷・↓菴惹ｸ九☆繧九′縲∬ｦ冶ｪ肴ｧ縺ｨ鬨帝浹繧呈怙蟆城剞縺ｫ謚大宛
    /// </summary>
    public class StealthProneState : IPlayerState
    {
        [Header("Stealth Prone Configuration")]
        [SerializeField] private float proneSpeed = 0.8f; // 髱槫ｸｸ縺ｫ菴朱・
        [SerializeField] private float maxStealthVisibility = 0.2f; // 80%隕冶ｪ肴ｧ蜑頑ｸ・
        [SerializeField] private float maxNoiseReduction = 0.9f; // 90%鬨帝浹蜑頑ｸ・
        [SerializeField] private float originalHeight;
        [SerializeField] private float proneHeight = 0.6f; // 譛菴主ｧｿ蜍｢

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private float _baseNoiseLevel;
        private bool _isCompletlyStill; // 螳悟・髱呎ｭ｢迥ｶ諷・
        private float _stillTimer;
        private float _stillThreshold = 2.0f; // 2遘帝撕豁｢縺ｧ螳悟・髫阡ｽ

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ蛹榊倹迥ｶ諷矩幕蟋・
        /// 譛螟ｧ髫阡ｽ繝｢繝ｼ繝峨ｒ遒ｺ遶・
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // 譛菴主ｧｿ蜍｢縺ｸ縺ｮ螟画峩
            if (stateMachine.CharacterController != null)
            {
                originalHeight = stateMachine.CharacterController.height;
                stateMachine.CharacterController.height = proneHeight;

                Vector3 center = stateMachine.CharacterController.center;
                center.y = proneHeight / 2f;
                stateMachine.CharacterController.center = center;
            }

            // 譛螟ｧ繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝蛾幕蟋・
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // 譛螟ｧ隕冶ｪ肴ｧ蜑頑ｸ・
                _stealthService.UpdatePlayerVisibility(maxStealthVisibility);
            }

            // 譌｢蟄牢tealthMovement邨ｱ蜷・
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Prone);
                _baseNoiseLevel = stateMachine.StealthMovement.GetNoiseLevel();
            }

            // 髱呎ｭ｢迥ｶ諷句・譛溷喧
            _isCompletlyStill = false;
            _stillTimer = 0f;

            Debug.Log($"[StealthProneState] Maximum stealth activated. Visibility: {maxStealthVisibility:F2}, Noise reduction: {maxNoiseReduction:F2}");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ蛹榊倹迥ｶ諷狗ｵゆｺ・
        /// 繧ｹ繝・Ν繧ｹ險ｭ螳壹ｒ驕ｩ蛻・↓繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // 蟋ｿ蜍｢蠕ｩ蜈・
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
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 騾壼ｸｸ縺ｮ隕冶ｪ肴ｧ縺ｫ謌ｻ縺・
                _stealthService.UpdatePlayerVisibility(1.0f);
                _stealthService.UpdatePlayerNoiseLevel(0.0f);
            }

            Debug.Log("[StealthProneState] Maximum stealth deactivated");
        }

        /// <summary>
        /// 繝輔Ξ繝ｼ繝譖ｴ譁ｰ
        /// 髱呎ｭ｢譎る俣縺ｫ繧医ｋ螳悟・髫阡ｽ蛻､螳・
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 髱呎ｭ｢迥ｶ諷九・蛻､螳壹→邂｡逅・
            bool isMoving = _moveInput.magnitude > 0.01f;

            if (!isMoving)
            {
                _stillTimer += Time.deltaTime;

                // 螳悟・髱呎ｭ｢迥ｶ諷矩＃謌・
                if (_stillTimer >= _stillThreshold && !_isCompletlyStill)
                {
                    _isCompletlyStill = true;
                    // 螳悟・髫阡ｽ迥ｶ諷・ 隕冶ｪ肴ｧ繧偵＆繧峨↓蜑頑ｸ・
                    _stealthService.UpdatePlayerVisibility(maxStealthVisibility * 0.5f); // 縺輔ｉ縺ｫ50%蜑頑ｸ・
                    _stealthService.UpdatePlayerNoiseLevel(0.0f); // 螳悟・辟｡髻ｳ

                    Debug.Log("[StealthProneState] Complete concealment achieved - nearly invisible");
                }
            }
            else
            {
                _stillTimer = 0f;
                if (_isCompletlyStill)
                {
                    _isCompletlyStill = false;
                    // 騾壼ｸｸ縺ｮ蛹榊倹髫阡ｽ縺ｫ謌ｻ縺・
                    _stealthService.UpdatePlayerVisibility(maxStealthVisibility);
                }
            }

            // 遘ｻ蜍墓凾縺ｮ鬨帝浹繝ｬ繝吶Ν隱ｿ謨ｴ
            if (isMoving)
            {
                float currentNoiseLevel = _baseNoiseLevel * (1.0f - maxNoiseReduction);
                _stealthService.UpdatePlayerNoiseLevel(currentNoiseLevel);
            }
        }

        /// <summary>
        /// 迚ｩ逅・峩譁ｰ
        /// 髱槫ｸｸ縺ｫ諷朱㍾縺ｧ菴朱溘↑蛹榊倹遘ｻ蜍・
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

            // 蛹榊倹遘ｻ蜍包ｼ域怙菴朱溷ｺｦ・・
            stateMachine.CharacterController.Move(moveDirection.normalized * proneSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 蜈･蜉帛・逅・
        /// 蛹榊倹迥ｶ諷九°繧峨・諷朱㍾縺ｪ驕ｷ遘ｻ蛻ｶ蠕｡
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // 繧ｸ繝｣繝ｳ繝怜・蜉帙〒縺励ｃ縺後∩迥ｶ諷九↓驕ｷ遘ｻ
            if (jumpInput)
            {
                if (IsSafeToRise(stateMachine))
                {
                    // 縺ｾ縺壹・縺励ｃ縺後∩迥ｶ諷九↓驕ｷ遘ｻ・域ｮｵ髫守噪縺ｪ蟋ｿ蜍｢螟画峩・・
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
                else
                {
                    Debug.Log("[StealthProneState] Cannot rise safely - remaining in prone position");
                }
            }

            // 繧ｫ繝舌・繧｢繧ｯ繧ｷ繝ｧ繝ｳ
            if (Input.GetKeyDown(KeyCode.C))
            {
                Transform nearestCover = FindNearestCover(stateMachine);
                if (nearestCover != null)
                {
                    // 繧ｫ繝舌・縺悟茜逕ｨ蜿ｯ閭ｽ縺ｪ蝣ｴ蜷医・縺ｿ驕ｷ遘ｻ
                    stateMachine.TransitionToState(PlayerStateType.InCover);
                }
            }

            // 迚ｹ谿翫せ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ・医が繝励す繝ｧ繝ｳ・・
            if (Input.GetKeyDown(KeyCode.E))
            {
                PerformStealthAction(stateMachine);
            }
        }

        /// <summary>
        /// 螳牙・縺ｪ襍ｷ縺堺ｸ翫′繧雁愛螳・
        /// 謨ｵ縺ｮ隕也ｷ壹→迚ｩ逅・噪蛻ｶ邏・・荳｡譁ｹ繧定・・
        /// </summary>
        private bool IsSafeToRise(DetailedPlayerStateMachine stateMachine)
        {
            // 迚ｩ逅・噪髫懷ｮｳ迚ｩ繝√ぉ繝・け
            RaycastHit hit;
            Vector3 origin = stateMachine.transform.position + Vector3.up * proneHeight;
            float checkDistance = originalHeight - proneHeight;

            bool physicallyCanRise = !Physics.SphereCast(origin, 0.4f, Vector3.up, out hit, checkDistance);

            // 繧ｹ繝・Ν繧ｹ螳牙・諤ｧ繝√ぉ繝・け
            bool stealthSafe = true;
            if (_stealthService != null)
            {
                // 迴ｾ蝨ｨ縺ｮ隕冶ｪ肴ｧ縺碁撼蟶ｸ縺ｫ菴弱＞蝣ｴ蜷医・縺ｿ螳牙・
                stealthSafe = _stealthService.PlayerVisibilityFactor < 0.3f;

                // 髫阡ｽ繧ｾ繝ｼ繝ｳ蜀・↓縺・ｋ蝣ｴ蜷医・霑ｽ蜉縺ｮ螳牙・諤ｧ
                if (_stealthService.IsPlayerConcealed)
                {
                    stealthSafe = true;
                }
            }

            return physicallyCanRise && stealthSafe;
        }

        /// <summary>
        /// 譛蟇・ｊ縺ｮ繧ｫ繝舌・繝昴う繝ｳ繝域爾邏｢
        /// </summary>
        private Transform FindNearestCover(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform nearestCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 2.0f) // 蛹榊倹縺ｧ縺ｯ2m莉･蜀・
                {
                    nearestDistance = distance;
                    nearestCover = cover.transform;
                }
            }

            return nearestCover;
        }

        /// <summary>
        /// 迚ｹ谿翫せ繝・Ν繧ｹ繧｢繧ｯ繧ｷ繝ｧ繝ｳ螳溯｡・
        /// 蛹榊倹迥ｶ諷九°繧峨・迺ｰ蠅・嶌莠剃ｽ懃畑
        /// </summary>
        private void PerformStealthAction(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 霑代￥縺ｮ繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ謗｢邏｢
            Collider[] interactables = Physics.OverlapSphere(
                stateMachine.transform.position, 1.5f,
                LayerMask.GetMask("Interactable"));

            foreach (var collider in interactables)
            {
                // 迺ｰ蠅・嶌莠剃ｽ懃畑縺ｮ螳溯｡鯉ｼ井ｾ具ｼ壹き繝｡繝ｩ辟｡蜉ｹ蛹悶√Λ繧､繝育ｴ螢顔ｭ会ｼ・
                if (collider.CompareTag("SecurityCamera"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.DisableCamera);
                    Debug.Log("[StealthProneState] Disabled security camera from prone position");
                    break;
                }
                else if (collider.CompareTag("Light"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.SabotageLight);
                    Debug.Log("[StealthProneState] Sabotaged light source from prone position");
                    break;
                }
            }
        }
    }
}


