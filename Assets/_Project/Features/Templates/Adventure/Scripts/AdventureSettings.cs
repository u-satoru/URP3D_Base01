using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure
{
    /// <summary>
    /// Adventure Template Settings
    /// Configuration data for adventure game template behavior and parameters
    /// </summary>
    [CreateAssetMenu(fileName = "AdventureSettings", menuName = "Templates/Adventure/Adventure Settings")]
    public class AdventureSettings : ScriptableObject
    {
        [TabGroup("General", "Basic Settings")]
        [Header("Template Behavior")]
        [SerializeField] 
        [Tooltip("Automatically activate template on scene start")]
        public bool autoActivateOnStart = true;
        
        [TabGroup("General", "Basic Settings")]
        [SerializeField]
        [Tooltip("Enable debug logging for template operations")]
        public bool enableDebugLogging = true;
        
        [TabGroup("General", "Basic Settings")]
        [SerializeField]
        [Tooltip("Template update frequency in seconds")]
        [Range(0.1f, 2.0f)]
        public float updateFrequency = 0.5f;
        
        [TabGroup("Dialogue", "Dialogue Settings")]
        [Header("Dialogue Configuration")]
        [SerializeField]
        [Tooltip("Default text display speed (characters per second)")]
        [Range(10f, 100f)]
        public float defaultTextSpeed = 30f;
        
        [TabGroup("Dialogue", "Dialogue Settings")]
        [SerializeField]
        [Tooltip("Auto-advance dialogue after this time (0 = manual only)")]
        [Range(0f, 10f)]
        public float autoAdvanceDelay = 0f;
        
        [TabGroup("Dialogue", "Dialogue Settings")]
        [SerializeField]
        [Tooltip("Enable voice acting audio support")]
        public bool enableVoiceActing = false;
        
        [TabGroup("Dialogue", "Dialogue Settings")]
        [SerializeField]
        [Tooltip("Default dialogue audio volume")]
        [Range(0f, 1f)]
        public float dialogueVolume = 0.8f;
        
        [TabGroup("Quests", "Quest Settings")]
        [Header("Quest Configuration")]
        [SerializeField]
        [Tooltip("Maximum number of active quests")]
        [Range(1, 20)]
        public int maxActiveQuests = 10;
        
        [TabGroup("Quests", "Quest Settings")]
        [SerializeField]
        [Tooltip("Show quest notifications")]
        public bool showQuestNotifications = true;
        
        [TabGroup("Quests", "Quest Settings")]
        [SerializeField]
        [Tooltip("Quest notification display duration")]
        [Range(1f, 10f)]
        public float questNotificationDuration = 3f;
        
        [TabGroup("Quests", "Quest Settings")]
        [SerializeField]
        [Tooltip("Enable quest auto-tracking")]
        public bool enableQuestAutoTracking = true;
        
        [TabGroup("Inventory", "Inventory Settings")]
        [Header("Inventory Configuration")]
        [SerializeField]
        [Tooltip("Maximum inventory slots")]
        [Range(10, 100)]
        public int maxInventorySlots = 50;
        
        [TabGroup("Inventory", "Inventory Settings")]
        [SerializeField]
        [Tooltip("Enable item stacking")]
        public bool enableItemStacking = true;
        
        [TabGroup("Inventory", "Inventory Settings")]
        [SerializeField]
        [Tooltip("Show item tooltips")]
        public bool showItemTooltips = true;
        
        [TabGroup("Inventory", "Inventory Settings")]
        [SerializeField]
        [Tooltip("Enable inventory sorting")]
        public bool enableInventorySorting = true;
        
        [TabGroup("Interaction", "Interaction Settings")]
        [Header("Interaction Configuration")]
        [SerializeField]
        [Tooltip("Interaction range for objects")]
        [Range(1f, 10f)]
        public float interactionRange = 3f;
        
        [TabGroup("Interaction", "Interaction Settings")]
        [SerializeField]
        [Tooltip("Show interaction prompts")]
        public bool showInteractionPrompts = true;
        
        [TabGroup("Interaction", "Interaction Settings")]
        [SerializeField]
        [Tooltip("Highlight interactable objects")]
        public bool highlightInteractables = true;
        
        [TabGroup("Interaction", "Interaction Settings")]
        [SerializeField]
        [Tooltip("Interaction prompt fade time")]
        [Range(0.1f, 2f)]
        public float promptFadeTime = 0.3f;
        
        [TabGroup("Audio", "Audio Settings")]
        [Header("Audio Configuration")]
        [SerializeField]
        [Tooltip("Master audio volume for adventure template")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        
        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        [Tooltip("Background music volume")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        
        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        [Tooltip("Sound effects volume")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
        
        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        [Tooltip("Ambient audio volume")]
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        
        [TabGroup("UI", "UI Settings")]
        [Header("UI Configuration")]
        [SerializeField]
        [Tooltip("UI animation speed multiplier")]
        [Range(0.5f, 3f)]
        public float uiAnimationSpeed = 1f;
        
        [TabGroup("UI", "UI Settings")]
        [SerializeField]
        [Tooltip("Enable UI sound effects")]
        public bool enableUISounds = true;
        
        [TabGroup("UI", "UI Settings")]
        [SerializeField]
        [Tooltip("Show tutorial hints")]
        public bool showTutorialHints = true;
        
        [TabGroup("UI", "UI Settings")]
        [SerializeField]
        [Tooltip("UI fade duration")]
        [Range(0.1f, 2f)]
        public float uiFadeDuration = 0.5f;
        
        [TabGroup("Performance", "Performance Settings")]
        [Header("Performance Configuration")]
        [SerializeField]
        [Tooltip("Enable performance optimizations")]
        public bool enablePerformanceOptimizations = true;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Target frame rate")]
        [Range(30, 120)]
        public int targetFrameRate = 60;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Maximum concurrent dialogue instances")]
        [Range(1, 5)]
        public int maxConcurrentDialogues = 1;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Culling distance for interactions")]
        [Range(10f, 100f)]
        public float interactionCullingDistance = 50f;
        
        #region Validation
        
        private void OnValidate()
        {
            // Ensure sensible value ranges
            updateFrequency = Mathf.Max(0.1f, updateFrequency);
            defaultTextSpeed = Mathf.Max(10f, defaultTextSpeed);
            maxActiveQuests = Mathf.Max(1, maxActiveQuests);
            maxInventorySlots = Mathf.Max(10, maxInventorySlots);
            interactionRange = Mathf.Max(1f, interactionRange);
            targetFrameRate = Mathf.Max(30, targetFrameRate);
        }
        
        #endregion
        
        #region Debug
        
        [TabGroup("Debug", "Debug Tools")]
        [Button("Reset to Defaults", ButtonSizes.Medium)]
        private void ResetToDefaults()
        {
            autoActivateOnStart = true;
            enableDebugLogging = true;
            updateFrequency = 0.5f;
            defaultTextSpeed = 30f;
            autoAdvanceDelay = 0f;
            enableVoiceActing = false;
            dialogueVolume = 0.8f;
            maxActiveQuests = 10;
            showQuestNotifications = true;
            questNotificationDuration = 3f;
            enableQuestAutoTracking = true;
            maxInventorySlots = 50;
            enableItemStacking = true;
            showItemTooltips = true;
            enableInventorySorting = true;
            interactionRange = 3f;
            showInteractionPrompts = true;
            highlightInteractables = true;
            promptFadeTime = 0.3f;
            masterVolume = 1f;
            musicVolume = 0.7f;
            sfxVolume = 0.8f;
            ambientVolume = 0.5f;
            uiAnimationSpeed = 1f;
            enableUISounds = true;
            showTutorialHints = true;
            uiFadeDuration = 0.5f;
            enablePerformanceOptimizations = true;
            targetFrameRate = 60;
            maxConcurrentDialogues = 1;
            interactionCullingDistance = 50f;
            
            Debug.Log("[Adventure Settings] Reset to default values");
        }
        
        [TabGroup("Debug", "Debug Tools")]
        [Button("Log Current Settings", ButtonSizes.Medium)]
        private void LogCurrentSettings()
        {
            Debug.Log($"[Adventure Settings] Current Configuration:");
            Debug.Log($"  Auto Activate: {autoActivateOnStart}");
            Debug.Log($"  Text Speed: {defaultTextSpeed}");
            Debug.Log($"  Max Active Quests: {maxActiveQuests}");
            Debug.Log($"  Max Inventory Slots: {maxInventorySlots}");
            Debug.Log($"  Interaction Range: {interactionRange}");
            Debug.Log($"  Target FPS: {targetFrameRate}");
        }
        
        #endregion
    }
}