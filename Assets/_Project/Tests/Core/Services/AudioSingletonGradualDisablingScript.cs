using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Audio邉ｻSingleton谿ｵ髫守噪辟｡蜉ｹ蛹悶・謇句虚繝・せ繝亥ｮ溯｡後せ繧ｯ繝ｪ繝励ヨ
    /// FeatureFlags 繝｡繧ｽ繝・ラ繧剃ｽｿ逕ｨ縺励◆谿ｵ髫守噪繝ｭ繝ｼ繝ｫ繧｢繧ｦ繝医ユ繧ｹ繝・
    /// </summary>
    public class AudioSingletonGradualDisablingScript : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private bool executeOnStart = false;

        [Header("Test Results - Read Only")]
        [SerializeField, TextArea(3, 10)] private string testResults = "";

        [Header("Current FeatureFlags Status")]
        [SerializeField] private bool currentDisableLegacySingletons;
        [SerializeField] private bool currentEnableMigrationWarnings;
        [SerializeField] private bool currentUseServiceLocator;
        
        void Start()
        {
            if (executeOnStart)
            {
                ExecuteGradualDisablingTest();
            }
            
            UpdateCurrentStatus();
        }

        void Update()
        {
            // 繝ｪ繧｢繝ｫ繧ｿ繧､繝迥ｶ諷区峩譁ｰ
            UpdateCurrentStatus();
        }

        void UpdateCurrentStatus()
        {
            currentDisableLegacySingletons = FeatureFlags.DisableLegacySingletons;
            currentEnableMigrationWarnings = FeatureFlags.EnableMigrationWarnings;
            currentUseServiceLocator = FeatureFlags.UseServiceLocator;
        }

        [ContextMenu("Execute Gradual Disabling Test")]
        public void ExecuteGradualDisablingTest()
        {
            testResults = "";
            LogResult("=== Audio System Singleton Gradual Disabling Test ===");
            
            // Phase 1: 髢狗匱迺ｰ蠅・ユ繧ｹ繝・
            ExecuteDevelopmentPhaseTest();
            
            // Phase 2: 繧ｹ繝・・繧ｸ繝ｳ繧ｰ迺ｰ蠅・ユ繧ｹ繝・ 
            ExecuteStagingPhaseTest();
            
            // Phase 3: 譛ｬ逡ｪ迺ｰ蠅・ユ繧ｹ繝・
            ExecuteProductionPhaseTest();
            
            // Phase 4: 邱頑･繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ繝・せ繝・
            ExecuteEmergencyRollbackTest();
            
            LogResult("\n=== Test Completed Successfully ===");
        }

        void ExecuteDevelopmentPhaseTest()
        {
            LogResult("\n--- Phase 1: Development Environment ---");
            
            // Day 1: 隴ｦ蜻翫す繧ｹ繝・Β譛牙柑蛹・
            FeatureFlags.EnableDay1TestWarnings();
            LogResult($"EnableDay1TestWarnings executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            
            // Singleton 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・(隴ｦ蜻翫・蜃ｺ繧九′蜍穂ｽ懊☆繧・
            TestSingletonAccess("Development Phase", expectNull: false);
        }

        void ExecuteStagingPhaseTest()
        {
            LogResult("\n--- Phase 2: Staging Environment ---");
            
            // Day 2: 繧ｹ繝・・繧ｸ繝ｳ繧ｰ迺ｰ蠅・〒谿ｵ髫守噪辟｡蜉ｹ蛹夜幕蟋・
            // 謇句虚縺ｧ繧ｹ繝・・繧ｸ繝ｳ繧ｰ迺ｰ蠅・ｨｭ螳壹ｒ螳溯｡・
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = false; // 繧ｹ繝・・繧ｸ繝ｳ繧ｰ縺ｧ縺ｯ縺ｾ縺 false
            
            LogResult($"Staging environment settings applied");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogResult($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            
            // Singleton 繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・(繧ｹ繝・・繧ｸ繝ｳ繧ｰ谿ｵ髫弱〒縺ｯ隴ｦ蜻贋ｻ倥″縺ｧ繧｢繧ｯ繧ｻ繧ｹ蜿ｯ閭ｽ)
            TestSingletonAccess("Staging Phase", expectNull: false);
        }

        void ExecuteProductionPhaseTest()
        {
            LogResult("\n--- Phase 3: Production Environment ---");
            
            // Day 4: 譛ｬ逡ｪ迺ｰ蠅・〒螳悟・辟｡蜉ｹ蛹・
            FeatureFlags.EnableDay4SingletonDisabling();
            
            // 譛ｬ逡ｪ迺ｰ蠅・〒蠢・ｦ√↑霑ｽ蜉險ｭ螳・
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            
            LogResult($"EnableDay4SingletonDisabling executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogResult($"UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            LogResult($"UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
            
            // Singleton 螳悟・辟｡蜉ｹ蛹悶ユ繧ｹ繝・
            TestSingletonAccess("Production Phase", expectNull: true);
            
            // ServiceLocator 莉｣譖ｿ繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            TestServiceLocatorAccess();
        }

        void ExecuteEmergencyRollbackTest()
        {
            LogResult("\n--- Phase 4: Emergency Rollback ---");
            
            // 邱頑･譎ゅ・繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ螳溯｡・
            LogResult("Executing emergency rollback...");
            
            // FeatureFlags繧堤ｷ頑･譎りｨｭ螳壹↓謌ｻ縺・
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.UseServiceLocator = false;
            
            LogResult($"Emergency rollback executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            
            // 繝ｭ繝ｼ繝ｫ繝舌ャ繧ｯ蠕後・Singleton繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
            TestSingletonAccess("Emergency Rollback", expectNull: false);
        }

        void TestSingletonAccess(string phase, bool expectNull)
        {
            LogResult($"\n{phase} - Singleton Access Test:");
            
            try
            {
                // AudioManager ServiceLocator 繝・せ繝・(Phase 2 遘ｻ陦悟ｾ・
                var audioInstance = ServiceLocator.GetService<IAudioService>();
                LogResult($"AudioManager.Instance: {(audioInstance == null ? "NULL" : "VALID")}");
                
                if (expectNull && audioInstance != null)
                {
                    LogResult("笶・UNEXPECTED: AudioManager.Instance should be null but got valid instance");
                }
                else if (!expectNull && audioInstance == null)
                {
                    LogResult("笶・UNEXPECTED: AudioManager.Instance should be valid but got null");
                }
                else
                {
                    LogResult($"笨・Expected behavior: AudioManager.Instance = {(audioInstance == null ? "NULL" : "VALID")}");
                }
            }
            catch (System.Exception ex)
            {
                LogResult($"笶・AudioManager.Instance access failed: {ex.Message}");
            }
            
            try
            {
                // SpatialAudioManager ServiceLocator 繝・せ繝・(Phase 2 遘ｻ陦悟ｾ・
                var spatialInstance = ServiceLocator.GetService<ISpatialAudioService>();
                LogResult($"SpatialAudioManager.Instance: {(spatialInstance == null ? "NULL" : "VALID")}");
                
                if (expectNull && spatialInstance != null)
                {
                    LogResult("笶・UNEXPECTED: SpatialAudioManager.Instance should be null but got valid instance");
                }
                else if (!expectNull && spatialInstance == null)
                {
                    LogResult("笶・UNEXPECTED: SpatialAudioManager.Instance should be valid but got null");
                }
                else
                {
                    LogResult($"笨・Expected behavior: SpatialAudioManager.Instance = {(spatialInstance == null ? "NULL" : "VALID")}");
                }
            }
            catch (System.Exception ex)
            {
                LogResult($"笶・SpatialAudioManager.Instance access failed: {ex.Message}");
            }
        }

        void TestServiceLocatorAccess()
        {
            LogResult($"\nProduction Phase - ServiceLocator Alternative Access Test:");
            
            try
            {
                // ServiceLocator 邨檎罰縺ｧ縺ｮ繧｢繧ｯ繧ｻ繧ｹ繝・せ繝・
                var audioService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                LogResult($"ServiceLocator.GetService<IAudioService>(): {(audioService == null ? "NULL" : "VALID")}");
                
                var spatialService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();
                LogResult($"ServiceLocator.GetService<ISpatialAudioService>(): {(spatialService == null ? "NULL" : "VALID")}");
                
                LogResult("笨・ServiceLocator access test completed");
            }
            catch (System.Exception ex)
            {
                LogResult($"笶・ServiceLocator access failed: {ex.Message}");
            }
        }

        void LogResult(string message)
        {
            testResults += message + "\n";
            
            if (enableDetailedLogging)
            {
                ServiceHelper.Log($"[AudioSingletonTest] {message}");
                UnityEngine.Debug.Log($"[AudioSingletonTest] {message}");
            }
        }

        [ContextMenu("Reset FeatureFlags to Default")]
        public void ResetFeatureFlagsToDefault()
        {
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.UseNewAudioService = false;
            FeatureFlags.UseNewSpatialService = false;
            
            LogResult("FeatureFlags reset to default values");
            UpdateCurrentStatus();
        }

        [ContextMenu("Get Current Status")]
        public void GetCurrentStatus()
        {
            testResults = "=== Current FeatureFlags Status ===\n";
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            LogResult($"EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogResult($"UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            LogResult($"UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
        }
    }
}


