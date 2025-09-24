using UnityEngine;
using UnityEditor;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// Editor utility to clear the Unity Console for Phase 4.1 verification
    /// </summary>
    public static class ConsoleClearer
    {
        [MenuItem("Tools/Clear Console %&c")]
        public static void ClearConsole()
        {
            // Clear the console using reflection to access LogEntries.Clear
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);

            Debug.Log("Console cleared for Phase 4.1 verification");
        }
    }
}