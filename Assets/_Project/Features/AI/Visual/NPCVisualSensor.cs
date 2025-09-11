using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Stealth.Detection;
// using asterivo.Unity60.Features.AI.Audio; // TODO: Implement Audio module
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// NPCの視覚センサーシステム
    /// ステルスゲームにおける視覚検知と反応を管理
    /// 10-20Hz可変頻度での継続的な視界スキャンを実装
    /// </summary>
    public class NPCVisualSensor : MonoBehaviour
    {
        [TabGroup("Detection", "Core Settings")]
        [BoxGroup("Detection/Core Settings/Vision Range")]
        [PropertyRange(5f, 50f)]
        [LabelText("Sight Range")]
        [SuffixLabel("m")]
        [SerializeField] private float sightRange = 15f;
        
        [BoxGroup("Detection/Core Settings/Vision Range")]
        [PropertyRange(30f, 180f)]
        [LabelText("Field of View")]
        [SuffixLabel("°")]
        [SerializeField] private float fieldOfView = 110f;
        
        [BoxGroup("Detection/Core Settings/Vision Range")]
        [LabelText("Eye Position")]
        [SerializeField] private Transform eyePosition;
        
        [TabGroup("Detection", "Scan Settings")]
        [BoxGroup("Detection/Scan Settings/Frequency")]
        [PropertyRange(10f, 20f)]
        [LabelText("Base Scan Frequency")]
        [SuffixLabel("Hz")]
        [SerializeField] private float baseScanFrequency = 15f;
        
        [BoxGroup("Detection/Scan Settings/Frequency")]
        [PropertyRange(5f, 30f)]
        [LabelText("Alert Scan Frequency")]
        [SuffixLabel("Hz")]
        [SerializeField] private float alertScanFrequency = 20f;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [PropertyRange(1, 10)]
        [LabelText("Targets Per Frame")]
        [SerializeField] private int maxTargetsPerFrame = 3;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Frame Distribution Time")]
        [SuffixLabel("s")]
        [SerializeField] private float frameDistributionTime = 0.5f;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [LabelText("Enable LOD Optimization")]
        [SerializeField] private bool enableLODOptimization = true;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [PropertyRange(0.5f, 2f)]
        [LabelText("LOD Distance Multiplier")]
        [ShowIf("enableLODOptimization")]
        [SerializeField] private float lodDistanceMultiplier = 1.5f;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [LabelText("Enable Early Culling")]
        [SerializeField] private bool enableEarlyCulling = true;
        
        [BoxGroup("Detection/Scan Settings/Performance")]
        [LabelText("Use Memory Pool")]
        [SerializeField] private bool useMemoryPool = true;
        
        [TabGroup("Detection", "Thresholds")]
        [BoxGroup("Detection/Thresholds/Detection")]
        [ProgressBar(0f, 1f, ColorGetter = "GetDetectionColor")]
        [LabelText("Detection Threshold")]
        [SerializeField] private float detectionThreshold = 0.3f;
        
        [BoxGroup("Detection/Thresholds/Detection")]
        [ProgressBar(0f, 1f, ColorGetter = "GetSuspiciousColor")]
        [LabelText("Suspicious Threshold")]
        // TODO: Legacy threshold - replaced by AlertSystemModule settings
#pragma warning disable CS0414 // Field assigned but never used - replaced by AlertSystemModule
        [SerializeField] private float suspiciousThreshold = 0.5f;
#pragma warning restore CS0414
        
        [BoxGroup("Detection/Thresholds/Detection")]
        [ProgressBar(0f, 1f, ColorGetter = "GetAlertColor")]
        [LabelText("Alert Threshold")]
        // TODO: Legacy threshold - replaced by AlertSystemModule settings
#pragma warning disable CS0414 // Field assigned but never used - replaced by AlertSystemModule
        [SerializeField] private float alertThreshold = 0.8f;
#pragma warning restore CS0414
        
        [TabGroup("Detection", "Components")]
        [BoxGroup("Detection/Components/Required")]
        [Required]
        [LabelText("Visibility Calculator")]
        [SerializeField] private VisibilityCalculator visibilityCalculator;
        
        [BoxGroup("Detection/Components/Required")]
        [Required]
        [LabelText("Detection Configuration")]
        [SerializeField] private DetectionConfiguration detectionConfig;
        
        [BoxGroup("Detection/Components/Settings")]
        [LabelText("Visual Sensor Settings")]
        [InfoBox("Optional: システム全体の設定を統一管理する場合に使用")]
        [SerializeField] private VisualSensorSettings sensorSettings;
        
        [TabGroup("Detection", "Modules")]
        [BoxGroup("Detection/Modules/Alert System")]
        [LabelText("Alert System Module")]
        [SerializeField] private AlertSystemModule alertSystem = new AlertSystemModule();
        
        [BoxGroup("Detection/Modules/Memory System")]
        [LabelText("Memory Module")]
        [SerializeField] private MemoryModule memorySystem = new MemoryModule();
        
        [BoxGroup("Detection/Modules/Target Tracking")]
        [LabelText("Target Tracking Module")]
        [SerializeField] private TargetTrackingModule targetTracking = new TargetTrackingModule();
        
        [TabGroup("Detection", "Memory")]
        [BoxGroup("Detection/Memory/Settings")]
        [PropertyRange(1, 10)]
        [LabelText("Max Simultaneous Targets")]
        [SerializeField] private int maxSimultaneousTargets = 5;
        
        [TabGroup("Detection", "Events")]
        [BoxGroup("Detection/Events/Detection Events")]
        [LabelText("Target Spotted")]
        [SerializeField] private GameEvent onTargetSpotted;
        
        [BoxGroup("Detection/Events/Detection Events")]
        [LabelText("Target Lost")]
        [SerializeField] private GameEvent onTargetLost;
        
        [BoxGroup("Detection/Events/State Events")]
        [LabelText("Suspicious Activity")]
        [SerializeField] private GameEvent onSuspiciousActivity;
        
        [BoxGroup("Detection/Events/State Events")]
        [LabelText("Alert Level Changed")]
        [SerializeField] private GameEvent onAlertLevelChanged;
        
        [TabGroup("Debug", "Runtime Info")]
        [BoxGroup("Debug/Runtime Info/Current State")]
        [ShowInInspector, ReadOnly]
        [LabelText("Current Alert Level")]
        private AlertLevel currentAlertLevel = AlertLevel.Unaware;
        
        [BoxGroup("Debug/Runtime Info/Current State")]
        [ShowInInspector, ReadOnly]
        [LabelText("Active Targets")]
        private int activeTargetsCount;
        
        [BoxGroup("Debug/Runtime Info/Current State")]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0f, 1f)]
        [LabelText("Highest Detection Score")]
        private float highestDetectionScore;
        
        [BoxGroup("Debug/Runtime Info/Performance")]
        [ShowInInspector, ReadOnly]
        [LabelText("Current Scan Frequency")]
        [SuffixLabel("Hz")]
        private float currentScanFrequency;
        
        [BoxGroup("Debug/Runtime Info/Performance")]
        [ShowInInspector, ReadOnly]
        [LabelText("Targets Scanned This Frame")]
        private int targetsScannedThisFrame;
        
        [TabGroup("Debug", "Visualization")]
        [BoxGroup("Debug/Visualization/Gizmos")]
        [LabelText("Draw Sight Range")]
        [SerializeField] private bool drawSightRange = true;
        
        [BoxGroup("Debug/Visualization/Gizmos")]
        [LabelText("Draw Field of View")]
        [SerializeField] private bool drawFieldOfView = true;
        
        [BoxGroup("Debug/Visualization/Gizmos")]
        [LabelText("Draw Detection Rays")]
        [SerializeField] private bool drawDetectionRays = true;
        
        private List<DetectedTarget> detectedTargets = new List<DetectedTarget>();
        private List<Transform> potentialTargets = new List<Transform>();
        
        private Coroutine scanCoroutine;
        private float lastScanTime;
        // TODO: Frame-distributed scanning future enhancement
