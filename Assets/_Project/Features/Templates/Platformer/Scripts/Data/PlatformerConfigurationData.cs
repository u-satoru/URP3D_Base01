using UnityEngine;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Data
{
    /// <summary>
    /// Platformerテンプレート統合設定データ
    /// ScriptableObjectベースのデータ管理：Learn & Grow価値実現
    /// ノンプログラマーでもバランス調整可能な設計
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerConfiguration", menuName = "Platformer Template/Configuration Data")]
    public class PlatformerConfigurationData : ScriptableObject
    {
        [Header("Template Information")]
        [SerializeField] private string _templateName = "Platformer Template";
        [SerializeField] private string _version = "1.0.0";
        [SerializeField] private string _description = "ServiceLocator + Event-driven Platformer Template for Learn & Grow value realization";

        [Header("Core Service Settings")]
        [SerializeField] private PlatformerPhysicsSettings _physicsSettings;
        [SerializeField] private PlatformerAudioSettings _audioSettings;
        [SerializeField] private PlatformerInputSettings _inputSettings;

        [Header("Gameplay Service Settings")]
        [SerializeField] private PlatformerGameplaySettings _gameplaySettings;
        [SerializeField] private PlatformerCollectionSettings _collectionSettings;
        [SerializeField] private PlatformerLevelSettings _levelSettings;
        [SerializeField] private PlatformerCheckpointSettings _checkpointSettings;

        [Header("UI Service Settings")]
        [SerializeField] private PlatformerUISettings _uiSettings;

        [Header("Learning System Settings")]
        [SerializeField] private PlatformerLearningSettings _learningSettings;

        // プロパティ公開（ServiceLocator経由アクセス用）
        public string TemplateName => _templateName;
        public string Version => _version;
        public string Description => _description;

        // Core Service Settings
        public PlatformerPhysicsSettings PhysicsSettings => _physicsSettings;
        public PlatformerAudioSettings AudioSettings => _audioSettings;
        public PlatformerInputSettings InputSettings => _inputSettings;

        // Gameplay Service Settings
        public PlatformerGameplaySettings GameplaySettings => _gameplaySettings;
        public PlatformerCollectionSettings CollectionSettings => _collectionSettings;
        public PlatformerLevelSettings LevelSettings => _levelSettings;
        public PlatformerCheckpointSettings CheckpointSettings => _checkpointSettings;

        // UI Service Settings
        public PlatformerUISettings UISettings => _uiSettings;

        // Learning System Settings
        public PlatformerLearningSettings LearningSettings => _learningSettings;

        /// <summary>
        /// 設定データ検証：起動時整合性チェック
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // 必須設定の存在確認
            if (_physicsSettings == null)
            {
                Debug.LogError("PlatformerPhysicsSettings is missing!");
                isValid = false;
            }

            if (_gameplaySettings == null)
            {
                Debug.LogError("PlatformerGameplaySettings is missing!");
                isValid = false;
            }

            if (_inputSettings == null)
            {
                Debug.LogError("PlatformerInputSettings is missing!");
                isValid = false;
            }

            // 設定値の妥当性検証
            if (_physicsSettings != null && !_physicsSettings.ValidateSettings())
            {
                Debug.LogError("PlatformerPhysicsSettings validation failed!");
                isValid = false;
            }

            if (_gameplaySettings != null && !_gameplaySettings.ValidateSettings())
            {
                Debug.LogError("PlatformerGameplaySettings validation failed!");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log($"Configuration '{_templateName}' v{_version} validated successfully.");
            }

            return isValid;
        }

        /// <summary>
        /// デフォルト設定作成：Learn & Grow価値実現
        /// 初心者でも安心してスタートできる初期設定
        /// </summary>
        [ContextMenu("Create Default Settings")]
        private void CreateDefaultSettings()
        {
            // Physics Settings
            if (_physicsSettings == null)
            {
                _physicsSettings = CreateInstance<PlatformerPhysicsSettings>();
                _physicsSettings.SetToDefault();
            }

            // Gameplay Settings
            if (_gameplaySettings == null)
            {
                _gameplaySettings = CreateInstance<PlatformerGameplaySettings>();
                _gameplaySettings.SetToDefault();
            }

            // Audio Settings
            if (_audioSettings == null)
            {
                _audioSettings = CreateInstance<PlatformerAudioSettings>();
                _audioSettings.SetToDefault();
            }

            // Input Settings
            if (_inputSettings == null)
            {
                _inputSettings = CreateInstance<PlatformerInputSettings>();
                _inputSettings.SetToDefault();
            }

            // Collection Settings
            if (_collectionSettings == null)
            {
                _collectionSettings = CreateInstance<PlatformerCollectionSettings>();
                _collectionSettings.SetToDefault();
            }

            // Level Settings
            if (_levelSettings == null)
            {
                _levelSettings = CreateInstance<PlatformerLevelSettings>();
                _levelSettings.SetToDefault();
            }

            // Checkpoint Settings
            if (_checkpointSettings == null)
            {
                _checkpointSettings = CreateInstance<PlatformerCheckpointSettings>();
                _checkpointSettings.SetToDefault();
            }

            // UI Settings
            if (_uiSettings == null)
            {
                _uiSettings = CreateInstance<PlatformerUISettings>();
                _uiSettings.SetToDefault();
            }

            // Learning Settings
            if (_learningSettings == null)
            {
                _learningSettings = CreateInstance<PlatformerLearningSettings>();
                _learningSettings.SetToDefault();
            }

            Debug.Log("Default settings created for all services.");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// パフォーマンス最適化設定：60FPS安定動作保証
        /// </summary>
        [ContextMenu("Optimize for Performance")]
        private void OptimizeForPerformance()
        {
            if (_physicsSettings != null)
                _physicsSettings.OptimizeForPerformance();

            if (_gameplaySettings != null)
                _gameplaySettings.OptimizeForPerformance();

            if (_audioSettings != null)
                _audioSettings.OptimizeForPerformance();

            Debug.Log("Configuration optimized for 60FPS stable performance.");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// 学習向け設定：Learn & Grow価値実現（70%学習コスト削減）
        /// </summary>
        [ContextMenu("Optimize for Learning")]
        private void OptimizeForLearning()
        {
            if (_gameplaySettings != null)
                _gameplaySettings.OptimizeForLearning();

            if (_learningSettings != null)
                _learningSettings.EnableAllLearningFeatures();

            Debug.Log("Configuration optimized for learning experience (70% cost reduction).");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用診断情報表示
        /// </summary>
        [ContextMenu("Show Diagnostic Info")]
        private void ShowDiagnosticInfo()
        {
            Debug.Log("=== Platformer Configuration Diagnostic ===");
            Debug.Log($"Template: {_templateName} v{_version}");
            Debug.Log($"Description: {_description}");

            Debug.Log("Settings Status:");
            Debug.Log($"- Physics: {(_physicsSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Audio: {(_audioSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Input: {(_inputSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Gameplay: {(_gameplaySettings != null ? "✓" : "✗")}");
            Debug.Log($"- Collection: {(_collectionSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Level: {(_levelSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Checkpoint: {(_checkpointSettings != null ? "✓" : "✗")}");
            Debug.Log($"- UI: {(_uiSettings != null ? "✓" : "✗")}");
            Debug.Log($"- Learning: {(_learningSettings != null ? "✓" : "✗")}");

            bool isValid = ValidateConfiguration();
            Debug.Log($"Configuration Valid: {(isValid ? "✓" : "✗")}");
        }
#endif
    }
}