using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Legacy Singleton使用状況を監視し、移行進捗を追跡する
    /// Step 3.9: Legacy Singleton警告システムの一部
    /// </summary>
    public class MigrationMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableRealTimeLogging = true;
        [SerializeField] private bool enableUsageTracking = true;
        [SerializeField] private float reportingInterval = 30f; // 30秒ごとにレポート

        [Header("Usage Statistics")]
        [SerializeField] private int totalSingletonAccesses = 0;
        [SerializeField] private int uniqueSingletonClasses = 0;
        
        // 使用状況の記録
        private Dictionary<Type, SingletonUsageInfo> usageStats = new Dictionary<Type, SingletonUsageInfo>();
        private List<SingletonUsageEvent> recentEvents = new List<SingletonUsageEvent>();
        private const int MAX_RECENT_EVENTS = 100;
        
        private void Start()
        {
            if (enableUsageTracking && reportingInterval > 0)
            {
                InvokeRepeating(nameof(GenerateUsageReport), reportingInterval, reportingInterval);
            }
            
            EventLogger.LogStatic("[MigrationMonitor] Started monitoring singleton usage");
        }
        
        private void OnDestroy()
        {
            if (enableUsageTracking)
            {
                GenerateUsageReport();
                SaveUsageStatistics();
            }
        }
        
        /// <summary>
        /// Singleton使用を記録する
        /// </summary>
        /// <param name="singletonType">使用されたSingletonの型</param>
        /// <param name="accessMethod">アクセス方法 (例: "AudioManager.Instance")</param>
        public void LogSingletonUsage(Type singletonType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            totalSingletonAccesses++;
            
            // 統計情報を更新
            if (!usageStats.ContainsKey(singletonType))
            {
                usageStats[singletonType] = new SingletonUsageInfo
                {
                    SingletonType = singletonType,
                    AccessMethod = accessMethod,
                    FirstAccessTime = DateTime.Now,
                    AccessCount = 0
                };
                uniqueSingletonClasses++;
            }
            
            var info = usageStats[singletonType];
            info.AccessCount++;
            info.LastAccessTime = DateTime.Now;
            
            // 最近のイベントを記録
            var usageEvent = new SingletonUsageEvent
            {
                Timestamp = DateTime.Now,
                SingletonType = singletonType.Name,
                AccessMethod = accessMethod,
                StackTrace = enableRealTimeLogging ? Environment.StackTrace : null
            };
            
            recentEvents.Add(usageEvent);
            if (recentEvents.Count > MAX_RECENT_EVENTS)
            {
                recentEvents.RemoveAt(0);
            }
            
            // リアルタイムログ出力
            if (enableRealTimeLogging)
            {
                if (FeatureFlags.EnableMigrationWarnings)
                {
                    EventLogger.LogWarningStatic($"[MigrationMonitor] Singleton access detected: {singletonType.Name}.{accessMethod} (Total: {info.AccessCount})");
                }
                else
                {
                    EventLogger.LogStatic($"[MigrationMonitor] Singleton access: {singletonType.Name}.{accessMethod}");
                }
            }
        }
        
        /// <summary>
        /// 現在の使用状況レポートを生成
        /// </summary>
        [ContextMenu("Generate Usage Report")]
        public void GenerateUsageReport()
        {
            if (usageStats.Count == 0)
            {
                EventLogger.LogStatic("[MigrationMonitor] No singleton usage detected");
                return;
            }
            
            EventLogger.LogStatic("[MigrationMonitor] === Singleton Usage Report ===");
            EventLogger.LogStatic($"  Total Accesses: {totalSingletonAccesses}");
            EventLogger.LogStatic($"  Unique Classes: {uniqueSingletonClasses}");
            EventLogger.LogStatic($"  Monitoring Period: {Time.time:F1} seconds");
            
            EventLogger.LogStatic("  Usage Details:");
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                var duration = info.LastAccessTime - info.FirstAccessTime;
                EventLogger.LogStatic($"    - {info.SingletonType.Name}: {info.AccessCount} accesses " +
                               $"(First: {info.FirstAccessTime:HH:mm:ss}, Last: {info.LastAccessTime:HH:mm:ss}, " +
                               $"Duration: {duration.TotalSeconds:F1}s)");
            }
        }
        
        /// <summary>
        /// 最近の使用イベントを表示
        /// </summary>
        [ContextMenu("Show Recent Events")]
        public void ShowRecentEvents()
        {
            if (recentEvents.Count == 0)
            {
                EventLogger.LogStatic("[MigrationMonitor] No recent singleton events");
                return;
            }
            
            EventLogger.LogStatic("[MigrationMonitor] === Recent Singleton Events ===");
            int displayCount = Mathf.Min(recentEvents.Count, 10); // 最新10件を表示
            
            for (int i = recentEvents.Count - displayCount; i < recentEvents.Count; i++)
            {
                var evt = recentEvents[i];
                EventLogger.LogStatic($"  [{evt.Timestamp:HH:mm:ss}] {evt.SingletonType}.{evt.AccessMethod}");
            }
        }
        
        /// <summary>
        /// 使用統計をPlayerPrefsに保存
        /// </summary>
        public void SaveUsageStatistics()
        {
            try
            {
                PlayerPrefs.SetInt("MigrationMonitor_TotalAccesses", totalSingletonAccesses);
                PlayerPrefs.SetInt("MigrationMonitor_UniqueClasses", uniqueSingletonClasses);
                PlayerPrefs.SetString("MigrationMonitor_LastReportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // 各Singletonの使用回数を保存
                foreach (var kvp in usageStats)
                {
                    string key = $"MigrationMonitor_Usage_{kvp.Key.Name}";
                    PlayerPrefs.SetInt(key, kvp.Value.AccessCount);
                }
                
                PlayerPrefs.Save();
                EventLogger.LogStatic("[MigrationMonitor] Usage statistics saved");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[MigrationMonitor] Failed to save statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存された統計を読み込み
        /// </summary>
        public void LoadUsageStatistics()
        {
            try
            {
                totalSingletonAccesses = PlayerPrefs.GetInt("MigrationMonitor_TotalAccesses", 0);
                uniqueSingletonClasses = PlayerPrefs.GetInt("MigrationMonitor_UniqueClasses", 0);
                string lastReportTime = PlayerPrefs.GetString("MigrationMonitor_LastReportTime", "Never");
                
                EventLogger.LogStatic($"[MigrationMonitor] Loaded statistics - Total: {totalSingletonAccesses}, " +
                               $"Unique: {uniqueSingletonClasses}, Last Report: {lastReportTime}");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[MigrationMonitor] Failed to load statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 統計をリセット
        /// </summary>
        [ContextMenu("Reset Statistics")]
        public void ResetStatistics()
        {
            usageStats.Clear();
            recentEvents.Clear();
            totalSingletonAccesses = 0;
            uniqueSingletonClasses = 0;
            
            // PlayerPrefsからも削除
            PlayerPrefs.DeleteKey("MigrationMonitor_TotalAccesses");
            PlayerPrefs.DeleteKey("MigrationMonitor_UniqueClasses");
            PlayerPrefs.DeleteKey("MigrationMonitor_LastReportTime");
            
            EventLogger.LogStatic("[MigrationMonitor] Statistics reset");
        }
        
        /// <summary>
        /// 移行の推奨事項を生成
        /// </summary>
        [ContextMenu("Generate Migration Recommendations")]
        public void GenerateMigrationRecommendations()
        {
            EventLogger.LogStatic("[MigrationMonitor] === Migration Recommendations ===");
            
            if (usageStats.Count == 0)
            {
                EventLogger.LogStatic("  ✅ No singleton usage detected - migration appears complete!");
                return;
            }
            
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                string recommendation = GetMigrationRecommendation(info);
                EventLogger.LogStatic($"  📋 {info.SingletonType.Name}: {recommendation}");
            }
        }
        
        /// <summary>
        /// 特定のSingletonに対する移行推奨事項を取得
        /// </summary>
        private string GetMigrationRecommendation(SingletonUsageInfo info)
        {
            if (info.AccessCount > 50)
            {
                return "❗ High usage detected - Priority migration recommended";
            }
            else if (info.AccessCount > 10)
            {
                return "⚠️  Medium usage - Schedule migration soon";
            }
            else
            {
                return "💡 Low usage - Can be migrated when convenient";
            }
        }
    }
    
    /// <summary>
    /// Singleton使用情報を格納するクラス
    /// </summary>
    [System.Serializable]
    public class SingletonUsageInfo
    {
        public Type SingletonType;
        public string AccessMethod;
        public DateTime FirstAccessTime;
        public DateTime LastAccessTime;
        public int AccessCount;
    }
    
    /// <summary>
    /// Singleton使用イベントを格納するクラス
    /// </summary>
    [System.Serializable]
    public class SingletonUsageEvent
    {
        public DateTime Timestamp;
        public string SingletonType;
        public string AccessMethod;
        public string StackTrace;
    }
}