using UnityEngine;
using UnityEngine.Events;

namespace asterivo.Unity60.Core.Events
{
    /// <summary>
    /// Event channel for boolean values
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoolEventChannel", menuName = "asterivo.Unity60/Events/Bool Event Channel")]
    public class BoolEventChannelSO : ScriptableObject
    {
        [Header("Event Description")]
        [TextArea(2, 4)]
        public string description = "Boolean event for UI state changes, toggles, etc.";
        
        public UnityAction<bool> OnEventRaised;
        
        public void Raise(bool value) 
        {
            OnEventRaised?.Invoke(value);
        }
        
        /// <summary>
        /// For debugging purposes
        /// </summary>
        [ContextMenu("Test Raise True")]
        private void TestRaiseTrue()
        {
            Raise(true);
        }
        
        [ContextMenu("Test Raise False")]
        private void TestRaiseFalse()
        {
            Raise(false);
        }
    }
}