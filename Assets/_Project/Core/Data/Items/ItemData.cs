using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ScriptableObject for item definitions using polymorphic serialization
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "asterivo.Unity60/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Information")]
        [SerializeField] private string itemName = "New Item";
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private Sprite icon;
        
        [Header("Item Properties")]
        [SerializeField] private ItemType itemType = ItemType.Consumable;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private float weight = 1.0f;
        [SerializeField] private int value = 10;
        
        [Header("Use Commands")]
        [SerializeReference] 
        [Tooltip("Commands executed when this item is used")]
        private List<ICommandDefinition> useCommands = new List<ICommandDefinition>();
        
        [Header("Equip Commands")]
        [SerializeReference] 
        [Tooltip("Commands executed when this item is equipped (if equippable)")]
        private List<ICommandDefinition> equipCommands = new List<ICommandDefinition>();
        
        [Header("Unequip Commands")]
        [SerializeReference] 
        [Tooltip("Commands executed when this item is unequipped")]
        private List<ICommandDefinition> unequipCommands = new List<ICommandDefinition>();
        
        public enum ItemType
        {
            Consumable,
            Equipment,
            Weapon,
            Armor,
            Quest,
            Material,
            Currency
        }
        
        /// <summary>
        /// Checks if the item can be used
        /// </summary>
        public bool CanUse(object context = null)
        {
            foreach (var command in useCommands)
            {
                if (command != null && !command.CanExecute(context))
                    return false;
            }
            return useCommands.Count > 0;
        }
        
        /// <summary>
        /// Uses the item by creating and returning commands
        /// </summary>
        public List<ICommand> Use(object context = null)
        {
            var commands = new List<ICommand>();
            
            if (!CanUse(context))
            {
                Debug.LogWarning($"Cannot use item: {itemName}");
                return commands;
            }
            
            foreach (var definition in useCommands)
            {
                if (definition != null)
                {
                    var command = definition.CreateCommand(context);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
            }
            
            return commands;
        }
        
        /// <summary>
        /// Checks if the item can be equipped
        /// </summary>
        public bool CanEquip(object context = null)
        {
            if (itemType != ItemType.Equipment && 
                itemType != ItemType.Weapon && 
                itemType != ItemType.Armor)
                return false;
                
            foreach (var command in equipCommands)
            {
                if (command != null && !command.CanExecute(context))
                    return false;
            }
            return equipCommands.Count > 0;
        }
        
        /// <summary>
        /// Equips the item by creating and returning commands
        /// </summary>
        public List<ICommand> Equip(object context = null)
        {
            var commands = new List<ICommand>();
            
            if (!CanEquip(context))
            {
                Debug.LogWarning($"Cannot equip item: {itemName}");
                return commands;
            }
            
            foreach (var definition in equipCommands)
            {
                if (definition != null)
                {
                    var command = definition.CreateCommand(context);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
            }
            
            return commands;
        }
        
        /// <summary>
        /// Unequips the item by creating and returning commands
        /// </summary>
        public List<ICommand> Unequip(object context = null)
        {
            var commands = new List<ICommand>();
            
            foreach (var definition in unequipCommands)
            {
                if (definition != null)
                {
                    var command = definition.CreateCommand(context);
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }
            }
            
            return commands;
        }
        
        // Properties
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemType Type => itemType;
        public int MaxStackSize => maxStackSize;
        public float Weight => weight;
        public int Value => value;
        public bool IsStackable => maxStackSize > 1;
        public bool IsEquippable => itemType == ItemType.Equipment || 
                                   itemType == ItemType.Weapon || 
                                   itemType == ItemType.Armor;
    }
}