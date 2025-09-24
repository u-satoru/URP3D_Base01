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
    /// Step 3.12: 鬮伜ｺｦ縺ｪ邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ逶｣隕悶す繧ｹ繝・Β
    /// 繧ｷ繧ｹ繝・Β迥ｶ諷九・邯咏ｶ夂噪逶｣隕悶∽ｺ域ｸｬ逧・撫鬘梧､懷・縲∬・蜍募ｯｾ蠢懈ｩ溯・
    /// </summary>
    public class AdvancedRollbackMonitor : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableContinuousMonitoring = true;
        [SerializeField] private bool enablePredictiveAnalysis = true;
        [SerializeField] private bool enableAutoRecovery = true;
        [SerializeField] private float monitoringInterval = 5f; // 5遘偵＃縺ｨ
        [SerializeField] private float healthCheckInterval = 10f; // 10遘偵＃縺ｨ
        
        [Header("Thresholds")]
        [SerializeField] private int criticalHealthThreshold = 30;
        [SerializeField] private int warningHealthThreshold = 60;
        [SerializeField] private int maxConsecutiveFailures = 3;
        [SerializeField] private float performanceThreshold = 0.5f; // 50%諤ｧ閭ｽ菴惹ｸ九〒隴ｦ蜻・        
        [Header("Current Status")]
        [SerializeField] private SystemHealthLevel currentHealthLevel = SystemHealthLevel.Unknown;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private float lastHealthScore = 100f;
        [SerializeField] private string lastIssueDetected = "";
        
        // 逶｣隕悶ョ繝ｼ繧ｿ
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
        /// 逶｣隕悶す繧ｹ繝・Β縺ｮ蛻晄悄蛹・        /// </summary>
        private void InitializeMonitoring()
        {
            LoadMonitoringData();
            RegisterServiceMetrics();
            PerformInitialHealthCheck();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Monitoring system initialized");
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ繝｡繝医Μ繧ｯ繧ｹ縺ｮ逋ｻ骭ｲ
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
        /// 繧ｷ繧ｹ繝・Β繝√ぉ繝・け縺ｮ螳溯｡・        /// </summary>
        private void PerformSystemCheck()
        {
            if (!enableContinuousMonitoring) return;
            
            try
            {
                // 蝓ｺ譛ｬ逧・↑繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ繝√ぉ繝・け
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                UpdateHealthHistory(healthStatus);
                
                // 繧ｵ繝ｼ繝薙せ蛻･隧ｳ邏ｰ繝√ぉ繝・け
                CheckIndividualServices();
                
                // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝｡繝医Μ繧ｯ繧ｹ繝√ぉ繝・け
                CheckPerformanceMetrics();
                
                // 莠域ｸｬ蛻・梵縺ｮ螳溯｡・                if (enablePredictiveAnalysis)
                {
                    PerformPredictiveAnalysis();
                }
                
                // 蝠城｡梧､懷・縺ｨ蟇ｾ蠢・                DetectAndHandleIssues(healthStatus);
                
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] System check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 蛛･蜈ｨ諤ｧ螻･豁ｴ縺ｮ譖ｴ譁ｰ
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
            
            // 迴ｾ蝨ｨ縺ｮ蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν繧呈峩譁ｰ
            UpdateCurrentHealthLevel(healthStatus.HealthScore);
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν繧呈峩譁ｰ
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
        /// 蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν螟画峩譎ゅ・蜃ｦ逅・        /// </summary>
        private void OnHealthLevelChanged(SystemHealthLevel previous, SystemHealthLevel current, int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Health level changed: {previous} -> {current} (Score: {score})");
            
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
        /// 蛟句挨繧ｵ繝ｼ繝薙せ縺ｮ繝√ぉ繝・け
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
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Service check failed for {serviceName}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ蛛･蜈ｨ諤ｧ縺ｮ蛟句挨繝√ぉ繝・け
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
        /// 繧ｵ繝ｼ繝薙せ蠢懃ｭ疲凾髢薙・貂ｬ螳・        /// </summary>
        private float MeasureServiceResponseTime(string serviceName)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // 邁｡蜊倥↑蠢懃ｭ疲凾髢捺ｸｬ螳夲ｼ亥ｮ滄圀縺ｮ繧ｵ繝ｼ繝薙せ蜻ｼ縺ｳ蜃ｺ縺暦ｼ・                CheckServiceHealth(serviceName);
                stopwatch.Stop();
                return (float)stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                stopwatch.Stop();
                return -1f; // 繧ｨ繝ｩ繝ｼ縺ｮ蝣ｴ蜷・            }
        }
        
        /// <summary>
        /// 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ繝｡繝医Μ繧ｯ繧ｹ縺ｮ繝√ぉ繝・け
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
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Performance check failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 莠域ｸｬ蛻・梵縺ｮ螳溯｡・        /// </summary>
        private void PerformPredictiveAnalysis()
        {
            if (healthHistory.Count < 5) return; // 譛菴・蝗槭・繝・・繧ｿ縺悟ｿ・ｦ・            
            try
            {
                // 蛛･蜈ｨ諤ｧ繧ｹ繧ｳ繧｢縺ｮ蛯ｾ蜷大・譫・                AnalyzeHealthTrend();
                
                // 繧ｨ繝ｩ繝ｼ逋ｺ逕溘ヱ繧ｿ繝ｼ繝ｳ縺ｮ蛻・梵
                AnalyzeErrorPatterns();
                
                // 繧ｵ繝ｼ繝薙せ蜩∬ｳｪ縺ｮ蜉｣蛹紋ｺ域ｸｬ
                PredictServiceDegradation();
                
            }
            catch (Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Predictive analysis failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 蛛･蜈ｨ諤ｧ蛯ｾ蜷代・蛻・梵
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
            
            if (trendChange < -15f) // 15轤ｹ莉･荳翫・謔ｪ蛹・            {
                RecordIssue($"Negative health trend detected: {trendChange:F1} point decline", 
                           IssueType.HealthTrend, IssueSeverity.Warning);
            }
        }
        
        /// <summary>
        /// 繧ｨ繝ｩ繝ｼ繝代ち繝ｼ繝ｳ縺ｮ蛻・梵
        /// </summary>
        private void AnalyzeErrorPatterns()
        {
            var recentIssuesList = recentIssues.ToArray();
            if (recentIssuesList.Length < 3) return;
            
            // 蜷檎ｨｮ縺ｮ繧ｨ繝ｩ繝ｼ縺檎洒譛滄俣縺ｫ隍・焚逋ｺ逕溘＠縺ｦ縺・ｋ蝣ｴ蜷・            var issueGroups = new Dictionary<string, int>();
            var cutoffTime = DateTime.Now.AddMinutes(-10); // 驕主悉10蛻・俣
            
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
                if (kvp.Value >= 3) // 蜷後§蝠城｡後′3蝗樔ｻ･荳・                {
                    RecordIssue($"Recurring issue pattern detected: '{kvp.Key}' occurred {kvp.Value} times", 
                               IssueType.RecurringError, IssueSeverity.Error);
                }
            }
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ蜩∬ｳｪ蜉｣蛹悶・莠域ｸｬ
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
        /// 蝠城｡後・讀懷・縺ｨ蟇ｾ蠢・        /// </summary>
        private void DetectAndHandleIssues(SystemHealthStatus healthStatus)
        {
            // 騾｣邯壼､ｱ謨励き繧ｦ繝ｳ繧ｿ繝ｼ縺ｮ譖ｴ譁ｰ
            if (!healthStatus.IsHealthy)
            {
                consecutiveFailures++;
            }
            else
            {
                consecutiveFailures = 0;
            }
            
            // 邱頑･莠区・縺ｮ讀懷・
            if (consecutiveFailures >= maxConsecutiveFailures)
            {
                HandleEmergencyCondition($"System failed {consecutiveFailures} consecutive health checks");
            }
            
            // 險ｭ螳夂泝逶ｾ縺ｮ讀懷・
            if (healthStatus.HasInconsistentConfiguration)
            {
                HandleConfigurationInconsistency(healthStatus.Issues);
            }
        }
        
        /// <summary>
        /// 邱頑･莠区・縺ｮ蟇ｾ蠢・        /// </summary>
        private void HandleEmergencyCondition(string reason)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] EMERGENCY CONDITION: {reason}");
            
            if (enableAutoRecovery)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[AdvancedRollbackMonitor] Attempting automatic recovery...");
                
                // 谿ｵ髫守噪縺ｪ蝗槫ｾｩ繧定ｩｦ陦・                if (TryGradualRecovery())
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
        /// 谿ｵ髫守噪蝗槫ｾｩ縺ｮ隧ｦ陦・        /// </summary>
        private bool TryGradualRecovery()
        {
            try
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Attempting gradual recovery...");
                
                // Step 1: 譛譁ｰ縺ｮ繧ｵ繝ｼ繝薙せ險ｭ螳壹ｒ辟｡蜉ｹ蛹・                if (FeatureFlags.UseNewStealthService)
                {
                    FeatureFlags.UseNewStealthService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled StealthService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 2: Spatial Audio險ｭ螳壹ｒ辟｡蜉ｹ蛹・                if (FeatureFlags.UseNewSpatialService)
                {
                    FeatureFlags.UseNewSpatialService = false;
                    ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Disabled SpatialService");
                    if (CheckSystemHealthImprovement()) return true;
                }
                
                // Step 3: 逶｣隕匁ｩ溯・繧剃ｸ譎ょ●豁｢
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
        /// 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ縺ｮ謾ｹ蝟・ｒ遒ｺ隱・        /// </summary>
        private bool CheckSystemHealthImprovement()
        {
            System.Threading.Thread.Sleep(1000); // 1遘貞ｾ・ｩ・            
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            return healthStatus.HealthScore > criticalHealthThreshold;
        }
        
        /// <summary>
        /// 險ｭ螳夂泝逶ｾ縺ｮ蟇ｾ蠢・        /// </summary>
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
        /// 驥榊､ｧ蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleCriticalHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[AdvancedRollbackMonitor] CRITICAL HEALTH LEVEL: Score {score}");
            
            if (enableAutoRecovery)
            {
                HandleEmergencyCondition($"Critical health level: {score}");
            }
        }
        
        /// <summary>
        /// 隴ｦ蜻雁▼蜈ｨ諤ｧ繝ｬ繝吶Ν縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleWarningHealthLevel(int score)
        {
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[AdvancedRollbackMonitor] WARNING HEALTH LEVEL: Score {score}");
            
            // 莠磯亟逧・蒔鄂ｮ縺ｮ螳溯｡・            PerformPreventiveMeasures();
        }
        
        /// <summary>
        /// 蛛･蜈ｨ諤ｧ蝗槫ｾｩ縺ｮ蜃ｦ逅・        /// </summary>
        private void HandleHealthRecovery(SystemHealthLevel previous, SystemHealthLevel current)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] System health recovered from {previous} to {current}");
            
            // 蝗槫ｾｩ蠕後・螳牙ｮ壽ｧ遒ｺ隱・            InvokeRepeating(nameof(ConfirmHealthStability), 30f, 10f);
        }
        
        /// <summary>
        /// 莠磯亟逧・蒔鄂ｮ縺ｮ螳溯｡・        /// </summary>
        private void PerformPreventiveMeasures()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Performing preventive measures...");
            
            // 繧ｬ繝吶・繧ｸ繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳溯｡・            System.GC.Collect();
            
            // 繧ｵ繝ｼ繝薙せ邨ｱ險医・繝ｪ繧ｻ繝・ヨ
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
        /// 蛛･蜈ｨ諤ｧ螳牙ｮ壽ｧ縺ｮ遒ｺ隱・        /// </summary>
        private void ConfirmHealthStability()
        {
            if (currentHealthLevel == SystemHealthLevel.Good || currentHealthLevel == SystemHealthLevel.Excellent)
            {
                CancelInvoke(nameof(ConfirmHealthStability));
                ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] Health stability confirmed");
            }
        }
        
        /// <summary>
        /// 蛛･蜈ｨ諤ｧ蛻・梵縺ｮ螳溯｡・        /// </summary>
        private void PerformHealthAnalysis()
        {
            if (healthHistory.Count < 5) return;
            
            // 驕主悉縺ｮ蛯ｾ蜷大・譫・            AnalyzeLongTermTrends();
            
            // 繧ｵ繝ｼ繝薙せ蜩∬ｳｪ繝ｬ繝昴・繝医・逕滓・
            if (healthHistory.Count % 12 == 0) // 1蛻・＃縺ｨ・・遘津・2蝗橸ｼ・            {
                GenerateServiceQualityReport();
            }
        }
        
        /// <summary>
        /// 髟ｷ譛溷だ蜷代・蛻・梵
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
        /// 繧ｵ繝ｼ繝薙せ蜩∬ｳｪ繝ｬ繝昴・繝医・逕滓・
        /// </summary>
        private void GenerateServiceQualityReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[AdvancedRollbackMonitor] === Service Quality Report ===");
            
            foreach (var kvp in serviceMetrics)
            {
                var metrics = kvp.Value;
                string status = metrics.IsHealthy ? "笨・ : "笶・;
                ServiceLocator.GetService<IEventLogger>()?.Log($"  {status} {metrics.ServiceName}: " +
                               $"Success Rate: {metrics.SuccessRate:F1}%, " +
                               $"Avg Response: {metrics.ResponseTime:F1}ms, " +
                               $"Errors: {metrics.ErrorCount}");
            }
        }
        
        /// <summary>
        /// 蝠城｡後・險倬鹸
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
            
            // 繝ｭ繧ｰ蜃ｺ蜉・            switch (severity)
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
        /// 逶｣隕悶ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・        /// </summary>
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
        /// 逶｣隕悶ョ繝ｼ繧ｿ縺ｮ隱ｭ縺ｿ霎ｼ縺ｿ
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
        /// 蛻晏屓蛛･蜈ｨ諤ｧ繝√ぉ繝・け
        /// </summary>
        private void PerformInitialHealthCheck()
        {
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            UpdateHealthHistory(healthStatus);
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"[AdvancedRollbackMonitor] Initial health check: {healthStatus.HealthScore}% ({currentHealthLevel})");
        }
        
        /// <summary>
        /// 逶｣隕也憾豕√Ξ繝昴・繝医・逕滓・
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
        /// 逶｣隕悶す繧ｹ繝・Β縺ｮ繝ｪ繧ｻ繝・ヨ
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
    /// 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν
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
    /// 蛛･蜈ｨ諤ｧ繧ｹ繝翫ャ繝励す繝ｧ繝・ヨ
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
    /// 繧ｵ繝ｼ繝薙せ蛛･蜈ｨ諤ｧ繝｡繝医Μ繧ｯ繧ｹ
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
    /// 繧ｷ繧ｹ繝・Β蝠城｡・    /// </summary>
    [System.Serializable]
    public class SystemIssue
    {
        public DateTime Timestamp;
        public string Description;
        public IssueType Type;
        public IssueSeverity Severity;
    }
    
    /// <summary>
    /// 蝠城｡後ち繧､繝・    /// </summary>
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
    /// 蝠城｡碁㍾隕∝ｺｦ
    /// </summary>
    public enum IssueSeverity
    {
        Info,
        Warning,
        Error
    }
}
