using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using asterivo.Unity60.Features.Templates.Stealth.AI;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.UI;
using asterivo.Unity60.Features.Templates.Stealth.Data;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;

namespace asterivo.Unity60.Features.Templates.Stealth.Integration
{
    /// <summary>
    /// Learn & Grow萓｡蛟､螳溽樟繧ｷ繧ｹ繝・Β邨ｱ蜷・
    /// 70%蟄ｦ鄙偵さ繧ｹ繝亥炎貂帙→15蛻・ご繝ｼ繝繝励Ξ繧､鄙貞ｾ励ｒ螳溽樟縺吶ｋ邨ｱ蜷亥宛蠕｡繧ｷ繧ｹ繝・Β
    /// </summary>
    public class StealthLearnAndGrowIntegrator : MonoBehaviour
    {
        [Header("Learn & Grow Configuration")]
        [SerializeField] private bool _enableLearnAndGrowSystem = true;
        [SerializeField] private float _masteryTargetTime = 900f; // 15蛻・= 900遘・
        [SerializeField] private int _requiredSuccessfulActions = 10; // 鄙貞ｾ励↓蠢・ｦ√↑謌仙粥繧｢繧ｯ繧ｷ繝ｧ繝ｳ謨ｰ
        
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
            Introduction,    // 蝓ｺ譛ｬ讎ょｿｵ蟄ｦ鄙・
            Practice,        // 螳溯ｷｵ邱ｴ鄙・
            Mastery,         // 鄙貞ｾ礼｢ｺ隱・
            Advanced,        // 蠢懃畑繝・け繝九ャ繧ｯ
            Complete         // 螳悟・鄙貞ｾ・
        }
        
        public enum StealthSkill
        {
            BasicMovement,      // 蝓ｺ譛ｬ遘ｻ蜍・
            NoiseControl,       // 髻ｳ髻ｿ蛻ｶ蠕｡
            CoverUtilization,   // 驕ｮ阡ｽ迚ｩ蛻ｩ逕ｨ
            AIBehaviorReading,  // AI陦悟虚隱ｭ縺ｿ
            EnvironmentUsage,   // 迺ｰ蠅・ｴｻ逕ｨ
            TimingMastery       // 繧ｿ繧､繝溘Φ繧ｰ鄙貞ｾ・
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
            // 繧ｷ繧ｹ繝・Β蜿ら・縺ｮ閾ｪ蜍募叙蠕・
            if (_mechanicsController == null)
                _mechanicsController = FindObjectOfType<StealthMechanicsController>();
            if (_aiCoordinator == null)
                _aiCoordinator = FindObjectOfType<StealthAICoordinator>();
            if (_environmentManager == null)
                _environmentManager = FindObjectOfType<StealthEnvironmentManager>();
            if (_uiManager == null)
                _uiManager = FindObjectOfType<StealthUIManager>();
                
            // 繧ｹ繧ｭ繝ｫ騾ｲ謐怜・譛溷喧
            foreach (StealthSkill skill in System.Enum.GetValues(typeof(StealthSkill)))
            {
                _skillProgress[skill] = 0f;
            }
            
            // 繝｡繝医Μ繧ｯ繧ｹ蛻晄悄蛹・
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
                "縺励ｃ縺後∩遘ｻ蜍輔〒雜ｳ髻ｳ繧定ｻｽ貂帙〒縺阪∪縺・,
                "證励＞蝣ｴ謇縺ｧ縺ｯ逋ｺ隕九＆繧後↓縺上￥縺ｪ繧翫∪縺・,
                "NPC縺ｮ隕也阜繧ｳ繝ｼ繝ｳ繧帝∩縺代※遘ｻ蜍輔＠縺ｾ縺励ｇ縺・,
                "迺ｰ蠅・浹繧貞茜逕ｨ縺励※雜ｳ髻ｳ繧偵・繧ｹ繧ｯ縺ｧ縺阪∪縺・,
                "NPC縺ｮ閭悟ｾ後・譛繧ょｮ牙・縺ｪ遘ｻ蜍輔Ν繝ｼ繝医〒縺・,
                "驕ｮ阡ｽ迚ｩ縺ｮ髯ｰ縺ｧ蠕・ｩ滓凾髢薙ｒ譛牙柑豢ｻ逕ｨ縺励∪縺励ｇ縺・,
                "NPC縺ｮ陦悟虚繝代ち繝ｼ繝ｳ繧定ｦｳ蟇溘＠縺ｦ譛驕ｩ縺ｪ繧ｿ繧､繝溘Φ繧ｰ繧定ｦ九▽縺代∪縺励ｇ縺・,
                "隍・焚縺ｮNPC縺後＞繧句ｴ蜷医・蜊碑ｪｿ讀懷・縺ｫ豕ｨ諢上＠縺ｦ縺上□縺輔＞"
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
            
