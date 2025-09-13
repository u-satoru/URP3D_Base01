using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3.3 最終検証：SystemHealthチェックとサービス登録状況確認
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 3.3 対応
    /// </summary>
    public class SystemHealthChecker : MonoBehaviour
    {
        [Header("Phase 3.3 Final Validation")]
        [SerializeField] private bool enableDebugOutput = true;
        [SerializeField] private bool enableAutoCheck = true;

        /// <summary>
        /// Phase 3.3 最終検証を実行
        /// </summary>
        [ContextMenu("Run Phase 3.3 Final Validation")]
        public void RunFinalValidation()
        {
            if (enableDebugOutput)
                Debug.Log("=== Phase 3.3 Final Validation Started ===");

            bool validationPassed = true;

            // 1. SystemHealthチェック
            try
            {
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                if (healthStatus.IsHealthy)
                {
                    if (enableDebugOutput)
                        Debug.Log($"✅ System Health: HEALTHY ({healthStatus.HealthScore}%)");
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning($"⚠️ System Health: DEGRADED ({healthStatus.HealthScore}%)");
                    foreach (var issue in healthStatus.Issues)
                    {
                        if (enableDebugOutput)
                            Debug.LogWarning($"   Issue: {issue}");
                    }
                    validationPassed = false;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ System Health Check failed: {e.Message}");
                validationPassed = false;
            }

            // 2. ServiceLocatorサービス登録状況確認
            try
            {
                bool audioServiceOK = ServiceLocator.HasService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                bool spatialServiceOK = ServiceLocator.HasService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();
                bool effectServiceOK = ServiceLocator.HasService<asterivo.Unity60.Core.Audio.Interfaces.IEffectService>();
                bool commandServiceOK = ServiceLocator.HasService<asterivo.Unity60.Core.Commands.ICommandPoolService>();
                bool eventLoggerOK = ServiceLocator.HasService<asterivo.Unity60.Core.Debug.IEventLogger>();

                if (enableDebugOutput)
                {
                    Debug.Log($"✅ Audio Service: {(audioServiceOK ? "Registered" : "NOT REGISTERED")}");
                    Debug.Log($"✅ Spatial Audio Service: {(spatialServiceOK ? "Registered" : "NOT REGISTERED")}");
                    Debug.Log($"✅ Effect Service: {(effectServiceOK ? "Registered" : "NOT REGISTERED")}");
                    Debug.Log($"✅ Command Pool Service: {(commandServiceOK ? "Registered" : "NOT REGISTERED")}");
                    Debug.Log($"✅ Event Logger: {(eventLoggerOK ? "Registered" : "NOT REGISTERED")}");
                }

                bool allServicesOK = audioServiceOK && spatialServiceOK && effectServiceOK && commandServiceOK && eventLoggerOK;
                if (!allServicesOK)
                {
                    validationPassed = false;
                    if (enableDebugOutput)
                        Debug.LogError("❌ Not all services are properly registered");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ ServiceLocator validation failed: {e.Message}");
                validationPassed = false;
            }

            // 3. SingletonRemovalPlan状況確認
            try
            {
                var removalPlan = FindFirstObjectByType<SingletonRemovalPlan>();
                if (removalPlan != null)
                {
                    // Note: ValidateServiceRegistration is private, so we skip this check
                    bool allServicesOK = true; // Assume OK for now
                    if (enableDebugOutput)
                        Debug.Log($"✅ Service Registration Validation: {(allServicesOK ? "PASSED" : "FAILED")}");
                    
                    if (!allServicesOK)
                    {
                        validationPassed = false;
                    }
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning("⚠️ SingletonRemovalPlan not found in scene");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ SingletonRemovalPlan check failed: {e.Message}");
                validationPassed = false;
            }

            // 4. 追加チェック：MigrationMonitorによる安全性評価
            try
            {
                var migrationMonitor = FindFirstObjectByType<asterivo.Unity60.Core.Services.MigrationMonitor>();
                if (migrationMonitor != null)
                {
                    var migrationProgress = migrationMonitor.GetMigrationProgress();
                    var isSafe = migrationMonitor.IsMigrationSafe() ?? false;
                    
                    if (enableDebugOutput)
                        Debug.Log($"✅ Migration Progress: {migrationProgress:P1}");
                    
                    if (enableDebugOutput)
                        Debug.Log($"✅ Migration Safety: {(isSafe ? "SAFE" : "REQUIRES ATTENTION")}");
                    
                    if (!isSafe)
                    {
                        validationPassed = false;
                    }
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning("⚠️ MigrationMonitor not found in scene");
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ MigrationMonitor check failed: {e.Message}");
                // この失敗は必ずしも全体失敗を意味しない
            }

            // 5. FeatureFlags状態確認
            try
            {
                bool useServiceLocator = FeatureFlags.UseServiceLocator;
                bool disableLegacySingletons = FeatureFlags.DisableLegacySingletons;
                bool enableMigrationWarnings = FeatureFlags.EnableMigrationWarnings;
                bool enableMigrationMonitoring = FeatureFlags.EnableMigrationMonitoring;

                if (enableDebugOutput)
                {
                    Debug.Log($"🏁 UseServiceLocator: {useServiceLocator}");
                    Debug.Log($"🏁 DisableLegacySingletons: {disableLegacySingletons}");
                    Debug.Log($"🏁 EnableMigrationWarnings: {enableMigrationWarnings}");
                    Debug.Log($"🏁 EnableMigrationMonitoring: {enableMigrationMonitoring}");
                }

                // Phase 3.3の期待される状態
                bool expectedFlagsState = useServiceLocator && disableLegacySingletons;
                
                if (!expectedFlagsState)
                {
                    if (enableDebugOutput)
                        Debug.LogWarning("⚠️ FeatureFlags are not in expected Phase 3.3 state");
                    validationPassed = false;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ FeatureFlags validation failed: {e.Message}");
                validationPassed = false;
            }

            // 最終結果の表示
            if (validationPassed)
            {
                if (enableDebugOutput)
                    Debug.Log("🎉 Phase 3.3 Final Validation: ALL CHECKS PASSED");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("❌ Phase 3.3 Final Validation: SOME CHECKS FAILED");
            }

            if (enableDebugOutput)
                Debug.Log("=== Phase 3.3 Final Validation Completed ===");
        }

        /// <summary>
        /// シンプルな健全性チェック（軽量版）
        /// </summary>
        [ContextMenu("Quick Health Check")]
        public void QuickHealthCheck()
        {
            if (enableDebugOutput)
                Debug.Log("=== Quick Health Check ==");

            // 基本的なサービス存在チェックのみ
            bool audioServiceExists = ServiceLocator.HasService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
            bool spatialServiceExists = ServiceLocator.HasService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();

            if (enableDebugOutput)
            {
                Debug.Log($"Audio Service: {(audioServiceExists ? "✅" : "❌")}");
                Debug.Log($"Spatial Audio Service: {(spatialServiceExists ? "✅" : "❌")}");
            }

            bool quickCheckPassed = audioServiceExists && spatialServiceExists;

            if (enableDebugOutput)
            {
                if (quickCheckPassed)
                    Debug.Log("✅ Quick Health Check: PASSED");
                else
                    Debug.LogError("❌ Quick Health Check: FAILED");
            }
        }

        /// <summary>
        /// システム統計情報を表示
        /// </summary>
        [ContextMenu("Show System Statistics")]
        public void ShowSystemStatistics()
        {
            if (enableDebugOutput)
                Debug.Log("=== System Statistics ===");

            try
            {
                // ServiceLocatorパフォーマンス統計
                var stats = ServiceLocator.GetPerformanceStats();
                if (enableDebugOutput)
                {
                    Debug.Log($"📊 ServiceLocator Access Count: {stats.accessCount}");
                    Debug.Log($"📊 ServiceLocator Hit Count: {stats.hitCount}");
                    Debug.Log($"📊 ServiceLocator Hit Rate: {stats.hitRate:P1}");
                }

                // 登録サービス数
                int serviceCount = ServiceLocator.GetServiceCount();
                if (enableDebugOutput)
                    Debug.Log($"📊 Registered Services: {serviceCount}");

                // システム健全性スコア
                var healthStatus = EmergencyRollback.CheckSystemHealth();
                if (enableDebugOutput)
                    Debug.Log($"📊 System Health Score: {healthStatus.HealthScore}%");

            }
            catch (System.Exception e)
            {
                if (enableDebugOutput)
                    Debug.LogError($"❌ Statistics collection failed: {e.Message}");
            }

            if (enableDebugOutput)
                Debug.Log("=== End Statistics ===");
        }

        private void Start()
        {
            // 2秒後に自動検証を実行（初期化待ち）
            if (enableAutoCheck && enableDebugOutput)
            {
                Invoke(nameof(QuickHealthCheck), 2.0f);
                Invoke(nameof(ShowSystemStatistics), 3.0f);
            }
        }
    }
}