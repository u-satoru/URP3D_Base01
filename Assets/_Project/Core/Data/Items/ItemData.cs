using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using asterivo.Unity60.Core.Commands;

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
    public List<ICommandDefinition> commandDefinitions = new List<ICommandDefinition>();
    
    [Header("Equipment System (Project Extension)")]
    [SerializeReference]
    [Tooltip("Commands executed when the item is equipped (プロジェクト独自拡張)")]
    public List<ICommandDefinition> equipCommandDefinitions = new List<ICommandDefinition>();
    
    [SerializeReference]
    [Tooltip("Commands executed when the item is unequipped (プロジェクト独自拡張)")]
    public List<ICommandDefinition> unequipCommandDefinitions = new List<ICommandDefinition>();
    
    /// <summary>
    /// Checks if the item can be used
    /// </summary>
    public bool CanUse(object context = null)
    {
        // Check all command definitions
        foreach (var definition in commandDefinitions)
        {
            if (definition != null && !definition.CanExecute(context))
                return false;
        }
        
        return true;
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
        
        foreach (var definition in commandDefinitions)
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
        // Check all equip command definitions
        foreach (var definition in equipCommandDefinitions)
        {
            if (definition != null && !definition.CanExecute(context))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Equips the item by creating and returning equip commands
    /// </summary>
    public List<ICommand> Equip(object context = null)
    {
        var commands = new List<ICommand>();
        
        if (!CanEquip(context))
        {
            Debug.LogWarning($"Cannot equip item: {itemName}");
            return commands;
        }
        
        foreach (var definition in equipCommandDefinitions)
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
    /// Checks if the item can be unequipped
    /// </summary>
    public bool CanUnequip(object context = null)
    {
        // Check all unequip command definitions
        foreach (var definition in unequipCommandDefinitions)
        {
            if (definition != null && !definition.CanExecute(context))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Unequips the item by creating and returning unequip commands
    /// </summary>
    public List<ICommand> Unequip(object context = null)
    {
        var commands = new List<ICommand>();
        
        if (!CanUnequip(context))
        {
            Debug.LogWarning($"Cannot unequip item: {itemName}");
            return commands;
        }
        
        foreach (var definition in unequipCommandDefinitions)
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
}
