using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ遘ｻ蜍輔さ繝槭Φ繝峨す繧ｹ繝・Β
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・Ν繧ｹ遘ｻ蜍輔い繧ｯ繧ｷ繝ｧ繝ｳ繧偵き繝励そ繝ｫ蛹・
    /// ServiceLocator邨ｱ蜷医↓繧医ｋ荳ｭ螟ｮ蛻ｶ蠕｡縺ｨObjectPool譛驕ｩ蛹門ｯｾ蠢・
    /// </summary>
    public class StealthMovementCommand : IResettableCommand
    {
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ遘ｻ蜍輔・遞ｮ鬘・
        /// </summary>
        public enum StealthMovementType
        {
            SneakMode,          // 蠢阪・雜ｳ繝｢繝ｼ繝蛾幕蟋・
            CrouchWalk,         // 縺励ｃ縺後∩豁ｩ縺・
            ProneMovement,      // 蛹榊倹遘ｻ蜍・
            QuickHide,          // 邱頑･髫阡ｽ
            SilentSprint,       // 辟｡髻ｳ逍ｾ襍ｰ
            WallHug,            // 螢∵ｲｿ縺・ｧｻ蜍・
            CoverTocover,       // 繧ｫ繝舌・髢鍋ｧｻ蜍・
            StealthClimb,       // 繧ｹ繝・Ν繧ｹ逋ｻ謾
            ShadowMove,         // 蠖ｱ遘ｻ蜍・
            DistractionMove     // 髯ｽ蜍慕ｧｻ蜍・
        }

        private StealthMovementType _movementType;
        private Vector3 _targetPosition;
        private float _duration;
        private float _speedMultiplier;
        private bool _maintainStealth;

        // 蜑榊屓縺ｮ迥ｶ諷具ｼ・ndo逕ｨ・・
        private Vector3 _previousPosition;
        private PlayerStateType _previousPlayerState;
        private float _previousVisibility;
        private float _previousNoiseLevel;

        // 繧ｵ繝ｼ繝薙せ蜿ら・
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;
        private Transform _playerTransform;

        // 螳溯｡檎ｵ先棡
        private bool _wasExecuted;
        private bool _stealthMaintained;

        #region ICommand Implementation

        public void Execute()
        {
            if (_wasExecuted) return;

            // ServiceLocator邨檎罰縺ｧ繧ｵ繝ｼ繝薙せ蜿門ｾ・
            _stealthService = ServiceLocator.GetService<IStealthService>();
            _playerStateMachine = Object.FindObjectOfType<DetailedPlayerStateMachine>();

            if (_playerStateMachine != null)
            {
                _playerTransform = _playerStateMachine.transform;
            }

            if (_stealthService == null || _playerStateMachine == null)
            {
                Debug.LogError("[StealthMovementCommand] Required services not available");
                return;
            }

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ菫晏ｭ假ｼ・ndo逕ｨ・・
            _previousPosition = _playerTransform.position;
            _previousPlayerState = _playerStateMachine.GetCurrentStateType();
            _previousVisibility = _stealthService.PlayerVisibilityFactor;
            _previousNoiseLevel = _stealthService.PlayerNoiseLevel;

            // 繧ｹ繝・Ν繧ｹ遘ｻ蜍募ｮ溯｡・
            ExecuteStealthMovement();
            _wasExecuted = true;

            Debug.Log($"[StealthMovementCommand] Executed: {_movementType} to {_targetPosition}");
        }

        public void Undo()
        {
            if (!_wasExecuted || !CanUndo) return;

            // 菴咲ｽｮ蠕ｩ蜈・
            if (_playerTransform != null)
            {
                _playerTransform.position = _previousPosition;
            }

            // 繝励Ξ繧､繝､繝ｼ迥ｶ諷句ｾｩ蜈・
            if (_playerStateMachine != null)
            {
                _playerStateMachine.TransitionToState(_previousPlayerState);
            }

            // 繧ｹ繝・Ν繧ｹ迥ｶ諷句ｾｩ蜈・
            if (_stealthService != null)
            {
                _stealthService.UpdatePlayerVisibility(_previousVisibility);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel);
            }

            _wasExecuted = false;
            Debug.Log($"[StealthMovementCommand] Undid: {_movementType}");
        }

        public bool CanUndo => _wasExecuted && _playerTransform != null;

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _movementType = StealthMovementType.SneakMode;
            _targetPosition = Vector3.zero;
            _duration = 1.0f;
            _speedMultiplier = 1.0f;
            _maintainStealth = true;

            _previousPosition = Vector3.zero;
            _previousPlayerState = PlayerStateType.Idle;
            _previousVisibility = 1.0f;
            _previousNoiseLevel = 0.0f;

            _stealthService = null;
            _playerStateMachine = null;
            _playerTransform = null;

            _wasExecuted = false;
            _stealthMaintained = false;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                Debug.LogError("[StealthMovementCommand] Invalid parameters. Expected: (StealthMovementType, Vector3, [float], [float], [bool])");
                return;
            }

            _movementType = (StealthMovementType)parameters[0];
            _targetPosition = (Vector3)parameters[1];

            // 繧ｪ繝励す繝ｧ繝ｳ繝代Λ繝｡繝ｼ繧ｿ
            _duration = parameters.Length > 2 ? (float)parameters[2] : 1.0f;
            _speedMultiplier = parameters.Length > 3 ? (float)parameters[3] : 1.0f;
            _maintainStealth = parameters.Length > 4 ? (bool)parameters[4] : true;
        }

        /// <summary>
        /// 蝙句ｮ牙・縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize(StealthMovementType movementType, Vector3 targetPosition,
                              float duration = 1.0f, float speedMultiplier = 1.0f, bool maintainStealth = true)
        {
            _movementType = movementType;
            _targetPosition = targetPosition;
            _duration = duration;
            _speedMultiplier = speedMultiplier;
            _maintainStealth = maintainStealth;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ遘ｻ蜍輔・蜈ｷ菴鍋噪螳溯｡・
        /// </summary>
        private void ExecuteStealthMovement()
        {
            switch (_movementType)
            {
                case StealthMovementType.SneakMode:
                    ExecuteSneakMode();
                    break;

                case StealthMovementType.CrouchWalk:
                    ExecuteCrouchWalk();
                    break;

                case StealthMovementType.ProneMovement:
                    ExecuteProneMovement();
                    break;

                case StealthMovementType.QuickHide:
                    ExecuteQuickHide();
                    break;

                case StealthMovementType.SilentSprint:
                    ExecuteSilentSprint();
                    break;

                case StealthMovementType.WallHug:
                    ExecuteWallHug();
                    break;

                case StealthMovementType.CoverTocover:
                    ExecuteCoverTocover();
                    break;

                case StealthMovementType.StealthClimb:
                    ExecuteStealthClimb();
                    break;

                case StealthMovementType.ShadowMove:
                    ExecuteShadowMove();
                    break;

                case StealthMovementType.DistractionMove:
                    ExecuteDistractionMove();
                    break;
            }
        }

        private void ExecuteSneakMode()
        {
            // 蠢阪・雜ｳ繝｢繝ｼ繝会ｼ夊ｦ冶ｪ肴ｧ30%蜑頑ｸ帙∫ｧｻ蜍暮溷ｺｦ70%蜑頑ｸ・
            _playerStateMachine.TransitionToState(PlayerStateType.Walking);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.7f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.3f);

            MoveToTarget(0.3f); // 30%騾溷ｺｦ
            _stealthMaintained = true;
        }

        private void ExecuteCrouchWalk()
        {
            // 縺励ｃ縺後∩豁ｩ縺搾ｼ夊ｦ冶ｪ肴ｧ50%蜑頑ｸ帙∫ｧｻ蜍暮溷ｺｦ50%蜑頑ｸ・
            _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.5f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

            MoveToTarget(0.5f); // 50%騾溷ｺｦ
            _stealthMaintained = true;
        }

        private void ExecuteProneMovement()
        {
            // 蛹榊倹遘ｻ蜍包ｼ夊ｦ冶ｪ肴ｧ80%蜑頑ｸ帙∫ｧｻ蜍暮溷ｺｦ90%蜑頑ｸ・
            _playerStateMachine.TransitionToState(PlayerStateType.Prone);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.2f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.1f);

            MoveToTarget(0.1f); // 10%騾溷ｺｦ
            _stealthMaintained = true;
        }

        private void ExecuteQuickHide()
        {
            // 邱頑･髫阡ｽ・壽怙蟇・ｊ縺ｮ髫阡ｽ繝昴う繝ｳ繝医∈遘ｻ蜍・
            Vector3 hidePosition = FindNearestHidingSpot();
            if (hidePosition != Vector3.zero)
            {
                _targetPosition = hidePosition;
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(0.1f); // 90%隕冶ｪ肴ｧ蜑頑ｸ・

                MoveToTarget(1.5f); // 150%騾溷ｺｦ・育ｷ頑･・・
                _stealthMaintained = true;
            }
        }

        private void ExecuteSilentSprint()
        {
            // 辟｡髻ｳ逍ｾ襍ｰ・夐ｫ倬溽ｧｻ蜍輔□縺碁浹髻ｿ蛻ｶ蠕｡
            _playerStateMachine.TransitionToState(PlayerStateType.Running);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 1.2f); // 20%隕冶ｪ肴ｧ蠅怜刈
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.4f); // 60%鬨帝浹蜑頑ｸ・

            MoveToTarget(2.0f); // 200%騾溷ｺｦ
            _stealthMaintained = _maintainStealth;
        }

        private void ExecuteWallHug()
        {
            // 螢∵ｲｿ縺・ｧｻ蜍包ｼ壼｣√↓豐ｿ縺｣縺溷ｮ牙・縺ｪ遘ｻ蜍・
            Vector3 wallDirection = FindWallDirection();
            if (wallDirection != Vector3.zero)
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.6f);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.3f);

                MoveAlongWall(wallDirection);
                _stealthMaintained = true;
            }
        }

        private void ExecuteCoverTocover()
        {
            // 繧ｫ繝舌・髢鍋ｧｻ蜍包ｼ夐・阡ｽ迚ｩ髢薙・謌ｦ陦鍋噪遘ｻ蜍・
            if (ValidateCoverPath())
            {
                _playerStateMachine.TransitionToState(PlayerStateType.InCover);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.4f);
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

                MoveToTarget(1.0f);
                _stealthMaintained = true;
            }
        }

        private void ExecuteStealthClimb()
        {
            // 繧ｹ繝・Ν繧ｹ逋ｻ謾・夐撕縺九↑逋ｻ謾遘ｻ蜍・
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.8f);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.5f);

            ClimbToTarget();
            _stealthMaintained = true;
        }

        private void ExecuteShadowMove()
        {
            // 蠖ｱ遘ｻ蜍包ｼ壼ｽｱ繧貞茜逕ｨ縺励◆遘ｻ蜍・
            if (IsInShadow(_targetPosition))
            {
                _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.3f); // 70%隕冶ｪ肴ｧ蜑頑ｸ・
                _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * 0.2f);

                MoveToTarget(0.8f);
                _stealthMaintained = true;
            }
        }

        private void ExecuteDistractionMove()
        {
            // 髯ｽ蜍慕ｧｻ蜍包ｼ壽ｳｨ諢上ｒ騾ｸ繧峨＠縺ｪ縺後ｉ縺ｮ遘ｻ蜍・
            CreateDistraction();
            _playerStateMachine.TransitionToState(PlayerStateType.Crouching);
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.9f);

            MoveToTarget(1.2f);
            _stealthMaintained = _maintainStealth;
        }

        #endregion

        #region Helper Methods

        private void MoveToTarget(float speedModifier)
        {
            if (_playerTransform == null) return;

            float adjustedSpeed = _speedMultiplier * speedModifier;
            Vector3 direction = (_targetPosition - _playerTransform.position).normalized;

            // 邁｡譏鍋ｧｻ蜍募ｮ溯｣・ｼ亥ｮ滄圀縺ｫ縺ｯCharacterController繧НavMeshAgent繧剃ｽｿ逕ｨ・・
            _playerTransform.position = Vector3.MoveTowards(
                _playerTransform.position,
                _targetPosition,
                adjustedSpeed * Time.deltaTime
            );
        }

        private Vector3 FindNearestHidingSpot()
        {
            // 譛蟇・ｊ縺ｮ髫阡ｽ繝昴う繝ｳ繝医ｒ謗｢邏｢
            GameObject[] hidingSpots = GameObject.FindGameObjectsWithTag("HidingSpot");
            Vector3 nearestSpot = Vector3.zero;
            float nearestDistance = float.MaxValue;

            foreach (GameObject spot in hidingSpots)
            {
                float distance = Vector3.Distance(_playerTransform.position, spot.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpot = spot.transform.position;
                }
            }

            return nearestSpot;
        }

        private Vector3 FindWallDirection()
        {
            // 螢∵婿蜷代・讀懷・・医Ξ繧､繧ｭ繝｣繧ｹ繝医↓繧医ｋ邁｡譏灘ｮ溯｣・ｼ・
            RaycastHit hit;
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (Vector3 dir in directions)
            {
                if (Physics.Raycast(_playerTransform.position, dir, out hit, 2.0f))
                {
                    if (hit.collider.CompareTag("Wall"))
                    {
                        return Vector3.Cross(hit.normal, Vector3.up);
                    }
                }
            }

            return Vector3.zero;
        }

        private void MoveAlongWall(Vector3 wallDirection)
        {
            Vector3 targetAlongWall = _playerTransform.position + wallDirection * 5.0f;
            MoveToTarget(0.8f);
        }

        private bool ValidateCoverPath()
        {
            // 繧ｫ繝舌・髢薙・邨瑚ｷｯ讀懆ｨｼ
            RaycastHit hit;
            Vector3 direction = (_targetPosition - _playerTransform.position).normalized;
            float distance = Vector3.Distance(_playerTransform.position, _targetPosition);

            return !Physics.Raycast(_playerTransform.position, direction, out hit, distance, LayerMask.GetMask("Enemy"));
        }

        private void ClimbToTarget()
        {
            // 逋ｻ謾遘ｻ蜍輔・螳溯｣・
            Vector3 climbDirection = Vector3.up + (_targetPosition - _playerTransform.position).normalized;
            _playerTransform.position = Vector3.MoveTowards(
                _playerTransform.position,
                _targetPosition,
                _speedMultiplier * 0.5f * Time.deltaTime
            );
        }

        private bool IsInShadow(Vector3 position)
        {
            // 蠖ｱ縺ｫ縺・ｋ縺九←縺・°縺ｮ蛻､螳夲ｼ育ｰ｡譏灘ｮ溯｣・ｼ・
            return _stealthService?.IsPlayerConcealed ?? false;
        }

        private void CreateDistraction()
        {
            // 髯ｽ蜍暮浹縺ｮ菴懈・
            Vector3 distractionPoint = _playerTransform.position + Vector3.right * 5.0f;
            _stealthService?.CreateDistraction(distractionPoint, 0.6f);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 遘ｻ蜍輔ち繧､繝・
        /// </summary>
        public StealthMovementType MovementType => _movementType;

        /// <summary>
        /// 逶ｮ讓吩ｽ咲ｽｮ
        /// </summary>
        public Vector3 TargetPosition => _targetPosition;

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ縺檎ｶｭ謖√＆繧後◆縺九←縺・°
        /// </summary>
        public bool StealthMaintained => _stealthMaintained;

        /// <summary>
        /// 螳溯｡梧ｸ医∩縺九←縺・°
        /// </summary>
        public bool WasExecuted => _wasExecuted;

        #endregion
    }
}


