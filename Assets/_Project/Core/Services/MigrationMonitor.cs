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
        private List<ServiceLocatorUsageEvent> recentServiceLocatorEvents = new List<ServiceLocatorUsageEvent>();
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
        /// ServiceLocator使用を記録する
        /// </summary>
        /// <param name="serviceType">使用されたサービスの型</param>
        /// <param name="accessMethod">アクセス方法 (例: "ServiceLocator.GetService<IAudioService>()")</param>
        public void LogServiceLocatorUsage(Type serviceType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            // ServiceLocator使用の記録（ポジティブな指標として扱う）
            
            // リアルタイムログ出力
            if (enableRealTimeLogging)
            {
                EventLogger.LogStatic($"[MigrationMonitor] ServiceLocator usage: {serviceType.Name} via {accessMethod}");
            }
            
            // ServiceLocator使用イベントを記録
            var usageEvent = new ServiceLocatorUsageEvent
            {
                Timestamp = DateTime.Now,
                ServiceType = serviceType.Name,
                AccessMethod = accessMethod
            };
            
            // 最近のイベントに追加（ServiceLocatorイベント用のリストがあればそこに、なければ既存のリストに追加）
            if (recentServiceLocatorEvents == null)
            {
                recentServiceLocatorEvents = new List<ServiceLocatorUsageEvent>();
            }
            
            recentServiceLocatorEvents.Add(usageEvent);
            if (recentServiceLocatorEvents.Count > MAX_RECENT_EVENTS)
            {
                recentServiceLocatorEvents.RemoveAt(0);
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
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の範囲で取得
        /// ServiceLocator使用率と Legacy Singleton無効化状態から算出
        /// </summary>
        /// <returns>移行進捗 (0.0 = 未開始, 1.0 = 完了)</returns>
        public float GetMigrationProgress()
        {
            // Phase 1: Legacy Singleton無効化チェック
            float phase1Progress = FeatureFlags.DisableLegacySingletons ? 0.6f : 0.0f;
            
            // Phase 2: ServiceLocator使用チェック
            float phase2Progress = FeatureFlags.UseServiceLocator ? 0.3f : 0.0f;
            
            // Phase 3: Legacy使用状況チェック（使用量が少ないほど進捗が高い）
            float phase3Progress = 0.0f;
            if (totalSingletonAccesses == 0)
            {
                // Legacy使用なし = 完了
                phase3Progress = 0.1f;
            }
            else if (totalSingletonAccesses < 10)
            {
                // 低使用量
                phase3Progress = 0.05f;
            }
            // 高使用量の場合はphase3Progress = 0.0f
            
            float totalProgress = phase1Progress + phase2Progress + phase3Progress;
            
            // デバッグログ出力
            if (enableRealTimeLogging)
            {
                EventLogger.LogStatic($"[MigrationMonitor] Migration Progress: {totalProgress:P1} " +
                               $"(Phase1: {phase1Progress:P1}, Phase2: {phase2Progress:P1}, Phase3: {phase3Progress:P1})");
            }
            
            return totalProgress;
        }

        /// <summary>
        /// 移行の安全性を判定する
        /// 重要なサービスの登録状態とLegacy Singleton使用状況を総合的に評価
        /// </summary>
        /// <returns>true=安全, false=危険, null=判定不能</returns>
        public bool? IsMigrationSafe()
        {
            try
            {
                // 1. ServiceLocatorの基本動作確認
                if (!FeatureFlags.UseServiceLocator)
                {
                    if (enableRealTimeLogging)
                        EventLogger.LogWarningStatic("[MigrationMonitor] ServiceLocator is disabled - migration safety uncertain");
                    return null; // ServiceLocatorが無効の場合は判定不能
                }
                
                // 2. 重要なサービスの登録状態チェック
                int criticalServicesCount = 0;
                int registeredServicesCount = 0;
                
                // 重要サービスのチェック
                var audioService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                if (audioService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var spatialService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();
                if (spatialService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var effectService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IEffectService>();
                if (effectService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var commandService = ServiceLocator.GetService<asterivo.Unity60.Core.Commands.ICommandPoolService>();
                if (commandService != null) registeredServicesCount++;
                criticalServicesCount++;
                
                var eventLogger = ServiceLocator.GetService<asterivo.Unity60.Core.Debug.IEventLogger>();
                if (eventLogger != null) registeredServicesCount++;
                criticalServicesCount++;
                
                // サービス登録率を算出
                float serviceRegistrationRatio = criticalServicesCount > 0 ? 
                    (float)registeredServicesCount / criticalServicesCount : 0f;
                
                // 3. Legacy Singleton使用量チェック
                bool legacySingletonUsageAcceptable = totalSingletonAccesses < 50; // 50回未満なら安全範囲
                
                // 4. 総合判定
                bool isServicesSafe = serviceRegistrationRatio >= 0.8f; // 80%以上のサービスが登録済み
                bool isLegacyUsageSafe = legacySingletonUsageAcceptable;
                bool isFeatureFlagsSafe = FeatureFlags.UseServiceLocator; // ServiceLocatorが有効
                
                bool overallSafety = isServicesSafe && isLegacyUsageSafe && isFeatureFlagsSafe;
                
                // デバッグ情報出力
                if (enableRealTimeLogging)
                {
                    EventLogger.LogStatic($"[MigrationMonitor] Safety Assessment:");
                    EventLogger.LogStatic($"  Services: {registeredServicesCount}/{criticalServicesCount} ({serviceRegistrationRatio:P1}) - {(isServicesSafe ? "安全" : "危険")}");
                    EventLogger.LogStatic($"  Legacy Usage: {totalSingletonAccesses} accesses - {(isLegacyUsageSafe ? "安全" : "危険")}");
                    EventLogger.LogStatic($"  FeatureFlags: ServiceLocator={FeatureFlags.UseServiceLocator} - {(isFeatureFlagsSafe ? "安全" : "危険")}");
                    EventLogger.LogStatic($"  Overall Safety: {(overallSafety ? "✅ SAFE" : "⚠️ UNSAFE")}");
                }
                
                return overallSafety;
            }
            catch (System.Exception ex)
            {
                if (enableRealTimeLogging)
                    EventLogger.LogErrorStatic($"[MigrationMonitor] Safety assessment failed: {ex.Message}");
                return null; // 例外発生時は判定不能
            }
        }

        
        /// <summary>
        /// Singleton使用統計を取得
        /// </summary>
        /// <returns>Singleton使用統計のディクショナリ</returns>
        public Dictionary<Type, SingletonUsageInfo> GetSingletonUsageStats()
        {
            return new Dictionary<Type, SingletonUsageInfo>(usageStats);
        }
        
        /// <summary>
        /// ServiceLocator使用統計を取得
        /// </summary>
        /// <returns>ServiceLocator使用イベントのリスト</returns>
        public List<ServiceLocatorUsageEvent> GetServiceLocatorUsageStats()
        {
            return new List<ServiceLocatorUsageEvent>(recentServiceLocatorEvents ?? new List<ServiceLocatorUsageEvent>());
        }
/// <summary>
        /// 簡易版の安全性チェック (コンテキストメニュー用)
        /// </summary>
        [ContextMenu("Check Migration Safety")]
        public void CheckMigrationSafety()
        {
            var safetyResult = IsMigrationSafe();
            
            if (safetyResult == null)
            {
                EventLogger.LogWarningStatic("[MigrationMonitor] ⚠️ Migration safety assessment inconclusive");
            }
            else if (safetyResult.Value)
            {
                EventLogger.LogStatic("[MigrationMonitor] ✅ Migration is SAFE to proceed");
            }
            else
            {
                EventLogger.LogErrorStatic("[MigrationMonitor] ⚠️ Migration is UNSAFE - review issues before proceeding");
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
    
    /// <summary>
    /// ServiceLocator使用イベントを格納するクラス
    /// </summary>
    [System.Serializable]
    public class ServiceLocatorUsageEvent
    {
        public DateTime Timestamp;
        public string ServiceType;
        public string AccessMethod;
    }
}