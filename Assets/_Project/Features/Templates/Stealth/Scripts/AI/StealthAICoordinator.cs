using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.AI;
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.AI.Audio;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Data;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;

namespace asterivo.Unity60.Features.Templates.Stealth.AI
{
    /// <summary>
    /// Layer 4: AI System Integration
    /// ステルスAI統合コーディネーター
    /// 既存のAI検知システム（NPCVisualSensor, NPCAuditorySensor, NPCMultiSensorDetector）を
    /// ステルステンプレート用に拡張・統合し、協調検知システムを実現
    /// Learn & Grow価値実現: 50体NPCの0.1ms/frame性能要件を満たしつつ高度なAI体験を提供
    /// </summary>
    public class StealthAICoordinator : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private StealthAIConfig _config;

        [Header("Performance Settings")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxProcessingPerFrame = 10;
        [SerializeField] private float _updateFrequency = 0.1f;

        [Header("Event Integration")]
        [SerializeField] private GameEvent _onDetectionEvent;
        [SerializeField] private GameEvent _onSuspicionLevelChanged;
        [SerializeField] private GameEvent _onCooperativeDetection;

        [Header("Debug Settings")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _enableAIVisualization = true;

        #endregion

        #region Properties

        /// <summary>登録されているNPCの総数</summary>
        public int RegisteredNPCCount => _registeredNPCs.Count;

        /// <summary>アクティブなNPCの総数</summary>
        public int ActiveNPCCount => _registeredNPCs.Count(npc => npc != null && npc.gameObject.activeInHierarchy);

        /// <summary>現在の平均疑心レベル</summary>
        public float AverageSuspicionLevel => _suspicionLevels.Count > 0 ? _suspicionLevels.Values.Average() : 0f;

        /// <summary>協調検知がアクティブかどうか</summary>
        public bool IsCooperativeDetectionActive { get; private set; } = true;

        #endregion

        #region Private Fields

        // Existing AI Systems Integration
        private readonly List<NPCVisualSensor> _visualSensors = new();
        private readonly List<NPCAuditorySensor> _auditorySensors = new();
        private readonly List<NPCMultiSensorDetector> _multiSensorDetectors = new();

        // NPC Management
        private readonly List<MonoBehaviour> _registeredNPCs = new();
        private readonly Dictionary<MonoBehaviour, StealthAIMemory> _npcMemories = new();
        private readonly Dictionary<MonoBehaviour, float> _suspicionLevels = new();
        private readonly Dictionary<MonoBehaviour, asterivo.Unity60.Core.Data.AlertLevel> _alertLevels = new();

        // Performance Optimization - 既存のObjectPool最適化を活用
        private readonly Queue<StealthDetectionEvent> _detectionEventPool = new();
        private readonly List<MonoBehaviour> _processingQueue = new();
        private int _currentProcessingIndex = 0;

        // Detection Coordination
        private readonly Dictionary<Transform, List<MonoBehaviour>> _targetDetectors = new();
        private readonly List<CooperativeDetectionData> _cooperativeDetections = new();

        // Update Management for 50 NPCs performance optimization
        private float _lastUpdateTime = 0f;
        private int _frameProcessingCount = 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializePerformanceSettings();
            InitializeDetectionEventPool();
        }

        private void Start()
        {
            DiscoverAndRegisterExistingNPCs();
            ValidateConfiguration();
        }

        private void Update()
        {
            if (!IsUpdateRequired()) return;

            _frameProcessingCount = 0;
            ProcessNPCsInBatches();
            ProcessCooperativeDetections();
            UpdateMemories();

            _lastUpdateTime = Time.time;
        }

        private void OnDestroy()
        {
            UnregisterAllNPCs();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// パフォーマンス設定の初期化
        /// 50体NPC・0.1ms/frame要件への最適化
        /// </summary>
        private void InitializePerformanceSettings()
        {
            if (_config != null)
            {
                _maxProcessingPerFrame = _config.MaxSimultaneousNPCs / 5; // 分散処理
                _updateFrequency = _config.MaxFrameTimeMs / 1000f; // ms→秒変換
            }
        }

        /// <summary>
        /// 検知イベントプールの初期化
        /// ObjectPool最適化による95%メモリ削減効果の活用
        /// </summary>
        private void InitializeDetectionEventPool()
        {
            // 初期プールサイズは最大NPC数の2倍
            int initialPoolSize = (_config?.MaxSimultaneousNPCs ?? 50) * 2;
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                _detectionEventPool.Enqueue(new StealthDetectionEvent());
            }
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        private void ValidateConfiguration()
        {
            if (_config == null)
            {
                Debug.LogError("StealthAICoordinator: StealthAIConfig is not assigned!");
                return;
            }

            if (_config.MaxSimultaneousNPCs > 50)
            {
                Debug.LogWarning($"StealthAICoordinator: MaxSimultaneousNPCs ({_config.MaxSimultaneousNPCs}) exceeds recommended limit (50)");
            }

            Debug.Log($"StealthAICoordinator: Initialized for {_config.MaxSimultaneousNPCs} NPCs with {_config.MaxFrameTimeMs}ms frame budget");
        }

        #endregion

        #region NPC Registration & Discovery

        /// <summary>
        /// 既存NPCの自動発見と登録
        /// </summary>
        private void DiscoverAndRegisterExistingNPCs()
        {
            // NPCMultiSensorDetectorを持つオブジェクトを自動登録
            var multiSensorNPCs = FindObjectsOfType<NPCMultiSensorDetector>();
            foreach (var npc in multiSensorNPCs)
            {
                RegisterNPC(npc);
            }

            // 単体センサーNPCも登録
            var visualOnlyNPCs = FindObjectsOfType<NPCVisualSensor>()
                .Where(v => v.GetComponent<NPCMultiSensorDetector>() == null);
            foreach (var npc in visualOnlyNPCs)
            {
                RegisterNPC(npc);
            }

            var auditoryOnlyNPCs = FindObjectsOfType<NPCAuditorySensor>()
                .Where(a => a.GetComponent<NPCMultiSensorDetector>() == null && 
                           a.GetComponent<NPCVisualSensor>() == null);
            foreach (var npc in auditoryOnlyNPCs)
            {
                RegisterNPC(npc);
            }

            Debug.Log($"StealthAICoordinator: Auto-registered {_registeredNPCs.Count} NPCs");
        }

        /// <summary>
        /// NPCの登録（外部からの登録もサポート）
        /// </summary>
        public void RegisterNPC(MonoBehaviour npcComponent)
        {
            if (npcComponent == null || _registeredNPCs.Contains(npcComponent))
                return;

            // 容量制限チェック
            if (_registeredNPCs.Count >= (_config?.MaxSimultaneousNPCs ?? 50))
            {
                Debug.LogWarning("StealthAICoordinator: Maximum NPC capacity reached!");
                return;
            }

            _registeredNPCs.Add(npcComponent);

            // センサーコンポーネントの登録
            RegisterSensorComponents(npcComponent);

            // ステルス固有データの初期化
            InitializeStealthAIData(npcComponent);

            Debug.Log($"StealthAICoordinator: Registered NPC {npcComponent.name}");
        }

        /// <summary>
        /// センサーコンポーネントの登録
        /// </summary>
        private void RegisterSensorComponents(MonoBehaviour npcComponent)
        {
            // Visual Sensor integration
            var visualSensor = npcComponent.GetComponent<NPCVisualSensor>();
            if (visualSensor != null)
            {
                _visualSensors.Add(visualSensor);
                // イベント駆動連携 - GameEventベースのため代替方法で実装
                // TODO: NPCVisualSensorのGameEventシステムと統合する適切な方法を実装
                Debug.Log($"Visual sensor registered for NPC: {npcComponent.name}");
            }

            // Auditory Sensor integration
            var auditorySensor = npcComponent.GetComponent<NPCAuditorySensor>();
            if (auditorySensor != null)
            {
                _auditorySensors.Add(auditorySensor);
                // イベント駆動連携 - GameEventベースのため代替方法で実装
                // TODO: NPCAuditorySensorのGameEventシステムと統合する適切な方法を実装
                Debug.Log($"Auditory sensor registered for NPC: {npcComponent.name}");
            }

            // Multi Sensor Detector integration
            var multiSensor = npcComponent.GetComponent<NPCMultiSensorDetector>();
            if (multiSensor != null)
            {
                _multiSensorDetectors.Add(multiSensor);
            }
        }

        /// <summary>
        /// ステルス固有AIデータの初期化
        /// </summary>
        private void InitializeStealthAIData(MonoBehaviour npcComponent)
        {
            float memoryRetention = _config?.MemoryRetentionTime ?? 30f;
            _npcMemories[npcComponent] = new StealthAIMemory(memoryRetention);
            _suspicionLevels[npcComponent] = 0f;
            _alertLevels[npcComponent] = asterivo.Unity60.Core.Data.AlertLevel.Unaware;
        }

        /// <summary>
        /// NPCの登録解除
        /// </summary>
        public void UnregisterNPC(MonoBehaviour npcComponent)
        {
            if (!_registeredNPCs.Contains(npcComponent))
                return;

            _registeredNPCs.Remove(npcComponent);
            UnregisterSensorComponents(npcComponent);

            // データクリーンアップ
            _npcMemories.Remove(npcComponent);
            _suspicionLevels.Remove(npcComponent);
            _alertLevels.Remove(npcComponent);

            Debug.Log($"StealthAICoordinator: Unregistered NPC {npcComponent.name}");
        }

        /// <summary>
        /// 全NPCの登録解除
        /// </summary>
        private void UnregisterAllNPCs()
        {
            var npcsToUnregister = new List<MonoBehaviour>(_registeredNPCs);
            foreach (var npc in npcsToUnregister)
            {
                UnregisterNPC(npc);
            }
        }

        /// <summary>
        /// センサーコンポーネントの登録解除
        /// </summary>
        private void UnregisterSensorComponents(MonoBehaviour npcComponent)
        {
            var visualSensor = npcComponent.GetComponent<NPCVisualSensor>();
            if (visualSensor != null)
            {
                _visualSensors.Remove(visualSensor);
                // GameEventベースのため、RemoveAllListeners()は不要
                Debug.Log($"Visual sensor unregistered for NPC: {npcComponent.name}");
            }

            var auditorySensor = npcComponent.GetComponent<NPCAuditorySensor>();
            if (auditorySensor != null)
            {
                _auditorySensors.Remove(auditorySensor);
                // GameEventベースのため、RemoveAllListeners()は不要
                Debug.Log($"Auditory sensor unregistered for NPC: {npcComponent.name}");
            }

            var multiSensor = npcComponent.GetComponent<NPCMultiSensorDetector>();
            if (multiSensor != null)
            {
                _multiSensorDetectors.Remove(multiSensor);
            }
        }

        #endregion

        #region Detection Handling

        /// <summary>
        /// 視覚検知の処理
        /// </summary>
        private void HandleVisualDetection(MonoBehaviour npcComponent, DetectedTarget target)
        {
            if (target?.transform == null) return;

            // ステルスレベルの計算
            float targetStealthLevel = CalculateTargetStealthLevel(target.transform);
            float detectionChance = CalculateDetectionChance(npcComponent, targetStealthLevel, DetectionType.Visual);

            // 検知判定
            if (detectionChance > Random.value)
            {
                ProcessDetection(npcComponent, target, DetectionType.Visual, detectionChance);
            }

            if (_showDebugInfo)
            {
                Debug.Log($"StealthAI: Visual detection by {npcComponent.name}, chance: {detectionChance:F2}, stealth: {targetStealthLevel:F2}");
            }
        }

        /// <summary>
        /// 聴覚検知の処理
        /// </summary>
        private void HandleAuditoryDetection(MonoBehaviour npcComponent)
        {
            // 簡易実装 - より詳細な実装は後で追加
            var auditorySensor = npcComponent.GetComponent<NPCAuditorySensor>();
            if (auditorySensor == null) return;

            // 音の情報から検知対象を特定（現在はプレイヤーを仮定）
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            var detectedTarget = new DetectedTarget(player.transform, 0.8f, Time.time);
            ProcessDetection(npcComponent, detectedTarget, DetectionType.Auditory, 0.8f);
        }

        /// <summary>
        /// 目標喪失の処理
        /// </summary>
        private void HandleTargetLost(MonoBehaviour npcComponent, DetectedTarget target)
        {
            if (!_npcMemories.TryGetValue(npcComponent, out var memory)) return;

            memory.RecordTargetLost(target, Time.time);
            
            // 疑心レベルの減衰
            if (_suspicionLevels.ContainsKey(npcComponent))
            {
                _suspicionLevels[npcComponent] = Mathf.Max(0f, _suspicionLevels[npcComponent] - 0.1f);
                UpdateAIAlertLevel(npcComponent, _suspicionLevels[npcComponent]);
            }
        }

        /// <summary>
        /// 検知の処理
        /// </summary>
        private void ProcessDetection(MonoBehaviour npcComponent, DetectedTarget target, DetectionType detectionType, float detectionStrength)
        {
            // 疑心レベルの更新
            float currentSuspicion = _suspicionLevels.GetValueOrDefault(npcComponent, 0f);
            float suspicionIncrease = CalculateSuspicionIncrease(detectionType, target, detectionStrength);
            _suspicionLevels[npcComponent] = Mathf.Clamp01(currentSuspicion + suspicionIncrease);

            // AI警戒レベルの更新
            UpdateAIAlertLevel(npcComponent, _suspicionLevels[npcComponent]);

            // メモリに記録
            if (_npcMemories.TryGetValue(npcComponent, out var memory))
            {
                memory.RecordDetection(target, detectionType, Time.time);
            }

            // 協調検知の処理
            ProcessCooperativeDetection(npcComponent, target, detectionType);

            // イベント通知（Event駆動アーキテクチャ）
            NotifyDetectionEvent(npcComponent, target, detectionType, _suspicionLevels[npcComponent]);

            if (_showDebugInfo)
            {
                Debug.Log($"StealthAI: {npcComponent.name} detected {target.transform.name} via {detectionType}, suspicion: {_suspicionLevels[npcComponent]:F2}");
            }
        }

        #endregion

        #region Stealth Calculation

        /// <summary>
        /// 目標のステルスレベル計算
        /// StealthMechanicsControllerとの連携
        /// </summary>
        private float CalculateTargetStealthLevel(Transform target)
        {
            var stealthController = target.GetComponent<StealthMechanicsController>();
            return stealthController?.CurrentStealthLevel ?? 0.5f; // デフォルト値
        }

        /// <summary>
        /// 検知確率の計算
        /// </summary>
        private float CalculateDetectionChance(MonoBehaviour npcComponent, float targetStealthLevel, DetectionType detectionType)
        {
            float baseChance = 1f - targetStealthLevel; // ステルスレベルが高いほど検知されにくい

            // 検知タイプによる修正
            float typeMultiplier = detectionType switch
            {
                DetectionType.Visual => 1.0f,      // 視覚は基準
                DetectionType.Auditory => 0.8f,    // 聴覚は少し弱い
                DetectionType.Environmental => 0.6f, // 環境手がかりは間接的
                DetectionType.Cooperative => 1.2f,   // 協調検知は強力
                _ => 1.0f
            };

            // NPC固有の能力補正
            float npcMultiplier = CalculateNPCDetectionMultiplier(npcComponent, detectionType);

            return Mathf.Clamp01(baseChance * typeMultiplier * npcMultiplier);
        }

        /// <summary>
        /// NPC固有の検知能力補正の計算
        /// </summary>
        private float CalculateNPCDetectionMultiplier(MonoBehaviour npcComponent, DetectionType detectionType)
        {
            float multiplier = 1f;

            // 現在の警戒レベルによる補正
            if (_alertLevels.TryGetValue(npcComponent, out var alertLevel))
            {
                multiplier *= alertLevel switch
                {
                    asterivo.Unity60.Core.Data.AlertLevel.Unaware => 0.8f,
                    asterivo.Unity60.Core.Data.AlertLevel.Suspicious => 1.0f,
                    asterivo.Unity60.Core.Data.AlertLevel.Alert => 1.2f,
                    asterivo.Unity60.Core.Data.AlertLevel.Combat => 1.5f,
                    _ => 1.0f
                };
            }

            return multiplier;
        }

        /// <summary>
        /// 疑心レベル増加量の計算
        /// </summary>
        private float CalculateSuspicionIncrease(DetectionType detectionType, DetectedTarget target, float detectionStrength)
        {
            float baseIncrease = detectionType switch
            {
                DetectionType.Visual => 0.3f,
                DetectionType.Auditory => 0.2f,
                DetectionType.Environmental => 0.1f,
                DetectionType.Cooperative => 0.4f,
                _ => 0.2f
            };

            return baseIncrease * detectionStrength;
        }

        #endregion

        #region AI State Management

        /// <summary>
        /// AI警戒レベルの更新
        /// 疑心レベルに基づく段階的状態遷移
        /// </summary>
        private void UpdateAIAlertLevel(MonoBehaviour npcComponent, float suspicionLevel)
        {
            asterivo.Unity60.Core.Data.AlertLevel newAlertLevel = suspicionLevel switch
            {
                <= 0.2f => asterivo.Unity60.Core.Data.AlertLevel.Unaware,
                <= 0.5f => asterivo.Unity60.Core.Data.AlertLevel.Suspicious,
                <= 0.7f => asterivo.Unity60.Core.Data.AlertLevel.Alert,
                _ => asterivo.Unity60.Core.Data.AlertLevel.Combat
            };

            if (_alertLevels.TryGetValue(npcComponent, out var currentLevel) && currentLevel != newAlertLevel)
            {
                _alertLevels[npcComponent] = newAlertLevel;
                
                // Multi Sensor Detector への通知
                var multiSensor = npcComponent.GetComponent<NPCMultiSensorDetector>();
                // TODO: SetAlertLevelメソッドがNPCMultiSensorDetectorに実装されていないため、代替実装が必要
                // multiSensor?.SetAlertLevel(newAlertLevel);

                if (_showDebugInfo)
                {
                    Debug.Log($"StealthAI: {npcComponent.name} alert level changed to {newAlertLevel}");
                }
            }
        }

        #endregion

        #region Cooperative Detection

        /// <summary>
        /// 協調検知の処理
        /// </summary>
        private void ProcessCooperativeDetection(MonoBehaviour detectingNPC, DetectedTarget target, DetectionType originalType)
        {
            if (!IsCooperativeDetectionActive || target?.transform == null) return;

            var cooperativeData = new CooperativeDetectionData
            {
                DetectingNPC = detectingNPC,
                Target = target.transform,
                OriginalDetectionType = originalType,
                DetectionTime = Time.time,
                SuspicionLevel = _suspicionLevels.GetValueOrDefault(detectingNPC, 0f)
            };

            _cooperativeDetections.Add(cooperativeData);

            // 近接NPCに情報共有
            ShareDetectionWithNearbyNPCs(detectingNPC, target.transform, cooperativeData);
        }

        /// <summary>
        /// 近接NPCとの検知情報共有
        /// </summary>
        private void ShareDetectionWithNearbyNPCs(MonoBehaviour detectingNPC, Transform target, CooperativeDetectionData data)
        {
            float shareRange = _config?.CooperativeDetectionRange ?? 20f;
            Vector3 detectingPos = detectingNPC.transform.position;

            foreach (var otherNPC in _registeredNPCs)
            {
                if (otherNPC == detectingNPC || otherNPC == null) continue;

                float distance = Vector3.Distance(detectingPos, otherNPC.transform.position);
                if (distance <= shareRange)
                {
                    // 情報を共有
                    ShareDetectionInfo(otherNPC, data);
                }
            }
        }

        /// <summary>
        /// 検知情報の共有
        /// </summary>
        private void ShareDetectionInfo(MonoBehaviour receivingNPC, CooperativeDetectionData data)
        {
            // 疑心レベルの軽微な上昇
            float currentSuspicion = _suspicionLevels.GetValueOrDefault(receivingNPC, 0f);
            float sharedSuspicionIncrease = data.SuspicionLevel * 0.3f; // 共有情報は30%の影響
            _suspicionLevels[receivingNPC] = Mathf.Clamp01(currentSuspicion + sharedSuspicionIncrease);

            // メモリに記録（協調情報として）
            if (_npcMemories.TryGetValue(receivingNPC, out var memory))
            {
                var sharedTarget = new DetectedTarget(data.Target, data.SuspicionLevel * 0.5f, data.DetectionTime);
                memory.RecordDetection(sharedTarget, DetectionType.Cooperative, data.DetectionTime);
            }

            // 警戒レベル更新
            UpdateAIAlertLevel(receivingNPC, _suspicionLevels[receivingNPC]);
        }

        /// <summary>
        /// 協調検知データの処理
        /// </summary>
        private void ProcessCooperativeDetections()
        {
            // 古い協調検知データのクリーンアップ
            float currentTime = Time.time;
            _cooperativeDetections.RemoveAll(data => currentTime - data.DetectionTime > 10f);
        }

        #endregion

        #region Performance Management

        /// <summary>
        /// 更新が必要かどうかの判定
        /// 50体NPC・0.1ms/frame性能最適化
        /// </summary>
        private bool IsUpdateRequired()
        {
            return Time.time - _lastUpdateTime >= _updateFrequency;
        }

        /// <summary>
        /// NPCのバッチ処理
        /// フレーム分散による性能最適化
        /// </summary>
        private void ProcessNPCsInBatches()
        {
            int processedThisFrame = 0;
            int totalNPCs = _registeredNPCs.Count;

            if (totalNPCs == 0) return;

            while (processedThisFrame < _maxProcessingPerFrame && processedThisFrame < totalNPCs)
            {
                if (_currentProcessingIndex >= totalNPCs)
                    _currentProcessingIndex = 0;

                var npc = _registeredNPCs[_currentProcessingIndex];
                if (npc != null && npc.gameObject.activeInHierarchy)
                {
                    ProcessNPCUpdate(npc);
                }

                _currentProcessingIndex++;
                processedThisFrame++;
                _frameProcessingCount++;
            }
        }

        /// <summary>
        /// 個別NPCの更新処理
        /// </summary>
        private void ProcessNPCUpdate(MonoBehaviour npc)
        {
            // 疑心レベルの自然減衰
            if (_suspicionLevels.ContainsKey(npc))
            {
                float decay = (_config?.SuspicionDecayRate ?? 0.1f) * _updateFrequency;
                _suspicionLevels[npc] = Mathf.Max(0f, _suspicionLevels[npc] - decay);
                
                UpdateAIAlertLevel(npc, _suspicionLevels[npc]);
            }
        }

        /// <summary>
        /// メモリシステムの更新
        /// </summary>
        private void UpdateMemories()
        {
            foreach (var memory in _npcMemories.Values)
            {
                memory?.UpdateMemory(Time.time);
            }
        }

        #endregion

        #region Event Management

        /// <summary>
        /// 検知イベントの通知
        /// ObjectPool最適化による効率的なイベント管理
        /// </summary>
        private void NotifyDetectionEvent(MonoBehaviour npcComponent, DetectedTarget target, DetectionType detectionType, float suspicionLevel)
        {
            var detectionEvent = GetPooledDetectionEvent();
            detectionEvent.Initialize(npcComponent, target, detectionType, suspicionLevel);

            // Event駆動アーキテクチャによる通知
            _onDetectionEvent?.Raise();

            // イベント使用後はプールに返却
            ReturnDetectionEventToPool(detectionEvent);
        }

        /// <summary>
        /// プールから検知イベントを取得
        /// </summary>
        private StealthDetectionEvent GetPooledDetectionEvent()
        {
            return _detectionEventPool.Count > 0 ? _detectionEventPool.Dequeue() : new StealthDetectionEvent();
        }

        /// <summary>
        /// 検知イベントをプールに返却
        /// </summary>
        private void ReturnDetectionEventToPool(StealthDetectionEvent detectionEvent)
        {
            detectionEvent.Reset();
            _detectionEventPool.Enqueue(detectionEvent);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 協調検知の有効/無効切り替え
        /// </summary>
        public void SetCooperativeDetectionEnabled(bool enabled)
        {
            IsCooperativeDetectionActive = enabled;
            Debug.Log($"StealthAI: Cooperative detection {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 全NPCの疑心レベルリセット
        /// </summary>
        public void ResetAllSuspicionLevels()
        {
            foreach (var npc in _registeredNPCs)
            {
                if (_suspicionLevels.ContainsKey(npc))
                {
                    _suspicionLevels[npc] = 0f;
                    UpdateAIAlertLevel(npc, 0f);
                }
            }
            Debug.Log("StealthAI: All suspicion levels reset");
        }

        /// <summary>
        /// 特定NPCの疑心レベル取得
        /// </summary>
        public float GetSuspicionLevel(MonoBehaviour npc)
        {
            return _suspicionLevels.GetValueOrDefault(npc, 0f);
        }

        /// <summary>
        /// 設定の更新
        /// </summary>
        public void UpdateConfiguration(StealthAIConfig newConfig)
        {
            _config = newConfig;
            InitializePerformanceSettings();
            ValidateConfiguration();
            Debug.Log("StealthAI: Configuration updated");
        }

        /// <summary>
        /// 設定の適用
        /// </summary>
        public void ApplyConfiguration(StealthAIConfig config)
        {
            if (config != null)
            {
                UpdateConfiguration(config);
                UnityEngine.Debug.Log($"[StealthAICoordinator] Configuration applied: {config.name}");
            }
        }

        /// <summary>
        /// パフォーマンス統計の取得
        /// </summary>
        public StealthAIPerformanceStats GetPerformanceStats()
        {
            return new StealthAIPerformanceStats
            {
                RegisteredNPCs = RegisteredNPCCount,
                ActiveNPCs = ActiveNPCCount,
                AverageSuspicionLevel = AverageSuspicionLevel,
                ProcessedThisFrame = _frameProcessingCount,
                PooledEvents = _detectionEventPool.Count
            };
        }

        /// <summary>
        /// 検知イベントの処理
        /// StealthTemplateManagerから呼び出される検知イベントハンドラ
        /// </summary>
        public void OnDetectionEvent(StealthDetectionEvent detectionEvent)
        {
            if (detectionEvent == null || !detectionEvent.IsValid)
            {
                LogDebug("Invalid detection event received");
                return;
            }

            LogDebug($"Processing detection event - Type: {detectionEvent.DetectionType}, Suspicion: {detectionEvent.SuspicionLevel:F2}, Position: {detectionEvent.DetectionPosition}");

            // NPCの疑心レベル更新
            if (detectionEvent.DetectingNPC != null && _suspicionLevels.ContainsKey(detectionEvent.DetectingNPC))
            {
                _suspicionLevels[detectionEvent.DetectingNPC] = detectionEvent.SuspicionLevel;
                UpdateAIAlertLevel(detectionEvent.DetectingNPC, detectionEvent.SuspicionLevel);
            }

            // 協調検知システムへの通知
            ShareDetectionWithNearbyNPCs(detectionEvent);
        }

        #endregion

        #region Debug & Visualization

        /// <summary>
        /// デバッグ情報をログ出力
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugging)
            {
                UnityEngine.Debug.Log($"[StealthAICoordinator] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!_enableAIVisualization || !Application.isPlaying) return;

            // NPC疑心レベルの視覚化
            foreach (var kvp in _suspicionLevels)
            {
                if (kvp.Key == null) continue;

                Vector3 npcPos = kvp.Key.transform.position;
                float suspicion = kvp.Value;

                // 疑心レベルに応じた色
                Color suspicionColor = Color.Lerp(Color.green, Color.red, suspicion);
                Gizmos.color = suspicionColor;
                Gizmos.DrawSphere(npcPos + Vector3.up * 3f, 0.2f + suspicion * 0.3f);
            }

            // 協調検知範囲の表示
            if (_config != null && IsCooperativeDetectionActive)
            {
                Gizmos.color = Color.yellow;
                float range = _config?.CooperativeDetectionRange ?? 20f;
                foreach (var npc in _registeredNPCs)
                {
                    if (npc != null)
                        Gizmos.DrawWireSphere(npc.transform.position, range);
                }
            }
        }

        #endregion
    }

    #region Supporting Data Structures

    /// <summary>
    /// ステルスAI記憶システム
    /// </summary>
    public class StealthAIMemory
    {
        private readonly float _retentionTime;
        private readonly List<MemoryEntry> _memories = new();

        public StealthAIMemory(float retentionTime)
        {
            _retentionTime = retentionTime;
        }

        public void RecordDetection(DetectedTarget target, DetectionType type, float time)
        {
            _memories.Add(new MemoryEntry
            {
                Target = target,
                DetectionType = type,
                Timestamp = time,
                Reliability = CalculateReliability(type)
            });
        }

        public void RecordTargetLost(DetectedTarget target, float time)
        {
            var existingMemory = _memories.FirstOrDefault(m => m.Target?.transform == target?.transform);
            if (existingMemory != null)
            {
                existingMemory.LastSeenTime = time;
            }
        }

        public void UpdateMemory(float currentTime)
        {
            _memories.RemoveAll(m => currentTime - m.Timestamp > _retentionTime);
        }

        private float CalculateReliability(DetectionType type)
        {
            return type switch
            {
                DetectionType.Visual => 0.9f,
                DetectionType.Auditory => 0.7f,
                DetectionType.Environmental => 0.5f,
                DetectionType.Cooperative => 0.6f,
                _ => 0.5f
            };
        }

        private class MemoryEntry
        {
            public DetectedTarget Target;
            public DetectionType DetectionType;
            public float Timestamp;
            public float LastSeenTime;
            public float Reliability;
        }
    }

    /// <summary>
    /// 協調検知データ
    /// </summary>
    public class CooperativeDetectionData
    {
        public MonoBehaviour DetectingNPC;
        public Transform Target;
        public DetectionType OriginalDetectionType;
        public float DetectionTime;
        public float SuspicionLevel;
    }

    /// <summary>
    /// ステルスAIパフォーマンス統計
    /// </summary>
    public class StealthAIPerformanceStats
    {
        public int RegisteredNPCs;
        public int ActiveNPCs;
        public float AverageSuspicionLevel;
        public int ProcessedThisFrame;
        public int PooledEvents;
    }

    #endregion
}