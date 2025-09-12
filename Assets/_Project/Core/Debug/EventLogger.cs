using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// 中央イベントロギングシステム（ServiceLocator移行版）
    /// ServiceLocatorパターンでイベントログ管理へのアクセスを提供する
    /// 
    /// 設計思想:
    /// - 中央集権的なログ管理による一貫性確保
    /// - 複数出力形式対応（Console, File, DebugWindow, RemoteDebugger）
    /// - Unity MonoBehaviourのライフサイクルに統合された安全なサービス管理
    /// - ServiceLocatorパターンによる依存性注入対応
    /// - 後方互換性を維持しながら段階的移行を支援
    /// 
    /// 推奨使用例:
    /// var logger = ServiceLocator.GetService&lt;IEventLogger&gt;();
    /// logger.Log("Application started");
    /// logger.LogEvent("PlayerDamaged", 3, "damage:25");
    /// </summary>
    public class EventLogger : MonoBehaviour, IEventLogger, IInitializable
    {
        // ✅ ServiceLocator移行: Legacy Singleton警告システム（後方互換性のため）
        
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
        /// 初期化優先度（数値が小さいほど早く初期化される）
        /// EventLoggerは他のサービスより早く初期化される必要がある
        /// </summary>
        public int Priority => 5;
        
        /// <summary>
        /// サービスが初期化済みかどうかを示すフラグ
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// サービスの初期化処理
        /// ServiceLocatorによって呼び出される
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            
            InitializeService();
        }
        
        
        
        
        

        
        #endregion
        
        #region Properties (IEventLogger Implementation)
        
        /// <summary>
        /// ログが有効かどうか（IEventLoggerインターフェース実装）
        /// </summary>
        public bool IsEnabled => enableLogging;
        
        /// <summary>
        /// 現在のイベントログエントリのリスト（IEventLoggerインターフェース実装）
        /// </summary>
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
            
            // Editor環境ではDontDestroyOnLoadは使用不可のため条件チェック
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
        /// サービスの内部初期化処理
        /// EventLoggerの設定読み込みと基本状態設定を行う
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
        /// ServiceLocator経由でEventLoggerインスタンスを取得
        /// </summary>
        private static IEventLogger GetServiceInstance()
        {
            return ServiceLocator.GetService<IEventLogger>();
        }
        
        private void AddLogEntry(EventLogEntry entry)
        {
            if (!enableLogging) return;
            
            eventLog.Add(entry);
            
            // 最大エントリ数を超えた場合、古いエントリを削除
            if (eventLog.Count > maxLogEntries)
            {
                eventLog.RemoveRange(0, eventLog.Count - maxLogEntries);
            }
        }
        
        /// <summary>
        /// サービスの現在の状態をログ出力
        /// </summary>
        public void LogServiceStatus()
        {
            if (!IsEnabled) return;
            
            var statusMessage = $"EventLogger Service Status - Enabled: {IsEnabled}, Entries: {EventLog.Count}, ServiceLocator: {FeatureFlags.UseServiceLocator}";
            UnityEngine.Debug.Log($"<color=green>[EventLogger Service]</color> {statusMessage}");
        }
        
        #endregion
        
        #region IEventLogger Implementation
        
        /// <summary>
        /// 簡潔なログメソッド - Unity標準Debug.Logの代替（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// 簡潔な警告ログメソッド - Unity標準Debug.LogWarningの代替（IEventLoggerインターフェース実装）
        /// </summary>
        public void LogWarning(string message)
        {
            LogWarning("General", 0, message);
        }
        
        /// <summary>
        /// 警告レベルのイベントログを記録（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// 簡潔なエラーログメソッド - Unity標準Debug.LogErrorの代替（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// エラーレベルのイベントログを記録（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// イベントログを記録（リスナー数とペイロード付き）（IEventLoggerインターフェース実装）
        /// </summary>
        public void LogEvent(string eventName, int listenerCount, string payload = "")
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry(eventName, listenerCount, payload, LogLevel.Info));
        }
        
        /// <summary>
        /// 型安全なペイロード付きイベントログを記録（IEventLoggerインターフェース実装）
        /// </summary>
        public void LogEventWithPayload<T>(string eventName, int listenerCount, T payload)
        {
            if (!IsEnabled) return;
            
            string payloadString = payload?.ToString() ?? "";
            LogEvent(eventName, listenerCount, payloadString);
        }
        
        /// <summary>
        /// ログをクリア（IEventLoggerインターフェース実装）
        /// </summary>
        public void ClearLog()
        {
            eventLog.Clear();
        }
        
        /// <summary>
        /// フィルタリングされたログエントリを取得（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// ログの統計情報を取得（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// ログをCSVファイルにエクスポート（IEventLoggerインターフェース実装）
        /// </summary>
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
        /// 静的アクセス用のログメソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().Log()を使用してください
        /// </summary>
        public static void LogStatic(string message)
        {
            GetServiceInstance()?.Log(message);
        }
        
        /// <summary>
        /// 静的アクセス用の警告ログメソッド（レガシー互換性用）
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogWarning() instead")]
        public static void LogWarningStatic(string message)
        {
            GetServiceInstance()?.LogWarning(message);
        }
        
        /// <summary>
        /// 静的アクセス用のエラーログメソッド（レガシー互換性用）
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogError() instead")]
        public static void LogErrorStatic(string message)
        {
            GetServiceInstance()?.LogError(message);
        }
        
        /// <summary>
        /// 静的アクセス用のイベントログメソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEvent()を使用してください
        /// </summary>
        public static void LogEventStatic(string eventName, int listenerCount, string payload = "")
        {
            GetServiceInstance()?.LogEvent(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 静的アクセス用の型安全ペイロード付きイベントログメソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEventWithPayload()を使用してください
        /// </summary>
        public static void LogEventWithPayloadStatic<T>(string eventName, int listenerCount, T payload)
        {
            GetServiceInstance()?.LogEventWithPayload(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 静的アクセス用のログクリアメソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ClearLog()を使用してください
        /// </summary>
        public static void ClearLogStatic()
        {
            GetServiceInstance()?.ClearLog();
        }
        
        /// <summary>
        /// 静的アクセス用のフィルタリングログ取得メソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetFilteredLog()を使用してください
        /// </summary>
        public static List<EventLogEntry> GetFilteredLogStatic(string nameFilter = "", LogLevel minLevel = LogLevel.Info)
        {
            return GetServiceInstance()?.GetFilteredLog(nameFilter, minLevel) ?? new List<EventLogEntry>();
        }
        
        /// <summary>
        /// 静的アクセス用の統計情報取得メソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetStatistics()を使用してください
        /// </summary>
        public static LogStatistics GetStatisticsStatic()
        {
            return GetServiceInstance()?.GetStatistics() ?? new LogStatistics();
        }
        
        /// <summary>
        /// 静的アクセス用のCSVエクスポートメソッド（レガシー互換性用）
        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ExportToCSV()を使用してください
        /// </summary>
        public static void ExportToCSVStatic(string filePath)
        {
            GetServiceInstance()?.ExportToCSV(filePath);
        }
        
        #endregion
        
        #region Data Structures
        
        /// <summary>
        /// イベントログエントリのデータ構造
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
        /// ログレベル列挙体
        /// </summary>
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