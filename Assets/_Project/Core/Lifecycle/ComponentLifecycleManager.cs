using System.Collections.Generic;
using UnityEngine;
// using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Lifecycle
{
    /// <summary>
    /// 動的に追加されるコンポーネントのライフサイクル管理
    /// メモリリーク防止とリソース管理を行う
    /// </summary>
    public class ComponentLifecycleManager : MonoBehaviour
    {
        private List<Component> managedComponents = new List<Component>();
        private Dictionary<System.Type, int> componentCounts = new Dictionary<System.Type, int>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLog = true;
        [SerializeField] private bool trackComponentCounts = true;
        
        /// <summary>
        /// 動的コンポーネントを安全に追加し、ライフサイクル管理に登録
        /// </summary>
        /// <typeparam name="T">追加するコンポーネントの型</typeparam>
        /// <param name="target">コンポーネントを追加する対象GameObject</param>
        /// <returns>追加されたコンポーネント</returns>
        public T AddManagedComponent<T>(GameObject target) where T : Component
        {
            if (target == null)
            {
                LogError("Cannot add component to null GameObject");
                return null;
            }
            
            // 既存のコンポーネントをチェック
            var existingComponent = target.GetComponent<T>();
            if (existingComponent != null)
            {
                LogWarning($"Component {typeof(T).Name} already exists on {target.name}, returning existing instance");
                RegisterComponent(existingComponent);
                return existingComponent;
            }
            
            // 新しいコンポーネントを追加
            var newComponent = target.AddComponent<T>();
            RegisterComponent(newComponent);
            
            Log($"Added managed component {typeof(T).Name} to {target.name}");
            return newComponent;
        }
        
        /// <summary>
        /// コンポーネントを管理下に登録
        /// </summary>
        /// <param name="component">登録するコンポーネント</param>
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
        /// 管理下のコンポーネントを安全に削除
        /// </summary>
        /// <param name="component">削除するコンポーネント</param>
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
                
                if (component != null) // 削除前にnullチェック
                {
                    DestroyImmediate(component);
                }
            }
        }
        
        /// <summary>
        /// 指定した型のコンポーネントを全て削除
        /// </summary>
        /// <typeparam name="T">削除する型</typeparam>
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
        /// 全ての管理下コンポーネントをクリーンアップ
        /// </summary>
        public void CleanupAllManagedComponents()
        {
            Log($"Cleaning up {managedComponents.Count} managed components");
            
            // 逆順でクリーンアップ（依存関係を考慮）
            for (int i = managedComponents.Count - 1; i >= 0; i--)
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
        /// 現在の管理状況を取得
        /// </summary>
        /// <returns>管理状況の辞書</returns>
        public Dictionary<System.Type, int> GetManagedComponentCounts()
        {
            return new Dictionary<System.Type, int>(componentCounts);
        }
        
        /// <summary>
        /// デバッグ情報を表示
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