using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 動的音響環境システム
    /// 時間帯、天候、場所による音響環境の動的変化を管理    /// </summary>
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
        [SerializeField] private float weatherChangeInterval = 60f; // 私E        [SerializeField] private AudioEvent weatherChangeEvent;
        
        [Header("Ambient Sound Sources")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private SoundDataSO[] dayAmbientSounds;
        [SerializeField] private SoundDataSO[] nightAmbientSounds;
        [SerializeField] private SoundDataSO[] rainAmbientSounds;
        
        // 現在適用中の環境宁E        private EnvironmentPreset activePreset;
        private float currentMaskingLevel;
        private float weatherChangeTimer;
        
        // 音響環境の変化アニメーション
        private Coroutine environmentTransition;
        
        // リスナー位置追跡
        private Transform audioListener;
        
        // イベント通知
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
        /// 環境システムの初期化        /// </summary>
        private void InitializeEnvironment()
        {
            activePreset = GetPresetForEnvironment(currentEnvironment);
        }
        
        /// <summary>
        /// 環境定を適用
        /// </summary>
        private void ApplyEnvironmentSettings()
        {
            if (environmentMixer == null) return;
            
            // Audio Mixerのパラメータを設定            environmentMixer.SetFloat(ambientVolumeParam, activePreset.ambientVolume);
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
            
            // SpatialAudioManagerにマスキングレベルを通知
            NotifyMaskingChange();
        }
        
        /// <summary>
        /// 位置に基づく環境新
        /// </summary>
        private void UpdateEnvironmentBasedOnPosition()
        {
            if (audioListener == null) return;
            
            // 環境定（例：室内/屋外）を検出            EnvironmentType detectedEnvironment = DetectEnvironmentAtPosition(audioListener.position);
            
            if (detectedEnvironment != currentEnvironment)
            {
                ChangeEnvironment(detectedEnvironment);
            }
        }
        
        /// <summary>
        /// 指定位置での環境別を検出
        /// </summary>
        private EnvironmentType DetectEnvironmentAtPosition(Vector3 position)
        {
            // 天井があるかチェック
            if (Physics.Raycast(position, Vector3.up, 20f))
            {
                // さらに詳細な環境宁E                Collider[] nearbyColliders = Physics.OverlapSphere(position, 5f);
                
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
        /// 動的天候シシステムの更新
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
        /// ランダムに天候を変更
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
        /// アンビエントサウンドの開始
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
        /// 現在の状態に対応するアンビエントサウンドを取得        /// </summary>
        private SoundDataSO[] GetAmbientSoundsForCurrentState()
        {
            // 天候優允E            if (currentWeather == WeatherType.Rain && rainAmbientSounds.Length > 0)
            {
                return rainAmbientSounds;
            }
            
            // 時間帯による選択
            return currentTimeOfDay switch
            {
                TimeOfDay.Night => nightAmbientSounds,
                _ => dayAmbientSounds
            };
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 環境変更
        /// </summary>
        public void ChangeEnvironment(EnvironmentType newEnvironment)
        {
            if (newEnvironment == currentEnvironment) return;
            
            EnvironmentType previousEnvironment = currentEnvironment;
            currentEnvironment = newEnvironment;
            activePreset = GetPresetForEnvironment(newEnvironment);
            
            // スムーズな環境遷移
            if (environmentTransition != null)
            {
                StopCoroutine(environmentTransition);
            }
            environmentTransition = StartCoroutine(TransitionEnvironment());
            
            OnEnvironmentChanged?.Invoke(newEnvironment);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Environment changed: {previousEnvironment} ↁE{newEnvironment}");
#endif
            #endif
        }
        
        /// <summary>
        /// 天候を変更
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (newWeather == currentWeather) return;
            
            WeatherType previousWeather = currentWeather;
            currentWeather = newWeather;
            
            UpdateMaskingLevels();
            StartAmbientSounds(); // アンビエントサウンドを再開
            
            OnWeatherChanged?.Invoke(newWeather);
            weatherChangeEvent?.RaiseAtPosition("WeatherChange", transform.position);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Weather changed: {previousWeather} ↁE{newWeather}");
#endif
            #endif
        }
        
        /// <summary>
        /// 時間帯を変更
        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTimeOfDay)
        {
            if (newTimeOfDay == currentTimeOfDay) return;
            
            TimeOfDay previousTime = currentTimeOfDay;
            currentTimeOfDay = newTimeOfDay;
            
            UpdateMaskingLevels();
            StartAmbientSounds(); // アンビエントサウンドを再開
            
            OnTimeOfDayChanged?.Invoke(newTimeOfDay);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Time of day changed: {previousTime} ↁE{newTimeOfDay}");
#endif
            #endif
        }
        
        /// <summary>
        /// 現在のマスキングレベルを取得        /// </summary>
        public float GetCurrentMaskingLevel() => currentMaskingLevel;
        
        /// <summary>
        /// 現在の環境報を取得        /// </summary>
        public (EnvironmentType env, WeatherType weather, TimeOfDay time) GetCurrentState()
        {
            return (currentEnvironment, currentWeather, currentTimeOfDay);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// AudioListenerを検索
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
        /// 環境プリセットを取得        /// </summary>
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
        /// デフォルトプリセチE��を作成
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
        /// マスキング変化の通知
        /// </summary>
        private void NotifyMaskingChange()
        {
            // SpatialAudioManagerなど他のシシステムに通知
            // 実装は具体的な連携方法に依存
        }
        
        /// <summary>
        /// 環境遷移のコルーチン
        /// </summary>
        private System.Collections.IEnumerator TransitionEnvironment()
        {
            float duration = 2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 環境定を段階的に適用
                ApplyEnvironmentSettings();
                
                yield return null;
            }
            
            environmentTransition = null;
        }
        
        #endregion
    }
    
    #region Supporting Classes and Enums
    
    /// <summary>
    /// 環境の種類    /// </summary>
    public enum EnvironmentType
    {
        Indoor,      // 室冁E        Outdoor,     // 屋夁E        Urban,       // 都市部

        Cave,        // 洞突E        Forest,      // 森极E        Underwater   // 水中
    }
    
    /// <summary>
    /// 天候の種類
    /// </summary>
    public enum WeatherType
    {
        Clear,       // 晴天
        Rain,        // 雨
        Storm,       // 嵁E        Fog          // 霧
    }
    
    /// <summary>
    /// 時間帯
    /// </summary>
    public enum TimeOfDay
    {
        Day,         // 昼
        Evening,     // 夕方
        Night,       // 夁E        Dawn         // 明け方
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