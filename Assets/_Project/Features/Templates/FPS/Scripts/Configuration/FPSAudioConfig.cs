using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// FPS Template用オーディオ設定
    /// 武器音、足音、環境音、UI音等の設定を管理
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Audio Config")]
    public class FPSAudioConfig : ScriptableObject
    {
        [Header("Volume Settings")]
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _sfxVolume = 1.0f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _voiceVolume = 1.0f;

        [Header("Weapon Audio")]
        [SerializeField] private float _weaponSoundRange = 50f;
        [SerializeField] private bool _enable3DWeaponAudio = true;
        [SerializeField] private AnimationCurve _weaponFalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Footstep Audio")]
        [SerializeField] private float _footstepVolume = 0.8f;
        [SerializeField] private float _footstepRange = 10f;
        [SerializeField] private bool _enableFootstepVariation = true;

        [Header("Environmental Audio")]
        [SerializeField] private float _ambientVolume = 0.5f;
        [SerializeField] private bool _enableReverb = true;
        [SerializeField] private bool _enableEcho = false;

        [Header("UI Audio")]
        [SerializeField] private float _uiSoundVolume = 0.9f;
        [SerializeField] private bool _enableUIConfirmationSounds = true;

        [Header("Audio Occlusion")]
        [SerializeField] private bool _enableAudioOcclusion = true;
        [SerializeField] private LayerMask _occlusionLayerMask = -1;
        [SerializeField] private float _occlusionUpdateRate = 10f;

        // Properties
        public float MasterVolume => _masterVolume;
        public float SFXVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
        public float VoiceVolume => _voiceVolume;
        public float WeaponSoundRange => _weaponSoundRange;
        public bool Enable3DWeaponAudio => _enable3DWeaponAudio;
        public AnimationCurve WeaponFalloffCurve => _weaponFalloffCurve;
        public float FootstepVolume => _footstepVolume;
        public float FootstepRange => _footstepRange;
        public bool EnableFootstepVariation => _enableFootstepVariation;
        public float AmbientVolume => _ambientVolume;
        public bool EnableReverb => _enableReverb;
        public bool EnableEcho => _enableEcho;
        public float UISoundVolume => _uiSoundVolume;
        public bool EnableUIConfirmationSounds => _enableUIConfirmationSounds;
        public bool EnableAudioOcclusion => _enableAudioOcclusion;
        public LayerMask OcclusionLayerMask => _occlusionLayerMask;
        public float OcclusionUpdateRate => _occlusionUpdateRate;

        /// <summary>
        /// 最終音量を計算（マスター音量 × カテゴリ音量）
        /// </summary>
        public float GetFinalVolume(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.SFX => _masterVolume * _sfxVolume,
                AudioCategory.Music => _masterVolume * _musicVolume,
                AudioCategory.Voice => _masterVolume * _voiceVolume,
                AudioCategory.UI => _masterVolume * _uiSoundVolume,
                _ => _masterVolume
            };
        }
    }

    public enum AudioCategory
    {
        SFX,
        Music,
        Voice,
        UI
    }
}
