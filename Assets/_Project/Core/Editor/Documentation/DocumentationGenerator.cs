using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Commands;
using Debug = UnityEngine.Debug;


namespace asterivo.Unity60.Core.Editor.Documentation
{
    /// <summary>
    /// 繝励Ο繧ｸ繧ｧ繧ｯ繝医ラ繧ｭ繝･繝｡繝ｳ繝医ｒ閾ｪ蜍慕函謌舌☆繧九お繝・ぅ繧ｿ繝・・繝ｫ
    /// </summary>
    public static class DocumentationGenerator
    {
        private const string DOC_OUTPUT_PATH = "Assets/_Project/Docs/Generated/";

        [MenuItem("Project/Documentation/Generate All Documentation", priority = 1)]
        public static void GenerateAllDocumentation()
        {
            EnsureOutputDirectoryExists();
            
            GenerateEventFlowDiagram();
            GenerateCommandListDoc();
            GenerateComponentDependencyGraph();
            GenerateSetupValidationReport();
            
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("笨・All documentation generated successfully!");
        }

        [MenuItem("Project/Documentation/1. Generate Event Flow Diagram")]
        public static void GenerateEventFlowDiagram()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Event Flow Diagram");
            sb.AppendLine($"*Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
            sb.AppendLine();
            
            // 繧､繝吶Φ繝井ｸ隕ｧ繧貞叙蠕・            var eventTypes = GetEventTypes();
            
            sb.AppendLine("## Event Types Overview");
            sb.AppendLine();
            
            foreach (var eventType in eventTypes)
            {
                sb.AppendLine($"### {eventType.Name}");
                sb.AppendLine($"- **Namespace**: `{eventType.Namespace}`");
                sb.AppendLine($"- **Assembly**: `{eventType.Assembly.GetName().Name}`");
                
                // 邯呎価髢｢菫・                if (eventType.BaseType != null && eventType.BaseType != typeof(object))
                {
                    sb.AppendLine($"- **Inherits**: `{eventType.BaseType.Name}`");
                }
                
                // 繧ｸ繧ｧ繝阪Μ繝・け蝙九・蝣ｴ蜷・                if (eventType.IsGenericType)
                {
                    var genericArgs = eventType.GetGenericArguments();
                    sb.AppendLine($"- **Generic Args**: `{string.Join(", ", genericArgs.Select(t => t.Name))}`");
                }
                
                sb.AppendLine();
            }
            
            // Mermaid蝗ｳ蠑冗函謌・            sb.AppendLine("## Event Flow Mermaid Diagram");
            sb.AppendLine();
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph TD");
            
            foreach (var eventType in eventTypes)
            {
                string eventName = eventType.Name.Replace("Event", "");
                sb.AppendLine($"    {eventName}[{eventName}]");
                
                if (eventType.BaseType != null && eventType.BaseType.Name.Contains("GameEvent"))
                {
                    sb.AppendLine($"    GameEvent --> {eventName}");
                }
            }
            
            sb.AppendLine("```");
            
            WriteToFile("EventFlowDiagram.md", sb.ToString());
            UnityEngine.Debug.Log("笨・Event Flow Diagram generated");
        }

