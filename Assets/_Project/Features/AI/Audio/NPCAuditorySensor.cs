using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Data;
using asterivo.Unity60.Core.Audio.Events;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.AI.Audio
{
    /// <summary>
    /// NPCの聴覚センサーシステム
    /// ステルスゲームにおける音の検知と反応を管理
    /// </summary>
    public class NPCAuditorySensor : MonoBehaviour, IGameEventListener<AudioEventData>
    {
        [Header("Hearing Capabilities")]
        [SerializeField] private float baseHearingRadius = 10f;
        [SerializeField] private float hearingAccuity = 1f; // 聴力の鋭敏さ (0.1～3.0)
        [SerializeField] private AnimationCurve hearingCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Detection Thresholds")]
        [SerializeField] private float detectionThreshold = 0.3f; // 音を認識する最小閾値
        [SerializeField] private float investigationThreshold = 0.6f; // 調査を始める閾値
        [SerializeField] private float alertThreshold = 0.8f; // 警戒状態に入る閾値
        
        [Header("State Management")]
        [SerializeField] private float memoryDuration = 5f; // 音の記憶保持時間
        [SerializeField] private int maxSimultaneousSounds = 5; // 同時に追跡する最大音数
        
        [Header("Environmental Factors")]
        [SerializeField] private bool affectedByMasking = true; // 他の音によるマスキング効果
        [SerializeField] private float ambientMaskingLevel = 0.2f; // 環境音によるマスキング
        
        [Header("Events")]
        [SerializeField] private AudioEvent globalAudioEvent; // 全体音響イベントチャネル
        [SerializeField] private GameEvent onSoundDetected;
        [SerializeField] private GameEvent onSoundLost;
        [SerializeField] private GameEvent onAlertStateChanged;
        
        // 検出された音の履歴
        private List<DetectedSound> detectedSounds = new List<DetectedSound>();
        private List<DetectedSound> soundMemory = new List<DetectedSound>();
        
        // 現在の警戒レベル
        private AlertLevel currentAlertLevel = AlertLevel.Relaxed;
        private float lastAlertTime;
        
        // デバッグ情報
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        [Header("Debug Info (Runtime)")]
        [SerializeField] private int activeSoundsCount;
        [SerializeField] private string currentAlertState;
        [SerializeField] private float loudestSoundLevel;
        #endif
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // イベントリスナーとして登録
            if (globalAudioEvent != null)
            {
                globalAudioEvent.RegisterListener(this);
            }
        }
        
        private void Update()
        {
            UpdateSoundTracking();
            UpdateAlertLevel();
            CleanupOldSounds();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateDebugInfo();
            #endif
        }
        
        private void OnDestroy()
        {
            // イベントリスナーの解除
            if (globalAudioEvent != null)
            {
                globalAudioEvent.UnregisterListener(this);
            }
        }
        
        #endregion
        
        #region IGameEventListener Implementation
        
        public void OnEventRaised(AudioEventData audioData)
        {
            ProcessIncomingSound(audioData);
        }
        
        #endregion
        
        #region Sound Processing
        
        /// <summary>
        /// 受信した音響イベントを処理
        /// </summary>
        private void ProcessIncomingSound(AudioEventData audioData)
        {
            // 距離チェック
            float distance = Vector3.Distance(transform.position, audioData.worldPosition);
            float effectiveHearingRadius = CalculateEffectiveHearingRadius(audioData);
            
            if (distance > effectiveHearingRadius) return;
            
            // 音の強度計算
            float soundIntensity = CalculateSoundIntensity(audioData, distance);
            
            // マスキング効果の適用
            if (affectedByMasking)
            {
                soundIntensity = ApplyMaskingEffects(soundIntensity, audioData);
            }
            
            // 検出閾値チェック
            if (soundIntensity < detectionThreshold) return;
            
            // 音を検出として記録
            DetectedSound detectedSound = new DetectedSound(audioData, soundIntensity, distance, Time.time);
            RegisterDetectedSound(detectedSound);
            
            // イベント通知
            onSoundDetected?.Raise();
        }
        
        /// <summary>
        /// 音の強度を計算
        /// </summary>
        private float CalculateSoundIntensity(AudioEventData audioData, float distance)
        {
            // 基本強度（音量と優先度）
            float baseIntensity = audioData.volume * audioData.priority;
            
            // 距離減衰の適用
            float distanceAttenuation = CalculateDistanceAttenuation(distance, audioData.hearingRadius);
            
            // 聴力による調整
            float adjustedIntensity = baseIntensity * distanceAttenuation * hearingAccuity;
            
            return Mathf.Clamp01(adjustedIntensity);
        }
        
        /// <summary>
        /// 距離減衰の計算
        /// </summary>
        private float CalculateDistanceAttenuation(float distance, float maxRange)
        {
            if (distance <= 0f) return 1f;
            if (distance >= maxRange) return 0f;
            
            float normalizedDistance = distance / maxRange;
            return hearingCurve.Evaluate(normalizedDistance);
        }
        
        /// <summary>
        /// 実効聴取範囲の計算
        /// </summary>
        private float CalculateEffectiveHearingRadius(AudioEventData audioData)
        {
            float baseRadius = Mathf.Max(audioData.hearingRadius, baseHearingRadius);
            return baseRadius * hearingAccuity;
        }
        
        /// <summary>
        /// マスキング効果の適用
        /// </summary>
        private float ApplyMaskingEffects(float originalIntensity, AudioEventData audioData)
        {
            if (!audioData.canBemasked) return originalIntensity;
            
            // 環境音によるマスキング
            float maskedIntensity = originalIntensity - ambientMaskingLevel;
            
            // 他の同時音によるマスキング
            float competingSoundsLevel = CalculateCompetingSoundsLevel(audioData);
            maskedIntensity -= competingSoundsLevel * 0.3f;
            
            return Mathf.Clamp01(maskedIntensity);
        }
        
        /// <summary>
        /// 競合する同時音のレベル計算
        /// </summary>
        private float CalculateCompetingSoundsLevel(AudioEventData newSound)
        {
            float competingLevel = 0f;
            
            foreach (var sound in detectedSounds)
            {
                if (sound.audioData.timestamp != newSound.timestamp && 
                    Time.time - sound.detectionTime < 2f) // 2秒以内の音
                {
                    competingLevel += sound.intensity * 0.5f;
                }
            }
            
            return Mathf.Clamp01(competingLevel);
        }
        
        #endregion
        
        #region Sound Management
        
        /// <summary>
        /// 検出した音を登録
        /// </summary>
        private void RegisterDetectedSound(DetectedSound sound)
        {
            // 最大同時音数の制限
            if (detectedSounds.Count >= maxSimultaneousSounds)
            {
                // 最も古い音を削除
                detectedSounds.RemoveAt(0);
            }
            
            detectedSounds.Add(sound);
            soundMemory.Add(sound);
        }
        
        /// <summary>
        /// 音の追跡状態を更新
        /// </summary>
        private void UpdateSoundTracking()
        {
            for (int i = detectedSounds.Count - 1; i >= 0; i--)
            {
                var sound = detectedSounds[i];
                
                // 音の継続時間チェック（継続音の場合）
                if (Time.time - sound.detectionTime > 1f) // 1秒で音は自然減衰
                {
                    detectedSounds.RemoveAt(i);
                    onSoundLost?.Raise();
                }
            }
        }
        
        /// <summary>
        /// 古い記憶を削除
        /// </summary>
        private void CleanupOldSounds()
        {
            soundMemory.RemoveAll(sound => Time.time - sound.detectionTime > memoryDuration);
        }
        
        #endregion
        
        #region Alert System
        
        /// <summary>
        /// 警戒レベルの更新
        /// </summary>
        private void UpdateAlertLevel()
        {
            float currentMaxIntensity = GetCurrentMaxSoundIntensity();
            AlertLevel newAlertLevel = CalculateAlertLevel(currentMaxIntensity);
            
            if (newAlertLevel != currentAlertLevel)
            {
                AlertLevel previousLevel = currentAlertLevel;
                currentAlertLevel = newAlertLevel;
                lastAlertTime = Time.time;
                
                OnAlertLevelChanged(previousLevel, newAlertLevel);
            }
        }
        
        /// <summary>
        /// 現在の最大音強度を取得
        /// </summary>
        private float GetCurrentMaxSoundIntensity()
        {
            float maxIntensity = 0f;
            
            foreach (var sound in detectedSounds)
            {
                if (sound.intensity > maxIntensity)
                {
                    maxIntensity = sound.intensity;
                }
            }
            
            return maxIntensity;
        }
        
        /// <summary>
        /// 音強度から警戒レベルを計算
        /// </summary>
        private AlertLevel CalculateAlertLevel(float soundIntensity)
        {
            if (soundIntensity >= alertThreshold) return AlertLevel.Alert;
            if (soundIntensity >= investigationThreshold) return AlertLevel.Investigating;
            if (soundIntensity >= detectionThreshold) return AlertLevel.Suspicious;
            return AlertLevel.Relaxed;
        }
        
        /// <summary>
        /// 警戒レベル変更時の処理
        /// </summary>
        private void OnAlertLevelChanged(AlertLevel previousLevel, AlertLevel newLevel)
        {
            onAlertStateChanged?.Raise();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=orange>[NPCAuditorySensor]</color> {gameObject.name}: " +
                     $"Alert level changed from {previousLevel} to {newLevel}");
            #endif
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 現在の警戒レベルを取得
        /// </summary>
        public AlertLevel GetCurrentAlertLevel() => currentAlertLevel;
        
        /// <summary>
        /// 最近検出した音のリストを取得
        /// </summary>
        public List<DetectedSound> GetRecentSounds() => new List<DetectedSound>(detectedSounds);
        
        /// <summary>
        /// 音の記憶を取得
        /// </summary>
        public List<DetectedSound> GetSoundMemory() => new List<DetectedSound>(soundMemory);
        
        /// <summary>
        /// 特定位置の音が聞こえるかテスト
        /// </summary>
        public bool CanHearAtPosition(Vector3 position, float soundVolume, float soundRadius)
        {
            float distance = Vector3.Distance(transform.position, position);
            if (distance > soundRadius * hearingAccuity) return false;
            
            float intensity = soundVolume * CalculateDistanceAttenuation(distance, soundRadius) * hearingAccuity;
            return intensity >= detectionThreshold;
        }
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void UpdateDebugInfo()
        {
            activeSoundsCount = detectedSounds.Count;
            currentAlertState = currentAlertLevel.ToString();
            loudestSoundLevel = GetCurrentMaxSoundIntensity();
        }
        
        private void OnDrawGizmosSelected()
        {
            // 聴取範囲の可視化
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, baseHearingRadius);
            
            // 警戒レベルに応じた色分け
            Color alertColor = currentAlertLevel switch
            {
                AlertLevel.Alert => Color.red,
                AlertLevel.Investigating => new Color(1f, 0.5f, 0f), // Orange
                AlertLevel.Suspicious => Color.yellow,
                _ => Color.green
            };
            
            Gizmos.color = alertColor;
            Gizmos.DrawWireSphere(transform.position, baseHearingRadius * 0.8f);
            
            // 検出した音の可視化
            foreach (var sound in detectedSounds)
            {
                Gizmos.color = Color.Lerp(Color.white, alertColor, sound.intensity);
                Gizmos.DrawLine(transform.position, sound.audioData.worldPosition);
                Gizmos.DrawWireCube(sound.audioData.worldPosition, Vector3.one * 0.5f);
            }
        }
        #endif
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// 検出された音の情報
    /// </summary>
    [System.Serializable]
    public class DetectedSound
    {
        public AudioEventData audioData;
        public float intensity;
        public float distance;
        public float detectionTime;
        public Vector3 estimatedPosition;
        
        public DetectedSound(AudioEventData data, float soundIntensity, float soundDistance, float time)
        {
            audioData = data;
            intensity = soundIntensity;
            distance = soundDistance;
            detectionTime = time;
            estimatedPosition = data.worldPosition;
        }
    }
    

    #endregion
}
