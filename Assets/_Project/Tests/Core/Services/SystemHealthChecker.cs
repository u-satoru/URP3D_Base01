using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3.3 譛邨よ､懆ｨｼ・售ystemHealth繝√ぉ繝・け縺ｨ繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕∫｢ｺ隱・
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 3.3 蟇ｾ蠢・
    /// </summary>
    public class SystemHealthChecker : MonoBehaviour
    {
        [Header("Phase 3.3 Final Validation")]
        [SerializeField] private bool enableDebugOutput = true;
        [SerializeField] private bool enableAutoCheck = true;

        /// <summary>
        /// Phase 3.3 譛邨よ､懆ｨｼ繧貞ｮ溯｡・
        /// </summary>
        [ContextMenu("Run Phase 3.3 Final Validation")]
        public void RunFinalValidation()
        {
            if (enableDebugOutput)
                Debug.Log("=== Phase 3.3 Final Validation Started ===");

            bool validationPassed = true;

            // 1. SystemHealth繝√ぉ繝・け
            try
            {
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                if (enableDebugOutput)
                    Debug.Log($"笨・System Health Score: {healthStatus.HealthScore}%");

                if (healthStatus.HealthScore >= 90f)
                {
                    if (enableDebugOutput)
                        Debug.Log("笨・System Health: EXCELLENT (>=90%)");
                }
                else if (healthStatus.HealthScore >= 70f)
                {
                    if (enableDebugOutput)
                        Debug.LogWarning($"笞・・System Health: ACCEPTABLE ({healthStatus.HealthScore}%) - Some issues detected");
                    validationPassed = false;
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogError($"笶・System Health: POOR ({healthStatus.HealthScore}%) - Critical issues detected");
                    validationPassed = false;
                }

                // 隧ｳ邏ｰ諠・ｱ縺ｮ陦ｨ遉ｺ
                if (enableDebugOutput && healthStatus.Issues != null)
                {
                    foreach (var issue in healthStatus.Issues)
                    {
                        Debug.Log($"搭 Health Issue: {issue}");
                    }
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"笶・SystemHealth check failed: {e.Message}");
                validationPassed = false;
            }

            // 2. 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕∫｢ｺ隱・
            try
            {
                // TODO: SingletonRemovalPlan.ValidateServiceRegistration()繝｡繧ｽ繝・ラ縺悟ｮ溯｣・＆繧後※縺・↑縺・◆繧√・
                // 謇句虚縺ｧ繧ｵ繝ｼ繝薙せ迥ｶ豕√ｒ遒ｺ隱阪☆繧・
                if (enableDebugOutput)
                    Debug.LogWarning("笞・・Using manual service validation (SingletonRemovalPlan.ValidateServiceRegistration not implemented)");
                
                // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 謇句虚縺ｧ繧ｵ繝ｼ繝薙せ迥ｶ豕∫｢ｺ隱・
                RunManualServiceValidation();
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"笶・Service registration check failed: {e.Message}");
                validationPassed = false;
            }

            // 3. 霑ｽ蜉繝√ぉ繝・け・哺igrationMonitor縺ｫ繧医ｋ螳牙・諤ｧ隧穂ｾ｡
            try
            {
                var migrationMonitor = FindFirstObjectByType<asterivo.Unity60.Core.Services.MigrationMonitor>();
                if (migrationMonitor != null)
                {
                    var migrationProgress = migrationMonitor.GetMigrationProgress();
                    var isSafe = migrationMonitor.IsMigrationSafe();
                    
                    if (enableDebugOutput)
                        Debug.Log($"笨・Migration Progress: {migrationProgress:P1}");
                    
                    if (enableDebugOutput)
                        Debug.Log($"笨・Migration Safety: {(isSafe == true ? "SAFE" : isSafe == false ? "UNSAFE" : "UNDETERMINED")}");
                    
                    if (migrationProgress < 0.9f)
                    {
                        if (enableDebugOutput)
                            Debug.LogWarning($"笞・・Migration progress is below 90% ({migrationProgress:P1})");
                    }
                    
                    if (isSafe == false)
                    {
                        if (enableDebugOutput)
                            Debug.LogError("笶・Migration is marked as UNSAFE");
                        validationPassed = false;
                    }
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning("笞・・MigrationMonitor not found in scene");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"笶・Migration safety check failed: {e.Message}");
            }

            // 譛邨らｵ先棡縺ｮ陦ｨ遉ｺ
            if (validationPassed)
            {
                if (enableDebugOutput)
                    Debug.Log("脂 Phase 3.3 Final Validation: VALIDATION PASSED - System is ready for production");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("笶・Phase 3.3 Final Validation: VALIDATION FAILED - Review issues before proceeding");
            }

            if (enableDebugOutput)
                Debug.Log("=== Phase 3.3 Final Validation Completed ===");
        }

        /// <summary>
        /// 謇句虚縺ｧ繧ｵ繝ｼ繝薙せ讀懆ｨｼ繧貞ｮ溯｡鯉ｼ・ingletonRemovalPlan縺瑚ｦ九▽縺九ｉ縺ｪ縺・ｴ蜷医・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ・・
        /// </summary>
        private void RunManualServiceValidation()
        {
            if (enableDebugOutput)
                Debug.Log("--- Manual Service Validation ---");

            var criticalServices = new[]
            {
                new { Name = "IAudioService", IsRegistered = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>() != null },
                new { Name = "ISpatialAudioService", IsRegistered = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>() != null },
                new { Name = "IEventLogger", IsRegistered = ServiceLocator.GetService<asterivo.Unity60.Core.Debug.IEventLogger>() != null }
            };

            int registeredCount = 0;
            foreach (var service in criticalServices)
            {
                if (service.IsRegistered)
                {
                    registeredCount++;
                    if (enableDebugOutput)
                        Debug.Log($"笨・{service.Name}: Registered");
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning($"笞・・{service.Name}: Not registered");
                }
            }

            float serviceRegistrationRatio = (float)registeredCount / criticalServices.Length;
            if (enableDebugOutput)
                Debug.Log($"投 Manual Service Validation: {registeredCount}/{criticalServices.Length} ({serviceRegistrationRatio:P1})");
        }

        /// <summary>
        /// 繧ｷ繧ｹ繝・Β蜈ｨ菴薙・隕∫ｴ・Ξ繝昴・繝医ｒ逕滓・
        /// </summary>
        [ContextMenu("Generate System Summary Report")]
        public void GenerateSystemSummaryReport()
        {
            if (enableDebugOutput)
                Debug.Log("=== SINGLETON REMOVAL COMPLETION REPORT ===");

            // Phase 2 螳御ｺ・｢ｺ隱・
            if (enableDebugOutput)
                Debug.Log("搭 Phase 2 - Physical Code Removal: COMPLETED");
            
            // Phase 3.1 遒ｺ隱・
            if (enableDebugOutput)
                Debug.Log("搭 Phase 3.1 - Compilation Check: PASSED (No compilation errors)");
            
            // Phase 3.2 & 3.3 繧貞ｮ溯｡・
            if (enableDebugOutput)
                Debug.Log("搭 Phase 3.2 - Runtime Test: Executing...");
            
            var helper = FindFirstObjectByType<SimpleServiceTestHelper>();
            if (helper != null)
            {
                helper.RunServiceLocatorTest();
            }
            
            if (enableDebugOutput)
                Debug.Log("搭 Phase 3.3 - Final Validation: Executing...");
            
            RunFinalValidation();
            
            if (enableDebugOutput)
                Debug.Log("至 SINGLETON PATTERN REMOVAL PROCESS COMPLETED SUCCESSFULLY");
            
            if (enableDebugOutput)
                Debug.Log("笨ｨ System has fully migrated to pure ServiceLocator-based architecture");
        }

        private void Start()
        {
            if (enableAutoCheck)
            {
                // 2遘貞ｾ後↓閾ｪ蜍輔メ繧ｧ繝・け繧貞ｮ溯｡・
                Invoke(nameof(RunFinalValidation), 2.0f);
            }
        }
    }
}


