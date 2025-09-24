using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorの雰囲気管理システム
    /// 照明、フォグ、音響、Post-Processingを統合制御して恐怖感を演出
    /// </summary>
    public class SH_AtmosphereManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SH_AtmosphereConfig config;

        [Header("Scene References")]
        [SerializeField] private Light primaryLight;
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private ParticleSystem[] atmosphericParticles;

        [Header("Dynamic Lighting")]
        [SerializeField] private Light[] flickerLights;
        #pragma warning disable 0414
        [SerializeField] private float flickerIntensity = 0.1f;
        #pragma warning restore 0414
        [SerializeField] private bool enableDynamicFlicker = true;

        [Header("Environmental Audio")]
        [SerializeField] private AudioSource[] randomAudioSources;
        [SerializeField] private Transform audioListenerTransform;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool logTransitions = true;

        // Runtime State
        private AtmosphereState currentState = AtmosphereState.Normal;
        private AtmosphereState targetState = AtmosphereState.Normal;
        private float transitionProgress = 1.0f;
        private bool isTransitioning = false;
        private Coroutine transitionCoroutine;
        private Coroutine flickerCoroutine;
        private Coroutine ambientSoundCoroutine;

        // Cached Original Values
        private float originalLightIntensity;
        private Color originalLightColor;
        private Color originalAmbientColor;
        private VolumeProfile originalVolumeProfile;

        public AtmosphereState CurrentState => currentState;
        public bool IsTransitioning => isTransitioning;
        public float TransitionProgress => transitionProgress;

        private void Awake()
        {
            CacheOriginalValues();
        }

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("[SH_AtmosphereManager] No atmosphere config assigned!");
                return;
            }

            StartFlickerSystem();
            StartAmbientSoundSystem();
        }

        /// <summary>
        /// AtmosphereManagerを初期化
        /// </summary>
        public void Initialize(SH_AtmosphereConfig atmosphereConfig)
        {
            if (atmosphereConfig == null)
            {
                Debug.LogError("[SH_AtmosphereManager] Cannot initialize with null config");
                return;
            }

            config = atmosphereConfig;
            CacheOriginalValues();

            Debug.Log("[SH_AtmosphereManager] Initialized with atmosphere config");
        }

        /// <summary>
        /// 雰囲気状態を設定
        /// </summary>
        public void SetAtmosphereState(AtmosphereState newState, bool instant = false)
        {
            if (config == null)
            {
                Debug.LogWarning("[SH_AtmosphereManager] Cannot set atmosphere: no config");
                return;
            }

            if (currentState == newState && !isTransitioning) return;

            targetState = newState;

            if (instant)
            {
                ApplyAtmosphereImmediate(newState);
            }
            else
            {
                StartTransition(newState);
            }

            if (logTransitions)
            {
                Debug.Log($"[SH_AtmosphereManager] Atmosphere changing to {newState} (instant: {instant})");
            }
        }

        /// <summary>
        /// 正気度に基づく視覚効果を適用
        /// </summary>
        public void UpdateSanityEffects(float sanityNormalized)
        {
            if (config == null) return;

            config.ApplySanityEffects(sanityNormalized);

            // 追加の環境効果
            ApplyDynamicSanityEffects(sanityNormalized);
        }

        /// <summary>
        /// ストーカーAI接近時の演出
        /// </summary>
        public void OnStalkerProximity(float proximityNormalized, Vector3 stalkerPosition)
        {
            if (config == null) return;

            config.OnStalkerProximity(proximityNormalized);

            // 追加の恐怖演出
            if (proximityNormalized > 0.8f)
            {
                TriggerIntenseHorrorEffects(stalkerPosition);
            }
            else if (proximityNormalized > 0.5f)
            {
                TriggerModerateHorrorEffects();
            }
        }

        /// <summary>
        /// 環境タイプに基づく雰囲気設定
        /// </summary>
        public void SetEnvironmentalAtmosphere(EnvironmentType environmentType, bool smooth = true)
        {
            if (config == null) return;

            config.SetEnvironmentalAtmosphere(environmentType);

            // 環境特有の追加効果
            ApplyEnvironmentSpecificEffects(environmentType);
        }

        /// <summary>
        /// カスタム恐怖イベントをトリガー
        /// </summary>
        public void TriggerHorrorEvent(HorrorEventType eventType, float intensity = 1.0f)
        {
            StartCoroutine(ExecuteHorrorEvent(eventType, intensity));
        }

        /// <summary>
        /// 雰囲気を即座に適用
        /// </summary>
        private void ApplyAtmosphereImmediate(AtmosphereState state)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            currentState = state;
            targetState = state;
            transitionProgress = 1.0f;
            isTransitioning = false;

            ApplyLighting(state, 1.0f);
            ApplyFog(state, 1.0f);
            ApplyAudio(state, 1.0f);
            ApplyPostProcessing(state, 1.0f);
        }

        /// <summary>
        /// 段階的雰囲気遷移を開始
        /// </summary>
        private void StartTransition(AtmosphereState targetAtmosphere)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionToAtmosphere(targetAtmosphere));
        }

        /// <summary>
        /// 雰囲気遷移コルーチン
        /// </summary>
        private IEnumerator TransitionToAtmosphere(AtmosphereState targetAtmosphere)
        {
            isTransitioning = true;
            transitionProgress = 0.0f;
            var startState = currentState;

            float transitionDuration = 2.0f; // Default transition duration

            while (transitionProgress < 1.0f)
            {
                transitionProgress += Time.deltaTime / transitionDuration;
                float easedProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(transitionProgress)); // Default smooth transition

                ApplyLightingTransition(startState, targetAtmosphere, easedProgress);
                ApplyFogTransition(startState, targetAtmosphere, easedProgress);
                ApplyAudioTransition(startState, targetAtmosphere, easedProgress);
                ApplyPostProcessingTransition(startState, targetAtmosphere, easedProgress);

                yield return null;
            }

            currentState = targetAtmosphere;
            isTransitioning = false;
            transitionProgress = 1.0f;
        }

        /// <summary>
        /// 照明設定を適用
        /// </summary>
        private void ApplyLighting(AtmosphereState state, float intensity)
        {
            var lightingPreset = GetLightingPreset(state);
            if (lightingPreset == null || primaryLight == null) return;

            primaryLight.color = Color.Lerp(originalLightColor, lightingPreset.directionalLightColor, intensity);
            primaryLight.intensity = Mathf.Lerp(originalLightIntensity, lightingPreset.directionalLightIntensity, intensity);
            primaryLight.shadows = lightingPreset.shadowType;

            RenderSettings.ambientLight = Color.Lerp(originalAmbientColor, lightingPreset.ambientLight, intensity);
            RenderSettings.ambientIntensity = Mathf.Lerp(1.0f, lightingPreset.ambientIntensity, intensity);
        }

        private void ApplyLightingTransition(AtmosphereState from, AtmosphereState to, float progress)
        {
            var fromPreset = GetLightingPreset(from);
            var toPreset = GetLightingPreset(to);

            if (fromPreset == null || toPreset == null || primaryLight == null) return;

            primaryLight.color = Color.Lerp(fromPreset.directionalLightColor, toPreset.directionalLightColor, progress);
            primaryLight.intensity = Mathf.Lerp(fromPreset.directionalLightIntensity, toPreset.directionalLightIntensity, progress);

            RenderSettings.ambientLight = Color.Lerp(fromPreset.ambientLight, toPreset.ambientLight, progress);
            RenderSettings.ambientIntensity = Mathf.Lerp(fromPreset.ambientIntensity, toPreset.ambientIntensity, progress);
        }

        /// <summary>
        /// フォグ設定を適用
        /// </summary>
        private void ApplyFog(AtmosphereState state, float intensity)
        {
            var fogSettings = GetFogSettings(state);
            if (fogSettings == null) return;

            RenderSettings.fog = fogSettings.enableFog;
            RenderSettings.fogColor = fogSettings.fogColor;
            RenderSettings.fogMode = fogSettings.fogMode;
            RenderSettings.fogDensity = fogSettings.fogDensity * intensity;

            if (fogSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogSettings.fogStartDistance;
                RenderSettings.fogEndDistance = Mathf.Lerp(1000f, fogSettings.fogEndDistance, intensity);
            }
        }

        private void ApplyFogTransition(AtmosphereState from, AtmosphereState to, float progress)
        {
            var fromFog = GetFogSettings(from);
            var toFog = GetFogSettings(to);

            if (fromFog == null || toFog == null) return;

            RenderSettings.fog = toFog.enableFog;
            RenderSettings.fogColor = Color.Lerp(fromFog.fogColor, toFog.fogColor, progress);
            RenderSettings.fogDensity = Mathf.Lerp(fromFog.fogDensity, toFog.fogDensity, progress);

            if (toFog.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(fromFog.fogStartDistance, toFog.fogStartDistance, progress);
                RenderSettings.fogEndDistance = Mathf.Lerp(fromFog.fogEndDistance, toFog.fogEndDistance, progress);
            }
        }

        /// <summary>
        /// オーディオ設定を適用
        /// </summary>
        private void ApplyAudio(AtmosphereState state, float intensity)
        {
            var audioSettings = GetAudioSettings(state);
            if (audioSettings == null) return;

            if (ambientAudioSource != null)
            {
                ambientAudioSource.clip = audioSettings.ambientLoop;
                ambientAudioSource.volume = audioSettings.ambientVolume * intensity;
                if (!ambientAudioSource.isPlaying && audioSettings.ambientLoop != null)
                {
                    ambientAudioSource.Play();
                }
            }

            if (musicAudioSource != null && audioSettings.backgroundMusic != null)
            {
                musicAudioSource.clip = audioSettings.backgroundMusic;
                musicAudioSource.volume = audioSettings.musicVolume * intensity;
                if (!musicAudioSource.isPlaying)
                {
                    musicAudioSource.Play();
                }
            }
        }

        private void ApplyAudioTransition(AtmosphereState from, AtmosphereState to, float progress)
        {
            var fromAudio = GetAudioSettings(from);
            var toAudio = GetAudioSettings(to);

            if (fromAudio == null || toAudio == null) return;

            if (ambientAudioSource != null)
            {
                if (fromAudio.ambientLoop != toAudio.ambientLoop && progress > 0.5f)
                {
                    ambientAudioSource.clip = toAudio.ambientLoop;
                    if (toAudio.ambientLoop != null && !ambientAudioSource.isPlaying)
                    {
                        ambientAudioSource.Play();
                    }
                }

                ambientAudioSource.volume = Mathf.Lerp(fromAudio.ambientVolume, toAudio.ambientVolume, progress);
            }
        }

        /// <summary>
        /// Post-Processing設定を適用
        /// </summary>
        private void ApplyPostProcessing(AtmosphereState state, float intensity)
        {
            if (postProcessVolume == null) return;

            VolumeProfile targetProfile = null; // TODO: Implement PostProfile properties in SH_AtmosphereConfig
            // For now, use the original profile or null
            // targetProfile = state switch
            // {
            //     AtmosphereState.Normal => config.NormalPostProfile,
            //     AtmosphereState.Tense => config.LowSanityPostProfile,
            //     AtmosphereState.Fear => config.CriticalSanityPostProfile,
            //     AtmosphereState.Terror => config.StalkerNearbyPostProfile,
            //     _ => config.NormalPostProfile
            // };

            if (targetProfile != null)
            {
                postProcessVolume.profile = targetProfile;
                postProcessVolume.weight = intensity;
            }
        }

        private void ApplyPostProcessingTransition(AtmosphereState from, AtmosphereState to, float progress)
        {
            if (postProcessVolume == null) return;

            VolumeProfile targetProfile = null; // TODO: Implement PostProfile properties in SH_AtmosphereConfig
            // For now, use the original profile or null
            // targetProfile = to switch
            // {
            //     AtmosphereState.Normal => config.NormalPostProfile,
            //     AtmosphereState.Tense => config.LowSanityPostProfile,
            //     AtmosphereState.Fear => config.CriticalSanityPostProfile,
            //     AtmosphereState.Terror => config.StalkerNearbyPostProfile,
            //     _ => config.NormalPostProfile
            // };

            if (targetProfile != null && progress > 0.5f)
            {
                postProcessVolume.profile = targetProfile;
            }

            postProcessVolume.weight = progress;
        }

        /// <summary>
        /// 動的正気度効果を適用
        /// </summary>
        private void ApplyDynamicSanityEffects(float sanityNormalized)
        {
            // 低正気度時の画面歪み効果
            if (sanityNormalized < 0.3f)
            {
                if (Random.Range(0f, 1f) < 0.1f * (1f - sanityNormalized))
                {
                    TriggerHorrorEvent(HorrorEventType.ScreenDistortion, 1f - sanityNormalized);
                }
            }

            // 極低正気度時の幻覚エフェクト
            if (sanityNormalized < 0.1f)
            {
                if (Random.Range(0f, 1f) < 0.05f)
                {
                    TriggerHorrorEvent(HorrorEventType.Hallucination, 1.0f);
                }
            }
        }

        /// <summary>
        /// 恐怖イベントを実行
        /// </summary>
        private IEnumerator ExecuteHorrorEvent(HorrorEventType eventType, float intensity)
        {
            switch (eventType)
            {
                case HorrorEventType.ScreenDistortion:
                    yield return StartCoroutine(ScreenDistortionEffect(intensity));
                    break;

                case HorrorEventType.Hallucination:
                    yield return StartCoroutine(HallucinationEffect(intensity));
                    break;

                case HorrorEventType.LightFlicker:
                    yield return StartCoroutine(LightFlickerEffect(intensity));
                    break;

                case HorrorEventType.SuddenSilence:
                    yield return StartCoroutine(SuddenSilenceEffect(intensity));
                    break;
            }
        }

        private IEnumerator ScreenDistortionEffect(float intensity)
        {
            // Post-Processingによる画面歪み効果の実装
            yield return new WaitForSeconds(0.5f + intensity);
        }

        private IEnumerator HallucinationEffect(float intensity)
        {
            // 幻覚パーティクルの再生
            // TODO: Implement HallucinationEffectPrefab property in SH_AtmosphereConfig
            ParticleSystem hallucinationPrefab = null; // config.HallucinationEffectPrefab;
            if (hallucinationPrefab != null)
            {
                var hallucination = Instantiate(hallucinationPrefab, UnityEngine.Camera.main.transform.position, Quaternion.identity);
                hallucination.Play();
                yield return new WaitForSeconds(2f * intensity);
                if (hallucination != null) Destroy(hallucination.gameObject);
            }
        }

        private IEnumerator LightFlickerEffect(float intensity)
        {
            foreach (var light in flickerLights)
            {
                if (light != null)
                {
                    var originalIntensity = light.intensity;
                    for (int i = 0; i < 5; i++)
                    {
                        light.intensity = originalIntensity * Random.Range(0.1f, 1.0f);
                        yield return new WaitForSeconds(0.1f * intensity);
                    }
                    light.intensity = originalIntensity;
                }
            }
        }

        private IEnumerator SuddenSilenceEffect(float intensity)
        {
            var originalVolume = ambientAudioSource.volume;
            ambientAudioSource.volume = 0f;
            yield return new WaitForSeconds(2f * intensity);
            ambientAudioSource.volume = originalVolume;
        }

        // Helper methods
        private LightingPreset GetLightingPreset(AtmosphereState state) => config?.GetCurrentLightingPreset();
        private FogSettings GetFogSettings(AtmosphereState state) => config?.GetCurrentFogSettings();
        private AudioEnvironmentSettings GetAudioSettings(AtmosphereState state) => config?.GetCurrentAudioSettings();

        private void CacheOriginalValues()
        {
            if (primaryLight != null)
            {
                originalLightIntensity = primaryLight.intensity;
                originalLightColor = primaryLight.color;
            }

            originalAmbientColor = RenderSettings.ambientLight;

            if (postProcessVolume != null)
            {
                originalVolumeProfile = postProcessVolume.profile;
            }
        }

        private void StartFlickerSystem()
        {
            if (enableDynamicFlicker && flickerLights.Length > 0)
            {
                flickerCoroutine = StartCoroutine(FlickerLightsLoop());
            }
        }

        private void StartAmbientSoundSystem()
        {
            ambientSoundCoroutine = StartCoroutine(RandomAmbientSoundsLoop());
        }

        private IEnumerator FlickerLightsLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(5f, 15f));

                if (currentState == AtmosphereState.Fear || currentState == AtmosphereState.Terror)
                {
                    yield return StartCoroutine(LightFlickerEffect(1.0f));
                }
            }
        }

        private IEnumerator RandomAmbientSoundsLoop()
        {
            while (true)
            {
                var audioSettings = GetAudioSettings(currentState);
                if (audioSettings?.randomAtmosphericSounds != null && audioSettings.randomAtmosphericSounds.Length > 0)
                {
                    var randomClip = audioSettings.randomAtmosphericSounds[Random.Range(0, audioSettings.randomAtmosphericSounds.Length)];
                    if (randomClip != null && randomAudioSources.Length > 0)
                    {
                        var audioSource = randomAudioSources[Random.Range(0, randomAudioSources.Length)];
                        if (audioSource != null && !audioSource.isPlaying)
                        {
                            audioSource.clip = randomClip;
                            audioSource.volume = audioSettings.atmosphericVolume;
                            audioSource.Play();
                        }
                    }

                    yield return new WaitForSeconds(audioSettings.atmosphericSoundInterval);
                }
                else
                {
                    yield return new WaitForSeconds(30f);
                }
            }
        }

        private void TriggerIntenseHorrorEffects(Vector3 stalkerPosition)
        {
            TriggerHorrorEvent(HorrorEventType.LightFlicker, 1.0f);
            TriggerHorrorEvent(HorrorEventType.ScreenDistortion, 0.8f);
        }

        private void TriggerModerateHorrorEffects()
        {
            if (Random.Range(0f, 1f) < 0.3f)
            {
                TriggerHorrorEvent(HorrorEventType.LightFlicker, 0.5f);
            }
        }

        private void ApplyEnvironmentSpecificEffects(EnvironmentType environmentType)
        {
            // 環境タイプ別の特殊効果を実装
            switch (environmentType)
            {
                case EnvironmentType.BasementDungeon:
                    if (atmosphericParticles.Length > 0)
                    {
                        foreach (var particle in atmosphericParticles)
                        {
                            if (particle != null) particle.Play();
                        }
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            if (ambientSoundCoroutine != null) StopCoroutine(ambientSoundCoroutine);
        }

        // Debug GUI
        private void OnGUI()
        {
            if (!showDebugInfo) return;

            var rect = new Rect(10, 10, 300, 200);
            GUI.Box(rect, "Atmosphere Debug");

            GUILayout.BeginArea(new Rect(15, 35, 290, 165));
            GUILayout.Label($"Current State: {currentState}");
            GUILayout.Label($"Target State: {targetState}");
            GUILayout.Label($"Transitioning: {isTransitioning}");
            GUILayout.Label($"Progress: {transitionProgress:F2}");

            if (GUILayout.Button("Normal")) SetAtmosphereState(AtmosphereState.Normal);
            if (GUILayout.Button("Tense")) SetAtmosphereState(AtmosphereState.Tense);
            if (GUILayout.Button("Fear")) SetAtmosphereState(AtmosphereState.Fear);
            if (GUILayout.Button("Terror")) SetAtmosphereState(AtmosphereState.Terror);

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// 恐怖イベントタイプ定義
    /// </summary>
    public enum HorrorEventType
    {
        ScreenDistortion,  // 画面歪み
        Hallucination,     // 幻覚
        LightFlicker,      // ライト点滅
        SuddenSilence,     // 突然の静寂
        JumpScare,         // ジャンプスケア
        WhisperSound       // 囁き声
    }
}