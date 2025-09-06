using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 空間音響システムの中央管理クラス
    /// ステルスゲーム用の高度な3D音響処理を担当
    /// </summary>
    public class SpatialAudioManager : MonoBehaviour
    {
        [Header("Audio Manager Settings")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private int maxConcurrentSounds = 32;
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        
        [Header("Distance Attenuation")]
        [SerializeField] private AnimationCurve distanceAttenuationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private float globalHearingMultiplier = 1f;
        
        [Header("Occlusion System")]
        [SerializeField] private bool enableOcclusion = true;
        [SerializeField] private float occlusionCheckInterval = 0.1f;
        [SerializeField] private float maxOcclusionReduction = 0.8f;
        
        [Header("Environment Reverb")]
                // [SerializeField] private bool enableEnvironmentReverb = true;e;
        [SerializeField] private AudioReverbZone[] reverbZones;
        
        // オーディオソースプール
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();
        
        // 聴取者（通常はプレイヤー）
        private Transform listener;
        private AudioListener audioListener;
        
        // オクルージョンチェック用
        private Dictionary<AudioSource, float> occlusionValues = new Dictionary<AudioSource, float>();
        
        private static SpatialAudioManager instance;
        public static SpatialAudioManager Instance => instance;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioSourcePool();
            FindAudioListener();
        }
        
        private void Start()
        {
            if (enableOcclusion)
            {
                InvokeRepeating(nameof(UpdateOcclusion), 0f, occlusionCheckInterval);
            }
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 空間音響でサウンドを再生
        /// </summary>
        public AudioSource PlaySoundAtPosition(SoundDataSO soundData, Vector3 position, float volumeMultiplier = 1f)
        {
            if (soundData == null) return null;
            
            var audioSource = GetPooledAudioSource();
            if (audioSource == null) return null;
            
            SetupAudioSource(audioSource, soundData, position, volumeMultiplier);
            
            var clip = soundData.GetRandomClip();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                StartCoroutine(ReturnToPoolWhenFinished(audioSource, clip.length));
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// AudioEventDataを使用してサウンドを再生
        /// </summary>
        public AudioSource PlaySoundFromEvent(AudioEventData eventData, SoundDataSO soundData)
        {
            if (soundData == null) return null;
            
            var audioSource = PlaySoundAtPosition(soundData, eventData.worldPosition, eventData.volume);
            
            if (audioSource != null)
            {
                // イベントデータの追加設定を適用
                audioSource.pitch = soundData.GetRandomPitch() * eventData.pitch;
                
                // 表面材質による調整
                ApplySurfaceModifications(audioSource, eventData, soundData);
            }
            
            return audioSource;
        }
        
        /// <summary>
        /// 音源間の距離に基づく音量計算
        /// </summary>
        public float CalculateVolumeAtDistance(float distance, float maxHearingRadius)
        {
            if (distance <= 0f) return 1f;
            if (distance >= maxHearingRadius) return 0f;
            
            float normalizedDistance = distance / maxHearingRadius;
            return distanceAttenuationCurve.Evaluate(normalizedDistance) * globalHearingMultiplier;
        }
        
        /// <summary>
        /// 音源が聞こえるかどうかを判定
        /// </summary>
        public bool IsAudibleAtPosition(Vector3 soundPosition, float hearingRadius, Vector3 listenerPosition)
        {
            float distance = Vector3.Distance(soundPosition, listenerPosition);
            float volume = CalculateVolumeAtDistance(distance, hearingRadius);
            
            // オクルージョンも考慮
            if (enableOcclusion)
            {
                float occlusion = CalculateOcclusion(soundPosition, listenerPosition);
                volume *= (1f - occlusion);
            }
            
            return volume > 0.01f; // 最小閾値
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// オーディオソースプールの初期化
        /// </summary>
        private void InitializeAudioSourcePool()
        {
            for (int i = 0; i < maxConcurrentSounds; i++)
            {
                var go = new GameObject($"PooledAudioSource_{i}");
                go.transform.SetParent(transform);
                
                var audioSource = go.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D音響
                
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        /// <summary>
        /// AudioListenerを検索
        /// </summary>
        private void FindAudioListener()
        {
            audioListener = FindFirstObjectByType<AudioListener>();
            if (audioListener != null)
            {
                listener = audioListener.transform;
            }
        }
        
        /// <summary>
        /// プールからオーディオソースを取得
        /// </summary>
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                var audioSource = audioSourcePool.Dequeue();
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
            
            UnityEngine.Debug.LogWarning("[SpatialAudioManager] オーディオソースプールが枯渇しました");
            return null;
        }
        
        /// <summary>
        /// オーディオソースをプールに返却
        /// </summary>
        private void ReturnToPool(AudioSource audioSource)
        {
            if (audioSource == null) return;
            
            audioSource.Stop();
            audioSource.clip = null;
            
            activeAudioSources.Remove(audioSource);
            audioSourcePool.Enqueue(audioSource);
            
            occlusionValues.Remove(audioSource);
        }
        
        /// <summary>
        /// オーディオソースの設定
        /// </summary>
        private void SetupAudioSource(AudioSource audioSource, SoundDataSO soundData, Vector3 position, float volumeMultiplier)
        {
            audioSource.transform.position = position;
            audioSource.volume = soundData.GetRandomVolume() * volumeMultiplier;
            audioSource.pitch = soundData.GetRandomPitch();
            
            if (soundData.Is3D)
            {
                audioSource.spatialBlend = soundData.SpatialBlend;
                audioSource.minDistance = soundData.MinDistance;
                audioSource.maxDistance = soundData.MaxDistance;
                audioSource.rolloffMode = soundData.RolloffMode;
            }
            
            if (soundData.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = soundData.MixerGroup;
            }
        }
        
        /// <summary>
        /// 表面材質による音響効果を適用
        /// </summary>
        private void ApplySurfaceModifications(AudioSource audioSource, AudioEventData eventData, SoundDataSO soundData)
        {
            if (eventData.surfaceType != SurfaceMaterial.Default)
            {
                float surfaceVolumeMultiplier = soundData.GetVolumeMultiplierForSurface(eventData.surfaceType);
                audioSource.volume *= surfaceVolumeMultiplier;
            }
        }
        
        /// <summary>
        /// オクルージョン（遮蔽）の計算
        /// </summary>
        private float CalculateOcclusion(Vector3 soundPosition, Vector3 listenerPosition)
        {
            if (listener == null) return 0f;
            
            Vector3 direction = listenerPosition - soundPosition;
            float distance = direction.magnitude;
            
            if (Physics.Raycast(soundPosition, direction.normalized, out RaycastHit hit, distance, obstacleLayerMask))
            {
                // 障害物までの距離の割合で遮蔽度を計算
                float occlusionFactor = hit.distance / distance;
                return Mathf.Lerp(maxOcclusionReduction, 0f, occlusionFactor);
            }
            
            return 0f;
        }
        
        /// <summary>
        /// 全アクティブな音源のオクルージョンを更新
        /// </summary>
        private void UpdateOcclusion()
        {
            if (listener == null) return;
            
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    float occlusion = CalculateOcclusion(audioSource.transform.position, listener.position);
                    occlusionValues[audioSource] = occlusion;
                    
                    // 音量にオクルージョンを適用
                    // 注意: ここでは簡略化のため直接音量を変更していますが、
                    // 実際にはLowPassFilterなどを使用する方が自然です
                }
            }
        }
        
        /// <summary>
        /// 再生終了後にプールに返却するコルーチン
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource audioSource, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);
            ReturnToPool(audioSource);
        }
        
        #endregion
        
        #region Editor Helpers
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (listener == null) return;
            
            // アクティブな音源の可視化
            Gizmos.color = Color.yellow;
            foreach (var audioSource in activeAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    Gizmos.DrawWireSphere(audioSource.transform.position, 2f);
                    Gizmos.DrawLine(listener.position, audioSource.transform.position);
                }
            }
        }
        #endif
        
        #endregion
    }
}