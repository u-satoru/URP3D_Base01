using UnityEngine;
using UnityEngine.SceneManagement;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorシーンの統合管理システム
    /// ScriptableObjectsの設定を統合し、ゲームプレイシーンを構築
    /// </summary>
    public class SH_SceneManager : MonoBehaviour
    {
        [Header("Configuration References")]
        [SerializeField] private SH_TemplateConfig templateConfig;
        [SerializeField] private SH_AtmosphereConfig atmosphereConfig;
        [SerializeField] private SH_ItemDatabase itemDatabase;
        [SerializeField] private SH_ResourceManagerConfig resourceConfig;

        [Header("Scene Components")]
        [SerializeField] private SH_AtmosphereManager atmosphereManager;
        [SerializeField] private SH_ResourceManager resourceManager;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Environment Setup")]
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;
        [SerializeField] private AudioSource ambientAudioSource;

        [Header("Sample Area Setup")]
        [SerializeField] private Transform[] itemSpawnPoints;
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private Transform[] safeAreas;

        [Header("Events")]
        [SerializeField] private GameEvent onSceneInitialized;
        [SerializeField] private GameEvent<bool> onPlayerSpawned;

        // Runtime State
        private GameObject playerInstance;
        private bool isSceneInitialized = false;
        private float sceneStartTime;

        private void Start()
        {
            sceneStartTime = Time.time;
            InitializeScene();
        }

        /// <summary>
        /// シーンの完全初期化
        /// </summary>
        public void InitializeScene()
        {
            if (isSceneInitialized) return;

            ValidateConfiguration();
            InitializeTemplateConfiguration();
            InitializeAtmosphere();
            InitializeResourceSystem();
            SetupEnvironment();
            SpawnPlayer();
            PopulateTestItems();

            isSceneInitialized = true;
            onSceneInitialized?.Raise();

            Debug.Log("[SH_SceneManager] SurvivalHorror scene initialized successfully");
        }

        /// <summary>
        /// テンプレート設定を初期化
        /// </summary>
        private void InitializeTemplateConfiguration()
        {
            if (templateConfig == null)
            {
                Debug.LogError("[SH_SceneManager] Template config is missing!");
                return;
            }

            // テンプレートを有効化
            templateConfig.ActivateTemplate();

            // 難易度設定を適用
            templateConfig.SetDifficulty(templateConfig.DefaultDifficulty);

            Debug.Log($"[SH_SceneManager] Template activated with {templateConfig.CurrentDifficulty} difficulty");
        }

        /// <summary>
        /// 雰囲気システムを初期化
        /// </summary>
        private void InitializeAtmosphere()
        {
            if (atmosphereConfig == null || atmosphereManager == null)
            {
                Debug.LogWarning("[SH_SceneManager] Atmosphere system not fully configured");
                return;
            }

            // AtmosphereManagerに設定を注入
            atmosphereManager.Initialize(atmosphereConfig);

            // 初期雰囲気を設定（Normalから開始）
            atmosphereManager.SetAtmosphereState(AtmosphereState.Normal, true);

            // 照明の初期設定
            if (mainDirectionalLight != null)
            {
                var lightingPreset = atmosphereConfig.GetCurrentLightingPreset();
                ApplyLightingPreset(lightingPreset);
            }

            Debug.Log("[SH_SceneManager] Atmosphere system initialized");
        }

        /// <summary>
        /// リソース管理システムを初期化
        /// </summary>
        private void InitializeResourceSystem()
        {
            if (resourceConfig == null || resourceManager == null)
            {
                Debug.LogWarning("[SH_SceneManager] Resource system not fully configured");
                return;
            }

            // ResourceManagerに設定を注入
            resourceManager.Initialize(resourceConfig, itemDatabase);

            // テンプレート設定と連動
            resourceConfig.UpdateConfiguration(templateConfig);

            Debug.Log("[SH_SceneManager] Resource system initialized");
        }

        /// <summary>
        /// 環境設定を適用
        /// </summary>
        private void SetupEnvironment()
        {
            // フォグ設定を適用
            if (atmosphereConfig != null)
            {
                var fogSettings = atmosphereConfig.GetCurrentFogSettings();
                ApplyFogSettings(fogSettings);
            }

            // 環境音を設定
            if (ambientAudioSource != null && atmosphereConfig != null)
            {
                var audioSettings = atmosphereConfig.GetCurrentAudioSettings();
                ApplyAudioSettings(audioSettings);
            }

            Debug.Log("[SH_SceneManager] Environment setup completed");
        }

        /// <summary>
        /// プレイヤーをスポーン
        /// </summary>
        private void SpawnPlayer()
        {
            if (playerPrefab == null || playerSpawnPoint == null)
            {
                Debug.LogError("[SH_SceneManager] Player prefab or spawn point is missing!");
                return;
            }

            // 既存のプレイヤーインスタンスを削除
            if (playerInstance != null)
            {
                DestroyImmediate(playerInstance);
            }

            // 新しいプレイヤーインスタンスを生成
            playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerInstance.name = "SH_Player";

            // プレイヤーコンポーネントの設定
            ConfigurePlayerComponents();

            onPlayerSpawned?.Raise(true);
            Debug.Log("[SH_SceneManager] Player spawned successfully");
        }

        /// <summary>
        /// プレイヤーコンポーネントを設定
        /// </summary>
        private void ConfigurePlayerComponents()
        {
            if (playerInstance == null) return;

            // SanityComponentの設定
            var sanityComponent = playerInstance.GetComponent<asterivo.Unity60.Core.Components.SanityComponent>();
            if (sanityComponent != null)
            {
                // maxSanity and decay rate are configured via Inspector
                // No public SetMaxSanity or SetDecayRate methods available
            }

            // LimitedInventoryComponentの設定
            var inventoryComponent = playerInstance.GetComponent<asterivo.Unity60.Core.Components.LimitedInventoryComponent>();
            if (inventoryComponent != null)
            {
                // maxSlots is configured via Inspector
                // No public SetMaxSlots method available
            }

            Debug.Log("[SH_SceneManager] Player components configured");
        }

        /// <summary>
        /// テスト用アイテムを配置
        /// </summary>
        private void PopulateTestItems()
        {
            if (itemDatabase == null || itemSpawnPoints == null || resourceManager == null)
            {
                Debug.LogWarning("[SH_SceneManager] Cannot populate test items: missing references");
                return;
            }

            int itemsSpawned = 0;

            foreach (var spawnPoint in itemSpawnPoints)
            {
                if (spawnPoint == null) continue;

                // ランダムなアイテムを選択
                var randomItem = itemDatabase.GetRandomItem();
                if (randomItem == null) continue;

                // リソースマネージャーを通じてスポーン試行
                if (resourceManager.TrySpawnItem(randomItem, spawnPoint.position))
                {
                    itemsSpawned++;
                }
            }

            Debug.Log($"[SH_SceneManager] Spawned {itemsSpawned} test items");
        }

        /// <summary>
        /// 照明プリセットを適用
        /// </summary>
        private void ApplyLightingPreset(LightingPreset preset)
        {
            if (preset == null || mainDirectionalLight == null) return;

            mainDirectionalLight.color = preset.directionalLightColor;
            mainDirectionalLight.intensity = preset.directionalLightIntensity;
            mainDirectionalLight.shadows = preset.shadowType;

            RenderSettings.ambientLight = preset.ambientLight;
            RenderSettings.ambientIntensity = preset.ambientIntensity;
        }

        /// <summary>
        /// フォグ設定を適用
        /// </summary>
        private void ApplyFogSettings(FogSettings fogSettings)
        {
            if (fogSettings == null) return;

            RenderSettings.fog = fogSettings.enableFog;
            RenderSettings.fogColor = fogSettings.fogColor;
            RenderSettings.fogMode = fogSettings.fogMode;

            if (fogSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogSettings.fogStartDistance;
                RenderSettings.fogEndDistance = fogSettings.fogEndDistance;
            }
            else
            {
                RenderSettings.fogDensity = fogSettings.fogDensity;
            }
        }

        /// <summary>
        /// オーディオ設定を適用
        /// </summary>
        private void ApplyAudioSettings(AudioEnvironmentSettings audioSettings)
        {
            if (audioSettings == null || ambientAudioSource == null) return;

            ambientAudioSource.clip = audioSettings.ambientLoop;
            ambientAudioSource.volume = audioSettings.ambientVolume;
            ambientAudioSource.loop = true;

            if (!ambientAudioSource.isPlaying && audioSettings.ambientLoop != null)
            {
                ambientAudioSource.Play();
            }
        }

        /// <summary>
        /// 設定の妥当性を検証
        /// </summary>
        private void ValidateConfiguration()
        {
            bool hasErrors = false;

            if (templateConfig == null)
            {
                Debug.LogError("[SH_SceneManager] Template config is not assigned!");
                hasErrors = true;
            }

            if (atmosphereConfig == null)
            {
                Debug.LogError("[SH_SceneManager] Atmosphere config is not assigned!");
                hasErrors = true;
            }

            if (itemDatabase == null)
            {
                Debug.LogError("[SH_SceneManager] Item database is not assigned!");
                hasErrors = true;
            }

            if (resourceConfig == null)
            {
                Debug.LogError("[SH_SceneManager] Resource config is not assigned!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                Debug.LogError("[SH_SceneManager] Critical configuration errors detected. Scene may not function properly.");
            }
        }

        /// <summary>
        /// シーンの統計情報を取得
        /// </summary>
        public SurvivalHorrorSceneStats GetSceneStatistics()
        {
            return new SurvivalHorrorSceneStats
            {
                SceneUptime = Time.time - sceneStartTime,
                IsPlayerAlive = playerInstance != null,
                CurrentDifficulty = templateConfig?.CurrentDifficulty ?? DifficultyLevel.Normal,
                CurrentAtmosphere = atmosphereManager?.CurrentState ?? AtmosphereState.Normal,
                ActiveItemCount = resourceManager?.GetActiveItemCount(ResourceType.Special) ?? 0,
                PlayerSanity = GetPlayerSanity(),
                PlayerHealth = GetPlayerHealth()
            };
        }

        private float GetPlayerSanity()
        {
            if (playerInstance == null) return 0f;
            var sanity = playerInstance.GetComponent<asterivo.Unity60.Core.Components.SanityComponent>();
            return sanity?.SanityNormalized ?? 0f;
        }

        private float GetPlayerHealth()
        {
            if (playerInstance == null) return 0f;
            var health = playerInstance.GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            return health?.HealthPercentage ?? 0f;
        }

        // Public API for runtime control
        public void ChangeDifficulty(DifficultyLevel newDifficulty)
        {
            templateConfig?.SetDifficulty(newDifficulty);
            resourceConfig?.UpdateConfiguration(templateConfig);
        }

        public void SetAtmosphere(AtmosphereState newState)
        {
            atmosphereManager?.SetAtmosphereState(newState);
        }

        public void RespawnPlayer()
        {
            SpawnPlayer();
        }

        // エディタ専用デバッグ機能
        #if UNITY_EDITOR
        [ContextMenu("Reinitialize Scene")]
        private void ReinitializeScene()
        {
            isSceneInitialized = false;
            InitializeScene();
        }

        [ContextMenu("Test Atmosphere Transition")]
        private void TestAtmosphereTransition()
        {
            if (atmosphereManager != null)
            {
                var currentState = atmosphereManager.CurrentState;
                var nextState = currentState switch
                {
                    AtmosphereState.Normal => AtmosphereState.Tense,
                    AtmosphereState.Tense => AtmosphereState.Fear,
                    AtmosphereState.Fear => AtmosphereState.Terror,
                    AtmosphereState.Terror => AtmosphereState.Normal,
                    _ => AtmosphereState.Normal
                };
                atmosphereManager.SetAtmosphereState(nextState);
            }
        }
        #endif
    }

    /// <summary>
    /// シーン統計情報
    /// </summary>
    [System.Serializable]
    public class SurvivalHorrorSceneStats
    {
        public float SceneUptime;
        public bool IsPlayerAlive;
        public DifficultyLevel CurrentDifficulty;
        public AtmosphereState CurrentAtmosphere;
        public int ActiveItemCount;
        public float PlayerSanity;
        public float PlayerHealth;
    }
}
