using System;
using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Features.Templates.Adventure.Dialogue;
using asterivo.Unity60.Features.Templates.Adventure.Quest;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using asterivo.Unity60.Features.Templates.Adventure.Interaction;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure
{
    /// <summary>
    /// Adventure Template Manager
    /// Coordinates dialogue, quest, inventory, and interaction systems for adventure games
    /// Provides integrated template management for narrative-driven gameplay experiences
    /// </summary>
    [System.Serializable]
    public class AdventureTemplateManager : MonoBehaviour
    {
        [TabGroup("Adventure", "Core Systems")]
        [Header("Template Settings")]
        [SerializeField] private AdventureSettings adventureSettings;
        
        [TabGroup("Adventure", "Core Systems")]
        [Header("System Components")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private AdventureInventoryManager inventoryManager;
        [SerializeField] private InteractionManager interactionManager;
        
        [TabGroup("Adventure", "Events")]
        [Header("Template Events")]
        [SerializeField] private GameEvent onAdventureTemplateInitialized;
        [SerializeField] private GameEvent onAdventureTemplateActivated;
        [SerializeField] private GameEvent onAdventureTemplateDeactivated;
        [SerializeField] private StringGameEvent onStoryProgressChanged;
        
        [TabGroup("Adventure", "State")]
        [ShowInInspector, ReadOnly]
        private bool isInitialized = false;
        
        [TabGroup("Adventure", "State")]
        [ShowInInspector, ReadOnly]
        private bool isActive = false;
        
        [TabGroup("Adventure", "State")]
        [ShowInInspector, ReadOnly]
        private string currentStoryPhase = "Introduction";
        
        // Events
        public event Action OnTemplateInitialized;
        public event Action OnTemplateActivated;
        public event Action OnTemplateDeactivated;
        public event Action<string> OnStoryProgressChanged;
        
        // Properties
        public bool IsInitialized => isInitialized;
        public bool IsActive => isActive;
        public string CurrentStoryPhase => currentStoryPhase;
        public DialogueManager DialogueManager => dialogueManager;
        public QuestManager QuestManager => questManager;
        public AdventureInventoryManager InventoryManager => inventoryManager;
        public InteractionManager InteractionManager => interactionManager;
        
        private void Awake()
        {
            ValidateComponents();
        }
        
        private void Start()
        {
            StartCoroutine(InitializeTemplate());
        }
        
        private void OnDestroy()
        {
            DeactivateTemplate();
            UnregisterFromServices();
        }
        
        #region Initialization
        
        /// <summary>
        /// Initialize the Adventure Template system
        /// </summary>
        private IEnumerator InitializeTemplate()
        {
            if (isInitialized)
                yield break;
                
            Debug.Log("[Adventure Template] Starting initialization...");
            
            // Initialize core systems in proper order
            yield return StartCoroutine(InitializeDialogueSystem());
            yield return StartCoroutine(InitializeQuestSystem());
            yield return StartCoroutine(InitializeInventorySystem());
            yield return StartCoroutine(InitializeInteractionSystem());
            
            // Register with service locator
            RegisterWithServices();
            
            // Connect system events
            ConnectSystemEvents();
            
            isInitialized = true;
            
            // Notify initialization complete
            OnTemplateInitialized?.Invoke();
            onAdventureTemplateInitialized?.Raise();
            
            Debug.Log("[Adventure Template] Initialization complete!");
            
            // Auto-activate if configured
            if (adventureSettings != null && adventureSettings.autoActivateOnStart)
            {
                ActivateTemplate();
            }
        }
        
        private IEnumerator InitializeDialogueSystem()
        {
            if (dialogueManager != null)
            {
                Debug.Log("[Adventure Template] Initializing dialogue system...");
                yield return dialogueManager.InitializeAsync();
            }
            yield return null;
        }
        
        private IEnumerator InitializeQuestSystem()
        {
            if (questManager != null)
            {
                Debug.Log("[Adventure Template] Quest system initializing automatically...");
                // QuestManager initializes automatically in its Start() method
            }
            yield return null;
        }

        private IEnumerator InitializeInventorySystem()
        {
            if (inventoryManager != null)
            {
                Debug.Log("[Adventure Template] Inventory system initializing automatically...");
                // AdventureInventoryManager initializes automatically in its Start() method
            }
            yield return null;
        }

        private IEnumerator InitializeInteractionSystem()
        {
            if (interactionManager != null)
            {
                Debug.Log("[Adventure Template] Initializing interaction system...");
                // InteractionManager initializes automatically via Start() method
                Debug.Log("[Adventure Template] InteractionManager will initialize automatically");
            }
            yield return null;
        }
        
        #endregion
        
        #region Template Lifecycle
        
        /// <summary>
        /// Activate the Adventure Template
        /// </summary>
        public void ActivateTemplate()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[Adventure Template] Cannot activate - template not initialized!");
                return;
            }
            
            if (isActive)
                return;
                
            Debug.Log("[Adventure Template] Activating template...");
            
            // Systems activate automatically via their own lifecycle methods
            Debug.Log("[Adventure Template] All systems are initializing automatically...");
            
            isActive = true;
            
            OnTemplateActivated?.Invoke();
            onAdventureTemplateActivated?.Raise();
            
            Debug.Log("[Adventure Template] Template activated!");
        }
        
        /// <summary>
        /// Deactivate the Adventure Template
        /// </summary>
        public void DeactivateTemplate()
        {
            if (!isActive)
                return;
                
            Debug.Log("[Adventure Template] Deactivating template...");
            
            // Systems deactivate automatically via their own lifecycle methods
            Debug.Log("[Adventure Template] All systems are deactivating automatically...");
            
            isActive = false;
            
            OnTemplateDeactivated?.Invoke();
            onAdventureTemplateDeactivated?.Raise();
            
            Debug.Log("[Adventure Template] Template deactivated!");
        }
        
        #endregion
        
        #region Story Progress Management
        
        /// <summary>
        /// Update story progress and notify systems
        /// </summary>
        public void UpdateStoryProgress(string newPhase)
        {
            if (string.IsNullOrEmpty(newPhase))
                return;
                
            string previousPhase = currentStoryPhase;
            currentStoryPhase = newPhase;
            
            Debug.Log($"[Adventure Template] Story progress: {previousPhase} -> {currentStoryPhase}");
            
            // Notify systems about story progress
            OnStoryProgressChanged?.Invoke(currentStoryPhase);
            onStoryProgressChanged?.Raise(currentStoryPhase);
            
            // Update quest system with new story phase
            // TODO: Convert string story phase to int or create different method
            // questManager?.TriggerStoryPhaseChanged(GetStoryPhaseIndex(currentStoryPhase));
            
            // Update dialogue system with new story context
            dialogueManager?.OnStoryPhaseChanged(currentStoryPhase);
        }
        
        /// <summary>
        /// Get current story progress percentage (0-100)
        /// </summary>
        public float GetStoryProgressPercentage()
        {
            if (questManager != null)
            {
                return questManager.GetOverallProgressPercentage();
            }
            return 0f;
        }
        
        #endregion
        
        #region System Integration
        
        private void ConnectSystemEvents()
        {
            // Connect dialogue to quest system
            if (dialogueManager != null && questManager != null)
            {
                dialogueManager.OnDialogueCompleted += questManager.TriggerDialogueCompleted;
                dialogueManager.OnChoiceMade += questManager.TriggerPlayerChoiceMade;
            }
            
            // Connect quest to inventory system
            if (questManager != null && inventoryManager != null)
            {
                questManager.OnItemRequired += (itemId) => inventoryManager.CheckItemAvailability(itemId);
                questManager.OnItemReward += (itemId) => inventoryManager.AddQuestReward(itemId);
                inventoryManager.OnItemUsed += questManager.TriggerItemUsed;
            }
            
            // Connect interaction to dialogue system
            if (interactionManager != null && dialogueManager != null)
            {
                interactionManager.OnNPCInteraction += (npc) => {
                    if (npc is MonoBehaviour npcComponent)
                        dialogueManager.StartDialogue(npcComponent.gameObject.name);
                };
                interactionManager.OnObjectInteraction += (obj) => {
                    if (obj is InteractableObject interactableObj)
                        HandleObjectInteraction(interactableObj);
                };
            }
            
            Debug.Log("[Adventure Template] System events connected successfully!");
        }
        
        private void HandleObjectInteraction(InteractableObject obj)
        {
            if (obj == null) return;
            
            // Handle different types of object interactions
            switch (obj.InteractionType)
            {
                case InteractionType.Examine:
                    dialogueManager?.ShowExaminationText(obj.ExaminationText);
                    break;
                case InteractionType.PickupItem:
                    if (obj.ItemToPickup != null)
                    {
                        inventoryManager?.AddItem(obj.ItemToPickup);
                    }
                    break;
                case InteractionType.TriggerQuest:
                    if (!string.IsNullOrEmpty(obj.QuestID))
                    {
                        questManager?.TriggerQuest(obj.QuestID);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Service Registration
        
        private void RegisterWithServices()
        {
            try
            {
                asterivo.Unity60.Core.ServiceLocator.GetService<AdventureTemplateManager>();
                // Already registered
            }
            catch
            {
                asterivo.Unity60.Core.ServiceLocator.RegisterService<AdventureTemplateManager>(this);
                Debug.Log("[Adventure Template] Registered with ServiceLocator");
            }
        }
        
        private void UnregisterFromServices()
        {
            try
            {
                asterivo.Unity60.Core.ServiceLocator.UnregisterService<AdventureTemplateManager>();
                Debug.Log("[Adventure Template] Unregistered from ServiceLocator");
            }
            catch
            {
                // ServiceLocator not available or service not registered
            }
        }
        
        #endregion
        
        #region Validation
        
        private void ValidateComponents()
        {
            if (dialogueManager == null)
            {
                Debug.LogWarning("[Adventure Template] DialogueManager not assigned!");
            }
            
            if (questManager == null)
            {
                Debug.LogWarning("[Adventure Template] QuestManager not assigned!");
            }
            
            if (inventoryManager == null)
            {
                Debug.LogWarning("[Adventure Template] AdventureInventoryManager not assigned!");
            }
            
            if (interactionManager == null)
            {
                Debug.LogWarning("[Adventure Template] InteractionManager not assigned!");
            }
            
            if (adventureSettings == null)
            {
                Debug.LogWarning("[Adventure Template] AdventureSettings not assigned!");
            }
        }
        
        #endregion
        
        #region Debug Commands
        
        [TabGroup("Adventure", "Debug")]
        [Button("Initialize Template", ButtonSizes.Medium)]
        [EnableIf("@!isInitialized")]
        private void DebugInitialize()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(InitializeTemplate());
            }
        }
        
        [TabGroup("Adventure", "Debug")]
        [Button("Activate Template", ButtonSizes.Medium)]
        [EnableIf("@isInitialized && !isActive")]
        private void DebugActivate()
        {
            ActivateTemplate();
        }
        
        [TabGroup("Adventure", "Debug")]
        [Button("Deactivate Template", ButtonSizes.Medium)]
        [EnableIf("@isActive")]
        private void DebugDeactivate()
        {
            DeactivateTemplate();
        }
        
        [TabGroup("Adventure", "Debug")]
        [Button("Show System Status", ButtonSizes.Medium)]
        private void DebugShowStatus()
        {
            Debug.Log($"[Adventure Template] Status Report:");
            Debug.Log($"  - Initialized: {isInitialized}");
            Debug.Log($"  - Active: {isActive}");
            Debug.Log($"  - Story Phase: {currentStoryPhase}");
            Debug.Log($"  - Story Progress: {GetStoryProgressPercentage():F1}%");
            Debug.Log($"  - Dialogue System: {(dialogueManager != null ? "OK" : "Missing")}");
            Debug.Log($"  - Quest System: {(questManager != null ? "OK" : "Missing")}");
            Debug.Log($"  - Inventory System: {(inventoryManager != null ? "OK" : "Missing")}");
            Debug.Log($"  - Interaction System: {(interactionManager != null ? "OK" : "Missing")}");
        }
        
        #endregion
    }
}