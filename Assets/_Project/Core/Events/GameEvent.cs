using UnityEngine;
using System.Collections.Generic;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency
using System.Linq;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// 繝代Λ繝｡繝ｼ繧ｿ縺ｪ縺励・蝓ｺ譛ｬ繧､繝吶Φ繝医メ繝｣繝阪Ν
    /// Unity 6譛驕ｩ蛹也沿 - 蜆ｪ蜈亥ｺｦ莉倥″繝ｪ繧ｹ繝翫・邂｡逅・ｯｾ蠢・    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "asterivo.Unity60/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // 繝ｪ繧ｹ繝翫・縺ｮHashSet縺ｫ繧医ｋ鬮倬溽ｮ｡逅・        private readonly HashSet<IGameEventListener> listeners = new HashSet<IGameEventListener>();

        // 蜆ｪ蜈亥ｺｦ繧ｽ繝ｼ繝域ｸ医∩繝ｪ繧ｹ繝翫・繝ｪ繧ｹ繝茨ｼ医く繝｣繝・す繝･・・        private List<IGameEventListener> sortedListeners;
        private bool isDirty = true;
        
        #if UNITY_EDITOR
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField, TextArea(3, 5)] private string eventDescription;
        
        // 繧ｨ繝・ぅ繧ｿ逕ｨ繝・ヰ繝・げ諠・ｱ
        [Header("Runtime Info (Editor Only)")]
        [SerializeField, asterivo.Unity60.Core.Attributes.ReadOnly] private int listenerCount;
        #endif

        /// <summary>
        /// 繧､繝吶Φ繝医ｒ逋ｺ轣ｫ縺吶ｋ
        /// </summary>
        public void Raise()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (debugMode)
            {
                UnityEngine.Debug.Log($"<color=cyan>[GameEvent]</color> '{name}' raised at {Time.time:F2}s with {listeners.Count} listeners", this);
            }
            #endif
            #if UNITY_EDITOR
            listenerCount = listeners.Count;
            #endif
            
            // 繧､繝吶Φ繝医Ο繧ｰ縺ｫ險倬鹸・育ｰ｡逡･蛹也沿・・            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"[GameEvent] {name} raised to {listeners.Count} listeners");
            #endif
            
            // 蜆ｪ蜈亥ｺｦ縺ｧ繧ｽ繝ｼ繝茨ｼ亥ｿ・ｦ∵凾縺ｮ縺ｿ・・            if (isDirty)
            {
                RebuildSortedList();
            }
            
            // 騾・・〒螳溯｡鯉ｼ医Μ繧ｹ繝翫・縺瑚・霄ｫ繧貞炎髯､縺励※繧ょｮ牙・・・            for (int i = sortedListeners.Count - 1; i >= 0; i--)
            {
                if (sortedListeners[i] != null && sortedListeners[i].enabled)
                {
                    sortedListeners[i].OnEventRaised();
                }
            }
        }
        
        /// <summary>
        /// 髱槫酔譛溘〒繧､繝吶Φ繝医ｒ逋ｺ轣ｫ・医ヵ繝ｬ繝ｼ繝蛻・淵・・        /// </summary>
        public System.Collections.IEnumerator RaiseAsync()
        {
            if (isDirty)
            {
                RebuildSortedList();
            }
            
            foreach (var listener in sortedListeners)
            {
                if (listener != null && listener.enabled)
                {
                    listener.OnEventRaised();
                    yield return null; // 谺｡繝輔Ξ繝ｼ繝縺ｸ
                }
            }
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝翫・繧堤匳骭ｲ
        /// </summary>
        public void RegisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Add(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=green>[GameEvent]</color> Listener registered to '{name}'", this);
                }
                #endif
            }
        }

        /// <summary>
        /// 繝ｪ繧ｹ繝翫・繧定ｧ｣髯､
        /// </summary>
        public void UnregisterListener(IGameEventListener listener)
        {
            if (listener == null) return;
            
            if (listeners.Remove(listener))
            {
                isDirty = true;
                
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (debugMode)
                {
                    UnityEngine.Debug.Log($"<color=yellow>[GameEvent]</color> Listener unregistered from '{name}'", this);
                }
                #endif
            }
        }
        
        /// <summary>
        /// 蜈ｨ繝ｪ繧ｹ繝翫・繧偵け繝ｪ繧｢
        /// </summary>
        public void ClearAllListeners()
        {
            listeners.Clear();
            sortedListeners?.Clear();
            isDirty = true;
        }
        
        /// <summary>
        /// 繧｢繧ｯ繝・ぅ繝悶↑繝ｪ繧ｹ繝翫・謨ｰ繧貞叙蠕・        /// </summary>
        public int GetListenerCount() => listeners.Count;
        
        /// <summary>
        /// 繝ｪ繧ｹ繝翫・繝ｪ繧ｹ繝医ｒ蜀肴ｧ狗ｯ・        /// </summary>
        private void RebuildSortedList()
        {
            sortedListeners = listeners
                .Where(l => l != null)
                .OrderByDescending(l => l.Priority)
                .ToList();
            isDirty = false;
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ・壽焔蜍輔〒繧､繝吶Φ繝医ｒ逋ｺ轣ｫ
        /// </summary>
        [ContextMenu("Raise Event")]
        private void RaiseManually()
        {
            Raise();
        }
        
        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繝ｪ繧ｹ繝翫・繧偵Ο繧ｰ蜃ｺ蜉・        /// </summary>
        [ContextMenu("Log All Listeners")]
        private void LogListeners()
        {
            UnityEngine.Debug.Log($"=== Listeners for '{name}' ===");
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    var component = listener as Component;
                    if (component != null)
                    {
                        UnityEngine.Debug.Log($"  - {component.gameObject.name}.{listener.GetType().Name} (Priority: {listener.Priority})", component);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"  - {listener.GetType().Name} (Priority: {listener.Priority})");
                    }
                }
            }
        }
        #endif
    }
    
    #if UNITY_EDITOR
    // 繧ｨ繝・ぅ繧ｿ逕ｨ縺ｮReadOnly螻樊ｧ
    namespace asterivo.Unity60.Core.Attributes
    {
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
    #endif
}
