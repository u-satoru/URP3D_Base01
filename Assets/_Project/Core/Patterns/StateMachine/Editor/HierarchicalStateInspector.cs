#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using asterivo.Unity60.Core.StateMachine;

namespace asterivo.Unity60.Core.Patterns.StateMachine.Editor
{
    /// <summary>
    /// 階層化ステートマシンのカスタムPropertyDrawer
    /// Inspector上で階層構造と現在の状態を視覚的に表示
    /// </summary>
    [CustomPropertyDrawer(typeof(HierarchicalState<>))]
    public class HierarchicalStateInspector : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float IndentWidth = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 階層状態の視覚的表示
            var hierarchicalState = GetHierarchicalState(property);
            if (hierarchicalState != null)
            {
                DrawStateHierarchy(position, hierarchicalState, label.text);
            }
            else
            {
                // フォールバック: 通常のプロパティ表示
                EditorGUI.LabelField(position, label.text, "HierarchicalState (Runtime Only)");
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hierarchicalState = GetHierarchicalState(property);
            if (hierarchicalState != null)
            {
                var childStates = GetChildStates(hierarchicalState);
                // ヘッダー + 現在の子状態 + 子状態リスト + デバッグ情報
                return LineHeight * (3 + (childStates?.Count ?? 0) + 2);
            }

            return LineHeight;
        }

        private void DrawStateHierarchy(Rect position, object hierarchicalState, string stateName)
        {
            var rect = new Rect(position.x, position.y, position.width, LineHeight);

            // ヘッダー
            EditorGUI.LabelField(rect, stateName, EditorStyles.boldLabel);
            rect.y += LineHeight;

            // 現在の子状態表示
            var currentChildStateKey = GetCurrentChildStateKey(hierarchicalState);
            var isActive = GetIsActive(hierarchicalState);

            var statusColor = isActive ? Color.green : Color.gray;
            var originalColor = GUI.color;
            GUI.color = statusColor;

            EditorGUI.LabelField(rect, $"Current Child: {currentChildStateKey ?? "None"}");
            GUI.color = originalColor;
            rect.y += LineHeight;

            // 子状態リスト
            var childStates = GetChildStates(hierarchicalState);
            if (childStates != null && childStates.Count > 0)
            {
                EditorGUI.LabelField(rect, "Child States:", EditorStyles.miniBoldLabel);
                rect.y += LineHeight;

                foreach (var kvp in childStates)
                {
                    var childKey = kvp.Key;
                    var isActiveChild = childKey == currentChildStateKey;
                    var style = isActiveChild ? EditorStyles.boldLabel : EditorStyles.label;

                    rect.x += IndentWidth;

                    if (isActiveChild)
                    {
                        GUI.color = Color.cyan;
                    }

                    EditorGUI.LabelField(rect, $"• {childKey}", style);

                    if (isActiveChild)
                    {
                        GUI.color = originalColor;
                    }

                    rect.x -= IndentWidth;
                    rect.y += LineHeight;
                }
            }

            // デバッグ情報
            DrawDebugInfo(rect, hierarchicalState);
        }

        private void DrawDebugInfo(Rect rect, object hierarchicalState)
        {
            if (!Application.isPlaying) return;

            var stateHistory = GetStateHistory(hierarchicalState);
            var previousChildState = GetPreviousChildState(hierarchicalState);

            EditorGUI.LabelField(rect, "Debug Info:", EditorStyles.miniLabel);
            rect.y += LineHeight;

            var debugText = $"Previous: {previousChildState ?? "None"} | History: {stateHistory?.Count ?? 0} entries";
            EditorGUI.LabelField(rect, debugText, EditorStyles.miniLabel);
        }

        // リフレクションヘルパーメソッド
        private object GetHierarchicalState(SerializedProperty property)
        {
            try
            {
                // SerializedPropertyから実際のオブジェクトを取得
                var targetObject = property.serializedObject.targetObject;
                var fieldInfo = targetObject.GetType().GetField(property.propertyPath,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                return fieldInfo?.GetValue(targetObject);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get state object: {ex.Message}");
                return null;
            }
        }

        private string GetCurrentChildStateKey(object state)
        {
            try
            {
                var method = state.GetType().GetMethod("GetCurrentChildStateKey");
                return method?.Invoke(state, null) as string;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get current child state: {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, object> GetChildStates(object state)
        {
            try
            {
                var method = state.GetType().GetMethod("GetChildStates");
                var result = method?.Invoke(state, null) as IDictionary;

                if (result == null) return null;

                var dictionary = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in result)
                {
                    dictionary[entry.Key.ToString()] = entry.Value;
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get child states: {ex.Message}");
                return null;
            }
        }

        private bool GetIsActive(object state)
        {
            try
            {
                var property = state.GetType().GetProperty("IsActive");
                return property?.GetValue(state) as bool? ?? false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get IsActive: {ex.Message}");
                return false;
            }
        }

        private string GetPreviousChildState(object state)
        {
            try
            {
                var method = state.GetType().GetMethod("GetPreviousChildState");
                return method?.Invoke(state, null) as string;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get previous child state: {ex.Message}");
                return null;
            }
        }

        private ICollection GetStateHistory(object state)
        {
            try
            {
                var method = state.GetType().GetMethod("GetStateHistory");
                return method?.Invoke(state, null) as ICollection;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"HierarchicalStateInspector: Failed to get state history: {ex.Message}");
                return null;
            }
        }
    }
}
#endif
