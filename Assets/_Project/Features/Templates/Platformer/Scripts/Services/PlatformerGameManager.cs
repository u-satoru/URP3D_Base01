using System;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer繧ｲ繝ｼ繝繝槭ロ繝ｼ繧ｸ繝｣繝ｼ・售erviceLocator + Event鬧・虚繝上う繝悶Μ繝・ラ繧｢繝ｼ繧ｭ繝・け繝√Ε螳溯｣・
    /// 荳ｭ螟ｮ繧ｲ繝ｼ繝迥ｶ諷狗ｮ｡逅・・繝励Ξ繧､繝､繝ｼ蛻ｶ蠕｡繝ｻLearn & Grow萓｡蛟､螳溽樟
    /// </summary>
    public class PlatformerGameManager : IPlatformerGameManager
    {
        // 繧ｲ繝ｼ繝迥ｶ諷狗ｮ｡逅・
        private GameState _currentState = GameState.MainMenu;
        private bool _isGamePaused = false;
        private bool _isPlayerAlive = true;
        private int _currentLevel = 1;
        private float _gameTime = 0f;

        // 繝励Ξ繧､繝､繝ｼ迥ｶ諷・
        private int _playerScore = 0;
        private int _playerLives = 3;
        private int _playerHealth = 100;
        private int _maxHealth = 100;

        // 險ｭ螳壹→繧ｿ繧､繝槭・
        private PlatformerGameplaySettings _settings;
        private float _invulnerabilityTimer = 0f;
        private bool _isInvulnerable = false;

        // Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε・壻ｻ悶す繧ｹ繝・Β縺ｨ縺ｮ逍守ｵ仙粋騾壻ｿ｡
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnHealthChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;
        public event Action OnGameOver;
        public event Action OnLevelCompleted;
        public event Action<bool> OnGamePausedChanged;

        // 繝励Ο繝代ユ繧｣蜈ｬ髢・
        public GameState CurrentState => _currentState;
        public bool IsGamePaused => _isGamePaused;
        public bool IsPlayerAlive => _isPlayerAlive;
        public int CurrentLevel => _currentLevel;
        public float GameTime => _gameTime;
        public int PlayerScore => _playerScore;
        public int PlayerLives => _playerLives;
        public int PlayerHealth => _playerHealth;

        /// <summary>
        /// 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・夊ｨｭ螳壹・繝ｼ繧ｹ蛻晄悄蛹・
        /// </summary>
        public PlatformerGameManager(PlatformerGameplaySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            InitializeFromSettings();

            Debug.Log("[PlatformerGameManager] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 險ｭ螳壹°繧峨・蛻晄悄蛹厄ｼ售criptableObject繝吶・繧ｹ縺ｮ繝・・繧ｿ邂｡逅・
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
        /// 繧ｲ繝ｼ繝髢句ｧ具ｼ夂憾諷矩・遘ｻ縺ｨ繧､繝吶Φ繝磯夂衍
        /// </summary>
        public void StartGame()
        {
            ChangeGameState(GameState.Playing);
            _gameTime = 0f;
            _isPlayerAlive = true;

            // ServiceLocator邨檎罰縺ｧ莉悶し繝ｼ繝薙せ縺ｫ騾夂衍
            NotifyGameStart();

            Debug.Log("Game started!");
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝荳譎ょ●豁｢・夂憾諷狗ｮ｡逅・ヱ繧ｿ繝ｼ繝ｳ
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
        /// 繧ｲ繝ｼ繝蜀埼幕・夂憾諷句ｾｩ蟶ｰ
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
        /// 繝ｬ繝吶Ν蜀埼幕蟋具ｼ壹メ繧ｧ繝・け繝昴う繝ｳ繝医す繧ｹ繝・Β騾｣謳ｺ
        /// </summary>
        public void RestartLevel()
        {
            // ServiceLocator邨檎罰縺ｧCheckpointService縺九ｉ蠕ｩ蟶ｰ
            var checkpointService = ServiceLocator.GetService<ICheckpointService>();
            checkpointService?.LoadFromCheckpoint();

            // 繝励Ξ繧､繝､繝ｼ迥ｶ諷九Μ繧ｻ繝・ヨ・郁ｨｭ螳壹↓蠢懊§縺ｦ・・
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
        /// 谺｡繝ｬ繝吶Ν縺ｸ・夐ｲ陦檎ｮ｡逅・
        /// </summary>
        public void NextLevel()
        {
            _currentLevel++;

            // ServiceLocator邨檎罰縺ｧLevelGenerationService縺ｧ繝ｬ繝吶Ν逕滓・
            var levelService = ServiceLocator.GetService<ILevelGenerationService>();
            levelService?.GenerateLevel(_currentLevel);

            // 菴灘鴨蝗槫ｾｩ・郁ｨｭ螳壹↓蠢懊§縺ｦ・・
            if (_settings.EnableHealthRegeneration)
            {
                _playerHealth = _maxHealth;
                OnHealthChanged?.Invoke(_playerHealth);
            }

            ChangeGameState(GameState.Playing);

            Debug.Log($"Advanced to level {_currentLevel}!");
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・・夂ｵゆｺ・・逅・
        /// </summary>
        public void GameOver()
        {
            _isPlayerAlive = false;
            ChangeGameState(GameState.GameOver);
            OnGameOver?.Invoke();

            // ServiceLocator邨檎罰縺ｧ邨ｱ險医し繝ｼ繝薙せ縺ｫ險倬鹸
            RecordGameStatistics();

            Debug.Log("Game Over!");
        }

        /// <summary>
        /// 繝ｬ繝吶Ν螳御ｺ・ｼ夐＃謌仙・逅・
        /// </summary>
        public void CompleteLevel()
        {
            // 譎る俣繝懊・繝翫せ險育ｮ・
            if (_settings.EnableTimeBonusScore)
            {
                int timeBonus = Mathf.RoundToInt((_settings.LevelTimeLimit - _gameTime) * _settings.TimeBonusPerSecond);
                if (timeBonus > 0)
                {
                    AddScore(timeBonus);
                }
            }

            // 繝ｬ繝吶Ν螳御ｺ・・繝ｼ繝翫せ
            AddScore(_settings.LevelCompletionScore);

            ChangeGameState(GameState.LevelComplete);
            OnLevelCompleted?.Invoke();

            // ServiceLocator邨檎罰縺ｧCheckpointService縺ｫ菫晏ｭ・
            var checkpointService = ServiceLocator.GetService<ICheckpointService>();
            checkpointService?.SaveProgress(_currentLevel, _playerScore, _playerLives);

            Debug.Log($"Level {_currentLevel} completed! Score: {_playerScore}");
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ菴灘鴨險ｭ螳夲ｼ夂ｯ・峇蛻ｶ髯蝉ｻ倥″
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
        /// 繝励Ξ繧､繝､繝ｼ繝繝｡繝ｼ繧ｸ・夂┌謨ｵ譎る俣閠・・
        /// </summary>
        public void DamagePlayer(int damage)
        {
            if (_isInvulnerable || !_isPlayerAlive) return;

            int actualDamage = Mathf.RoundToInt(damage * _settings.EnemyDamageMultiplier);
            SetPlayerHealth(_playerHealth - actualDamage);

            // 辟｡謨ｵ譎る俣險ｭ螳・
            _isInvulnerable = true;
            _invulnerabilityTimer = _settings.InvulnerabilityTime;

            // ServiceLocator邨檎罰縺ｧAudioService縺ｧ蜉ｹ譫憺浹蜀咲函
            var audioService = ServiceLocator.GetService<IPlatformerAudioService>();
            audioService?.PlayDamageSound();

            Debug.Log($"Player took {actualDamage} damage. Health: {_playerHealth}");
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ蝗槫ｾｩ・壻ｸ企剞蛻ｶ髯・
        /// </summary>
        public void HealPlayer(int healing)
        {
            if (!_isPlayerAlive) return;

            SetPlayerHealth(_playerHealth + healing);
            Debug.Log($"Player healed {healing}. Health: {_playerHealth}");
        }

        /// <summary>
        /// 繝ｩ繧､繝戊ｿｽ蜉・壻ｸ企剞遒ｺ隱・
        /// </summary>
        public void AddLife()
        {
            _playerLives++;
            OnLivesChanged?.Invoke(_playerLives);
            Debug.Log($"Life added. Lives: {_playerLives}");
        }

        /// <summary>
        /// 繝ｩ繧､繝墓ｸ帛ｰ托ｼ壹ご繝ｼ繝繧ｪ繝ｼ繝舌・蛻､螳・
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
        /// 繧ｹ繧ｳ繧｢霑ｽ蜉・壼咲紫驕ｩ逕ｨ
        /// </summary>
        public void AddScore(int points)
        {
            int actualPoints = Mathf.RoundToInt(points * _settings.ScoreMultiplier);
            _playerScore += actualPoints;
            OnScoreChanged?.Invoke(_playerScore);

            Debug.Log($"Score added: {actualPoints}. Total: {_playerScore}");
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ邨ｱ險医Μ繧ｻ繝・ヨ・壽眠繧ｲ繝ｼ繝逕ｨ
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
        /// 險ｭ螳壽峩譁ｰ・壹Λ繝ｳ繧ｿ繧､繝險ｭ螳壼､画峩蟇ｾ蠢・
        /// </summary>
        public void UpdateSettings(PlatformerGameplaySettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));

            // 迴ｾ蝨ｨ縺ｮ菴灘鴨荳企剞繧呈眠險ｭ螳壹↓蜷医ｏ縺帙※隱ｿ謨ｴ
            _maxHealth = _settings.MaxHealth;
            if (_playerHealth > _maxHealth)
            {
                SetPlayerHealth(_maxHealth);
            }

            Debug.Log("Game settings updated at runtime.");
        }

        /// <summary>
        /// 髮｣譏灘ｺｦ險ｭ螳夲ｼ壹ご繝ｼ繝繝舌Λ繝ｳ繧ｹ隱ｿ謨ｴ
        /// </summary>
        public void SetGameDifficulty(PlatformerGameplaySettings.GameDifficulty difficulty)
        {
            _settings.ApplyDifficultySettings(difficulty);

            // 迴ｾ蝨ｨ縺ｮ繧ｲ繝ｼ繝迥ｶ諷九↓蜊ｳ蠎ｧ蜿肴丐
            if (_currentState == GameState.Playing)
            {
                // ServiceLocator邨檎罰縺ｧ莉悶す繧ｹ繝・Β縺ｫ髮｣譏灘ｺｦ螟画峩騾夂衍
                NotifyDifficultyChange(difficulty);
            }

            Debug.Log($"Game difficulty set to {difficulty}");
        }

        /// <summary>
        /// 迥ｶ諷句､画峩・哘vent鬧・虚騾夂衍
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
        /// 繝励Ξ繧､繝､繝ｼ豁ｻ莠｡蜃ｦ逅・ｼ壹Λ繧､繝慕ｮ｡逅・
        /// </summary>
        private void PlayerDied()
        {
            _isPlayerAlive = false;
            OnPlayerDied?.Invoke();

            // 繝ｪ繧ｹ繝昴・繝ｳ驕・ｻｶ蜃ｦ逅・
            if (_settings.RespawnDelay > 0)
            {
                // 螳滄圀縺ｮ繧ｲ繝ｼ繝縺ｧ縺ｯCoroutine繧Уimer繧剃ｽｿ逕ｨ
                Debug.Log($"Player will respawn in {_settings.RespawnDelay} seconds.");
            }

            RemoveLife();
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝髢句ｧ矩夂衍・售erviceLocator邨ｱ蜷・
        /// </summary>
        private void NotifyGameStart()
        {
            // ServiceLocator邨檎罰縺ｧ莉悶し繝ｼ繝薙せ縺ｫ髢句ｧ矩夂衍
            var audioService = ServiceLocator.GetService<IPlatformerAudioService>();
            audioService?.PlayBackgroundMusic();

            var uiService = ServiceLocator.GetService<IPlatformerUIService>();
            uiService?.ShowGameUI();
        }

        /// <summary>
        /// 髮｣譏灘ｺｦ螟画峩騾夂衍・售erviceLocator邨ｱ蜷・
        /// </summary>
        private void NotifyDifficultyChange(PlatformerGameplaySettings.GameDifficulty difficulty)
        {
            // ServiceLocator邨檎罰縺ｧ莉悶す繧ｹ繝・Β縺ｫ騾夂衍
            var physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
            physicsService?.ApplyDifficultyModifiers(difficulty);
        }

        /// <summary>
        /// 繧ｲ繝ｼ繝邨ｱ險郁ｨ倬鹸・售erviceLocator邨ｱ蜷・
        /// </summary>
        private void RecordGameStatistics()
        {
            // ServiceLocator邨檎罰縺ｧ邨ｱ險医し繝ｼ繝薙せ縺ｫ險倬鹸・亥ｰ・擂螳溯｣・ｼ・
            Debug.Log($"Game Statistics - Level: {_currentLevel}, Score: {_playerScore}, Time: {_gameTime:F1}s");
        }

        /// <summary>
        /// 譖ｴ譁ｰ蜃ｦ逅・ｼ壽凾髢鍋ｮ｡逅・→迥ｶ諷区峩譁ｰ
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_currentState == GameState.Playing && !_isGamePaused)
            {
                // 繧ｲ繝ｼ繝譎る俣譖ｴ譁ｰ
                _gameTime += deltaTime;

                // 譎る俣蛻ｶ髯舌メ繧ｧ繝・け
                if (_settings.EnableTimeLimit && _gameTime >= _settings.LevelTimeLimit)
                {
                    Debug.Log("Time limit reached!");
                    DamagePlayer(_maxHealth); // 蜊ｳ蠎ｧ縺ｫ繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・
                }

                // 辟｡謨ｵ譎る俣譖ｴ譁ｰ
                if (_isInvulnerable)
                {
                    _invulnerabilityTimer -= deltaTime;
                    if (_invulnerabilityTimer <= 0)
                    {
                        _isInvulnerable = false;
                    }
                }

                // 菴灘鴨蝗槫ｾｩ蜃ｦ逅・
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
        /// 繝ｪ繧ｽ繝ｼ繧ｹ隗｣謾ｾ・唔Disposable螳溯｣・
        /// </summary>
        public void Dispose()
        {
            // 繧､繝吶Φ繝郁ｧ｣髯､
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
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ繝・ヰ繝・げ諠・ｱ
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


