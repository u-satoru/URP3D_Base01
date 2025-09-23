using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
// using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// 中央イベントロギングシスチE���E�EerviceLocator移行版�E�E    /// ServiceLocatorパターンでイベントログ管琁E��のアクセスを提供すめE    /// 
    /// 設計思想:
    /// - 中央雁E��皁E��ログ管琁E��よる一貫性確俁E    /// - 褁E��出力形式対応！Eonsole, File, DebugWindow, RemoteDebugger�E�E    /// - Unity MonoBehaviourのライフサイクルに統合された安�Eなサービス管琁E    /// - ServiceLocatorパターンによる依存性注入対忁E    /// - 後方互換性を維持しながら段階的移行を支援
    /// 
    /// 推奨使用侁E
    /// var logger = ServiceLocator.GetService&lt;IEventLogger&gt;();
    /// logger.Log("Application started");
    /// logger.LogEvent("PlayerDamaged", 3, "damage:25");
    /// </summary>
    public class EventLogger : MonoBehaviour, IEventLogger, IInitializable
    {
        // ✁EServiceLocator移衁E Legacy Singleton警告シスチE���E�後方互換性のため�E�E        
        private List<EventLogEntry> eventLog = new List<EventLogEntry>();
        private EventLoggerSettings settings;
        
        [Header("Runtime Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int maxLogEntries = 1000;
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        /// <summary>初期化状態フラグ</summary>
        private bool _isInitialized = false;
        
        #region IInitializable Implementation
        
        /// <summary>
        /// 初期化優先度�E�数値が小さぁE��ど早く�E期化される！E        /// EventLoggerは他�Eサービスより早く�E期化される忁E��がある
        /// </summary>
        public int Priority => 5;
        
        /// <summary>
        /// サービスが�E期化済みかどぁE��を示すフラグ
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// サービスの初期化�E琁E        /// ServiceLocatorによって呼び出されめE        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            
            InitializeService();
        }
        
        
        
        
        

        
        #endregion
        
        #region Properties (IEventLogger Implementation)
        
        /// <summary>
        /// ログが有効かどぁE���E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public bool IsEnabled => enableLogging;
        
        /// <summary>
        /// 現在のイベントログエントリのリスト！EEventLoggerインターフェース実裁E��E        /// </summary>
        public List<EventLogEntry> EventLog => eventLog;
        
        #endregion
        
        #region Static Properties (Backward Compatibility)
        
        /// <summary>
        /// 後方互換性のための静的プロパティ
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().IsEnabled instead")]
        public static bool IsEnabledStatic => GetServiceInstance()?.IsEnabled ?? false;
        
        /// <summary>
        /// 後方互換性のための静的プロパティ
        /// </summary>
        public static List<EventLogEntry> EventLogStatic => GetServiceInstance()?.EventLog ?? new List<EventLogEntry>();
        
        #endregion
        
        #region MonoBehaviour Lifecycle
        
        void Awake()
        {
            // ServiceLocatorへの登録
            if (autoRegisterOnAwake)
            {
                RegisterToServiceLocator();
                LogServiceStatus();
            }
            
            // Editor環墁E��はDontDestroyOnLoadは使用不可のため条件チェチE��
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        void OnDestroy()
        {
            // ServiceLocatorからの登録解除
            try
            {
                ServiceLocator.UnregisterService<IEventLogger>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("EventLogger unregistered from ServiceLocator");
#endif
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to unregister EventLogger: {ex.Message}");
            }
            
            _isInitialized = false;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// ServiceLocatorにEventLoggerサービスを登録
        /// </summary>
        private void RegisterToServiceLocator()
        {
            try
            {
                ServiceLocator.RegisterService<IEventLogger>(this);
                InitializeService();
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("EventLogger registered to ServiceLocator successfully");
#endif
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to register EventLogger to ServiceLocator: {ex.Message}");
            }
        }
        
        /// <summary>
        /// サービスの冁E��初期化�E琁E        /// EventLoggerの設定読み込みと基本状態設定を行う
        /// </summary>
        private void InitializeService()
        {
            LoadSettings();
            _isInitialized = true;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("EventLogger initialized with ServiceLocator integration");
#endif
        }
        
        private void LoadSettings()
        {
            settings = Resources.Load<EventLoggerSettings>("EventLoggerSettings");
            if (settings != null)
            {
                enableLogging = settings.enableLogging;
                maxLogEntries = settings.maxLogEntries;
            }
        }
        
        /// <summary>
        /// ServiceLocator経由でEventLoggerインスタンスを取征E        /// </summary>
        private static IEventLogger GetServiceInstance()
        {
            return ServiceLocator.GetService<IEventLogger>();
        }
        
        private void AddLogEntry(EventLogEntry entry)
        {
            if (!enableLogging) return;
            
            eventLog.Add(entry);
            
            // 最大エントリ数を趁E��た場合、古ぁE��ントリを削除
            if (eventLog.Count > maxLogEntries)
            {
                eventLog.RemoveRange(0, eventLog.Count - maxLogEntries);
            }
        }
        
        /// <summary>
        /// サービスの現在の状態をログ出劁E        /// </summary>
        public void LogServiceStatus()
        {
            if (!IsEnabled) return;
            
            var statusMessage = $"EventLogger Service Status - Enabled: {IsEnabled}, Entries: {EventLog.Count}, ServiceLocator: {FeatureFlags.UseServiceLocator}";
            UnityEngine.Debug.Log($"<color=green>[EventLogger Service]</color> {statusMessage}");
        }
        
        #endregion
        
        #region IEventLogger Implementation
        
        /// <summary>
        /// 簡潔なログメソチE�� - Unity標準Debug.Logの代替�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void Log(string message)
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry("General", 0, message, LogLevel.Info));
            
            if (settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.Log($"<color=cyan>[EventLogger]</color> {message}");
            }
        }
        
        /// <summary>
        /// 簡潔な警告ログメソチE�� - Unity標準Debug.LogWarningの代替�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogWarning(string message)
        {
            LogWarning("General", 0, message);
        }
        
        /// <summary>
        /// 警告レベルのイベントログを記録�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogWarning(string eventName, int listenerCount, string message)
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry(eventName, listenerCount, message, LogLevel.Warning));
            
            if (settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.LogWarning($"[EventLogger] {eventName} (Listeners: {listenerCount}) - {message}");
            }
        }
        
        /// <summary>
        /// 簡潔なエラーログメソチE�� - Unity標準Debug.LogErrorの代替�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogError(string message)
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry("Error", 0, message, LogLevel.Error));
            
            if (settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.LogError($"[EventLogger] {message}");
            }
        }
        
        /// <summary>
        /// エラーレベルのイベントログを記録�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogError(string eventName, int listenerCount, string message)
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry(eventName, listenerCount, message, LogLevel.Error));
            
            if (settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.LogError($"[EventLogger] {eventName} (Listeners: {listenerCount}) - {message}");
            }
        }
        
        /// <summary>
        /// イベントログを記録�E�リスナ�E数とペイロード付き�E�！EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogEvent(string eventName, int listenerCount, string payload = "")
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry(eventName, listenerCount, payload, LogLevel.Info));
        }
        
        /// <summary>
        /// 型安�Eなペイロード付きイベントログを記録�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void LogEventWithPayload<T>(string eventName, int listenerCount, T payload)
        {
            if (!IsEnabled) return;
            
            string payloadString = payload?.ToString() ?? "";
            LogEvent(eventName, listenerCount, payloadString);
        }
        
        /// <summary>
        /// ログをクリア�E�EEventLoggerインターフェース実裁E��E        /// </summary>
        public void ClearLog()
        {
            eventLog.Clear();
        }
        
        /// <summary>
        /// フィルタリングされたログエントリを取得！EEventLoggerインターフェース実裁E��E        /// </summary>
        public List<EventLogEntry> GetFilteredLog(string nameFilter = "", LogLevel minLevel = LogLevel.Info)
        {
            var filtered = new List<EventLogEntry>();
            
            foreach (var entry in eventLog)
            {
                if (entry.level < minLevel) continue;
                if (!string.IsNullOrEmpty(nameFilter) && !entry.eventName.ToLower().Contains(nameFilter.ToLower())) continue;
                
                filtered.Add(entry);
            }
            
            return filtered;
        }
        
        /// <summary>
        /// ログの統計情報を取得！EEventLoggerインターフェース実裁E��E        /// </summary>
        public LogStatistics GetStatistics()
        {
            var stats = new LogStatistics();
            var eventCounts = new Dictionary<string, int>();
            
            foreach (var entry in eventLog)
            {
                stats.totalEvents++;
                
                switch (entry.level)
                {
                    case LogLevel.Info: stats.infoCount++; break;
                    case LogLevel.Warning: stats.warningCount++; break;
                    case LogLevel.Error: stats.errorCount++; break;
                }
                
                if (eventCounts.ContainsKey(entry.eventName))
                {
                    eventCounts[entry.eventName]++;
                }
                else
                {
                    eventCounts[entry.eventName] = 1;
                }
            }
            
            stats.eventCounts = eventCounts;
            return stats;
        }
        
        /// <summary>
        /// ログをCSVファイルにエクスポ�Eト！EEventLoggerインターフェース実裁E��E        /// </summary>
        public void ExportToCSV(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Timestamp,EventName,ListenerCount,Payload,Level");
                    
                    foreach (var entry in eventLog)
                    {
                        var line = $"{entry.timestamp:yyyy-MM-dd HH:mm:ss},{entry.eventName},{entry.listenerCount},\"{entry.payload}\",{entry.level}";
                        writer.WriteLine(line);
                    }
                }
                
                UnityEngine.Debug.Log($"EventLog exported to: {filePath}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to export EventLog: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Static Methods (Backward Compatibility)
        
        /// <summary>
        /// 静的アクセス用のログメソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().Log()を使用してください
        /// </summary>
        public static void LogStatic(string message)
        {
            GetServiceInstance()?.Log(message);
        }
        
        /// <summary>
        /// 静的アクセス用の警告ログメソチE���E�レガシー互換性用�E�E        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogWarning() instead")]
        public static void LogWarningStatic(string message)
        {
            GetServiceInstance()?.LogWarning(message);
        }
        
        /// <summary>
        /// 静的アクセス用のエラーログメソチE���E�レガシー互換性用�E�E        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogError() instead")]
        public static void LogErrorStatic(string message)
        {
            GetServiceInstance()?.LogError(message);
        }
        
        /// <summary>
        /// 静的アクセス用のイベントログメソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEvent()を使用してください
        /// </summary>
        public static void LogEventStatic(string eventName, int listenerCount, string payload = "")
        {
            GetServiceInstance()?.LogEvent(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 静的アクセス用の型安�Eペイロード付きイベントログメソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEventWithPayload()を使用してください
        /// </summary>
        public static void LogEventWithPayloadStatic<T>(string eventName, int listenerCount, T payload)
        {
            GetServiceInstance()?.LogEventWithPayload(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 静的アクセス用のログクリアメソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ClearLog()を使用してください
        /// </summary>
        public static void ClearLogStatic()
        {
            GetServiceInstance()?.ClearLog();
        }
        
        /// <summary>
        /// 静的アクセス用のフィルタリングログ取得メソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetFilteredLog()を使用してください
        /// </summary>
        public static List<EventLogEntry> GetFilteredLogStatic(string nameFilter = "", LogLevel minLevel = LogLevel.Info)
        {
            return GetServiceInstance()?.GetFilteredLog(nameFilter, minLevel) ?? new List<EventLogEntry>();
        }
        
        /// <summary>
        /// 静的アクセス用の統計情報取得メソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetStatistics()を使用してください
        /// </summary>
        public static LogStatistics GetStatisticsStatic()
        {
            return GetServiceInstance()?.GetStatistics() ?? new LogStatistics();
        }
        
        /// <summary>
        /// 静的アクセス用のCSVエクスポ�EトメソチE���E�レガシー互換性用�E�E        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ExportToCSV()を使用してください
        /// </summary>
        public static void ExportToCSVStatic(string filePath)
        {
            GetServiceInstance()?.ExportToCSV(filePath);
        }
        
        #endregion
        
        #region Data Structures
        
        /// <summary>
        /// イベントログエントリのチE�Eタ構造
        /// </summary>
        [System.Serializable]
        public class EventLogEntry
        {
            public string eventName;
            public int listenerCount;
            public string payload;
            public LogLevel level;
            public DateTime timestamp;
            
            public EventLogEntry(string eventName, int listenerCount, string payload, LogLevel level)
            {
                this.eventName = eventName;
                this.listenerCount = listenerCount;
                this.payload = payload;
                this.level = level;
                this.timestamp = DateTime.Now;
            }
        }
        
        /// <summary>
        /// ログレベル列挙佁E        /// </summary>
        public enum LogLevel
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }
        
        /// <summary>
        /// ログ統計情報の構造
        /// </summary>
        [System.Serializable]
        public class LogStatistics
        {
            public int totalEvents;
            public int infoCount;
            public int warningCount;
            public int errorCount;
            public Dictionary<string, int> eventCounts = new Dictionary<string, int>();
        }
        
        #endregion
    }
}