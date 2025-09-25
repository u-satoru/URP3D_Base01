using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// 繝励Ο繧ｸ繧ｧ繧ｯ繝医い繝ｼ繧ｭ繝・け繝√Ε讀懆ｨｼ繝・・繝ｫ
    /// Unity 6繝励Ο繧ｸ繧ｧ繧ｯ繝医・繧､繝吶Φ繝医す繧ｹ繝・Β縲√さ繝槭Φ繝峨す繧ｹ繝・Β縲√い繧ｻ繝ｳ繝悶Μ讒矩縺ｮ謨ｴ蜷域ｧ繧偵メ繧ｧ繝・け
    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧､繝吶Φ繝域磁邯壹・讀懆ｨｼ・亥ｭ､遶九Μ繧ｹ繝翫・縲∵悴菴ｿ逕ｨ繧､繝吶Φ繝医・逋ｺ隕具ｼ・    /// - 繧ｳ繝槭Φ繝峨す繧ｹ繝・Β縺ｮ讒区・讀懆ｨｼ・・nvoker, Pool繧ｵ繝ｼ繝薙せ縺ｮ遒ｺ隱搾ｼ・    /// - 繧｢繧ｻ繝ｳ繝悶Μ螳夂ｾｩ繝輔ぃ繧､繝ｫ縺ｮ蟄伜惠繝√ぉ繝・け
    /// - SerializeReference縺ｮ菴ｿ逕ｨ迥ｶ豕∫屮隕・    /// - 繝励Ο繧ｸ繧ｧ繧ｯ繝医ヵ繧ｩ繝ｫ繝讒矩縺ｮ讀懆ｨｼ
    /// - 閾ｪ蜍穂ｿｮ豁｣讖溯・・医ヵ繧ｩ繝ｫ繝菴懈・遲会ｼ・    /// - 隧ｳ邏ｰ繝ｬ繝昴・繝医・逕滓・縺ｨ繧ｨ繧ｯ繧ｹ繝昴・繝・    /// 
    /// 菴ｿ逕ｨ繧ｷ繝ｼ繝ｳ・・    /// - 繝励Ο繧ｸ繧ｧ繧ｯ繝医・蜩∬ｳｪ邂｡逅・    /// - 譁ｰ繝｡繝ｳ繝舌・縺ｮ繧ｪ繝ｳ繝懊・繝・ぅ繝ｳ繧ｰ譎ゅ・邨・ｹ斐メ繧ｧ繝・け
    /// - CI/CD繝代う繝励Λ繧､繝ｳ縺ｧ縺ｮ閾ｪ蜍募刀雉ｪ繧ｲ繝ｼ繝・    /// - 繝ｪ繝輔ぃ繧ｯ繧ｿ繝ｪ繝ｳ繧ｰ蠕後・謨ｴ蜷域ｧ讀懆ｨｼ
    /// 
    /// 繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕包ｼ啅nity 繝｡繝九Η繝ｼ > asterivo.Unity60/Tools/Project Validation
    /// </summary>
    public class ProjectValidationWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showDetails = true;
        private string validationReport = "";
        private bool hasRunValidation = false;
        
        /// <summary>
        /// 繝励Ο繧ｸ繧ｧ繧ｯ繝域､懆ｨｼ繧ｦ繧｣繝ｳ繝峨え繧定｡ｨ遉ｺ
        /// Unity繝｡繝九Η繝ｼ縺九ｉ蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧九お繝・ぅ繧ｿ諡｡蠑ｵ繝｡繝九Η繝ｼ繧｢繧､繝・Β
        /// </summary>
        /// <remarks>
        /// 繧ｦ繧｣繝ｳ繝峨え縺ｮ譛蟆上し繧､繧ｺ縺ｯ500x400縺ｫ險ｭ螳壹＆繧後・        /// 蛹・峡逧・↑繝励Ο繧ｸ繧ｧ繧ｯ繝域､懆ｨｼ縺ｨ繝ｬ繝昴・繝域ｩ溯・繧呈署萓帙＠縺ｾ縺吶・        /// </remarks>
        [MenuItem("asterivo.Unity60/Tools/Project Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectValidationWindow>("Project Validation");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        /// <summary>
        /// 繧ｦ繧｣繝ｳ繝峨え縺ｮGUI謠冗判蜃ｦ逅・        /// 繧ｿ繧､繝医Ν縲√ヤ繝ｼ繝ｫ繝舌・縲∵､懆ｨｼ邨先棡陦ｨ遉ｺ繧帝・ｬ｡謠冗判
        /// </summary>
        /// <remarks>
        /// GUI縺ｮ讒区・鬆・ｺ擾ｼ・        /// 1. 繝倥ャ繝繝ｼ・医・繝ｭ繧ｸ繧ｧ繧ｯ繝医い繝ｼ繧ｭ繝・け繝√Ε讀懆ｨｼ繧ｿ繧､繝医Ν・・        /// 2. 繝・・繝ｫ繝舌・・域､懆ｨｼ螳溯｡後∬・蜍穂ｿｮ豁｣縲√Ξ繝昴・繝育函謌舌・繧ｿ繝ｳ・・        /// 3. 讀懆ｨｼ邨先棡・医せ繧ｯ繝ｭ繝ｼ繝ｫ蜿ｯ閭ｽ縺ｪ繝・く繧ｹ繝医お繝ｪ繧｢・・        /// </remarks>
        void OnGUI()
        {
            EditorGUILayout.LabelField("Unity 6 Project Architecture Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawToolbar();
            DrawValidationResults();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("剥 Run Full Validation", EditorStyles.toolbarButton))
            {
                RunValidation();
            }
            
            if (GUILayout.Button("肌 Auto-Fix Issues", EditorStyles.toolbarButton))
            {
                AutoFixIssues();
            }
            
            if (GUILayout.Button("投 Generate Report", EditorStyles.toolbarButton))
            {
                GenerateReport();
            }
            
            GUILayout.FlexibleSpace();
            
            showDetails = GUILayout.Toggle(showDetails, "Details", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawValidationResults()
        {
            if (!hasRunValidation)
            {
                EditorGUILayout.HelpBox("Click 'Run Full Validation' to check your project architecture", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            if (!string.IsNullOrEmpty(validationReport))
            {
                EditorGUILayout.TextArea(validationReport, EditorStyles.wordWrappedLabel);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 蛹・峡逧・↑繝励Ο繧ｸ繧ｧ繧ｯ繝域､懆ｨｼ縺ｮ螳溯｡・        /// 繧､繝吶Φ繝医√さ繝槭Φ繝峨√い繧ｻ繝ｳ繝悶Μ縲√ヵ繧ｩ繝ｫ繝讒矩遲峨ｒ鬆・ｬ｡繝√ぉ繝・け
        /// </summary>
        /// <remarks>
        /// 讀懆ｨｼ鬆・岼・・        /// 1. 繧､繝吶Φ繝域磁邯壽､懆ｨｼ・・alidateEventConnections・・        /// 2. 繧ｳ繝槭Φ繝牙ｮ夂ｾｩ讀懆ｨｼ・・alidateCommandDefinitions・・        /// 3. 繧｢繧ｻ繝ｳ繝悶Μ讒矩讀懆ｨｼ・・alidateAssemblyStructure・・        /// 4. SerializeReference菴ｿ逕ｨ迥ｶ豕√メ繧ｧ繝・け・・alidateSerializeReferenceUsage・・        /// 5. ScriptableObject繧｢繧ｻ繝・ヨ讀懆ｨｼ・・alidateScriptableObjectAssets・・        /// 6. 繝励Ο繧ｸ繧ｧ繧ｯ繝域ｧ矩讀懆ｨｼ・・alidateProjectStructure・・        /// 
        /// 蜷・､懆ｨｼ邨先棡縺ｯvalidationReport縺ｫ險倬鹸縺輔ｌ縲；UI縺ｧ陦ｨ遉ｺ縺輔ｌ縺ｾ縺吶・        /// </remarks>
        private void RunValidation()
        {
            validationReport = "";
            hasRunValidation = true;
            
            AddToReport("=== PROJECT VALIDATION STARTED ===");
            AddToReport($"Unity Version: {Application.unityVersion}");
            AddToReport($"Platform: {Application.platform}");
            AddToReport($"Time: {System.DateTime.Now}");
            AddToReport("");
            
            ValidateEventConnections();
            ValidateCommandDefinitions();
            ValidateAssemblyStructure();
            ValidateSerializeReferenceUsage();
            ValidateScriptableObjectAssets();
            ValidateProjectStructure();
            
            AddToReport("=== PROJECT VALIDATION COMPLETED ===");
            
            UnityEngine.Debug.Log("Project validation completed. Check the Project Validation window for details.");
        }
        
        private void ValidateEventConnections()
        {
            AddToReport("剥 VALIDATING EVENT CONNECTIONS");
            
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var orphanedListeners = listeners.Where(l => l.Event == null).ToArray();
            var totalListeners = listeners.Length;
            
            if (orphanedListeners.Length > 0)
            {
                AddToReport($"笶・Found {orphanedListeners.Length}/{totalListeners} GameEventListeners without assigned Events:");
                foreach (var listener in orphanedListeners)
                {
                    AddToReport($"  - {listener.gameObject.name} [{listener.gameObject.scene.name}]");
                    if (showDetails)
                    {
                        AddToReport($"    Path: {GetGameObjectPath(listener.gameObject)}");
                    }
                }
            }
            else
            {
                AddToReport($"笨・All {totalListeners} GameEventListeners have valid Event references");
            }
            
            // Check for duplicate priorities
            var eventGroups = listeners.Where(l => l.Event != null)
                                     .GroupBy(l => l.Event);
            
            int duplicatePriorityEvents = 0;
            foreach (var group in eventGroups)
            {
                var priorities = group.Select(l => l.Priority).ToList();
                var duplicates = priorities.GroupBy(p => p).Where(g => g.Count() > 1);
                
                if (duplicates.Any())
                {
                    duplicatePriorityEvents++;
                    if (duplicatePriorityEvents == 1)
                    {
                        AddToReport("笞・・Events with duplicate listener priorities:");
                    }
                    AddToReport($"  - {group.Key.name}: {string.Join(", ", duplicates.Select(g => $"Priority {g.Key} ({g.Count()}x)"))}");
                }
            }
            
            if (duplicatePriorityEvents == 0)
            {
                AddToReport("笨・No duplicate listener priorities found");
            }
            
            AddToReport("");
        }
        
        private void ValidateCommandDefinitions()
        {
            AddToReport("剥 VALIDATING COMMAND DEFINITIONS");
            
            // Find all ICommandDefinition implementations
            var commandDefinitionTypes = GetAllTypesImplementing<ICommandDefinition>();
            AddToReport($"Found {commandDefinitionTypes.Count} command definition types:");
            
            foreach (var type in commandDefinitionTypes)
            {
                AddToReport($"  笨・{type.Name}");
            }
            
            // Find CommandInvokers
            var invokers = FindObjectsByType<CommandInvoker>(FindObjectsSortMode.None);
            if (invokers.Length == 0)
            {
                AddToReport("笶・No CommandInvoker found in scene");
            }
            else
            {
                AddToReport($"笨・Found {invokers.Length} CommandInvoker(s)");
                foreach (var invoker in invokers)
                {
                    AddToReport($"  - {invoker.gameObject.name} [{invoker.gameObject.scene.name}]");
                }
            }
            
            // Find CommandPoolServices
            var poolServices = FindObjectsByType<CommandPoolService>(FindObjectsSortMode.None);
            if (poolServices.Length == 0)
            {
                AddToReport("笞・・No CommandPoolService found in scene - command pooling may not be active");
            }
            else
            {
                AddToReport($"笨・Found {poolServices.Length} CommandPoolService(s)");
                foreach (var service in poolServices)
                {
                    AddToReport($"  - {service.gameObject.name} [{service.gameObject.scene.name}]");
                }
            }
            
            AddToReport("");
        }
        
        private void ValidateAssemblyStructure()
        {
            AddToReport("剥 VALIDATING ASSEMBLY STRUCTURE");
            
            var assemblyDefFiles = Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories);
            
            if (assemblyDefFiles.Length == 0)
            {
                AddToReport("笞・・No assembly definition files found");
            }
            else
            {
                AddToReport($"笨・Found {assemblyDefFiles.Length} assembly definition files:");
                foreach (var file in assemblyDefFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    AddToReport($"  - {fileName}");
                }
            }
            
            // Check for common assembly issues
            CheckAssemblyReferences(assemblyDefFiles);
            
            AddToReport("");
        }
        
        private void ValidateSerializeReferenceUsage()
        {
            AddToReport("剥 VALIDATING SERIALIZEREFERENCE USAGE");
            
            // This is a simplified check - in a real implementation you'd parse C# files
            var scriptFiles = Directory.GetFiles("Assets/_Project", "*.cs", SearchOption.AllDirectories)
                                     .Where(f => !f.Contains("/Editor/"));
            
            int serializeReferenceCount = 0;
            int itemDataFiles = 0;
            
            foreach (var file in scriptFiles)
            {
                var content = File.ReadAllText(file);
                if (content.Contains("[SerializeReference]"))
                {
                    serializeReferenceCount++;
                    AddToReport($"  笨・SerializeReference found in: {Path.GetFileName(file)}");
                }
                
                if (content.Contains("class ItemData"))
                {
                    itemDataFiles++;
                }
            }
            
            if (serializeReferenceCount == 0)
            {
                AddToReport("笞・・No SerializeReference attributes found - hybrid architecture may not be implemented");
            }
            else
            {
                AddToReport($"笨・Found {serializeReferenceCount} files using SerializeReference");
            }
            
            AddToReport("");
        }
        
        private void ValidateScriptableObjectAssets()
        {
            AddToReport("剥 VALIDATING SCRIPTABLEOBJECT ASSETS");
            
            var gameEvents = Resources.FindObjectsOfTypeAll<GameEvent>();
            var commandEvents = Resources.FindObjectsOfTypeAll<CommandGameEvent>();
            
            AddToReport($"Found {gameEvents.Length} GameEvent assets");
            AddToReport($"Found {commandEvents.Length} CommandGameEvent assets");
            
            // Check for unused events
            var usedEvents = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None)
                           .Where(l => l.Event != null)
                           .Select(l => l.Event)
                           .Distinct()
                           .Count();
            
            var unusedEvents = gameEvents.Length - usedEvents;
            if (unusedEvents > 0)
            {
                AddToReport($"笞・・{unusedEvents} GameEvent assets appear to be unused");
            }
            else
            {
                AddToReport("笨・All GameEvent assets appear to be in use");
            }
            
            AddToReport("");
        }
        
        private void ValidateProjectStructure()
        {
            AddToReport("剥 VALIDATING PROJECT STRUCTURE");
            
            var expectedFolders = new[]
            {
                "Assets/_Project",
                "Assets/_Project/Core",
                "Assets/_Project/Core/Events",
                "Assets/_Project/Core/Commands",
                "Assets/_Project/Core/Data",
                "Assets/_Project/Features",
                "Assets/_Project/Scenes"
            };
            
            int missingFolders = 0;
            foreach (var folder in expectedFolders)
            {
                if (Directory.Exists(folder))
                {
                    AddToReport($"  笨・{folder}");
                }
                else
                {
                    AddToReport($"  笶・Missing: {folder}");
                    missingFolders++;
                }
            }
            
            if (missingFolders == 0)
            {
                AddToReport("笨・Project structure follows recommended layout");
            }
            else
            {
                AddToReport($"笞・・{missingFolders} recommended folders are missing");
            }
            
            AddToReport("");
        }
        
        private void CheckAssemblyReferences(string[] assemblyFiles)
        {
            // Simplified assembly reference check
            foreach (var file in assemblyFiles)
            {
                var content = File.ReadAllText(file);
                var data = JsonUtility.FromJson<AssemblyDefinitionData>(content);
                
                if (data != null && data.references != null)
                {
                    AddToReport($"  {data.name}: {data.references.Length} references");
                }
            }
        }
        
        private void AutoFixIssues()
        {
            AddToReport("肌 ATTEMPTING AUTO-FIXES");
            
            // Auto-fix: Create missing folders
            var expectedFolders = new[]
            {
                "Assets/_Project/Core/Editor",
                "Assets/_Project/ScriptableObjects/Events",
                "Assets/_Project/Prefabs"
            };
            
            int foldersCreated = 0;
            foreach (var folder in expectedFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    foldersCreated++;
                    AddToReport($"  笨・Created folder: {folder}");
                }
            }
            
            if (foldersCreated > 0)
            {
                AssetDatabase.Refresh();
                AddToReport($"笨・Auto-created {foldersCreated} missing folders");
            }
            else
            {
                AddToReport("笨・No folder auto-fixes needed");
            }
            
            AddToReport("");
        }
        
        private void GenerateReport()
        {
            var path = EditorUtility.SaveFilePanel("Save Validation Report", "", "ProjectValidation_Report.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, validationReport);
                AddToReport($"投 Report saved to: {path}");
                
                // Also log to console
                UnityEngine.Debug.Log("=== PROJECT VALIDATION REPORT ===");
                UnityEngine.Debug.Log(validationReport);
            }
        }
        
        private void AddToReport(string line)
        {
            validationReport += line + "\n";
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            var path = obj.name;
            var parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        private List<System.Type> GetAllTypesImplementing<T>()
        {
            var interfaceType = typeof(T);
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();
        }
        
        [System.Serializable]
        private class AssemblyDefinitionData
        {
            public string name;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
        }
    }
}
