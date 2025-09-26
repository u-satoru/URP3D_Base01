using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ閭ｽ蜉帙さ繝槭Φ繝峨す繧ｹ繝・Β
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ繧ｹ繝・Ν繧ｹ迚ｹ谿願・蜉帙ｒ繧ｫ繝励そ繝ｫ蛹・
    /// ServiceLocator邨ｱ蜷医→ObjectPool譛驕ｩ蛹悶↓繧医ｋ鬮俶ｧ閭ｽ螳溯｣・
    /// </summary>
    public class StealthAbilityCommand : IResettableCommand
    {
        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ閭ｽ蜉帙・遞ｮ鬘・
        /// </summary>
        public enum StealthAbilityType
        {
            InvisibilityCloak,      // 蜈牙ｭｦ霑ｷ蠖ｩ
            SoundDampening,         // 髻ｳ髻ｿ貂幄｡ｰ
            MotionDetection,        // 蜍穂ｽ懈､懃衍
            ThermalMasking,         // 辭ｱ貅宣・阡ｽ
            ElectronicJamming,      // 髮ｻ蟄仙ｦｨ螳ｳ
            TimeSlowdown,           // 譎る俣貂幃・
            WallPhase,              // 螢・夐℃
            ShadowMeld,             // 蠖ｱ陞榊粋
            DistractionProjection, // 蟷ｻ蠖ｱ謚募ｰ・
            EnvironmentalCamouflage // 迺ｰ蠅・ｿｷ蠖ｩ
        }

        private StealthAbilityType _abilityType;
        private float _duration;
        private float _intensity;
        private Vector3 _targetLocation;
        private GameObject _targetObject;

        // 閭ｽ蜉帛ｮ溯｡悟燕縺ｮ迥ｶ諷具ｼ・ndo逕ｨ・・
        private float _previousVisibility;
        private float _previousNoiseLevel;
        private bool _previousStealthMode;
        private Vector3 _previousPosition;
        private PlayerStateType _previousPlayerState;

        // 閭ｽ蜉帛柑譫懊・迥ｶ諷・
        private bool _abilityActive;
        private float _remainingDuration;
        private float _activationTime;

        // 繧ｵ繝ｼ繝薙せ蜿ら・
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;
        private Transform _playerTransform;

        // 螳溯｡檎ｵ先棡
        private bool _wasExecuted;
        private bool _wasSuccessful;

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

            if (_stealthService == null)
            {
                Debug.LogError("[StealthAbilityCommand] StealthService not available");
                return;
            }

            // 迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ菫晏ｭ假ｼ・ndo逕ｨ・・
            SaveCurrentState();

            // 閭ｽ蜉帛ｮ溯｡・
            _wasSuccessful = ExecuteStealthAbility();
            _wasExecuted = true;

            if (_wasSuccessful)
            {
                _abilityActive = true;
                _remainingDuration = _duration;
                _activationTime = Time.time;
            }

            Debug.Log($"[StealthAbilityCommand] Executed: {_abilityType}, Success: {_wasSuccessful}");
        }

        public void Undo()
        {
            if (!_wasExecuted || !CanUndo) return;

            // 閭ｽ蜉帛柑譫懊ｒ辟｡蜉ｹ蛹・
            DeactivateAbility();

            // 迥ｶ諷句ｾｩ蜈・
            RestorePreviousState();

            _wasExecuted = false;
            _abilityActive = false;
            _wasSuccessful = false;

            Debug.Log($"[StealthAbilityCommand] Undid: {_abilityType}");
        }

        public bool CanUndo => _wasExecuted && _stealthService != null;

        #endregion

        #region IResettableCommand Implementation

        public void Reset()
        {
            _abilityType = StealthAbilityType.InvisibilityCloak;
            _duration = 5.0f;
            _intensity = 1.0f;
            _targetLocation = Vector3.zero;
            _targetObject = null;

            _previousVisibility = 1.0f;
            _previousNoiseLevel = 0.0f;
            _previousStealthMode = false;
            _previousPosition = Vector3.zero;
            _previousPlayerState = PlayerStateType.Idle;

            _abilityActive = false;
            _remainingDuration = 0.0f;
            _activationTime = 0.0f;

            _stealthService = null;
            _playerStateMachine = null;
            _playerTransform = null;

            _wasExecuted = false;
            _wasSuccessful = false;
        }

        public void Initialize(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
            {
                Debug.LogError("[StealthAbilityCommand] Invalid parameters. Expected: (StealthAbilityType, [float], [float], [Vector3], [GameObject])");
                return;
            }

            _abilityType = (StealthAbilityType)parameters[0];

            // 繧ｪ繝励す繝ｧ繝ｳ繝代Λ繝｡繝ｼ繧ｿ
            _duration = parameters.Length > 1 ? (float)parameters[1] : 5.0f;
            _intensity = parameters.Length > 2 ? (float)parameters[2] : 1.0f;
            _targetLocation = parameters.Length > 3 ? (Vector3)parameters[3] : Vector3.zero;
            _targetObject = parameters.Length > 4 ? (GameObject)parameters[4] : null;
        }

        /// <summary>
        /// 蝙句ｮ牙・縺ｪ蛻晄悄蛹悶Γ繧ｽ繝・ラ
        /// </summary>
        public void Initialize(StealthAbilityType abilityType, float duration = 5.0f, float intensity = 1.0f,
                              Vector3 targetLocation = default, GameObject targetObject = null)
        {
            _abilityType = abilityType;
            _duration = duration;
            _intensity = intensity;
            _targetLocation = targetLocation;
            _targetObject = targetObject;
        }

        #endregion

        #region Private Methods

        private void SaveCurrentState()
        {
            _previousVisibility = _stealthService.PlayerVisibilityFactor;
            _previousNoiseLevel = _stealthService.PlayerNoiseLevel;
            _previousStealthMode = _stealthService.IsStealthModeActive;

            if (_playerStateMachine != null)
            {
                _previousPlayerState = _playerStateMachine.GetCurrentStateType();
            }

            if (_playerTransform != null)
            {
                _previousPosition = _playerTransform.position;
            }
        }

        private void RestorePreviousState()
        {
            _stealthService.UpdatePlayerVisibility(_previousVisibility);
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel);

            if (_previousStealthMode != _stealthService.IsStealthModeActive)
            {
                _stealthService.SetStealthMode(_previousStealthMode);
            }

            if (_playerStateMachine != null)
            {
                _playerStateMachine.TransitionToState(_previousPlayerState);
            }

            if (_playerTransform != null)
            {
                _playerTransform.position = _previousPosition;
            }
        }

        /// <summary>
        /// 繧ｹ繝・Ν繧ｹ閭ｽ蜉帙・蜈ｷ菴鍋噪螳溯｡・
        /// </summary>
        private bool ExecuteStealthAbility()
        {
            switch (_abilityType)
            {
                case StealthAbilityType.InvisibilityCloak:
                    return ExecuteInvisibilityCloak();

                case StealthAbilityType.SoundDampening:
                    return ExecuteSoundDampening();

                case StealthAbilityType.MotionDetection:
                    return ExecuteMotionDetection();

                case StealthAbilityType.ThermalMasking:
                    return ExecuteThermalMasking();

                case StealthAbilityType.ElectronicJamming:
                    return ExecuteElectronicJamming();

                case StealthAbilityType.TimeSlowdown:
                    return ExecuteTimeSlowdown();

                case StealthAbilityType.WallPhase:
                    return ExecuteWallPhase();

                case StealthAbilityType.ShadowMeld:
                    return ExecuteShadowMeld();

                case StealthAbilityType.DistractionProjection:
                    return ExecuteDistractionProjection();

                case StealthAbilityType.EnvironmentalCamouflage:
                    return ExecuteEnvironmentalCamouflage();

                default:
                    return false;
            }
        }

        private bool ExecuteInvisibilityCloak()
        {
            // 蜈牙ｭｦ霑ｷ蠖ｩ・壼ｮ悟・騾乗・蛹・
            float invisibilityFactor = Mathf.Clamp01(1.0f - _intensity);
            _stealthService.UpdatePlayerVisibility(invisibilityFactor);
            _stealthService.SetStealthMode(true);

            Debug.Log($"[StealthAbilityCommand] Invisibility activated: {_intensity * 100}% for {_duration}s");
            return true;
        }

        private bool ExecuteSoundDampening()
        {
            // 髻ｳ髻ｿ貂幄｡ｰ・夐浹縺ｮ螟ｧ蟷・炎貂・
            float soundReduction = _intensity * 0.9f; // 譛螟ｧ90%蜑頑ｸ・
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * (1.0f - soundReduction));

            Debug.Log($"[StealthAbilityCommand] Sound dampening activated: {soundReduction * 100}% reduction");
            return true;
        }

        private bool ExecuteMotionDetection()
        {
            // 蜍穂ｽ懈､懃衍・壼捉蝗ｲ縺ｮ謨ｵ縺ｮ蜍輔″繧呈─遏･
            Collider[] nearbyEnemies = Physics.OverlapSphere(_playerTransform.position, 20.0f * _intensity,
                LayerMask.GetMask("Enemy"));

            foreach (var enemy in nearbyEnemies)
            {
                // 謨ｵ繧呈､懃衍貂医∩縺ｨ縺励※繝槭・繧ｯ・・I陦ｨ遉ｺ遲会ｼ・
                Debug.Log($"[MotionDetection] Enemy detected: {enemy.name} at {enemy.transform.position}");
            }

            return nearbyEnemies.Length > 0;
        }

        private bool ExecuteThermalMasking()
        {
            // 辭ｱ貅宣・阡ｽ・夊ｵ､螟也ｷ壽､懃衍蝗樣∩
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.3f);

            // 辭ｱ貅先､懃衍NPC縺ｫ蟇ｾ縺吶ｋ迚ｹ蛻･縺ｪ髫阡ｽ蜉ｹ譫・
            Debug.Log("[StealthAbilityCommand] Thermal masking activated - immune to infrared detection");
            return true;
        }

        private bool ExecuteElectronicJamming()
        {
            // 髮ｻ蟄仙ｦｨ螳ｳ・夐崕蟄先ｩ溷勣縺ｮ辟｡蜉帛喧
            Collider[] electronics = Physics.OverlapSphere(_playerTransform.position, 10.0f * _intensity,
                LayerMask.GetMask("Electronics"));

            foreach (var device in electronics)
            {
                if (device.CompareTag("SecurityCamera") || device.CompareTag("Alarm"))
                {
                    _stealthService.InteractWithEnvironment(device.gameObject, StealthInteractionType.DisableCamera);
                }
            }

            Debug.Log($"[StealthAbilityCommand] Electronic jamming activated: {electronics.Length} devices affected");
            return electronics.Length > 0;
        }

        private bool ExecuteTimeSlowdown()
        {
            // 譎る俣貂幃滂ｼ壼渚蠢懈凾髢灘髄荳・
            Time.timeScale = 1.0f - (_intensity * 0.7f); // 譛螟ｧ70%貂幃・
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            Debug.Log($"[StealthAbilityCommand] Time slowdown activated: {Time.timeScale * 100}% speed");
            return true;
        }

        private bool ExecuteWallPhase()
        {
            // 螢・夐℃・夂洒霍晞屬迸ｬ髢鍋ｧｻ蜍・
            if (_targetLocation != Vector3.zero)
            {
                Vector3 direction = (_targetLocation - _playerTransform.position).normalized;
                float maxDistance = 5.0f * _intensity;

                RaycastHit hit;
                if (Physics.Raycast(_playerTransform.position, direction, out hit, maxDistance))
                {
                    // 螢√・蜷代％縺・・縺ｫ遘ｻ蜍・
                    Vector3 phasePosition = hit.point + direction * 2.0f;
                    _playerTransform.position = phasePosition;

                    Debug.Log("[StealthAbilityCommand] Wall phase successful");
                    return true;
                }
            }

            return false;
        }

        private bool ExecuteShadowMeld()
        {
            // 蠖ｱ陞榊粋・壼ｽｱ縺ｮ荳ｭ縺ｧ螳悟・髫阡ｽ
            if (_stealthService.IsPlayerConcealed)
            {
                _stealthService.UpdatePlayerVisibility(0.05f); // 95%隕冶ｪ肴ｧ蜑頑ｸ・
                _stealthService.UpdatePlayerNoiseLevel(0.0f);  // 螳悟・辟｡髻ｳ

                Debug.Log("[StealthAbilityCommand] Shadow meld activated - near perfect concealment");
                return true;
            }

            Debug.Log("[StealthAbilityCommand] Shadow meld failed - not in shadow");
            return false;
        }

        private bool ExecuteDistractionProjection()
        {
            // 蟷ｻ蠖ｱ謚募ｰ・ｼ夐區蜍慕畑縺ｮ蟷ｻ蠖ｱ繧剃ｽ懈・
            Vector3 projectionPoint = _targetLocation != Vector3.zero ?
                _targetLocation : _playerTransform.position + _playerTransform.forward * 10.0f;

            _stealthService.CreateDistraction(projectionPoint, _intensity);

            Debug.Log($"[StealthAbilityCommand] Distraction projection created at {projectionPoint}");
            return true;
        }

        private bool ExecuteEnvironmentalCamouflage()
        {
            // 迺ｰ蠅・ｿｷ蠖ｩ・壼捉蝗ｲ縺ｮ迺ｰ蠅・↓貅ｶ縺題ｾｼ繧
            Collider[] envObjects = Physics.OverlapSphere(_playerTransform.position, 5.0f,
                LayerMask.GetMask("Environment"));

            if (envObjects.Length > 0)
            {
                float camoEffectiveness = Mathf.Min(_intensity, envObjects.Length * 0.2f);
                _stealthService.UpdatePlayerVisibility(_previousVisibility * (1.0f - camoEffectiveness));

                Debug.Log($"[StealthAbilityCommand] Environmental camouflage: {camoEffectiveness * 100}% effectiveness");
                return true;
            }

            return false;
        }

        private void DeactivateAbility()
        {
            switch (_abilityType)
            {
                case StealthAbilityType.TimeSlowdown:
                    // 譎る俣繧ｹ繧ｱ繝ｼ繝ｫ繧貞・縺ｫ謌ｻ縺・
                    Time.timeScale = 1.0f;
                    Time.fixedDeltaTime = 0.02f;
                    break;

                case StealthAbilityType.WallPhase:
                    // 螢・夐℃縺ｯ迸ｬ髢灘柑譫懊↑縺ｮ縺ｧ迚ｹ蛻･縺ｪ辟｡蜉ｹ蛹悶・荳崎ｦ・
                    break;

                // 縺昴・莉悶・閭ｽ蜉帙・迥ｶ諷句ｾｩ蜈・〒蜃ｦ逅・
            }

            _abilityActive = false;
            _remainingDuration = 0.0f;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 閭ｽ蜉帙ち繧､繝・
        /// </summary>
        public StealthAbilityType AbilityType => _abilityType;

        /// <summary>
        /// 謖∫ｶ壽凾髢・
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 蠑ｷ蠎ｦ
        /// </summary>
        public float Intensity => _intensity;

        /// <summary>
        /// 閭ｽ蜉帙′繧｢繧ｯ繝・ぅ繝悶°縺ｩ縺・°
        /// </summary>
        public bool IsActive => _abilityActive;

        /// <summary>
        /// 谿九ｊ譎る俣
        /// </summary>
        public float RemainingDuration => _remainingDuration;

        /// <summary>
        /// 螳溯｡後′謌仙粥縺励◆縺九←縺・°
        /// </summary>
        public bool WasSuccessful => _wasSuccessful;

        #endregion

        #region Public Methods

        /// <summary>
        /// 閭ｽ蜉帙・譖ｴ譁ｰ・域戟邯壽凾髢鍋ｮ｡逅・ｼ・
        /// </summary>
        public void UpdateAbility()
        {
            if (!_abilityActive) return;

            _remainingDuration -= Time.deltaTime;

            if (_remainingDuration <= 0.0f)
            {
                // 閾ｪ蜍慕噪縺ｫ閭ｽ蜉帙ｒ辟｡蜉ｹ蛹・
                DeactivateAbility();
                Debug.Log($"[StealthAbilityCommand] {_abilityType} duration expired");
            }
        }

        /// <summary>
        /// 謇句虚縺ｧ閭ｽ蜉帙ｒ辟｡蜉ｹ蛹・
        /// </summary>
        public void ManualDeactivate()
        {
            if (_abilityActive)
            {
                DeactivateAbility();
                Debug.Log($"[StealthAbilityCommand] {_abilityType} manually deactivated");
            }
        }

        #endregion
    }
}


