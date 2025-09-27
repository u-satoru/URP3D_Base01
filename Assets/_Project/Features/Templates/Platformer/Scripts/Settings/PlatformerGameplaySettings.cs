using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformerゲームプレイ設定：中央ゲーム状態管理
    /// ServiceLocator統合：ゲームマネージャー制御設定
    /// Learn & Grow価値実現：学習体験最適化パラメータ
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerGameplaySettings", menuName = "Platformer Template/Settings/Gameplay Settings")]
    public class PlatformerGameplaySettings : ScriptableObject
    {
        [Header("Player Life System")]
        [SerializeField, Range(1, 10)] private int _maxLives = 3;
        [SerializeField, Range(1, 100)] private int _maxHealth = 100;
        [SerializeField, Range(1, 50)] private int _startingHealth = 100;
        [SerializeField] private bool _enableHealthRegeneration = false;
        [SerializeField, Range(0.1f, 10f)] private float _healthRegenerationRate = 1f;

        [Header("Respawn System")]
        [SerializeField, Range(0f, 10f)] private float _respawnDelay = 2f;
        [SerializeField, Range(0f, 5f)] private float _invulnerabilityTime = 2f;
        [SerializeField] private bool _enableCheckpointSystem = true;
        [SerializeField] private bool _resetLevelOnGameOver = true;

        [Header("Scoring System")]
        [SerializeField] private bool _enableScoring = true;
        [SerializeField, Range(10, 1000)] private int _collectibleBaseScore = 100;
        [SerializeField, Range(100, 10000)] private int _enemyDefeatScore = 500;
        [SerializeField, Range(1000, 50000)] private int _levelCompletionScore = 5000;
        [SerializeField, Range(1f, 10f)] private float _scoreMultiplier = 1f;

        [Header("Time System")]
        [SerializeField] private bool _enableTimeLimit = false;
        [SerializeField, Range(30f, 600f)] private float _levelTimeLimit = 300f; // 5 minutes
        [SerializeField] private bool _enableTimeBonusScore = true;
        [SerializeField, Range(1, 100)] private int _timeBonusPerSecond = 10;

        [Header("Difficulty Settings")]
        [SerializeField] private GameDifficulty _defaultDifficulty = GameDifficulty.Normal;
        [SerializeField, Range(0.1f, 3f)] private float _enemySpeedMultiplier = 1f;
        [SerializeField, Range(0.5f, 2f)] private float _enemyDamageMultiplier = 1f;
        [SerializeField, Range(0.5f, 3f)] private float _platformSpeedMultiplier = 1f;

        [Header("Collectible System")]
        [SerializeField] private bool _enableCollectibles = true;
        [SerializeField, Range(1, 100)] private int _totalCollectiblesInLevel = 20;
        [SerializeField] private bool _requireAllCollectiblesForCompletion = false;
        [SerializeField, Range(0f, 100f)] private float _collectibleCompletionPercentage = 80f;

        [Header("Power-Up System")]
        [SerializeField] private bool _enablePowerUps = true;
        [SerializeField, Range(5f, 60f)] private float _powerUpDuration = 15f;
        [SerializeField] private bool _allowMultiplePowerUps = false;
        [SerializeField, Range(0.1f, 2f)] private float _powerUpSpawnRate = 0.3f;

        [Header("Learning System")]
        [SerializeField] private bool _enableTutorialMode = true;
        [SerializeField] private bool _showControlHints = true;
        [SerializeField] private bool _enableProgressTracking = true;
        [SerializeField, Range(1f, 10f)] private float _learningCurveAdjustment = 1f;
        [SerializeField] private bool _enableAdaptiveDifficulty = true;

        [Header("Performance Settings")]
        [SerializeField, Range(30, 120)] private int _targetFrameRate = 60;
        [SerializeField] private bool _enableVSync = true;
        [SerializeField] private bool _enablePerformanceMonitoring = true;

        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugMode = false;
        [SerializeField] private bool _showDebugUI = false;
        [SerializeField] private bool _enableInfiniteHealth = false;
        [SerializeField] private bool _enableInfiniteLives = false;

        public enum GameDifficulty
        {
            Easy,
            Normal,
            Hard,
            Expert
        }

        // プロパティ公開（ServiceLocator経由アクセス）
        public int MaxLives => _maxLives;
        public int MaxHealth => _maxHealth;
        public int StartingHealth => _startingHealth;
        public bool EnableHealthRegeneration => _enableHealthRegeneration;
        public float HealthRegenerationRate => _healthRegenerationRate;

        public float RespawnDelay => _respawnDelay;
        public float InvulnerabilityTime => _invulnerabilityTime;
        public bool EnableCheckpointSystem => _enableCheckpointSystem;
        public bool ResetLevelOnGameOver => _resetLevelOnGameOver;

        public bool EnableScoring => _enableScoring;
        public int CollectibleBaseScore => _collectibleBaseScore;
        public int EnemyDefeatScore => _enemyDefeatScore;
        public int LevelCompletionScore => _levelCompletionScore;
        public float ScoreMultiplier => _scoreMultiplier;

        public bool EnableTimeLimit => _enableTimeLimit;
        public float LevelTimeLimit => _levelTimeLimit;
        public bool EnableTimeBonusScore => _enableTimeBonusScore;
        public int TimeBonusPerSecond => _timeBonusPerSecond;

        public GameDifficulty DefaultDifficulty => _defaultDifficulty;
        public float EnemySpeedMultiplier => _enemySpeedMultiplier;
        public float EnemyDamageMultiplier => _enemyDamageMultiplier;
        public float PlatformSpeedMultiplier => _platformSpeedMultiplier;

        public bool EnableCollectibles => _enableCollectibles;
        public int TotalCollectiblesInLevel => _totalCollectiblesInLevel;
        public bool RequireAllCollectiblesForCompletion => _requireAllCollectiblesForCompletion;
        public float CollectibleCompletionPercentage => _collectibleCompletionPercentage;

        public bool EnablePowerUps => _enablePowerUps;
        public float PowerUpDuration => _powerUpDuration;
        public bool AllowMultiplePowerUps => _allowMultiplePowerUps;
        public float PowerUpSpawnRate => _powerUpSpawnRate;

        public bool EnableTutorialMode => _enableTutorialMode;
        public bool ShowControlHints => _showControlHints;
        public bool EnableProgressTracking => _enableProgressTracking;
        public float LearningCurveAdjustment => _learningCurveAdjustment;
        public bool EnableAdaptiveDifficulty => _enableAdaptiveDifficulty;

        public int TargetFrameRate => _targetFrameRate;
        public bool EnableVSync => _enableVSync;
        public bool EnablePerformanceMonitoring => _enablePerformanceMonitoring;

        public bool EnableDebugMode => _enableDebugMode;
        public bool ShowDebugUI => _showDebugUI;
        public bool EnableInfiniteHealth => _enableInfiniteHealth;
        public bool EnableInfiniteLives => _enableInfiniteLives;

        /// <summary>
        /// 難易度別設定調整：ゲームバランス自動調整
        /// </summary>
        public void ApplyDifficultySettings(GameDifficulty difficulty)
        {
            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    _maxLives = 5;
                    _enemySpeedMultiplier = 0.7f;
                    _enemyDamageMultiplier = 0.5f;
                    _platformSpeedMultiplier = 0.8f;
                    _invulnerabilityTime = 3f;
                    _enableAdaptiveDifficulty = true;
                    break;

                case GameDifficulty.Normal:
                    _maxLives = 3;
                    _enemySpeedMultiplier = 1f;
                    _enemyDamageMultiplier = 1f;
                    _platformSpeedMultiplier = 1f;
                    _invulnerabilityTime = 2f;
                    _enableAdaptiveDifficulty = false;
                    break;

                case GameDifficulty.Hard:
                    _maxLives = 2;
                    _enemySpeedMultiplier = 1.3f;
                    _enemyDamageMultiplier = 1.5f;
                    _platformSpeedMultiplier = 1.2f;
                    _invulnerabilityTime = 1.5f;
                    _enableAdaptiveDifficulty = false;
                    break;

                case GameDifficulty.Expert:
                    _maxLives = 1;
                    _enemySpeedMultiplier = 1.5f;
                    _enemyDamageMultiplier = 2f;
                    _platformSpeedMultiplier = 1.5f;
                    _invulnerabilityTime = 1f;
                    _enableAdaptiveDifficulty = false;
                    break;
            }

            Debug.Log($"Difficulty set to {difficulty}");
        }

        /// <summary>
        /// 設定値検証：起動時整合性確認
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // ライフシステムの妥当性
            if (_maxLives <= 0 || _maxHealth <= 0 || _startingHealth <= 0)
            {
                Debug.LogError("Invalid life/health settings: All values must be positive.");
                isValid = false;
            }

            if (_startingHealth > _maxHealth)
            {
                Debug.LogWarning("Starting health exceeds max health. Adjusting to max health.");
                _startingHealth = _maxHealth;
            }

            // タイムシステムの妥当性
            if (_enableTimeLimit && _levelTimeLimit <= 0)
            {
                Debug.LogError("Invalid time limit: Must be positive when time limit is enabled.");
                isValid = false;
            }

            // 収集システムの妥当性
            if (_enableCollectibles && _totalCollectiblesInLevel <= 0)
            {
                Debug.LogError("Invalid collectible count: Must be positive when collectibles are enabled.");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// デフォルト設定：Learn & Grow価値実現
        /// </summary>
        public void SetToDefault()
        {
            _maxLives = 3;
            _maxHealth = 100;
            _startingHealth = 100;
            _enableHealthRegeneration = false;
            _healthRegenerationRate = 1f;

            _respawnDelay = 2f;
            _invulnerabilityTime = 2f;
            _enableCheckpointSystem = true;
            _resetLevelOnGameOver = true;

            _enableScoring = true;
            _collectibleBaseScore = 100;
            _enemyDefeatScore = 500;
            _levelCompletionScore = 5000;
            _scoreMultiplier = 1f;

            _enableTimeLimit = false;
            _levelTimeLimit = 300f;
            _enableTimeBonusScore = true;
            _timeBonusPerSecond = 10;

            _defaultDifficulty = GameDifficulty.Normal;
            _enemySpeedMultiplier = 1f;
            _enemyDamageMultiplier = 1f;
            _platformSpeedMultiplier = 1f;

            _enableCollectibles = true;
            _totalCollectiblesInLevel = 20;
            _requireAllCollectiblesForCompletion = false;
            _collectibleCompletionPercentage = 80f;

            _enablePowerUps = true;
            _powerUpDuration = 15f;
            _allowMultiplePowerUps = false;
            _powerUpSpawnRate = 0.3f;

            _enableTutorialMode = true;
            _showControlHints = true;
            _enableProgressTracking = true;
            _learningCurveAdjustment = 1f;
            _enableAdaptiveDifficulty = true;

            _targetFrameRate = 60;
            _enableVSync = true;
            _enablePerformanceMonitoring = true;

            _enableDebugMode = false;
            _showDebugUI = false;
            _enableInfiniteHealth = false;
            _enableInfiniteLives = false;

            Debug.Log("PlatformerGameplaySettings set to default values.");
        }

        /// <summary>
        /// パフォーマンス最適化設定：60FPS安定動作
        /// </summary>
        public void OptimizeForPerformance()
        {
            _targetFrameRate = 60;
            _enableVSync = false; // パフォーマンス優先
            _enablePerformanceMonitoring = true;

            // デバッグ機能無効化
            _enableDebugMode = false;
            _showDebugUI = false;

            // 処理負荷軽減
            _enableAdaptiveDifficulty = false; // 動的調整無効化

            Debug.Log("PlatformerGameplaySettings optimized for 60FPS performance.");
        }

        /// <summary>
        /// 学習向け設定：Learn & Grow価値実現（70%学習コスト削減）
        /// </summary>
        public void OptimizeForLearning()
        {
            // 初心者に優しい設定
            _maxLives = 5; // ライフ増加
            _enableHealthRegeneration = true; // 体力回復有効
            _invulnerabilityTime = 3f; // 無敵時間延長

            // 学習支援機能有効化
            _enableTutorialMode = true;
            _showControlHints = true;
            _enableProgressTracking = true;
            _enableAdaptiveDifficulty = true;

            // 緩やかな学習曲線
            _learningCurveAdjustment = 0.7f; // 難易度を緩やかに

            // デバッグ支援
            _enableDebugMode = true;
            _showDebugUI = true;

            Debug.Log("PlatformerGameplaySettings optimized for learning experience (70% cost reduction).");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用難易度テスト
        /// </summary>
        [ContextMenu("Test Easy Difficulty")]
        private void TestEasyDifficulty() => ApplyDifficultySettings(GameDifficulty.Easy);

        [ContextMenu("Test Normal Difficulty")]
        private void TestNormalDifficulty() => ApplyDifficultySettings(GameDifficulty.Normal);

        [ContextMenu("Test Hard Difficulty")]
        private void TestHardDifficulty() => ApplyDifficultySettings(GameDifficulty.Hard);

        [ContextMenu("Test Expert Difficulty")]
        private void TestExpertDifficulty() => ApplyDifficultySettings(GameDifficulty.Expert);
#endif
    }
}
