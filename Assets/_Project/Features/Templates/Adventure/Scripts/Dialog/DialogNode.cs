using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialog
{
    /// <summary>
    /// Represents a single node in a dialog tree
    /// </summary>
    [System.Serializable]
    public class DialogNode
    {
        [Header("Node Information")]
        [SerializeField] public string nodeId = "";
        [SerializeField] public string characterId = "";
        [SerializeField, TextArea(3, 6)] public string dialogText = "";

        [Header("Node Behavior")]
        [SerializeField] public DialogNodeType nodeType = DialogNodeType.Standard;
        [SerializeField] public string nextNodeId = "";
        [SerializeField] public List<DialogChoice> choices = new List<DialogChoice>();

        [Header("Presentation")]
        [SerializeField] public float textSpeed = -1f; // -1 uses default speed
        [SerializeField] public AudioClip voiceClip;
        [SerializeField] public bool waitForInput = true;
        [SerializeField] public float autoAdvanceDelay = 0f;

        [Header("Conditions")]
        [SerializeField] public List<DialogCondition> conditions = new List<DialogCondition>();
        [SerializeField] public List<DialogAction> actions = new List<DialogAction>();

        /// <summary>
        /// Check if this node can be activated based on its conditions
        /// </summary>
        public bool CanActivate()
        {
            foreach (var condition in conditions)
            {
                if (!condition.IsConditionMet())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Execute all actions associated with this node
        /// </summary>
        public void ExecuteActions()
        {
            foreach (var action in actions)
            {
                action.ExecuteAction();
            }
        }

        /// <summary>
        /// Get the effective text speed for this node
        /// </summary>
        public float GetTextSpeed(float defaultSpeed)
        {
            return textSpeed > 0f ? textSpeed : defaultSpeed;
        }

        /// <summary>
        /// Check if this node has choices
        /// </summary>
        public bool HasChoices => choices != null && choices.Count > 0 && nodeType == DialogNodeType.Choice;

        /// <summary>
        /// Check if this node should auto-advance
        /// </summary>
        public bool ShouldAutoAdvance => !waitForInput && autoAdvanceDelay > 0f;

        /// <summary>
        /// Get valid choices (those that meet their conditions)
        /// </summary>
        public List<DialogChoice> GetValidChoices()
        {
            if (!HasChoices) return new List<DialogChoice>();

            List<DialogChoice> validChoices = new List<DialogChoice>();
            foreach (var choice in choices)
            {
                if (choice.CanSelect())
                {
                    validChoices.Add(choice);
                }
            }
            return validChoices;
        }

        public DialogNode()
        {
            nodeId = System.Guid.NewGuid().ToString();
        }

        public DialogNode(string id, string character, string text)
        {
            nodeId = id;
            characterId = character;
            dialogText = text;
        }
    }

    /// <summary>
    /// Types of dialog nodes
    /// </summary>
    public enum DialogNodeType
    {
        Standard,   // Regular dialog node with continue button
        Choice,     // Node with multiple choice options
        End,        // Terminal node that ends the dialog
        Redirect,   // Node that immediately redirects to another node
        Event       // Node that triggers a game event
    }

    /// <summary>
    /// Character information for dialog
    /// </summary>
    [System.Serializable]
    public class DialogCharacter
    {
        [SerializeField] public string characterId = "";
        [SerializeField] public string characterName = "";
        [SerializeField] public Sprite characterPortrait;
        [SerializeField] public Color nameColor = Color.white;
        [SerializeField] public AudioClip characterVoice;

        public DialogCharacter()
        {
            characterId = System.Guid.NewGuid().ToString();
        }

        public DialogCharacter(string id, string name, Sprite portrait = null)
        {
            characterId = id;
            characterName = name;
            characterPortrait = portrait;
        }
    }

    /// <summary>
    /// Condition that must be met for a dialog node to be accessible
    /// </summary>
    [System.Serializable]
    public class DialogCondition
    {
        [Header("Condition Settings")]
        [SerializeField] public DialogConditionType conditionType = DialogConditionType.Always;
        [SerializeField] public string variableName = "";
        [SerializeField] public string requiredValue = "";
        [SerializeField] public ComparisonType comparison = ComparisonType.Equals;

        public bool IsConditionMet()
        {
            switch (conditionType)
            {
                case DialogConditionType.Always:
                    return true;

                case DialogConditionType.Never:
                    return false;

                case DialogConditionType.VariableEquals:
                    return CheckVariableCondition();

                case DialogConditionType.QuestCompleted:
                    // TODO: Implement quest system integration
                    return true;

                case DialogConditionType.ItemInInventory:
                    // TODO: Implement inventory system integration
                    return true;

                default:
                    return true;
            }
        }

        private bool CheckVariableCondition()
        {
            // TODO: Implement variable system integration
            // For now, always return true
            return true;
        }
    }

    /// <summary>
    /// Action to execute when a dialog node is activated
    /// </summary>
    [System.Serializable]
    public class DialogAction
    {
        [Header("Action Settings")]
        [SerializeField] public DialogActionType actionType = DialogActionType.None;
        [SerializeField] public string targetName = "";
        [SerializeField] public string actionValue = "";

        public void ExecuteAction()
        {
            switch (actionType)
            {
                case DialogActionType.None:
                    break;

                case DialogActionType.SetVariable:
                    // TODO: Implement variable system integration
                    Debug.Log($"[DialogAction] Set variable '{targetName}' to '{actionValue}'");
                    break;

                case DialogActionType.GiveItem:
                    // TODO: Implement inventory system integration
                    Debug.Log($"[DialogAction] Give item '{targetName}'");
                    break;

                case DialogActionType.StartQuest:
                    // TODO: Implement quest system integration
                    Debug.Log($"[DialogAction] Start quest '{targetName}'");
                    break;

                case DialogActionType.CompleteQuest:
                    // TODO: Implement quest system integration
                    Debug.Log($"[DialogAction] Complete quest '{targetName}'");
                    break;

                case DialogActionType.PlaySound:
                    // TODO: Implement audio system integration
                    Debug.Log($"[DialogAction] Play sound '{targetName}'");
                    break;
            }
        }
    }

    public enum DialogConditionType
    {
        Always,
        Never,
        VariableEquals,
        QuestCompleted,
        ItemInInventory
    }

    public enum DialogActionType
    {
        None,
        SetVariable,
        GiveItem,
        StartQuest,
        CompleteQuest,
        PlaySound
    }

    public enum ComparisonType
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }
}