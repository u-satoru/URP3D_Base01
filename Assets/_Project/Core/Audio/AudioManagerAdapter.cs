using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Helpers;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 笨・ServiceLocator蟆ら畑螳溯｣・・AudioManager繧｢繝繝励ち繝ｼ
    /// ServiceLocator繝代ち繝ｼ繝ｳ繧剃ｽｿ逕ｨ縺励◆繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ縺ｸ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧呈署萓・    /// </summary>
    public class AudioManagerAdapter : MonoBehaviour
    {
        
        
        private IAudioService audioService;
        
        private void Awake()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocator縺九ｉ繧ｪ繝ｼ繝・ぅ繧ｪ繧ｵ繝ｼ繝薙せ繧貞叙蠕・            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    if (audioService != null && FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManagerAdapter] Successfully retrieved AudioService from ServiceLocator");
                    }
                    else if (audioService == null && FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogWarning("[AudioManagerAdapter] AudioService not found in ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError($"[AudioManagerAdapter] Failed to get AudioService from ServiceLocator: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 譌｢蟄倥・繧ｳ繝ｼ繝峨→縺ｮ莠呈鋤諤ｧ縺ｮ縺溘ａ縺ｮ繝｡繧ｽ繝・ラ鄒､
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (audioService != null)
            {
                audioService.PlaySound(soundId, position, volume);
            }
            else
            {
                // 笨・ServiceLocator蟆ら畑螳溯｣・- 逶ｴ謗･EffectManager繧剃ｽｿ逕ｨ
                var effectService = ServiceLocator.GetService<IEffectService>();
                if (effectService != null)
                {
                    effectService.PlayEffect(soundId, position, volume);
                }
                else
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManagerAdapter] IEffectService not found in ServiceLocator");
                }
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            if (audioService != null)
            {
                audioService.SetMasterVolume(volume);
            }
            else
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManagerAdapter] No audio service available and legacy singletons are disabled");
            }
        }
        
        public void PlayBGM(string bgmName, float fadeTime = 1f)
        {
            if (audioService is asterivo.Unity60.Core.Audio.Services.AudioService service)
            {
                service.PlayBGM(bgmName, fadeTime);
            }
            else
            {
                // 笨・ServiceLocator蟆ら畑螳溯｣・- BGMManager繧担erviceLocator邨檎罰縺ｧ蜿門ｾ・                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    // TODO: bgmName縺九ｉBGMCategory縺ｸ縺ｮ螟画鋤繝ｭ繧ｸ繝・け縺悟ｿ・ｦ・                    // 迴ｾ蝨ｨ縺ｯ繝・ヵ繧ｩ繝ｫ繝医き繝・ざ繝ｪ繧剃ｽｿ逕ｨ
                    bgmManager.PlayBGMCategory(BGMCategory.Exploration, fadeTime <= 0);
                }
                else
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManagerAdapter] BGMManager not found");
                }
            }
        }
        
        public void StopBGM(float fadeTime = 1f)
        {
            if (audioService != null)
            {
                // IAudioService縺ｫ縺ｯ逶ｴ謗･StopBGM縺後↑縺・・縺ｧ縲ヾtopAllSounds繧剃ｽｿ逕ｨ
                audioService.StopAllSounds();
            }
            else
            {
                // 笨・ServiceLocator蟆ら畑螳溯｣・- BGMManager繧堤峩謗･蜿門ｾ・                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    bgmManager.StopBGM(fadeTime);
                }
                else
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.LogError("[AudioManagerAdapter] BGMManager not found");
                }
            }
        }
        
        public void PlayEffect(string effectName, Vector3 position = default, float volume = 1f)
        {
            PlaySound(effectName, position, volume);
        }
        
        private void OnDestroy()
        {
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManagerAdapter] Adapter destroyed");
            }
        }
    }
}
