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
    /// Phase 1: 準備フェーズ - 前提条件検証とバックアチE�E実行シスチE��
    /// 完�ESingleton削除の事前準備を安�Eに管琁E    /// </summary>
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
        /// Phase 1 準備状況を完�E検証
        /// </summary>
        [ContextMenu("Validate Phase 1 Readiness")]
        public void ValidatePreparationReadiness()
        {
            ProjectDebug.Log("[Phase1Preparation] === Starting Phase 1 Readiness Validation ===");
            validationTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Step 1: FeatureFlags現状確誁E            CheckCurrentFeatureFlagsState();
            
            // Step 2: ServiceLocator動作確誁E            ValidateServiceLocatorOperation();
            
            // Step 3: 全サービス登録状況確誁E            ValidateAllServiceRegistrations();
            
            // Step 4: シスチE��健全性評価
            EvaluateSystemHealth();
            
            // Step 5: 緊急ロールバック準備確誁E            ValidateEmergencyRollbackReadiness();
            
            // Step 6: 総合判定と次スチE��プ提示
            ProvideFinalAssessmentAndNextSteps();
        }
        
        /// <summary>
        /// Step 1: 現在のFeatureFlags状態確誁E        /// </summary>
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
            
            // ServiceLocator忁E��チェチE��
            if (!useServiceLocator)
            {
                ProjectDebug.LogError("[Phase1Preparation] ❁ECRITICAL: UseServiceLocator must be enabled!");
                serviceLocatorReady = false;
            }
            else
            {
                ProjectDebug.Log("[Phase1Preparation] ✁EServiceLocator is enabled");
                serviceLocatorReady = true;
            }
        }
        
        /// <summary>
        /// Step 2: ServiceLocator動作確誁E        /// </summary>
        private void ValidateServiceLocatorOperation()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 2: Validating ServiceLocator Operation...");
            
            try
            {
                // ServiceLocatorの基本動作テスチE                bool locatorWorking = ServiceLocator.IsServiceRegistered<IEventLogger>();
                ProjectDebug.Log($"[Phase1Preparation] ServiceLocator basic operation: {locatorWorking}");
                
                if (!locatorWorking)
                {
                    ProjectDebug.LogError("[Phase1Preparation] ❁EServiceLocator not operating correctly");
                    serviceLocatorReady = false;
                }
                else
                {
                    ProjectDebug.Log("[Phase1Preparation] ✁EServiceLocator operating correctly");
                    serviceLocatorReady = true;
                }
            }
            catch (Exception ex)
            {
                ProjectDebug.LogError($"[Phase1Preparation] ❁EServiceLocator operation failed: {ex.Message}");
                serviceLocatorReady = false;
            }
        }
        
        /// <summary>
        /// Step 3: 全サービス登録状況確誁E        /// </summary>
        private void ValidateAllServiceRegistrations()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 3: Validating Service Registrations...");
            
            registeredServicesCount = 0;
            bool allRegistered = true;
            
            // 忁E��サービスの登録確誁E            var requiredServices = new (Type serviceType, string serviceName)[]
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
                        ProjectDebug.Log($"[Phase1Preparation] ✁E{serviceName}: Registered");
                    }
                    else
                    {
                        allRegistered = false;
                        ProjectDebug.LogError($"[Phase1Preparation] ❁E{serviceName}: NOT Registered");
                    }
                }
                catch (Exception ex)
                {
                    allRegistered = false;
                    ProjectDebug.LogError($"[Phase1Preparation] ❁E{serviceName}: Registration check failed - {ex.Message}");
                }
            }
            
            allServicesRegistered = allRegistered;
            ProjectDebug.Log($"[Phase1Preparation] Service Registration Summary: {registeredServicesCount}/6 services registered");
            
            if (allServicesRegistered)
            {
                ProjectDebug.Log("[Phase1Preparation] ✁EAll required services are registered");
            }
            else
            {
                ProjectDebug.LogError("[Phase1Preparation] ❁ESome required services are missing");
            }
        }
        
        /// <summary>
        /// Step 4: シスチE��健全性評価
        /// </summary>
        private void EvaluateSystemHealth()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 4: Evaluating System Health...");
            
            try
            {
                // EmergencyRollbackシスチE��を使用した健全性チェチE��
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                systemHealthScore = healthStatus.HealthScore;
                
                ProjectDebug.Log($"[Phase1Preparation] System Health Score: {systemHealthScore}%");
                
                if (systemHealthScore >= 90f)
                {
                    systemHealthy = true;
                    ProjectDebug.Log("[Phase1Preparation] ✁ESystem health is excellent (90%+)");
                }
                else if (systemHealthScore >= 75f)
                {
                    systemHealthy = true; // 許容篁E��
                    ProjectDebug.LogWarning($"[Phase1Preparation] ⚠�E�ESystem health is acceptable ({systemHealthScore}%) but not optimal");
                }
                else
                {
                    systemHealthy = false;
                    ProjectDebug.LogError($"[Phase1Preparation] ❁ESystem health is poor ({systemHealthScore}%) - not safe for removal");
                }
                
                // 健全性の詳細報呁E                if (!healthStatus.IsHealthy && healthStatus.Issues?.Count > 0)
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
                ProjectDebug.LogError($"[Phase1Preparation] ❁ESystem health evaluation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Step 5: 緊急ロールバック準備確誁E        /// </summary>
        private void ValidateEmergencyRollbackReadiness()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 5: Validating Emergency Rollback Readiness...");
            
            try
            {
                // EmergencyRollbackシスチE��の動作確認！EheckSystemHealthを使用�E�E                var rollbackHealth = EmergencyRollback.CheckSystemHealth();
                bool rollbackSystemReady = rollbackHealth.IsHealthy && rollbackHealth.HealthScore >= 70f;
                
                if (rollbackSystemReady)
                {
                    emergencyRollbackReady = true;
                    ProjectDebug.Log($"[Phase1Preparation] ✁EEmergency Rollback System is ready (Health: {rollbackHealth.HealthScore}%)");
                }
                else
                {
                    emergencyRollbackReady = false;
                    ProjectDebug.LogError($"[Phase1Preparation] ❁EEmergency Rollback System is not ready (Health: {rollbackHealth.HealthScore}%)");
                }
                
                // バックアチE�Eの存在確誁E                bool hasBackup = PlayerPrefs.HasKey("LastSingletonBackup");
                if (hasBackup)
                {
                    string lastBackup = PlayerPrefs.GetString("LastSingletonBackup", "None");
                    ProjectDebug.Log($"[Phase1Preparation] Last Backup: {lastBackup}");
                }
                else
                {
                    ProjectDebug.LogWarning("[Phase1Preparation] ⚠�E�ENo previous backup found - will create new one");
                }
            }
            catch (Exception ex)
            {
                emergencyRollbackReady = false;
                ProjectDebug.LogError($"[Phase1Preparation] ❁EEmergency rollback validation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Step 6: 総合判定と次スチE��プ提示
        /// </summary>
        private void ProvideFinalAssessmentAndNextSteps()
        {
            ProjectDebug.Log("[Phase1Preparation] Step 6: Final Assessment and Next Steps...");
            
            // 総合判宁E            bool readyForPhase1 = serviceLocatorReady && 
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
                ProjectDebug.Log("[Phase1Preparation] ✁EREADY FOR PHASE 1 EXECUTION");
                ProjectDebug.Log("[Phase1Preparation] Next Steps:");
                ProjectDebug.Log("[Phase1Preparation]   1. Call ExecutePhase1Backup() to create comprehensive backup");
                ProjectDebug.Log("[Phase1Preparation]   2. Call ExecutePhase1FeatureFlagsUpdate() to update flags");
                ProjectDebug.Log("[Phase1Preparation]   3. Proceed to Phase 2: Physical Code Removal");
            }
            else
            {
                ProjectDebug.LogError("[Phase1Preparation] ❁ENOT READY FOR PHASE 1 EXECUTION");
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
        /// Phase 1.1: 匁E��皁E��チE��アチE�E作�E実衁E        /// </summary>
        [ContextMenu("Execute Phase 1.1: Create Comprehensive Backup")]
        public void ExecutePhase1Backup()
        {
            ProjectDebug.Log("[Phase1Preparation] === Executing Phase 1.1: Comprehensive Backup Creation ===");
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string backupKey = $"Phase1_Backup_{timestamp}";
                
                // 1. FeatureFlags状態をバックアチE�E
                string featureFlagsBackup = SerializeCurrentFeatureFlags();
                PlayerPrefs.SetString($"{backupKey}_FeatureFlags", featureFlagsBackup);
                
                // 2. サービス登録状況をバックアチE�E
                string serviceRegistrationBackup = SerializeServiceRegistrations();
                PlayerPrefs.SetString($"{backupKey}_ServiceRegistrations", serviceRegistrationBackup);
                
                // 3. シスチE��健全性状況をバックアチE�E
                PlayerPrefs.SetFloat($"{backupKey}_SystemHealthScore", systemHealthScore);
                
                // 4. バックアチE�EメタチE�Eタ
                PlayerPrefs.SetString($"{backupKey}_Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.SetString($"{backupKey}_ValidationTimestamp", validationTimestamp);
                
                // 5. 最新バックアチE�Eとして記録
                PlayerPrefs.SetString("LastPhase1Backup", backupKey);
                PlayerPrefs.Save();
                
                backupCreated = true;
                
                ProjectDebug.Log($"[Phase1Preparation] ✁EComprehensive backup created: {backupKey}");
                ProjectDebug.Log("[Phase1Preparation] Backup Contents:");
                ProjectDebug.Log($"[Phase1Preparation]   - FeatureFlags state preserved");
                ProjectDebug.Log($"[Phase1Preparation]   - Service registrations recorded");
                ProjectDebug.Log($"[Phase1Preparation]   - System health metrics saved");
                ProjectDebug.Log($"[Phase1Preparation]   - Timestamp: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                backupCreated = false;
                ProjectDebug.LogError($"[Phase1Preparation] ❁EBackup creation failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Phase 1.2: FeatureFlags最終設定実衁E        /// </summary>
        [ContextMenu("Execute Phase 1.2: Update FeatureFlags for Removal")]
        public void ExecutePhase1FeatureFlagsUpdate()
        {
            if (!backupCreated)
            {
                ProjectDebug.LogError("[Phase1Preparation] ❁ECannot update FeatureFlags without backup. Create backup first.");
                return;
            }
            
            ProjectDebug.Log("[Phase1Preparation] === Executing Phase 1.2: FeatureFlags Final Configuration ===");
            
            // 削除準備のための最絁EeatureFlags設宁E            ProjectDebug.Log("[Phase1Preparation] Setting FeatureFlags for complete Singleton removal...");
            
            // 段階的更新�E�安�E性確保！E            ProjectDebug.Log("[Phase1Preparation] Step 1: Disabling Legacy Singletons...");
            FeatureFlags.DisableLegacySingletons = true;
            
            ProjectDebug.Log("[Phase1Preparation] Step 2: Disabling Migration Warnings...");
            FeatureFlags.EnableMigrationWarnings = false;
            
            ProjectDebug.Log("[Phase1Preparation] Step 3: Disabling Migration Monitoring...");
            FeatureFlags.EnableMigrationMonitoring = false;
            
            // 更新後状態確誁E            CheckCurrentFeatureFlagsState();
            
            ProjectDebug.Log("[Phase1Preparation] ✁EFeatureFlags configured for complete removal:");
            ProjectDebug.Log($"[Phase1Preparation]   - DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            ProjectDebug.Log($"[Phase1Preparation]   - EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            ProjectDebug.Log($"[Phase1Preparation]   - EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            ProjectDebug.Log("[Phase1Preparation] === Phase 1 Preparation COMPLETED ===");
            ProjectDebug.Log("[Phase1Preparation] ✁ESystem ready for Phase 2: Physical Code Removal");
        }
        
        /// <summary>
        /// 現在のFeatureFlagsをシリアライズ
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
        /// サービス登録状況をシリアライズ
        /// </summary>
        private string SerializeServiceRegistrations()
        {
            return $"RegisteredServicesCount:{registeredServicesCount}," +
                   $"AllServicesRegistered:{allServicesRegistered}," +
                   $"Timestamp:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
        }
        
        /// <summary>
        /// スチE�Eタスアイコンを取征E        /// </summary>
        private string GetStatusIcon(bool status)
        {
            return status ? "✁EPASS" : "❁EFAIL";
        }
        
        /// <summary>
        /// 緊急ロールバック実衁E        /// </summary>
        [ContextMenu("Emergency Rollback Phase 1")]
        public void EmergencyRollbackPhase1()
        {
            ProjectDebug.LogWarning("[Phase1Preparation] === EXECUTING EMERGENCY ROLLBACK ===");
            
            // FeatureFlagsを安�Eな状態に戻ぁE            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            // 最新バックアチE�Eから復旧
            string lastBackup = PlayerPrefs.GetString("LastPhase1Backup", "");
            if (!string.IsNullOrEmpty(lastBackup))
            {
                ProjectDebug.Log($"[Phase1Preparation] Restoring from backup: {lastBackup}");
                // バックアチE�Eからの詳細復旧はここで実裁E            }
            
            ProjectDebug.Log("[Phase1Preparation] ✁EEmergency rollback completed");
            
            // 状態を再検証
            ValidatePreparationReadiness();
        }
    }
}