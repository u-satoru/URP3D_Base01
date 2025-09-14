using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// ジャンル固有のテンプレート設定を管理するScriptableObject
    /// TASK-004.1: GenreTemplateConfig (ScriptableObject) システム実装
    /// DESIGN.md Template Configuration Layer準拠
    /// </summary>
    [CreateAssetMenu(fileName = "GenreTemplateConfig", menuName = "Templates/Genre Template Config")]
    public class GenreTemplateConfig : ScriptableObject
    {
        [Header("基本設定")]
        [SerializeField] private GenreType _genreType;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(3, 5)] private string _description;
        
        [Header("ゲームプレイ設定")]
        [SerializeField] private string _mainScenePath;
        [SerializeField] private string _uiScenePath;
        [SerializeField] private string _settingsScenePath;
        [SerializeField] private int _estimatedGameplayMinutes = 15;
        
        [Header("Cinemachine 3.1統合設定")]
        [SerializeField] private string _cameraProfilePath;
        [SerializeField] private bool _enableFirstPersonMode = false;
        [SerializeField] private bool _enableThirdPersonMode = true;
        [SerializeField] private bool _enableAimMode = false;
        [SerializeField] private bool _enableCoverMode = false;
        
        [Header("Input System設定")]
        [SerializeField] private string _inputActionAssetPath;
        [SerializeField] private List<string> _enabledActionMaps = new List<string>();
        
        [Header("AI統合設定")]
        [SerializeField] private bool _useAIVisualSensor = false;
        [SerializeField] private bool _useAIAuditorySensor = false;
        [SerializeField] private bool _useAIStateMachine = false;
        [SerializeField] private string _aiConfigurationPath;
        
        [Header("Audio設定")]
        [SerializeField] private bool _useStealthAudio = false;
        [SerializeField] private string _audioMixerPath;
        [SerializeField] private List<string> _requiredAudioGroups = new List<string>();
        
        [Header("学習・チュートリアル設定")]
        [SerializeField] private bool _hasInteractiveTutorial = false;
        [SerializeField] private string _tutorialScenePath;
        [SerializeField] private List<string> _learningObjectives = new List<string>();
        
        [Header("パフォーマンス設定")]
        [SerializeField] private int _maxNPCCount = 10;
        [SerializeField] private bool _requiresOptimization = false;
        [SerializeField] private List<string> _performanceNotes = new List<string>();
        
        // Properties
        public GenreType GenreType => _genreType;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? GenreUtilities.GetDisplayName(_genreType) : _displayName;
        public string Description => _description;
        public string MainScenePath => _mainScenePath;
        public string UIScenePath => _uiScenePath;
        public string SettingsScenePath => _settingsScenePath;
        public int EstimatedGameplayMinutes => _estimatedGameplayMinutes;
        
        // Cinemachine Properties
        public string CameraProfilePath => _cameraProfilePath;
        public bool EnableFirstPersonMode => _enableFirstPersonMode;
        public bool EnableThirdPersonMode => _enableThirdPersonMode;
        public bool EnableAimMode => _enableAimMode;
        public bool EnableCoverMode => _enableCoverMode;
        
        // Input Properties
        public string InputActionAssetPath => _inputActionAssetPath;
        public IReadOnlyList<string> EnabledActionMaps => _enabledActionMaps.AsReadOnly();
        
        // AI Properties
        public bool UseAIVisualSensor => _useAIVisualSensor;
        public bool UseAIAuditorySensor => _useAIAuditorySensor;
        public bool UseAIStateMachine => _useAIStateMachine;
        public string AIConfigurationPath => _aiConfigurationPath;
        
        // Audio Properties
        public bool UseStealthAudio => _useStealthAudio;
        public string AudioMixerPath => _audioMixerPath;
        public IReadOnlyList<string> RequiredAudioGroups => _requiredAudioGroups.AsReadOnly();
        
        // Learning Properties
        public bool HasInteractiveTutorial => _hasInteractiveTutorial;
        public string TutorialScenePath => _tutorialScenePath;
        public IReadOnlyList<string> LearningObjectives => _learningObjectives.AsReadOnly();
        
        // Performance Properties
        public int MaxNPCCount => _maxNPCCount;
        public bool RequiresOptimization => _requiresOptimization;
        public IReadOnlyList<string> PerformanceNotes => _performanceNotes.AsReadOnly();
        
        /// <summary>
        /// ジャンルの優先度を取得
        /// </summary>
        /// <returns>優先度レベル</returns>
        public GenrePriority GetPriority()
        {
            return GenreUtilities.GetPriority(_genreType);
        }
        
        /// <summary>
        /// 推定学習時間を取得（分）
        /// </summary>
        /// <returns>学習時間（分）</returns>
        public int GetEstimatedLearningTimeMinutes()
        {
            return GenreUtilities.GetEstimatedLearningTimeMinutes(_genreType);
        }
        
        /// <summary>
        /// テンプレートの設定が有効かどうかチェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(_mainScenePath))
                return false;
                
            if (_estimatedGameplayMinutes <= 0)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// AI機能が必要かどうかチェック
        /// </summary>
        /// <returns>AI機能が必要な場合true</returns>
        public bool RequiresAI()
        {
            return _useAIVisualSensor || _useAIAuditorySensor || _useAIStateMachine;
        }
        
        /// <summary>
        /// エディタでの初期化（開発用）
        /// </summary>
        [ContextMenu("Initialize Default Values")]
        private void InitializeDefaults()
        {
            _displayName = GenreUtilities.GetDisplayName(_genreType);
            _estimatedGameplayMinutes = 15;
            _maxNPCCount = 10;
            
            // ジャンル固有の初期値を設定
            switch (_genreType)
            {
                case GenreType.Stealth:
                    _useAIVisualSensor = true;
                    _useAIAuditorySensor = true;
                    _useAIStateMachine = true;
                    _useStealthAudio = true;
                    _enableThirdPersonMode = true;
                    _enableCoverMode = true;
                    _maxNPCCount = 50;
                    _requiresOptimization = true;
                    break;
                    
                case GenreType.FPS:
                    _enableFirstPersonMode = true;
                    _enableAimMode = true;
                    break;
                    
                case GenreType.TPS:
                    _enableThirdPersonMode = true;
                    _enableAimMode = true;
                    _enableCoverMode = true;
                    break;
                    
                case GenreType.ActionRPG:
                    _useAIStateMachine = true;
                    _enableThirdPersonMode = true;
                    _maxNPCCount = 20;
                    break;
                    
                case GenreType.Platformer:
                    _enableThirdPersonMode = true;
                    break;
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}