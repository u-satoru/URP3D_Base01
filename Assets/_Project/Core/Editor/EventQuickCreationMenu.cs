using System.IO;
using UnityEditor;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// イベントアセチE��をクイチE��作�EするためのメニューアイチE��
    /// プロジェクトウィンドウから右クリチE��で素早く作�E可能
    /// 
    /// 主な機�E�E�E    /// - Assets/Create メニューへのイベント作�Eメニュー追加
    /// - 基本イベント！EameEvent�E�から専用イベントまで幁E��E��タイプをサポ�EチE    /// - 現在選択中のフォルダへの自動保孁E    /// - ファイル名�E重褁E��ェチE��と自動リネ�Eム
    /// - よく使用されるイベント�EチE��プレートとバッチ作�E
    /// - ショートカチE��キー対応！Etrl+Shift+E�E�E    /// 
    /// サポ�Eトされるイベントタイプ！E    /// - 基本タイチE GameEvent, String/Int/Float/Bool/Vector2/Vector3イベンチE    /// - 専用タイチE PlayerState/CameraState/GameState/CommandイベンチE    /// - チE��プレーチE Health/Level/Item/DamageイベンチE    /// - チE��チE��用: DebugLog/PerformanceWarningイベンチE    /// 
    /// アクセス方法！E    /// 1. プロジェクトウィンドウで右クリチE�� > Create > Unity6 Events > ...
    /// 2. Unity メニュー > Assets > Create > Unity6 Events > ...
    /// </summary>
    public static class EventQuickCreationMenu
    {
        private const string BaseMenuPath = "Assets/Create/Unity6 Events/";
        private const string DefaultEventPath = "Assets/_Project/Core/ScriptableObjects/Events/Core/";
        
        /// <summary>
        /// パラメーターなし�E基本GameEventアセチE��を作�E
        /// シンプルな通知イベントに最適
        /// </summary>
        /// <remarks>
        /// 使用例：ゲーム開始、�Eーズ、レベルクリア等�Eシンプルな状態変化
        /// </remarks>
        // 基本皁E��GameEvent
        [MenuItem(BaseMenuPath + "Game Event")]
        public static void CreateGameEvent()
        {
            CreateEventAsset<GameEvent>("NewGameEvent");
        }
        
        // 型付きイベンチE- 基本垁E        [MenuItem(BaseMenuPath + "Generic Events/String Event")]
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
        
        // Vector型イベンチE        [MenuItem(BaseMenuPath + "Vector Events/Vector2 Event")]
        public static void CreateVector2Event()
        {
            CreateEventAsset<Vector2GameEvent>("NewVector2Event");
        }
        
        [MenuItem(BaseMenuPath + "Vector Events/Vector3 Event")]
        public static void CreateVector3Event()
        {
            CreateEventAsset<Vector3GameEvent>("NewVector3Event");
        }
        
        // GameObject型イベンチE        [MenuItem(BaseMenuPath + "Object Events/GameObject Event")]
        public static void CreateGameObjectEvent()
        {
            CreateEventAsset<GameObjectGameEvent>("NewGameObjectEvent");
        }
        
        // 専用イベンチE        [MenuItem(BaseMenuPath + "Specialized Events/Player State Event")]
        public static void CreatePlayerStateEvent()
        {
            CreateEventAsset<PlayerStateEvent>("PlayerStateEvent");
        }
        
        [MenuItem(BaseMenuPath + "Specialized Events/Camera State Event")]
        public static void CreateCameraStateEvent()
        {
            CreateEventAsset<CameraStateEvent>("CameraStateEvent");
        }
        
        [MenuItem(BaseMenuPath + "Specialized Events/Game State Event")]
        public static void CreateGameStateEvent()
        {
            CreateEventAsset<GameStateEvent>("GameStateEvent");
        }
        
        [MenuItem(BaseMenuPath + "Specialized Events/Command Event")]
        public static void CreateCommandEvent()
        {
            CreateEventAsset<CommandGameEvent>("NewCommandEvent");
        }
        
        // Unity Input System統吁E        [MenuItem(BaseMenuPath + "Input Events/Input Vector2 Event")]
        public static void CreateInputVector2Event()
        {
            CreateEventAsset<Vector2GameEvent>("InputVector2Event");
        }
        
        [MenuItem(BaseMenuPath + "Input Events/Input Float Event")]
        public static void CreateInputFloatEvent()
        {
            CreateEventAsset<FloatGameEvent>("InputFloatEvent");
        }
        
        [MenuItem(BaseMenuPath + "Input Events/Input Bool Event")]
        public static void CreateInputBoolEvent()
        {
            CreateEventAsset<BoolGameEvent>("InputBoolEvent");
        }
        
        // よく使用されるイベント�EチE��プレーチE        [MenuItem(BaseMenuPath + "Common Templates/Health Changed Event")]
        public static void CreateHealthChangedEvent()
        {
            var healthEvent = CreateEventAsset<IntGameEvent>("OnHealthChanged");
            SetEventDescription(healthEvent, "プレイヤーまた�E敵のHealth値が変更されたときに発行されるイベンチE);
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Level Complete Event")]
        public static void CreateLevelCompleteEvent()
        {
            var levelEvent = CreateEventAsset<GameEvent>("OnLevelComplete");
            SetEventDescription(levelEvent, "レベルクリア時に発行されるイベンチE);
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Item Collected Event")]
        public static void CreateItemCollectedEvent()
        {
            var itemEvent = CreateEventAsset<StringGameEvent>("OnItemCollected");
            SetEventDescription(itemEvent, "アイチE��収集時に発行されるイベント。アイチE��名がペイロードとして送られる");
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Damage Dealt Event")]
        public static void CreateDamageDealtEvent()
        {
            var damageEvent = CreateEventAsset<IntGameEvent>("OnDamageDealt");
            SetEventDescription(damageEvent, "ダメージが与えられたときに発行されるイベント。ダメージ量がペイローチE);
        }
        
        // チE��チE��用イベンチE        [MenuItem(BaseMenuPath + "Debug Events/Debug Log Event")]
        public static void CreateDebugLogEvent()
        {
            var debugEvent = CreateEventAsset<StringGameEvent>("OnDebugLog");
            SetEventDescription(debugEvent, "チE��チE��用ログ出力イベンチE);
        }
        
        [MenuItem(BaseMenuPath + "Debug Events/Performance Warning Event")]
        public static void CreatePerformanceWarningEvent()
        {
            var perfEvent = CreateEventAsset<FloatGameEvent>("OnPerformanceWarning");
            SetEventDescription(perfEvent, "パフォーマンス警告イベント。フレーム時間が�EイローチE);
        }
        
        // バッチ作�E
        [MenuItem(BaseMenuPath + "Batch Creation/Create Common Event Set")]
        public static void CreateCommonEventSet()
        {
            if (EditorUtility.DisplayDialog(
                "バッチ作�E",
                "よく使用される基本皁E��イベントセチE��を作�Eしますか�E�\n\n" +
                "以下�Eイベントが作�EされまぁE\n" +
                "• OnGameStart (GameEvent)\n" +
                "• OnGamePause (BoolGameEvent)\n" +
                "• OnScoreChanged (IntGameEvent)\n" +
                "• OnHealthChanged (FloatGameEvent)\n" +
                "• OnPlayerDied (GameEvent)",
                "作�E", "キャンセル"))
            {
                CreateEventAsset<GameEvent>("OnGameStart");
                CreateEventAsset<BoolGameEvent>("OnGamePause");
                CreateEventAsset<IntGameEvent>("OnScoreChanged");
                CreateEventAsset<FloatGameEvent>("OnHealthChanged");
                CreateEventAsset<GameEvent>("OnPlayerDied");
                
                UnityEngine.Debug.Log("基本皁E��イベントセチE��を作�Eしました、E);
                AssetDatabase.Refresh();
            }
        }
        
        // プライベ�EチEヘルパ�EメソチE��
        private static T CreateEventAsset<T>(string eventName) where T : ScriptableObject
        {
            // 現在選択されてぁE��フォルダを取得、なければチE��ォルトパス使用
            string targetPath = GetSelectedFolderPath();
            
            // フォルダが存在しなぁE��合�E作�E
            EnsureFolderExists(targetPath);
            
            // 同名ファイルがある場合�E番号を追加
            string uniqueName = GetUniqueFileName(targetPath, eventName);
            
            // ScriptableObjectインスタンスを作�E
            T instance = ScriptableObject.CreateInstance<T>();
            
            // ファイルパスを構篁E            string fullPath = Path.Combine(targetPath, $"{uniqueName}.asset");
            
            // アセチE��を作�E
            AssetDatabase.CreateAsset(instance, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 作�EされたアセチE��を選抁E            Selection.activeObject = instance;
            EditorUtility.FocusProjectWindow();
            
            UnityEngine.Debug.Log($"イベントアセチE��を作�Eしました: {fullPath}");
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
            // 現在はログに記録
            UnityEngine.Debug.Log($"{eventAsset.name}: {description}");
        }
        
        // 選択されたオブジェクトがフォルダかどぁE��を確認するメニュー検証
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
        
        // コンチE��ストメニュー�E�右クリチE��メニュー�E��EバリチE�Eション
        [MenuItem("CONTEXT/Transform/Create Player State Event")]
        public static void CreatePlayerStateEventFromContext()
        {
            CreateEventAsset<PlayerStateEvent>("PlayerStateEvent");
        }
        
        // ショートカチE��キー付きメニュー
        [MenuItem(BaseMenuPath + "Quick Game Event %#e")] // Ctrl+Shift+E
        public static void CreateGameEventWithShortcut()
        {
            CreateEventAsset<GameEvent>("QuickGameEvent");
        }
    }
}