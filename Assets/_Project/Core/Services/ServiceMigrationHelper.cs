using UnityEngine;
using _Project.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;

namespace _Project.Core.Services
{
    /// <summary>
    /// サービス段階的移行ヘルパークラス
    /// Step 3.6 で作成された段階的更新パターンの汎用化
    /// </summary>
    public static class ServiceMigrationHelper
    {
        /// <summary>
        /// 段階的更新の結果データ
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
        /// IAudioServiceの段階的取得
        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するか</param>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        /// <param name="enableDebugLogs">デバッグログを有効にするか</param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IAudioService> GetAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IAudioService>();

            LogDebug($"[{context}] Getting IAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取得
            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合はそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方式
            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// IStealthAudioServiceの段階的取得
        /// ServiceLocator優先、フォールバック付き
        /// </summary>
        /// <param name="useServiceLocator">ServiceLocatorを使用するか</param>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        /// <param name="enableDebugLogs">デバッグログを有効にするか</param>
        /// <returns>取得結果</returns>
        public static MigrationResult<IStealthAudioService> GetStealthAudioService(
            bool useServiceLocator = true, 
            string context = "Unknown", 
            bool enableDebugLogs = true)
        {
            var result = new MigrationResult<IStealthAudioService>();

            LogDebug($"[{context}] Getting IStealthAudioService (useServiceLocator: {useServiceLocator})", enableDebugLogs);

            // ServiceLocator優先取得
            if (useServiceLocator && FeatureFlags.UseServiceLocator)
            {
                result = GetStealthAudioServiceFromServiceLocator(context, enableDebugLogs);
                
                // ServiceLocatorで取得できた場合はそれを返す
                if (result.IsSuccessful)
                {
                    return result;
                }
            }

            // フォールバック: レガシー方式
            LogDebug($"[{context}] ServiceLocator failed, trying legacy fallback", enableDebugLogs);
            return GetStealthAudioServiceLegacy(context, enableDebugLogs);
        }

        /// <summary>
        /// 段階的更新の状態診断
        /// </summary>
        /// <param name="context">呼び出し元のコンテキスト名</param>
        public static void DiagnoseMigrationState(string context = "Unknown")
        {
            EventLogger.Log($"=== Migration State Diagnosis - {context} ===");
            
            // FeatureFlags状態
            EventLogger.Log($"FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            EventLogger.Log($"FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
            EventLogger.Log($"FeatureFlags.EnableDebugLogging: {FeatureFlags.EnableDebugLogging}");
            EventLogger.Log($"FeatureFlags.EnableMigrationMonitoring: {FeatureFlags.EnableMigrationMonitoring}");
            
            // ServiceLocator状態
            try
            {
                var audioService = ServiceLocator.GetService<IAudioService>();
                var stealthService = ServiceLocator.GetService<IStealthAudioService>();
                
                EventLogger.Log($"ServiceLocator IAudioService: {(audioService != null ? audioService.GetType().Name : "null")}");
                EventLogger.Log($"ServiceLocator IStealthAudioService: {(stealthService != null ? stealthService.GetType().Name : "null")}");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogError($"ServiceLocator access failed: {ex.Message}");
            }
            
            // レガシーシステム状態
            try
            {
                var legacyAudioManager = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                var legacyStealthCoordinator = Object.FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();
                
                EventLogger.Log($"Legacy AudioManager: {(legacyAudioManager != null ? "Found" : "Not Found")}");
                EventLogger.Log($"Legacy StealthAudioCoordinator: {(legacyStealthCoordinator != null ? "Found" : "Not Found")}");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogError($"Legacy system check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 段階的更新の推奨設定取得
        /// </summary>
        /// <returns>推奨設定情報</returns>
        public static (bool useServiceLocator, string reason) GetRecommendedSettings()
        {
            // FeatureFlagsに基づいた推奨設定の判定
            if (FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator)
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
        /// ServiceLocatorからIAudioServiceを取得
        /// </summary>
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
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IAudioService";
                    LogDebug($"[{context}] ❌ ServiceLocator returned null for IAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ ServiceLocator IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// ServiceLocatorからIStealthAudioServiceを取得
        /// </summary>
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
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IStealthAudioService from ServiceLocator: {result.ServiceTypeName}", enableDebugLogs);
                }
                else
                {
                    result.ErrorMessage = "ServiceLocator returned null for IStealthAudioService";
                    LogDebug($"[{context}] ❌ ServiceLocator returned null for IStealthAudioService", enableDebugLogs);
                }
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ ServiceLocator IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIAudioServiceを取得
        /// </summary>
        private static MigrationResult<IAudioService> GetAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❌ Legacy singletons are disabled", enableDebugLogs);
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
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        EventLogger.LogWarning($"[{context}] Using legacy AudioManager access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy AudioManager not found";
                    LogDebug($"[{context}] ❌ Legacy AudioManager not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ Legacy IAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// レガシー方式でIStealthAudioServiceを取得
        /// </summary>
        private static MigrationResult<IStealthAudioService> GetStealthAudioServiceLegacy(string context, bool enableDebugLogs)
        {
            var result = new MigrationResult<IStealthAudioService>();

            if (!FeatureFlags.AllowSingletonFallback)
            {
                result.ErrorMessage = "Legacy singletons are disabled";
                LogDebug($"[{context}] ❌ Legacy singletons are disabled", enableDebugLogs);
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
                    
                    LogDebug($"[{context}] ✅ Successfully obtained IStealthAudioService from legacy system", enableDebugLogs);
                    
                    if (FeatureFlags.EnableMigrationMonitoring)
                    {
                        EventLogger.LogWarning($"[{context}] Using legacy StealthAudioCoordinator access");
                    }
                }
                else
                {
                    result.ErrorMessage = "Legacy StealthAudioCoordinator not found";
                    LogDebug($"[{context}] ❌ Legacy StealthAudioCoordinator not found", enableDebugLogs);
                }
#pragma warning restore CS0618
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = ex.Message;
                LogDebug($"[{context}] ❌ Legacy IStealthAudioService access failed: {ex.Message}", enableDebugLogs);
            }

            return result;
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private static void LogDebug(string message, bool enableDebugLogs)
        {
            if (enableDebugLogs && FeatureFlags.EnableDebugLogging)
            {
                EventLogger.Log(message);
            }
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// 単純なIAudioService取得（エラーハンドリング付き）
        /// </summary>
        /// <param name="context">呼び出し元コンテキスト</param>
        /// <returns>IAudioServiceまたはnull</returns>
        public static IAudioService GetAudioServiceSimple(string context = "Unknown")
        {
            var result = GetAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 単純なIStealthAudioService取得（エラーハンドリング付き）
        /// </summary>
        /// <param name="context">呼び出し元コンテキスト</param>
        /// <returns>IStealthAudioServiceまたはnull</returns>
        public static IStealthAudioService GetStealthAudioServiceSimple(string context = "Unknown")
        {
            var result = GetStealthAudioService(true, context, false);
            return result.IsSuccessful ? result.Service : null;
        }

        /// <summary>
        /// 移行状態のクイックチェック
        /// </summary>
        /// <returns>移行が有効かどうか</returns>
        public static bool IsMigrationActive()
        {
            return FeatureFlags.UseServiceLocator && FeatureFlags.MigrateStealthAudioCoordinator;
        }

        /// <summary>
        /// レガシーシステムが利用可能かチェック
        /// </summary>
        /// <returns>レガシーシステムが利用可能かどうか</returns>
        public static bool IsLegacySystemAvailable()
        {
            return FeatureFlags.AllowSingletonFallback;
        }

        #endregion
    }
}