using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Stealth.Detection;

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// ステルスシステムの中央制御サービス実装
    /// ServiceLocatorパターンに準拠し、既存のDetectionSystemと統合
    /// </summary>
    public class StealthService : MonoBehaviour, IStealthService
    {
        [Header("Stealth Configuration")]
        [SerializeField] private DetectionConfiguration _detectionConfig;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private bool _enableDebugLogs = true;

        [Header("Light and Shadow System")]
        [SerializeField] private LayerMask _lightSourceLayers = -1;
        [SerializeField] private LayerMask _obstructionLayers = -1;
        [SerializeField] private float _ambientLightLevel = 0.1f;

        [Header("Audio Integration")]
        [SerializeField] private string _stealthAudioChannelName = "StealthAudio";

        // Core system components
        private VisibilityCalculator _visibilityCalculator;
        private readonly HashSet<IConcealmentZone> _activeConcealmentZones = new HashSet<IConcealmentZone>();
        private IConcealmentZone _currentConcealmentZone;

        // State tracking
        private float _currentVisibilityFactor = 1.0f;
        private float _currentNoiseLevel = 0.0f;
        private bool _isStealthModeActive = false;
        private StealthStatistics _statistics;

        // Events for communication with other systems
        private GameEvent<float> _playerVisibilityChangedEvent;
        private GameEvent<float> _playerNoiseEvent;
        private GameEvent<StealthDetectionData> _stealthDetectionEvent;

        #region Service Lifecycle
        public string ServiceName => "StealthService";
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[{ServiceName}] Already initialized!");
                return;
            }

            InitializeComponents();
            InitializeEvents();
            SubscribeToEvents();

            IsInitialized = true;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Initialized successfully");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            UnsubscribeFromEvents();
            IsInitialized = false;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Shutdown completed");
        }

        private void InitializeComponents()
        {
            // VisibilityCalculatorの初期化
            _visibilityCalculator = GetComponent<VisibilityCalculator>();
            if (_visibilityCalculator == null)
            {
                _visibilityCalculator = gameObject.AddComponent<VisibilityCalculator>();
            }

            // プレイヤートランスフォームの自動検出
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }

            // DetectionConfigurationの自動取得
            if (_detectionConfig == null)
            {
                _detectionConfig = Resources.Load<DetectionConfiguration>("StealthDetectionConfig");
            }

            // 統計情報の初期化
            _statistics = new StealthStatistics
            {
                AverageVisibilityFactor = 1.0f,
                TotalStealthTime = 0.0f
            };
        }

        private void InitializeEvents()
        {
            // GameEventの初期化
            _playerVisibilityChangedEvent = ScriptableObject.CreateInstance<GameEvent<float>>();
            _playerVisibilityChangedEvent.name = "PlayerVisibilityChangedEvent";

            _playerNoiseEvent = ScriptableObject.CreateInstance<GameEvent<float>>();
            _playerNoiseEvent.name = "PlayerNoiseEvent";

            _stealthDetectionEvent = ScriptableObject.CreateInstance<GameEvent<StealthDetectionData>>();
            _stealthDetectionEvent.name = "StealthDetectionEvent";
        }

        private void SubscribeToEvents()
        {
            // AI検出システムからのイベントを購読
            // 既存のイベントシステムと統合
        }

        private void UnsubscribeFromEvents()
        {
            // イベント購読の解除
        }
        #endregion

        #region Visibility Management
        public float PlayerVisibilityFactor => _currentVisibilityFactor;
        public float PlayerNoiseLevel => _currentNoiseLevel;

        public float CalculateLightLevel(Vector3 position)
        {
            if (_visibilityCalculator == null) return _ambientLightLevel;

            // 既存のVisibilityCalculatorを活用した光量計算
            var lightSources = FindObjectsOfType<Light>();
            float totalLight = _ambientLightLevel;

            foreach (var light in lightSources)
            {
                if (light.enabled && ((1 << light.gameObject.layer) & _lightSourceLayers) != 0)
                {
                    float distance = Vector3.Distance(position, light.transform.position);

                    // 光源の種類に応じた計算
                    if (light.type == LightType.Point)
                    {
                        float intensity = Mathf.Clamp01(1.0f - (distance / light.range));
                        totalLight += light.intensity * intensity;
                    }
                    else if (light.type == LightType.Spot)
                    {
                        Vector3 directionToPosition = (position - light.transform.position).normalized;
                        float angle = Vector3.Angle(light.transform.forward, directionToPosition);

                        if (angle <= light.spotAngle * 0.5f)
                        {
                            float intensity = Mathf.Clamp01(1.0f - (distance / light.range));
                            float angleIntensity = Mathf.Clamp01(1.0f - (angle / (light.spotAngle * 0.5f)));
                            totalLight += light.intensity * intensity * angleIntensity;
                        }
                    }
                }
            }

            return Mathf.Clamp01(totalLight);
        }

        public void UpdatePlayerVisibility(float visibilityFactor)
        {
            float previousFactor = _currentVisibilityFactor;
            _currentVisibilityFactor = Mathf.Clamp01(visibilityFactor);

            // 隠蔽ゾーンの効果を適用
            if (_currentConcealmentZone != null && _currentConcealmentZone.IsActive)
            {
                _currentVisibilityFactor *= (1.0f - _currentConcealmentZone.ConcealmentStrength);
            }

            // 統計情報の更新
            UpdateVisibilityStatistics();

            // イベント発行（変化があった場合のみ）
            if (Mathf.Abs(_currentVisibilityFactor - previousFactor) > 0.01f)
            {
                _playerVisibilityChangedEvent?.Raise(_currentVisibilityFactor);

                if (_enableDebugLogs)
                    Debug.Log($"[{ServiceName}] Visibility updated: {_currentVisibilityFactor:F2}");
            }
        }

        public void UpdatePlayerNoiseLevel(float noiseLevel)
        {
            float previousLevel = _currentNoiseLevel;
            _currentNoiseLevel = Mathf.Clamp01(noiseLevel);

            // イベント発行（変化があった場合のみ）
            if (Mathf.Abs(_currentNoiseLevel - previousLevel) > 0.01f)
            {
                _playerNoiseEvent?.Raise(_currentNoiseLevel);

                if (_enableDebugLogs)
                    Debug.Log($"[{ServiceName}] Noise level updated: {_currentNoiseLevel:F2}");
            }
        }

        private void UpdateVisibilityStatistics()
        {
            // 移動平均でVisibilityFactorを更新
            _statistics.AverageVisibilityFactor =
                (_statistics.AverageVisibilityFactor + _currentVisibilityFactor) / 2.0f;
        }
        #endregion

        #region Concealment System
        public bool IsPlayerConcealed => _currentConcealmentZone != null && _currentConcealmentZone.IsActive;
        public IConcealmentZone CurrentConcealmentZone => _currentConcealmentZone;

        public void EnterConcealmentZone(IConcealmentZone concealmentZone)
        {
            if (concealmentZone == null) return;

            _activeConcealmentZones.Add(concealmentZone);

            // 最も強力な隠蔽ゾーンをアクティブにする
            UpdateActiveConcealmentZone();

            _statistics.ConcealmentZonesUsed++;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Entered concealment zone: {concealmentZone.ZoneType}");
        }

        public void ExitConcealmentZone(IConcealmentZone concealmentZone)
        {
            if (concealmentZone == null) return;

            _activeConcealmentZones.Remove(concealmentZone);

            // アクティブゾーンの更新
            UpdateActiveConcealmentZone();

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Exited concealment zone: {concealmentZone.ZoneType}");
        }

        private void UpdateActiveConcealmentZone()
        {
            IConcealmentZone bestZone = null;
            float bestStrength = 0.0f;

            foreach (var zone in _activeConcealmentZones)
            {
                if (zone.IsActive && zone.ConcealmentStrength > bestStrength)
                {
                    bestZone = zone;
                    bestStrength = zone.ConcealmentStrength;
                }
            }

            _currentConcealmentZone = bestZone;

            // 視認性の再計算をトリガー
            UpdatePlayerVisibility(_currentVisibilityFactor);
        }
        #endregion

        #region Environmental Interaction
        public bool InteractWithEnvironment(GameObject interactableObject, StealthInteractionType interactionType)
        {
            if (interactableObject == null) return false;

            var interactable = interactableObject.GetComponent<IStealthInteractable>();
            if (interactable == null)
            {
                if (_enableDebugLogs)
                    Debug.LogWarning($"[{ServiceName}] Object {interactableObject.name} is not stealth interactable");
                return false;
            }

            bool success = interactable.Interact(interactionType);

            if (success)
            {
                _statistics.EnvironmentalInteractions++;

                if (_enableDebugLogs)
                    Debug.Log($"[{ServiceName}] Environmental interaction successful: {interactionType}");
            }

            return success;
        }

        public void CreateDistraction(Vector3 position, float noiseLevel)
        {
            // 陽動音の発生
            var distractionData = new StealthDetectionData
            {
                Position = position,
                NoiseLevel = Mathf.Clamp01(noiseLevel),
                DetectionType = StealthDetectionType.Distraction,
                Timestamp = Time.time
            };

            _stealthDetectionEvent?.Raise(distractionData);
            _statistics.DistrictionsCreated++;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Distraction created at {position} with noise level {noiseLevel:F2}");
        }
        #endregion

        #region Detection Integration
        public void OnAISuspicionChanged(GameObject detector, float suspicionLevel)
        {
            var detectionData = new StealthDetectionData
            {
                Detector = detector,
                SuspicionLevel = Mathf.Clamp01(suspicionLevel),
                DetectionType = StealthDetectionType.SuspicionChange,
                Timestamp = Time.time
            };

            _stealthDetectionEvent?.Raise(detectionData);

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] AI Suspicion changed: {detector.name} - {suspicionLevel:F2}");
        }

        public void OnPlayerSpotted(GameObject detector)
        {
            var detectionData = new StealthDetectionData
            {
                Detector = detector,
                DetectionType = StealthDetectionType.Spotted,
                Timestamp = Time.time
            };

            _stealthDetectionEvent?.Raise(detectionData);
            _statistics.TimesSpotted++;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Player spotted by: {detector.name}");
        }

        public void OnPlayerLost(GameObject detector)
        {
            var detectionData = new StealthDetectionData
            {
                Detector = detector,
                DetectionType = StealthDetectionType.Lost,
                Timestamp = Time.time
            };

            _stealthDetectionEvent?.Raise(detectionData);

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Player lost by: {detector.name}");
        }
        #endregion

        #region State Management
        public bool IsStealthModeActive => _isStealthModeActive;

        public void SetStealthMode(bool enabled)
        {
            if (_isStealthModeActive == enabled) return;

            _isStealthModeActive = enabled;

            if (_isStealthModeActive)
            {
                OnStealthModeActivated();
            }
            else
            {
                OnStealthModeDeactivated();
            }

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Stealth mode: {(_isStealthModeActive ? "Activated" : "Deactivated")}");
        }

        private void OnStealthModeActivated()
        {
            // ステルスモード開始時の処理
            _statistics.TotalStealthTime = Time.time;
        }

        private void OnStealthModeDeactivated()
        {
            // ステルスモード終了時の処理
            if (_statistics.TotalStealthTime > 0)
            {
                _statistics.TotalStealthTime = Time.time - _statistics.TotalStealthTime;
            }
        }

        public StealthStatistics GetStealthStatistics()
        {
            if (_isStealthModeActive && _statistics.TotalStealthTime > 0)
            {
                // アクティブ中は現在時間での統計を返す
                var currentStats = _statistics;
                currentStats.TotalStealthTime = Time.time - _statistics.TotalStealthTime;
                return currentStats;
            }

            return _statistics;
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (!IsInitialized || !_isStealthModeActive) return;

            // プレイヤー位置での光量を定期的に更新
            if (_playerTransform != null)
            {
                float lightLevel = CalculateLightLevel(_playerTransform.position);
                UpdatePlayerVisibility(lightLevel);
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_enableDebugLogs || _playerTransform == null) return;

            // デバッグ用ギズモの描画
            Gizmos.color = Color.Lerp(Color.green, Color.red, _currentVisibilityFactor);
            Gizmos.DrawWireSphere(_playerTransform.position, 1.0f);

            if (_currentConcealmentZone != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(_playerTransform.position, Vector3.one * 2.0f);
            }
        }
        #endregion
    }

    /// <summary>
    /// 環境オブジェクトとの相互作用インターフェース
    /// </summary>
    public interface IStealthInteractable
    {
        /// <summary>
        /// 相互作用を実行
        /// </summary>
        /// <param name="interactionType">相互作用の種類</param>
        /// <returns>成功したかどうか</returns>
        bool Interact(StealthInteractionType interactionType);

        /// <summary>
        /// この相互作用が現在可能かどうか
        /// </summary>
        /// <param name="interactionType">相互作用の種類</param>
        /// <returns>可能かどうか</returns>
        bool CanInteract(StealthInteractionType interactionType);
    }
}