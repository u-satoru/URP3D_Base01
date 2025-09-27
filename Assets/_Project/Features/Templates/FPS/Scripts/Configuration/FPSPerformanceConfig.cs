using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用パフォーマンス設定
    /// フレームレート、品質、最適化設定等を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Performance Config")]
    public class FPSPerformanceConfig : ScriptableObject
    {
        [Header("Frame Rate Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableVSync = false;
        [SerializeField] private bool _limitFrameRate = true;

        [Header("Quality Settings")]
        [SerializeField] private QualityLevel _qualityLevel = QualityLevel.Medium;
        [SerializeField] private int _textureQuality = 0; // 0 = Full Resolution
        [SerializeField] private bool _enableAntiAliasing = true;
        [SerializeField] private AntiAliasingMode _antiAliasingMode = AntiAliasingMode.FXAA;

        [Header("Rendering Optimization")]
        [SerializeField] private bool _enableOcclusion = true;
        [SerializeField] private float _lodBias = 1.0f;
        [SerializeField] private int _maximumLODLevel = 0;
        [SerializeField] private bool _enableBatching = true;

        [Header("Shadow Settings")]
        [SerializeField] private ShadowQuality _shadowQuality = ShadowQuality.Medium;
        [SerializeField] private float _shadowDistance = 50f;
        [SerializeField] private bool _enableSoftShadows = true;

        [Header("Post Processing")]
        [SerializeField] private bool _enablePostProcessing = true;
        [SerializeField] private float _postProcessingQuality = 1.0f;

        [Header("Memory Management")]
        [SerializeField] private bool _enableMemoryOptimization = true;
        [SerializeField] private int _objectPoolingThreshold = 10;
        [SerializeField] private bool _preloadAssets = false;

        // Properties
        public int TargetFrameRate => _targetFrameRate;
        public bool EnableVSync => _enableVSync;
        public bool LimitFrameRate => _limitFrameRate;
        public QualityLevel QualityLevel => _qualityLevel;
        public int TextureQuality => _textureQuality;
        public bool EnableAntiAliasing => _enableAntiAliasing;
        public AntiAliasingMode AntiAliasingMode => _antiAliasingMode;
        public bool EnableOcclusion => _enableOcclusion;
        public float LodBias => _lodBias;
        public int MaximumLODLevel => _maximumLODLevel;
        public bool EnableBatching => _enableBatching;
        public ShadowQuality ShadowQuality => _shadowQuality;
        public float ShadowDistance => _shadowDistance;
        public bool EnableSoftShadows => _enableSoftShadows;
        public bool EnablePostProcessing => _enablePostProcessing;
        public float PostProcessingQuality => _postProcessingQuality;
        public bool EnableMemoryOptimization => _enableMemoryOptimization;
        public int ObjectPoolingThreshold => _objectPoolingThreshold;
        public bool PreloadAssets => _preloadAssets;

        /// <summary>
        /// 設定をUnityに適用
        /// </summary>
        public void ApplySettings()
        {
            Application.targetFrameRate = _limitFrameRate ? _targetFrameRate : -1;
            QualitySettings.vSyncCount = _enableVSync ? 1 : 0;
            QualitySettings.globalTextureMipmapLimit = _textureQuality;
            QualitySettings.lodBias = _lodBias;
            QualitySettings.maximumLODLevel = _maximumLODLevel;
            QualitySettings.shadowDistance = _shadowDistance;
        }
    }

    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Ultra
    }

    public enum AntiAliasingMode
    {
        None,
        FXAA,
        SMAA,
        TAA
    }

    public enum ShadowQuality
    {
        Disabled,
        Low,
        Medium,
        High
    }
}
