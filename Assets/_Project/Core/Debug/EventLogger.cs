using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// 中央イベントロギングシステム
    /// 全てのGameEventの発行を記録し、デバッグを支援する
    /// </summary>
    public class EventLogger : MonoBehaviour
    {
        private static EventLogger instance;
        private List<EventLogEntry> eventLog = new List<EventLogEntry>();
        private EventLoggerSettings settings;
        
        [Header("Runtime Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int maxLogEntries = 1000;
        
        public static EventLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("EventLogger");
                    instance = go.AddComponent<EventLogger>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public static bool IsEnabled => Instance.enableLogging;
        public static List<EventLogEntry> EventLog => Instance.eventLog;
        
        /// <summary>
        /// ログエントリのデータ構造
        /// </summary>
        [System.Serializable]
        public class EventLogEntry
        {
            public float timestamp;
            public string eventName;
            public string payloadData;
            public int listenerCount;
            public LogLevel level;
            public string stackTrace;
            public Color displayColor;
            
            public EventLogEntry(string name, int listeners, string payload = "", LogLevel logLevel = LogLevel.Info)
            {
                timestamp = Time.realtimeSinceStartup;
                eventName = name;
                payloadData = payload;
                listenerCount = listeners;
                level = logLevel;
                stackTrace = Environment.StackTrace;
                
                // イベント名に基づいた色分け
                var hash = eventName.GetHashCode();
                displayColor = Color.HSVToRGB((hash % 360) / 360f, 0.6f, 0.9f);
            }
        }
        
        /// <summary>
        /// ログレベル定義
        /// </summary>
        public enum LogLevel
        {
            Info,
            Warning, 
            Error
        }
        
        /// <summary>
        /// 出力形式オプション
        /// </summary>
        public enum LogOutputFormat
        {
            Console,
            File,
            DebugWindow,
            RemoteDebugger
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
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
        /// 基本的なイベントログを記録
        /// </summary>
        public static void LogEvent(string eventName, int listenerCount, string payload = "")
        {
            if (!IsEnabled) return;
            
            Instance.AddLogEntry(new EventLogEntry(eventName, listenerCount, payload, LogLevel.Info));
        }
        
        /// <summary>
        /// ペイロード付きイベントログを記録
        /// </summary>
        public static void LogEventWithPayload<T>(string eventName, int listenerCount, T payload)
        {
            if (!IsEnabled) return;
            
            string payloadStr = "";
            if (payload != null)
            {
                if (payload is UnityEngine.Object unityObj)
                {
                    payloadStr = $"[{typeof(T).Name}] {unityObj.name}";
                }
                else
                {
                    payloadStr = $"[{typeof(T).Name}] {payload.ToString()}";
                }
            }
            
            Instance.AddLogEntry(new EventLogEntry(eventName, listenerCount, payloadStr, LogLevel.Info));
        }
        
        /// <summary>
        /// 警告レベルのイベントログを記録
        /// </summary>
        public static void LogWarning(string eventName, int listenerCount, string message)
        {
            if (!IsEnabled) return;
            
            Instance.AddLogEntry(new EventLogEntry(eventName, listenerCount, message, LogLevel.Warning));
            
            if (Instance.settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.LogWarning($"[EventLogger] {eventName}: {message}");
            }
        }
        
        /// <summary>
        /// エラーレベルのイベントログを記録
        /// </summary>
        public static void LogError(string eventName, int listenerCount, string message)
        {
            if (!IsEnabled) return;
            
            Instance.AddLogEntry(new EventLogEntry(eventName, listenerCount, message, LogLevel.Error));
            
            if (Instance.settings?.logToConsole ?? true)
            {
                UnityEngine.Debug.LogError($"[EventLogger] {eventName}: {message}");
            }
        }
        
        private void AddLogEntry(EventLogEntry entry)
        {
            eventLog.Add(entry);
            
            // ログサイズ制限
            while (eventLog.Count > maxLogEntries)
            {
                eventLog.RemoveAt(0);
            }
            
            // 設定に応じてコンソール出力
            if (settings?.logToConsole ?? false)
            {
                string logMessage = $"[{entry.timestamp:F2}s] {entry.eventName} ({entry.listenerCount} listeners)";
                if (!string.IsNullOrEmpty(entry.payloadData))
                {
                    logMessage += $" - {entry.payloadData}";
                }
                
                switch (entry.level)
                {
                    case LogLevel.Info:
                        UnityEngine.Debug.Log($"<color=cyan>[EventLogger]</color> {logMessage}");
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning($"[EventLogger] {logMessage}");
                        break;
                    case LogLevel.Error:
                        UnityEngine.Debug.LogError($"[EventLogger] {logMessage}");
                        break;
                }
            }
            
            // ファイル出力
            if (settings?.logToFile ?? false)
            {
                WriteToFile(entry);
            }
        }
        
        private void WriteToFile(EventLogEntry entry)
        {
            try
            {
                string logDir = Path.Combine(Application.persistentDataPath, "EventLogs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                string fileName = $"EventLog_{DateTime.Now:yyyy-MM-dd}.txt";
                string filePath = Path.Combine(logDir, fileName);
                
                string logLine = $"[{entry.timestamp:F2}s] [{entry.level}] {entry.eventName} ({entry.listenerCount} listeners)";
                if (!string.IsNullOrEmpty(entry.payloadData))
                {
                    logLine += $" - {entry.payloadData}";
                }
                logLine += "\n";
                
                File.AppendAllText(filePath, logLine);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"EventLogger: Failed to write to file - {ex.Message}");
            }
        }
        
        /// <summary>
        /// ログをクリア
        /// </summary>
        public static void ClearLog()
        {
            Instance.eventLog.Clear();
        }
        
        /// <summary>
        /// フィルタリングされたログエントリを取得
        /// </summary>
        public static List<EventLogEntry> GetFilteredLog(string nameFilter = "", LogLevel minLevel = LogLevel.Info)
        {
            var filtered = new List<EventLogEntry>();
            
            foreach (var entry in Instance.eventLog)
            {
                if (entry.level < minLevel) continue;
                if (!string.IsNullOrEmpty(nameFilter) && !entry.eventName.ToLower().Contains(nameFilter.ToLower())) continue;
                
                filtered.Add(entry);
            }
            
            return filtered;
        }
        
        /// <summary>
        /// ログの統計情報を取得
        /// </summary>
        public static LogStatistics GetStatistics()
        {
            var stats = new LogStatistics();
            var eventCounts = new Dictionary<string, int>();
            
            foreach (var entry in Instance.eventLog)
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
                    stats.uniqueEventTypes++;
                }
            }
            
            // 最も頻繁なイベントを特定
            stats.mostFrequentEvent = "";
            int maxCount = 0;
            foreach (var kvp in eventCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    stats.mostFrequentEvent = kvp.Key;
                    stats.mostFrequentEventCount = maxCount;
                }
            }
            
            return stats;
        }
        
        [System.Serializable]
        public class LogStatistics
        {
            public int totalEvents;
            public int infoCount;
            public int warningCount; 
            public int errorCount;
            public int uniqueEventTypes;
            public string mostFrequentEvent;
            public int mostFrequentEventCount;
        }
        
        /// <summary>
        /// CSVファイルにエクスポート
        /// </summary>
        public static void ExportToCSV(string filePath)
        {
            try
            {
                var csv = "Timestamp,EventName,ListenerCount,Level,PayloadData\n";
                
                foreach (var entry in Instance.eventLog)
                {
                    csv += $"{entry.timestamp:F2},{entry.eventName},{entry.listenerCount},{entry.level},\"{entry.payloadData}\"\n";
                }
                
                File.WriteAllText(filePath, csv);
                UnityEngine.Debug.Log($"Event log exported to: {filePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"EventLogger: Failed to export CSV - {ex.Message}");
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// エディタ用：ログの状態を表示
        /// </summary>
        [ContextMenu("Show Log Status")]
        private void ShowLogStatus()
        {
            var stats = GetStatistics();
            UnityEngine.Debug.Log($"EventLogger Status:\n" +
                     $"Total Events: {stats.totalEvents}\n" +
                     $"Info: {stats.infoCount}, Warnings: {stats.warningCount}, Errors: {stats.errorCount}\n" +
                     $"Unique Event Types: {stats.uniqueEventTypes}\n" +
                     $"Most Frequent: {stats.mostFrequentEvent} ({stats.mostFrequentEventCount} times)");
        }
        #endif
    }
}