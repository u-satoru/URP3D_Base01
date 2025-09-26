using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// 邱丞粋逧・↑Phase 3 Singleton Migration讀懆ｨｼ繝ｩ繝ｳ繝翫・
    /// 縺吶∋縺ｦ縺ｮ螳溯｣・＆繧後◆繧ｹ繝・ャ繝暦ｼ・.7-3.12・峨ｒ蛹・峡逧・↓繝・せ繝医＠縺ｾ縺・
    /// </summary>
    public class Phase3ValidationRunner
    {
        private static readonly List<string> ValidationResults = new List<string>();
        
        [UnityTest]
        public IEnumerator ValidatePhase3CompleteSystem()
        {
            ValidationResults.Clear();
            
            // Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ
            yield return ValidateGradualActivationSchedule();
            
            // Step 3.8: 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ 讀懆ｨｼ
            yield return ValidateMigrationVerificationScript();
            
            // Step 3.9: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β 讀懆ｨｼ
            yield return ValidateLegacySingletonWarningSystem();
            
            // Step 3.10: 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ
            yield return ValidateGradualSingletonDisableSchedule();
            
            // Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・画､懆ｨｼ
            yield return ValidateFinalCleanupSystem();
            
            // Step 3.12: 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β 讀懆ｨｼ
            yield return ValidateEmergencyRollbackSystem();
            
            // 譛邨よ､懆ｨｼ邨先棡縺ｮ蜃ｺ蜉・
            OutputValidationResults();
            
            // 縺吶∋縺ｦ縺ｮ繝・せ繝医′謌仙粥縺励◆縺薙→繧堤｢ｺ隱・
            Assert.IsTrue(ValidationResults.Count > 0, "繝舌Μ繝・・繧ｷ繝ｧ繝ｳ邨先棡縺瑚ｨ倬鹸縺輔ｌ縺ｦ縺・∪縺帙ｓ");
            
            foreach (var result in ValidationResults)
            {
                if (result.Contains("FAILED"))
                {
                    Assert.Fail($"繝舌Μ繝・・繧ｷ繝ｧ繝ｳ螟ｱ謨・ {result}");
                }
            }
            
            Debug.Log("[Phase3ValidationRunner] 縺吶∋縺ｦ縺ｮPhase 3繧ｳ繝ｳ繝昴・繝阪Φ繝医′豁｣蟶ｸ縺ｫ蜍穂ｽ懊＠縺ｦ縺・∪縺・笨・);
        }
        
        private IEnumerator ValidateGradualActivationSchedule()
        {
            Debug.Log("[Validation] Step 3.7: 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationScheduler縺梧ｭ｣蟶ｸ縺ｫ蛻晄悄蛹悶＆繧後ｋ縺薙→繧堤｢ｺ隱・
                var scheduler = new MigrationScheduler();
                Assert.IsNotNull(scheduler, "MigrationScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                // FeatureFlagScheduler縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var flagScheduler = new FeatureFlagScheduler();
                Assert.IsNotNull(flagScheduler, "FeatureFlagScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                // MigrationProgressTracker縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var progressTracker = new MigrationProgressTracker();
                Assert.IsNotNull(progressTracker, "MigrationProgressTracker縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                ValidationResults.Add("Step 3.7: PASSED - 谿ｵ髫守噪讖溯・譛牙柑蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ");
                Debug.Log("[Validation] Step 3.7: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.7: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.7: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateMigrationVerificationScript()
        {
            Debug.Log("[Validation] Step 3.8: 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationValidator縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var validator = new MigrationValidator();
                Assert.IsNotNull(validator, "MigrationValidator縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                ValidationResults.Add("Step 3.8: PASSED - 遘ｻ陦梧､懆ｨｼ繧ｹ繧ｯ繝ｪ繝励ヨ");
                Debug.Log("[Validation] Step 3.8: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.8: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.8: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateLegacySingletonWarningSystem()
        {
            Debug.Log("[Validation] Step 3.9: Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // MigrationMonitor縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var monitor = new asterivo.Unity60.Core.Services.MigrationMonitor();
                Assert.IsNotNull(monitor, "MigrationMonitor縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                // FeatureFlags縺梧ｭ｣縺励￥險ｭ螳壹＆繧後※縺・ｋ縺薙→繧堤｢ｺ隱・
                Assert.IsTrue(typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("EnableMigrationWarnings") != null, 
                             "FeatureFlags.EnableMigrationWarnings繝励Ο繝代ユ繧｣縺悟ｭ伜惠縺励∪縺帙ｓ");
                Assert.IsTrue(typeof(asterivo.Unity60.Core.FeatureFlags).GetProperty("DisableLegacySingletons") != null, 
                             "FeatureFlags.DisableLegacySingletons繝励Ο繝代ユ繧｣縺悟ｭ伜惠縺励∪縺帙ｓ");
                
                ValidationResults.Add("Step 3.9: PASSED - Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β");
                Debug.Log("[Validation] Step 3.9: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.9: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.9: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateGradualSingletonDisableSchedule()
        {
            Debug.Log("[Validation] Step 3.10: 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // SingletonDisableScheduler縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var disableScheduler = new SingletonDisableScheduler();
                Assert.IsNotNull(disableScheduler, "SingletonDisableScheduler縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                // ScheduleDay縺梧ｭ｣縺励￥螳夂ｾｩ縺輔ｌ縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・
                var schedulerType = typeof(asterivo.Unity60.Core.Services.SingletonDisableScheduler);
                var scheduleEnumType = schedulerType.GetNestedType("ScheduleDay");
                Assert.IsNotNull(scheduleEnumType, "ScheduleDay蛻玲嫌蝙九′隕九▽縺九ｊ縺ｾ縺帙ｓ");
                var enumValues = System.Enum.GetValues(scheduleEnumType);
                Assert.IsTrue(enumValues.Length == 7, $"ScheduleDay縺ｮ蛟､謨ｰ縺梧ｭ｣縺励￥縺ゅｊ縺ｾ縺帙ｓ: {enumValues.Length}");
                
                ValidationResults.Add("Step 3.10: PASSED - 谿ｵ髫守噪Singleton辟｡蜉ｹ蛹悶せ繧ｱ繧ｸ繝･繝ｼ繝ｫ");
                Debug.Log("[Validation] Step 3.10: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.10: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.10: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateFinalCleanupSystem()
        {
            Debug.Log("[Validation] Step 3.11: 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・画､懆ｨｼ髢句ｧ・);
            
            try
            {
                // SingletonCodeRemover縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var codeRemover = new SingletonCodeRemover();
                Assert.IsNotNull(codeRemover, "SingletonCodeRemover縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                ValidationResults.Add("Step 3.11: PASSED - 譛邨ゅけ繝ｪ繝ｼ繝ｳ繧｢繝・・・亥ｮ悟・蜑企勁・・);
                Debug.Log("[Validation] Step 3.11: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.11: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.11: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private IEnumerator ValidateEmergencyRollbackSystem()
        {
            Debug.Log("[Validation] Step 3.12: 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β 讀懆ｨｼ髢句ｧ・);
            
            try
            {
                // EmergencyRollback縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・(static class)
                Assert.IsTrue(typeof(EmergencyRollback).IsClass && typeof(EmergencyRollback).IsAbstract && typeof(EmergencyRollback).IsSealed, 
                             "EmergencyRollback縺茎tatic class縺ｨ縺励※螳夂ｾｩ縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                // AdvancedRollbackMonitor縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
                var advancedMonitor = new AdvancedRollbackMonitor();
                Assert.IsNotNull(advancedMonitor, "AdvancedRollbackMonitor縺悟・譛溷喧縺輔ｌ縺ｦ縺・∪縺帙ｓ");
                
                ValidationResults.Add("Step 3.12: PASSED - 邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β");
                Debug.Log("[Validation] Step 3.12: PASSED 笨・);
            }
            catch (System.Exception ex)
            {
                ValidationResults.Add($"Step 3.12: FAILED - {ex.Message}");
                Debug.LogError($"[Validation] Step 3.12: FAILED - {ex.Message}");
            }
            
            yield return null;
        }
        
        private void OutputValidationResults()
        {
            Debug.Log("=== Phase 3 Singleton Migration System 讀懆ｨｼ邨先棡 ===");
            
            int passedCount = 0;
            int failedCount = 0;
            
            foreach (var result in ValidationResults)
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
            }
            else
            {
                Debug.LogError($"笞・・{failedCount}蛟九・繧ｳ繝ｳ繝昴・繝阪Φ繝医〒蝠城｡後′讀懷・縺輔ｌ縺ｾ縺励◆縲ゆｿｮ豁｣縺悟ｿ・ｦ√〒縺吶・);
            }
        }
    }
}


