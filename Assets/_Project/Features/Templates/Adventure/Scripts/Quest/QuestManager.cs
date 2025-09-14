using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
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
        // [SerializeField] private GameEvent<QuestData, QuestObjective> onObjectiveCompleted; // TODO: Implement after creating QuestObjective class
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
        
        private void Initialize()
        {
            if (questSettings == null)
            {
                Debug.LogError("[QuestManager] QuestSettings is null. Cannot initialize quest system.");
                return;
            }
            
            IsActive = true;
            
            // Register with Service Locator if available
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.RegisterService<QuestManager>(this);
            }
            
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
            if (ServiceLocator.Instance != null)
            {
                ServiceLocator.Instance.UnregisterService<QuestManager>();
            }
            
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
            if (questSettings.enableRewards && questData.rewards != null && questData.rewards.Count > 0)
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
            if (questSettings.enableQuestTimers)
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