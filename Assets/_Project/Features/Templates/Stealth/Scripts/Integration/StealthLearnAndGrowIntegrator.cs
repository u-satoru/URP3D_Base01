using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using asterivo.Unity60.Features.Templates.Stealth.AI;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.UI;
using asterivo.Unity60.Features.Templates.Stealth.Data;

namespace asterivo.Unity60.Features.Templates.Stealth.Integration
{
    /// <summary>
    /// Learn & Grow価値実現システム統合
    /// 70%学習コスト削減と15分ゲームプレイ習得を実現する統合制御システム
    /// </summary>
    public class StealthLearnAndGrowIntegrator : MonoBehaviour
    {
        [Header("Learn & Grow Configuration")]
        [SerializeField] private bool _enableLearnAndGrowSystem = true;
        [SerializeField] private float _masteryTargetTime = 900f; // 15分 = 900秒
        [SerializeField] private int _requiredSuccessfulActions = 10; // 習得に必要な成功アクション数
        
        [Header("Learning Analytics")]
        [SerializeField] private float _learningProgress; // 0.0 - 1.0
        [SerializeField] private int _successfulStealthActions;
        [SerializeField] private int _detectionCount;
        [SerializeField] private float _sessionStartTime;
        [SerializeField] private bool _masteryAchieved;
        
        [Header("System References")]
        [SerializeField] private StealthMechanicsController _mechanicsController;
        [SerializeField] private StealthAICoordinator _aiCoordinator;
        [SerializeField] private StealthEnvironmentManager _environmentManager;
        [SerializeField] private StealthUIManager _uiManager;
        
        // Learn & Grow Events
        public event System.Action<float> OnLearningProgressChanged;
        public event System.Action OnMasteryAchieved;
        public event System.Action<string> OnLearningTipTriggered;
        
        // Learning State
        private StealthLearningPhase _currentPhase = StealthLearningPhase.Introduction;
        private Dictionary<StealthSkill, float> _skillProgress = new();
        private List<string> _availableTips = new();
        private bool _isTrackingSession;
        
        // Performance Analytics for 70% Cost Reduction
        private struct LearningMetrics
        {
            public float TimeToFirstSuccess;
            public float AverageDetectionTime;
            public int HintsUsed;
            public float EfficiencyScore;
            public Dictionary<StealthSkill, float> SkillMasteryTimes;
        }
        
        private LearningMetrics _currentMetrics;
        
        private enum StealthLearningPhase
        {
            Introduction,    // 基本概念学習
            Practice,        // 実践練習
            Mastery,         // 習得確認
            Advanced,        // 応用テクニック
            Complete         // 完全習得
        }
        
        private enum StealthSkill
        {
            BasicMovement,      // 基本移動
            NoiseControl,       // 音響制御
            CoverUtilization,   // 遮蔽物利用
            AIBehaviorReading,  // AI行動読み
            EnvironmentUsage,   // 環境活用
            TimingMastery       // タイミング習得
        }
        
        private void Awake()
        {
            InitializeLearnAndGrowSystem();
            InitializeLearningTips();
        }
        
        private void Start()
        {
            StartLearningSession();
            RegisterSystemEventListeners();
        }
        
        private void OnDestroy()
        {
            UnregisterSystemEventListeners();
        }
        
        #region Initialization
        
        private void InitializeLearnAndGrowSystem()
        {
            // システム参照の自動取得
            if (_mechanicsController == null)
                _mechanicsController = FindObjectOfType<StealthMechanicsController>();
            if (_aiCoordinator == null)
                _aiCoordinator = FindObjectOfType<StealthAICoordinator>();
            if (_environmentManager == null)
                _environmentManager = FindObjectOfType<StealthEnvironmentManager>();
            if (_uiManager == null)
                _uiManager = FindObjectOfType<StealthUIManager>();
                
            // スキル進捗初期化
            foreach (StealthSkill skill in System.Enum.GetValues(typeof(StealthSkill)))
            {
                _skillProgress[skill] = 0f;
            }
            
            // メトリクス初期化
            _currentMetrics = new LearningMetrics
            {
                SkillMasteryTimes = new Dictionary<StealthSkill, float>()
            };
            
            foreach (StealthSkill skill in System.Enum.GetValues(typeof(StealthSkill)))
            {
                _currentMetrics.SkillMasteryTimes[skill] = 0f;
            }
            
            Debug.Log($"<color=cyan>[StealthLearnAndGrow]</color> Learn & Grow System Initialized. Target: {_masteryTargetTime}s mastery");
        }
        
