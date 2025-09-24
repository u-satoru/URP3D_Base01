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
    /// Legacy Singleton菴ｿ逕ｨ迥ｶ豕√ｒ逶｣隕悶＠縲∫ｧｻ陦碁ｲ謐励ｒ霑ｽ霍｡縺吶ｋ
    /// Step 3.9: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β縺ｮ荳驛ｨ
    /// </summary>
    public class MigrationMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableRealTimeLogging = true;
        [SerializeField] private bool enableUsageTracking = true;
        [SerializeField] private float reportingInterval = 30f; // 30遘偵＃縺ｨ縺ｫ繝ｬ繝昴・繝・
        [Header("Usage Statistics")]
        [SerializeField] private int totalSingletonAccesses = 0;
        [SerializeField] private int uniqueSingletonClasses = 0;
        
        // 菴ｿ逕ｨ迥ｶ豕√・險倬鹸
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
        /// Singleton菴ｿ逕ｨ繧定ｨ倬鹸縺吶ｋ
        /// </summary>
        /// <param name="singletonType">菴ｿ逕ｨ縺輔ｌ縺欖ingleton縺ｮ蝙・/param>
        /// <param name="accessMethod">繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕・(萓・ "AudioManager.Instance")</param>
        public void LogSingletonUsage(Type singletonType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            totalSingletonAccesses++;
            
            // 邨ｱ險域ュ蝣ｱ繧呈峩譁ｰ
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
            
            // 譛霑代・繧､繝吶Φ繝医ｒ險倬鹸
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
            
            // 繝ｪ繧｢繝ｫ繧ｿ繧､繝繝ｭ繧ｰ蜃ｺ蜉・            if (enableRealTimeLogging)
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
        /// ServiceLocator菴ｿ逕ｨ繧定ｨ倬鹸縺吶ｋ
        /// </summary>
        /// <param name="serviceType">菴ｿ逕ｨ縺輔ｌ縺溘し繝ｼ繝薙せ縺ｮ蝙・/param>
        /// <param name="accessMethod">繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕・(萓・ "ServiceLocator.GetService<IAudioService>()")</param>
        public void LogServiceLocatorUsage(Type serviceType, string accessMethod)
        {
            if (!enableUsageTracking) return;
            
            // ServiceLocator菴ｿ逕ｨ縺ｮ險倬鹸・医・繧ｸ繝・ぅ繝悶↑謖・ｨ吶→縺励※謇ｱ縺・ｼ・            
            // 繝ｪ繧｢繝ｫ繧ｿ繧､繝繝ｭ繧ｰ蜃ｺ蜉・            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] ServiceLocator usage: {serviceType.Name} via {accessMethod}");
            }
            
            // ServiceLocator菴ｿ逕ｨ繧､繝吶Φ繝医ｒ險倬鹸
            var usageEvent = new ServiceLocatorUsageEvent
            {
                Timestamp = DateTime.Now,
                ServiceType = serviceType.Name,
                AccessMethod = accessMethod
            };
            
            // 譛霑代・繧､繝吶Φ繝医↓霑ｽ蜉・・erviceLocator繧､繝吶Φ繝育畑縺ｮ繝ｪ繧ｹ繝医′縺ゅｌ縺ｰ縺昴％縺ｫ縲√↑縺代ｌ縺ｰ譌｢蟄倥・繝ｪ繧ｹ繝医↓霑ｽ蜉・・            if (recentServiceLocatorEvents == null)
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
        /// 迴ｾ蝨ｨ縺ｮ菴ｿ逕ｨ迥ｶ豕√Ξ繝昴・繝医ｒ逕滓・
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
        /// 譛霑代・菴ｿ逕ｨ繧､繝吶Φ繝医ｒ陦ｨ遉ｺ
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
            int displayCount = Mathf.Min(recentEvents.Count, 10); // 譛譁ｰ10莉ｶ繧定｡ｨ遉ｺ
            
            for (int i = recentEvents.Count - displayCount; i < recentEvents.Count; i++)
            {
                var evt = recentEvents[i];
                ServiceLocator.GetService<IEventLogger>()?.Log($"  [{evt.Timestamp:HH:mm:ss}] {evt.SingletonType}.{evt.AccessMethod}");
            }
        }
        
        /// <summary>
        /// 菴ｿ逕ｨ邨ｱ險医ｒPlayerPrefs縺ｫ菫晏ｭ・        /// </summary>
        public void SaveUsageStatistics()
        {
            try
            {
                PlayerPrefs.SetInt("MigrationMonitor_TotalAccesses", totalSingletonAccesses);
                PlayerPrefs.SetInt("MigrationMonitor_UniqueClasses", uniqueSingletonClasses);
                PlayerPrefs.SetString("MigrationMonitor_LastReportTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // 蜷Тingleton縺ｮ菴ｿ逕ｨ蝗樊焚繧剃ｿ晏ｭ・                foreach (var kvp in usageStats)
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
        /// 菫晏ｭ倥＆繧後◆邨ｱ險医ｒ隱ｭ縺ｿ霎ｼ縺ｿ
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
        /// 邨ｱ險医ｒ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        [ContextMenu("Reset Statistics")]
        public void ResetStatistics()
        {
            usageStats.Clear();
            recentEvents.Clear();
            totalSingletonAccesses = 0;
            uniqueSingletonClasses = 0;
            
            // PlayerPrefs縺九ｉ繧ょ炎髯､
            PlayerPrefs.DeleteKey("MigrationMonitor_TotalAccesses");
            PlayerPrefs.DeleteKey("MigrationMonitor_UniqueClasses");
            PlayerPrefs.DeleteKey("MigrationMonitor_LastReportTime");
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] Statistics reset");
        }
        
        /// <summary>
        /// 遘ｻ陦後・謗ｨ螂ｨ莠矩・ｒ逕滓・
        /// </summary>
        [ContextMenu("Generate Migration Recommendations")]
        public void GenerateMigrationRecommendations()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] === Migration Recommendations ===");
            
            if (usageStats.Count == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("  笨・No singleton usage detected - migration appears complete!");
                return;
            }
            
            foreach (var kvp in usageStats)
            {
                var info = kvp.Value;
                string recommendation = GetMigrationRecommendation(info);
                ServiceLocator.GetService<IEventLogger>()?.Log($"  搭 {info.SingletonType.Name}: {recommendation}");
            }
        }
        
                
        /// <summary>
        /// 遘ｻ陦碁ｲ謐励ｒ0.0-1.0縺ｮ遽・峇縺ｧ蜿門ｾ・        /// ServiceLocator菴ｿ逕ｨ邇・→ Legacy Singleton辟｡蜉ｹ蛹也憾諷九°繧臥ｮ怜・
        /// </summary>
        /// <returns>遘ｻ陦碁ｲ謐・(0.0 = 譛ｪ髢句ｧ・ 1.0 = 螳御ｺ・</returns>
        /// <summary>
        /// 遘ｻ陦碁ｲ謐励ｒ0.0-1.0縺ｮ遽・峇縺ｧ蜿門ｾ・        /// ServiceLocator菴ｿ逕ｨ邇・→ Legacy Singleton辟｡蜉ｹ蛹也憾諷九°繧臥ｮ怜・
        /// </summary>
        /// <returns>遘ｻ陦碁ｲ謐・(0.0 = 譛ｪ髢句ｧ・ 1.0 = 螳御ｺ・</returns>
        /// <summary>
        /// 遘ｻ陦碁ｲ謐励ｒ0.0-1.0縺ｮ遽・峇縺ｧ蜿門ｾ・        /// ServiceLocator菴ｿ逕ｨ邇・→ Legacy Singleton辟｡蜉ｹ蛹也憾諷九°繧臥ｮ怜・
        /// </summary>
        /// <returns>遘ｻ陦碁ｲ謐・(0.0 = 譛ｪ髢句ｧ・ 1.0 = 螳御ｺ・</returns>
        public float GetMigrationProgress()
        {
            // Phase 1: Legacy Singleton辟｡蜉ｹ蛹悶メ繧ｧ繝・け
            float phase1Progress = FeatureFlags.DisableLegacySingletons ? 0.6f : 0.0f;
            
            // Phase 2: ServiceLocator菴ｿ逕ｨ繝√ぉ繝・け
            float phase2Progress = FeatureFlags.UseServiceLocator ? 0.3f : 0.0f;
            
            // Phase 3: Legacy菴ｿ逕ｨ迥ｶ豕√メ繧ｧ繝・け・井ｽｿ逕ｨ驥上′蟆代↑縺・⊇縺ｩ騾ｲ謐励′鬮倥＞・・            float phase3Progress = 0.0f;
            if (totalSingletonAccesses == 0)
            {
                // Legacy菴ｿ逕ｨ縺ｪ縺・= 螳御ｺ・                phase3Progress = 0.1f;
            }
            else if (totalSingletonAccesses < 10)
            {
                // 菴惹ｽｿ逕ｨ驥・                phase3Progress = 0.05f;
            }
            // 鬮倅ｽｿ逕ｨ驥上・蝣ｴ蜷医・phase3Progress = 0.0f
            
            float totalProgress = phase1Progress + phase2Progress + phase3Progress;
            
            // 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・            if (enableRealTimeLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Migration Progress: {totalProgress:P1} " +
                               $"(Phase1: {phase1Progress:P1}, Phase2: {phase2Progress:P1}, Phase3: {phase3Progress:P1})");
            }
            
            return totalProgress;
        }

        /// <summary>
        /// 遘ｻ陦後・螳牙・諤ｧ繧貞愛螳壹☆繧・        /// 驥崎ｦ√↑繧ｵ繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ迥ｶ諷九→Legacy Singleton菴ｿ逕ｨ迥ｶ豕√ｒ邱丞粋逧・↓隧穂ｾ｡
        /// </summary>
        /// <returns>true=螳牙・, false=蜊ｱ髯ｺ, null=蛻､螳壻ｸ崎・</returns>
        public bool? IsMigrationSafe()
        {
            try
            {
                // 1. ServiceLocator縺ｮ蝓ｺ譛ｬ蜍穂ｽ懃｢ｺ隱・                if (!FeatureFlags.UseServiceLocator)
                {
                    if (enableRealTimeLogging)
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] ServiceLocator is disabled - migration safety uncertain");
                    return null; // ServiceLocator縺檎┌蜉ｹ縺ｮ蝣ｴ蜷医・蛻､螳壻ｸ崎・
                }
                
                // 2. 驥崎ｦ√↑繧ｵ繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ迥ｶ諷九メ繧ｧ繝・け
                int criticalServicesCount = 0;
                int registeredServicesCount = 0;
                
                // 驥崎ｦ√し繝ｼ繝薙せ縺ｮ繝√ぉ繝・け
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
                
                // 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ邇・ｒ邂怜・
                float serviceRegistrationRatio = criticalServicesCount > 0 ? 
                    (float)registeredServicesCount / criticalServicesCount : 0f;
                
                // 3. Legacy Singleton菴ｿ逕ｨ驥上メ繧ｧ繝・け
                bool legacySingletonUsageAcceptable = totalSingletonAccesses < 50; // 50蝗樊悴貅縺ｪ繧牙ｮ牙・遽・峇
                
                // 4. 邱丞粋蛻､螳・                bool isServicesSafe = serviceRegistrationRatio >= 0.8f; // 80%莉･荳翫・繧ｵ繝ｼ繝薙せ縺檎匳骭ｲ貂医∩
                bool isLegacyUsageSafe = legacySingletonUsageAcceptable;
                bool isFeatureFlagsSafe = FeatureFlags.UseServiceLocator; // ServiceLocator縺梧怏蜉ｹ
                
                bool overallSafety = isServicesSafe && isLegacyUsageSafe && isFeatureFlagsSafe;
                
                // 繝・ヰ繝・げ諠・ｱ蜃ｺ蜉・                if (enableRealTimeLogging)
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationMonitor] Safety Assessment:");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Services: {registeredServicesCount}/{criticalServicesCount} ({serviceRegistrationRatio:P1}) - {(isServicesSafe ? "螳牙・" : "蜊ｱ髯ｺ")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Legacy Usage: {totalSingletonAccesses} accesses - {(isLegacyUsageSafe ? "螳牙・" : "蜊ｱ髯ｺ")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  FeatureFlags: ServiceLocator={FeatureFlags.UseServiceLocator} - {(isFeatureFlagsSafe ? "螳牙・" : "蜊ｱ髯ｺ")}");
                    ServiceLocator.GetService<IEventLogger>()?.Log($"  Overall Safety: {(overallSafety ? "笨・SAFE" : "笞・・UNSAFE")}");
                }
                
                return overallSafety;
            }
            catch (System.Exception ex)
            {
                if (enableRealTimeLogging)
                    ServiceLocator.GetService<IEventLogger>()?.LogError($"[MigrationMonitor] Safety assessment failed: {ex.Message}");
                return null; // 萓句､也匱逕滓凾縺ｯ蛻､螳壻ｸ崎・
            }
        }

        
        /// <summary>
        /// Singleton菴ｿ逕ｨ邨ｱ險医ｒ蜿門ｾ・        /// </summary>
        /// <returns>Singleton菴ｿ逕ｨ邨ｱ險医・繝・ぅ繧ｯ繧ｷ繝ｧ繝翫Μ</returns>
        public Dictionary<Type, SingletonUsageInfo> GetSingletonUsageStats()
        {
            return new Dictionary<Type, SingletonUsageInfo>(usageStats);
        }
        
        /// <summary>
        /// ServiceLocator菴ｿ逕ｨ邨ｱ險医ｒ蜿門ｾ・        /// </summary>
        /// <returns>ServiceLocator菴ｿ逕ｨ繧､繝吶Φ繝医・繝ｪ繧ｹ繝・/returns>
        public List<ServiceLocatorUsageEvent> GetServiceLocatorUsageStats()
        {
            return new List<ServiceLocatorUsageEvent>(recentServiceLocatorEvents ?? new List<ServiceLocatorUsageEvent>());
        }
