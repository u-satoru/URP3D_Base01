using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3.2 ランタイムテスト：ServiceLocator経由のサービスアクセステスト
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 3.2 対応
    /// </summary>
    public class SimpleServiceTestHelper : MonoBehaviour
    {
        [Header("Phase 3.2 Runtime Test")]
        [SerializeField] private bool enableDebugOutput = true;

        /// <summary>
        /// ServiceLocator経由でのサービスアクセステストを実行
        /// </summary>
        [ContextMenu("Run Phase 3.2 ServiceLocator Test")]
        public void RunServiceLocatorTest()
        {
            if (enableDebugOutput)
                Debug.Log("=== Phase 3.2 ServiceLocator Runtime Test Started ===");

            bool allTestsPassed = true;

            // Test 1: IAudioService アクセステスト
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("✅ AudioService: ServiceLocator access successful");
                
                // 実際のサービス呼び出しテスト（安全なメソッドのみ）
                try
                {
                    float volume = audioService.GetMasterVolume();
                    if (enableDebugOutput)
                        Debug.Log($"✅ AudioService: GetMasterVolume() = {volume}");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"❌ AudioService method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("❌ AudioService: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // Test 2: ISpatialAudioService アクセステスト
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            if (spatialService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("✅ SpatialAudioService: ServiceLocator access successful");
                
                // サービスの基本機能テスト（副作用の少ないメソッドのみ）
                try
                {
                    // Note: ISpatialAudioServiceにはIsInitializedプロパティが存在しないため、
                    // サービスが正常に取得できた時点で成功とする
                    if (enableDebugOutput)
                        Debug.Log($"✅ SpatialAudioService: Service instance available");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"❌ SpatialAudioService method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("❌ SpatialAudioService: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // Test 3: ICommandPoolService アクセステスト
            var commandService = ServiceLocator.GetService<ICommandPoolService>();
            if (commandService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("✅ CommandPoolService: ServiceLocator access successful");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogWarning("⚠️ CommandPoolService: ServiceLocator access failed (service may not be active in scene)");
            }

            // Test 4: IEventLogger アクセステスト
            var eventLogger = ServiceLocator.GetService<IEventLogger>();
            if (eventLogger != null)
            {
                if (enableDebugOutput)
                    Debug.Log("✅ EventLogger: ServiceLocator access successful");
                
                // ログ機能のテスト
                try
                {
                    eventLogger.Log("[Phase3.2Test] ServiceLocator runtime test executed successfully");
                    bool isEnabled = eventLogger.IsEnabled;
                    if (enableDebugOutput)
                        Debug.Log($"✅ EventLogger: IsEnabled = {isEnabled}");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"❌ EventLogger method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("❌ EventLogger: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // 最終結果の表示
            if (allTestsPassed)
            {
                if (enableDebugOutput)
                    Debug.Log("🎉 Phase 3.2 ServiceLocator Runtime Test: ALL TESTS PASSED");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("❌ Phase 3.2 ServiceLocator Runtime Test: SOME TESTS FAILED");
            }

            if (enableDebugOutput)
                Debug.Log("=== Phase 3.2 ServiceLocator Runtime Test Completed ===");
        }

        /// <summary>
        /// サービス登録状況の詳細確認
        /// </summary>
        [ContextMenu("Check Service Registration Status")]
        public void CheckServiceRegistrationStatus()
        {
            if (enableDebugOutput)
                Debug.Log("=== Service Registration Status Check ===");

            var services = new[]
            {
                new { Name = "IAudioService", Service = (object)ServiceLocator.GetService<IAudioService>() },
                new { Name = "ISpatialAudioService", Service = (object)ServiceLocator.GetService<ISpatialAudioService>() },
                new { Name = "IEffectService", Service = (object)ServiceLocator.GetService<IEffectService>() },
                new { Name = "ICommandPoolService", Service = (object)ServiceLocator.GetService<ICommandPoolService>() },
                new { Name = "IEventLogger", Service = (object)ServiceLocator.GetService<IEventLogger>() }
            };

            int registeredCount = 0;
            foreach (var serviceInfo in services)
            {
                if (serviceInfo.Service != null)
                {
                    registeredCount++;
                    if (enableDebugOutput)
                        Debug.Log($"✅ {serviceInfo.Name}: Registered ({serviceInfo.Service.GetType().Name})");
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning($"⚠️ {serviceInfo.Name}: Not registered");
                }
            }

            float registrationRatio = (float)registeredCount / services.Length;
            if (enableDebugOutput)
                Debug.Log($"📊 Service Registration Summary: {registeredCount}/{services.Length} ({registrationRatio:P1})");

            if (registrationRatio >= 0.8f)
            {
                if (enableDebugOutput)
                    Debug.Log("✅ Service registration is healthy (>=80%)");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogWarning("⚠️ Service registration may need attention (<80%)");
            }
        }

        private void Start()
        {
            // 1秒後に自動テストを実行（シーン起動直後の初期化待ち）
            if (enableDebugOutput)
                Invoke(nameof(RunServiceLocatorTest), 1.0f);
        }
    }
}