        private void InitializeLearningTips()
        {
            _availableTips = new List<string>
            {
                "しゃがみ移動で足音を軽減できます",
                "暗い場所では発見されにくくなります",
                "NPCの視界コーンを避けて移動しましょう",
                "環境音を利用して足音をマスクできます",
                "NPCの背後は最も安全な移動ルートです",
                "遮蔽物の陰で待機時間を有効活用しましょう",
                "NPCの行動パターンを観察して最適なタイミングを見つけましょう",
                "複数のNPCがいる場合は協調検出に注意してください"
            };
        }
        
        #endregion
        
        #region Learning Session Management
        
        private void StartLearningSession()
        {
            if (!_enableLearnAndGrowSystem) return;
            
            _sessionStartTime = Time.time;
            _learningProgress = 0f;
            _successfulStealthActions = 0;
            _detectionCount = 0;
            _masteryAchieved = false;
            _isTrackingSession = true;
            _currentPhase = StealthLearningPhase.Introduction;
            
            // 初回学習ヒント表示
            StartCoroutine(ShowIntroductionTips());
            
            Debug.Log("<color=cyan>[StealthLearnAndGrow]</color> Learning session started. Target mastery time: 15 minutes");
        }
        
        private IEnumerator ShowIntroductionTips()
        {
            yield return new WaitForSeconds(2f);
            TriggerLearningTip("ステルスゲームへようこそ！隠密行動の基本を学びましょう");
            
            yield return new WaitForSeconds(5f);
            TriggerLearningTip("WASDで移動、Shiftでしゃがみ移動ができます");
            
            yield return new WaitForSeconds(3f);
            TriggerLearningTip("NPCの視界（赤いコーン）を避けて移動してみましょう");
        }
        
        #endregion
        
        #region Event Registration
        
        private void RegisterSystemEventListeners()
        {
            // StealthMechanicsController Events
            if (_mechanicsController != null)
            {
                _mechanicsController.OnStealthStateChanged += HandleStealthStateChanged;
                _mechanicsController.OnSuccessfulStealth += HandleSuccessfulStealthAction;
            }
            
            // StealthAICoordinator Events
            if (_aiCoordinator != null)
            {
                _aiCoordinator.OnPlayerDetected += HandlePlayerDetected;
                _aiCoordinator.OnPlayerLost += HandlePlayerLost;
                _aiCoordinator.OnCooperativeDetection += HandleCooperativeDetection;
            }
            
            // StealthEnvironmentManager Events
            if (_environmentManager != null)
            {
                _environmentManager.OnHidingSpotEntered += HandleHidingSpotUsage;
                _environmentManager.OnEnvironmentalInteraction += HandleEnvironmentalInteraction;
            }
        }
        
        private void UnregisterSystemEventListeners()
        {
            if (_mechanicsController != null)
            {
                _mechanicsController.OnStealthStateChanged -= HandleStealthStateChanged;
                _mechanicsController.OnSuccessfulStealth -= HandleSuccessfulStealthAction;
            }
            
            if (_aiCoordinator != null)
            {
                _aiCoordinator.OnPlayerDetected -= HandlePlayerDetected;
                _aiCoordinator.OnPlayerLost -= HandlePlayerLost;
                _aiCoordinator.OnCooperativeDetection -= HandleCooperativeDetection;
            }
            
            if (_environmentManager != null)
            {
                _environmentManager.OnHidingSpotEntered -= HandleHidingSpotUsage;
                _environmentManager.OnEnvironmentalInteraction -= HandleEnvironmentalInteraction;
            }
        }
        
