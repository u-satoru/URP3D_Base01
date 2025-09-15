using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using GameEventTwoArgs = asterivo.Unity60.Core.Events.GameEvent<asterivo.Unity60.Features.Templates.Adventure.Quest.QuestData, asterivo.Unity60.Features.Templates.Adventure.Quest.QuestObjective>;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Dialogue;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Quest
{
    /// <summary>
    /// Adventure Template Quest Management System
    /// Handles quest assignment, progress tracking, completion, and rewards
    /// Integrates with dialogue system and story progression
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [TabGroup("Quest", "Configuration")]
        [Header("Quest Settings")]
        [SerializeField] private QuestSettings questSettings;
        
        [TabGroup("Quest", "Configuration")]
        [Header("Event Channels")]
        [SerializeField] private QuestDataGameEvent onQuestStarted;
        [SerializeField] private QuestDataGameEvent onQuestCompleted;
        [SerializeField] private GameEventTwoArgs onObjectiveCompleted;
        [SerializeField] private QuestDataGameEvent onQuestFailed;
        [SerializeField] private GameEvent onQuestsUpdated;
        
        [TabGroup("Quest", "Runtime State")]
        [Header("Active Quests")]
        [SerializeField, ReadOnly] 
        private List<QuestInstance> activeQuests = new List<QuestInstance>();
        
        [TabGroup("Quest", "Runtime State")]
        [SerializeField, ReadOnly] 
        private List<QuestData> completedQuests = new List<QuestData>();
        
        [TabGroup("Quest", "Runtime State")]
        [SerializeField, ReadOnly] 
        private List<QuestData> failedQuests = new List<QuestData>();
        
        [TabGroup("Quest", "Debug")]
        [Header("Debug Information")]
        [SerializeField, ReadOnly] private int totalActiveQuests;
        [SerializeField, ReadOnly] private int totalCompletedQuests;
        [SerializeField, ReadOnly] private float questCompletionRate;
        
        // Events
        public event Action<QuestData> OnQuestStarted;
        public event Action<QuestData> OnQuestCompleted;
        public event Action<QuestData, QuestObjective> OnObjectiveCompleted;
        public event Action<QuestData> OnQuestFailed;
        public event Action OnQuestsUpdated;
        
        // Properties
        public IReadOnlyList<QuestInstance> ActiveQuests => activeQuests.AsReadOnly();
        public IReadOnlyList<QuestData> CompletedQuests => completedQuests.AsReadOnly();
        public IReadOnlyList<QuestData> FailedQuests => failedQuests.AsReadOnly();
        
        public bool IsActive { get; private set; }
        
        private void Awake()
        {
            ValidateConfiguration();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            if (IsActive && questSettings != null)
            {
                UpdateQuests();
                UpdateDebugInfo();
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        #region Initialization
        
        public void Initialize()
        {
            if (questSettings == null)
            {
                Debug.LogError("[QuestManager] QuestSettings is null. Cannot initialize quest system.");
                return;
            }
            
            IsActive = true;
            
            // Register with Service Locator
            asterivo.Unity60.Core.ServiceLocator.RegisterService<QuestManager>(this);
            
            Debug.Log("[QuestManager] Quest system initialized successfully.");
        }
        
        private void ValidateConfiguration()
        {
            if (questSettings == null)
            {
                Debug.LogWarning("[QuestManager] QuestSettings not assigned. Quest system functionality will be limited.");
            }
        }
        
        private void Cleanup()
        {
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<QuestManager>();
            IsActive = false;
        }
        
        #endregion
        
        #region Quest Management
        
        /// <summary>
        /// Starts a new quest if conditions are met
        /// </summary>
        public bool StartQuest(QuestData questData)
        {
            if (!IsActive || questData == null)
                return false;
            
            // Check if quest is already active or completed
            if (IsQuestActive(questData) || IsQuestCompleted(questData))
            {
                Debug.LogWarning($"[QuestManager] Quest '{questData.questName}' is already active or completed.");
                return false;
            }
            
            // Check prerequisites
            if (!CheckQuestPrerequisites(questData))
            {
                Debug.LogWarning($"[QuestManager] Prerequisites not met for quest '{questData.questName}'.");
                return false;
            }
            
            // Create quest instance
            var questInstance = new QuestInstance(questData);
            activeQuests.Add(questInstance);
            
            // Trigger events
            OnQuestStarted?.Invoke(questData);
            onQuestStarted?.Raise(questData);
            OnQuestsUpdated?.Invoke();
            onQuestsUpdated?.Raise();
            
            Debug.Log($"[QuestManager] Started quest: {questData.questName}");
            return true;
        }
        
        /// <summary>
        /// Updates objective progress for a specific quest
        /// </summary>
        public bool UpdateObjectiveProgress(QuestData questData, string objectiveId, int progress = 1)
        {
            if (!IsActive || questData == null || string.IsNullOrEmpty(objectiveId))
                return false;
            
            var questInstance = GetActiveQuestInstance(questData);
            if (questInstance == null)
                return false;
            
            var objective = questInstance.GetObjective(objectiveId);
            if (objective == null || objective.IsCompleted)
                return false;
            
            // Update progress
            objective.currentProgress += progress;
            objective.currentProgress = Mathf.Clamp(objective.currentProgress, 0, objective.targetAmount);
            
            // Check if objective is completed
            if (objective.currentProgress >= objective.targetAmount && !objective.IsCompleted)
            {
                objective.IsCompleted = true;
                objective.completionTime = DateTime.Now;
                
                // Trigger objective completion events
                OnObjectiveCompleted?.Invoke(questData, objective);
                onObjectiveCompleted?.Raise(questData, objective);
                
                Debug.Log($"[QuestManager] Objective completed: {objective.description} in quest {questData.questName}");
                
                // Check if all objectives are completed
                if (AreAllObjectivesCompleted(questInstance))
                {
                    CompleteQuest(questData);
                }
            }
            
            OnQuestsUpdated?.Invoke();
            onQuestsUpdated?.Raise();
            
            return true;
        }
        
        /// <summary>
        /// Completes a quest and handles rewards
        /// </summary>
        public bool CompleteQuest(QuestData questData)
        {
            if (!IsActive || questData == null)
                return false;
            
            var questInstance = GetActiveQuestInstance(questData);
            if (questInstance == null)
                return false;
            
            // Remove from active quests
            activeQuests.Remove(questInstance);
            completedQuests.Add(questData);
            
            // Handle rewards if specified
            if (questSettings.EnableRewards && questData.rewards != null && questData.rewards.Count > 0)
            {
                HandleQuestRewards(questData);
            }
            
            // Trigger events
            OnQuestCompleted?.Invoke(questData);
            onQuestCompleted?.Raise(questData);
            OnQuestsUpdated?.Invoke();
            onQuestsUpdated?.Raise();
            
            Debug.Log($"[QuestManager] Completed quest: {questData.questName}");
            return true;
        }
        
        /// <summary>
        /// Fails a quest and moves it to failed list
        /// </summary>
        public bool FailQuest(QuestData questData)
        {
            if (!IsActive || questData == null)
                return false;
            
            var questInstance = GetActiveQuestInstance(questData);
            if (questInstance == null)
                return false;
            
            // Remove from active quests
            activeQuests.Remove(questInstance);
            failedQuests.Add(questData);
            
            // Trigger events
            OnQuestFailed?.Invoke(questData);
            onQuestFailed?.Raise(questData);
            OnQuestsUpdated?.Invoke();
            onQuestsUpdated?.Raise();
            
            Debug.Log($"[QuestManager] Failed quest: {questData.questName}");
            return true;
        }
        
        #endregion
        
        #region Quest Queries
        
        /// <summary>
        /// Checks if a quest is currently active
        /// </summary>
        public bool IsQuestActive(QuestData questData)
        {
            return questData != null && activeQuests.Any(q => q.questData == questData);
        }
        
        /// <summary>
        /// Checks if a quest has been completed
        /// </summary>
        public bool IsQuestCompleted(QuestData questData)
        {
            return questData != null && completedQuests.Contains(questData);
        }
        
        /// <summary>
        /// Checks if a quest has failed
        /// </summary>
        public bool IsQuestFailed(QuestData questData)
        {
            return questData != null && failedQuests.Contains(questData);
        }
        
        /// <summary>
        /// Gets the progress of a specific objective
        /// </summary>
        public QuestObjective GetObjectiveProgress(QuestData questData, string objectiveId)
        {
            var questInstance = GetActiveQuestInstance(questData);
            return questInstance?.GetObjective(objectiveId);
        }
        
        /// <summary>
        /// Gets the overall progress of a quest (0.0 to 1.0)
        /// </summary>
        public float GetQuestProgress(QuestData questData)
        {
            var questInstance = GetActiveQuestInstance(questData);
            if (questInstance == null)
                return 0f;
            
            if (questInstance.questData.objectives.Count == 0)
                return 0f;
            
            int completedObjectives = questInstance.questData.objectives.Count(obj => obj.IsCompleted);
            return (float)completedObjectives / questInstance.questData.objectives.Count;
        }
        
        #endregion
        
        #region Private Methods
        
        private void UpdateQuests()
        {
            // Update quest timers if enabled
            if (questSettings.EnableQuestTimers)
            {
                for (int i = activeQuests.Count - 1; i >= 0; i--)
                {
                    var quest = activeQuests[i];
                    if (quest.questData.hasTimeLimit)
                    {
                        quest.elapsedTime += Time.deltaTime;
                        if (quest.elapsedTime >= quest.questData.timeLimit)
                        {
                            FailQuest(quest.questData);
                        }
                    }
                }
            }
        }
        
        private void UpdateDebugInfo()
        {
            totalActiveQuests = activeQuests.Count;
            totalCompletedQuests = completedQuests.Count;
            
            int totalQuests = totalActiveQuests + totalCompletedQuests + failedQuests.Count;
            questCompletionRate = totalQuests > 0 ? (float)totalCompletedQuests / totalQuests : 0f;
        }
        
        private QuestInstance GetActiveQuestInstance(QuestData questData)
        {
            return activeQuests.FirstOrDefault(q => q.questData == questData);
        }
        
        private bool CheckQuestPrerequisites(QuestData questData)
        {
            if (questData.prerequisites == null || questData.prerequisites.Count == 0)
                return true;
            
            foreach (var prerequisite in questData.prerequisites)
            {
                if (!IsQuestCompleted(prerequisite))
                    return false;
            }
            
            return true;
        }
        
        private bool AreAllObjectivesCompleted(QuestInstance questInstance)
        {
            return questInstance.questData.objectives.All(obj => obj.IsCompleted);
        }
        
        private void HandleQuestRewards(QuestData questData)
        {
            foreach (var reward in questData.rewards)
            {
                // This could be extended to handle different reward types
                Debug.Log($"[QuestManager] Rewarded: {reward.amount} {reward.rewardType} for quest {questData.questName}");
            }
        }
        
        #endregion
        
        #region Editor Methods
        
        [TabGroup("Quest", "Debug")]
        [Button("Complete All Active Quests")]
        [ShowIf("@IsActive && Application.isPlaying")]
        private void CompleteAllActiveQuests()
        {
            var questsToComplete = activeQuests.ToList();
            foreach (var quest in questsToComplete)
            {
                CompleteQuest(quest.questData);
            }
        }
        
        [TabGroup("Quest", "Debug")]
        [Button("Clear All Quests")]
        [ShowIf("@IsActive && Application.isPlaying")]
        private void ClearAllQuests()
        {
            activeQuests.Clear();
            completedQuests.Clear();
            failedQuests.Clear();
            OnQuestsUpdated?.Invoke();
            onQuestsUpdated?.Raise();
        }
        
        [TabGroup("Quest", "Debug")]
        [Button("Log Quest Status")]
        [ShowIf("@IsActive && Application.isPlaying")]
        private void LogQuestStatus()
        {
            Debug.Log($"[QuestManager] Active Quests: {activeQuests.Count}");
            Debug.Log($"[QuestManager] Completed Quests: {completedQuests.Count}");
            Debug.Log($"[QuestManager] Failed Quests: {failedQuests.Count}");
            Debug.Log($"[QuestManager] Completion Rate: {questCompletionRate:P}");
        }

        #endregion

        #region Adventure Template Integration Methods

        /// <summary>
        /// Event fired when story phase changes - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnStoryPhaseChanged;

        /// <summary>
        /// Event fired when dialogue is completed - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnDialogueCompleted;

        /// <summary>
        /// Event fired when player makes a choice - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnPlayerChoiceMade;

        /// <summary>
        /// Event fired when quest requires an item - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnItemRequired;

        /// <summary>
        /// Event fired when quest rewards an item - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnItemReward;

        /// <summary>
        /// Event fired when item is used in quest context - used by AdventureTemplateManager
        /// </summary>
        public event Action<string> OnItemUsed;

        /// <summary>
        /// Gets overall progress percentage across all active quests
        /// </summary>
        public float GetOverallProgressPercentage()
        {
            if (!IsActive || activeQuests.Count == 0)
                return 0f;

            float totalProgress = 0f;

            foreach (var questInstance in activeQuests)
            {
                totalProgress += GetQuestProgress(questInstance.questData);
            }

            return totalProgress / activeQuests.Count;
        }

        /// <summary>
        /// Triggers a quest by ID - alias for StartQuest
        /// </summary>
        public bool TriggerQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId) || questSettings == null)
                return false;

            // Find quest data by ID
            // TODO: Fix availableQuests property - QuestSettings doesn't have this property
            // var questData = questSettings.availableQuests?.FirstOrDefault(q => q.questId == questId);
            QuestData questData = null;
            if (questData == null)
            {
                Debug.LogWarning($"[QuestManager] Quest with ID '{questId}' not found in quest settings.");
                return false;
            }

            return StartQuest(questData);
        }

        /// <summary>
        /// Gets quest data by ID
        /// </summary>
                /// <summary>
        /// Gets the current state of a quest by ID
        /// </summary>
        public QuestState GetQuestState(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return QuestState.None;

            // Check active quests
            if (activeQuests?.Any(q => q.questData.questId == questId) == true)
                return QuestState.InProgress;

            // Check completed quests
            if (completedQuests?.Any(q => q.questId == questId) == true)
                return QuestState.Completed;

            // Check failed quests
            if (failedQuests?.Any(q => q.questId == questId) == true)
                return QuestState.Failed;

            // Check if quest exists in settings but not started
            // TODO: Fix availableQuests property - QuestSettings doesn't have this property
            // var questData = questSettings?.availableQuests?.FirstOrDefault(q => q.questId == questId);
            QuestData questData = null;
            if (questData != null)
                return QuestState.Available;

            return QuestState.None;
        }

        /// <summary>
        /// Gets quest data by ID
        /// </summary>
