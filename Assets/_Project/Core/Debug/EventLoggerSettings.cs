using UnityEngine;

namespace asterivo.Unity60.Core.Debug
{
    /// <summary>
    /// EventLoggerの設定を管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EventLoggerSettings", menuName = "asterivo.Unity60/Debug/Event Logger Settings")]
    public class EventLoggerSettings : ScriptableObject
    {
        [Header("Logging Configuration")]
        [SerializeField] public bool enableLogging = true;
        [SerializeField] public bool logToConsole = false;
        [SerializeField] public bool logToFile = false;
        [SerializeField] public int maxLogEntries = 1000;
        
        [Header("Performance Settings")]
        [SerializeField] public bool enableInBuild = false;
        [SerializeField] public float logFlushInterval = 1.0f; // ファイル書き込み間隔
        [SerializeField] public bool useAsyncLogging = true;
        
        [Header("Filter Settings")]
        [SerializeField] public EventLogger.LogLevel minimumLogLevel = EventLogger.LogLevel.Info;
        [SerializeField] public string[] eventNameFilters;
        [SerializeField] public string[] excludeEventNames;
        [SerializeField] public bool excludeSystemEvents = false;
        
        [Header("Output Settings")]
        [SerializeField] public bool includeStackTrace = false;
        [SerializeField] public bool includeTimestamp = true;
        [SerializeField] public bool includePayloadData = true;
        [SerializeField] public string logFilePrefix = "EventLog";
        
        [Header("Debug Window Settings")]
        [SerializeField] public bool autoScrollEnabled = true;
        [SerializeField] public int maxDisplayEntries = 500;
        [SerializeField] public bool useColorCoding = true;
        [SerializeField] public bool showPayloadPreview = true;
        
        [Header("System Events")]
        [Tooltip("Unity内部システムイベントをログに含めるか")]
        [SerializeField] public bool logUnitySystemEvents = false;
        [Tooltip("エディタ専用イベントをログに含めるか")]
        [SerializeField] public bool logEditorOnlyEvents = false;
        
        /// <summary>
        /// 指定されたイベント名がフィルタリング対象かチェック
        /// </summary>
        public bool IsEventFiltered(string eventName)
        {
            // 除外リストにある場合は除外
            if (excludeEventNames != null)
            {
                foreach (var exclude in excludeEventNames)
                {
                    if (!string.IsNullOrEmpty(exclude) && eventName.Contains(exclude))
                    {
                        return true;
                    }
                }
            }
            
            // 包含フィルターが設定されている場合はそれをチェック
            if (eventNameFilters != null && eventNameFilters.Length > 0)
            {
                bool hasMatch = false;
                foreach (var filter in eventNameFilters)
                {
                    if (!string.IsNullOrEmpty(filter) && eventName.Contains(filter))
                    {
                        hasMatch = true;
                        break;
                    }
                }
                return !hasMatch; // マッチしない場合は除外
            }
            
            // システムイベントの除外チェック
            if (excludeSystemEvents && IsSystemEvent(eventName))
            {
                return true;
            }
            
            return false; // フィルタリングしない
        }
        
        /// <summary>
        /// システムイベントかどうかを判定
        /// </summary>
        private bool IsSystemEvent(string eventName)
        {
            var systemPrefixes = new[]
            {
                "Unity",
                "System",
                "Editor",
                "Application",
                "Scene"
            };
            
            foreach (var prefix in systemPrefixes)
            {
                if (eventName.StartsWith(prefix))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// ログレベルがフィルタリング条件を満たすかチェック
        /// </summary>
        public bool IsLogLevelEnabled(EventLogger.LogLevel level)
        {
            return level >= minimumLogLevel;
        }
        
        /// <summary>
        /// 現在の設定でログが有効かどうか
        /// </summary>
        public bool IsLoggingEnabled
        {
            get
            {
                #if UNITY_EDITOR
                return enableLogging;
                #elif DEVELOPMENT_BUILD
                return enableLogging && enableInBuild;
                #else
                return false;
                #endif
            }
        }
        
        /// <summary>
        /// デフォルト設定を作成
        /// </summary>
        [ContextMenu("Reset to Default")]
        private void ResetToDefault()
        {
            enableLogging = true;
            logToConsole = false;
            logToFile = false;
            maxLogEntries = 1000;
            
            enableInBuild = false;
            logFlushInterval = 1.0f;
            useAsyncLogging = true;
            
            minimumLogLevel = EventLogger.LogLevel.Info;
            eventNameFilters = new string[0];
            excludeEventNames = new string[0];
            excludeSystemEvents = false;
            
            includeStackTrace = false;
            includeTimestamp = true;
            includePayloadData = true;
            logFilePrefix = "EventLog";
            
            autoScrollEnabled = true;
            maxDisplayEntries = 500;
            useColorCoding = true;
            showPayloadPreview = true;
            
            logUnitySystemEvents = false;
            logEditorOnlyEvents = false;
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// プリセット設定：開発用
        /// </summary>
        [ContextMenu("Development Preset")]
        private void SetDevelopmentPreset()
        {
            enableLogging = true;
            logToConsole = true;
            logToFile = true;
            maxLogEntries = 2000;
            minimumLogLevel = EventLogger.LogLevel.Info;
            includeStackTrace = true;
            useColorCoding = true;
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// プリセット設定：パフォーマンス重視
        /// </summary>
        [ContextMenu("Performance Preset")]
        private void SetPerformancePreset()
        {
            enableLogging = true;
            logToConsole = false;
            logToFile = false;
            maxLogEntries = 500;
            minimumLogLevel = EventLogger.LogLevel.Warning;
            includeStackTrace = false;
            useAsyncLogging = true;
            excludeSystemEvents = true;
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        /// <summary>
        /// プリセット設定：デバッグ専用
        /// </summary>
        [ContextMenu("Debug Preset")]
        private void SetDebugPreset()
        {
            enableLogging = true;
            logToConsole = true;
            logToFile = true;
            maxLogEntries = 5000;
            minimumLogLevel = EventLogger.LogLevel.Info;
            includeStackTrace = true;
            includePayloadData = true;
            logUnitySystemEvents = true;
            logEditorOnlyEvents = true;
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}