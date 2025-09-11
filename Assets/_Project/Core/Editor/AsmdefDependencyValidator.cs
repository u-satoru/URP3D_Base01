using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using asterivo.Unity60.Core.Debug;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Core ← Features の単方向依存を検証する軽量バリデータ。
    /// Core配下のasmdefがFeatures系アセンブリに参照を張っていないかをチェックする。
    /// </summary>
    public static class AsmdefDependencyValidator
    {
#if UNITY_EDITOR
        private static readonly string ProjectRoot = Application.dataPath.Replace("/Assets", "/Assets/_Project");

        [MenuItem("Tools/Architecture/Validate Asmdef Dependencies")] 
        public static void ValidateMenu()
        {
            ValidateAndReport();
        }

        [InitializeOnLoadMethod]
        private static void AutoValidateOnLoad()
        {
            // Editor起動毎に軽く検証（重くない）
            ValidateAndReport();
        }

        private static void ValidateAndReport()
        {
            try
            {
                var asmdefPaths = Directory.GetFiles(ProjectRoot, "*.asmdef", SearchOption.AllDirectories);
                var violations = new List<string>();

                foreach (var path in asmdefPaths)
                {
                    var json = File.ReadAllText(path);
                    var name = ExtractJsonValue(json, "name");
                    var references = ExtractJsonArray(json, "references");
                    bool isCoreAsm = path.Replace('\\','/').Contains("/Core/");

                    if (isCoreAsm)
                    {
                        foreach (var r in references)
                        {
                            if (r.StartsWith("asterivo.Unity60.") && !r.StartsWith("asterivo.Unity60.Core"))
                            {
                                violations.Add($"{Path.GetFileName(path)} references '{r}'");
                            }
                        }
                    }
                }

                if (violations.Count > 0)
                {
                    UnityEngine.Debug.LogError("[AsmdefValidator] Core asmdef has forbidden references:\n - " + string.Join("\n - ", violations));
                }
                else
                {
                    UnityEngine.Debug.Log("[AsmdefValidator] OK: Core asmdef dependency direction validated.");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[AsmdefValidator] Validation failed: {e.Message}");
            }
        }

        private static string ExtractJsonValue(string json, string key)
        {
            // ざっくり抽出（厳密なJSONパースは不要）
            var marker = $"\"{key}\"";
            var idx = json.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return string.Empty;
            idx = json.IndexOf(':', idx) + 1;
            var end = json.IndexOf('\n', idx);
            var line = json.Substring(idx, end - idx);
            var val = line.Trim().Trim(',').Trim().Trim('"');
            return val;
        }

        private static List<string> ExtractJsonArray(string json, string key)
        {
            var result = new List<string>();
            var marker = $"\"{key}\"";
            var idx = json.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return result;
            idx = json.IndexOf('[', idx) + 1;
            var end = json.IndexOf(']', idx);
            if (end < 0) return result;
            var slice = json.Substring(idx, end - idx);
            foreach (var part in slice.Split(','))
            {
                var s = part.Trim().Trim('"');
                if (!string.IsNullOrEmpty(s)) result.Add(s);
            }
            return result;
        }
#endif
    }
}

