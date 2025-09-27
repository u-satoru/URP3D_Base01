using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Console Clear Window for Unity 6
/// Provides a simple window to clear the console
/// </summary>
public class ConsoleClearWindow : EditorWindow
{
    [MenuItem("Window/Console Tools/Clear Console Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConsoleClearWindow>("Console Clear");
        window.minSize = new Vector2(250, 100);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Console Management", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Clear Console Now", GUILayout.Height(30)))
        {
            ClearConsole();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Force Refresh", GUILayout.Height(30)))
        {
            AssetDatabase.Refresh();
            Debug.Log("[Assets Refreshed] at " + System.DateTime.Now.ToString("HH:mm:ss"));
        }
    }

    private static void ClearConsole()
    {
        // Method 1: Using LogEntries
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method?.Invoke(new object(), null);

        Debug.Log("[Console Cleared] at " + System.DateTime.Now.ToString("HH:mm:ss"));
    }

    // Also add direct menu item
    [MenuItem("Tools/Clear Console Immediately")]
    public static void ClearConsoleDirectly()
    {
        ClearConsole();
    }
}
