using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// 隠蔽システム中央管理
    /// ServiceLocator統合による隠蔽ゾーンの一元管理
    /// プレイヤーの隠蔽状態と効果の動的計算・適用
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

        // 隠蔽ゾーン管理
        private readonly HashSet<IConcealmentZone> _activeZones = new();
        private readonly Dictionary<ConcealmentType, List<IConcealmentZone>> _zonesByType = new();

        // プレイヤー状態
        private Transform _playerTransform;
        private IStealthService _stealthService;

        // 現在の隠蔽状態
        private IConcealmentZone _currentZone;
        private ConcealmentEffect _currentEffect;
        private ConcealmentEffect _targetEffect;
        private float _transitionProgress;

        // 更新制御
        private float _lastUpdateTime;
        private bool _isInConcealmentTransition;

        // 統計情報
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
            // ServiceLocator経由でStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();

            if (_stealthService == null)
            {
                Debug.LogWarning("[ConcealmentManager] StealthService not found in ServiceLocator");
            }

            // プレイヤー参照を取得
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("[ConcealmentManager] Player GameObject not found");
            }

            // 初期隠蔽効果を設定
            _currentEffect = ConcealmentEffect.Default;
            _targetEffect = ConcealmentEffect.Default;

            if (_enableDebugLogs)
                Debug.Log("[ConcealmentManager] Concealment Manager initialized");
        }

        private void RegisterWithServiceLocator()
        {
            // ServiceLocatorに自分自身を登録
            ServiceLocator.RegisterService<ConcealmentManager>(this);

            if (_enableDebugLogs)
                Debug.Log("[ConcealmentManager] Registered with ServiceLocator");
        }

        private void FindExistingConcealmentZones()
        {
            // シーン内の既存隠蔽ゾーンを検索・登録
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
        /// 隠蔽ゾーンの登録
        /// </summary>
        public void RegisterConcealmentZone(IConcealmentZone zone)
        {
            if (zone == null) return;

            _activeZones.Add(zone);

            // タイプ別辞書に追加
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
        /// 隠蔽ゾーンの登録解除
        /// </summary>
        public void UnregisterConcealmentZone(IConcealmentZone zone)
        {
            if (zone == null) return;

            _activeZones.Remove(zone);

            if (_zonesByType.ContainsKey(zone.ZoneType))
            {
                _zonesByType[zone.ZoneType].Remove(zone);

                // リストが空になったら削除
                if (_zonesByType[zone.ZoneType].Count == 0)
                {
                    _zonesByType.Remove(zone.ZoneType);
                }
            }

            // 現在のゾーンが解除された場合
            if (_currentZone == zone)
            {
                ExitConcealment();
            }

            if (_enableDebugLogs)
                Debug.Log($"[ConcealmentManager] Unregistered concealment zone: {zone.ZoneType}");
        }

        /// <summary>
        /// 指定タイプの隠蔽ゾーンを取得
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

                // ゾーンとの距離・重複チェック
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
            // EnvironmentConcealmentZoneの場合は、Colliderベースの判定
            if (zone is EnvironmentConcealmentZone envZone)
            {
                var collider = envZone.GetComponent<Collider>();
                if (collider != null)
                {
                    return collider.bounds.Contains(_playerTransform.position);
                }
            }

            // フォールバック: 距離ベースの判定
            if (zone is MonoBehaviour zoneBehaviour)
            {
                float distance = Vector3.Distance(_playerTransform.position, zoneBehaviour.transform.position);
                return distance <= 2.0f; // デフォルト半径
            }

            return false;
        }

        private void EnterConcealment(IConcealmentZone zone)
        {
            _currentZone = zone;
            _targetEffect = ConcealmentUtility.GetDefaultEffectForType(zone.ZoneType);

            // 隠蔽強度を適用
            _targetEffect.VisibilityReduction *= zone.ConcealmentStrength;
            _targetEffect.NoiseDampening *= zone.ConcealmentStrength;

            StartConcealmentTransition();

            // イベント発行
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

            // イベント発行
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

            // 隠蔽効果を補間
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

                // Boolean値は50%を境に切り替え
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

            // StealthServiceに隠蔽効果を適用
            float baseVisibility = _stealthService.PlayerVisibilityFactor;
            float modifiedVisibility = baseVisibility * (1f - effect.VisibilityReduction);
            _stealthService.UpdatePlayerVisibility(modifiedVisibility);

            float baseNoise = _stealthService.PlayerNoiseLevel;
            float modifiedNoise = baseNoise * (1f - effect.NoiseDampening);
            _stealthService.UpdatePlayerNoiseLevel(modifiedNoise);

            // 移動速度の調整（PlayerStateMachineとの連動が必要な場合）
            // TODO: PlayerStateMachineとの統合実装
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 現在の隠蔽状態を取得
        /// </summary>
        public bool IsPlayerConcealed => _currentZone != null;

        /// <summary>
        /// 現在の隠蔽強度を取得
        /// </summary>
        public float CurrentConcealmentStrength => _currentZone?.ConcealmentStrength ?? 0f;

        /// <summary>
        /// 現在の隠蔽タイプを取得
        /// </summary>
        public ConcealmentType? CurrentConcealmentType => _currentZone?.ZoneType;

        /// <summary>
        /// 現在の隠蔽効果を取得
        /// </summary>
        public ConcealmentEffect CurrentConcealmentEffect => _currentEffect;

        /// <summary>
        /// 指定位置での隠蔽評価
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
        /// 隠蔽統計情報を取得
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
                    // 隠蔽ゾーンの可視化
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

            // プレイヤーの隠蔽状態可視化
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
    /// 隠蔽システム統計情報
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
