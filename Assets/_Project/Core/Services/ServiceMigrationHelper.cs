using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
// using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Services;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// サービス段階的移行�Eルパ�Eクラス
    /// Step 3.6 で作�Eされた段階的更新パターンの汎用匁E    /// </summary>
    public static class ServiceMigrationHelper
    {
        /// <summary>
        /// 段階的更新の結果チE�Eタ
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
        /// IAudioServiceの段階的取征E        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するぁE/param>
        /// <param name="context">呼び出し�EのコンチE��スト名</param>
        /// <param name="enableDebugLogs">チE��チE��ログを有効にするぁE/param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IAudioService> GetAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IAudioService>();

            LogDebug($"[{context}] Getting IAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取征E            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合�Eそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方弁E            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// IStealthAudioServiceの段階的取征E        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するぁE/param>
        /// <param name="context">呼び出し�EのコンチE��スト名</param>
        /// <param name="enableDebugLogs">チE��チE��ログを有効にするぁE/param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IStealthAudioService> GetStealthAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IStealthAudioService>();

            LogDebug($"[{context}] Getting IStealthAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取征E            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetStealthAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合�Eそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方弁E            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetStealthAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// 段階的更新の状態診断
        /// </summary>
        /// <param name="context">呼び出し�EのコンチE��スト名</param>
/// <summary>
        /// 段階的更新の状態診断
        /// </summary>
        /// <param name="context">呼び出し�EのコンチE��スト名</param>
        public static void DiagnoseMigrationState(string context = "Unknown")
        {
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== Migration State Diagnosis - {context} ===");
            
            // FeatureFlags状慁E            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            ServiceLocator.GetService<IEventLogger>()?.Log($"FeatureFlags.EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // ServiceLocator状慁E            try
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
            
            // レガシーシスチE��状慁E            // CheckLegacySystemState(); // Method not implemented - removed to fix compilation
            
            ServiceLocator.GetService<IEventLogger>()?.Log($"=== End Migration State Diagnosis - {context} ===");
        }

        /// <summary>
        /// 段階的更新の推奨設定取征E        /// </summary>
        /// <returns>推奨設定情報</returns>
        public static (bool useServiceLocator, string reason) GetRecommendedSettings()
        {
            // FeatureFlagsに基づぁE��推奨設定�E判宁E            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
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
        /// ServiceLocatorからIAudioServiceを取征E        /// </summary>
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
                    
                    LogDebug($"[{context}] ✁ESuccessfully obtained IAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IAudioService";
                    LogDebug($"[{context}] ❁EServiceLocator returned null for IAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❁EServiceLocator IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// ServiceLocatorからIStealthAudioServiceを取征E        /// </summary>
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
                    
                    LogDebug($"[{context}] ✁ESuccessfully obtained IStealthAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IStealthAudioService";
                    LogDebug($"[{context}] ❁EServiceLocator returned null for IStealthAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❁EServiceLocator IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIAudioServiceを取征E        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❁ELegacy singletons are disabled", enableDebugLogs);
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
                    
                    LogDebug($"[{context}] ✁ESuccessfully obtained IAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy AudioManager access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy AudioManager not found";
                    LogDebug($"[{context}] ❁ELegacy AudioManager not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❁ELegacy IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIStealthAudioServiceを取征E        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❁ELegacy singletons are disabled", enableDebugLogs);
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
                    
                    LogDebug($"[{context}] ✁ESuccessfully obtained IStealthAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        ServiceLocator.GetService<IEventLogger>()?.LogWarning($"[{context}] Using legacy StealthAudioCoordinator access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy StealthAudioCoordinator not found";
                    LogDebug($"[{context}] ❁ELegacy StealthAudioCoordinator not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❁ELegacy IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// チE��チE��ログ出劁E        /// </summary>
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
        /// 単純なIAudioService取得（エラーハンドリング付き�E�E        /// </summary>
        /// <param name="context">呼び出し�EコンチE��スチE/param>
        /// <returns>IAudioServiceまた�Enull</returns>
        public static IAudioService GetAudioServiceSimple(string context = "Unknown")
        {
            var result = GetAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 単純なIStealthAudioService取得（エラーハンドリング付き�E�E        /// </summary>
        /// <param name="context">呼び出し�EコンチE��スチE/param>
        /// <returns>IStealthAudioServiceまた�Enull</returns>
        public static IStealthAudioService GetStealthAudioServiceSimple(string context = "Unknown")
        {
            var result = GetStealthAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 移行状態�EクイチE��チェチE��
        /// </summary>
        /// <returns>移行が有効かどぁE��</returns>
        public static bool IsMigrationActive()
        {
            return FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator;
        }

        /// <summary>
        /// レガシーシスチE��が利用可能かチェチE��
        /// </summary>
        /// <returns>レガシーシスチE��が利用可能かどぁE��</returns>
        public static bool IsLegacySystemAvailable()
        {
            return FeatureFlags.AllowSingletonFallback;
        }

        #endregion
    }
}