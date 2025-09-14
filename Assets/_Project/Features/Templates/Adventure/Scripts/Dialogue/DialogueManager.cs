using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialogue
{
    /// <summary>
    /// Dialogue Manager for Adventure Template
    /// Handles NPC conversations, dialogue trees, and narrative interactions
    /// Supports branching dialogue, choice systems, and story integration
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [TabGroup("Dialogue", "Configuration")]
        [Header("Dialogue Settings")]
        [SerializeField] private DialogueSettings dialogueSettings;
        
        [TabGroup("Dialogue", "Configuration")]
        [SerializeField] private float textDisplaySpeed = 30f;
        
        [TabGroup("Dialogue", "Configuration")]
        [SerializeField] private bool enableTypewriterEffect = true;
        
        [TabGroup("Dialogue", "Events")]
        [Header("Dialogue Events")]
        [SerializeField] private GameEvent onDialogueStarted;
        [SerializeField] private GameEvent onDialogueEnded;
        [SerializeField] private StringGameEvent onDialogueLineDisplayed;
        [SerializeField] private IntGameEvent onChoiceSelected;
        
        [TabGroup("Dialogue", "State")]
        [ShowInInspector, ReadOnly]
        private bool isInitialized = false;
        
        [TabGroup("Dialogue", "State")]
        [ShowInInspector, ReadOnly]
        private bool isActive = false;
        
        [TabGroup("Dialogue", "State")]
        [ShowInInspector, ReadOnly]
        private bool isDialogueActive = false;
        
        [TabGroup("Dialogue", "State")]
        [ShowInInspector, ReadOnly]
        private DialogueData currentDialogue;
        
        [TabGroup("Dialogue", "State")]
        [ShowInInspector, ReadOnly]
        private int currentLineIndex = 0;
        
        // Events
        public event Action<DialogueData> OnDialogueCompleted;
        public event Action<int, string> OnChoiceMade;
        public event Action<string> OnDialogueLineChanged;
        
        // Properties
        public bool IsInitialized => isInitialized;
        public bool IsActive => isActive;
        public bool IsDialogueActive => isDialogueActive;
        public DialogueData CurrentDialogue => currentDialogue;
        
        private Queue<DialogueData> dialogueQueue = new Queue<DialogueData>();
        private Coroutine currentDialogueCoroutine;
        
        #region Initialization
        
        /// <summary>
        /// Initialize the dialogue system asynchronously
        /// </summary>
        public IEnumerator InitializeAsync()
        {
            if (isInitialized)
                yield break;
                
            Debug.Log("[Dialogue Manager] Initializing dialogue system...");
            
            // Load dialogue settings if not assigned
            if (dialogueSettings == null)
            {
                dialogueSettings = Resources.Load<DialogueSettings>("DefaultDialogueSettings");
            }
            
            // Initialize dialogue queue
            dialogueQueue = new Queue<DialogueData>();
            
            // Apply settings
            ApplySettings();
            
            isInitialized = true;
            Debug.Log("[Dialogue Manager] Dialogue system initialized successfully!");
            
            yield return null;
        }
        
        private void ApplySettings()
        {
            if (dialogueSettings != null)
            {
                textDisplaySpeed = dialogueSettings.textSpeed;
                enableTypewriterEffect = dialogueSettings.enableTypewriterEffect;
            }
        }
        
        #endregion
        
        #region System Lifecycle
        
        public void ActivateSystem()
        {
            if (!isInitialized || isActive)
                return;
                
            isActive = true;
            Debug.Log("[Dialogue Manager] System activated");
        }
        
        public void DeactivateSystem()
        {
            if (!isActive)
                return;
                
            // Stop any active dialogue
            StopDialogue();
            
            isActive = false;
            Debug.Log("[Dialogue Manager] System deactivated");
        }
        
        #endregion
        
        #region Dialogue Control
        
        /// <summary>
        /// Start a dialogue sequence
        /// </summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (!isActive || dialogue == null)
                return;
                
            if (isDialogueActive)
            {
                // Queue dialogue if one is already active
                dialogueQueue.Enqueue(dialogue);
                return;
            }
            
            currentDialogue = dialogue;
            currentLineIndex = 0;
            isDialogueActive = true;
            
            // Notify dialogue started
            onDialogueStarted?.Raise();
            OnDialogueLineChanged?.Invoke(dialogue.dialogueLines[0].text);
            
            // Start dialogue coroutine
            currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence());
            
            Debug.Log($"[Dialogue Manager] Started dialogue: {dialogue.dialogueID}");
        }
        
        /// <summary>
        /// Start dialogue by NPC name (convenience method)
        /// </summary>
        public void StartDialogue(string npcName)
        {
            // This would typically load dialogue data from resources or database
            // For now, create a simple placeholder
            var dialogue = CreatePlaceholderDialogue(npcName);
            StartDialogue(dialogue);
        }
        
        /// <summary>
        /// Stop current dialogue
        /// </summary>
        public void StopDialogue()
        {
            if (!isDialogueActive)
                return;
                
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
                currentDialogueCoroutine = null;
            }
            
            // Notify dialogue completed
            OnDialogueCompleted?.Invoke(currentDialogue);
            onDialogueEnded?.Raise();
            
            currentDialogue = null;
            currentLineIndex = 0;
            isDialogueActive = false;
            
            // Process next dialogue in queue
            ProcessDialogueQueue();
            
            Debug.Log("[Dialogue Manager] Dialogue stopped");
        }
        
        /// <summary>
        /// Advance to next dialogue line
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!isDialogueActive || currentDialogue == null)
                return;
                
            currentLineIndex++;
            
            if (currentLineIndex >= currentDialogue.dialogueLines.Length)
            {
                // End of dialogue
                StopDialogue();
            }
            else
            {
                // Display next line
                var currentLine = currentDialogue.dialogueLines[currentLineIndex];
                OnDialogueLineChanged?.Invoke(currentLine.text);
                onDialogueLineDisplayed?.Raise(currentLine.text);
            }
        }
        
        /// <summary>
        /// Make a choice in branching dialogue
        /// </summary>
        public void MakeChoice(int choiceIndex)
        {
            if (!isDialogueActive || currentDialogue == null)
                return;
                
            var currentLine = currentDialogue.dialogueLines[currentLineIndex];
            
            if (currentLine.choices != null && choiceIndex < currentLine.choices.Length)
            {
                var choice = currentLine.choices[choiceIndex];
                
                // Notify choice made
                OnChoiceMade?.Invoke(choiceIndex, choice.choiceText);
                onChoiceSelected?.Raise(choiceIndex);
                
                // Handle choice consequences
                HandleChoiceConsequences(choice);
                
                Debug.Log($"[Dialogue Manager] Choice made: {choiceIndex} - {choice.choiceText}");
            }
        }
        
        #endregion
        
        #region Dialogue Processing
        
        private IEnumerator PlayDialogueSequence()
        {
            while (isDialogueActive && currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Length)
            {
                var currentLine = currentDialogue.dialogueLines[currentLineIndex];
                
                // Display dialogue line with typewriter effect if enabled
                if (enableTypewriterEffect)
                {
                    yield return StartCoroutine(DisplayTextWithTypewriter(currentLine.text));
                }
                else
                {
                    OnDialogueLineChanged?.Invoke(currentLine.text);
                    onDialogueLineDisplayed?.Raise(currentLine.text);
                }
                
                // Handle choices
                if (currentLine.choices != null && currentLine.choices.Length > 0)
                {
                    // Wait for player choice
                    yield return new WaitUntil(() => !isDialogueActive || currentLineIndex != Array.IndexOf(currentDialogue.dialogueLines, currentLine));
                }
                else
                {
                    // Auto-advance or wait for input based on settings
                    if (dialogueSettings != null && dialogueSettings.autoAdvanceDelay > 0)
                    {
                        yield return new WaitForSeconds(dialogueSettings.autoAdvanceDelay);
                        AdvanceDialogue();
                    }
                    else
                    {
                        // Wait for manual advance
                        yield return new WaitUntil(() => !isDialogueActive || currentLineIndex != Array.IndexOf(currentDialogue.dialogueLines, currentLine));
                    }
                }
            }
            
            if (isDialogueActive)
            {
                StopDialogue();
            }
        }
        
        private IEnumerator DisplayTextWithTypewriter(string text)
        {
            string displayText = "";
            
            for (int i = 0; i < text.Length; i++)
            {
                displayText += text[i];
                OnDialogueLineChanged?.Invoke(displayText);
                
                yield return new WaitForSeconds(1f / textDisplaySpeed);
            }
            
            onDialogueLineDisplayed?.Raise(text);
        }
        
        private void ProcessDialogueQueue()
        {
            if (dialogueQueue.Count > 0)
            {
                var nextDialogue = dialogueQueue.Dequeue();
                StartDialogue(nextDialogue);
            }
        }
        
        private void HandleChoiceConsequences(DialogueChoice choice)
        {
            // Handle quest triggers
            if (!string.IsNullOrEmpty(choice.questID))
            {
                // This would trigger quest system integration
                Debug.Log($"[Dialogue Manager] Choice triggered quest: {choice.questID}");
            }
            
            // Handle story phase changes
            if (!string.IsNullOrEmpty(choice.storyPhase))
            {
                var adventureManager = ServiceLocator.Instance?.GetService<AdventureTemplateManager>();
                adventureManager?.UpdateStoryProgress(choice.storyPhase);
            }
            
            // Continue to next dialogue line or end
            if (choice.nextLineIndex >= 0)
            {
                currentLineIndex = choice.nextLineIndex;
            }
            else
            {
                StopDialogue();
            }
        }
        
        #endregion
        
        #region Story Integration
        
        /// <summary>
        /// Called when story phase changes
        /// </summary>
        public void OnStoryPhaseChanged(string newPhase)
        {
            Debug.Log($"[Dialogue Manager] Story phase changed to: {newPhase}");
            // Update available dialogues based on story phase
        }
        
        /// <summary>
        /// Show examination text for objects
        /// </summary>
        public void ShowExaminationText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
                
            var examDialogue = CreateExaminationDialogue(text);
            StartDialogue(examDialogue);
        }
        
        #endregion
        
        #region Helper Methods
        
        private DialogueData CreatePlaceholderDialogue(string npcName)
        {
            return new DialogueData
            {
                dialogueID = $"placeholder_{npcName}",
                speakerName = npcName,
                dialogueLines = new DialogueLine[]
                {
                    new DialogueLine
                    {
                        text = $"Hello! I'm {npcName}. This is a placeholder dialogue.",
                        speakerName = npcName,
                        choices = new DialogueChoice[]
                        {
                            new DialogueChoice { choiceText = "Nice to meet you!", nextLineIndex = 1 },
                            new DialogueChoice { choiceText = "Goodbye.", nextLineIndex = -1 }
                        }
                    },
                    new DialogueLine
                    {
                        text = "Great to meet you too! Have a wonderful adventure!",
                        speakerName = npcName,
                        choices = null
                    }
                }
            };
        }
        
        private DialogueData CreateExaminationDialogue(string text)
        {
            return new DialogueData
            {
                dialogueID = "examination",
                speakerName = "Narrator",
                dialogueLines = new DialogueLine[]
                {
                    new DialogueLine
                    {
                        text = text,
                        speakerName = "Narrator",
                        choices = null
                    }
                }
            };
        }
        
        #endregion
        
        #region Debug Commands
        
        [TabGroup("Dialogue", "Debug")]
        [Button("Test Dialogue", ButtonSizes.Medium)]
        private void TestDialogue()
        {
            if (Application.isPlaying && isActive)
            {
                StartDialogue("Test NPC");
            }
        }
        
        [TabGroup("Dialogue", "Debug")]
        [Button("Stop Current Dialogue", ButtonSizes.Medium)]
        [EnableIf("@isDialogueActive")]
        private void DebugStopDialogue()
        {
            StopDialogue();
        }
        
        [TabGroup("Dialogue", "Debug")]
        [Button("Show System Status", ButtonSizes.Medium)]
        private void DebugShowStatus()
        {
            Debug.Log($"[Dialogue Manager] System Status:");
            Debug.Log($"  - Initialized: {isInitialized}");
            Debug.Log($"  - Active: {isActive}");
            Debug.Log($"  - Dialogue Active: {isDialogueActive}");
            Debug.Log($"  - Queue Count: {dialogueQueue.Count}");
            Debug.Log($"  - Current Line: {currentLineIndex}");
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class DialogueData
    {
        public string dialogueID;
        public string speakerName;
        public DialogueLine[] dialogueLines;
    }
    
    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public string speakerName;
        public float displayDuration;
        public AudioClip voiceClip;
        public DialogueChoice[] choices;
    }
    
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public int nextLineIndex = -1;
        public string questID;
        public string storyPhase;
        public bool requiresItem;
        public string requiredItemID;
    }
    
    #endregion
}