public QuestData GetQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId) || questSettings == null)
                return null;

            // First check active quests
            var activeQuest = activeQuests?.FirstOrDefault(q => q.questData.questId == questId);
            if (activeQuest != null)
                return activeQuest.questData;

            // Then check all available quests in settings
            // TODO: Fix availableQuests property - QuestSettings doesn't have this property
            // return questSettings.availableQuests?.FirstOrDefault(q => q.questId == questId);
            return null;
        }

        /// <summary>
        /// Triggers story phase change event
        /// </summary>
        public void TriggerStoryPhaseChange(string phaseName)
        {
            OnStoryPhaseChanged?.Invoke(phaseName);
            Debug.Log($"[QuestManager] Story phase changed to: {phaseName}");
        }

        /// <summary>
        /// Triggers dialogue completion event
        /// </summary>
        public void TriggerDialogueCompleted(string dialogueId)
        {
            OnDialogueCompleted?.Invoke(dialogueId);
            Debug.Log($"[QuestManager] Dialogue completed: {dialogueId}");
        }

        /// <summary>
        /// Triggers player choice made event
        /// </summary>
        public void TriggerPlayerChoiceMade(string choiceId)
        {
            OnPlayerChoiceMade?.Invoke(choiceId);
            Debug.Log($"[QuestManager] Player choice made: {choiceId}");
        }

        /// <summary>
        /// Triggers item required event
        /// </summary>
        public void TriggerItemRequired(string itemId)
        {
            OnItemRequired?.Invoke(itemId);
            Debug.Log($"[QuestManager] Item required: {itemId}");
        }

        /// <summary>
        /// Triggers item reward event
        /// </summary>
        public void TriggerItemReward(string itemId)
        {
            OnItemReward?.Invoke(itemId);
            Debug.Log($"[QuestManager] Item reward: {itemId}");
        }

        /// <summary>
        /// Triggers item used event
        /// </summary>
        public void TriggerItemUsed(string itemId)
        {
            OnItemUsed?.Invoke(itemId);
            Debug.Log($"[QuestManager] Item used: {itemId}");
        }

        

        #region Adventure Template Trigger Methods

        /// <summary>
        /// Trigger method for story phase changes from AdventureTemplateManager
        /// </summary>
        public void TriggerStoryPhaseChanged(int storyPhase)
        {
            OnStoryPhaseChanged?.Invoke(storyPhase.ToString()); // Convert int to string for Action<string> event
        }

        /// <summary>
        /// Trigger method for dialogue completion from AdventureTemplateManager
        /// </summary>
        public void TriggerDialogueCompleted(DialogueData dialogueData)
        {
            OnDialogueCompleted?.Invoke(dialogueData?.dialogueID ?? "Unknown"); // Use dialogueID instead of name property // Convert DialogueData to string for Action<string> event
        }

        /// <summary>
        /// Trigger method for player choice from AdventureTemplateManager
        /// </summary>
        public void TriggerPlayerChoiceMade(int choiceIndex, string choiceText)
        {
            OnPlayerChoiceMade?.Invoke(choiceText); // Only pass choiceText as the event expects Action<string>
        }

        /// <summary>
        /// Trigger method for item usage from AdventureTemplateManager
        /// </summary>
        public void TriggerItemUsed(AdventureItemData itemData)
        {
            OnItemUsed?.Invoke(itemData?.name ?? "Unknown"); // Convert AdventureItemData to string for Action<string> event
        }

        #endregion
#endregion
    }
    
    /// <summary>
    /// Runtime instance of a quest with progress tracking
    /// </summary>
    [System.Serializable]
    public class QuestInstance
    {
        public QuestData questData;
        public DateTime startTime;
        public float elapsedTime;
        
        public QuestInstance(QuestData data)
        {
            questData = data;
            startTime = DateTime.Now;
            elapsedTime = 0f;
            
            // Initialize objectives
            if (questData.objectives != null)
            {
                foreach (var objective in questData.objectives)
                {
                    objective.Reset();
                }
            }
        }
        
        public QuestObjective GetObjective(string objectiveId)
        {
            return questData.objectives?.FirstOrDefault(obj => obj.objectiveId == objectiveId);
        }
    }
}