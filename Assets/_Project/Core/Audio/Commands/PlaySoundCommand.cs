using UnityEngine;
// using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio.Commands
{
    /// <summary>
    /// 音声再生コマンチE- ObjectPoolに対応したリセチE��可能な実裁E    /// スチE��スゲーム用の高機�E音響シスチE��
    /// </summary>
    public class PlaySoundCommand : IResettableCommand
    {
        // コマンド実行に忁E��なチE�Eタ
        private AudioEventData audioData;
        private SoundDataSO soundData;
        private AudioSource audioSource;
        private Transform listenerTransform;
        
        // 実行状態�E管琁E        private bool wasExecuted = false;
        private float originalVolume;
        private float originalPitch;
        
        public bool CanUndo => true;
        
        /// <summary>
        /// IResettableCommand.Initialize実裁E        /// </summary>
        public void Initialize(params object[] parameters)
    {
        if (parameters.Length >= 3)
        {
            Initialize((AudioEventData)parameters[0], (SoundDataSO)parameters[1], (AudioSource)parameters[2], 
                      parameters.Length > 3 ? (Transform)parameters[3] : null);
        }
    }
    
    /// <summary>
    /// コマンド�E初期匁E    /// </summary>
    public void Initialize(AudioEventData data, SoundDataSO soundAsset, AudioSource source, Transform listener = null)
        {
            audioData = data;
            soundData = soundAsset;
            audioSource = source;
            listenerTransform = listener;
            wasExecuted = false;
        }
        
        /// <summary>
        /// 音声を�E甁E        /// </summary>
        public void Execute()
        {
            if (audioSource == null || soundData == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                ProjectDebug.LogWarning("[PlaySoundCommand] AudioSource または SoundData が null です");
#endif
                return;
            }
            
            // 音響設定の適用
            ApplyAudioSettings();
            
            // 3D音響の設定
            if (soundData.Is3D && audioData.use3D)
            {
                Apply3DAudioSettings();
            }
            
            // 表面素材による調整
            ApplySurfaceModifications();
            
            // クリチE�Eの選択と再生
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
        /// 再生を停止�E�Endo操作！E        /// </summary>
        public void Undo()
        {
            if (wasExecuted && audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                
                // 元の設定に戻す
                audioSource.volume = originalVolume;
                audioSource.pitch = originalPitch;
            }
        }
        
        /// <summary>
        /// コマンドをリセチE���E�EbjectPool用�E�E        /// </summary>
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
        /// 基本皁E��音響設定を適用
        /// </summary>
        private void ApplyAudioSettings()
        {
            // 允E�E値を保孁E            originalVolume = audioSource.volume;
            originalPitch = audioSource.pitch;
            
            // 新しい値を設宁E            audioSource.volume = soundData.GetRandomVolume() * audioData.volume;
            audioSource.pitch = soundData.GetRandomPitch() * audioData.pitch;
            
            // ミキサーグループ�E設宁E            if (soundData.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            }
        }
        
        /// <summary>
        /// 3D音響設定を適用
        /// </summary>
        private void Apply3DAudioSettings()
        {
            audioSource.spatialBlend = soundData.SpatialBlend;
            audioSource.minDistance = soundData.MinDistance;
            audioSource.maxDistance = soundData.MaxDistance;
            audioSource.rolloffMode = soundData.RolloffMode;
            
            // 位置の設宁E            if (audioData.worldPosition != Vector3.zero)
            {
                audioSource.transform.position = audioData.worldPosition;
            }
        }
        
        /// <summary>
        /// 表面素材による音響調整を適用
        /// </summary>
        private void ApplySurfaceModifications()
        {
            if (audioData.surfaceType != SurfaceMaterial.Default)
            {
                // 表面素材に応じた音量調整
                float surfaceVolumeMultiplier = soundData.GetVolumeMultiplierForSurface(audioData.surfaceType);
                audioSource.volume *= surfaceVolumeMultiplier;
                
                // 聴取篁E��の調整�E�EudioDataに反映�E�E                audioData.hearingRadius = soundData.GetHearingRadiusForSurface(audioData.surfaceType);
            }
        }
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// チE��チE��用の再生ログ
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
