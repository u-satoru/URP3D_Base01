using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformerオーディオ設定：音響システム・効果音・BGM管理
    /// ServiceLocator統合：オーディオサービス制御設定
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerAudioSettings", menuName = "Platformer Template/Settings/Audio Settings")]
    public class PlatformerAudioSettings : ScriptableObject
    {
        [Header("Master Volume")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _ambientVolume = 0.5f;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _jumpSound;
        [SerializeField] private AudioClip _landSound;
        [SerializeField] private AudioClip _collectibleSound;
        [SerializeField] private AudioClip _damageSound;
        [SerializeField] private AudioClip _backgroundMusic;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public float AmbientVolume => _ambientVolume;
        public AudioClip JumpSound => _jumpSound;
        public AudioClip LandSound => _landSound;
        public AudioClip CollectibleSound => _collectibleSound;
        public AudioClip DamageSound => _damageSound;
        public AudioClip BackgroundMusic => _backgroundMusic;

        public void SetToDefault()
        {
            _masterVolume = 1f;
            _musicVolume = 0.7f;
            _sfxVolume = 0.8f;
            _ambientVolume = 0.5f;
        }

        public void OptimizeForPerformance()
        {
            _sfxVolume = 0.6f; // 効果音を少し抑制
        }
    }
}
