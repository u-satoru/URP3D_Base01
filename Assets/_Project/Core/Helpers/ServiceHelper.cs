using UnityEngine;
// using _Project.Core; // 直接参照を避け、CoreFeatureFlags経由に統一

namespace asterivo.Unity60.Core.Helpers
{
    /// <summary>
    /// サービス取得の統一インターフェース
    /// DRY原則違反を解消し、サービス取得ロジックを一元化
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ServiceLocatorを優先し、失敗時はFindFirstObjectByTypeにフォールバック
        /// </summary>
        public static T GetServiceWithFallback<T>() where T : class
        {
            // ServiceLocator使用（推奨）
            if (CoreFeatureFlags.UseServiceLocator)
            {
                var service = ServiceLocator.GetService<T>();
                if (service != null) 
                {
                    LogServiceAcquisition<T>("ServiceLocator");
                    return service;
                }
            }
            
            // フォールバック: Unity標準検索（開発ビルド/エディタ限定）
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
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

            // 本番ビルドではフォールバックしない
            LogServiceNotFound<T>();
            return null;
        }
        
        private static void LogServiceAcquisition<T>(string method)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (CoreFeatureFlags.EnableDebugLogging)
            {
                UnityEngine.Debug.Log($"[ServiceHelper] {typeof(T).Name} acquired via {method}");
            }
            #endif
        }
        
        private static void LogServiceNotFound<T>()
        {
            UnityEngine.Debug.LogWarning($"[ServiceHelper] Failed to acquire service: {typeof(T).Name}");
        }
    }
}
