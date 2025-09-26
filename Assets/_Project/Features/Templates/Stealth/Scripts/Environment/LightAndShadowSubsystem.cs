using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// 蜈峨→蠖ｱ縺ｮ繧ｵ繝悶す繧ｹ繝・Β
    /// 繝励Ξ繧､繝､繝ｼ縺ｮ菴咲ｽｮ縺ｨ蜻ｨ蝗ｲ縺ｮ蜈画ｺ舌ｒ蝓ｺ縺ｫ隕冶ｪ肴ｧ菫よ焚繧堤ｮ怜・
    /// 隧ｳ邏ｰ險ｭ險域嶌縺ｮLightAndShadowSubsystem螳溯｣・
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
        private int _shadowRaycastSamples = 8; // 隍・焚譁ｹ蜷代°繧峨・蜈臥ｷ壹メ繧ｧ繝・け

        [Header("Performance")]
        [SerializeField]
        private float _updateInterval = 0.1f; // 10Hz譖ｴ譁ｰ

        [SerializeField]
        private int _maxLightSources = 20; // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蛻ｶ髯・

        [SerializeField]
        private float _lightInfluenceThreshold = 0.01f; // 蠖ｱ髻ｿ縺悟ｰ上＆縺・・貅舌ｒ辟｡隕・

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
            // ServiceLocator縺九ｉStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning($"[{gameObject.name}] StealthService not found in ServiceLocator");
            }

            // 繝励Ξ繧､繝､繝ｼ縺ｮ蜿門ｾ・
            FindPlayerTransform();

            // 蜈画ｺ舌・蛻晄悄讀懃ｴ｢
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

            // 繧ｷ繝ｼ繝ｳ蜀・・蜈ｨ蜈画ｺ舌ｒ蜿門ｾ・
            Light[] allLights = FindObjectsOfType<Light>();

            foreach (var light in allLights)
            {
                if (light.enabled && ((1 << light.gameObject.layer) & _lightSourceLayers) != 0)
                {
                    _activeLightSources.Add(light);
                    UpdateLightDataCache(light);
                }
            }

            // ReflectionProbe繧ょ叙蠕暦ｼ磯俣謗･蜈峨・險育ｮ礼畑・・
            _reflectionProbes.Clear();
            ReflectionProbe[] allProbes = FindObjectsOfType<ReflectionProbe>();
            _reflectionProbes.AddRange(allProbes);

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ蛻ｶ髯舌・驕ｩ逕ｨ
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

                // 霍晞屬縺ｨ蠑ｷ蠎ｦ繧定・・縺励◆驥崎ｦ∝ｺｦ險育ｮ・
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

            // 菴咲ｽｮ縺悟､ｧ縺阪￥螟峨ｏ縺｣縺溷ｴ蜷医・蜈画ｺ舌Μ繧ｹ繝医ｒ譖ｴ譁ｰ
            if (Vector3.Distance(currentPosition, _lastPlayerPosition) > 10.0f)
            {
                RefreshLightSources();
                _lastPlayerPosition = currentPosition;
            }

            // 蜈蛾㍼縺ｮ險育ｮ・
            float newLightLevel = CalculateLightLevelAtPosition(currentPosition);

            // 譛画э縺ｪ螟牙喧縺後≠縺｣縺溷ｴ蜷医・縺ｿ譖ｴ譁ｰ
            if (Mathf.Abs(newLightLevel - _currentLightLevel) > 0.01f)
            {
                _currentLightLevel = newLightLevel;
                NotifyVisibilityChange();
            }
        }

        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｧ縺ｮ蜈蛾㍼繝ｬ繝吶Ν繧定ｨ育ｮ暦ｼ・.0 = 螳悟・縺ｪ髣・ 1.0 = 螳悟・縺ｪ蜈会ｼ・
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

            // 髢捺磁蜈峨・險育ｮ暦ｼ・eflectionProbe繝吶・繧ｹ・・
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

            // 遽・峇螟悶メ繧ｧ繝・け
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

            // 蠖ｱ縺ｮ險育ｮ・
            if (_enableShadowDetection && lightContribution > 0.0f)
            {
                float shadowFactor = CalculateShadowFactor(lightPosition, position);
                lightContribution *= shadowFactor;
            }

            return lightContribution;
        }

        private float CalculatePointLightContribution(LightData lightData, Vector3 position, float distance)
        {
            // 邱壼ｽ｢貂幄｡ｰ + 莠梧ｬ｡貂幄｡ｰ縺ｮ繝悶Ξ繝ｳ繝・
            float linearFalloff = Mathf.Clamp01(1.0f - (distance / lightData.Range));
            float quadraticFalloff = 1.0f / (1.0f + distance * distance * 0.1f);

            float falloff = Mathf.Lerp(quadraticFalloff, linearFalloff, 0.5f);

            return lightData.Intensity * falloff;
        }

        private float CalculateSpotLightContribution(LightData lightData, Vector3 position, float distance)
        {
            Vector3 directionToPosition = (position - lightData.Position).normalized;
            float angle = Vector3.Angle(lightData.Direction, directionToPosition);

            // 繧ｹ繝昴ャ繝郁ｧ貞ｺｦ螟悶・蝣ｴ蜷・
            if (angle > lightData.SpotAngle * 0.5f) return 0.0f;

            // 霍晞屬貂幄｡ｰ
            float distanceFalloff = Mathf.Clamp01(1.0f - (distance / lightData.Range));

            // 隗貞ｺｦ貂幄｡ｰ
            float angleFalloff = Mathf.Clamp01(1.0f - (angle / (lightData.SpotAngle * 0.5f)));
            angleFalloff = Mathf.Pow(angleFalloff, 2.0f); // 繧医ｊ貊代ｉ縺九↑貂幄｡ｰ

            return lightData.Intensity * distanceFalloff * angleFalloff;
        }

        private float CalculateDirectionalLightContribution(LightData lightData, Vector3 position)
        {
            // 蟷ｳ陦悟・貅舌・霍晞屬縺ｫ髢｢菫ゅ↑縺丈ｸ螳壹・蠑ｷ蠎ｦ
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
                    // 邁｡譏鍋噪縺ｪ髢捺磁蜈芽ｨ育ｮ・
                    float influence = 1.0f - (distance / probe.size.magnitude);
                    indirectLight += probe.intensity * influence * 0.2f; // 髢捺磁蜈峨・逶ｴ謗･蜈峨ｈ繧雁ｼｱ縺・
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

            // 隍・焚縺ｮ繝ｬ繧､繧ｭ繝｣繧ｹ繝医〒繧ｽ繝輔ヨ繧ｷ繝｣繝峨え繧定ｿ台ｼｼ
            for (int i = 0; i < _shadowRaycastSamples; i++)
            {
                Vector3 sampleDirection = direction;

                // 蠕ｮ蟆上↑繝ｩ繝ｳ繝繝繧ｪ繝輔そ繝・ヨ繧定ｿｽ蜉縺励※繧ｽ繝輔ヨ繧ｷ繝｣繝峨え蜉ｹ譫・
                if (i > 0)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 0.1f;
                    sampleDirection = (direction + randomOffset).normalized;
                }

                if (Physics.Raycast(lightPosition, sampleDirection, distance, _shadowCasterLayers))
                {
                    hitCount++;
                }

                // 繝・ヰ繝・げ繝ｬ繧､縺ｮ謠冗判
                if (_showDebugRays)
                {
                    Color rayColor = Physics.Raycast(lightPosition, sampleDirection, distance, _shadowCasterLayers)
                        ? Color.red : Color.green;
                    Debug.DrawRay(lightPosition, sampleDirection * distance, rayColor, 0.1f);
                }
            }

            // 蠖ｱ縺ｮ蠑ｷ蠎ｦ繧定ｨ育ｮ・
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
        /// StealthService縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧句・驥剰ｨ育ｮ・
        /// </summary>
        public float GetCurrentLightLevel()
        {
            return _currentLightLevel;
        }

        /// <summary>
        /// 迚ｹ螳壻ｽ咲ｽｮ縺ｧ縺ｮ蜈蛾㍼繧貞叉蠎ｧ縺ｫ險育ｮ・
        /// </summary>
        public float GetLightLevelAtPosition(Vector3 position)
        {
            return CalculateLightLevelAtPosition(position);
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 蜈画ｺ舌Μ繧ｹ繝医ｒ謇句虚縺ｧ譖ｴ譁ｰ
        /// </summary>
        public void ForceRefreshLightSources()
        {
            RefreshLightSources();
        }

        /// <summary>
        /// 繧｢繝ｳ繝薙お繝ｳ繝亥・繝ｬ繝吶Ν繧定ｨｭ螳・
        /// </summary>
        public void SetAmbientLightLevel(float level)
        {
            _ambientLightLevel = Mathf.Clamp01(level);
        }

        /// <summary>
        /// 譖ｴ譁ｰ髢馴囈繧貞､画峩
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            _updateInterval = Mathf.Max(0.016f, interval); // 譛菴・0FPS
        }

        /// <summary>
        /// 迴ｾ蝨ｨ繧｢繧ｯ繝・ぅ繝悶↑蜈画ｺ舌・謨ｰ
        /// </summary>
        public int ActiveLightSourceCount => _activeLightSources.Count;
        #endregion

        #region Debug
        private void DrawDebugGizmos()
        {
            if (_playerTransform == null) return;

            Vector3 playerPos = _playerTransform.position;

            // 蜈画ｺ舌・蠖ｱ髻ｿ遽・峇繧定｡ｨ遉ｺ
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
                        // 繧ｹ繝昴ャ繝医Λ繧､繝医・蜀・倹繧呈緒逕ｻ
                        DrawSpotLightGizmo(light);
                    }
                }

                // 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ縺ｧ縺ｮ蜈蛾㍼繝ｬ繝吶Ν繧定｡ｨ遉ｺ
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

            // 蜀・倹縺ｮ謠冗判
            float radius = range * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad);
            Vector3 endPos = position + direction * range;

            Gizmos.DrawLine(position, endPos);

            // 蜀・倹縺ｮ蠅・阜邱・
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


