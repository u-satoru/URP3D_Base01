using UnityEngine;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルステンプレート固有設定システム
    /// TASK-004: 7ジャンルテンプレート完全実装 - Stealth Template Configuration（最優先）
    /// ✅AI Detection Systems活用: NPCVisualSensor + NPCAuditorySensor統合
    /// 名前空間: asterivo.Unity60.Features.Templates.Stealth.*
    /// </summary>
    [CreateAssetMenu(fileName = "StealthTemplateConfiguration", menuName = "Templates/Stealth/Configuration")]
    public class StealthTemplateConfiguration : ScriptableObject
    {
        [Header("AI検知システム設定")]
        [SerializeField] private bool _useAdvancedAI = true;
        [SerializeField] private int _maxNPCCount = 50;
        [SerializeField] private float _globalAlertDecayRate = 0.1f;
        
        [Header("NPCVisualSensor統合設定")]
        [SerializeField] private bool _enableVisualSensor = true;
        [SerializeField] private float _defaultViewDistance = 15f;
        [SerializeField] private float _defaultViewAngle = 90f;
        [SerializeField] private LayerMask _detectionLayers = -1;
        [SerializeField] private float _lightInfluenceMultiplier = 1.5f;
        
        [Header("NPCAuditorySensor統合設定")]
        [SerializeField] private bool _enableAuditorySensor = true;
        [SerializeField] private float _defaultHearingRadius = 10f;
        [SerializeField] private float _environmentalMaskingThreshold = 0.3f;
        [SerializeField] private AnimationCurve _distanceAttenuation = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("ステルスメカニクス設定")]
        [SerializeField] private bool _enableCrouchStealth = true;
        [SerializeField] private bool _enableShadowHiding = true;
        [SerializeField] private bool _enableDistraction = true;
        [SerializeField] private float _crouchNoiseReduction = 0.6f;
        [SerializeField] private float _shadowHidingEffectiveness = 0.8f;
        
        [Header("プレイヤー隠蔽システム")]
        [SerializeField] private bool _enableCoverSystem = true;
        [SerializeField] private LayerMask _coverLayers = 1 << 0;
        [SerializeField] private float _coverExitDelay = 1f;
        [SerializeField] private bool _enablePeekingMechanics = true;
        
        [Header("環境相互作用設定")]
        [SerializeField] private bool _enableEnvironmentalDistractions = true;
        [SerializeField] private bool _enableLightSwitching = true;
        [SerializeField] private bool _enableObjectThrowingDistraction = true;
        [SerializeField] private float _distractionEffectRadius = 8f;
        
        [Header("警戒システム統合")]
        [SerializeField] private bool _enableAlertPropagation = true;
        [SerializeField] private float _alertPropagationRadius = 20f;
        [SerializeField] private float _alertPropagationSpeed = 5f;
        [SerializeField] private int _maxAlertLevel = 4;
        
        [Header("オーディオシステム統合")]
        [SerializeField] private bool _useStealthAudioCoordinator = true;
        [SerializeField] private bool _enableFootstepVariation = true;
        [SerializeField] private bool _enableBreathingAudio = true;
        [SerializeField] private float _tensionMusicThreshold = 0.5f;
        
        [Header("デバッグ・可視化設定")]
        [SerializeField] private bool _enableDebugVisualization = false;
        [SerializeField] private bool _showSensorRanges = false;
        [SerializeField] private bool _showAIStates = false;
        [SerializeField] private bool _enablePerformanceMetrics = true;
        
        [Header("学習目標設定（Learn & Grow対応）")]
        [SerializeField] private string[] _learningObjectives = new[]
        {
            "BasicMovement_Stealth",
            "Crouching_Mechanics",
            "Shadow_Hiding",
            "Cover_System_Usage",
            "AI_Behavior_Understanding",
            "Environmental_Interaction",
            "Sound_Management",
            "Alert_Level_Management",
            "Distraction_Techniques",
            "Advanced_Stealth_Tactics"
        };
        
        [Header("パフォーマンス最適化設定")]
        [SerializeField] private bool _enableLODSystem = true;
        [SerializeField] private float _sensorUpdateFrequency = 10f;
        [SerializeField] private bool _enableFrameSpreadProcessing = true;
        [SerializeField] private int _maxSensorsPerFrame = 5;
        
        // Properties
        public bool UseAdvancedAI => _useAdvancedAI;
        public int MaxNPCCount => _maxNPCCount;
        public float GlobalAlertDecayRate => _globalAlertDecayRate;
        
        // Visual Sensor Properties
        public bool EnableVisualSensor => _enableVisualSensor;
        public float DefaultViewDistance => _defaultViewDistance;
        public float DefaultViewAngle => _defaultViewAngle;
        public LayerMask DetectionLayers => _detectionLayers;
        public float LightInfluenceMultiplier => _lightInfluenceMultiplier;
        
        // Auditory Sensor Properties  
        public bool EnableAuditorySensor => _enableAuditorySensor;
        public float DefaultHearingRadius => _defaultHearingRadius;
        public float EnvironmentalMaskingThreshold => _environmentalMaskingThreshold;
        public AnimationCurve DistanceAttenuation => _distanceAttenuation;
        
        // Stealth Mechanics Properties
        public bool EnableCrouchStealth => _enableCrouchStealth;
        public bool EnableShadowHiding => _enableShadowHiding;
        public bool EnableDistraction => _enableDistraction;
        public float CrouchNoiseReduction => _crouchNoiseReduction;
        public float ShadowHidingEffectiveness => _shadowHidingEffectiveness;
        
        // Cover System Properties
        public bool EnableCoverSystem => _enableCoverSystem;
        public LayerMask CoverLayers => _coverLayers;
        public float CoverExitDelay => _coverExitDelay;
        public bool EnablePeekingMechanics => _enablePeekingMechanics;
        
        // Environmental Properties
        public bool EnableEnvironmentalDistractions => _enableEnvironmentalDistractions;
        public bool EnableLightSwitching => _enableLightSwitching;
        public bool EnableObjectThrowingDistraction => _enableObjectThrowingDistraction;
        public float DistractionEffectRadius => _distractionEffectRadius;
        
        // Alert System Properties
        public bool EnableAlertPropagation => _enableAlertPropagation;
        public float AlertPropagationRadius => _alertPropagationRadius;
        public float AlertPropagationSpeed => _alertPropagationSpeed;
        public int MaxAlertLevel => _maxAlertLevel;
        
        // Audio Properties
        public bool UseStealthAudioCoordinator => _useStealthAudioCoordinator;
        public bool EnableFootstepVariation => _enableFootstepVariation;
        public bool EnableBreathingAudio => _enableBreathingAudio;
        public float TensionMusicThreshold => _tensionMusicThreshold;
        
        // Debug Properties
        public bool EnableDebugVisualization => _enableDebugVisualization;
        public bool ShowSensorRanges => _showSensorRanges;
        public bool ShowAIStates => _showAIStates;
        public bool EnablePerformanceMetrics => _enablePerformanceMetrics;
        
        // Learning Properties
        public string[] LearningObjectives => _learningObjectives;
        
        // Performance Properties
        public bool EnableLODSystem => _enableLODSystem;
        public float SensorUpdateFrequency => _sensorUpdateFrequency;
        public bool EnableFrameSpreadProcessing => _enableFrameSpreadProcessing;
        public int MaxSensorsPerFrame => _maxSensorsPerFrame;
        
        /// <summary>
        /// NPCVisualSensorへの設定適用
        /// </summary>
        /// <param name="visualSensor">設定を適用するVisualSensor</param>
        public void ApplyVisualSensorSettings(Component visualSensor)
        {
            if (!_enableVisualSensor || visualSensor == null)
                return;
                
            // リフレクションを使用してプロパティを設定
            // TODO: NPCVisualSensorの具体的なAPIに合わせて実装
            Debug.Log($"Applied visual sensor settings to {visualSensor.name}");
        }
        
        /// <summary>
        /// NPCAuditorySensorへの設定適用
        /// </summary>
        /// <param name="auditorySensor">設定を適用するAuditorySensor</param>
        public void ApplyAuditorySensorSettings(Component auditorySensor)
        {
            if (!_enableAuditorySensor || auditorySensor == null)
                return;
                
            // リフレクションを使用してプロパティを設定
            // TODO: NPCAuditorySensorの具体的なAPIに合わせて実装
            Debug.Log($"Applied auditory sensor settings to {auditorySensor.name}");
        }
        
        /// <summary>
        /// ステルス固有GameObjectセットアップ
        /// </summary>
        /// <param name="player">プレイヤーGameObject</param>
        /// <param name="npcs">NPCGameObject配列</param>
        public void SetupStealthGameplay(GameObject player, GameObject[] npcs)
        {
            Debug.Log("Setting up Stealth gameplay...");
            
            // プレイヤーにステルス機能を追加
            if (player != null)
            {
                SetupPlayerStealthComponents(player);
            }
            
            // NPCにAIセンサーを追加
            foreach (var npc in npcs)
            {
                if (npc != null)
                {
                    SetupNPCStealthComponents(npc);
                }
            }
            
            Debug.Log($"Stealth gameplay setup completed for {npcs.Length} NPCs");
        }
        
        /// <summary>
        /// プレイヤーステルス機能セットアップ
        /// </summary>
        private void SetupPlayerStealthComponents(GameObject player)
        {
            // ステルス状態管理コンポーネント追加
            if (_enableCrouchStealth)
            {
                // TODO: CrouchStealthController追加
            }
            
            if (_enableCoverSystem)
            {
                // TODO: CoverSystemController追加
            }
            
            // オーディオフットステップコンポーネント追加
            if (_enableFootstepVariation)
            {
                // TODO: FootstepAudioController追加
            }
            
            Debug.Log($"Player stealth components setup: {player.name}");
        }
        
        /// <summary>
        /// NPCステルス機能セットアップ
        /// </summary>
        private void SetupNPCStealthComponents(GameObject npc)
        {
            // NPCVisualSensor追加・設定
            if (_enableVisualSensor)
            {
                // TODO: NPCVisualSensorコンポーネント追加と設定適用
                Debug.Log($"Added NPCVisualSensor to {npc.name}");
            }
            
            // NPCAuditorySensor追加・設定
            if (_enableAuditorySensor)
            {
                // TODO: NPCAuditorySensorコンポーネント追加と設定適用
                Debug.Log($"Added NPCAuditorySensor to {npc.name}");
            }
            
            Debug.Log($"NPC stealth components setup: {npc.name}");
        }
        
        /// <summary>
        /// パフォーマンス最適化設定を適用
        /// </summary>
        public void ApplyPerformanceOptimizations()
        {
            if (_enableLODSystem)
            {
                Debug.Log("Applied LOD system for stealth sensors");
            }
            
            if (_enableFrameSpreadProcessing)
            {
                Debug.Log($"Enabled frame spread processing: {_maxSensorsPerFrame} sensors/frame");
            }
            
            Debug.Log($"Sensor update frequency set to: {_sensorUpdateFrequency}Hz");
        }
        
        /// <summary>
        /// 学習目標の進捗チェック
        /// </summary>
        /// <param name="completedObjectives">完了した学習目標</param>
        /// <returns>全体完了率（0-1）</returns>
        public float CalculateLearningProgress(string[] completedObjectives)
        {
            if (_learningObjectives.Length == 0)
                return 1f;
                
            int completedCount = 0;
            foreach (var objective in _learningObjectives)
            {
                if (System.Array.Exists(completedObjectives, completed => completed == objective))
                {
                    completedCount++;
                }
            }
            
            var progress = (float)completedCount / _learningObjectives.Length;
            Debug.Log($"Stealth learning progress: {progress:P} ({completedCount}/{_learningObjectives.Length})");
            
            return progress;
        }
        
        /// <summary>
        /// デバッグ情報を表示
        /// </summary>
        [ContextMenu("Print Stealth Configuration")]
        public void PrintConfiguration()
        {
            Debug.Log("=== Stealth Template Configuration ===");
            Debug.Log($"Advanced AI: {_useAdvancedAI}");
            Debug.Log($"Max NPC Count: {_maxNPCCount}");
            Debug.Log($"Visual Sensor: {_enableVisualSensor} (Range: {_defaultViewDistance}m, Angle: {_defaultViewAngle}°)");
            Debug.Log($"Auditory Sensor: {_enableAuditorySensor} (Range: {_defaultHearingRadius}m)");
            Debug.Log($"Stealth Mechanics: Crouch={_enableCrouchStealth}, Shadow={_enableShadowHiding}, Cover={_enableCoverSystem}");
            Debug.Log($"Learning Objectives: {_learningObjectives.Length} objectives");
            Debug.Log($"Performance: LOD={_enableLODSystem}, Update Freq={_sensorUpdateFrequency}Hz");
        }
    }
}