#pragma warning disable CS0414 // Field assigned but never used - planned for frame distribution optimization
        private int frameTargetIndex;
#pragma warning restore CS0414
        
        // Performance optimization fields
        private static readonly Queue<DetectedTarget> detectedTargetPool = new Queue<DetectedTarget>();
        private float lastCullingTime;
        private const float cullingInterval = 0.1f;
        private Dictionary<Transform, float> targetLODLevels = new Dictionary<Transform, float>();
        private Dictionary<Transform, float> lastTargetDistances = new Dictionary<Transform, float>();
        
        // Performance metrics
        [ShowInInspector, ReadOnly]
        [BoxGroup("Debug/Runtime Info/Performance")]
        [LabelText("Pooled Objects")]
        private int pooledObjectsCount = 0;
        
        [ShowInInspector, ReadOnly]
        [BoxGroup("Debug/Runtime Info/Performance")]
        [LabelText("Culled Targets")]
        private int culledTargetsThisFrame = 0;
        
        // Event optimization fields
        private VisualSensorEventManager eventManager;
        private const float eventCooldownTime = 0.1f; // イベント発行の最小間隔
        
        private VisualDetectionModule detectionModule;
        private AlertLevel previousAlertLevel;
        
        // AI State Machine連動
        private asterivo.Unity60.Features.AI.States.AIStateMachine aiStateMachine;
        
        // イベント発行用
        [Header("AI Integration")]
        [SerializeField] private AlertLevelEvent alertLevelEvent;
        [SerializeField] private AlertStateEvent alertStateEvent;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeComponents();
            InitializeDetectionModule();
            InitializeModules();
        }
        
        private void Start()
        {
            StartScanning();
            FindPotentialTargets();
        }
        
        private void Update()
        {
            UpdateModules();
            UpdateDebugInfo();
            
            // Event Managerの更新
            eventManager?.Update(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            StopScanning();
            
            // Event Managerのクリーンアップ
            eventManager?.Cleanup();
            
            // イベントハンドラのクリーンアップ
            if (alertSystem != null)
            {
                alertSystem.OnAlertLevelChanged -= OnAlertSystemLevelChanged;
                alertSystem.OnAlertStateChanged -= OnAlertSystemStateChanged;
            }
            
            if (memorySystem != null)
            {
                memorySystem.OnMemoryAdded -= OnMemoryAdded;
                memorySystem.OnTargetPositionPredicted -= OnTargetPositionPredicted;
            }
            
            if (targetTracking != null)
            {
                targetTracking.OnTargetAdded -= OnTargetTrackingTargetAdded;
                targetTracking.OnTargetRemoved -= OnTargetTrackingTargetRemoved;
                targetTracking.OnPrimaryTargetChanged -= OnTargetTrackingPrimaryChanged;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeComponents()
        {
            if (eyePosition == null)
                eyePosition = transform;
                
            if (visibilityCalculator == null)
                visibilityCalculator = GetComponent<VisibilityCalculator>();
        }
        
        private void InitializeDetectionModule()
        {
            detectionModule = new VisualDetectionModule(this, visibilityCalculator, detectionConfig);
        }
        
        private void InitializeModules()
        {
            // AI State Machineの取得
            aiStateMachine = GetComponent<asterivo.Unity60.Features.AI.States.AIStateMachine>();
            if (aiStateMachine == null)
            {
                Debug.LogWarning($"[NPCVisualSensor] {gameObject.name}: AIStateMachine component not found!");
            }
            
            // Performance optimizationの初期化
            InitializeMemoryPool();
            
            // Event Managerの初期化
            eventManager = new VisualSensorEventManager();
            eventManager.Initialize(this);
            
            // Alert System Moduleの初期化
            alertSystem.Initialize();
            alertSystem.OnAlertLevelChanged += OnAlertSystemLevelChanged;
            alertSystem.OnAlertStateChanged += OnAlertSystemStateChanged;
            
            // Memory Moduleの初期化
            memorySystem.Initialize();
            memorySystem.OnMemoryAdded += OnMemoryAdded;
            memorySystem.OnTargetPositionPredicted += OnTargetPositionPredicted;
            
            // Target Tracking Moduleの初期化
            targetTracking.Initialize(this.transform);
            targetTracking.OnTargetAdded += OnTargetTrackingTargetAdded;
            targetTracking.OnTargetRemoved += OnTargetTrackingTargetRemoved;
            targetTracking.OnPrimaryTargetChanged += OnTargetTrackingPrimaryChanged;
            
            Debug.Log($"[NPCVisualSensor] {gameObject.name}: All modules initialized");
        }
        
        private void UpdateModules()
        {
            float deltaTime = Time.deltaTime;
            
            // モジュールの更新
            alertSystem.UpdateAlertSystem(deltaTime);
            memorySystem.UpdateMemorySystem(deltaTime);
            targetTracking.UpdateTrackingSystem(deltaTime);
            
            // 既存の警戒レベル更新をモジュール経由に変更
            UpdateAlertLevelFromDetection();
            UpdateMemoryCleanup();
        }
        
        #endregion
        
        #region Scanning System
        
        private void StartScanning()
        {
            if (scanCoroutine != null)
                StopCoroutine(scanCoroutine);
                
            scanCoroutine = StartCoroutine(ContinuousScanCoroutine());
        }
        
        private void StopScanning()
        {
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }
        }
        
        private IEnumerator ContinuousScanCoroutine()
        {
            while (true)
            {
                UpdateScanFrequency();
                float scanInterval = 1f / currentScanFrequency;
                
                yield return new WaitForSeconds(scanInterval);
                
                PerformScan();
                lastScanTime = Time.time;
            }
        }
        
        private void UpdateScanFrequency()
        {
            currentScanFrequency = currentAlertLevel switch
            {
                AlertLevel.Alert => alertScanFrequency,
                AlertLevel.Investigating => Mathf.Lerp(baseScanFrequency, alertScanFrequency, 0.7f),
                AlertLevel.Suspicious => Mathf.Lerp(baseScanFrequency, alertScanFrequency, 0.5f),
                _ => baseScanFrequency
            };
        }
        
        private void PerformScan()
        {
            targetsScannedThisFrame = 0;
            frameTargetIndex = 0;
            culledTargetsThisFrame = 0;
            
            if (enableEarlyCulling && Time.time - lastCullingTime >= cullingInterval)
            {
                PerformEarlyCulling();
                lastCullingTime = Time.time;
            }
            
            StartCoroutine(FrameDistributedScan());
        }
        
        private IEnumerator FrameDistributedScan()
        {
            float timePerFrame = frameDistributionTime / Mathf.CeilToInt((float)potentialTargets.Count / maxTargetsPerFrame);
            
            for (int i = 0; i < potentialTargets.Count; i++)
            {
                if (targetsScannedThisFrame >= maxTargetsPerFrame)
                {
                    yield return new WaitForSeconds(timePerFrame);
                    targetsScannedThisFrame = 0;
                }
                
                if (potentialTargets[i] != null)
                {
                    ScanTarget(potentialTargets[i]);
                    targetsScannedThisFrame++;
                }
            }
        }
        
        #endregion
        
        #region Target Detection
        
        private void ScanTarget(Transform target)
        {
            // LOD最適化による検出スコア計算の調整
            float lodMultiplier = CalculateLODMultiplier(target);
            float detectionScore = detectionModule.CalculateDetectionScore(target) * lodMultiplier;
            DetectedTarget existingTarget = GetDetectedTarget(target);
            
            if (detectionScore >= detectionThreshold)
            {
                if (existingTarget == null)
                {
                    RegisterNewTarget(target, detectionScore);
                }
                else
                {
                    UpdateExistingTarget(existingTarget, detectionScore);
                }
                
                // Alert System Moduleに検出スコアを通知
                alertSystem.UpdateAlertLevel(detectionScore, target.position);
                
                // Memory Moduleに記憶を追加/強化
                int targetId = target.GetInstanceID();
                if (memorySystem.HasMemoryOf(targetId))
                {
                    memorySystem.ReinforceMemory(targetId, target.position, detectionScore * 0.1f);
                }
                else
                {
                    memorySystem.AddMemory(targetId, target.position, detectionScore, $"Visual detection of {target.name}", MemoryType.Visual);
                }
                
                // Target Tracking Moduleに目標を追加/更新
                if (existingTarget == null)
                {
                    targetTracking.AddTarget(target, detectionScore);
                }
                else
                {
                    targetTracking.UpdateTarget(target, detectionScore);
                }
            }
            else if (existingTarget != null)
            {
                RemoveDetectedTarget(existingTarget);
            }
        }
        
        private void RegisterNewTarget(Transform target, float score)
        {
            if (detectedTargets.Count >= maxSimultaneousTargets)
            {
                RemoveOldestTarget();
            }
            
            // メモリプールからDetectedTargetを取得または新規作成
            DetectedTarget detectedTarget;
            if (useMemoryPool && detectedTargetPool.Count > 0)
            {
                detectedTarget = detectedTargetPool.Dequeue();
                detectedTarget.ResetTarget(target, score, Time.time);
                pooledObjectsCount = detectedTargetPool.Count;
            }
            else
            {
                detectedTarget = new DetectedTarget(target, score, Time.time);
            }
            
            detectedTargets.Add(detectedTarget);
            
            // 最適化されたイベント発行
            eventManager.OnTargetSpotted(detectedTarget, score);
            onTargetSpotted?.Raise();
            
            // Target Tracking Moduleに目標を追加（既にScanTargetで処理済みのため、ここでは重複処理を回避）
            
            // AI State Machineに目標発見を通知
            if (aiStateMachine != null)
            {
                aiStateMachine.SetTarget(target);
                aiStateMachine.OnSightTarget(target);
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=yellow>[NPCVisualSensor]</color> {gameObject.name}: New target spotted - {target.name} (Score: {score:F2})");
            #endif
        }
        
        private void UpdateExistingTarget(DetectedTarget target, float score)
        {
            target.detectionScore = score;
            target.lastSeenTime = Time.time;
            target.lastKnownPosition = target.transform.position;
        }
        
        private void RemoveDetectedTarget(DetectedTarget target)
        {
            detectedTargets.Remove(target);
            
            // メモリプールに戻す
            if (useMemoryPool && detectedTargetPool.Count < 50) // プールサイズ制限
            {
                detectedTargetPool.Enqueue(target);
                pooledObjectsCount = detectedTargetPool.Count;
            }
            
            // 最適化されたイベント発行
            eventManager.OnTargetLost(target, Time.time - target.firstDetectedTime);
            onTargetLost?.Raise();
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=blue>[NPCVisualSensor]</color> {gameObject.name}: Target lost - {target.transform.name}");
            #endif
        }
        
        private void RemoveOldestTarget()
        {
            if (detectedTargets.Count > 0)
            {
                var oldest = detectedTargets[0];
                foreach (var target in detectedTargets)
                {
                    if (target.firstDetectedTime < oldest.firstDetectedTime)
                        oldest = target;
                }
                RemoveDetectedTarget(oldest);
            }
        }
        
        private DetectedTarget GetDetectedTarget(Transform target)
        {
            return detectedTargets.Find(dt => dt.transform == target);
        }
        
        #endregion
        
        #region Alert System
        
        private void UpdateAlertLevelFromDetection()
        {
            // 既存の警戒レベル更新ロジックはAlert System Moduleに統合済み
            // 現在の警戒レベルをAlert System Moduleから取得
            if (alertSystem.CurrentAlertLevel != currentAlertLevel)
            {
                previousAlertLevel = currentAlertLevel;
                currentAlertLevel = alertSystem.CurrentAlertLevel;
            }
        }
        
        private float GetHighestDetectionScore()
        {
            float highest = 0f;
            foreach (var target in detectedTargets)
            {
                if (target.detectionScore > highest)
                    highest = target.detectionScore;
            }
            return highest;
        }
        
        // Alert System Moduleからのイベントハンドラ
        private void OnAlertSystemLevelChanged(AlertLevel previous, AlertLevel current)
        {
            previousAlertLevel = previous;
            currentAlertLevel = current;
            
            // 最適化されたイベント発行
            eventManager.OnAlertLevelChanged(previous, current, alertSystem.AlertIntensity);
            
            // 既存のイベント発行
            onAlertLevelChanged?.Raise();
            
            // 新しいイベントシステムへの発行
            alertLevelEvent?.Raise(current);
            
            if (current > AlertLevel.Suspicious && previous <= AlertLevel.Suspicious)
            {
                eventManager.OnSuspiciousActivity(GetHighestPriorityTarget(), current);
                onSuspiciousActivity?.Raise();
            }
            
            // AI State Machineへの通知
            if (aiStateMachine != null)
            {
                aiStateMachine.IncreaseSuspicion(alertSystem.AlertIntensity);
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=orange>[NPCVisualSensor]</color> {gameObject.name}: Alert level changed from {previous} to {current} (Intensity: {alertSystem.AlertIntensity:F2})");
            #endif
        }
        
        private void OnAlertSystemStateChanged(AlertStateInfo stateInfo)
        {
            // Alert State Eventの発行
            alertStateEvent?.Raise(stateInfo);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=cyan>[NPCVisualSensor]</color> {gameObject.name}: Alert state changed - Level: {stateInfo.currentLevel}, Timer: {stateInfo.alertTimer:F2}s");
            #endif
        }
        
        #endregion
        
        #region Memory Management
        
        private void UpdateMemoryCleanup()
        {
            CleanupDetectedTargets();
            // Memory ModuleのクリーンアップはUpdateMemorySystem()で自動実行
        }
        
        private void CleanupDetectedTargets()
        {
            for (int i = detectedTargets.Count - 1; i >= 0; i--)
            {
                var target = detectedTargets[i];
                // Memory Moduleから経過時間を取得
                int targetId = target.transform.GetInstanceID();
                if (memorySystem.HasMemoryOf(targetId))
                {
                    float memoryConfidence = memorySystem.GetMemoryConfidence(targetId);
                    if (memoryConfidence < 0.1f) // 信頼度が低い場合は削除
                    {
                        RemoveDetectedTarget(target);
                    }
                }
                else if (Time.time - target.lastSeenTime > 5f) // メモリにない場合は5秒で削除
                {
                    RemoveDetectedTarget(target);
                }
            }
        }
        
        // Memory Moduleからのイベントハンドラ
        private void OnMemoryAdded(MemoryEntry memory)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=lightblue>[NPCVisualSensor]</color> {gameObject.name}: Memory added - Target {memory.targetId} at {memory.position} (confidence: {memory.confidence:F2})");
            #endif
        }
        
        private void OnTargetPositionPredicted(int targetId, Vector3 predictedPosition)
        {
            // AI State Machineに予測位置を通知
            if (aiStateMachine != null)
            {
                aiStateMachine.LastKnownPosition = predictedPosition;
            }
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"<color=magenta>[NPCVisualSensor]</color> {gameObject.name}: Predicted position for target {targetId}: {predictedPosition}");
            #endif
        }
        
        #endregion
        
        #region Potential Targets Management
        
        private void FindPotentialTargets()
        {
            potentialTargets.Clear();
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                potentialTargets.Add(playerObject.transform);
            }
            
            var allNPCs = GameObject.FindGameObjectsWithTag("NPC");
            foreach (var npc in allNPCs)
            {
                if (npc != gameObject)
                {
                    potentialTargets.Add(npc.transform);
                }
            }
        }
        
        #endregion
        
        #region Public Interface
        
        public AlertLevel GetCurrentAlertLevel() => currentAlertLevel;
        public List<DetectedTarget> GetDetectedTargets() => new List<DetectedTarget>(detectedTargets);
        public Transform EyePosition => eyePosition ?? transform;
        
        // 新しいMemory Moduleへのアクセス
        public MemoryModule GetMemoryModule() => memorySystem;
        public AlertSystemModule GetAlertSystemModule() => alertSystem;
        public TargetTrackingModule GetTargetTrackingModule() => targetTracking;
        public bool HasMemoryOf(Transform target) => target != null && memorySystem.HasMemoryOf(target.GetInstanceID());
        public Vector3 GetLastKnownPosition(Transform target) => target != null ? memorySystem.GetLastKnownPosition(target.GetInstanceID()) : Vector3.zero;
        public Vector3 GetPredictedPosition(Transform target) => target != null ? memorySystem.CalculatePredictedPosition(target.GetInstanceID()) : Vector3.zero;
        public float GetMemoryConfidence(Transform target) => target != null ? memorySystem.GetMemoryConfidence(target.GetInstanceID()) : 0f;
        
        // Target Tracking Moduleへのアクセス
        public DetectedTarget GetPrimaryTarget() => targetTracking.PrimaryTarget;
        public int GetActiveTargetCount() => targetTracking.ActiveTargetCount;
        public System.Collections.Generic.IReadOnlyList<DetectedTarget> GetTrackedTargets() => targetTracking.TrackedTargets;
        public bool CanTrackMoreTargets() => targetTracking.CanTrackMoreTargets;
        public Transform GetHighestPriorityTarget()
        {
            if (detectedTargets.Count == 0) return null;
            
            var highestScore = detectedTargets[0];
            foreach (var target in detectedTargets)
            {
                if (target.detectionScore > highestScore.detectionScore)
                    highestScore = target;
            }
            return highestScore.transform;
        }
        
        public bool CanSeeTarget(Transform target)
        {
            if (target == null) return false;
            
            float distance = Vector3.Distance(eyePosition.position, target.position);
            if (distance > sightRange) return false;
            
            if (!IsInFieldOfView(target.position)) return false;
            
            return HasLineOfSight(target.position);
        }
        
        private bool IsInFieldOfView(Vector3 position)
        {
            Vector3 directionToTarget = (position - eyePosition.position).normalized;
            float angle = Vector3.Angle(eyePosition.forward, directionToTarget);
            return angle <= fieldOfView / 2f;
        }
        
        private bool HasLineOfSight(Vector3 position)
        {
            Vector3 direction = position - eyePosition.position;
            return !Physics.Raycast(eyePosition.position, direction.normalized, direction.magnitude, 
                -1); // デフォルトで全レイヤーをチェック
        }
        
        #endregion
        
        #region Event Handlers
        
        // Target Tracking Module Event Handlers
        private void OnTargetTrackingTargetAdded(DetectedTarget target)
        {
            Debug.Log($"[NPCVisualSensor] Target added to tracking: {target.transform?.name}");
            
            // AIStateMachineに通知
            if (aiStateMachine != null && target.transform != null)
            {
                aiStateMachine.OnSightTarget(target.transform);
            }
        }
        
        private void OnTargetTrackingTargetRemoved(DetectedTarget target)
        {
            Debug.Log($"[NPCVisualSensor] Target removed from tracking: {target.transform?.name}");
        }
        
        private void OnTargetTrackingPrimaryChanged(DetectedTarget oldPrimary, DetectedTarget newPrimary)
        {
            Debug.Log($"[NPCVisualSensor] Primary target changed from {oldPrimary?.transform?.name ?? "None"} to {newPrimary?.transform?.name ?? "None"}");
            
            // AIStateMachineに新しいプライマリターゲットを設定
            if (aiStateMachine != null)
            {
                if (newPrimary?.transform != null)
                {
                    aiStateMachine.SetTarget(newPrimary.transform);
                }
                else
                {
                    aiStateMachine.ClearTarget();
                }
            }
        }
        
        #endregion
        
        #region Performance Optimization
        
        /// <summary>
        /// 早期カリング処理 - 検出範囲外や遮蔽された目標を事前に除外
        /// </summary>
        private void PerformEarlyCulling()
        {
            for (int i = potentialTargets.Count - 1; i >= 0; i--)
            {
                Transform target = potentialTargets[i];
                if (target == null)
                {
                    potentialTargets.RemoveAt(i);
                    culledTargetsThisFrame++;
                    continue;
                }
                
                // 距離による早期カリング
                float distance = Vector3.Distance(eyePosition.position, target.position);
                if (distance > sightRange * 1.2f) // 少し余裕を持たせる
                {
                    culledTargetsThisFrame++;
                    continue;
                }
                
                // 視野角による早期カリング
                if (!IsInFieldOfView(target.position))
                {
                    culledTargetsThisFrame++;
                    continue;
                }
                
                // 距離履歴の更新
                lastTargetDistances[target] = distance;
            }
        }
        
        /// <summary>
        /// LOD（Level of Detail）による検出スコア調整
        /// 距離に応じて検出精度を動的に調整
        /// </summary>
        private float CalculateLODMultiplier(Transform target)
        {
            if (!enableLODOptimization || target == null)
                return 1f;
                
            float distance = Vector3.Distance(eyePosition.position, target.position);
            float lodLevel = CalculateLODLevel(distance);
            targetLODLevels[target] = lodLevel;
            
            return lodLevel switch
            {
                >= 3f => 1f,      // 至近距離 - フル精度
                >= 2f => 0.8f,    // 近距離 - 高精度
                >= 1f => 0.6f,    // 中距離 - 中精度
                _ => 0.4f         // 遠距離 - 低精度
            };
        }
        
        /// <summary>
        /// 距離に基づくLODレベル計算
        /// </summary>
        private float CalculateLODLevel(float distance)
        {
            float normalizedDistance = distance / sightRange;
            return Mathf.Clamp(4f - (normalizedDistance * lodDistanceMultiplier), 0f, 4f);
        }
        
        /// <summary>
        /// メモリプールの事前初期化
        /// </summary>
        private void InitializeMemoryPool(int initialPoolSize = 20)
        {
            if (!useMemoryPool) return;
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                var target = new DetectedTarget(null, 0f, 0f);
                detectedTargetPool.Enqueue(target);
            }
            pooledObjectsCount = detectedTargetPool.Count;
            
            Debug.Log($"[NPCVisualSensor] Memory pool initialized with {initialPoolSize} objects");
        }
        
        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        public PerformanceStats GetPerformanceStats()
        {
            return new PerformanceStats
            {
                activeTargets = detectedTargets.Count,
                potentialTargets = potentialTargets.Count,
                pooledObjects = pooledObjectsCount,
                culledTargetsThisFrame = culledTargetsThisFrame,
                currentScanFrequency = currentScanFrequency,
                lodOptimizationEnabled = enableLODOptimization,
                earlyCullingEnabled = enableEarlyCulling,
                memoryPoolEnabled = useMemoryPool
            };
        }
        
        /// <summary>
        /// Event Manager統計の取得
        /// </summary>
        public EventManagerStats GetEventManagerStats()
        {
            return eventManager?.GetStats() ?? new EventManagerStats
            {
                eventsBuffered = 0,
                eventsSentThisFrame = 0,
                eventsSuppressed = 0,
                bufferEnabled = false,
                cooldownTime = 0f
            };
        }
        
        #endregion
        
        #region Debug
        
        private void UpdateDebugInfo()
        {
            activeTargetsCount = detectedTargets.Count;
            highestDetectionScore = GetHighestDetectionScore();
            pooledObjectsCount = detectedTargetPool.Count;
        }
        
        private Color GetDetectionColor() => Color.yellow;
        private Color GetSuspiciousColor() => new Color(1f, 0.65f, 0f);
        private Color GetAlertColor() => Color.red;
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmosSelected()
        {
            if (eyePosition == null) return;
            
            if (drawSightRange)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(eyePosition.position, sightRange);
            }
            
            if (drawFieldOfView)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
                Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * eyePosition.forward * sightRange;
                Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * eyePosition.forward * sightRange;
                
                Gizmos.DrawRay(eyePosition.position, leftBoundary);
                Gizmos.DrawRay(eyePosition.position, rightBoundary);
            }
            
            if (drawDetectionRays)
            {
                foreach (var target in detectedTargets)
                {
                    if (target.transform != null)
                    {
                        Color rayColor = Color.Lerp(Color.green, Color.red, target.detectionScore);
                        Gizmos.color = rayColor;
                        Gizmos.DrawLine(eyePosition.position, target.transform.position);
                        
                        Gizmos.DrawWireCube(target.transform.position, Vector3.one * 0.5f);
                    }
                }
            }
        }
        #endif
        
        #endregion
    }
    
    /// <summary>
    /// NPCVisualSensorのパフォーマンス統計情報
    /// </summary>
    [System.Serializable]
    public struct PerformanceStats
    {
        public int activeTargets;
        public int potentialTargets;
        public int pooledObjects;
        public int culledTargetsThisFrame;
        public float currentScanFrequency;
        public bool lodOptimizationEnabled;
        public bool earlyCullingEnabled;
        public bool memoryPoolEnabled;
    }
}