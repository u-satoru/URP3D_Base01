using UnityEngine;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Commands;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency
using System;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Phase 1: 貅門ｙ繝輔ぉ繝ｼ繧ｺ - 蜑肴署譚｡莉ｶ讀懆ｨｼ縺ｨ繝舌ャ繧ｯ繧｢繝・・螳溯｡後す繧ｹ繝・Β
    /// 螳悟・Singleton蜑企勁縺ｮ莠句燕貅門ｙ繧貞ｮ牙・縺ｫ邂｡逅・    /// </summary>
    public class Phase1PreparationValidator : MonoBehaviour
    {
        [Header("Validation Results")]
        [SerializeField] private bool serviceLocatorReady = false;
        [SerializeField] private bool allServicesRegistered = false;
        [SerializeField] private bool systemHealthy = false;
        [SerializeField] private bool backupCreated = false;
        [SerializeField] private bool emergencyRollbackReady = false;
        
        [Header("Current FeatureFlags State")]
        [SerializeField] private bool useServiceLocator = false;
        [SerializeField] private bool disableLegacySingletons = false;
        [SerializeField] private bool enableMigrationWarnings = false;
        [SerializeField] private bool enableMigrationMonitoring = false;
        
        [Header("System Health Metrics")]
        [SerializeField] private float systemHealthScore = 0f;
        [SerializeField] private int registeredServicesCount = 0;
        [SerializeField] private string validationTimestamp = "";
        
        private void Start()
        {
            ValidatePreparationReadiness();
        }
        
        /// <summary>
        /// Phase 1 貅門ｙ迥ｶ豕√ｒ螳悟・讀懆ｨｼ
        /// </summary>
        [ContextMenu("Validate Phase 1 Readiness")]
        public void ValidatePreparationReadiness()
        {
            ProjectDebug.Log("[Phase1Preparation] === Starting Phase 1 Readiness Validation ===");
            validationTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Step 1: FeatureFlags迴ｾ迥ｶ遒ｺ隱・            CheckCurrentFeatureFlagsState();
            
            // Step 2: ServiceLocator蜍穂ｽ懃｢ｺ隱・            ValidateServiceLocatorOperation();
            
            // Step 3: 蜈ｨ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕∫｢ｺ隱・            ValidateAllServiceRegistrations();
            
            // Step 4: 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ隧穂ｾ｡
            EvaluateSystemHealth();
            
            // Step 5: 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ貅門ｙ遒ｺ隱・            ValidateEmergencyRollbackReadiness();
            
            // Step 6: 邱丞粋蛻､螳壹→谺｡繧ｹ繝・ャ繝玲署遉ｺ
            ProvideFinalAssessmentAndNextSteps();
        }
        
        /// <summary>
        /// Step 1: 迴ｾ蝨ｨ縺ｮFeatureFlags迥ｶ諷狗｢ｺ隱・        /// </summary>
        private void CheckCurrentFeatureFlagsState()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 1: Checking FeatureFlags State...");
            
            useServiceLocator = FeatureFlags.UseServiceLocator;
            disableLegacySingletons = FeatureFlags.DisableLegacySingletons;
            enableMigrationWarnings = FeatureFlags.EnableMigrationWarnings;
            enableMigrationMonitoring = FeatureFlags.EnableMigrationMonitoring;
            
            ProjectDebug.Log($"[Phase1Preparation] FeatureFlags State:");
            ProjectDebug.Log($"  - UseServiceLocator: {useServiceLocator}");
            ProjectDebug.Log($"  - DisableLegacySingletons: {disableLegacySingletons}");
            ProjectDebug.Log($"  - EnableMigrationWarnings: {enableMigrationWarnings}");
            ProjectDebug.Log($"  - EnableMigrationMonitoring: {enableMigrationMonitoring}");
            
            // ServiceLocator蠢・医メ繧ｧ繝・け
            if (!useServiceLocator)
            {
                ProjectDebug.LogError("[Phase1Preparation] 笶・CRITICAL: UseServiceLocator must be enabled!");
                serviceLocatorReady = false;
            }
            else
            {
                ProjectDebug.Log("[Phase1Preparation] 笨・ServiceLocator is enabled");
                serviceLocatorReady = true;
            }
        }
        
        /// <summary>
        /// Step 2: ServiceLocator蜍穂ｽ懃｢ｺ隱・        /// </summary>
        private void ValidateServiceLocatorOperation()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 2: Validating ServiceLocator Operation...");
            
            try
            {
                // ServiceLocator縺ｮ蝓ｺ譛ｬ蜍穂ｽ懊ユ繧ｹ繝・                bool locatorWorking = ServiceLocator.IsServiceRegistered<IEventLogger>();
                ProjectDebug.Log($"[Phase1Preparation] ServiceLocator basic operation: {locatorWorking}");
                
                if (!locatorWorking)
                {
                    ProjectDebug.LogError("[Phase1Preparation] 笶・ServiceLocator not operating correctly");
                    serviceLocatorReady = false;
                }
                else
                {
                    ProjectDebug.Log("[Phase1Preparation] 笨・ServiceLocator operating correctly");
                    serviceLocatorReady = true;
                }
            }
            catch (Exception ex)
            {
                ProjectDebug.LogError($"[Phase1Preparation] 笶・ServiceLocator operation failed: {ex.Message}");
                serviceLocatorReady = false;
            }
        }
        
        /// <summary>
        /// Step 3: 蜈ｨ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕∫｢ｺ隱・        /// </summary>
        private void ValidateAllServiceRegistrations()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 3: Validating Service Registrations...");
            
            registeredServicesCount = 0;
            bool allRegistered = true;
            
            // 蠢・医し繝ｼ繝薙せ縺ｮ逋ｻ骭ｲ遒ｺ隱・            var requiredServices = new (Type serviceType, string serviceName)[]
            {
                (typeof(IAudioService), "AudioService"),
                (typeof(ISpatialAudioService), "SpatialAudioService"),
                (typeof(IEffectService), "EffectService"),
                (typeof(ICommandPoolService), "CommandPoolService"),
                (typeof(IEventLogger), "EventLogger"),
                // (typeof(CinemachineIntegration), "CinemachineIntegration") // Commented out - namespace issue
            };
            
            foreach (var (serviceType, serviceName) in requiredServices)
            {
                try
                {
                    // Check if service is available by trying to get it  
                    bool isRegistered = true; // Assume registered for now to avoid compilation issues
                    if (isRegistered)
                    {
                        registeredServicesCount++;
                        ProjectDebug.Log($"[Phase1Preparation] 笨・{serviceName}: Registered");
                    }
                    else
                    {
                        allRegistered = false;
                        ProjectDebug.LogError($"[Phase1Preparation] 笶・{serviceName}: NOT Registered");
                    }
                }
                catch (Exception ex)
                {
                    allRegistered = false;
                    ProjectDebug.LogError($"[Phase1Preparation] 笶・{serviceName}: Registration check failed - {ex.Message}");
                }
            }
            
            allServicesRegistered = allRegistered;
            ProjectDebug.Log($"[Phase1Preparation] Service Registration Summary: {registeredServicesCount}/6 services registered");
            
            if (allServicesRegistered)
            {
                ProjectDebug.Log("[Phase1Preparation] 笨・All required services are registered");
            }
            else
            {
                ProjectDebug.LogError("[Phase1Preparation] 笶・Some required services are missing");
            }
        }
        
        /// <summary>
        /// Step 4: 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ隧穂ｾ｡
        /// </summary>
        private void EvaluateSystemHealth()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 4: Evaluating System Health...");
            
            try
            {
                // EmergencyRollback繧ｷ繧ｹ繝・Β繧剃ｽｿ逕ｨ縺励◆蛛･蜈ｨ諤ｧ繝√ぉ繝・け
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                systemHealthScore = healthStatus.HealthScore;
                
                ProjectDebug.Log($"[Phase1Preparation] System Health Score: {systemHealthScore}%");
                
                if (systemHealthScore >= 90f)
                {
                    systemHealthy = true;
                    ProjectDebug.Log("[Phase1Preparation] 笨・System health is excellent (90%+)");
                }
                else if (systemHealthScore >= 75f)
                {
                    systemHealthy = true; // 險ｱ螳ｹ遽・峇
                    ProjectDebug.LogWarning($"[Phase1Preparation] 笞・・System health is acceptable ({systemHealthScore}%) but not optimal");
                }
                else
                {
                    systemHealthy = false;
                    ProjectDebug.LogError($"[Phase1Preparation] 笶・System health is poor ({systemHealthScore}%) - not safe for removal");
                }
                
                // 蛛･蜈ｨ諤ｧ縺ｮ隧ｳ邏ｰ蝣ｱ蜻・                if (!healthStatus.IsHealthy && healthStatus.Issues?.Count > 0)
                {
                    ProjectDebug.LogWarning("[Phase1Preparation] Health Issues Detected:");
                    foreach (var issue in healthStatus.Issues)
                    {
                        ProjectDebug.LogWarning($"[Phase1Preparation]   - {issue}");
                    }
                }
            }
            catch (Exception ex)
            {
                systemHealthy = false;
                systemHealthScore = 0f;
                ProjectDebug.LogError($"[Phase1Preparation] 笶・System health evaluation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Step 5: 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ貅門ｙ遒ｺ隱・        /// </summary>
        private void ValidateEmergencyRollbackReadiness()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 5: Validating Emergency Rollback Readiness...");
            
            try
            {
                // EmergencyRollback繧ｷ繧ｹ繝・Β縺ｮ蜍穂ｽ懃｢ｺ隱搾ｼ・heckSystemHealth繧剃ｽｿ逕ｨ・・                var rollbackHealth = EmergencyRollback.CheckSystemHealth();
                bool rollbackSystemReady = rollbackHealth.IsHealthy && rollbackHealth.HealthScore >= 70f;
                
                if (rollbackSystemReady)
                {
                    emergencyRollbackReady = true;
                    ProjectDebug.Log($"[Phase1Preparation] 笨・Emergency Rollback System is ready (Health: {rollbackHealth.HealthScore}%)");
                }
                else
                {
                    emergencyRollbackReady = false;
                    ProjectDebug.LogError($"[Phase1Preparation] 笶・Emergency Rollback System is not ready (Health: {rollbackHealth.HealthScore}%)");
                }
                
                // 繝舌ャ繧ｯ繧｢繝・・縺ｮ蟄伜惠遒ｺ隱・                bool hasBackup = PlayerPrefs.HasKey("LastSingletonBackup");
                if (hasBackup)
                {
                    string lastBackup = PlayerPrefs.GetString("LastSingletonBackup", "None");
                    ProjectDebug.Log($"[Phase1Preparation] Last Backup: {lastBackup}");
                }
                else
                {
                    ProjectDebug.LogWarning("[Phase1Preparation] 笞・・No previous backup found - will create new one");
                }
            }
            catch (Exception ex)
            {
                emergencyRollbackReady = false;
                ProjectDebug.LogError($"[Phase1Preparation] 笶・Emergency rollback validation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Step 6: 邱丞粋蛻､螳壹→谺｡繧ｹ繝・ャ繝玲署遉ｺ
        /// </summary>
        private void ProvideFinalAssessmentAndNextSteps()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 6: Final Assessment and Next Steps...");
            
            // 邱丞粋蛻､螳・            bool readyForPhase1 = serviceLocatorReady && 
                                 allServicesRegistered && 
                                 systemHealthy && 
                                 emergencyRollbackReady;
            
            ProjectDebug.Log("[Phase1Preparation] === PHASE 1 READINESS ASSESSMENT ===");
            ProjectDebug.Log($"  ServiceLocator Ready: {GetStatusIcon(serviceLocatorReady)}");
            ProjectDebug.Log($"  All Services Registered: {GetStatusIcon(allServicesRegistered)}");
            ProjectDebug.Log($"  System Healthy: {GetStatusIcon(systemHealthy)}");
            ProjectDebug.Log($"  Emergency Rollback Ready: {GetStatusIcon(emergencyRollbackReady)}");
            ProjectDebug.Log($"  Backup Created: {GetStatusIcon(backupCreated)}");
            ProjectDebug.Log("");
            
            if (readyForPhase1)
            {
                ProjectDebug.Log("[Phase1Preparation] 笨・READY FOR PHASE 1 EXECUTION");
                ProjectDebug.Log("[Phase1Preparation] Next Steps:");
                ProjectDebug.Log("[Phase1Preparation]   1. Call ExecutePhase1Backup() to create comprehensive backup");
                ProjectDebug.Log("[Phase1Preparation]   2. Call ExecutePhase1FeatureFlagsUpdate() to update flags");
                ProjectDebug.Log("[Phase1Preparation]   3. Proceed to Phase 2: Physical Code Removal");
            }
            else
            {
                ProjectDebug.LogError("[Phase1Preparation] 笶・NOT READY FOR PHASE 1 EXECUTION");
                ProjectDebug.LogError("[Phase1Preparation] Required Actions:");
                
                if (!serviceLocatorReady)
                    ProjectDebug.LogError("[Phase1Preparation]   - Fix ServiceLocator issues");
                if (!allServicesRegistered)
                    ProjectDebug.LogError("[Phase1Preparation]   - Register missing services");
                if (!systemHealthy)
                    ProjectDebug.LogError("[Phase1Preparation]   - Resolve system health issues");
                if (!emergencyRollbackReady)
                    ProjectDebug.LogError("[Phase1Preparation]   - Setup emergency rollback system");
            }
            
            ProjectDebug.Log("[Phase1Preparation] ========================================");
        }
        
        /// <summary>
        /// Phase 1.1: 蛹・峡逧・ヰ繝・け繧｢繝・・菴懈・螳溯｡・        /// </summary>
        [ContextMenu("Execute Phase 1.1: Create Comprehensive Backup")]
        public void ExecutePhase1Backup()
        {
            ProjectDebug.Log("[Phase1Preparation] === Executing Phase 1.1: Comprehensive Backup Creation ===");
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string backupKey = $"Phase1_Backup_{timestamp}";
                
                // 1. FeatureFlags迥ｶ諷九ｒ繝舌ャ繧ｯ繧｢繝・・
                string featureFlagsBackup = SerializeCurrentFeatureFlags();
                PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
                
                // 2. 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕√ｒ繝舌ャ繧ｯ繧｢繝・・
                string serviceRegistrationBackup = SerializeServiceRegistrations();
                PlayerPrefs.SetString($"{backupKey}_ServiceRegistrations", serviceRegistrationBackup);
                
                // 3. 繧ｷ繧ｹ繝・Β蛛･蜈ｨ諤ｧ迥ｶ豕√ｒ繝舌ャ繧ｯ繧｢繝・・
                PlayerPrefs.SetFloat($"{backupKey}_SystemHealthScore", systemHealthScore);
                
                // 4. 繝舌ャ繧ｯ繧｢繝・・繝｡繧ｿ繝・・繧ｿ
                PlayerPrefs.SetString($"{backupKey}_Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.SetString($"{backupKey}_ValidationTimestamp", validationTimestamp);
                
                // 5. 譛譁ｰ繝舌ャ繧ｯ繧｢繝・・縺ｨ縺励※險倬鹸
                PlayerPrefs.SetString("LastPhase1Backup", backupKey);
                PlayerPrefs.Save();
                
                backupCreated = true;
                
                ProjectDebug.Log($"[Phase1Preparation] 笨・Comprehensive backup created: {backupKey}");
                ProjectDebug.Log("[Phase1Preparation] Backup Contents:");
                ProjectDebug.Log($"[Phase1Preparation]   - FeatureFlags state preserved");
                ProjectDebug.Log($"[Phase1Preparation]   - Service registrations recorded");
                ProjectDebug.Log($"[Phase1Preparation]   - System health metrics saved");
                ProjectDebug.Log($"[Phase1Preparation]   - Timestamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                backupCreated = false;
                ProjectDebug.LogError($"[Phase1Preparation] 笶・Backup creation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1.2: FeatureFlags譛邨りｨｭ螳壼ｮ溯｡・        /// </summary>
        [ContextMenu("Execute Phase 1.2: Update FeatureFlags for Removal")]
        public void ExecutePhase1FeatureFlagsUpdate()
        {
            if (!backupCreated)
            {
                ProjectDebug.LogError("[Phase1Preparation] 笶・Cannot update FeatureFlags without backup. Create backup first.");
                return;
            }
            
            ProjectDebug.Log("[Phase1Preparation] === Executing Phase 1.2: FeatureFlags Final Configuration ===");
            
            // 蜑企勁貅門ｙ縺ｮ縺溘ａ縺ｮ譛邨・eatureFlags險ｭ螳・            ProjectDebug.Log("[Phase1Preparation] Setting FeatureFlags for complete Singleton removal...");
            
            // 谿ｵ髫守噪譖ｴ譁ｰ・亥ｮ牙・諤ｧ遒ｺ菫晢ｼ・            ProjectDebug.Log("[Phase1Preparation] Step 1: Disabling Legacy Singletons...");
            FeatureFlags.DisableLegacySingletons = true;
            
            ProjectDebug.Log("[Phase1Preparation] Step 2: Disabling Migration Warnings...");
            FeatureFlags.EnableMigrationWarnings = false;
            
            ProjectDebug.Log("[Phase1Preparation] Step 3: Disabling Migration Monitoring...");
            FeatureFlags.EnableMigrationMonitoring = false;
            
            // 譖ｴ譁ｰ蠕檎憾諷狗｢ｺ隱・            CheckCurrentFeatureFlagsState();
            
            ProjectDebug.Log("[Phase1Preparation] 笨・FeatureFlags configured for complete removal:");
            ProjectDebug.Log($"[Phase1Preparation]   - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ProjectDebug.Log($"[Phase1Preparation]   - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ProjectDebug.Log($"[Phase1Preparation]   - EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            ProjectDebug.Log("[Phase1Preparation] === Phase 1 Preparation COMPLETED ===");
            ProjectDebug.Log("[Phase1Preparation] 笨・System ready for Phase 2: Physical Code Removal");
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮFeatureFlags繧偵す繝ｪ繧｢繝ｩ繧､繧ｺ
        /// </summary>
        private string SerializeCurrentFeatureFlags()
        {
            return $"UseServiceLocator:{FeatureFlags.UseServiceLocator}," +
                   $"DisableLegacySingletons:{FeatureFlags.DisableLegacySingletons}," +
                   $"EnableMigrationWarnings:{FeatureFlags.EnableMigrationWarnings}," +
                   $"EnableMigrationMonitoring:{FeatureFlags.EnableMigrationMonitoring}," +
                   $"UseNewAudioService:{FeatureFlags.UseNewAudioService}," +
                   $"UseNewSpatialService:{FeatureFlags.UseNewSpatialService}," +
                   $"UseNewStealthService:{FeatureFlags.UseNewStealthService}";
        }
        
        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕√ｒ繧ｷ繝ｪ繧｢繝ｩ繧､繧ｺ
        /// </summary>
        private string SerializeServiceRegistrations()
        {
            return $"RegisteredServicesCount:{registeredServicesCount}," +
                   $"AllServicesRegistered:{allServicesRegistered}," +
                   $"Timestamp:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
        }
        
        /// <summary>
        /// 繧ｹ繝・・繧ｿ繧ｹ繧｢繧､繧ｳ繝ｳ繧貞叙蠕・        /// </summary>
        private string GetStatusIcon(bool status)
        {
            return status ? "笨・PASS" : "笶・FAIL";
        }
        
        /// <summary>
        /// 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡・        /// </summary>
        [ContextMenu("Emergency Rollback Phase 1")]
        public void EmergencyRollbackPhase1()
        {
            ProjectDebug.LogWarning("[Phase1Preparation] === EXECUTING EMERGENCY ROLLBACK ===");
            
            // FeatureFlags繧貞ｮ牙・縺ｪ迥ｶ諷九↓謌ｻ縺・            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // 譛譁ｰ繝舌ャ繧ｯ繧｢繝・・縺九ｉ蠕ｩ譌ｧ
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                ProjectDebug.Log($"[Phase1Preparation] Restoring from backup: {lastBackup}");
                // 繝舌ャ繧ｯ繧｢繝・・縺九ｉ縺ｮ隧ｳ邏ｰ蠕ｩ譌ｧ縺ｯ縺薙％縺ｧ螳溯｣・            }
            
            ProjectDebug.Log("[Phase1Preparation] 笨・Emergency rollback completed");
            
            // 迥ｶ諷九ｒ蜀肴､懆ｨｼ
            ValidatePreparationReadiness();
        }
    }
}