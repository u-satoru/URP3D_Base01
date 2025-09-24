using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Player.States;

namespace asterivo.Unity60.Features.Templates.Stealth.Commands
{
    /// <summary>
    /// ステルス能力コマンドシステム
    /// プレイヤーのステルス特殊能力をカプセル化
    /// ServiceLocator統合とObjectPool最適化による高性能実装
    /// </summary>
    public class StealthAbilityCommand : IResettableCommand
    {
        /// <summary>
        /// ステルス能力の種類
        /// </summary>
        public enum StealthAbilityType
        {
            InvisibilityCloak,      // 光学迷彩
            SoundDampening,         // 音響減衰
            MotionDetection,        // 動作検知
            ThermalMasking,         // 熱源遮蔽
            ElectronicJamming,      // 電子妨害
            TimeSlowdown,           // 時間減速
            WallPhase,              // 壁通過
            ShadowMeld,             // 影融合
            DistractionProjection, // 幻影投射
            EnvironmentalCamouflage // 環境迷彩
        }

        private StealthAbilityType _abilityType;
        private float _duration;
        private float _intensity;
        private Vector3 _targetLocation;
        private GameObject _targetObject;

        // 能力実行前の状態（Undo用）
        private float _previousVisibility;
        private float _previousNoiseLevel;
        private bool _previousStealthMode;
        private Vector3 _previousPosition;
        private PlayerStateType _previousPlayerState;

        // 能力効果の状態
        private bool _abilityActive;
        private float _remainingDuration;
        private float _activationTime;

        // サービス参照
        private IStealthService _stealthService;
        private DetailedPlayerStateMachine _playerStateMachine;
        private Transform _playerTransform;

        // 実行結果
        private bool _wasExecuted;
        private bool _wasSuccessful;

        #region ICommand Implementation

        public void Execute()
        {
            if (_wasExecuted) return;

            // ServiceLocator経由でサービス取得
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

            // 現在の状態を保存（Undo用）
            SaveCurrentState();

            // 能力実行
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

            // 能力効果を無効化
            DeactivateAbility();

            // 状態復元
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

            // オプションパラメータ
            _duration = parameters.Length > 1 ? (float)parameters[1] : 5.0f;
            _intensity = parameters.Length > 2 ? (float)parameters[2] : 1.0f;
            _targetLocation = parameters.Length > 3 ? (Vector3)parameters[3] : Vector3.zero;
            _targetObject = parameters.Length > 4 ? (GameObject)parameters[4] : null;
        }

        /// <summary>
        /// 型安全な初期化メソッド
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
        /// ステルス能力の具体的実行
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
            // 光学迷彩：完全透明化
            float invisibilityFactor = Mathf.Clamp01(1.0f - _intensity);
            _stealthService.UpdatePlayerVisibility(invisibilityFactor);
            _stealthService.SetStealthMode(true);

            Debug.Log($"[StealthAbilityCommand] Invisibility activated: {_intensity * 100}% for {_duration}s");
            return true;
        }

        private bool ExecuteSoundDampening()
        {
            // 音響減衰：音の大幅削減
            float soundReduction = _intensity * 0.9f; // 最大90%削減
            _stealthService.UpdatePlayerNoiseLevel(_previousNoiseLevel * (1.0f - soundReduction));

            Debug.Log($"[StealthAbilityCommand] Sound dampening activated: {soundReduction * 100}% reduction");
            return true;
        }

        private bool ExecuteMotionDetection()
        {
            // 動作検知：周囲の敵の動きを感知
            Collider[] nearbyEnemies = Physics.OverlapSphere(_playerTransform.position, 20.0f * _intensity,
                LayerMask.GetMask("Enemy"));

            foreach (var enemy in nearbyEnemies)
            {
                // 敵を検知済みとしてマーク（UI表示等）
                Debug.Log($"[MotionDetection] Enemy detected: {enemy.name} at {enemy.transform.position}");
            }

            return nearbyEnemies.Length > 0;
        }

