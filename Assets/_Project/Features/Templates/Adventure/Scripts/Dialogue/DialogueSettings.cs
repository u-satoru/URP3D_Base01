using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialogue
{
    /// <summary>
    /// Dialogue Settings ScriptableObject
    /// Configuration data for dialogue system behavior and presentation
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueSettings", menuName = "Templates/Adventure/Dialogue Settings")]
    public class DialogueSettings : ScriptableObject
    {
        [TabGroup("Text", "Text Display")]
        [Header("Text Configuration")]
        [SerializeField]
        [Tooltip("Text display speed (characters per second)")]
        [Range(10f, 100f)]
        public float textSpeed = 30f;
        
        [TabGroup("Text", "Text Display")]
        [SerializeField]
        [Tooltip("Enable typewriter text effect")]
        public bool enableTypewriterEffect = true;
        
        [TabGroup("Text", "Text Display")]
        [SerializeField]
        [Tooltip("Auto-advance dialogue after this time (0 = manual only)")]
        [Range(0f, 10f)]
        public float autoAdvanceDelay = 0f;
        
        [TabGroup("Text", "Text Display")]
        [SerializeField]
        [Tooltip("Skip typewriter effect on user input")]
        public bool allowSkipTypewriter = true;
        
        [TabGroup("Audio", "Audio Configuration")]
        [Header("Audio Settings")]
        [SerializeField]
        [Tooltip("Enable voice acting")]
        public bool enableVoiceActing = false;
        
        [TabGroup("Audio", "Audio Configuration")]
        [SerializeField]
        [Tooltip("Dialogue audio volume")]
        [Range(0f, 1f)]
        public float dialogueVolume = 0.8f;
        
        [TabGroup("Audio", "Audio Configuration")]
        [SerializeField]
        [Tooltip("Enable typewriter sound effects")]
        public bool enableTypewriterSounds = true;
        
        [TabGroup("Audio", "Audio Configuration")]
        [SerializeField]
        [Tooltip("Typewriter sound effect")]
        public AudioClip typewriterSound;
        
        [TabGroup("UI", "UI Configuration")]
        [Header("UI Settings")]
        [SerializeField]
        [Tooltip("Show speaker name")]
        public bool showSpeakerName = true;
        
        [TabGroup("UI", "UI Configuration")]
        [SerializeField]
        [Tooltip("Show dialogue portraits")]
        public bool showPortraits = true;
        
        [TabGroup("UI", "UI Configuration")]
        [SerializeField]
        [Tooltip("Dialogue box fade time")]
        [Range(0.1f, 2f)]
        public float dialogueBoxFadeTime = 0.3f;
        
        [TabGroup("UI", "UI Configuration")]
        [SerializeField]
        [Tooltip("Choice selection highlight color")]
        public Color choiceHighlightColor = Color.yellow;
        
        [TabGroup("Choices", "Choice Configuration")]
        [Header("Choice Settings")]
        [SerializeField]
        [Tooltip("Maximum choices to display")]
        [Range(2, 10)]
        public int maxChoicesDisplay = 6;
        
        [TabGroup("Choices", "Choice Configuration")]
        [SerializeField]
        [Tooltip("Choice selection timeout (0 = no timeout)")]
        [Range(0f, 30f)]
        public float choiceTimeout = 0f;
        
        [TabGroup("Choices", "Choice Configuration")]
        [SerializeField]
        [Tooltip("Default choice index on timeout")]
        public int defaultChoiceOnTimeout = 0;
        
        [TabGroup("Choices", "Choice Configuration")]
        [SerializeField]
        [Tooltip("Show choice numbers")]
        public bool showChoiceNumbers = true;
        
        [TabGroup("Performance", "Performance Settings")]
        [Header("Performance Configuration")]
        [SerializeField]
        [Tooltip("Maximum concurrent dialogues")]
        [Range(1, 5)]
        public int maxConcurrentDialogues = 1;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Preload dialogue audio")]
        public bool preloadDialogueAudio = false;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Cache dialogue data")]
        public bool cacheDialogueData = true;
        
        [TabGroup("Performance", "Performance Settings")]
        [SerializeField]
        [Tooltip("Dialogue cache size (number of dialogues)")]
        [Range(10, 100)]
        public int dialogueCacheSize = 50;
        
        #region Validation
        
        private void OnValidate()
        {
            // Ensure sensible value ranges
            textSpeed = Mathf.Max(10f, textSpeed);
            autoAdvanceDelay = Mathf.Max(0f, autoAdvanceDelay);
            dialogueVolume = Mathf.Clamp01(dialogueVolume);
            dialogueBoxFadeTime = Mathf.Max(0.1f, dialogueBoxFadeTime);
            maxChoicesDisplay = Mathf.Max(2, maxChoicesDisplay);
            choiceTimeout = Mathf.Max(0f, choiceTimeout);
            defaultChoiceOnTimeout = Mathf.Max(0, defaultChoiceOnTimeout);
            maxConcurrentDialogues = Mathf.Max(1, maxConcurrentDialogues);
            dialogueCacheSize = Mathf.Max(10, dialogueCacheSize);
        }
        
        #endregion
        
        #region Debug
        
        [TabGroup("Debug", "Debug Tools")]
        [Button("Reset to Defaults", ButtonSizes.Medium)]
        private void ResetToDefaults()
        {
            textSpeed = 30f;
            enableTypewriterEffect = true;
            autoAdvanceDelay = 0f;
            allowSkipTypewriter = true;
            enableVoiceActing = false;
            dialogueVolume = 0.8f;
            enableTypewriterSounds = true;
            showSpeakerName = true;
            showPortraits = true;
            dialogueBoxFadeTime = 0.3f;
            choiceHighlightColor = Color.yellow;
            maxChoicesDisplay = 6;
            choiceTimeout = 0f;
            defaultChoiceOnTimeout = 0;
            showChoiceNumbers = true;
            maxConcurrentDialogues = 1;
            preloadDialogueAudio = false;
            cacheDialogueData = true;
            dialogueCacheSize = 50;
            
            Debug.Log("[Dialogue Settings] Reset to default values");
        }
        
        [TabGroup("Debug", "Debug Tools")]
        [Button("Log Current Settings", ButtonSizes.Medium)]
        private void LogCurrentSettings()
        {
            Debug.Log($"[Dialogue Settings] Current Configuration:");
            Debug.Log($"  Text Speed: {textSpeed}");
            Debug.Log($"  Typewriter Effect: {enableTypewriterEffect}");
            Debug.Log($"  Auto Advance: {autoAdvanceDelay}s");
            Debug.Log($"  Voice Acting: {enableVoiceActing}");
            Debug.Log($"  Max Choices: {maxChoicesDisplay}");
            Debug.Log($"  Cache Size: {dialogueCacheSize}");
        }
        
        #endregion
    }
}