using UnityEngine;
using UnityEditor;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// SystemRequirementChecker の動作確認用テストクラス
    /// </summary>
    public static class SystemRequirementCheckerTest
    {
        [MenuItem("Tools/Test System Requirements", priority = 200)]
        public static void TestSystemRequirements()
        {
            UnityEngine.Debug.Log("=== SystemRequirementChecker Test Starting ===");
            
            try
            {
                // SystemRequirementChecker を実行
                var report = SystemRequirementChecker.CheckAllRequirements();
                
                // 結果をログに出力
                UnityEngine.Debug.Log($"Test completed successfully! Report contains {report.results.Count} checks.");
                UnityEngine.Debug.Log($"Overall validity: {report.isValid}");
                
                // 個別の結果を表示
                foreach (var result in report.results)
                {
                    string resultStatus = result.isPassed ? "✅ PASS" : "❌ FAIL";
                    UnityEngine.Debug.Log($"{resultStatus} - {result.checkName}: {result.message}");
                }
                
                UnityEngine.Debug.Log("\n" + report.summary);
                UnityEngine.Debug.Log("=== SystemRequirementChecker Test Completed ===");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"SystemRequirementChecker test failed with exception: {ex.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}