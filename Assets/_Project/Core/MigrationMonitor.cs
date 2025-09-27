using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using asterivo.Unity60.Core;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using System;

namespace asterivo.Unity60.Core
{
    /// <summary>
    /// Singleton→ServiceLocator移行統合監視システム
    ///
    /// Unity 6における3層アーキテクチャ移行プロセスを包括的に監視・分析する
    /// Phase 3移行計画の中核システムです。リアルタイム進捗追跡、パフォーマンス測定、
    /// 安全性評価、緊急時ロールバック支援を統合的に提供します。
    ///
    /// 【監視範囲】
    /// - 5つの主要サービス移行状況（Audio/Spatial/Effect/Update/Stealth）
    /// - Singleton使用頻度とServiceLocator使用頻度の比較分析
    /// - フレームタイム監視による性能劣化検出
    /// - 移行プロセス中の警告・エラー事象追跡
    ///
    /// 【アーキテクチャ統合】
    /// - FeatureFlagsとの密接連携による動的制御
    /// - ServiceLocatorとの統計データ交換
    /// - Odin Inspector TabGroupによる分類UI表示
    /// - エディタ・ランタイム両対応の診断機能
    ///
    /// 【安全性保証】
    /// - IsMigrationSafe()による移行可否の自動判定
    /// - パフォーマンス閾値（25FPS）による安全性確保
    /// - Singleton使用率50%未満での移行推奨
    /// - 緊急時ロールバック機能との統合
    ///
    /// 【データ収集戦略】
    /// - 60フレーム履歴によるパフォーマンス移動平均
    /// - 使用統計のType別詳細分類
    /// - 最新20件の移行イベント履歴保持
    /// - タイムスタンプ付きトレーサビリティ
    ///
    /// 【プロダクション対応】
    /// - 本番環境での継続監視とレポート生成
    /// - A/Bテスト用の移行率調整支援
    /// - 障害時の詳細診断情報提供
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
        [SerializeField, ReadOnly] private float performanceRatio; // ServiceLocator/Singleton ratio

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

        // Internal statistics data
        private Dictionary<Type, int> singletonUsageCount = new Dictionary<Type, int>();
        private Dictionary<Type, int> serviceLocatorUsageCount = new Dictionary<Type, int>();
        private Dictionary<string, DateTime> lastUsageTime = new Dictionary<string, DateTime>();
        private DateTime monitoringStartTime;

        // Performance measurement
        private Queue<float> recentFrameTimes = new Queue<float>();
        private const int FRAME_HISTORY_SIZE = 60; // 60 frame history

