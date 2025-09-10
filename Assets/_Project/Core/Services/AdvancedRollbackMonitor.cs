using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Debug;

namespace _Project.Core.Services
{
    /// <summary>
    /// Step 3.12: 高度な緊急時ロールバック監視システム
    /// システム状態の継続的監視、予測的問題検出、自動対応機能
    /// </summary>
    public class AdvancedRollbackMonitor : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableContinuousMonitoring = true;
        [SerializeField] private bool enablePredictiveAnalysis = true;
        [SerializeField] private bool enableAutoRecovery = true;
        [SerializeField] private float monitoringInterval = 5f; // 5秒ごと
        [SerializeField] private float healthCheckInterval = 10f; // 10秒ごと
        
        [Header("Thresholds")]
        [SerializeField] private int criticalHealthThreshold = 30;
        [SerializeField] private int warningHealthThreshold = 60;
        [SerializeField] private int maxConsecutiveFailures = 3;
        [SerializeField] private float performanceThreshold = 0.5f; // 50%性能低下で警告
        
        [Header("Current Status")]
        [SerializeField] private SystemHealthLevel currentHealthLevel = SystemHealthLevel.Unknown;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private float lastHealthScore = 100f;
        [SerializeField] private string lastIssueDetected = "";
        
        // 監視データ
        private List<HealthSnapshot> healthHistory = new List<HealthSnapshot>();
        private Dictionary<string, ServiceHealthMetrics> serviceMetrics = new Dictionary<string, ServiceHealthMetrics>();
        private Queue<SystemIssue> recentIssues = new Queue<SystemIssue>();
        
        private const int MAX_HEALTH_HISTORY = 100;
        private const int MAX_RECENT_ISSUES = 20;
        
        private void Start()
        {
            InitializeMonitoring();
            
            if (enableContinuousMonitoring)
            {
                InvokeRepeating(nameof(PerformSystemCheck), monitoringInterval, monitoringInterval);
                InvokeRepeating(nameof(PerformHealthAnalysis), healthCheckInterval, healthCheckInterval);
            }
            
            EventLogger.Log("[AdvancedRollbackMonitor] Advanced monitoring system started");
        }
        
        private void OnDestroy()
        {
            SaveMonitoringData();
        }
        
        /// <summary>
        /// 監視システムの初期化
        /// </summary>
        private void InitializeMonitoring()
        {
            LoadMonitoringData();
            RegisterServiceMetrics();
            PerformInitialHealthCheck();
            
            EventLogger.Log("[AdvancedRollbackMonitor] Monitoring system initialized");
        }
        
        /// <summary>
        /// サービスメトリクスの登録
        /// </summary>
        private void RegisterServiceMetrics()
        {
            string[] services = {
                "AudioService",
                "SpatialAudioService", 
                "StealthAudioService",
                "EffectService",
                "AudioUpdateService"
            };
            
            foreach (var service in services)
            {
                serviceMetrics[service] = new ServiceHealthMetrics
                {
                    ServiceName = service,
                    LastCheckTime = DateTime.Now,
                    IsHealthy = true,
                    ResponseTime = 0f,
                    ErrorCount = 0,
                    SuccessRate = 100f
                };
            }
        }
        
