using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using Debug = UnityEngine.Debug;


namespace asterivo.Unity60.Core.Editor
{
    /// <summary>
    /// ゲームプロジェクト用のシーンテンプレートを作成するエディタツール
    /// </summary>
    public static class SceneTemplateCreator
    {
        private const string TEMPLATE_PATH = "Assets/_Project/Scenes/Templates/";
        private const string PREFAB_PATH = "Assets/_Project/Core/Prefabs/Templates/";

        [MenuItem("Project/Scene Templates/Create All Templates", priority = 1)]
        public static void CreateAllTemplates()
        {
            CreateMainMenuTemplate();
            CreateGameplayTemplate();
            CreateLoadingTemplate();
            CreateSettingsTemplate();
            
            UnityEngine.Debug.Log("✅ All scene templates created successfully!");
        }

        [MenuItem("Project/Scene Templates/1. Create Main Menu Template")]
        public static void CreateMainMenuTemplate()
        {
            Scene scene = CreateNewScene("MainMenuTemplate");
            
            // UI Canvas設定
            SetupUICanvas(scene, "Main Menu Canvas");
            
            // メニュー用オーディオマネージャー
            InstantiatePrefabInScene("AudioManager", Vector3.zero);
            
            // メニュー用ライティング
            SetupMenuLighting();
            
            SaveScene(scene, "MainMenuTemplate.unity");
            UnityEngine.Debug.Log("✅ Main Menu Template created");
        }

        [MenuItem("Project/Scene Templates/2. Create Gameplay Template")]
        public static void CreateGameplayTemplate()
        {
            Scene scene = CreateNewScene("GameplayTemplate");
            
            // プレイヤー
            InstantiatePrefabInScene("DefaultPlayer", Vector3.zero);
            
            // ゲームマネージャー
            InstantiatePrefabInScene("GameManager", Vector3.zero);
            
            // コマンドシステム
            InstantiatePrefabInScene("CommandSystem", Vector3.zero);
            
            // UI Canvas
            SetupUICanvas(scene, "Game HUD Canvas");
            
            // オーディオマネージャー
            InstantiatePrefabInScene("AudioManager", Vector3.zero);
            
            // 環境設定
            InstantiatePrefabInScene("DefaultGround", Vector3.zero);
            InstantiatePrefabInScene("DefaultLighting", Vector3.zero);
            InstantiatePrefabInScene("SpawnPoint", Vector3.zero);
            
            // カメラ設定
            SetupGameplayCamera();
            
            SaveScene(scene, "GameplayTemplate.unity");
            UnityEngine.Debug.Log("✅ Gameplay Template created");
        }

        [MenuItem("Project/Scene Templates/3. Create Loading Template")]
        public static void CreateLoadingTemplate()
        {
            Scene scene = CreateNewScene("LoadingTemplate");
            
            // ローディング専用UI
            SetupLoadingUI();
            
            // ローディング用オーディオ
            InstantiatePrefabInScene("AudioManager", Vector3.zero);
            
            // シンプルなライティング
            SetupMinimalLighting();
            
            SaveScene(scene, "LoadingTemplate.unity");
            UnityEngine.Debug.Log("✅ Loading Template created");
        }

        [MenuItem("Project/Scene Templates/4. Create Settings Template")]
        public static void CreateSettingsTemplate()
        {
            Scene scene = CreateNewScene("SettingsTemplate");
            
            // 設定用UI Canvas
            SetupUICanvas(scene, "Settings Canvas");
            
            // 設定用オーディオマネージャー
            InstantiatePrefabInScene("AudioManager", Vector3.zero);
            
            // 設定メニュー用ライティング
            SetupMenuLighting();
            
            SaveScene(scene, "SettingsTemplate.unity");
            UnityEngine.Debug.Log("✅ Settings Template created");
        }

        // ヘルパーメソッド
        private static Scene CreateNewScene(string name)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = name;
            return scene;
        }

        private static void SetupUICanvas(Scene scene, string canvasName)
        {
            // UICanvasプリファブを探してインスタンス化
            GameObject canvasInstance = InstantiatePrefabInScene("UICanvas", Vector3.zero);
            if (canvasInstance != null)
            {
                canvasInstance.name = canvasName;
            }
            else
            {
                // プリファブがない場合は手動で作成
                CreateManualUICanvas(canvasName);
            }
        }

        private static void CreateManualUICanvas(string canvasName)
        {
            // Canvas
            GameObject canvasGO = new GameObject(canvasName);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static GameObject InstantiatePrefabInScene(string prefabName, Vector3 position)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            if (guids.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.transform.position = position;
                return instance;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"⚠️ Prefab '{prefabName}' not found");
                return null;
            }
        }

        private static void SetupGameplayCamera()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<UnityEngine.Camera>();
                cameraGO.tag = "MainCamera";
            }

            // ゲームプレイ用カメラ設定
            mainCamera.transform.position = new Vector3(0, 5, -10);
            mainCamera.transform.rotation = Quaternion.Euler(15, 0, 0);
            mainCamera.fieldOfView = 60f;
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 1000f;

            // オーディオリスナー
            if (mainCamera.GetComponent<AudioListener>() == null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
            }
        }

        private static void SetupMenuLighting()
        {
            // Directional Light
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 環境光設定
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1.0f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.3f);
        }

        private static void SetupMinimalLighting()
        {
            // 最小限のライティング
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.3f);
        }

        private static void SetupLoadingUI()
        {
            CreateManualUICanvas("Loading Canvas");
            
            // ローディング画面用の簡単なUI要素を作成
            GameObject canvas = GameObject.Find("Loading Canvas");
            if (canvas != null)
            {
                // ローディングテキスト
                GameObject loadingTextGO = new GameObject("Loading Text");
                loadingTextGO.transform.SetParent(canvas.transform, false);
                
                var text = loadingTextGO.AddComponent<UnityEngine.UI.Text>();
                text.text = "Loading...";
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 24;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;
                
                var rectTransform = loadingTextGO.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(200, 50);
            }
        }

        private static void SaveScene(Scene scene, string fileName)
        {
            string fullPath = TEMPLATE_PATH + fileName;
            EnsureDirectoryExists(Path.GetDirectoryName(fullPath));
            EditorSceneManager.SaveScene(scene, fullPath);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
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
}
