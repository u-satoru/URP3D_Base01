using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// Layer 1: Configuration Foundation - メインのステルステンプレート設定
    /// ScriptableObjectベースのモジュラー設定システム
    /// Learn & Grow価値実現のための統合設定管理
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/StealthTemplateConfig")]
    public class StealthTemplateConfig : ScriptableObject
    {
        [Header("Core Stealth Systems")]
        [SerializeField] private StealthMechanicsConfig _mechanics;
        [SerializeField] private StealthAIConfig _aiConfiguration;
        [SerializeField] private StealthEnvironmentConfig _environment;
        [SerializeField] private StealthAudioConfig _audioSettings;
        [SerializeField] private StealthUIConfig _uiSettings;

        [Header("Learning & Tutorial Settings")]
        [SerializeField] private StealthTutorialConfig _tutorialConfig;
        [SerializeField] private StealthProgressionConfig _progressionConfig;

        [Header("Performance Optimization")]
        [SerializeField] private StealthPerformanceConfig _performanceConfig;

        // Event Channels（Event駆動アーキテクチャ準拠）
        [Header("Event Channels")]
        [SerializeField] private GameEvent _onStealthStateChanged;
        [SerializeField] private GameEvent _onDetectionLevelChanged;
        [SerializeField] private GameEvent _onEnvironmentInteraction;

        // Properties with validation
        public StealthMechanicsConfig Mechanics => _mechanics;
        public StealthAIConfig AIConfiguration => _aiConfiguration;
        public StealthEnvironmentConfig Environment => _environment;
        public StealthAudioConfig AudioSettings => _audioSettings;
        public StealthUIConfig UISettings => _uiSettings;
        public StealthTutorialConfig TutorialConfig => _tutorialConfig;
        public StealthProgressionConfig ProgressionConfig => _progressionConfig;
        public StealthPerformanceConfig PerformanceConfig => _performanceConfig;

        // Event Channel Properties
        public GameEvent OnStealthStateChanged => _onStealthStateChanged;
        public GameEvent OnDetectionLevelChanged => _onDetectionLevelChanged;
        public GameEvent OnEnvironmentInteraction => _onEnvironmentInteraction;

        private void OnValidate()
        {
            // Odin Validator統合による設定検証
            ValidateConfiguration();
        }

        /// <summary>
        /// 設定全体の検証とデフォルト値設定
        /// Learn & Grow価値実現のための適切な設定保証
        /// </summary>
        private void ValidateConfiguration()
        {
            // 各設定モジュールの初期化確認
            if (_mechanics == null)
                _mechanics = new StealthMechanicsConfig();

            if (_aiConfiguration == null)
                _aiConfiguration = new StealthAIConfig();

            if (_environment == null)
                _environment = new StealthEnvironmentConfig();

            if (_audioSettings == null)
                _audioSettings = new StealthAudioConfig();

            if (_uiSettings == null)
                _uiSettings = new StealthUIConfig();

            if (_tutorialConfig == null)
                _tutorialConfig = new StealthTutorialConfig();

            if (_progressionConfig == null)
                _progressionConfig = new StealthProgressionConfig();

            if (_performanceConfig == null)
                _performanceConfig = new StealthPerformanceConfig();

            // Learn & Grow価値実現のための設定検証
            ValidateLearnAndGrowSettings();
        }

        /// <summary>
        /// Learn & Grow価値実現のための設定検証
        /// 15分ゲームプレイ、70%学習コスト削減の要件確認
        /// </summary>
        private void ValidateLearnAndGrowSettings()
        {
            // チュートリアル設定検証
            if (_tutorialConfig != null)
            {
                // 15分ゲームプレイ実現のための基本設定確認
                if (_tutorialConfig.EnableQuickStart == false)
                {
                    Debug.LogWarning("StealthTemplateConfig: QuickStart is disabled. Enable for Learn & Grow value realization.");
                }
            }

            // パフォーマンス設定検証
            if (_performanceConfig != null)
            {
                // 50体NPC、0.1ms/frameの要件確認
                if (_performanceConfig.MaxSimultaneousNPCs > 50)
                {
                    Debug.LogWarning("StealthTemplateConfig: MaxSimultaneousNPCs exceeds recommended limit (50).");
                }
            }
        }

        /// <summary>
        /// 設定のリセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Configuration")]
        public void ResetConfiguration()
        {
            _mechanics = new StealthMechanicsConfig();
            _aiConfiguration = new StealthAIConfig();
            _environment = new StealthEnvironmentConfig();
            _audioSettings = new StealthAudioConfig();
            _uiSettings = new StealthUIConfig();
            _tutorialConfig = new StealthTutorialConfig();
            _progressionConfig = new StealthProgressionConfig();
            _performanceConfig = new StealthPerformanceConfig();

            Debug.Log("StealthTemplateConfig: Configuration has been reset to defaults.");
        }
    }
}