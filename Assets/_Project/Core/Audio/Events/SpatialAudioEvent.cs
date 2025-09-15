using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Audio.Events
{
    /// <summary>
    /// 空間音響用のイベントデータ
    /// </summary>
    [System.Serializable]
    public class SpatialAudioData
    {
        public string soundId;
        public Vector3 position;
        public float maxDistance = 50f;
        public float volume = 1f;
        public Transform source; // 移動する音源の場合
        public bool isLooping = false;
        public AudioEventType eventType = AudioEventType.Play;
    }
    
    /// <summary>
    /// 音響イベントのタイプ
    /// </summary>
    public enum AudioEventType
    {
        Play,
        Stop,
        Update,
        FadeIn,
        FadeOut
    }
    
    /// <summary>
    /// 空間音響イベント
    /// </summary>
    [CreateAssetMenu(fileName = "SpatialAudioEvent", menuName = "Game Events/Audio/Spatial Audio Event")]
    public class SpatialAudioEvent : GameEvent<SpatialAudioData>
    {
    }
}