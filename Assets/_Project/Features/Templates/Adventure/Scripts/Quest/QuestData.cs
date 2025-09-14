using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Quest
{
    /// <summary>
    /// ScriptableObject containing quest definition and configuration
    /// Supports complex quest structures with objectives, prerequisites, and rewards
    /// </summary>
    [CreateAssetMenu(fileName = "QuestData", menuName = "Templates/Adventure/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [TabGroup("Basic", "Quest Information")]
        [Header("Basic Information")]
        [SerializeField]
        public string questId = "";
        
        [TabGroup("Basic", "Quest Information")]
        [SerializeField]
        public string questName = "New Quest";
        
        [TabGroup("Basic", "Quest Information")]
        [SerializeField]
        [TextArea(3, 6)]
        public string description = "Quest description here...";
        
        [TabGroup("Basic", "Quest Information")]
        [SerializeField]
        public QuestType questType = QuestType.Main;
        
        [TabGroup("Basic", "Quest Information")]
        [SerializeField]
        [Range(1, 100)]
        public int questLevel = 1;
        
        [TabGroup("Basic", "Quest Information")]
        [SerializeField]
        public QuestPriority priority = QuestPriority.Normal;
        
        [TabGroup("Objectives", "Quest Objectives")]
        [Header("Objectives")]
        [SerializeField]
        public List<QuestObjective> objectives = new List<QuestObjective>();
        
        [TabGroup("Requirements", "Prerequisites")]
        [Header("Prerequisites")]
        [SerializeField]
        [Tooltip("Quests that must be completed before this quest can be started")]
        public List<QuestData> prerequisites = new List<QuestData>();
        
        [TabGroup("Requirements", "Prerequisites")]
        [SerializeField]
        [Range(1, 100)]
        [Tooltip("Minimum player level required to start this quest")]
        public int requiredLevel = 1;
        
        [TabGroup("Timing", "Time Settings")]
        [Header("Time Management")]
        [SerializeField]
        public bool hasTimeLimit = false;
        
        [TabGroup("Timing", "Time Settings")]
        [SerializeField]
        [ShowIf("hasTimeLimit")]
        [Range(60f, 3600f)]
        [Tooltip("Time limit in seconds")]
        public float timeLimit = 300f;
        
        [TabGroup("Rewards", "Quest Rewards")]
        [Header("Rewards")]
        [SerializeField]
        public List<QuestReward> rewards = new List<QuestReward>();
        
        [TabGroup("Story", "Story Integration")]
        [Header("Story Integration")]
        [SerializeField]
        [TextArea(2, 4)]
        public string startDialogue = "";
        
        [TabGroup("Story", "Story Integration")]
        [SerializeField]
        [TextArea(2, 4)]
        public string completionDialogue = "";
        
        [TabGroup("Story", "Story Integration")]
        [SerializeField]
        public bool triggersCutscene = false;
        
        [TabGroup("Story", "Story Integration")]
        [SerializeField]
        [ShowIf("triggersCutscene")]
        public string cutsceneId = "";
        
        [TabGroup("Audio", "Audio Settings")]
        [Header("Audio")]
        [SerializeField]
        public AudioClip questStartSound;
        
        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        public AudioClip questCompleteSound;
        
        [TabGroup("Audio", "Audio Settings")]
        [SerializeField]
        public AudioClip objectiveCompleteSound;
        
        #region Validation
        
        private void OnValidate()
        {
            ValidateQuestId();
            ValidateObjectives();
            ValidateRewards();
        }
        
        private void ValidateQuestId()
        {
            if (string.IsNullOrEmpty(questId))
            {
                questId = name.Replace(" ", "_").ToLower();
            }
        }
        
        private void ValidateObjectives()
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                if (string.IsNullOrEmpty(objectives[i].objectiveId))
                {
                    objectives[i].objectiveId = $"{questId}_objective_{i}";
                }
            }
        }
        
        private void ValidateRewards()
        {
            // Remove any rewards with invalid data
            rewards.RemoveAll(r => r.amount <= 0);
        }
        
        #endregion
        
        #region Editor Helpers
        
        [TabGroup("Debug", "Quest Info")]
        [Button("Generate Unique Quest ID")]
        private void GenerateUniqueQuestId()
        {
            questId = $"quest_{Guid.NewGuid().ToString("N")[..8]}";
        }
        
        [TabGroup("Debug", "Quest Info")]
        [Button("Add Default Objective")]
        private void AddDefaultObjective()
        {
            var newObjective = new QuestObjective
            {
                objectiveId = $"{questId}_objective_{objectives.Count}",
                description = "New Objective",
                objectiveType = ObjectiveType.Collect,
                targetAmount = 1
            };
            objectives.Add(newObjective);
        }
        
        [TabGroup("Debug", "Quest Info")]
        [ShowInInspector, ReadOnly]
        public string QuestSummary => $"{questName} ({questType}) - {objectives.Count} objectives";
        
        #endregion
    }
    
    /// <summary>
    /// Individual quest objective with progress tracking
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        [Header("Objective Information")]
        public string objectiveId = "";
        
        [TextArea(2, 3)]
        public string description = "Objective description";
        
        public ObjectiveType objectiveType = ObjectiveType.Collect;
        
        [Header("Progress Tracking")]
        [Range(1, 1000)]
        public int targetAmount = 1;
        
        [SerializeField, ReadOnly]
        public int currentProgress = 0;
        
        [Header("Optional Settings")]
        public bool isOptional = false;
        public bool trackProgress = true;
        
        // Runtime data (not serialized)
        [System.NonSerialized]
        public DateTime? completionTime;
        
        public bool IsCompleted => currentProgress >= targetAmount;
        
        public float ProgressPercentage => targetAmount > 0 ? (float)currentProgress / targetAmount : 0f;
        
        public void Reset()
        {
            currentProgress = 0;
            completionTime = null;
        }
    }
    
    /// <summary>
    /// Quest reward definition
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        public RewardType rewardType = RewardType.Experience;
        
        [Range(1, 10000)]
        public int amount = 1;
        
        [ShowIf("rewardType", RewardType.Item)]
        public string itemId = "";
        
        public string description = "";
    }
    
    /// <summary>
    /// Types of quests available in the adventure template
    /// </summary>
    public enum QuestType
    {
        Main,           // Main story quest
        Side,           // Side quest
        Daily,          // Daily repeatable quest
        Weekly,         // Weekly repeatable quest
        Tutorial,       // Tutorial quest
        Achievement,    // Achievement-based quest
        Exploration,    // Exploration quest
        Collection      // Collection quest
    }
    
    /// <summary>
    /// Quest priority levels for UI ordering
    /// </summary>
    public enum QuestPriority
    {
        Low,
        Normal,
        High,
        Critical
    }
    
    /// <summary>
    /// Types of objectives that can be tracked
    /// </summary>
    public enum ObjectiveType
    {
        Collect,        // Collect items
        Kill,           // Defeat enemies
        Interact,       // Interact with objects/NPCs
        Reach,          // Reach a location
        Deliver,        // Deliver items
        Escort,         // Escort NPCs
        Survive,        // Survive for time
        Discover,       // Discover locations/secrets
        Use,            // Use specific items/abilities
        Custom          // Custom objective type
    }
    
    /// <summary>
    /// Types of rewards available for quest completion
    /// </summary>
    public enum RewardType
    {
        Experience,     // Experience points
        Gold,           // Currency
        Item,           // Specific item
        Skill,          // Skill points
        Achievement,    // Achievement unlock
        Story,          // Story progression
        Access          // Access to new areas/content
    }
}