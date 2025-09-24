using System.Collections.Generic;
using UnityEngine;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Lifecycle
{
    /// <summary>
    /// 蜍慕噪縺ｫ霑ｽ蜉縺輔ｌ繧九さ繝ｳ繝昴・繝阪Φ繝医・繝ｩ繧､繝輔し繧､繧ｯ繝ｫ邂｡逅・    /// 繝｡繝｢繝ｪ繝ｪ繝ｼ繧ｯ髦ｲ豁｢縺ｨ繝ｪ繧ｽ繝ｼ繧ｹ邂｡逅・ｒ陦後≧
    /// </summary>
    public class ComponentLifecycleManager : MonoBehaviour
    {
        private List<Component> managedComponents = new List<Component>();
        private Dictionary<System.Type, int> componentCounts = new Dictionary<System.Type, int>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool trackComponentCounts = true;
        
        /// <summary>
        /// 蜍慕噪繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ螳牙・縺ｫ霑ｽ蜉縺励√Λ繧､繝輔し繧､繧ｯ繝ｫ邂｡逅・↓逋ｻ骭ｲ
        /// </summary>
        /// <typeparam name="T">霑ｽ蜉縺吶ｋ繧ｳ繝ｳ繝昴・繝阪Φ繝医・蝙・/typeparam>
        /// <param name="target">繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ霑ｽ蜉縺吶ｋ蟇ｾ雎｡GameObject</param>
        /// <returns>霑ｽ蜉縺輔ｌ縺溘さ繝ｳ繝昴・繝阪Φ繝・/returns>
        public T AddManagedComponent<T>(GameObject target) where T : Component
        {
            if (target == null)
            {
                LogError("Cannot add component to null GameObject");
                return null;
            }
            
            // 譌｢蟄倥・繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ繝√ぉ繝・け
            var existingComponent = target.GetComponent<T>();
            if (existingComponent != null)
            {
                LogWarning($"Component {typeof(T).Name} already exists on {target.name}, returning existing instance");
                RegisterComponent(existingComponent);
                return existingComponent;
            }
            
            // 譁ｰ縺励＞繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ霑ｽ蜉
            var newComponent = target.AddComponent<T>();
            RegisterComponent(newComponent);
            
            Log($"Added managed component {typeof(T).Name} to {target.name}");
            return newComponent;
        }
        
        /// <summary>
        /// 繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ邂｡逅・ｸ九↓逋ｻ骭ｲ
        /// </summary>
        /// <param name="component">逋ｻ骭ｲ縺吶ｋ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        private void RegisterComponent(Component component)
        {
            if (component == null) return;
            
            if (!managedComponents.Contains(component))
            {
                managedComponents.Add(component);
                
                if (trackComponentCounts)
                {
                    var type = component.GetType();
                    componentCounts[type] = componentCounts.GetValueOrDefault(type) + 1;
                }
            }
        }
        
        /// <summary>
        /// 邂｡逅・ｸ九・繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ螳牙・縺ｫ蜑企勁
        /// </summary>
        /// <param name="component">蜑企勁縺吶ｋ繧ｳ繝ｳ繝昴・繝阪Φ繝・/param>
        public void RemoveManagedComponent(Component component)
        {
            if (component == null) return;
            
            if (managedComponents.Contains(component))
            {
                managedComponents.Remove(component);
                
                if (trackComponentCounts)
                {
                    var type = component.GetType();
                    if (componentCounts.ContainsKey(type))
                    {
                        componentCounts[type] = Mathf.Max(0, componentCounts[type] - 1);
                    }
                }
                
                Log($"Removed managed component {component.GetType().Name}");
                
                if (component != null) // 蜑企勁蜑阪↓null繝√ぉ繝・け
                {
                    DestroyImmediate(component);
                }
            }
        }
        
        /// <summary>
        /// 謖・ｮ壹＠縺溷梛縺ｮ繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜈ｨ縺ｦ蜑企勁
        /// </summary>
        /// <typeparam name="T">蜑企勁縺吶ｋ蝙・/typeparam>
        public void RemoveAllManagedComponents<T>() where T : Component
        {
            var componentsToRemove = new List<Component>();
            
            foreach (var component in managedComponents)
            {
                if (component is T)
                {
                    componentsToRemove.Add(component);
                }
            }
            
            foreach (var component in componentsToRemove)
            {
                RemoveManagedComponent(component);
            }
        }
        
        /// <summary>
        /// 蜈ｨ縺ｦ縺ｮ邂｡逅・ｸ九さ繝ｳ繝昴・繝阪Φ繝医ｒ繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・
        /// </summary>
        public void CleanupAllManagedComponents()
        {
            Log($"Cleaning up {managedComponents.Count} managed components");
            
            // 騾・・〒繧ｯ繝ｪ繝ｼ繝ｳ繧｢繝・・・井ｾ晏ｭ倬未菫ゅｒ閠・・・・            for (int i = managedComponents.Count - 1; i >= 0; i--)
            {
                var component = managedComponents[i];
                if (component != null)
                {
                    DestroyImmediate(component);
                }
            }
            
            managedComponents.Clear();
            componentCounts.Clear();
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ邂｡逅・憾豕√ｒ蜿門ｾ・        /// </summary>
        /// <returns>邂｡逅・憾豕√・霎樊嶌</returns>
        public Dictionary<System.Type, int> GetManagedComponentCounts()
        {
            return new Dictionary<System.Type, int>(componentCounts);
        }
        
        /// <summary>
        /// 繝・ヰ繝・げ諠・ｱ繧定｡ｨ遉ｺ
        /// </summary>
        [ContextMenu("Show Debug Info")]
        public void ShowDebugInfo()
        {
            Log($"=== ComponentLifecycleManager Debug Info ===");
            Log($"Total managed components: {managedComponents.Count}");
            
            if (trackComponentCounts)
            {
                foreach (var kvp in componentCounts)
                {
                    Log($"  {kvp.Key.Name}: {kvp.Value}");
                }
            }
        }
        
        private void OnDestroy()
        {
            CleanupAllManagedComponents();
        }
        
        #region Logging
        private void Log(string message)
        {
            if (enableDebugLog)
                Debug.Log($"[ComponentLifecycleManager] {message}");
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLog)
                Debug.LogWarning($"[ComponentLifecycleManager] {message}");
        }
        
        private void LogError(string message)
        {
            if (enableDebugLog)
                Debug.LogError($"[ComponentLifecycleManager] {message}");
        }
        #endregion
    }
}
