using UnityEngine;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using _Project.Core;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 既存のAudioManager.Instanceを使用しているコードとの後方互換性を保つアダプター
    /// 段階的移行期間中に使用
    /// </summary>
    public class AudioManagerAdapter : MonoBehaviour
    {
        private static AudioManagerAdapter instance;
        
        /// <summary>
        /// 後方互換性のためのInstance（非推奨）
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IAudioService>() instead")]
        public static AudioManagerAdapter Instance
        {
            get
            {
                if (instance == null && FeatureFlags.UseServiceLocator)
                {
                    // ServiceLocatorから取得を試みる
                    var service = ServiceLocator.GetService<IAudioService>();
                    if (service is AudioManagerAdapter adapter)
                    {
                        instance = adapter;
                    }
                    else if (service != null)
                    {
                        // AudioServiceをラップするアダプターを作成
                        var go = new GameObject("AudioManagerAdapter");
                        instance = go.AddComponent<AudioManagerAdapter>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        private IAudioService audioService;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            // ServiceLocatorからオーディオサービスを取得
            if (FeatureFlags.UseServiceLocator)
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                if (audioService == null && FeatureFlags.EnableDebugLogging)
                {
                    EventLogger.LogWarning("[AudioManagerAdapter] AudioService not found in ServiceLocator");
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
            else if (AudioManager.Instance != null)
            {
                // フォールバック：古いAudioManagerを使用
                // EffectManagerを直接使用
                var effectManager = FindFirstObjectByType<EffectManager>();
                if (effectManager != null)
                {
                    effectManager.PlayEffect(soundId, position, volume);
                }
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            if (audioService != null)
            {
                audioService.SetMasterVolume(volume);
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(volume);
            }
        }
        
        public void PlayBGM(string bgmName, float fadeTime = 1f)
        {
            if (audioService is asterivo.Unity60.Core.Audio.Services.AudioService service)
            {
                service.PlayBGM(bgmName, fadeTime);
            }
            else if (AudioManager.Instance != null)
            {
                // BGMManagerを直接使用
                var bgmManager = FindFirstObjectByType<BGMManager>();
                if (bgmManager != null)
                {
                    // TODO: bgmNameからBGMCategoryへの変換ロジックが必要
                    // 現在はデフォルトカテゴリを使用
                    bgmManager.PlayBGMCategory(BGMCategory.Exploration, fadeTime <= 0);
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
            else if (AudioManager.Instance != null)
            {
                // BGMManagerを直接使用
                var bgmManager = FindFirstObjectByType<BGMManager>();
                if (bgmManager != null)
                {
                    bgmManager.StopBGM(fadeTime);
                }
            }
        }
        
        public void PlayEffect(string effectName, Vector3 position = default, float volume = 1f)
        {
            PlaySound(effectName, position, volume);
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}