/// <summary>
        /// 邁｡譏鍋沿縺ｮ螳牙・諤ｧ繝√ぉ繝・け (繧ｳ繝ｳ繝・く繧ｹ繝医Γ繝九Η繝ｼ逕ｨ)
        /// </summary>
        [ContextMenu("Check Migration Safety")]
        public void CheckMigrationSafety()
        {
            var safetyResult = IsMigrationSafe();
            
            if (safetyResult == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationMonitor] 笞・・Migration safety assessment inconclusive");
            }
            else if (safetyResult.Value)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationMonitor] 笨・Migration is SAFE to proceed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationMonitor] 笞・・Migration is UNSAFE - review issues before proceeding");
            }
        }


/// <summary>
        /// 迚ｹ螳壹・Singleton縺ｫ蟇ｾ縺吶ｋ遘ｻ陦梧耳螂ｨ莠矩・ｒ蜿門ｾ・        /// </summary>
        private string GetMigrationRecommendation(SingletonUsageInfo info)
        {
            if (info.AccessCount > 50)
            {
                return "笶・High usage detected - Priority migration recommended";
            }
            else if (info.AccessCount > 10)
            {
                return "笞・・ Medium usage - Schedule migration soon";
            }
            else
            {
                return "庁 Low usage - Can be migrated when convenient";
            }
        }
    }
    
    /// <summary>
    /// Singleton菴ｿ逕ｨ諠・ｱ繧呈ｼ邏阪☆繧九け繝ｩ繧ｹ
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
    /// Singleton菴ｿ逕ｨ繧､繝吶Φ繝医ｒ譬ｼ邏阪☆繧九け繝ｩ繧ｹ
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
    /// ServiceLocator菴ｿ逕ｨ繧､繝吶Φ繝医ｒ譬ｼ邏阪☆繧九け繝ｩ繧ｹ
    /// </summary>
    [System.Serializable]
    public class ServiceLocatorUsageEvent
    {
        public DateTime Timestamp;
        public string ServiceType;
        public string AccessMethod;
    }
}