using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 邱頑･譎ゅ・繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β
    /// 遘ｻ陦御ｸｭ縺ｫ蝠城｡後′逋ｺ逕溘＠縺溷ｴ蜷医・邱頑･蟇ｾ蠢・    /// Step 3.10縺ｮ荳驛ｨ縺ｨ縺励※螳溯｣・    /// </summary>
    public static class EmergencyRollback 
    {
        // 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡後ヵ繝ｩ繧ｰ
        private const string EMERGENCY_FLAG_KEY = "EmergencyRollback_Active";
        private const string ROLLBACK_REASON_KEY = "EmergencyRollback_Reason";
        private const string ROLLBACK_TIME_KEY = "EmergencyRollback_Time";
        
        /// <summary>
        /// 襍ｷ蜍墓凾縺ｫ邱頑･繝輔Λ繧ｰ繧偵メ繧ｧ繝・け
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CheckEmergencyFlag()
        {
            // 繧ｨ繝・ぅ繧ｿ險ｭ螳壹ｄ繧ｳ繝槭Φ繝峨Λ繧､繝ｳ蠑墓焚縺ｧ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繝輔Λ繧ｰ繧堤｢ｺ隱・            bool emergencyFlagSet = false;
            
            #if UNITY_EDITOR
            emergencyFlagSet = UnityEditor.EditorPrefs.GetBool("EmergencyRollback", false);
            if (emergencyFlagSet)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in Editor");
                UnityEditor.EditorPrefs.SetBool("EmergencyRollback", false); // 繝輔Λ繧ｰ繧偵Μ繧ｻ繝・ヨ
            }
            #endif
            
            // PlayerPrefs縺ｧ繧らｷ頑･繝輔Λ繧ｰ繧偵メ繧ｧ繝・け
            if (PlayerPrefs.GetInt(EMERGENCY_FLAG_KEY, 0) == 1)
            {
                emergencyFlagSet = true;
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in PlayerPrefs");
                PlayerPrefs.SetInt(EMERGENCY_FLAG_KEY, 0); // 繝輔Λ繧ｰ繧偵Μ繧ｻ繝・ヨ
                PlayerPrefs.Save();
            }
            
            // 繧ｳ繝槭Φ繝峨Λ繧､繝ｳ蠑墓焚縺ｧ繧ゅメ繧ｧ繝・け
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-emergency-rollback")
                {
                    emergencyFlagSet = true;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in command line args");
                    break;
                }
            }
            
            if (emergencyFlagSet)
            {
                ExecuteEmergencyRollback("System startup emergency flag detected");
            }
        }
        
        /// <summary>
        /// 螳悟・邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繧貞ｮ溯｡・        /// </summary>
        public static void ExecuteEmergencyRollback(string reason = "Manual execution")
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Executing emergency rollback: {reason}");
            
            // 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡瑚ｨ倬鹸
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString(ROLLBACK_REASON_KEY, reason);
            PlayerPrefs.SetString(ROLLBACK_TIME_KEY, timestamp);
            
            // 蜈ｨ縺ｦ縺ｮFeatureFlag繧貞ｮ牙・縺ｪ迥ｶ諷九↓謌ｻ縺・            asterivo.Unity60.Core.FeatureFlags.UseServiceLocator = true;  // ServiceLocator閾ｪ菴薙・菫晄戟
            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;  
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.DisableLegacySingletons = false; // Singleton繧｢繧ｯ繧ｻ繧ｹ繧定ｨｱ蜿ｯ
            FeatureFlags.EnableMigrationWarnings = false; // 隴ｦ蜻翫ｒ蛛懈ｭ｢
            FeatureFlags.EnableMigrationMonitoring = false; // 逶｣隕悶ｒ蛛懈ｭ｢
            FeatureFlags.EnableAutoRollback = false; // 閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ繧貞●豁｢
            
            // Phase 3 譁ｰ讖溯・繧堤┌蜉ｹ蛹・            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.EnablePerformanceMonitoring = false;
            
            // 谿ｵ髫守噪遘ｻ陦後ヵ繝ｩ繧ｰ繧偵Μ繧ｻ繝・ヨ
            FeatureFlags.MigrateAudioManager = false;
            FeatureFlags.MigrateSpatialAudioManager = false;
            FeatureFlags.MigrateEffectManager = false;
            FeatureFlags.MigrateStealthAudioCoordinator = false;
            FeatureFlags.MigrateAudioUpdateCoordinator = false;
            
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.LogError("[EMERGENCY] Complete rollback executed successfully");
            ServiceLocator.GetService<IEventLogger>()?.LogError("EMERGENCY] Reverted to legacy Singleton system. All new services disabled.");
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Rollback reason: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Rollback time: {timestamp}");
            ServiceLocator.GetService<IEventLogger>()?.LogError("EMERGENCY] Please check logs for the cause of rollback and fix issues before retrying migration.");
            
            // SingletonDisableScheduler繧ゅΜ繧ｻ繝・ヨ
            ResetScheduler();
        }
        
        /// <summary>
        /// 驛ｨ蛻・Ο繝ｼ繝ｫ繝舌ャ繧ｯ - 迚ｹ螳壹・繧ｵ繝ｼ繝薙せ縺ｮ縺ｿ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        public static void RollbackSpecificService(string serviceName, string reason = "Service-specific issue")
        {
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EMERGENCY] Rolling back service '{serviceName}': {reason}");
            
            switch (serviceName.ToLower())
            {
                case "audio":
                case "audioservice":
                    FeatureFlags.UseNewAudioService = false;
                    FeatureFlags.MigrateAudioManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] AudioService rolled back to Singleton");
                    break;
                    
                case "spatial":
                case "spatialaudio":
                    FeatureFlags.UseNewSpatialService = false;
                    FeatureFlags.MigrateSpatialAudioManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] SpatialAudioService rolled back to Singleton");
                    break;
                    
                case "stealth":
                case "stealthaudio":
                    FeatureFlags.UseNewStealthService = false;
                    FeatureFlags.MigrateStealthAudioCoordinator = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] StealthAudioService rolled back to Singleton");
                    break;
                    
                case "effect":
                case "effectmanager":
                    FeatureFlags.MigrateEffectManager = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] EffectManager rolled back to Singleton");
                    break;
                    
                case "audioupdate":
                case "audiocoordinator":
                    FeatureFlags.UseNewAudioUpdateSystem = false;
                    FeatureFlags.MigrateAudioUpdateCoordinator = false;
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EMERGENCY] AudioUpdateCoordinator rolled back to Singleton");
                    break;
                    
                default:
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[EMERGENCY] Unknown service name for rollback: {serviceName}");
                    return;
            }
            
            // 驛ｨ蛻・Ο繝ｼ繝ｫ繝舌ャ繧ｯ險倬鹸
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string partialRollbackKey = $"PartialRollback_{serviceName}";
            PlayerPrefs.SetString(partialRollbackKey, $"{timestamp}: {reason}");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EMERGENCY] Service '{serviceName}' rollback completed");
        }
        
        /// <summary>
        /// 蠕ｩ譌ｧ - 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ迥ｶ諷九°繧画ｭ｣蟶ｸ迥ｶ諷九↓謌ｻ縺・        /// </summary>
        public static void RestoreFromRollback(string reason = "Manual recovery")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] Restoring from emergency rollback: {reason}");
            
            // 谿ｵ髫守噪縺ｫ蠕ｩ譌ｧ・亥ｮ牙・縺ｮ縺溘ａ・・            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnableMigrationWarnings = true;
            
            // 譁ｰ繧ｵ繝ｼ繝薙せ繧呈ｮｵ髫守噪縺ｫ譛牙柑蛹・            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;
            
            // 遘ｻ陦後ヵ繝ｩ繧ｰ繧貞ｾｩ豢ｻ
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.MigrateEffectManager = true;
            FeatureFlags.MigrateStealthAudioCoordinator = true;
            FeatureFlags.MigrateAudioUpdateCoordinator = true;
            
            // 蠕ｩ譌ｧ險倬鹸
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("Recovery_Time", timestamp);
            PlayerPrefs.SetString("Recovery_Reason", reason);
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] All services restored to new implementation");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery reason: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery time: {timestamp}");
        }
        
        /// <summary>
        /// 邱頑･繝輔Λ繧ｰ繧定ｨｭ螳夲ｼ域ｬ｡蝗櫁ｵｷ蜍墓凾縺ｫ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡鯉ｼ・        /// </summary>
        public static void SetEmergencyFlag(string reason = "Emergency flag set programmatically")
        {
            PlayerPrefs.SetInt(EMERGENCY_FLAG_KEY, 1);
            PlayerPrefs.SetString(ROLLBACK_REASON_KEY, reason);
            PlayerPrefs.Save();
            
            #if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool("EmergencyRollback", true);
            #endif
            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] Emergency flag set: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Rollback will execute on next application start");
        }
        
        /// <summary>
        /// SingletonDisableScheduler繧偵Μ繧ｻ繝・ヨ
        /// </summary>
        private static void ResetScheduler()
        {
            PlayerPrefs.SetInt("SingletonDisableScheduler_CurrentDay", 0); // NotStarted
            PlayerPrefs.SetString("SingletonDisableScheduler_StartTime", "");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[EmergencyRollback] SingletonDisableScheduler reset to initial state");
        }
        
        /// <summary>
        /// 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螻･豁ｴ繧貞叙蠕・        /// </summary>
        public static RollbackHistory GetRollbackHistory()
        {
            return new RollbackHistory
            {
                LastEmergencyRollbackTime = PlayerPrefs.GetString(ROLLBACK_TIME_KEY, "Never"),
                LastEmergencyRollbackReason = PlayerPrefs.GetString(ROLLBACK_REASON_KEY, "No rollback recorded"),
                LastRecoveryTime = PlayerPrefs.GetString("Recovery_Time", "Never"),
                LastRecoveryReason = PlayerPrefs.GetString("Recovery_Reason", "No recovery recorded")
            };
        }
        
        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ繝√ぉ繝・け
        /// </summary>
        public static SystemHealthStatus CheckSystemHealth()
        {
            var health = new SystemHealthStatus();
            
            // 蝓ｺ譛ｬ逧・↑險ｭ螳壹・謨ｴ蜷域ｧ繝√ぉ繝・け
            health.ServiceLocatorEnabled = FeatureFlags.UseServiceLocator;
            health.SingletonsDisabled = FeatureFlags.DisableLegacySingletons;
            health.MigrationWarningsEnabled = FeatureFlags.EnableMigrationWarnings;
            
            // 遏帷崟讀懷・
            if (!FeatureFlags.UseServiceLocator && (FeatureFlags.UseNewAudioService || 
                FeatureFlags.UseNewSpatialService || FeatureFlags.UseNewStealthService))
            {
                health.HasInconsistentConfiguration = true;
                health.Issues.Add("ServiceLocator is disabled but new services are enabled");
            }
            
            if (FeatureFlags.DisableLegacySingletons && !FeatureFlags.EnableMigrationWarnings)
            {
                health.HasInconsistentConfiguration = true;
                health.Issues.Add("Singletons are disabled but migration warnings are off");
            }
            
            // 蛛･蜈ｨ諤ｧ繧ｹ繧ｳ繧｢險育ｮ・            int healthScore = 100;
            if (health.HasInconsistentConfiguration) healthScore -= 30;
            if (!health.ServiceLocatorEnabled) healthScore -= 20;
            if (health.Issues.Count > 0) healthScore -= (health.Issues.Count * 10);
            
            health.HealthScore = Mathf.Max(0, healthScore);
            health.IsHealthy = health.HealthScore >= 70;
            
            return health;
        }
        
        /// <summary>
        /// 邱頑･迥ｶ豕∵､懷・縺ｨ閾ｪ蜍募ｯｾ蠢・        /// </summary>
        public static void MonitorSystemHealth()
        {
            var health = CheckSystemHealth();
            
            if (!health.IsHealthy)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] System health degraded: {health.HealthScore}%");
                
                foreach (var issue in health.Issues)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EmergencyRollback] Health Issue: {issue}");
                }
                
                // 驥榊､ｧ縺ｪ蝠城｡後′縺ゅｋ蝣ｴ蜷医・閾ｪ蜍輔Ο繝ｼ繝ｫ繝舌ャ繧ｯ繧呈､懆ｨ・                if (health.HealthScore < 30)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogError("[EmergencyRollback] Critical system health detected");
                    
                    if (FeatureFlags.EnableAutoRollback)
                    {
                        ExecuteEmergencyRollback("Automatic rollback due to critical health issues");
                    }
                    else
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogError("[EmergencyRollback] Auto rollback disabled. Manual intervention required.");
                        SetEmergencyFlag("Critical health issues detected - manual rollback required");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螻･豁ｴ
    /// </summary>
    [System.Serializable]
    public class RollbackHistory
    {
        public string LastEmergencyRollbackTime;
        public string LastEmergencyRollbackReason;
        public string LastRecoveryTime;
        public string LastRecoveryReason;
    }
    
    /// <summary>
    /// 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ繧ｹ繝・・繧ｿ繧ｹ
    /// </summary>
    [System.Serializable]
    public class SystemHealthStatus
    {
        public bool IsHealthy;
        public int HealthScore; // 0-100
        public bool HasInconsistentConfiguration;
        public bool ServiceLocatorEnabled;
        public bool SingletonsDisabled;
        public bool MigrationWarningsEnabled;
        public System.Collections.Generic.List<string> Issues = new System.Collections.Generic.List<string>();
    }
}
