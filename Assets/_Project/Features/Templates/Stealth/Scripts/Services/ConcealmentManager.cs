using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// 髫阡ｽ繧ｷ繧ｹ繝・Β荳ｭ螟ｮ邂｡逅・
    /// ServiceLocator邨ｱ蜷医↓繧医ｋ髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ荳蜈・ｮ｡逅・
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ髫阡ｽ迥ｶ諷九→蜉ｹ譫懊・蜍慕噪險育ｮ励・驕ｩ逕ｨ
    /// </summary>
    public class ConcealmentManager : MonoBehaviour
    {
        [Header("Concealment Configuration")]
        [SerializeField] private float _concealmentUpdateInterval = 0.1f;
        [SerializeField] private float _transitionBlendTime = 0.5f;
        [SerializeField] private LayerMask _concealmentLayers = -1;

        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _showConcealmentGizmos = true;

        // 髫阡ｽ繧ｾ繝ｼ繝ｳ邂｡逅・
        private readonly HashSet<IConcealmentZone> _activeZones = new();
        private readonly Dictionary<ConcealmentType, List<IConcealmentZone>> _zonesByType = new();

        // 繝励Ξ繧､繝､繝ｼ迥ｶ諷・
        private Transform _playerTransform;
        private IStealthService _stealthService;

        // 迴ｾ蝨ｨ縺ｮ髫阡ｽ迥ｶ諷・
        private IConcealmentZone _currentZone;
        private ConcealmentEffect _currentEffect;
        private ConcealmentEffect _targetEffect;
        private float _transitionProgress;

        // 譖ｴ譁ｰ蛻ｶ蠕｡
        private float _lastUpdateTime;
        private bool _isInConcealmentTransition;

        // 邨ｱ險域ュ蝣ｱ
        private int _totalZonesRegistered;
        private int _concealmentEnterEvents;
        private int _concealmentExitEvents;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeService();
        }

        private void Start()
        {
            RegisterWithServiceLocator();
            FindExistingConcealmentZones();
        }

        private void Update()
        {
            if (ShouldUpdateConcealment())
            {
                UpdateConcealmentState();
                _lastUpdateTime = Time.time;
            }

            if (_isInConcealmentTransition)
            {
                UpdateConcealmentTransition();
            }
        }

        #endregion

        #region Initialization

        private void InitializeService()
        {
            // ServiceLocator邨檎罰縺ｧStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();

            if (_stealthService == null)
            {
                Debug.LogWarning("[ConcealmentManager] StealthService not found in ServiceLocator");
            }

            // 繝励Ξ繧､繝､繝ｼ蜿ら・繧貞叙蠕・
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("[ConcealmentManager] Player GameObject not found");
            }

            // 蛻晄悄髫阡ｽ蜉ｹ譫懊ｒ險ｭ螳・
            _currentEffect = ConcealmentEffect.Default;
            _targetEffect = ConcealmentEffect.Default;

            if (_enableDebugLogs)
                Debug.Log("[ConcealmentManager] Concealment Manager initialized");
        }

        private void RegisterWithServiceLocator()
        {
            // ServiceLocator縺ｫ閾ｪ蛻・・霄ｫ繧堤匳骭ｲ
            ServiceLocator.RegisterService<ConcealmentManager>(this);

            if (_enableDebugLogs)
                Debug.Log("[ConcealmentManager] Registered with ServiceLocator");
        }

        private void FindExistingConcealmentZones()
        {
            // 繧ｷ繝ｼ繝ｳ蜀・・譌｢蟄倬國阡ｽ繧ｾ繝ｼ繝ｳ繧呈､懃ｴ｢繝ｻ逋ｻ骭ｲ
            var existingZones = FindObjectsOfType<EnvironmentConcealmentZone>();

            foreach (var zone in existingZones)
            {
                RegisterConcealmentZone(zone);
            }

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Found and registered {existingZones.Length} existing concealment zones");
        }

        #endregion

        #region Concealment Zone Management

        /// <summary>
        /// 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ逋ｻ骭ｲ
        /// </summary>
        public void RegisterConcealmentZone(IConcealmentZone zone)
        {
            if (zone == null) return;

            _activeZones.Add(zone);

            // 繧ｿ繧､繝怜挨霎樊嶌縺ｫ霑ｽ蜉
            if (!_zonesByType.ContainsKey(zone.ZoneType))
            {
                _zonesByType[zone.ZoneType] = new List<IConcealmentZone>();
            }
            _zonesByType[zone.ZoneType].Add(zone);

            _totalZonesRegistered++;

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Registered concealment zone: {zone.ZoneType}");
        }

        /// <summary>
        /// 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ逋ｻ骭ｲ隗｣髯､
        /// </summary>
        public void UnregisterConcealmentZone(IConcealmentZone zone)
        {
            if (zone == null) return;

            _activeZones.Remove(zone);

            if (_zonesByType.ContainsKey(zone.ZoneType))
            {
                _zonesByType[zone.ZoneType].Remove(zone);

                // 繝ｪ繧ｹ繝医′遨ｺ縺ｫ縺ｪ縺｣縺溘ｉ蜑企勁
                if (_zonesByType[zone.ZoneType].Count == 0)
                {
                    _zonesByType.Remove(zone.ZoneType);
                }
            }

            // 迴ｾ蝨ｨ縺ｮ繧ｾ繝ｼ繝ｳ縺瑚ｧ｣髯､縺輔ｌ縺溷ｴ蜷・
            if (_currentZone == zone)
            {
                ExitConcealment();
            }

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Unregistered concealment zone: {zone.ZoneType}");
        }

        /// <summary>
        /// 謖・ｮ壹ち繧､繝励・髫阡ｽ繧ｾ繝ｼ繝ｳ繧貞叙蠕・
        /// </summary>
        public List<IConcealmentZone> GetZonesByType(ConcealmentType type)
        {
            return _zonesByType.TryGetValue(type, out var zones) ? zones : new List<IConcealmentZone>();
        }

        #endregion

        #region Concealment State Management

        private bool ShouldUpdateConcealment()
        {
            return _playerTransform != null &&
                   Time.time - _lastUpdateTime >= _concealmentUpdateInterval;
        }

        private void UpdateConcealmentState()
        {
            var bestZone = FindBestConcealmentZone();

            if (bestZone != _currentZone)
            {
                if (bestZone != null)
                {
                    EnterConcealment(bestZone);
                }
                else
                {
                    ExitConcealment();
                }
            }
        }

        private IConcealmentZone FindBestConcealmentZone()
        {
            if (_playerTransform == null) return null;

            IConcealmentZone bestZone = null;
            float bestConcealmentStrength = 0f;

            foreach (var zone in _activeZones)
            {
                if (!zone.IsActive) continue;

                // 繧ｾ繝ｼ繝ｳ縺ｨ縺ｮ霍晞屬繝ｻ驥崎､・メ繧ｧ繝・け
                if (IsPlayerInZone(zone))
                {
                    if (zone.ConcealmentStrength > bestConcealmentStrength)
                    {
                        bestZone = zone;
                        bestConcealmentStrength = zone.ConcealmentStrength;
                    }
                }
            }

            return bestZone;
        }

        private bool IsPlayerInZone(IConcealmentZone zone)
        {
            // EnvironmentConcealmentZone縺ｮ蝣ｴ蜷医・縲，ollider繝吶・繧ｹ縺ｮ蛻､螳・
            if (zone is EnvironmentConcealmentZone envZone)
            {
                var collider = envZone.GetComponent<Collider>();
                if (collider != null)
                {
                    return collider.bounds.Contains(_playerTransform.position);
                }
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 霍晞屬繝吶・繧ｹ縺ｮ蛻､螳・
            if (zone is MonoBehaviour zoneBehaviour)
            {
                float distance = Vector3.Distance(_playerTransform.position, zoneBehaviour.transform.position);
                return distance <= 2.0f; // 繝・ヵ繧ｩ繝ｫ繝亥濠蠕・
            }

            return false;
        }

        private void EnterConcealment(IConcealmentZone zone)
        {
            _currentZone = zone;
            _targetEffect = ConcealmentUtility.GetDefaultEffectForType(zone.ZoneType);

            // 髫阡ｽ蠑ｷ蠎ｦ繧帝←逕ｨ
            _targetEffect.VisibilityReduction *= zone.ConcealmentStrength;
            _targetEffect.NoiseDampening *= zone.ConcealmentStrength;

            StartConcealmentTransition();

            // 繧､繝吶Φ繝育匱陦・
            var eventData = new StealthConcealmentEventData
            {
                ConcealmentType = zone.ZoneType,
                ConcealmentStrength = zone.ConcealmentStrength,
                Position = _playerTransform.position,
                IsEntering = true
            };

            StealthEvents.OnConcealmentChanged?.Raise(eventData);
            _concealmentEnterEvents++;

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Entered concealment zone: {zone.ZoneType}, Strength: {zone.ConcealmentStrength:F2}");
        }

        private void ExitConcealment()
        {
            if (_currentZone == null) return;

            var previousZone = _currentZone;
            _currentZone = null;
            _targetEffect = ConcealmentEffect.Default;

            StartConcealmentTransition();

            // 繧､繝吶Φ繝育匱陦・
            var eventData = new StealthConcealmentEventData
            {
                ConcealmentType = previousZone.ZoneType,
                ConcealmentStrength = 0f,
                Position = _playerTransform.position,
                IsEntering = false
            };

            StealthEvents.OnConcealmentChanged?.Raise(eventData);
            _concealmentExitEvents++;

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Exited concealment zone: {previousZone.ZoneType}");
        }

        #endregion

        #region Concealment Transition

        private void StartConcealmentTransition()
        {
            _isInConcealmentTransition = true;
            _transitionProgress = 0f;
        }

        private void UpdateConcealmentTransition()
        {
            _transitionProgress += Time.deltaTime / _transitionBlendTime;

            if (_transitionProgress >= 1f)
            {
                _transitionProgress = 1f;
                _isInConcealmentTransition = false;
            }

            // 髫阡ｽ蜉ｹ譫懊ｒ陬憺俣
            var blendedEffect = BlendConcealmentEffects(_currentEffect, _targetEffect, _transitionProgress);
            ApplyConcealmentEffect(blendedEffect);

            _currentEffect = blendedEffect;
        }

        private ConcealmentEffect BlendConcealmentEffects(ConcealmentEffect from, ConcealmentEffect to, float t)
        {
            return new ConcealmentEffect
            {
                VisibilityReduction = Mathf.Lerp(from.VisibilityReduction, to.VisibilityReduction, t),
                NoiseDampening = Mathf.Lerp(from.NoiseDampening, to.NoiseDampening, t),
                MovementSpeedMultiplier = Mathf.Lerp(from.MovementSpeedMultiplier, to.MovementSpeedMultiplier, t),

                // Boolean蛟､縺ｯ50%繧貞｢・↓蛻・ｊ譖ｿ縺・
                ImmuneToVisualDetection = t > 0.5f ? to.ImmuneToVisualDetection : from.ImmuneToVisualDetection,
                ImmuneToAudioDetection = t > 0.5f ? to.ImmuneToAudioDetection : from.ImmuneToAudioDetection,
                ImmuneToThermalDetection = t > 0.5f ? to.ImmuneToThermalDetection : from.ImmuneToThermalDetection,
                ImmuneToMotionDetection = t > 0.5f ? to.ImmuneToMotionDetection : from.ImmuneToMotionDetection,

                MaxConcealmentTime = to.MaxConcealmentTime,
                EntryTime = to.EntryTime,
                ExitTime = to.ExitTime
            };
        }

        private void ApplyConcealmentEffect(ConcealmentEffect effect)
        {
            if (_stealthService == null) return;

            // StealthService縺ｫ髫阡ｽ蜉ｹ譫懊ｒ驕ｩ逕ｨ
            float baseVisibility = _stealthService.PlayerVisibilityFactor;
            float modifiedVisibility = baseVisibility * (1f - effect.VisibilityReduction);
            _stealthService.UpdatePlayerVisibility(modifiedVisibility);

            float baseNoise = _stealthService.PlayerNoiseLevel;
            float modifiedNoise = baseNoise * (1f - effect.NoiseDampening);
            _stealthService.UpdatePlayerNoiseLevel(modifiedNoise);

            // 遘ｻ蜍暮溷ｺｦ縺ｮ隱ｿ謨ｴ・・layerStateMachine縺ｨ縺ｮ騾｣蜍輔′蠢・ｦ√↑蝣ｴ蜷茨ｼ・
            // TODO: PlayerStateMachine縺ｨ縺ｮ邨ｱ蜷亥ｮ溯｣・
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ髫阡ｽ迥ｶ諷九ｒ蜿門ｾ・
        /// </summary>
        public bool IsPlayerConcealed => _currentZone != null;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ髫阡ｽ蠑ｷ蠎ｦ繧貞叙蠕・
        /// </summary>
        public float CurrentConcealmentStrength => _currentZone?.ConcealmentStrength ?? 0f;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ髫阡ｽ繧ｿ繧､繝励ｒ蜿門ｾ・
        /// </summary>
        public ConcealmentType? CurrentConcealmentType => _currentZone?.ZoneType;

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ髫阡ｽ蜉ｹ譫懊ｒ蜿門ｾ・
        /// </summary>
        public ConcealmentEffect CurrentConcealmentEffect => _currentEffect;

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｧ縺ｮ髫阡ｽ隧穂ｾ｡
        /// </summary>
        public ConcealmentQuality EvaluateConcealmentAtPosition(Vector3 position)
        {
            var tempPlayerPos = _playerTransform.position;
            _playerTransform.position = position;

            var zone = FindBestConcealmentZone();
            var quality = zone != null ?
                ConcealmentUtility.GetQualityFromStrength(zone.ConcealmentStrength) :
                ConcealmentQuality.Poor;

            _playerTransform.position = tempPlayerPos;
            return quality;
        }

        /// <summary>
        /// 髫阡ｽ邨ｱ險域ュ蝣ｱ繧貞叙蠕・
        /// </summary>
        public ConcealmentStatistics GetStatistics()
        {
            return new ConcealmentStatistics
            {
                TotalZonesRegistered = _totalZonesRegistered,
                ActiveZones = _activeZones.Count,
                ConcealmentEnterEvents = _concealmentEnterEvents,
                ConcealmentExitEvents = _concealmentExitEvents,
                CurrentConcealmentStrength = CurrentConcealmentStrength,
                IsCurrentlyConcealed = IsPlayerConcealed
            };
        }

        #endregion

        #region Debug & Visualization

        private void OnDrawGizmos()
        {
            if (!_showConcealmentGizmos || _activeZones == null) return;

            foreach (var zone in _activeZones)
            {
                if (zone is MonoBehaviour zoneBehaviour)
                {
                    // 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ蜿ｯ隕門喧
                    Gizmos.color = zone == _currentZone ? Color.green : Color.yellow;
                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);

                    var collider = zoneBehaviour.GetComponent<Collider>();
                    if (collider != null)
                    {
                        Gizmos.DrawCube(collider.bounds.center, collider.bounds.size);
                    }
                    else
                    {
                        Gizmos.DrawSphere(zoneBehaviour.transform.position, 2f);
                    }
                }
            }

            // 繝励Ξ繧､繝､繝ｼ縺ｮ髫阡ｽ迥ｶ諷句庄隕門喧
            if (_playerTransform != null && IsPlayerConcealed)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_playerTransform.position, 1f);
            }
        }

        private void OnGUI()
        {
            if (!_enableDebugLogs || !Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(10, 400, 350, 200));
            GUILayout.Label("=== Concealment Manager Debug ===");
            GUILayout.Label($"Active Zones: {_activeZones.Count}");
            GUILayout.Label($"Is Concealed: {IsPlayerConcealed}");
            GUILayout.Label($"Concealment Strength: {CurrentConcealmentStrength:F2}");
            GUILayout.Label($"Current Type: {CurrentConcealmentType}");
            GUILayout.Label($"Visibility Reduction: {_currentEffect.VisibilityReduction:F2}");
            GUILayout.Label($"Noise Dampening: {_currentEffect.NoiseDampening:F2}");
            GUILayout.Label($"Enter Events: {_concealmentEnterEvents}");
            GUILayout.Label($"Exit Events: {_concealmentExitEvents}");
            GUILayout.EndArea();
        }

        #endregion
    }

    /// <summary>
    /// 髫阡ｽ繧ｷ繧ｹ繝・Β邨ｱ險域ュ蝣ｱ
    /// </summary>
    [System.Serializable]
    public struct ConcealmentStatistics
    {
        public int TotalZonesRegistered;
        public int ActiveZones;
        public int ConcealmentEnterEvents;
        public int ConcealmentExitEvents;
        public float CurrentConcealmentStrength;
        public bool IsCurrentlyConcealed;
    }
}


