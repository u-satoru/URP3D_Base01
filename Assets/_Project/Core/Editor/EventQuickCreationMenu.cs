using System.IO;
using UnityEditor;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// 繧､繝吶Φ繝医い繧ｻ繝・ヨ繧偵け繧､繝・け菴懈・縺吶ｋ縺溘ａ縺ｮ繝｡繝九Η繝ｼ繧｢繧､繝・Β
    /// 繝励Ο繧ｸ繧ｧ繧ｯ繝医え繧｣繝ｳ繝峨え縺九ｉ蜿ｳ繧ｯ繝ｪ繝・け縺ｧ邏譌ｩ縺丈ｽ懈・蜿ｯ閭ｽ
    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - Assets/Create 繝｡繝九Η繝ｼ縺ｸ縺ｮ繧､繝吶Φ繝井ｽ懈・繝｡繝九Η繝ｼ霑ｽ蜉
    /// - 蝓ｺ譛ｬ繧､繝吶Φ繝茨ｼ・ameEvent・峨°繧牙ｰら畑繧､繝吶Φ繝医∪縺ｧ蟷・ｹ・＞繧ｿ繧､繝励ｒ繧ｵ繝昴・繝・    /// - 迴ｾ蝨ｨ驕ｸ謚樔ｸｭ縺ｮ繝輔か繝ｫ繝縺ｸ縺ｮ閾ｪ蜍穂ｿ晏ｭ・    /// - 繝輔ぃ繧､繝ｫ蜷阪・驥崎､・メ繧ｧ繝・け縺ｨ閾ｪ蜍輔Μ繝阪・繝
    /// - 繧医￥菴ｿ逕ｨ縺輔ｌ繧九う繝吶Φ繝医・繝・Φ繝励Ξ繝ｼ繝医→繝舌ャ繝∽ｽ懈・
    /// - 繧ｷ繝ｧ繝ｼ繝医き繝・ヨ繧ｭ繝ｼ蟇ｾ蠢懶ｼ・trl+Shift+E・・    /// 
    /// 繧ｵ繝昴・繝医＆繧後ｋ繧､繝吶Φ繝医ち繧､繝暦ｼ・    /// - 蝓ｺ譛ｬ繧ｿ繧､繝・ GameEvent, String/Int/Float/Bool/Vector2/Vector3繧､繝吶Φ繝・    /// - 蟆ら畑繧ｿ繧､繝・ PlayerState/CameraState/GameState/Command繧､繝吶Φ繝・    /// - 繝・Φ繝励Ξ繝ｼ繝・ Health/Level/Item/Damage繧､繝吶Φ繝・    /// - 繝・ヰ繝・げ逕ｨ: DebugLog/PerformanceWarning繧､繝吶Φ繝・    /// 
    /// 繧｢繧ｯ繧ｻ繧ｹ譁ｹ豕包ｼ・    /// 1. 繝励Ο繧ｸ繧ｧ繧ｯ繝医え繧｣繝ｳ繝峨え縺ｧ蜿ｳ繧ｯ繝ｪ繝・け > Create > Unity6 Events > ...
    /// 2. Unity 繝｡繝九Η繝ｼ > Assets > Create > Unity6 Events > ...
    /// </summary>
    public static class EventQuickCreationMenu
    {
        private const string BaseMenuPath = "Assets/Create/Unity6 Events/";
        private const string DefaultEventPath = "Assets/_Project/Core/ScriptableObjects/Events/Core/";
        
        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ繝ｼ縺ｪ縺励・蝓ｺ譛ｬGameEvent繧｢繧ｻ繝・ヨ繧剃ｽ懈・
        /// 繧ｷ繝ｳ繝励Ν縺ｪ騾夂衍繧､繝吶Φ繝医↓譛驕ｩ
        /// </summary>
        /// <remarks>
        /// 菴ｿ逕ｨ萓具ｼ壹ご繝ｼ繝髢句ｧ九√・繝ｼ繧ｺ縲√Ξ繝吶Ν繧ｯ繝ｪ繧｢遲峨・繧ｷ繝ｳ繝励Ν縺ｪ迥ｶ諷句､牙喧
        /// </remarks>
        // 蝓ｺ譛ｬ逧・↑GameEvent
        [MenuItem(BaseMenuPath + "Game Event")]
        public static void CreateGameEvent()
        {
            CreateEventAsset<GameEvent>("NewGameEvent");
        }
        
        // 蝙倶ｻ倥″繧､繝吶Φ繝・- 蝓ｺ譛ｬ蝙・        [MenuItem(BaseMenuPath + "Generic Events/String Event")]
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
        
        // Vector蝙九う繝吶Φ繝・        [MenuItem(BaseMenuPath + "Vector Events/Vector2 Event")]
        public static void CreateVector2Event()
        {
            CreateEventAsset<Vector2GameEvent>("NewVector2Event");
        }
        
        [MenuItem(BaseMenuPath + "Vector Events/Vector3 Event")]
        public static void CreateVector3Event()
        {
            CreateEventAsset<Vector3GameEvent>("NewVector3Event");
        }
        
        // GameObject蝙九う繝吶Φ繝・        [MenuItem(BaseMenuPath + "Object Events/GameObject Event")]
        public static void CreateGameObjectEvent()
        {
            CreateEventAsset<GameObjectGameEvent>("NewGameObjectEvent");
        }
        
        // 蟆ら畑繧､繝吶Φ繝・        [MenuItem(BaseMenuPath + "Specialized Events/Player State Event")]
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
        
        // Unity Input System邨ｱ蜷・        [MenuItem(BaseMenuPath + "Input Events/Input Vector2 Event")]
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
        
        // 繧医￥菴ｿ逕ｨ縺輔ｌ繧九う繝吶Φ繝医・繝・Φ繝励Ξ繝ｼ繝・        [MenuItem(BaseMenuPath + "Common Templates/Health Changed Event")]
        public static void CreateHealthChangedEvent()
        {
            var healthEvent = CreateEventAsset<IntGameEvent>("OnHealthChanged");
            SetEventDescription(healthEvent, "繝励Ξ繧､繝､繝ｼ縺ｾ縺溘・謨ｵ縺ｮHealth蛟､縺悟､画峩縺輔ｌ縺溘→縺阪↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝・);
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Level Complete Event")]
        public static void CreateLevelCompleteEvent()
        {
            var levelEvent = CreateEventAsset<GameEvent>("OnLevelComplete");
            SetEventDescription(levelEvent, "繝ｬ繝吶Ν繧ｯ繝ｪ繧｢譎ゅ↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝・);
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Item Collected Event")]
        public static void CreateItemCollectedEvent()
        {
            var itemEvent = CreateEventAsset<StringGameEvent>("OnItemCollected");
            SetEventDescription(itemEvent, "繧｢繧､繝・Β蜿朱寔譎ゅ↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝医ゅい繧､繝・Β蜷阪′繝壹う繝ｭ繝ｼ繝峨→縺励※騾√ｉ繧後ｋ");
        }
        
        [MenuItem(BaseMenuPath + "Common Templates/Damage Dealt Event")]
        public static void CreateDamageDealtEvent()
        {
            var damageEvent = CreateEventAsset<IntGameEvent>("OnDamageDealt");
            SetEventDescription(damageEvent, "繝繝｡繝ｼ繧ｸ縺御ｸ弱∴繧峨ｌ縺溘→縺阪↓逋ｺ陦後＆繧後ｋ繧､繝吶Φ繝医ゅム繝｡繝ｼ繧ｸ驥上′繝壹う繝ｭ繝ｼ繝・);
        }
        
        // 繝・ヰ繝・げ逕ｨ繧､繝吶Φ繝・        [MenuItem(BaseMenuPath + "Debug Events/Debug Log Event")]
        public static void CreateDebugLogEvent()
        {
            var debugEvent = CreateEventAsset<StringGameEvent>("OnDebugLog");
            SetEventDescription(debugEvent, "繝・ヰ繝・げ逕ｨ繝ｭ繧ｰ蜃ｺ蜉帙う繝吶Φ繝・);
        }
        
        [MenuItem(BaseMenuPath + "Debug Events/Performance Warning Event")]
        public static void CreatePerformanceWarningEvent()
        {
            var perfEvent = CreateEventAsset<FloatGameEvent>("OnPerformanceWarning");
            SetEventDescription(perfEvent, "繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ隴ｦ蜻翫う繝吶Φ繝医ゅヵ繝ｬ繝ｼ繝譎る俣縺後・繧､繝ｭ繝ｼ繝・);
        }
        
        // 繝舌ャ繝∽ｽ懈・
        [MenuItem(BaseMenuPath + "Batch Creation/Create Common Event Set")]
        public static void CreateCommonEventSet()
        {
            if (EditorUtility.DisplayDialog(
                "繝舌ャ繝∽ｽ懈・",
                "繧医￥菴ｿ逕ｨ縺輔ｌ繧句渕譛ｬ逧・↑繧､繝吶Φ繝医そ繝・ヨ繧剃ｽ懈・縺励∪縺吶°・歃n\n" +
                "莉･荳九・繧､繝吶Φ繝医′菴懈・縺輔ｌ縺ｾ縺・\n" +
                "窶｢ OnGameStart (GameEvent)\n" +
                "窶｢ OnGamePause (BoolGameEvent)\n" +
                "窶｢ OnScoreChanged (IntGameEvent)\n" +
                "窶｢ OnHealthChanged (FloatGameEvent)\n" +
                "窶｢ OnPlayerDied (GameEvent)",
                "菴懈・", "繧ｭ繝｣繝ｳ繧ｻ繝ｫ"))
            {
                CreateEventAsset<GameEvent>("OnGameStart");
                CreateEventAsset<BoolGameEvent>("OnGamePause");
                CreateEventAsset<IntGameEvent>("OnScoreChanged");
                CreateEventAsset<FloatGameEvent>("OnHealthChanged");
                CreateEventAsset<GameEvent>("OnPlayerDied");
                
                UnityEngine.Debug.Log("蝓ｺ譛ｬ逧・↑繧､繝吶Φ繝医そ繝・ヨ繧剃ｽ懈・縺励∪縺励◆縲・);
                AssetDatabase.Refresh();
            }
        }
        
        // 繝励Λ繧､繝吶・繝・繝倥Ν繝代・繝｡繧ｽ繝・ラ
        private static T CreateEventAsset<T>(string eventName) where T : ScriptableObject
        {
            // 迴ｾ蝨ｨ驕ｸ謚槭＆繧後※縺・ｋ繝輔か繝ｫ繝繧貞叙蠕励√↑縺代ｌ縺ｰ繝・ヵ繧ｩ繝ｫ繝医ヱ繧ｹ菴ｿ逕ｨ
            string targetPath = GetSelectedFolderPath();
            
            // 繝輔か繝ｫ繝縺悟ｭ伜惠縺励↑縺・ｴ蜷医・菴懈・
            EnsureFolderExists(targetPath);
            
            // 蜷悟錐繝輔ぃ繧､繝ｫ縺後≠繧句ｴ蜷医・逡ｪ蜿ｷ繧定ｿｽ蜉
            string uniqueName = GetUniqueFileName(targetPath, eventName);
            
            // ScriptableObject繧､繝ｳ繧ｹ繧ｿ繝ｳ繧ｹ繧剃ｽ懈・
            T instance = ScriptableObject.CreateInstance<T>();
            
            // 繝輔ぃ繧､繝ｫ繝代せ繧呈ｧ狗ｯ・            string fullPath = Path.Combine(targetPath, $"{uniqueName}.asset");
            
            // 繧｢繧ｻ繝・ヨ繧剃ｽ懈・
            AssetDatabase.CreateAsset(instance, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 菴懈・縺輔ｌ縺溘い繧ｻ繝・ヨ繧帝∈謚・            Selection.activeObject = instance;
            EditorUtility.FocusProjectWindow();
            
            UnityEngine.Debug.Log($"繧､繝吶Φ繝医い繧ｻ繝・ヨ繧剃ｽ懈・縺励∪縺励◆: {fullPath}");
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
            // 迴ｾ蝨ｨ縺ｯ繝ｭ繧ｰ縺ｫ險倬鹸
            UnityEngine.Debug.Log($"{eventAsset.name}: {description}");
        }
        
        // 驕ｸ謚槭＆繧後◆繧ｪ繝悶ず繧ｧ繧ｯ繝医′繝輔か繝ｫ繝縺九←縺・°繧堤｢ｺ隱阪☆繧九Γ繝九Η繝ｼ讀懆ｨｼ
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
        
        // 繧ｳ繝ｳ繝・く繧ｹ繝医Γ繝九Η繝ｼ・亥承繧ｯ繝ｪ繝・け繝｡繝九Η繝ｼ・峨・繝舌Μ繝・・繧ｷ繝ｧ繝ｳ
        [MenuItem("CONTEXT/Transform/Create Player State Event")]
        public static void CreatePlayerStateEventFromContext()
        {
            CreateEventAsset<PlayerStateEvent>("PlayerStateEvent");
        }
        
        // 繧ｷ繝ｧ繝ｼ繝医き繝・ヨ繧ｭ繝ｼ莉倥″繝｡繝九Η繝ｼ
        [MenuItem(BaseMenuPath + "Quick Game Event %#e")] // Ctrl+Shift+E
        public static void CreateGameEventWithShortcut()
        {
            CreateEventAsset<GameEvent>("QuickGameEvent");
        }
    }
}