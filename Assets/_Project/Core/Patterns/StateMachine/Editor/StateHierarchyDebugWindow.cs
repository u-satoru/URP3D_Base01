#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace asterivo.Unity60.Core.Patterns.StateMachine.Editor
{
    /// <summary>
    /// 階層化ステートマシンのリアルタイムデバッグウィンドウ
    /// 選択されたGameObjectの階層化ステートマシンの状態を視覚的に監視
    /// </summary>
    public class StateHierarchyDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private GameObject selectedGameObject;
        private MonoBehaviour[] stateMachines;
        private bool autoRefresh = true;
        private float refreshRate = 10f; // Hz
        private double lastRefreshTime = 0;

        [MenuItem("Tools/State Machine/Hierarchy Debugger")]
        public static void ShowWindow()
        {
            var window = GetWindow<StateHierarchyDebugWindow>("State Hierarchy Debugger");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (autoRefresh && Application.isPlaying)
            {
                var currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - lastRefreshTime > (1.0 / refreshRate))
                {
                    Repaint();
                    lastRefreshTime = currentTime;
                }
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawSettings();
            DrawGameObjectSelection();

            if (selectedGameObject != null)
            {
                DrawStateMachineList();
                DrawStateHierarchy();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Hierarchical State Machine Debugger", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawSettings()
        {
            EditorGUILayout.BeginHorizontal();

            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);

            EditorGUI.BeginDisabledGroup(!autoRefresh);
            refreshRate = EditorGUILayout.Slider("Refresh Rate (Hz)", refreshRate, 1f, 60f);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Manual Refresh", GUILayout.Width(100)))
            {
                RefreshStateMachines();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void DrawGameObjectSelection()
        {
            var newSelection = EditorGUILayout.ObjectField("Target GameObject",
                selectedGameObject, typeof(GameObject), true) as GameObject;

            if (newSelection != selectedGameObject)
            {
                selectedGameObject = newSelection;
                RefreshStateMachines();
            }

            // 現在の選択オブジェクトからの自動選択
            if (GUILayout.Button("Use Selected GameObject"))
            {
                if (Selection.activeGameObject != null)
                {
                    selectedGameObject = Selection.activeGameObject;
                    RefreshStateMachines();
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawStateMachineList()
        {
            if (stateMachines == null || stateMachines.Length == 0)
            {
                EditorGUILayout.HelpBox("No state machines found on this GameObject.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Found {stateMachines.Length} MonoBehaviour component(s)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var stateMachine in stateMachines)
            {
                if (stateMachine == null) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(stateMachine.GetType().Name, EditorStyles.boldLabel);

                // 階層状態の表示
                DrawStateMachineHierarchy(stateMachine);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        private void DrawStateHierarchy()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // リアルタイム更新のための Repaint
            if (Application.isPlaying && autoRefresh)
            {
                var statusText = $"Status: Playing | Refresh Rate: {refreshRate:F1}Hz | Last Update: {System.DateTime.Now:HH:mm:ss}";
                EditorGUILayout.LabelField(statusText, EditorStyles.miniLabel);
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshStateMachines()
        {
            if (selectedGameObject == null)
            {
                stateMachines = null;
                return;
            }

            // StateMachine系のコンポーネントを検索
            stateMachines = selectedGameObject.GetComponents<MonoBehaviour>();
        }

        private void DrawStateMachineHierarchy(MonoBehaviour stateMachine)
        {
            // リフレクションを使用してHierarchicalStateを検出・表示
            var fields = stateMachine.GetType().GetFields(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance);

            bool foundHierarchicalState = false;

            foreach (var field in fields)
            {
                if (IsHierarchicalStateField(field))
                {
                    var hierarchicalState = field.GetValue(stateMachine);
                    if (hierarchicalState != null)
                    {
                        foundHierarchicalState = true;
                        DrawHierarchicalStateInfo(field.Name, hierarchicalState);
                    }
                }
            }

            if (!foundHierarchicalState)
            {
                EditorGUILayout.LabelField("  No hierarchical states found", EditorStyles.label);
            }
        }

        private bool IsHierarchicalStateField(FieldInfo field)
        {
            if (field.FieldType.IsGenericType)
            {
                var genericTypeDef = field.FieldType.GetGenericTypeDefinition();
                return genericTypeDef.Name.StartsWith("HierarchicalState");
            }
            return false;
        }

        private void DrawHierarchicalStateInfo(string fieldName, object hierarchicalState)
        {
            EditorGUILayout.LabelField($"  {fieldName}:", EditorStyles.boldLabel);

            // 状態の基本情報
            var isActive = GetIsActive(hierarchicalState);
            var activeColor = isActive ? Color.green : Color.red;
            var originalColor = GUI.color;

            GUI.color = activeColor;
            EditorGUILayout.LabelField($"    Active: {isActive}", EditorStyles.label);
            GUI.color = originalColor;

            // 現在の子状態を表示
            var currentChild = GetCurrentChildStateKey(hierarchicalState);
            EditorGUILayout.LabelField($"    Current Child: {currentChild ?? "None"}");

            // 前の子状態を表示
            var previousChild = GetPreviousChildState(hierarchicalState);
            if (!string.IsNullOrEmpty(previousChild))
            {
                EditorGUILayout.LabelField($"    Previous Child: {previousChild}", EditorStyles.label);
            }

            // 子状態一覧を表示
            var childStates = GetChildStates(hierarchicalState);
            if (childStates != null && childStates.Count > 0)
            {
                EditorGUILayout.LabelField("    Child States:", EditorStyles.miniLabel);

                foreach (DictionaryEntry kvp in childStates)
                {
                    var key = kvp.Key.ToString();
                    var isActiveChild = key == currentChild;
                    var style = isActiveChild ? EditorStyles.boldLabel : EditorStyles.label;
                    var color = isActiveChild ? Color.cyan : Color.gray;

                    GUI.color = color;
                    EditorGUILayout.LabelField($"      • {key}", style);
                    GUI.color = originalColor;
                }
            }

            // 状態履歴を表示
            if (Application.isPlaying)
            {
                var stateHistory = GetStateHistory(hierarchicalState);
                if (stateHistory != null && stateHistory.Count > 0)
                {
                    EditorGUILayout.LabelField($"    History Count: {stateHistory.Count}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space();
        }

        // リフレクションヘルパーメソッド
        private string GetCurrentChildStateKey(object hierarchicalState)
        {
            try
            {
                var method = hierarchicalState.GetType().GetMethod("GetCurrentChildStateKey");
                return method?.Invoke(hierarchicalState, null) as string;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"StateHierarchyDebugWindow: Failed to get current child state: {ex.Message}");
                return null;
            }
        }

        private IDictionary GetChildStates(object hierarchicalState)
        {
            try
            {
                var method = hierarchicalState.GetType().GetMethod("GetChildStates");
                return method?.Invoke(hierarchicalState, null) as IDictionary;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"StateHierarchyDebugWindow: Failed to get child states: {ex.Message}");
                return null;
            }
        }

        private bool GetIsActive(object hierarchicalState)
        {
            try
            {
                var property = hierarchicalState.GetType().GetProperty("IsActive");
                return property?.GetValue(hierarchicalState) as bool? ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"StateHierarchyDebugWindow: Failed to get IsActive: {ex.Message}");
                return false;
            }
        }

        private string GetPreviousChildState(object hierarchicalState)
        {
            try
            {
                var method = hierarchicalState.GetType().GetMethod("GetPreviousChildState");
                return method?.Invoke(hierarchicalState, null) as string;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"StateHierarchyDebugWindow: Failed to get previous child state: {ex.Message}");
                return null;
            }
        }

        private ICollection GetStateHistory(object hierarchicalState)
        {
            try
            {
                var method = hierarchicalState.GetType().GetMethod("GetStateHistory");
                return method?.Invoke(hierarchicalState, null) as ICollection;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"StateHierarchyDebugWindow: Failed to get state history: {ex.Message}");
                return null;
            }
        }
    }
}
#endif
