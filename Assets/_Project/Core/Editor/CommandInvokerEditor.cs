using UnityEngine;
using UnityEditor;
using System.Linq;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Core.Editor
{
    [CustomEditor(typeof(CommandInvoker))]
    public class CommandInvokerEditor : UnityEditor.Editor
    {
        private CommandInvoker invoker;
        private bool showCommandDetails = false;
        
        void OnEnable()
        {
            invoker = (CommandInvoker)target;
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Command History Debug", EditorStyles.boldLabel);
            
            if (Application.isPlaying)
            {
                DrawPlayModeInterface();
            }
            else
            {
                DrawEditModeInterface();
            }
        }
        
        private void DrawPlayModeInterface()
        {
            // Current state display
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Undo Stack: {invoker.UndoStackCount}", GUILayout.Width(120));
            EditorGUILayout.LabelField($"Redo Stack: {invoker.RedoStackCount}", GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = invoker.CanUndo;
            if (GUILayout.Button("⬅️ Undo"))
            {
                invoker.Undo();
            }
            
            GUI.enabled = invoker.CanRedo;
            if (GUILayout.Button("➡️ Redo"))
            {
                invoker.Redo();
            }
            
            GUI.enabled = invoker.UndoStackCount > 0 || invoker.RedoStackCount > 0;
            if (GUILayout.Button("🗑️ Clear History"))
            {
                invoker.ClearHistory();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            // Command history visualization toggle
            showCommandDetails = EditorGUILayout.Toggle("Show Command Details", showCommandDetails);
            
            if (showCommandDetails)
            {
                DrawCommandHistory();
            }
            
            // Test command buttons
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Test Commands", EditorStyles.boldLabel);
            DrawTestCommands();
        }
        
        private void DrawEditModeInterface()
        {
            EditorGUILayout.HelpBox("Command history is only available during Play mode", MessageType.Info);
            
            // Configuration validation
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration Validation", EditorStyles.boldLabel);
            
            var onCommandReceived = serializedObject.FindProperty("onCommandReceived");
            var onUndoStateChanged = serializedObject.FindProperty("onUndoStateChanged");
            var onRedoStateChanged = serializedObject.FindProperty("onRedoStateChanged");
            var playerHealthComponent = serializedObject.FindProperty("playerHealthComponent");
            
            // Check for missing references
            if (onCommandReceived.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Command Event assigned", MessageType.Warning);
            }
            
            if (onUndoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Undo State Event assigned", MessageType.Warning);
            }
            
            if (onRedoStateChanged.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Redo State Event assigned", MessageType.Warning);
            }
            
            if (playerHealthComponent.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ No Player Health Component assigned", MessageType.Warning);
            }
            else
            {
                // Check if the component implements IHealthTarget
                var component = playerHealthComponent.objectReferenceValue as Component;
                if (component != null)
                {
                    var healthTarget = component.GetComponent<IHealthTarget>();
                    if (healthTarget == null)
                    {
                        EditorGUILayout.HelpBox("⚠️ Assigned component doesn't implement IHealthTarget", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("✅ Configuration valid", MessageType.Info);
                    }
                }
            }
        }
        
        private void DrawCommandHistory()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Command Stack Visualization:", EditorStyles.boldLabel);
            
            if (invoker.UndoStackCount == 0 && invoker.RedoStackCount == 0)
            {
                EditorGUILayout.LabelField("No commands in history", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }
            
            // This is a simplified visualization since we can't access the actual stacks
            // In a real implementation, you'd need to expose the stacks or their contents
            
            // Redo Stack visualization (top to bottom)
            if (invoker.RedoStackCount > 0)
            {
                EditorGUILayout.LabelField("Redo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.RedoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"🔄 Command #{invoker.RedoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            // Current position marker
            EditorGUILayout.LabelField("━━━ CURRENT ━━━", EditorStyles.centeredGreyMiniLabel);
            
            // Undo Stack visualization (top to bottom)
            if (invoker.UndoStackCount > 0)
            {
                EditorGUILayout.LabelField("Undo Stack:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < invoker.UndoStackCount; i++)
                {
                    EditorGUILayout.LabelField($"↩️ Command #{invoker.UndoStackCount - i}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawTestCommands()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Test commands only available in Play mode", MessageType.Info);
                return;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("❤️ Test Heal (10)"))
            {
                TestHealCommand(10);
            }
            
            if (GUILayout.Button("💔 Test Damage (10)"))
            {
                TestDamageCommand(10);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("❤️ Test Heal (25)"))
            {
                TestHealCommand(25);
            }
            
            if (GUILayout.Button("💔 Test Damage (25)"))
            {
                TestDamageCommand(25);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void TestHealCommand(int amount)
        {
            try
            {
                var healthComponent = invoker.GetComponent<IHealthTarget>();
                if (healthComponent == null)
                {
                    // Try to find it in children or get from the assigned component
                    var playerHealthComp = serializedObject.FindProperty("playerHealthComponent").objectReferenceValue as Component;
                    if (playerHealthComp != null)
                    {
                        healthComponent = playerHealthComp.GetComponent<IHealthTarget>();
                    }
                }
                
                if (healthComponent != null)
                {
                    var healCommand = new HealCommand(healthComponent, amount);
                    invoker.ExecuteCommand(healCommand);
                    Debug.Log($"Test heal command executed: +{amount} HP");
                }
                else
                {
                    Debug.LogWarning("No IHealthTarget found for test command");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to execute test heal command: {e.Message}");
            }
        }
        
        private void TestDamageCommand(int amount)
        {
            try
            {
                var healthComponent = invoker.GetComponent<IHealthTarget>();
                if (healthComponent == null)
                {
                    var playerHealthComp = serializedObject.FindProperty("playerHealthComponent").objectReferenceValue as Component;
                    if (playerHealthComp != null)
                    {
                        healthComponent = playerHealthComp.GetComponent<IHealthTarget>();
                    }
                }
                
                if (healthComponent != null)
                {
                    var damageCommand = new DamageCommand(healthComponent, amount);
                    invoker.ExecuteCommand(damageCommand);
                    Debug.Log($"Test damage command executed: -{amount} HP");
                }
                else
                {
                    Debug.LogWarning("No IHealthTarget found for test command");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to execute test damage command: {e.Message}");
            }
        }
    }
}