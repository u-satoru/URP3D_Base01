using System.IO;
using UnityEditor;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// イベントアセットをクイック作成するためのメニューアイテム
    /// プロジェクトウィンドウから右クリックで素早く作成可能
    /// </summary>
    public static class EventQuickCreationMenu
    {
        private const string BaseMenuPath = "Assets/Create/Unity6 Events/";
        private const string DefaultEventPath = "Assets/_Project/Core/ScriptableObjects/Events/Core/";
        
        // 基本的なGameEvent
        [MenuItem(BaseMenuPath + "Game Event")]
        public static void CreateGameEvent()
        {
            CreateEventAsset<GameEvent>("NewGameEvent");
        }
        
        // 型付きイベント - 基本型
        [MenuItem(BaseMenuPath + "Generic Events/String Event")]
        public static void CreateStringEvent()
        {
            CreateEventAsset<StringGameEvent>("NewStringEvent");
        }
        
        [MenuItem(BaseMenuPath + "Generic Events/Int Event")]
        public static void CreateIntEvent()
        {
            CreateEventAsset<IntGameEvent>("NewIntEvent");
        }
        
        [MenuItem(BaseMenuPath + "Generic Events/Float Event")]
        public static void CreateFloatEvent()
        {
            CreateEventAsset<FloatGameEvent>("NewFloatEvent");
        }
        
        [MenuItem(BaseMenuPath + "Generic Events/Bool Event")]
        public static void CreateBoolEvent()
        {
            CreateEventAsset<BoolGameEvent>("NewBoolEvent");
        }
        
        // Vector型イベント
        [MenuItem(BaseMenuPath + "Vector Events/Vector2 Event")]
        public static void CreateVector2Event()
        {
            CreateEventAsset<Vector2GameEvent>("NewVector2Event");
        }
        
        [MenuItem(BaseMenuPath + "Vector Events/Vector3 Event")]
        public static void CreateVector3Event()
        {
            CreateEventAsset<Vector3GameEvent>("NewVector3Event");
        }
        
        // 専用イベント
        [MenuItem(BaseMenuPath + "Specialized Events/Player State Event")]
        public static void CreatePlayerStateEvent()
        {
            CreateEventAsset<PlayerStateEvent>("NewPlayerStateEvent");
        }
        
        [MenuItem(BaseMenuPath + "Specialized Events/Camera State Event")]
        public static void CreateCameraStateEvent()
        {
            CreateEventAsset<CameraStateEvent>("NewCameraStateEvent");
        }
        
        [MenuItem(BaseMenuPath + "Specialized Events/Command Event")]
        public static void CreateCommandEvent()
        {
            CreateEventAsset<CommandGameEvent>("NewCommandEvent");
        }
        
        // Unity Input System統合
        [MenuItem(BaseMenuPath + "Input Events/Input Action Event")]
        public static void CreateInputActionEvent()
        {
            // InputActionに対応したカスタムイベント
            CreateEventAsset<Vector2GameEvent>("NewInputActionEvent");
        }
        
        // よく使用されるイベントのテンプレート
        [MenuItem(BaseMenuPath + "Common Templates/Health Changed Event")]
        public static void CreateHealthChangedEvent()
        {
            var healthEvent = CreateEventAsset<IntGameEvent>("OnHealthChanged");
            SetEventDescription(healthEvent, "プレイヤーまたは敵のHealth値が変更されたときに発行されるイベント");
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Level Complete Event")]
        public static void CreateLevelCompleteEvent()
        {
            var levelEvent = CreateEventAsset<GameEvent>("OnLevelComplete");
            SetEventDescription(levelEvent, "レベルクリア時に発行されるイベント");
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Item Collected Event")]
        public static void CreateItemCollectedEvent()
        {
            var itemEvent = CreateEventAsset<StringGameEvent>("OnItemCollected");
            SetEventDescription(itemEvent, "アイテム収集時に発行されるイベント。アイテム名がペイロードとして送られる");
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Damage Dealt Event")]
        public static void CreateDamageDealtEvent()
        {
            var damageEvent = CreateEventAsset<IntGameEvent>("OnDamageDealt");
            SetEventDescription(damageEvent, "ダメージが与えられたときに発行されるイベント。ダメージ量がペイロード");
        }
        
        // デバッグ用イベント
        [MenuItem(BaseMenuPath + "Debug Events/Debug Log Event")]
        public static void CreateDebugLogEvent()
        {
            var debugEvent = CreateEventAsset<StringGameEvent>("OnDebugLog");
            SetEventDescription(debugEvent, "デバッグ用ログ出力イベント");
        }
        
        [MenuItem(BaseMenuPath + "Debug Events/Performance Warning Event")]
        public static void CreatePerformanceWarningEvent()
        {
            var perfEvent = CreateEventAsset<FloatGameEvent>("OnPerformanceWarning");
            SetEventDescription(perfEvent, "パフォーマンス警告イベント。フレーム時間がペイロード");
        }
        
        // バッチ作成
        [MenuItem(BaseMenuPath + "Batch Creation/Create Common Event Set")]
        public static void CreateCommonEventSet()
        {
            if (EditorUtility.DisplayDialog(
                "バッチ作成",
                "よく使用される基本的なイベントセットを作成しますか？\n\n" +
                "以下のイベントが作成されます:\n" +
                "• OnGameStart (GameEvent)\n" +
                "• OnGamePause (BoolGameEvent)\n" +
                "• OnScoreChanged (IntGameEvent)\n" +
                "• OnHealthChanged (FloatGameEvent)\n" +
                "• OnPlayerDied (GameEvent)",
                "作成", "キャンセル"))
            {
                CreateEventAsset<GameEvent>("OnGameStart");
                CreateEventAsset<BoolGameEvent>("OnGamePause");
                CreateEventAsset<IntGameEvent>("OnScoreChanged");
                CreateEventAsset<FloatGameEvent>("OnHealthChanged");
                CreateEventAsset<GameEvent>("OnPlayerDied");
                
                UnityEngine.Debug.Log("基本的なイベントセットを作成しました。");
                AssetDatabase.Refresh();
            }
        }
        
        // プライベート ヘルパーメソッド
        private static T CreateEventAsset<T>(string eventName) where T : ScriptableObject
        {
            // 現在選択されているフォルダを取得、なければデフォルトパス使用
            string targetPath = GetSelectedFolderPath();
            
            // フォルダが存在しない場合は作成
            EnsureFolderExists(targetPath);
            
            // 同名ファイルがある場合は番号を追加
            string uniqueName = GetUniqueFileName(targetPath, eventName);
            
            // ScriptableObjectインスタンスを作成
            T instance = ScriptableObject.CreateInstance<T>();
            
            // ファイルパスを構築
            string fullPath = Path.Combine(targetPath, $"{uniqueName}.asset");
            
            // アセットを作成
            AssetDatabase.CreateAsset(instance, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 作成されたアセットを選択
            Selection.activeObject = instance;
            EditorUtility.FocusProjectWindow();
            
            UnityEngine.Debug.Log($"イベントアセットを作成しました: {fullPath}");
            return instance;
        }
        
        private static string GetSelectedFolderPath()
        {
            string selectedPath = DefaultEventPath;
            
            if (Selection.activeObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    selectedPath = assetPath;
                }
                else if (File.Exists(assetPath))
                {
                    selectedPath = Path.GetDirectoryName(assetPath);
                }
            }
            
            return selectedPath;
        }
        
        private static void EnsureFolderExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0]; // "Assets"
                
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
        }
        
        private static string GetUniqueFileName(string folderPath, string baseName)
        {
            string testPath = Path.Combine(folderPath, $"{baseName}.asset");
            
            if (!File.Exists(testPath))
                return baseName;
            
            int counter = 1;
            string uniqueName;
            
            do
            {
                uniqueName = $"{baseName}_{counter:D2}";
                testPath = Path.Combine(folderPath, $"{uniqueName}.asset");
                counter++;
            }
            while (File.Exists(testPath) && counter < 100);
            
            return uniqueName;
        }
        
        private static void SetEventDescription(ScriptableObject eventAsset, string description)
        {
            // ScriptableObjectに説明を設定する方法は限定的だが、
            // 将来的にカスタムフィールドを追加する際の拡張ポイント
            // 現在はログに記録
            UnityEngine.Debug.Log($"{eventAsset.name}: {description}");
        }
        
        // 選択されたオブジェクトがフォルダかどうかを確認するメニュー検証
        [MenuItem(BaseMenuPath + "Game Event", true)]
        private static bool ValidateCreateGameEvent()
        {
            return IsValidFolderSelected();
        }
        
        private static bool IsValidFolderSelected()
        {
            if (Selection.activeObject == null)
                return true;
                
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path) || File.Exists(path);
        }
        
        // コンテキストメニュー（右クリックメニュー）のバリデーション
        [MenuItem("CONTEXT/Transform/Create Player State Event")]
        public static void CreatePlayerStateEventFromContext()
        {
            CreateEventAsset<PlayerStateEvent>("PlayerStateEvent");
        }
        
        // ショートカットキー付きメニュー
        [MenuItem(BaseMenuPath + "Quick Game Event %#e")] // Ctrl+Shift+E
        public static void CreateGameEventWithShortcut()
        {
            CreateEventAsset<GameEvent>("QuickGameEvent");
        }
    }
}