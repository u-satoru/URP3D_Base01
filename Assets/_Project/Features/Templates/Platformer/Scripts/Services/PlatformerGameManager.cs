using System;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformerゲームマネージャー：ServiceLocator + Event駆動ハイブリッドアーキテクチャ実装
    /// 中央ゲーム状態管理・プレイヤー制御・Learn & Grow価値実現
    /// </summary>
    public class PlatformerGameManager : IPlatformerGameManager
    {
        // ゲーム状態管理
        private GameState _currentState = GameState.MainMenu;
        private bool _isGamePaused = false;
        private bool _isPlayerAlive = true;
        private int _currentLevel = 1;
        private float _gameTime = 0f;

        // プレイヤー状態
        private int _playerScore = 0;
        private int _playerLives = 3;
        private int _playerHealth = 100;
        private int _maxHealth = 100;

        // 設定とタイマー
        private PlatformerGameplaySettings _settings;
        private float _invulnerabilityTimer = 0f;
        private bool _isInvulnerable = false;

        // Event駆動アーキテクチャ：他システムとの疎結合通信
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHealthChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;
        public event Action OnGameOver;
        public event Action OnLevelCompleted;
        public event Action<bool> OnGamePausedChanged;

        // プロパティ公開
        public GameState CurrentState => _currentState;
        public bool IsGamePaused => _isGamePaused;
        public bool IsPlayerAlive => _isPlayerAlive;
        public int CurrentLevel => _currentLevel;
        public float GameTime => _gameTime;
        public int PlayerScore => _playerScore;
        public int PlayerLives => _playerLives;
        public int PlayerHealth => _playerHealth;

        /// <summary>
        /// コンストラクタ：設定ベース初期化
        /// </summary>
        public PlatformerGameManager(PlatformerGameplaySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeFromSettings();

            Debug.Log("[PlatformerGameManager] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 設定からの初期化：ScriptableObjectベースのデータ管理
        /// </summary>
        private void InitializeFromSettings()
        {
            _playerLives = _settings.MaxLives;
            _playerHealth = _settings.StartingHealth;
            _maxHealth = _settings.MaxHealth;
            _playerScore = 0;
            _currentLevel = 1;
            _gameTime = 0f;
            _isPlayerAlive = true;

            Debug.Log($"Game initialized - Lives: {_playerLives}, Health: {_playerHealth}, Max Health: {_maxHealth}");
        }

        /// <summary>
        /// ゲーム開始：状態遷移とイベント通知
        /// </summary>
        public void StartGame()
        {
            ChangeGameState(GameState.Playing);
            _gameTime = 0f;
            _isPlayerAlive = true;

            // ServiceLocator経由で他サービスに通知
            NotifyGameStart();

            Debug.Log("Game started!");
        }

        /// <summary>
        /// ゲーム一時停止：状態管理パターン
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                _isGamePaused = true;
                ChangeGameState(GameState.Paused);
                OnGamePausedChanged?.Invoke(true);

                Debug.Log("Game paused.");
            }
        }

        /// <summary>
        /// ゲーム再開：状態復帰
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                _isGamePaused = false;
                ChangeGameState(GameState.Playing);
                OnGamePausedChanged?.Invoke(false);

                Debug.Log("Game resumed.");
            }
        }

        /// <summary>
        /// レベル再開始：チェックポイントシステム連携
        /// </summary>
        public void RestartLevel()
        {
            // ServiceLocator経由でCheckpointServiceから復帰
            var checkpointService = ServiceLocator.GetService<ICheckpointService>();
            checkpointService?.LoadFromCheckpoint();

            // プレイヤー状態リセット（設定に応じて）
            if (_settings.ResetLevelOnGameOver)
            {
                _playerHealth = _settings.StartingHealth;
                _isPlayerAlive = true;
                _isInvulnerable = false;
                _invulnerabilityTimer = 0f;
            }

            ChangeGameState(GameState.Playing);
            OnHealthChanged?.Invoke(_playerHealth);

            Debug.Log($"Level {_currentLevel} restarted.");
        }

        /// <summary>
        /// 次レベルへ：進行管理
        /// </summary>
        public void NextLevel()
        {
            _currentLevel++;

            // ServiceLocator経由でLevelGenerationServiceでレベル生成
            var levelService = ServiceLocator.GetService<ILevelGenerationService>();
            levelService?.GenerateLevel(_currentLevel);

            // 体力回復（設定に応じて）
            if (_settings.EnableHealthRegeneration)
            {
                _playerHealth = _maxHealth;
                OnHealthChanged?.Invoke(_playerHealth);
            }

            ChangeGameState(GameState.Playing);

            Debug.Log($"Advanced to level {_currentLevel}!");
        }

        /// <summary>
        /// ゲームオーバー：終了処理
        /// </summary>
        public void GameOver()
        {
            _isPlayerAlive = false;
            ChangeGameState(GameState.GameOver);
            OnGameOver?.Invoke();

            // ServiceLocator経由で統計サービスに記録
            RecordGameStatistics();

            Debug.Log("Game Over!");
        }

        /// <summary>
        /// レベル完了：達成処理
        /// </summary>
        public void CompleteLevel()
        {
            // 時間ボーナス計算
            if (_settings.EnableTimeBonusScore)
            {
                int timeBonus = Mathf.RoundToInt((_settings.LevelTimeLimit - _gameTime) * _settings.TimeBonusPerSecond);
                if (timeBonus > 0)
                {
                    AddScore(timeBonus);
                }
            }

            // レベル完了ボーナス
            AddScore(_settings.LevelCompletionScore);

            ChangeGameState(GameState.LevelComplete);
            OnLevelCompleted?.Invoke();

            // ServiceLocator経由でCheckpointServiceに保存
            var checkpointService = ServiceLocator.GetService<ICheckpointService>();
            checkpointService?.SaveProgress(_currentLevel, _playerScore, _playerLives);

            Debug.Log($"Level {_currentLevel} completed! Score: {_playerScore}");
        }

        /// <summary>
        /// プレイヤー体力設定：範囲制限付き
        /// </summary>
        public void SetPlayerHealth(int health)
        {
            _playerHealth = Mathf.Clamp(health, 0, _maxHealth);
            OnHealthChanged?.Invoke(_playerHealth);

            if (_playerHealth <= 0 && _isPlayerAlive)
            {
                PlayerDied();
            }
        }

        /// <summary>
        /// プレイヤーダメージ：無敵時間考慮
        /// </summary>
        public void DamagePlayer(int damage)
        {
            if (_isInvulnerable || !_isPlayerAlive) return;

            int actualDamage = Mathf.RoundToInt(damage * _settings.EnemyDamageMultiplier);
            SetPlayerHealth(_playerHealth - actualDamage);

            // 無敵時間設定
            _isInvulnerable = true;
            _invulnerabilityTimer = _settings.InvulnerabilityTime;

            // ServiceLocator経由でAudioServiceで効果音再生
            var audioService = ServiceLocator.GetService<IPlatformerAudioService>();
            audioService?.PlayDamageSound();

            Debug.Log($"Player took {actualDamage} damage. Health: {_playerHealth}");
        }

        /// <summary>
        /// プレイヤー回復：上限制限
        /// </summary>
        public void HealPlayer(int healing)
        {
            if (!_isPlayerAlive) return;

            SetPlayerHealth(_playerHealth + healing);
            Debug.Log($"Player healed {healing}. Health: {_playerHealth}");
        }

        /// <summary>
        /// ライフ追加：上限確認
        /// </summary>
        public void AddLife()
        {
            _playerLives++;
            OnLivesChanged?.Invoke(_playerLives);
            Debug.Log($"Life added. Lives: {_playerLives}");
        }

        /// <summary>
        /// ライフ減少：ゲームオーバー判定
        /// </summary>
        public void RemoveLife()
        {
            _playerLives = Mathf.Max(0, _playerLives - 1);
            OnLivesChanged?.Invoke(_playerLives);

            if (_playerLives <= 0)
            {
                GameOver();
            }
            else
            {
                RestartLevel();
            }

            Debug.Log($"Life lost. Lives: {_playerLives}");
        }

        /// <summary>
        /// スコア追加：倍率適用
        /// </summary>
        public void AddScore(int points)
        {
            int actualPoints = Mathf.RoundToInt(points * _settings.ScoreMultiplier);
            _playerScore += actualPoints;
            OnScoreChanged?.Invoke(_playerScore);

            Debug.Log($"Score added: {actualPoints}. Total: {_playerScore}");
        }

        /// <summary>
        /// プレイヤー統計リセット：新ゲーム用
        /// </summary>
        public void ResetPlayerStats()
        {
            InitializeFromSettings();
            OnScoreChanged?.Invoke(_playerScore);
            OnHealthChanged?.Invoke(_playerHealth);
            OnLivesChanged?.Invoke(_playerLives);

            Debug.Log("Player stats reset to default values.");
        }

        /// <summary>
        /// 設定更新：ランタイム設定変更対応
        /// </summary>
        public void UpdateSettings(PlatformerGameplaySettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));

            // 現在の体力上限を新設定に合わせて調整
            _maxHealth = _settings.MaxHealth;
            if (_playerHealth > _maxHealth)
            {
                SetPlayerHealth(_maxHealth);
            }

            Debug.Log("Game settings updated at runtime.");
        }

        /// <summary>
        /// 難易度設定：ゲームバランス調整
        /// </summary>
        public void SetGameDifficulty(PlatformerGameplaySettings.GameDifficulty difficulty)
        {
            _settings.ApplyDifficultySettings(difficulty);

            // 現在のゲーム状態に即座反映
            if (_currentState == GameState.Playing)
            {
                // ServiceLocator経由で他システムに難易度変更通知
                NotifyDifficultyChange(difficulty);
            }

            Debug.Log($"Game difficulty set to {difficulty}");
        }

        /// <summary>
        /// 状態変更：Event駆動通知
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            if (_currentState != newState)
            {
                GameState previousState = _currentState;
                _currentState = newState;
                OnGameStateChanged?.Invoke(newState);

                Debug.Log($"Game state changed: {previousState} -> {newState}");
            }
        }

        /// <summary>
        /// プレイヤー死亡処理：ライフ管理
        /// </summary>
        private void PlayerDied()
        {
            _isPlayerAlive = false;
            OnPlayerDied?.Invoke();

            // リスポーン遅延処理
            if (_settings.RespawnDelay > 0)
            {
                // 実際のゲームではCoroutineやTimerを使用
                Debug.Log($"Player will respawn in {_settings.RespawnDelay} seconds.");
            }

            RemoveLife();
        }

        /// <summary>
        /// ゲーム開始通知：ServiceLocator統合
        /// </summary>
        private void NotifyGameStart()
        {
            // ServiceLocator経由で他サービスに開始通知
            var audioService = ServiceLocator.GetService<IPlatformerAudioService>();
            audioService?.PlayBackgroundMusic();

            var uiService = ServiceLocator.GetService<IPlatformerUIService>();
            uiService?.ShowGameUI();
        }

        /// <summary>
        /// 難易度変更通知：ServiceLocator統合
        /// </summary>
        private void NotifyDifficultyChange(PlatformerGameplaySettings.GameDifficulty difficulty)
        {
            // ServiceLocator経由で他システムに通知
            var physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
            physicsService?.ApplyDifficultyModifiers(difficulty);
        }

        /// <summary>
        /// ゲーム統計記録：ServiceLocator統合
        /// </summary>
        private void RecordGameStatistics()
        {
            // ServiceLocator経由で統計サービスに記録（将来実装）
            Debug.Log($"Game Statistics - Level: {_currentLevel}, Score: {_playerScore}, Time: {_gameTime:F1}s");
        }

        /// <summary>
        /// 更新処理：時間管理と状態更新
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_currentState == GameState.Playing && !_isGamePaused)
            {
                // ゲーム時間更新
                _gameTime += deltaTime;

                // 時間制限チェック
                if (_settings.EnableTimeLimit && _gameTime >= _settings.LevelTimeLimit)
                {
                    Debug.Log("Time limit reached!");
                    DamagePlayer(_maxHealth); // 即座にゲームオーバー
                }

                // 無敵時間更新
                if (_isInvulnerable)
                {
                    _invulnerabilityTimer -= deltaTime;
                    if (_invulnerabilityTimer <= 0)
                    {
                        _isInvulnerable = false;
                    }
                }

                // 体力回復処理
                if (_settings.EnableHealthRegeneration && _playerHealth < _maxHealth && _isPlayerAlive)
                {
                    float healing = _settings.HealthRegenerationRate * deltaTime;
                    if (healing >= 1f)
                    {
                        HealPlayer(Mathf.FloorToInt(healing));
                    }
                }
            }
        }

        /// <summary>
        /// リソース解放：IDisposable実装
        /// </summary>
        public void Dispose()
        {
            // イベント解除
            OnGameStateChanged = null;
            OnScoreChanged = null;
            OnHealthChanged = null;
            OnLivesChanged = null;
            OnPlayerDied = null;
            OnGameOver = null;
            OnLevelCompleted = null;
            OnGamePausedChanged = null;

            Debug.Log("[PlatformerGameManager] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用デバッグ情報
        /// </summary>
        public void ShowDebugInfo()
        {
            Debug.Log("=== PlatformerGameManager Debug Info ===");
            Debug.Log($"State: {_currentState}");
            Debug.Log($"Paused: {_isGamePaused}");
            Debug.Log($"Player Alive: {_isPlayerAlive}");
            Debug.Log($"Level: {_currentLevel}");
            Debug.Log($"Time: {_gameTime:F1}s");
            Debug.Log($"Score: {_playerScore}");
            Debug.Log($"Lives: {_playerLives}");
            Debug.Log($"Health: {_playerHealth}/{_maxHealth}");
            Debug.Log($"Invulnerable: {_isInvulnerable} ({_invulnerabilityTimer:F1}s)");
        }
#endif
    }
}