using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer UI Service Implementation：ユーザーインターフェース・メニュー・HUD管理
    /// ServiceLocator + Event駆動アーキテクチャによる高度なUI管理システム
    /// Learn & Grow価値実現：直感的なUI・ゲーム状態の視覚化・プラットフォーマー特化HUD
    /// </summary>
    public class PlatformerUIService : IPlatformerUIService
    {
        #region Core Fields & Properties

        private readonly PlatformerUISettings _settings;
        private bool _isInitialized = false;
        private bool _uiEnabled = true;
        private bool _disposed = false;

        // ==================================================
        // IPlatformerService 基底インターフェース実装
        // ==================================================

        // IPlatformerServiceで必要なプロパティ
        public bool IsInitialized => _isInitialized;
        public bool IsEnabled => _uiEnabled;

        // UI State Management
        private bool _gameUIVisible = true;
        private bool _pauseMenuVisible = false;
        private bool _gameOverScreenVisible = false;
        private bool _levelCompleteScreenVisible = false;

        // Current UI Values
        private int _currentHealth = 100;
        private int _maxHealth = 100;
        private int _currentScore = 0;
        private int _currentLives = 3;

        #endregion

        #region UI Components

        // Main UI Containers
        private Canvas _mainCanvas;
        private GameObject _gameUIContainer;
        private GameObject _pauseMenuContainer;
        private GameObject _gameOverContainer;
        private GameObject _levelCompleteContainer;

        // HUD Elements
        private Image _healthBarFill;
        private Text _healthText;
        private Text _scoreText;
        private Text _livesText;
        private Text _timerText;

        // Menu Elements
        private Button _pauseResumeButton;
        private Button _pauseRestartButton;
        private Button _pauseMainMenuButton;
        private Button _gameOverRestartButton;
        private Button _gameOverMainMenuButton;
        private Button _levelCompleteNextButton;
        private Button _levelCompleteRestartButton;

        // Animation & Effects
        private readonly Dictionary<string, Coroutine> _activeAnimations = new Dictionary<string, Coroutine>();
        private MonoBehaviour _coroutineRunner;

        #endregion

        #region Events

        public event Action OnGameUIShown;
        public event Action OnGameUIHidden;
        public event Action OnPauseMenuShown;
        public event Action OnPauseMenuHidden;
        public event Action OnGameOverShown;
        public event Action OnLevelCompleteShown;
        public event Action<int, int> OnHealthChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;

        // Button Events
        public event Action OnResumeRequested;
        public event Action OnRestartRequested;
        public event Action OnMainMenuRequested;
        public event Action OnNextLevelRequested;

        #endregion

        #region Constructor & Initialization

        public PlatformerUIService(PlatformerUISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                CreateUIStructure();
                SetupUIComponents();
                ApplySettings();
                BindUIEvents();

                _isInitialized = true;

                Debug.Log("[PlatformerUIService] Successfully initialized with comprehensive UI management");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerUIService] Failed to initialize: {ex.Message}");
                throw;
            }
        }

        private void CreateUIStructure()
        {
            // Create main canvas if it doesn't exist
            _mainCanvas = GameObject.FindObjectOfType<Canvas>();
            if (_mainCanvas == null)
            {
                var canvasObject = new GameObject("PlatformerUI Canvas");
                _mainCanvas = canvasObject.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _mainCanvas.sortingOrder = 100;

                // Add CanvasScaler for responsive UI
                var scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                // Add GraphicRaycaster for UI interaction
                canvasObject.AddComponent<GraphicRaycaster>();

                GameObject.DontDestroyOnLoad(canvasObject);
            }

            // Create coroutine runner
            var runnerObject = new GameObject("PlatformerUI CoroutineRunner");
            _coroutineRunner = runnerObject.AddComponent<UICoroutineRunner>();
            GameObject.DontDestroyOnLoad(runnerObject);

            // Create UI containers
            CreateGameUI();
            CreatePauseMenu();
            CreateGameOverScreen();
            CreateLevelCompleteScreen();
        }

        private void CreateGameUI()
        {
            _gameUIContainer = new GameObject("GameUI");
            _gameUIContainer.transform.SetParent(_mainCanvas.transform, false);

            var rectTransform = _gameUIContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Create Health Bar
            CreateHealthBar();

            // Create Score Display
            CreateScoreDisplay();

            // Create Lives Display
            CreateLivesDisplay();

            // Create Timer Display
            if (_settings.ShowTimer)
            {
                CreateTimerDisplay();
            }
        }

        private void CreateHealthBar()
        {
            if (!_settings.ShowHealthBar) return;

            var healthBarContainer = new GameObject("HealthBar");
            healthBarContainer.transform.SetParent(_gameUIContainer.transform, false);

            var containerRect = healthBarContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(50, -50);
            containerRect.sizeDelta = new Vector2(200, 30);

            // Background
            var background = new GameObject("Background");
            background.transform.SetParent(healthBarContainer.transform, false);
            var bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill
            var fill = new GameObject("Fill");
            fill.transform.SetParent(healthBarContainer.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            _healthBarFill = fill.AddComponent<Image>();
            _healthBarFill.color = Color.green;
            _healthBarFill.type = Image.Type.Filled;
            _healthBarFill.fillMethod = Image.FillMethod.Horizontal;

            // Health Text
            var healthTextObj = new GameObject("HealthText");
            healthTextObj.transform.SetParent(healthBarContainer.transform, false);
            var textRect = healthTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _healthText = healthTextObj.AddComponent<Text>();
            _healthText.text = "100/100";
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.fontSize = 14;
            _healthText.alignment = TextAnchor.MiddleCenter;
            _healthText.color = Color.white;
        }

        private void CreateScoreDisplay()
        {
            if (!_settings.ShowScore) return;

            var scoreContainer = new GameObject("Score");
            scoreContainer.transform.SetParent(_gameUIContainer.transform, false);

            var containerRect = scoreContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.anchoredPosition = new Vector2(-100, -30);
            containerRect.sizeDelta = new Vector2(180, 40);

            _scoreText = scoreContainer.AddComponent<Text>();
            _scoreText.text = "Score: 0";
            _scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _scoreText.fontSize = 18;
            _scoreText.alignment = TextAnchor.MiddleRight;
            _scoreText.color = Color.white;
        }

        private void CreateLivesDisplay()
        {
            var livesContainer = new GameObject("Lives");
            livesContainer.transform.SetParent(_gameUIContainer.transform, false);

            var containerRect = livesContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(0, 1);
            containerRect.anchoredPosition = new Vector2(50, -100);
            containerRect.sizeDelta = new Vector2(120, 30);

            _livesText = livesContainer.AddComponent<Text>();
            _livesText.text = "Lives: 3";
            _livesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _livesText.fontSize = 16;
            _livesText.alignment = TextAnchor.MiddleLeft;
            _livesText.color = Color.white;
        }

        private void CreateTimerDisplay()
        {
            var timerContainer = new GameObject("Timer");
            timerContainer.transform.SetParent(_gameUIContainer.transform, false);

            var containerRect = timerContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 1);
            containerRect.anchorMax = new Vector2(0.5f, 1);
            containerRect.anchoredPosition = new Vector2(0, -30);
            containerRect.sizeDelta = new Vector2(120, 30);

            _timerText = timerContainer.AddComponent<Text>();
            _timerText.text = "00:00";
            _timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _timerText.fontSize = 16;
            _timerText.alignment = TextAnchor.MiddleCenter;
            _timerText.color = Color.white;
        }

        private void CreatePauseMenu()
        {
            _pauseMenuContainer = new GameObject("PauseMenu");
            _pauseMenuContainer.transform.SetParent(_mainCanvas.transform, false);

            var rectTransform = _pauseMenuContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Semi-transparent background
            var background = _pauseMenuContainer.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f);

            // Create pause menu panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(_pauseMenuContainer.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(300, 200);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Create buttons
            _pauseResumeButton = CreateButton("ResumeButton", panel.transform, new Vector2(0, 50), "Resume");
            _pauseRestartButton = CreateButton("RestartButton", panel.transform, new Vector2(0, 0), "Restart");
            _pauseMainMenuButton = CreateButton("MainMenuButton", panel.transform, new Vector2(0, -50), "Main Menu");

            _pauseMenuContainer.SetActive(false);
        }

        private void CreateGameOverScreen()
        {
            _gameOverContainer = new GameObject("GameOverScreen");
            _gameOverContainer.transform.SetParent(_mainCanvas.transform, false);

            var rectTransform = _gameOverContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Background
            var background = _gameOverContainer.AddComponent<Image>();
            background.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);

            // Create panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(_gameOverContainer.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(400, 250);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Game Over Title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(300, 50);

            var titleText = title.AddComponent<Text>();
            titleText.text = "GAME OVER";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 32;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.red;

            // Buttons
            _gameOverRestartButton = CreateButton("RestartButton", panel.transform, new Vector2(0, 0), "Restart");
            _gameOverMainMenuButton = CreateButton("MainMenuButton", panel.transform, new Vector2(0, -50), "Main Menu");

            _gameOverContainer.SetActive(false);
        }

        private void CreateLevelCompleteScreen()
        {
            _levelCompleteContainer = new GameObject("LevelCompleteScreen");
            _levelCompleteContainer.transform.SetParent(_mainCanvas.transform, false);

            var rectTransform = _levelCompleteContainer.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Background
            var background = _levelCompleteContainer.AddComponent<Image>();
            background.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);

            // Create panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(_levelCompleteContainer.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(400, 250);

            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Level Complete Title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0, 80);
            titleRect.sizeDelta = new Vector2(350, 50);

            var titleText = title.AddComponent<Text>();
            titleText.text = "LEVEL COMPLETE!";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 28;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.green;

            // Buttons
            _levelCompleteNextButton = CreateButton("NextLevelButton", panel.transform, new Vector2(0, 20), "Next Level");
            _levelCompleteRestartButton = CreateButton("RestartButton", panel.transform, new Vector2(0, -30), "Restart");

            _levelCompleteContainer.SetActive(false);
        }

        private Button CreateButton(string name, Transform parent, Vector2 position, string text)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = new Vector2(200, 40);

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 16;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            return button;
        }

        private void SetupUIComponents()
        {
            // Initialize UI with current values
            UpdateHealthBarImmediate();
            UpdateScoreImmediate();
            UpdateLivesImmediate();
        }

        private void ApplySettings()
        {
            // Apply visibility settings
            if (_healthBarFill != null && _healthBarFill.transform.parent != null)
            {
                _healthBarFill.transform.parent.gameObject.SetActive(_settings.ShowHealthBar);
            }

            if (_scoreText != null)
            {
                _scoreText.gameObject.SetActive(_settings.ShowScore);
            }

            if (_timerText != null)
            {
                _timerText.gameObject.SetActive(_settings.ShowTimer);
            }

            Debug.Log($"[PlatformerUIService] Settings applied - Health: {_settings.ShowHealthBar}, Score: {_settings.ShowScore}, Timer: {_settings.ShowTimer}");
        }

        private void BindUIEvents()
        {
            // Bind button events
            if (_pauseResumeButton != null)
                _pauseResumeButton.onClick.AddListener(() => OnResumeRequested?.Invoke());

            if (_pauseRestartButton != null)
                _pauseRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (_pauseMainMenuButton != null)
                _pauseMainMenuButton.onClick.AddListener(() => OnMainMenuRequested?.Invoke());

            if (_gameOverRestartButton != null)
                _gameOverRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());

            if (_gameOverMainMenuButton != null)
                _gameOverMainMenuButton.onClick.AddListener(() => OnMainMenuRequested?.Invoke());

            if (_levelCompleteNextButton != null)
                _levelCompleteNextButton.onClick.AddListener(() => OnNextLevelRequested?.Invoke());

            if (_levelCompleteRestartButton != null)
                _levelCompleteRestartButton.onClick.AddListener(() => OnRestartRequested?.Invoke());
        }

        #endregion

        #region Public Methods

        public void ShowGameUI()
        {
            if (_gameUIContainer != null)
            {
                _gameUIContainer.SetActive(true);
                _gameUIVisible = true;
                OnGameUIShown?.Invoke();
                Debug.Log("[PlatformerUIService] Game UI shown");
            }
        }

        // ==================================================
        // IPlatformerService インターフェース実装
        // ==================================================

        public void Enable()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[PlatformerUIService] Cannot enable - not initialized yet.");
                return;
            }

            _uiEnabled = true;
            if (_mainCanvas != null)
            {
                _mainCanvas.enabled = true;
            }
            Debug.Log("[PlatformerUIService] Enabled.");
        }

        public void Disable()
        {
            _uiEnabled = false;
            if (_mainCanvas != null)
            {
                _mainCanvas.enabled = false;
            }
            Debug.Log("[PlatformerUIService] Disabled.");
        }

        public void Reset()
        {
            // Reset UI state to defaults
            _currentHealth = _maxHealth;
            _currentScore = 0;
            _currentLives = 3;

            // Hide all menus
            HideAllScreens();

            // Update UI displays
            UpdateHealthBarImmediate();
            UpdateScoreImmediate();
            UpdateLivesImmediate();

            Debug.Log("[PlatformerUIService] Reset completed.");
        }

        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                // Verify UI service can integrate with other services if needed
                bool integration = _isInitialized && _mainCanvas != null;
                Debug.Log($"[PlatformerUIService] ServiceLocator integration verified: {integration}");
                return integration;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerUIService] ServiceLocator integration failed: {ex.Message}");
                return false;
            }
        }

        public void UpdateService(float deltaTime)
        {
            if (!_isInitialized || !_uiEnabled) return;

            // Update any dynamic UI elements that need frame updates
            // This can be extended for animations or time-based UI effects
        }

        public void HideGameUI()
        {
            if (_gameUIContainer != null)
            {
                _gameUIContainer.SetActive(false);
                _gameUIVisible = false;
                OnGameUIHidden?.Invoke();
                Debug.Log("[PlatformerUIService] Game UI hidden");
            }
        }

        public void ShowPauseMenu()
        {
            if (_pauseMenuContainer != null)
            {
                _pauseMenuContainer.SetActive(true);
                _pauseMenuVisible = true;
                OnPauseMenuShown?.Invoke();
                Debug.Log("[PlatformerUIService] Pause menu shown");
            }
        }

        public void HidePauseMenu()
        {
            if (_pauseMenuContainer != null)
            {
                _pauseMenuContainer.SetActive(false);
                _pauseMenuVisible = false;
                OnPauseMenuHidden?.Invoke();
                Debug.Log("[PlatformerUIService] Pause menu hidden");
            }
        }

        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            _currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            _maxHealth = Mathf.Max(1, maxHealth);

            if (_healthBarFill != null && _healthText != null)
            {
                StartAnimation("HealthUpdate", AnimateHealthBar(_currentHealth, _maxHealth));
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void UpdateScore(int score)
        {
            _currentScore = Mathf.Max(0, score);

            if (_scoreText != null)
            {
                StartAnimation("ScoreUpdate", AnimateScore(_currentScore));
            }

            OnScoreChanged?.Invoke(_currentScore);
        }

        public void UpdateLives(int lives)
        {
            _currentLives = Mathf.Max(0, lives);

            if (_livesText != null)
            {
                StartAnimation("LivesUpdate", AnimateLives(_currentLives));
            }

            OnLivesChanged?.Invoke(_currentLives);
        }

        public void ShowGameOverScreen()
        {
            if (_gameOverContainer != null)
            {
                _gameOverContainer.SetActive(true);
                _gameOverScreenVisible = true;
                OnGameOverShown?.Invoke();
                Debug.Log("[PlatformerUIService] Game over screen shown");
            }
        }

        public void ShowLevelCompleteScreen()
        {
            if (_levelCompleteContainer != null)
            {
                _levelCompleteContainer.SetActive(true);
                _levelCompleteScreenVisible = true;
                OnLevelCompleteShown?.Invoke();
                Debug.Log("[PlatformerUIService] Level complete screen shown");
            }
        }

        public void UpdateTimer(float timeInSeconds)
        {
            if (_timerText != null && _settings.ShowTimer)
            {
                int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
                int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
                _timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        public void HideAllScreens()
        {
            HidePauseMenu();

            if (_gameOverContainer != null)
            {
                _gameOverContainer.SetActive(false);
                _gameOverScreenVisible = false;
            }

            if (_levelCompleteContainer != null)
            {
                _levelCompleteContainer.SetActive(false);
                _levelCompleteScreenVisible = false;
            }
        }

        #endregion

        #region Animation Methods

        private void StartAnimation(string animationName, IEnumerator animation)
        {
            if (_coroutineRunner == null) return;

            // Stop existing animation if running
            if (_activeAnimations.ContainsKey(animationName))
            {
                _coroutineRunner.StopCoroutine(_activeAnimations[animationName]);
                _activeAnimations.Remove(animationName);
            }

            // Start new animation
            var coroutine = _coroutineRunner.StartCoroutine(animation);
            _activeAnimations[animationName] = coroutine;
        }

        private IEnumerator AnimateHealthBar(int targetHealth, int maxHealth)
        {
            if (_healthBarFill == null || _healthText == null) yield break;

            float targetFillAmount = (float)targetHealth / maxHealth;
            float currentFillAmount = _healthBarFill.fillAmount;
            float animationTime = 0.5f;
            float elapsed = 0f;

            while (elapsed < animationTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationTime;

                _healthBarFill.fillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, t);
                _healthText.text = $"{targetHealth}/{maxHealth}";

                // Update color based on health percentage
                if (targetFillAmount > 0.6f)
                    _healthBarFill.color = Color.Lerp(_healthBarFill.color, Color.green, t);
                else if (targetFillAmount > 0.3f)
                    _healthBarFill.color = Color.Lerp(_healthBarFill.color, Color.yellow, t);
                else
                    _healthBarFill.color = Color.Lerp(_healthBarFill.color, Color.red, t);

                yield return null;
            }

            _healthBarFill.fillAmount = targetFillAmount;
            _activeAnimations.Remove("HealthUpdate");
        }

        private IEnumerator AnimateScore(int targetScore)
        {
            if (_scoreText == null) yield break;

            int startScore = _currentScore;
            float animationTime = 0.3f;
            float elapsed = 0f;

            while (elapsed < animationTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationTime;

                int currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                _scoreText.text = $"Score: {currentDisplayScore}";

                yield return null;
            }

            _scoreText.text = $"Score: {targetScore}";
            _activeAnimations.Remove("ScoreUpdate");
        }

        private IEnumerator AnimateLives(int targetLives)
        {
            if (_livesText == null) yield break;

            _livesText.text = $"Lives: {targetLives}";

            // Simple flash animation for lives change
            Color originalColor = _livesText.color;
            _livesText.color = Color.yellow;

            yield return new WaitForSeconds(0.2f);

            _livesText.color = originalColor;
            _activeAnimations.Remove("LivesUpdate");
        }

        #endregion

        #region Private Helper Methods

        private void UpdateHealthBarImmediate()
        {
            if (_healthBarFill != null && _healthText != null)
            {
                float fillAmount = (float)_currentHealth / _maxHealth;
                _healthBarFill.fillAmount = fillAmount;
                _healthText.text = $"{_currentHealth}/{_maxHealth}";

                // Set color based on health
                if (fillAmount > 0.6f)
                    _healthBarFill.color = Color.green;
                else if (fillAmount > 0.3f)
                    _healthBarFill.color = Color.yellow;
                else
                    _healthBarFill.color = Color.red;
            }
        }

        private void UpdateScoreImmediate()
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {_currentScore}";
            }
        }

        private void UpdateLivesImmediate()
        {
            if (_livesText != null)
            {
                _livesText.text = $"Lives: {_currentLives}";
            }
        }

        #endregion

        #region Lifecycle Management

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Stop all animations
                foreach (var animation in _activeAnimations.Values)
                {
                    if (_coroutineRunner != null && animation != null)
                    {
                        _coroutineRunner.StopCoroutine(animation);
                    }
                }
                _activeAnimations.Clear();

                // Unbind UI events
                UnbindUIEvents();

                // Destroy UI objects
                if (_mainCanvas != null && _mainCanvas.gameObject != null)
                {
                    GameObject.Destroy(_mainCanvas.gameObject);
                }

                if (_coroutineRunner != null && _coroutineRunner.gameObject != null)
                {
                    GameObject.Destroy(_coroutineRunner.gameObject);
                }

                // Clear events
                OnGameUIShown = null;
                OnGameUIHidden = null;
                OnPauseMenuShown = null;
                OnPauseMenuHidden = null;
                OnGameOverShown = null;
                OnLevelCompleteShown = null;
                OnHealthChanged = null;
                OnScoreChanged = null;
                OnLivesChanged = null;
                OnResumeRequested = null;
                OnRestartRequested = null;
                OnMainMenuRequested = null;
                OnNextLevelRequested = null;

                _disposed = true;

                Debug.Log("[PlatformerUIService] Disposed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlatformerUIService] Error during disposal: {ex.Message}");
            }
        }

        private void UnbindUIEvents()
        {
            if (_pauseResumeButton != null)
                _pauseResumeButton.onClick.RemoveAllListeners();

            if (_pauseRestartButton != null)
                _pauseRestartButton.onClick.RemoveAllListeners();

            if (_pauseMainMenuButton != null)
                _pauseMainMenuButton.onClick.RemoveAllListeners();

            if (_gameOverRestartButton != null)
                _gameOverRestartButton.onClick.RemoveAllListeners();

            if (_gameOverMainMenuButton != null)
                _gameOverMainMenuButton.onClick.RemoveAllListeners();

            if (_levelCompleteNextButton != null)
                _levelCompleteNextButton.onClick.RemoveAllListeners();

            if (_levelCompleteRestartButton != null)
                _levelCompleteRestartButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Debug & Diagnostics

        public void ShowUIDebugInfo()
        {
            if (!_isInitialized)
            {
                Debug.Log("[PlatformerUIService] Service not initialized");
                return;
            }

            Debug.Log("=== Platformer UI Service Debug Info ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"Game UI Visible: {_gameUIVisible}");
            Debug.Log($"Pause Menu Visible: {_pauseMenuVisible}");
            Debug.Log($"Game Over Screen Visible: {_gameOverScreenVisible}");
            Debug.Log($"Level Complete Screen Visible: {_levelCompleteScreenVisible}");
            Debug.Log($"Current Health: {_currentHealth}/{_maxHealth}");
            Debug.Log($"Current Score: {_currentScore}");
            Debug.Log($"Current Lives: {_currentLives}");
            Debug.Log($"Active Animations: {_activeAnimations.Count}");

            if (_settings != null)
            {
                Debug.Log($"Show Health Bar: {_settings.ShowHealthBar}");
                Debug.Log($"Show Score: {_settings.ShowScore}");
                Debug.Log($"Show Timer: {_settings.ShowTimer}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper MonoBehaviour for running coroutines in the UI service
    /// </summary>
    public class UICoroutineRunner : MonoBehaviour
    {
        // This class exists solely to provide coroutine functionality to the service
    }
}
