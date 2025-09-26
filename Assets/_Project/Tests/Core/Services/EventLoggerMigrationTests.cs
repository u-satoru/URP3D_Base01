using UnityEngine;
using NUnit.Framework;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// EventLogger縺ｮServiceLocator遘ｻ陦後ユ繧ｹ繝医せ繧､繝ｼ繝・
    /// Phase 1螳御ｺ・､懆ｨｼ: EventLogger螳悟・遘ｻ陦後・蛹・峡逧・ユ繧ｹ繝・
    /// </summary>
    public class EventLoggerMigrationTests
    {
        private GameObject testGameObject;
        private EventLogger eventLogger;
        private List<IEventLogger> eventLoggers;
        
        [SetUp]
        public void SetUp()
        {
            // ServiceLocator繧偵け繝ｪ繧｢
            ServiceLocator.Clear();
            
            // 繝・せ繝育畑GameObject繧剃ｽ懈・
            testGameObject = new GameObject("TestEventLogger");
            eventLoggers = new List<IEventLogger>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // 繝・せ繝育腸蠅・ｒ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            
            foreach (var logger in eventLoggers)
            {
                if (logger is EventLogger el && el != null)
                {
                    if (el.gameObject != null)
                    {
                        Object.DestroyImmediate(el.gameObject);
                    }
                }
            }
            eventLoggers.Clear();
            
            ServiceLocator.Clear();
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = false;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.DisableLegacySingletons = false;
        }
        
        #region EventLogger ServiceLocator Integration Tests
        
        [Test]
        public void Migration_EventLogger_ServiceLocatorIntegration()
        {
            // EventLogger縺ｮServiceLocator邨ｱ蜷医ユ繧ｹ繝・
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // ServiceLocator縺ｸ縺ｮ逋ｻ骭ｲ繝・せ繝・
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            var retrievedService = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNotNull(retrievedService, "EventLogger should be accessible via ServiceLocator");
            Assert.AreEqual(eventLogger, retrievedService, "Retrieved service should be the same instance");
            
            // 繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ讖溯・繝・せ繝・
            retrievedService.Log("Test message");
            Assert.IsTrue(retrievedService.IsEnabled, "EventLogger should be enabled by default");
            
            retrievedService.LogEvent("TestEvent", 5, "test payload");
            retrievedService.LogWarning("Test warning");
            retrievedService.LogError("Test error");
            
            Assert.DoesNotThrow(() => retrievedService.GetStatistics(), "Service statistics retrieval should work");
        }
        
        [Test]
        public void Migration_EventLogger_LegacySingletonWarnings()
        {
            // Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β繝・せ繝・
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.DisableLegacySingletons = false; // 繝ｬ繧ｬ繧ｷ繝ｼ繧｢繧ｯ繧ｻ繧ｹ繧剃ｸ譎ら噪縺ｫ險ｱ蜿ｯ
            
            eventLogger = testGameObject.AddComponent<EventLogger>();
            var loggedWarnings = new List<string>();
            
            // 繝ｭ繧ｰ蜃ｺ蜉帙ｒ繧ｭ繝｣繝励メ繝｣・育ｰ｡逡･蛹厄ｼ・
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (type == LogType.Warning && condition.Contains("DEPRECATED"))
                {
                    loggedWarnings.Add(condition);
                }
            };
            
            // ServiceLocator邨檎罰縺ｧ縺ｮ繧｢繧ｯ繧ｻ繧ｹ (Phase 2 遘ｻ陦悟ｾ・
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Application.logMessageReceived -= (condition, stackTrace, type) => { };
            
            // 隴ｦ蜻翫′繝ｭ繧ｰ縺輔ｌ縺溘％縺ｨ繧堤｢ｺ隱・
            Assert.Greater(loggedWarnings.Count, 0, "Should log deprecation warning");
            Assert.IsTrue(loggedWarnings.Exists(w => w.Contains("ServiceLocator")), 
                "Warning should mention ServiceLocator alternative");
        }
        
        [Test] 
        public void Migration_EventLogger_FeatureFlagDisabling()
        {
            // FeatureFlag辟｡蜉ｹ蛹悶ユ繧ｹ繝・
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // Legacy Singleton繧堤┌蜉ｹ蛹・
            FeatureFlags.DisableLegacySingletons = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            var loggedErrors = new List<string>();
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (type == LogType.Error && condition.Contains("DEPRECATED"))
                {
                    loggedErrors.Add(condition);
                }
            };
            
            // ServiceLocator邨檎罰縺ｧ縺ｮ繧｢繧ｯ繧ｻ繧ｹ (Phase 2 遘ｻ陦悟ｾ・
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Application.logMessageReceived -= (condition, stackTrace, type) => { };
            
            // 繧ｨ繝ｩ繝ｼ繝ｭ繧ｰ縺ｨ null 謌ｻ繧雁､縺ｮ遒ｺ隱・
            Assert.IsNull(instance, "Should return null when legacy singletons are disabled");
            Assert.Greater(loggedErrors.Count, 0, "Should log deprecation error");
        }
        
        [Test]
        public void Migration_EventLogger_StaticMethodsCompatibility()
        {
            // 髱咏噪繝｡繧ｽ繝・ラ縺ｮ蠕梧婿莠呈鋤諤ｧ繝・せ繝・
            FeatureFlags.UseServiceLocator = true;
            eventLogger = testGameObject.AddComponent<EventLogger>();
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            // 髱咏噪繝｡繧ｽ繝・ラ縺梧ｭ｣蟶ｸ縺ｫ蜍穂ｽ懊☆繧九％縺ｨ繧堤｢ｺ隱・
            Assert.DoesNotThrow(() => EventLogger.LogStatic("Static log test"));
            Assert.DoesNotThrow(() => EventLogger.LogWarningStatic("Static warning test"));
            Assert.DoesNotThrow(() => EventLogger.LogErrorStatic("Static error test"));
            Assert.DoesNotThrow(() => EventLogger.LogEventStatic("StaticEvent", 3, "test"));
            Assert.DoesNotThrow(() => EventLogger.LogEventWithPayloadStatic("TypedEvent", 2, 42));
            Assert.DoesNotThrow(() => EventLogger.ClearLogStatic());
            
            // 繝励Ο繝代ユ繧｣繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            Assert.IsTrue(EventLogger.IsEnabledStatic, "Static IsEnabled should work");
            Assert.IsNotNull(EventLogger.EventLogStatic, "Static EventLog should work");
            
            // 邨ｱ險医→繧ｨ繧ｯ繧ｹ繝昴・繝域ｩ溯・繝・せ繝・
            var stats = EventLogger.GetStatisticsStatic();
            Assert.IsNotNull(stats, "Should return statistics");
            
            var filteredLogs = EventLogger.GetFilteredLogStatic("test");
            Assert.IsNotNull(filteredLogs, "Should return filtered logs");
        }
        
        [Test]
        public void Migration_EventLogger_ServiceInstanceFallback()
        {
            // ServiceLocator辟｡蜉ｹ譎ゅ・繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ讖溯・繝・せ繝・
            FeatureFlags.UseServiceLocator = false;
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // ServiceLocator邨檎罰縺ｧ縺ｮ繧｢繧ｯ繧ｻ繧ｹ (Phase 2 遘ｻ陦悟ｾ・
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Assert.IsNotNull(instance, "Should fall back to Instance when ServiceLocator is disabled");
            
            // 髱咏噪繝｡繧ｽ繝・ラ縺御ｾ晉┯縺ｨ縺励※讖溯・縺吶ｋ縺薙→繧堤｢ｺ隱・
            Assert.DoesNotThrow(() => EventLogger.LogStatic("Fallback test"));
            Assert.IsTrue(EventLogger.IsEnabledStatic, "IsEnabled should work through fallback");
        }
        
        [Test]
        public void Migration_EventLogger_IInitializableImplementation()
        {
            // IInitializable繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・ユ繧ｹ繝・
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            Assert.AreEqual(5, eventLogger.Priority, "Priority should be 5 for early initialization");
            Assert.IsTrue(eventLogger.IsInitialized, "Should be initialized after Awake");
            
            // 謇句虚蛻晄悄蛹悶ユ繧ｹ繝・
            eventLogger.Initialize();
            Assert.IsTrue(eventLogger.IsInitialized, "Should remain initialized after manual Initialize call");
        }
        
        [Test]
        public void Migration_EventLogger_ServiceRegistrationLifecycle()
        {
            // 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ繝ｻ隗｣髯､繝ｩ繧､繝輔し繧､繧ｯ繝ｫ繝・せ繝・
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // 閾ｪ蜍慕匳骭ｲ遒ｺ隱・
            var service = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNotNull(service, "Should be auto-registered to ServiceLocator");
            Assert.AreEqual(eventLogger, service, "Should be the same instance");
            
            // 謇句虚隗｣髯､繝・せ繝・
            ServiceLocator.UnregisterService<IEventLogger>();
            var unregisteredService = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNull(unregisteredService, "Should be null after unregistering");
            
            // 蜀咲匳骭ｲ繝・せ繝・
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            var reregisteredService = ServiceLocator.GetService<IEventLogger>();
            Assert.AreEqual(eventLogger, reregisteredService, "Should work after re-registering");
        }
        
        #endregion
        
        #region Comprehensive Interface Tests
        
        [Test]
        public void Migration_EventLogger_AllInterfaceMethodsAccessible()
        {
            // IEventLogger繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ縺ｮ蜈ｨ繝｡繧ｽ繝・ラ繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            eventLogger = testGameObject.AddComponent<EventLogger>();
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            var service = ServiceLocator.GetService<IEventLogger>();
            
            // 蜈ｨ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ繝｡繧ｽ繝・ラ縺ｮ繝・せ繝・
            Assert.DoesNotThrow(() => service.Log("Interface test"));
            Assert.DoesNotThrow(() => service.LogWarning("Interface warning"));
            Assert.DoesNotThrow(() => service.LogError("Interface error"));
            Assert.DoesNotThrow(() => service.LogEvent("InterfaceEvent", 4, "payload"));
            Assert.DoesNotThrow(() => service.LogEventWithPayload("TypedEvent", 3, new { test = "data" }));
            Assert.DoesNotThrow(() => service.LogWarning("EventName", 2, "warning message"));
            Assert.DoesNotThrow(() => service.LogError("EventName", 1, "error message"));
            
            // 繝・・繧ｿ繧｢繧ｯ繧ｻ繧ｹ繝｡繧ｽ繝・ラ繝・せ繝・
            Assert.IsNotNull(service.EventLog, "EventLog property should be accessible");
            Assert.IsTrue(service.IsEnabled, "IsEnabled property should be accessible");
            
            var filteredLogs = service.GetFilteredLog("test");
            Assert.IsNotNull(filteredLogs, "GetFilteredLog should return data");
            
            var statistics = service.GetStatistics();
            Assert.IsNotNull(statistics, "GetStatistics should return data");
            
            Assert.DoesNotThrow(() => service.ClearLog(), "ClearLog should be accessible");
            Assert.DoesNotThrow(() => service.ExportToCSV("test.csv"), "ExportToCSV should be accessible");
        }
        
        #endregion
    }
}


