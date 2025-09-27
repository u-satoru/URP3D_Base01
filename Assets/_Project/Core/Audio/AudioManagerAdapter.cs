using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Helpers;
// using asterivo.Unity60.Core.Services; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// AudioManagerのアダプタクラス
    /// ServiceLocatorからAudioManagerを取得し、AudioManagerの機能を提供する
    /// ServiceLocator経由でAudioManagerにアクセスすることで、依存関係を解消する
    /// </summary>
    public class AudioManagerAdapter : MonoBehaviour
    {


        private IAudioService audioService;

        private void Awake()
        {
            // ServiceLocatorからAudioManagerを取得し、AudioManagerの機能を提供する
            if (FeatureFlags.UseServiceLocator)
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
        private void Awake()
        {
            // ServiceLocatorからAudioManagerを取得し、AudioManagerの機能を提供する
            DontDestroyOnLoad(gameObject);

            // ServiceLocator経由でAudioManagerにアクセスすることで、依存関係を解消する
            if (FeatureFlags.UseServiceLocator)
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
        /// サウンドを再生する
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (audioService != null)
            {
                audioService.PlaySound(soundId, position, volume);
            }
            else
            {
                // ServiceLocatorからEffectManagerを取得し、EffectManagerの機能を提供する
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
                // ServiceLocator実装 - BGMManagerをServiceLocator経由で取得
                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    // TODO: bgmNameからBGMCategoryを取得する処理を実装する
                    // 現在は仮にExplorationカテゴリを使用
                    // BGMCategoryを取得する処理を実装する
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
                // IAudioServiceからStopBGMを呼び出す
                audioService.StopAllSounds();
            }
            else
            {
                // ServiceLocator実装 - BGMManagerを直接取得
                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
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
            // ServiceLocatorからAudioManagerを取得し、AudioManagerの機能を提供する
            if (FeatureFlags.EnableDebugLogging)
            {
                var eventLogger = ServiceLocator.GetService<IEventLogger>(); if (eventLogger != null) eventLogger.Log("[AudioManagerAdapter] Adapter destroyed");
            }
        }
    }
}
