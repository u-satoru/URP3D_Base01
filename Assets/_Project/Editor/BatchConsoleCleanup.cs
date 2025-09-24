using UnityEngine;
using UnityEditor;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Batch mode utility for cleaning up console errors and refreshing assets
    /// </summary>
    public static class BatchConsoleCleanup
    {
        [MenuItem("Tools/Batch Cleanup/Execute Console Cleanup")]
        public static void ExecuteCleanup()
        {
            Debug.Log("Starting batch console cleanup...");

            // Force clear console using existing ConsoleClearer
            ConsoleClearer.ForceCompleteClear();

            // Refresh the Asset Database
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            // Clear console again after refresh
            ConsoleClearer.EmergencyReset();

            Debug.Log("Batch console cleanup completed successfully");

            // For batch mode execution
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        // Method for batch mode execution
        public static void BatchExecute()
        {
            ExecuteCleanup();
        }
    }
}
