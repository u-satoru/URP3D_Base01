using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Player.States
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ迚ｹ蛹悶・繧ｫ繝舌・迥ｶ諷・
    /// 驕ｮ阡ｽ迚ｩ繧貞茜逕ｨ縺励◆謌ｦ陦鍋噪髫阡ｽ縺ｨ繝斐・繧ｭ繝ｳ繧ｰ讖溯・
    /// 蜍慕噪縺ｪ隕也阜蛻ｶ蠕｡縺ｨ謨ｵ菴咲ｽｮ縺ｮ蛛ｵ蟇溯・蜉帙ｒ謠蝉ｾ・
    /// </summary>
    public class StealthInCoverState : IPlayerState
    {
        [Header("Stealth Cover Configuration")]
        [SerializeField] private float coverMoveSpeed = 1.0f; // 繧ｫ繝舌・髢鍋ｧｻ蜍暮溷ｺｦ
        [SerializeField] private float coverVisibilityReduction = 0.8f; // 80%隕冶ｪ肴ｧ蜑頑ｸ・
        [SerializeField] private float peekVisibilityIncrease = 0.4f; // 繝斐・繧ｯ譎・0%隕冶ｪ肴ｧ蠅怜刈
        [SerializeField] private float coverNoiseReduction = 0.8f; // 80%鬨帝浹蜑頑ｸ・

        private Vector2 _moveInput;
        private IStealthService _stealthService;
        private bool _wasInStealthMode;
        private Transform _currentCover;
        private bool _isPeeking;
        private bool _canPeekLeft;
        private bool _canPeekRight;
        private Vector3 _originalCoverPosition;
        private UnityEngine.Camera _playerCamera;

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｫ繝舌・迥ｶ諷矩幕蟋・
        /// 驕ｮ阡ｽ迚ｩ縺ｨ縺ｮ菴咲ｽｮ髢｢菫ゅｒ遒ｺ遶九＠縲∵怙驕ｩ縺ｪ髫阡ｽ蟋ｿ蜍｢繧定ｨｭ螳・
        /// </summary>
        public void Enter(DetailedPlayerStateMachine stateMachine)
        {
            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();

            // 繧ｫ繝舌・繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ迚ｹ螳・
            _currentCover = FindOptimalCoverPosition(stateMachine);
            if (_currentCover != null)
            {
                _originalCoverPosition = _currentCover.position;
                AlignTocover(stateMachine, _currentCover);
            }

            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝臥｢ｺ隱阪・髢句ｧ・
            if (_stealthService != null)
            {
                _wasInStealthMode = _stealthService.IsStealthModeActive;
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(true);
                }

                // 繧ｫ繝舌・迥ｶ諷九〒縺ｮ隕冶ｪ肴ｧ蜑頑ｸ・
                _stealthService.UpdatePlayerVisibility(coverVisibilityReduction);
                _stealthService.UpdatePlayerNoiseLevel(0.0f); // 繧ｫ繝舌・荳ｭ縺ｯ髱咎浹
            }

            // 譌｢蟄牢tealthMovement邨ｱ蜷・
            if (stateMachine.StealthMovement != null)
            {
                stateMachine.StealthMovement.SetStance(Core.Data.MovementStance.Crouching);
            }

            // 繝斐・繧ｭ繝ｳ繧ｰ蜿ｯ閭ｽ諤ｧ蛻､螳・
            EvaluatePeekingOptions(stateMachine);

            // 繧ｫ繝｡繝ｩ蜿ら・蜿門ｾ・
            _playerCamera = UnityEngine.Camera.main ?? Object.FindFirstObjectByType<UnityEngine.Camera>();

            _isPeeking = false;

            Debug.Log($"[StealthInCoverState] Cover position established. Visibility reduction: {coverVisibilityReduction:F2}");
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ繧ｫ繝舌・迥ｶ諷狗ｵゆｺ・
        /// </summary>
        public void Exit(DetailedPlayerStateMachine stateMachine)
        {
            // 繝斐・繧ｭ繝ｳ繧ｰ迥ｶ諷玖ｧ｣髯､
            if (_isPeeking)
            {
                StopPeeking(stateMachine);
            }

            // 繧ｹ繝・Ν繧ｹ險ｭ螳壼ｾｩ蜈・
            if (_stealthService != null)
            {
                if (!_wasInStealthMode)
                {
                    _stealthService.SetStealthMode(false);
                }

                // 騾壼ｸｸ隕冶ｪ肴ｧ縺ｫ謌ｻ縺・
                _stealthService.UpdatePlayerVisibility(1.0f);
            }

            _currentCover = null;

            Debug.Log("[StealthInCoverState] Cover position abandoned");
        }

        /// <summary>
        /// 繝輔Ξ繝ｼ繝譖ｴ譁ｰ
        /// 繧ｫ繝舌・迥ｶ諷九・邯ｭ謖√→迺ｰ蠅・屮隕・
        /// </summary>
        public void Update(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 繧ｫ繝舌・繧ｪ繝悶ず繧ｧ繧ｯ繝医→縺ｮ霍晞屬逶｣隕・
            if (_currentCover != null)
            {
                float distanceToCover = Vector3.Distance(
                    stateMachine.transform.position, _currentCover.position);

                // 繧ｫ繝舌・縺九ｉ髮｢繧後☆縺弱◆蝣ｴ蜷・
                if (distanceToCover > 2.0f)
                {
                    Debug.Log("[StealthInCoverState] Too far from cover - transitioning out");
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
            }

            // 繝斐・繧ｭ繝ｳ繧ｰ迥ｶ諷九・隕冶ｪ肴ｧ邂｡逅・
            if (_isPeeking && _stealthService != null)
            {
                float peekingVisibility = coverVisibilityReduction + peekVisibilityIncrease;
                _stealthService.UpdatePlayerVisibility(Mathf.Clamp01(peekingVisibility));
            }
        }

        /// <summary>
        /// 迚ｩ逅・峩譁ｰ
        /// 繧ｫ繝舌・髢薙・諷朱㍾縺ｪ遘ｻ蜍・
        /// </summary>
        public void FixedUpdate(DetailedPlayerStateMachine stateMachine)
        {
            if (stateMachine.CharacterController == null) return;

            Transform transform = stateMachine.transform;
            Vector3 moveDirection = Vector3.zero;

            // 繧ｫ繝舌・豐ｿ縺・・遘ｻ蜍輔・縺ｿ險ｱ蜿ｯ
            if (_currentCover != null)
            {
                Vector3 coverDirection = (_currentCover.position - transform.position).normalized;
                Vector3 coverRight = Vector3.Cross(Vector3.up, coverDirection);

                moveDirection = coverRight * _moveInput.x;
            }

            // 驥榊鴨蜃ｦ逅・
            if (!stateMachine.CharacterController.isGrounded)
            {
                moveDirection.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            // 繧ｫ繝舌・遘ｻ蜍・
            stateMachine.CharacterController.Move(moveDirection * coverMoveSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 蜈･蜉帛・逅・
        /// 繧ｫ繝舌・迥ｶ諷九〒縺ｮ迚ｹ谿頑桃菴・
        /// </summary>
        public void HandleInput(DetailedPlayerStateMachine stateMachine, Vector2 moveInput, bool jumpInput)
        {
            _moveInput = moveInput;

            // 繧ｸ繝｣繝ｳ繝怜・蜉帙〒繧ｫ繝舌・髮｢閼ｱ
            if (jumpInput)
            {
                if (IsSafeToLeaveCover(stateMachine))
                {
                    stateMachine.TransitionToState(PlayerStateType.Crouching);
                    return;
                }
                else
                {
                    Debug.Log("[StealthInCoverState] Not safe to leave cover");
                }
            }

            // 繝斐・繧ｭ繝ｳ繧ｰ謫堺ｽ・
            if (Input.GetKeyDown(KeyCode.Q) && _canPeekLeft)
            {
                TogglePeeking(stateMachine, PeekDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.E) && _canPeekRight)
            {
                TogglePeeking(stateMachine, PeekDirection.Right);
            }

            // 繝斐・繧ｭ繝ｳ繧ｰ邨ゆｺ・
            if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
            {
                if (_isPeeking)
                {
                    StopPeeking(stateMachine);
                }
            }

            // 莉悶・繧ｫ繝舌・縺ｸ縺ｮ遘ｻ蜍・
            if (Input.GetKeyDown(KeyCode.F))
            {
                Transform nextCover = FindNearestAlternateCover(stateMachine);
                if (nextCover != null)
                {
                    MoveToCover(stateMachine, nextCover);
                }
            }

            // 繧ｫ繝舌・縺九ｉ縺ｮ迺ｰ蠅・嶌莠剃ｽ懃畑
            if (Input.GetKeyDown(KeyCode.R))
            {
                PerformCoverAction(stateMachine);
            }
        }

        /// <summary>
        /// 譛驕ｩ縺ｪ繧ｫ繝舌・菴咲ｽｮ繧堤音螳・
        /// </summary>
        private Transform FindOptimalCoverPosition(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform optimalCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 3.0f)
                {
                    nearestDistance = distance;
                    optimalCover = cover.transform;
                }
            }

            return optimalCover;
        }

        /// <summary>
        /// 繧ｫ繝舌・繧ｪ繝悶ず繧ｧ繧ｯ繝医↓謨ｴ蛻・
        /// </summary>
        private void AlignTocover(DetailedPlayerStateMachine stateMachine, Transform cover)
        {
            Vector3 directionToCover = (cover.position - stateMachine.transform.position).normalized;
            Vector3 alignPosition = cover.position - directionToCover * 1.2f; // 繧ｫ繝舌・縺九ｉ1.2m霍晞屬

            stateMachine.transform.position = alignPosition;
            stateMachine.transform.LookAt(cover.position);
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ蜿ｯ閭ｽ諤ｧ隧穂ｾ｡
        /// </summary>
        private void EvaluatePeekingOptions(DetailedPlayerStateMachine stateMachine)
        {
            if (_currentCover == null) return;

            Vector3 playerPos = stateMachine.transform.position;
            Vector3 coverPos = _currentCover.position;

            // 蟾ｦ蜿ｳ縺ｮ繝斐・繧ｭ繝ｳ繧ｰ蜿ｯ閭ｽ諤ｧ繧偵Ξ繧､繧ｭ繝｣繧ｹ繝医〒遒ｺ隱・
            Vector3 rightDirection = stateMachine.transform.right;
            Vector3 leftDirection = -stateMachine.transform.right;

            _canPeekRight = !Physics.Raycast(playerPos, rightDirection, 1.5f);
            _canPeekLeft = !Physics.Raycast(playerPos, leftDirection, 1.5f);
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ蛻・ｊ譖ｿ縺・
        /// </summary>
        private void TogglePeeking(DetailedPlayerStateMachine stateMachine, PeekDirection direction)
        {
            if (_isPeeking)
            {
                StopPeeking(stateMachine);
            }
            else
            {
                StartPeeking(stateMachine, direction);
            }
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ髢句ｧ・
        /// </summary>
        private void StartPeeking(DetailedPlayerStateMachine stateMachine, PeekDirection direction)
        {
            _isPeeking = true;

            // 繧ｫ繝｡繝ｩ繧ｪ繝輔そ繝・ヨ驕ｩ逕ｨ・育ｰ｡譏灘ｮ溯｣・ｼ・
            if (_playerCamera != null)
            {
                Vector3 peekOffset = direction == PeekDirection.Left ?
                    -stateMachine.transform.right * 0.8f :
                    stateMachine.transform.right * 0.8f;

                _playerCamera.transform.position += peekOffset;
            }

            // 隕冶ｪ肴ｧ蠅怜刈・医ヴ繝ｼ繧ｭ繝ｳ繧ｰ繝ｪ繧ｹ繧ｯ・・
            if (_stealthService != null)
            {
                float peekingVisibility = coverVisibilityReduction + peekVisibilityIncrease;
                _stealthService.UpdatePlayerVisibility(Mathf.Clamp01(peekingVisibility));
            }

            Debug.Log($"[StealthInCoverState] Started peeking {direction}");
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ邨ゆｺ・
        /// </summary>
        private void StopPeeking(DetailedPlayerStateMachine stateMachine)
        {
            _isPeeking = false;

            // 繧ｫ繝｡繝ｩ菴咲ｽｮ蠕ｩ蜈・ｼ育ｰ｡譏灘ｮ溯｣・ｼ・
            if (_playerCamera != null)
            {
                // 譛ｬ譚･縺ｯ繧ｫ繝｡繝ｩ縺ｮ蜈・ｽ咲ｽｮ繧剃ｿ晏ｭ倥＠縺ｦ縺翫￥縺ｹ縺・
                Vector3 playerPos = stateMachine.transform.position;
                _playerCamera.transform.position = playerPos + Vector3.up * 1.7f;
            }

            // 隕冶ｪ肴ｧ繧偵き繝舌・迥ｶ諷九↓謌ｻ縺・
            if (_stealthService != null)
            {
                _stealthService.UpdatePlayerVisibility(coverVisibilityReduction);
            }

            Debug.Log("[StealthInCoverState] Stopped peeking");
        }

        /// <summary>
        /// 螳牙・縺ｪ繧ｫ繝舌・髮｢閼ｱ蛻､螳・
        /// </summary>
        private bool IsSafeToLeaveCover(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return true;

            // 迴ｾ蝨ｨ縺ｮ隕冶ｪ肴ｧ縺ｨ髫阡ｽ迥ｶ諷九ｒ隧穂ｾ｡
            bool lowVisibility = _stealthService.PlayerVisibilityFactor < 0.5f;
            bool isConcealed = _stealthService.IsPlayerConcealed;

            return lowVisibility || isConcealed;
        }

        /// <summary>
        /// 莉｣譖ｿ繧ｫ繝舌・謗｢邏｢
        /// </summary>
        private Transform FindNearestAlternateCover(DetailedPlayerStateMachine stateMachine)
        {
            GameObject[] coverObjects = GameObject.FindGameObjectsWithTag("Cover");
            Transform alternateCover = null;
            float nearestDistance = float.MaxValue;

            foreach (GameObject cover in coverObjects)
            {
                if (cover.transform == _currentCover) continue; // 迴ｾ蝨ｨ縺ｮ繧ｫ繝舌・縺ｯ髯､螟・

                float distance = Vector3.Distance(stateMachine.transform.position, cover.transform.position);
                if (distance < nearestDistance && distance <= 5.0f)
                {
                    nearestDistance = distance;
                    alternateCover = cover.transform;
                }
            }

            return alternateCover;
        }

        /// <summary>
        /// 蛻･繧ｫ繝舌・縺ｸ縺ｮ遘ｻ蜍・
        /// </summary>
        private void MoveToCover(DetailedPlayerStateMachine stateMachine, Transform newCover)
        {
            _currentCover = newCover;
            AlignTocover(stateMachine, newCover);
            EvaluatePeekingOptions(stateMachine);

            Debug.Log("[StealthInCoverState] Moved to new cover position");
        }

        /// <summary>
        /// 繧ｫ繝舌・縺九ｉ縺ｮ迺ｰ蠅・嶌莠剃ｽ懃畑
        /// </summary>
        private void PerformCoverAction(DetailedPlayerStateMachine stateMachine)
        {
            if (_stealthService == null) return;

            // 繧ｫ繝舌・菴咲ｽｮ縺九ｉ縺ｮ逶ｸ莠剃ｽ懃畑遽・峇諡｡蠑ｵ
            Collider[] interactables = Physics.OverlapSphere(
                stateMachine.transform.position, 3.0f,
                LayerMask.GetMask("Interactable"));

            foreach (var collider in interactables)
            {
                if (collider.CompareTag("SecurityCamera"))
                {
                    _stealthService.InteractWithEnvironment(collider.gameObject, StealthInteractionType.DisableCamera);
                    Debug.Log("[StealthInCoverState] Disabled camera from cover");
                    break;
                }
                else if (collider.CompareTag("Guard"))
                {
                    // 髯ｽ蜍暮浹縺ｮ菴懈・
                    Vector3 distractionPos = collider.transform.position + Vector3.right * 5f;
                    _stealthService.CreateDistraction(distractionPos, 0.8f);
                    Debug.Log("[StealthInCoverState] Created distraction from cover");
                    break;
                }
            }
        }

        /// <summary>
        /// 繝斐・繧ｭ繝ｳ繧ｰ譁ｹ蜷・
        /// </summary>
        private enum PeekDirection
        {
            Left,
            Right
        }
    }
}