        [MenuItem("Project/Documentation/2. Generate Command List Documentation")]
        public static void GenerateCommandListDoc()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Command Definitions Documentation");
            sb.AppendLine($"*Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
            sb.AppendLine();
            
            // Command Definition荳隕ｧ繧貞叙蠕・            var commandTypes = GetCommandDefinitionTypes();
            
            sb.AppendLine("## Command Categories");
            sb.AppendLine();
            
            var categoryGroups = commandTypes.GroupBy(GetCommandCategory);
            
            foreach (var group in categoryGroups.OrderBy(g => g.Key))
            {
                sb.AppendLine($"### {group.Key}");
                sb.AppendLine();
                
                foreach (var commandType in group.OrderBy(t => t.Name))
                {
                    sb.AppendLine($"#### {commandType.Name}");
                    
                    // XML繝峨く繝･繝｡繝ｳ繝医°繧芽ｪｬ譏弱ｒ蜿門ｾ暦ｼ育ｰ｡譏鍋沿・・                    var summaryAttribute = commandType.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
                    if (summaryAttribute != null)
                    {
                        sb.AppendLine($"**Description**: {summaryAttribute.Description}");
                        sb.AppendLine();
                    }
                    
                    // 繝代ヶ繝ｪ繝・け繝輔ぅ繝ｼ繝ｫ繝峨→繝励Ο繝代ユ繧｣
                    var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var properties = commandType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    
                    if (fields.Length > 0 || properties.Length > 0)
                    {
                        sb.AppendLine("**Parameters**:");
                        
                        foreach (var field in fields)
                        {
                            sb.AppendLine($"- `{field.Name}`: {GetTypeName(field.FieldType)}");
                        }
                        
                        foreach (var property in properties)
                        {
                            if (property.CanRead && property.GetMethod.IsPublic)
                            {
                                sb.AppendLine($"- `{property.Name}`: {GetTypeName(property.PropertyType)}");
                            }
                        }
                        
                        sb.AppendLine();
                    }
                    
                    // 繝｡繧ｽ繝・ラ諠・ｱ
                    var methods = commandType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (methods.Length > 0)
                    {
                        sb.AppendLine("**Methods**:");
                        foreach (var method in methods.Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")))
                        {
                            var parameters = method.GetParameters();
                            var parameterNames = parameters.Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}");
                            sb.AppendLine($"- `{method.Name}({string.Join(", ", parameterNames)})`");
                        }
                        sb.AppendLine();
                    }
                }
            }
            
            WriteToFile("CommandListDocumentation.md", sb.ToString());
            UnityEngine.Debug.Log("笨・Command List Documentation generated");
        }

        [MenuItem("Project/Documentation/3. Generate Component Dependency Graph")]
        public static void GenerateComponentDependencyGraph()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Component Dependency Graph");
            sb.AppendLine($"*Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
            sb.AppendLine();
            
            // MonoBehaviour繧ｯ繝ｩ繧ｹ縺ｮ萓晏ｭ倬未菫ゅｒ蛻・梵
            var monoBehaviourTypes = GetMonoBehaviourTypes();
            
            sb.AppendLine("## Core Components");
            sb.AppendLine();
            
            foreach (var type in monoBehaviourTypes.Where(t => t.Namespace?.Contains("asterivo.Unity60.Core") == true))
            {
                sb.AppendLine($"### {type.Name}");
                sb.AppendLine($"- **Namespace**: `{type.Namespace}`");
                
                // RequireComponent螻樊ｧ繧偵メ繧ｧ繝・け
                var requireAttributes = type.GetCustomAttributes<RequireComponent>();
                if (requireAttributes.Any())
                {
                    sb.AppendLine("- **Required Components**:");
                    foreach (var attr in requireAttributes)
                    {
                        if (attr.m_Type0 != null) sb.AppendLine($"  - `{attr.m_Type0.Name}`");
                        if (attr.m_Type1 != null) sb.AppendLine($"  - `{attr.m_Type1.Name}`");
                        if (attr.m_Type2 != null) sb.AppendLine($"  - `{attr.m_Type2.Name}`");
                    }
                }
                
                // 繝輔ぅ繝ｼ繝ｫ繝峨・萓晏ｭ倬未菫・                var componentFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => typeof(Component).IsAssignableFrom(f.FieldType))
                    .ToList();
                
                if (componentFields.Any())
                {
                    sb.AppendLine("- **Component Dependencies**:");
                    foreach (var field in componentFields)
                    {
                        sb.AppendLine($"  - `{field.Name}`: {GetTypeName(field.FieldType)}");
                    }
                }
                
                sb.AppendLine();
            }
            
            // Feature Components
            sb.AppendLine("## Feature Components");
            sb.AppendLine();
            
            foreach (var type in monoBehaviourTypes.Where(t => t.Namespace?.Contains("asterivo.Unity60") == true && !t.Namespace.Contains("Core")))
            {
                sb.AppendLine($"### {type.Name}");
                sb.AppendLine($"- **Namespace**: `{type.Namespace}`");
                
                var requireAttributes = type.GetCustomAttributes<RequireComponent>();
                if (requireAttributes.Any())
                {
                    sb.AppendLine("- **Required Components**:");
                    foreach (var attr in requireAttributes)
                    {
                        if (attr.m_Type0 != null) sb.AppendLine($"  - `{attr.m_Type0.Name}`");
                        if (attr.m_Type1 != null) sb.AppendLine($"  - `{attr.m_Type1.Name}`");
                        if (attr.m_Type2 != null) sb.AppendLine($"  - `{attr.m_Type2.Name}`");
                    }
                }
                
                sb.AppendLine();
            }
            
            WriteToFile("ComponentDependencyGraph.md", sb.ToString());
            UnityEngine.Debug.Log("笨・Component Dependency Graph generated");
        }

