using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency

/// <summary>
/// Item data using polymorphic command serialization (繝峨く繝･繝｡繝ｳ繝育ｬｬ4遶:444-452陦檎岼縺ｮ螳溯｣・
/// 
/// 繝上う繝悶Μ繝・ラ繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｮ螳溯｣・ｾ具ｼ・/// - SerializeReference螻樊ｧ縺ｫ繧医ｋ繝昴Μ繝｢繝ｼ繝輔ぅ繝・け繧ｷ繝ｪ繧｢繝ｩ繧､繧ｼ繝ｼ繧ｷ繝ｧ繝ｳ
/// - 繝・じ繧､繝翫・縺栗nspector縺ｧHealCommandDefinition遲峨ｒ繝ｪ繧ｹ繝医↓霑ｽ蜉蜿ｯ閭ｽ
/// - 陬・ｙ繧ｷ繧ｹ繝・Β(Equip/Unequip)縺ｯ繝励Ο繧ｸ繧ｧ繧ｯ繝育峡閾ｪ縺ｮ諡｡蠑ｵ讖溯・
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "Data/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;

    [Header("Item Commands")]
    [SerializeReference]
    [Tooltip("Commands executed when the item is used (繝峨く繝･繝｡繝ｳ繝育ｬｬ4遶蟇ｾ蠢・")]
    public List<object> commandDefinitions = new List<object>();
    
    [Header("Equipment System (Project Extension)")]
    [SerializeReference]
    [Tooltip("Commands executed when the item is equipped (繝励Ο繧ｸ繧ｧ繧ｯ繝育峡閾ｪ諡｡蠑ｵ)")]
    public List<object> equipCommandDefinitions = new List<object>();
    
    [SerializeReference]
    [Tooltip("Commands executed when the item is unequipped (繝励Ο繧ｸ繧ｧ繧ｯ繝育峡閾ｪ諡｡蠑ｵ)")]
    public List<object> unequipCommandDefinitions = new List<object>();
    
    /// <summary>
    /// Checks if the item can be used
    /// </summary>
    public bool CanUse(object context = null)
    {
        // Check all command definitions
        foreach (var definition in commandDefinitions)
        {
            // Temporarily comment out to resolve circular dependency
            // if (definition != null && !definition.CanExecute(context))
            //     return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Uses the item by creating and returning commands
    /// </summary>
    public List<object> Use(object context = null)
    {
        var commands = new List<object>();
        
        if (!CanUse(context))
        {
            Debug.LogWarning($"Cannot use item: {itemName}");
            return commands;
        }
        
        foreach (var definition in commandDefinitions)
        {
            if (definition != null)
            {
                // Temporarily comment out to resolve circular dependency
                // var command = definition.CreateCommand(context);
                // if (command != null)
                // {
                //     commands.Add(command);
                // }
            }
        }
        
        return commands;
    }
    
    /// <summary>
    /// Checks if the item can be equipped
    /// </summary>
    public bool CanEquip(object context = null)
    {
        // Check all equip command definitions
        foreach (var definition in equipCommandDefinitions)
        {
            // Temporarily comment out to resolve circular dependency
            // if (definition != null && !definition.CanExecute(context))
            //     return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Equips the item by creating and returning equip commands
    /// </summary>
    public List<object> Equip(object context = null)
    {
        var commands = new List<object>();
        
        if (!CanEquip(context))
        {
            Debug.LogWarning($"Cannot equip item: {itemName}");
            return commands;
        }
        
        foreach (var definition in equipCommandDefinitions)
        {
            if (definition != null)
            {
                // Temporarily comment out to resolve circular dependency
                // var command = definition.CreateCommand(context);
                // if (command != null)
                // {
                //     commands.Add(command);
                // }
            }
        }
        
        return commands;
    }
    
    /// <summary>
    /// Checks if the item can be unequipped
    /// </summary>
    public bool CanUnequip(object context = null)
    {
        // Check all unequip command definitions
        foreach (var definition in unequipCommandDefinitions)
        {
            // Temporarily comment out to resolve circular dependency
            // if (definition != null && !definition.CanExecute(context))
            //     return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Unequips the item by creating and returning unequip commands
    /// </summary>
    public List<object> Unequip(object context = null)
    {
        var commands = new List<object>();
        
        if (!CanUnequip(context))
        {
            Debug.LogWarning($"Cannot unequip item: {itemName}");
            return commands;
        }
        
        foreach (var definition in unequipCommandDefinitions)
        {
            if (definition != null)
            {
                // Temporarily comment out to resolve circular dependency
                // var command = definition.CreateCommand(context);
                // if (command != null)
                // {
                //     commands.Add(command);
                // }
            }
        }
        
        return commands;
    }
}
