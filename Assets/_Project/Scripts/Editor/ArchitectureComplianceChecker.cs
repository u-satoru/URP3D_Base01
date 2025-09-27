using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// アーキテクチャ準拠性チェックツール
    /// namespace規約とGameObject.Find()使用箇所を検出・報告
    /// </summary>
    public class ArchitectureComplianceChecker : EditorWindow
    {
        [MenuItem("Tools/Architecture/Compliance Checker")]
        public static void ShowWindow()
        {
            GetWindow<ArchitectureComplianceChecker>("Architecture Compliance Checker");
        }

        private Vector2 scrollPosition;
        private ComplianceReport lastReport;
        private bool showDetails = false;

        void OnGUI()
        {
            GUILayout.Label("Architecture Compliance Checker", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Run Compliance Check", GUILayout.Height(30)))
            {
                RunComplianceCheck();
            }

            GUILayout.Space(10);

            if (lastReport != null)
            {
                DisplayReport();
            }
        }

        private void RunComplianceCheck()
        {
            lastReport = new ComplianceReport();

            // namespace規約チェック
            CheckNamespaceCompliance();

            // GameObject.Find()使用箇所チェック
            CheckGameObjectFindUsage();

            // Core→Features参照チェック
            CheckLayerDependencies();

            UnityEngine.Debug.Log($"[ArchitectureComplianceChecker] Check completed. Issues found: {lastReport.TotalIssues}");
        }

        private void CheckNamespaceCompliance()
        {
            var csFiles = Directory.GetFiles("Assets/_Project", "*.cs", SearchOption.AllDirectories)
                                  .Where(f => !f.Contains("Editor") || f.Contains("_Project/Scripts/Editor"))
                                  .ToArray();

            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    // namespace _Project.* 検出
                    if (Regex.IsMatch(line, @"namespace\s+_Project\."))
                    {
                        lastReport.NamespaceViolations.Add(new ComplianceIssue
                        {
                            File = file,
                            Line = i + 1,
                            Content = line,
                            Severity = IssueSeverity.Error,
                            Description = "Prohibited _Project.* namespace usage"
                        });
                    }

                    // using _Project.* 検出
                    if (Regex.IsMatch(line, @"using\s+_Project\."))
                    {
                        lastReport.NamespaceViolations.Add(new ComplianceIssue
                        {
                            File = file,
                            Line = i + 1,
                            Content = line,
                            Severity = IssueSeverity.Warning,
                            Description = "Prohibited _Project.* using statement"
                        });
                    }
                }
            }
        }

        private void CheckGameObjectFindUsage()
        {
            var csFiles = Directory.GetFiles("Assets/_Project", "*.cs", SearchOption.AllDirectories)
                                  .Where(f => !f.Contains("Editor") && !f.Contains("Tests/Templates"))
                                  .ToArray();

            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    // GameObject.Find系メソッド検出
                    if (Regex.IsMatch(line, @"GameObject\.Find"))
                    {
                        var severity = DetermineGameObjectFindSeverity(file, line);
                        var recommendation = GetGameObjectFindRecommendation(line);

                        lastReport.GameObjectFindUsages.Add(new ComplianceIssue
                        {
                            File = file,
                            Line = i + 1,
                            Content = line,
                            Severity = severity,
                            Description = $"GameObject.Find() usage detected. {recommendation}"
                        });
                    }
                }
            }
        }

        private void CheckLayerDependencies()
        {
            var coreFiles = Directory.GetFiles("Assets/_Project/Core", "*.cs", SearchOption.AllDirectories);

            foreach (var file in coreFiles)
            {
                var content = File.ReadAllText(file);
                var lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    // Core→Features参照検出
                    if (Regex.IsMatch(line, @"using\s+asterivo\.Unity60\.Features"))
                    {
                        lastReport.LayerViolations.Add(new ComplianceIssue
                        {
                            File = file,
                            Line = i + 1,
                            Content = line,
                            Severity = IssueSeverity.Error,
                            Description = "Prohibited Core→Features layer dependency"
                        });
                    }
                }
            }
        }

        private IssueSeverity DetermineGameObjectFindSeverity(string file, string line)
        {
            // Update()やフレーム毎実行での使用は高リスク
            if (file.Contains("Update") || line.Contains("Update()"))
                return IssueSeverity.Error;

            // UI・サービス系での使用は中リスク
            if (file.Contains("UI") || file.Contains("Service") || file.Contains("Manager"))
                return IssueSeverity.Warning;

            // その他は低リスク
            return IssueSeverity.Info;
        }

        private string GetGameObjectFindRecommendation(string line)
        {
            if (line.Contains("FindGameObjectWithTag(\"Player\")"))
                return "Consider using SerializeField reference or ServiceLocator.GetService<PlayerController>()";
            
            if (line.Contains("FindGameObjectWithTag"))
                return "Consider using SerializeField reference for fixed objects";
            
            if (line.Contains("FindGameObjectsWithTag"))
                return "Consider caching results or using manager pattern";

            return "Consider using direct references or ServiceLocator pattern";
        }

        private void DisplayReport()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // サマリー表示
            GUILayout.Label("Compliance Summary", EditorStyles.boldLabel);
            
            DisplaySummaryItem("Namespace Violations", lastReport.NamespaceViolations.Count, Color.red);
            DisplaySummaryItem("GameObject.Find() Usages", lastReport.GameObjectFindUsages.Count, Color.yellow);
            DisplaySummaryItem("Layer Violations", lastReport.LayerViolations.Count, Color.red);
            DisplaySummaryItem("Total Issues", lastReport.TotalIssues, lastReport.TotalIssues == 0 ? Color.green : Color.red);

            GUILayout.Space(10);

            // 詳細表示トグル
            showDetails = EditorGUILayout.Toggle("Show Details", showDetails);

            if (showDetails)
            {
                DisplayIssueCategory("Namespace Violations", lastReport.NamespaceViolations);
                DisplayIssueCategory("GameObject.Find() Usages", lastReport.GameObjectFindUsages);
                DisplayIssueCategory("Layer Violations", lastReport.LayerViolations);
            }

            // アクションボタン
            GUILayout.Space(10);
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Compliance Report"))
            {
                GenerateComplianceReport();
            }

            if (GUILayout.Button("Open Architecture Guidelines"))
            {
                var path = "Assets/_Project/Docs/Architecture_Guidelines.md";
                if (File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DisplaySummaryItem(string label, int count, Color color)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(200));
            
            var previousColor = GUI.color;
            GUI.color = color;
            GUILayout.Label(count.ToString(), EditorStyles.boldLabel);
            GUI.color = previousColor;
            
            GUILayout.EndHorizontal();
        }

        private void DisplayIssueCategory(string categoryName, List<ComplianceIssue> issues)
        {
            if (issues.Count == 0) return;

            GUILayout.Space(10);
            GUILayout.Label(categoryName, EditorStyles.boldLabel);

            foreach (var issue in issues)
            {
                var color = GetSeverityColor(issue.Severity);
                var previousColor = GUI.color;
                GUI.color = color;

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUI.color = previousColor;

                GUILayout.Label($"[{issue.Severity}] {issue.Description}", EditorStyles.wordWrappedLabel);
                GUILayout.Label($"File: {Path.GetFileName(issue.File)}:{issue.Line}", EditorStyles.miniLabel);
                GUILayout.Label($"Code: {issue.Content}", EditorStyles.miniLabel);

                if (GUILayout.Button("Open File", GUILayout.Width(80)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.File);
                    AssetDatabase.OpenAsset(asset, issue.Line);
                }

                GUILayout.EndVertical();
            }
        }

        private Color GetSeverityColor(IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Error: return Color.red;
                case IssueSeverity.Warning: return Color.yellow;
                case IssueSeverity.Info: return Color.cyan;
                default: return Color.white;
            }
        }

        private void GenerateComplianceReport()
        {
            var reportPath = "Assets/_Project/Tests/Results/Architecture_Compliance_Report.md";
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            var report = GenerateMarkdownReport();
            File.WriteAllText(reportPath, report);

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Compliance report generated: {reportPath}");
        }

        private string GenerateMarkdownReport()
        {
            var report = $@"# Architecture Compliance Report

**Generated**: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}  
**Total Issues**: {lastReport.TotalIssues}

