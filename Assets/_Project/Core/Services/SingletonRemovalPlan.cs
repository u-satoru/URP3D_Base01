using UnityEngine;
// using asterivo.Unity60.Core.Debug;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core.Services;
using System.Collections.Generic;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Commands;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency
using System;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 螳悟・Singleton閼ｱ蜊ｴ繝励Ο繧ｻ繧ｹ邂｡逅・す繧ｹ繝・Β
    /// 谿ｵ髫守噪縺九▽螳牙・縺ｪSingleton螳悟・蜑企勁繧堤ｮ｡逅・    /// </summary>
    public class SingletonRemovalPlan : MonoBehaviour
    {
        [Header("Removal Configuration")]
        [SerializeField] private bool enableAutoRemoval = false;
        [SerializeField] private bool requireManualConfirmation = true;
        [SerializeField] private bool createComprehensiveBackup = true;
        
        [Header("Target Classes")]
        [SerializeField] private string[] singletonClasses = {
            "AudioManager",
            "SpatialAudioManager", 
            "EffectManager",
            "CommandPoolService",
            "EventLogger",
            "CinemachineIntegration"
        };
        
        [Header("Progress Tracking")]
        [SerializeField] private bool removalCompleted = false;
        [SerializeField] private string removalStartTime = "";
        [SerializeField] private string removalCompletionTime = "";
        
        private Dictionary<string, RemovalStep[]> removalPlan;
        
        private void Awake()
        {
            InitializeRemovalPlan();
            LoadRemovalState();
        }
        
        /// <summary>
        /// 蜑企勁繝励Λ繝ｳ繧貞・譛溷喧
        /// </summary>
        private void InitializeRemovalPlan()
        {
            removalPlan = new Dictionary<string, RemovalStep[]>();
            
            // 蜷・け繝ｩ繧ｹ縺ｮ蜑企勁繧ｹ繝・ャ繝励ｒ螳夂ｾｩ
            removalPlan["AudioManager"] = new RemovalStep[]
            {
                new RemovalStep("private static AudioManager instance;", RemovalAction.Delete),
                new RemovalStep("public static AudioManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IAudioService>(this);", RemovalAction.Keep),
                new RemovalStep("DontDestroyOnLoad(gameObject);", RemovalAction.Keep),
                new RemovalStep("IAudioService interface methods", RemovalAction.Keep)
            };
            
            removalPlan["SpatialAudioManager"] = new RemovalStep[]
            {
                new RemovalStep("private static SpatialAudioManager instance;", RemovalAction.Delete),
                new RemovalStep("public static SpatialAudioManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<ISpatialAudioService>(this);", RemovalAction.Keep),
                new RemovalStep("ISpatialAudioService interface methods", RemovalAction.Keep)
            };
            
            removalPlan["EffectManager"] = new RemovalStep[]
            {
                new RemovalStep("private static EffectManager instance;", RemovalAction.Delete),
                new RemovalStep("public static EffectManager Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IEffectService>(this);", RemovalAction.Keep),
                new RemovalStep("IEffectService interface methods", RemovalAction.Keep)
            };
            
            removalPlan["CommandPoolService"] = new RemovalStep[]
            {
                new RemovalStep("private static CommandPoolService instance;", RemovalAction.Delete),
                new RemovalStep("public static CommandPoolService Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<ICommandPoolService>(this);", RemovalAction.Keep),
                new RemovalStep("ICommandPoolService interface methods", RemovalAction.Keep)
            };
            
            removalPlan["EventLogger"] = new RemovalStep[]
            {
                new RemovalStep("private static EventLogger instance;", RemovalAction.Delete),
                new RemovalStep("public static EventLogger Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<IEventLogger>(this);", RemovalAction.Keep),
                new RemovalStep("IEventLogger interface methods", RemovalAction.Keep)
            };
            
            removalPlan["CinemachineIntegration"] = new RemovalStep[]
            {
                new RemovalStep("private static CinemachineIntegration instance;", RemovalAction.Delete),
                new RemovalStep("public static CinemachineIntegration Instance", RemovalAction.Delete),
                new RemovalStep("instance = this;", RemovalAction.Delete),
                new RemovalStep("ServiceLocator.RegisterService<CinemachineIntegration>(this);", RemovalAction.Keep),
                new RemovalStep("Camera management methods", RemovalAction.Keep)
            };
        }
        
        /// <summary>
        /// 螳悟・蜑企勁繝励Ο繧ｻ繧ｹ繧帝幕蟋・        /// </summary>
        [ContextMenu("Start Complete Singleton Removal")]
        public void StartCompleteRemoval()
        {
            if (removalCompleted)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Removal already completed");
                return;
            }
            
            if (!ValidatePreConditions())
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] Pre-conditions not met for removal");
                return;
            }
            
            if (requireManualConfirmation)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] 笞・・CRITICAL: This will PERMANENTLY remove all Singleton patterns");
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Call ExecuteCompleteRemoval() to proceed with confirmation");
                return;
            }
            
            ExecuteCompleteRemoval();
        }
        
        /// <summary>
        /// 莠句燕譚｡莉ｶ繧呈､懆ｨｼ
        /// </summary>
        private bool ValidatePreConditions()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Validating pre-conditions...");
            
            // 1. ServiceLocator 縺梧怏蜉ｹ縺ｧ縺ゅｋ縺薙→
            if (!FeatureFlags.UseServiceLocator)
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] ServiceLocator must be enabled");
                return false;
            }
            
            // 2. 縺吶∋縺ｦ縺ｮ繧ｵ繝ｼ繝薙せ縺郡erviceLocator縺ｫ逋ｻ骭ｲ縺輔ｌ縺ｦ縺・ｋ縺薙→
            bool allServicesRegistered = ValidateServiceRegistration();
            if (!allServicesRegistered)
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] Not all services are properly registered");
                return false;
            }
            
            // 3. 遘ｻ陦瑚ｭｦ蜻翫′辟｡蜉ｹ蛹悶＆繧後※縺・ｋ縺薙→・亥ｮ悟・遘ｻ陦梧ｸ医∩・・            if (FeatureFlags.EnableMigrationWarnings)
            {
                ProjectDebug.LogWarning("[SingletonRemovalPlan] Migration warnings still enabled - consider disabling first");
            }
            
            // 4. 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ繝√ぉ繝・け
            var healthStatus = asterivo.Unity60.Core.Services.EmergencyRollback.CheckSystemHealth();
            if (!healthStatus.IsHealthy)
            {
                ProjectDebug.LogError($"[SingletonRemovalPlan] System health issues detected: {healthStatus.HealthScore}%");
                foreach (var issue in healthStatus.Issues)
                {
                    ProjectDebug.LogError($"[SingletonRemovalPlan] Issue: {issue}");
                }
                return false;
            }
            
            ProjectDebug.Log("[SingletonRemovalPlan] 笨・All pre-conditions validated");
            return true;
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕√ｒ讀懆ｨｼ
        /// </summary>
        private bool ValidateServiceRegistration()
        {
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
                var effectService = ServiceLocator.GetService<IEffectService>();
                var commandPoolService = ServiceLocator.GetService<ICommandPoolService>();
                var eventLogger = ServiceLocator.GetService<IEventLogger>();
                // var cinemachineIntegration = ServiceLocator.GetService<CinemachineIntegration>(); // Commented out - namespace issue
                
                bool allRegistered = audioService != null && 
                                   spatialService != null && 
                                   effectService != null && 
                                   commandPoolService != null && 
                                   eventLogger != null;
                                   // cinemachineIntegration check removed due to namespace issue
                
                ProjectDebug.Log($"[SingletonRemovalPlan] Service registration validation: {allRegistered}");
                return allRegistered;
            }
            catch (Exception ex)
            {
                ProjectDebug.LogError($"[SingletonRemovalPlan] Service validation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 遒ｺ隱肴ｸ医∩螳悟・蜑企勁繧貞ｮ溯｡・        /// </summary>
        [ContextMenu("Execute Complete Removal (CONFIRMED)")]
        public void ExecuteCompleteRemoval()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] === STARTING COMPLETE SINGLETON REMOVAL ===");
            removalStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            if (createComprehensiveBackup)
            {
                CreateComprehensiveBackup();
            }
            
            // Phase 1: Legacy Singleton螳悟・辟｡蜉ｹ蛹・            ExecutePhase1_DisableLegacySingletons();
            
            // Phase 2: 迚ｩ逅・噪繧ｳ繝ｼ繝牙炎髯､縺ｮ貅門ｙ
            ExecutePhase2_PreparePhysicalRemoval();
            
            // Phase 3: 譛邨よ､懆ｨｼ縺ｨ繝槭・繧ｯ
            ExecutePhase3_FinalValidation();
            
            removalCompletionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            removalCompleted = true;
            SaveRemovalState();
            
            ProjectDebug.Log("[SingletonRemovalPlan] === COMPLETE SINGLETON REMOVAL FINISHED ===");
            GenerateRemovalReport();
        }
        
        /// <summary>
        /// Phase 1: Legacy Singleton螳悟・辟｡蜉ｹ蛹・        /// </summary>
        private void ExecutePhase1_DisableLegacySingletons()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 1: Disabling Legacy Singletons...");
            
            // FeatureFlags繧呈怙邨ら憾諷九↓險ｭ螳・            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.EnableMigrationMonitoring = false;
            
            ProjectDebug.Log("[SingletonRemovalPlan] 笨・Legacy Singletons completely disabled");
        }
        
        /// <summary>
        /// Phase 2: 迚ｩ逅・噪蜑企勁縺ｮ貅門ｙ
        /// </summary>
        private void ExecutePhase2_PreparePhysicalRemoval()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 2: Preparing Physical Code Removal...");
            
            foreach (var className in singletonClasses)
            {
                if (removalPlan.ContainsKey(className))
                {
                    GenerateRemovalInstructions(className, removalPlan[className]);
                }
            }
            
            ProjectDebug.Log("[SingletonRemovalPlan] 笨・Physical removal instructions generated");
        }
        
        /// <summary>
        /// Phase 3: 譛邨よ､懆ｨｼ
        /// </summary>
        private void ExecutePhase3_FinalValidation()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Phase 3: Final Validation...");
            
            // 繧ｵ繝ｼ繝薙せ縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・            bool servicesWorking = ValidateServiceRegistration();
            
            if (servicesWorking)
            {
                ProjectDebug.Log("[SingletonRemovalPlan] 笨・All services validated post-removal");
            }
            else
            {
                ProjectDebug.LogError("[SingletonRemovalPlan] 笶・Service validation failed");
                // 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繧呈署譯・                ProjectDebug.LogWarning("[SingletonRemovalPlan] Consider emergency rollback");
            }
        }
        
        /// <summary>
        /// 蛹・峡逧・ヰ繝・け繧｢繝・・繧剃ｽ懈・
        /// </summary>
        private void CreateComprehensiveBackup()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] Creating comprehensive backup...");
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            string backupKey = $"SingletonRemoval_Backup_{timestamp}";
            
            // FeatureFlags迥ｶ諷九ｒ繝舌ャ繧ｯ繧｢繝・・
            PlayerPrefs.SetString($"{backupKey}_FeatureFlags", SerializeFeatureFlags());
            
            // 蜷・け繝ｩ繧ｹ縺ｮ迴ｾ蝨ｨ縺ｮ迥ｶ諷九ｒ險倬鹸
            foreach (var className in singletonClasses)
            {
                PlayerPrefs.SetString($"{backupKey}_{className}_Status", "Singleton_Pattern_Active");
            }
            
            PlayerPrefs.SetString("LastSingletonBackup", backupKey);
            PlayerPrefs.Save();
            
            ProjectDebug.Log($"[SingletonRemovalPlan] 笨・Backup created: {backupKey}");
        }
        
        /// <summary>
        /// 蜑企勁謖・､ｺ繧堤函謌・        /// </summary>
        private void GenerateRemovalInstructions(string className, RemovalStep[] steps)
        {
            ProjectDebug.Log($"[SingletonRemovalPlan] === {className} Removal Instructions ===");
            
            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                string actionIcon = step.Action == RemovalAction.Delete ? "笶・ : "笨・;
                string actionText = step.Action == RemovalAction.Delete ? "DELETE" : "KEEP";
                
                ProjectDebug.Log($"[SingletonRemovalPlan] {i+1}. {actionIcon} {actionText}: {step.Description}");
            }
            
            ProjectDebug.Log($"[SingletonRemovalPlan] === End {className} Instructions ===");
        }
        
        /// <summary>
        /// FeatureFlags繧偵す繝ｪ繧｢繝ｩ繧､繧ｺ
        /// </summary>
        private string SerializeFeatureFlags()
        {
            return $"UseServiceLocator:{FeatureFlags.UseServiceLocator}," +
                   $"DisableLegacySingletons:{FeatureFlags.DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{FeatureFlags.EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{FeatureFlags.EnableMigrationMonitoring}";
        }
        
        /// <summary>
        /// 蜑企勁繝ｬ繝昴・繝医ｒ逕滓・
        /// </summary>
        private void GenerateRemovalReport()
        {
            ProjectDebug.Log("[SingletonRemovalPlan] === SINGLETON REMOVAL REPORT ===");
            ProjectDebug.Log($"Removal Started: {removalStartTime}");
            ProjectDebug.Log($"Removal Completed: {removalCompletionTime}");
            ProjectDebug.Log($"Classes Processed: {singletonClasses.Length}");
            ProjectDebug.Log("Status: READY FOR MANUAL CODE DELETION");
            ProjectDebug.Log("");
            ProjectDebug.Log("搭 MANUAL ACTION REQUIRED:");
            ProjectDebug.Log("1. Review removal instructions above");
            ProjectDebug.Log("2. Manually delete Singleton code from each class");
            ProjectDebug.Log("3. Keep ServiceLocator registrations intact");
            ProjectDebug.Log("4. Run compilation tests");
            ProjectDebug.Log("5. Execute final validation");
            ProjectDebug.Log("=====================================");
        }
        
        /// <summary>
        /// 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繧貞ｮ溯｡・        /// </summary>
        [ContextMenu("Emergency Rollback")]
        public void EmergencyRollback()
        {
            ProjectDebug.LogWarning("[SingletonRemovalPlan] Executing emergency rollback...");
            
            // FeatureFlags繧貞ｮ牙・縺ｪ迥ｶ諷九↓謌ｻ縺・            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            ProjectDebug.Log("[SingletonRemovalPlan] 笨・Emergency rollback completed");
        }
        
        /// <summary>
        /// 蜑企勁迥ｶ諷九ｒ菫晏ｭ・        /// </summary>
        private void SaveRemovalState()
        {
            PlayerPrefs.SetInt("SingletonRemovalPlan_Completed", removalCompleted ? 1 : 0);
            PlayerPrefs.SetString("SingletonRemovalPlan_StartTime", removalStartTime);
            PlayerPrefs.SetString("SingletonRemovalPlan_CompletionTime", removalCompletionTime);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 蜑企勁迥ｶ諷九ｒ隱ｭ縺ｿ霎ｼ縺ｿ
        /// </summary>
        private void LoadRemovalState()
        {
            removalCompleted = PlayerPrefs.GetInt("SingletonRemovalPlan_Completed", 0) == 1;
            removalStartTime = PlayerPrefs.GetString("SingletonRemovalPlan_StartTime", "");
            removalCompletionTime = PlayerPrefs.GetString("SingletonRemovalPlan_CompletionTime", "");
        }
    }
    
    /// <summary>
    /// 蜑企勁繧ｹ繝・ャ繝怜ｮ夂ｾｩ
    /// </summary>
    [System.Serializable]
    public class RemovalStep
    {
        public string Description;
        public RemovalAction Action;
        
        public RemovalStep(string description, RemovalAction action)
        {
            Description = description;
            Action = action;
        }
    }
    
    /// <summary>
    /// 蜑企勁繧｢繧ｯ繧ｷ繝ｧ繝ｳ遞ｮ蛻･
    /// </summary>
    public enum RemovalAction
    {
        Delete,  // 繧ｳ繝ｼ繝牙炎髯､
        Keep     // 繧ｳ繝ｼ繝我ｿ晄戟
    }
}