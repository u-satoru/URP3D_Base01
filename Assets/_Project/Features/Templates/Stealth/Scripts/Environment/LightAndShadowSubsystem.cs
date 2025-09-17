using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// 光と影のサブシステム
    /// プレイヤーの位置と周囲の光源を基に視認性係数を算出
    /// 詳細設計書のLightAndShadowSubsystem実装
    /// </summary>
    public class LightAndShadowSubsystem : MonoBehaviour
    {
        [Header("Light Detection Configuration")]
        [SerializeField]
        private LayerMask _lightSourceLayers = -1;

        [SerializeField]
        private LayerMask _shadowCasterLayers = -1;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _ambientLightLevel = 0.1f;

        [SerializeField, Range(0.0f, 1.0f)]
        private float _maxLightLevel = 1.0f;

        [Header("Shadow Detection")]
        [SerializeField]
        private bool _enableShadowDetection = true;

        [SerializeField]
        private float _shadowRaycastDistance = 50.0f;

        [SerializeField]
        private int _shadowRaycastSamples = 8; // 複数方向からの光線チェック

        [Header("Performance")]
        [SerializeField]
        private float _updateInterval = 0.1f; // 10Hz更新

        [SerializeField]
        private int _maxLightSources = 20; // パフォーマンス制限

        [SerializeField]
        private float _lightInfluenceThreshold = 0.01f; // 影響が小さい光源を無視

        [Header("Debug")]
        [SerializeField]
        private bool _enableDebugLogs = false;

        [SerializeField]
        private bool _showDebugRays = false;

        [SerializeField]
        private bool _showLightInfluence = false;

        // Private fields
        private IStealthService _stealthService;
        private Transform _playerTransform;
        private List<Light> _activeLightSources = new List<Light>();
        private List<ReflectionProbe> _reflectionProbes = new List<ReflectionProbe>();

        private float _lastUpdateTime = 0.0f;
        private float _currentLightLevel = 0.0f;
        private Vector3 _lastPlayerPosition;

        // Cached components for performance
        private readonly Dictionary<Light, LightData> _lightDataCache = new Dictionary<Light, LightData>();

        // Structs for cached data
        [System.Serializable]
        private struct LightData
        {
            public Vector3 Position;
            public float Range;
            public float Intensity;
            public Color Color;
            public LightType Type;
            public float SpotAngle;
            public Vector3 Direction;
            public bool IsStatic;
            public float LastUpdateTime;
        }

        #region Unity Lifecycle
        private void Start()
        {
            InitializeSubsystem();
        }

        private void Update()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                UpdateLightCalculation();
                _lastUpdateTime = Time.time;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_showDebugRays || _showLightInfluence)
            {
                DrawDebugGizmos();
            }
        }
        #endregion

        #region Initialization
        private void InitializeSubsystem()
        {
            // ServiceLocatorからStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning($"[{gameObject.name}] StealthService not found in ServiceLocator");
            }

            // プレイヤーの取得
            FindPlayerTransform();

            // 光源の初期検索
            RefreshLightSources();

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] LightAndShadowSubsystem initialized");
        }

        private void FindPlayerTransform()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Player object not found");
            }
        }

        private void RefreshLightSources()
        {
            _activeLightSources.Clear();
            _lightDataCache.Clear();

            // シーン内の全光源を取得
            Light[] allLights = FindObjectsOfType<Light>();

            foreach (var light in allLights)
            {
                if (light.enabled && ((1 << light.gameObject.layer) & _lightSourceLayers) != 0)
                {
                    _activeLightSources.Add(light);
                    UpdateLightDataCache(light);
                }
            }

            // ReflectionProbeも取得（間接光の計算用）
            _reflectionProbes.Clear();
            ReflectionProbe[] allProbes = FindObjectsOfType<ReflectionProbe>();
            _reflectionProbes.AddRange(allProbes);

            // パフォーマンス制限の適用
            if (_activeLightSources.Count > _maxLightSources)
            {
                SortLightsByImportance();
                _activeLightSources.RemoveRange(_maxLightSources,
                    _activeLightSources.Count - _maxLightSources);
            }

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Found {_activeLightSources.Count} active light sources");
        }

        private void UpdateLightDataCache(Light light)
        {
            var data = new LightData
            {
                Position = light.transform.position,
                Range = light.range,
                Intensity = light.intensity,
                Color = light.color,
                Type = light.type,
                SpotAngle = light.spotAngle,
                Direction = light.transform.forward,
                IsStatic = !light.transform.hasChanged,
                LastUpdateTime = Time.time
            };

            _lightDataCache[light] = data;
        }

        private void SortLightsByImportance()
        {
            if (_playerTransform == null) return;

            Vector3 playerPos = _playerTransform.position;

            _activeLightSources.Sort((a, b) =>
            {
                float distanceA = Vector3.Distance(playerPos, a.transform.position);
                float distanceB = Vector3.Distance(playerPos, b.transform.position);

                // 距離と強度を考慮した重要度計算
                float importanceA = (a.intensity / Mathf.Max(1.0f, distanceA));
                float importanceB = (b.intensity / Mathf.Max(1.0f, distanceB));

                return importanceB.CompareTo(importanceA);
            });
        }
        #endregion

        #region Light Calculation
        private void UpdateLightCalculation()
        {
            if (_playerTransform == null) return;

            Vector3 currentPosition = _playerTransform.position;

            // 位置が大きく変わった場合は光源リストを更新
            if (Vector3.Distance(currentPosition, _lastPlayerPosition) > 10.0f)
            {
                RefreshLightSources();
                _lastPlayerPosition = currentPosition;
            }

            // 光量の計算
            float newLightLevel = CalculateLightLevelAtPosition(currentPosition);

            // 有意な変化があった場合のみ更新
            if (Mathf.Abs(newLightLevel - _currentLightLevel) > 0.01f)
            {
                _currentLightLevel = newLightLevel;
                NotifyVisibilityChange();
            }
        }

        /// <summary>
        /// 指定位置での光量レベルを計算（0.0 = 完全な闇, 1.0 = 完全な光）
        /// </summary>
        public float CalculateLightLevelAtPosition(Vector3 position)
        {
            float totalLight = _ambientLightLevel;

            foreach (var light in _activeLightSources)
            {
                if (light == null || !light.enabled) continue;

                float lightContribution = CalculateLightContribution(light, position);

                if (lightContribution > _lightInfluenceThreshold)
                {
                    totalLight += lightContribution;
                }
            }

            // 間接光の計算（ReflectionProbeベース）
            if (_reflectionProbes.Count > 0)
            {
                totalLight += CalculateIndirectLight(position);
            }

            return Mathf.Clamp01(totalLight / _maxLightLevel);
        }

        private float CalculateLightContribution(Light light, Vector3 position)
        {
            if (!_lightDataCache.TryGetValue(light, out LightData lightData))
            {
                UpdateLightDataCache(light);
                lightData = _lightDataCache[light];
            }

            Vector3 lightPosition = lightData.Position;
            float distance = Vector3.Distance(position, lightPosition);

            // 範囲外チェック
            if (distance > lightData.Range) return 0.0f;

            float lightContribution = 0.0f;

            switch (lightData.Type)
            {
                case LightType.Point:
                    lightContribution = CalculatePointLightContribution(lightData, position, distance);
                    break;

                case LightType.Spot:
                    lightContribution = CalculateSpotLightContribution(lightData, position, distance);
                    break;

                case LightType.Directional:
                    lightContribution = CalculateDirectionalLightContribution(lightData, position);
                    break;
            }

            // 影の計算
            if (_enableShadowDetection && lightContribution > 0.0f)
            {
                float shadowFactor = CalculateShadowFactor(lightPosition, position);
                lightContribution *= shadowFactor;
            }

            return lightContribution;
        }

        private float CalculatePointLightContribution(LightData lightData, Vector3 position, float distance)
        {
            // 線形減衰 + 二次減衰のブレンド
            float linearFalloff = Mathf.Clamp01(1.0f - (distance / lightData.Range));
            float quadraticFalloff = 1.0f / (1.0f + distance * distance * 0.1f);

            float falloff = Mathf.Lerp(quadraticFalloff, linearFalloff, 0.5f);

            return lightData.Intensity * falloff;
        }

        private float CalculateSpotLightContribution(LightData lightData, Vector3 position, float distance)
        {
            Vector3 directionToPosition = (position - lightData.Position).normalized;
            float angle = Vector3.Angle(lightData.Direction, directionToPosition);

            // スポット角度外の場合
            if (angle > lightData.SpotAngle * 0.5f) return 0.0f;

            // 距離減衰
            float distanceFalloff = Mathf.Clamp01(1.0f - (distance / lightData.Range));

            // 角度減衰
            float angleFalloff = Mathf.Clamp01(1.0f - (angle / (lightData.SpotAngle * 0.5f)));
            angleFalloff = Mathf.Pow(angleFalloff, 2.0f); // より滑らかな減衰

            return lightData.Intensity * distanceFalloff * angleFalloff;
        }

        private float CalculateDirectionalLightContribution(LightData lightData, Vector3 position)
        {
            // 平行光源は距離に関係なく一定の強度
            return lightData.Intensity;
        }

        private float CalculateIndirectLight(Vector3 position)
        {
            float indirectLight = 0.0f;

            foreach (var probe in _reflectionProbes)
            {
                if (probe == null || !probe.enabled) continue;

                Vector3 probePosition = probe.transform.position;
                float distance = Vector3.Distance(position, probePosition);

                if (distance <= probe.size.magnitude)
                {
                    // 簡易的な間接光計算
                    float influence = 1.0f - (distance / probe.size.magnitude);
                    indirectLight += probe.intensity * influence * 0.2f; // 間接光は直接光より弱い
                }
            }

            return indirectLight;
        }

        private float CalculateShadowFactor(Vector3 lightPosition, Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - lightPosition).normalized;
            float distance = Vector3.Distance(lightPosition, targetPosition);

            float shadowFactor = 1.0f;
            int hitCount = 0;

            // 複数のレイキャストでソフトシャドウを近似
            for (int i = 0; i < _shadowRaycastSamples; i++)
            {
                Vector3 sampleDirection = direction;

                // 微小なランダムオフセットを追加してソフトシャドウ効果
                if (i > 0)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 0.1f;
                    sampleDirection = (direction + randomOffset).normalized;
                }

                if (Physics.Raycast(lightPosition, sampleDirection, distance, _shadowCasterLayers))
                {
                    hitCount++;
                }

                // デバッグレイの描画
                if (_showDebugRays)
                {
                    Color rayColor = Physics.Raycast(lightPosition, sampleDirection, distance, _shadowCasterLayers)
                        ? Color.red : Color.green;
                    Debug.DrawRay(lightPosition, sampleDirection * distance, rayColor, 0.1f);
                }
            }

            // 影の強度を計算
            float shadowIntensity = (float)hitCount / _shadowRaycastSamples;
            shadowFactor = 1.0f - shadowIntensity;

            return Mathf.Clamp01(shadowFactor);
        }
        #endregion

        #region Integration with Stealth Service
        private void NotifyVisibilityChange()
        {
            _stealthService?.UpdatePlayerVisibility(_currentLightLevel);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Light level updated: {_currentLightLevel:F3}");
        }

        /// <summary>
        /// StealthServiceから呼び出される光量計算
        /// </summary>
        public float GetCurrentLightLevel()
        {
            return _currentLightLevel;
        }

        /// <summary>
        /// 特定位置での光量を即座に計算
        /// </summary>
        public float GetLightLevelAtPosition(Vector3 position)
        {
            return CalculateLightLevelAtPosition(position);
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 光源リストを手動で更新
        /// </summary>
        public void ForceRefreshLightSources()
        {
            RefreshLightSources();
        }

        /// <summary>
        /// アンビエント光レベルを設定
        /// </summary>
        public void SetAmbientLightLevel(float level)
        {
            _ambientLightLevel = Mathf.Clamp01(level);
        }

        /// <summary>
        /// 更新間隔を変更
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            _updateInterval = Mathf.Max(0.016f, interval); // 最低60FPS
        }

        /// <summary>
        /// 現在アクティブな光源の数
        /// </summary>
        public int ActiveLightSourceCount => _activeLightSources.Count;
        #endregion

        #region Debug
        private void DrawDebugGizmos()
        {
            if (_playerTransform == null) return;

            Vector3 playerPos = _playerTransform.position;

            // 光源の影響範囲を表示
            if (_showLightInfluence)
            {
                foreach (var light in _activeLightSources)
                {
                    if (light == null) continue;

                    Gizmos.color = Color.yellow;

                    if (light.type == LightType.Point)
                    {
                        Gizmos.DrawWireSphere(light.transform.position, light.range);
                    }
                    else if (light.type == LightType.Spot)
                    {
                        // スポットライトの円錐を描画
                        DrawSpotLightGizmo(light);
                    }
                }

                // プレイヤー位置での光量レベルを表示
                Gizmos.color = Color.Lerp(Color.black, Color.white, _currentLightLevel);
                Gizmos.DrawSphere(playerPos, 0.5f);
            }
        }

        private void DrawSpotLightGizmo(Light spotLight)
        {
            Vector3 position = spotLight.transform.position;
            Vector3 direction = spotLight.transform.forward;
            float range = spotLight.range;
            float angle = spotLight.spotAngle;

            // 円錐の描画
            float radius = range * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad);
            Vector3 endPos = position + direction * range;

            Gizmos.DrawLine(position, endPos);

            // 円錐の境界線
            Vector3 up = spotLight.transform.up * radius;
            Vector3 right = spotLight.transform.right * radius;

            Gizmos.DrawLine(position, endPos + up);
            Gizmos.DrawLine(position, endPos - up);
            Gizmos.DrawLine(position, endPos + right);
            Gizmos.DrawLine(position, endPos - right);
        }
        #endregion
    }
}