        private bool ExecuteThermalMasking()
        {
            // 熱源遮蔽：赤外線検知回避
            _stealthService.UpdatePlayerVisibility(_previousVisibility * 0.3f);

            // 熱源検知NPCに対する特別な隠蔽効果
            Debug.Log("[StealthAbilityCommand] Thermal masking activated - immune to infrared detection");
            return true;
        }

        private bool ExecuteElectronicJamming()
        {
            // 電子妨害：電子機器の無力化
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
            // 時間減速：反応時間向上
            Time.timeScale = 1.0f - (_intensity * 0.7f); // 最大70%減速
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            Debug.Log($"[StealthAbilityCommand] Time slowdown activated: {Time.timeScale * 100}% speed");
            return true;
        }

        private bool ExecuteWallPhase()
        {
            // 壁通過：短距離瞬間移動
            if (_targetLocation != Vector3.zero)
            {
                Vector3 direction = (_targetLocation - _playerTransform.position).normalized;
                float maxDistance = 5.0f * _intensity;

                RaycastHit hit;
                if (Physics.Raycast(_playerTransform.position, direction, out hit, maxDistance))
                {
                    // 壁の向こう側に移動
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
            // 影融合：影の中で完全隠蔽
            if (_stealthService.IsPlayerConcealed)
            {
                _stealthService.UpdatePlayerVisibility(0.05f); // 95%視認性削減
                _stealthService.UpdatePlayerNoiseLevel(0.0f);  // 完全無音

                Debug.Log("[StealthAbilityCommand] Shadow meld activated - near perfect concealment");
                return true;
            }

            Debug.Log("[StealthAbilityCommand] Shadow meld failed - not in shadow");
            return false;
        }

        private bool ExecuteDistractionProjection()
        {
            // 幻影投射：陽動用の幻影を作成
            Vector3 projectionPoint = _targetLocation != Vector3.zero ?
                _targetLocation : _playerTransform.position + _playerTransform.forward * 10.0f;

            _stealthService.CreateDistraction(projectionPoint, _intensity);

            Debug.Log($"[StealthAbilityCommand] Distraction projection created at {projectionPoint}");
            return true;
        }

        private bool ExecuteEnvironmentalCamouflage()
        {
            // 環境迷彩：周囲の環境に溶け込む
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
                    // 時間スケールを元に戻す
                    Time.timeScale = 1.0f;
                    Time.fixedDeltaTime = 0.02f;
                    break;

                case StealthAbilityType.WallPhase:
                    // 壁通過は瞬間効果なので特別な無効化は不要
                    break;

                // その他の能力は状態復元で処理
            }

            _abilityActive = false;
            _remainingDuration = 0.0f;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 能力タイプ
        /// </summary>
        public StealthAbilityType AbilityType => _abilityType;

        /// <summary>
        /// 持続時間
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// 強度
        /// </summary>
        public float Intensity => _intensity;

        /// <summary>
        /// 能力がアクティブかどうか
        /// </summary>
        public bool IsActive => _abilityActive;

        /// <summary>
        /// 残り時間
        /// </summary>
        public float RemainingDuration => _remainingDuration;

        /// <summary>
        /// 実行が成功したかどうか
        /// </summary>
        public bool WasSuccessful => _wasSuccessful;

        #endregion

        #region Public Methods

        /// <summary>
        /// 能力の更新（持続時間管理）
        /// </summary>
        public void UpdateAbility()
        {
            if (!_abilityActive) return;

            _remainingDuration -= Time.deltaTime;

            if (_remainingDuration <= 0.0f)
            {
                // 自動的に能力を無効化
                DeactivateAbility();
                Debug.Log($"[StealthAbilityCommand] {_abilityType} duration expired");
            }
        }

        /// <summary>
        /// 手動で能力を無効化
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
