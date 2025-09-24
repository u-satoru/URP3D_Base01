using UnityEngine;
using System;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// NPCVisualSensorのイベント発行を最適化・管理するクラス
    /// イベントの重複発行防止、バッファリング、詳細データ提供を行う
    /// </summary>
    [System.Serializable]
    public class VisualSensorEventManager
    {
        #region Event Data Structures
        
        /// <summary>
        /// 目標発見イベントデータ
        /// </summary>
        [System.Serializable]
        public struct TargetSpottedEventData
        {
            public Transform target;
            public float detectionScore;
            public Vector3 detectedPosition;
            public float detectionTime;
            public AlertLevel currentAlertLevel;
            public int totalActiveTargets;
        }
        
        /// <summary>
        /// 目標紛失イベントデータ
        /// </summary>
        [System.Serializable]
        public struct TargetLostEventData
        {
            public Transform target;
            public Vector3 lastKnownPosition;
            public float timeTracked;
            public float lostTime;
            public AlertLevel currentAlertLevel;
            public int remainingActiveTargets;
        }
        
        /// <summary>
        /// 警戒レベル変更イベントデータ
        /// </summary>
        [System.Serializable]
        public struct AlertLevelChangedEventData
        {
            public AlertLevel previousLevel;
            public AlertLevel currentLevel;
            public float alertIntensity;
            public float changeTime;
            public Transform primaryTarget;
            public int activeTargetCount;
        }
        
        /// <summary>
        /// 疑念活動イベントデータ
        /// </summary>
        [System.Serializable]
        public struct SuspiciousActivityEventData
        {
            public Transform suspiciousTarget;
            public Vector3 activityLocation;
            public AlertLevel alertLevel;
            public float suspicionScore;
            public float eventTime;
        }
        
        #endregion
        
        #region Settings
        
        [BoxGroup("Event Settings")]
        [PropertyRange(0.05f, 0.5f)]
        [LabelText("Event Cooldown Time")]
        [SuffixLabel("s")]
        [SerializeField] private float eventCooldownTime = 0.1f;
        
        [BoxGroup("Event Settings")]
        [LabelText("Buffer Events")]
        [SerializeField] private bool bufferEvents = true;
        
        [BoxGroup("Event Settings")]
        [PropertyRange(1, 20)]
        [LabelText("Max Buffer Size")]
        [ShowIf("bufferEvents")]
        [SerializeField] private int maxBufferSize = 10;
        
        [BoxGroup("Event Settings")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Buffer Flush Interval")]
        [ShowIf("bufferEvents")]
        [SuffixLabel("s")]
        [SerializeField] private float bufferFlushInterval = 0.2f;
        
        #endregion
        
        #region Runtime Data
        
        private NPCVisualSensor sensor;
        private Dictionary<string, float> lastEventTimes = new Dictionary<string, float>();
        private Queue<Action> eventBuffer = new Queue<Action>();
        private float lastBufferFlushTime = 0f;
        
        // イベント統計
        [ShowInInspector, ReadOnly]
        [LabelText("Events Buffered")]
        private int eventsBuffered = 0;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Events Sent This Frame")]
        private int eventsSentThisFrame = 0;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Events Suppressed")]
        private int eventsSuppressed = 0;
        
        #endregion
        
        #region Events
        
        public event Action<TargetSpottedEventData> OnTargetSpottedDetailed;
        public event Action<TargetLostEventData> OnTargetLostDetailed;
        public event Action<AlertLevelChangedEventData> OnAlertLevelChangedDetailed;
        public event Action<SuspiciousActivityEventData> OnSuspiciousActivityDetailed;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Event Managerの初期化
        /// </summary>
        public void Initialize(NPCVisualSensor visualSensor)
        {
            sensor = visualSensor;
            lastEventTimes.Clear();
            eventBuffer.Clear();
            lastBufferFlushTime = Time.time;
            
            Debug.Log("[EventManager] Visual Sensor Event Manager initialized");
        }
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// Event Managerの更新
        /// </summary>
        public void Update(float deltaTime)
        {
            eventsSentThisFrame = 0;
            
            // バッファされたイベントの定期的な送信
            if (bufferEvents && Time.time - lastBufferFlushTime >= bufferFlushInterval)
            {
                FlushEventBuffer();
                lastBufferFlushTime = Time.time;
            }
        }
        
        #endregion
        
        #region Event Methods
        
        /// <summary>
        /// 目標発見イベントの発行
        /// </summary>
        public void OnTargetSpotted(DetectedTarget target, float detectionScore)
        {
            if (target?.transform == null) return;
            
            string eventKey = $"target_spotted_{target.transform.GetInstanceID()}";
            if (!CanSendEvent(eventKey)) return;
            
            var eventData = new TargetSpottedEventData
            {
                target = target.transform,
                detectionScore = detectionScore,
                detectedPosition = target.lastKnownPosition,
                detectionTime = Time.time,
                currentAlertLevel = sensor.GetCurrentAlertLevel(),
                totalActiveTargets = sensor.GetActiveTargetCount()
            };
            
            SendOrBufferEvent(eventKey, () => OnTargetSpottedDetailed?.Invoke(eventData));
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EventManager] Target spotted event: {target.transform.name} (Score: {detectionScore:F2})");
            #endif
        }
        
        /// <summary>
        /// 目標紛失イベントの発行
        /// </summary>
        public void OnTargetLost(DetectedTarget target, float timeTracked)
        {
            if (target?.transform == null) return;
            
            string eventKey = $"target_lost_{target.transform.GetInstanceID()}";
            if (!CanSendEvent(eventKey)) return;
            
            var eventData = new TargetLostEventData
            {
                target = target.transform,
                lastKnownPosition = target.lastKnownPosition,
                timeTracked = timeTracked,
                lostTime = Time.time,
                currentAlertLevel = sensor.GetCurrentAlertLevel(),
                remainingActiveTargets = sensor.GetActiveTargetCount()
            };
            
            SendOrBufferEvent(eventKey, () => OnTargetLostDetailed?.Invoke(eventData));
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EventManager] Target lost event: {target.transform.name} (Tracked for: {timeTracked:F1}s)");
            #endif
        }
        
        /// <summary>
        /// 警戒レベル変更イベントの発行
        /// </summary>
        public void OnAlertLevelChanged(AlertLevel previous, AlertLevel current, float alertIntensity)
        {
            string eventKey = $"alert_level_changed_{current}";
            if (!CanSendEvent(eventKey)) return;
            
            var eventData = new AlertLevelChangedEventData
            {
                previousLevel = previous,
                currentLevel = current,
                alertIntensity = alertIntensity,
                changeTime = Time.time,
                primaryTarget = sensor.GetPrimaryTarget()?.transform,
                activeTargetCount = sensor.GetActiveTargetCount()
            };
            
            SendOrBufferEvent(eventKey, () => OnAlertLevelChangedDetailed?.Invoke(eventData));
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EventManager] Alert level changed: {previous} → {current} (Intensity: {alertIntensity:F2})");
            #endif
        }
        
        /// <summary>
        /// 疑念活動イベントの発行
        /// </summary>
        public void OnSuspiciousActivity(Transform suspiciousTarget, AlertLevel alertLevel)
        {
            if (suspiciousTarget == null) return;
            
            string eventKey = $"suspicious_activity_{suspiciousTarget.GetInstanceID()}";
            if (!CanSendEvent(eventKey)) return;
            
            var eventData = new SuspiciousActivityEventData
            {
                suspiciousTarget = suspiciousTarget,
                activityLocation = suspiciousTarget.position,
                alertLevel = alertLevel,
                suspicionScore = sensor.GetMemoryConfidence(suspiciousTarget),
                eventTime = Time.time
            };
            
            SendOrBufferEvent(eventKey, () => OnSuspiciousActivityDetailed?.Invoke(eventData));
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EventManager] Suspicious activity: {suspiciousTarget.name} at {alertLevel}");
            #endif
        }
        
        #endregion
        
        #region Event Optimization
        
        /// <summary>
        /// イベント発行可能かチェック（クールダウン制御）
        /// </summary>
        private bool CanSendEvent(string eventKey)
        {
            if (!lastEventTimes.ContainsKey(eventKey))
            {
                lastEventTimes[eventKey] = 0f;
            }
            
            if (Time.time - lastEventTimes[eventKey] < eventCooldownTime)
            {
                eventsSuppressed++;
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// イベントの送信またはバッファリング
        /// </summary>
        private void SendOrBufferEvent(string eventKey, Action eventAction)
        {
            lastEventTimes[eventKey] = Time.time;
            
            if (bufferEvents && eventBuffer.Count < maxBufferSize)
            {
                eventBuffer.Enqueue(eventAction);
                eventsBuffered = eventBuffer.Count;
            }
            else
            {
                eventAction?.Invoke();
                eventsSentThisFrame++;
            }
        }
        
        /// <summary>
        /// バッファされたイベントの送信
        /// </summary>
        private void FlushEventBuffer()
        {
            while (eventBuffer.Count > 0)
            {
                var eventAction = eventBuffer.Dequeue();
                eventAction?.Invoke();
                eventsSentThisFrame++;
            }
            
            eventsBuffered = 0;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (eventsSentThisFrame > 0)
            {
                Debug.Log($"[EventManager] Flushed {eventsSentThisFrame} buffered events");
            }
            #endif
        }
        
        /// <summary>
        /// イベント統計情報の取得
        /// </summary>
        public EventManagerStats GetStats()
        {
            return new EventManagerStats
            {
                eventsBuffered = eventsBuffered,
                eventsSentThisFrame = eventsSentThisFrame,
                eventsSuppressed = eventsSuppressed,
                bufferEnabled = bufferEvents,
                cooldownTime = eventCooldownTime
            };
        }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Event Managerのクリーンアップ
        /// </summary>
        public void Cleanup()
        {
            lastEventTimes.Clear();
            eventBuffer.Clear();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event Manager統計情報
    /// </summary>
    [System.Serializable]
    public struct EventManagerStats
    {
        public int eventsBuffered;
        public int eventsSentThisFrame;
        public int eventsSuppressed;
        public bool bufferEnabled;
        public float cooldownTime;
    }
}
