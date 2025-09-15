using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialog
{
    /// <summary>
    /// Represents a choice option in a dialog node
    /// </summary>
    [System.Serializable]
    public class DialogChoice
    {
        [Header("Choice Information")]
        [SerializeField] public string choiceId = "";
        [SerializeField, TextArea(2, 4)] public string choiceText = "";
        [SerializeField] public string nextNodeId = "";

        [Header("Choice Settings")]
        [SerializeField] public bool isEnabled = true;
        [SerializeField] public bool showOnlyOnce = false;
        [SerializeField] public bool hasBeenSelected = false;

        [Header("Conditions")]
        [SerializeField] public List<DialogCondition> conditions = new List<DialogCondition>();
        [SerializeField] public List<DialogAction> actions = new List<DialogAction>();

        [Header("Visual Settings")]
        [SerializeField] public Color textColor = Color.white;
        [SerializeField] public Color disabledColor = Color.gray;

        /// <summary>
        /// Check if this choice can be selected based on conditions
        /// </summary>
        public bool CanSelect()
        {
            // Check if disabled
            if (!isEnabled) return false;

            // Check if already selected and should only show once
            if (showOnlyOnce && hasBeenSelected) return false;

            // Check conditions
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
        /// Execute this choice - mark as selected and execute actions
        /// </summary>
        public void ExecuteChoice()
        {
            hasBeenSelected = true;

            foreach (var action in actions)
            {
                action.ExecuteAction();
            }

            Debug.Log($"[DialogChoice] Executed choice: {choiceText}");
        }

        /// <summary>
        /// Get the display text for this choice
        /// </summary>
        public string GetDisplayText()
        {
            if (!CanSelect())
            {
                return $"<color=#{ColorUtility.ToHtmlStringRGBA(disabledColor)}>{choiceText}</color>";
            }

            return choiceText;
        }

        /// <summary>
        /// Get the color to use for this choice
        /// </summary>
        public Color GetChoiceColor()
        {
            return CanSelect() ? textColor : disabledColor;
        }

        /// <summary>
        /// Reset this choice (clear selection history)
        /// </summary>
        public void Reset()
        {
            hasBeenSelected = false;
        }

        public DialogChoice()
        {
            choiceId = System.Guid.NewGuid().ToString();
        }

        public DialogChoice(string text, string nextNode)
        {
            choiceId = System.Guid.NewGuid().ToString();
            choiceText = text;
            nextNodeId = nextNode;
        }

        public DialogChoice(string id, string text, string nextNode)
        {
            choiceId = id;
            choiceText = text;
            nextNodeId = nextNode;
        }
    }

    /// <summary>
    /// Event data for dialog choice events
    /// </summary>
    [System.Serializable]
    public class DialogChoiceMadeEventData
    {
        public DialogChoice choice;
        public DialogNode fromNode;
        public string nextNodeId;

        public DialogChoiceMadeEventData(DialogChoice selectedChoice, DialogNode currentNode, string nextNode)
        {
            choice = selectedChoice;
            fromNode = currentNode;
            nextNodeId = nextNode;
        }
    }

    /// <summary>
    /// Event data for dialog start events
    /// </summary>
    [System.Serializable]
    public class DialogStartedEventData
    {
        public DialogData dialog;
        public DialogNode startNode;
        public GameObject initiator;

        public DialogStartedEventData(DialogData dialogData, DialogNode firstNode, GameObject source = null)
        {
            dialog = dialogData;
            startNode = firstNode;
            initiator = source;
        }
    }

    /// <summary>
    /// Event data for dialog end events
    /// </summary>
    [System.Serializable]
    public class DialogEndedEventData
    {
        public DialogData dialog;
        public DialogNode lastNode;
        public bool wasCompleted;

        public DialogEndedEventData(DialogData dialogData, DialogNode finalNode, bool completed = true)
        {
            dialog = dialogData;
            lastNode = finalNode;
            wasCompleted = completed;
        }
    }

    /// <summary>
    /// Event data for dialog node change events
    /// </summary>
    [System.Serializable]
    public class DialogNodeChangedEventData
    {
        public DialogData dialog;
        public DialogNode previousNode;
        public DialogNode currentNode;

        public DialogNodeChangedEventData(DialogData dialogData, DialogNode prevNode, DialogNode newNode)
        {
            dialog = dialogData;
            previousNode = prevNode;
            currentNode = newNode;
        }
    }
}