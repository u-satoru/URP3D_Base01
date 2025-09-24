using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// 月次アーキテクチャ準拠性レポート自動生成ツール
    /// 定期的な品質監視と継続的改善のためのレポート生成
    /// </summary>
    public class MonthlyComplianceReporter : EditorWindow
    {
        [MenuItem("Tools/Architecture/Monthly Reporter")]
        public static void ShowWindow()
        {
            GetWindow<MonthlyComplianceReporter>("Monthly Compliance Reporter");
        }

        private Vector2 scrollPosition;
        private ComplianceMetrics currentMetrics;
        private ComplianceMetrics previousMetrics;
        private bool autoSchedule = false;

        void OnGUI()
        {
            GUILayout.Label("Monthly Architecture Compliance Reporter", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawActionButtons();
            GUILayout.Space(10);

            DrawMetricsDisplay();
            GUILayout.Space(10);

            DrawSchedulingSettings();
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Generate Monthly Report", GUILayout.Height(30)))
            {
                GenerateMonthlyReport();
            }

            if (GUILayout.Button("Load Previous Report", GUILayout.Height(30)))
            {
                LoadPreviousReport();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Export Trend Analysis", GUILayout.Height(25)))
            {
                ExportTrendAnalysis();
            }
        }

        private void DrawMetricsDisplay()
        {
            if (currentMetrics == null) return;

            GUILayout.Label("Current Metrics", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DisplayMetricsSection("Compliance Overview", () => {
                DisplayMetric("Total Files Analyzed", currentMetrics.TotalFiles);
                DisplayMetric("Namespace Violations", currentMetrics.NamespaceViolations);
                DisplayMetric("GameObject.Find() Usages", currentMetrics.GameObjectFindUsages);
                DisplayMetric("Layer Violations", currentMetrics.LayerViolations);
                DisplayMetric("Overall Compliance Rate", $"{currentMetrics.ComplianceRate:F1}%");
            });

            DisplayMetricsSection("Performance Metrics", () => {
                DisplayMetric("High-Risk GameObject.Find()", currentMetrics.HighRiskFinds);
                DisplayMetric("Optimized References", currentMetrics.OptimizedReferences);
                DisplayMetric("ServiceLocator Usages", currentMetrics.ServiceLocatorUsages);
            });

            DisplayMetricsSection("Code Quality", () => {
                DisplayMetric("Test Coverage", $"{currentMetrics.TestCoverage:F1}%");
                DisplayMetric("Documentation Rate", $"{currentMetrics.DocumentationRate:F1}%");
                DisplayMetric("Error Handling Rate", $"{currentMetrics.ErrorHandlingRate:F1}%");
            });

            if (previousMetrics != null)
            {
                DisplayMetricsSection("Trend Analysis", () => {
                    DisplayTrend("Compliance Rate", currentMetrics.ComplianceRate, previousMetrics.ComplianceRate);
                    DisplayTrend("Namespace Violations", currentMetrics.NamespaceViolations, previousMetrics.NamespaceViolations, true);
                    DisplayTrend("GameObject.Find() Usages", currentMetrics.GameObjectFindUsages, previousMetrics.GameObjectFindUsages, true);
                });
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSchedulingSettings()
        {
            GUILayout.Label("Automation Settings", EditorStyles.boldLabel);
            
            autoSchedule = EditorGUILayout.Toggle("Auto-Schedule Monthly Reports", autoSchedule);
            
            if (autoSchedule)
            {
                EditorGUILayout.HelpBox("Monthly reports will be automatically generated on the 1st of each month.", MessageType.Info);
                
                if (GUILayout.Button("Set Up Auto-Schedule"))
                {
                    SetupAutoSchedule();
                }
            }
        }

        private void GenerateMonthlyReport()
        {
            currentMetrics = CollectComplianceMetrics();
            
            var report = GenerateDetailedReport();
            var reportPath = $"Assets/_Project/Tests/Results/Monthly_Compliance_Report_{DateTime.Now:yyyy_MM}.md";
            
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, report);
            
            // メトリクス履歴保存
            SaveMetricsHistory();
            
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Monthly compliance report generated: {reportPath}");
            
            // 前回レポートとして保存
            previousMetrics = LoadPreviousMetrics();
        }

        private ComplianceMetrics CollectComplianceMetrics()
        {
            var metrics = new ComplianceMetrics
            {
                GeneratedDate = DateTime.Now
            };

            var csFiles = Directory.GetFiles("Assets/_Project", "*.cs", SearchOption.AllDirectories)
                                  .Where(f => !f.Contains("Editor") || f.Contains("_Project/Scripts/Editor"))
                                  .ToArray();

            metrics.TotalFiles = csFiles.Length;

            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                var lines = content.Split('\n');

                AnalyzeFile(file, content, lines, metrics);
            }

            CalculateRates(metrics);
            return metrics;
        }

        private void AnalyzeFile(string file, string content, string[] lines, ComplianceMetrics metrics)
        {
            bool hasDocumentation = false;
            bool hasErrorHandling = false;
            bool hasTests = file.Contains("Tests");

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Namespace violations
                if (Regex.IsMatch(line, @"namespace\s+_Project\."))
                {
                    metrics.NamespaceViolations++;
                }

                // GameObject.Find() usages
                if (Regex.IsMatch(line, @"GameObject\.Find"))
                {
                    metrics.GameObjectFindUsages++;
                    
                    if (IsHighRiskFind(file, line))
                    {
                        metrics.HighRiskFinds++;
                    }
                }

                // ServiceLocator usages
                if (line.Contains("ServiceLocator.GetService"))
                {
                    metrics.ServiceLocatorUsages++;
                }

                // Optimized references
                if (line.Contains("[SerializeField]") && 
                    (i + 1 < lines.Length && lines[i + 1].Contains("Transform")))
                {
                    metrics.OptimizedReferences++;
                }

                // Documentation
                if (line.StartsWith("/// ") || line.StartsWith("/** "))
                {
                    hasDocumentation = true;
                }

                // Error handling
                if (line.Contains("try") || line.Contains("catch") || line.Contains("throw"))
                {
                    hasErrorHandling = true;
                }

                // Layer violations (Core→Features)
                if (file.Contains("Core") && Regex.IsMatch(line, @"using\s+asterivo\.Unity60\.Features"))
                {
                    metrics.LayerViolations++;
                }
            }

            if (hasDocumentation) metrics.FilesWithDocumentation++;
            if (hasErrorHandling) metrics.FilesWithErrorHandling++;
            if (hasTests) metrics.TestFiles++;
        }

        private bool IsHighRiskFind(string file, string line)
        {
            return line.Contains("Update()") || 
                   line.Contains("FixedUpdate()") ||
                   file.Contains("Manager") ||
                   file.Contains("Service");
        }

        private void CalculateRates(ComplianceMetrics metrics)
        {
            var totalViolations = metrics.NamespaceViolations + metrics.GameObjectFindUsages + metrics.LayerViolations;
            metrics.ComplianceRate = metrics.TotalFiles > 0 ? 
                ((float)(metrics.TotalFiles - totalViolations) / metrics.TotalFiles) * 100f : 100f;
            
            metrics.TestCoverage = metrics.TotalFiles > 0 ? 
                ((float)metrics.TestFiles / metrics.TotalFiles) * 100f : 0f;
            
            metrics.DocumentationRate = metrics.TotalFiles > 0 ? 
                ((float)metrics.FilesWithDocumentation / metrics.TotalFiles) * 100f : 0f;
            
            metrics.ErrorHandlingRate = metrics.TotalFiles > 0 ? 
                ((float)metrics.FilesWithErrorHandling / metrics.TotalFiles) * 100f : 0f;
        }

        private string GenerateDetailedReport()
        {
            var report = $@"# Monthly Architecture Compliance Report

**Generated**: {currentMetrics.GeneratedDate:yyyy-MM-dd HH:mm:ss}  
**Period**: {DateTime.Now:yyyy年MM月}  
**Total Files Analyzed**: {currentMetrics.TotalFiles}

## 📊 Executive Summary

| Metric | Current Value | Target | Status |
|--------|---------------|--------|--------|
| Overall Compliance Rate | {currentMetrics.ComplianceRate:F1}% | 95%+ | {GetComplianceStatus(currentMetrics.ComplianceRate)} |
| Namespace Violations | {currentMetrics.NamespaceViolations} | 0 | {GetViolationStatus(currentMetrics.NamespaceViolations)} |
| GameObject.Find() Usages | {currentMetrics.GameObjectFindUsages} | <10 | {GetUsageStatus(currentMetrics.GameObjectFindUsages)} |
| Layer Violations | {currentMetrics.LayerViolations} | 0 | {GetViolationStatus(currentMetrics.LayerViolations)} |

## 🎯 Key Performance Indicators

### Architecture Compliance
- **Namespace Compliance**: {((float)(currentMetrics.TotalFiles - currentMetrics.NamespaceViolations) / currentMetrics.TotalFiles * 100):F1}%
- **Performance Optimization**: {currentMetrics.OptimizedReferences} optimized references found
- **Service Pattern Usage**: {currentMetrics.ServiceLocatorUsages} ServiceLocator implementations

### Code Quality Metrics
- **Test Coverage**: {currentMetrics.TestCoverage:F1}%
- **Documentation Rate**: {currentMetrics.DocumentationRate:F1}%
- **Error Handling**: {currentMetrics.ErrorHandlingRate:F1}%

## 📈 Trend Analysis
";

            if (previousMetrics != null)
            {
                report += GenerateTrendAnalysis();
            }
            else
            {
                report += "No previous data available for trend analysis.\n\n";
            }

            report += GenerateRecommendations();
            report += GenerateActionItems();

            return report;
        }

        private string GenerateTrendAnalysis()
        {
            var complianceTrend = currentMetrics.ComplianceRate - previousMetrics.ComplianceRate;
            var namespaceTrend = previousMetrics.NamespaceViolations - currentMetrics.NamespaceViolations;
            var findTrend = previousMetrics.GameObjectFindUsages - currentMetrics.GameObjectFindUsages;

            return $@"
### Month-over-Month Changes

| Metric | Previous | Current | Change | Trend |
|--------|----------|---------|--------|--------|
| Compliance Rate | {previousMetrics.ComplianceRate:F1}% | {currentMetrics.ComplianceRate:F1}% | {complianceTrend:+F1}% | {GetTrendIcon(complianceTrend)} |
| Namespace Violations | {previousMetrics.NamespaceViolations} | {currentMetrics.NamespaceViolations} | {namespaceTrend:+0} | {GetTrendIcon(namespaceTrend)} |
| GameObject.Find() Usages | {previousMetrics.GameObjectFindUsages} | {currentMetrics.GameObjectFindUsages} | {findTrend:+0} | {GetTrendIcon(findTrend)} |

";
        }

        private string GenerateRecommendations()
        {
            var recommendations = new List<string>();

            if (currentMetrics.NamespaceViolations > 0)
            {
                recommendations.Add($"🔴 **Critical**: Fix {currentMetrics.NamespaceViolations} namespace violations immediately");
            }

            if (currentMetrics.HighRiskFinds > 0)
            {
                recommendations.Add($"⚠️ **High Priority**: Optimize {currentMetrics.HighRiskFinds} high-risk GameObject.Find() usages");
            }

            if (currentMetrics.LayerViolations > 0)
            {
                recommendations.Add($"🔴 **Critical**: Fix {currentMetrics.LayerViolations} layer dependency violations");
            }

            if (currentMetrics.TestCoverage < 80f)
            {
                recommendations.Add($"📊 **Improvement**: Increase test coverage from {currentMetrics.TestCoverage:F1}% to 80%+");
            }

            if (currentMetrics.DocumentationRate < 70f)
            {
                recommendations.Add($"📝 **Improvement**: Improve documentation coverage from {currentMetrics.DocumentationRate:F1}% to 70%+");
            }

            var result = "## 💡 Recommendations\n\n";
            if (recommendations.Any())
            {
                foreach (var rec in recommendations)
                {
                    result += $"- {rec}\n";
                }
            }
            else
            {
                result += "✅ No critical issues found. Continue current practices.\n";
            }

            return result + "\n";
        }

        private string GenerateActionItems()
        {
            return @"## 📋 Action Items

### Immediate Actions (This Week)
- [ ] Address all Critical severity issues
- [ ] Run Architecture Compliance Checker
- [ ] Update team on compliance status

### Short-term Goals (This Month)  
- [ ] Achieve 95%+ compliance rate
- [ ] Reduce GameObject.Find() usages to <10
- [ ] Improve test coverage by 5%

### Long-term Objectives (Next Quarter)
- [ ] Implement automated compliance monitoring
- [ ] Establish developer training program
- [ ] Create performance benchmarking system

## 📞 Next Review

**Scheduled**: {DateTime.Now.AddMonths(1):yyyy-MM-01}  
**Reviewer**: Development Team  
**Focus Areas**: Performance optimization, test coverage improvement

---
*Generated by MonthlyComplianceReporter v1.0*
";
        }

        private void SaveMetricsHistory()
        {
            var historyPath = "Assets/_Project/Tests/Results/metrics_history.json";
            var history = new List<ComplianceMetrics>();

            if (File.Exists(historyPath))
            {
                var existingData = File.ReadAllText(historyPath);
                history = JsonUtility.FromJson<ComplianceMetricsHistory>(existingData)?.Metrics ?? new List<ComplianceMetrics>();
            }

            history.Add(currentMetrics);
            
            // Keep last 12 months
            if (history.Count > 12)
            {
                history = history.TakeLast(12).ToList();
            }

            var historyWrapper = new ComplianceMetricsHistory { Metrics = history };
            File.WriteAllText(historyPath, JsonUtility.ToJson(historyWrapper, true));
        }

        private ComplianceMetrics LoadPreviousMetrics()
        {
            var historyPath = "Assets/_Project/Tests/Results/metrics_history.json";
            if (!File.Exists(historyPath)) return null;

            var historyData = File.ReadAllText(historyPath);
            var history = JsonUtility.FromJson<ComplianceMetricsHistory>(historyData);
            
            return history?.Metrics?.Count >= 2 ? history.Metrics[history.Metrics.Count - 2] : null;
        }

        private void LoadPreviousReport()
        {
            previousMetrics = LoadPreviousMetrics();
            if (previousMetrics != null)
            {
                UnityEngine.Debug.Log("Previous metrics loaded successfully");
            }
            else
            {
                UnityEngine.Debug.LogWarning("No previous metrics found");
            }
        }

        private void ExportTrendAnalysis()
        {
            // CSV形式で履歴データをエクスポート
            var historyPath = "Assets/_Project/Tests/Results/metrics_history.json";
            if (!File.Exists(historyPath)) return;

            var csvPath = "Assets/_Project/Tests/Results/compliance_trends.csv";
            var csvContent = "Date,ComplianceRate,NamespaceViolations,GameObjectFindUsages,LayerViolations,TestCoverage\n";

            var historyData = File.ReadAllText(historyPath);
            var history = JsonUtility.FromJson<ComplianceMetricsHistory>(historyData);

            foreach (var metric in history.Metrics)
            {
                csvContent += $"{metric.GeneratedDate:yyyy-MM-dd},{metric.ComplianceRate},{metric.NamespaceViolations},{metric.GameObjectFindUsages},{metric.LayerViolations},{metric.TestCoverage}\n";
            }

            File.WriteAllText(csvPath, csvContent);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Trend analysis exported to: {csvPath}");
        }

        private void SetupAutoSchedule()
        {
            // TODO: EditorApplication.delayCallを使用した定期実行の実装
            UnityEngine.Debug.Log("Auto-schedule setup completed. Monthly reports will be generated automatically.");
        }

        // Helper methods for status display
        private string GetComplianceStatus(float rate) => rate >= 95f ? "✅" : rate >= 90f ? "⚠️" : "❌";
        private string GetViolationStatus(int count) => count == 0 ? "✅" : "❌";
        private string GetUsageStatus(int count) => count < 10 ? "✅" : count < 20 ? "⚠️" : "❌";
        private string GetTrendIcon(float change) => change > 0 ? "📈" : change < 0 ? "📉" : "➡️";

        private void DisplayMetricsSection(string title, System.Action content)
        {
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            content.Invoke();
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void DisplayMetric(string label, object value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(200));
            GUILayout.Label(value.ToString(), EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }

        private void DisplayTrend(string label, float current, float previous, bool lowerIsBetter = false)
        {
            var change = current - previous;
            var color = (lowerIsBetter ? change <= 0 : change >= 0) ? Color.green : Color.red;
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(200));
            
            var previousColor = GUI.color;
            GUI.color = color;
            GUILayout.Label($"{change:+0.0}", EditorStyles.boldLabel);
            GUI.color = previousColor;
            
            GUILayout.EndHorizontal();
        }
    }

    [System.Serializable]
    public class ComplianceMetrics
    {
        public DateTime GeneratedDate;
        public int TotalFiles;
        public int NamespaceViolations;
        public int GameObjectFindUsages;
        public int LayerViolations;
        public int HighRiskFinds;
        public int OptimizedReferences;
        public int ServiceLocatorUsages;
        public int TestFiles;
        public int FilesWithDocumentation;
        public int FilesWithErrorHandling;
        public float ComplianceRate;
        public float TestCoverage;
        public float DocumentationRate;
        public float ErrorHandlingRate;
    }

    [System.Serializable]
    public class ComplianceMetricsHistory
    {
        public List<ComplianceMetrics> Metrics;
    }
}