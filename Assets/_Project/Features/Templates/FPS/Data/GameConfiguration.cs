using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// ゲーム設定データ
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ScriptableObjectベースのデータ管理システム
    /// ノンプログラマーによるゲームバランス調整対応
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "FPS Template/Data/Game Configuration", order = 3)]
    public class GameConfiguration : ScriptableObject
    {
        [Header("ゲーム基本設定")]
        [SerializeField] private string _gameTitle = "FPS Game";
        [SerializeField] private string _gameVersion = "1.0.0";
        [SerializeField] private float _gameTimeScale = 1f;
        [SerializeField] private bool _pauseWhenFocusLost = true;
        [SerializeField] private int _targetFrameRate = 60;

        [Header("難易度設定")]
        [SerializeField] private DifficultySettings _easyDifficulty;
        [SerializeField] private DifficultySettings _normalDifficulty;
        [SerializeField] private DifficultySettings _hardDifficulty;
        [SerializeField] private DifficultySettings _currentDifficulty;
        [SerializeField] private Events.GameState _defaultGameState = Events.GameState.MainMenu;

        [Header("プレイヤー基本設定")]
        [SerializeField] private PlayerData _defaultPlayerData;
        [SerializeField] private float _respawnTime = 3f;
        [SerializeField] private int _maxLives = 3;
        [SerializeField] private bool _friendlyFire = false;

        [Header("UI設定")]
        [SerializeField] private Events.UIConfiguration _uiConfiguration;
        [SerializeField] private bool _showFPS = false;
        [SerializeField] private bool _showMinimap = true;
        [SerializeField] private bool _showCrosshair = true;
        [SerializeField] private float _uiScale = 1f;

        [Header("オーディオ設定")]
        [SerializeField] private AudioConfiguration _audioConfiguration;
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _musicVolume = 0.8f;
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private bool _muteWhenFocusLost = true;

        [Header("グラフィック設定")]
        [SerializeField] private GraphicsConfiguration _graphicsConfiguration;
        [SerializeField] private int _qualityLevel = 2; // 0=Low, 1=Medium, 2=High, 3=Ultra
        [SerializeField] private bool _vsyncEnabled = true;
        [SerializeField] private int _antiAliasing = 4; // 0, 2, 4, 8

        [Header("入力設定")]
        [SerializeField] private InputConfiguration _inputConfiguration;
        [SerializeField] private float _defaultMouseSensitivity = 2f;
        [SerializeField] private bool _invertMouseY = false;

        [Header("ネットワーク設定（将来拡張用）")]
        [SerializeField] private NetworkConfiguration _networkConfiguration;
        [SerializeField] private bool _enableNetworking = false;
        [SerializeField] private int _maxPlayers = 8;
        [SerializeField] private float _networkTickRate = 60f;

        [Header("デバッグ・開発設定")]
        [SerializeField] private bool _debugMode = false;
        [SerializeField] private bool _showDebugConsole = false;
        [SerializeField] private bool _enableCheats = false;
        [SerializeField] private bool _logPerformanceMetrics = false;

        [Header("ゲームプレイバランス")]
        [SerializeField] private BalanceConfiguration _balanceConfiguration;
        [SerializeField] private float _globalDamageMultiplier = 1f;
        [SerializeField] private float _globalHealthMultiplier = 1f;
        [SerializeField] private float _globalSpeedMultiplier = 1f;

        [Header("セーブ・ロード設定")]
        [SerializeField] private SaveConfiguration _saveConfiguration;
        [SerializeField] private bool _autoSave = true;
        [SerializeField] private float _autoSaveInterval = 300f; // 5分
        [SerializeField] private int _maxSaveSlots = 10;

        // プロパティ（読み取り専用）
        public string GameTitle => _gameTitle;
        public string GameVersion => _gameVersion;
        public float GameTimeScale => _gameTimeScale;
        public bool PauseWhenFocusLost => _pauseWhenFocusLost;
        public int TargetFrameRate => _targetFrameRate;

        // 難易度設定
        public DifficultySettings EasyDifficulty => _easyDifficulty;
        public DifficultySettings NormalDifficulty => _normalDifficulty;
        public DifficultySettings HardDifficulty => _hardDifficulty;
        public DifficultySettings CurrentDifficulty => _currentDifficulty;
        public Events.GameState DefaultGameState => _defaultGameState;

        // プレイヤー設定
        public PlayerData DefaultPlayerData => _defaultPlayerData;
        public float RespawnTime => _respawnTime;
        public int MaxLives => _maxLives;
        public bool FriendlyFire => _friendlyFire;

        // UI設定
        public Events.UIConfiguration UIConfiguration => _uiConfiguration;
        public bool ShowFPS => _showFPS;
        public bool ShowMinimap => _showMinimap;
        public bool ShowCrosshair => _showCrosshair;
        public float UIScale => _uiScale;

        // オーディオ設定
        public AudioConfiguration AudioConfiguration => _audioConfiguration;
        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;
        public bool MuteWhenFocusLost => _muteWhenFocusLost;

        // グラフィック設定
        public GraphicsConfiguration GraphicsConfiguration => _graphicsConfiguration;
        public int QualityLevel => _qualityLevel;
        public bool VsyncEnabled => _vsyncEnabled;
        public int AntiAliasing => _antiAliasing;

        // 入力設定
        public InputConfiguration InputConfiguration => _inputConfiguration;
        public float DefaultMouseSensitivity => _defaultMouseSensitivity;
        public bool InvertMouseY => _invertMouseY;

        // ネットワーク設定
        public NetworkConfiguration NetworkConfiguration => _networkConfiguration;
        public bool EnableNetworking => _enableNetworking;
        public int MaxPlayers => _maxPlayers;
        public float NetworkTickRate => _networkTickRate;

        // デバッグ設定
        public bool DebugMode => _debugMode;
        public bool ShowDebugConsole => _showDebugConsole;
        public bool EnableCheats => _enableCheats;
        public bool LogPerformanceMetrics => _logPerformanceMetrics;

        // バランス設定
        public BalanceConfiguration BalanceConfiguration => _balanceConfiguration;
        public float GlobalDamageMultiplier => _globalDamageMultiplier;
        public float GlobalHealthMultiplier => _globalHealthMultiplier;
        public float GlobalSpeedMultiplier => _globalSpeedMultiplier;

        // セーブ設定
        public SaveConfiguration SaveConfiguration => _saveConfiguration;
        public bool AutoSave => _autoSave;
        public float AutoSaveInterval => _autoSaveInterval;
        public int MaxSaveSlots => _maxSaveSlots;

        /// <summary>
        /// 難易度変更
        /// </summary>
        public void SetDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    _currentDifficulty = _easyDifficulty;
                    break;
                case DifficultyLevel.Normal:
                    _currentDifficulty = _normalDifficulty;
                    break;
                case DifficultyLevel.Hard:
                    _currentDifficulty = _hardDifficulty;
                    break;
            }

            // 難易度変更イベント発行
            var difficultyData = new Events.DifficultyChangeData(
                previousDifficulty: GetCurrentDifficultyLevel(),
                newDifficulty: difficulty,
                settings: _currentDifficulty
            );

            var difficultyEvent = Resources.Load<Events.DifficultyChangeEvent>("Events/DifficultyChangeEvent");
            difficultyEvent?.RaiseDifficultyChanged(difficultyData);
        }

        /// <summary>
        /// 現在の難易度レベル取得
        /// </summary>
        public DifficultyLevel GetCurrentDifficultyLevel()
        {
            if (_currentDifficulty == _easyDifficulty) return DifficultyLevel.Easy;
            if (_currentDifficulty == _hardDifficulty) return DifficultyLevel.Hard;
            return DifficultyLevel.Normal;
        }

        /// <summary>
        /// 品質設定適用
        /// </summary>
        public void ApplyQualitySettings()
        {
            QualitySettings.SetQualityLevel(_qualityLevel);
            QualitySettings.vSyncCount = _vsyncEnabled ? 1 : 0;
            QualitySettings.antiAliasing = _antiAliasing;
            Application.targetFrameRate = _targetFrameRate;
        }

        /// <summary>
        /// オーディオ設定適用
        /// </summary>
        public void ApplyAudioSettings()
        {
            AudioListener.volume = _masterVolume;

            // AudioMixer経由での設定は実際のAudioMixerGroupが必要
            var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            audioService?.SetMasterVolume(_masterVolume);
            audioService?.SetMusicVolume(_musicVolume);
            audioService?.SetSFXVolume(_sfxVolume);
        }

        /// <summary>
        /// タイムスケール設定
        /// </summary>
        public void SetTimeScale(float timeScale)
        {
            _gameTimeScale = Mathf.Clamp(timeScale, 0f, 10f);
            Time.timeScale = _gameTimeScale;

            // タイムスケール変更イベント発行
            var timeScaleData = new Events.TimeScaleChangeData(
                previousTimeScale: Time.timeScale,
                newTimeScale: _gameTimeScale
            );

            var timeScaleEvent = Resources.Load<Events.TimeScaleChangeEvent>("Events/TimeScaleChangeEvent");
            timeScaleEvent?.RaiseTimeScaleChanged(timeScaleData);
        }

        /// <summary>
        /// ゲーム設定リセット
        /// </summary>
        public void ResetToDefaults()
        {
            _gameTimeScale = 1f;
            _targetFrameRate = 60;
            _masterVolume = 1f;
            _musicVolume = 0.8f;
            _sfxVolume = 1f;
            _qualityLevel = 2;
            _vsyncEnabled = true;
            _antiAliasing = 4;
            SetDifficulty(DifficultyLevel.Normal);
        }

        /// <summary>
        /// 設定データ検証
        /// </summary>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            if (_targetFrameRate < 30 || _targetFrameRate > 300)
            {
                Debug.LogWarning($"[GameConfiguration] Target frame rate {_targetFrameRate} is outside recommended range (30-300)");
            }

            if (_masterVolume < 0f || _masterVolume > 1f)
            {
                Debug.LogError($"[GameConfiguration] Master volume must be between 0 and 1");
                isValid = false;
            }

            if (_maxSaveSlots < 1 || _maxSaveSlots > 100)
            {
                Debug.LogWarning($"[GameConfiguration] Max save slots {_maxSaveSlots} may cause issues");
            }

            return isValid;
        }

        /// <summary>
        /// 設定をファイルに保存
        /// </summary>
        public void SaveSettings()
        {
            var settings = new GameSettingsData
            {
                masterVolume = _masterVolume,
                musicVolume = _musicVolume,
                sfxVolume = _sfxVolume,
                qualityLevel = _qualityLevel,
                vsyncEnabled = _vsyncEnabled,
                mouseSensitivity = _defaultMouseSensitivity,
                invertMouseY = _invertMouseY,
                difficulty = GetCurrentDifficultyLevel()
            };

            string json = JsonUtility.ToJson(settings, true);
            string path = Application.persistentDataPath + "/GameSettings.json";
            System.IO.File.WriteAllText(path, json);

            Debug.Log($"[GameConfiguration] Settings saved to {path}");
        }

        /// <summary>
        /// ファイルから設定を読み込み
        /// </summary>
        public void LoadSettings()
        {
            string path = Application.persistentDataPath + "/GameSettings.json";

            if (System.IO.File.Exists(path))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(path);
                    var settings = JsonUtility.FromJson<GameSettingsData>(json);

                    _masterVolume = settings.masterVolume;
                    _musicVolume = settings.musicVolume;
                    _sfxVolume = settings.sfxVolume;
                    _qualityLevel = settings.qualityLevel;
                    _vsyncEnabled = settings.vsyncEnabled;
                    _defaultMouseSensitivity = settings.mouseSensitivity;
                    _invertMouseY = settings.invertMouseY;
                    SetDifficulty(settings.difficulty);

                    Debug.Log($"[GameConfiguration] Settings loaded from {path}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GameConfiguration] Failed to load settings: {ex.Message}");
                }
            }
            else
            {
                Debug.Log("[GameConfiguration] No settings file found, using defaults");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // 値の範囲制限
            _gameTimeScale = Mathf.Clamp(_gameTimeScale, 0.1f, 10f);
            _targetFrameRate = Mathf.Clamp(_targetFrameRate, 30, 300);
            _masterVolume = Mathf.Clamp01(_masterVolume);
            _musicVolume = Mathf.Clamp01(_musicVolume);
            _sfxVolume = Mathf.Clamp01(_sfxVolume);
            _qualityLevel = Mathf.Clamp(_qualityLevel, 0, 5);
            _uiScale = Mathf.Clamp(_uiScale, 0.5f, 2f);

            ValidateConfiguration();
        }
