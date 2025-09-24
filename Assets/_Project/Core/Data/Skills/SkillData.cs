using System.Collections.Generic;
using UnityEngine;
// // using asterivo.Unity60.Core.Commands; // Removed to avoid circular dependency

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ScriptableObject for skill definitions using polymorphic serialization
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "asterivo.Unity60/Skills/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Skill Information")]
        [SerializeField] private string skillName = "New Skill";
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private Sprite icon;
        
        [Header("Skill Properties")]
        [SerializeField] private float cooldownTime = 1.0f;
        [SerializeField] private float manaCost = 10f;
        [SerializeField] private float castTime = 0f;
        
        [Header("Command Definitions")]
        [SerializeReference] 
        [Tooltip("List of command definitions that will be executed when this skill is used")]
        private List<object> commandDefinitions = new List<object>();
        
        [Header("Requirements")]
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private List<SkillData> prerequisiteSkills = new List<SkillData>();
        
        // Runtime data
        private float lastUsedTime = -999f;
        
        /// <summary>
        /// Checks if the skill can be used
        /// </summary>
        public bool CanUse(object context = null)
        {
            // Check cooldown
            if (Time.time - lastUsedTime < cooldownTime)
                return false;
                
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
        /// Uses the skill by creating and returning commands
        /// </summary>
        public List<object> Use(object context = null)
        {
            var commands = new List<object>();
            
            if (!CanUse(context))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Cannot use skill: {skillName}");
#endif
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
            
            lastUsedTime = Time.time;
            return commands;
        }
        
        // Properties
        public string SkillName => skillName;
        public string Description => description;
        public Sprite Icon => icon;
        public float CooldownTime => cooldownTime;
        public float ManaCost => manaCost;
        public float CastTime => castTime;
        public int RequiredLevel => requiredLevel;
        public bool IsOnCooldown => Time.time - lastUsedTime < cooldownTime;
        public float CooldownRemaining => Mathf.Max(0, cooldownTime - (Time.time - lastUsedTime));
        
        /// <summary>
        /// Resets the cooldown
        /// </summary>
        public void ResetCooldown()
        {
            lastUsedTime = -999f;
        }
    }
}