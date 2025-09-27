using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// StealthAudioCoordinatorのServiceLocator登録テスト用スクリプト
    /// Step 3.4の検証用
    /// </summary>
    public class StealthAudioCoordinatorServiceLocatorTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool runTestOnStart = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                TestServiceLocatorIntegration();
            }
        }

        [ContextMenu("Test ServiceLocator Integration")]
        public void TestServiceLocatorIntegration()
        {
            ServiceHelper.Log("[TEST] Starting StealthAudioCoordinator ServiceLocator integration test");

            // ServiceLocatorからIStealthAudioServiceを取得
            var stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();

            if (stealthAudioService != null)
            {
                ServiceHelper.Log("[TEST] ✅ Successfully retrieved IStealthAudioService from ServiceLocator");
                
                // 基本機能のテスト
                TestBasicFunctionality(stealthAudioService);
            }
            else
            {
                ServiceHelper.LogError("[TEST] ❌ Failed to retrieve IStealthAudioService from ServiceLocator");
            }
        }

        private void TestBasicFunctionality(IStealthAudioService service)
        {
            try
            {
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

                ServiceHelper.Log("[TEST] 🎉 All basic functionality tests passed!");
            }
            catch (System.Exception ex)
            {
                ServiceHelper.LogError($"[TEST] ❌ Basic functionality test failed: {ex.Message}");
            }
        }
    }
}
