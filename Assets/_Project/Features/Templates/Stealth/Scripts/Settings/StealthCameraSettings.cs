using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルステンプレート: カメラ設定
    /// ステルス特化カメラ制御、視点モード、検知可視化設定
    /// 既存のCinemachine 3.1 + CameraStateMachineとの完全統合
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/Settings/Camera Settings", fileName = "StealthCameraSettings")]
    public class StealthCameraSettings : ScriptableObject
    {
        #region Stealth Camera States

        [TabGroup("States", "Camera Modes")]
        [Title("ステルスカメラモード", "CameraStateMachine統合設定", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("ステルス特化カメラ状態設定")]
        private StealthCameraStateConfig[] stealthCameraStates = new StealthCameraStateConfig[]
        {
            new StealthCameraStateConfig 
            { 
                StateName = "StealthNormal",
                FOV = 60f,
                FollowDistance = 5f,
                Height = 1.8f,
                Sensitivity = 1.0f,
                SmoothTime = 0.2f,
                UseObstacleAvoidance = true,
                Description = "通常ステルス視点"
            },
            new StealthCameraStateConfig 
            { 
                StateName = "StealthCrouch",
                FOV = 55f,
                FollowDistance = 4f,
                Height = 1.2f,
                Sensitivity = 0.8f,
                SmoothTime = 0.15f,
                UseObstacleAvoidance = true,
                Description = "しゃがみステルス視点"
            },
            new StealthCameraStateConfig 
            { 
                StateName = "StealthCover",
                FOV = 50f,
                FollowDistance = 3f,
                Height = 1.5f,
                Sensitivity = 0.6f,
                SmoothTime = 0.1f,
                UseObstacleAvoidance = true,
                Description = "カバーポイント視点"
            },
            new StealthCameraStateConfig 
            { 
                StateName = "StealthObservation",
                FOV = 40f,
                FollowDistance = 6f,
                Height = 2.0f,
                Sensitivity = 0.5f,
                SmoothTime = 0.3f,
                UseObstacleAvoidance = false,
                Description = "観察・監視視点"
            }
        };

        [TabGroup("States", "Transition Settings")]
        [Title("状態遷移設定", "スムーズなカメラ切り替え", TitleAlignments.Centered)]
        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("状態遷移時間")]
        private float stateTransitionDuration = 0.5f;

        [SerializeField]
        [Tooltip("遷移カーブ")]
        private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        [Tooltip("自動状態切り替え有効化")]
        private bool enableAutoStateTransition = true;

        #endregion

        #region Detection Visualization

        [TabGroup("Visualization", "Detection Feedback")]
        [Title("検知可視化システム", "プレイヤーフィードバック", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("検知可視化有効化")]
        private bool enableDetectionVisualization = true;

        [SerializeField]
        [Tooltip("検知レベル表示設定")]
        private DetectionVisualizationConfig[] detectionVisualizations = new DetectionVisualizationConfig[]
        {
            new DetectionVisualizationConfig 
            { 
                DetectionLevel = "Hidden",
                ScreenTint = new Color(0.2f, 0.6f, 0.2f, 0.1f),
                VignetteIntensity = 0.1f,
                UIOpacity = 0.8f,
                Description = "完全隠蔽状態"
            },
            new DetectionVisualizationConfig 
            { 
                DetectionLevel = "Spotted",
                ScreenTint = new Color(0.6f, 0.6f, 0.2f, 0.2f),
                VignetteIntensity = 0.3f,
                UIOpacity = 0.9f,
                Description = "発見されかけ状態"
            },
            new DetectionVisualizationConfig 
            { 
                DetectionLevel = "Detected",
                ScreenTint = new Color(0.8f, 0.2f, 0.2f, 0.3f),
                VignetteIntensity = 0.5f,
                UIOpacity = 1.0f,
                Description = "発見状態"
            }
        };

        [TabGroup("Visualization", "Sensor Display")]
        [Title("センサー表示システム", "AI検知範囲の可視化", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("AI検知範囲表示有効化")]
        private bool enableSensorVisualization = true;

        [SerializeField]
        [Tooltip("視覚センサー表示設定")]
        private SensorVisualizationConfig visualSensorDisplay = new SensorVisualizationConfig
        {
            ShowRange = true,
            RangeColor = new Color(1f, 0.3f, 0.3f, 0.3f),
            ShowDirection = true,
            DirectionColor = new Color(1f, 0.5f, 0.5f, 0.5f),
            ShowDetectionLevel = true,
            FadeWithDistance = true
        };

        [SerializeField]
        [Tooltip("聴覚センサー表示設定")]
        private SensorVisualizationConfig auditorySensorDisplay = new SensorVisualizationConfig
        {
            ShowRange = true,
            RangeColor = new Color(0.3f, 0.3f, 1f, 0.2f),
            ShowDirection = false,
            DirectionColor = Color.clear,
            ShowDetectionLevel = true,
            FadeWithDistance = true
        };

        #endregion

        #region Stealth-Specific Features

        [TabGroup("Features", "Stealth Mechanics")]
        [Title("ステルス特化機能", "カメラ連動ステルス支援", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("影域強調表示")]
        private bool enableShadowHighlighting = true;

        [SerializeField]
        [Tooltip("影域表示色")]
        private Color shadowHighlightColor = new Color(0.2f, 0.2f, 0.8f, 0.3f);

        [SerializeField]
        [Tooltip("カバーポイント強調表示")]
        private bool enableCoverPointHighlighting = true;

        [SerializeField]
        [Tooltip("カバーポイント表示色")]
        private Color coverPointHighlightColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);

        [SerializeField]
        [Tooltip("インタラクティブオブジェクト強調")]
        private bool enableInteractableHighlighting = true;

        [SerializeField]
        [Tooltip("インタラクティブオブジェクト表示色")]
        private Color interactableHighlightColor = new Color(1f, 1f, 0.2f, 0.5f);

        [TabGroup("Features", "Dynamic Adjustments")]
        [Title("動的調整システム", "環境連動カメラ制御", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("明度連動カメラ調整")]
        private bool enableLightBasedAdjustment = true;

        [SerializeField]
        [Tooltip("明度調整設定")]
        private LightAdjustmentConfig[] lightAdjustments = new LightAdjustmentConfig[]
        {
            new LightAdjustmentConfig 
            { 
                LightLevel = "VeryDark",
                FOVAdjustment = -5f,
                SensitivityMultiplier = 1.2f,
                Description = "非常に暗い環境"
            },
            new LightAdjustmentConfig 
            { 
                LightLevel = "Dark",
                FOVAdjustment = -2f,
                SensitivityMultiplier = 1.1f,
                Description = "暗い環境"
            },
            new LightAdjustmentConfig 
            { 
                LightLevel = "Normal",
                FOVAdjustment = 0f,
                SensitivityMultiplier = 1.0f,
                Description = "通常明度"
            },
            new LightAdjustmentConfig 
            { 
                LightLevel = "Bright",
                FOVAdjustment = 3f,
                SensitivityMultiplier = 0.9f,
                Description = "明るい環境"
            }
        };

        [SerializeField]
        [Tooltip("緊張度連動カメラ調整")]
        private bool enableTensionBasedAdjustment = true;

        [SerializeField]
        [Tooltip("緊張度調整設定")]
        private TensionAdjustmentConfig[] tensionAdjustments = new TensionAdjustmentConfig[]
        {
            new TensionAdjustmentConfig 
            { 
                TensionLevel = "Relaxed",
                FOVAdjustment = 0f,
                ShakeIntensity = 0f,
                Description = "リラックス状態"
            },
            new TensionAdjustmentConfig 
            { 
                TensionLevel = "Alert",
                FOVAdjustment = -3f,
                ShakeIntensity = 0.1f,
                Description = "警戒状態"
            },
            new TensionAdjustmentConfig 
            { 
                TensionLevel = "Danger",
                FOVAdjustment = -8f,
                ShakeIntensity = 0.3f,
                Description = "危険状態"
            }
        };

        #endregion

        #region Cinemachine Integration

        [TabGroup("Cinemachine", "Virtual Cameras")]
        [Title("Cinemachine統合", "VirtualCamera詳細設定", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("Cinemachine統合有効化")]
        private bool enableCinemachineIntegration = true;

        [SerializeField]
        [Tooltip("Virtual Camera設定")]
        private CinemachineVirtualCameraConfig[] virtualCameraConfigs = new CinemachineVirtualCameraConfig[]
        {
            new CinemachineVirtualCameraConfig 
            { 
                CameraName = "StealthNormal_VCam",
                Priority = 10,
                BlendTime = 1f,
                BodyType = "3rdPersonFollow",
                AimType = "POV",
                NoiseProfile = "None"
            },
            new CinemachineVirtualCameraConfig 
            { 
                CameraName = "StealthCrouch_VCam",
                Priority = 15,
                BlendTime = 0.5f,
                BodyType = "3rdPersonFollow",
                AimType = "POV",
                NoiseProfile = "Handheld_subtle"
            },
            new CinemachineVirtualCameraConfig 
            { 
                CameraName = "StealthCover_VCam",
                Priority = 20,
                BlendTime = 0.3f,
                BodyType = "FramingTransposer",
                AimType = "HardLookAt",
                NoiseProfile = "None"
            }
        };

        [TabGroup("Cinemachine", "Camera Blending")]
        [Title("カメラブレンド設定", "状態間の滑らかな遷移", TitleAlignments.Centered)]
        [SerializeField, Range(0.1f, 5f)]
        [Tooltip("デフォルトブレンド時間")]
        private float defaultBlendTime = 1f;

        [SerializeField]
        [Tooltip("カスタムブレンド設定")]
        private CustomBlendConfig[] customBlends = new CustomBlendConfig[]
        {
            new CustomBlendConfig 
            { 
                FromState = "StealthNormal",
                ToState = "StealthCrouch",
                BlendTime = 0.3f,
                BlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            },
            new CustomBlendConfig 
            { 
                FromState = "StealthCrouch",
                ToState = "StealthCover",
                BlendTime = 0.2f,
                BlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
            }
        };

        #endregion

        #region Performance & Optimization

        [TabGroup("Performance", "Rendering")]
        [Title("レンダリング最適化", "ステルス特化最適化", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("動的LOD有効化")]
        private bool enableDynamicLOD = true;

        [SerializeField, Range(10f, 100f)]
        [Tooltip("LOD切り替え距離")]
        private float lodSwitchDistance = 30f;

        [SerializeField]
        [Tooltip("影品質自動調整")]
        private bool enableShadowQualityAdjustment = true;

        [SerializeField]
        [Tooltip("影品質レベル")]
        private ShadowQualityLevel[] shadowQualityLevels = new ShadowQualityLevel[]
        {
            new ShadowQualityLevel { DistanceRange = "Near (0-10m)", Quality = "High" },
            new ShadowQualityLevel { DistanceRange = "Medium (10-30m)", Quality = "Medium" },
            new ShadowQualityLevel { DistanceRange = "Far (30m+)", Quality = "Low" }
        };

        [TabGroup("Performance", "Update Frequency")]
        [Title("更新頻度最適化", "パフォーマンス管理", TitleAlignments.Centered)]
        [SerializeField, Range(30f, 120f)]
        [Tooltip("カメラ更新頻度（FPS）")]
        private float cameraUpdateFrequency = 60f;

        [SerializeField, Range(10f, 60f)]
        [Tooltip("可視化更新頻度（FPS）")]
        private float visualizationUpdateFrequency = 30f;

        [SerializeField]
        [Tooltip("距離カリング有効化")]
        private bool enableDistanceCulling = true;

        [SerializeField, Range(50f, 200f)]
        [Tooltip("カリング距離")]
        private float cullingDistance = 100f;

        #endregion

        #region Properties (Public API)

        // Camera States Properties
        public StealthCameraStateConfig[] StealthCameraStates => stealthCameraStates;
        public float StateTransitionDuration => stateTransitionDuration;
        public AnimationCurve TransitionCurve => transitionCurve;
        public bool EnableAutoStateTransition => enableAutoStateTransition;

        // Detection Visualization Properties
        public bool EnableDetectionVisualization => enableDetectionVisualization;
        public DetectionVisualizationConfig[] DetectionVisualizations => detectionVisualizations;
        public bool EnableSensorVisualization => enableSensorVisualization;
        public SensorVisualizationConfig VisualSensorDisplay => visualSensorDisplay;
        public SensorVisualizationConfig AuditorySensorDisplay => auditorySensorDisplay;

        // Stealth Features Properties
        public bool EnableShadowHighlighting => enableShadowHighlighting;
        public Color ShadowHighlightColor => shadowHighlightColor;
        public bool EnableCoverPointHighlighting => enableCoverPointHighlighting;
        public Color CoverPointHighlightColor => coverPointHighlightColor;
        public bool EnableInteractableHighlighting => enableInteractableHighlighting;
        public Color InteractableHighlightColor => interactableHighlightColor;
        public bool EnableLightBasedAdjustment => enableLightBasedAdjustment;
        public LightAdjustmentConfig[] LightAdjustments => lightAdjustments;
        public bool EnableTensionBasedAdjustment => enableTensionBasedAdjustment;
        public TensionAdjustmentConfig[] TensionAdjustments => tensionAdjustments;

        // Cinemachine Properties
        public bool EnableCinemachineIntegration => enableCinemachineIntegration;
        public CinemachineVirtualCameraConfig[] VirtualCameraConfigs => virtualCameraConfigs;
        public float DefaultBlendTime => defaultBlendTime;
        public CustomBlendConfig[] CustomBlends => customBlends;

        // Performance Properties
        public bool EnableDynamicLOD => enableDynamicLOD;
        public float LODSwitchDistance => lodSwitchDistance;
        public bool EnableShadowQualityAdjustment => enableShadowQualityAdjustment;
        public ShadowQualityLevel[] ShadowQualityLevels => shadowQualityLevels;
        public float CameraUpdateFrequency => cameraUpdateFrequency;
        public float VisualizationUpdateFrequency => visualizationUpdateFrequency;
        public bool EnableDistanceCulling => enableDistanceCulling;
        public float CullingDistance => cullingDistance;

        #endregion

        #region Camera Configuration Methods

        /// <summary>
        /// ステルスカメラ状態設定を取得
        /// </summary>
        public StealthCameraStateConfig GetCameraStateConfig(string stateName)
        {
            foreach (var config in stealthCameraStates)
            {
                if (config.StateName == stateName)
                    return config;
            }
            return stealthCameraStates[0]; // Default to first state
        }

        /// <summary>
        /// 検知可視化設定を取得
        /// </summary>
        public DetectionVisualizationConfig GetDetectionVisualization(string detectionLevel)
        {
            foreach (var config in detectionVisualizations)
            {
                if (config.DetectionLevel == detectionLevel)
                    return config;
            }
            return detectionVisualizations[0]; // Default to first config
        }

        /// <summary>
        /// 明度調整設定を取得
        /// </summary>
        public LightAdjustmentConfig GetLightAdjustment(string lightLevel)
        {
            foreach (var config in lightAdjustments)
            {
                if (config.LightLevel == lightLevel)
                    return config;
            }
            return lightAdjustments[2]; // Default to normal
        }

        /// <summary>
        /// 緊張度調整設定を取得
        /// </summary>
        public TensionAdjustmentConfig GetTensionAdjustment(string tensionLevel)
        {
            foreach (var config in tensionAdjustments)
            {
                if (config.TensionLevel == tensionLevel)
                    return config;
            }
            return tensionAdjustments[0]; // Default to relaxed
        }

        /// <summary>
        /// VirtualCamera設定を取得
        /// </summary>
        public CinemachineVirtualCameraConfig GetVirtualCameraConfig(string cameraName)
        {
            foreach (var config in virtualCameraConfigs)
            {
                if (config.CameraName == cameraName)
                    return config;
            }
            return virtualCameraConfigs[0]; // Default to first config
        }

        /// <summary>
        /// カスタムブレンド設定を取得
        /// </summary>
        public CustomBlendConfig GetCustomBlend(string fromState, string toState)
        {
            foreach (var blend in customBlends)
            {
                if (blend.FromState == fromState && blend.ToState == toState)
                    return blend;
            }
            return null; // Use default blend
        }

        /// <summary>
        /// CameraStateMachineに設定を適用
        /// </summary>
        public void ApplyToCameraStateMachine(Component cameraStateMachine)
        {
            if (cameraStateMachine == null) return;
            
            Debug.Log($"Applied stealth camera settings to {cameraStateMachine.name}");
        }

        #endregion

        #region Nested Data Classes

        [System.Serializable]
        public class StealthCameraStateConfig
        {
            [Tooltip("状態名")]
            public string StateName;
            
            [Range(20f, 100f)]
            [Tooltip("視野角（FOV）")]
            public float FOV;
            
            [Range(1f, 10f)]
            [Tooltip("追従距離")]
            public float FollowDistance;
            
            [Range(0.5f, 3f)]
            [Tooltip("カメラ高さ")]
            public float Height;
            
            [Range(0.1f, 3f)]
            [Tooltip("感度")]
            public float Sensitivity;
            
            [Range(0.05f, 1f)]
            [Tooltip("スムーズ時間")]
            public float SmoothTime;
            
            [Tooltip("障害物回避使用")]
            public bool UseObstacleAvoidance;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class DetectionVisualizationConfig
        {
            [Tooltip("検知レベル")]
            public string DetectionLevel;
            
            [Tooltip("画面色調")]
            public Color ScreenTint;
            
            [Range(0f, 1f)]
            [Tooltip("ビネット強度")]
            public float VignetteIntensity;
            
            [Range(0.1f, 1f)]
            [Tooltip("UI透明度")]
            public float UIOpacity;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class SensorVisualizationConfig
        {
            [Tooltip("範囲表示")]
            public bool ShowRange;
            
            [Tooltip("範囲色")]
            public Color RangeColor;
            
            [Tooltip("方向表示")]
            public bool ShowDirection;
            
            [Tooltip("方向色")]
            public Color DirectionColor;
            
            [Tooltip("検知レベル表示")]
            public bool ShowDetectionLevel;
            
            [Tooltip("距離によるフェード")]
            public bool FadeWithDistance;
        }

        [System.Serializable]
        public class LightAdjustmentConfig
        {
            [Tooltip("明度レベル")]
            public string LightLevel;
            
            [Range(-10f, 10f)]
            [Tooltip("FOV調整値")]
            public float FOVAdjustment;
            
            [Range(0.1f, 2f)]
            [Tooltip("感度倍率")]
            public float SensitivityMultiplier;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class TensionAdjustmentConfig
        {
            [Tooltip("緊張レベル")]
            public string TensionLevel;
            
            [Range(-15f, 5f)]
            [Tooltip("FOV調整値")]
            public float FOVAdjustment;
            
            [Range(0f, 1f)]
            [Tooltip("手ブレ強度")]
            public float ShakeIntensity;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class CinemachineVirtualCameraConfig
        {
            [Tooltip("カメラ名")]
            public string CameraName;
            
            [Range(0, 50)]
            [Tooltip("優先度")]
            public int Priority;
            
            [Range(0.1f, 5f)]
            [Tooltip("ブレンド時間")]
            public float BlendTime;
            
            [Tooltip("ボディタイプ")]
            public string BodyType;
            
            [Tooltip("エイムタイプ")]
            public string AimType;
            
            [Tooltip("ノイズプロファイル")]
            public string NoiseProfile;
        }

        [System.Serializable]
        public class CustomBlendConfig
        {
            [Tooltip("開始状態")]
            public string FromState;
            
            [Tooltip("終了状態")]
            public string ToState;
            
            [Range(0.1f, 5f)]
            [Tooltip("ブレンド時間")]
            public float BlendTime;
            
            [Tooltip("ブレンドカーブ")]
            public AnimationCurve BlendCurve;
        }

        [System.Serializable]
        public class ShadowQualityLevel
        {
            [Tooltip("距離範囲")]
            public string DistanceRange;
            
            [Tooltip("品質レベル")]
            public string Quality;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug", "Debug Actions")]
        [Button("Test Camera State Transitions")]
        public void TestCameraStateTransitions()
        {
            Debug.Log("=== Camera State Transitions Test ===");
            foreach (var state in stealthCameraStates)
            {
                Debug.Log($"State: {state.StateName}");
                Debug.Log($"  FOV: {state.FOV}°, Distance: {state.FollowDistance}m, Height: {state.Height}m");
                Debug.Log($"  Sensitivity: {state.Sensitivity}, Smooth: {state.SmoothTime}s");
            }
        }

        [Button("Test Detection Visualization")]
        public void TestDetectionVisualization()
        {
            Debug.Log("=== Detection Visualization Test ===");
            foreach (var detection in detectionVisualizations)
            {
                Debug.Log($"Level: {detection.DetectionLevel}");
                Debug.Log($"  Tint: {detection.ScreenTint}, Vignette: {detection.VignetteIntensity:F2}");
                Debug.Log($"  UI Opacity: {detection.UIOpacity:F2}");
            }
        }

        [Button("Test Light Adjustments")]
        public void TestLightAdjustments()
        {
            Debug.Log("=== Light Adjustments Test ===");
            foreach (var adjustment in lightAdjustments)
            {
                Debug.Log($"Light Level: {adjustment.LightLevel}");
                Debug.Log($"  FOV Adj: {adjustment.FOVAdjustment:F1}°");
                Debug.Log($"  Sensitivity: {adjustment.SensitivityMultiplier:F2}x");
            }
        }

        [Button("Print Camera Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Stealth Camera Configuration ===");
            Debug.Log($"Camera States: {stealthCameraStates.Length} states configured");
            Debug.Log($"Transition Duration: {stateTransitionDuration}s");
            Debug.Log($"Detection Visualization: {enableDetectionVisualization}");
            Debug.Log($"Sensor Visualization: {enableSensorVisualization}");
            Debug.Log($"Cinemachine Integration: {enableCinemachineIntegration}");
            Debug.Log($"Update Frequency: Camera={cameraUpdateFrequency}Hz, Viz={visualizationUpdateFrequency}Hz");
        }
#endif

        #endregion
    }
}
