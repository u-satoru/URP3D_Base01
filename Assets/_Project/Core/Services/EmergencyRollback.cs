using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 緊急時�EロールバックシスチE��
    /// 移行中に問題が発生した場合�E緊急対忁E    /// Step 3.10の一部として実裁E    /// </summary>
    public static class EmergencyRollback 
    {
        // 緊急ロールバック実行フラグ
        private const string EMERGENCY_FLAG_KEY = "EmergencyRollback_Active";
        private const string ROLLBACK_REASON_KEY = "EmergencyRollback_Reason";
        private const string ROLLBACK_TIME_KEY = "EmergencyRollback_Time";
        
        /// <summary>
        /// 起動時に緊急フラグをチェチE��
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CheckEmergencyFlag()
        {
            // エチE��タ設定やコマンドライン引数でロールバックフラグを確誁E            bool emergencyFlagSet = false;
            
            #if UNITY_EDITOR
            emergencyFlagSet = UnityEditor.EditorPrefs.GetBool("EmergencyRollback", false);
            if (emergencyFlagSet)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in Editor");
                UnityEditor.EditorPrefs.SetBool("EmergencyRollback", false); // フラグをリセチE��
            }
            #endif
            
            // PlayerPrefsでも緊急フラグをチェチE��
            if (PlayerPrefs.GetInt(EMERGENCY_FLAG_KEY, 0) == 1)
            {
                emergencyFlagSet = true;
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[EmergencyRollback] Emergency flag detected in PlayerPrefs");
                PlayerPrefs.SetInt(EMERGENCY_FLAG_KEY, 0); // フラグをリセチE��
                PlayerPrefs.Save();
            }
            
            // コマンドライン引数でもチェチE��
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
        /// 完�E緊急ロールバックを実衁E        /// </summary>
        public static void ExecuteEmergencyRollback(string reason = "Manual execution")
        {
            ServiceLocator.GetService<IEventLogger>()?.LogError($"[EMERGENCY] Executing emergency rollback: {reason}");
            
            // 緊急ロールバック実行記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString(ROLLBACK_REASON_KEY, reason);
            PlayerPrefs.SetString(ROLLBACK_TIME_KEY, timestamp);
            
            // 全てのFeatureFlagを安�Eな状態に戻ぁE            asterivo.Unity60.Core.FeatureFlags.UseServiceLocator = true;  // ServiceLocator自体�E保持
            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;  
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.DisableLegacySingletons = false; // Singletonアクセスを許可
            FeatureFlags.EnableMigrationWarnings = false; // 警告を停止
            FeatureFlags.EnableMigrationMonitoring = false; // 監視を停止
            FeatureFlags.EnableAutoRollback = false; // 自動ロールバックを停止
            
            // Phase 3 新機�Eを無効匁E            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;
            FeatureFlags.UseNewStealthService = false;
            FeatureFlags.EnablePerformanceMonitoring = false;
            
            // 段階的移行フラグをリセチE��
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
            
            // SingletonDisableSchedulerもリセチE��
            ResetScheduler();
        }
        
        /// <summary>
        /// 部刁E��ールバック - 特定�Eサービスのみロールバック
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
            
            // 部刁E��ールバック記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string partialRollbackKey = $"PartialRollback_{serviceName}";
            PlayerPrefs.SetString(partialRollbackKey, $"{timestamp}: {reason}");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[EMERGENCY] Service '{serviceName}' rollback completed");
        }
        
        /// <summary>
        /// 復旧 - ロールバック状態から正常状態に戻ぁE        /// </summary>
        public static void RestoreFromRollback(string reason = "Manual recovery")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] Restoring from emergency rollback: {reason}");
            
            // 段階的に復旧�E�安�Eのため�E�E            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.EnableMigrationWarnings = true;
            
            // 新サービスを段階的に有効匁E            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;
            
            // 移行フラグを復活
            FeatureFlags.MigrateAudioManager = true;
            FeatureFlags.MigrateSpatialAudioManager = true;
            FeatureFlags.MigrateEffectManager = true;
            FeatureFlags.MigrateStealthAudioCoordinator = true;
            FeatureFlags.MigrateAudioUpdateCoordinator = true;
            
            // 復旧記録
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString("Recovery_Time", timestamp);
            PlayerPrefs.SetString("Recovery_Reason", reason);
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[RECOVERY] All services restored to new implementation");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery reason: {reason}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[RECOVERY] Recovery time: {timestamp}");
        }
        
        /// <summary>
        /// 緊急フラグを設定（次回起動時にロールバック実行！E        /// </summary>
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
        /// SingletonDisableSchedulerをリセチE��
        /// </summary>
        private static void ResetScheduler()
        {
            PlayerPrefs.SetInt("SingletonDisableScheduler_CurrentDay", 0); // NotStarted
            PlayerPrefs.SetString("SingletonDisableScheduler_StartTime", "");
            PlayerPrefs.Save();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[EmergencyRollback] SingletonDisableScheduler reset to initial state");
        }
        
        /// <summary>
        /// ロールバック履歴を取征E        /// </summary>
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
        /// シスチE��健全性チェチE��
        /// </summary>
        public static SystemHealthStatus CheckSystemHealth()
        {
            var health = new SystemHealthStatus();
            
            // 基本皁E��設定�E整合性チェチE��
            health.ServiceLocatorEnabled = FeatureFlags.UseServiceLocator;
            health.SingletonsDisabled = FeatureFlags.DisableLegacySingletons;
            health.MigrationWarningsEnabled = FeatureFlags.EnableMigrationWarnings;
            
            // 矛盾検�E
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
            
            // 健全性スコア計箁E            int healthScore = 100;
            if (health.HasInconsistentConfiguration) healthScore -= 30;
            if (!health.ServiceLocatorEnabled) healthScore -= 20;
            if (health.Issues.Count > 0) healthScore -= (health.Issues.Count * 10);
            
            health.HealthScore = Mathf.Max(0, healthScore);
            health.IsHealthy = health.HealthScore >= 70;
            
            return health;
        }
        
        /// <summary>
        /// 緊急状況検�Eと自動対忁E        /// </summary>
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
                
                // 重大な問題がある場合�E自動ロールバックを検訁E                if (health.HealthScore < 30)
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
    /// ロールバック履歴
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
    /// シスチE��健全性スチE�Eタス
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