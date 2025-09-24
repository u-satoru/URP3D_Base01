using UnityEngine;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
// using asterivo.Unity60.Core; // 逶ｴ謗･蜿ら・繧帝∩縺代：eatureFlags邨檎罰縺ｫ邨ｱ荳

namespace asterivo.Unity60.Core.Helpers
{
    /// <summary>
    /// 繧ｵ繝ｼ繝薙せ蜿門ｾ励・邨ｱ荳繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// DRY蜴溷援驕募渚繧定ｧ｣豸医＠縲√し繝ｼ繝薙せ蜿門ｾ励Ο繧ｸ繝・け繧剃ｸ蜈・喧
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// ServiceLocator繧貞━蜈医＠縲∝､ｱ謨玲凾縺ｯFindFirstObjectByType縺ｫ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ
        /// </summary>
        public static T GetServiceWithFallback<T>() where T : class
        {
            // ServiceLocator菴ｿ逕ｨ・域耳螂ｨ・・            if (FeatureFlags.UseServiceLocator)
            {
                var service = ServiceLocator.GetService<T>();
                if (service != null) 
                {
                    LogServiceAcquisition<T>("ServiceLocator");
                    return service;
                }
            }
            
            // 繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ: Unity讓呎ｺ匁､懃ｴ｢・磯幕逋ｺ繝薙Ν繝・繧ｨ繝・ぅ繧ｿ髯仙ｮ夲ｼ・            #if UNITY_EDITOR || DEVELOPMENT_BUILD
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

            // 譛ｬ逡ｪ繝薙Ν繝峨〒縺ｯ繝輔か繝ｼ繝ｫ繝舌ャ繧ｯ縺励↑縺・            LogServiceNotFound<T>();
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
