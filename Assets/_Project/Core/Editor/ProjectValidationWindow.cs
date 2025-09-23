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
    /// プロジェクトアーキチE��チャ検証チE�Eル
    /// Unity 6プロジェクト�EイベントシスチE��、コマンドシスチE��、アセンブリ構造の整合性をチェチE��
    /// 
    /// 主な機�E�E�E    /// - イベント接続�E検証�E�孤立リスナ�E、未使用イベント�E発見！E    /// - コマンドシスチE��の構�E検証�E�Envoker, Poolサービスの確認！E    /// - アセンブリ定義ファイルの存在チェチE��
    /// - SerializeReferenceの使用状況監要E    /// - プロジェクトフォルダ構造の検証
    /// - 自動修正機�E�E�フォルダ作�E等！E    /// - 詳細レポ�Eト�E生�Eとエクスポ�EチE    /// 
    /// 使用シーン�E�E    /// - プロジェクト�E品質管琁E    /// - 新メンバ�Eのオンボ�EチE��ング時�E絁E��チェチE��
    /// - CI/CDパイプラインでの自動品質ゲーチE    /// - リファクタリング後�E整合性検証
    /// 
    /// アクセス方法：Unity メニュー > asterivo.Unity60/Tools/Project Validation
    /// </summary>
    public class ProjectValidationWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool showDetails = true;
        private string validationReport = "";
        private bool hasRunValidation = false;
        
        /// <summary>
        /// プロジェクト検証ウィンドウを表示
        /// Unityメニューから呼び出されるエチE��タ拡張メニューアイチE��
        /// </summary>
        /// <remarks>
        /// ウィンドウの最小サイズは500x400に設定され、E        /// 匁E��皁E��プロジェクト検証とレポ�Eト機�Eを提供します、E        /// </remarks>
        [MenuItem("asterivo.Unity60/Tools/Project Validation")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectValidationWindow>("Project Validation");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        /// <summary>
        /// ウィンドウのGUI描画処琁E        /// タイトル、ツールバ�E、検証結果表示を頁E��描画
        /// </summary>
        /// <remarks>
        /// GUIの構�E頁E��！E        /// 1. ヘッダー�E��EロジェクトアーキチE��チャ検証タイトル�E�E        /// 2. チE�Eルバ�E�E�検証実行、�E動修正、レポ�Eト生成�Eタン�E�E        /// 3. 検証結果�E�スクロール可能なチE��ストエリア�E�E        /// </remarks>
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
            
            if (GUILayout.Button("🔍 Run Full Validation", EditorStyles.toolbarButton))
            {
                RunValidation();
            }
            
            if (GUILayout.Button("🔧 Auto-Fix Issues", EditorStyles.toolbarButton))
            {
                AutoFixIssues();
            }
            
            if (GUILayout.Button("📊 Generate Report", EditorStyles.toolbarButton))
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
        /// 匁E��皁E��プロジェクト検証の実衁E        /// イベント、コマンド、アセンブリ、フォルダ構造等を頁E��チェチE��
        /// </summary>
        /// <remarks>
        /// 検証頁E���E�E        /// 1. イベント接続検証�E�EalidateEventConnections�E�E        /// 2. コマンド定義検証�E�EalidateCommandDefinitions�E�E        /// 3. アセンブリ構造検証�E�EalidateAssemblyStructure�E�E        /// 4. SerializeReference使用状況チェチE���E�EalidateSerializeReferenceUsage�E�E        /// 5. ScriptableObjectアセチE��検証�E�EalidateScriptableObjectAssets�E�E        /// 6. プロジェクト構造検証�E�EalidateProjectStructure�E�E        /// 
        /// 吁E��証結果はvalidationReportに記録され、GUIで表示されます、E        /// </remarks>
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
            AddToReport("🔍 VALIDATING EVENT CONNECTIONS");
            
            var listeners = FindObjectsByType<GameEventListener>(FindObjectsSortMode.None);
            var orphanedListeners = listeners.Where(l => l.Event == null).ToArray();
            var totalListeners = listeners.Length;
            
            if (orphanedListeners.Length > 0)
            {
                AddToReport($"❁EFound {orphanedListeners.Length}/{totalListeners} GameEventListeners without assigned Events:");
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
                AddToReport($"✁EAll {totalListeners} GameEventListeners have valid Event references");
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
                        AddToReport("⚠�E�EEvents with duplicate listener priorities:");
                    }
                    AddToReport($"  - {group.Key.name}: {string.Join(", ", duplicates.Select(g => $"Priority {g.Key} ({g.Count()}x)"))}");
                }
            }
            
            if (duplicatePriorityEvents == 0)
            {
                AddToReport("✁ENo duplicate listener priorities found");
            }
            
            AddToReport("");
        }
        
        private void ValidateCommandDefinitions()
        {
            AddToReport("🔍 VALIDATING COMMAND DEFINITIONS");
            
            // Find all ICommandDefinition implementations
            var commandDefinitionTypes = GetAllTypesImplementing<ICommandDefinition>();
            AddToReport($"Found {commandDefinitionTypes.Count} command definition types:");
            
            foreach (var type in commandDefinitionTypes)
            {
                AddToReport($"  ✁E{type.Name}");
            }
            
            // Find CommandInvokers
            var invokers = FindObjectsByType<CommandInvoker>(FindObjectsSortMode.None);
            if (invokers.Length == 0)
            {
                AddToReport("❁ENo CommandInvoker found in scene");
            }
            else
            {
                AddToReport($"✁EFound {invokers.Length} CommandInvoker(s)");
                foreach (var invoker in invokers)
                {
                    AddToReport($"  - {invoker.gameObject.name} [{invoker.gameObject.scene.name}]");
                }
            }
            
            // Find CommandPoolServices
            var poolServices = FindObjectsByType<CommandPoolService>(FindObjectsSortMode.None);
            if (poolServices.Length == 0)
            {
                AddToReport("⚠�E�ENo CommandPoolService found in scene - command pooling may not be active");
            }
            else
            {
                AddToReport($"✁EFound {poolServices.Length} CommandPoolService(s)");
                foreach (var service in poolServices)
                {
                    AddToReport($"  - {service.gameObject.name} [{service.gameObject.scene.name}]");
                }
            }
            
            AddToReport("");
        }
        
        private void ValidateAssemblyStructure()
        {
            AddToReport("🔍 VALIDATING ASSEMBLY STRUCTURE");
            
            var assemblyDefFiles = Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories);
            
            if (assemblyDefFiles.Length == 0)
            {
                AddToReport("⚠�E�ENo assembly definition files found");
            }
            else
            {
                AddToReport($"✁EFound {assemblyDefFiles.Length} assembly definition files:");
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
            AddToReport("🔍 VALIDATING SERIALIZEREFERENCE USAGE");
            
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
                    AddToReport($"  ✁ESerializeReference found in: {Path.GetFileName(file)}");
                }
                
                if (content.Contains("class ItemData"))
                {
                    itemDataFiles++;
                }
            }
            
            if (serializeReferenceCount == 0)
            {
                AddToReport("⚠�E�ENo SerializeReference attributes found - hybrid architecture may not be implemented");
            }
            else
            {
                AddToReport($"✁EFound {serializeReferenceCount} files using SerializeReference");
            }
            
            AddToReport("");
        }
        
        private void ValidateScriptableObjectAssets()
        {
            AddToReport("🔍 VALIDATING SCRIPTABLEOBJECT ASSETS");
            
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
                AddToReport($"⚠�E�E{unusedEvents} GameEvent assets appear to be unused");
            }
            else
            {
                AddToReport("✁EAll GameEvent assets appear to be in use");
            }
            
            AddToReport("");
        }
        
        private void ValidateProjectStructure()
        {
            AddToReport("🔍 VALIDATING PROJECT STRUCTURE");
            
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
                    AddToReport($"  ✁E{folder}");
                }
                else
                {
                    AddToReport($"  ❁EMissing: {folder}");
                    missingFolders++;
                }
            }
            
            if (missingFolders == 0)
            {
                AddToReport("✁EProject structure follows recommended layout");
            }
            else
            {
                AddToReport($"⚠�E�E{missingFolders} recommended folders are missing");
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
            AddToReport("🔧 ATTEMPTING AUTO-FIXES");
            
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
                    AddToReport($"  ✁ECreated folder: {folder}");
                }
            }
            
            if (foldersCreated > 0)
            {
                AssetDatabase.Refresh();
                AddToReport($"✁EAuto-created {foldersCreated} missing folders");
            }
            else
            {
                AddToReport("✁ENo folder auto-fixes needed");
            }
            
            AddToReport("");
        }
        
        private void GenerateReport()
        {
            var path = EditorUtility.SaveFilePanel("Save Validation Report", "", "ProjectValidation_Report.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, validationReport);
                AddToReport($"📊 Report saved to: {path}");
                
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