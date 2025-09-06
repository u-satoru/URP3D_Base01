using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core.Events;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Core.UI
{
    /// <summary>
    /// メニューシステムを管理するクラス。
    /// メインメニュー、設定メニュー、ポーズメニューなどを制御します。
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [TabGroup("Menu Elements", "Main Menu")]
        [LabelText("Main Menu Panel")]
        [SerializeField] private GameObject mainMenuPanel;
        
        [TabGroup("Menu Elements", "Main Menu")]
        [LabelText("Play Button")]
        [SerializeField] private Button playButton;
        
        [TabGroup("Menu Elements", "Main Menu")]
        [LabelText("Settings Button")]
        [SerializeField] private Button settingsButton;
        
        [TabGroup("Menu Elements", "Main Menu")]
        [LabelText("Quit Button")]
        [SerializeField] private Button quitButton;
        
        [TabGroup("Menu Elements", "Pause Menu")]
        [LabelText("Pause Menu Panel")]
        [SerializeField] private GameObject pauseMenuPanel;
        
        [TabGroup("Menu Elements", "Pause Menu")]
        [LabelText("Resume Button")]
        [SerializeField] private Button resumeButton;
        
        [TabGroup("Menu Elements", "Pause Menu")]
        [LabelText("Main Menu Button")]
        [SerializeField] private Button mainMenuButton;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("Settings Panel")]
        [SerializeField] private GameObject settingsPanel;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("Master Volume Slider")]
        [SerializeField] private Slider masterVolumeSlider;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("Music Volume Slider")]
        [SerializeField] private Slider musicVolumeSlider;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("SFX Volume Slider")]
        [SerializeField] private Slider sfxVolumeSlider;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("Graphics Quality Dropdown")]
        [SerializeField] private Dropdown graphicsQualityDropdown;
        
        [TabGroup("Menu Elements", "Settings Menu")]
        [LabelText("Back Button")]
        [SerializeField] private Button backButton;
        
        [TabGroup("Menu Events", "Game State")]
        [LabelText("Game Started")]
        [SerializeField] private GameEvent onGameStarted;
        
        [TabGroup("Menu Events", "Game State")]
        [LabelText("Game Paused")]
        [SerializeField] private GameEvent onGamePaused;
        
        [TabGroup("Menu Events", "Game State")]
        [LabelText("Game Resumed")]
        [SerializeField] private GameEvent onGameResumed;
        
        [TabGroup("Menu Events", "Game State")]
        [LabelText("Return to Main Menu")]
        [SerializeField] private GameEvent onReturnToMainMenu;
        
        [TabGroup("Menu Events", "Settings")]
        [LabelText("Settings Changed")]
        [SerializeField] private GameEvent onSettingsChanged;
        
        [TabGroup("Menu Settings", "Scene Management")]
        [LabelText("Game Scene Name")]
        [SerializeField] private string gameSceneName = "GameScene";
        
        [TabGroup("Menu Settings", "Scene Management")]
        [LabelText("Main Menu Scene Name")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [TabGroup("Menu Settings", "Animation")]
        [LabelText("Menu Transition Duration")]
        [PropertyRange(0.1f, 2f)]
        [SuffixLabel("s", overlay: true)]
        [SerializeField] private float menuTransitionDuration = 0.5f;
        
        [TabGroup("Menu Settings", "Animation")]
        [LabelText("Button Scale Effect")]
        [PropertyRange(1f, 1.3f)]
        [SerializeField] private float buttonScaleEffect = 1.1f;
        
        [TabGroup("Menu Debug", "Current State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Current Menu")]
        private string currentMenu = "None";
        
        [TabGroup("Menu Debug", "Current State")]
        [ReadOnly]
        [ShowInInspector]
        [LabelText("Is Game Paused")]
        private bool isGamePaused = false;
        
        private void Awake()
        {
            InitializeMenus();
            SetupButtonListeners();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        /// <summary>
        /// メニューを初期化します
        /// </summary>
        private void InitializeMenus()
        {
            // 初期状態ですべてのメニューを非表示
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
                currentMenu = "MainMenu";
            }
            
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
                
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
                
            // 最初にメインメニューを表示（シーンによって判断）
            if (SceneManager.GetActiveScene().name == mainMenuSceneName)
            {
                ShowMainMenu();
            }
            
            LoadSettings();
        }
        
        /// <summary>
        /// ボタンのリスナーを設定します
        /// </summary>
        private void SetupButtonListeners()
        {
            // メインメニューボタン
            if (playButton != null)
            {
                playButton.onClick.AddListener(StartGame);
                SetupButtonEffect(playButton);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ShowSettings);
                SetupButtonEffect(settingsButton);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
                SetupButtonEffect(quitButton);
            }
            
            // ポーズメニューボタン
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
                SetupButtonEffect(resumeButton);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
                SetupButtonEffect(mainMenuButton);
            }
            
            // 設定メニューボタン
            if (backButton != null)
            {
                backButton.onClick.AddListener(HideSettings);
                SetupButtonEffect(backButton);
            }
            
            // 設定項目のリスナー
            SetupSettingsListeners();
        }
        
        /// <summary>
        /// 設定項目のリスナーを設定します
        /// </summary>
        private void SetupSettingsListeners()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                
            if (graphicsQualityDropdown != null)
                graphicsQualityDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);
        }
        
        /// <summary>
        /// ボタンにホバー・クリック効果を追加します
        /// </summary>
        private void SetupButtonEffect(Button button)
        {
            var buttonTransform = button.transform;
            
            // ホバー効果
            var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            // PointerEnter
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((_) => {
                buttonTransform.DOScale(buttonScaleEffect, 0.2f).SetEase(Ease.OutQuad);
            });
            eventTrigger.triggers.Add(pointerEnter);
            
            // PointerExit
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((_) => {
                buttonTransform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
            });
            eventTrigger.triggers.Add(pointerExit);
        }
        
        /// <summary>
        /// 入力処理を行います
        /// </summary>
        private void HandleInput()
        {
            // Escapeキーでポーズメニュー切替
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SceneManager.GetActiveScene().name == gameSceneName)
                {
                    if (isGamePaused)
                        ResumeGame();
                    else
                        PauseGame();
                }
            }
        }
        
        /// <summary>
        /// メインメニューを表示します
        /// </summary>
        public void ShowMainMenu()
        {
            HideAllMenus();
            
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
                AnimateMenuIn(mainMenuPanel);
                currentMenu = "MainMenu";
            }
        }
        
        /// <summary>
        /// ポーズメニューを表示します
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
                AnimateMenuIn(pauseMenuPanel);
                currentMenu = "PauseMenu";
            }
        }
        
        /// <summary>
        /// 設定メニューを表示します
        /// </summary>
        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                AnimateMenuIn(settingsPanel);
                currentMenu = "Settings";
            }
        }
        
        /// <summary>
        /// 設定メニューを非表示にします
        /// </summary>
        public void HideSettings()
        {
            if (settingsPanel != null)
            {
                AnimateMenuOut(settingsPanel, () => {
                    settingsPanel.SetActive(false);
                    
                    // 前のメニューに戻る
                    if (SceneManager.GetActiveScene().name == mainMenuSceneName)
                        ShowMainMenu();
                    else
                        ShowPauseMenu();
                });
            }
        }
        
        /// <summary>
        /// すべてのメニューを非表示にします
        /// </summary>
        private void HideAllMenus()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            currentMenu = "None";
        }
        
        /// <summary>
        /// ゲームを開始します
        /// </summary>
        public void StartGame()
        {
            onGameStarted?.Raise();
            SceneManager.LoadScene(gameSceneName);
        }
        
        /// <summary>
        /// ゲームを一時停止します
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0f;
            isGamePaused = true;
            ShowPauseMenu();
            onGamePaused?.Raise();
        }
        
        /// <summary>
        /// ゲームを再開します
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            isGamePaused = false;
            HideAllMenus();
            onGameResumed?.Raise();
        }
        
        /// <summary>
        /// メインメニューに戻ります
        /// </summary>
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isGamePaused = false;
            onReturnToMainMenu?.Raise();
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        /// <summary>
        /// ゲームを終了します
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        /// <summary>
        /// メニューのフェードイン アニメーション
        /// </summary>
        private void AnimateMenuIn(GameObject menu)
        {
            var canvasGroup = menu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = menu.AddComponent<CanvasGroup>();
                
            canvasGroup.alpha = 0f;
            canvasGroup.alpha = 1f;
            
            menu.transform.localScale = Vector3.one * 0.9f;
            menu.transform.DOScale(1f, menuTransitionDuration).SetEase(Ease.OutBack);
        }
        
        /// <summary>
        /// メニューのフェードアウト アニメーション
        /// </summary>
        private void AnimateMenuOut(GameObject menu, System.Action onComplete = null)
        {
            var canvasGroup = menu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = menu.AddComponent<CanvasGroup>();
                
            canvasGroup.alpha = 0f;
            menu.transform.DOScale(0.9f, menuTransitionDuration).SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }
        
        /// <summary>
        /// 設定を保存します
        /// </summary>
        private void SaveSettings()
        {
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
                
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
                
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
                
            if (graphicsQualityDropdown != null)
                PlayerPrefs.SetInt("GraphicsQuality", graphicsQualityDropdown.value);
                
            PlayerPrefs.Save();
            onSettingsChanged?.Raise();
        }
        
        /// <summary>
        /// 設定を読み込みます
        /// </summary>
        private void LoadSettings()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                
            if (graphicsQualityDropdown != null)
                graphicsQualityDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        }
        
        /// <summary>
        /// マスター音量変更のハンドラ
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            AudioListener.volume = value;
            SaveSettings();
        }
        
        /// <summary>
        /// 音楽音量変更のハンドラ
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            // 音楽用のAudioSourceがあれば設定
            SaveSettings();
        }
        
        /// <summary>
        /// 効果音音量変更のハンドラ
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            // 効果音用のAudioSourceがあれば設定
            SaveSettings();
        }
        
        /// <summary>
        /// グラフィック品質変更のハンドラ
        /// </summary>
        private void OnGraphicsQualityChanged(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            SaveSettings();
        }
        
        #if UNITY_EDITOR
        [TabGroup("Menu Debug", "Test Functions")]
        [Button("Test Pause")]
        private void TestPause()
        {
            if (Application.isPlaying)
                PauseGame();
        }
        
        [TabGroup("Menu Debug", "Test Functions")]
        [Button("Test Resume")]
        private void TestResume()
        {
            if (Application.isPlaying)
                ResumeGame();
        }
        #endif
    }
}