using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 繧ｵ繝ｼ繝薙せ谿ｵ髫守噪遘ｻ陦後・繝ｫ繝代・繧ｯ繝ｩ繧ｹ
    /// Step 3.6 縺ｧ菴懈・縺輔ｌ縺滓ｮｵ髫守噪譖ｴ譁ｰ繝代ち繝ｼ繝ｳ縺ｮ豎守畑蛹・    /// </summary>
    public static class ServiceMigrationHelper
    {
        /// <summary>
        /// 谿ｵ髫守噪譖ｴ譁ｰ縺ｮ邨先棡繝・・繧ｿ
        /// </summary>
        public class MigrationResult<T> where T : class
        {
            public T Service { get; set; }
            public bool IsUsingServiceLocator { get; set; }
            public bool IsSuccessful { get; set; }
            public string ServiceTypeName { get; set; }
            public string ErrorMessage { get; set; }

            public MigrationResult()
            {
                IsSuccessful = false;
                ServiceTypeName = "Unknown";
                ErrorMessage = string.Empty;
            }
        }

        /// <summary>
        /// IAudioService縺ｮ谿ｵ髫守噪蜿門ｾ・        /// ServiceLocator蜆ｪ蜈医√ヵ繧ｩ繝ｼ繝ｫ繝舌ャ繧ｯ莉倥″
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocator繧剃ｽｿ逕ｨ縺吶ｋ縺・/param>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・縺ｮ繧ｳ繝ｳ繝・く繧ｹ繝亥錐</param>
        /// <param name="enableDebugLogs">繝・ヰ繝・げ繝ｭ繧ｰ繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・/param>
        /// <returns>蜿門ｾ礼ｵ先棡</returns>
        public static MigrationResult<IAudioService> GetAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IAudioService>();

            LogDebug($"[{context}] Getting IAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator蜆ｪ蜈亥叙蠕・            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocator縺ｧ蜿門ｾ励〒縺阪◆蝣ｴ蜷医・縺昴ｌ繧定ｿ斐☆
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 繝ｬ繧ｬ繧ｷ繝ｼ譁ｹ蠑・            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// IStealthAudioService縺ｮ谿ｵ髫守噪蜿門ｾ・        /// ServiceLocator蜆ｪ蜈医√ヵ繧ｩ繝ｼ繝ｫ繝舌ャ繧ｯ莉倥″
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocator繧剃ｽｿ逕ｨ縺吶ｋ縺・/param>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・縺ｮ繧ｳ繝ｳ繝・く繧ｹ繝亥錐</param>
        /// <param name="enableDebugLogs">繝・ヰ繝・げ繝ｭ繧ｰ繧呈怏蜉ｹ縺ｫ縺吶ｋ縺・/param>
        /// <returns>蜿門ｾ礼ｵ先棡</returns>
        public static MigrationResult<IStealthAudioService> GetStealthAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IStealthAudioService>();

            LogDebug($"[{context}] Getting IStealthAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator蜆ｪ蜈亥叙蠕・            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetStealthAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocator縺ｧ蜿門ｾ励〒縺阪◆蝣ｴ蜷医・縺昴ｌ繧定ｿ斐☆
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: 繝ｬ繧ｬ繧ｷ繝ｼ譁ｹ蠑・            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetStealthAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// 谿ｵ髫守噪譖ｴ譁ｰ縺ｮ迥ｶ諷玖ｨｺ譁ｭ
        /// </summary>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・縺ｮ繧ｳ繝ｳ繝・く繧ｹ繝亥錐</param>
/// <summary>
        /// 谿ｵ髫守噪譖ｴ譁ｰ縺ｮ迥ｶ諷玖ｨｺ譁ｭ
        /// </summary>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・縺ｮ繧ｳ繝ｳ繝・く繧ｹ繝亥錐</param>
        public static void DiagnoseMigrationState(string context = "Unknown")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== Migration State Diagnosis - {context} ===");
            
            // FeatureFlags迥ｶ諷・            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // ServiceLocator迥ｶ諷・            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var stealthService = ServiceLocator.GetService<IStealthAudioService>();
                
                ServiceLocator.GetService<IEventLogger>()?.Log($"ServiceLocator IAudioService: {(audioService != null ? audioService.GetType().Name : "null")}");
                ServiceLocator.GetService<IEventLogger>()?.Log($"ServiceLocator IStealthAudioService: {(stealthService != null ? stealthService.GetType().Name : "null")}");
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"ServiceLocator access failed: {ex.Message}");
            }
            
            // 繝ｬ繧ｬ繧ｷ繝ｼ繧ｷ繧ｹ繝・Β迥ｶ諷・            // CheckLegacySystemState(); // Method not implemented - removed to fix compilation
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== End Migration State Diagnosis - {context} ===");
        }

        /// <summary>
        /// 谿ｵ髫守噪譖ｴ譁ｰ縺ｮ謗ｨ螂ｨ險ｭ螳壼叙蠕・        /// </summary>
        /// <returns>謗ｨ螂ｨ險ｭ螳壽ュ蝣ｱ</returns>
        public static (bool useServiceLocator, string reason) GetRecommendedSettings()
        {
            // FeatureFlags縺ｫ蝓ｺ縺･縺・◆謗ｨ螂ｨ險ｭ螳壹・蛻､螳・            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
            {
                return (true, "ServiceLocator and migration flags are enabled");
            }
            
            if (!FeatureFlags.UseServiceLocator)
            {
                return (false, "ServiceLocator is disabled in FeatureFlags");
            }
            
            if (FeatureFlags.UseServiceLocator && !FeatureFlags.MigrateStealthAudioCoordinator)
            {
                return (false, "ServiceLocator enabled but StealthAudioCoordinator migration disabled");
            }
            
            return (false, "Unable to determine optimal settings");
        }

        #region Private Methods

        /// <summary>
        /// ServiceLocator縺九ｉIAudioService繧貞叙蠕・        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceFromServiceLocator(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            try
            {
                result.Service = ServiceLocator.GetService<IAudioService>();
                
                if (result.Service != null)
                {
                    result.IsUsingServiceLocator = true;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = result.Service.GetType().Name;
                    
                    LogDebug($"[{context}] 笨・Successfully obtained IAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IAudioService";
                    LogDebug($"[{context}] 笶・ServiceLocator returned null for IAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] 笶・ServiceLocator IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// ServiceLocator縺九ｉIStealthAudioService繧貞叙蠕・        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceFromServiceLocator(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            try
            {
                result.Service = ServiceLocator.GetService<IStealthAudioService>();
                
                if (result.Service != null)
                {
                    result.IsUsingServiceLocator = true;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = result.Service.GetType().Name;
                    
                    LogDebug($"[{context}] 笨・Successfully obtained IStealthAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IStealthAudioService";
                    LogDebug($"[{context}] 笶・ServiceLocator returned null for IStealthAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] 笶・ServiceLocator IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// 繝ｬ繧ｬ繧ｷ繝ｼ譁ｹ蠑上〒IAudioService繧貞叙蠕・        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] 笶・Legacy singletons are disabled", enableDebugLogs);
                return result;
            }

            try
            {
#pragma warning disable CS0618
                var legacyAudioManager = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                
                if (legacyAudioManager != null)
                {
                    result.Service = legacyAudioManager;
                    result.IsUsingServiceLocator = false;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = "AudioManager (Legacy)";
                    
                    LogDebug($"[{context}] 笨・Successfully obtained IAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy AudioManager access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy AudioManager not found";
                    LogDebug($"[{context}] 笶・Legacy AudioManager not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] 笶・Legacy IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// 繝ｬ繧ｬ繧ｷ繝ｼ譁ｹ蠑上〒IStealthAudioService繧貞叙蠕・        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] 笶・Legacy singletons are disabled", enableDebugLogs);
                return result;
            }

            try
            {
#pragma warning disable CS0618
                var legacyStealthCoordinator = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
                
                if (legacyStealthCoordinator != null)
                {
                    result.Service = legacyStealthCoordinator;
                    result.IsUsingServiceLocator = false;
                    result.IsSuccessful = true;
                    result.ServiceTypeName = "StealthAudioCoordinator (Legacy)";
                    
                    LogDebug($"[{context}] 笨・Successfully obtained IStealthAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy StealthAudioCoordinator access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy StealthAudioCoordinator not found";
                    LogDebug($"[{context}] 笶・Legacy StealthAudioCoordinator not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] 笶・Legacy IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// 繝・ヰ繝・げ繝ｭ繧ｰ蜃ｺ蜉・        /// </summary>
        private static void LogDebug(string message, bool enableDebugLogs)
        {
            if (enableDebugLogs && FeatureFlags.EnableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log(message);
            }
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// 蜊倡ｴ斐↑IAudioService蜿門ｾ暦ｼ医お繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ莉倥″・・        /// </summary>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・繧ｳ繝ｳ繝・く繧ｹ繝・/param>
        /// <returns>IAudioService縺ｾ縺溘・null</returns>
        public static IAudioService GetAudioServiceSimple(string context = "Unknown")
        {
            var result = GetAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 蜊倡ｴ斐↑IStealthAudioService蜿門ｾ暦ｼ医お繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ莉倥″・・        /// </summary>
        /// <param name="context">蜻ｼ縺ｳ蜃ｺ縺怜・繧ｳ繝ｳ繝・く繧ｹ繝・/param>
        /// <returns>IStealthAudioService縺ｾ縺溘・null</returns>
        public static IStealthAudioService GetStealthAudioServiceSimple(string context = "Unknown")
        {
            var result = GetStealthAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 遘ｻ陦檎憾諷九・繧ｯ繧､繝・け繝√ぉ繝・け
        /// </summary>
        /// <returns>遘ｻ陦後′譛牙柑縺九←縺・°</returns>
        public static bool IsMigrationActive()
        {
            return FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator;
        }

        /// <summary>
        /// 繝ｬ繧ｬ繧ｷ繝ｼ繧ｷ繧ｹ繝・Β縺悟茜逕ｨ蜿ｯ閭ｽ縺九メ繧ｧ繝・け
        /// </summary>
        /// <returns>繝ｬ繧ｬ繧ｷ繝ｼ繧ｷ繧ｹ繝・Β縺悟茜逕ｨ蜿ｯ閭ｽ縺九←縺・°</returns>
        public static bool IsLegacySystemAvailable()
        {
            return FeatureFlags.AllowSingletonFallback;
        }

        #endregion
    }
}