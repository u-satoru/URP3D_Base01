using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
// using asterivo.Unity60.Stealth.Detection; // Fixed: namespace doesn't exist

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// 繧ｹ繝・Ν繧ｹ繧ｷ繧ｹ繝・Β縺ｮ荳ｭ螟ｮ蛻ｶ蠕｡繧ｵ繝ｼ繝薙せ螳溯｣・
    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｫ貅匁侠縺励∵里蟄倥・DetectionSystem縺ｨ邨ｱ蜷・
    /// </summary>
    public class StealthService : MonoBehaviour, IStealthService
    {
        [Header("Stealth Configuration")]
        // [SerializeField] private DetectionConfiguration _detectionConfig; // TODO: Type needs to be created
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private bool _enableDebugLogs = true;

        [Header("Light and Shadow System")]
        [SerializeField] private LayerMask _lightSourceLayers = -1;
        [SerializeField] private LayerMask _obstructionLayers = -1;
        [SerializeField] private float _ambientLightLevel = 0.1f;

        [Header("Audio Integration")]
        [SerializeField] private string _stealthAudioChannelName = "StealthAudio";

        // Core system components
        // private VisibilityCalculator _visibilityCalculator; // TODO: Type needs to be created
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
            // VisibilityCalculator縺ｮ蛻晄悄蛹・
            // TODO: VisibilityCalculator type needs to be created
            // _visibilityCalculator = GetComponent<VisibilityCalculator>();
            // if (_visibilityCalculator == null)
            // {
            //     _visibilityCalculator = gameObject.AddComponent<VisibilityCalculator>();
            // }

            // 繝励Ξ繧､繝､繝ｼ繝医Λ繝ｳ繧ｹ繝輔か繝ｼ繝縺ｮ閾ｪ蜍墓､懷・
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }

            // DetectionConfiguration縺ｮ閾ｪ蜍募叙蠕・
            // TODO: DetectionConfiguration type needs to be created
            // if (_detectionConfig == null)
            // {
            //     _detectionConfig = Resources.Load<DetectionConfiguration>("StealthDetectionConfig");
            // }

            // 邨ｱ險域ュ蝣ｱ縺ｮ蛻晄悄蛹・
            _statistics = new StealthStatistics
            {
                AverageVisibilityFactor = 1.0f,
                TotalStealthTime = 0.0f
            };
        }

        private void InitializeEvents()
        {
            // GameEvent縺ｮ蛻晄悄蛹・
            _playerVisibilityChangedEvent = ScriptableObject.CreateInstance<GameEvent<float>>();
            _playerVisibilityChangedEvent.name = "PlayerVisibilityChangedEvent";

            _playerNoiseEvent = ScriptableObject.CreateInstance<GameEvent<float>>();
            _playerNoiseEvent.name = "PlayerNoiseEvent";

            _stealthDetectionEvent = ScriptableObject.CreateInstance<GameEvent<StealthDetectionData>>();
            _stealthDetectionEvent.name = "StealthDetectionEvent";
        }

        private void SubscribeToEvents()
        {
            // AI讀懷・繧ｷ繧ｹ繝・Β縺九ｉ縺ｮ繧､繝吶Φ繝医ｒ雉ｼ隱ｭ
            // 譌｢蟄倥・繧､繝吶Φ繝医す繧ｹ繝・Β縺ｨ邨ｱ蜷・
        }

        private void UnsubscribeFromEvents()
        {
            // 繧､繝吶Φ繝郁ｳｼ隱ｭ縺ｮ隗｣髯､
        }
        #endregion

        #region Visibility Management
        public float PlayerVisibilityFactor => _currentVisibilityFactor;
        public float PlayerNoiseLevel => _currentNoiseLevel;

        public float CalculateLightLevel(Vector3 position)
        {
            // if (_visibilityCalculator == null) return _ambientLightLevel; // TODO: VisibilityCalculator needs to be created
            return _ambientLightLevel; // Temporary direct return

            // 譌｢蟄倥・VisibilityCalculator繧呈ｴｻ逕ｨ縺励◆蜈蛾㍼險育ｮ・
            var lightSources = FindObjectsOfType<Light>();
            float totalLight = _ambientLightLevel;

            foreach (var light in lightSources)
            {
                if (light.enabled && ((1 << light.gameObject.layer) & _lightSourceLayers) != 0)
                {
                    float distance = Vector3.Distance(position, light.transform.position);

                    // 蜈画ｺ舌・遞ｮ鬘槭↓蠢懊§縺溯ｨ育ｮ・
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

            // 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ蜉ｹ譫懊ｒ驕ｩ逕ｨ
            if (_currentConcealmentZone != null && _currentConcealmentZone.IsActive)
            {
                _currentVisibilityFactor *= (1.0f - _currentConcealmentZone.ConcealmentStrength);
            }

            // 邨ｱ險域ュ蝣ｱ縺ｮ譖ｴ譁ｰ
            UpdateVisibilityStatistics();

            // 繧､繝吶Φ繝育匱陦鯉ｼ亥､牙喧縺後≠縺｣縺溷ｴ蜷医・縺ｿ・・
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

            // 繧､繝吶Φ繝育匱陦鯉ｼ亥､牙喧縺後≠縺｣縺溷ｴ蜷医・縺ｿ・・
            if (Mathf.Abs(_currentNoiseLevel - previousLevel) > 0.01f)
            {
                _playerNoiseEvent?.Raise(_currentNoiseLevel);

                if (_enableDebugLogs)
                    Debug.Log($"[{ServiceName}] Noise level updated: {_currentNoiseLevel:F2}");
            }
        }

        private void UpdateVisibilityStatistics()
        {
            // 遘ｻ蜍募ｹｳ蝮・〒VisibilityFactor繧呈峩譁ｰ
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

            // 譛繧ょｼｷ蜉帙↑髫阡ｽ繧ｾ繝ｼ繝ｳ繧偵い繧ｯ繝・ぅ繝悶↓縺吶ｋ
            UpdateActiveConcealmentZone();

            _statistics.ConcealmentZonesUsed++;

            if (_enableDebugLogs)
                Debug.Log($"[{ServiceName}] Entered concealment zone: {concealmentZone.ZoneType}");
        }

        public void ExitConcealmentZone(IConcealmentZone concealmentZone)
        {
            if (concealmentZone == null) return;

            _activeConcealmentZones.Remove(concealmentZone);

            // 繧｢繧ｯ繝・ぅ繝悶だ繝ｼ繝ｳ縺ｮ譖ｴ譁ｰ
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

            // 隕冶ｪ肴ｧ縺ｮ蜀崎ｨ育ｮ励ｒ繝医Μ繧ｬ繝ｼ
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
            // 髯ｽ蜍暮浹縺ｮ逋ｺ逕・
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
            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝蛾幕蟋区凾縺ｮ蜃ｦ逅・
            _statistics.TotalStealthTime = Time.time;
        }

        private void OnStealthModeDeactivated()
        {
            // 繧ｹ繝・Ν繧ｹ繝｢繝ｼ繝臥ｵゆｺ・凾縺ｮ蜃ｦ逅・
            if (_statistics.TotalStealthTime > 0)
            {
                _statistics.TotalStealthTime = Time.time - _statistics.TotalStealthTime;
            }
        }

        public StealthStatistics GetStealthStatistics()
        {
            if (_isStealthModeActive && _statistics.TotalStealthTime > 0)
            {
                // 繧｢繧ｯ繝・ぅ繝紋ｸｭ縺ｯ迴ｾ蝨ｨ譎る俣縺ｧ縺ｮ邨ｱ險医ｒ霑斐☆
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

            // 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ縺ｧ縺ｮ蜈蛾㍼繧貞ｮ壽悄逧・↓譖ｴ譁ｰ
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

            // 繝・ヰ繝・げ逕ｨ繧ｮ繧ｺ繝｢縺ｮ謠冗判
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
    /// 迺ｰ蠅・が繝悶ず繧ｧ繧ｯ繝医→縺ｮ逶ｸ莠剃ｽ懃畑繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IStealthInteractable
    {
        /// <summary>
        /// 逶ｸ莠剃ｽ懃畑繧貞ｮ溯｡・
        /// </summary>
        /// <param name="interactionType">逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘・/param>
        /// <returns>謌仙粥縺励◆縺九←縺・°</returns>
        bool Interact(StealthInteractionType interactionType);

        /// <summary>
        /// 縺薙・逶ｸ莠剃ｽ懃畑縺檎樟蝨ｨ蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        /// <param name="interactionType">逶ｸ莠剃ｽ懃畑縺ｮ遞ｮ鬘・/param>
        /// <returns>蜿ｯ閭ｽ縺九←縺・°</returns>
        bool CanInteract(StealthInteractionType interactionType);
    }
}


