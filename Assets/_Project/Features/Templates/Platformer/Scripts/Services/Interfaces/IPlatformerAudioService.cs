using System;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformerオーディオサービス インターフェース
    /// 音響システム・効果音・BGM管理
    /// </summary>
    public interface IPlatformerAudioService : IPlatformerService
    {
        void PlayJumpSound();
        void PlayLandSound();
        void PlayCollectibleSound();
        void PlayDamageSound();
        void PlayBackgroundMusic();
        void StopBackgroundMusic();
        void SetMasterVolume(float volume);
        void SetSfxVolume(float volume);
        void SetMusicVolume(float volume);
    }
}
