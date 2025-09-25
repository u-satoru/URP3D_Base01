using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// CheckpointService：ServiceLocator + Event駆動ハイブリッドアーキテクチャによるセーブ・リスポーンシステム
    /// Learn & Grow価値実現：安全な進捗保存によりプレイヤーの学習体験を向上
    /// </summary>
    public class CheckpointService : ICheckpointService
    {
        // セーブデータ構造
        [System.Serializable]
        public class SaveData
        {
            public Vector3 playerPosition;
            public int playerLevel;
            public int playerScore;
            public int playerLives;
            public int playerHealth;
            public List<int> collectedItems = new List<int>();
            public List<int> activatedCheckpoints = new List<int>();
            public DateTime saveTime;
            public string levelName;
            public float playTime;
        }

        // チェックポイント管理
        private Dictionary<int, Vector3> _checkpoints = new Dictionary<int, Vector3>();
        private Vector3 _lastCheckpointPosition = Vector3.zero;
        private int _currentCheckpointIndex = -1;
        private bool _hasActiveCheckpoint = false;

        // セーブスロット管理
        private Dictionary<int, SaveData> _saveSlots = new Dictionary<int, SaveData>();
        private SaveData _currentSaveData = new SaveData();

        // 設定とタイマー
        private PlatformerCheckpointSettings _settings;
        private float _autoSaveTimer = 0f;
        private bool _autoSaveEnabled = true;

        // ServiceLocator統合とEvent駆動通信
        private IPlatformerGameManager _gameManager;
        private ICollectionService _collectionService;

        // プロパティ公開
        public Vector3 LastCheckpointPosition => _lastCheckpointPosition;
        public bool HasActiveCheckpoint => _hasActiveCheckpoint;
        public bool HasCheckpoint => _hasActiveCheckpoint; // ICheckpointService準拠

        // エラー解決用：メソッド形式でのアクセス
        public Vector3 GetLastCheckpointPosition()
        {
            return _lastCheckpointPosition;
        }
        public int CurrentCheckpointIndex => _currentCheckpointIndex;
        public bool IsAutoSaveEnabled => _autoSaveEnabled;

        // IPlatformerService基底インターフェース実装
        public bool IsInitialized { get; private set; } = false;
        public bool IsEnabled { get; private set; } = true;

        // Event駆動アーキテクチャ：他システムとの疎結合通信
        public event Action<Vector3> OnCheckpointActivated;
        public event Action<int> OnProgressSaved;
        public event Action OnProgressLoaded;
        public event Action OnRespawnRequested;
        public event Action<Vector3> OnPlayerRespawned;

        /// <summary>
        /// コンストラクタ：設定ベース初期化
        /// </summary>
        public CheckpointService(PlatformerCheckpointSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Initialize();
        }

        /// <summary>
        /// IPlatformerService.Initialize実装：設定ベース初期化
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            InitializeFromSettings();

            // ServiceLocator経由で他サービスへの参照取得
            InitializeServiceReferences();

            IsInitialized = true;
            Debug.Log("[CheckpointService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 設定からの初期化：ScriptableObjectベースのデータ管理
        /// </summary>
        private void InitializeFromSettings()
        {
            _autoSaveEnabled = _settings.EnableAutoSave;
            _autoSaveTimer = 0f;

            // セーブスロット初期化
            for (int i = 0; i < _settings.MaxSaveSlots; i++)
            {
                _saveSlots[i] = null;
            }

            Debug.Log($"Checkpoint system initialized - AutoSave: {_autoSaveEnabled}, MaxSlots: {_settings.MaxSaveSlots}");
        }

        /// <summary>
        /// ServiceLocator統合：他サービスとの連携初期化
        /// </summary>
        private void InitializeServiceReferences()
        {
            // ServiceLocator経由で他サービスへの参照を取得
            _gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
            _collectionService = ServiceLocator.GetService<ICollectionService>();

            if (_gameManager == null)
            {
                Debug.LogWarning("[CheckpointService] GameManager not found in ServiceLocator. Some features may not work.");
            }

            if (_collectionService == null)
            {
                Debug.LogWarning("[CheckpointService] CollectionService not found in ServiceLocator. Collection data will not be saved.");
            }
        }

        /// <summary>
        /// 進捗保存：中央データ管理
        /// </summary>
        public void SaveProgress(int level, int score, int lives)
        {
            try
            {
                _currentSaveData.playerLevel = level;
                _currentSaveData.playerScore = score;
                _currentSaveData.playerLives = lives;
                _currentSaveData.saveTime = DateTime.Now;

                // ServiceLocator経由でGameManagerから体力取得
                if (_gameManager != null)
                {
                    _currentSaveData.playerHealth = _gameManager.PlayerHealth;
                    _currentSaveData.playTime = _gameManager.GameTime;
                }

                // ServiceLocator経由でCollectionServiceから収集データ取得
                if (_collectionService != null)
                {
                    _currentSaveData.collectedItems = _collectionService.GetCollectedItemIds().ToList();
                }

                _currentSaveData.playerPosition = _lastCheckpointPosition;
                _currentSaveData.activatedCheckpoints = new List<int>(_checkpoints.Keys);

                // 非同期保存処理
                if (_settings.Performance.EnableAsyncSaving)
                {
                    SaveDataAsync(_currentSaveData);
                }
                else
                {
                    SaveDataSync(_currentSaveData);
                }

                OnProgressSaved?.Invoke(level);
                Debug.Log($"Progress saved - Level: {level}, Score: {score}, Lives: {lives}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Failed to save progress: {ex.Message}");
            }
        }

        /// <summary>
        /// 進捗ロード：データ復元と状態同期
        /// </summary>
        public void LoadProgress()
        {
            try
            {
                var saveData = LoadDataSync();
                if (saveData != null)
                {
                    _currentSaveData = saveData;

                    // ServiceLocator経由でGameManagerに状態復元
                    if (_gameManager != null)
                    {
                        _gameManager.SetPlayerHealth(saveData.playerHealth);
                        // TODO: レベル・スコア・ライフの復元（GameManagerのメソッド拡張が必要）
                    }

                    // ServiceLocator経由でCollectionServiceに収集データ復元
                    if (_collectionService != null)
                    {
                        _collectionService.RestoreCollectedItems(saveData.collectedItems);
                    }

                    // チェックポイント状態復元
                    _lastCheckpointPosition = saveData.playerPosition;
                    if (saveData.activatedCheckpoints != null)
                    {
                        foreach (int checkpointId in saveData.activatedCheckpoints)
                        {
                            // チェックポイント復元処理
                            if (_checkpoints.ContainsKey(checkpointId))
                            {
                                _currentCheckpointIndex = checkpointId;
                                _hasActiveCheckpoint = true;
                            }
                        }
                    }

                    OnProgressLoaded?.Invoke();
                    Debug.Log($"Progress loaded - Level: {saveData.playerLevel}, Score: {saveData.playerScore}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Failed to load progress: {ex.Message}");
            }
        }

        /// <summary>
        /// チェックポイントからロード：迅速復帰
        /// </summary>
        public void LoadFromCheckpoint()
        {
            if (_hasActiveCheckpoint)
            {
                // ServiceLocator経由でGameManagerでプレイヤー位置復元
                if (_gameManager != null)
                {
                    // プレイヤーをチェックポイント位置に移動
                    // TODO: GameManagerにプレイヤー位置設定メソッドが必要
                }

                Debug.Log($"Loaded from checkpoint at position: {_lastCheckpointPosition}");
            }
            else
            {
                Debug.LogWarning("[CheckpointService] No active checkpoint to load from.");
            }
        }

        /// <summary>
        /// ICheckpointService.SetCheckpoint実装：チェックポイント設定
        /// </summary>
        public void SetCheckpoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            _hasActiveCheckpoint = true;

            // 自動保存が有効な場合は即座保存
            if (_autoSaveEnabled)
            {
                SaveCurrentState();
            }

            OnCheckpointActivated?.Invoke(position);
            Debug.Log($"Checkpoint set at position: {position}");
        }

        /// <summary>
        /// 現在状態保存：即座保存
        /// </summary>
        public void SaveCurrentState()
        {
            if (_gameManager != null)
            {
                SaveProgress(
                    _gameManager.CurrentLevel,
                    _gameManager.PlayerScore,
                    _gameManager.PlayerLives
                );
            }
        }

        /// <summary>
        /// チェックポイント登録：配置管理
        /// </summary>
        public void RegisterCheckpoint(Vector3 position, int checkpointId)
        {
            _checkpoints[checkpointId] = position;
            Debug.Log($"Checkpoint registered - ID: {checkpointId}, Position: {position}");
        }

        /// <summary>
        /// チェックポイント有効化：状態更新とイベント通知
        /// </summary>
        public void ActivateCheckpoint(int checkpointId)
        {
            if (_checkpoints.ContainsKey(checkpointId))
            {
                _currentCheckpointIndex = checkpointId;
                _lastCheckpointPosition = _checkpoints[checkpointId];
                _hasActiveCheckpoint = true;

                // 自動保存が有効な場合は即座保存
                if (_autoSaveEnabled)
                {
                    SaveCurrentState();
                }

                OnCheckpointActivated?.Invoke(_lastCheckpointPosition);
                Debug.Log($"Checkpoint activated - ID: {checkpointId}, Position: {_lastCheckpointPosition}");
            }
            else
            {
                Debug.LogWarning($"[CheckpointService] Checkpoint ID {checkpointId} not found.");
            }
        }

        /// <summary>
        /// リスポーンポイント設定：柔軟な位置管理
        /// </summary>
        public void SetRespawnPoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            _hasActiveCheckpoint = true;
            Debug.Log($"Respawn point set to: {position}");
        }

        /// <summary>
        /// リスポーン位置取得：安全な位置提供
        /// </summary>
        public Vector3 GetRespawnPosition()
        {
            return _hasActiveCheckpoint ? _lastCheckpointPosition : Vector3.zero;
        }

        /// <summary>
        /// プレイヤーリスポーン：即座復帰処理
        /// </summary>
        public void RespawnPlayer()
        {
            if (_hasActiveCheckpoint)
            {
                // ServiceLocator経由でGameManagerで復帰処理
                if (_gameManager != null)
                {
                    // 体力回復設定に応じた処理
                    if (_settings.RespawnConfiguration.RestoreHealthOnRespawn)
                    {
                        int restoreAmount = Mathf.RoundToInt(
                            _gameManager.PlayerHealth * _settings.RespawnConfiguration.HealthRestoreAmount
                        );
                        _gameManager.HealPlayer(restoreAmount);
                    }
                }

                OnPlayerRespawned?.Invoke(_lastCheckpointPosition);
                Debug.Log($"Player respawned at: {_lastCheckpointPosition}");
            }
            else
            {
                Debug.LogWarning("[CheckpointService] No active checkpoint for respawn.");
            }
        }

        /// <summary>
        /// リスポーン要求：遅延処理対応
        /// </summary>
        public void RequestRespawn(float delay = 0f)
        {
            OnRespawnRequested?.Invoke();

            if (delay > 0f)
            {
                // TODO: 実際のゲームではCoroutineやTimerを使用
                Debug.Log($"Respawn requested with {delay}s delay");
            }
            else
            {
                RespawnPlayer();
            }
        }

        /// <summary>
        /// セーブスロット保存：マルチスロット対応
        /// </summary>
        public void SaveToSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _settings.MaxSaveSlots)
            {
                _saveSlots[slotIndex] = _currentSaveData;
                Debug.Log($"Game saved to slot {slotIndex}");
            }
            else
            {
                Debug.LogError($"[CheckpointService] Invalid save slot index: {slotIndex}");
            }
        }

        /// <summary>
        /// セーブスロットロード：選択的復元
        /// </summary>
        public void LoadFromSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _settings.MaxSaveSlots && _saveSlots[slotIndex] != null)
            {
                _currentSaveData = _saveSlots[slotIndex];
                LoadProgress();
                Debug.Log($"Game loaded from slot {slotIndex}");
            }
            else
            {
                Debug.LogError($"[CheckpointService] Cannot load from slot {slotIndex}: empty or invalid");
            }
        }

        /// <summary>
        /// スロット空確認：UI支援
        /// </summary>
        public bool IsSlotEmpty(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _settings.MaxSaveSlots && _saveSlots[slotIndex] == null;
        }

        /// <summary>
        /// スロット削除：データ管理
        /// </summary>
        public void DeleteSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _settings.MaxSaveSlots)
            {
                _saveSlots[slotIndex] = null;
                Debug.Log($"Save slot {slotIndex} deleted");
            }
        }

        /// <summary>
        /// 設定更新：ランタイム設定変更対応
        /// </summary>
        public void UpdateSettings(object settings)
        {
            if (settings is PlatformerCheckpointSettings checkpointSettings)
            {
                _settings = checkpointSettings;
                _autoSaveEnabled = _settings.EnableAutoSave;
                Debug.Log("Checkpoint settings updated at runtime.");
            }
        }

        /// <summary>
        /// 自動保存制御：柔軟な保存管理
        /// </summary>
        public void SetAutoSave(bool enabled)
        {
            _autoSaveEnabled = enabled;
            Debug.Log($"Auto save {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 同期セーブ処理
        /// </summary>
        private void SaveDataSync(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);

                if (_settings.SaveDataConfiguration.EnableEncryption)
                {
                    // TODO: 暗号化処理の実装
                    json = EncryptData(json);
                }

                string filePath = GetSaveFilePath();
                File.WriteAllText(filePath, json);

                if (_settings.SaveDataConfiguration.CreateBackups)
                {
                    CreateBackup(filePath);
                }

                Debug.Log($"Save data written to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 非同期セーブ処理
        /// </summary>
        private async void SaveDataAsync(SaveData data)
        {
            try
            {
                // TODO: UniTaskを使用した真の非同期処理
                SaveDataSync(data);
                Debug.Log("Async save completed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Async save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 同期ロード処理
        /// </summary>
        private SaveData LoadDataSync()
        {
            try
            {
                string filePath = GetSaveFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    if (_settings.SaveDataConfiguration.EnableEncryption)
                    {
                        json = DecryptData(json);
                    }

                    SaveData data = JsonUtility.FromJson<SaveData>(json);

                    if (_settings.SaveDataConfiguration.EnableIntegrityCheck)
                    {
                        if (!ValidateDataIntegrity(data))
                        {
                            Debug.LogWarning("[CheckpointService] Save data integrity check failed. Loading backup...");
                            return LoadBackup();
                        }
                    }

                    Debug.Log($"Save data loaded from: {filePath}");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Load failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// チェックポイント情報表示：デバッグ支援
        /// </summary>
        public void ShowCheckpointInfo()
        {
            Debug.Log("=== Checkpoint Service Info ===");
            Debug.Log($"Active Checkpoint: {_hasActiveCheckpoint}");
            Debug.Log($"Current Position: {_lastCheckpointPosition}");
            Debug.Log($"Checkpoint Index: {_currentCheckpointIndex}");
            Debug.Log($"Registered Checkpoints: {_checkpoints.Count}");
            Debug.Log($"Auto Save Enabled: {_autoSaveEnabled}");
        }

        /// <summary>
        /// チェックポイント検証：整合性確認
        /// </summary>
        public void ValidateCheckpoints()
        {
            Debug.Log($"Validating {_checkpoints.Count} checkpoints...");
            foreach (var checkpoint in _checkpoints)
            {
                Debug.Log($"Checkpoint {checkpoint.Key}: {checkpoint.Value}");
            }
        }

        // ヘルパーメソッド
        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "platformer_save.json");
        }

        private string EncryptData(string data)
        {
            // TODO: AES暗号化の実装
            return data; // 一時的にそのまま返す
        }

        private string DecryptData(string encryptedData)
        {
            // TODO: AES復号化の実装
            return encryptedData; // 一時的にそのまま返す
        }

        private bool ValidateDataIntegrity(SaveData data)
        {
            // TODO: データ整合性チェックの実装
            return data != null && !string.IsNullOrEmpty(data.levelName);
        }

        private void CreateBackup(string originalPath)
        {
            try
            {
                string backupPath = originalPath + ".backup";
                File.Copy(originalPath, backupPath, true);
                Debug.Log($"Backup created: {backupPath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CheckpointService] Backup creation failed: {ex.Message}");
            }
        }

        private SaveData LoadBackup()
        {
            try
            {
                string backupPath = GetSaveFilePath() + ".backup";
                if (File.Exists(backupPath))
                {
                    string json = File.ReadAllText(backupPath);
                    return JsonUtility.FromJson<SaveData>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Backup load failed: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// IPlatformerService.Enable実装：サービス有効化
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
            Debug.Log("[CheckpointService] Service enabled.");
        }

        /// <summary>
        /// IPlatformerService.Disable実装：サービス無効化
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            Debug.Log("[CheckpointService] Service disabled.");
        }

        /// <summary>
        /// IPlatformerService.Reset実装：サービス状態リセット
        /// </summary>
        public void Reset()
        {
            _checkpoints.Clear();
            _saveSlots.Clear();
            _hasActiveCheckpoint = false;
            _lastCheckpointPosition = Vector3.zero;
            _currentCheckpointIndex = -1;
            _autoSaveTimer = 0f;
            _currentSaveData = new SaveData();

            Debug.Log("[CheckpointService] Service reset completed.");
        }

        /// <summary>
        /// IPlatformerService.VerifyServiceLocatorIntegration実装
        /// </summary>
        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                var collectionService = ServiceLocator.GetService<ICollectionService>();

                bool integration = gameManager != null && collectionService != null;
                Debug.Log($"[CheckpointService] ServiceLocator integration verified: {integration}");
                return integration;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] ServiceLocator integration failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// IPlatformerService.UpdateService実装：自動保存とタイマー管理
        /// </summary>
        public void UpdateService(float deltaTime)
        {
            if (!IsEnabled || !IsInitialized) return;

            if (_autoSaveEnabled && _settings.AutoSaveInterval > 0)
            {
                _autoSaveTimer += deltaTime;
                if (_autoSaveTimer >= _settings.AutoSaveInterval)
                {
                    SaveCurrentState();
                    _autoSaveTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 更新処理：自動保存とタイマー管理（レガシー互換）
        /// </summary>
        public void Update(float deltaTime)
        {
            UpdateService(deltaTime);
        }

        /// <summary>
        /// リソース解放：IDisposable実装
        /// </summary>
        public void Dispose()
        {
            // イベント解除
            OnCheckpointActivated = null;
            OnProgressSaved = null;
            OnProgressLoaded = null;
            OnRespawnRequested = null;
            OnPlayerRespawned = null;

            // データクリア
            _checkpoints.Clear();
            _saveSlots.Clear();

            Debug.Log("[CheckpointService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用デバッグ情報
        /// </summary>
        public void ShowDebugInfo()
        {
            Debug.Log("=== CheckpointService Debug Info ===");
            Debug.Log($"Active Checkpoint: {_hasActiveCheckpoint}");
            Debug.Log($"Last Position: {_lastCheckpointPosition}");
            Debug.Log($"Current Index: {_currentCheckpointIndex}");
            Debug.Log($"Auto Save: {_autoSaveEnabled}");
            Debug.Log($"Timer: {_autoSaveTimer:F1}s");
            Debug.Log($"Checkpoints: {_checkpoints.Count}");
            Debug.Log($"Save Slots: {_saveSlots.Count}");
        }
#endif
    }
}