#endif
    }

    /// <summary>
    /// 難易度設定
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        [Header("プレイヤー設定")]
        public float HealthMultiplier = 1f;
        public float DamageMultiplier = 1f;
        public float MovementSpeedMultiplier = 1f;
        public float AccuracyBonus = 0f;

        [Header("敵設定")]
        public float EnemyHealthMultiplier = 1f;
        public float EnemyDamageMultiplier = 1f;
        public float EnemySpeedMultiplier = 1f;
        public float EnemyAccuracyMultiplier = 1f;
        public float EnemyAwarenessMultiplier = 1f;

        [Header("ゲームプレイ設定")]
        public float ExperienceMultiplier = 1f;
        public float LootDropRateMultiplier = 1f;
        public bool ShowEnemyHealthBars = true;
        public bool ShowDamageNumbers = true;
        public int MaxCheckpoints = 5;
    }

    /// <summary>
    /// バランス設定
    /// </summary>
    [System.Serializable]
    public class BalanceConfiguration
    {
        [Header("武器バランス")]
        public float WeaponDamageScale = 1f;
        public float WeaponFireRateScale = 1f;
        public float WeaponReloadTimeScale = 1f;

        [Header("移動バランス")]
        public float MovementSpeedScale = 1f;
        public float JumpHeightScale = 1f;
        public float StaminaScale = 1f;

        [Header("経験値・進行")]
        public float ExperienceScale = 1f;
        public float LevelUpRequirementScale = 1f;
    }

    /// <summary>
    /// オーディオ設定
    /// </summary>
    [System.Serializable]
    public class AudioConfiguration
    {
        public bool Enable3DAudio = true;
        public float DopplerLevel = 1f;
        public float AudioDSPBufferSize = 1024;
        public int MaxVoices = 32;
    }

    /// <summary>
    /// グラフィック設定
    /// </summary>
    [System.Serializable]
    public class GraphicsConfiguration
    {
        public bool EnablePostProcessing = true;
        public bool EnableBloom = true;
        public bool EnableMotionBlur = false;
        public bool EnableSSAO = true;
        public float RenderScale = 1f;
        public bool EnableDynamicResolution = false;
    }

    /// <summary>
    /// 入力設定
    /// </summary>
    [System.Serializable]
    public class InputConfiguration
    {
        public bool EnableGamepad = true;
        public float GamepadDeadzone = 0.2f;
        public bool EnableTouchControls = false;
        public float TouchSensitivity = 1f;
    }

    /// <summary>
    /// ネットワーク設定
    /// </summary>
    [System.Serializable]
    public class NetworkConfiguration
    {
        public string ServerAddress = "localhost";
        public int ServerPort = 7777;
        public float TimeoutDuration = 30f;
        public int MaxReconnectAttempts = 3;
    }

    /// <summary>
    /// セーブ設定
    /// </summary>
    [System.Serializable]
    public class SaveConfiguration
    {
        public bool CompressSaveData = true;
        public bool EncryptSaveData = false;
        public bool CreateBackups = true;
        public int MaxBackups = 3;
    }

    /// <summary>
    /// 難易度レベル
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// 設定保存用データクラス
    /// </summary>
    [System.Serializable]
    public class GameSettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public int qualityLevel = 2;
        public bool vsyncEnabled = true;
        public float mouseSensitivity = 2f;
        public bool invertMouseY = false;
        public DifficultyLevel difficulty = DifficultyLevel.Normal;
    }
}