        #endregion
        
        #region Learning Event Handlers
        
        private void HandleStealthStateChanged(StealthState newState, StealthState previousState)
        {
            UpdateSkillProgress(StealthSkill.BasicMovement, 0.1f);
            
            switch (newState)
            {
                case StealthState.Hidden:
                    UpdateSkillProgress(StealthSkill.CoverUtilization, 0.2f);
                    if (_currentPhase == StealthLearningPhase.Introduction)
                    {
                        TriggerLearningTip("素晴らしい！隠れることができました");
                        AdvancePhaseIfReady();
                    }
                    break;
                    
                case StealthState.Detected:
                    _detectionCount++;
                    ProvideFeedbackForDetection();
                    break;
                    
                case StealthState.Concealed:
                    UpdateSkillProgress(StealthSkill.EnvironmentUsage, 0.15f);
                    break;
            }
            
            UpdateLearningProgress();
        }
        
        private void HandleSuccessfulStealthAction(Vector3 position)
        {
            _successfulStealthActions++;
            UpdateSkillProgress(StealthSkill.TimingMastery, 0.3f);
            
            // 成功アクションに基づく動的ヒント
            if (_successfulStealthActions == 3)
            {
                TriggerLearningTip("連続成功！ステルスの基本をつかんできましたね");
            }
            else if (_successfulStealthActions == 7)
            {
                TriggerLearningTip("上達しています！より複雑な環境にチャレンジしてみましょう");
            }
            
            UpdateLearningProgress();
            CheckMasteryAchievement();
        }
        
        private void HandlePlayerDetected(MonoBehaviour detector, DetectionType detectionType)
        {
            // 検出タイプ別のフィードバック
            string tip = detectionType switch
            {
                DetectionType.Visual => "視覚検出されました。NPCの視界を避けましょう",
                DetectionType.Auditory => "音で発見されました。しゃがみ移動を試してみましょう",
                DetectionType.Environmental => "環境による検出です。周囲の状況に注意しましょう",
                DetectionType.Cooperative => "複数のNPCに発見されました。1体ずつ対処しましょう",
                _ => "発見されました。落ち着いて隠れ場所を探しましょう"
            };
            
            TriggerLearningTip(tip);
            
            // 検出タイプ別スキル更新
            switch (detectionType)
            {
                case DetectionType.Visual:
                    UpdateSkillProgress(StealthSkill.AIBehaviorReading, 0.1f);
                    break;
                case DetectionType.Auditory:
                    UpdateSkillProgress(StealthSkill.NoiseControl, 0.1f);
                    break;
            }
        }
        
        private void HandlePlayerLost(MonoBehaviour detector)
        {
            UpdateSkillProgress(StealthSkill.AIBehaviorReading, 0.2f);
            TriggerLearningTip("うまく逃げました！この調子で進みましょう");
        }
        
        private void HandleCooperativeDetection(List<MonoBehaviour> detectors)
        {
            TriggerLearningTip($"{detectors.Count}体のNPCが連携しています。より慎重に進みましょう");
            UpdateSkillProgress(StealthSkill.AIBehaviorReading, 0.25f);
        }
        
        private void HandleHidingSpotUsage(HidingSpot hidingSpot)
        {
            UpdateSkillProgress(StealthSkill.CoverUtilization, 0.3f);
            
            if (_currentPhase == StealthLearningPhase.Introduction)
            {
                TriggerLearningTip("隠れ場所を上手に利用できました！");
            }
        }
        
        private void HandleEnvironmentalInteraction(EnvironmentalElement element)
        {
            UpdateSkillProgress(StealthSkill.EnvironmentUsage, 0.2f);
            TriggerLearningTip("環境を活用した素晴らしい判断です！");
        }
        
        #endregion
        
        #region Learning Progress Management
        