        private void Awake()
        {
            monitoringStartTime = DateTime.Now;
            migrationStartTime = monitoringStartTime.ToString("yyyy-MM-dd HH:mm:ss");

            LogMigrationEvent("MigrationMonitor initialized");

            if (FeatureFlags.EnableDebugLogging)
            {
                Debug.Log("[MigrationMonitor] Migration monitoring started");
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
            // Check migration status of each service
            audioServiceMigrated = CheckAudioServiceMigration();
            spatialServiceMigrated = CheckSpatialServiceMigration();
            effectServiceMigrated = CheckEffectServiceMigration();
            updateServiceMigrated = CheckUpdateServiceMigration();
            stealthServiceMigrated = CheckStealthServiceMigration();

            // Overall migration status
            int migratedCount = 0;
            if (audioServiceMigrated) migratedCount++;
            if (spatialServiceMigrated) migratedCount++;
            if (effectServiceMigrated) migratedCount++;
            if (updateServiceMigrated) migratedCount++;
            if (stealthServiceMigrated) migratedCount++;

            float previousProgress = migrationProgress;
            migrationProgress = (float)migratedCount / 5.0f * 100f;
            allServicesMigrated = migratedCount == 5;

            // Log progress changes
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
            // Update list of active Singleton usages
            activeSingletonUsages.Clear();

            if (!audioServiceMigrated) activeSingletonUsages.Add("AudioManager.Instance");
            if (!spatialServiceMigrated) activeSingletonUsages.Add("SpatialAudioManager.Instance");
            if (!effectServiceMigrated) activeSingletonUsages.Add("EffectManager.Instance");
            if (!updateServiceMigrated) activeSingletonUsages.Add("AudioUpdateCoordinator.Instance");
            if (!stealthServiceMigrated) activeSingletonUsages.Add("StealthAudioCoordinator.Instance");

            // Calculate performance ratio
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
            // Monitor frame times
            recentFrameTimes.Enqueue(Time.unscaledDeltaTime);

            if (recentFrameTimes.Count > FRAME_HISTORY_SIZE)
            {
                recentFrameTimes.Dequeue();
            }

            // Check for performance anomalies
            if (recentFrameTimes.Count >= FRAME_HISTORY_SIZE)
            {
                float averageFrameTime = GetAverageFrameTime();
                if (averageFrameTime > 0.033f) // 30FPS threshold
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
        /// Singleton使用状況ログ・警告システム
        ///
        /// レガシーSingletonパターンの使用を検出・記録し、移行進捗の阻害要因を
        /// 特定します。使用頻度統計、位置情報、タイムスタンプを包括的に記録し、
        /// 移行計画の最適化に必要なデータを提供します。
        ///
        /// 【記録データ】
        /// - singletonUsageCount: Type別使用回数統計
        /// - singletonCallCount: 総Singleton呼び出し回数
        /// - lastUsageTime: 最終使用時刻（Type@Location形式）
        /// - 警告履歴: totalWarningCount、lastWarningTime
        ///
        /// 【警告トリガー】
        /// - FeatureFlags.EnableDebugLoggingによる制御
        /// - Debug.LogWarning()による即座警告表示
        /// - LogMigrationEvent()による履歴記録
        ///
        /// 【統計活用】
        /// - パフォーマンス比率計算（performanceRatio）
        /// - 移行安全性評価（IsMigrationSafe）
        /// - レポート生成での詳細分析
        ///
        /// 【呼び出し元】
        /// レガシーManager.Instanceアクセス箇所から手動または自動呼び出し
        /// </summary>
        /// <param name="singletonType">使用されたSingletonの型情報</param>
        /// <param name="location">使用箇所の識別子（クラス名.メソッド名等）</param>
        public void LogSingletonUsage(Type singletonType, string location)
        {
            if (singletonType == null) return;

            // Record usage count
            if (!singletonUsageCount.ContainsKey(singletonType))
            {
                singletonUsageCount[singletonType] = 0;
            }
            singletonUsageCount[singletonType]++;
            singletonCallCount++;

            // Record last usage time
            string key = $"{singletonType.Name}@{location}";
            lastUsageTime[key] = DateTime.Now;

            // Log warning
            if (FeatureFlags.EnableDebugLogging)
            {
                string message = $"[MIGRATION] Singleton usage: {singletonType.Name} at {location}";
                Debug.LogWarning(message);

                totalWarningCount++;
                lastWarningTime = DateTime.Now.ToString("HH:mm:ss");

                LogMigrationEvent($"Singleton usage: {singletonType.Name}");
            }
        }

        /// <summary>
        /// Log ServiceLocator usage
        /// </summary>
        public void LogServiceLocatorUsage(Type serviceType, string location = "Unknown")
        {
            if (serviceType == null) return;

            // Record usage count
            if (!serviceLocatorUsageCount.ContainsKey(serviceType))
            {
                serviceLocatorUsageCount[serviceType] = 0;
            }
            serviceLocatorUsageCount[serviceType]++;
            serviceLocatorCallCount++;

            // Record last usage time
            string key = $"{serviceType.Name}@{location}";
            lastUsageTime[key] = DateTime.Now;

            if (FeatureFlags.EnableDebugLogging)
            {
                Debug.Log($"[ServiceLocator] Service usage: {serviceType.Name} at {location}");
            }
        }

        /// <summary>
        /// Log migration event
        /// </summary>
        private void LogMigrationEvent(string eventDescription)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {eventDescription}";

            migrationEvents.Add(logEntry);
            lastMigrationEvent = logEntry;

            // Keep only latest 20 events
            if (migrationEvents.Count > 20)
            {
                migrationEvents.RemoveAt(0);
            }

            if (FeatureFlags.EnableDebugLogging)
            {
                Debug.Log($"[MigrationMonitor] {eventDescription}");
            }
        }

        /// <summary>
        /// Log performance warning
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
        /// Get current migration progress
        /// </summary>
        public float GetMigrationProgress()
        {
            return migrationProgress;
        }

        /// <summary>
        /// Get migration status of specific service
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
        /// Get Singleton usage statistics
        /// </summary>
        public Dictionary<Type, int> GetSingletonUsageStats()
        {
            return new Dictionary<Type, int>(singletonUsageCount);
        }

        /// <summary>
        /// Get ServiceLocator usage statistics
        /// </summary>
        public Dictionary<Type, int> GetServiceLocatorUsageStats()
        {
            return new Dictionary<Type, int>(serviceLocatorUsageCount);
        }

        /// <summary>
        /// 移行安全性総合評価システム
        ///
        /// ServiceLocator移行の実行可否を多角的に評価し、安全な移行タイミングを
        /// 判定します。基盤システム状態、パフォーマンス指標、使用統計を総合的に
        /// 分析し、移行失敗リスクを最小化するための意思決定支援を提供します。
        ///
        /// 【評価項目】
        /// 1. 基盤システム準備状況
        ///    - FeatureFlags.UseServiceLocator: ServiceLocator基盤の有効性
        ///    - FeatureFlags.EnableMigrationMonitoring: 監視システムの稼働状況
        ///
        /// 2. パフォーマンス安全性
        ///    - 平均フレームタイム閾値: 40ms以下（25FPS以上）
        ///    - フレーム安定性: 60フレーム移動平均による評価
        ///
        /// 3. 使用パターン分析
        ///    - Singleton使用率: 50%未満を安全水準と設定
        ///    - ServiceLocator移行率: 高い値ほど安全
        ///
        /// 【判定ロジック】
        /// 全評価項目がtrueの場合のみ安全と判定（AND条件）
        /// 1つでもfalseの場合は移行延期を推奨
        ///
        /// 【活用場面】
        /// - 自動移行フェーズ遷移の可否判定
        /// - GenerateMigrationReport()での安全性表示
        /// - 緊急時ロールバック要否の判断材料
        /// </summary>
        /// <returns>移行実行が安全な場合true、リスクが高い場合false</returns>
        public bool IsMigrationSafe()
        {
            // Basic safety checks
            if (!FeatureFlags.UseServiceLocator) return false;
            if (!FeatureFlags.EnableMigrationMonitoring) return false;

            // Performance safety check
            float avgFrameTime = GetAverageFrameTime();
            if (avgFrameTime > 0.040f) return false; // Below 25FPS is dangerous

            // Singleton usage frequency check
            float recentSingletonUsage = GetRecentSingletonUsageRate();
            if (recentSingletonUsage > 0.5f) return false; // Over 50% Singleton usage is dangerous

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

            Debug.Log($"[MigrationMonitor] Progress: {migrationProgress:F1}% - " +
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
            Debug.Log("[MigrationMonitor] All counters reset");
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
- Audio Service: {(audioServiceMigrated ? "✓" : "✗")}
- Spatial Service: {(spatialServiceMigrated ? "✓" : "✗")}
- Effect Service: {(effectServiceMigrated ? "✓" : "✗")}
- Update Service: {(updateServiceMigrated ? "✓" : "✗")}
- Stealth Service: {(stealthServiceMigrated ? "✓" : "✗")}

USAGE STATISTICS:
- Singleton Calls: {singletonCallCount}
- ServiceLocator Calls: {serviceLocatorCallCount}
- Performance Ratio: {performanceRatio:F2}
- Total Warnings: {totalWarningCount}

SAFETY STATUS:
- Migration Safe: {(IsMigrationSafe() ? "✓ YES" : "✗ NO")}
- Average Frame Time: {GetAverageFrameTime() * 1000:F2}ms
- Recent Singleton Usage: {GetRecentSingletonUsageRate() * 100:F1}%

ACTIVE SINGLETON USAGES:
{string.Join("\n", activeSingletonUsages.ConvertAll(s => "- " + s))}

RECENT EVENTS:
{string.Join("\n", migrationEvents.ConvertAll(s => "  " + s))}
=================================";

            Debug.Log(report);
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
            // Check on value changes in editor
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
                Debug.Log($"[MigrationMonitor] Monitoring session ended. Duration: {totalHours:F1} hours");
            }
        }

        #endregion
    }
}