using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.ActionRPG.UI
{
    /// <summary>
    /// ActionRPGテンプレート用メニューマネージャー
    /// ゲーム内メニュー、設定、一時停止画面管理
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("メニュー設定")]
        [SerializeField] private bool pauseGameWhenMenuOpen = true;
        [SerializeField] private bool hideCursorInGame = true;
        [SerializeField] private KeyCode menuToggleKey = KeyCode.Escape;
        
        [Header("メニューパネル")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsMenuPanel;
        [SerializeField] private GameObject confirmationDialogPanel;
        
        [Header("メニューボタン")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitGameButton;
        
        [Header("設定UI")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Dropdown graphicsQualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown resolutionDropdown;
        
        [Header("確認ダイアログ")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private UnityEngine.UI.Text dialogMessageText;
        
        [Header("イベント")]
        [SerializeField] private StringGameEvent onMenuStateChanged;
        [SerializeField] private StringGameEvent onSettingsChanged;
        
        // メニュー状態管理
        public MenuState CurrentMenuState { get; private set; } = MenuState.Game;
        public bool IsAnyMenuOpen => CurrentMenuState != MenuState.Game;
        
        // 内部状態
        private float previousTimeScale = 1f;
        private Dictionary<MenuState, GameObject> menuPanels;
        private System.Action currentConfirmationCallback;
        
        public enum MenuState
        {
            Game,           // ゲームプレイ中
            PauseMenu,      // 一時停止メニュー
            Settings,       // 設定メニュー
            MainMenu,       // メインメニュー
            Confirmation    // 確認ダイアログ
        }
        
        private void Start()
        {
            InitializeMenuSystem();
            SetupButtonListeners();
            SetupSettingsUI();
        }
        
        private void Update()
        {
            HandleMenuInput();
        }
        
        /// <summary>
        /// メニューシステム初期化
        /// </summary>
        private void InitializeMenuSystem()
        {
            // メニューパネル辞書の初期化
            menuPanels = new Dictionary<MenuState, GameObject>
            {
                { MenuState.PauseMenu, pauseMenuPanel },
                { MenuState.Settings, settingsMenuPanel },
                { MenuState.MainMenu, mainMenuPanel },
                { MenuState.Confirmation, confirmationDialogPanel }
            };
            
            // 全メニューを非表示に
            foreach (var panel in menuPanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            
            // ゲーム開始時の設定
            SetMenuState(MenuState.Game);
            
            Debug.Log("[MenuManager] メニューシステム初期化完了");
        }

        /// <summary>
        /// 外部からのメニューマネージャー初期化
        /// </summary>
        public void Initialize()
        {
            InitializeMenuSystem();
            SetupButtonListeners();
            SetupSettingsUI();
            Debug.Log("[MenuManager] External initialization completed");
        }

        /// <summary>
        /// ボタンリスナー設定
        /// </summary>
        private void SetupButtonListeners()
        {
            // メインボタン
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => SetMenuState(MenuState.Game));
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => SetMenuState(MenuState.Settings));
                
            if (saveGameButton != null)
                saveGameButton.onClick.AddListener(SaveGame);
                
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => ShowConfirmationDialog("メインメニューに戻りますか？", ReturnToMainMenu));
                
            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(() => ShowConfirmationDialog("ゲームを終了しますか？", QuitGame));
            
            // 確認ダイアログボタン
            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmAction);
                
            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelAction);
        }
        
        /// <summary>
        /// 設定UI初期化
        /// </summary>
        private void SetupSettingsUI()
        {
            // ボリューム設定
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            // グラフィック設定
            if (graphicsQualityDropdown != null)
            {
                graphicsQualityDropdown.value = QualitySettings.GetQualityLevel();
                graphicsQualityDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
            
            // 解像度設定
            SetupResolutionDropdown();
        }
        
        /// <summary>
        /// 解像度ドロップダウン設定
        /// </summary>
        private void SetupResolutionDropdown()
        {
            if (resolutionDropdown == null) return;
            
            var resolutions = Screen.resolutions;
            var options = new List<string>();
            int currentResolutionIndex = 0;
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                var resolution = resolutions[i];
                string option = $"{resolution.width}x{resolution.height}@{resolution.refreshRateRatio.value:0}Hz";
                options.Add(option);
                
                if (resolution.width == Screen.currentResolution.width && 
                    resolution.height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        
        /// <summary>
        /// メニュー入力処理
        /// </summary>
        private void HandleMenuInput()
        {
            if (Input.GetKeyDown(menuToggleKey))
            {
                TogglePauseMenu();
            }
        }
        
        /// <summary>
        /// 一時停止メニュー切り替え
        /// </summary>
        public void TogglePauseMenu()
        {
            if (CurrentMenuState == MenuState.Game)
            {
                SetMenuState(MenuState.PauseMenu);
            }
            else if (CurrentMenuState == MenuState.PauseMenu)
            {
                SetMenuState(MenuState.Game);
            }
        }
        
        /// <summary>
        /// メニュー状態設定
        /// </summary>
        public void SetMenuState(MenuState newState)
        {
            var previousState = CurrentMenuState;
            CurrentMenuState = newState;
            
            // パネル表示・非表示
            foreach (var kvp in menuPanels)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(kvp.Key == newState);
            }
            
            // 時間制御
            if (pauseGameWhenMenuOpen)
            {
                if (newState == MenuState.Game)
                {
                    Time.timeScale = previousTimeScale;
                }
                else if (previousState == MenuState.Game)
                {
                    previousTimeScale = Time.timeScale;
                    Time.timeScale = 0f;
                }
            }
            
            // カーソル制御
            if (hideCursorInGame)
            {
                if (newState == MenuState.Game)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            
            onMenuStateChanged?.Raise(newState.ToString());
            
            Debug.Log($"[MenuManager] メニュー状態変更: {previousState} → {newState}");
        }
        
        /// <summary>
        /// 確認ダイアログ表示
        /// </summary>
        public void ShowConfirmationDialog(string message, System.Action confirmCallback)
        {
            if (dialogMessageText != null)
                dialogMessageText.text = message;
                
            currentConfirmationCallback = confirmCallback;
            SetMenuState(MenuState.Confirmation);
        }
        
        /// <summary>
        /// 確認アクション
        /// </summary>
        private void ConfirmAction()
        {
            currentConfirmationCallback?.Invoke();
            currentConfirmationCallback = null;
            SetMenuState(MenuState.PauseMenu);
        }
        
        /// <summary>
        /// キャンセルアクション
        /// </summary>
        private void CancelAction()
        {
            currentConfirmationCallback = null;
            SetMenuState(MenuState.PauseMenu);
        }
        
        /// <summary>
        /// ゲーム保存
        /// </summary>
        private void SaveGame()
        {
            Debug.Log("[MenuManager] ゲーム保存実行");
            // 実際の保存処理を実装
        }
        
        /// <summary>
        /// ゲーム読み込み
        /// </summary>
        private void LoadGame()
        {
            Debug.Log("[MenuManager] ゲーム読み込み実行");
            // 実際の読み込み処理を実装
        }
        
        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        private void ReturnToMainMenu()
        {
            Debug.Log("[MenuManager] メインメニューに戻る");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        /// <summary>
        /// ゲーム終了
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[MenuManager] ゲーム終了");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // 設定変更イベントハンドラー
        private void OnMasterVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MasterVolume", value);
            AudioListener.volume = value;
            onSettingsChanged?.Raise($"MasterVolume: {value}");
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            onSettingsChanged?.Raise($"MusicVolume: {value}");
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            onSettingsChanged?.Raise($"SFXVolume: {value}");
        }
        
        private void OnGraphicsQualityChanged(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
            onSettingsChanged?.Raise($"GraphicsQuality: {qualityIndex}");
        }
        
        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            onSettingsChanged?.Raise($"Fullscreen: {isFullscreen}");
        }
        
        private void OnResolutionChanged(int resolutionIndex)
        {
            var resolutions = Screen.resolutions;
            if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
            {
                var resolution = resolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                PlayerPrefs.SetString("Resolution", $"{resolution.width}x{resolution.height}");
                onSettingsChanged?.Raise($"Resolution: {resolution.width}x{resolution.height}");
            }
        }
        
        /// <summary>
        /// 設定保存
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.Save();
            Debug.Log("[MenuManager] 設定保存完了");
        }
        
        /// <summary>
        /// デフォルト設定復元
        /// </summary>
        public void RestoreDefaultSettings()
        {
            // デフォルト値設定
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.8f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (graphicsQualityDropdown != null) graphicsQualityDropdown.value = 2; // Medium
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            
            Debug.Log("[MenuManager] デフォルト設定復元完了");
        }
        
        private void OnDestroy()
        {
            // 時間制御リセット
            Time.timeScale = 1f;
        }

        /// <summary>
        /// メインメニュー表示
        /// </summary>
        public void ShowMainMenu()
        {
            SetMenuState(MenuState.MainMenu);
            Debug.Log("[MenuManager] Main menu shown");
        }

        /// <summary>
        /// 一時停止メニュー表示
        /// </summary>
        public void ShowPauseMenu()
        {
            SetMenuState(MenuState.PauseMenu);
            Debug.Log("[MenuManager] Pause menu shown");
        }

        /// <summary>
        /// ゲームオーバーメニュー表示
        /// </summary>
        public void ShowGameOverMenu()
        {
            // ゲームオーバー用のUIがない場合はメインメニューを表示
            SetMenuState(MenuState.MainMenu);
            Debug.Log("[MenuManager] Game over menu shown (using main menu)");

            // TODO: 専用のゲームオーバーパネルを作成した場合はここで処理
        }
    }
}