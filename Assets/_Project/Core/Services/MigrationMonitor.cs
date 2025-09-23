using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Legacy Singleton使用状況を監視し、移行進捗を追跡する
    /// Step 3.9: Legacy Singleton警告シスチE��の一部
    /// </summary>
    public class MigrationMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableRealTimeLogging = true;
        [SerializeField] private bool enableUsageTracking = true;
        [SerializeField] private float reportingInterval = 30f; // 30秒ごとにレポ�EチE
        [Header("Usage Statistics")]
        [SerializeField] private int totalSingletonAccesses = 0;
        [SerializeField] private int uniqueSingletonClasses = 0;
        
        // 使用状況�E記録
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
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Started monitoring singleton usage");
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
        /// <param name="singletonType">使用されたSingletonの垁E/param>
        /// <param name="accessMethod">アクセス方況E(侁E "AudioManager.Instance")</param>
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
            
            // 最近�Eイベントを記録
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
            
            // リアルタイムログ出劁E            if (enableRealTimeLogging)
            {
                if (FeatureFlags.EnableMigrationWarnings)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[MigrationMonitor] Singleton access detected: {singletonType.Name}.{accessMethod} (Total: {info.AccessCount})");
                }
                else
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Singleton access: {singletonType.Name}.{accessMethod}");
                }
            }
        }

        /// <summary>
        /// ServiceLocator使用を記録する
        /// </summary>
        /// <param name="serviceType">使用されたサービスの垁E/param>
        /// <param name="accessMethod">アクセス方況E(侁E "ServiceLocator.GetService<IAudioService>()")</param>
        public void LogServiceLocatorUsage(Type serviceType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            // ServiceLocator使用の記録�E��EジチE��ブな持E��として扱ぁE��E            
            // リアルタイムログ出劁E            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] ServiceLocator usage: {serviceType.Name} via {accessMethod}");
            }
            
            // ServiceLocator使用イベントを記録
            var usageEvent = new ServiceLocatorUsageEvent
            {
                Timestamp = DateTime.Now,
                ServiceType = serviceType.Name,
                AccessMethod = accessMethod
            };
            
            // 最近�Eイベントに追加�E�EerviceLocatorイベント用のリストがあればそこに、なければ既存�Eリストに追加�E�E            if (recentServiceLocatorEvents == null)
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
        /// 現在の使用状況レポ�Eトを生�E
        /// </summary>
        [ContextMenu("Generate Usage Report")]
        public void GenerateUsageReport()
        {
            if (usageStats.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] No singleton usage detected");
                return;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Singleton Usage Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Total Accesses: {totalSingletonAccesses}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Unique Classes: {uniqueSingletonClasses}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Monitoring Period: {Time.time:F1} seconds");
            
            ServiceLocator.GetService<IEventLogger>()?.Log("  Usage Details:");
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                var duration = info.LastAccessTime - info.FirstAccessTime;
                ServiceLocator.GetService<IEventLogger>()?.Log($"    - {info.SingletonType.Name}: {info.AccessCount} accesses " +
                               $"(First: {info.FirstAccessTime:HH:mm:ss}, Last: {info.LastAccessTime:HH:mm:ss}, " +
                               $"Duration: {duration.TotalSeconds:F1}s)");
            }
        }
        
        /// <summary>
        /// 最近�E使用イベントを表示
        /// </summary>
        [ContextMenu("Show Recent Events")]
        public void ShowRecentEvents()
        {
            if (recentEvents.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] No recent singleton events");
                return;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Recent Singleton Events ===");
            int displayCount = Mathf.Min(recentEvents.Count, 10); // 最新10件を表示
            
            for (int i = recentEvents.Count - displayCount; i < recentEvents.Count; i++)
            {
                var evt = recentEvents[i];
                ServiceLocator.GetService<IEventLogger>()?.Log($"  [{evt.Timestamp:HH:mm:ss}] {evt.SingletonType}.{evt.AccessMethod}");
            }
        }
        
        /// <summary>
        /// 使用統計をPlayerPrefsに保孁E        /// </summary>
        public void SaveUsageStatistics()
        {
            try
            {
                PlayerPrefs.SetInt("MigrationMonitor_TotalAccesses", totalSingletonAccesses);
                PlayerPrefs.SetInt("MigrationMonitor_UniqueClasses", uniqueSingletonClasses);
                PlayerPrefs.SetString("MigrationMonitor_LastReportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // 各Singletonの使用回数を保孁E                foreach (var kvp in usageStats)
                {
                    string key = $"MigrationMonitor_Usage_{kvp.Key.Name}";
                    PlayerPrefs.SetInt(key, kvp.Value.AccessCount);
                }
                
                PlayerPrefs.Save();
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Usage statistics saved");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Failed to save statistics: {ex.Message}");
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
                
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Loaded statistics - Total: {totalSingletonAccesses}, " +
                               $"Unique: {uniqueSingletonClasses}, Last Report: {lastReportTime}");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Failed to load statistics: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 統計をリセチE��
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
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Statistics reset");
        }
        
        /// <summary>
        /// 移行�E推奨事頁E��生�E
        /// </summary>
        [ContextMenu("Generate Migration Recommendations")]
        public void GenerateMigrationRecommendations()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Migration Recommendations ===");
            
            if (usageStats.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("  ✁ENo singleton usage detected - migration appears complete!");
                return;
            }
            
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                string recommendation = GetMigrationRecommendation(info);
                ServiceLocator.GetService<IEventLogger>()?.Log($"  📋 {info.SingletonType.Name}: {recommendation}");
            }
        }
        
                
        /// <summary>
        /// 移行進捗を0.0-1.0の篁E��で取征E        /// ServiceLocator使用玁E�� Legacy Singleton無効化状態から算�E
        /// </summary>
        /// <returns>移行進捁E(0.0 = 未開姁E 1.0 = 完亁E</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の篁E��で取征E        /// ServiceLocator使用玁E�� Legacy Singleton無効化状態から算�E
        /// </summary>
        /// <returns>移行進捁E(0.0 = 未開姁E 1.0 = 完亁E</returns>
        /// <summary>
        /// 移行進捗を0.0-1.0の篁E��で取征E        /// ServiceLocator使用玁E�� Legacy Singleton無効化状態から算�E
        /// </summary>
        /// <returns>移行進捁E(0.0 = 未開姁E 1.0 = 完亁E</returns>
        public float GetMigrationProgress()
        {
            // Phase 1: Legacy Singleton無効化チェチE��
            float phase1Progress = FeatureFlags.DisableLegacySingletons ? 0.6f : 0.0f;
            
            // Phase 2: ServiceLocator使用チェチE��
            float phase2Progress = FeatureFlags.UseServiceLocator ? 0.3f : 0.0f;
            
            // Phase 3: Legacy使用状況チェチE���E�使用量が少なぁE��ど進捗が高い�E�E            float phase3Progress = 0.0f;
            if (totalSingletonAccesses == 0)
            {
                // Legacy使用なぁE= 完亁E                phase3Progress = 0.1f;
            }
            else if (totalSingletonAccesses < 10)
            {
                // 低使用釁E                phase3Progress = 0.05f;
            }
            // 高使用量�E場合�Ephase3Progress = 0.0f
            
            float totalProgress = phase1Progress + phase2Progress + phase3Progress;
            
            // チE��チE��ログ出劁E            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Migration Progress: {totalProgress:P1} " +
                               $"(Phase1: {phase1Progress:P1}, Phase2: {phase2Progress:P1}, Phase3: {phase3Progress:P1})");
            }
            
            return totalProgress;
        }

        /// <summary>
        /// 移行�E安�E性を判定すめE        /// 重要なサービスの登録状態とLegacy Singleton使用状況を総合皁E��評価
        /// </summary>
        /// <returns>true=安�E, false=危険, null=判定不�E</returns>
        public bool? IsMigrationSafe()
        {
            try
            {
                // 1. ServiceLocatorの基本動作確誁E                if (!FeatureFlags.UseServiceLocator)
                {
                    if (enableRealTimeLogging)
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] ServiceLocator is disabled - migration safety uncertain");
                    return null; // ServiceLocatorが無効の場合�E判定不�E
                }
                
                // 2. 重要なサービスの登録状態チェチE��
                int criticalServicesCount = 0;
                int registeredServicesCount = 0;
                
                // 重要サービスのチェチE��
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
                
                // サービス登録玁E��算�E
                float serviceRegistrationRatio = criticalServicesCount > 0 ? 
                    (float)registeredServicesCount / criticalServicesCount : 0f;
                
                // 3. Legacy Singleton使用量チェチE��
                bool legacySingletonUsageAcceptable = totalSingletonAccesses < 50; // 50回未満なら安�E篁E��
                
                // 4. 総合判宁E                bool isServicesSafe = serviceRegistrationRatio >= 0.8f; // 80%以上�Eサービスが登録済み
                bool isLegacyUsageSafe = legacySingletonUsageAcceptable;
                bool isFeatureFlagsSafe = FeatureFlags.UseServiceLocator; // ServiceLocatorが有効
                
                bool overallSafety = isServicesSafe && isLegacyUsageSafe && isFeatureFlagsSafe;
                
                // チE��チE��惁E��出劁E                if (enableRealTimeLogging)
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Safety Assessment:");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Services: {registeredServicesCount}/{criticalServicesCount} ({serviceRegistrationRatio:P1}) - {(isServicesSafe ? "安�E" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Legacy Usage: {totalSingletonAccesses} accesses - {(isLegacyUsageSafe ? "安�E" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  FeatureFlags: ServiceLocator={FeatureFlags.UseServiceLocator} - {(isFeatureFlagsSafe ? "安�E" : "危険")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Overall Safety: {(overallSafety ? "✁ESAFE" : "⚠�E�EUNSAFE")}");
                }
                
                return overallSafety;
            }
            catch (System.Exception ex)
            {
                if (enableRealTimeLogging)
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Safety assessment failed: {ex.Message}");
                return null; // 例外発生時は判定不�E
            }
        }

        
        /// <summary>
        /// Singleton使用統計を取征E        /// </summary>
        /// <returns>Singleton使用統計�EチE��クショナリ</returns>
        public Dictionary<Type, SingletonUsageInfo> GetSingletonUsageStats()
        {
            return new Dictionary<Type, SingletonUsageInfo>(usageStats);
        }
        
        /// <summary>
        /// ServiceLocator使用統計を取征E        /// </summary>
        /// <returns>ServiceLocator使用イベント�EリスチE/returns>
        public List<ServiceLocatorUsageEvent> GetServiceLocatorUsageStats()
        {
            return new List<ServiceLocatorUsageEvent>(recentServiceLocatorEvents ?? new List<ServiceLocatorUsageEvent>());
        }
/// <summary>
        /// 簡易版の安�E性チェチE�� (コンチE��ストメニュー用)
        /// </summary>
        [ContextMenu("Check Migration Safety")]
        public void CheckMigrationSafety()
        {
            var safetyResult = IsMigrationSafe();
            
            if (safetyResult == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] ⚠�E�EMigration safety assessment inconclusive");
            }
            else if (safetyResult.Value)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] ✁EMigration is SAFE to proceed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationMonitor] ⚠�E�EMigration is UNSAFE - review issues before proceeding");
            }
        }


/// <summary>
        /// 特定�ESingletonに対する移行推奨事頁E��取征E        /// </summary>
        private string GetMigrationRecommendation(SingletonUsageInfo info)
        {
            if (info.AccessCount > 50)
            {
                return "❁EHigh usage detected - Priority migration recommended";
            }
            else if (info.AccessCount > 10)
            {
                return "⚠�E�E Medium usage - Schedule migration soon";
            }
            else
            {
                return "💡 Low usage - Can be migrated when convenient";
            }
        }
    }
    
    /// <summary>
    /// Singleton使用惁E��を格納するクラス
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