using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Simple console clear utility for Unity 6
/// Updated to force recompilation
/// </summary>
public static class SimpleConsoleClear
{
    [MenuItem("Edit/Clear Console Now")]
    public static void ClearNow()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);

        Debug.Log("[Console Cleared] at " + System.DateTime.Now.ToString("HH:mm:ss"));
    }
}
