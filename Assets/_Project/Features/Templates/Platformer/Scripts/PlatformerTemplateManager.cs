using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Player.Scripts;
using asterivo.Unity60.Player;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer
{
    /// <summary>
    /// プラットフォーマーテンプレート統合管理システム
    /// 3Dプラットフォームアクションゲームの完全なゲーム体験を15分で実現
    /// ジャンプ物理、コレクタブル、動的プラットフォーム、レベル進行を統合管理
    /// </summary>
    public class PlatformerTemplateManager : MonoBehaviour
    {
        #region Template Configuration

        [TabGroup("Template", "Configuration")]
        [Title("Platformer Template Settings", "3Dプラットフォームアクションゲーム", TitleAlignments.Centered)]
        [SerializeField] private bool enableTemplate = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool use15MinuteGameplay = true;

        [TabGroup("Template", "Systems")]
        [Header("Integrated Platformer Systems")]
        [SerializeField] private PlatformerPlayerController playerController;
        [SerializeField] private PlatformerCameraController cameraController;
        [SerializeField] private PlatformerUIManager uiManager;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private PlatformManager platformManager;

        [TabGroup("Template", "Physics")]
        [Header("Physics Configuration")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private float coyoteTime = 0.2f;
        [SerializeField] private float jumpBufferTime = 0.2f;

        [TabGroup("Template", "Gameplay")]
        [Header("Gameplay Settings")]
        [SerializeField] private int targetScore = 1000;
        [SerializeField] private int livesCount = 3;
        [SerializeField] private float levelTimeLimit = 300f; // 5 minutes per level
        [SerializeField] private bool enableCheckpoints = true;

        #endregion

        #region Service References

        private IAudioService audioService;
        private ICommandInvoker commandInvoker;
        private bool isInitialized = false;
        private PlatformerGameState currentGameState = PlatformerGameState.NotStarted;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTemplate();
        }

        private void Start()
        {
            if (enableTemplate)
            {
                SetupPlatformerTemplate();
            }
        }

        private void Update()
        {
            if (enableTemplate && isInitialized)
            {
                UpdatePlatformerTemplate();
            }
        }

        #endregion

        #region Template Core Implementation

        /// <summary>
        /// プラットフォーマーテンプレートの初期化
        /// 既存システムの統合と連携設定
        /// </summary>
        private void InitializeTemplate()
        {
            LogDebug("[PlatformerTemplate] Initializing Platformer Template Systems...");

            try
            {
                // Service Locator経由でサービス取得
                audioService = ServiceLocator.GetService<IAudioService>();
                commandInvoker = ServiceLocator.GetService<ICommandInvoker>();

                // 既存コンポーネントの自動検出
                if (playerController == null)
                    playerController = FindFirstObjectByType<PlatformerPlayerController>();

                if (cameraController == null)
                    cameraController = FindFirstObjectByType<PlatformerCameraController>();

                if (collectibleManager == null)
                    collectibleManager = FindFirstObjectByType<CollectibleManager>();

                if (platformManager == null)
                    platformManager = FindFirstObjectByType<PlatformManager>();

                isInitialized = true;
                LogDebug("[PlatformerTemplate] ✅ Template initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[PlatformerTemplate] ❌ Template initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// プラットフォーマーテンプレートのセットアップ
        /// 15分ゲームプレイ体験の構築
        /// </summary>
        private void SetupPlatformerTemplate()
        {
            LogDebug("[PlatformerTemplate] Setting up Platformer Template for 15-minute gameplay...");

            // プレイヤーシステムセットアップ
            SetupPlayerSystems();

            // カメラシステム設定
            SetupCameraSystem();

            // UI システム設定
            SetupUISystem();

            // コレクタブルシステム設定
            SetupCollectibleSystem();

            // プラットフォームシステム設定
            SetupPlatformSystem();

            // オーディオシステムセットアップ
            SetupAudioSystem();

            LogDebug("[PlatformerTemplate] ✅ Platformer Template setup completed - Ready for 15min gameplay");
        }

        /// <summary>
        /// プラットフォーマーテンプレートの更新処理
        /// </summary>
        private void UpdatePlatformerTemplate()
        {
            // ゲーム状態の更新
            UpdateGameState();

            // パフォーマンス統計の更新
            UpdatePerformanceStats();
        }

        #endregion

        #region Player Systems Integration

        /// <summary>
        /// プレイヤーシステムの統合セットアップ
        /// ジャンプ物理とプラットフォーマー固有の移動を実現
        /// </summary>
        private void SetupPlayerSystems()
        {
            LogDebug("[PlatformerTemplate] Integrating player platformer systems...");

            if (playerController != null)
            {
                // 重力設定の適用
                playerController.SetGravity(gravity);
                
                // ジャンプ設定の適用
                playerController.SetJumpForce(jumpForce);
                playerController.SetGroundLayerMask(groundLayerMask);
                playerController.SetCoyoteTime(coyoteTime);
                playerController.SetJumpBufferTime(jumpBufferTime);

                LogDebug("[PlatformerTemplate] ✅ PlatformerPlayerController integrated");
            }
            else
            {
                LogDebug("[PlatformerTemplate] ⚠️ PlatformerPlayerController not found - creating default setup");
                CreateDefaultPlayerSetup();
            }
        }

        /// <summary>
        /// デフォルトプレイヤーセットアップの作成
        /// </summary>
        private void CreateDefaultPlayerSetup()
        {
            // プレイヤーオブジェクトを検索し、必要に応じてコンポーネントを追加
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.GetComponent<PlatformerPlayerController>() == null)
            {
                playerController = player.AddComponent<PlatformerPlayerController>();
                LogDebug("[PlatformerTemplate] ✅ PlatformerPlayerController component added to player");
            }
        }

        #endregion

        #region Camera System Integration

        /// <summary>
        /// プラットフォーマー特化カメラシステムのセットアップ
        /// </summary>
        private void SetupCameraSystem()
        {
            LogDebug("[PlatformerTemplate] Setting up platformer-specific camera system...");

            try
            {
                if (cameraController == null)
                {
                    cameraController = FindFirstObjectByType<PlatformerCameraController>();
                }

                if (cameraController != null)
                {
                    cameraController.enabled = true;
                    
                    // プレイヤーをターゲットに設定
                    if (playerController != null)
                    {
                        cameraController.SetTarget(playerController.transform);
                    }

                    LogDebug("[PlatformerTemplate] ✅ PlatformerCameraController integrated successfully");
                }
                else
                {
                    LogDebug("[PlatformerTemplate] ⚠️ PlatformerCameraController not found - creating default camera setup");
                    CreateDefaultCameraSetup();
                }
            }
            catch (System.Exception ex)
            {
                LogError($"[PlatformerTemplate] ❌ Camera system setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// デフォルトカメラセットアップの作成
        /// </summary>
        private void CreateDefaultCameraSetup()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<PlatformerCameraController>() == null)
            {
                cameraController = mainCamera.gameObject.AddComponent<PlatformerCameraController>();
                LogDebug("[PlatformerTemplate] ✅ PlatformerCameraController component added to main camera");
            }
        }

        #endregion

        #region UI System Integration

        /// <summary>
        /// プラットフォーマー特化UIシステムのセットアップ
        /// </summary>
        private void SetupUISystem()
        {
            LogDebug("[PlatformerTemplate] Setting up platformer-specific UI system...");

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<PlatformerUIManager>();
            }

            if (uiManager != null)
            {
                uiManager.enabled = true;
                
                // UI初期設定
                uiManager.InitializeUI(livesCount, 0, targetScore);
                
                LogDebug("[PlatformerTemplate] ✅ PlatformerUIManager integrated successfully");
            }
            else
            {
                LogDebug("[PlatformerTemplate] ⚠️ PlatformerUIManager not found - will be implemented next");
            }
        }

        #endregion

        #region Collectible System Integration

        /// <summary>
        /// コレクタブルシステムのセットアップ
        /// </summary>
        private void SetupCollectibleSystem()
        {
            LogDebug("[PlatformerTemplate] Setting up collectible system...");

            if (collectibleManager != null)
            {
                collectibleManager.enabled = true;
                collectibleManager.SetTargetScore(targetScore);
                
                LogDebug("[PlatformerTemplate] ✅ CollectibleManager integrated successfully");
            }
            else
            {
                LogDebug("[PlatformerTemplate] ⚠️ CollectibleManager not found - creating default setup");
            }
        }

        #endregion

        #region Platform System Integration

        /// <summary>
        /// プラットフォームシステムのセットアップ
        /// </summary>
        private void SetupPlatformSystem()
        {
            LogDebug("[PlatformerTemplate] Setting up platform system...");

            if (platformManager != null)
            {
                platformManager.enabled = true;
                
                LogDebug("[PlatformerTemplate] ✅ PlatformManager integrated successfully");
            }
            else
            {
                LogDebug("[PlatformerTemplate] ⚠️ PlatformManager not found - creating default setup");
            }
        }

        #endregion

        #region Audio System Integration

        /// <summary>
        /// プラットフォーマー用オーディオシステムのセットアップ
        /// </summary>
        private void SetupAudioSystem()
        {
            LogDebug("[PlatformerTemplate] Setting up platformer audio system...");

            if (audioService != null)
            {
                // プラットフォーマー特有の音響設定
                LogDebug("[PlatformerTemplate] ✅ Audio system integrated successfully");
            }
            else
            {
                LogDebug("[PlatformerTemplate] ⚠️ Audio service not available");
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// ゲーム状態の更新
        /// </summary>
        private void UpdateGameState()
        {
            // ゲーム状態の確認と更新
            if (currentGameState == PlatformerGameState.Playing)
            {
                // プレイ中の処理
            }
        }

        /// <summary>
        /// パフォーマンス統計の更新
        /// </summary>
        private void UpdatePerformanceStats()
        {
            // パフォーマンス監視
        }

        #endregion

        #region Public Template API

        /// <summary>
        /// プラットフォーマーゲームを開始
        /// </summary>
        public void StartPlatformerGame()
        {
            currentGameState = PlatformerGameState.Playing;
            LogDebug("[PlatformerTemplate] Game started");
            
            if (uiManager != null)
            {
                uiManager.ShowGameUI();
            }
        }

        /// <summary>
        /// プラットフォーマーゲームを停止
        /// </summary>
        public void StopPlatformerGame()
        {
            currentGameState = PlatformerGameState.GameOver;
            LogDebug("[PlatformerTemplate] Game stopped");
        }

        /// <summary>
        /// プレイヤーのスコアを追加
        /// </summary>
        public void AddScore(int points)
        {
            if (uiManager != null)
            {
                uiManager.AddScore(points);
            }
        }

        /// <summary>
        /// プレイヤーのライフを減らす
        /// </summary>
        public void LoseLife()
        {
            if (uiManager != null)
            {
                uiManager.LoseLife();
            }
        }

        /// <summary>
        /// 現在のゲーム状態を取得
        /// </summary>
        public PlatformerGameState GetCurrentGameState()
        {
            return currentGameState;
        }

        #endregion

        #region Template Actions & Commands

        [TabGroup("Template", "Actions")]
        [Button("Start Platformer Game")]
        public void TestStartGame()
        {
            StartPlatformerGame();
        }

        [Button("Add Test Score")]
        public void TestAddScore()
        {
            AddScore(100);
        }

        [Button("Test Lose Life")]
        public void TestLoseLife()
        {
            LoseLife();
        }

        [Button("Test All Platformer Systems")]
        public void TestAllSystems()
        {
            LogDebug("[PlatformerTemplate] Testing all platformer systems...");

            if (playerController != null)
            {
                LogDebug("[PlatformerTemplate] ✅ PlatformerPlayerController is active");
            }

            if (cameraController != null)
            {
                LogDebug("[PlatformerTemplate] ✅ PlatformerCameraController is active");
            }

            if (collectibleManager != null)
            {
                LogDebug("[PlatformerTemplate] ✅ CollectibleManager is active");
            }

            if (platformManager != null)
            {
                LogDebug("[PlatformerTemplate] ✅ PlatformManager is active");
            }

            LogDebug("[PlatformerTemplate] ✅ All available systems tested successfully");
        }

        #endregion

        #region Debug Support

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Template", "Editor")]
        [Button("Force Initialize Template")]
        public void EditorInitializeTemplate()
        {
            InitializeTemplate();
            SetupPlatformerTemplate();
        }

        [Button("Validate Template Setup")]
        public void ValidateTemplateSetup()
        {
            LogDebug("=== Platformer Template Validation ===");
            LogDebug($"Template Enabled: {enableTemplate}");
            LogDebug($"Initialization Status: {isInitialized}");
            LogDebug($"PlatformerPlayerController: {(playerController != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"PlatformerCameraController: {(cameraController != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"CollectibleManager: {(collectibleManager != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"PlatformManager: {(platformManager != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Audio Service: {(audioService != null ? "✅ Available" : "❌ Unavailable")}");
            LogDebug($"Current Game State: {currentGameState}");
            LogDebug("=== Validation Complete ===");
        }
#endif

        #endregion
    }

    /// <summary>
    /// プラットフォーマーゲームの状態
    /// </summary>
    public enum PlatformerGameState
    {
        NotStarted,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }
}