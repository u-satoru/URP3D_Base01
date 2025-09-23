using System.Collections.Generic;
using UnityEngine;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Lifecycle
{
    /// <summary>
    /// 動的に追加されるコンポ�Eネント�Eライフサイクル管琁E    /// メモリリーク防止とリソース管琁E��行う
    /// </summary>
    public class ComponentLifecycleManager : MonoBehaviour
    {
        private List<Component> managedComponents = new List<Component>();
        private Dictionary<System.Type, int> componentCounts = new Dictionary<System.Type, int>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool trackComponentCounts = true;
        
        /// <summary>
        /// 動的コンポ�Eネントを安�Eに追加し、ライフサイクル管琁E��登録
        /// </summary>
        /// <typeparam name="T">追加するコンポ�Eネント�E垁E/typeparam>
        /// <param name="target">コンポ�Eネントを追加する対象GameObject</param>
        /// <returns>追加されたコンポ�EネンチE/returns>
        public T AddManagedComponent<T>(GameObject target) where T : Component
        {
            if (target == null)
            {
                LogError("Cannot add component to null GameObject");
                return null;
            }
            
            // 既存�Eコンポ�EネントをチェチE��
            var existingComponent = target.GetComponent<T>();
            if (existingComponent != null)
            {
                LogWarning($"Component {typeof(T).Name} already exists on {target.name}, returning existing instance");
                RegisterComponent(existingComponent);
                return existingComponent;
            }
            
            // 新しいコンポ�Eネントを追加
            var newComponent = target.AddComponent<T>();
            RegisterComponent(newComponent);
            
            Log($"Added managed component {typeof(T).Name} to {target.name}");
            return newComponent;
        }
        
        /// <summary>
        /// コンポ�Eネントを管琁E��に登録
        /// </summary>
        /// <param name="component">登録するコンポ�EネンチE/param>
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
        /// 管琁E���Eコンポ�Eネントを安�Eに削除
        /// </summary>
        /// <param name="component">削除するコンポ�EネンチE/param>
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
                
                if (component != null) // 削除前にnullチェチE��
                {
                    DestroyImmediate(component);
                }
            }
        }
        
        /// <summary>
        /// 持E��した型のコンポ�Eネントを全て削除
        /// </summary>
        /// <typeparam name="T">削除する垁E/typeparam>
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
        /// 全ての管琁E��コンポ�EネントをクリーンアチE�E
        /// </summary>
        public void CleanupAllManagedComponents()
        {
            Log($"Cleaning up {managedComponents.Count} managed components");
            
            // 送E��E��クリーンアチE�E�E�依存関係を老E�E�E�E            for (int i = managedComponents.Count - 1; i >= 0; i--)
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
        /// 現在の管琁E��況を取征E        /// </summary>
        /// <returns>管琁E��況�E辞書</returns>
        public Dictionary<System.Type, int> GetManagedComponentCounts()
        {
            return new Dictionary<System.Type, int>(componentCounts);
        }
        
        /// <summary>
        /// チE��チE��惁E��を表示
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