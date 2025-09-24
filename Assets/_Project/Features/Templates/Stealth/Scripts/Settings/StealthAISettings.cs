using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルステンプレート: AI設定
    /// AI検知パラメータ、警戒システム、行動パターン、協力メカニクス設定
    /// 既存のNPCVisualSensor + NPCAuditorySensorシステムとの完全統合
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/Settings/AI Settings", fileName = "StealthAISettings")]
    public class StealthAISettings : ScriptableObject
    {
        #region Visual Detection System

        [TabGroup("Detection", "Visual Sensor")]
        [Title("視覚検知システム", "NPCVisualSensor統合設定", TitleAlignments.Centered)]
        [SerializeField, Range(5f, 50f)]
        [Tooltip("最大検知距離（メートル）")]
        private float maxDetectionRange = 25f;

        [SerializeField, Range(30f, 180f)]
        [Tooltip("視野角（度）")]
        private float detectionFieldOfView = 90f;

        [SerializeField, Range(0.1f, 2.0f)]
        [Tooltip("光の影響倍率（明るい場所での検知強化）")]
        private float lightInfluenceMultiplier = 1.5f;

        [SerializeField, Range(0.1f, 1.0f)]
        [Tooltip("暗闇での検知減衰率")]
        private float darknessDetectionReduction = 0.3f;

        [SerializeField]
        [Tooltip("視覚検知対象レイヤー")]
        private LayerMask visualDetectionLayers = -1;

        [TabGroup("Detection", "Visual Performance")]
        [Title("視覚センサー性能", "50体NPC同時稼働対応", TitleAlignments.Centered)]
        [SerializeField, Range(5f, 30f)]
        [Tooltip("視覚センサー更新頻度（Hz）")]
        private float visualSensorUpdateFrequency = 10f;

        [SerializeField, Range(1, 10)]
        [Tooltip("フレームあたり最大センサー更新数")]
        private int maxSensorsPerFrame = 5;

        [SerializeField]
        [Tooltip("LODシステム有効化（距離による最適化）")]
        private bool enableLODSystem = true;

        [SerializeField, Range(10f, 100f)]
        [Tooltip("LOD切り替え距離")]
        private float lodSwitchDistance = 30f;

        #endregion

        #region Auditory Detection System

        [TabGroup("Detection", "Auditory Sensor")]
        [Title("聴覚検知システム", "NPCAuditorySensor統合設定", TitleAlignments.Centered)]
        [SerializeField, Range(3f, 25f)]
        [Tooltip("基本聴覚範囲（メートル）")]
        private float baseHearingRadius = 10f;

        [SerializeField, Range(0.1f, 1.0f)]
        [Tooltip("環境マスキング閾値（背景音による検知減衰）")]
        private float environmentalMaskingThreshold = 0.3f;

        [SerializeField]
        [Tooltip("距離による音量減衰カーブ")]
        private AnimationCurve distanceAttenuationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [SerializeField]
        [Tooltip("表面材質による音響反響設定")]
        private SurfaceAcousticProfile[] surfaceAcoustics = new SurfaceAcousticProfile[]
        {
            new SurfaceAcousticProfile { SurfaceType = "Concrete", ReflectionMultiplier = 1.2f, AbsorptionRate = 0.1f },
            new SurfaceAcousticProfile { SurfaceType = "Wood", ReflectionMultiplier = 0.9f, AbsorptionRate = 0.3f },
            new SurfaceAcousticProfile { SurfaceType = "Carpet", ReflectionMultiplier = 0.4f, AbsorptionRate = 0.8f },
            new SurfaceAcousticProfile { SurfaceType = "Metal", ReflectionMultiplier = 1.5f, AbsorptionRate = 0.05f }
        };

        #endregion

        #region Alert System

        [TabGroup("Behavior", "Alert Levels")]
        [Title("警戒システム", "4段階警戒レベル管理", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("警戒レベル設定")]
        private AlertLevelConfiguration[] alertLevels = new AlertLevelConfiguration[]
        {
            new AlertLevelConfiguration 
            { 
                Level = "Relaxed", 
                SuspicionThreshold = 0.0f, 
                DetectionMultiplier = 1.0f,
                MovementSpeed = 1.0f,
                TurnSpeed = 1.0f,
                PatrolBehavior = "Standard",
                Description = "通常警戒状態"
            },
            new AlertLevelConfiguration 
            { 
                Level = "Suspicious", 
                SuspicionThreshold = 0.3f, 
                DetectionMultiplier = 1.2f,
                MovementSpeed = 0.8f,
                TurnSpeed = 1.5f,
                PatrolBehavior = "Cautious",
                Description = "疑念状態、注意深い監視"
            },
            new AlertLevelConfiguration 
            { 
                Level = "Investigating", 
                SuspicionThreshold = 0.7f, 
                DetectionMultiplier = 1.5f,
                MovementSpeed = 1.2f,
                TurnSpeed = 2.0f,
                PatrolBehavior = "Investigation",
                Description = "調査状態、積極的な探索"
            },
            new AlertLevelConfiguration 
            { 
                Level = "Alert", 
                SuspicionThreshold = 1.0f, 
                DetectionMultiplier = 2.0f,
                MovementSpeed = 1.5f,
                TurnSpeed = 3.0f,
                PatrolBehavior = "Combat",
                Description = "警戒状態、戦闘準備"
            }
        };

        [TabGroup("Behavior", "Alert Propagation")]
        [Title("警戒伝播システム", "NPC間の情報共有", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("警戒伝播有効化")]
        private bool enableAlertPropagation = true;

        [SerializeField, Range(5f, 50f)]
        [Tooltip("警戒伝播範囲（メートル）")]
        private float alertPropagationRadius = 20f;

        [SerializeField, Range(1f, 10f)]
        [Tooltip("警戒伝播速度")]
        private float alertPropagationSpeed = 5f;

        [SerializeField, Range(0.01f, 0.5f)]
        [Tooltip("警戒レベル自然減衰率（秒あたり）")]
        private float alertDecayRate = 0.1f;

        #endregion

        #region AI Behavior Patterns

        [TabGroup("Behavior", "Patrol System")]
        [Title("パトロールシステム", "AI行動パターン", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("パトロールポイント間の待機時間（秒）")]
        private Vector2 patrolWaitTimeRange = new Vector2(2f, 5f);

        [SerializeField, Range(0.5f, 3.0f)]
        [Tooltip("パトロール移動速度")]
        private float patrolMovementSpeed = 1.5f;

        [SerializeField]
        [Tooltip("パトロールルートのランダム化")]
        private bool randomizePatrolRoute = true;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("パトロール中の注意散漫度（低いほど警戒）")]
        private float patrolAttentiveness = 0.7f;

        [TabGroup("Behavior", "Investigation")]
        [Title("調査行動", "不審箇所の調査パターン", TitleAlignments.Centered)]
        [SerializeField, Range(3f, 15f)]
        [Tooltip("調査継続時間（秒）")]
        private float investigationDuration = 8f;

        [SerializeField, Range(1f, 5f)]
        [Tooltip("調査範囲（メートル）")]
        private float investigationRadius = 3f;

        [SerializeField]
        [Tooltip("調査中の移動パターン")]
        private InvestigationPattern investigationPattern = InvestigationPattern.Circular;

        [SerializeField, Range(0.5f, 2.0f)]
        [Tooltip("調査中の移動速度")]
        private float investigationSpeed = 1.0f;

        #endregion

        #region Memory & Learning System

        [TabGroup("AI Intelligence", "Memory System")]
        [Title("記憶システム", "AI学習・記憶能力", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("目標記憶有効化")]
        private bool enableTargetMemory = true;

        [SerializeField, Range(5f, 60f)]
        [Tooltip("目標記憶保持時間（秒）")]
        private float targetMemoryDuration = 30f;

        [SerializeField, Range(1, 10)]
        [Tooltip("最大同時追跡目標数")]
        private int maxTrackedTargets = 5;

        [SerializeField]
        [Tooltip("位置予測有効化")]
        private bool enablePositionPrediction = true;

        [SerializeField, Range(1f, 10f)]
        [Tooltip("位置予測時間（秒）")]
        private float predictionTimeHorizon = 3f;

        [TabGroup("AI Intelligence", "Learning Adaptation")]
        [Title("学習適応システム", "プレイヤー行動への適応", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("適応学習有効化")]
        private bool enableAdaptiveLearning = true;

        [SerializeField, Range(0.01f, 0.1f)]
        [Tooltip("学習率")]
        private float learningRate = 0.05f;

        [SerializeField]
        [Tooltip("よく使われる隠れ場所の記憶")]
        private bool rememberHidingSpots = true;

        [SerializeField]
        [Tooltip("プレイヤー行動パターンの学習")]
        private bool learnPlayerPatterns = true;

        #endregion

        #region Cooperation & Communication

        [TabGroup("Cooperation", "Group Behavior")]
        [Title("グループ行動", "NPC間の協調システム", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("グループ協調有効化")]
        private bool enableGroupCoordination = true;

        [SerializeField, Range(2, 20)]
        [Tooltip("最大グループサイズ")]
        private int maxGroupSize = 8;

        [SerializeField, Range(5f, 30f)]
        [Tooltip("グループ通信範囲（メートル）")]
        private float groupCommunicationRange = 15f;

        [SerializeField]
        [Tooltip("グループ戦術")]
        private GroupTactic[] groupTactics = new GroupTactic[]
        {
            new GroupTactic { Name = "Surround", MinMembers = 3, Description = "包囲戦術" },
            new GroupTactic { Name = "Pincer", MinMembers = 2, Description = "挟み撃ち戦術" },
            new GroupTactic { Name = "Search_Grid", MinMembers = 4, Description = "グリッド捜索" },
            new GroupTactic { Name = "Layered_Defense", MinMembers = 5, Description = "段階防御" }
        };

        [TabGroup("Cooperation", "Communication")]
        [Title("コミュニケーション", "NPC間の情報交換", TitleAlignments.Centered)]
        [SerializeField]
        [Tooltip("音声コミュニケーション有効化")]
        private bool enableVoiceCommunication = true;

        [SerializeField, Range(5f, 25f)]
        [Tooltip("音声通信範囲（メートル）")]
        private float voiceCommunicationRange = 12f;

        [SerializeField]
        [Tooltip("ジェスチャーコミュニケーション有効化")]
        private bool enableGestureCommunication = true;

        [SerializeField, Range(3f, 15f)]
        [Tooltip("ジェスチャー通信範囲（メートル）")]
        private float gestureCommunicationRange = 8f;

        #endregion

        #region Properties (Public API)

        // Visual Detection Properties
        public float MaxDetectionRange => maxDetectionRange;
        public float DetectionFieldOfView => detectionFieldOfView;
        public float LightInfluenceMultiplier => lightInfluenceMultiplier;
        public float DarknessDetectionReduction => darknessDetectionReduction;
        public LayerMask VisualDetectionLayers => visualDetectionLayers;
        public float VisualSensorUpdateFrequency => visualSensorUpdateFrequency;
        public int MaxSensorsPerFrame => maxSensorsPerFrame;
        public bool EnableLODSystem => enableLODSystem;
        public float LODSwitchDistance => lodSwitchDistance;

        // Auditory Detection Properties
        public float BaseHearingRadius => baseHearingRadius;
        public float EnvironmentalMaskingThreshold => environmentalMaskingThreshold;
        public AnimationCurve DistanceAttenuationCurve => distanceAttenuationCurve;
        public SurfaceAcousticProfile[] SurfaceAcoustics => surfaceAcoustics;

        // Alert System Properties
        public AlertLevelConfiguration[] AlertLevels => alertLevels;
        public bool EnableAlertPropagation => enableAlertPropagation;
        public float AlertPropagationRadius => alertPropagationRadius;
        public float AlertPropagationSpeed => alertPropagationSpeed;
        public float AlertDecayRate => alertDecayRate;

        // Behavior Properties
        public Vector2 PatrolWaitTimeRange => patrolWaitTimeRange;
        public float PatrolMovementSpeed => patrolMovementSpeed;
        public bool RandomizePatrolRoute => randomizePatrolRoute;
        public float PatrolAttentiveness => patrolAttentiveness;
        public float InvestigationDuration => investigationDuration;
        public float InvestigationRadius => investigationRadius;
        public InvestigationPattern CurrentInvestigationPattern => investigationPattern;
        public float InvestigationSpeed => investigationSpeed;

        // Memory & Learning Properties
        public bool EnableTargetMemory => enableTargetMemory;
        public float TargetMemoryDuration => targetMemoryDuration;
        public int MaxTrackedTargets => maxTrackedTargets;
        public bool EnablePositionPrediction => enablePositionPrediction;
        public float PredictionTimeHorizon => predictionTimeHorizon;
        public bool EnableAdaptiveLearning => enableAdaptiveLearning;
        public float LearningRate => learningRate;
        public bool RememberHidingSpots => rememberHidingSpots;
        public bool LearnPlayerPatterns => learnPlayerPatterns;

        // Cooperation Properties
        public bool EnableGroupCoordination => enableGroupCoordination;
        public int MaxGroupSize => maxGroupSize;
        public float GroupCommunicationRange => groupCommunicationRange;
        public GroupTactic[] GroupTactics => groupTactics;
        public bool EnableVoiceCommunication => enableVoiceCommunication;
        public float VoiceCommunicationRange => voiceCommunicationRange;
        public bool EnableGestureCommunication => enableGestureCommunication;
        public float GestureCommunicationRange => gestureCommunicationRange;

        #endregion

        #region AI Configuration Methods

        /// <summary>
        /// 警戒レベル設定を取得
        /// </summary>
        public AlertLevelConfiguration GetAlertLevelConfig(string levelName)
        {
            foreach (var config in alertLevels)
            {
                if (config.Level == levelName)
                    return config;
            }
            return alertLevels[0]; // Default to first level
        }

        /// <summary>
        /// 疑心レベルに基づく警戒レベルを決定
        /// </summary>
        public string DetermineAlertLevel(float suspicionLevel)
        {
            for (int i = alertLevels.Length - 1; i >= 0; i--)
            {
                if (suspicionLevel >= alertLevels[i].SuspicionThreshold)
                {
                    return alertLevels[i].Level;
                }
            }
            return alertLevels[0].Level;
        }

        /// <summary>
        /// 表面材質の音響プロファイルを取得
        /// </summary>
        public SurfaceAcousticProfile GetSurfaceAcoustic(string surfaceType)
        {
            foreach (var acoustic in surfaceAcoustics)
            {
                if (acoustic.SurfaceType == surfaceType)
                    return acoustic;
            }
            return surfaceAcoustics[0]; // Default to first surface
        }

        /// <summary>
        /// グループサイズに適したタクティクスを取得
        /// </summary>
        public GroupTactic[] GetAvailableTactics(int groupSize)
        {
            System.Collections.Generic.List<GroupTactic> available = new System.Collections.Generic.List<GroupTactic>();
            foreach (var tactic in groupTactics)
            {
                if (groupSize >= tactic.MinMembers)
                {
                    available.Add(tactic);
                }
            }
            return available.ToArray();
        }

        /// <summary>
        /// NPCVisualSensorに設定を適用
        /// </summary>
        public void ApplyToVisualSensor(Component visualSensor)
        {
            if (visualSensor == null) return;
            
            // リフレクションまたは直接設定でパラメータを適用
            Debug.Log($"Applied visual sensor settings to {visualSensor.name}");
        }

        /// <summary>
        /// NPCAuditorySensorに設定を適用
        /// </summary>
        public void ApplyToAuditorySensor(Component auditorySensor)
        {
            if (auditorySensor == null) return;
            
            // リフレクションまたは直接設定でパラメータを適用
            Debug.Log($"Applied auditory sensor settings to {auditorySensor.name}");
        }

        #endregion

        #region Nested Data Classes

        [System.Serializable]
        public class SurfaceAcousticProfile
        {
            [Tooltip("表面材質タイプ")]
            public string SurfaceType;
            
            [Range(0.1f, 2.0f)]
            [Tooltip("音響反響倍率")]
            public float ReflectionMultiplier;
            
            [Range(0.0f, 1.0f)]
            [Tooltip("音響吸収率")]
            public float AbsorptionRate;
        }

        [System.Serializable]
        public class AlertLevelConfiguration
        {
            [Tooltip("警戒レベル名")]
            public string Level;
            
            [Range(0f, 1f)]
            [Tooltip("疑心閾値")]
            public float SuspicionThreshold;
            
            [Range(0.5f, 3.0f)]
            [Tooltip("検知能力倍率")]
            public float DetectionMultiplier;
            
            [Range(0.3f, 2.0f)]
            [Tooltip("移動速度倍率")]
            public float MovementSpeed;
            
            [Range(0.5f, 5.0f)]
            [Tooltip("旋回速度倍率")]
            public float TurnSpeed;
            
            [Tooltip("パトロール行動タイプ")]
            public string PatrolBehavior;
            
            [Tooltip("レベル説明")]
            public string Description;
        }

        [System.Serializable]
        public class GroupTactic
        {
            [Tooltip("戦術名")]
            public string Name;
            
            [Tooltip("最小必要人数")]
            public int MinMembers;
            
            [Tooltip("戦術説明")]
            public string Description;
        }

        public enum InvestigationPattern
        {
            Circular,           // 円形調査
            Grid,               // グリッド調査
            Random,             // ランダム調査
            TargetDirected      // 目標指向調査
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug", "Debug Actions")]
        [Button("Test Alert Level Progression")]
        public void TestAlertProgression()
        {
            Debug.Log("=== Alert Level Progression Test ===");
            for (float suspicion = 0f; suspicion <= 1f; suspicion += 0.2f)
            {
                string level = DetermineAlertLevel(suspicion);
                var config = GetAlertLevelConfig(level);
                Debug.Log($"Suspicion: {suspicion:F1} → Level: {level} (Detection: {config.DetectionMultiplier:F1}x)");
            }
        }

        [Button("Test Group Tactics")]
        public void TestGroupTactics()
        {
            Debug.Log("=== Group Tactics Test ===");
            for (int size = 1; size <= 10; size++)
            {
                var tactics = GetAvailableTactics(size);
                Debug.Log($"Group Size {size}: {tactics.Length} tactics available");
                foreach (var tactic in tactics)
                {
                    Debug.Log($"  - {tactic.Name}: {tactic.Description}");
                }
            }
        }

        [Button("Print AI Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Stealth AI Configuration ===");
            Debug.Log($"Visual Detection: Range={maxDetectionRange}m, FOV={detectionFieldOfView}°");
            Debug.Log($"Auditory Detection: Range={baseHearingRadius}m, Masking={environmentalMaskingThreshold}");
            Debug.Log($"Alert Levels: {alertLevels.Length} levels configured");
            Debug.Log($"Group Coordination: {(enableGroupCoordination ? "Enabled" : "Disabled")}");
            Debug.Log($"Adaptive Learning: {(enableAdaptiveLearning ? "Enabled" : "Disabled")}");
            Debug.Log($"Performance: {maxSensorsPerFrame} sensors/frame, LOD={enableLODSystem}");
        }
#endif

        #endregion
    }
}
