using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.AI.Audio;
using asterivo.Unity60.Core.Data;
using CoreAlertLevel = asterivo.Unity60.Core.Data.AlertLevel;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Audio.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI
{
    /// <summary>
    /// NPCの統合センサーシステム（Visual-Auditory Detection統合）
    /// 視覚センサーと聴覚センサーを統合し、総合的な検知判定を行う
    /// TASK-005: Visual-Auditory Detection統合システムのメインコンポーネント
    /// </summary>
    public class NPCMultiSensorDetector : MonoBehaviour
    {
        [TabGroup("Multi-Sensor", "Core Components")]
        [BoxGroup("Multi-Sensor/Core Components/Sensors")]
        [LabelText("Visual Sensor")]
        [Required]
        [SerializeField] private NPCVisualSensor visualSensor;
        
        [BoxGroup("Multi-Sensor/Core Components/Sensors")]
        [LabelText("Auditory Sensor")]
        [Required]
        [SerializeField] private NPCAuditorySensor auditorySensor;
        
        [TabGroup("Multi-Sensor", "Integration Settings")]
        [BoxGroup("Multi-Sensor/Integration Settings/Fusion")]
        [LabelText("Visual Weight")]
        [PropertyRange(0f, 1f)]
        [SerializeField] private float visualWeight = 0.6f;
        
        [BoxGroup("Multi-Sensor/Integration Settings/Fusion")]
        [LabelText("Auditory Weight")]
        [PropertyRange(0f, 1f)]
        [SerializeField] private float auditoryWeight = 0.4f;
        
        [BoxGroup("Multi-Sensor/Integration Settings/Thresholds")]
        [LabelText("Detection Threshold")]
        [PropertyRange(0f, 1f)]
        [SerializeField] private float integrationThreshold = 0.5f;
        
        [BoxGroup("Multi-Sensor/Integration Settings/Thresholds")]
        [LabelText("Alert Threshold")]
        [PropertyRange(0f, 1f)]
        [SerializeField] private float alertThreshold = 0.8f;
        
        [TabGroup("Multi-Sensor", "Sensor Fusion")]
        [BoxGroup("Multi-Sensor/Sensor Fusion/Algorithm")]
        [LabelText("Fusion Method")]
        [SerializeField] private SensorFusionMethod fusionMethod = SensorFusionMethod.WeightedAverage;
        
        [BoxGroup("Multi-Sensor/Sensor Fusion/Algorithm")]
        [LabelText("Confidence Boost")]
        [PropertyRange(1f, 2f)]
        [Tooltip("両センサーが同時に検知した場合の信頼度増幅")]
        [SerializeField] private float confidenceBoost = 1.3f;
        
        [BoxGroup("Multi-Sensor/Sensor Fusion/Temporal")]
        [LabelText("Temporal Window")]
        [SuffixLabel("s")]
        [Tooltip("センサー間の時間的相関窓")]
        [SerializeField] private float temporalWindow = 2f;
        
        [TabGroup("Multi-Sensor", "Events")]
        [BoxGroup("Multi-Sensor/Events/Detection")]
        [SerializeField] private GameEvent onTargetDetected;
        [SerializeField] private GameEvent onTargetLost;
        [SerializeField] private GameEvent onAlertLevelChanged;
        [SerializeField] private GameEvent onSensorFusionTriggered;
        
        [TabGroup("Multi-Sensor", "Debug Info")]
        [ShowInInspector, ReadOnly]
        [LabelText("Current Alert Level")]
        private CoreAlertLevel currentAlertLevel = CoreAlertLevel.Unaware;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Integrated Detection Score")]
        [ProgressBar(0, 1)]
        private float integratedScore = 0f;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Active Detections")]
        private int activeDetections = 0;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Sensor Correlation")]
        [ProgressBar(0, 1)]
        private float sensorCorrelation = 0f;
        
        // 内部状態管理
        private List<IntegratedDetection> integratedDetections = new List<IntegratedDetection>();
        private Dictionary<Transform, DetectionInfo> targetDetectionInfo = new Dictionary<Transform, DetectionInfo>();
        private float lastVisualDetectionTime = -1f;
        private float lastAuditoryDetectionTime = -1f;
        private CoreAlertLevel previousAlertLevel;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            ValidateComponents();
            InitializeDetectionSystem();
        }
        
        private void Start()
        {
            RegisterEventListeners();
        }
        
        private void Update()
        {
            UpdateIntegratedDetections();
            UpdateAlertLevel();
            UpdateSensorCorrelation();
            CleanupOldDetections();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateDebugInfo();
            #endif
        }
        
        private void OnDestroy()
        {
            UnregisterEventListeners();
        }
        
        #endregion
        
        #region Initialization
        
        private void ValidateComponents()
        {
            if (visualSensor == null)
                visualSensor = GetComponent<NPCVisualSensor>();
                
            if (auditorySensor == null)
                auditorySensor = GetComponent<NPCAuditorySensor>();
                
            if (visualSensor == null || auditorySensor == null)
            {
                Debug.LogError($"[NPCMultiSensorDetector] {gameObject.name}: Required sensor components not found!");
            }
        }
        
        private void InitializeDetectionSystem()
        {
            // ウェイトの正規化
            float totalWeight = visualWeight + auditoryWeight;
            if (totalWeight > 0)
            {
                visualWeight /= totalWeight;
                auditoryWeight /= totalWeight;
            }
            
            previousAlertLevel = currentAlertLevel;
        }
        
        private void RegisterEventListeners()
        {
            // Visual Sensorからのイベント監視は直接コンポーネント参照で行う
            // Auditory Sensorからのイベント監視も同様
        }
        
        private void UnregisterEventListeners()
        {
            // イベントリスナーの解除
        }
        
        #endregion
        
        #region Sensor Integration
        
        /// <summary>
        /// 統合検知の更新
        /// </summary>
        private void UpdateIntegratedDetections()
        {
            // 視覚センサーの検知リストを取得
            var visualTargets = GetVisualDetections();
            var auditoryTargets = GetAuditoryDetections();
            
            // 統合検知処理
            ProcessSensorFusion(visualTargets, auditoryTargets);
            
            // アクティブ検知数の更新
            activeDetections = integratedDetections.Count;
            
            // 最高検知スコアの更新
            integratedScore = GetMaxDetectionScore();
        }
        
        /// <summary>
        /// センサー融合処理
        /// </summary>
        private void ProcessSensorFusion(List<Transform> visualTargets, List<DetectedSound> auditoryTargets)
        {
            // 既存の統合検知をリセット
            integratedDetections.Clear();
            
            // 視覚検知を処理
            foreach (var target in visualTargets)
            {
                if (target == null) continue;
                
                float visualScore = GetVisualDetectionScore(target);
                float auditoryScore = GetAuditoryDetectionScore(target.position);
                
                // センサー融合計算
                float integratedScore = CalculateIntegratedScore(visualScore, auditoryScore);
                
                if (integratedScore >= integrationThreshold)
                {
                    var detection = new IntegratedDetection
                    {
                        target = target,
                        integratedScore = integratedScore,
                        visualScore = visualScore,
                        auditoryScore = auditoryScore,
                        detectionTime = Time.time,
                        fusionConfidence = CalculateFusionConfidence(visualScore, auditoryScore)
                    };
                    
                    integratedDetections.Add(detection);
                    
                    // DetectionInfoの更新
                    UpdateTargetDetectionInfo(target, visualScore, auditoryScore);
                }
            }
            
            // 聴覚のみの検知も処理（視覚で確認できていないもの）
            foreach (var sound in auditoryTargets)
            {
                if (IsPositionVisuallyDetected(sound.estimatedPosition)) continue;
                
                float auditoryScore = sound.intensity;
                float integratedScore = auditoryScore * auditoryWeight;
                
                if (integratedScore >= integrationThreshold)
                {
                    var detection = new IntegratedDetection
                    {
                        estimatedPosition = sound.estimatedPosition,
                        integratedScore = integratedScore,
                        visualScore = 0f,
                        auditoryScore = auditoryScore,
                        detectionTime = Time.time,
                        fusionConfidence = 0.5f // 聴覚のみは信頼度中程度
                    };
                    
                    integratedDetections.Add(detection);
                }
            }
        }
        
        /// <summary>
        /// 統合スコア計算
        /// </summary>
        private float CalculateIntegratedScore(float visualScore, float auditoryScore)
        {
            switch (fusionMethod)
            {
                case SensorFusionMethod.WeightedAverage:
                    return (visualScore * visualWeight + auditoryScore * auditoryWeight);
                    
                case SensorFusionMethod.Maximum:
                    return Mathf.Max(visualScore, auditoryScore);
                    
                case SensorFusionMethod.DempsterShafer:
                    return CalculateDempsterShaferFusion(visualScore, auditoryScore);
                    
                case SensorFusionMethod.BayesianFusion:
                    return CalculateBayesianFusion(visualScore, auditoryScore);
                    
                default:
                    return (visualScore * visualWeight + auditoryScore * auditoryWeight);
            }
        }
        
        /// <summary>
        /// 融合信頼度計算
        /// </summary>
        private float CalculateFusionConfidence(float visualScore, float auditoryScore)
        {
            // 両方のセンサーが検知している場合は信頼度を向上
            if (visualScore > 0.1f && auditoryScore > 0.1f)
            {
                float baseConfidence = (visualScore + auditoryScore) * 0.5f;
                return Mathf.Min(1f, baseConfidence * confidenceBoost);
            }
            
            // 単一センサーの場合は通常信頼度
            return Mathf.Max(visualScore, auditoryScore);
        }
        
        #endregion
        
        #region Sensor Data Access
        
        private List<Transform> GetVisualDetections()
        {
            if (visualSensor == null) return new List<Transform>();
            
            var detectedTargets = visualSensor.GetDetectedTargets();
            var targets = new List<Transform>();
            
            foreach (var detection in detectedTargets)
            {
                if (detection.transform != null)
                    targets.Add(detection.transform);
            }
            
            return targets;
        }
        
        private List<DetectedSound> GetAuditoryDetections()
        {
            if (auditorySensor == null) return new List<DetectedSound>();
            
            return auditorySensor.GetRecentSounds();
        }
        
        private float GetVisualDetectionScore(Transform target)
        {
            if (visualSensor == null || target == null) return 0f;
            
            var detectedTargets = visualSensor.GetDetectedTargets();
            foreach (var detection in detectedTargets)
            {
                if (detection.transform == target)
                    return detection.detectionScore;
            }
            
            return 0f;
        }
        
        private float GetAuditoryDetectionScore(Vector3 position)
        {
            if (auditorySensor == null) return 0f;
            
            float maxScore = 0f;
            var recentSounds = auditorySensor.GetRecentSounds();
            
            foreach (var sound in recentSounds)
            {
                float distance = Vector3.Distance(position, sound.estimatedPosition);
                if (distance < 3f) // 3メートル以内なら同じ位置とみなす
                {
                    maxScore = Mathf.Max(maxScore, sound.intensity);
                }
            }
            
            return maxScore;
        }
        
        private bool IsPositionVisuallyDetected(Vector3 position)
        {
            var visualTargets = GetVisualDetections();
            foreach (var target in visualTargets)
            {
                if (Vector3.Distance(target.position, position) < 2f)
                    return true;
            }
            return false;
        }
        
        #endregion
        
        #region Alert System
        
        private void UpdateAlertLevel()
        {
            float maxScore = GetMaxDetectionScore();
            CoreAlertLevel newAlertLevel = CalculateAlertLevel(maxScore);
            
            if (newAlertLevel != currentAlertLevel)
            {
                previousAlertLevel = currentAlertLevel;
                currentAlertLevel = newAlertLevel;
                
                OnAlertLevelChanged();
            }
        }
        
        private CoreAlertLevel CalculateAlertLevel(float detectionScore)
        {
            if (detectionScore >= alertThreshold) return CoreAlertLevel.Alert;
            if (detectionScore >= integrationThreshold * 1.5f) return CoreAlertLevel.Searching;
            if (detectionScore >= integrationThreshold) return CoreAlertLevel.Investigating;
            if (detectionScore > 0.1f) return CoreAlertLevel.Suspicious;
            return CoreAlertLevel.Unaware;
        }
        
        private void OnAlertLevelChanged()
        {
            onAlertLevelChanged?.Raise();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=yellow>[NPCMultiSensorDetector]</color> {gameObject.name}: " +
                     $"Alert level changed from {previousAlertLevel} to {currentAlertLevel}");
            #endif
        }
        
        #endregion
        
        #region Utility Methods
        
        private float GetMaxDetectionScore()
        {
            float maxScore = 0f;
            foreach (var detection in integratedDetections)
            {
                maxScore = Mathf.Max(maxScore, detection.integratedScore);
            }
            return maxScore;
        }
        
        private void UpdateTargetDetectionInfo(Transform target, float visualScore, float auditoryScore)
        {
            DetectionInfo info = new DetectionInfo(visualScore, auditoryScore, target.position)
            {
                suspicionLevel = CalculateIntegratedScore(visualScore, auditoryScore),
                timeSinceLastSeen = 0f,
                detectorId = gameObject.GetInstanceID()
            };
            
            targetDetectionInfo[target] = info;
        }
        
        private void UpdateSensorCorrelation()
        {
            // 時間的相関の計算
            if (lastVisualDetectionTime > 0f && lastAuditoryDetectionTime > 0f)
            {
                float timeDiff = Mathf.Abs(lastVisualDetectionTime - lastAuditoryDetectionTime);
                if (timeDiff <= temporalWindow)
                {
                    sensorCorrelation = 1f - (timeDiff / temporalWindow);
                }
                else
                {
                    sensorCorrelation = 0f;
                }
            }
        }
        
        private void CleanupOldDetections()
        {
            integratedDetections.RemoveAll(detection => 
                Time.time - detection.detectionTime > temporalWindow);
                
            var keysToRemove = new List<Transform>();
            foreach (var kvp in targetDetectionInfo)
            {
                if (Time.time - kvp.Value.timeSinceLastSeen > temporalWindow)
                    keysToRemove.Add(kvp.Key);
            }
            
            foreach (var key in keysToRemove)
                targetDetectionInfo.Remove(key);
        }
        
        #endregion
        
        #region Advanced Fusion Methods
        
        private float CalculateDempsterShaferFusion(float visualScore, float auditoryScore)
        {
            // Dempster-Shafer理論に基づく証拠結合
            float m1_target = visualScore;
            float m1_unknown = 1f - visualScore;
            
            float m2_target = auditoryScore;
            float m2_unknown = 1f - auditoryScore;
            
            float k = m1_target * m2_target + m1_target * m2_unknown + m1_unknown * m2_target;
            
            if (k <= 0f) return 0f;
            
            return (m1_target * m2_target) / k;
        }
        
        private float CalculateBayesianFusion(float visualScore, float auditoryScore)
        {
            // ベイジアン融合
            float priorProbability = 0.1f; // 事前確率
            float visualLikelihood = visualScore * 0.9f + 0.1f;
            float auditoryLikelihood = auditoryScore * 0.9f + 0.1f;
            
            float posterior = (visualLikelihood * auditoryLikelihood * priorProbability) /
                            ((visualLikelihood * auditoryLikelihood * priorProbability) +
                             ((1f - visualLikelihood) * (1f - auditoryLikelihood) * (1f - priorProbability)));
            
            return Mathf.Clamp01(posterior);
        }
        
        #endregion
        
        #region Debug
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void UpdateDebugInfo()
        {
            // デバッグ情報の更新は既にフィールドに反映済み
        }
        
        private void OnDrawGizmosSelected()
        {
            // 統合検知の可視化
            foreach (var detection in integratedDetections)
            {
                Color color = Color.Lerp(Color.yellow, Color.red, detection.integratedScore);
                
                if (detection.target != null)
                {
                    Gizmos.color = color;
                    Gizmos.DrawWireSphere(detection.target.position, 1f);
                    Gizmos.DrawLine(transform.position, detection.target.position);
                }
                else
                {
                    Gizmos.color = new Color(color.r, color.g, color.b, 0.5f);
                    Gizmos.DrawWireCube(detection.estimatedPosition, Vector3.one * 0.8f);
                    Gizmos.DrawLine(transform.position, detection.estimatedPosition);
                }
            }
            
            // センサー範囲の表示
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            if (visualSensor != null)
                Gizmos.DrawWireSphere(transform.position, 15f); // 視覚範囲
                
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
            if (auditorySensor != null)
                Gizmos.DrawWireSphere(transform.position, 10f); // 聴覚範囲
        }
        #endif
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// 現在の警戒レベルを取得
        /// </summary>
        public CoreAlertLevel GetCurrentAlertLevel() => currentAlertLevel;
        
        /// <summary>
        /// 統合検知リストを取得
        /// </summary>
        public List<IntegratedDetection> GetIntegratedDetections() => new List<IntegratedDetection>(integratedDetections);
        
        /// <summary>
        /// 指定ターゲットの検知情報を取得
        /// </summary>
        public DetectionInfo? GetTargetDetectionInfo(Transform target)
        {
            return targetDetectionInfo.TryGetValue(target, out DetectionInfo info) ? info : null;
        }
        
        /// <summary>
        /// センサー融合メソッドを変更
        /// </summary>
        public void SetFusionMethod(SensorFusionMethod method)
        {
            fusionMethod = method;
        }
        
        #endregion
    }
    
    #region Supporting Classes and Enums
    
    /// <summary>
    /// 統合検知データ
    /// </summary>
    [System.Serializable]
    public class IntegratedDetection
    {
        public Transform target;
        public Vector3 estimatedPosition;
        public float integratedScore;
        public float visualScore;
        public float auditoryScore;
        public float detectionTime;
        public float fusionConfidence;
    }
    
    /// <summary>
    /// センサー融合方法
    /// </summary>
    public enum SensorFusionMethod
    {
        WeightedAverage,    // 重み付け平均
        Maximum,            // 最大値選択
        DempsterShafer,     // Dempster-Shafer理論
        BayesianFusion      // ベイジアン融合
    }
    
    #endregion
}