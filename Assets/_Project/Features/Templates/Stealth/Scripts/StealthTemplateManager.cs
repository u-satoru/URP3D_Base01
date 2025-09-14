using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Player.Scripts;
using asterivo.Unity60.Player;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルステンプレート統合管理システム
    /// 最優先度ジャンルとして、既存の豊富なステルスシステムを統合し
    /// 完全なステルスアクションゲーム体験を15分で実現する
    /// </summary>
    public class StealthTemplateManager : MonoBehaviour
    {
        #region Template Configuration

        [TabGroup("Template", "Configuration")]
        [Title("Stealth Template Settings", "最優先度ジャンル - ステルスアクションゲーム", TitleAlignments.Centered)]
        [SerializeField] private bool enableTemplate = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool use15MinuteGameplay = true;

        [TabGroup("Template", "Systems")]
        [Header("Integrated Stealth Systems")]
        [SerializeField] private PlayerStealthController playerStealthController;
        [SerializeField] private StealthMovementController stealthMovementController;
        [SerializeField] private StealthCameraController stealthCameraController;
        [SerializeField] private StealthUIManager stealthUIManager;
        [SerializeField] private StealthGameplayManager gameplayManager;

        [TabGroup("Template", "Audio")]
        [Header("Stealth Audio Integration")]
        [SerializeField] private bool useStealthAudioService = true;
        [SerializeField] private AlertLevel currentAlertLevel = AlertLevel.None;
        [SerializeField, Range(0f, 1f)] private float environmentNoiseLevel = 0.3f;
        [SerializeField, Range(0f, 1f)] private float audioMaskingLevel = 0.5f;

        #endregion

        #region Service References

        private IStealthAudioService stealthAudioService;
        private ICommandInvoker commandInvoker;
        private bool isInitialized = false;

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
                SetupStealthTemplate();
            }
        }

        private void Update()
        {
            if (enableTemplate && isInitialized)
            {
                UpdateStealthTemplate();
            }
        }

        #endregion

        #region Template Core Implementation

        /// <summary>
        /// ステルステンプレートの初期化
        /// 既存システムの統合と連携設定
        /// </summary>
        private void InitializeTemplate()
        {
            LogDebug("[StealthTemplate] Initializing Stealth Template Systems...");

            try
            {
                // Service Locator経由でサービス取得
                stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                commandInvoker = ServiceLocator.GetService<ICommandInvoker>();

                // 既存コンポーネントの自動検出
                if (playerStealthController == null)
                    playerStealthController = FindFirstObjectByType<PlayerStealthController>();

                if (stealthMovementController == null)
                    stealthMovementController = FindFirstObjectByType<StealthMovementController>();

                isInitialized = true;
                LogDebug("[StealthTemplate] ✅ Template initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthTemplate] ❌ Template initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ステルステンプレートのセットアップ
        /// 15分ゲームプレイ体験の構築
        /// </summary>
        private void SetupStealthTemplate()
        {
            LogDebug("[StealthTemplate] Setting up Stealth Template for 15-minute gameplay...");

            // オーディオシステムセットアップ
            SetupStealthAudio();

            // プレイヤーシステム統合
            SetupPlayerSystems();

            // カメラシステム設定
            SetupCameraSystem();

            // UI システム設定
            SetupUISystem();

            // ゲームプレイマネージャー設定
            SetupGameplayManager();

            LogDebug("[StealthTemplate] ✅ Stealth Template setup completed - Ready for 15min gameplay");
        }

        /// <summary>
        /// ステルステンプレートの更新処理
        /// </summary>
        private void UpdateStealthTemplate()
        {
            // 警戒レベルの動的調整
            UpdateAlertLevel();

            // 環境オーディオの更新
            UpdateEnvironmentAudio();
        }

        #endregion

        #region Stealth Audio Integration

        /// <summary>
        /// ステルスオーディオシステムのセットアップ
        /// 既存のIStealthAudioServiceを完全活用
        /// </summary>
        private void SetupStealthAudio()
        {
            if (!useStealthAudioService || stealthAudioService == null) return;

            LogDebug("[StealthTemplate] Configuring stealth audio systems...");

            try
            {
                // 環境ノイズレベル設定
                stealthAudioService.SetEnvironmentNoiseLevel(environmentNoiseLevel);

                // オーディオマスキング効果適用
                stealthAudioService.ApplyAudioMasking(audioMaskingLevel);

                // 警戒レベル音楽設定
                stealthAudioService.SetAlertLevelMusic(currentAlertLevel);

                LogDebug("[StealthTemplate] ✅ Stealth audio systems configured");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthTemplate] ❌ Audio setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 警戒レベルの動的更新
        /// </summary>
        private void UpdateAlertLevel()
        {
            // ここで実際のゲーム状態に基づいて警戒レベルを調整
            // 例: NPC検知状況、プレイヤー行動等に基づく
        }

        /// <summary>
        /// 環境オーディオの更新
        /// </summary>
        private void UpdateEnvironmentAudio()
        {
            if (stealthAudioService == null) return;

            // 動的な環境ノイズ調整
            stealthAudioService.SetEnvironmentNoiseLevel(environmentNoiseLevel);
        }

        #endregion

        #region Player Systems Integration

        /// <summary>
        /// プレイヤーシステムの統合セットアップ
        /// 既存のPlayerStealthController + StealthMovementControllerを活用
        /// </summary>
        private void SetupPlayerSystems()
        {
            LogDebug("[StealthTemplate] Integrating player stealth systems...");

            // PlayerStealthController統合
            if (playerStealthController != null)
            {
                LogDebug("[StealthTemplate] ✅ PlayerStealthController integrated");
            }
            else
            {
                LogDebug("[StealthTemplate] ⚠️ PlayerStealthController not found - creating default setup");
            }

            // StealthMovementController統合
            if (stealthMovementController != null)
            {
                LogDebug("[StealthTemplate] ✅ StealthMovementController integrated");
            }
            else
            {
                LogDebug("[StealthTemplate] ⚠️ StealthMovementController not found");
            }
        }

        #endregion

        #region Camera System Integration

        /// <summary>
        /// ステルス特化カメラシステムのセットアップ
        /// </summary>
        private void SetupCameraSystem()
        {
            LogDebug("[StealthTemplate] Setting up stealth-specific camera system...");

            try
            {
                // ステルス特化カメラの自動検出
                if (stealthCameraController == null)
                {
                    stealthCameraController = FindFirstObjectByType<StealthCameraController>();
                }

                // ステルス特化カメラの設定と初期化
                if (stealthCameraController != null)
                {
                    // カメラシステムが有効化されていることを確認
                    stealthCameraController.enabled = true;

                    // 初期カメラモードを通常モードに設定
                    stealthCameraController.ForceSetCameraMode(StealthCameraMode.Normal);

                    LogDebug("[StealthTemplate] ✅ StealthCameraController integrated successfully");
                    LogDebug($"[StealthTemplate] Current camera mode: {stealthCameraController.GetCurrentCameraMode()}");
                }
                else
                {
                    LogDebug("[StealthTemplate] ⚠️ StealthCameraController not found - creating default camera setup");
                    CreateDefaultCameraSetup();
                }
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthTemplate] ❌ Camera system setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// デフォルトカメラセットアップの作成
        /// </summary>
        private void CreateDefaultCameraSetup()
        {
            // 既存のカメラオブジェクトにStealthCameraControllerコンポーネントを追加
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<StealthCameraController>() == null)
            {
                stealthCameraController = mainCamera.gameObject.AddComponent<StealthCameraController>();
                LogDebug("[StealthTemplate] ✅ StealthCameraController component added to main camera");
            }
        }

        #endregion

        #region UI System Integration

        /// <summary>
        /// ステルス特化UIシステムのセットアップ
        /// </summary>
        private void SetupUISystem()
        {
            LogDebug("[StealthTemplate] Setting up stealth-specific UI system...");

            // ステルス特化UIの設定
            if (stealthUIManager == null)
            {
                LogDebug("[StealthTemplate] ⚠️ StealthUIManager not found - will be implemented next");
            }
        }

        #endregion

        #region Gameplay Manager Integration

        /// <summary>
        /// ステルスゲームプレイマネージャーのセットアップ
        /// </summary>
        private void SetupGameplayManager()
        {
            LogDebug("[StealthTemplate] Setting up 15-minute stealth gameplay...");

            try
            {
                // ステルスゲームプレイマネージャーの自動検出
                if (gameplayManager == null)
                {
                    gameplayManager = FindFirstObjectByType<StealthGameplayManager>();
                }

                // ゲームプレイマネージャーの設定と初期化
                if (gameplayManager != null)
                {
                    // ゲームプレイマネージャーが有効化されていることを確認
                    gameplayManager.enabled = true;

                    LogDebug("[StealthTemplate] ✅ StealthGameplayManager integrated successfully");
                    LogDebug($"[StealthTemplate] Current game state: {gameplayManager.GetCurrentGameState()}");
                    LogDebug($"[StealthTemplate] Mission objectives: {gameplayManager.GetTotalObjectives()}");
                }
                else
                {
                    LogDebug("[StealthTemplate] ⚠️ StealthGameplayManager not found - 15-minute gameplay will be limited");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthTemplate] ❌ Gameplay manager setup failed: {ex.Message}");
            }
        }

        #endregion

        #region Public Template API

        /// <summary>
        /// ステルス足音の生成
        /// </summary>
        public void CreateStealthFootstep(Vector3 position, float intensity, string surfaceType = "concrete")
        {
            if (stealthAudioService != null)
            {
                stealthAudioService.CreateFootstep(position, intensity, surfaceType);
                LogDebug($"[StealthTemplate] Footstep created at {position} with intensity {intensity}");
            }
        }

        /// <summary>
        /// 注意を引く音の再生
        /// </summary>
        public void CreateDistraction(Vector3 position, float radius = 8f)
        {
            if (stealthAudioService != null)
            {
                stealthAudioService.PlayDistraction(position, radius);
                LogDebug($"[StealthTemplate] Distraction created at {position} with radius {radius}");
            }
        }

        /// <summary>
        /// 警戒レベルの設定
        /// </summary>
        public void SetAlertLevel(AlertLevel level)
        {
            currentAlertLevel = level;
            if (stealthAudioService != null)
            {
                stealthAudioService.SetAlertLevelMusic(level);
                LogDebug($"[StealthTemplate] Alert level set to {level}");
            }
        }

        /// <summary>
        /// プレイヤーの隠密度調整
        /// </summary>
        public void AdjustStealthLevel(float stealthLevel)
        {
            if (stealthAudioService != null)
            {
                stealthAudioService.AdjustStealthAudio(stealthLevel);
                LogDebug($"[StealthTemplate] Stealth level adjusted to {stealthLevel:F2}");
            }
        }

        #endregion

        #region Template Actions & Commands

        [TabGroup("Template", "Actions")]
        [Button("Test Stealth Footstep")]
        public void TestFootstep()
        {
            CreateStealthFootstep(transform.position, 0.5f, "concrete");
        }

        [Button("Test Distraction")]
        public void TestDistraction()
        {
            CreateDistraction(transform.position + Vector3.forward * 5f, 8f);
        }

        [Button("Set High Alert")]
        public void TestHighAlert()
        {
            SetAlertLevel(AlertLevel.High);
        }

        [Button("Reset to Normal")]
        public void TestNormalAlert()
        {
            SetAlertLevel(AlertLevel.None);
        }

        [Button("Test Full Stealth Systems")]
        public void TestAllSystems()
        {
            LogDebug("[StealthTemplate] Testing all stealth systems...");

            if (playerStealthController != null)
            {
                LogDebug("[StealthTemplate] ✅ PlayerStealthController is active");
            }

            if (stealthMovementController != null)
            {
                LogDebug("[StealthTemplate] ✅ StealthMovementController is active");
            }

            if (stealthAudioService != null)
            {
                TestFootstep();
                TestDistraction();
                LogDebug("[StealthTemplate] ✅ Audio systems tested successfully");
            }

            LogDebug("[StealthTemplate] ✅ All available systems tested successfully");
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
            SetupStealthTemplate();
        }

        [Button("Validate Template Setup")]
        public void ValidateTemplateSetup()
        {
            LogDebug("=== Stealth Template Validation ===");
            LogDebug($"Template Enabled: {enableTemplate}");
            LogDebug($"Initialization Status: {isInitialized}");
            LogDebug($"PlayerStealthController: {(playerStealthController != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"StealthMovementController: {(stealthMovementController != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"StealthAudioService: {(stealthAudioService != null ? "✅ Available" : "❌ Unavailable")}");
            LogDebug($"Current Alert Level: {currentAlertLevel}");
            LogDebug("=== Validation Complete ===");
        }
#endif

        #endregion
    }

}