        [MenuItem("Project/Documentation/4. Generate Setup Validation Report")]
        public static void GenerateSetupValidationReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Project Setup Validation Report");
            sb.AppendLine($"*Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
            sb.AppendLine();
            
            int issues = 0;
            int warnings = 0;
            int successes = 0;
            
            sb.AppendLine("## Project Structure Validation");
            sb.AppendLine();
            
            // 蠢・ｦ√↑繝輔か繝ｫ繝縺ｮ讀懆ｨｼ
            string[] requiredFolders = {
                "Assets/_Project/Core",
                "Assets/_Project/Features",
                "Assets/_Project/Scenes",
                "Assets/_Project/Docs",
                "Assets/_Project/Core/Commands/Definitions",
                "Assets/_Project/Core/Prefabs/Templates",
                "Assets/_Project/Core/ScriptableObjects/Events/Core"
            };
            
            sb.AppendLine("### Directory Structure");
            foreach (string folder in requiredFolders)
            {
                bool exists = AssetDatabase.IsValidFolder(folder);
                if (exists)
                {
                    sb.AppendLine($"笨・`{folder}`");
                    successes++;
                }
                else
                {
                    sb.AppendLine($"笶・`{folder}` - Missing");
                    issues++;
                }
            }
            sb.AppendLine();
            
            // 蠢・ｦ√↑繝励Μ繝輔ぃ繝悶・讀懆ｨｼ
            sb.AppendLine("### Required Prefabs");
            string[] requiredPrefabs = { "DefaultPlayer", "GameManager", "UICanvas", "AudioManager", "DefaultGround", "DefaultCamera", "DefaultLighting", "SpawnPoint" };
            
            foreach (string prefabName in requiredPrefabs)
            {
                string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    sb.AppendLine($"笨・`{prefabName}.prefab` - Found at `{path}`");
                    successes++;
                }
                else
                {
                    sb.AppendLine($"笞・・`{prefabName}.prefab` - Not found");
                    warnings++;
                }
            }
            sb.AppendLine();
            
            // Command Definitions讀懆ｨｼ
            sb.AppendLine("### Command Definitions");
            string[] requiredCommands = { "MoveCommandDefinition", "JumpCommandDefinition", "AttackCommandDefinition", "DamageCommandDefinition", "HealCommandDefinition" };
            
            foreach (string commandName in requiredCommands)
            {
                string[] guids = AssetDatabase.FindAssets($"{commandName} t:Script");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    sb.AppendLine($"笨・`{commandName}.cs` - Found at `{path}`");
                    successes++;
                }
                else
                {
                    sb.AppendLine($"笶・`{commandName}.cs` - Missing");
                    issues++;
                }
            }
            sb.AppendLine();
            
