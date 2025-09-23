using UnityEngine;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio.Commands
{
    /// <summary>
    /// 髻ｳ螢ｰ蜀咲函繧ｳ繝槭Φ繝・- ObjectPool縺ｫ蟇ｾ蠢懊＠縺溘Μ繧ｻ繝・ヨ蜿ｯ閭ｽ縺ｪ螳溯｣・    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝逕ｨ縺ｮ鬮俶ｩ溯・髻ｳ髻ｿ繧ｷ繧ｹ繝・Β
    /// </summary>
    public class PlaySoundCommand : IResettableCommand
    {
        // 繧ｳ繝槭Φ繝牙ｮ溯｡後↓蠢・ｦ√↑繝・・繧ｿ
        private AudioEventData audioData;
        private SoundDataSO soundData;
        private AudioSource audioSource;
        private Transform listenerTransform;
        
        // 螳溯｡檎憾諷九・邂｡逅・        private bool wasExecuted = false;
        private float originalVolume;
        private float originalPitch;
        
        public bool CanUndo => true;
        
        /// <summary>
        /// IResettableCommand.Initialize螳溯｣・        /// </summary>
        public void Initialize(params object[] parameters)
    {
        if (parameters.Length >= 3)
        {
            Initialize((AudioEventData)parameters[0], (SoundDataSO)parameters[1], (AudioSource)parameters[2], 
                      parameters.Length > 3 ? (Transform)parameters[3] : null);
        }
    }
    
    /// <summary>
    /// 繧ｳ繝槭Φ繝峨・蛻晄悄蛹・    /// </summary>
    public void Initialize(AudioEventData data, SoundDataSO soundAsset, AudioSource source, Transform listener = null)
        {
            audioData = data;
            soundData = soundAsset;
            audioSource = source;
            listenerTransform = listener;
            wasExecuted = false;
        }
        
        /// <summary>
        /// 髻ｳ螢ｰ繧貞・逕・        /// </summary>
        public void Execute()
        {
            if (audioSource == null || soundData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                ProjectDebug.LogWarning("[PlaySoundCommand] AudioSource 縺ｾ縺溘・ SoundData 縺・null 縺ｧ縺・);
#endif
                return;
            }
            
            // 髻ｳ髻ｿ險ｭ螳壹・驕ｩ逕ｨ
            ApplyAudioSettings();
            
            // 3D髻ｳ髻ｿ縺ｮ險ｭ螳・            if (soundData.Is3D && audioData.use3D)
            {
                Apply3DAudioSettings();
            }
            
            // 陦ｨ髱｢邏譚舌↓繧医ｋ隱ｿ謨ｴ
            ApplySurfaceModifications();
            
            // 繧ｯ繝ｪ繝・・縺ｮ驕ｸ謚槭→蜀咲函
            var clip = soundData.GetRandomClip();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                wasExecuted = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                LogPlayback();
                #endif
            }
        }
        
        /// <summary>
        /// 蜀咲函繧貞●豁｢・・ndo謫堺ｽ懶ｼ・        /// </summary>
        public void Undo()
        {
            if (wasExecuted && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                
                // 蜈・・險ｭ螳壹↓謌ｻ縺・                audioSource.volume = originalVolume;
                audioSource.pitch = originalPitch;
            }
        }
        
        /// <summary>
        /// 繧ｳ繝槭Φ繝峨ｒ繝ｪ繧ｻ繝・ヨ・・bjectPool逕ｨ・・        /// </summary>
        public void Reset()
        {
            audioData = default;
            soundData = null;
            audioSource = null;
            listenerTransform = null;
            wasExecuted = false;
            originalVolume = 0f;
            originalPitch = 0f;
        }
        
        /// <summary>
        /// 蝓ｺ譛ｬ逧・↑髻ｳ髻ｿ險ｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        private void ApplyAudioSettings()
        {
            // 蜈・・蛟､繧剃ｿ晏ｭ・            originalVolume = audioSource.volume;
            originalPitch = audioSource.pitch;
            
            // 譁ｰ縺励＞蛟､繧定ｨｭ螳・            audioSource.volume = soundData.GetRandomVolume() * audioData.volume;
            audioSource.pitch = soundData.GetRandomPitch() * audioData.pitch;
            
            // 繝溘く繧ｵ繝ｼ繧ｰ繝ｫ繝ｼ繝励・險ｭ螳・            if (soundData.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            }
        }
        
        /// <summary>
        /// 3D髻ｳ髻ｿ險ｭ螳壹ｒ驕ｩ逕ｨ
        /// </summary>
        private void Apply3DAudioSettings()
        {
            audioSource.spatialBlend = soundData.SpatialBlend;
            audioSource.minDistance = soundData.MinDistance;
            audioSource.maxDistance = soundData.MaxDistance;
            audioSource.rolloffMode = soundData.RolloffMode;
            
            // 菴咲ｽｮ縺ｮ險ｭ螳・            if (audioData.worldPosition != Vector3.zero)
            {
                audioSource.transform.position = audioData.worldPosition;
            }
        }
        
        /// <summary>
        /// 陦ｨ髱｢邏譚舌↓繧医ｋ髻ｳ髻ｿ隱ｿ謨ｴ繧帝←逕ｨ
        /// </summary>
        private void ApplySurfaceModifications()
        {
            if (audioData.surfaceType != SurfaceMaterial.Default)
            {
                // 陦ｨ髱｢邏譚舌↓蠢懊§縺滄浹驥剰ｪｿ謨ｴ
                float surfaceVolumeMultiplier = soundData.GetVolumeMultiplierForSurface(audioData.surfaceType);
                audioSource.volume *= surfaceVolumeMultiplier;
                
                // 閨ｴ蜿也ｯ・峇縺ｮ隱ｿ謨ｴ・・udioData縺ｫ蜿肴丐・・                audioData.hearingRadius = soundData.GetHearingRadiusForSurface(audioData.surfaceType);
            }
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// 繝・ヰ繝・げ逕ｨ縺ｮ蜀咲函繝ｭ繧ｰ
        /// </summary>
        private void LogPlayback()
        {
            ProjectDebug.Log($"<color=cyan>[PlaySoundCommand]</color> Playing '{soundData.SoundID}' " +
                     $"at {audioData.worldPosition} | Volume: {audioSource.volume:F2} | " +
                     $"Surface: {audioData.surfaceType} | Hearing Radius: {audioData.hearingRadius:F1}m");
        }
        #endif
    }
}