        private void UpdateSkillProgress(StealthSkill skill, float increment)
        {
            float previousProgress = _skillProgress[skill];
            _skillProgress[skill] = Mathf.Clamp01(_skillProgress[skill] + increment);
            
            // スキル習得時のフィードバック
            if (previousProgress < 1f && _skillProgress[skill] >= 1f)
            {
                string skillName = GetSkillDisplayName(skill);
                TriggerLearningTip($"{skillName}をマスターしました！");
                _currentMetrics.SkillMasteryTimes[skill] = Time.time - _sessionStartTime;
            }
        }
        
        private void UpdateLearningProgress()
        {
            // 全体的な学習進捗の計算
            float totalProgress = _skillProgress.Values.Average();
            float actionProgress = Mathf.Clamp01((float)_successfulStealthActions / _requiredSuccessfulActions);
            float timeProgress = Mathf.Clamp01((Time.time - _sessionStartTime) / _masteryTargetTime);
            
            // 加重平均で進捗計算（スキル50%、アクション30%、時間20%）
            _learningProgress = (totalProgress * 0.5f) + (actionProgress * 0.3f) + (timeProgress * 0.2f);
            
            OnLearningProgressChanged?.Invoke(_learningProgress);
            
            // UIに進捗を通知
            if (_uiManager != null)
            {
                _uiManager.UpdateLearningProgress(_learningProgress);
            }
        }
        
        private void CheckMasteryAchievement()
        {
            if (_masteryAchieved) return;
            
            bool skillsMastered = _skillProgress.Values.All(progress => progress >= 0.8f);
            bool actionsCompleted = _successfulStealthActions >= _requiredSuccessfulActions;
            bool timeEfficient = (Time.time - _sessionStartTime) <= _masteryTargetTime;
            
            if (skillsMastered && actionsCompleted)
            {
                _masteryAchieved = true;
                float masteryTime = Time.time - _sessionStartTime;
                
                OnMasteryAchieved?.Invoke();
                TriggerLearningTip($"おめでとうございます！{masteryTime:F1}秒でステルス技術を習得しました！");
                
                // 70%学習コスト削減の実現確認
                if (timeEfficient)
                {
                    Debug.Log($"<color=green>[StealthLearnAndGrow]</color> Learn & Grow SUCCESS: Mastery achieved in {masteryTime:F1}s (Target: {_masteryTargetTime}s)");
                    TriggerLearningTip("目標時間内での習得達成！Learn & Grow システムが効果を発揮しました");
                }
                
                // 最終フェーズに移行
                _currentPhase = StealthLearningPhase.Complete;
            }
        }
        
        private void AdvancePhaseIfReady()
        {
            switch (_currentPhase)
            {
                case StealthLearningPhase.Introduction when _successfulStealthActions >= 2:
                    _currentPhase = StealthLearningPhase.Practice;
                    TriggerLearningTip("基本をマスターしました！より複雑な状況に挑戦しましょう");
                    break;
                    
                case StealthLearningPhase.Practice when _successfulStealthActions >= 5:
                    _currentPhase = StealthLearningPhase.Mastery;
                    TriggerLearningTip("実践力が身についてきました！習得確認に進みましょう");
                    break;
                    
                case StealthLearningPhase.Mastery when _successfulStealthActions >= 8:
                    _currentPhase = StealthLearningPhase.Advanced;
                    TriggerLearningTip("高度なテクニックを学習しましょう");
                    break;
            }
        }
        
        #endregion
        
        #region Feedback System
        
        private void ProvideFeedbackForDetection()
        {
            // 検出回数に基づく適応的フィードバック
            if (_detectionCount <= 2)
            {
                TriggerLearningTip("大丈夫です！失敗から学んで次に活かしましょう");
            }
            else if (_detectionCount <= 5)
            {
                TriggerLearningTip("NPCの動きをよく観察してタイミングを計りましょう");
            }
            else
            {
                TriggerLearningTip("別のルートを試してみることをお勧めします");
                // より具体的なヒントを提供
                ProvideContextualHint();
            }
        }
        
        private void ProvideContextualHint()
        {
            if (_availableTips.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _availableTips.Count);
                string tip = _availableTips[randomIndex];
                TriggerLearningTip($"ヒント: {tip}");
                _currentMetrics.HintsUsed++;
            }
        }
        
