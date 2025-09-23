using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 蜍慕噪髻ｳ髻ｿ迺ｰ蠅・す繧ｹ繝・Β
    /// 譎る俣蟶ｯ縲∝､ｩ蛟吶∝ｴ謇縺ｫ繧医ｋ髻ｳ髻ｿ迺ｰ蠅・・蜍慕噪螟牙喧繧堤ｮ｡逅・    /// </summary>
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
        [SerializeField] private float weatherChangeInterval = 60f; // 遘・        [SerializeField] private AudioEvent weatherChangeEvent;
        
        [Header("Ambient Sound Sources")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private SoundDataSO[] dayAmbientSounds;
        [SerializeField] private SoundDataSO[] nightAmbientSounds;
        [SerializeField] private SoundDataSO[] rainAmbientSounds;
        
        // 迴ｾ蝨ｨ驕ｩ逕ｨ荳ｭ縺ｮ迺ｰ蠅・ｨｭ螳・        private EnvironmentPreset activePreset;
        private float currentMaskingLevel;
        private float weatherChangeTimer;
        
        // 髻ｳ髻ｿ迺ｰ蠅・・螟牙喧繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ
        private Coroutine environmentTransition;
        
        // 繝ｪ繧ｹ繝翫・菴咲ｽｮ霑ｽ霍｡
        private Transform audioListener;
        
        // 繧､繝吶Φ繝磯夂衍
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
        /// 迺ｰ蠅・す繧ｹ繝・Β縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeEnvironment()
        {
            activePreset = GetPresetForEnvironment(currentEnvironment);
        }
        
        /// <summary>
        /// 迺ｰ蠅・ｨｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        private void ApplyEnvironmentSettings()
        {
            if (environmentMixer == null) return;
            
            // Audio Mixer縺ｮ繝代Λ繝｡繝ｼ繧ｿ繧定ｨｭ螳・            environmentMixer.SetFloat(ambientVolumeParam, activePreset.ambientVolume);
            environmentMixer.SetFloat(reverbParam, activePreset.reverbLevel);
            environmentMixer.SetFloat(lowPassParam, activePreset.lowPassFrequency);
            
            // 繝槭せ繧ｭ繝ｳ繧ｰ繝ｬ繝吶Ν縺ｮ譖ｴ譁ｰ
            UpdateMaskingLevels();
        }
        
        /// <summary>
        /// 繝槭せ繧ｭ繝ｳ繧ｰ繝ｬ繝吶Ν縺ｮ譖ｴ譁ｰ
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
            
            // SpatialAudioManager縺ｫ繝槭せ繧ｭ繝ｳ繧ｰ繝ｬ繝吶Ν繧帝夂衍
            NotifyMaskingChange();
        }
        
        /// <summary>
        /// 菴咲ｽｮ縺ｫ蝓ｺ縺･縺冗腸蠅・峩譁ｰ
        /// </summary>
        private void UpdateEnvironmentBasedOnPosition()
        {
            if (audioListener == null) return;
            
            // 迺ｰ蠅・愛螳夲ｼ井ｾ具ｼ壼ｮ､蜀・螻句､悶・讀懷・・・            EnvironmentType detectedEnvironment = DetectEnvironmentAtPosition(audioListener.position);
            
            if (detectedEnvironment != currentEnvironment)
            {
                ChangeEnvironment(detectedEnvironment);
            }
        }
        
        /// <summary>
        /// 謖・ｮ壻ｽ咲ｽｮ縺ｧ縺ｮ迺ｰ蠅・ｨｮ蛻･繧呈､懷・
        /// </summary>
        private EnvironmentType DetectEnvironmentAtPosition(Vector3 position)
        {
            // 螟ｩ莠輔′縺ゅｋ縺九メ繧ｧ繝・け
            if (Physics.Raycast(position, Vector3.up, 20f))
            {
                // 縺輔ｉ縺ｫ隧ｳ邏ｰ縺ｪ迺ｰ蠅・愛螳・                Collider[] nearbyColliders = Physics.OverlapSphere(position, 5f);
                
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
        /// 蜍慕噪螟ｩ蛟吶す繧ｹ繝・Β縺ｮ譖ｴ譁ｰ
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
        /// 繝ｩ繝ｳ繝繝縺ｫ螟ｩ蛟吶ｒ螟画峩
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
        /// 繧｢繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨・髢句ｧ・        /// </summary>
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
        /// 迴ｾ蝨ｨ縺ｮ迥ｶ諷九↓蟇ｾ蠢懊☆繧九い繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨ｒ蜿門ｾ・        /// </summary>
        private SoundDataSO[] GetAmbientSoundsForCurrentState()
        {
            // 螟ｩ蛟吝━蜈・            if (currentWeather == WeatherType.Rain && rainAmbientSounds.Length > 0)
            {
                return rainAmbientSounds;
            }
            
            // 譎る俣蟶ｯ縺ｫ繧医ｋ驕ｸ謚・            return currentTimeOfDay switch
            {
                TimeOfDay.Night => nightAmbientSounds,
                _ => dayAmbientSounds
            };
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 迺ｰ蠅・ｒ螟画峩
        /// </summary>
        public void ChangeEnvironment(EnvironmentType newEnvironment)
        {
            if (newEnvironment == currentEnvironment) return;
            
            EnvironmentType previousEnvironment = currentEnvironment;
            currentEnvironment = newEnvironment;
            activePreset = GetPresetForEnvironment(newEnvironment);
            
            // 繧ｹ繝繝ｼ繧ｺ縺ｪ迺ｰ蠅・・遘ｻ
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
        /// 螟ｩ蛟吶ｒ螟画峩
        /// </summary>
        public void ChangeWeather(WeatherType newWeather)
        {
            if (newWeather == currentWeather) return;
            
            WeatherType previousWeather = currentWeather;
            currentWeather = newWeather;
            
            UpdateMaskingLevels();
            StartAmbientSounds(); // 繧｢繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨ｒ蜀埼幕
            
            OnWeatherChanged?.Invoke(newWeather);
            weatherChangeEvent?.RaiseAtPosition("WeatherChange", transform.position);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Weather changed: {previousWeather} 竊・{newWeather}");
#endif
            #endif
        }
        
        /// <summary>
        /// 譎る俣蟶ｯ繧貞､画峩
        /// </summary>
        public void ChangeTimeOfDay(TimeOfDay newTimeOfDay)
        {
            if (newTimeOfDay == currentTimeOfDay) return;
            
            TimeOfDay previousTime = currentTimeOfDay;
            currentTimeOfDay = newTimeOfDay;
            
            UpdateMaskingLevels();
            StartAmbientSounds(); // 繧｢繝ｳ繝薙お繝ｳ繝医し繧ｦ繝ｳ繝峨ｒ蜀埼幕
            
            OnTimeOfDayChanged?.Invoke(newTimeOfDay);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ProjectDebug.Log($"<color=blue>[DynamicAudioEnvironment]</color> Time of day changed: {previousTime} 竊・{newTimeOfDay}");
#endif
            #endif
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝槭せ繧ｭ繝ｳ繧ｰ繝ｬ繝吶Ν繧貞叙蠕・        /// </summary>
        public float GetCurrentMaskingLevel() => currentMaskingLevel;
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ迺ｰ蠅・ュ蝣ｱ繧貞叙蠕・        /// </summary>
        public (EnvironmentType env, WeatherType weather, TimeOfDay time) GetCurrentState()
        {
            return (currentEnvironment, currentWeather, currentTimeOfDay);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// AudioListener繧呈､懃ｴ｢
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
        /// 迺ｰ蠅・・繝ｪ繧ｻ繝・ヨ繧貞叙蠕・        /// </summary>
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
        /// 繝・ヵ繧ｩ繝ｫ繝医・繝ｪ繧ｻ繝・ヨ繧剃ｽ懈・
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
        /// 繝槭せ繧ｭ繝ｳ繧ｰ螟牙喧縺ｮ騾夂衍
        /// </summary>
        private void NotifyMaskingChange()
        {
            // SpatialAudioManager縺ｪ縺ｩ莉悶・繧ｷ繧ｹ繝・Β縺ｫ騾夂衍
            // 螳溯｣・・蜈ｷ菴鍋噪縺ｪ騾｣謳ｺ譁ｹ豕輔↓萓晏ｭ・        }
        
        /// <summary>
        /// 迺ｰ蠅・・遘ｻ縺ｮ繧ｳ繝ｫ繝ｼ繝√Φ
        /// </summary>
        private System.Collections.IEnumerator TransitionEnvironment()
        {
            float duration = 2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 迺ｰ蠅・ｨｭ螳壹ｒ谿ｵ髫守噪縺ｫ驕ｩ逕ｨ
                ApplyEnvironmentSettings();
                
                yield return null;
            }
            
            environmentTransition = null;
        }
        
        #endregion
    }
    
    #region Supporting Classes and Enums
    
    /// <summary>
    /// 迺ｰ蠅・・遞ｮ鬘・    /// </summary>
    public enum EnvironmentType
    {
        Indoor,      // 螳､蜀・        Outdoor,     // 螻句､・        Urban,       // 驛ｽ蟶るΚ

        Cave,        // 豢樒ｪ・        Forest,      // 譽ｮ譫・        Underwater   // 豌ｴ荳ｭ
    }
    
    /// <summary>
    /// 螟ｩ蛟吶・遞ｮ鬘・    /// </summary>
    public enum WeatherType
    {
        Clear,       // 譎ｴ螟ｩ
        Rain,        // 髮ｨ
        Storm,       // 蠏・        Fog          // 髴ｧ
    }
    
    /// <summary>
    /// 譎る俣蟶ｯ
    /// </summary>
    public enum TimeOfDay
    {
        Day,         // 譏ｼ
        Evening,     // 螟墓婿
        Night,       // 螟・        Dawn         // 譏弱￠譁ｹ
    }
    
    /// <summary>
    /// 迺ｰ蠅・・繝ｪ繧ｻ繝・ヨ
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