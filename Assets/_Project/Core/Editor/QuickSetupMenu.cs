using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.Rendering;
// using Debug = UnityEngine.Debug;


namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// プロジェクトの迅速なセットアップを提供するエディタメニュー
    /// </summary>
    public static class QuickSetupMenu
    {
        private const string MENU_ROOT = "Project/Quick Setup/";
        private const string PREFAB_PATH = "Assets/_Project/Core/Prefabs/Templates/";
        private const string SCENE_PATH = "Assets/_Project/Scenes/";

        [MenuItem(MENU_ROOT + "1. Create Default Scene", priority = 1)]
        public static void CreateDefaultScene()
        {
            // 新しいシーンを作成
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // シーンにデフォルトオブジェクトを追加
            SetupDefaultSceneObjects();
            
            // シーンを保存
            string scenePath = $"{SCENE_PATH}DefaultGameplayScene.unity";
            EnsureDirectoryExists(Path.GetDirectoryName(scenePath));
            EditorSceneManager.SaveScene(newScene, scenePath);
            
            UnityEngine.Debug.Log($"✅ Default scene created at: {scenePath}");
        }

        [MenuItem(MENU_ROOT + "2. Setup Player Character", priority = 2)]
        public static void SetupPlayerCharacter()
        {
            // DefaultPlayerプリファブを探す
            string[] prefabGuids = AssetDatabase.FindAssets("DefaultPlayer t:Prefab");
            if (prefabGuids.Length == 0)
            {
                UnityEngine.Debug.LogWarning("❌ DefaultPlayer.prefab not found. Please ensure the prefab exists in Core/Prefabs/Templates/");
                return;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            // 既存のプレイヤーが存在するかチェック
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                if (EditorUtility.DisplayDialog("Player Already Exists", 
                    "A Player object already exists in the scene. Replace it?", "Replace", "Cancel"))
                {
                    Object.DestroyImmediate(existingPlayer);
                }
                else
                {
                    return;
                }
            }

            // プレイヤーをシーンにインスタンス化
            GameObject playerInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            playerInstance.transform.position = Vector3.zero;
            
            // Selectionに設定
            Selection.activeGameObject = playerInstance;
            
            UnityEngine.Debug.Log("✅ Player character setup complete");
        }

        [MenuItem(MENU_ROOT + "3. Create Game Manager", priority = 3)]
        public static void CreateGameManager()
        {
            // GameManagerが既に存在するかチェック - 循環依存回避のため一時的にコメントアウト
            // TODO: GameManagerはFeaturesアセンブリにあるため、Coreから直接参照できない
            // GameObject existingGameManager = GameObject.FindFirstObjectByType<asterivo.Unity60.Core.GameManager>()?.gameObject;
            GameObject existingGameManager = null; // 一時的な回避策
            if (existingGameManager != null)
            {
                UnityEngine.Debug.LogWarning("⚠️ GameManager already exists in the scene");
                Selection.activeGameObject = existingGameManager;
                return;
            }

            // GameManagerプリファブを探す
            string[] prefabGuids = AssetDatabase.FindAssets("GameManager t:Prefab");
            if (prefabGuids.Length == 0)
            {
                UnityEngine.Debug.LogWarning("❌ GameManager.prefab not found. Creating empty GameManager object...");
                CreateEmptyGameManager();
                return;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            // GameManagerをシーンにインスタンス化
            GameObject gameManagerInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            
            // Selectionに設定
            Selection.activeGameObject = gameManagerInstance;
            
            UnityEngine.Debug.Log("✅ Game Manager created successfully");
        }

        [MenuItem(MENU_ROOT + "4. Validate Project Setup", priority = 4)]
        public static void ValidateProjectSetup()
        {
            UnityEngine.Debug.Log("🔍 Starting Project Setup Validation...");
            
            int issues = 0;
            
            // 必要なフォルダの存在チェック
            string[] requiredFolders = {
                "Assets/_Project/Core",
                "Assets/_Project/Features",
                "Assets/_Project/Scenes",
                "Assets/_Project/Core/Prefabs/Templates"
            };
            
            foreach (string folder in requiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    UnityEngine.Debug.LogError($"❌ Missing required folder: {folder}");
                    issues++;
                }
            }
            
            // 必要なプリファブの存在チェック
            string[] requiredPrefabs = {
                "DefaultPlayer",
                "GameManager",
                "UICanvas",
                "AudioManager"
            };
            
            foreach (string prefabName in requiredPrefabs)
            {
                string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
                if (guids.Length == 0)
                {
                    UnityEngine.Debug.LogWarning($"⚠️ Missing prefab: {prefabName}.prefab");
                    issues++;
                }
            }
            
            // Input System設定チェック
            string[] inputAssets = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
            if (inputAssets.Length == 0)
            {
                UnityEngine.Debug.LogWarning("⚠️ InputSystem_Actions.inputactions not found");
                issues++;
            }
            
            // プレイヤー設定チェック
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.LandscapeLeft)
            {
                UnityEngine.Debug.Log("ℹ️ Consider setting Default Orientation to Landscape Left for better mobile experience");
            }
            
            if (issues == 0)
            {
                UnityEngine.Debug.Log("✅ Project setup validation completed successfully! No issues found.");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"⚠️ Validation completed with {issues} issues. Please review the warnings above.");
            }
        }

        [MenuItem(MENU_ROOT + "5. Fix Common Issues", priority = 5)]
        public static void FixCommonIssues()
        {
            UnityEngine.Debug.Log("🔧 Fixing common project issues...");
            
            int fixedIssues = 0;
            
            // 必要なフォルダを作成
            string[] requiredFolders = {
                "Assets/_Project/Scenes/Templates",
                "Assets/_Project/Samples",
                "Assets/_Project/Tests",
                "Assets/_Project/Core/Editor/Documentation"
            };
            
            foreach (string folder in requiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    EnsureDirectoryExists(folder);
                    UnityEngine.Debug.Log($"✅ Created missing folder: {folder}");
                    fixedIssues++;
                }
            }
            
            // Player設定の最適化
            if (PlayerSettings.colorSpace != ColorSpace.Linear)
            {
                PlayerSettings.colorSpace = ColorSpace.Linear;
                UnityEngine.Debug.Log("✅ Set Color Space to Linear");
                fixedIssues++;
            }
            
            // Graphics設定の最適化
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (QualitySettings.renderPipeline == null)
            {
                // URPアセットを探して設定
                string[] urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
                if (urpAssets.Length > 0)
                {
                    string urpPath = AssetDatabase.GUIDToAssetPath(urpAssets[0]);
                    var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(urpPath);
                    QualitySettings.renderPipeline = urpAsset;
                    UnityEngine.Debug.Log($"✅ Set URP asset: {urpPath}");
                    fixedIssues++;
                }
            }
            
            // アセットデータベースをリフレッシュ
            AssetDatabase.Refresh();
            
            UnityEngine.Debug.Log($"✅ Fixed {fixedIssues} common issues");
        }
        
        [MenuItem(MENU_ROOT + "Reset All Settings", priority = 100)]
        public static void ResetAllSettings()
        {
            if (EditorUtility.DisplayDialog("Reset Project Settings", 
                "This will reset all project settings to defaults. Are you sure?", "Reset", "Cancel"))
            {
                // プレイヤー設定のリセット
                PlayerSettings.colorSpace = ColorSpace.Gamma;
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                
                UnityEngine.Debug.Log("✅ Project settings reset to defaults");
            }
        }

        // ヘルパーメソッド
        private static void SetupDefaultSceneObjects()
        {
            // デフォルトのライトとカメラが既に存在する場合はそのまま使用
            
            // Directional Lightの設定を最適化
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights.Where(l => l.type == LightType.Directional))
            {
                light.shadows = LightShadows.Soft;
                light.intensity = 1.5f;
            }
            
            // Cameraの設定を最適化
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                cam.allowMSAA = true;
                cam.allowDynamicResolution = true;
            }
        }

        private static void CreateEmptyGameManager()
        {
            GameObject gameManager = new GameObject("GameManager");
            // TODO: GameManagerはFeaturesアセンブリにあるため、Coreから直接参照できない
            // gameManager.AddComponent<asterivo.Unity60.Core.GameManager>();
            UnityEngine.Debug.LogWarning("⚠️ GameManagerコンポーネントの追加は循環依存のためスキップされました");
            Selection.activeGameObject = gameManager;
        }

        private static void EnsureDirectoryExists(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = nextPath;
            }
        }
    }
}