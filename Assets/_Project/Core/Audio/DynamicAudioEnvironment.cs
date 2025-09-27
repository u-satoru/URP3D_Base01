using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Data;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 動的オーディオ環境管理クラス
    /// </summary>
    public class DynamicAudioEnvironment : MonoBehaviour
    {
        [Header("Environment State")]
        [SerializeField] private EnvironmentType currentEnvironment = EnvironmentType.Indoor;
        [SerializeField] private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
        [SerializeField] private WeatherType currentWeather = WeatherType.Clear;

        [Header("Audio Mixer Integration")]
        [SerializeField] private AudioMixer environmentMixer;
        [SerializeField] private string ambientVolumeParam = "AmbientVolume";
        [SerializeField] private string reverbParam = "ReverbWetMix";
        [SerializeField] private string lowPassParam = "LowPassFreq";

        [Header("Masking Effects")]
        [SerializeField] private float baseAmbientMasking = 0.1f;
        [SerializeField] private AnimationCurve maskingByWeather = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.8f);
        [SerializeField] private AnimationCurve maskingByTimeOfDay = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 0.05f);

        [Header("Environment Presets")]
        [SerializeField] private EnvironmentPreset[] environmentPresets;

        [Header("Dynamic Weather")]
        [SerializeField] private bool enableDynamicWeather = true;
        [SerializeField] private float weatherChangeInterval = 60f; // 遷移間隔
        [SerializeField] private AudioEvent weatherChangeEvent;

        [Header("Ambient Sound Sources")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private SoundDataSO[] dayAmbientSounds;
        [SerializeField] private SoundDataSO[] nightAmbientSounds;
        [SerializeField] private SoundDataSO[] rainAmbientSounds;

        // 現在の環境プリセット
        private EnvironmentPreset activePreset;
        private float currentMaskingLevel;
        private float weatherChangeTimer;

        // 環境遷移用コルーチン
        private Coroutine environmentTransition;

        // AudioListenerのTransform
        private Transform audioListener;

        // 環境変化通知用イベント
        public System.Action<EnvironmentType> OnEnvironmentChanged;
        public System.Action<WeatherType> OnWeatherChanged;
        public System.Action<TimeOfDay> OnTimeOfDayChanged;

        #region Unity Lifecycle

        private void Awake()
        {
            FindAudioListener();
            InitializeEnvironment();
        }

        private void Start()
        {
            ApplyEnvironmentSettings();
            StartAmbientSounds();
        }

        private void Update()
        {
            if (enableDynamicWeather)
            {
                UpdateDynamicWeather();
            }

            UpdateMaskingLevels();
            UpdateEnvironmentBasedOnPosition();
        }

        #endregion

        #region Environment Management

        /// <summary>
        /// 環境の初期化
        /// </summary>
        private void InitializeEnvironment()
        {
            activePreset = GetPresetForEnvironment(currentEnvironment);
        }

        /// <summary>
        /// 環境設定の適用
        /// </summary>
        private void ApplyEnvironmentSettings()
        {
            if (environmentMixer == null) return;

            // Audio Mixerに環境設定を適用
            environmentMixer.SetFloat(ambientVolumeParam, activePreset.ambientVolume);
            environmentMixer.SetFloat(reverbParam, activePreset.reverbLevel);
            environmentMixer.SetFloat(lowPassParam, activePreset.lowPassFrequency);

            // マスキングレベルの更新
            UpdateMaskingLevels();
        }

        /// <summary>
        /// マスキングレベルの更新
        /// </summary>
        private void UpdateMaskingLevels()
        {
            float weatherMasking = maskingByWeather.Evaluate((float)currentWeather / 3f);
            float timeMasking = maskingByTimeOfDay.Evaluate((float)currentTimeOfDay / 3f);
            float environmentMasking = activePreset.maskingMultiplier;

            currentMaskingLevel = baseAmbientMasking +
                                 (weatherMasking * 0.4f) +
                                 (timeMasking * 0.2f) *
                                 environmentMasking;

            // SpatialAudioManagerへのマスキングレベルの適用
            NotifyMaskingChange();
        }

        /// <summary>
        /// 環境の位置に基づく更新
        /// </summary>
        private void UpdateEnvironmentBasedOnPosition()
        {
            if (audioListener == null) return;

            // 現在の環境を検出
            EnvironmentType detectedEnvironment = DetectEnvironmentAtPosition(audioListener.position);

            if (detectedEnvironment != currentEnvironment)
            {
                ChangeEnvironment(detectedEnvironment);
            }
        }

        /// <summary>
        /// 環境の検出
        /// </summary>
        private EnvironmentType DetectEnvironmentAtPosition(Vector3 position)
        {
            // 3D空間での環境検出
            if (Physics.Raycast(position, Vector3.up, 20f))
            {
                // 地面の種類を判定
                Collider[] nearbyColliders = Physics.OverlapSphere(position, 5f);

                foreach (var collider in nearbyColliders)
                {
                    if (collider.CompareTag("Cave"))
                        return EnvironmentType.Cave;
                    if (collider.CompareTag("Forest"))
                        return EnvironmentType.Forest;
                    if (collider.CompareTag("Water"))
                        return EnvironmentType.Underwater;
                }

                return EnvironmentType.Indoor;
            }

            return EnvironmentType.Outdoor;
        }

        #endregion

        #region Weather System

        /// <summary>
        /// 動的天候システムの更新
        /// </summary>
        private void UpdateDynamicWeather()
        {
            weatherChangeTimer += Time.deltaTime;

            if (weatherChangeTimer >= weatherChangeInterval)
            {
                ChangeWeatherRandomly();
                weatherChangeTimer = 0f;
            }
        }

        /// <summary>
        /// 天候をランダムに変更
        /// </summary>
        private void ChangeWeatherRandomly()
        {
            WeatherType[] weatherTypes = System.Enum.GetValues(typeof(WeatherType)) as WeatherType[];
            WeatherType newWeather = weatherTypes[Random.Range(0, weatherTypes.Length)];

            if (newWeather != currentWeather)
            {
                ChangeWeather(newWeather);
            }
        }

        #endregion

        #region Ambient Sound Management

        /// <summary>
        /// 環境音の再生を開始
        /// </summary>
        private void StartAmbientSounds()
        {
            if (ambientAudioSource == null) return;

            SoundDataSO[] ambientSounds = GetAmbientSoundsForCurrentState();

            if (ambientSounds != null && ambientSounds.Length > 0)
            {
                var selectedSound = ambientSounds[Random.Range(0, ambientSounds.Length)];
                var clip = selectedSound.GetRandomClip();

                if (clip != null)
                {
                    ambientAudioSource.clip = clip;
                    ambientAudioSource.volume = selectedSound.GetRandomVolume();
                    ambientAudioSource.loop = true;
                    ambientAudioSource.Play();
                }
            }
        }

        /// <summary>
        /// 現在の状態に基づく環境音の取得
        /// </summary>
        private SoundDataSO[] GetAmbientSoundsForCurrentState()
        {
            // 雨の環境音を取得
            if (currentWeather == WeatherType.Rain && rainAmbientSounds.Length > 0)
            {
                return rainAmbientSounds;
            }

            // 森の環境音を取得
            return currentTimeOfDay switch
            {
                TimeOfDay.Night => nightAmbientSounds,
                _ => dayAmbientSounds
            };
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeEnvironment(EnvironmentType newEnvironment)
        {
            if (newEnvironment == currentEnvironment) return;

            EnvironmentType previousEnvironment = currentEnvironment;
            currentEnvironment = newEnvironment;
            activePreset = GetPresetForEnvironment(newEnvironment);

            // 環境の遷移を開始
            if (environmentTransition != null)
            {
                StopCoroutine(environmentTransition);
            }
            environmentTransition = StartCoroutine(TransitionEnvironment());

            OnEnvironmentChanged?.Invoke(newEnvironment);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Environment changed: {previousEnvironment} 竊・{newEnvironment}");
#endif
#endif
        }

        /// <summary>
        /// 天候の変更
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (newWeather == currentWeather) return;

            WeatherType previousWeather = currentWeather;
            currentWeather = newWeather;

            UpdateMaskingLevels();
            StartAmbientSounds(); // 環境音の再生を開始

            OnWeatherChanged?.Invoke(newWeather);
            weatherChangeEvent?.RaiseAtPosition("WeatherChange", transform.position);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Weather changed: {previousWeather} 竊・{newWeather}");
#endif
#endif
        }

        /// <summary>
        /// 環境の変更
        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTimeOfDay)
        {
            if (newTimeOfDay == currentTimeOfDay) return;

            TimeOfDay previousTime = currentTimeOfDay;
            currentTimeOfDay = newTimeOfDay;

            UpdateMaskingLevels();
            StartAmbientSounds(); // 環境音の再生を開始

            OnTimeOfDayChanged?.Invoke(newTimeOfDay);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Time of day changed: {previousTime} 竊・{newTimeOfDay}");
#endif
#endif
        }

        /// <summary>
        /// 現在のマスキングレベルの取得
        /// </summary>
        public float GetCurrentMaskingLevel() => currentMaskingLevel;

        /// <summary>
        /// 現在の環境、天候、時間帯の取得
        /// </summary>
        public (EnvironmentType env, WeatherType weather, TimeOfDay time) GetCurrentState()
        {
            return (currentEnvironment, currentWeather, currentTimeOfDay);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// AudioListenerの取得
        /// </summary>
        private void FindAudioListener()
        {
            var listener = FindFirstObjectByType<AudioListener>();
            if (listener != null)
            {
                audioListener = listener.transform;
            }
        }

        /// <summary>
        /// 環境に基づくプリセットの取得
        /// </summary>
        private EnvironmentPreset GetPresetForEnvironment(EnvironmentType environment)
        {
            foreach (var preset in environmentPresets)
            {
                if (preset.environmentType == environment)
                {
                    return preset;
                }
            }
            return CreateDefaultPreset();
        }

        /// <summary>
        /// デフォルトの環境プリセットの作成
        /// </summary>
        private EnvironmentPreset CreateDefaultPreset()
        {
            return new EnvironmentPreset
            {
                environmentType = EnvironmentType.Outdoor,
                ambientVolume = 0f,
                reverbLevel = 0f,
                lowPassFrequency = 22000f,
                maskingMultiplier = 1f
            };
        }

        /// <summary>
        /// マスキングレベルの変更通知
        /// </summary>
        private void NotifyMaskingChange()
        {
            // SpatialAudioManagerにマスキングレベルの変更を通知
            // 複数のリスナーに対して通知を行う
        }

        /// <summary>
        /// 環境の遷移を開始
        /// </summary>
        private System.Collections.IEnumerator TransitionEnvironment()
        {
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 環境の遷移を適用
                ApplyEnvironmentSettings();

                yield return null;
            }

            environmentTransition = null;
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// 環境プリセット
    /// </summary>
    [System.Serializable]
    public struct EnvironmentPreset
    {
        public EnvironmentType environmentType;
        [Range(-80f, 0f)] public float ambientVolume;
        [Range(0f, 1f)] public float reverbLevel;
        [Range(80f, 22000f)] public float lowPassFrequency;
        [Range(0f, 2f)] public float maskingMultiplier;
    }

    /// <summary>
    /// 環境プリセット
    /// </summary>
    [System.Serializable]
    public struct EnvironmentPreset
    {
        public EnvironmentType environmentType;
        [Range(-80f, 0f)] public float ambientVolume;
        [Range(0f, 1f)] public float reverbLevel;
        [Range(80f, 22000f)] public float lowPassFrequency;
        [Range(0f, 2f)] public float maskingMultiplier;
    }

    #endregion
}

