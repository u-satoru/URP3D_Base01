using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for audio management service
    /// Provides centralized audio control for the application
    /// </summary>
    public interface IAudioManager : IService
    {
        /// <summary>
        /// Set master volume level
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        void SetMasterVolume(float volume);

        /// <summary>
        /// Set SFX volume level
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        void SetSFXVolume(float volume);

        /// <summary>
        /// Set music volume level
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        void SetMusicVolume(float volume);

        /// <summary>
        /// Get current master volume
        /// </summary>
        float GetMasterVolume();

        /// <summary>
        /// Get current SFX volume
        /// </summary>
        float GetSFXVolume();

        /// <summary>
        /// Get current music volume
        /// </summary>
        float GetMusicVolume();

        /// <summary>
        /// Play a sound effect
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="volume">Volume multiplier</param>
        /// <param name="pitch">Pitch multiplier</param>
        void PlaySFX(AudioClip clip, float volume = 1.0f, float pitch = 1.0f);

        /// <summary>
        /// Play a sound effect by name (TPS Template compatibility)
        /// </summary>
        /// <param name="clipName">Name of the audio clip to play</param>
        /// <param name="volume">Volume multiplier</param>
        /// <param name="pitch">Pitch multiplier</param>
        void PlaySFX(string clipName, float volume = 1.0f, float pitch = 1.0f);

        /// <summary>
        /// Play background music
        /// </summary>
        /// <param name="clip">Music clip to play</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="fadeIn">Fade in duration</param>
        void PlayMusic(AudioClip clip, bool loop = true, float fadeIn = 0.0f);

        /// <summary>
        /// Stop current music
        /// </summary>
        /// <param name="fadeOut">Fade out duration</param>
        void StopMusic(float fadeOut = 0.0f);

        /// <summary>
        /// Pause all audio
        /// </summary>
        void PauseAll();

        /// <summary>
        /// Resume all audio
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// Mute all audio
        /// </summary>
        void MuteAll();

        /// <summary>
        /// Unmute all audio
        /// </summary>
        void UnmuteAll();

        /// <summary>
        /// Check if audio is currently muted
        /// </summary>
        bool IsMuted { get; }

        /// <summary>
        /// Stop background music (TPS Template compatibility)
        /// </summary>
        void StopBGM();

        /// <summary>
        /// Resume background music (TPS Template compatibility)
        /// </summary>
        void ResumeBGM();
    }
}
