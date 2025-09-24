using UnityEngine;
using UnityEngine.Events;
// // using asterivo.Unity60.Core.Debug; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Vector2繝代Λ繝｡繝ｼ繧ｿ莉倥″繧､繝吶Φ繝医Μ繧ｹ繝翫・
    /// 繧ｫ繝｡繝ｩ繝ｫ繝・け蜈･蜉帷畑
    /// </summary>
    public class Vector2GameEventListener : MonoBehaviour, IGameEventListener<Vector2>
    {
        [Header("Event Settings")]
        [SerializeField] private Vector2GameEvent gameEvent;
        [SerializeField] private int priority = 0;
        
        [Header("Response")]
        [SerializeField] private Vector2UnityEvent response;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        public int Priority => priority;
        public Vector2GameEvent GameEvent => gameEvent;
        public Vector2UnityEvent Response => response;
        
        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.RegisterListener(this);
            }
            else if (debugMode)
            {
                UnityEngine.Debug.LogWarning($"[{name}] GameEvent is not assigned!");
            }
        }
        
        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.UnregisterListener(this);
            }
        }
        
        /// <summary>
        /// Vector2GameEvent縺檎匱逕溘＠縺滓凾縺ｮ蜃ｦ逅・        /// </summary>
        /// <param name="value">Vector2蛟､</param>
        public void OnEventRaised(Vector2 value)
        {
            // 繝｡繧ｽ繝・ラ縺瑚ｨｭ螳壹＆繧後※縺・ｋ蝣ｴ蜷医・縺ｿ螳溯｡・            if (Response != null)
            {
                Response.Invoke(value);
            }
        }
        
        /// <summary>
        /// 謇句虚縺ｧ繧､繝吶Φ繝医ｒ逋ｺ陦鯉ｼ医ユ繧ｹ繝育畑・・        /// </summary>
        [ContextMenu("Raise Event (Test)")]
        public void RaiseEventTest()
        {
            if (gameEvent != null)
            {
                gameEvent.Raise(Vector2.one);
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameEvent == null && debugMode)
            {
                UnityEngine.Debug.LogWarning($"[{name}] GameEvent reference is missing!");
            }
        }
        #endif
    }
    
    /// <summary>
    /// Vector2繝代Λ繝｡繝ｼ繧ｿ逕ｨUnityEvent
    /// </summary>
    [System.Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }
}
