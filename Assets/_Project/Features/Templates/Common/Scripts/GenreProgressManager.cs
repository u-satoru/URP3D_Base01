using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// ジャンル間のユーザー進捗保持・管理システム
    /// TASK-004.1: State Preservation System実装
    /// 切り替え時のユーザー進捗保持、学習進捗の永続化
    /// </summary>
    public class GenreProgressManager : MonoBehaviour
    {
        [Header("進捗保存設定")]
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveInterval = 30f;
        [SerializeField] private bool _enableCloudSync = false;
        
        [Header("データ移行設定")]
        [SerializeField] private bool _enableCrosGenreDataMigration = true;
        [SerializeField] private bool _preserveCommonSettings = true;
        [SerializeField] private bool _preserveLearningProgress = true;
        
        [Header("デバッグ設定")]
        [SerializeField] private bool _enableDebugLogging = false;
        [SerializeField] private bool _enableProgressValidation = true;
        
        // Events（Event駆動アーキテクチャ準拠）
        [Header("イベントチャネル")]
        [SerializeField] private GenreTypeGameEvent _onProgressSaved;
        [SerializeField] private GenreTypeGameEvent _onProgressLoaded;
        [SerializeField] private GameEvent _onProgressMigrated;
        [SerializeField] private GameEvent _onProgressCleared;
        
        // Progress管理
        private readonly Dictionary<GenreType, GenreProgressData> _genreProgressMap = new Dictionary<GenreType, GenreProgressData>();
        private GenreProgressData _currentProgress;
        private string _saveFilePath;
        
        // Singleton管理
        private static GenreProgressManager _instance;
        public static GenreProgressManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GenreProgressManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GenreProgressManager");
                        _instance = go.AddComponent<GenreProgressManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        // Properties
        public GenreProgressData CurrentProgress => _currentProgress;
        public int TotalGenresPlayed => _genreProgressMap.Count;
        public float TotalPlayTime => _genreProgressMap.Values.Sum(p => p.PlayTime);
        public bool HasAnyProgress => _genreProgressMap.Count > 0;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton設定
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // セーブファイルパス設定
            _saveFilePath = Application.persistentDataPath + "/GenreProgress.json";
            
            LogDebug("GenreProgressManager initialized");
        }
        
        private void Start()
        {
            // 進捗データ読み込み
            LoadAllProgress();
            
            // 自動保存開始
            if (_autoSaveEnabled)
            {
                InvokeRepeating(nameof(AutoSave), _autoSaveInterval, _autoSaveInterval);
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveAllProgress();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveAllProgress();
            }
        }
        
        private void OnDestroy()
        {
            SaveAllProgress();
        }
        
        #endregion
        
        #region Progress Management
        
        /// <summary>
        /// 指定ジャンルの進捗を初期化/取得
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <returns>進捗データ</returns>
        public GenreProgressData GetOrCreateProgress(GenreType genreType)
        {
            if (!_genreProgressMap.ContainsKey(genreType))
            {
                var newProgress = new GenreProgressData(genreType);
                _genreProgressMap[genreType] = newProgress;
                
                LogDebug($"Created new progress for genre: {genreType}");
            }
            
            return _genreProgressMap[genreType];
        }
        
        /// <summary>
        /// 現在のジャンル進捗を設定
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        public void SetCurrentGenre(GenreType genreType)
        {
            _currentProgress = GetOrCreateProgress(genreType);
            _currentProgress.LastAccessTime = DateTime.Now;
            
            LogDebug($"Set current genre progress: {genreType}");
        }
        
        /// <summary>
        /// 進捗を更新
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="playTime">追加プレイ時間（秒）</param>
        /// <param name="newAchievements">新しい実績</param>
        public void UpdateProgress(GenreType genreType, float playTime = 0f, params string[] newAchievements)
        {
            var progress = GetOrCreateProgress(genreType);
            
            progress.PlayTime += playTime;
            progress.SessionCount++;
            progress.LastAccessTime = DateTime.Now;
            
            foreach (var achievement in newAchievements)
            {
                if (!progress.UnlockedAchievements.Contains(achievement))
                {
                    progress.UnlockedAchievements.Add(achievement);
                    LogDebug($"Unlocked achievement: {achievement} for {genreType}");
                }
            }
            
            LogDebug($"Updated progress for {genreType}: PlayTime={progress.PlayTime}s, Sessions={progress.SessionCount}");
        }
        
        /// <summary>
        /// 学習進捗を更新
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="objectiveId">学習目標ID</param>
        /// <param name="completionPercentage">完了率（0-1）</param>
        public void UpdateLearningProgress(GenreType genreType, string objectiveId, float completionPercentage)
        {
            var progress = GetOrCreateProgress(genreType);
            
            progress.LearningObjectives[objectiveId] = Mathf.Clamp01(completionPercentage);
            progress.LastAccessTime = DateTime.Now;
            
            // 完了時の処理
            if (completionPercentage >= 1.0f && !progress.CompletedObjectives.Contains(objectiveId))
            {
                progress.CompletedObjectives.Add(objectiveId);
                LogDebug($"Completed learning objective: {objectiveId} for {genreType}");
            }
            
            // 全体完了率計算
            var totalCompletion = progress.LearningObjectives.Values.Sum() / Mathf.Max(1, progress.LearningObjectives.Count);
            progress.OverallLearningCompletion = totalCompletion;
            
            LogDebug($"Updated learning progress for {genreType}: {objectiveId}={completionPercentage:P}, Overall={totalCompletion:P}");
        }
        
        /// <summary>
        /// プレイヤー設定を保存
        /// </summary>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="settingKey">設定キー</param>
        /// <param name="settingValue">設定値</param>
        public void SavePlayerSetting(GenreType genreType, string settingKey, object settingValue)
        {
            var progress = GetOrCreateProgress(genreType);
            
            progress.PlayerSettings[settingKey] = settingValue.ToString();
            progress.LastAccessTime = DateTime.Now;
            
            LogDebug($"Saved player setting for {genreType}: {settingKey}={settingValue}");
        }
        
        /// <summary>
        /// プレイヤー設定を取得
        /// </summary>
        /// <typeparam name="T">設定値の型</typeparam>
        /// <param name="genreType">ジャンルタイプ</param>
        /// <param name="settingKey">設定キー</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>設定値</returns>
        public T GetPlayerSetting<T>(GenreType genreType, string settingKey, T defaultValue)
        {
            var progress = GetOrCreateProgress(genreType);
            
            if (!progress.PlayerSettings.ContainsKey(settingKey))
            {
                return defaultValue;
            }
            
            var stringValue = progress.PlayerSettings[settingKey];
            
            try
            {
                return (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch (Exception ex)
            {
                LogError($"Failed to convert setting {settingKey}: {ex.Message}");
                return defaultValue;
            }
        }
        
        #endregion
        
        #region Data Migration
        
        /// <summary>
        /// ジャンル間でデータ移行
        /// </summary>
        /// <param name="fromGenre">移行元ジャンル</param>
        /// <param name="toGenre">移行先ジャンル</param>
        /// <param name="migrateSettings">設定を移行するか</param>
        /// <param name="migrateLearning">学習進捗を移行するか</param>
        public void MigrateProgress(GenreType fromGenre, GenreType toGenre, bool migrateSettings = true, bool migrateLearning = true)
        {
            if (!_enableCrosGenreDataMigration)
            {
                LogDebug("Cross-genre data migration is disabled");
                return;
            }
            
            var fromProgress = GetOrCreateProgress(fromGenre);
            var toProgress = GetOrCreateProgress(toGenre);
            
            LogDebug($"Starting data migration: {fromGenre} -> {toGenre}");
            
            // 共通設定の移行
            if (migrateSettings && _preserveCommonSettings)
            {
                MigrateCommonSettings(fromProgress, toProgress);
            }
            
            // 学習進捗の移行
            if (migrateLearning && _preserveLearningProgress)
            {
                MigrateLearningProgress(fromProgress, toProgress);
            }
            
            toProgress.LastAccessTime = DateTime.Now;
            
            LogDebug($"Data migration completed: {fromGenre} -> {toGenre}");
            
            // イベント発行
            _onProgressMigrated?.Raise();
        }
        
        /// <summary>
        /// 共通設定を移行
        /// </summary>
        private void MigrateCommonSettings(GenreProgressData from, GenreProgressData to)
        {
            // 汎用的な設定項目を移行
            var commonSettings = new[]
            {
                "Graphics_Quality",
                "Audio_MasterVolume",
                "Audio_SFXVolume",
                "Audio_MusicVolume",
                "Input_MouseSensitivity",
                "Gameplay_Subtitles",
                "Accessibility_ColorBlind",
                "UI_Language"
            };
            
            foreach (var setting in commonSettings)
            {
                if (from.PlayerSettings.ContainsKey(setting))
                {
                    to.PlayerSettings[setting] = from.PlayerSettings[setting];
                }
            }
            
            LogDebug($"Migrated {commonSettings.Length} common settings");
        }
        
        /// <summary>
        /// 学習進捗を移行
        /// </summary>
        private void MigrateLearningProgress(GenreProgressData from, GenreProgressData to)
        {
            // 基本的なスキル項目を移行
            var transferableSkills = new[]
            {
                "BasicMovement",
                "CameraControl",
                "UI_Navigation",
                "InputSystem_Familiarity"
            };
            
            foreach (var skill in transferableSkills)
            {
                if (from.LearningObjectives.ContainsKey(skill))
                {
                    // 部分的な進捗移行（75%割り引き）
                    var originalProgress = from.LearningObjectives[skill];
                    var transferredProgress = originalProgress * 0.75f;
                    
                    if (!to.LearningObjectives.ContainsKey(skill) || to.LearningObjectives[skill] < transferredProgress)
                    {
                        to.LearningObjectives[skill] = transferredProgress;
                    }
                }
            }
            
            LogDebug($"Migrated learning progress for {transferableSkills.Length} skills");
        }
        
        #endregion
        
        #region Save/Load
        
        /// <summary>
        /// 全進捗を保存
        /// </summary>
        public void SaveAllProgress()
        {
            try
            {
                var saveData = new ProgressSaveData
                {
                    SaveVersion = "1.0",
                    SaveTime = DateTime.Now,
                    GenreProgressList = _genreProgressMap.Values.ToList()
                };
                
                var json = JsonUtility.ToJson(saveData, true);
                System.IO.File.WriteAllText(_saveFilePath, json);
                
                LogDebug($"Saved progress for {_genreProgressMap.Count} genres");
                
                // クラウド同期（必要な場合）
                if (_enableCloudSync)
                {
                    SyncToCloud();
                }
                
                // イベント発行
                foreach (var genreType in _genreProgressMap.Keys)
                {
                    _onProgressSaved?.Raise(genreType);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to save progress: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 全進捗を読み込み
        /// </summary>
        public void LoadAllProgress()
        {
            try
            {
                if (!System.IO.File.Exists(_saveFilePath))
                {
                    LogDebug("No save file found. Starting with empty progress.");
                    return;
                }
                
                var json = System.IO.File.ReadAllText(_saveFilePath);
                var saveData = JsonUtility.FromJson<ProgressSaveData>(json);
                
                _genreProgressMap.Clear();
                
                foreach (var progressData in saveData.GenreProgressList)
                {
                    _genreProgressMap[progressData.GenreType] = progressData;
                }
                
                LogDebug($"Loaded progress for {_genreProgressMap.Count} genres (Save Version: {saveData.SaveVersion})");
                
                // データ検証
                if (_enableProgressValidation)
                {
                    ValidateLoadedData();
                }
                
                // イベント発行
                foreach (var genreType in _genreProgressMap.Keys)
                {
                    _onProgressLoaded?.Raise(genreType);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load progress: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 自動保存
        /// </summary>
        private void AutoSave()
        {
            if (_genreProgressMap.Count > 0)
            {
                SaveAllProgress();
                LogDebug("Auto-save completed");
            }
        }
        
        /// <summary>
        /// クラウド同期（プレースホルダ）
        /// </summary>
        private void SyncToCloud()
        {
            // TODO: Cloud sync implementation
            // Steam Cloud, Google Play Games, etc.
            LogDebug("Cloud sync triggered");
        }
        
        #endregion
        
        #region Validation & Statistics
        
        /// <summary>
        /// 読み込んだデータの検証
        /// </summary>
        private void ValidateLoadedData()
        {
            var invalidCount = 0;
            var keysToRemove = new List<GenreType>();
            
            foreach (var kvp in _genreProgressMap)
            {
                if (kvp.Value == null || !kvp.Value.IsValid())
                {
                    keysToRemove.Add(kvp.Key);
                    invalidCount++;
                }
            }
            
            // 無効なデータを削除
            foreach (var key in keysToRemove)
            {
                _genreProgressMap.Remove(key);
            }
            
            if (invalidCount > 0)
            {
                LogWarning($"Removed {invalidCount} invalid progress entries");
            }
            
            LogDebug($"Progress data validation completed: {_genreProgressMap.Count} valid entries");
        }
        
        /// <summary>
        /// 進捗統計を取得
        /// </summary>
        /// <returns>統計情報</returns>
        public ProgressStatistics GetStatistics()
        {
            var stats = new ProgressStatistics
            {
                TotalGenres = _genreProgressMap.Count,
                TotalPlayTime = TotalPlayTime,
                TotalSessions = _genreProgressMap.Values.Sum(p => p.SessionCount),
                TotalAchievements = _genreProgressMap.Values.Sum(p => p.UnlockedAchievements.Count),
                AverageLearningCompletion = _genreProgressMap.Values.Average(p => p.OverallLearningCompletion),
                MostPlayedGenre = GetMostPlayedGenre(),
                LastPlayedGenre = GetLastPlayedGenre(),
                FirstPlayTime = GetFirstPlayTime()
            };
            
            return stats;
        }
        
        private GenreType GetMostPlayedGenre()
        {
            if (_genreProgressMap.Count == 0) return GenreType.Stealth;
            
            return _genreProgressMap.OrderByDescending(p => p.Value.PlayTime).First().Key;
        }
        
        private GenreType GetLastPlayedGenre()
        {
            if (_genreProgressMap.Count == 0) return GenreType.Stealth;
            
            return _genreProgressMap.OrderByDescending(p => p.Value.LastAccessTime).First().Key;
        }
        
        private DateTime GetFirstPlayTime()
        {
            if (_genreProgressMap.Count == 0) return DateTime.Now;
            
            return _genreProgressMap.Values.Min(p => p.FirstAccessTime);
        }
        
        #endregion
        
        #region Debug & Logging
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[GenreProgressManager] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GenreProgressManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GenreProgressManager] {message}");
        }
        
        /// <summary>
        /// 全進捗情報をコンソールに出力（デバッグ用）
        /// </summary>
        [ContextMenu("Print All Progress")]
        public void PrintAllProgress()
        {
            Debug.Log("=== All Genre Progress ===");
            
            foreach (var kvp in _genreProgressMap.OrderBy(p => p.Key))
            {
                var progress = kvp.Value;
                Debug.Log($"{kvp.Key}: PlayTime={progress.PlayTime:F1}s, Learning={progress.OverallLearningCompletion:P}, " +
                         $"Achievements={progress.UnlockedAchievements.Count}, LastPlayed={progress.LastAccessTime:yyyy-MM-dd}");
            }
            
            var stats = GetStatistics();
            Debug.Log($"Statistics: Total={stats.TotalPlayTime:F1}s, Most Played={stats.MostPlayedGenre}, " +
                     $"Avg Learning={stats.AverageLearningCompletion:P}");
        }
        
        /// <summary>
        /// 進捗をクリア（デバッグ用）
        /// </summary>
        [ContextMenu("Clear All Progress")]
        public void ClearAllProgress()
        {
            _genreProgressMap.Clear();
            _currentProgress = null;
            
            if (System.IO.File.Exists(_saveFilePath))
            {
                System.IO.File.Delete(_saveFilePath);
            }
            
            LogDebug("All progress cleared");
            
            // イベント発行
            _onProgressCleared?.Raise();
        }
        
        #endregion
    }
    
    /// <summary>
    /// ジャンル固有の進捗データ
    /// </summary>
    [System.Serializable]
    public class GenreProgressData
    {
        public GenreType GenreType;
        public float PlayTime;
        public int SessionCount;
        public DateTime FirstAccessTime;
        public DateTime LastAccessTime;
        
        [SerializeField] private List<string> _unlockedAchievements = new List<string>();
        [SerializeField] private List<string> _completedObjectives = new List<string>();
        [SerializeField] private SerializableDictionary<string, float> _learningObjectives = new SerializableDictionary<string, float>();
        [SerializeField] private SerializableDictionary<string, string> _playerSettings = new SerializableDictionary<string, string>();
        
        public float OverallLearningCompletion;
        
        // Properties
        public List<string> UnlockedAchievements => _unlockedAchievements;
        public List<string> CompletedObjectives => _completedObjectives;
        public SerializableDictionary<string, float> LearningObjectives => _learningObjectives;
        public SerializableDictionary<string, string> PlayerSettings => _playerSettings;
        
        public GenreProgressData(GenreType genreType)
        {
            GenreType = genreType;
            PlayTime = 0f;
            SessionCount = 0;
            FirstAccessTime = DateTime.Now;
            LastAccessTime = DateTime.Now;
            OverallLearningCompletion = 0f;
        }
        
        /// <summary>
        /// データが有効かチェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            return PlayTime >= 0 && SessionCount >= 0 && FirstAccessTime <= LastAccessTime;
        }
    }
    
    /// <summary>
    /// 保存用データ構造
    /// </summary>
    [System.Serializable]
    public class ProgressSaveData
    {
        public string SaveVersion;
        public DateTime SaveTime;
        public List<GenreProgressData> GenreProgressList;
    }
    
    /// <summary>
    /// 進捗統計情報
    /// </summary>
    [System.Serializable]
    public struct ProgressStatistics
    {
        public int TotalGenres;
        public float TotalPlayTime;
        public int TotalSessions;
        public int TotalAchievements;
        public float AverageLearningCompletion;
        public GenreType MostPlayedGenre;
        public GenreType LastPlayedGenre;
        public DateTime FirstPlayTime;
    }
    
    /// <summary>
    /// シリアライズ可能なDictionary（Unity対応）
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();
        
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            
            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }
        
        public void OnAfterDeserialize()
        {
            Clear();
            
            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
            {
                this[_keys[i]] = _values[i];
            }
        }
    }
}
