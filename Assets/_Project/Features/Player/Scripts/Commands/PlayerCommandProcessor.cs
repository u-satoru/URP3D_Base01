using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Player.States;
using System.Collections.Generic;

namespace asterivo.Unity60.Player.Commands
{
    /// <summary>
    /// Processes commands for the player, integrating with skills and items
    /// </summary>
    public class PlayerCommandProcessor : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private DetailedPlayerStateMachine playerStateMachine;
        [SerializeField] private CommandInvoker commandInvoker;
        
        [Header("Command Events")]
        [SerializeField] private CommandGameEvent onCommandIssued;
        
        [Header("Available Skills")]
        [SerializeField] private List<SkillData> availableSkills = new List<SkillData>();
        
        [Header("Inventory Items")]
        [SerializeField] private List<ItemData> inventoryItems = new List<ItemData>();
        
        private PlayerCommandContext playerContext;
        
        private void Awake()
        {
            if (playerStateMachine == null)
            {
                playerStateMachine = GetComponent<DetailedPlayerStateMachine>();
            }
            
            if (commandInvoker == null)
            {
                commandInvoker = GetComponent<CommandInvoker>();
                if (commandInvoker == null)
                {
                    commandInvoker = gameObject.AddComponent<CommandInvoker>();
                }
            }
            
            // Create the player context
            playerContext = new PlayerCommandContext(playerStateMachine);
        }
        
        /// <summary>
        /// Uses a skill by index
        /// </summary>
        public void UseSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= availableSkills.Count)
            {
                Debug.LogWarning($"Invalid skill index: {skillIndex}");
                return;
            }
            
            var skill = availableSkills[skillIndex];
            if (skill == null)
                return;
                
            var commands = skill.Use(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Uses a skill by reference
        /// </summary>
        public void UseSkill(SkillData skill)
        {
            if (skill == null)
                return;
                
            var commands = skill.Use(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Uses an item by index
        /// </summary>
        public void UseItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= inventoryItems.Count)
            {
                Debug.LogWarning($"Invalid item index: {itemIndex}");
                return;
            }
            
            var item = inventoryItems[itemIndex];
            if (item == null)
                return;
                
            var commands = item.Use(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Uses an item by reference
        /// </summary>
        public void UseItem(ItemData item)
        {
            if (item == null)
                return;
                
            var commands = item.Use(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Equips an item
        /// </summary>
        public void EquipItem(ItemData item)
        {
            if (item == null)
                return;
                
            var commands = item.Equip(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Unequips an item
        /// </summary>
        public void UnequipItem(ItemData item)
        {
            if (item == null)
                return;
                
            var commands = item.Unequip(playerContext);
            ExecuteCommands(commands);
        }
        
        /// <summary>
        /// Executes a list of commands
        /// </summary>
        private void ExecuteCommands(List<ICommand> commands)
        {
            foreach (var command in commands)
            {
                if (command != null)
                {
                    // Send through event system if event is configured
                    if (onCommandIssued != null)
                    {
                        onCommandIssued.Raise(command);
                    }
                    else if (commandInvoker != null)
                    {
                        // Direct execution if no event configured
                        commandInvoker.ExecuteCommand(command);
                    }
                    else
                    {
                        // Fallback to immediate execution
                        command.Execute();
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds a skill to available skills
        /// </summary>
        public void LearnSkill(SkillData skill)
        {
            if (skill != null && !availableSkills.Contains(skill))
            {
                availableSkills.Add(skill);
            }
        }
        
        /// <summary>
        /// Removes a skill from available skills
        /// </summary>
        public void ForgetSkill(SkillData skill)
        {
            if (skill != null)
            {
                availableSkills.Remove(skill);
            }
        }
        
        /// <summary>
        /// Adds an item to inventory
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item != null)
            {
                inventoryItems.Add(item);
            }
        }
        
        /// <summary>
        /// Removes an item from inventory
        /// </summary>
        public void RemoveItem(ItemData item)
        {
            if (item != null)
            {
                inventoryItems.Remove(item);
            }
        }
        
        // Undo/Redo delegation
        public void Undo()
        {
            commandInvoker?.Undo();
        }
        
        public void Redo()
        {
            commandInvoker?.Redo();
        }
        
        public bool CanUndo => commandInvoker?.CanUndo ?? false;
        public bool CanRedo => commandInvoker?.CanRedo ?? false;
    }
}