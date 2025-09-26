using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// CheckpointService・售erviceLocator + Event鬧・虚繝上う繝悶Μ繝・ラ繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｫ繧医ｋ繧ｻ繝ｼ繝悶・繝ｪ繧ｹ繝昴・繝ｳ繧ｷ繧ｹ繝・Β
    /// Learn & Grow萓｡蛟､螳溽樟・壼ｮ牙・縺ｪ騾ｲ謐嶺ｿ晏ｭ倥↓繧医ｊ繝励Ξ繧､繝､繝ｼ縺ｮ蟄ｦ鄙剃ｽ馴ｨ薙ｒ蜷台ｸ・
    /// </summary>
    public class CheckpointService : ICheckpointService
    {
        // 繧ｻ繝ｼ繝悶ョ繝ｼ繧ｿ讒矩
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

        // 繝√ぉ繝・け繝昴う繝ｳ繝育ｮ｡逅・
        private Dictionary<int, Vector3> _checkpoints = new Dictionary<int, Vector3>();
        private Vector3 _lastCheckpointPosition = Vector3.zero;
        private int _currentCheckpointIndex = -1;
        private bool _hasActiveCheckpoint = false;

        // 繧ｻ繝ｼ繝悶せ繝ｭ繝・ヨ邂｡逅・
        private Dictionary<int, SaveData> _saveSlots = new Dictionary<int, SaveData>();
        private SaveData _currentSaveData = new SaveData();

        // 險ｭ螳壹→繧ｿ繧､繝槭・
        private PlatformerCheckpointSettings _settings;
        private float _autoSaveTimer = 0f;
        private bool _autoSaveEnabled = true;

        // ServiceLocator邨ｱ蜷医→Event鬧・虚騾壻ｿ｡
        private IPlatformerGameManager _gameManager;
        private ICollectionService _collectionService;

        // 繝励Ο繝代ユ繧｣蜈ｬ髢・
        public Vector3 LastCheckpointPosition => _lastCheckpointPosition;
        public bool HasActiveCheckpoint => _hasActiveCheckpoint;
        public bool HasCheckpoint => _hasActiveCheckpoint; // ICheckpointService貅匁侠

        // 繧ｨ繝ｩ繝ｼ隗｣豎ｺ逕ｨ・壹Γ繧ｽ繝・ラ蠖｢蠑上〒縺ｮ繧｢繧ｯ繧ｻ繧ｹ
        public Vector3 GetLastCheckpointPosition()
        {
            return _lastCheckpointPosition;
        }
        public int CurrentCheckpointIndex => _currentCheckpointIndex;
        public bool IsAutoSaveEnabled => _autoSaveEnabled;

        // IPlatformerService蝓ｺ蠎輔う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・
        public bool IsInitialized { get; private set; } = false;
        public bool IsEnabled { get; private set; } = true;

        // Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε・壻ｻ悶す繧ｹ繝・Β縺ｨ縺ｮ逍守ｵ仙粋騾壻ｿ｡
        public event Action<Vector3> OnCheckpointActivated;
        public event Action<int> OnProgressSaved;
        public event Action OnProgressLoaded;
        public event Action OnRespawnRequested;
        public event Action<Vector3> OnPlayerRespawned;

        /// <summary>
        /// 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・夊ｨｭ螳壹・繝ｼ繧ｹ蛻晄悄蛹・
        /// </summary>
        public CheckpointService(PlatformerCheckpointSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Initialize();
        }

        /// <summary>
        /// IPlatformerService.Initialize螳溯｣・ｼ夊ｨｭ螳壹・繝ｼ繧ｹ蛻晄悄蛹・
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;

            InitializeFromSettings();

            // ServiceLocator邨檎罰縺ｧ莉悶し繝ｼ繝薙せ縺ｸ縺ｮ蜿ら・蜿門ｾ・
            InitializeServiceReferences();

            IsInitialized = true;
            Debug.Log("[CheckpointService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 險ｭ螳壹°繧峨・蛻晄悄蛹厄ｼ售criptableObject繝吶・繧ｹ縺ｮ繝・・繧ｿ邂｡逅・
        /// </summary>
        private void InitializeFromSettings()
        {
            _autoSaveEnabled = _settings.EnableAutoSave;
            _autoSaveTimer = 0f;

            // 繧ｻ繝ｼ繝悶せ繝ｭ繝・ヨ蛻晄悄蛹・
            for (int i = 0; i < _settings.MaxSaveSlots; i++)
            {
                _saveSlots[i] = null;
            }

            Debug.Log($"Checkpoint system initialized - AutoSave: {_autoSaveEnabled}, MaxSlots: {_settings.MaxSaveSlots}");
        }

        /// <summary>
        /// ServiceLocator邨ｱ蜷茨ｼ壻ｻ悶し繝ｼ繝薙せ縺ｨ縺ｮ騾｣謳ｺ蛻晄悄蛹・
        /// </summary>
        private void InitializeServiceReferences()
        {
            // ServiceLocator邨檎罰縺ｧ莉悶し繝ｼ繝薙せ縺ｸ縺ｮ蜿ら・繧貞叙蠕・
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
        /// 騾ｲ謐嶺ｿ晏ｭ假ｼ壻ｸｭ螟ｮ繝・・繧ｿ邂｡逅・
        /// </summary>
        public void SaveProgress(int level, int score, int lives)
        {
            try
            {
                _currentSaveData.playerLevel = level;
                _currentSaveData.playerScore = score;
                _currentSaveData.playerLives = lives;
                _currentSaveData.saveTime = DateTime.Now;

                // ServiceLocator邨檎罰縺ｧGameManager縺九ｉ菴灘鴨蜿門ｾ・
                if (_gameManager != null)
                {
                    _currentSaveData.playerHealth = _gameManager.PlayerHealth;
                    _currentSaveData.playTime = _gameManager.GameTime;
                }

                // ServiceLocator邨檎罰縺ｧCollectionService縺九ｉ蜿朱寔繝・・繧ｿ蜿門ｾ・
                if (_collectionService != null)
                {
                    _currentSaveData.collectedItems = _collectionService.GetCollectedItemIds().ToList();
                }

                _currentSaveData.playerPosition = _lastCheckpointPosition;
                _currentSaveData.activatedCheckpoints = new List<int>(_checkpoints.Keys);

                // 髱槫酔譛滉ｿ晏ｭ伜・逅・
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
        /// 騾ｲ謐励Ο繝ｼ繝会ｼ壹ョ繝ｼ繧ｿ蠕ｩ蜈・→迥ｶ諷句酔譛・
        /// </summary>
        public void LoadProgress()
        {
            try
            {
                var saveData = LoadDataSync();
                if (saveData != null)
                {
                    _currentSaveData = saveData;

                    // ServiceLocator邨檎罰縺ｧGameManager縺ｫ迥ｶ諷句ｾｩ蜈・
                    if (_gameManager != null)
                    {
                        _gameManager.SetPlayerHealth(saveData.playerHealth);
                        // TODO: 繝ｬ繝吶Ν繝ｻ繧ｹ繧ｳ繧｢繝ｻ繝ｩ繧､繝輔・蠕ｩ蜈・ｼ・ameManager縺ｮ繝｡繧ｽ繝・ラ諡｡蠑ｵ縺悟ｿ・ｦ・ｼ・
                    }

                    // ServiceLocator邨檎罰縺ｧCollectionService縺ｫ蜿朱寔繝・・繧ｿ蠕ｩ蜈・
                    if (_collectionService != null)
                    {
                        _collectionService.RestoreCollectedItems(saveData.collectedItems);
                    }

                    // 繝√ぉ繝・け繝昴う繝ｳ繝育憾諷句ｾｩ蜈・
                    _lastCheckpointPosition = saveData.playerPosition;
                    if (saveData.activatedCheckpoints != null)
                    {
                        foreach (int checkpointId in saveData.activatedCheckpoints)
                        {
                            // 繝√ぉ繝・け繝昴う繝ｳ繝亥ｾｩ蜈・・逅・
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
        /// 繝√ぉ繝・け繝昴う繝ｳ繝医°繧峨Ο繝ｼ繝会ｼ夊ｿ・溷ｾｩ蟶ｰ
        /// </summary>
        public void LoadFromCheckpoint()
        {
            if (_hasActiveCheckpoint)
            {
                // ServiceLocator邨檎罰縺ｧGameManager縺ｧ繝励Ξ繧､繝､繝ｼ菴咲ｽｮ蠕ｩ蜈・
                if (_gameManager != null)
                {
                    // 繝励Ξ繧､繝､繝ｼ繧偵メ繧ｧ繝・け繝昴う繝ｳ繝井ｽ咲ｽｮ縺ｫ遘ｻ蜍・
                    // TODO: GameManager縺ｫ繝励Ξ繧､繝､繝ｼ菴咲ｽｮ險ｭ螳壹Γ繧ｽ繝・ラ縺悟ｿ・ｦ・
                }

                Debug.Log($"Loaded from checkpoint at position: {_lastCheckpointPosition}");
            }
            else
            {
                Debug.LogWarning("[CheckpointService] No active checkpoint to load from.");
            }
        }

        /// <summary>
        /// ICheckpointService.SetCheckpoint螳溯｣・ｼ壹メ繧ｧ繝・け繝昴う繝ｳ繝郁ｨｭ螳・
        /// </summary>
        public void SetCheckpoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            _hasActiveCheckpoint = true;

            // 閾ｪ蜍穂ｿ晏ｭ倥′譛牙柑縺ｪ蝣ｴ蜷医・蜊ｳ蠎ｧ菫晏ｭ・
            if (_autoSaveEnabled)
            {
                SaveCurrentState();
            }

            OnCheckpointActivated?.Invoke(position);
            Debug.Log($"Checkpoint set at position: {position}");
        }

        /// <summary>
        /// 迴ｾ蝨ｨ迥ｶ諷倶ｿ晏ｭ假ｼ壼叉蠎ｧ菫晏ｭ・
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
        /// 繝√ぉ繝・け繝昴う繝ｳ繝育匳骭ｲ・夐・鄂ｮ邂｡逅・
        /// </summary>
        public void RegisterCheckpoint(Vector3 position, int checkpointId)
        {
            _checkpoints[checkpointId] = position;
            Debug.Log($"Checkpoint registered - ID: {checkpointId}, Position: {position}");
        }

        /// <summary>
        /// 繝√ぉ繝・け繝昴う繝ｳ繝域怏蜉ｹ蛹厄ｼ夂憾諷区峩譁ｰ縺ｨ繧､繝吶Φ繝磯夂衍
        /// </summary>
        public void ActivateCheckpoint(int checkpointId)
        {
            if (_checkpoints.ContainsKey(checkpointId))
            {
                _currentCheckpointIndex = checkpointId;
                _lastCheckpointPosition = _checkpoints[checkpointId];
                _hasActiveCheckpoint = true;

                // 閾ｪ蜍穂ｿ晏ｭ倥′譛牙柑縺ｪ蝣ｴ蜷医・蜊ｳ蠎ｧ菫晏ｭ・
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
        /// 繝ｪ繧ｹ繝昴・繝ｳ繝昴う繝ｳ繝郁ｨｭ螳夲ｼ壽沐霆溘↑菴咲ｽｮ邂｡逅・
        /// </summary>
        public void SetRespawnPoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            _hasActiveCheckpoint = true;
            Debug.Log($"Respawn point set to: {position}");
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝昴・繝ｳ菴咲ｽｮ蜿門ｾ暦ｼ壼ｮ牙・縺ｪ菴咲ｽｮ謠蝉ｾ・
        /// </summary>
        public Vector3 GetRespawnPosition()
        {
            return _hasActiveCheckpoint ? _lastCheckpointPosition : Vector3.zero;
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繝ｪ繧ｹ繝昴・繝ｳ・壼叉蠎ｧ蠕ｩ蟶ｰ蜃ｦ逅・
        /// </summary>
        public void RespawnPlayer()
        {
            if (_hasActiveCheckpoint)
            {
                // ServiceLocator邨檎罰縺ｧGameManager縺ｧ蠕ｩ蟶ｰ蜃ｦ逅・
                if (_gameManager != null)
                {
                    // 菴灘鴨蝗槫ｾｩ險ｭ螳壹↓蠢懊§縺溷・逅・
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
        /// 繝ｪ繧ｹ繝昴・繝ｳ隕∵ｱゑｼ夐≦蟒ｶ蜃ｦ逅・ｯｾ蠢・
        /// </summary>
        public void RequestRespawn(float delay = 0f)
        {
            OnRespawnRequested?.Invoke();

            if (delay > 0f)
            {
                // TODO: 螳滄圀縺ｮ繧ｲ繝ｼ繝縺ｧ縺ｯCoroutine繧Уimer繧剃ｽｿ逕ｨ
                Debug.Log($"Respawn requested with {delay}s delay");
            }
            else
            {
                RespawnPlayer();
            }
        }

        /// <summary>
        /// 繧ｻ繝ｼ繝悶せ繝ｭ繝・ヨ菫晏ｭ假ｼ壹・繝ｫ繝√せ繝ｭ繝・ヨ蟇ｾ蠢・
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
        /// 繧ｻ繝ｼ繝悶せ繝ｭ繝・ヨ繝ｭ繝ｼ繝会ｼ夐∈謚樒噪蠕ｩ蜈・
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
        /// 繧ｹ繝ｭ繝・ヨ遨ｺ遒ｺ隱搾ｼ啅I謾ｯ謠ｴ
        /// </summary>
        public bool IsSlotEmpty(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _settings.MaxSaveSlots && _saveSlots[slotIndex] == null;
        }

        /// <summary>
        /// 繧ｹ繝ｭ繝・ヨ蜑企勁・壹ョ繝ｼ繧ｿ邂｡逅・
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
        /// 險ｭ螳壽峩譁ｰ・壹Λ繝ｳ繧ｿ繧､繝險ｭ螳壼､画峩蟇ｾ蠢・
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
        /// 閾ｪ蜍穂ｿ晏ｭ伜宛蠕｡・壽沐霆溘↑菫晏ｭ倡ｮ｡逅・
        /// </summary>
        public void SetAutoSave(bool enabled)
        {
            _autoSaveEnabled = enabled;
            Debug.Log($"Auto save {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 蜷梧悄繧ｻ繝ｼ繝門・逅・
        /// </summary>
        private void SaveDataSync(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);

                if (_settings.SaveDataConfiguration.EnableEncryption)
                {
                    // TODO: 證怜捷蛹門・逅・・螳溯｣・
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
        /// 髱槫酔譛溘そ繝ｼ繝門・逅・
        /// </summary>
        private async void SaveDataAsync(SaveData data)
        {
            try
            {
                // TODO: UniTask繧剃ｽｿ逕ｨ縺励◆逵溘・髱槫酔譛溷・逅・
                SaveDataSync(data);
                Debug.Log("Async save completed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CheckpointService] Async save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 蜷梧悄繝ｭ繝ｼ繝牙・逅・
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
        /// 繝√ぉ繝・け繝昴う繝ｳ繝域ュ蝣ｱ陦ｨ遉ｺ・壹ョ繝舌ャ繧ｰ謾ｯ謠ｴ
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
        /// 繝√ぉ繝・け繝昴う繝ｳ繝域､懆ｨｼ・壽紛蜷域ｧ遒ｺ隱・
        /// </summary>
        public void ValidateCheckpoints()
        {
            Debug.Log($"Validating {_checkpoints.Count} checkpoints...");
            foreach (var checkpoint in _checkpoints)
            {
                Debug.Log($"Checkpoint {checkpoint.Key}: {checkpoint.Value}");
            }
        }

        // 繝倥Ν繝代・繝｡繧ｽ繝・ラ
        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "platformer_save.json");
        }

        private string EncryptData(string data)
        {
            // TODO: AES證怜捷蛹悶・螳溯｣・
            return data; // 荳譎ら噪縺ｫ縺昴・縺ｾ縺ｾ霑斐☆
        }

        private string DecryptData(string encryptedData)
        {
            // TODO: AES蠕ｩ蜿ｷ蛹悶・螳溯｣・
            return encryptedData; // 荳譎ら噪縺ｫ縺昴・縺ｾ縺ｾ霑斐☆
        }

        private bool ValidateDataIntegrity(SaveData data)
        {
            // TODO: 繝・・繧ｿ謨ｴ蜷域ｧ繝√ぉ繝・け縺ｮ螳溯｣・
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
        /// IPlatformerService.Enable螳溯｣・ｼ壹し繝ｼ繝薙せ譛牙柑蛹・
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
            Debug.Log("[CheckpointService] Service enabled.");
        }

        /// <summary>
        /// IPlatformerService.Disable螳溯｣・ｼ壹し繝ｼ繝薙せ辟｡蜉ｹ蛹・
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
            Debug.Log("[CheckpointService] Service disabled.");
        }

        /// <summary>
        /// IPlatformerService.Reset螳溯｣・ｼ壹し繝ｼ繝薙せ迥ｶ諷九Μ繧ｻ繝・ヨ
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
        /// IPlatformerService.VerifyServiceLocatorIntegration螳溯｣・
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
        /// IPlatformerService.UpdateService螳溯｣・ｼ夊・蜍穂ｿ晏ｭ倥→繧ｿ繧､繝槭・邂｡逅・
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
        /// 譖ｴ譁ｰ蜃ｦ逅・ｼ夊・蜍穂ｿ晏ｭ倥→繧ｿ繧､繝槭・邂｡逅・ｼ医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤・・
        /// </summary>
        public void Update(float deltaTime)
        {
            UpdateService(deltaTime);
        }

        /// <summary>
        /// 繝ｪ繧ｽ繝ｼ繧ｹ隗｣謾ｾ・唔Disposable螳溯｣・
        /// </summary>
        public void Dispose()
        {
            // 繧､繝吶Φ繝郁ｧ｣髯､
            OnCheckpointActivated = null;
            OnProgressSaved = null;
            OnProgressLoaded = null;
            OnRespawnRequested = null;
            OnPlayerRespawned = null;

            // 繝・・繧ｿ繧ｯ繝ｪ繧｢
            _checkpoints.Clear();
            _saveSlots.Clear();

            Debug.Log("[CheckpointService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ繝・ヰ繝・げ諠・ｱ
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


