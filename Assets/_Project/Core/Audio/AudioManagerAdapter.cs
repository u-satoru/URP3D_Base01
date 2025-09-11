using UnityEngine;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Helpers;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// ✅ ServiceLocator専用実装のAudioManagerアダプター
    /// ServiceLocatorパターンを使用したオーディオサービスへのアクセスを提供
    /// </summary>
    public class AudioManagerAdapter : MonoBehaviour
    {
        
        
        private IAudioService audioService;
        
        private void Awake()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorからオーディオサービスを取得
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    audioService = ServiceLocator.GetService<IAudioService>();
                    if (audioService != null && FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.Log("[AudioManagerAdapter] Successfully retrieved AudioService from ServiceLocator");
                    }
                    else if (audioService == null && FeatureFlags.EnableDebugLogging)
                    {
                        EventLogger.LogWarning("[AudioManagerAdapter] AudioService not found in ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogError($"[AudioManagerAdapter] Failed to get AudioService from ServiceLocator: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 既存のコードとの互換性のためのメソッド群
        /// </summary>
        public void PlaySound(string soundId, Vector3 position = default, float volume = 1f)
        {
            if (audioService != null)
            {
                audioService.PlaySound(soundId, position, volume);
            }
            else
            {
                // ✅ ServiceLocator専用実装 - 直接EffectManagerを使用
                var effectService = ServiceLocator.GetService<IEffectService>();
                if (effectService != null)
                {
                    effectService.PlayEffect(soundId, position, volume);
                }
                else
                {
                    EventLogger.LogError("[AudioManagerAdapter] IEffectService not found in ServiceLocator");
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
                EventLogger.LogError("[AudioManagerAdapter] No audio service available and legacy singletons are disabled");
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
                // ✅ ServiceLocator専用実装 - BGMManagerをServiceLocator経由で取得
                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    // TODO: bgmNameからBGMCategoryへの変換ロジックが必要
                    // 現在はデフォルトカテゴリを使用
                    bgmManager.PlayBGMCategory(BGMCategory.Exploration, fadeTime <= 0);
                }
                else
                {
                    EventLogger.LogError("[AudioManagerAdapter] BGMManager not found");
                }
            }
        }
        
        public void StopBGM(float fadeTime = 1f)
        {
            if (audioService != null)
            {
                // IAudioServiceには直接StopBGMがないので、StopAllSoundsを使用
                audioService.StopAllSounds();
            }
            else
            {
                // ✅ ServiceLocator専用実装 - BGMManagerを直接取得
                var bgmManager = ServiceHelper.GetServiceWithFallback<BGMManager>();
                if (bgmManager != null)
                {
                    bgmManager.StopBGM(fadeTime);
                }
                else
                {
                    EventLogger.LogError("[AudioManagerAdapter] BGMManager not found");
                }
            }
        }
        
        public void PlayEffect(string effectName, Vector3 position = default, float volume = 1f)
        {
            PlaySound(effectName, position, volume);
        }
        
        private void OnDestroy()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            if (FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log("[AudioManagerAdapter] Adapter destroyed");
            }
        }
    }
}