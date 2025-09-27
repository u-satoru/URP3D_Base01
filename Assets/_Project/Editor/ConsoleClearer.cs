using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Advanced Console clearer utility for Unity Editor
    /// Resolves persistent error display issues by forcing UI refresh
    /// </summary>
    [InitializeOnLoad]
    public static class ConsoleClearer
    {
        private static Assembly _editorAssembly;
        private static System.Type _logEntriesType;
        private static System.Type _consoleWindowType;

        static ConsoleClearer()
        {
            _editorAssembly = Assembly.GetAssembly(typeof(SceneView));
            _logEntriesType = _editorAssembly.GetType("UnityEditor.LogEntries");
            _consoleWindowType = _editorAssembly.GetType("UnityEditor.ConsoleWindow");
        }

        [MenuItem("Tools/Force Clear Console %#k")]
        public static void ForceCompleteClear()
        {
            ClearConsoleCompletely();
            RefreshConsoleWindow();
            ForceRepaintAllWindows();

            Debug.Log("Console force cleared - all caches and UI refreshed");
        }

        [MenuItem("Tools/Clear Console %&c")]
        public static void ClearConsole()
        {
            ClearConsoleCompletely();
            Debug.Log("Console cleared");
        }

        [MenuItem("Tools/Refresh Console Window")]
        public static void RefreshConsoleOnly()
        {
            RefreshConsoleWindow();
            ForceRepaintAllWindows();
        }

        private static void ClearConsoleCompletely()
        {
            if (_logEntriesType == null) return;

            // Method 1: Standard Clear
            var clearMethod = _logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);

            // Method 2: Clear with flags
            var clearOnPlayMethod = _logEntriesType.GetMethod("ClearOnPlay", BindingFlags.Static | BindingFlags.Public);
            clearOnPlayMethod?.Invoke(null, null);

            // Method 3: Clear console entries directly
            var clearConsoleEntriesMethod = _logEntriesType.GetMethod("ClearConsoleEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (clearConsoleEntriesMethod != null)
            {
                try
                {
                    clearConsoleEntriesMethod.Invoke(null, null);
                }
                catch { }
            }

            // Method 4: Reset error/warning counts
            var setConsoleErrorPauseMethod = _logEntriesType.GetMethod("SetConsoleErrorPause", BindingFlags.Static | BindingFlags.Public);
            setConsoleErrorPauseMethod?.Invoke(null, new object[] { false });
        }

        private static void RefreshConsoleWindow()
        {
            if (_consoleWindowType == null) return;

            // Get all console windows
            var windows = Resources.FindObjectsOfTypeAll(_consoleWindowType);

            foreach (var window in windows)
            {
                if (window == null) continue;

                // Force refresh
                var repaintMethod = window.GetType().GetMethod("Repaint", BindingFlags.Instance | BindingFlags.Public);
                repaintMethod?.Invoke(window, null);

                // Clear internal caches if they exist
                var clearMethod = window.GetType().GetMethod("ClearEntries", BindingFlags.Instance | BindingFlags.NonPublic);
                clearMethod?.Invoke(window, null);

                // Force update
                var updateMethod = window.GetType().GetMethod("DoLogChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                if (updateMethod != null)
                {
                    try
                    {
                        updateMethod.Invoke(window, null);
                    }
                    catch { }
                }
            }
        }

        private static void ForceRepaintAllWindows()
        {
            // Force repaint of all editor windows
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();

            // Repaint all windows
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window != null)
                    window.Repaint();
            }

            // Force Unity to update its internal state
            EditorUtility.RequestScriptReload();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        [MenuItem("Tools/Emergency Console Reset")]
        public static void EmergencyReset()
        {
            // Nuclear option - clears everything and forces recompile
            ForceCompleteClear();

            // Clear Unity's internal error state
            EditorPrefs.DeleteKey("ConsoleWindowLog");
            EditorPrefs.DeleteKey("ConsoleWindowStacktrace");

            // Force a script recompile to reset everything
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

            Debug.LogWarning("Emergency console reset complete - Unity will recompile scripts");
        }
    }
}
