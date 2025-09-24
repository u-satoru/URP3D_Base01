using UnityEngine;
using UnityEngine.Audio;

namespace asterivo.Unity60.Core.Audio.Data
{
    /// <summary>
    /// サウンドクリップとその設定を管理するScriptableObject
    /// ステルスゲーム用の詳細な音響特性を含む
    /// </summary>
    [CreateAssetMenu(fileName = "New Sound Data", menuName = "asterivo.Unity60/Audio/Sound Data")]
    public class SoundDataSO : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private string soundID;
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private AudioMixerGroup mixerGroup;
        
        [Header("音量・ピッチ設定")]
        [Range(0f, 1f)]
        [SerializeField] private float baseVolume = 1f;
        [Range(0f, 2f)]
        [SerializeField] private float basePitch = 1f;
        [SerializeField] private Vector2 volumeVariation = new Vector2(0.9f, 1.1f);
        [SerializeField] private Vector2 pitchVariation = new Vector2(0.95f, 1.05f);
        
        [Header("3D音響設定")]
        [SerializeField] private bool is3D = true;
        [Range(0f, 1f)]
        [SerializeField] private float spatialBlend = 1f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        
        [Header("ステルスゲーム特化設定")]
        [SerializeField] private float baseHearingRadius = 5f;
        [SerializeField] private float priority = 0.5f;
        [SerializeField] private bool canBeMasked = true;
        [SerializeField] private SurfaceMaterial[] affectedSurfaces;
        
        [Header("表面素材別設定")]
        [SerializeField] private SurfaceAudioModifier[] surfaceModifiers;
        
        // プロパティ
        public string SoundID => soundID;
        public AudioClip[] AudioClips => audioClips;
        public AudioMixerGroup MixerGroup => mixerGroup;
        public float BaseVolume => baseVolume;
        public float BasePitch => basePitch;
        public Vector2 VolumeVariation => volumeVariation;
        public Vector2 PitchVariation => pitchVariation;
        public bool Is3D => is3D;
        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public AudioRolloffMode RolloffMode => rolloffMode;
        public float BaseHearingRadius => baseHearingRadius;
        public float Priority => priority;
        public bool CanBeMasked => canBeMasked;
        public SurfaceMaterial[] AffectedSurfaces => affectedSurfaces;
        
        /// <summary>
        /// ランダムなクリップを取得
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (audioClips == null || audioClips.Length == 0) return null;
            return audioClips[Random.Range(0, audioClips.Length)];
        }
        
        /// <summary>
        /// ランダムな音量を取得
        /// </summary>
        public float GetRandomVolume()
        {
            return baseVolume * Random.Range(volumeVariation.x, volumeVariation.y);
        }
        
        /// <summary>
        /// ランダムなピッチを取得
        /// </summary>
        public float GetRandomPitch()
        {
            return basePitch * Random.Range(pitchVariation.x, pitchVariation.y);
        }
        
        /// <summary>
        /// 表面素材に応じた聴取範囲を取得
        /// </summary>
        public float GetHearingRadiusForSurface(SurfaceMaterial surface)
        {
            if (surfaceModifiers != null)
            {
                foreach (var modifier in surfaceModifiers)
                {
                    if (modifier.surfaceType == surface)
                    {
                        return baseHearingRadius * modifier.hearingRadiusMultiplier;
                    }
                }
            }
            return baseHearingRadius;
        }
        
        /// <summary>
        /// 表面素材に応じた音量倍率を取得
        /// </summary>
        public float GetVolumeMultiplierForSurface(SurfaceMaterial surface)
        {
            if (surfaceModifiers != null)
            {
                foreach (var modifier in surfaceModifiers)
                {
                    if (modifier.surfaceType == surface)
                    {
                        return modifier.volumeMultiplier;
                    }
                }
            }
            return 1f;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(soundID) && audioClips != null && audioClips.Length > 0 && audioClips[0] != null)
            {
                soundID = audioClips[0].name;
            }
        }
        #endif
    }
    
    /// <summary>
    /// 表面素材による音響効果の調整値
    /// </summary>
    [System.Serializable]
    public struct SurfaceAudioModifier
    {
        public SurfaceMaterial surfaceType;
        [Range(0f, 2f)] public float volumeMultiplier;
        [Range(0f, 3f)] public float hearingRadiusMultiplier;
        [Range(0f, 2f)] public float pitchMultiplier;
    }
}
