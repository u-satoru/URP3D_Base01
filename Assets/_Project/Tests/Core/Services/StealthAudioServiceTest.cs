using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
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
            ServiceHelper.Log("[TEST] Starting NEW StealthAudioService integration test");

            // ServiceLocatorからIStealthAudioServiceを取得
            var stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();

            if (stealthAudioService != null)
            {
                ServiceHelper.Log("[TEST] ✅ Successfully retrieved IStealthAudioService from ServiceLocator");
                
                // サービスの型を確認
                string serviceType = stealthAudioService.GetType().Name;
                ServiceHelper.Log($"[TEST] Service type: {serviceType}");
                
                // 新サービス（StealthAudioService）か従来サービス（StealthAudioCoordinator）かを判定
                if (serviceType == "StealthAudioService")
                {
                    ServiceHelper.Log("[TEST] 🎉 NEW StealthAudioService is being used! (Step 3.5 Success)");
                    TestNewServiceFunctionality(stealthAudioService);
                }
                else if (serviceType == "StealthAudioCoordinator")
                {
                    ServiceHelper.LogWarning("[TEST] ⚠️ Still using legacy StealthAudioCoordinator");
                }
                else
                {
                    ServiceHelper.LogWarning($"[TEST] ⚠️ Unknown service type: {serviceType}");
                }
            }
            else
            {
                ServiceHelper.LogError("[TEST] ❌ Failed to retrieve IStealthAudioService from ServiceLocator");
                
                // ServiceLocatorの現在の状態をデバッグ
                DebugServiceLocatorState();
            }
        }

        private void TestNewServiceFunctionality(IStealthAudioService service)
        {
            try
            {
                ServiceHelper.Log("[TEST] Testing NEW StealthAudioService functionality...");

                // CreateFootstepテスト
                service.CreateFootstep(Vector3.zero, 0.5f, "concrete");
                ServiceHelper.Log("[TEST] ✅ CreateFootstep method works");

                // SetEnvironmentNoiseLevelテスト
                service.SetEnvironmentNoiseLevel(0.3f);
                ServiceHelper.Log("[TEST] ✅ SetEnvironmentNoiseLevel method works");

                // EmitDetectableSoundテスト
                service.EmitDetectableSound(Vector3.forward, 5f, 0.7f, "test");
                ServiceHelper.Log("[TEST] ✅ EmitDetectableSound method works");

                // PlayDistractionテスト
                service.PlayDistraction(Vector3.back, 3f);
                ServiceHelper.Log("[TEST] ✅ PlayDistraction method works");

                // SetAlertLevelMusicテスト
                service.SetAlertLevelMusic(AlertLevel.Suspicious);
                ServiceHelper.Log("[TEST] ✅ SetAlertLevelMusic method works");

                // ApplyAudioMaskingテスト
                service.ApplyAudioMasking(0.4f);
                ServiceHelper.Log("[TEST] ✅ ApplyAudioMasking method works");

                // AdjustStealthAudioテスト
                service.AdjustStealthAudio(0.6f);
                ServiceHelper.Log("[TEST] ✅ AdjustStealthAudio method works");

                ServiceHelper.Log("[TEST] 🎉 All NEW StealthAudioService functionality tests passed!");
            }
            catch (System.Exception ex)
            {
                ServiceHelper.LogError($"[TEST] ❌ NEW StealthAudioService test failed: {ex.Message}");
            }
        }

        private void DebugServiceLocatorState()
        {
            ServiceHelper.Log("[DEBUG] ServiceLocator debug information:");
            
            // ServiceLocatorにアクセスして登録済みサービス数を確認
            try
            {
                // この部分は実際のServiceLocator実装に依存
                ServiceHelper.Log("[DEBUG] Checking ServiceLocator registration...");
                
                // Feature Flagsの状態を確認
                ServiceHelper.Log($"[DEBUG] FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
                ServiceHelper.Log($"[DEBUG] FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
                ServiceHelper.Log($"[DEBUG] FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            }
            catch (System.Exception ex)
            {
                ServiceHelper.LogError($"[DEBUG] ServiceLocator debug failed: {ex.Message}");
            }
        }
    }
}
