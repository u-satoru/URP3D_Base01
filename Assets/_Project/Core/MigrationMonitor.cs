using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using System;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Singleton→ServiceLocator移行状況の監視システム
    /// Phase 3移行計画 Step 3.2の実装
    /// </summary>
    public class MigrationMonitor : MonoBehaviour 
    {
        [TabGroup("Migration Status", "Current Status")]
        [Header("Service Migration Progress")]
        [SerializeField, ReadOnly] private bool audioServiceMigrated;
        [SerializeField, ReadOnly] private bool spatialServiceMigrated;
        [SerializeField, ReadOnly] private bool effectServiceMigrated;
        [SerializeField, ReadOnly] private bool updateServiceMigrated;
        [SerializeField, ReadOnly] private bool stealthServiceMigrated;
        [SerializeField, ReadOnly] private bool allServicesMigrated;
        
        [TabGroup("Migration Status", "Performance")]
        [Header("Performance Metrics")]
        [SerializeField, ReadOnly] private float singletonCallCount;
        [SerializeField, ReadOnly] private float serviceLocatorCallCount;
        [SerializeField, ReadOnly] private float migrationProgress;
        [SerializeField, ReadOnly] private float performanceRatio; // ServiceLocator/Singleton比率
        
        [TabGroup("Migration Status", "Warnings")]
        [Header("Migration Warnings")]
        [SerializeField, ReadOnly] private List<string> activeSingletonUsages = new List<string>();
        [SerializeField, ReadOnly] private int totalWarningCount;
        [SerializeField, ReadOnly] private string lastWarningTime;
        
        [TabGroup("Migration Status", "History")]
        [Header("Migration History")]
        [SerializeField, ReadOnly] private List<string> migrationEvents = new List<string>();
        [SerializeField, ReadOnly] private string migrationStartTime;
        [SerializeField, ReadOnly] private string lastMigrationEvent;
        
        // 内部統計データ
        private Dictionary<Type, int> singletonUsageCount = new Dictionary<Type, int>();
        private Dictionary<Type, int> serviceLocatorUsageCount = new Dictionary<Type, int>();
        private Dictionary<string, DateTime> lastUsageTime = new Dictionary<string, DateTime>();
        private DateTime monitoringStartTime;
        
        // パフォーマンス測定用
        private Queue<float> recentFrameTimes = new Queue<float>();
        private const int FRAME_HISTORY_SIZE = 60; // 60フレーム履歴
        
        private void Awake()
        {
            monitoringStartTime = DateTime.Now;
            migrationStartTime = monitoringStartTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            LogMigrationEvent("MigrationMonitor initialized");
            
            if (FeatureFlags.EnableDebugLogging)
            {
                    UnityEngine.Debug.Log("[MigrationMonitor] Migration monitoring started");
            }
        }
        
        private void Update() 
        {
            if (FeatureFlags.EnableMigrationMonitoring) 
            {
                MonitorMigrationProgress();
                UpdateMigrationStatus();
                UpdatePerformanceMetrics();
            }
        }
        
        private void MonitorMigrationProgress() 
        {
            // 各サービスの移行状況をチェック
            audioServiceMigrated = CheckAudioServiceMigration();
            spatialServiceMigrated = CheckSpatialServiceMigration();
            effectServiceMigrated = CheckEffectServiceMigration();
            updateServiceMigrated = CheckUpdateServiceMigration();
            stealthServiceMigrated = CheckStealthServiceMigration();
            
            // 全体の移行状況
            int migratedCount = 0;
            if (audioServiceMigrated) migratedCount++;
            if (spatialServiceMigrated) migratedCount++;
            if (effectServiceMigrated) migratedCount++;
            if (updateServiceMigrated) migratedCount++;
            if (stealthServiceMigrated) migratedCount++;
            
            float previousProgress = migrationProgress;
            migrationProgress = (float)migratedCount / 5.0f * 100f;
            allServicesMigrated = migratedCount == 5;
            
            // 進捗変化をログに記録
            if (Math.Abs(migrationProgress - previousProgress) > 0.1f)
            {
                LogMigrationEvent($"Progress updated: {migrationProgress:F1}%");
            }
        }
        
        private bool CheckAudioServiceMigration()
        {
            return FeatureFlags.MigrateAudioManager && 
                   FeatureFlags.UseServiceLocator &&
                   ServiceLocator.GetServiceCount() > 0;
        }
        
        private bool CheckSpatialServiceMigration()
        {
            return FeatureFlags.MigrateSpatialAudioManager && 
                   FeatureFlags.UseServiceLocator;
        }
        
        private bool CheckEffectServiceMigration()
        {
            return FeatureFlags.MigrateEffectManager && 
                   FeatureFlags.UseServiceLocator;
        }
        
        private bool CheckUpdateServiceMigration()
        {
            return FeatureFlags.MigrateAudioUpdateCoordinator && 
                   FeatureFlags.UseServiceLocator;
        }
        
        private bool CheckStealthServiceMigration()
        {
            return FeatureFlags.MigrateStealthAudioCoordinator && 
                   FeatureFlags.UseServiceLocator;
        }
        
        private void UpdateMigrationStatus()
        {
            // アクティブなSingleton使用箇所のリストを更新
            activeSingletonUsages.Clear();
            
            if (!audioServiceMigrated) activeSingletonUsages.Add("AudioManager.Instance");
            if (!spatialServiceMigrated) activeSingletonUsages.Add("SpatialAudioManager.Instance");
            if (!effectServiceMigrated) activeSingletonUsages.Add("EffectManager.Instance");
            if (!updateServiceMigrated) activeSingletonUsages.Add("AudioUpdateCoordinator.Instance");
            if (!stealthServiceMigrated) activeSingletonUsages.Add("StealthAudioCoordinator.Instance");
            
            // パフォーマンス比率の計算
            if (singletonCallCount > 0)
            {
                performanceRatio = serviceLocatorCallCount / singletonCallCount;
            }
            else
            {
                performanceRatio = serviceLocatorCallCount > 0 ? float.PositiveInfinity : 0f;
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            // フレーム時間の監視
            recentFrameTimes.Enqueue(Time.unscaledDeltaTime);
            
            if (recentFrameTimes.Count > FRAME_HISTORY_SIZE)
            {
                recentFrameTimes.Dequeue();
            }
            
            // パフォーマンス異常の検出
            if (recentFrameTimes.Count >= FRAME_HISTORY_SIZE)
            {
                float averageFrameTime = GetAverageFrameTime();
                if (averageFrameTime > 0.033f) // 30FPS閾値
                {
                    if (FeatureFlags.EnablePerformanceMeasurement)
                    {
                        LogPerformanceWarning($"Frame time spike detected: {averageFrameTime * 1000:F2}ms");
                    }
                }
            }
        }
        
        private float GetAverageFrameTime()
        {
            if (recentFrameTimes.Count == 0) return 0f;
            
            float total = 0f;
            foreach (float frameTime in recentFrameTimes)
            {
                total += frameTime;
            }
            return total / recentFrameTimes.Count;
        }
        
        /// <summary>
        /// Singleton使用をログに記録
        /// </summary>
        public void LogSingletonUsage(Type singletonType, string location)
        {
            if (singletonType == null) return;
            
            // 使用回数の記録
            if (!singletonUsageCount.ContainsKey(singletonType))
            {
                singletonUsageCount[singletonType] = 0;
            }
            singletonUsageCount[singletonType]++;
            singletonCallCount++;
            
            // 最終使用時刻の記録
            string key = $"{singletonType.Name}@{location}";
            lastUsageTime[key] = DateTime.Now;
            
            // 警告の記録
            if (FeatureFlags.EnableDebugLogging)
            {
                string message = $"[MIGRATION] Singleton usage: {singletonType.Name} at {location}";
                UnityEngine.Debug.LogWarning(message);
                
                totalWarningCount++;
                lastWarningTime = DateTime.Now.ToString("HH:mm:ss");
                
                LogMigrationEvent($"Singleton usage: {singletonType.Name}");
            }
        }
        
        /// <summary>
        /// ServiceLocator使用をログに記録
        /// </summary>
        public void LogServiceLocatorUsage(Type serviceType, string location = "Unknown")
        {
            if (serviceType == null) return;
            
            // 使用回数の記録
            if (!serviceLocatorUsageCount.ContainsKey(serviceType))
            {
                serviceLocatorUsageCount[serviceType] = 0;
            }
            serviceLocatorUsageCount[serviceType]++;
            serviceLocatorCallCount++;
            
            // 最終使用時刻の記録
            string key = $"{serviceType.Name}@{location}";
            lastUsageTime[key] = DateTime.Now;
            
            if (FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ServiceLocator] Service usage: {serviceType.Name} at {location}");
            }
        }
        
        /// <summary>
        /// 移行イベントをログに記録
        /// </summary>
        private void LogMigrationEvent(string eventDescription)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {eventDescription}";
            
            migrationEvents.Add(logEntry);
            lastMigrationEvent = logEntry;
            
            // 最新20件まで保持
            if (migrationEvents.Count > 20)
            {
                migrationEvents.RemoveAt(0);
            }
            
            if (FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[MigrationMonitor] {eventDescription}");
            }
        }
        
        /// <summary>
        /// パフォーマンス警告をログに記録
        /// </summary>
        private void LogPerformanceWarning(string warning)
        {
            string message = $"[PERFORMANCE] {warning}";
            UnityEngine.Debug.LogWarning(message);
            LogMigrationEvent($"Performance warning: {warning}");
            
            totalWarningCount++;
            lastWarningTime = DateTime.Now.ToString("HH:mm:ss");
        }
        
        #region Public API
        
        /// <summary>
        /// 現在の移行進捗を取得
        /// </summary>
        public float GetMigrationProgress()
        {
            return migrationProgress;
        }
        
        /// <summary>
        /// 特定サービスの移行状況を取得
        /// </summary>
        public bool IsServiceMigrated(string serviceName)
        {
            return serviceName.ToLower() switch
            {
                "audio" => audioServiceMigrated,
                "spatial" => spatialServiceMigrated,
                "effect" => effectServiceMigrated,
                "update" => updateServiceMigrated,
                "stealth" => stealthServiceMigrated,
                _ => false
            };
        }
        
        /// <summary>
        /// Singleton使用統計を取得
        /// </summary>
        public Dictionary<Type, int> GetSingletonUsageStats()
        {
            return new Dictionary<Type, int>(singletonUsageCount);
        }
        
        /// <summary>
        /// ServiceLocator使用統計を取得
        /// </summary>
        public Dictionary<Type, int> GetServiceLocatorUsageStats()
        {
            return new Dictionary<Type, int>(serviceLocatorUsageCount);
        }
        
        /// <summary>
        /// 移行の安全性を評価
        /// </summary>
        public bool IsMigrationSafe()
        {
            // 基本的な安全性チェック
            if (!FeatureFlags.UseServiceLocator) return false;
            if (!FeatureFlags.EnableMigrationMonitoring) return false;
            
            // パフォーマンス安全性チェック
            float avgFrameTime = GetAverageFrameTime();
            if (avgFrameTime > 0.040f) return false; // 25FPS以下は危険
            
            // Singleton使用頻度チェック
            float recentSingletonUsage = GetRecentSingletonUsageRate();
            if (recentSingletonUsage > 0.5f) return false; // 50%以上Singleton使用は危険
            
            return true;
        }
        
        private float GetRecentSingletonUsageRate()
        {
            float totalCalls = singletonCallCount + serviceLocatorCallCount;
            if (totalCalls == 0) return 0f;
            return singletonCallCount / totalCalls;
        }
        
        #endregion
        
        #region Editor Actions
        
        [TabGroup("Migration Status", "Actions")]
        [Button("Force Migration Check")]
        private void ForceMigrationCheck()
        {
            MonitorMigrationProgress();
            UpdateMigrationStatus();
            
            UnityEngine.Debug.Log($"[MigrationMonitor] Progress: {migrationProgress:F1}% - " +
                      $"Audio: {audioServiceMigrated}, Spatial: {spatialServiceMigrated}, " +
                      $"Effect: {effectServiceMigrated}, Update: {updateServiceMigrated}, " +
                      $"Stealth: {stealthServiceMigrated}");
            
            LogMigrationEvent($"Manual check executed - Progress: {migrationProgress:F1}%");
        }
        
        [Button("Reset Counters")]
        private void ResetCounters()
        {
            singletonUsageCount.Clear();
            serviceLocatorUsageCount.Clear();
            singletonCallCount = 0;
            serviceLocatorCallCount = 0;
            totalWarningCount = 0;
            activeSingletonUsages.Clear();
            recentFrameTimes.Clear();
            
            LogMigrationEvent("Counters reset by user");
            UnityEngine.Debug.Log("[MigrationMonitor] All counters reset");
        }
        
        [Button("Generate Migration Report")]
        private void GenerateMigrationReport()
        {
            var report = $@"
=== MIGRATION MONITOR REPORT ===
Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Monitoring Start: {migrationStartTime}
Monitoring Duration: {(DateTime.Now - monitoringStartTime).TotalHours:F1} hours

MIGRATION PROGRESS:
- Overall Progress: {migrationProgress:F1}%
- Audio Service: {(audioServiceMigrated ? "✅" : "❌")}
- Spatial Service: {(spatialServiceMigrated ? "✅" : "❌")}
- Effect Service: {(effectServiceMigrated ? "✅" : "❌")}
- Update Service: {(updateServiceMigrated ? "✅" : "❌")}
- Stealth Service: {(stealthServiceMigrated ? "✅" : "❌")}

USAGE STATISTICS:
- Singleton Calls: {singletonCallCount}
- ServiceLocator Calls: {serviceLocatorCallCount}
- Performance Ratio: {performanceRatio:F2}
- Total Warnings: {totalWarningCount}

SAFETY STATUS:
- Migration Safe: {(IsMigrationSafe() ? "✅ YES" : "❌ NO")}
- Average Frame Time: {GetAverageFrameTime() * 1000:F2}ms
- Recent Singleton Usage: {GetRecentSingletonUsageRate() * 100:F1}%

ACTIVE SINGLETON USAGES:
{string.Join("\n", activeSingletonUsages.ConvertAll(s => "- " + s))}

RECENT EVENTS:
{string.Join("\n", migrationEvents.ConvertAll(s => "  " + s))}
=================================";
            
            UnityEngine.Debug.Log(report);
            LogMigrationEvent("Migration report generated");
        }
        
        [Button("Test Emergency Rollback")]
        private void TestEmergencyRollback()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("Emergency Rollback Test", 
                "This will test the emergency rollback system. Continue?", "Yes", "No"))
            {
                LogMigrationEvent("Emergency rollback test initiated");
                FeatureFlags.EmergencyRollback();
                LogMigrationEvent("Emergency rollback test completed");
            }
#else
            LogMigrationEvent("Emergency rollback test (editor only)");
            FeatureFlags.EmergencyRollback();
#endif
        }
        
        #endregion
        
        #region Runtime Diagnostics
        
        private void OnValidate()
        {
            // エディタでの値変更時の検証
            if (Application.isPlaying && FeatureFlags.EnableMigrationMonitoring)
            {
                MonitorMigrationProgress();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                LogMigrationEvent("Application paused");
            }
            else
            {
                LogMigrationEvent("Application resumed");
            }
        }
        
        private void OnDestroy()
        {
            float totalHours = (float)(DateTime.Now - monitoringStartTime).TotalHours;
            LogMigrationEvent($"Monitor destroyed after {totalHours:F1} hours");
            
            if (FeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[MigrationMonitor] Monitoring session ended. Duration: {totalHours:F1} hours");
            }
        }
        
        #endregion
    }
}