        /// <summary>
        /// システムチェックの実行
        /// </summary>
        private void PerformSystemCheck()
        {
            if (!enableContinuousMonitoring) return;
            
            try
            {
                // 基本的なシステム健全性チェック
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                UpdateHealthHistory(healthStatus);
                
                // サービス別詳細チェック
                CheckIndividualServices();
                
                // パフォーマンスメトリクスチェック
                CheckPerformanceMetrics();
                
                // 予測分析の実行
                if (enablePredictiveAnalysis)
                {
                    PerformPredictiveAnalysis();
                }
                
                // 問題検出と対応
                DetectAndHandleIssues(healthStatus);
                
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] System check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 健全性履歴の更新
        /// </summary>
        private void UpdateHealthHistory(SystemHealthStatus healthStatus)
        {
            var snapshot = new HealthSnapshot
            {
                Timestamp = DateTime.Now,
                HealthScore = healthStatus.HealthScore,
                IsHealthy = healthStatus.IsHealthy,
                IssueCount = healthStatus.Issues.Count,
                HasInconsistentConfiguration = healthStatus.HasInconsistentConfiguration
            };
            
            healthHistory.Add(snapshot);
            if (healthHistory.Count > MAX_HEALTH_HISTORY)
            {
                healthHistory.RemoveAt(0);
            }
            
            // 現在の健全性レベルを更新
            UpdateCurrentHealthLevel(healthStatus.HealthScore);
        }
        
        /// <summary>
        /// 現在の健全性レベルを更新
        /// </summary>
        private void UpdateCurrentHealthLevel(int healthScore)
        {
            SystemHealthLevel newLevel;
            
            if (healthScore >= 80)
                newLevel = SystemHealthLevel.Excellent;
            else if (healthScore >= warningHealthThreshold)
                newLevel = SystemHealthLevel.Good;
            else if (healthScore >= criticalHealthThreshold)
                newLevel = SystemHealthLevel.Warning;
            else
                newLevel = SystemHealthLevel.Critical;
            
            if (newLevel != currentHealthLevel)
            {
                var previousLevel = currentHealthLevel;
                currentHealthLevel = newLevel;
                OnHealthLevelChanged(previousLevel, newLevel, healthScore);
            }
            
            lastHealthScore = healthScore;
        }
        
        /// <summary>
        /// 健全性レベル変更時の処理
        /// </summary>
        private void OnHealthLevelChanged(SystemHealthLevel previous, SystemHealthLevel current, int score)
        {
            EventLogger.Log($"[AdvancedRollbackMonitor] Health level changed: {previous} -> {current} (Score: {score})");
            
            switch (current)
            {
                case SystemHealthLevel.Critical:
                    HandleCriticalHealthLevel(score);
                    break;
                case SystemHealthLevel.Warning:
                    HandleWarningHealthLevel(score);
                    break;
                case SystemHealthLevel.Good:
                case SystemHealthLevel.Excellent:
                    if (previous == SystemHealthLevel.Warning || previous == SystemHealthLevel.Critical)
                    {
                        HandleHealthRecovery(previous, current);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 個別サービスのチェック
        /// </summary>
        private void CheckIndividualServices()
        {
            foreach (var kvp in serviceMetrics.ToArray())
            {
                var serviceName = kvp.Key;
                var metrics = kvp.Value;
                
                try
                {
                    bool isHealthy = CheckServiceHealth(serviceName);
                    float responseTime = MeasureServiceResponseTime(serviceName);
                    
                    metrics.LastCheckTime = DateTime.Now;
                    metrics.IsHealthy = isHealthy;
                    metrics.ResponseTime = responseTime;
                    
                    if (isHealthy)
                    {
                        metrics.SuccessRate = Mathf.Min(100f, metrics.SuccessRate + 1f);
                    }
                    else
                    {
                        metrics.ErrorCount++;
                        metrics.SuccessRate = Mathf.Max(0f, metrics.SuccessRate - 5f);
                    }
                    
                    serviceMetrics[serviceName] = metrics;
                }
                catch (Exception ex)
                {
                    EventLogger.LogError($"[AdvancedRollbackMonitor] Service check failed for {serviceName}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// サービス健全性の個別チェック
        /// </summary>
        private bool CheckServiceHealth(string serviceName)
        {
            switch (serviceName)
            {
                case "AudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>() != null;
                case "SpatialAudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>() != null;
                case "StealthAudioService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IStealthAudioService>() != null;
                case "EffectService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IEffectService>() != null;
                case "AudioUpdateService":
                    return ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioUpdateService>() != null;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// サービス応答時間の測定
        /// </summary>
        private float MeasureServiceResponseTime(string serviceName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // 簡単な応答時間測定（実際のサービス呼び出し）
                CheckServiceHealth(serviceName);
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                stopwatch.Stop();
                return -1f; // エラーの場合
            }
        }
        
        /// <summary>
        /// パフォーマンスメトリクスのチェック
        /// </summary>
        private void CheckPerformanceMetrics()
        {
            try
            {
                float frameTime = Time.deltaTime;
                float fps = 1f / frameTime;
                float targetFps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60f;
                
                float performanceRatio = fps / targetFps;
                
                if (performanceRatio < performanceThreshold)
                {
                    RecordIssue($"Performance degradation detected: {performanceRatio:P1} of target FPS", 
                               IssueType.Performance, IssueSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] Performance check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 予測分析の実行
        /// </summary>
        private void PerformPredictiveAnalysis()
        {
            if (healthHistory.Count < 5) return; // 最低5回のデータが必要
            
            try
            {
                // 健全性スコアの傾向分析
                AnalyzeHealthTrend();
                
                // エラー発生パターンの分析
                AnalyzeErrorPatterns();
                
                // サービス品質の劣化予測
                PredictServiceDegradation();
                
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] Predictive analysis failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 健全性傾向の分析
        /// </summary>
        private void AnalyzeHealthTrend()
        {
            if (healthHistory.Count < 10) return;
            
            var recent10 = healthHistory.GetRange(healthHistory.Count - 10, 10);
            float avgRecent = 0f;
            float avgOlder = 0f;
            
            for (int i = 0; i < 5; i++)
            {
                avgOlder += recent10[i].HealthScore;
                avgRecent += recent10[i + 5].HealthScore;
            }
            
            avgOlder /= 5f;
            avgRecent /= 5f;
            
            float trendChange = avgRecent - avgOlder;
            
            if (trendChange < -15f) // 15点以上の悪化
            {
                RecordIssue($"Negative health trend detected: {trendChange:F1} point decline", 
                           IssueType.HealthTrend, IssueSeverity.Warning);
            }
        }
        
        /// <summary>
        /// エラーパターンの分析
        /// </summary>
        private void AnalyzeErrorPatterns()
        {
            var recentIssuesList = recentIssues.ToArray();
            if (recentIssuesList.Length < 3) return;
            
            // 同種のエラーが短期間に複数発生している場合
            var issueGroups = new Dictionary<string, int>();
            var cutoffTime = DateTime.Now.AddMinutes(-10); // 過去10分間
            
            foreach (var issue in recentIssuesList)
            {
                if (issue.Timestamp > cutoffTime)
                {
                    if (!issueGroups.ContainsKey(issue.Description))
                        issueGroups[issue.Description] = 0;
                    issueGroups[issue.Description]++;
                }
            }
            
            foreach (var kvp in issueGroups)
            {
                if (kvp.Value >= 3) // 同じ問題が3回以上
                {
                    RecordIssue($"Recurring issue pattern detected: '{kvp.Key}' occurred {kvp.Value} times", 
                               IssueType.RecurringError, IssueSeverity.Error);
                }
            }
        }
        
        /// <summary>
        /// サービス品質劣化の予測
        /// </summary>
        private void PredictServiceDegradation()
        {
            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                
                if (metrics.SuccessRate < 90f && metrics.ErrorCount > 5)
                {
                    RecordIssue($"Service quality degradation predicted for {metrics.ServiceName}: " +
                               $"Success rate {metrics.SuccessRate:F1}%, Errors: {metrics.ErrorCount}", 
                               IssueType.ServiceDegradation, IssueSeverity.Warning);
                }
            }
        }
        
        /// <summary>
        /// 問題の検出と対応
        /// </summary>
        private void DetectAndHandleIssues(SystemHealthStatus healthStatus)
        {
            // 連続失敗カウンターの更新
            if (!healthStatus.IsHealthy)
            {
                consecutiveFailures++;
            }
            else
            {
                consecutiveFailures = 0;
            }
            
            // 緊急事態の検出
            if (consecutiveFailures >= maxConsecutiveFailures)
            {
                HandleEmergencyCondition($"System failed {consecutiveFailures} consecutive health checks");
            }
            
            // 設定矛盾の検出
            if (healthStatus.HasInconsistentConfiguration)
            {
                HandleConfigurationInconsistency(healthStatus.Issues);
            }
        }
        
        /// <summary>
        /// 緊急事態の対応
        /// </summary>
        private void HandleEmergencyCondition(string reason)
        {
            EventLogger.LogError($"[AdvancedRollbackMonitor] EMERGENCY CONDITION: {reason}");
            
            if (enableAutoRecovery)
            {
                EventLogger.LogError("[AdvancedRollbackMonitor] Attempting automatic recovery...");
                
                // 段階的な回復を試行
                if (TryGradualRecovery())
                {
                    EventLogger.Log("[AdvancedRollbackMonitor] Gradual recovery succeeded");
                    consecutiveFailures = 0;
                }
                else
                {
                    EventLogger.LogError("[AdvancedRollbackMonitor] Gradual recovery failed, executing emergency rollback");
                    EmergencyRollback.ExecuteEmergencyRollback($"Auto rollback triggered: {reason}");
                }
            }
            else
            {
                EventLogger.LogError("[AdvancedRollbackMonitor] Auto recovery disabled, manual intervention required");
                EmergencyRollback.SetEmergencyFlag($"Emergency condition detected: {reason}");
            }
        }
        
        /// <summary>
        /// 段階的回復の試行
        /// </summary>
        private bool TryGradualRecovery()
        {
            try
            {
                EventLogger.Log("[AdvancedRollbackMonitor] Attempting gradual recovery...");
                
                // Step 1: 最新のサービス設定を無効化
                if (FeatureFlags.UseNewStealthService)
                {
                    FeatureFlags.UseNewStealthService = false;
                    EventLogger.Log("[AdvancedRollbackMonitor] Disabled StealthService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 2: Spatial Audio設定を無効化
                if (FeatureFlags.UseNewSpatialService)
                {
                    FeatureFlags.UseNewSpatialService = false;
                    EventLogger.Log("[AdvancedRollbackMonitor] Disabled SpatialService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 3: 監視機能を一時停止
                if (FeatureFlags.EnableMigrationMonitoring)
                {
                    FeatureFlags.EnableMigrationMonitoring = false;
                    EventLogger.Log("[AdvancedRollbackMonitor] Disabled migration monitoring");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] Gradual recovery failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// システム健全性の改善を確認
        /// </summary>
        private bool CheckSystemHealthImprovement()
        {
            System.Threading.Thread.Sleep(1000); // 1秒待機
            
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            return healthStatus.HealthScore > criticalHealthThreshold;
        }
        
        /// <summary>
        /// 設定矛盾の対応
        /// </summary>
        private void HandleConfigurationInconsistency(List<string> issues)
        {
            foreach (var issue in issues)
            {
                RecordIssue($"Configuration inconsistency: {issue}", 
                           IssueType.Configuration, IssueSeverity.Warning);
            }
            
            lastIssueDetected = $"Configuration issues: {issues.Count} problems detected";
        }
        
        /// <summary>
        /// 重大健全性レベルの処理
        /// </summary>
        private void HandleCriticalHealthLevel(int score)
        {
            EventLogger.LogError($"[AdvancedRollbackMonitor] CRITICAL HEALTH LEVEL: Score {score}");
            
            if (enableAutoRecovery)
            {
                HandleEmergencyCondition($"Critical health level: {score}");
            }
        }
        
        /// <summary>
        /// 警告健全性レベルの処理
        /// </summary>
        private void HandleWarningHealthLevel(int score)
        {
            EventLogger.LogWarning($"[AdvancedRollbackMonitor] WARNING HEALTH LEVEL: Score {score}");
            
            // 予防的措置の実行
            PerformPreventiveMeasures();
        }
        
        /// <summary>
        /// 健全性回復の処理
        /// </summary>
        private void HandleHealthRecovery(SystemHealthLevel previous, SystemHealthLevel current)
        {
            EventLogger.Log($"[AdvancedRollbackMonitor] System health recovered from {previous} to {current}");
            
            // 回復後の安定性確認
            InvokeRepeating(nameof(ConfirmHealthStability), 30f, 10f);
        }
        
        /// <summary>
        /// 予防的措置の実行
        /// </summary>
        private void PerformPreventiveMeasures()
        {
            EventLogger.Log("[AdvancedRollbackMonitor] Performing preventive measures...");
            
            // ガベージコレクションの実行
            System.GC.Collect();
            
            // サービス統計のリセット
            foreach (var key in serviceMetrics.Keys.ToArray())
            {
                var metrics = serviceMetrics[key];
                if (metrics.ErrorCount > 10)
                {
                    metrics.ErrorCount = 0;
                    serviceMetrics[key] = metrics;
                }
            }
        }
        
        /// <summary>
        /// 健全性安定性の確認
        /// </summary>
        private void ConfirmHealthStability()
        {
            if (currentHealthLevel == SystemHealthLevel.Good || currentHealthLevel == SystemHealthLevel.Excellent)
            {
                CancelInvoke(nameof(ConfirmHealthStability));
                EventLogger.Log("[AdvancedRollbackMonitor] Health stability confirmed");
            }
        }
        
        /// <summary>
        /// 健全性分析の実行
        /// </summary>
        private void PerformHealthAnalysis()
        {
            if (healthHistory.Count < 5) return;
            
            // 過去の傾向分析
            AnalyzeLongTermTrends();
            
            // サービス品質レポートの生成
            if (healthHistory.Count % 12 == 0) // 1分ごと（5秒×12回）
            {
                GenerateServiceQualityReport();
            }
        }
        
        /// <summary>
        /// 長期傾向の分析
        /// </summary>
        private void AnalyzeLongTermTrends()
        {
            if (healthHistory.Count < 20) return;
            
            var recent = healthHistory.GetRange(healthHistory.Count - 10, 10);
            var older = healthHistory.GetRange(healthHistory.Count - 20, 10);
            
            float avgRecent = (float)recent.Average(h => h.HealthScore);
            float avgOlder = (float)older.Average(h => h.HealthScore);
            
            float longTermTrend = avgRecent - avgOlder;
            
            if (Math.Abs(longTermTrend) > 5f)
            {
                string trendDirection = longTermTrend > 0 ? "improving" : "declining";
                EventLogger.Log($"[AdvancedRollbackMonitor] Long-term health trend: {trendDirection} by {Math.Abs(longTermTrend):F1} points");
            }
        }
        
        /// <summary>
        /// サービス品質レポートの生成
        /// </summary>
        private void GenerateServiceQualityReport()
        {
            EventLogger.Log("[AdvancedRollbackMonitor] === Service Quality Report ===");
            
            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                string status = metrics.IsHealthy ? "✅" : "❌";
                EventLogger.Log($"  {status} {metrics.ServiceName}: " +
                               $"Success Rate: {metrics.SuccessRate:F1}%, " +
                               $"Avg Response: {metrics.ResponseTime:F1}ms, " +
                               $"Errors: {metrics.ErrorCount}");
            }
        }
        
        /// <summary>
        /// 問題の記録
        /// </summary>
        private void RecordIssue(string description, IssueType type, IssueSeverity severity)
        {
            var issue = new SystemIssue
            {
                Timestamp = DateTime.Now,
                Description = description,
                Type = type,
                Severity = severity
            };
            
            recentIssues.Enqueue(issue);
            if (recentIssues.Count > MAX_RECENT_ISSUES)
            {
                recentIssues.Dequeue();
            }
            
            // ログ出力
            switch (severity)
            {
                case IssueSeverity.Info:
                    EventLogger.Log($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Warning:
                    EventLogger.LogWarning($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Error:
                    EventLogger.LogError($"[AdvancedRollbackMonitor] {description}");
                    break;
            }
        }
        
        /// <summary>
        /// 監視データの保存
        /// </summary>
        private void SaveMonitoringData()
        {
            try
            {
                PlayerPrefs.SetFloat("AdvancedMonitor_LastHealthScore", lastHealthScore);
                PlayerPrefs.SetString("AdvancedMonitor_LastIssue", lastIssueDetected);
                PlayerPrefs.SetInt("AdvancedMonitor_HealthLevel", (int)currentHealthLevel);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] Failed to save monitoring data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 監視データの読み込み
        /// </summary>
        private void LoadMonitoringData()
        {
            try
            {
                lastHealthScore = PlayerPrefs.GetFloat("AdvancedMonitor_LastHealthScore", 100f);
                lastIssueDetected = PlayerPrefs.GetString("AdvancedMonitor_LastIssue", "");
                currentHealthLevel = (SystemHealthLevel)PlayerPrefs.GetInt("AdvancedMonitor_HealthLevel", 0);
            }
            catch (Exception ex)
            {
                EventLogger.LogError($"[AdvancedRollbackMonitor] Failed to load monitoring data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初回健全性チェック
        /// </summary>
        private void PerformInitialHealthCheck()
        {
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            UpdateHealthHistory(healthStatus);
            
            EventLogger.Log($"[AdvancedRollbackMonitor] Initial health check: {healthStatus.HealthScore}% ({currentHealthLevel})");
        }
        
        /// <summary>
        /// 監視状況レポートの生成
        /// </summary>
        [ContextMenu("Generate Monitoring Report")]
        public void GenerateMonitoringReport()
        {
            EventLogger.Log("[AdvancedRollbackMonitor] === Monitoring Status Report ===");
            EventLogger.Log($"  Current Health Level: {currentHealthLevel} (Score: {lastHealthScore:F1})");
            EventLogger.Log($"  Consecutive Failures: {consecutiveFailures}");
            EventLogger.Log($"  Continuous Monitoring: {enableContinuousMonitoring}");
            EventLogger.Log($"  Predictive Analysis: {enablePredictiveAnalysis}");
            EventLogger.Log($"  Auto Recovery: {enableAutoRecovery}");
            EventLogger.Log($"  Health History: {healthHistory.Count} entries");
            EventLogger.Log($"  Recent Issues: {recentIssues.Count} issues");
            
            if (!string.IsNullOrEmpty(lastIssueDetected))
            {
                EventLogger.Log($"  Last Issue: {lastIssueDetected}");
            }
        }
        
        /// <summary>
        /// 監視システムのリセット
        /// </summary>
        [ContextMenu("Reset Monitoring System")]
        public void ResetMonitoringSystem()
        {
            healthHistory.Clear();
            serviceMetrics.Clear();
            recentIssues.Clear();
            consecutiveFailures = 0;
            currentHealthLevel = SystemHealthLevel.Unknown;
            lastHealthScore = 100f;
            lastIssueDetected = "";
            
            RegisterServiceMetrics();
            PerformInitialHealthCheck();
            
            EventLogger.Log("[AdvancedRollbackMonitor] Monitoring system reset");
        }
    }
    
    /// <summary>
    /// システム健全性レベル
    /// </summary>
    public enum SystemHealthLevel
    {
        Unknown = 0,
        Critical = 1,
        Warning = 2,
        Good = 3,
        Excellent = 4
    }
    
    /// <summary>
    /// 健全性スナップショット
    /// </summary>
    [System.Serializable]
    public class HealthSnapshot
    {
        public DateTime Timestamp;
        public int HealthScore;
        public bool IsHealthy;
        public int IssueCount;
        public bool HasInconsistentConfiguration;
    }
    
    /// <summary>
    /// サービス健全性メトリクス
    /// </summary>
    [System.Serializable]
    public class ServiceHealthMetrics
    {
        public string ServiceName;
        public DateTime LastCheckTime;
        public bool IsHealthy;
        public float ResponseTime;
        public int ErrorCount;
        public float SuccessRate;
    }
    
    /// <summary>
    /// システム問題
    /// </summary>
    [System.Serializable]
    public class SystemIssue
    {
        public DateTime Timestamp;
        public string Description;
        public IssueType Type;
        public IssueSeverity Severity;
    }
    
    /// <summary>
    /// 問題タイプ
    /// </summary>
    public enum IssueType
    {
        Configuration,
        Performance,
        ServiceDegradation,
        HealthTrend,
        RecurringError,
        SystemFailure
    }
    
    /// <summary>
    /// 問題重要度
    /// </summary>
    public enum IssueSeverity
    {
        Info,
        Warning,
        Error
    }
}