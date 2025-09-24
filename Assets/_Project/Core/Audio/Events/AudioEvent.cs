using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
// using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio.Events
{
    /// <summary>
    /// 繧ｪ繝ｼ繝・ぅ繧ｪ繧､繝吶Φ繝育畑縺ｮ豎守畑GameEvent
    /// 繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝縺ｫ迚ｹ蛹悶＠縺滄浹髻ｿ繧ｷ繧ｹ繝・Β縺ｮ蝓ｺ逶､
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Event", menuName = "asterivo.Unity60/Audio/Events/Audio Event")]
    public class AudioEvent : GameEvent<AudioEventData>
    {
        #if UNITY_EDITOR
        [Header("Audio Debug Info")]
        [SerializeField] private bool showAudioDebug = true;
        #endif

        /// <summary>
        /// 髻ｳ髻ｿ繧､繝吶Φ繝医ｒ逋ｺ轣ｫ
        /// </summary>
        /// <param name="audioData">髻ｳ髻ｿ繝・・繧ｿ</param>
        public new void Raise(AudioEventData audioData)
        {
            #if UNITY_EDITOR
            if (showAudioDebug && Application.isPlaying)
            {
                ProjectDebug.Log($"<color=magenta>[AudioEvent]</color> '{name}' - Sound: {audioData.soundID}, " +
                         $"Volume: {audioData.volume:F2}, Position: {audioData.worldPosition}");
            }
            #endif
            
            base.Raise(audioData);
        }
        
        /// <summary>
        /// 邁｡譏鍋沿: 菴咲ｽｮ諠・ｱ莉倥″縺ｧ髻ｳ繧貞・逕・        /// </summary>
        public void RaiseAtPosition(string soundID, Vector3 position, float volume = 1f)
        {
            var data = new AudioEventData
            {
                soundID = soundID,
                worldPosition = position,
                volume = volume,
                timestamp = Time.time
            };
            Raise(data);
        }
        
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ髻ｳ貅千畑縺ｮ邁｡譏鍋匱轣ｫ
        /// </summary>
        public void RaisePlayerSound(string soundID, float volume = 1f, AudioSourceType sourceType = AudioSourceType.Player)
        {
            var data = new AudioEventData
            {
                soundID = soundID,
                sourceType = sourceType,
                volume = volume,
                timestamp = Time.time,
                isPlayerGenerated = true
            };
            Raise(data);
        }
    }
}
