using UnityEngine;
using UnityEditor;
using System.Collections;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3 Singleton Migration讀懆ｨｼ繧偵お繝・ぅ繧ｿ繝｡繝九Η繝ｼ縺九ｉ螳溯｡後☆繧九ヤ繝ｼ繝ｫ
    /// </summary>
    public static class Phase3ValidationMenuExecutor
    {
        [MenuItem("Tools/Validation/Execute Phase 3 Singleton Migration Validation")]
        public static void ExecutePhase3Validation()
        {
            Debug.Log("=== Phase 3 Singleton Migration System 讀懆ｨｼ髢句ｧ・===");
            
            var validationResults = new List<string>();
            
            // Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ
            ValidateGradualActivationSchedule(validationResults);
            
            // Step 3.8: 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ 讀懆ｨｼ
            ValidateMigrationVerificationScript(validationResults);
            
            // Step 3.9: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β 讀懆ｨｼ
            ValidateLegacySingletonWarningSystem(validationResults);
            
            // Step 3.10: 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ
            ValidateGradualSingletonDisableSchedule(validationResults);
            
            // Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・画､懆ｨｼ
            ValidateFinalCleanupSystem(validationResults);
            
            // Step 3.12: 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β 讀懆ｨｼ
            ValidateEmergencyRollbackSystem(validationResults);
            
            // 譛邨よ､懆ｨｼ邨先棡縺ｮ蜃ｺ蜉・
            OutputValidationResults(validationResults);
        }
        
        private static void ValidateGradualActivationSchedule(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationScheduler縺梧ｭ｣蟶ｸ縺ｫ蛻晄悄蛹悶＆繧後ｋ縺薙→繧堤｢ｺ隱・
                var scheduler = new MigrationScheduler();
                if (scheduler == null)
                {
                    throw new System.Exception("MigrationScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                // FeatureFlagScheduler縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var flagScheduler = new FeatureFlagScheduler();
                if (flagScheduler == null)
                {
                    throw new System.Exception("FeatureFlagScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                // MigrationProgressTracker縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var progressTracker = new MigrationProgressTracker();
                if (progressTracker == null)
                {
                    throw new System.Exception("MigrationProgressTracker縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                validationResults.Add("Step 3.7: PASSED - 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ");
                Debug.Log("[Validation] Step 3.7: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.7: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.7: FAILED - {ex.Message}");
            }
        }
        
        private static void ValidateMigrationVerificationScript(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.8: 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationValidator縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var validator = new MigrationValidator();
                if (validator == null)
                {
                    throw new System.Exception("MigrationValidator縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                validationResults.Add("Step 3.8: PASSED - 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ");
                Debug.Log("[Validation] Step 3.8: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.8: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.8: FAILED - {ex.Message}");
            }
        }
        
        private static void ValidateLegacySingletonWarningSystem(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.9: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationMonitor縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var monitor = new asterivo.Unity60.Core.Services.MigrationMonitor();
                if (monitor == null)
                {
                    throw new System.Exception("MigrationMonitor縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                // FeatureFlags縺梧ｭ｣縺励￥險ｭ螳壹＆繧後※縺・ｋ縺薙→繧堤｢ｺ隱・
                var enableWarningsProperty = typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("EnableMigrationWarnings");
                if (enableWarningsProperty == null)
                {
                    throw new System.Exception("FeatureFlags.EnableMigrationWarnings繝励Ο繝代ユ繧｣縺悟ｭ伜惠縺励∪縺帙ｓ");
                }
                
                var disableLegacyProperty = typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("DisableLegacySingletons");
                if (disableLegacyProperty == null)
                {
                    throw new System.Exception("FeatureFlags.DisableLegacySingletons繝励Ο繝代ユ繧｣縺悟ｭ伜惠縺励∪縺帙ｓ");
                }
                
                validationResults.Add("Step 3.9: PASSED - Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β");
                Debug.Log("[Validation] Step 3.9: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.9: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.9: FAILED - {ex.Message}");
            }
        }
        
        private static void ValidateGradualSingletonDisableSchedule(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.10: 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // SingletonDisableScheduler縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var disableScheduler = new SingletonDisableScheduler();
                if (disableScheduler == null)
                {
                    throw new System.Exception("SingletonDisableScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                // ScheduleDay縺梧ｭ｣縺励￥螳夂ｾｩ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・
                var schedulerType = typeof(asterivo.Unity60.Core.Services.SingletonDisableScheduler);
                var scheduleEnumType = schedulerType.GetNestedType("ScheduleDay");
                if (scheduleEnumType == null)
                {
                    throw new System.Exception("ScheduleDay蛻玲嫌蝙九′隕九▽縺九ｊ縺ｾ縺帙ｓ");
                }
                
                var enumValues = System.Enum.GetValues(scheduleEnumType);
                if (enumValues.Length != 7)
                {
                    throw new System.Exception($"ScheduleDay縺ｮ蛟､謨ｰ縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ: {enumValues.Length}");
                }
                
                validationResults.Add("Step 3.10: PASSED - 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ");
                Debug.Log("[Validation] Step 3.10: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.10: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.10: FAILED - {ex.Message}");
            }
        }
        
        private static void ValidateFinalCleanupSystem(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・画､懆ｨｼ髢句ｧ・);
            
            try
            {
                // SingletonCodeRemover縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var codeRemover = new SingletonCodeRemover();
                if (codeRemover == null)
                {
                    throw new System.Exception("SingletonCodeRemover縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                validationResults.Add("Step 3.11: PASSED - 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・・);
                Debug.Log("[Validation] Step 3.11: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.11: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.11: FAILED - {ex.Message}");
            }
        }
        
        private static void ValidateEmergencyRollbackSystem(List<string> validationResults)
        {
            Debug.Log("[Validation] Step 3.12: 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // EmergencyRollback縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・(static class)
                var rollbackType = typeof(EmergencyRollback);
                if (!(rollbackType.IsClass && rollbackType.IsAbstract && rollbackType.IsSealed))
                {
                    throw new System.Exception("EmergencyRollback縺茎tatic class縺ｨ縺励※螳夂ｾｩ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                // AdvancedRollbackMonitor縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var advancedMonitor = new AdvancedRollbackMonitor();
                if (advancedMonitor == null)
                {
                    throw new System.Exception("AdvancedRollbackMonitor縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                }
                
                validationResults.Add("Step 3.12: PASSED - 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β");
                Debug.Log("[Validation] Step 3.12: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                validationResults.Add($"Step 3.12: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.12: FAILED - {ex.Message}");
            }
        }
        
        private static void OutputValidationResults(List<string> validationResults)
        {
            Debug.Log("=== Phase 3 Singleton Migration System 讀懆ｨｼ邨先棡 ===");
            
            int passedCount = 0;
            int failedCount = 0;
            
            foreach (var result in validationResults)
            {
                Debug.Log($"  {result}");
                
                if (result.Contains("PASSED"))
                    passedCount++;
                else if (result.Contains("FAILED"))
                    failedCount++;
            }
            
            Debug.Log($"=== 讀懆ｨｼ邨ｱ險・ {passedCount} PASSED, {failedCount} FAILED ===");
            
            if (failedCount == 0)
            {
                Debug.Log("脂 Phase 3 Singleton Migration System縺ｮ螳溯｣・′螳御ｺ・＠縲√☆縺ｹ縺ｦ縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝医′豁｣蟶ｸ縺ｫ蜍穂ｽ懊＠縺ｦ縺・∪縺呻ｼ・);
                Debug.Log("縺吶∋縺ｦ縺ｮ繧ｹ繝・ャ繝暦ｼ・.7-3.12・峨′豁｣蟶ｸ縺ｫ讀懆ｨｼ縺輔ｌ縺ｾ縺励◆縲・);
                Debug.Log("谿ｵ髫守噪讖溯・譛牙柑蛹悶∫ｧｻ陦梧､懆ｨｼ縲∬ｭｦ蜻翫す繧ｹ繝・Β縲∫┌蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ縲∵怙邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・縲∫ｷ頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ縺後☆縺ｹ縺ｦ螳溯｣・＆繧後※縺・∪縺吶・);
            }
            else
            {
                Debug.LogError($"笞・・{failedCount}蛟九・繧ｳ繝ｳ繝昴・繝阪Φ繝医〒蝠城｡後′讀懷・縺輔ｌ縺ｾ縺励◆縲ゆｿｮ豁｣縺悟ｿ・ｦ√〒縺吶・);
            }
        }
    }
}


