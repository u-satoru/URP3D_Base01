using UnityEngine;
using _Project.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Debug;

namespace _Project.Tests.Core.Services
{
    /// <summary>
    /// StealthAudioService（新サービス）のServiceLocator統合テスト
    /// Step 3.5の検証用
    /// </summary>
    public class StealthAudioServiceTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool runTestOnStart = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                TestNewStealthAudioService();
            }
        }

        [ContextMenu("Test New StealthAudioService")]
        public void TestNewStealthAudioService()
        {
            EventLogger.LogStatic("[TEST] Starting NEW StealthAudioService integration test");

            // ServiceLocatorからIStealthAudioServiceを取得
            var stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();

            if (stealthAudioService != null)
            {
                EventLogger.LogStatic("[TEST] ✅ Successfully retrieved IStealthAudioService from ServiceLocator");
                
                // サービスの型を確認
                string serviceType = stealthAudioService.GetType().Name;
                EventLogger.LogStatic($"[TEST] Service type: {serviceType}");
                
                // 新サービス（StealthAudioService）か従来サービス（StealthAudioCoordinator）かを判定
                if (serviceType == "StealthAudioService")
                {
                    EventLogger.LogStatic("[TEST] 🎉 NEW StealthAudioService is being used! (Step 3.5 Success)");
                    TestNewServiceFunctionality(stealthAudioService);
                }
                else if (serviceType == "StealthAudioCoordinator")
                {
                    EventLogger.LogWarningStatic("[TEST] ⚠️ Still using legacy StealthAudioCoordinator");
                }
                else
                {
                    EventLogger.LogWarningStatic($"[TEST] ⚠️ Unknown service type: {serviceType}");
                }
            }
            else
            {
                EventLogger.LogErrorStatic("[TEST] ❌ Failed to retrieve IStealthAudioService from ServiceLocator");
                
                // ServiceLocatorの現在の状態をデバッグ
                DebugServiceLocatorState();
            }
        }

        private void TestNewServiceFunctionality(IStealthAudioService service)
        {
            try
            {
                EventLogger.LogStatic("[TEST] Testing NEW StealthAudioService functionality...");

                // CreateFootstepテスト
                service.CreateFootstep(Vector3.zero, 0.5f, "concrete");
                EventLogger.LogStatic("[TEST] ✅ CreateFootstep method works");

                // SetEnvironmentNoiseLevelテスト
                service.SetEnvironmentNoiseLevel(0.3f);
                EventLogger.LogStatic("[TEST] ✅ SetEnvironmentNoiseLevel method works");

                // EmitDetectableSoundテスト
                service.EmitDetectableSound(Vector3.forward, 5f, 0.7f, "test");
                EventLogger.LogStatic("[TEST] ✅ EmitDetectableSound method works");

                // PlayDistractionテスト
                service.PlayDistraction(Vector3.back, 3f);
                EventLogger.LogStatic("[TEST] ✅ PlayDistraction method works");

                // SetAlertLevelMusicテスト
                service.SetAlertLevelMusic(AlertLevel.Low);
                EventLogger.LogStatic("[TEST] ✅ SetAlertLevelMusic method works");

                // ApplyAudioMaskingテスト
                service.ApplyAudioMasking(0.4f);
                EventLogger.LogStatic("[TEST] ✅ ApplyAudioMasking method works");

                // AdjustStealthAudioテスト
                service.AdjustStealthAudio(0.6f);
                EventLogger.LogStatic("[TEST] ✅ AdjustStealthAudio method works");

                EventLogger.LogStatic("[TEST] 🎉 All NEW StealthAudioService functionality tests passed!");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[TEST] ❌ NEW StealthAudioService test failed: {ex.Message}");
            }
        }

        private void DebugServiceLocatorState()
        {
            EventLogger.LogStatic("[DEBUG] ServiceLocator debug information:");
            
            // ServiceLocatorにアクセスして登録済みサービス数を確認
            try
            {
                // この部分は実際のServiceLocator実装に依存
                EventLogger.LogStatic("[DEBUG] Checking ServiceLocator registration...");
                
                // Feature Flagsの状態を確認
                EventLogger.LogStatic($"[DEBUG] FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
                EventLogger.LogStatic($"[DEBUG] FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
                EventLogger.LogStatic($"[DEBUG] FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[DEBUG] ServiceLocator debug failed: {ex.Message}");
            }
        }
    }
}