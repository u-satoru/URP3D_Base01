using UnityEngine;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
// using asterivo.Unity60.Core; // 直接参�Eを避け、FeatureFlags経由に統一

namespace asterivo.Unity60.Core.Helpers
{
    /// <summary>
    /// サービス取得�E統一インターフェース
    /// DRY原則違反を解消し、サービス取得ロジチE��を一允E��
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ServiceLocatorを優先し、失敗時はFindFirstObjectByTypeにフォールバック
        /// </summary>
        public static T GetServiceWithFallback<T>() where T : class
        {
            // ServiceLocator使用�E�推奨�E�E            if (FeatureFlags.UseServiceLocator)
            {
                var service = ServiceLocator.GetService<T>();
                if (service != null) 
                {
                    LogServiceAcquisition<T>("ServiceLocator");
                    return service;
                }
            }
            
            // フォールバック: Unity標準検索�E�開発ビルチEエチE��タ限定！E            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object)))
            {
                var unityObject = UnityEngine.Object.FindFirstObjectByType(typeof(T)) as T;
                if (unityObject != null)
                {
                    LogServiceAcquisition<T>("FindFirstObjectByType");
                }
                return unityObject;
            }
            #endif

            // 本番ビルドではフォールバックしなぁE            LogServiceNotFound<T>();
            return null;
        }
        
        private static void LogServiceAcquisition<T>(string method)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (FeatureFlags.EnableDebugLogging)
            {
                Debug.Log($"[ServiceHelper] {typeof(T).Name} acquired via {method}");
            }
            #endif
        }
        
        private static void LogServiceNotFound<T>()
        {
            Debug.LogWarning($"[ServiceHelper] Failed to acquire service: {typeof(T).Name}");
        }
    }
}
