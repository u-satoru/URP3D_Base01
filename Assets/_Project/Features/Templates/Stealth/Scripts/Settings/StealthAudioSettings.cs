using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルステンプレート: オーディオ設定
    /// 3D空間オーディオ、環境マスキング、AI聴覚検知統合設定
    /// 既存のStealthAudioCoordinator + IStealthAudioServiceとの完全統合
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/Settings/Audio Settings", fileName = "StealthAudioSettings")]
    public class StealthAudioSettings : ScriptableObject
    {
        #region 3D Spatial Audio System

        [TabGroup("Spatial", "3D Audio")]
        [Title("3D空間オーディオ", "StealthAudioCoordinator統合設定", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("3D空間オーディオ有効化")]
        private bool enable3DSpatialAudio = true;

        [SerializeField, Range(1f, 100f)]
        [Tooltip("最大オーディオ距離（メートル）")]
        private float maxAudioDistance = 50f;

        [SerializeField, Range(0.1f, 10f)]
        [Tooltip("音速（メートル/秒） - リアルな音響遅延")]
        private float soundSpeed = 343f;

        [SerializeField]
        [Tooltip("ドップラー効果有効化")]
        private bool enableDopplerEffect = true;

        [SerializeField, Range(0f, 5f)]
        [Tooltip("ドップラー効果スケール")]
        private float dopplerScale = 1f;

        [TabGroup("Spatial", "Attenuation")]
        [Title("距離減衰システム", "音響物理シミュレーション", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("距離減衰カーブ（距離 vs 音量）")]
        private AnimationCurve distanceAttenuationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [SerializeField]
        [Tooltip("周波数別減衰設定")]
        private FrequencyAttenuation[] frequencyAttenuations = new FrequencyAttenuation[]
        {
            new FrequencyAttenuation { FrequencyRange = "Low (20-250Hz)", AttenuationMultiplier = 0.8f },
            new FrequencyAttenuation { FrequencyRange = "Mid (250-4000Hz)", AttenuationMultiplier = 1.0f },
            new FrequencyAttenuation { FrequencyRange = "High (4000-20000Hz)", AttenuationMultiplier = 1.5f }
        };

        [SerializeField, Range(0.1f, 3.0f)]
        [Tooltip("空気吸収係数（高周波の自然減衰）")]
        private float airAbsorptionCoefficient = 1.0f;

        #endregion

        #region Environmental Audio Masking

        [TabGroup("Environment", "Masking System")]
        [Title("環境音マスキング", "背景音による検知減衰システム", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("環境マスキング有効化")]
        private bool enableEnvironmentalMasking = true;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("ベース環境ノイズレベル")]
        private float baseEnvironmentNoiseLevel = 0.3f;

        [SerializeField]
        [Tooltip("環境音プロファイル")]
        private EnvironmentalAudioProfile[] environmentProfiles = new EnvironmentalAudioProfile[]
        {
            new EnvironmentalAudioProfile 
            { 
                EnvironmentName = "Urban",
                MaskingLevel = 0.6f,
                CharacteristicSounds = new string[] { "Traffic", "Pedestrians", "Construction" },
                Description = "都市環境 - 高いマスキング効果"
            },
            new EnvironmentalAudioProfile 
            { 
                EnvironmentName = "Forest",
                MaskingLevel = 0.3f,
                CharacteristicSounds = new string[] { "Wind", "Birds", "Rustling" },
                Description = "森林環境 - 中程度のマスキング効果"
            },
            new EnvironmentalAudioProfile 
            { 
                EnvironmentName = "Industrial",
                MaskingLevel = 0.8f,
                CharacteristicSounds = new string[] { "Machinery", "Ventilation", "Steam" },
                Description = "工業環境 - 非常に高いマスキング効果"
            },
            new EnvironmentalAudioProfile 
            { 
                EnvironmentName = "Library",
                MaskingLevel = 0.1f,
                CharacteristicSounds = new string[] { "Air Conditioning", "Page Turning" },
                Description = "図書館環境 - 最小マスキング効果"
            }
        };

        [TabGroup("Environment", "Dynamic Masking")]
        [Title("動的マスキング", "リアルタイム環境変化", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("動的マスキング有効化")]
        private bool enableDynamicMasking = true;

        [SerializeField, Range(0.1f, 5f)]
        [Tooltip("マスキングレベル変化速度")]
        private float maskingTransitionSpeed = 1f;

        [SerializeField]
        [Tooltip("時間帯による変化")]
        private TimeBasedMasking[] timeBasedMasking = new TimeBasedMasking[]
        {
            new TimeBasedMasking { TimeOfDay = "Dawn", MaskingMultiplier = 0.7f },
            new TimeBasedMasking { TimeOfDay = "Day", MaskingMultiplier = 1.0f },
            new TimeBasedMasking { TimeOfDay = "Dusk", MaskingMultiplier = 0.8f },
            new TimeBasedMasking { TimeOfDay = "Night", MaskingMultiplier = 0.5f }
        };

        #endregion

        #region Material Acoustic Properties

        [TabGroup("Materials", "Surface Acoustics")]
        [Title("表面音響特性", "材質別音響シミュレーション", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("材質音響システム有効化")]
        private bool enableMaterialAcoustics = true;

        [SerializeField]
        [Tooltip("材質音響プロファイル")]
        private MaterialAcousticProfile[] materialProfiles = new MaterialAcousticProfile[]
        {
            new MaterialAcousticProfile 
            { 
                MaterialName = "Concrete",
                ReflectionCoefficient = 0.95f,
                AbsorptionCoefficient = 0.05f,
                TransmissionCoefficient = 0.1f,
                SoundCharacter = "Hard, Echoing"
            },
            new MaterialAcousticProfile 
            { 
                MaterialName = "Wood",
                ReflectionCoefficient = 0.7f,
                AbsorptionCoefficient = 0.25f,
                TransmissionCoefficient = 0.3f,
                SoundCharacter = "Warm, Resonant"
            },
            new MaterialAcousticProfile 
            { 
                MaterialName = "Carpet",
                ReflectionCoefficient = 0.2f,
                AbsorptionCoefficient = 0.8f,
                TransmissionCoefficient = 0.05f,
                SoundCharacter = "Muffled, Soft"
            },
            new MaterialAcousticProfile 
            { 
                MaterialName = "Metal",
                ReflectionCoefficient = 0.98f,
                AbsorptionCoefficient = 0.02f,
                TransmissionCoefficient = 0.15f,
                SoundCharacter = "Sharp, Ringing"
            },
            new MaterialAcousticProfile 
            { 
                MaterialName = "Glass",
                ReflectionCoefficient = 0.92f,
                AbsorptionCoefficient = 0.08f,
                TransmissionCoefficient = 0.4f,
                SoundCharacter = "Brittle, Reflective"
            }
        };

        [TabGroup("Materials", "Footstep Audio")]
        [Title("足音オーディオ", "表面別足音特性", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("表面別足音設定")]
        private FootstepAudioProfile[] footstepProfiles = new FootstepAudioProfile[]
        {
            new FootstepAudioProfile 
            { 
                SurfaceType = "Concrete",
                BaseVolume = 0.8f,
                PitchVariation = 0.1f,
                DetectionRadius = 8f,
                AudioClips = new string[] { "footstep_concrete_01", "footstep_concrete_02", "footstep_concrete_03" }
            },
            new FootstepAudioProfile 
            { 
                SurfaceType = "Metal",
                BaseVolume = 1.0f,
                PitchVariation = 0.2f,
                DetectionRadius = 12f,
                AudioClips = new string[] { "footstep_metal_01", "footstep_metal_02" }
            },
            new FootstepAudioProfile 
            { 
                SurfaceType = "Gravel",
                BaseVolume = 0.9f,
                PitchVariation = 0.3f,
                DetectionRadius = 10f,
                AudioClips = new string[] { "footstep_gravel_01", "footstep_gravel_02", "footstep_gravel_03", "footstep_gravel_04" }
            }
        };

        #endregion

        #region AI Auditory Detection Integration

        [TabGroup("AI Detection", "Auditory AI")]
        [Title("AI聴覚検知統合", "NPCAuditorySensor統合", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("AI聴覚検知有効化")]
        private bool enableAIAuditoryDetection = true;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("AI聴覚感度グローバル倍率")]
        private float aiHearingSensitivityMultiplier = 1f;

        [SerializeField]
        [Tooltip("AI聴覚プロファイル")]
        private AIAuditoryProfile[] aiAuditoryProfiles = new AIAuditoryProfile[]
        {
            new AIAuditoryProfile 
            { 
                AIType = "Guard",
                HearingRange = 12f,
                FrequencySensitivity = new float[] { 0.8f, 1.0f, 1.2f }, // Low, Mid, High
                SuspicionIncrement = 0.3f,
                Description = "標準警備員 - バランス型聴覚"
            },
            new AIAuditoryProfile 
            { 
                AIType = "Patrol",
                HearingRange = 10f,
                FrequencySensitivity = new float[] { 1.0f, 1.0f, 0.8f },
                SuspicionIncrement = 0.25f,
                Description = "パトロール要員 - 中距離聴覚"
            },
            new AIAuditoryProfile 
            { 
                AIType = "Sentry",
                HearingRange = 15f,
                FrequencySensitivity = new float[] { 1.2f, 1.3f, 1.5f },
                SuspicionIncrement = 0.4f,
                Description = "歩哨 - 高感度聴覚"
            }
        };

        [TabGroup("AI Detection", "Sound Recognition")]
        [Title("音響認識システム", "AI音源識別能力", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("音源識別有効化")]
        private bool enableSoundRecognition = true;

        [SerializeField]
        [Tooltip("音源カテゴリ設定")]
        private SoundCategory[] soundCategories = new SoundCategory[]
        {
            new SoundCategory 
            { 
                CategoryName = "Footsteps",
                ThreatLevel = 0.6f,
                RecognitionAccuracy = 0.8f,
                RequiredVolume = 0.2f,
                Description = "足音 - 中程度の脅威"
            },
            new SoundCategory 
            { 
                CategoryName = "Impact",
                ThreatLevel = 0.9f,
                RecognitionAccuracy = 0.9f,
                RequiredVolume = 0.4f,
                Description = "衝撃音 - 高脅威"
            },
            new SoundCategory 
            { 
                CategoryName = "Ambient",
                ThreatLevel = 0.1f,
                RecognitionAccuracy = 0.5f,
                RequiredVolume = 0.1f,
                Description = "環境音 - 低脅威"
            }
        };

        #endregion

        #region Dynamic Audio System

        [TabGroup("Dynamic", "Adaptive Audio")]
        [Title("適応的オーディオ", "ゲーム状況連動システム", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("適応的オーディオ有効化")]
        private bool enableAdaptiveAudio = true;

        [SerializeField]
        [Tooltip("警戒レベル連動音響設定")]
        private AlertAudioConfiguration[] alertAudioConfigs = new AlertAudioConfiguration[]
        {
            new AlertAudioConfiguration 
            { 
                AlertLevel = "Relaxed",
                MusicVolume = 0.6f,
                AmbienceVolume = 0.8f,
                SFXVolume = 1.0f,
                Description = "通常状態 - リラックス音響"
            },
            new AlertAudioConfiguration 
            { 
                AlertLevel = "Suspicious",
                MusicVolume = 0.4f,
                AmbienceVolume = 0.6f,
                SFXVolume = 1.2f,
                Description = "疑念状態 - 緊張感増加"
            },
            new AlertAudioConfiguration 
            { 
                AlertLevel = "Alert",
                MusicVolume = 0.8f,
                AmbienceVolume = 0.3f,
                SFXVolume = 1.5f,
                Description = "警戒状態 - 高緊張音響"
            }
        };

        [TabGroup("Dynamic", "Audio Ducking")]
        [Title("オーディオダッキング", "重要音の強調システム", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("オーディオダッキング有効化")]
        private bool enableAudioDucking = true;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("ダッキング強度")]
        private float duckingIntensity = 0.7f;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("ダッキング応答速度")]
        private float duckingSpeed = 0.5f;

        [SerializeField]
        [Tooltip("優先音源設定")]
        private AudioPriorityLevel[] audioPriorities = new AudioPriorityLevel[]
        {
            new AudioPriorityLevel { SoundType = "PlayerFootsteps", Priority = 1 },
            new AudioPriorityLevel { SoundType = "AIVoice", Priority = 2 },
            new AudioPriorityLevel { SoundType = "ImportantSFX", Priority = 3 },
            new AudioPriorityLevel { SoundType = "Music", Priority = 4 },
            new AudioPriorityLevel { SoundType = "Ambience", Priority = 5 }
        };

        #endregion

        #region Properties (Public API)

        // 3D Spatial Audio Properties
        public bool Enable3DSpatialAudio => enable3DSpatialAudio;
        public float MaxAudioDistance => maxAudioDistance;
        public float SoundSpeed => soundSpeed;
        public bool EnableDopplerEffect => enableDopplerEffect;
        public float DopplerScale => dopplerScale;
        public AnimationCurve DistanceAttenuationCurve => distanceAttenuationCurve;
        public FrequencyAttenuation[] FrequencyAttenuations => frequencyAttenuations;
        public float AirAbsorptionCoefficient => airAbsorptionCoefficient;

        // Environmental Masking Properties
        public bool EnableEnvironmentalMasking => enableEnvironmentalMasking;
        public float BaseEnvironmentNoiseLevel => baseEnvironmentNoiseLevel;
        public EnvironmentalAudioProfile[] EnvironmentProfiles => environmentProfiles;
        public bool EnableDynamicMasking => enableDynamicMasking;
        public float MaskingTransitionSpeed => maskingTransitionSpeed;
        public TimeBasedMasking[] TimeBasedMaskingSettings => timeBasedMasking;

        // Material Acoustic Properties
        public bool EnableMaterialAcoustics => enableMaterialAcoustics;
        public MaterialAcousticProfile[] MaterialProfiles => materialProfiles;
        public FootstepAudioProfile[] FootstepProfiles => footstepProfiles;

        // AI Auditory Detection Properties
        public bool EnableAIAuditoryDetection => enableAIAuditoryDetection;
        public float AIHearingSensitivityMultiplier => aiHearingSensitivityMultiplier;
        public AIAuditoryProfile[] AIAuditoryProfiles => aiAuditoryProfiles;
        public bool EnableSoundRecognition => enableSoundRecognition;
        public SoundCategory[] SoundCategories => soundCategories;

        // Dynamic Audio Properties
        public bool EnableAdaptiveAudio => enableAdaptiveAudio;
        public AlertAudioConfiguration[] AlertAudioConfigs => alertAudioConfigs;
        public bool EnableAudioDucking => enableAudioDucking;
        public float DuckingIntensity => duckingIntensity;
        public float DuckingSpeed => duckingSpeed;
        public AudioPriorityLevel[] AudioPriorities => audioPriorities;

        #endregion

        #region Audio Configuration Methods

        /// <summary>
        /// 環境プロファイルを取得
        /// </summary>
        public EnvironmentalAudioProfile GetEnvironmentProfile(string environmentName)
        {
            foreach (var profile in environmentProfiles)
            {
                if (profile.EnvironmentName == environmentName)
                    return profile;
            }
            return environmentProfiles[0]; // Default to first profile
        }

        /// <summary>
        /// 材質音響プロファイルを取得
        /// </summary>
        public MaterialAcousticProfile GetMaterialProfile(string materialName)
        {
            foreach (var profile in materialProfiles)
            {
                if (profile.MaterialName == materialName)
                    return profile;
            }
            return materialProfiles[0]; // Default to first profile
        }

        /// <summary>
        /// 足音プロファイルを取得
        /// </summary>
        public FootstepAudioProfile GetFootstepProfile(string surfaceType)
        {
            foreach (var profile in footstepProfiles)
            {
                if (profile.SurfaceType == surfaceType)
                    return profile;
            }
            return footstepProfiles[0]; // Default to first profile
        }

        /// <summary>
        /// AI聴覚プロファイルを取得
        /// </summary>
        public AIAuditoryProfile GetAIAuditoryProfile(string aiType)
        {
            foreach (var profile in aiAuditoryProfiles)
            {
                if (profile.AIType == aiType)
                    return profile;
            }
            return aiAuditoryProfiles[0]; // Default to first profile
        }

        /// <summary>
        /// 警戒レベル音響設定を取得
        /// </summary>
        public AlertAudioConfiguration GetAlertAudioConfig(string alertLevel)
        {
            foreach (var config in alertAudioConfigs)
            {
                if (config.AlertLevel == alertLevel)
                    return config;
            }
            return alertAudioConfigs[0]; // Default to first config
        }

        /// <summary>
        /// 音源カテゴリを取得
        /// </summary>
        public SoundCategory GetSoundCategory(string categoryName)
        {
            foreach (var category in soundCategories)
            {
                if (category.CategoryName == categoryName)
                    return category;
            }
            return soundCategories[0]; // Default to first category
        }

        /// <summary>
        /// StealthAudioCoordinatorに設定を適用
        /// </summary>
        public void ApplyToStealthAudioCoordinator(Component audioCoordinator)
        {
            if (audioCoordinator == null) return;
            
            Debug.Log($"Applied stealth audio settings to {audioCoordinator.name}");
        }

        #endregion

        #region Nested Data Classes

        [System.Serializable]
        public class FrequencyAttenuation
        {
            [Tooltip("周波数帯域")]
            public string FrequencyRange;
            
            [Range(0.1f, 3f)]
            [Tooltip("減衰倍率")]
            public float AttenuationMultiplier;
        }

        [System.Serializable]
        public class EnvironmentalAudioProfile
        {
            [Tooltip("環境名")]
            public string EnvironmentName;
            
            [Range(0f, 1f)]
            [Tooltip("マスキングレベル")]
            public float MaskingLevel;
            
            [Tooltip("特徴的な音")]
            public string[] CharacteristicSounds;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class TimeBasedMasking
        {
            [Tooltip("時間帯")]
            public string TimeOfDay;
            
            [Range(0.1f, 2f)]
            [Tooltip("マスキング倍率")]
            public float MaskingMultiplier;
        }

        [System.Serializable]
        public class MaterialAcousticProfile
        {
            [Tooltip("材質名")]
            public string MaterialName;
            
            [Range(0f, 1f)]
            [Tooltip("反射係数")]
            public float ReflectionCoefficient;
            
            [Range(0f, 1f)]
            [Tooltip("吸収係数")]
            public float AbsorptionCoefficient;
            
            [Range(0f, 1f)]
            [Tooltip("透過係数")]
            public float TransmissionCoefficient;
            
            [Tooltip("音響特性")]
            public string SoundCharacter;
        }

        [System.Serializable]
        public class FootstepAudioProfile
        {
            [Tooltip("表面タイプ")]
            public string SurfaceType;
            
            [Range(0f, 2f)]
            [Tooltip("基本音量")]
            public float BaseVolume;
            
            [Range(0f, 1f)]
            [Tooltip("ピッチ変動")]
            public float PitchVariation;
            
            [Range(1f, 20f)]
            [Tooltip("検知半径")]
            public float DetectionRadius;
            
            [Tooltip("オーディオクリップ名")]
            public string[] AudioClips;
        }

        [System.Serializable]
        public class AIAuditoryProfile
        {
            [Tooltip("AIタイプ")]
            public string AIType;
            
            [Range(1f, 30f)]
            [Tooltip("聴覚範囲")]
            public float HearingRange;
            
            [Tooltip("周波数感度 [Low, Mid, High]")]
            public float[] FrequencySensitivity = new float[3];
            
            [Range(0f, 1f)]
            [Tooltip("疑心増加量")]
            public float SuspicionIncrement;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class SoundCategory
        {
            [Tooltip("カテゴリ名")]
            public string CategoryName;
            
            [Range(0f, 1f)]
            [Tooltip("脅威レベル")]
            public float ThreatLevel;
            
            [Range(0f, 1f)]
            [Tooltip("認識精度")]
            public float RecognitionAccuracy;
            
            [Range(0f, 1f)]
            [Tooltip("必要音量")]
            public float RequiredVolume;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class AlertAudioConfiguration
        {
            [Tooltip("警戒レベル")]
            public string AlertLevel;
            
            [Range(0f, 1f)]
            [Tooltip("音楽音量")]
            public float MusicVolume;
            
            [Range(0f, 1f)]
            [Tooltip("環境音音量")]
            public float AmbienceVolume;
            
            [Range(0f, 2f)]
            [Tooltip("効果音音量")]
            public float SFXVolume;
            
            [Tooltip("説明")]
            public string Description;
        }

        [System.Serializable]
        public class AudioPriorityLevel
        {
            [Tooltip("音源タイプ")]
            public string SoundType;
            
            [Range(1, 10)]
            [Tooltip("優先度（低いほど高優先）")]
            public int Priority;
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug", "Debug Actions")]
        [Button("Test Environmental Masking")]
        public void TestEnvironmentalMasking()
        {
            Debug.Log("=== Environmental Masking Test ===");
            foreach (var profile in environmentProfiles)
            {
                Debug.Log($"Environment: {profile.EnvironmentName}");
                Debug.Log($"  Masking Level: {profile.MaskingLevel:F2}");
                Debug.Log($"  Sounds: {string.Join(", ", profile.CharacteristicSounds)}");
            }
        }

        [Button("Test Material Acoustics")]
        public void TestMaterialAcoustics()
        {
            Debug.Log("=== Material Acoustics Test ===");
            foreach (var material in materialProfiles)
            {
                Debug.Log($"Material: {material.MaterialName}");
                Debug.Log($"  Reflection: {material.ReflectionCoefficient:F2}");
                Debug.Log($"  Absorption: {material.AbsorptionCoefficient:F2}");
                Debug.Log($"  Character: {material.SoundCharacter}");
            }
        }

        [Button("Test AI Auditory Profiles")]
        public void TestAIAuditoryProfiles()
        {
            Debug.Log("=== AI Auditory Profiles Test ===");
            foreach (var profile in aiAuditoryProfiles)
            {
                Debug.Log($"AI Type: {profile.AIType}");
                Debug.Log($"  Range: {profile.HearingRange}m");
                Debug.Log($"  Sensitivity: L{profile.FrequencySensitivity[0]:F1} M{profile.FrequencySensitivity[1]:F1} H{profile.FrequencySensitivity[2]:F1}");
                Debug.Log($"  Suspicion: +{profile.SuspicionIncrement:F2}");
            }
        }

        [Button("Print Audio Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Stealth Audio Configuration ===");
            Debug.Log($"3D Spatial Audio: {enable3DSpatialAudio}");
            Debug.Log($"Max Audio Distance: {maxAudioDistance}m");
            Debug.Log($"Environmental Masking: {enableEnvironmentalMasking}");
            Debug.Log($"Material Acoustics: {enableMaterialAcoustics}");
            Debug.Log($"AI Auditory Detection: {enableAIAuditoryDetection}");
            Debug.Log($"Adaptive Audio: {enableAdaptiveAudio}");
            Debug.Log($"Audio Ducking: {enableAudioDucking}");
        }
#endif

        #endregion
    }
}