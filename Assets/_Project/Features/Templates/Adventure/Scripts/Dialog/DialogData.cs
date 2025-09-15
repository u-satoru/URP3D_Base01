using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Adventure.Dialog
{
    /// <summary>
    /// ScriptableObject for storing dialog data in the Adventure template
    /// </summary>
    [CreateAssetMenu(fileName = "DialogData", menuName = "Adventure Template/Dialog Data")]
    public class DialogData : ScriptableObject
    {
        [Header("Dialog Information")]
        [SerializeField] private string dialogTitle = "New Dialog";
        [SerializeField, TextArea(2, 4)] private string dialogDescription = "";
        [SerializeField] private string startNodeId = "start";

        [Header("Characters")]
        [SerializeField] private List<DialogCharacter> characters = new List<DialogCharacter>();

        [Header("Dialog Nodes")]
        [SerializeField] private List<DialogNode> nodes = new List<DialogNode>();

        [Header("Dialog Settings")]
        [SerializeField] private bool canBeSkipped = true;
        [SerializeField] private bool canBeRepeated = true;
        [SerializeField] private float defaultTypewriterSpeed = 0.03f;

        // Public Properties
        public string DialogTitle => dialogTitle;
        public string DialogDescription => dialogDescription;
        public string StartNodeId => startNodeId;
        public List<DialogCharacter> Characters => characters;
        public List<DialogNode> Nodes => nodes;
        public bool CanBeSkipped => canBeSkipped;
        public bool CanBeRepeated => canBeRepeated;
        public float DefaultTypewriterSpeed => defaultTypewriterSpeed;

        /// <summary>
        /// Get a dialog node by its ID
        /// </summary>
        public DialogNode GetNode(string nodeId)
        {
            return nodes.Find(node => node.nodeId == nodeId);
        }

        /// <summary>
        /// Get the starting dialog node
        /// </summary>
        public DialogNode GetStartNode()
        {
            return GetNode(startNodeId);
        }

        /// <summary>
        /// Get a character by ID
        /// </summary>
        public DialogCharacter GetCharacter(string characterId)
        {
            return characters.Find(character => character.characterId == characterId);
        }

        /// <summary>
        /// Add a new node to the dialog
        /// </summary>
        public void AddNode(DialogNode node)
        {
            if (node != null && !nodes.Contains(node))
            {
                nodes.Add(node);
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }

        /// <summary>
        /// Remove a node from the dialog
        /// </summary>
        public void RemoveNode(DialogNode node)
        {
            if (node != null)
            {
                nodes.Remove(node);
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }

        /// <summary>
        /// Validate the dialog structure
        /// </summary>
        public bool ValidateDialog()
        {
            // Check if start node exists
            if (GetStartNode() == null)
            {
                Debug.LogError($"[DialogData] {dialogTitle}: Start node '{startNodeId}' not found");
                return false;
            }

            // Check for duplicate node IDs
            HashSet<string> nodeIds = new HashSet<string>();
            foreach (var node in nodes)
            {
                if (!nodeIds.Add(node.nodeId))
                {
                    Debug.LogError($"[DialogData] {dialogTitle}: Duplicate node ID '{node.nodeId}'");
                    return false;
                }
            }

            // Check for invalid node references
            foreach (var node in nodes)
            {
                // Check next node references
                if (!string.IsNullOrEmpty(node.nextNodeId) && GetNode(node.nextNodeId) == null)
                {
                    Debug.LogWarning($"[DialogData] {dialogTitle}: Node '{node.nodeId}' references invalid next node '{node.nextNodeId}'");
                }

                // Check choice node references
                foreach (var choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.nextNodeId) && GetNode(choice.nextNodeId) == null)
                    {
                        Debug.LogWarning($"[DialogData] {dialogTitle}: Choice in node '{node.nodeId}' references invalid node '{choice.nextNodeId}'");
                    }
                }
            }

            return true;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure start node ID is not empty
            if (string.IsNullOrEmpty(startNodeId))
                startNodeId = "start";

            // Ensure dialog title is not empty
            if (string.IsNullOrEmpty(dialogTitle))
                dialogTitle = name;
        }

        [ContextMenu("Validate Dialog")]
        private void ValidateDialogContext()
        {
            bool isValid = ValidateDialog();
            Debug.Log($"[DialogData] Dialog '{dialogTitle}' validation: {(isValid ? "PASSED" : "FAILED")}");
        }

        [ContextMenu("Create Basic Dialog Structure")]
        private void CreateBasicStructure()
        {
            // Clear existing data
            nodes.Clear();
            characters.Clear();

            // Add default character
            characters.Add(new DialogCharacter 
            { 
                characterId = "speaker",
                characterName = "Speaker",
                characterPortrait = null
            });

            // Add start node
            nodes.Add(new DialogNode
            {
                nodeId = "start",
                characterId = "speaker",
                dialogText = "Hello! This is a sample dialog.",
                nextNodeId = "end"
            });

            // Add end node
            nodes.Add(new DialogNode
            {
                nodeId = "end",
                characterId = "speaker",
                dialogText = "Goodbye!",
                nextNodeId = ""
            });

            Debug.Log($"[DialogData] Created basic dialog structure for '{dialogTitle}'");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        #endif
    }
}