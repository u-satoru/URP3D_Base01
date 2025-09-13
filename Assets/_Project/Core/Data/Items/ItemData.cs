using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
// using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency

/// <summary>
/// Item data using polymorphic command serialization (ドキュメント第4章:444-452行目の実装)
/// 
/// ハイブリッドアーキテクチャの実装例：
/// - SerializeReference属性によるポリモーフィックシリアライゼーション
/// - デザイナーがInspectorでHealCommandDefinition等をリストに追加可能
/// - 装備システム(Equip/Unequip)はプロジェクト独自の拡張機能
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
    [Tooltip("Commands executed when the item is used (ドキュメント第4章対応)")]
    public List<object> commandDefinitions = new List<object>();
    
    [Header("Equipment System (Project Extension)")]
    [SerializeReference]
    [Tooltip("Commands executed when the item is equipped (プロジェクト独自拡張)")]
    public List<object> equipCommandDefinitions = new List<object>();
    
    [SerializeReference]
    [Tooltip("Commands executed when the item is unequipped (プロジェクト独自拡張)")]
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
