using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Audio.Data
{
    /// <summary>
    /// 天気別環境音コレクション
    /// </summary>
    [System.Serializable]
    public class WeatherAmbientCollection
    {
        [Header("基本設定")]
        public string collectionName = "Weather Ambient";
        public WeatherType weatherType = WeatherType.Clear;
        public float baseVolume = 0.7f;
        public bool enableRandomization = true;
        
        [Header("オーディオクリップ")]
        public AudioClip[] ambientClips = new AudioClip[0];
        
        [Header("音響パラメータ")]
        [Range(0.5f, 2f)] public float pitchVariation = 0.1f;
        [Range(0f, 1f)] public float volumeVariation = 0.2f;
        [Range(0f, 10f)] public float fadeInTime = 2f;
        [Range(0f, 10f)] public float fadeOutTime = 2f;
        
        /// <summary>
        /// ランダムなオーディオクリップを取得
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (ambientClips == null || ambientClips.Length == 0)
                return null;
                
            return ambientClips[Random.Range(0, ambientClips.Length)];
        }
        
        /// <summary>
        /// バリエーション付きの音量を取得
        /// </summary>
        public float GetRandomVolume()
        {
            if (!enableRandomization) return baseVolume;
            
            float variation = Random.Range(-volumeVariation, volumeVariation);
            return Mathf.Clamp01(baseVolume + variation);
        }
        
        /// <summary>
        /// バリエーション付きのピッチを取得
        /// </summary>
        public float GetRandomPitch()
        {
            if (!enableRandomization) return 1f;
            
            return 1f + Random.Range(-pitchVariation, pitchVariation);
        }
    }
    
    /// <summary>
    /// 環境音コレクション（場所別）
    /// </summary>
    [System.Serializable]
    public class AmbientSoundCollection
    {
        [Header("基本設定")]
        public string collectionName = "Ambient Collection";
        public EnvironmentType environmentType = EnvironmentType.Outdoor;
        public float baseVolume = 0.6f;
        public bool loopAmbient = true;
        
        [Header("オーディオクリップ")]
        public AudioClip[] ambientClips = new AudioClip[0];
        
        [Header("レイヤー設定")]
        public int layerPriority = 1;
        public bool affectedByWeather = true;
        public bool affectedByTimeOfDay = true;
        
        /// <summary>
        /// ランダムなオーディオクリップを取得
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (ambientClips == null || ambientClips.Length == 0)
                return null;
                
            return ambientClips[Random.Range(0, ambientClips.Length)];
        }
    }
    
    /// <summary>
    /// 時間帯別環境音コレクション
    /// </summary>
    [System.Serializable]
    public class TimeAmbientCollection
    {
        [Header("基本設定")]
        public string collectionName = "Time Ambient";
        public TimeOfDay timeOfDay = TimeOfDay.Day;
        public float baseVolume = 0.5f;
        
        [Header("オーディオクリップ")]
        public AudioClip[] ambientClips = new AudioClip[0];
        
        [Header("時間遷移")]
        public float transitionDuration = 3f;
        public AnimationCurve volumeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        /// <summary>
        /// ランダムなオーディオクリップを取得
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (ambientClips == null || ambientClips.Length == 0)
                return null;
                
            return ambientClips[Random.Range(0, ambientClips.Length)];
        }
    }
    
    /// <summary>
    /// マスキング効果音コレクション
    /// </summary>
    [System.Serializable]
    public class MaskingSoundCollection
    {
        [Header("基本設定")]
        public string collectionName = "Masking Sound";
        public float maskingStrength = 0.3f;
        public float effectRadius = 15f;
        public bool isDynamic = true;
        
        [Header("オーディオクリップ")]
        public AudioClip[] maskingClips = new AudioClip[0];
        
        [Header("マスキングパラメータ")]
        [Range(0f, 1f)] public float frequencyMask = 0.5f; // 周波数マスキング
        [Range(0f, 1f)] public float temporalMask = 0.3f;  // 時間マスキング
        [Range(1f, 5f)] public float attenuationRate = 2f; // 距離減衰率
        
        /// <summary>
        /// ランダムなマスキング音を取得
        /// </summary>
        public AudioClip GetRandomClip()
        {
            if (maskingClips == null || maskingClips.Length == 0)
                return null;
                
            return maskingClips[Random.Range(0, maskingClips.Length)];
        }
        
        /// <summary>
        /// 距離に基づくマスキング強度を計算
        /// </summary>
        public float CalculateMaskingAtDistance(float distance)
        {
            if (distance >= effectRadius) return 0f;
            
            float normalizedDistance = distance / effectRadius;
            return maskingStrength * Mathf.Pow(1f - normalizedDistance, attenuationRate);
        }
    }
}
