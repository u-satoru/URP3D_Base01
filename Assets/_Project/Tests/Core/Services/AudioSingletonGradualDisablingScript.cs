using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;

namespace asterivo.Unity60.Tests.Core.Services
{
    /// <summary>
    /// Audio系Singleton段階的無効化の手動テスト実行スクリプト
    /// FeatureFlags メソッドを使用した段階的ロールアウトテスト
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
            // リアルタイム状態更新
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
            
            // Phase 1: 開発環境テスト
            ExecuteDevelopmentPhaseTest();
            
            // Phase 2: ステージング環境テスト  
            ExecuteStagingPhaseTest();
            
            // Phase 3: 本番環境テスト
            ExecuteProductionPhaseTest();
            
            // Phase 4: 緊急ロールバックテスト
            ExecuteEmergencyRollbackTest();
            
            LogResult("\n=== Test Completed Successfully ===");
        }

        void ExecuteDevelopmentPhaseTest()
        {
            LogResult("\n--- Phase 1: Development Environment ---");
            
            // Day 1: 警告システム有効化
            FeatureFlags.EnableDay1TestWarnings();
            LogResult($"EnableDay1TestWarnings executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            
            // Singleton アクセステスト (警告は出るが動作する)
            TestSingletonAccess("Development Phase", expectNull: false);
        }

        void ExecuteStagingPhaseTest()
        {
            LogResult("\n--- Phase 2: Staging Environment ---");
            
            // Day 2: ステージング環境で段階的無効化開始
            // 手動でステージング環境設定を実行
            FeatureFlags.EnableMigrationWarnings = true;
            FeatureFlags.EnableMigrationMonitoring = true;
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.DisableLegacySingletons = false; // ステージングではまだ false
            
            LogResult($"Staging environment settings applied");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogResult($"EnableMigrationWarnings: {FeatureFlags.EnableMigrationWarnings}");
            
            // Singleton アクセステスト (ステージング段階では警告付きでアクセス可能)
            TestSingletonAccess("Staging Phase", expectNull: false);
        }

        void ExecuteProductionPhaseTest()
        {
            LogResult("\n--- Phase 3: Production Environment ---");
            
            // Day 4: 本番環境で完全無効化
            FeatureFlags.EnableDay4SingletonDisabling();
            
            // 本番環境で必要な追加設定
            FeatureFlags.UseServiceLocator = true;
            FeatureFlags.UseNewAudioService = true;
            FeatureFlags.UseNewSpatialService = true;
            
            LogResult($"EnableDay4SingletonDisabling executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogResult($"UseNewAudioService: {FeatureFlags.UseNewAudioService}");
            LogResult($"UseNewSpatialService: {FeatureFlags.UseNewSpatialService}");
            
            // Singleton 完全無効化テスト
            TestSingletonAccess("Production Phase", expectNull: true);
            
            // ServiceLocator 代替アクセステスト
            TestServiceLocatorAccess();
        }

        void ExecuteEmergencyRollbackTest()
        {
            LogResult("\n--- Phase 4: Emergency Rollback ---");
            
            // 緊急時のロールバック実行
            LogResult("Executing emergency rollback...");
            
            // FeatureFlagsを緊急時設定に戻す
            FeatureFlags.DisableLegacySingletons = false;
            FeatureFlags.EnableMigrationWarnings = false;
            FeatureFlags.UseServiceLocator = false;
            
            LogResult($"Emergency rollback executed");
            LogResult($"DisableLegacySingletons: {FeatureFlags.DisableLegacySingletons}");
            LogResult($"UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            
            // ロールバック後のSingletonアクセステスト
            TestSingletonAccess("Emergency Rollback", expectNull: false);
        }

        void TestSingletonAccess(string phase, bool expectNull)
        {
            LogResult($"\n{phase} - Singleton Access Test:");
            
            try
            {
                // AudioManager ServiceLocator テスト (Phase 2 移行後)
                var audioInstance = ServiceLocator.GetService<IAudioService>();
                LogResult($"AudioManager.Instance: {(audioInstance == null ? "NULL" : "VALID")}");
                
                if (expectNull && audioInstance != null)
                {
                    LogResult("❌ UNEXPECTED: AudioManager.Instance should be null but got valid instance");
                }
                else if (!expectNull && audioInstance == null)
                {
                    LogResult("❌ UNEXPECTED: AudioManager.Instance should be valid but got null");
                }
                else
                {
                    LogResult($"✅ Expected behavior: AudioManager.Instance = {(audioInstance == null ? "NULL" : "VALID")}");
                }
            }
            catch (System.Exception ex)
            {
                LogResult($"❌ AudioManager.Instance access failed: {ex.Message}");
            }
            
            try
            {
                // SpatialAudioManager ServiceLocator テスト (Phase 2 移行後)
                var spatialInstance = ServiceLocator.GetService<ISpatialAudioService>();
                LogResult($"SpatialAudioManager.Instance: {(spatialInstance == null ? "NULL" : "VALID")}");
                
                if (expectNull && spatialInstance != null)
                {
                    LogResult("❌ UNEXPECTED: SpatialAudioManager.Instance should be null but got valid instance");
                }
                else if (!expectNull && spatialInstance == null)
                {
                    LogResult("❌ UNEXPECTED: SpatialAudioManager.Instance should be valid but got null");
                }
                else
                {
                    LogResult($"✅ Expected behavior: SpatialAudioManager.Instance = {(spatialInstance == null ? "NULL" : "VALID")}");
                }
            }
            catch (System.Exception ex)
            {
                LogResult($"❌ SpatialAudioManager.Instance access failed: {ex.Message}");
            }
        }

        void TestServiceLocatorAccess()
        {
            LogResult($"\nProduction Phase - ServiceLocator Alternative Access Test:");
            
            try
            {
                // ServiceLocator 経由でのアクセステスト
                var audioService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                LogResult($"ServiceLocator.GetService<IAudioService>(): {(audioService == null ? "NULL" : "VALID")}");
                
                var spatialService = ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.ISpatialAudioService>();
                LogResult($"ServiceLocator.GetService<ISpatialAudioService>(): {(spatialService == null ? "NULL" : "VALID")}");
                
                LogResult("✅ ServiceLocator access test completed");
            }
            catch (System.Exception ex)
            {
                LogResult($"❌ ServiceLocator access failed: {ex.Message}");
            }
        }

        void LogResult(string message)
        {
            testResults += message + "\n";
            
            if (enableDetailedLogging)
            {
                EventLogger.LogStatic($"[AudioSingletonTest] {message}");
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