## Summary

| Category | Count | Status |
|----------|-------|--------|
| Namespace Violations | {lastReport.NamespaceViolations.Count} | {(lastReport.NamespaceViolations.Count == 0 ? "✅" : "❌")} |
| GameObject.Find() Usages | {lastReport.GameObjectFindUsages.Count} | {(lastReport.GameObjectFindUsages.Count == 0 ? "✅" : "⚠️")} |
| Layer Violations | {lastReport.LayerViolations.Count} | {(lastReport.LayerViolations.Count == 0 ? "✅" : "❌")} |

## Detailed Issues

";

            report += GenerateIssueSection("Namespace Violations", lastReport.NamespaceViolations);
            report += GenerateIssueSection("GameObject.Find() Usages", lastReport.GameObjectFindUsages);
            report += GenerateIssueSection("Layer Violations", lastReport.LayerViolations);

            report += @"
## Recommendations

1. **Namespace Violations**: Replace `_Project.*` with `asterivo.Unity60.*`
2. **GameObject.Find() Usages**: Use SerializeField references or ServiceLocator pattern
3. **Layer Violations**: Avoid Core→Features dependencies, use events or interfaces

## Next Steps

- [ ] Fix all Error severity issues
- [ ] Review and optimize Warning severity issues
- [ ] Update architecture guidelines if needed
- [ ] Schedule next compliance check
";

            return report;
        }

        private string GenerateIssueSection(string title, List<ComplianceIssue> issues)
        {
            if (issues.Count == 0) return $"### {title}\n\nNo issues found. ✅\n\n";

            var section = $"### {title}\n\n";
            foreach (var issue in issues)
            {
                section += $"- **[{issue.Severity}]** `{Path.GetFileName(issue.File)}:{issue.Line}` - {issue.Description}\n";
                section += $"  ```csharp\n  {issue.Content.Trim()}\n  ```\n\n";
            }
            return section;
        }
    }

    [System.Serializable]
    public class ComplianceReport
    {
        public List<ComplianceIssue> NamespaceViolations = new List<ComplianceIssue>();
        public List<ComplianceIssue> GameObjectFindUsages = new List<ComplianceIssue>();
        public List<ComplianceIssue> LayerViolations = new List<ComplianceIssue>();

        public int TotalIssues => NamespaceViolations.Count + GameObjectFindUsages.Count + LayerViolations.Count;
    }

    [System.Serializable]
    public class ComplianceIssue
    {
        public string File;
        public int Line;
        public string Content;
        public IssueSeverity Severity;
        public string Description;
    }

    public enum IssueSeverity
    {
        Info,
        Warning, 
        Error
    }
}
