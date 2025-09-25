using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.12: Advanced Emergency Rollback Monitoring System
    /// Continuous system state monitoring, issue prediction, and automatic response mechanism
    /// </summary>
    public class AdvancedRollbackMonitor : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableContinuousMonitoring = true;
        [SerializeField] private bool enablePredictiveAnalysis = true;
        [SerializeField] private bool enableAutoRecovery = true;
        [SerializeField] private float monitoringInterval = 5f; // Every 5 seconds
        [SerializeField] private float healthCheckInterval = 10f; // Every 10 seconds

        [Header("Thresholds")]
        [SerializeField] private int criticalHealthThreshold = 30;
        [SerializeField] private int warningHealthThreshold = 60;
        [SerializeField] private int maxConsecutiveFailures = 3;
        [SerializeField] private float performanceThreshold = 0.5f; // Warning at 50% performance drop

        [Header("Current Status")]
        [SerializeField] private SystemHealthLevel currentHealthLevel = SystemHealthLevel.Unknown;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private float lastHealthScore = 100f;
        [SerializeField] private string lastIssueDetected = "";

        // Monitoring data
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

            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Advanced monitoring system started");
        }

        private void OnDestroy()
        {
            SaveMonitoringData();
        }

        /// <summary>
        /// Initialize monitoring system
        /// </summary>
        private void InitializeMonitoring()
        {
            LoadMonitoringData();
            RegisterServiceMetrics();
            PerformInitialHealthCheck();

            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Monitoring system initialized");
        }

        /// <summary>
        /// Register service metrics
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
        /// Perform system check
        /// </summary>
        private void PerformSystemCheck()
        {
            if (!enableContinuousMonitoring) return;

            try
            {
                // Basic system health check
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                UpdateHealthHistory(healthStatus);

                // Individual service check
                CheckIndividualServices();

                // Performance metrics check
                CheckPerformanceMetrics();

                // Predictive analysis execution
                if (enablePredictiveAnalysis)
                {
                    PerformPredictiveAnalysis();
                }

                // Issue detection and handling
                DetectAndHandleIssues(healthStatus);

            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] System check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Update health history
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

            // Update current health level
            UpdateCurrentHealthLevel(healthStatus.HealthScore);
        }

        /// <summary>
        /// Update current health level
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
        /// Handle health level change
        /// </summary>
        private void OnHealthLevelChanged(SystemHealthLevel previous, SystemHealthLevel current, int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Health level changed: {previous} -> {current} (Score: {score})");

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
        /// Check individual services
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
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Service check failed for {serviceName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Check individual service health
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
        /// Measure service response time
        /// </summary>
        private float MeasureServiceResponseTime(string serviceName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Simple response time measurement (actual service call)
                CheckServiceHealth(serviceName);
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                stopwatch.Stop();
                return -1f; // Error case
            }
        }

        /// <summary>
        /// Check performance metrics
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
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Performance check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform predictive analysis
        /// </summary>
        private void PerformPredictiveAnalysis()
        {
            if (healthHistory.Count < 5) return; // Need minimum data

            try
            {
                // Analyze health trends
                AnalyzeHealthTrend();

                // Analyze error patterns
                AnalyzeErrorPatterns();

                // Predict service degradation
                PredictServiceDegradation();

            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Predictive analysis failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Analyze health trends
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

            if (trendChange < -15f) // More than 15 point decline
            {
                RecordIssue($"Negative health trend detected: {trendChange:F1} point decline",
                           IssueType.HealthTrend, IssueSeverity.Warning);
            }
        }

        /// <summary>
        /// Analyze error patterns
        /// </summary>
        private void AnalyzeErrorPatterns()
        {
            var recentIssuesList = recentIssues.ToArray();
            if (recentIssuesList.Length < 3) return;

            // Check if same errors occur multiple times recently
            var issueGroups = new Dictionary<string, int>();
            var cutoffTime = DateTime.Now.AddMinutes(-10); // Last 10 minutes

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
                if (kvp.Value >= 3) // Same issue 3+ times
                {
                    RecordIssue($"Recurring issue pattern detected: '{kvp.Key}' occurred {kvp.Value} times",
                               IssueType.RecurringError, IssueSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Predict service quality degradation
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
        /// Detect issues and handle
        /// </summary>
        private void DetectAndHandleIssues(SystemHealthStatus healthStatus)
        {
            // Update consecutive failure counter
            if (!healthStatus.IsHealthy)
            {
                consecutiveFailures++;
            }
            else
            {
                consecutiveFailures = 0;
            }

            // Detect emergency condition
            if (consecutiveFailures >= maxConsecutiveFailures)
            {
                HandleEmergencyCondition($"System failed {consecutiveFailures} consecutive health checks");
            }

            // Detect configuration inconsistency
            if (healthStatus.HasInconsistentConfiguration)
            {
                HandleConfigurationInconsistency(healthStatus.Issues);
            }
        }

        /// <summary>
        /// Handle emergency condition
        /// </summary>
        private void HandleEmergencyCondition(string reason)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] EMERGENCY CONDITION: {reason}");

            if (enableAutoRecovery)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Attempting automatic recovery...");

                // Try gradual recovery
                if (TryGradualRecovery())
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Gradual recovery succeeded");
                    consecutiveFailures = 0;
                }
                else
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Gradual recovery failed, executing emergency rollback");
                    EmergencyRollback.ExecuteEmergencyRollback($"Auto rollback triggered: {reason}");
                }
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Auto recovery disabled, manual intervention required");
                EmergencyRollback.SetEmergencyFlag($"Emergency condition detected: {reason}");
            }
        }

        /// <summary>
        /// Try gradual recovery
        /// </summary>
        private bool TryGradualRecovery()
        {
            try
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Attempting gradual recovery...");

                // Step 1: Disable newest service settings
                if (FeatureFlags.UseNewStealthService)
                {
                    FeatureFlags.UseNewStealthService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled StealthService");
                    if (CheckSystemHealthImprovement()) return true;
                }

                // Step 2: Disable Spatial Audio settings
                if (FeatureFlags.UseNewSpatialService)
                {
                    FeatureFlags.UseNewSpatialService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled SpatialService");
                    if (CheckSystemHealthImprovement()) return true;
                }

                // Step 3: Temporarily stop monitoring
                if (FeatureFlags.EnableMigrationMonitoring)
                {
                    FeatureFlags.EnableMigrationMonitoring = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled migration monitoring");
                    if (CheckSystemHealthImprovement()) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Gradual recovery failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check system health improvement
        /// </summary>
        private bool CheckSystemHealthImprovement()
        {
            System.Threading.Thread.Sleep(1000); // Wait 1 second

            var healthStatus = EmergencyRollback.CheckSystemHealth();
            return healthStatus.HealthScore > criticalHealthThreshold;
        }

        /// <summary>
        /// Handle configuration inconsistency
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
        /// Handle critical health level
        /// </summary>
        private void HandleCriticalHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] CRITICAL HEALTH LEVEL: Score {score}");

            if (enableAutoRecovery)
            {
                HandleEmergencyCondition($"Critical health level: {score}");
            }
        }

        /// <summary>
        /// Handle warning health level
        /// </summary>
        private void HandleWarningHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[AdvancedRollbackMonitor] WARNING HEALTH LEVEL: Score {score}");

            // Perform preventive measures
            PerformPreventiveMeasures();
        }

        /// <summary>
        /// Handle health recovery
        /// </summary>
        private void HandleHealthRecovery(SystemHealthLevel previous, SystemHealthLevel current)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] System health recovered from {previous} to {current}");

            // Confirm stability after recovery
            InvokeRepeating(nameof(ConfirmHealthStability), 30f, 10f);
        }

        /// <summary>
        /// Perform preventive measures
        /// </summary>
        private void PerformPreventiveMeasures()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Performing preventive measures...");

            // Execute garbage collection
            System.GC.Collect();

            // Reset service statistics
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
        /// Confirm health stability
        /// </summary>
        private void ConfirmHealthStability()
        {
            if (currentHealthLevel == SystemHealthLevel.Good || currentHealthLevel == SystemHealthLevel.Excellent)
            {
                CancelInvoke(nameof(ConfirmHealthStability));
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Health stability confirmed");
            }
        }

        /// <summary>
        /// Perform health analysis
        /// </summary>
        private void PerformHealthAnalysis()
        {
            if (healthHistory.Count < 5) return;

            // Analyze long-term trends
            AnalyzeLongTermTrends();

            // Generate service quality report
            if (healthHistory.Count % 12 == 0) // Every minute (5 seconds x 12 times)
            {
                GenerateServiceQualityReport();
            }
        }

        /// <summary>
        /// Analyze long-term trends
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
                ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Long-term health trend: {trendDirection} by {Math.Abs(longTermTrend):F1} points");
            }
        }

        /// <summary>
        /// Generate service quality report
        /// </summary>
        private void GenerateServiceQualityReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] === Service Quality Report ===");

            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                string status = metrics.IsHealthy ? "OK" : "FAIL";
                ServiceLocator.GetService<IEventLogger>()?.Log($"  {status} {metrics.ServiceName}: " +
                               $"Success Rate: {metrics.SuccessRate:F1}%, " +
                               $"Avg Response: {metrics.ResponseTime:F1}ms, " +
                               $"Errors: {metrics.ErrorCount}");
            }
        }

        /// <summary>
        /// Record issue
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

            // Log output
            switch (severity)
            {
                case IssueSeverity.Info:
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Warning:
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[AdvancedRollbackMonitor] {description}");
                    break;
                case IssueSeverity.Error:
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] {description}");
                    break;
            }
        }

        /// <summary>
        /// Save monitoring data
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
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Failed to save monitoring data: {ex.Message}");
            }
        }

        /// <summary>
        /// Load monitoring data
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
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] Failed to load monitoring data: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform initial health check
        /// </summary>
        private void PerformInitialHealthCheck()
        {
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            UpdateHealthHistory(healthStatus);

            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Initial health check: {healthStatus.HealthScore}% ({currentHealthLevel})");
        }

        /// <summary>
        /// Generate monitoring report
        /// </summary>
        [ContextMenu("Generate Monitoring Report")]
        public void GenerateMonitoringReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] === Monitoring Status Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Current Health Level: {currentHealthLevel} (Score: {lastHealthScore:F1})");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Consecutive Failures: {consecutiveFailures}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Continuous Monitoring: {enableContinuousMonitoring}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Predictive Analysis: {enablePredictiveAnalysis}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Auto Recovery: {enableAutoRecovery}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Health History: {healthHistory.Count} entries");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Recent Issues: {recentIssues.Count} issues");

            if (!string.IsNullOrEmpty(lastIssueDetected))
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"  Last Issue: {lastIssueDetected}");
            }
        }

        /// <summary>
        /// Reset monitoring system
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

            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Monitoring system reset");
        }
    }

    /// <summary>
    /// System health level
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
    /// Health snapshot
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
    /// Service health metrics
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
    /// System issue
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
    /// Issue type
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
    /// Issue severity
    /// </summary>
    public enum IssueSeverity
    {
        Info,
        Warning,
        Error
    }
}