using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Phase 3.2 繝ｩ繝ｳ繧ｿ繧､繝繝・せ繝茨ｼ售erviceLocator邨檎罰縺ｮ繧ｵ繝ｼ繝薙せ繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
    /// SINGLETON_COMPLETE_REMOVAL_GUIDE.md Phase 3.2 蟇ｾ蠢・
    /// </summary>
    public class SimpleServiceTestHelper : MonoBehaviour
    {
        [Header("Phase 3.2 Runtime Test")]
        [SerializeField] private bool enableDebugOutput = true;

        /// <summary>
        /// ServiceLocator邨檎罰縺ｧ縺ｮ繧ｵ繝ｼ繝薙せ繧｢繧ｯ繧ｻ繧ｹ繝・せ繝医ｒ螳溯｡・
        /// </summary>
        [ContextMenu("Run Phase 3.2 ServiceLocator Test")]
        public void RunServiceLocatorTest()
        {
            if (enableDebugOutput)
                Debug.Log("=== Phase 3.2 ServiceLocator Runtime Test Started ===");

            bool allTestsPassed = true;

            // Test 1: IAudioService 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("笨・AudioService: ServiceLocator access successful");
                
                // 螳滄圀縺ｮ繧ｵ繝ｼ繝薙せ蜻ｼ縺ｳ蜃ｺ縺励ユ繧ｹ繝茨ｼ亥ｮ牙・縺ｪ繝｡繧ｽ繝・ラ縺ｮ縺ｿ・・
                try
                {
                    float volume = audioService.GetMasterVolume();
                    if (enableDebugOutput)
                        Debug.Log($"笨・AudioService: GetMasterVolume() = {volume}");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"笶・AudioService method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("笶・AudioService: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // Test 2: ISpatialAudioService 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            if (spatialService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("笨・SpatialAudioService: ServiceLocator access successful");
                
                // 繧ｵ繝ｼ繝薙せ縺ｮ蝓ｺ譛ｬ讖溯・繝・せ繝茨ｼ亥憶菴懃畑縺ｮ蟆代↑縺・Γ繧ｽ繝・ラ縺ｮ縺ｿ・・
                try
                {
                    // Note: ISpatialAudioService縺ｫ縺ｯIsInitialized繝励Ο繝代ユ繧｣縺悟ｭ伜惠縺励↑縺・◆繧√・
                    // 繧ｵ繝ｼ繝薙せ縺梧ｭ｣蟶ｸ縺ｫ蜿門ｾ励〒縺阪◆譎らせ縺ｧ謌仙粥縺ｨ縺吶ｋ
                    if (enableDebugOutput)
                        Debug.Log($"笨・SpatialAudioService: Service instance available");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"笶・SpatialAudioService method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("笶・SpatialAudioService: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // Test 3: ICommandPoolService 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            var commandService = ServiceLocator.GetService<ICommandPoolService>();
            if (commandService != null)
            {
                if (enableDebugOutput)
                    Debug.Log("笨・CommandPoolService: ServiceLocator access successful");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogWarning("笞・・CommandPoolService: ServiceLocator access failed (service may not be active in scene)");
            }

            // Test 4: IEventLogger 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            var eventLogger = ServiceLocator.GetService<IEventLogger>();
            if (eventLogger != null)
            {
                if (enableDebugOutput)
                    Debug.Log("笨・EventLogger: ServiceLocator access successful");
                
                // 繝ｭ繧ｰ讖溯・縺ｮ繝・せ繝・
                try
                {
                    eventLogger.Log("[Phase3.2Test] ServiceLocator runtime test executed successfully");
                    bool isEnabled = eventLogger.IsEnabled;
                    if (enableDebugOutput)
                        Debug.Log($"笨・EventLogger: IsEnabled = {isEnabled}");
                }
                catch (System.Exception e)
                {
                    if (enableDebugOutput)
                        Debug.LogError($"笶・EventLogger method call failed: {e.Message}");
                    allTestsPassed = false;
                }
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("笶・EventLogger: ServiceLocator access failed (service not registered)");
                allTestsPassed = false;
            }

            // 譛邨らｵ先棡縺ｮ陦ｨ遉ｺ
            if (allTestsPassed)
            {
                if (enableDebugOutput)
                    Debug.Log("脂 Phase 3.2 ServiceLocator Runtime Test: ALL TESTS PASSED");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogError("笶・Phase 3.2 ServiceLocator Runtime Test: SOME TESTS FAILED");
            }

            if (enableDebugOutput)
                Debug.Log("=== Phase 3.2 ServiceLocator Runtime Test Completed ===");
        }

        /// <summary>
        /// 繧ｵ繝ｼ繝薙せ逋ｻ骭ｲ迥ｶ豕√・隧ｳ邏ｰ遒ｺ隱・
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
                        Debug.Log($"笨・{serviceInfo.Name}: Registered ({serviceInfo.Service.GetType().Name})");
                }
                else
                {
                    if (enableDebugOutput)
                        Debug.LogWarning($"笞・・{serviceInfo.Name}: Not registered");
                }
            }

            float registrationRatio = (float)registeredCount / services.Length;
            if (enableDebugOutput)
                Debug.Log($"投 Service Registration Summary: {registeredCount}/{services.Length} ({registrationRatio:P1})");

            if (registrationRatio >= 0.8f)
            {
                if (enableDebugOutput)
                    Debug.Log("笨・Service registration is healthy (>=80%)");
            }
            else
            {
                if (enableDebugOutput)
                    Debug.LogWarning("笞・・Service registration may need attention (<80%)");
            }
        }

        private void Start()
        {
            // 1遘貞ｾ後↓閾ｪ蜍輔ユ繧ｹ繝医ｒ螳溯｡鯉ｼ医す繝ｼ繝ｳ襍ｷ蜍慕峩蠕後・蛻晄悄蛹門ｾ・■・・
            if (enableDebugOutput)
                Invoke(nameof(RunServiceLocatorTest), 1.0f);
        }
    }
}


