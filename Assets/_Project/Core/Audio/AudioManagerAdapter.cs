using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Helpers;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// ✁EServiceLocator専用実裁E�EAudioManagerアダプター
    /// ServiceLocatorパターンを使用したオーチE��オサービスへのアクセスを提侁E    /// </summary>
    public class AudioManagerAdapter : MonoBehaviour
    {
        
        
        private IAudioService audioService;
        
        private void Awake()
        {
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorからオーチE��オサービスを取征E            if (FeatureFlags.UseServiceLocator)
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
        /// 既存�Eコードとの互換性のためのメソチE��群
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (audioService != null)
            {
                audioService.PlaySound(soundId, position, volume);
            }
            else
            {
                // ✁EServiceLocator専用実裁E- 直接EffectManagerを使用
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
                // ✁EServiceLocator専用実裁E- BGMManagerをServiceLocator経由で取征E                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    // TODO: bgmNameからBGMCategoryへの変換ロジチE��が忁E��E                    // 現在はチE��ォルトカチE��リを使用
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
                // IAudioServiceには直接StopBGMがなぁE�Eで、StopAllSoundsを使用
                audioService.StopAllSounds();
            }
            else
            {
                // ✁EServiceLocator専用実裁E- BGMManagerを直接取征E                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
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
            // ✁EServiceLocator専用実裁E�Eみ - Singletonパターン完�E削除
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManagerAdapter] Adapter destroyed");
            }
        }
    }
}