using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Audio.Events
{
    /// <summary>
    /// オーディオイベント用の汎用GameEvent
    /// ステルスゲームに特化した音響システムの基盤
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Event", menuName = "asterivo.Unity60/Audio/Events/Audio Event")]
    public class AudioEvent : GameEvent<AudioEventData>
    {
        #if UNITY_EDITOR
        [Header("Audio Debug Info")]
        [SerializeField] private bool showAudioDebug = true;
        #endif

        /// <summary>
        /// 音響イベントを発火
        /// </summary>
        /// <param name="audioData">音響データ</param>
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
        /// 簡易版: 位置情報付きで音を再生
        /// </summary>
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
        /// プレイヤー音源用の簡易発火
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