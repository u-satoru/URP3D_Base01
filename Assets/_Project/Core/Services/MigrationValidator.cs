using UnityEngine;
using _Project.Core.Services;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core;

namespace _Project.Core.Services
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
            EventLogger.Log("[MigrationValidator] Starting migration validation...");
            
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
            EventLogger.Log($"[MigrationValidator] Migration validation {result}");
        }
        
        private bool ValidateServiceLocatorBasics()
        {
            EventLogger.Log("[MigrationValidator] Validating ServiceLocator basics...");
            
            // ServiceLocatorが動作していることを確認
            int serviceCount = ServiceLocator.GetServiceCount();
            if (serviceCount == 0)
            {
                EventLogger.LogError("[MigrationValidator] ServiceLocator has no registered services");
                return false;
            }
            
            EventLogger.Log($"[MigrationValidator] ServiceLocator has {serviceCount} registered services");
            return true;
        }
        
        private bool ValidateAudioService()
        {
            EventLogger.Log("[MigrationValidator] Validating AudioService...");
            
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService == null)
            {
                EventLogger.LogError("[MigrationValidator] IAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.Log("[MigrationValidator] IAudioService validation passed");
            return true;
        }
        
        private bool ValidateSpatialAudioService()
        {
            EventLogger.Log("[MigrationValidator] Validating SpatialAudioService...");
            
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            if (spatialService == null)
            {
                EventLogger.LogError("[MigrationValidator] ISpatialAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.Log("[MigrationValidator] ISpatialAudioService validation passed");
            return true;
        }
        
        private bool ValidateStealthAudioService()
        {
            EventLogger.Log("[MigrationValidator] Validating StealthAudioService...");
            
            var stealthService = ServiceLocator.GetService<IStealthAudioService>();
            if (stealthService == null)
            {
                EventLogger.LogError("[MigrationValidator] IStealthAudioService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.Log("[MigrationValidator] IStealthAudioService validation passed");
            return true;
        }
        
        private bool ValidateEffectService()
        {
            EventLogger.Log("[MigrationValidator] Validating EffectService...");
            
            var effectService = ServiceLocator.GetService<IEffectService>();
            if (effectService == null)
            {
                EventLogger.LogError("[MigrationValidator] IEffectService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.Log("[MigrationValidator] IEffectService validation passed");
            return true;
        }
        
        private bool ValidateAudioUpdateService()
        {
            EventLogger.Log("[MigrationValidator] Validating AudioUpdateService...");
            
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            if (updateService == null)
            {
                EventLogger.LogError("[MigrationValidator] IAudioUpdateService not found in ServiceLocator");
                return false;
            }
            
            EventLogger.Log("[MigrationValidator] IAudioUpdateService validation passed");
            return true;
        }
        
        private bool ValidateFeatureFlags()
        {
            EventLogger.Log("[MigrationValidator] Validating FeatureFlags...");
            
            if (!FeatureFlags.UseServiceLocator)
            {
                EventLogger.LogError("[MigrationValidator] UseServiceLocator is disabled");
                return false;
            }
            
            // Week 3の期待される設定
            if (FeatureFlags.UseNewAudioService && 
                FeatureFlags.UseNewSpatialService && 
                FeatureFlags.UseNewStealthService)
            {
                EventLogger.Log("[MigrationValidator] All new services are enabled");
                return true;
            }
            else
            {
                EventLogger.LogWarning("[MigrationValidator] Some new services are still disabled");
                return false;
            }
        }
    }
}