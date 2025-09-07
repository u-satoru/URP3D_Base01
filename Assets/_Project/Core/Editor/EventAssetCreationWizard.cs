using System.IO;
using UnityEditor;
using UnityEngine;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// イベントアセット作成ウィザード
    /// ScriptableObjectベースのイベントアセットを簡単に作成できるエディタツール
    /// </summary>
    public class EventAssetCreationWizard : EditorWindow
    {
        private string eventName = "NewGameEvent";
        private EventType selectedEventType = EventType.GameEvent;
        private string selectedPayloadType = "string";
        private string eventDescription = "";
        private string targetFolder = "Assets/_Project/Core/ScriptableObjects/Events/Core/";
        
        private readonly string[] commonPayloadTypes = {
            "string", "int", "float", "bool", "Vector2", "Vector3", 
            "PlayerStateType", "CameraStateType", "ICommand"
        };
        
        private enum EventType
        {
            GameEvent,
            GenericGameEvent,
            PlayerStateEvent,
            CameraStateEvent,
            CommandGameEvent
        }
        
        [MenuItem("asterivo.Unity60/Tools/Event Asset Creation Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<EventAssetCreationWizard>("Event Asset Creation Wizard");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Event Asset Creation Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "このウィザードは、Unity6プロジェクトのイベントシステム用ScriptableObjectアセットを作成します。", 
                MessageType.Info
            );
            
            EditorGUILayout.Space(10);
            DrawBasicSettings();
            EditorGUILayout.Space(10);
            DrawEventTypeSettings();
            EditorGUILayout.Space(10);
            DrawTargetFolderSettings();
            EditorGUILayout.Space(10);
            DrawPreview();
            EditorGUILayout.Space(10);
            DrawCreateButton();
        }
        
        private void DrawBasicSettings()
        {
            EditorGUILayout.LabelField("基本設定", EditorStyles.boldLabel);
            
            eventName = EditorGUILayout.TextField("イベント名", eventName);
            eventDescription = EditorGUILayout.TextField("説明", eventDescription);
            
            if (string.IsNullOrEmpty(eventName))
            {
                EditorGUILayout.HelpBox("イベント名を入力してください。", MessageType.Warning);
            }
        }
        
        private void DrawEventTypeSettings()
        {
            EditorGUILayout.LabelField("イベントタイプ設定", EditorStyles.boldLabel);
            
            selectedEventType = (EventType)EditorGUILayout.EnumPopup("イベントタイプ", selectedEventType);
            
            switch (selectedEventType)
            {
                case EventType.GameEvent:
                    EditorGUILayout.HelpBox("パラメーターなしの基本イベント", MessageType.Info);
                    break;
                    
                case EventType.GenericGameEvent:
                    EditorGUILayout.LabelField("ペイロードタイプ");
                    int selectedIndex = System.Array.IndexOf(commonPayloadTypes, selectedPayloadType);
                    if (selectedIndex == -1) selectedIndex = 0;
                    
                    selectedIndex = EditorGUILayout.Popup("一般的なタイプ", selectedIndex, commonPayloadTypes);
                    selectedPayloadType = commonPayloadTypes[selectedIndex];
                    
                    EditorGUILayout.Space(5);
                    selectedPayloadType = EditorGUILayout.TextField("カスタムタイプ", selectedPayloadType);
                    EditorGUILayout.HelpBox($"型付きデータを持つイベント: {selectedPayloadType}", MessageType.Info);
                    break;
                    
                case EventType.PlayerStateEvent:
                    EditorGUILayout.HelpBox("プレイヤー状態変更専用イベント", MessageType.Info);
                    break;
                    
                case EventType.CameraStateEvent:
                    EditorGUILayout.HelpBox("カメラ状態変更専用イベント", MessageType.Info);
                    break;
                    
                case EventType.CommandGameEvent:
                    EditorGUILayout.HelpBox("コマンドパターン統合イベント", MessageType.Info);
                    break;
            }
        }
        
        private void DrawTargetFolderSettings()
        {
            EditorGUILayout.LabelField("出力設定", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField("保存先フォルダ", targetFolder);
            if (GUILayout.Button("選択", GUILayout.Width(60)))
            {
                string selectedFolder = EditorUtility.OpenFolderPanel("保存先フォルダを選択", targetFolder, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    targetFolder = "Assets" + selectedFolder.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // よく使用されるフォルダのクイック選択
            EditorGUILayout.LabelField("クイック選択:");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Core Events"))
                targetFolder = "Assets/_Project/Core/ScriptableObjects/Events/Core/";
            if (GUILayout.Button("Player Events"))
                targetFolder = "Assets/_Project/Features/Player/ScriptableObjects/Events/";
            if (GUILayout.Button("Camera Events"))
                targetFolder = "Assets/_Project/Features/Camera/ScriptableObjects/Events/";
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPreview()
        {
            EditorGUILayout.LabelField("プレビュー", EditorStyles.boldLabel);
            
            string fileName = GetFileName();
            string fullPath = Path.Combine(targetFolder, fileName);
            
            EditorGUILayout.LabelField("作成されるファイル:");
            EditorGUILayout.SelectableLabel(fullPath, EditorStyles.textField, GUILayout.Height(18));
            
            EditorGUILayout.LabelField("イベントタイプ:");
            EditorGUILayout.SelectableLabel(GetEventClassName(), EditorStyles.textField, GUILayout.Height(18));
            
            if (!string.IsNullOrEmpty(eventDescription))
            {
                EditorGUILayout.LabelField("説明:");
                EditorGUILayout.SelectableLabel(eventDescription, EditorStyles.textArea, GUILayout.Height(36));
            }
        }
        
        private void DrawCreateButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !string.IsNullOrEmpty(eventName);
            
            if (GUILayout.Button("イベントアセットを作成", GUILayout.Height(30), GUILayout.Width(200)))
            {
                CreateEventAsset();
            }
            
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateEventAsset()
        {
            try
            {
                // フォルダが存在しない場合は作成
                if (!AssetDatabase.IsValidFolder(targetFolder))
                {
                    string[] folders = targetFolder.Split('/');
                    string currentPath = folders[0];
                    
                    for (int i = 1; i < folders.Length; i++)
                    {
                        string newPath = currentPath + "/" + folders[i];
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, folders[i]);
                        }
                        currentPath = newPath;
                    }
                }
                
                // イベントアセットを作成
                ScriptableObject eventAsset = CreateEventAssetInstance();
                
                if (eventAsset != null)
                {
                    string fileName = GetFileName();
                    string fullPath = Path.Combine(targetFolder, fileName);
                    
                    AssetDatabase.CreateAsset(eventAsset, fullPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    
                    // 作成されたアセットを選択
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = eventAsset;
                    
                    UnityEngine.Debug.Log($"イベントアセットを作成しました: {fullPath}");
                    
                    // 成功メッセージを表示
                    EditorUtility.DisplayDialog(
                        "作成完了", 
                        $"イベントアセット '{eventName}' が正常に作成されました。\n\n" +
                        $"場所: {fullPath}", 
                        "OK"
                    );
                    
                    // フィールドをリセット（連続作成用）
                    eventName = "NewGameEvent";
                    eventDescription = "";
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("エラー", $"イベントアセットの作成に失敗しました:\n{ex.Message}", "OK");
                UnityEngine.Debug.LogError($"EventAssetCreationWizard: {ex}");
            }
        }
        
        private ScriptableObject CreateEventAssetInstance()
        {
            switch (selectedEventType)
            {
                case EventType.GameEvent:
                    return ScriptableObject.CreateInstance<GameEvent>();
                    
                case EventType.GenericGameEvent:
                    return CreateGenericGameEvent();
                    
                case EventType.PlayerStateEvent:
                    return ScriptableObject.CreateInstance<PlayerStateEvent>();
                    
                case EventType.CameraStateEvent:
                    return ScriptableObject.CreateInstance<CameraStateEvent>();
                    
                case EventType.CommandGameEvent:
                    return ScriptableObject.CreateInstance<CommandGameEvent>();
                    
                default:
                    throw new System.ArgumentException($"未対応のイベントタイプ: {selectedEventType}");
            }
        }
        
        private ScriptableObject CreateGenericGameEvent()
        {
            // 型名に基づいて適切なGenericGameEventを作成
            switch (selectedPayloadType)
            {
                case "string":
                    return ScriptableObject.CreateInstance<StringGameEvent>();
                case "int":
                    return ScriptableObject.CreateInstance<IntGameEvent>();
                case "float":
                    return ScriptableObject.CreateInstance<FloatGameEvent>();
                case "bool":
                    return ScriptableObject.CreateInstance<BoolGameEvent>();
                case "Vector2":
                    return ScriptableObject.CreateInstance<Vector2GameEvent>();
                case "Vector3":
                    return ScriptableObject.CreateInstance<Vector3GameEvent>();
                case "PlayerStateType":
                    return ScriptableObject.CreateInstance<PlayerStateEvent>();
                case "CameraStateType":
                    return ScriptableObject.CreateInstance<CameraStateEvent>();
                case "ICommand":
                    return ScriptableObject.CreateInstance<CommandGameEvent>();
                default:
                    // カスタム型の場合は基本的なGameEventを作成
                    UnityEngine.Debug.LogWarning($"カスタム型 '{selectedPayloadType}' は直接サポートされていません。基本GameEventを作成します。");
                    return ScriptableObject.CreateInstance<GameEvent>();
            }
        }
        
        private string GetFileName()
        {
            return $"{eventName}.asset";
        }
        
        private string GetEventClassName()
        {
            switch (selectedEventType)
            {
                case EventType.GameEvent:
                    return "GameEvent";
                case EventType.GenericGameEvent:
                    return $"GenericGameEvent<{selectedPayloadType}>";
                case EventType.PlayerStateEvent:
                    return "PlayerStateEvent";
                case EventType.CameraStateEvent:
                    return "CameraStateEvent";
                case EventType.CommandGameEvent:
                    return "CommandGameEvent";
                default:
                    return "Unknown";
            }
        }
    }
}