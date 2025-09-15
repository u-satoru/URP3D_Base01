using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// 状態保持システム
    /// 切り替え時のユーザー進捗保持機能
    /// DESIGN.md Layer 2: Runtime Template Management準拠
    /// </summary>
    [System.Serializable]
    public class StatePreservationSystem
    {
        [Header("Preservation Settings")]
        [SerializeField] private bool enableStatePersistence = true;
        [SerializeField] private bool preservePlayerProgress = true;
        [SerializeField] private bool preserveAchievements = true;
        [SerializeField] private bool preserveSettings = true;
        [SerializeField] private bool preserveLearningProgress = true;
        
        [Header("Storage Settings")]
        [SerializeField] private string saveDirectory = "TemplateStates";
        [SerializeField] private bool useEncryption = false;
        [SerializeField] private int maxStateHistory = 10;
        
        // Events
        public static event Action<GenreType> OnStatePreserved;
        public static event Action<GenreType> OnStateRestored;
        public static event Action<string> OnStateError;
        
        private Dictionary<GenreType, GenreState> _preservedStates = new Dictionary<GenreType, GenreState>();
        private string _saveFilePath;
        
        public StatePreservationSystem()
        {
            InitializeSaveDirectory();
        }
        
        /// <summary>
        /// 現在の状態を保存
        /// </summary>
        public bool PreserveCurrentState(GenreType currentGenre, object stateData = null)
        {
            if (!enableStatePersistence) return true;
            
            try
            {
                var genreState = new GenreState
                {
                    Genre = currentGenre,
                    Timestamp = DateTime.UtcNow,
                    PlayerProgress = preservePlayerProgress ? CapturePlayerProgress() : null,
                    Achievements = preserveAchievements ? CaptureAchievements() : null,
                    Settings = preserveSettings ? CaptureSettings() : null,
                    LearningProgress = preserveLearningProgress ? CaptureLearningProgress() : null,
                    CustomData = stateData
                };
                
                _preservedStates[currentGenre] = genreState;
                
                // ディスクに永続化
                if (!SaveStateToDisk(genreState))
                {
                    Debug.LogWarning($"Failed to save state to disk for genre {currentGenre}");
                }
                
                OnStatePreserved?.Invoke(currentGenre);
                Debug.Log($"State preserved for genre {currentGenre}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to preserve state for {currentGenre}: {ex.Message}");
                OnStateError?.Invoke($"State preservation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 保存された状態を復元
        /// </summary>
        public bool RestoreState(GenreType targetGenre)
        {
            if (!enableStatePersistence) return true;
            
            try
            {
                GenreState genreState = null;
                
                // メモリから取得を試行
                if (_preservedStates.TryGetValue(targetGenre, out genreState))
                {
                    Debug.Log($"Restoring state from memory for {targetGenre}");
                }
                // ディスクから読み込み
                else
                {
                    genreState = LoadStateFromDisk(targetGenre);
                    if (genreState != null)
                    {
                        _preservedStates[targetGenre] = genreState;
                        Debug.Log($"Restored state from disk for {targetGenre}");
                    }
                }
                
                if (genreState != null)
                {
                    ApplyGenreState(genreState);
                    OnStateRestored?.Invoke(targetGenre);
                    return true;
                }
                else
                {
                    Debug.Log($"No preserved state found for {targetGenre}, using defaults");
                    return true; // デフォルト状態を使用
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to restore state for {targetGenre}: {ex.Message}");
                OnStateError?.Invoke($"State restoration failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 保存されている状態をクリア
        /// </summary>
        public void ClearPreservedStates()
        {
            _preservedStates.Clear();
            
            try
            {
                if (Directory.Exists(GetSaveDirectoryPath()))
                {
                    Directory.Delete(GetSaveDirectoryPath(), true);
                }
                InitializeSaveDirectory();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear preserved states: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 特定ジャンルの状態をクリア
        /// </summary>
        public void ClearGenreState(GenreType genre)
        {
            _preservedStates.Remove(genre);
            
            try
            {
                var filePath = GetStateFilePath(genre);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear state for {genre}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 状態のバックアップを作成
        /// </summary>
        public bool CreateStateBackup(GenreType genre, string backupName = null)
        {
            if (!_preservedStates.TryGetValue(genre, out var state)) return false;
            
            try
            {
                var backupFileName = !string.IsNullOrEmpty(backupName) 
                    ? $"{genre}_{backupName}.backup" 
                    : $"{genre}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.backup";
                
                var backupPath = Path.Combine(GetSaveDirectoryPath(), backupFileName);
                var json = JsonUtility.ToJson(state, true);
                File.WriteAllText(backupPath, json);
                
                Debug.Log($"State backup created: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create backup for {genre}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 保存されている状態の統計情報
        /// </summary>
        public StatePreservationStats GetStats()
        {
            return new StatePreservationStats
            {
                PreservedGenreCount = _preservedStates.Count,
                TotalStateSize = CalculateTotalStateSize(),
                OldestStateDate = GetOldestStateDate(),
                NewestStateDate = GetNewestStateDate(),
                EnabledFeatures = GetEnabledFeatures()
            };
        }
        
        #region Private Methods
        
        private PlayerProgressData CapturePlayerProgress()
        {
            return new PlayerProgressData
            {
                Level = 1, // TODO: 実際のプレイヤーレベル取得
                Experience = 0,
                Position = Vector3.zero, // TODO: 実際のプレイヤー位置取得
                Health = 100f,
                Stats = new Dictionary<string, float>()
            };
        }
        
        private AchievementData CaptureAchievements()
        {
            return new AchievementData
            {
                UnlockedAchievements = new List<string>(),
                AchievementProgress = new Dictionary<string, float>()
            };
        }
        
        private SettingsData CaptureSettings()
        {
            return new SettingsData
            {
                GraphicsQuality = QualitySettings.GetQualityLevel(),
                MasterVolume = 1f, // TODO: 実際の音量取得
                Difficulty = 1,
                ControlScheme = "Default"
            };
        }
        
        private LearningProgressData CaptureLearningProgress()
        {
            return new LearningProgressData
            {
                CompletedSteps = new List<string>(),
                CurrentStep = 0,
                TotalLearningTime = 0f,
                LastAccessTime = DateTime.UtcNow
            };
        }
        
        private void ApplyGenreState(GenreState state)
        {
            if (state.PlayerProgress != null && preservePlayerProgress)
            {
                // TODO: プレイヤー進捗の復元
            }
            
            if (state.Achievements != null && preserveAchievements)
            {
                // TODO: 実績の復元
            }
            
            if (state.Settings != null && preserveSettings)
            {
                // TODO: 設定の復元
            }
            
            if (state.LearningProgress != null && preserveLearningProgress)
            {
                // TODO: 学習進捗の復元
            }
        }
        
        private void InitializeSaveDirectory()
        {
            var saveDir = GetSaveDirectoryPath();
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
        }
        
        private string GetSaveDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, saveDirectory);
        }
        
        private string GetStateFilePath(GenreType genre)
        {
            return Path.Combine(GetSaveDirectoryPath(), $"{genre}_state.json");
        }
        
        private bool SaveStateToDisk(GenreState state)
        {
            try
            {
                var filePath = GetStateFilePath(state.Genre);
                var json = JsonUtility.ToJson(state, true);
                
                if (useEncryption)
                {
                    // TODO: 暗号化実装
                }
                
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save state to disk: {ex.Message}");
                return false;
            }
        }
        
        private GenreState LoadStateFromDisk(GenreType genre)
        {
            try
            {
                var filePath = GetStateFilePath(genre);
                if (!File.Exists(filePath)) return null;
                
                var json = File.ReadAllText(filePath);
                
                if (useEncryption)
                {
                    // TODO: 復号化実装
                }
                
                return JsonUtility.FromJson<GenreState>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load state from disk: {ex.Message}");
                return null;
            }
        }
        
        private long CalculateTotalStateSize()
        {
            long totalSize = 0;
            foreach (var state in _preservedStates.Values)
            {
                var json = JsonUtility.ToJson(state);
                totalSize += System.Text.Encoding.UTF8.GetByteCount(json);
            }
            return totalSize;
        }
        
        private DateTime GetOldestStateDate()
        {
            DateTime oldest = DateTime.MaxValue;
            foreach (var state in _preservedStates.Values)
            {
                if (state.Timestamp < oldest) oldest = state.Timestamp;
            }
            return oldest == DateTime.MaxValue ? DateTime.UtcNow : oldest;
        }
        
        private DateTime GetNewestStateDate()
        {
            DateTime newest = DateTime.MinValue;
            foreach (var state in _preservedStates.Values)
            {
                if (state.Timestamp > newest) newest = state.Timestamp;
            }
            return newest == DateTime.MinValue ? DateTime.UtcNow : newest;
        }
        
        private string GetEnabledFeatures()
        {
            var features = new List<string>();
            if (preservePlayerProgress) features.Add("PlayerProgress");
            if (preserveAchievements) features.Add("Achievements");
            if (preserveSettings) features.Add("Settings");
            if (preserveLearningProgress) features.Add("LearningProgress");
            return string.Join(", ", features);
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class GenreState
    {
        public GenreType Genre;
        public DateTime Timestamp;
        public PlayerProgressData PlayerProgress;
        public AchievementData Achievements;
        public SettingsData Settings;
        public LearningProgressData LearningProgress;
        public object CustomData;
    }
    
    [System.Serializable]
    public class PlayerProgressData
    {
        public int Level;
        public float Experience;
        public Vector3 Position;
        public float Health;
        public Dictionary<string, float> Stats;
    }
    
    [System.Serializable]
    public class AchievementData
    {
        public List<string> UnlockedAchievements;
        public Dictionary<string, float> AchievementProgress;
    }
    
    [System.Serializable]
    public class SettingsData
    {
        public int GraphicsQuality;
        public float MasterVolume;
        public int Difficulty;
        public string ControlScheme;
    }
    
    [System.Serializable]
    public class LearningProgressData
    {
        public List<string> CompletedSteps;
        public int CurrentStep;
        public float TotalLearningTime;
        public DateTime LastAccessTime;
    }
    
    [System.Serializable]
    public class StatePreservationStats
    {
        public int PreservedGenreCount;
        public long TotalStateSize;
        public DateTime OldestStateDate;
        public DateTime NewestStateDate;
        public string EnabledFeatures;
        
        public override string ToString()
        {
            return $"Preservation Stats: {PreservedGenreCount} genres, " +
                   $"{TotalStateSize} bytes, " +
                   $"oldest: {OldestStateDate:yyyy-MM-dd}, " +
                   $"features: {EnabledFeatures}";
        }
    }
    
    #endregion
}