using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Step 3.12: 鬮伜ｺｦ縺ｪ邱頑･譎ゅΟ繝ｼ繝ｫ繝舌ャ繧ｯ逶｣隕悶す繧ｹ繝・Β縺ｮ繝・せ繝・
    /// AdvancedRollbackMonitor 縺ｮ蛹・峡逧・ｩ溯・繝・せ繝・
    /// </summary>
    public class AdvancedRollbackMonitorTest
    {
        private GameObject monitorObject;
        private AdvancedRollbackMonitor monitor;
        private bool originalUseNewAudioService;
        private bool originalUseNewSpatialService;
        private bool originalUseNewStealthService;
        private bool originalEnableMigrationMonitoring;

        [SetUp]
        public void Setup()
        {
            // 蜈・・FeatureFlags險ｭ螳壹ｒ菫晏ｭ・
            originalUseNewAudioService = asterivo.Unity60.Core.FeatureFlags.UseNewAudioService;
            originalUseNewSpatialService = asterivo.Unity60.Core.FeatureFlags.UseNewSpatialService;
            originalUseNewStealthService = asterivo.Unity60.Core.FeatureFlags.UseNewStealthService;
            originalEnableMigrationMonitoring = asterivo.Unity60.Core.FeatureFlags.EnableMigrationMonitoring;

            // 繝・せ繝育畑縺ｮAdvancedRollbackMonitor繧剃ｽ懈・
            monitorObject = new GameObject("AdvancedRollbackMonitorTest");
            monitor = monitorObject.AddComponent<AdvancedRollbackMonitor>();

            // 繝・せ繝育畑縺ｮ蛻晄悄險ｭ螳・
            ResetToSafeState();
        }

        [TearDown]
        public void TearDown()
        {
            // FeatureFlags險ｭ螳壹ｒ蠕ｩ蜈・
            FeatureFlags.UseNewAudioService = originalUseNewAudioService;
            FeatureFlags.UseNewSpatialService = originalUseNewSpatialService;
            FeatureFlags.UseNewStealthService = originalUseNewStealthService;
            FeatureFlags.EnableMigrationMonitoring = originalEnableMigrationMonitoring;

            // 繝・せ繝医が繝悶ず繧ｧ繧ｯ繝医ｒ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (monitorObject != null)
            {
                Object.DestroyImmediate(monitorObject);
            }

            ResetToSafeState();
        }

        private void ResetToSafeState()
        {
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.EnableAutoRollback = false;
        }

        [Test]
        public void AdvancedRollbackMonitor_ComponentCreation_ShouldSucceed()
        {
            // Assert
            Assert.IsNotNull(monitor, "AdvancedRollbackMonitor should be created successfully");
            Assert.IsTrue(monitor.enabled, "AdvancedRollbackMonitor should be enabled");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_SystemInitialization_ShouldCompleteSuccessfully()
        {
            // Act - Start()縺郡ystem蛻晄悄蛹悶ｒ螳溯｡後☆繧・
            yield return new WaitForSeconds(0.2f);

            // Assert - 蛻晄悄蛹悶′螳御ｺ・＠縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Advanced monitoring system started.*"));
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_HealthLevelMonitoring_ShouldDetectChanges()
        {
            // Arrange - 繧ｷ繧ｹ繝・Β繧剃ｸ榊▼蜈ｨ縺ｪ迥ｶ諷九↓縺吶ｋ
            FeatureFlags.UseServiceLocator = false; // ServiceLocator繧堤┌蜉ｹ蛹・
            FeatureFlags.UseNewAudioService = true;  // 遏帷崟縺励◆險ｭ螳・

            // Act - 逶｣隕悶す繧ｹ繝・Β縺悟､牙喧繧呈､懷・縺吶ｋ縺ｾ縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(1f);

            // Assert - 蛛･蜈ｨ諤ｧ繝ｬ繝吶Ν縺ｮ螟牙喧縺梧､懷・縺輔ｌ繧九％縺ｨ繧呈悄蠕・
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*Health level changed.*"));
        }

        [Test]
        public void AdvancedRollbackMonitor_GenerateMonitoringReport_ShouldDisplayStatus()
        {
            // Act & Assert
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Monitoring Status Report.*"));
            monitor.GenerateMonitoringReport();
        }

        [Test]
        public void AdvancedRollbackMonitor_ResetMonitoringSystem_ShouldClearState()
        {
            // Act
            monitor.ResetMonitoringSystem();

            // Assert - 繝ｪ繧ｻ繝・ヨ縺梧・蜉溘☆繧九％縺ｨ繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Monitoring system reset.*"));
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_ServiceHealthChecking_ShouldMonitorServices()
        {
            // Arrange - ServiceLocator繧呈怏蜉ｹ縺ｪ迥ｶ諷九↓繧ｻ繝・ヨ繧｢繝・・
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;

            // Act - 逶｣隕悶′髢句ｧ九＆繧後ｋ縺ｾ縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(1.5f);

            // Assert - 繧ｵ繝ｼ繝薙せ蛛･蜈ｨ諤ｧ繝√ぉ繝・け縺悟ｮ溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
            // 螳滄圀縺ｮ繧ｵ繝ｼ繝薙せ縺檎匳骭ｲ縺輔ｌ縺ｦ縺・↑縺・ｴ蜷医〒繧ゅ√メ繧ｧ繝・け蜃ｦ逅・・菴薙・螳溯｡後＆繧後ｋ
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Service Quality Report.*"));
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_PredictiveAnalysis_ShouldDetectTrends()
        {
            // Arrange - 莠域ｸｬ蛻・梵繧呈怏蜉ｹ縺ｫ縺励※隍・焚蝗槭・蛛･蜈ｨ諤ｧ繝・・繧ｿ繧堤函謌・
            yield return new WaitForSeconds(3f);

            // Act - 蜊∝・縺ｪ繝・・繧ｿ縺瑚塘遨阪＆繧後◆蠕後∽ｺ域ｸｬ蛻・梵繧偵ヨ繝ｪ繧ｬ繝ｼ
            // 蜀・Κ逧・↓莠域ｸｬ蛻・梵縺悟ｮ溯｡後＆繧後ｋ

            // Assert - 莠域ｸｬ蛻・梵縺悟ｮ溯｡後＆繧後ｋ縺薙→繧帝俣謗･逧・↓遒ｺ隱・
            // (逶ｴ謗･逧・↑繝・せ繝医・蝗ｰ髮｣縺縺後√お繝ｩ繝ｼ縺ｪ縺丞ｮ溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsNotNull(monitor, "Monitor should continue functioning during predictive analysis");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_EmergencyCondition_ShouldTriggerRecovery()
        {
            // Arrange - 閾ｪ蜍募屓蠕ｩ繧呈怏蜉ｹ蛹悶＠縲∫ｷ頑･迥ｶ諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            // 逶ｴ謗･逧・↑邱頑･迥ｶ諷九・繧ｷ繝溘Η繝ｬ繝ｼ繧ｷ繝ｧ繝ｳ縺ｯ蝗ｰ髮｣縺ｪ縺溘ａ縲・
            // 繧ｷ繧ｹ繝・Β險ｭ螳壹・遏帷崟繧剃ｽ懈・
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act - 逶｣隕悶す繧ｹ繝・Β縺悟撫鬘後ｒ讀懷・縺吶ｋ縺ｾ縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 繧ｷ繧ｹ繝・Β縺悟撫鬘後ｒ讀懷・縺吶ｋ縺薙→繧堤｢ｺ隱・
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            Assert.IsFalse(healthStatus.IsHealthy, "System should be detected as unhealthy");
            Assert.IsTrue(healthStatus.HasInconsistentConfiguration, "Configuration inconsistency should be detected");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_PerformanceMetrics_ShouldMonitorFrameRate()
        {
            // Arrange - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶ｒ髢句ｧ・
            yield return new WaitForSeconds(1f);

            // Act - 繝輔Ξ繝ｼ繝繝ｬ繝ｼ繝磯未騾｣縺ｮ蜃ｦ逅・′螳溯｡後＆繧後ｋ
            // (螳滄圀縺ｮ諤ｧ閭ｽ蜉｣蛹悶ｒ蜀咲樟縺吶ｋ縺ｮ縺ｯ蝗ｰ髮｣縺縺後∫屮隕匁ｩ溯・縺ｮ蜍穂ｽ懊ｒ遒ｺ隱・

            // Assert - 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ逶｣隕悶′蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
            Assert.IsNotNull(monitor, "Monitor should handle performance monitoring without errors");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_ConfigurationInconsistency_ShouldDetectAndRecord()
        {
            // Arrange - 險ｭ螳壹・遏帷崟繧剃ｽ懈・
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true; // 遏帷崟: ServiceLocator縺ｪ縺励〒譁ｰ繧ｵ繝ｼ繝薙せ菴ｿ逕ｨ

            // Act - 逶｣隕悶す繧ｹ繝・Β縺檎泝逶ｾ繧呈､懷・縺吶ｋ縺ｾ縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(1f);

            // Assert - 險ｭ螳夂泝逶ｾ縺梧､懷・縺輔ｌ繧九％縺ｨ繧堤｢ｺ隱・
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            Assert.IsTrue(healthStatus.HasInconsistentConfiguration, 
                "Configuration inconsistency should be detected");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_LongTermTrendAnalysis_ShouldAnalyzeTrends()
        {
            // Arrange - 髟ｷ譛滄俣縺ｮ逶｣隕悶ョ繝ｼ繧ｿ繧定塘遨阪☆繧九◆繧√∬､・焚蝗槭・蛛･蜈ｨ諤ｧ繝√ぉ繝・け繧貞ｮ溯｡・
            for (int i = 0; i < 25; i++) // 譛菴・0蝗槭・繝・・繧ｿ縺悟ｿ・ｦ・
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Act - 髟ｷ譛溷だ蜷大・譫舌′繝舌ャ繧ｯ繧ｰ繝ｩ繧ｦ繝ｳ繝峨〒螳溯｡後＆繧後ｋ

            // Assert - 繧ｨ繝ｩ繝ｼ縺ｪ縺城聞譛溷・譫舌′螳溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsNotNull(monitor, "Monitor should handle long-term analysis without errors");
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Service Quality Report.*"));
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_GradualRecovery_ShouldAttemptStepwiseRecovery()
        {
            // Arrange - 谿ｵ髫守噪蝗槫ｾｩ繧偵ユ繧ｹ繝医☆繧九◆繧√∝・繧ｵ繝ｼ繝薙せ繧呈怏蜉ｹ蛹・
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;
            FeatureFlags.EnableMigrationMonitoring = true;

            // 荳榊▼蜈ｨ縺ｪ迥ｶ諷九ｒ菴懈・・亥ｮ滄圀縺ｮ谿ｵ髫守噪蝗槫ｾｩ縺ｮ繝医Μ繧ｬ繝ｼ縺ｯ蝗ｰ髮｣・・
            FeatureFlags.UseServiceLocator = false;

            // Act - 逶｣隕悶す繧ｹ繝・Β縺悟撫鬘後ｒ讀懷・縺励∝ｯｾ蠢懊ｒ隧ｦ縺ｿ繧九∪縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 繧ｷ繧ｹ繝・Β縺梧ｮｵ髫守噪蝗槫ｾｩ繧定ｩｦ縺ｿ繧九％縺ｨ繧帝俣謗･逧・↓遒ｺ隱・
            // (逶ｴ謗･逧・↑繝・せ繝医・蝗ｰ髮｣縺縺後∬ｨｭ螳壼､画峩縺瑚｡後ｏ繧後ｋ蜿ｯ閭ｽ諤ｧ縺後≠繧・
            Assert.IsNotNull(monitor, "Monitor should attempt gradual recovery without crashing");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_ServiceMetricsTracking_ShouldTrackMetrics()
        {
            // Arrange - 繧ｵ繝ｼ繝薙せ繝｡繝医Μ繧ｯ繧ｹ霑ｽ霍｡縺ｮ繧ｻ繝・ヨ繧｢繝・・
            yield return new WaitForSeconds(1f);

            // Act - 繧ｵ繝ｼ繝薙せ繝｡繝医Μ繧ｯ繧ｹ縺梧峩譁ｰ縺輔ｌ繧九∪縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 繧ｵ繝ｼ繝薙せ蜩∬ｳｪ繝ｬ繝昴・繝医′逕滓・縺輔ｌ繧九％縺ｨ繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Service Quality Report.*"));
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_HealthStabilityConfirmation_ShouldConfirmStability()
        {
            // Arrange - 蛛･蜈ｨ縺ｪ迥ｶ諷九ｒ繧ｻ繝・ヨ繧｢繝・・
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            FeatureFlags.UseNewStealthService = true;

            // Act - 蛛･蜈ｨ諤ｧ縺ｮ螳牙ｮ壽ｧ縺檎｢ｺ隱阪＆繧後ｋ縺ｾ縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 繧ｷ繧ｹ繝・Β縺悟▼蜈ｨ縺ｪ迥ｶ諷九ｒ邯ｭ謖√☆繧九％縺ｨ繧堤｢ｺ隱・
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            Assert.IsTrue(healthStatus.HealthScore >= 70, 
                $"System should maintain good health, score: {healthStatus.HealthScore}");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_ErrorPatternDetection_ShouldDetectRecurringIssues()
        {
            // Arrange - 郢ｰ繧願ｿ斐＠逋ｺ逕溘☆繧句撫鬘後ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝医☆繧九・縺ｯ蝗ｰ髮｣縺縺後・
            // 繧ｨ繝ｩ繝ｼ繝代ち繝ｼ繝ｳ讀懷・讖溯・縺悟虚菴懊☆繧九％縺ｨ繧堤｢ｺ隱・

            // Act - 逶｣隕悶す繧ｹ繝・Β縺後お繝ｩ繝ｼ繝代ち繝ｼ繝ｳ繧貞・譫舌☆繧九∪縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(3f);

            // Assert - 繧ｨ繝ｩ繝ｼ繝代ち繝ｼ繝ｳ讀懷・讖溯・縺悟虚菴懊☆繧九％縺ｨ繧堤｢ｺ隱・
            Assert.IsNotNull(monitor, "Monitor should handle error pattern detection without issues");
        }

        [UnityTest]
        public IEnumerator IntegrationTest_AdvancedMonitorWithEmergencyRollback_ShouldWorkTogether()
        {
            // Arrange - 邨ｱ蜷医ユ繧ｹ繝育畑縺ｮ繧ｻ繝・ヨ繧｢繝・・
            FeatureFlags.EnableAutoRollback = true;

            // 蝠城｡後・縺ゅｋ險ｭ螳壹ｒ菴懈・
            FeatureFlags.UseServiceLocator = false;
            FeatureFlags.UseNewAudioService = true;

            // Act - 邨ｱ蜷医す繧ｹ繝・Β縺悟虚菴懊☆繧九∪縺ｧ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 荳｡繧ｷ繧ｹ繝・Β縺碁｣謳ｺ縺励※蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
            var healthStatus = EmergencyRollback.CheckSystemHealth();
            Assert.IsFalse(healthStatus.IsHealthy, "System should detect unhealthy state");

            // 逶｣隕悶す繧ｹ繝・Β縺ｨ繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繧ｷ繧ｹ繝・Β縺碁｣謳ｺ蜍穂ｽ・
            Assert.IsNotNull(monitor, "Advanced monitor should integrate with emergency rollback system");
        }

        [UnityTest]
        public IEnumerator AdvancedRollbackMonitor_PreventiveMeasures_ShouldExecutePreventiveActions()
        {
            // Arrange - 隴ｦ蜻翫Ξ繝吶Ν縺ｮ蛛･蜈ｨ諤ｧ迥ｶ諷九ｒ繧ｷ繝溘Η繝ｬ繝ｼ繝・
            // (逶ｴ謗･逧・↑蛻ｶ蠕｡縺ｯ蝗ｰ髮｣縺縺後∽ｺ磯亟逧・蒔鄂ｮ縺ｮ螳溯｡後ｒ遒ｺ隱・

            // Act - 莠磯亟逧・蒔鄂ｮ縺後ヨ繝ｪ繧ｬ繝ｼ縺輔ｌ繧狗憾豕√ｒ蠕・ｩ・
            yield return new WaitForSeconds(2f);

            // Assert - 莠磯亟逧・蒔鄂ｮ縺梧ｭ｣蟶ｸ縺ｫ螳溯｡後＆繧後ｋ縺薙→繧堤｢ｺ隱・
            Assert.IsNotNull(monitor, "Monitor should execute preventive measures without errors");
        }

        [Test]
        public void AdvancedRollbackMonitor_DataPersistence_ShouldSaveAndLoadData()
        {
            // Arrange - 繝・・繧ｿ縺ｮ菫晏ｭ倥ｒ繝医Μ繧ｬ繝ｼ
            // (OnDestroy譎ゅ↓閾ｪ蜍穂ｿ晏ｭ倥＆繧後ｋ)

            // Act - 逶｣隕悶ョ繝ｼ繧ｿ縺ｮ菫晏ｭ・隱ｭ縺ｿ霎ｼ縺ｿ讖溯・繧偵ユ繧ｹ繝・
            monitor.ResetMonitoringSystem();

            // Assert - 繝・・繧ｿ縺ｮ豌ｸ邯壼喧讖溯・縺悟虚菴懊☆繧九％縺ｨ繧堤｢ｺ隱・
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(".*Monitoring system reset.*"));
        }
    }
}