            // 繝励Ο繧ｸ繧ｧ繧ｯ繝郁ｨｭ螳壹・讀懆ｨｼ
            sb.AppendLine("### Project Settings");
            
            sb.AppendLine($"- **Color Space**: {PlayerSettings.colorSpace} {(PlayerSettings.colorSpace == ColorSpace.Linear ? "笨・ : "笞・・(Recommended: Linear)")}");

// Unity 6 compatible approach with NamedBuildTarget
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            sb.AppendLine($"- **Scripting Backend**: {PlayerSettings.GetScriptingBackend(namedBuildTarget)}");
            sb.AppendLine($"- **API Compatibility**: {PlayerSettings.GetApiCompatibilityLevel(namedBuildTarget)}");
            
            if (PlayerSettings.colorSpace == ColorSpace.Linear) successes++; else warnings++;
            
            sb.AppendLine();
            
            // 繧ｵ繝槭Μ繝ｼ
            sb.AppendLine("## Validation Summary");
            sb.AppendLine();
            sb.AppendLine($"- 笨・**Successes**: {successes}");
            sb.AppendLine($"- 笞・・**Warnings**: {warnings}");
            sb.AppendLine($"- 笶・**Issues**: {issues}");
            sb.AppendLine();
            
            if (issues == 0 && warnings == 0)
            {
                sb.AppendLine("脂 **Project setup is perfect!**");
            }
            else if (issues == 0)
            {
                sb.AppendLine("総 **Project setup is good with minor recommendations.**");
            }
            else
            {
                sb.AppendLine("笞・・**Project setup has issues that need attention.**");
            }
            
            WriteToFile("SetupValidationReport.md", sb.ToString());
            UnityEngine.Debug.Log($"笨・Setup Validation Report generated - {successes} successes, {warnings} warnings, {issues} issues");
        }

        // 繝倥Ν繝代・繝｡繧ｽ繝・ラ
        private static void EnsureOutputDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Docs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Docs");
            if (!AssetDatabase.IsValidFolder(DOC_OUTPUT_PATH))
                AssetDatabase.CreateFolder("Assets/_Project/Docs", "Generated");
        }

        private static void WriteToFile(string fileName, string content)
        {
            string fullPath = DOC_OUTPUT_PATH + fileName;
            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }

        private static System.Type[] GetEventTypes()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); }
                    catch { return new System.Type[0]; }
                })
                .Where(type => type.Name.EndsWith("Event") && !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToArray();
        }

        private static System.Type[] GetCommandDefinitionTypes()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); }
                    catch { return new System.Type[0]; }
                })
                .Where(type => type.Name.EndsWith("CommandDefinition") && !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToArray();
        }

        private static System.Type[] GetMonoBehaviourTypes()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); }
                    catch { return new System.Type[0]; }
                })
                .Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsAbstract)
                .OrderBy(type => type.Name)
                .ToArray();
        }

        private static string GetCommandCategory(System.Type commandType)
        {
            string name = commandType.Name.Replace("CommandDefinition", "");
            
            if (name.Contains("Move") || name.Contains("Jump") || name.Contains("Sprint") || name.Contains("Crouch"))
                return "Movement Commands";
            if (name.Contains("Attack") || name.Contains("Defend") || name.Contains("Damage") || name.Contains("Heal"))
                return "Combat Commands";
            if (name.Contains("Interact") || name.Contains("Pickup") || name.Contains("Use"))
                return "Interaction Commands";
            if (name.Contains("Save") || name.Contains("Load") || name.Contains("Pause") || name.Contains("Quit"))
                return "System Commands";
            
            return "Other Commands";
        }

        private static string GetTypeName(System.Type type)
        {
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                var baseName = type.Name.Split('`')[0];
                return $"{baseName}<{string.Join(", ", genericArgs.Select(GetTypeName))}>";
            }
            
            return type.Name;
        }
    }
}
