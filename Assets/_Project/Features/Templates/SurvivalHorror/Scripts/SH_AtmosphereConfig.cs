using UnityEngine;
using UnityEngine.Rendering;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorの雰囲気・心理的恐怖演出を管理するScriptableObject
    /// 照明、フォグ、環境音、視覚効果を統合制御して恐怖感を演出
    /// </summary>
    [CreateAssetMenu(fileName = "SH_AtmosphereConfig", menuName = "Templates/SurvivalHorror/Atmosphere Config")]
    public class SH_AtmosphereConfig : ScriptableObject
    {
        [Header("Lighting Configuration")]
        [SerializeField] private LightingPreset normalLighting;
        [SerializeField] private LightingPreset tenseLighting;
        [SerializeField] private LightingPreset fearLighting;
        [SerializeField] private LightingPreset terrorLighting;

        [Header("Fog & Atmosphere")]
        [SerializeField] private FogSettings normalFog;
        [SerializeField] private FogSettings tenseFog;
        [SerializeField] private FogSettings fearFog;
        [SerializeField] private FogSettings terrorFog;

        [Header("Post-Processing Effects")]
        [SerializeField] private VolumeProfile normalPostProfile;
        [SerializeField] private VolumeProfile lowSanityPostProfile;
        [SerializeField] private VolumeProfile criticalSanityPostProfile;
        [SerializeField] private VolumeProfile stalkerNearbyPostProfile;

        [Header("Audio Environment")]
        [SerializeField] private AudioEnvironmentSettings normalAudio;
        [SerializeField] private AudioEnvironmentSettings tenseAudio;
        [SerializeField] private AudioEnvironmentSettings fearAudio;
        [SerializeField] private AudioEnvironmentSettings terrorAudio;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem dustParticlesPrefab;
        [SerializeField] private ParticleSystem bloodDropsPrefab;
        [SerializeField] private ParticleSystem hallucinationEffectPrefab;
        [SerializeField] private Material flickeringLightMaterial;

        [Header("Transition Settings")]
        [SerializeField] private float atmosphereTransitionSpeed = 2.0f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool allowInstantTransitions = false;

        [Header("Sanity-Based Effects")]
        [SerializeField] private SanityVisualEffects sanityEffects;

        [Header("Events")]
        [SerializeField] private GameEvent<AtmosphereState> onAtmosphereChanged;
        [SerializeField] private GameEvent<float> onSanityVisualUpdate;

        // Runtime State
        private AtmosphereState currentState = AtmosphereState.Normal;
        private float currentTransitionProgress = 1.0f;
        private AtmosphereState targetState = AtmosphereState.Normal;

        public AtmosphereState CurrentState => currentState;
        public float TransitionProgress => currentTransitionProgress;

        /// <summary>
        /// 雰囲気状態を変更
        /// </summary>
        public void SetAtmosphereState(AtmosphereState newState, bool instant = false)
        {
            if (currentState == newState && currentTransitionProgress >= 1.0f) return;

            targetState = newState;

            if (instant || allowInstantTransitions)
            {
                ApplyAtmosphereInstant(newState);
            }
            else
            {
                StartAtmosphereTransition(newState);
            }

            onAtmosphereChanged?.Raise(newState);
            Debug.Log($"[SH_AtmosphereConfig] Atmosphere changing to {newState}");
        }

        /// <summary>
        /// 正気度レベルに基づいて視覚効果を適用
        /// </summary>
        public void ApplySanityEffects(float sanityNormalized)
        {
            if (sanityEffects == null) return;

            onSanityVisualUpdate?.Raise(sanityNormalized);

            // 正気度レベルに応じたPost-Processingプロファイル選択
            VolumeProfile targetProfile = sanityNormalized switch
            {
                > 0.7f => normalPostProfile,
                > 0.3f => lowSanityPostProfile,
                > 0.1f => criticalSanityPostProfile,
                _ => criticalSanityPostProfile
            };

            ApplyPostProcessingProfile(targetProfile);
        }

        /// <summary>
        /// ストーカーAI接近時の演出を適用
        /// </summary>
        public void OnStalkerProximity(float proximityNormalized)
        {
            if (proximityNormalized > 0.8f)
            {
                // 極度に近い：恐怖状態に強制変更
                SetAtmosphereState(AtmosphereState.Terror);
                ApplyPostProcessingProfile(stalkerNearbyPostProfile);
            }
            else if (proximityNormalized > 0.5f)
            {
                // やや近い：緊張状態
                SetAtmosphereState(AtmosphereState.Fear);
            }
            else if (proximityNormalized > 0.2f)
            {
                // 警戒範囲：注意状態
                SetAtmosphereState(AtmosphereState.Tense);
            }
        }

        /// <summary>
        /// 環境ベースの雰囲気変更（部屋の種類、時間等）
        /// </summary>
        public void SetEnvironmentalAtmosphere(EnvironmentType environmentType)
        {
            AtmosphereState atmosphereState = environmentType switch
            {
                EnvironmentType.SafeRoom => AtmosphereState.Normal,
                EnvironmentType.Corridor => AtmosphereState.Tense,
                EnvironmentType.DarkRoom => AtmosphereState.Fear,
                EnvironmentType.BasementDungeon => AtmosphereState.Terror,
                _ => AtmosphereState.Normal
            };

            SetAtmosphereState(atmosphereState);
        }

        /// <summary>
        /// 現在の状態に対応する照明設定を取得
        /// </summary>
        public LightingPreset GetCurrentLightingPreset()
        {
            return currentState switch
            {
                AtmosphereState.Normal => normalLighting,
                AtmosphereState.Tense => tenseLighting,
                AtmosphereState.Fear => fearLighting,
                AtmosphereState.Terror => terrorLighting,
                _ => normalLighting
            };
        }

        /// <summary>
        /// 現在の状態に対応するフォグ設定を取得
        /// </summary>
        public FogSettings GetCurrentFogSettings()
        {
            return currentState switch
            {
                AtmosphereState.Normal => normalFog,
                AtmosphereState.Tense => tenseFog,
                AtmosphereState.Fear => fearFog,
                AtmosphereState.Terror => terrorFog,
                _ => normalFog
            };
        }

        /// <summary>
        /// 現在の状態に対応するオーディオ設定を取得
        /// </summary>
        public AudioEnvironmentSettings GetCurrentAudioSettings()
        {
            return currentState switch
            {
                AtmosphereState.Normal => normalAudio,
                AtmosphereState.Tense => tenseAudio,
                AtmosphereState.Fear => fearAudio,
                AtmosphereState.Terror => terrorAudio,
                _ => normalAudio
            };
        }

        /// <summary>
        /// 雰囲気を即座に変更
        /// </summary>
        private void ApplyAtmosphereInstant(AtmosphereState state)
        {
            currentState = state;
            currentTransitionProgress = 1.0f;

            // 実際の環境適用は AtmosphereManager が実行
        }

        /// <summary>
        /// 雰囲気の段階的変更を開始
        /// </summary>
        private void StartAtmosphereTransition(AtmosphereState state)
        {
            currentTransitionProgress = 0.0f;
            // 実際のトランジション制御は AtmosphereManager が実行
        }

        /// <summary>
        /// Post-Processingプロファイルを適用
        /// </summary>
        private void ApplyPostProcessingProfile(VolumeProfile profile)
        {
            if (profile == null) return;

            // 実際の適用は AtmosphereManager が Volume コンポーネントを通じて実行
        }

        /// <summary>
        /// 設定の整合性を検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (normalLighting == null)
            {
                Debug.LogError("[SH_AtmosphereConfig] Normal lighting preset is missing");
                isValid = false;
            }

            if (atmosphereTransitionSpeed <= 0)
            {
                Debug.LogError("[SH_AtmosphereConfig] Atmosphere transition speed must be positive");
                isValid = false;
            }

            return isValid;
        }
    }

    /// <summary>
    /// 雰囲気状態の定義
    /// </summary>
    public enum AtmosphereState
    {
        Normal,  // 通常状態：明るめ、安全
        Tense,   // 緊張状態：やや薄暗く、警戒
        Fear,    // 恐怖状態：暗く、不安
        Terror   // 恐怖状態：極暗、パニック
    }

    /// <summary>
    /// 環境タイプ定義
    /// </summary>
    public enum EnvironmentType
    {
        SafeRoom,        // セーブルーム等の安全地帯
        Corridor,        // 廊下・通路
        DarkRoom,        // 暗い部屋
        BasementDungeon  // 地下・ダンジョン
    }

    /// <summary>
    /// 照明設定
    /// </summary>
    [System.Serializable]
    public class LightingPreset
    {
        [Header("Main Lighting")]
        public Color ambientLight = Color.gray;
        public float ambientIntensity = 1.0f;
        public Color directionalLightColor = Color.white;
        public float directionalLightIntensity = 1.0f;

        [Header("Shadow Settings")]
        [Range(0f, 1f)] public float shadowStrength = 0.8f;
        public LightShadows shadowType = LightShadows.Soft;

        [Header("Mood")]
        public Color fogColor = Color.gray;
        public AnimationCurve lightFlickerCurve;
        public float flickerSpeed = 1.0f;
    }

    /// <summary>
    /// フォグ設定
    /// </summary>
    [System.Serializable]
    public class FogSettings
    {
        public bool enableFog = true;
        public Color fogColor = Color.gray;
        [Range(0f, 1f)] public float fogDensity = 0.02f;
        public float fogStartDistance = 10f;
        public float fogEndDistance = 50f;
        public FogMode fogMode = FogMode.ExponentialSquared;
    }

    /// <summary>
    /// オーディオ環境設定
    /// </summary>
    [System.Serializable]
    public class AudioEnvironmentSettings
    {
        [Header("Ambient Sounds")]
        public AudioClip ambientLoop;
        [Range(0f, 1f)] public float ambientVolume = 0.3f;

        [Header("Tension Music")]
        public AudioClip backgroundMusic;
        [Range(0f, 1f)] public float musicVolume = 0.2f;

        [Header("Atmospheric Effects")]
        public AudioClip[] randomAtmosphericSounds;
        public float atmosphericSoundInterval = 30f;
        [Range(0f, 1f)] public float atmosphericVolume = 0.4f;

        [Header("Audio Processing")]
        public bool enableReverb = true;
        public float reverbIntensity = 0.5f;
        public bool enableLowPassFilter = false;
        public float lowPassCutoff = 5000f;
    }

    /// <summary>
    /// 正気度ベースの視覚効果設定
    /// </summary>
    [System.Serializable]
    public class SanityVisualEffects
    {
        [Header("Screen Distortion")]
        public AnimationCurve distortionIntensityCurve;
        public float maxDistortionAmount = 0.1f;

        [Header("Color Grading")]
        public Gradient sanityColorGrading;
        public AnimationCurve saturationCurve;

        [Header("Vignette Effect")]
        public AnimationCurve vignetteIntensityCurve;
        public Color vignetteColor = Color.black;

        [Header("Hallucination Effects")]
        public ParticleSystem[] hallucinationPrefabs;
        public float hallucinationTriggerThreshold = 0.3f;
        public float hallucinationDuration = 2.0f;
    }
}