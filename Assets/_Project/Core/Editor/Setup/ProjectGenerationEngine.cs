using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace asterivo.Unity60.Core.Editor.Setup
{
    /// <summary>
    /// プロジェクト生成エンジン
    /// SetupWizardWindowからの指示に基づき、プロジェクトの自動生成と設定を行う
    /// </summary>
    public class ProjectGenerationEngine
    {
        private readonly SetupWizardWindow.WizardConfiguration config;
        private readonly Action<float, string> onProgress;

        public ProjectGenerationEngine(SetupWizardWindow.WizardConfiguration config, Action<float, string> onProgress)
        {
            this.config = config;
            this.onProgress = onProgress;
        }

        /// <summary>
        /// プロジェクト生成プロセスを開始
        /// </summary>
        public async Task<bool> GenerateProjectAsync()
        {
            try
            {
                onProgress?.Invoke(0, "プロジェクト生成を開始...");

                // 1. パッケージのインストール
                await InstallRequiredPackagesAsync();
                onProgress?.Invoke(25, "パッケージのインストール完了");

                // 2. シーンのセットアップ
                SetupScene();
                onProgress?.Invoke(50, "シーンのセットアップ完了");

                // 3. アセットとプレハブの配置
                DeployAssetsAndPrefabs();
                onProgress?.Invoke(75, "アセットとプレハブの配置完了");

                // 4. プロジェクト設定の適用
                ApplyProjectSettings();
                onProgress?.Invoke(100, "プロジェクト設定の適用完了");

                UnityEngine.Debug.Log("プロジェクト生成が正常に完了しました。");
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"プロジェクト生成中にエラーが発生しました: {ex}");
                return false;
            }
        }

        private async Task InstallRequiredPackagesAsync()
        {
            var packagesToInstall = GetRequiredPackages();
            
            if (packagesToInstall.Any())
            {
                onProgress?.Invoke(-1, $"{packagesToInstall.Count}個のパッケージをインストール中...");
                
                foreach (var pkg in packagesToInstall)
                {
                    onProgress?.Invoke(-1, $"パッケージ '{pkg}' をインストール中...");
                    await AddPackageAsync(pkg);
                }
                
                onProgress?.Invoke(-1, "パッケージのインストールが完了しました");
            }
        }

        private List<string> GetRequiredPackages()
        {
            var packages = new List<string>();
            var modulePackageMap = GetModulePackageMapping();

            // 選択されたモジュールに対応するパッケージを追加
            foreach (var module in config.selectedModules)
            {
                if (modulePackageMap.ContainsKey(module))
                {
                    packages.AddRange(modulePackageMap[module]);
                }
            }

            // ジャンル固有のパッケージを追加
            packages.AddRange(GetGenreSpecificPackages());

            return packages.Distinct().ToList();
        }

        private Dictionary<string, List<string>> GetModulePackageMapping()
        {
            return new Dictionary<string, List<string>>
            {
                { "Audio System", new List<string> { "com.unity.timeline", "com.unity.cinemachine" } },
                { "Localization", new List<string> { "com.unity.localization" } },
                { "Analytics", new List<string> { "com.unity.analytics" } },
                { "Input System", new List<string> { "com.unity.inputsystem" } },
                { "Addressables", new List<string> { "com.unity.addressables" } },
                { "Visual Scripting", new List<string> { "com.unity.visualscripting" } },
                { "AI Navigation", new List<string> { "com.unity.ai.navigation" } },
                { "Multiplayer", new List<string> { "com.unity.netcode.gameobjects" } }
            };
        }

        private List<string> GetGenreSpecificPackages()
        {
            var packages = new List<string>();
            
            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    packages.AddRange(new[] { "com.unity.cinemachine", "com.unity.inputsystem" });
                    break;
                case asterivo.Unity60.Core.Setup.GameGenreType.Stealth:
                    packages.AddRange(new[] { "com.unity.ai.navigation", "com.unity.cinemachine" });
                    break;
                case asterivo.Unity60.Core.Setup.GameGenreType.Strategy:
                    packages.AddRange(new[] { "com.unity.ai.navigation", "com.unity.timeline" });
                    break;
                case asterivo.Unity60.Core.Setup.GameGenreType.Adventure:
                    packages.AddRange(new[] { "com.unity.timeline", "com.unity.localization" });
                    break;
                case asterivo.Unity60.Core.Setup.GameGenreType.Platformer:
                    packages.AddRange(new[] { "com.unity.cinemachine", "com.unity.inputsystem" });
                    break;
            }
            
            return packages;
        }

        private async Task AddPackageAsync(string packageName)
        {
            AddRequest request = Client.Add(packageName);
            while (!request.IsCompleted)
                await Task.Delay(100);

            if (request.Status == StatusCode.Success)
                UnityEngine.Debug.Log($"パッケージ '{packageName}' のインストールに成功しました。");
            else if (request.Status >= StatusCode.Failure)
                UnityEngine.Debug.LogError($"パッケージ '{packageName}' のインストールに失敗しました: {request.Error.message}");
        }

        private void SetupScene()
        {
            onProgress?.Invoke(-1, "ゲームシーンを設定中...");
            
            // 新しいシーンを作成
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );

            // ジャンル別のシーン設定を適用
            ApplyGenreSceneSettings();
            
            // 基本的なライティング設定
            SetupLighting();
            
            // シーンを保存
            string scenePath = $"Assets/_Project/Scenes/{config.projectName ?? "GameScene"}.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            
            onProgress?.Invoke(-1, $"シーンを保存しました: {scenePath}");
        }

        private void ApplyGenreSceneSettings()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;

            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                    // FPS用カメラ設定
                    mainCamera.fieldOfView = 75f;
                    mainCamera.transform.position = new Vector3(0, 1.8f, 0);
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    // TPS用カメラ設定
                    mainCamera.fieldOfView = 60f;
                    mainCamera.transform.position = new Vector3(0, 2f, -5f);
                    mainCamera.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Strategy:
                    // Strategy用カメラ設定（俯瞰視点）
                    mainCamera.fieldOfView = 45f;
                    mainCamera.transform.position = new Vector3(0, 15f, -10f);
                    mainCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Platformer:
                    // Platformer用カメラ設定
                    mainCamera.fieldOfView = 65f;
                    mainCamera.transform.position = new Vector3(0, 3f, -8f);
                    break;
                    
                default:
                    // デフォルト設定
                    mainCamera.fieldOfView = 60f;
                    mainCamera.transform.position = new Vector3(0, 1f, -10f);
                    break;
            }
        }

        private void SetupLighting()
        {
            // 基本的なライティング設定
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Directional Lightの設定
            var sun = GameObject.Find("Directional Light");
            if (sun != null)
            {
                var light = sun.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = 1.2f;
                    light.color = new Color(1f, 0.95f, 0.8f, 1f);
                    sun.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
                }
            }
        }

        private void DeployAssetsAndPrefabs()
        {
            onProgress?.Invoke(-1, "ゲームオブジェクトを配置中...");
            
            // GameManagerの配置
            CreateGameManager();
            
            // UIキャンバスの作成
            CreateUICanvas();
            
            // プレイヤーオブジェクトの配置
            CreatePlayerObject();
            
            // ジャンル固有のオブジェクト配置
            CreateGenreSpecificObjects();
            
            // 基本的な環境オブジェクト
            CreateEnvironmentObjects();
            
            onProgress?.Invoke(-1, "ゲームオブジェクトの配置が完了しました");
        }

        private void CreateGameManager()
        {
            var gameManager = new GameObject("GameManager");
            
            // 基本的なGameManagerスクリプトをアタッチ（存在する場合）
            var gameManagerType = System.Type.GetType("asterivo.Unity60.Core.GameManager");
            if (gameManagerType != null)
            {
                gameManager.AddComponent(gameManagerType);
            }
        }

        private void CreateUICanvas()
        {
            var canvas = new GameObject("UI Canvas");
            var canvasComponent = canvas.AddComponent<Canvas>();
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // EventSystemの作成
            if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void CreatePlayerObject()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = Vector3.up;
            
            // Rigidbodyの追加
            var rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            // ジャンルに応じた追加コンポーネント
            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    // CharacterControllerの追加
                    GameObject.DestroyImmediate(player.GetComponent<CapsuleCollider>());
                    var characterController = player.AddComponent<CharacterController>();
                    characterController.height = 2f;
                    characterController.radius = 0.5f;
                    characterController.center = new Vector3(0, 1f, 0);
                    break;
            }
        }

        private void CreateGenreSpecificObjects()
        {
            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    CreateShooterGameObjects();
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Stealth:
                    CreateStealthGameObjects();
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Platformer:
                    CreatePlatformerGameObjects();
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Strategy:
                    CreateStrategyGameObjects();
                    break;
            }
        }

        private void CreateShooterGameObjects()
        {
            // ターゲット用オブジェクト
            for (int i = 0; i < 3; i++)
            {
                var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
                target.name = $"Target_{i + 1}";
                target.transform.position = new Vector3(UnityEngine.Random.Range(-5f, 5f), 1f, UnityEngine.Random.Range(5f, 15f));
                target.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        private void CreateStealthGameObjects()
        {
            // NPCガード配置
            var guard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            guard.name = "Guard";
            guard.transform.position = new Vector3(3f, 1f, 5f);
            guard.GetComponent<Renderer>().material.color = Color.blue;
            
            // 隠れる場所（障害物）
            for (int i = 0; i < 5; i++)
            {
                var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = $"Cover_{i + 1}";
                obstacle.transform.position = new Vector3(UnityEngine.Random.Range(-8f, 8f), 0.5f, UnityEngine.Random.Range(2f, 10f));
                obstacle.transform.localScale = new Vector3(2f, 1f, 1f);
                obstacle.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f);
            }
        }

        private void CreatePlatformerGameObjects()
        {
            // プラットフォーム配置
            for (int i = 0; i < 6; i++)
            {
                var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = $"Platform_{i + 1}";
                platform.transform.position = new Vector3(i * 3f - 7.5f, i * 0.5f + 0.5f, 0f);
                platform.transform.localScale = new Vector3(2f, 0.2f, 2f);
                platform.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.3f);
            }
        }

        private void CreateStrategyGameObjects()
        {
            // リソースポイント
            for (int i = 0; i < 4; i++)
            {
                var resource = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                resource.name = $"Resource_{i + 1}";
                resource.transform.position = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0.5f, UnityEngine.Random.Range(-10f, 10f));
                resource.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }

        private void CreateEnvironmentObjects()
        {
            // 地面
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = Vector3.one * 5f;
            ground.GetComponent<Renderer>().material.color = new Color(0.4f, 0.6f, 0.3f);
        }

        private void ApplyProjectSettings()
        {
            onProgress?.Invoke(-1, "プロジェクト設定を適用中...");
            
            // 基本的なプロジェクト設定
            ApplyBasicProjectSettings();
            
            // ジャンル固有の設定
            ApplyGenreSpecificSettings();
            
            // タグとレイヤーの設定
            SetupTagsAndLayers();
            
            // Physics設定
            ApplyPhysicsSettings();
            
            onProgress?.Invoke(-1, "プロジェクト設定の適用が完了しました");
        }

        private void ApplyBasicProjectSettings()
        {
            // Company Name & Product Name
            if (!string.IsNullOrEmpty(config.projectName))
            {
                PlayerSettings.productName = config.projectName;
                PlayerSettings.companyName = "Generated Project";
            }
            
            // Graphics設定
            if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null)
            {
                // URPの設定（URP Asset が存在する場合）
                var urpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
                if (urpAssets.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(urpAssets[0]);
                    var urpAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.RenderPipelineAsset>(assetPath);
                    if (urpAsset != null)
                    {
                        UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = urpAsset;
                    }
                }
            }
            
            // Quality設定
            QualitySettings.vSyncCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }

        private void ApplyGenreSpecificSettings()
        {
            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    // シューターゲーム用設定
                    QualitySettings.antiAliasing = 4;
                    Time.fixedDeltaTime = 1.0f / 60.0f; // 60Hz physics
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Strategy:
                    // ストラテジーゲーム用設定
                    QualitySettings.antiAliasing = 2;
                    Time.fixedDeltaTime = 1.0f / 30.0f; // 30Hz physics（軽量化）
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.Platformer:
                    // プラットフォーマー用設定
                    Physics.gravity = new Vector3(0, -19.62f, 0); // より強い重力
                    Time.fixedDeltaTime = 1.0f / 50.0f; // 50Hz physics
                    break;
            }
        }

        private void SetupTagsAndLayers()
        {
            // 基本的なタグの追加
            var tagsToAdd = new List<string> { "Enemy", "Pickup", "Interactable", "Checkpoint" };
            
            foreach (var tag in tagsToAdd)
            {
                AddTag(tag);
            }
            
            // 基本的なレイヤーの設定
            var layersToAdd = new Dictionary<int, string>
            {
                { 8, "Ground" },
                { 9, "Player" },
                { 10, "Enemy" },
                { 11, "Interactable" },
                { 12, "UI" }
            };
            
            foreach (var layer in layersToAdd)
            {
                SetLayerName(layer.Key, layer.Value);
            }
        }

        private void AddTag(string tagName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            
            // 既存のタグをチェック
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                    return; // 既に存在する
            }
            
            // 新しいタグを追加
            tagsProp.InsertArrayElementAtIndex(0);
            tagsProp.GetArrayElementAtIndex(0).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        private void SetLayerName(int layerIndex, string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            
            if (layerIndex >= 0 && layerIndex < layersProp.arraySize)
            {
                layersProp.GetArrayElementAtIndex(layerIndex).stringValue = layerName;
                tagManager.ApplyModifiedProperties();
            }
        }

        private void ApplyPhysicsSettings()
        {
            // 基本的な物理設定
            switch (config.selectedGenre)
            {
                case asterivo.Unity60.Core.Setup.GameGenreType.Platformer:
                    // プラットフォーマー用の物理設定
                    Physics.bounceThreshold = 0.5f;
                    Physics.defaultMaxDepenetrationVelocity = 5f;
                    break;
                    
                case asterivo.Unity60.Core.Setup.GameGenreType.FPS:
                case asterivo.Unity60.Core.Setup.GameGenreType.TPS:
                    // シューター用の物理設定
                    Physics.bounceThreshold = 0.2f;
                    Physics.defaultMaxDepenetrationVelocity = 10f;
                    break;
            }
        }
    }
}

