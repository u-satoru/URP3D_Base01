using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// システム関連イベント
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// Core層のGameEventシステム統合
    /// </summary>

    [CreateAssetMenu(fileName = "GameStateEvent", menuName = "FPS Template/Events/Game State Event")]
    public class GameStateEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<GameStateData> _gameStateChangedEvent;

        /// <summary>
        /// ゲーム状態変更イベント発行
        /// </summary>
        public void RaiseGameStateChanged(GameStateData data)
        {
            _gameStateChangedEvent?.Raise(data);
            Debug.Log($"[SystemEvent] Game state changed: {data.PreviousState} → {data.NewState}");
        }

        /// <summary>
        /// ゲーム状態変更リスナー登録
        /// </summary>
        public void RegisterListener(Action<GameStateData> callback)
        {
            _gameStateChangedEvent?.AddListener(callback);
        }

        /// <summary>
        /// ゲーム状態変更リスナー解除
        /// </summary>
        public void UnregisterListener(Action<GameStateData> callback)
        {
            _gameStateChangedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "LevelEvent", menuName = "FPS Template/Events/Level Event")]
    public class LevelEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<LevelEventData> _levelLoadStartedEvent;
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<LevelEventData> _levelLoadCompletedEvent;

        /// <summary>
        /// レベルロード開始イベント発行
        /// </summary>
        public void RaiseLevelLoadStarted(LevelEventData data)
        {
            _levelLoadStartedEvent?.Raise(data);
            Debug.Log($"[SystemEvent] Level load started: {data.LevelName}");
        }

        /// <summary>
        /// レベルロード完了イベント発行
        /// </summary>
        public void RaiseLevelLoadCompleted(LevelEventData data)
        {
            _levelLoadCompletedEvent?.Raise(data);
            Debug.Log($"[SystemEvent] Level load completed: {data.LevelName} in {data.LoadTime:F2}s");
        }

        /// <summary>
        /// レベルロード開始リスナー登録
        /// </summary>
        public void RegisterLoadStartedListener(Action<LevelEventData> callback)
        {
            _levelLoadStartedEvent?.AddListener(callback);
        }

        /// <summary>
        /// レベルロード完了リスナー登録
        /// </summary>
        public void RegisterLoadCompletedListener(Action<LevelEventData> callback)
        {
            _levelLoadCompletedEvent?.AddListener(callback);
        }

        /// <summary>
        /// レベルロード開始リスナー解除
        /// </summary>
        public void UnregisterLoadStartedListener(Action<LevelEventData> callback)
        {
            _levelLoadStartedEvent?.RemoveListener(callback);
        }

        /// <summary>
        /// レベルロード完了リスナー解除
        /// </summary>
        public void UnregisterLoadCompletedListener(Action<LevelEventData> callback)
        {
            _levelLoadCompletedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "PerformanceEvent", menuName = "FPS Template/Events/Performance Event")]
    public class PerformanceEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<PerformanceEventData> _performanceEvent;

        /// <summary>
        /// パフォーマンスイベント発行
        /// </summary>
        public void RaisePerformanceEvent(PerformanceEventData data)
        {
            _performanceEvent?.Raise(data);
            if (data.EventType == PerformanceEventType.Warning || data.EventType == PerformanceEventType.Critical)
            {
                Debug.LogWarning($"[SystemEvent] Performance {data.EventType}: {data.Message} (Value: {data.Value:F2})");
            }
        }

        /// <summary>
        /// パフォーマンスイベントリスナー登録
        /// </summary>
        public void RegisterListener(Action<PerformanceEventData> callback)
        {
            _performanceEvent?.AddListener(callback);
        }

        /// <summary>
        /// パフォーマンスイベントリスナー解除
        /// </summary>
        public void UnregisterListener(Action<PerformanceEventData> callback)
        {
            _performanceEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "SaveSystemEvent", menuName = "FPS Template/Events/Save System Event")]
    public class SaveSystemEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<SaveSystemEventData> _saveEvent;
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<SaveSystemEventData> _loadEvent;

        /// <summary>
        /// セーブイベント発行
        /// </summary>
        public void RaiseSaveEvent(SaveSystemEventData data)
        {
            _saveEvent?.Raise(data);
            Debug.Log($"[SystemEvent] Save {data.EventType}: {data.SlotName} - {(data.Success ? "Success" : "Failed")}");
        }

        /// <summary>
        /// ロードイベント発行
        /// </summary>
        public void RaiseLoadEvent(SaveSystemEventData data)
        {
            _loadEvent?.Raise(data);
            Debug.Log($"[SystemEvent] Load {data.EventType}: {data.SlotName} - {(data.Success ? "Success" : "Failed")}");
        }

        /// <summary>
        /// セーブイベントリスナー登録
        /// </summary>
        public void RegisterSaveListener(Action<SaveSystemEventData> callback)
        {
            _saveEvent?.AddListener(callback);
        }

        /// <summary>
        /// ロードイベントリスナー登録
        /// </summary>
        public void RegisterLoadListener(Action<SaveSystemEventData> callback)
        {
            _loadEvent?.AddListener(callback);
        }

        /// <summary>
        /// セーブイベントリスナー解除
        /// </summary>
        public void UnregisterSaveListener(Action<SaveSystemEventData> callback)
        {
            _saveEvent?.RemoveListener(callback);
        }

        /// <summary>
        /// ロードイベントリスナー解除
        /// </summary>
        public void UnregisterLoadListener(Action<SaveSystemEventData> callback)
        {
            _loadEvent?.RemoveListener(callback);
        }
    }

    /// <summary>
    /// ゲーム状態変更イベントデータ
    /// </summary>
    [System.Serializable]
    public class GameStateData
    {
        public GameState PreviousState;
        public GameState NewState;
        public float StateChangeDuration;
        public string Reason;
        public bool IsPlayerInitiated;
        public GameObject Context;

        public GameStateData(GameState previousState, GameState newState, float stateChangeDuration = 0f,
                            string reason = "", bool isPlayerInitiated = true, GameObject context = null)
        {
            PreviousState = previousState;
            NewState = newState;
            StateChangeDuration = stateChangeDuration;
            Reason = reason;
            IsPlayerInitiated = isPlayerInitiated;
            Context = context;
        }
    }

    /// <summary>
    /// レベルイベントデータ
    /// </summary>
    [System.Serializable]
    public class LevelEventData
    {
        public string LevelName;
        public string SceneName;
        public int LevelIndex;
        public float LoadTime;
        public float Progress;
        public bool IsAdditive;
        public LevelType LevelType;
        public string[] RequiredAssets;

        public LevelEventData(string levelName, string sceneName = "", int levelIndex = -1,
                             float loadTime = 0f, float progress = 0f, bool isAdditive = false,
                             LevelType levelType = LevelType.Gameplay, string[] requiredAssets = null)
        {
            LevelName = levelName;
            SceneName = sceneName;
            LevelIndex = levelIndex;
            LoadTime = loadTime;
            Progress = progress;
            IsAdditive = isAdditive;
            LevelType = levelType;
            RequiredAssets = requiredAssets ?? new string[0];
        }
    }

    /// <summary>
    /// パフォーマンスイベントデータ
    /// </summary>
    [System.Serializable]
    public class PerformanceEventData
    {
        public PerformanceEventType EventType;
        public string Message;
        public float Value;
        public float Threshold;
        public string MetricName;
        public float Timestamp;
        public GameObject Source;

        public PerformanceEventData(PerformanceEventType eventType, string message, float value,
                                   float threshold = 0f, string metricName = "", GameObject source = null)
        {
            EventType = eventType;
            Message = message;
            Value = value;
            Threshold = threshold;
            MetricName = metricName;
            Timestamp = Time.realtimeSinceStartup;
            Source = source;
        }
    }

    /// <summary>
    /// セーブシステムイベントデータ
    /// </summary>
    [System.Serializable]
    public class SaveSystemEventData
    {
        public SaveEventType EventType;
        public string SlotName;
        public int SlotIndex;
        public bool Success;
        public string ErrorMessage;
        public float Duration;
        public long FileSize;
        public string FilePath;

        public SaveSystemEventData(SaveEventType eventType, string slotName, int slotIndex = -1,
                                  bool success = true, string errorMessage = "", float duration = 0f,
                                  long fileSize = 0, string filePath = "")
        {
            EventType = eventType;
            SlotName = slotName;
            SlotIndex = slotIndex;
            Success = success;
            ErrorMessage = errorMessage;
            Duration = duration;
            FileSize = fileSize;
            FilePath = filePath;
        }
    }

    /// <summary>
    /// ゲーム状態
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Settings,
        Inventory,
        GameOver,
        Victory,
        Cutscene,
        Dialogue
    }

    /// <summary>
    /// レベルタイプ
    /// </summary>
    public enum LevelType
    {
        MainMenu,
        Gameplay,
        Cutscene,
        LoadingScreen,
        Tutorial,
        BossLevel,
        SecretLevel
    }

    /// <summary>
    /// パフォーマンスイベントタイプ
    /// </summary>
    public enum PerformanceEventType
    {
        Info,
        Warning,
        Critical,
        FPSDrop,
        MemoryWarning,
        LoadTimeExceeded,
        CPUSpike,
        GPUBottleneck
    }

    /// <summary>
    /// セーブイベントタイプ
    /// </summary>
    public enum SaveEventType
    {
        Started,
        Completed,
        Failed,
        AutoSave,
        QuickSave,
        Checkpoint
    }
}