using UnityEngine;
using asterivo.Unity60.Features.AI.Configuration;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// NPCVisualSensorシステム全体の設定を管理するScriptableObject
    /// 各モジュールの動作パラメータとシステム設定を一元管理
    /// </summary>
    [CreateAssetMenu(menuName = "asterivo/AI/Visual Sensor Settings", fileName = "VisualSensorSettings")]
    public class VisualSensorSettings : ScriptableObject
    {
        [Title("NPCVisualSensor System Settings")]
        [InfoBox("NPCVisualSensorシステム全体の動作パラメータを設定します。各モジュールの詳細設定もここから管理されます。")]
        
        #region Core System Settings
        
        [TabGroup("Core", "System")]
        [BoxGroup("Core/System/Performance")]
        [PropertyRange(5f, 60f)]
        [LabelText("Update Frequency")]
        [SuffixLabel("Hz")]
        [InfoBox("視覚センサーの更新頻度。高いほど精密だが処理負荷が増加")]
        [SerializeField] private float updateFrequency = 20f;
        
        [BoxGroup("Core/System/Performance")]
        [PropertyRange(1f, 30f)]
        [LabelText("Max Detection Range")]
        [SuffixLabel("m")]
        [InfoBox("視覚検出の最大範囲。この範囲を超えた目標は処理対象外")]
        [SerializeField] private float maxDetectionRange = 25f;
        
        [BoxGroup("Core/System/Performance")]
        [PropertyRange(1, 50)]
        [LabelText("Max Scan Targets")]
        [InfoBox("単一フレームで処理する最大目標数")]
        [SerializeField] private int maxScanTargets = 20;
        
        [BoxGroup("Core/System/Performance")]
        [LabelText("Use Frame Distribution")]
        [InfoBox("処理を複数フレームに分散してパフォーマンスを向上")]
        [SerializeField] private bool useFrameDistribution = true;
        
        [BoxGroup("Core/System/Debug")]
        [LabelText("Enable Debug Logging")]
        [InfoBox("デバッグログを有効にする")]
        [SerializeField] private bool enableDebugLogging = false;
        
        [BoxGroup("Core/System/Debug")]
        [ShowIf("enableDebugLogging")]
        [EnumToggleButtons]
        [LabelText("Log Level")]
        [SerializeField] private LogLevel debugLogLevel = LogLevel.Info;
        
        [BoxGroup("Core/System/Debug")]
        [LabelText("Show Gizmos")]
        [InfoBox("Scene ViewでGizmosを表示する")]
        [SerializeField] private bool showGizmos = true;
        
        #endregion
        
        #region Detection Module Settings
        
        [TabGroup("Core", "Detection")]
        [BoxGroup("Core/Detection/Basic")]
        [Required]
        [LabelText("Detection Configuration")]
        [InfoBox("基本的な検出パラメータの設定")]
        [SerializeField] private AIDetectionConfiguration detectionConfiguration;
        
        [BoxGroup("Core/Detection/Scoring")]
        [PropertyRange(0.1f, 5f)]
        [LabelText("Detection Score Multiplier")]
        [InfoBox("全体的な検出スコアの倍率")]
        [SerializeField] private float detectionScoreMultiplier = 1f;
        
        [BoxGroup("Core/Detection/Scoring")]
        [PropertyRange(0.01f, 1f)]
        [LabelText("Min Detection Threshold")]
        [InfoBox("検出として認識する最小スコア")]
        [SerializeField] private float minDetectionThreshold = 0.1f;
        
        [BoxGroup("Core/Detection/Scoring")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Strong Detection Threshold")]
        [InfoBox("強い検出として認識するスコア閾値")]
        [SerializeField] private float strongDetectionThreshold = 0.7f;
        
        [BoxGroup("Core/Detection/LayerMask")]
        [LabelText("Target Layers")]
        [InfoBox("検出対象とするレイヤーマスク")]
        [SerializeField] private LayerMask targetLayers = 1;
        
        [BoxGroup("Core/Detection/LayerMask")]
        [LabelText("Obstruction Layers")]
        [InfoBox("視界を遮る障害物のレイヤーマスク")]
        [SerializeField] private LayerMask obstructionLayers = 1;
        
        #endregion
        
        #region Alert System Settings
        
        [TabGroup("Modules", "Alert")]
        [BoxGroup("Modules/Alert/Levels")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Alert Intensity Build Rate")]
        [SuffixLabel("/s")]
        [InfoBox("警戒度の上昇速度")]
        [SerializeField] private float alertIntensityBuildRate = 1f;
        
        [BoxGroup("Modules/Alert/Levels")]
        [PropertyRange(0.05f, 1f)]
        [LabelText("Alert Decay Rate")]
        [SuffixLabel("/s")]
        [InfoBox("警戒度の自然減衰速度")]
        [SerializeField] private float alertDecayRate = 0.2f;
        
        [BoxGroup("Modules/Alert/Levels")]
        [PropertyRange(1f, 30f)]
        [LabelText("Alert Decay Delay")]
        [SuffixLabel("s")]
        [InfoBox("検出終了から減衰開始までの遅延時間")]
        [SerializeField] private float alertDecayDelay = 3f;
        
        [BoxGroup("Modules/Alert/Thresholds")]
        [PropertyRange(0.1f, 0.5f)]
        [LabelText("Suspicious Threshold")]
        [InfoBox("疑念状態への遷移閾値")]
        [SerializeField] private float suspiciousThreshold = 0.25f;
        
        [BoxGroup("Modules/Alert/Thresholds")]
        [PropertyRange(0.3f, 0.8f)]
        [LabelText("Investigating Threshold")]
        [InfoBox("調査状態への遷移閾値")]
        [SerializeField] private float investigatingThreshold = 0.5f;
        
        [BoxGroup("Modules/Alert/Thresholds")]
        [PropertyRange(0.6f, 1f)]
        [LabelText("Alert Threshold")]
        [InfoBox("警戒状態への遷移閾値")]
        [SerializeField] private float alertThreshold = 0.75f;
        
        #endregion
        
        #region Memory System Settings
        
        [TabGroup("Modules", "Memory")]
        [BoxGroup("Modules/Memory/Duration")]
        [PropertyRange(2f, 15f)]
        [LabelText("Short Term Memory Duration")]
        [SuffixLabel("s")]
        [InfoBox("短期記憶の保持時間")]
        [SerializeField] private float shortTermMemoryDuration = 5f;
        
        [BoxGroup("Modules/Memory/Duration")]
        [PropertyRange(10f, 120f)]
        [LabelText("Long Term Memory Duration")]
        [SuffixLabel("s")]
        [InfoBox("長期記憶の保持時間")]
        [SerializeField] private float longTermMemoryDuration = 30f;
        
        [BoxGroup("Modules/Memory/Capacity")]
        [PropertyRange(5, 100)]
        [LabelText("Max Memory Entries")]
        [InfoBox("記憶できる最大エントリ数")]
        [SerializeField] private int maxMemoryEntries = 20;
        
        [BoxGroup("Modules/Memory/Prediction")]
        [PropertyRange(0.5f, 10f)]
        [LabelText("Position Prediction Time")]
        [SuffixLabel("s")]
        [InfoBox("位置予測の時間範囲")]
        [SerializeField] private float positionPredictionTime = 2f;
        
        [BoxGroup("Modules/Memory/Prediction")]
        [PropertyRange(1f, 20f)]
        [LabelText("Max Prediction Distance")]
        [SuffixLabel("m")]
        [InfoBox("位置予測の最大距離")]
        [SerializeField] private float maxPredictionDistance = 5f;
        
        [BoxGroup("Modules/Memory/Confidence")]
        [PropertyRange(0.05f, 0.5f)]
        [LabelText("Memory Confidence Decay")]
        [SuffixLabel("/s")]
        [InfoBox("記憶の信頼度減衰速度")]
        [SerializeField] private float memoryConfidenceDecay = 0.1f;
        
        #endregion
        
        #region Target Tracking Settings
        
        [TabGroup("Modules", "Tracking")]
        [BoxGroup("Modules/Tracking/Capacity")]
        [PropertyRange(1, 15)]
        [LabelText("Max Tracked Targets")]
        [InfoBox("同時追跡可能な最大目標数")]
        [SerializeField] private int maxTrackedTargets = 5;
        
        [BoxGroup("Modules/Tracking/Capacity")]
        [PropertyRange(1f, 30f)]
        [LabelText("Target Expiry Time")]
        [SuffixLabel("s")]
        [InfoBox("目標の有効期限")]
        [SerializeField] private float targetExpiryTime = 8f;
        
        [BoxGroup("Modules/Tracking/Updates")]
        [PropertyRange(0.5f, 10f)]
        [LabelText("Tracking Update Frequency")]
        [SuffixLabel("Hz")]
        [InfoBox("追跡システムの更新頻度")]
        [SerializeField] private float trackingUpdateFrequency = 2f;
        
        [BoxGroup("Modules/Tracking/Priority")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Priority Update Rate")]
        [SuffixLabel("/s")]
        [InfoBox("優先度計算の更新頻度")]
        [SerializeField] private float priorityUpdateRate = 0.5f;
        
        [BoxGroup("Modules/Tracking/Priority")]
        [PropertyRange(0.5f, 5f)]
        [LabelText("Distance Priority Weight")]
        [InfoBox("距離による優先度の重み")]
        [SerializeField] private float distancePriorityWeight = 2f;
        
        [BoxGroup("Modules/Tracking/Priority")]
        [PropertyRange(0.5f, 5f)]
        [LabelText("Movement Priority Weight")]
        [InfoBox("移動による優先度の重み")]
        [SerializeField] private float movementPriorityWeight = 1.5f;
        
        #endregion
        
        #region Integration Settings
        
        [TabGroup("Integration", "AI")]
        [BoxGroup("Integration/AI/Communication")]
        [LabelText("Auto AI State Machine Integration")]
        [InfoBox("AIStateMachineとの自動連携を有効にする")]
        [SerializeField] private bool autoAIStateMachineIntegration = true;
        
        [BoxGroup("Integration/AI/Communication")]
        [LabelText("Send State Change Events")]
        [InfoBox("状態変化イベントをAIStateMachineに送信")]
        [SerializeField] private bool sendStateChangeEvents = true;
        
        [BoxGroup("Integration/AI/Communication")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("State Communication Frequency")]
        [SuffixLabel("Hz")]
        [InfoBox("AIStateMachineとの通信頻度")]
        [SerializeField] private float stateCommunicationFrequency = 1f;
        
        [TabGroup("Integration", "Events")]
        [BoxGroup("Integration/Events/System")]
        [LabelText("Use Event Driven Architecture")]
        [InfoBox("イベント駆動アーキテクチャを使用")]
        [SerializeField] private bool useEventDrivenArchitecture = true;
        
        [BoxGroup("Integration/Events/System")]
        [ShowIf("useEventDrivenArchitecture")]
        [LabelText("Event Batch Processing")]
        [InfoBox("イベントのバッチ処理を使用")]
        [SerializeField] private bool eventBatchProcessing = true;
        
        [BoxGroup("Integration/Events/System")]
        [ShowIf("useEventDrivenArchitecture")]
        [PropertyRange(1, 100)]
        [LabelText("Max Events Per Frame")]
        [InfoBox("1フレームあたりの最大イベント数")]
        [SerializeField] private int maxEventsPerFrame = 10;
        
        #endregion
        
        #region Presets
        
        [TabGroup("Presets")]
        [BoxGroup("Presets/Quick Setup")]
        [Button("Load Performance Preset", ButtonSizes.Medium)]
        [InfoBox("パフォーマンス重視のプリセット")]
        private void LoadPerformancePreset()
        {
            updateFrequency = 10f;
            maxScanTargets = 10;
            useFrameDistribution = true;
            maxTrackedTargets = 3;
            trackingUpdateFrequency = 1f;
            
            Debug.Log("[VisualSensorSettings] Performance preset loaded");
        }
        
        [BoxGroup("Presets/Quick Setup")]
        [Button("Load Quality Preset", ButtonSizes.Medium)]
        [InfoBox("品質重視のプリセット")]
        private void LoadQualityPreset()
        {
            updateFrequency = 30f;
            maxScanTargets = 30;
            useFrameDistribution = false;
            maxTrackedTargets = 8;
            trackingUpdateFrequency = 4f;
            
            Debug.Log("[VisualSensorSettings] Quality preset loaded");
        }
        
        [BoxGroup("Presets/Quick Setup")]
        [Button("Load Balanced Preset", ButtonSizes.Medium)]
        [InfoBox("バランス型プリセット（推奨）")]
        private void LoadBalancedPreset()
        {
            updateFrequency = 20f;
            maxScanTargets = 20;
            useFrameDistribution = true;
            maxTrackedTargets = 5;
            trackingUpdateFrequency = 2f;
            
            Debug.Log("[VisualSensorSettings] Balanced preset loaded");
        }
        
        #endregion
        
        #region Properties
        
        // Core System Properties
        public float UpdateFrequency => updateFrequency;
        public float MaxDetectionRange => maxDetectionRange;
        public int MaxScanTargets => maxScanTargets;
        public bool UseFrameDistribution => useFrameDistribution;
        public bool EnableDebugLogging => enableDebugLogging;
        public LogLevel DebugLogLevel => debugLogLevel;
        public bool ShowGizmos => showGizmos;
        
        // Detection Properties
        public AIDetectionConfiguration DetectionConfiguration => detectionConfiguration;
        public float DetectionScoreMultiplier => detectionScoreMultiplier;
        public float MinDetectionThreshold => minDetectionThreshold;
        public float StrongDetectionThreshold => strongDetectionThreshold;
        public LayerMask TargetLayers => targetLayers;
        public LayerMask ObstructionLayers => obstructionLayers;
        
        // Alert System Properties
        public float AlertIntensityBuildRate => alertIntensityBuildRate;
        public float AlertDecayRate => alertDecayRate;
        public float AlertDecayDelay => alertDecayDelay;
        public float SuspiciousThreshold => suspiciousThreshold;
        public float InvestigatingThreshold => investigatingThreshold;
        public float AlertThreshold => alertThreshold;
        
        // Memory System Properties
        public float ShortTermMemoryDuration => shortTermMemoryDuration;
        public float LongTermMemoryDuration => longTermMemoryDuration;
        public int MaxMemoryEntries => maxMemoryEntries;
        public float PositionPredictionTime => positionPredictionTime;
        public float MaxPredictionDistance => maxPredictionDistance;
        public float MemoryConfidenceDecay => memoryConfidenceDecay;
        
        // Target Tracking Properties
        public int MaxTrackedTargets => maxTrackedTargets;
        public float TargetExpiryTime => targetExpiryTime;
        public float TrackingUpdateFrequency => trackingUpdateFrequency;
        public float PriorityUpdateRate => priorityUpdateRate;
        public float DistancePriorityWeight => distancePriorityWeight;
        public float MovementPriorityWeight => movementPriorityWeight;
        
        // Integration Properties
        public bool AutoAIStateMachineIntegration => autoAIStateMachineIntegration;
        public bool SendStateChangeEvents => sendStateChangeEvents;
        public float StateCommunicationFrequency => stateCommunicationFrequency;
        public bool UseEventDrivenArchitecture => useEventDrivenArchitecture;
        public bool EventBatchProcessing => eventBatchProcessing;
        public int MaxEventsPerFrame => maxEventsPerFrame;
        
        #endregion
        
        #region Reset and Validation
        
        private void Reset()
        {
            LoadBalancedPreset();
            
            // Set default layer masks
            targetLayers = LayerMask.NameToLayer("Player") != -1 ? 1 << LayerMask.NameToLayer("Player") : 1;
            obstructionLayers = LayerMask.NameToLayer("Default") != -1 ? 1 << LayerMask.NameToLayer("Default") : 1;
        }
        
        private void OnValidate()
        {
            // Clamp values to ensure consistency
            updateFrequency = Mathf.Clamp(updateFrequency, 5f, 60f);
            maxDetectionRange = Mathf.Clamp(maxDetectionRange, 1f, 50f);
            maxScanTargets = Mathf.Clamp(maxScanTargets, 1, 100);
            
            // Alert thresholds validation
            if (suspiciousThreshold >= investigatingThreshold)
                suspiciousThreshold = investigatingThreshold - 0.1f;
            
            if (investigatingThreshold >= alertThreshold)
                investigatingThreshold = alertThreshold - 0.1f;
            
            // Memory duration validation
            if (shortTermMemoryDuration >= longTermMemoryDuration)
                shortTermMemoryDuration = longTermMemoryDuration * 0.5f;
        }
        
        #endregion
        
        #region Enums
        
        public enum LogLevel
        {
            Error,
            Warning,
            Info,
            Verbose
        }
        
        #endregion
    }
}