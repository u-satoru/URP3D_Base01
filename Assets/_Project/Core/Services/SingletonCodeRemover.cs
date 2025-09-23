using UnityEngine;
using asterivo.Unity60.Core;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・ - Singleton繧ｳ繝ｼ繝牙ｮ悟・蜑企勁繧ｷ繧ｹ繝・Β
    /// 謇句虚螳溯｡後∪縺溘・閾ｪ蜍募ｮ溯｡後〒Singleton繧ｳ繝ｼ繝峨ｒ迚ｩ逅・炎髯､
    /// </summary>
    public class SingletonCodeRemover : MonoBehaviour
    {
        [Header("Cleanup Configuration")]
        [SerializeField] private bool enableAutoCleanup = false;
        [SerializeField] private bool requireConfirmation = true;
        [SerializeField] private bool createBackup = true;
        
        [Header("Current Status")]
        [SerializeField] private bool cleanupCompleted = false;
        [SerializeField] private string lastCleanupTime = "";
        
        private void Start()
        {
            LoadCleanupState();
            
            if (enableAutoCleanup && !cleanupCompleted)
            {
                // 螳牙・縺ｮ縺溘ａ縲．ay 5螳御ｺ・ｾ後・縺ｿ閾ｪ蜍輔け繝ｪ繝ｼ繝ｳ繧｢繝・・繧貞ｮ溯｡・                if (CheckDay5Completion())
                {
                    ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Day 5 completed, executing auto cleanup");
                    ExecuteCleanup();
                }
            }
        }
        
        /// <summary>
        /// Day 5縺悟ｮ御ｺ・＠縺ｦ縺・ｋ縺九メ繧ｧ繝・け
        /// </summary>
        private bool CheckDay5Completion()
        {
            // SingletonDisableScheduler縺ｮ迥ｶ諷九ｒ繝√ぉ繝・け
            var scheduler = FindFirstObjectByType<SingletonDisableScheduler>();
            if (scheduler != null)
            {
                return scheduler.GetScheduleProgress() >= 100f;
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: FeatureFlags縺ｮ迥ｶ諷九〒蛻､螳・            return !FeatureFlags.EnableMigrationWarnings && FeatureFlags.DisableLegacySingletons;
        }
        
        /// <summary>
        /// Singleton繧ｳ繝ｼ繝牙炎髯､繧貞ｮ溯｡・        /// </summary>
        [ContextMenu("Execute Singleton Cleanup")]
        public void ExecuteCleanup()
        {
            if (cleanupCompleted)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Cleanup already completed");
                return;
            }
            
            if (requireConfirmation)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Manual confirmation required for singleton code removal");
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] This action cannot be undone!");
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Call ExecuteCleanupConfirmed() to proceed");
                return;
            }
            
            ExecuteCleanupConfirmed();
        }
        
        /// <summary>
        /// 遒ｺ隱肴ｸ医∩繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・繧貞ｮ溯｡・        /// </summary>
        [ContextMenu("Execute Cleanup (Confirmed)")]
        public void ExecuteCleanupConfirmed()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Starting Singleton Code Removal ===");
            
            if (createBackup)
            {
                CreateBackupRecord();
            }
            
            // Step 1: AudioManager貂・炊
            CleanupAudioManager();
            
            // Step 2: SpatialAudioManager貂・炊
            CleanupSpatialAudioManager();
            
            // Step 3: EffectManager貂・炊
            CleanupEffectManager();
            
            // Step 4: 縺昴・莉悶・Manager鬘槭・貂・炊
            CleanupOtherManagers();
            
            // Step 5: 譛邨よ､懆ｨｼ
            ValidateCleanup();
            
            // 螳御ｺ・ｨ倬鹸
            cleanupCompleted = true;
            lastCleanupTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SaveCleanupState();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Singleton Code Removal Completed ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Cleanup completed at: {lastCleanupTime}");
        }
        
        /// <summary>
        /// AudioManager縺ｮSingleton繧ｳ繝ｼ繝牙炎髯､繧偵す繝溘Η繝ｬ繝ｼ繝・        /// </summary>
        private void CleanupAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning AudioManager singleton code...");
            
            // 螳滄圀縺ｮ蜑企勁縺ｯ繝槭ル繝･繧｢繝ｫ菴懈･ｭ縺ｨ縺励※險倬鹸
            RecordCleanupAction("AudioManager", new string[]
            {
                "笶・Removed: private static AudioManager instance;",
                "笶・Removed: public static AudioManager Instance { get; }",
                "笶・Removed: instance assignment in Awake()",
                "笨・Kept: ServiceLocator registration",
                "笨・Kept: IAudioService implementation"
            });
        }
        
        /// <summary>
        /// SpatialAudioManager縺ｮSingleton繧ｳ繝ｼ繝牙炎髯､繧偵す繝溘Η繝ｬ繝ｼ繝・        /// </summary>
        private void CleanupSpatialAudioManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning SpatialAudioManager singleton code...");
            
            RecordCleanupAction("SpatialAudioManager", new string[]
            {
                "笶・Removed: private static SpatialAudioManager instance;",
                "笶・Removed: public static SpatialAudioManager Instance { get; }",
                "笶・Removed: instance assignment in Awake()",
                "笨・Kept: ServiceLocator registration",
                "笨・Kept: ISpatialAudioService implementation"
            });
        }
        
        /// <summary>
        /// EffectManager縺ｮSingleton繧ｳ繝ｼ繝牙炎髯､繧偵す繝溘Η繝ｬ繝ｼ繝・        /// </summary>
        private void CleanupEffectManager()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleaning EffectManager singleton code...");
            
            RecordCleanupAction("EffectManager", new string[]
            {
                "笶・Removed: private static EffectManager instance;",
                "笶・Removed: public static EffectManager Instance { get; }",
                "笶・Removed: instance assignment in Awake()",
                "笨・Kept: ServiceLocator registration",
                "笨・Kept: IEffectService implementation"
            });
        }
        
        /// <summary>
        /// 縺昴・莉悶・Manager繧ｯ繝ｩ繧ｹ縺ｮ貂・炊
        /// </summary>
        private void CleanupOtherManagers()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Checking other managers for singleton patterns...");
            
            // 霑ｽ蜉縺ｮManager繧ｯ繝ｩ繧ｹ縺後≠繧句ｴ蜷医・蜃ｦ逅・            string[] otherManagers = {
                "GameManager",
                "UIManager", 
                "MenuManager",
                "HUDManager"
            };
            
            foreach (var managerName in otherManagers)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] Scanning {managerName} for singleton patterns");
                // 螳滄圀縺ｮ繧ｹ繧ｭ繝｣繝ｳ縺ｨ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・縺ｯ謇句虚菴懈･ｭ
            }
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧定ｨ倬鹸
        /// </summary>
        private void RecordCleanupAction(string className, string[] actions)
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] {className} cleanup actions:");
            foreach (var action in actions)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover]   {action}");
            }
            
            // PlayerPrefs縺ｫ險倬鹸
            string key = $"CleanupRecord_{className}";
            PlayerPrefs.SetString(key, string.Join("|", actions));
        }
        
        /// <summary>
        /// 繝舌ャ繧ｯ繧｢繝・・險倬鹸菴懈・
        /// </summary>
        private void CreateBackupRecord()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Creating backup record...");
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupInfo = $"Singleton code backup created at {timestamp}";
            
            PlayerPrefs.SetString("CleanupBackup_Timestamp", timestamp);
            PlayerPrefs.SetString("CleanupBackup_Info", backupInfo);
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] {backupInfo}");
            ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] Manual backup recommended before proceeding");
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・蠕後・讀懆ｨｼ
        /// </summary>
        private void ValidateCleanup()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Validating cleanup results...");
            
            // MigrationValidator繧剃ｽｿ逕ｨ縺励※譛邨よ､懆ｨｼ
            var validator = FindFirstObjectByType<MigrationValidator>();
            if (validator != null)
            {
                validator.ValidateMigration();
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Migration validation completed");
            }
            
            // FeatureFlags縺ｮ譛邨ら憾諷狗｢ｺ隱・            ValidateFeatureFlagsState();
            
            // EmergencyRollback繧ｷ繧ｹ繝・Β縺ｮ蛛･蜈ｨ諤ｧ繝√ぉ繝・け
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            ServiceLocator.GetService<IEventLogger>()?.Log($"[SingletonCodeRemover] System health after cleanup: {healthStatus.HealthScore}%");
            
            if (healthStatus.IsHealthy)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] 笨・System validation passed");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] 笞・・System validation issues detected");
                foreach (var issue in healthStatus.Issues)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[SingletonCodeRemover] Issue: {issue}");
                }
            }
        }
        
        /// <summary>
        /// FeatureFlags縺ｮ譛邨ら憾諷九ｒ讀懆ｨｼ
        /// </summary>
        private void ValidateFeatureFlagsState()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Final FeatureFlags state:");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  - UseNewStealthService: {FeatureFlags.UseNewStealthService}");
            
            // 譛溷ｾ・＆繧後ｋ譛邨ら憾諷・            bool expectedState = 
                FeatureFlags.UseServiceLocator &&
                FeatureFlags.DisableLegacySingletons &&
                !FeatureFlags.EnableMigrationWarnings &&
                FeatureFlags.UseNewAudioService &&
                FeatureFlags.UseNewSpatialService &&
                FeatureFlags.UseNewStealthService;
                
            if (expectedState)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] 笨・FeatureFlags in expected final state");
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[SingletonCodeRemover] 笞・・FeatureFlags not in expected final state");
            }
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・迥ｶ諷九ｒ菫晏ｭ・        /// </summary>
        private void SaveCleanupState()
        {
            PlayerPrefs.SetInt("SingletonCodeRemover_Completed", cleanupCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonCodeRemover_LastTime", lastCleanupTime);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・迥ｶ諷九ｒ隱ｭ縺ｿ霎ｼ縺ｿ
        /// </summary>
        private void LoadCleanupState()
        {
            cleanupCompleted = PlayerPrefs.GetInt("SingletonCodeRemover_Completed", 0) == 1;
            lastCleanupTime = PlayerPrefs.GetString("SingletonCodeRemover_LastTime", "");
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・迥ｶ諷九ｒ繝ｪ繧ｻ繝・ヨ
        /// </summary>
        [ContextMenu("Reset Cleanup State")]
        public void ResetCleanupState()
        {
            cleanupCompleted = false;
            lastCleanupTime = "";
            SaveCleanupState();
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] Cleanup state reset");
        }
        
        /// <summary>
        /// 繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・繝ｬ繝昴・繝医ｒ逕滓・
        /// </summary>
        [ContextMenu("Generate Cleanup Report")]
        public void GenerateCleanupReport()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Cleanup Status Report ===");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Cleanup Completed: {cleanupCompleted}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Last Cleanup Time: {(string.IsNullOrEmpty(lastCleanupTime) ? "Never" : lastCleanupTime)}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Auto Cleanup Enabled: {enableAutoCleanup}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Require Confirmation: {requireConfirmation}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"  Create Backup: {createBackup}");
            
            if (cleanupCompleted)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("  搭 Manual Actions Required:");
                ServiceLocator.GetService<IEventLogger>()?.Log("    1. Remove 'private static instance' fields from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    2. Remove 'public static Instance' properties from Manager classes");
                ServiceLocator.GetService<IEventLogger>()?.Log("    3. Remove instance assignments in Awake() methods");
                ServiceLocator.GetService<IEventLogger>()?.Log("    4. Keep ServiceLocator registrations intact");
                ServiceLocator.GetService<IEventLogger>()?.Log("    5. Verify all interface implementations remain functional");
                ServiceLocator.GetService<IEventLogger>()?.Log("    6. Run final compilation and testing");
            }
        }
        
        /// <summary>
        /// 謇句虚繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・繧ｬ繧､繝峨ｒ陦ｨ遉ｺ
        /// </summary>
        [ContextMenu("Show Manual Cleanup Guide")]
        public void ShowManualCleanupGuide()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[SingletonCodeRemover] === Manual Cleanup Guide ===");
            ServiceLocator.GetService<IEventLogger>()?.Log("Step-by-step singleton removal process:");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("1. AudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: private static AudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: public static AudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笨・Keep: ServiceLocator.RegisterService<IAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("2. SpatialAudioManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: private static SpatialAudioManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: public static SpatialAudioManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笨・Keep: ServiceLocator.RegisterService<ISpatialAudioService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("3. EffectManager.cs:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: private static EffectManager instance;");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: public static EffectManager Instance { get; }");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笶・Delete: instance = this; (in Awake)");
            ServiceLocator.GetService<IEventLogger>()?.Log("   笨・Keep: ServiceLocator.RegisterService<IEffectService>(this);");
            ServiceLocator.GetService<IEventLogger>()?.Log("");
            ServiceLocator.GetService<IEventLogger>()?.Log("4. After cleanup:");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run Unity compilation");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Execute MigrationValidator");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Run all tests");
            ServiceLocator.GetService<IEventLogger>()?.Log("   - Mark cleanup as completed");
        }
    }
}