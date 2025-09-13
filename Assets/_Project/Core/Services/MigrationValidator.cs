using UnityEngine;

using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 移行完了状況の検証ツール
    /// </summary>
    public class MigrationValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool continousValidation = false;
        [SerializeField] private float validationInterval = 5f;
        
        private void Start()
        {
            if (runOnStart)
            {
                ValidateMigration();
            }
            
            if (continousValidation)
            {
                InvokeRepeating(nameof(ValidateMigration), validationInterval, validationInterval);
            }
        }
        
        [ContextMenu("Validate Migration")]
        public void ValidateMigration()
        {
            EventLogger.LogStatic("[MigrationValidator] Starting migration validation...");
            
            bool allPassed = true;
            
            // ServiceLocator基本機能の検証
            allPassed &= ValidateServiceLocatorBasics();
            
            // 各サービスの検証
            allPassed &= ValidateAudioService();
            allPassed &= ValidateSpatialAudioService();
            allPassed &= ValidateStealthAudioService();
            allPassed &= ValidateEffectService();
            allPassed &= ValidateAudioUpdateService();
            
            // Feature Flagsの検証
            allPassed &= ValidateFeatureFlags();
            
            string result = allPassed ? "PASSED" : "FAILED";
            EventLogger.LogStatic($"[MigrationValidator] Migration validation {result}");
        }
        
        private bool ValidateServiceLocatorBasics()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating ServiceLocator basics...");
            
            // ServiceLocatorが動作していることを確認
            int serviceCount = ServiceLocator.GetServiceCount();
            if (serviceCount == 0)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] ServiceLocator has no registered services");
                return false;
            }
            
            EventLogger.LogStatic($"[MigrationValidator] ServiceLocator has {serviceCount} registered services");
            return true;
        }
        
        private bool ValidateAudioService()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating AudioService...");
            
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService == null)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] IAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.LogStatic("[MigrationValidator] IAudioService validation passed");
            return true;
        }
        
        private bool ValidateSpatialAudioService()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating SpatialAudioService...");
            
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            if (spatialService == null)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] ISpatialAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.LogStatic("[MigrationValidator] ISpatialAudioService validation passed");
            return true;
        }
        
        private bool ValidateStealthAudioService()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating StealthAudioService...");
            
            var stealthService = ServiceLocator.GetService<IStealthAudioService>();
            if (stealthService == null)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] IStealthAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.LogStatic("[MigrationValidator] IStealthAudioService validation passed");
            return true;
        }
        
        private bool ValidateEffectService()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating EffectService...");
            
            var effectService = ServiceLocator.GetService<IEffectService>();
            if (effectService == null)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] IEffectService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.LogStatic("[MigrationValidator] IEffectService validation passed");
            return true;
        }
        
        private bool ValidateAudioUpdateService()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating AudioUpdateService...");
            
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            if (updateService == null)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] IAudioUpdateService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.LogStatic("[MigrationValidator] IAudioUpdateService validation passed");
            return true;
        }
        
        private bool ValidateFeatureFlags()
        {
            EventLogger.LogStatic("[MigrationValidator] Validating FeatureFlags...");
            
            if (!FeatureFlags.UseServiceLocator)
            {
                EventLogger.LogErrorStatic("[MigrationValidator] UseServiceLocator is disabled");
                return false;
            }
            
            // Week 3の期待される設定
            if (FeatureFlags.UseNewAudioService && 
                FeatureFlags.UseNewSpatialService && 
                FeatureFlags.UseNewStealthService)
            {
                EventLogger.LogStatic("[MigrationValidator] All new services are enabled");
                return true;
            }
            else
            {
                EventLogger.LogWarningStatic("[MigrationValidator] Some new services are still disabled");
                return false;
            }
        }
    }
}