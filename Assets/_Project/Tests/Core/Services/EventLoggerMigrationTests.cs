using UnityEngine;
using NUnit.Framework;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using System.Collections.Generic;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// EventLoggerのServiceLocator移行テストスイート
    /// Phase 1完了検証: EventLogger完全移行の包括的テスト
    /// </summary>
    public class EventLoggerMigrationTests
    {
        private GameObject testGameObject;
        private EventLogger eventLogger;
        private List<IEventLogger> eventLoggers;
        
        [SetUp]
        public void SetUp()
        {
            // ServiceLocatorをクリア
            ServiceLocator.Clear();
            
            // テスト用GameObjectを作成
            testGameObject = new GameObject("TestEventLogger");
            eventLoggers = new List<IEventLogger>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // テスト環境をクリーンアップ
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
            // EventLoggerのServiceLocator統合テスト
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // ServiceLocatorへの登録テスト
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            var retrievedService = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNotNull(retrievedService, "EventLogger should be accessible via ServiceLocator");
            Assert.AreEqual(eventLogger, retrievedService, "Retrieved service should be the same instance");
            
            // インターフェース機能テスト
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
            // Legacy Singleton警告システムテスト
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.DisableLegacySingletons = false; // レガシーアクセスを一時的に許可
            
            eventLogger = testGameObject.AddComponent<EventLogger>();
            var loggedWarnings = new List<string>();
            
            // ログ出力をキャプチャ（簡略化）
            Application.logMessageReceived += (condition, stackTrace, type) =>
            {
                if (type == LogType.Warning && condition.Contains("DEPRECATED"))
                {
                    loggedWarnings.Add(condition);
                }
            };
            
            // ServiceLocator経由でのアクセス (Phase 2 移行後)
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Application.logMessageReceived -= (condition, stackTrace, type) => { };
            
            // 警告がログされたことを確認
            Assert.Greater(loggedWarnings.Count, 0, "Should log deprecation warning");
            Assert.IsTrue(loggedWarnings.Exists(w => w.Contains("ServiceLocator")), 
                "Warning should mention ServiceLocator alternative");
        }
        
        [Test] 
        public void Migration_EventLogger_FeatureFlagDisabling()
        {
            // FeatureFlag無効化テスト
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // Legacy Singletonを無効化
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
            
            // ServiceLocator経由でのアクセス (Phase 2 移行後)
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Application.logMessageReceived -= (condition, stackTrace, type) => { };
            
            // エラーログと null 戻り値の確認
            Assert.IsNull(instance, "Should return null when legacy singletons are disabled");
            Assert.Greater(loggedErrors.Count, 0, "Should log deprecation error");
        }
        
        [Test]
        public void Migration_EventLogger_StaticMethodsCompatibility()
        {
            // 静的メソッドの後方互換性テスト
            FeatureFlags.UseServiceLocator = true;
            eventLogger = testGameObject.AddComponent<EventLogger>();
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            // 静的メソッドが正常に動作することを確認
            Assert.DoesNotThrow(() => EventLogger.LogStatic("Static log test"));
            Assert.DoesNotThrow(() => EventLogger.LogWarningStatic("Static warning test"));
            Assert.DoesNotThrow(() => EventLogger.LogErrorStatic("Static error test"));
            Assert.DoesNotThrow(() => EventLogger.LogEventStatic("StaticEvent", 3, "test"));
            Assert.DoesNotThrow(() => EventLogger.LogEventWithPayloadStatic("TypedEvent", 2, 42));
            Assert.DoesNotThrow(() => EventLogger.ClearLogStatic());
            
            // プロパティアクセステスト
            Assert.IsTrue(EventLogger.IsEnabledStatic, "Static IsEnabled should work");
            Assert.IsNotNull(EventLogger.EventLogStatic, "Static EventLog should work");
            
            // 統計とエクスポート機能テスト
            var stats = EventLogger.GetStatisticsStatic();
            Assert.IsNotNull(stats, "Should return statistics");
            
            var filteredLogs = EventLogger.GetFilteredLogStatic("test");
            Assert.IsNotNull(filteredLogs, "Should return filtered logs");
        }
        
        [Test]
        public void Migration_EventLogger_ServiceInstanceFallback()
        {
            // ServiceLocator無効時のフォールバック機能テスト
            FeatureFlags.UseServiceLocator = false;
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // ServiceLocator経由でのアクセス (Phase 2 移行後)
            var instance = ServiceLocator.GetService<IEventLogger>();
            
            Assert.IsNotNull(instance, "Should fall back to Instance when ServiceLocator is disabled");
            
            // 静的メソッドが依然として機能することを確認
            Assert.DoesNotThrow(() => EventLogger.LogStatic("Fallback test"));
            Assert.IsTrue(EventLogger.IsEnabledStatic, "IsEnabled should work through fallback");
        }
        
        [Test]
        public void Migration_EventLogger_IInitializableImplementation()
        {
            // IInitializableインターフェース実装テスト
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            Assert.AreEqual(5, eventLogger.Priority, "Priority should be 5 for early initialization");
            Assert.IsTrue(eventLogger.IsInitialized, "Should be initialized after Awake");
            
            // 手動初期化テスト
            eventLogger.Initialize();
            Assert.IsTrue(eventLogger.IsInitialized, "Should remain initialized after manual Initialize call");
        }
        
        [Test]
        public void Migration_EventLogger_ServiceRegistrationLifecycle()
        {
            // サービス登録・解除ライフサイクルテスト
            eventLogger = testGameObject.AddComponent<EventLogger>();
            
            // 自動登録確認
            var service = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNotNull(service, "Should be auto-registered to ServiceLocator");
            Assert.AreEqual(eventLogger, service, "Should be the same instance");
            
            // 手動解除テスト
            ServiceLocator.UnregisterService<IEventLogger>();
            var unregisteredService = ServiceLocator.GetService<IEventLogger>();
            Assert.IsNull(unregisteredService, "Should be null after unregistering");
            
            // 再登録テスト
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            var reregisteredService = ServiceLocator.GetService<IEventLogger>();
            Assert.AreEqual(eventLogger, reregisteredService, "Should work after re-registering");
        }
        
        #endregion
        
        #region Comprehensive Interface Tests
        
        [Test]
        public void Migration_EventLogger_AllInterfaceMethodsAccessible()
        {
            // IEventLoggerインターフェースの全メソッドアクセステスト
            eventLogger = testGameObject.AddComponent<EventLogger>();
            ServiceLocator.RegisterService<IEventLogger>(eventLogger);
            
            var service = ServiceLocator.GetService<IEventLogger>();
            
            // 全インターフェースメソッドのテスト
            Assert.DoesNotThrow(() => service.Log("Interface test"));
            Assert.DoesNotThrow(() => service.LogWarning("Interface warning"));
            Assert.DoesNotThrow(() => service.LogError("Interface error"));
            Assert.DoesNotThrow(() => service.LogEvent("InterfaceEvent", 4, "payload"));
            Assert.DoesNotThrow(() => service.LogEventWithPayload("TypedEvent", 3, new { test = "data" }));
            Assert.DoesNotThrow(() => service.LogWarning("EventName", 2, "warning message"));
            Assert.DoesNotThrow(() => service.LogError("EventName", 1, "error message"));
            
            // データアクセスメソッドテスト
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
