using UnityEngine;

using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 遘ｻ陦悟ｮ御ｺ・憾豕√・讀懆ｨｼ繝・・繝ｫ
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
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Starting migration validation...");
            
            bool allPassed = true;
            
            // ServiceLocator蝓ｺ譛ｬ讖溯・縺ｮ讀懆ｨｼ
            allPassed &= ValidateServiceLocatorBasics();
            
            // 蜷・し繝ｼ繝薙せ縺ｮ讀懆ｨｼ
            allPassed &= ValidateAudioService();
            allPassed &= ValidateSpatialAudioService();
            allPassed &= ValidateStealthAudioService();
            allPassed &= ValidateEffectService();
            allPassed &= ValidateAudioUpdateService();
            
            // Feature Flags縺ｮ讀懆ｨｼ
            allPassed &= ValidateFeatureFlags();
            
            string result = allPassed ? "PASSED" : "FAILED";
            ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationValidator] Migration validation {result}");
        }
        
        private bool ValidateServiceLocatorBasics()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating ServiceLocator basics...");
            
            // ServiceLocator縺悟虚菴懊＠縺ｦ縺・ｋ縺薙→繧堤｢ｺ隱・            int serviceCount = ServiceLocator.GetServiceCount();
            if (serviceCount == 0)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] ServiceLocator has no registered services");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"[MigrationValidator] ServiceLocator has {serviceCount} registered services");
            return true;
        }
        
        private bool ValidateAudioService()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating AudioService...");
            
            var audioService = ServiceLocator.GetService<IAudioService>();
            if (audioService == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] IAudioService not found in ServiceLocator");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] IAudioService validation passed");
            return true;
        }
        
        private bool ValidateSpatialAudioService()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating SpatialAudioService...");
            
            var spatialService = ServiceLocator.GetService<ISpatialAudioService>();
            if (spatialService == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] ISpatialAudioService not found in ServiceLocator");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] ISpatialAudioService validation passed");
            return true;
        }
        
        private bool ValidateStealthAudioService()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating StealthAudioService...");
            
            var stealthService = ServiceLocator.GetService<IStealthAudioService>();
            if (stealthService == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] IStealthAudioService not found in ServiceLocator");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] IStealthAudioService validation passed");
            return true;
        }
        
        private bool ValidateEffectService()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating EffectService...");
            
            var effectService = ServiceLocator.GetService<IEffectService>();
            if (effectService == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] IEffectService not found in ServiceLocator");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] IEffectService validation passed");
            return true;
        }
        
        private bool ValidateAudioUpdateService()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating AudioUpdateService...");
            
            var updateService = ServiceLocator.GetService<IAudioUpdateService>();
            if (updateService == null)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] IAudioUpdateService not found in ServiceLocator");
                return false;
            }
            
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] IAudioUpdateService validation passed");
            return true;
        }
        
        private bool ValidateFeatureFlags()
        {
            ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] Validating FeatureFlags...");
            
            if (!FeatureFlags.UseServiceLocator)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError("[MigrationValidator] UseServiceLocator is disabled");
                return false;
            }
            
            // Week 3縺ｮ譛溷ｾ・＆繧後ｋ險ｭ螳・            if (FeatureFlags.UseNewAudioService && 
                FeatureFlags.UseNewSpatialService && 
                FeatureFlags.UseNewStealthService)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log("[MigrationValidator] All new services are enabled");
                return true;
            }
            else
            {
                ServiceLocator.GetService<IEventLogger>()?.LogWarning("[MigrationValidator] Some new services are still disabled");
                return false;
            }
        }
    }
}