        private void TriggerLearningTip(string tip)
        {
            OnLearningTipTriggered?.Invoke(tip);
            
            // UIマネージャーに通知
            if (_uiManager != null)
            {
                _uiManager.ShowLearningTip(tip);
            }
            
            Debug.Log($"<color=yellow>[StealthLearnAndGrow]</color> Learning Tip: {tip}");
        }
        
        #endregion
        
        #region Analytics & Reporting
        
        public void GenerateLearningReport()
        {
            if (!_isTrackingSession) return;
            
            float sessionTime = Time.time - _sessionStartTime;
            _currentMetrics.EfficiencyScore = CalculateEfficiencyScore();
            
            Debug.Log($"=== Learn & Grow Learning Report ===");
            Debug.Log($"Session Time: {sessionTime:F1}s / Target: {_masteryTargetTime}s");
            Debug.Log($"Success Rate: {_successfulStealthActions}/{(_successfulStealthActions + _detectionCount)}");
            Debug.Log($"Learning Progress: {_learningProgress:P}");
            Debug.Log($"Mastery Achieved: {_masteryAchieved}");
            Debug.Log($"Hints Used: {_currentMetrics.HintsUsed}");
            Debug.Log($"Efficiency Score: {_currentMetrics.EfficiencyScore:F2}");
            
            foreach (var skill in _skillProgress)
            {
                Debug.Log($"{GetSkillDisplayName(skill.Key)}: {skill.Value:P}");
            }
        }
        
        private float CalculateEfficiencyScore()
        {
            float sessionTime = Time.time - _sessionStartTime;
            float timeEfficiency = Mathf.Clamp01(_masteryTargetTime / sessionTime);
            float skillEfficiency = _skillProgress.Values.Average();
            float successRate = (float)_successfulStealthActions / (_successfulStealthActions + _detectionCount + 1);
            
            return (timeEfficiency * 0.4f) + (skillEfficiency * 0.4f) + (successRate * 0.2f);
        }
        
        #endregion
        
        #region Utility Methods
        
        private string GetSkillDisplayName(StealthSkill skill)
        {
            return skill switch
            {
                StealthSkill.BasicMovement => "基本移動",
                StealthSkill.NoiseControl => "音響制御",
                StealthSkill.CoverUtilization => "遮蔽物利用",
                StealthSkill.AIBehaviorReading => "AI行動読み",
                StealthSkill.EnvironmentUsage => "環境活用",
                StealthSkill.TimingMastery => "タイミング習得",
                _ => skill.ToString()
            };
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 現在の学習進捗を取得
        /// </summary>
        public float GetLearningProgress() => _learningProgress;
        
        /// <summary>
        /// 特定スキルの進捗を取得
        /// </summary>
        public float GetSkillProgress(StealthSkill skill) => _skillProgress.GetValueOrDefault(skill, 0f);
        
        /// <summary>
        /// 習得状況を取得
        /// </summary>
        public bool IsMasteryAchieved() => _masteryAchieved;
        
        /// <summary>
        /// 学習セッションの手動リセット
        /// </summary>
        public void ResetLearningSession()
        {
            StartLearningSession();
        }
        
        /// <summary>
        /// Learn & Grow システムの有効/無効切り替え
        /// </summary>
        public void SetLearnAndGrowEnabled(bool enabled)
        {
            _enableLearnAndGrowSystem = enabled;
            if (!enabled && _isTrackingSession)
            {
                _isTrackingSession = false;
                TriggerLearningTip("Learn & Grow システムが無効化されました");
            }
        }
        
        #endregion
        
        #if UNITY_EDITOR
        [Header("Debug Information")]
        [SerializeField] private StealthLearningPhase _debugCurrentPhase;
        [SerializeField] private Dictionary<StealthSkill, float> _debugSkillProgress = new();
        
        private void OnValidate()
        {
            _debugCurrentPhase = _currentPhase;
            _debugSkillProgress = new Dictionary<StealthSkill, float>(_skillProgress);
        }
        #endif
    }
}