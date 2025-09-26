using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
// using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// 荳ｭ螟ｮ繧､繝吶Φ繝医Ο繧ｮ繝ｳ繧ｰ繧ｷ繧ｹ繝・Β・・erviceLocator遘ｻ陦檎沿・・    /// ServiceLocator繝代ち繝ｼ繝ｳ縺ｧ繧､繝吶Φ繝医Ο繧ｰ邂｡逅・∈縺ｮ繧｢繧ｯ繧ｻ繧ｹ繧呈署萓帙☆繧・    /// 
    /// 險ｭ險域晄Φ:
    /// - 荳ｭ螟ｮ髮・ｨｩ逧・↑繝ｭ繧ｰ邂｡逅・↓繧医ｋ荳雋ｫ諤ｧ遒ｺ菫・    /// - 隍・焚蜃ｺ蜉帛ｽ｢蠑丞ｯｾ蠢懶ｼ・onsole, File, DebugWindow, RemoteDebugger・・    /// - Unity MonoBehaviour縺ｮ繝ｩ繧､繝輔し繧､繧ｯ繝ｫ縺ｫ邨ｱ蜷医＆繧後◆螳牙・縺ｪ繧ｵ繝ｼ繝薙せ邂｡逅・    /// - ServiceLocator繝代ち繝ｼ繝ｳ縺ｫ繧医ｋ萓晏ｭ俶ｧ豕ｨ蜈･蟇ｾ蠢・    /// - 蠕梧婿莠呈鋤諤ｧ繧堤ｶｭ謖√＠縺ｪ縺後ｉ谿ｵ髫守噪遘ｻ陦後ｒ謾ｯ謠ｴ
    /// 
    /// 謗ｨ螂ｨ菴ｿ逕ｨ萓・
    /// var logger = ServiceLocator.GetService&lt;IEventLogger&gt;();
    /// logger.Log("Application started");
    /// logger.LogEvent("PlayerDamaged", 3, "damage:25");
    /// </summary>
    public class EventLogger : MonoBehaviour, IEventLogger, IInitializable
    {
        // 笨・ServiceLocator遘ｻ陦・ Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β・亥ｾ梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ・・        
        private List<EventLogEntry> eventLog = new List<EventLogEntry>();
        private EventLoggerSettings settings;
        
        [Header("Runtime Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int maxLogEntries = 1000;
        [SerializeField] private bool autoRegisterOnAwake = true;
        
        /// <summary>蛻晄悄蛹也憾諷九ヵ繝ｩ繧ｰ</summary>
        private bool _isInitialized = false;
        
        #region IInitializable Implementation
        
        /// <summary>
        /// 蛻晄悄蛹門━蜈亥ｺｦ・域焚蛟､縺悟ｰ上＆縺・⊇縺ｩ譌ｩ縺丞・譛溷喧縺輔ｌ繧具ｼ・        /// EventLogger縺ｯ莉悶・繧ｵ繝ｼ繝薙せ繧医ｊ譌ｩ縺丞・譛溷喧縺輔ｌ繧句ｿ・ｦ√′縺ゅｋ
        /// </summary>
        public int Priority => 5;
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺悟・譛溷喧貂医∩縺九←縺・°繧堤､ｺ縺吶ヵ繝ｩ繧ｰ
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蛻晄悄蛹門・逅・        /// ServiceLocator縺ｫ繧医▲縺ｦ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧・        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            
            InitializeService();
        }
        
        
        
        
        

        
        #endregion
        
        #region Properties (IEventLogger Implementation)
        
        /// <summary>
        /// 繝ｭ繧ｰ縺梧怏蜉ｹ縺九←縺・°・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public bool IsEnabled => enableLogging;
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧､繝吶Φ繝医Ο繧ｰ繧ｨ繝ｳ繝医Μ縺ｮ繝ｪ繧ｹ繝茨ｼ・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public List<EventLogEntry> EventLog => eventLog;
        
        #endregion
        
        #region Static Properties (Backward Compatibility)
        
        /// <summary>
        /// 蠕梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ縺ｮ髱咏噪繝励Ο繝代ユ繧｣
        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().IsEnabled instead")]
        public static bool IsEnabledStatic => GetServiceInstance()?.IsEnabled ?? false;
        
        /// <summary>
        /// 蠕梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ縺ｮ髱咏噪繝励Ο繝代ユ繧｣
        /// </summary>
        public static List<EventLogEntry> EventLogStatic => GetServiceInstance()?.EventLog ?? new List<EventLogEntry>();
        
        #endregion
        
        #region MonoBehaviour Lifecycle
        
        void Awake()
        {
            // ServiceLocator縺ｸ縺ｮ逋ｻ骭ｲ
            if (autoRegisterOnAwake)
            {
                RegisterToServiceLocator();
                LogServiceStatus();
            }
            
            // Editor迺ｰ蠅・〒縺ｯDontDestroyOnLoad縺ｯ菴ｿ逕ｨ荳榊庄縺ｮ縺溘ａ譚｡莉ｶ繝√ぉ繝・け
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        void OnDestroy()
        {
            // ServiceLocator縺九ｉ縺ｮ逋ｻ骭ｲ隗｣髯､
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
        /// ServiceLocator縺ｫEventLogger繧ｵ繝ｼ繝薙せ繧堤匳骭ｲ
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
        /// 繧ｵ繝ｼ繝薙せ縺ｮ蜀・Κ蛻晄悄蛹門・逅・        /// EventLogger縺ｮ險ｭ螳夊ｪｭ縺ｿ霎ｼ縺ｿ縺ｨ蝓ｺ譛ｬ迥ｶ諷玖ｨｭ螳壹ｒ陦後≧
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
        /// ServiceLocator邨檎罰縺ｧEventLogger繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧貞叙蠕・        /// </summary>
        private static IEventLogger GetServiceInstance()
        {
            return ServiceLocator.GetService<IEventLogger>();
        }
        
        private void AddLogEntry(EventLogEntry entry)
        {
            if (!enableLogging) return;
            
            eventLog.Add(entry);
            
            // 譛螟ｧ繧ｨ繝ｳ繝医Μ謨ｰ繧定ｶ・∴縺溷ｴ蜷医∝商縺・お繝ｳ繝医Μ繧貞炎髯､
            if (eventLog.Count > maxLogEntries)
            {
                eventLog.RemoveRange(0, eventLog.Count - maxLogEntries);
            }
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ縺ｮ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ繝ｭ繧ｰ蜃ｺ蜉・        /// </summary>
        public void LogServiceStatus()
        {
            if (!IsEnabled) return;
            
            var statusMessage = $"EventLogger Service Status - Enabled: {IsEnabled}, Entries: {EventLog.Count}, ServiceLocator: {FeatureFlags.UseServiceLocator}";
            UnityEngine.Debug.Log($"<color=green>[EventLogger Service]</color> {statusMessage}");
        }
        
        #endregion
        
        #region IEventLogger Implementation
        
        /// <summary>
        /// 邁｡貎斐↑繝ｭ繧ｰ繝｡繧ｽ繝・ラ - Unity讓呎ｺ縫ebug.Log縺ｮ莉｣譖ｿ・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 邁｡貎斐↑隴ｦ蜻翫Ο繧ｰ繝｡繧ｽ繝・ラ - Unity讓呎ｺ縫ebug.LogWarning縺ｮ莉｣譖ｿ・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public void LogWarning(string message)
        {
            LogWarning("General", 0, message);
        }
        
        /// <summary>
        /// 隴ｦ蜻翫Ξ繝吶Ν縺ｮ繧､繝吶Φ繝医Ο繧ｰ繧定ｨ倬鹸・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 邁｡貎斐↑繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ繝｡繧ｽ繝・ラ - Unity讓呎ｺ縫ebug.LogError縺ｮ莉｣譖ｿ・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 繧ｨ繝ｩ繝ｼ繝ｬ繝吶Ν縺ｮ繧､繝吶Φ繝医Ο繧ｰ繧定ｨ倬鹸・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 繧､繝吶Φ繝医Ο繧ｰ繧定ｨ倬鹸・医Μ繧ｹ繝翫・謨ｰ縺ｨ繝壹う繝ｭ繝ｼ繝我ｻ倥″・会ｼ・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public void LogEvent(string eventName, int listenerCount, string payload = "")
        {
            if (!IsEnabled) return;
            
            AddLogEntry(new EventLogEntry(eventName, listenerCount, payload, LogLevel.Info));
        }
        
        /// <summary>
        /// 蝙句ｮ牙・縺ｪ繝壹う繝ｭ繝ｼ繝我ｻ倥″繧､繝吶Φ繝医Ο繧ｰ繧定ｨ倬鹸・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public void LogEventWithPayload<T>(string eventName, int listenerCount, T payload)
        {
            if (!IsEnabled) return;
            
            string payloadString = payload?.ToString() ?? "";
            LogEvent(eventName, listenerCount, payloadString);
        }
        
        /// <summary>
        /// 繝ｭ繧ｰ繧偵け繝ｪ繧｢・・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
        public void ClearLog()
        {
            eventLog.Clear();
        }
        
        /// <summary>
        /// 繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ縺輔ｌ縺溘Ο繧ｰ繧ｨ繝ｳ繝医Μ繧貞叙蠕暦ｼ・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 繝ｭ繧ｰ縺ｮ邨ｱ險域ュ蝣ｱ繧貞叙蠕暦ｼ・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 繝ｭ繧ｰ繧辰SV繝輔ぃ繧､繝ｫ縺ｫ繧ｨ繧ｯ繧ｹ繝昴・繝茨ｼ・EventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ｼ・        /// </summary>
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
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繝ｭ繧ｰ繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().Log()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static void LogStatic(string message)
        {
            GetServiceInstance()?.Log(message);
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ隴ｦ蜻翫Ο繧ｰ繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogWarning() instead")]
        public static void LogWarningStatic(string message)
        {
            GetServiceInstance()?.LogWarning(message);
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// </summary>
        [System.Obsolete("Use ServiceLocator.GetService<IEventLogger>().LogError() instead")]
        public static void LogErrorStatic(string message)
        {
            GetServiceInstance()?.LogError(message);
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繧､繝吶Φ繝医Ο繧ｰ繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEvent()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static void LogEventStatic(string eventName, int listenerCount, string payload = "")
        {
            GetServiceInstance()?.LogEvent(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ蝙句ｮ牙・繝壹う繝ｭ繝ｼ繝我ｻ倥″繧､繝吶Φ繝医Ο繧ｰ繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().LogEventWithPayload()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static void LogEventWithPayloadStatic<T>(string eventName, int listenerCount, T payload)
        {
            GetServiceInstance()?.LogEventWithPayload(eventName, listenerCount, payload);
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繝ｭ繧ｰ繧ｯ繝ｪ繧｢繝｡繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ClearLog()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static void ClearLogStatic()
        {
            GetServiceInstance()?.ClearLog();
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ繝輔ぅ繝ｫ繧ｿ繝ｪ繝ｳ繧ｰ繝ｭ繧ｰ蜿門ｾ励Γ繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetFilteredLog()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static List<EventLogEntry> GetFilteredLogStatic(string nameFilter = "", LogLevel minLevel = LogLevel.Info)
        {
            return GetServiceInstance()?.GetFilteredLog(nameFilter, minLevel) ?? new List<EventLogEntry>();
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮ邨ｱ險域ュ蝣ｱ蜿門ｾ励Γ繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().GetStatistics()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static LogStatistics GetStatisticsStatic()
        {
            return GetServiceInstance()?.GetStatistics() ?? new LogStatistics();
        }
        
        /// <summary>
        /// 髱咏噪繧｢繧ｯ繧ｻ繧ｹ逕ｨ縺ｮCSV繧ｨ繧ｯ繧ｹ繝昴・繝医Γ繧ｽ繝・ラ・医Ξ繧ｬ繧ｷ繝ｼ莠呈鋤諤ｧ逕ｨ・・        /// ServiceLocator.GetService&lt;IEventLogger&gt;().ExportToCSV()繧剃ｽｿ逕ｨ縺励※縺上□縺輔＞
        /// </summary>
        public static void ExportToCSVStatic(string filePath)
        {
            GetServiceInstance()?.ExportToCSV(filePath);
        }
        
        #endregion
        
        #region Data Structures
        
        /// <summary>
        /// 繧､繝吶Φ繝医Ο繧ｰ繧ｨ繝ｳ繝医Μ縺ｮ繝・・繧ｿ讒矩
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
        /// 繝ｭ繧ｰ繝ｬ繝吶Ν蛻玲嫌菴・        /// </summary>
        public enum LogLevel
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }
        
        /// <summary>
        /// 繝ｭ繧ｰ邨ｱ險域ュ蝣ｱ縺ｮ讒矩
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