            // 蛻晏屓蟄ｦ鄙偵ヲ繝ｳ繝郁｡ｨ遉ｺ
            StartCoroutine(ShowIntroductionTips());
            
            Debug.Log("<color=cyan>[StealthLearnAndGrow]</color> Learning session started. Target mastery time: 15 minutes");
        }
        
        private IEnumerator ShowIntroductionTips()
        {
            yield return new WaitForSeconds(2f);
            TriggerLearningTip("繧ｹ繝・Ν繧ｹ繧ｲ繝ｼ繝縺ｸ繧医≧縺薙◎・・國蟇・｡悟虚縺ｮ蝓ｺ譛ｬ繧貞ｭｦ縺ｳ縺ｾ縺励ｇ縺・);
            
            yield return new WaitForSeconds(5f);
            TriggerLearningTip("WASD縺ｧ遘ｻ蜍輔ヾhift縺ｧ縺励ｃ縺後∩遘ｻ蜍輔′縺ｧ縺阪∪縺・);
            
            yield return new WaitForSeconds(3f);
            TriggerLearningTip("NPC縺ｮ隕也阜・郁ｵ､縺・さ繝ｼ繝ｳ・峨ｒ驕ｿ縺代※遘ｻ蜍輔＠縺ｦ縺ｿ縺ｾ縺励ｇ縺・);
        }
        
        #endregion
        
        #region Event Registration
        
        private void RegisterSystemEventListeners()
        {
            // StealthMechanicsController Events
            if (_mechanicsController != null)
            {
                // TODO: Implement proper event system integration once events are implemented
                // _mechanicsController.OnStealthStateChanged += HandleStealthStateChanged;
                // _mechanicsController.OnSuccessfulStealth += HandleSuccessfulStealthAction;
            }
            
            // StealthAICoordinator Events
            if (_aiCoordinator != null)
            {
                // TODO: Implement proper AI coordinator event integration
                // _aiCoordinator.OnPlayerDetected += HandlePlayerDetected;
                // _aiCoordinator.OnPlayerLost += HandlePlayerLost;
                // _aiCoordinator.OnCooperativeDetection += HandleCooperativeDetection;
            }
            
            // StealthEnvironmentManager Events
            if (_environmentManager != null)
            {
                // TODO: Implement proper environment manager event integration
                // _environmentManager.OnHidingSpotEntered += HandleHidingSpotUsage;
                // _environmentManager.OnEnvironmentalInteraction += HandleEnvironmentalInteraction;
            }
        }
        
        private void UnregisterSystemEventListeners()
        {
            if (_mechanicsController != null)
            {
                // TODO: Implement proper event system unregistration once events are implemented
                // _mechanicsController.OnStealthStateChanged -= HandleStealthStateChanged;
                // _mechanicsController.OnSuccessfulStealth -= HandleSuccessfulStealthAction;
            }
            
            if (_aiCoordinator != null)
            {
                // TODO: Implement proper AI coordinator event unregistration
                // _aiCoordinator.OnPlayerDetected -= HandlePlayerDetected;
                // _aiCoordinator.OnPlayerLost -= HandlePlayerLost;
                // _aiCoordinator.OnCooperativeDetection -= HandleCooperativeDetection;
            }
            
            if (_environmentManager != null)
            {
                // TODO: Implement proper environment manager event unregistration
                // _environmentManager.OnHidingSpotEntered -= HandleHidingSpotUsage;
                // _environmentManager.OnEnvironmentalInteraction -= HandleEnvironmentalInteraction;
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
                        TriggerLearningTip("邏譎ｴ繧峨＠縺・ｼ・國繧後ｋ縺薙→縺後〒縺阪∪縺励◆");
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
            
            // 謌仙粥繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｫ蝓ｺ縺･縺丞虚逧・ヲ繝ｳ繝・
            if (_successfulStealthActions == 3)
            {
                TriggerLearningTip("騾｣邯壽・蜉滂ｼ√せ繝・Ν繧ｹ縺ｮ蝓ｺ譛ｬ繧偵▽縺九ｓ縺ｧ縺阪∪縺励◆縺ｭ");
            }
            else if (_successfulStealthActions == 7)
            {
                TriggerLearningTip("荳企＃縺励※縺・∪縺呻ｼ√ｈ繧願､・尅縺ｪ迺ｰ蠅・↓繝√Ε繝ｬ繝ｳ繧ｸ縺励※縺ｿ縺ｾ縺励ｇ縺・);
            }
            
            UpdateLearningProgress();
            CheckMasteryAchievement();
        }
        
        private void HandlePlayerDetected(MonoBehaviour detector, DetectionType detectionType)
        {
            // 讀懷・繧ｿ繧､繝怜挨縺ｮ繝輔ぅ繝ｼ繝峨ヰ繝・け
            string tip = detectionType switch
            {
                DetectionType.Visual => "隕冶ｦ壽､懷・縺輔ｌ縺ｾ縺励◆縲・PC縺ｮ隕也阜繧帝∩縺代∪縺励ｇ縺・,
                DetectionType.Auditory => "髻ｳ縺ｧ逋ｺ隕九＆繧後∪縺励◆縲ゅ＠繧・′縺ｿ遘ｻ蜍輔ｒ隧ｦ縺励※縺ｿ縺ｾ縺励ｇ縺・,
                DetectionType.Environmental => "迺ｰ蠅・↓繧医ｋ讀懷・縺ｧ縺吶ょ捉蝗ｲ縺ｮ迥ｶ豕√↓豕ｨ諢上＠縺ｾ縺励ｇ縺・,
                DetectionType.Cooperative => "隍・焚縺ｮNPC縺ｫ逋ｺ隕九＆繧後∪縺励◆縲・菴薙★縺､蟇ｾ蜃ｦ縺励∪縺励ｇ縺・,
                _ => "逋ｺ隕九＆繧後∪縺励◆縲り誠縺｡逹縺・※髫繧悟ｴ謇繧呈爾縺励∪縺励ｇ縺・
            };
            
            TriggerLearningTip(tip);
            
            // 讀懷・繧ｿ繧､繝怜挨繧ｹ繧ｭ繝ｫ譖ｴ譁ｰ
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
            TriggerLearningTip("縺・∪縺城・￡縺ｾ縺励◆・√％縺ｮ隱ｿ蟄舌〒騾ｲ縺ｿ縺ｾ縺励ｇ縺・);
        }
        
        private void HandleCooperativeDetection(List<MonoBehaviour> detectors)
        {
            TriggerLearningTip($"{detectors.Count}菴薙・NPC縺碁｣謳ｺ縺励※縺・∪縺吶ゅｈ繧頑・驥阪↓騾ｲ縺ｿ縺ｾ縺励ｇ縺・);
            UpdateSkillProgress(StealthSkill.AIBehaviorReading, 0.25f);
        }
        
        private void HandleHidingSpotUsage(HidingSpot hidingSpot)
        {
            UpdateSkillProgress(StealthSkill.CoverUtilization, 0.3f);
            
            if (_currentPhase == StealthLearningPhase.Introduction)
            {
                TriggerLearningTip("髫繧悟ｴ謇繧剃ｸ頑焔縺ｫ蛻ｩ逕ｨ縺ｧ縺阪∪縺励◆・・);
            }
        }
        
        private void HandleEnvironmentalInteraction(EnvironmentalElement element)
        {
            UpdateSkillProgress(StealthSkill.EnvironmentUsage, 0.2f);
            TriggerLearningTip("迺ｰ蠅・ｒ豢ｻ逕ｨ縺励◆邏譎ｴ繧峨＠縺・愛譁ｭ縺ｧ縺呻ｼ・);
        }
        
        #endregion
        
        #region Learning Progress Management
        
        private void UpdateSkillProgress(StealthSkill skill, float increment)
        {
            float previousProgress = _skillProgress[skill];
            _skillProgress[skill] = Mathf.Clamp01(_skillProgress[skill] + increment);
            
            // 繧ｹ繧ｭ繝ｫ鄙貞ｾ玲凾縺ｮ繝輔ぅ繝ｼ繝峨ヰ繝・け
            if (previousProgress < 1f && _skillProgress[skill] >= 1f)
            {
                string skillName = GetSkillDisplayName(skill);
                TriggerLearningTip($"{skillName}繧偵・繧ｹ繧ｿ繝ｼ縺励∪縺励◆・・);
                _currentMetrics.SkillMasteryTimes[skill] = Time.time - _sessionStartTime;
            }
        }
        
        private void UpdateLearningProgress()
        {
            // 蜈ｨ菴鍋噪縺ｪ蟄ｦ鄙帝ｲ謐励・險育ｮ・
            float totalProgress = _skillProgress.Values.Average();
            float actionProgress = Mathf.Clamp01((float)_successfulStealthActions / _requiredSuccessfulActions);
            float timeProgress = Mathf.Clamp01((Time.time - _sessionStartTime) / _masteryTargetTime);
            
            // 蜉驥榊ｹｳ蝮・〒騾ｲ謐苓ｨ育ｮ暦ｼ医せ繧ｭ繝ｫ50%縲√い繧ｯ繧ｷ繝ｧ繝ｳ30%縲∵凾髢・0%・・
            _learningProgress = (totalProgress * 0.5f) + (actionProgress * 0.3f) + (timeProgress * 0.2f);
            
            OnLearningProgressChanged?.Invoke(_learningProgress);
            
            // UI縺ｫ騾ｲ謐励ｒ騾夂衍
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
                TriggerLearningTip($"縺翫ａ縺ｧ縺ｨ縺・＃縺悶＞縺ｾ縺呻ｼ＋masteryTime:F1}遘偵〒繧ｹ繝・Ν繧ｹ謚陦薙ｒ鄙貞ｾ励＠縺ｾ縺励◆・・);
                
                // 70%蟄ｦ鄙偵さ繧ｹ繝亥炎貂帙・螳溽樟遒ｺ隱・
                if (timeEfficient)
                {
                    Debug.Log($"<color=green>[StealthLearnAndGrow]</color> Learn & Grow SUCCESS: Mastery achieved in {masteryTime:F1}s (Target: {_masteryTargetTime}s)");
                    TriggerLearningTip("逶ｮ讓呎凾髢灘・縺ｧ縺ｮ鄙貞ｾ鈴＃謌撰ｼ´earn & Grow 繧ｷ繧ｹ繝・Β縺悟柑譫懊ｒ逋ｺ謠ｮ縺励∪縺励◆");
                }
                
                // 譛邨ゅヵ繧ｧ繝ｼ繧ｺ縺ｫ遘ｻ陦・
                _currentPhase = StealthLearningPhase.Complete;
            }
        }
        
        private void AdvancePhaseIfReady()
        {
            switch (_currentPhase)
            {
                case StealthLearningPhase.Introduction when _successfulStealthActions >= 2:
                    _currentPhase = StealthLearningPhase.Practice;
                    TriggerLearningTip("蝓ｺ譛ｬ繧偵・繧ｹ繧ｿ繝ｼ縺励∪縺励◆・√ｈ繧願､・尅縺ｪ迥ｶ豕√↓謖第姶縺励∪縺励ｇ縺・);
                    break;
                    
                case StealthLearningPhase.Practice when _successfulStealthActions >= 5:
                    _currentPhase = StealthLearningPhase.Mastery;
                    TriggerLearningTip("螳溯ｷｵ蜉帙′霄ｫ縺ｫ縺､縺・※縺阪∪縺励◆・∫ｿ貞ｾ礼｢ｺ隱阪↓騾ｲ縺ｿ縺ｾ縺励ｇ縺・);
                    break;
                    
                case StealthLearningPhase.Mastery when _successfulStealthActions >= 8:
                    _currentPhase = StealthLearningPhase.Advanced;
                    TriggerLearningTip("鬮伜ｺｦ縺ｪ繝・け繝九ャ繧ｯ繧貞ｭｦ鄙偵＠縺ｾ縺励ｇ縺・);
                    break;
            }
        }
        
        #endregion
        
        #region Feedback System
        
        private void ProvideFeedbackForDetection()
        {
            // 讀懷・蝗樊焚縺ｫ蝓ｺ縺･縺城←蠢懃噪繝輔ぅ繝ｼ繝峨ヰ繝・け
            if (_detectionCount <= 2)
            {
                TriggerLearningTip("螟ｧ荳亥､ｫ縺ｧ縺呻ｼ∝､ｱ謨励°繧牙ｭｦ繧薙〒谺｡縺ｫ豢ｻ縺九＠縺ｾ縺励ｇ縺・);
            }
            else if (_detectionCount <= 5)
            {
                TriggerLearningTip("NPC縺ｮ蜍輔″繧偵ｈ縺剰ｦｳ蟇溘＠縺ｦ繧ｿ繧､繝溘Φ繧ｰ繧定ｨ医ｊ縺ｾ縺励ｇ縺・);
            }
            else
            {
                TriggerLearningTip("蛻･縺ｮ繝ｫ繝ｼ繝医ｒ隧ｦ縺励※縺ｿ繧九％縺ｨ繧偵♀蜍ｧ繧√＠縺ｾ縺・);
                // 繧医ｊ蜈ｷ菴鍋噪縺ｪ繝偵Φ繝医ｒ謠蝉ｾ・
                ProvideContextualHint();
            }
        }
        
        private void ProvideContextualHint()
        {
            if (_availableTips.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, _availableTips.Count);
                string tip = _availableTips[randomIndex];
                TriggerLearningTip($"繝偵Φ繝・ {tip}");
                _currentMetrics.HintsUsed++;
            }
        }
        
        private void TriggerLearningTip(string tip)
        {
            OnLearningTipTriggered?.Invoke(tip);
            
            // UI繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｫ騾夂衍
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
                StealthSkill.BasicMovement => "蝓ｺ譛ｬ遘ｻ蜍・,
                StealthSkill.NoiseControl => "髻ｳ髻ｿ蛻ｶ蠕｡",
                StealthSkill.CoverUtilization => "驕ｮ阡ｽ迚ｩ蛻ｩ逕ｨ",
                StealthSkill.AIBehaviorReading => "AI陦悟虚隱ｭ縺ｿ",
                StealthSkill.EnvironmentUsage => "迺ｰ蠅・ｴｻ逕ｨ",
                StealthSkill.TimingMastery => "繧ｿ繧､繝溘Φ繧ｰ鄙貞ｾ・,
                _ => skill.ToString()
            };
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ蟄ｦ鄙帝ｲ謐励ｒ蜿門ｾ・
        /// </summary>
        public float GetLearningProgress() => _learningProgress;
        
        /// <summary>
        /// 迚ｹ螳壹せ繧ｭ繝ｫ縺ｮ騾ｲ謐励ｒ蜿門ｾ・
        /// </summary>
        public float GetSkillProgress(StealthSkill skill) => _skillProgress.GetValueOrDefault(skill, 0f);
        
        /// <summary>
        /// 鄙貞ｾ礼憾豕√ｒ蜿門ｾ・
        /// </summary>
        public bool IsMasteryAchieved() => _masteryAchieved;
        
        /// <summary>
        /// 蟄ｦ鄙偵そ繝・す繝ｧ繝ｳ縺ｮ謇句虚繝ｪ繧ｻ繝・ヨ
        /// </summary>
        public void ResetLearningSession()
        {
            StartLearningSession();
        }
        
        /// <summary>
        /// Learn & Grow 繧ｷ繧ｹ繝・Β縺ｮ譛牙柑/辟｡蜉ｹ蛻・ｊ譖ｿ縺・
        /// </summary>
        public void SetLearnAndGrowEnabled(bool enabled)
        {
            _enableLearnAndGrowSystem = enabled;
            if (!enabled && _isTrackingSession)
            {
                _isTrackingSession = false;
                TriggerLearningTip("Learn & Grow 繧ｷ繧ｹ繝・Β縺檎┌蜉ｹ蛹悶＆繧後∪縺励◆");
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


