using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Player
{
    /// <summary>
    /// プレイヤーのステルス機能制御コンポーネント
    /// Step 3.6 パターン1: プレイヤーコントローラーでの段階的更新例
    /// </summary>
    public class PlayerStealthController : MonoBehaviour
    {
        [TabGroup("Migration Settings", "Service Settings")]
        [Header("Service Dependencies")]
        [SerializeField] private bool useNewServices = true; // Inspector設定可能
        [SerializeField] private bool enableDebugLogs = true;

        [TabGroup("Migration Settings", "Stealth Settings")]
        [Header("Stealth Parameters")]
        [SerializeField, Range(0f, 1f)] private float currentStealthLevel = 0f;
        [SerializeField] private string currentSurfaceType = "concrete";
        [SerializeField, Range(0f, 1f)] private float footstepIntensity = 0.5f;

        [TabGroup("Migration Settings", "Runtime Info")]
        [Header("Runtime Information")]
        [SerializeField, ReadOnly] private bool isServiceAvailable;
        [SerializeField, ReadOnly] private string activeServiceType;
        [SerializeField, ReadOnly] private bool isUsingServiceLocator;

        // Service References
        private IAudioService audioService;
        private IStealthAudioService stealthAudioService;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeServices();
        }

        private void Update()
        {
            // 例: WASDキーでの移動とステルス制御
            HandleMovementAndStealth();
        }

        #endregion

        #region Service Initialization

        /// <summary>
        /// サービスの段階的初期化
        /// Phase 3 計画書のパターン1実装
        /// </summary>
        private void InitializeServices()
        {
            LogDebug("[PlayerStealthController] Initializing services...");

            // 新しい方法での取得 (推奨)
            if (useNewServices && FeatureFlags.UseServiceLocator)
            {
                InitializeWithServiceLocator();
            }
            else
            {
                InitializeWithLegacyMethod();
            }

            // サービス取得の検証
            ValidateServiceReferences();
        }

        /// <summary>
        /// ServiceLocatorを使用した新方式での初期化
        /// </summary>
        private void InitializeWithServiceLocator()
        {
            try
            {
                audioService = ServiceLocator.GetService<IAudioService>();
                stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();

                if (audioService != null && stealthAudioService != null)
                {
                    isUsingServiceLocator = true;
                    activeServiceType = stealthAudioService.GetType().Name;
                    isServiceAvailable = true;

                    LogDebug("[PlayerStealthController] Using ServiceLocator for audio services");
                    LogDebug($"[PlayerStealthController] StealthAudioService type: {activeServiceType}");
                }
                else
                {
                    LogDebug("[PlayerStealthController] ServiceLocator services not available, falling back to legacy");
                    InitializeWithLegacyMethod();
                }
            }
            catch (System.Exception ex)
            {
                ServiceLocator.GetService<IEventLogger>()?.LogError($"[PlayerStealthController] ServiceLocator initialization failed: {ex.Message}");
                InitializeWithLegacyMethod();
            }
        }

        /// <summary>
        /// 従来の方法での初期化（後方互換性）
        /// </summary>
        private void InitializeWithLegacyMethod()
        {
            try
            {
#pragma warning disable CS0618 // Obsolete warning suppression during migration
                // 従来のSingleton参照（フォールバック）
                var legacyAudioManager = FindFirstObjectByType<asterivo.Unity60.Core.Audio.AudioManager>();
                var legacyStealthCoordinator = FindFirstObjectByType<asterivo.Unity60.Core.Audio.StealthAudioCoordinator>();

                if (legacyAudioManager != null)
                {
                    audioService = legacyAudioManager;
                }

                if (legacyStealthCoordinator != null)
                {
                    stealthAudioService = legacyStealthCoordinator;
                    activeServiceType = "StealthAudioCoordinator (Legacy)";
                    isServiceAvailable = true;
                }
#pragma warning restore CS0618

                isUsingServiceLocator = false;

                if (FeatureFlags.EnableMigrationMonitoring)
                {
                    ServiceLocator.GetService<IEventLogger>()?.LogWarning("[PlayerStealthController] Using legacy Singleton access");
                }
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[PlayerStealthController] Legacy initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// サービス参照の検証
        /// </summary>
        private void ValidateServiceReferences()
        {
            if (audioService == null)
            {
                EventLogger.LogErrorStatic("[PlayerStealthController] Failed to get IAudioService");
                isServiceAvailable = false;
            }

            if (stealthAudioService == null)
            {
                EventLogger.LogErrorStatic("[PlayerStealthController] Failed to get IStealthAudioService");
                isServiceAvailable = false;
            }

            LogDebug($"[PlayerStealthController] Service validation complete. Available: {isServiceAvailable}");
        }

        #endregion

        #region Stealth Functionality

        /// <summary>
        /// 移動とステルス制御のハンドリング例
        /// </summary>
        private void HandleMovementAndStealth()
        {
            if (!isServiceAvailable) return;

            // 簡単なWASD移動例
            Vector3 movement = Vector3.zero;
            bool isMoving = false;

            if (Input.GetKey(KeyCode.W)) { movement += Vector3.forward; isMoving = true; }
            if (Input.GetKey(KeyCode.S)) { movement += Vector3.back; isMoving = true; }
            if (Input.GetKey(KeyCode.A)) { movement += Vector3.left; isMoving = true; }
            if (Input.GetKey(KeyCode.D)) { movement += Vector3.right; isMoving = true; }

            // 移動中の足音生成
            if (isMoving && Time.frameCount % 30 == 0) // 30フレームごと
            {
                CreateFootstep();
            }

            // ステルスレベルの動的調整
            if (Input.GetKey(KeyCode.LeftShift)) // Shiftキーでステルス
            {
                AdjustStealthLevel(0.8f);
            }
            else
            {
                AdjustStealthLevel(0.2f);
            }
        }

        /// <summary>
        /// 足音の生成
        /// </summary>
        private void CreateFootstep()
        {
            if (stealthAudioService != null)
            {
                try
                {
                    stealthAudioService.CreateFootstep(transform.position, footstepIntensity, currentSurfaceType);
                    LogDebug($"[PlayerStealthController] Footstep created at {transform.position}");
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[PlayerStealthController] Footstep creation failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ステルスレベルの調整
        /// </summary>
        private void AdjustStealthLevel(float targetLevel)
        {
            if (stealthAudioService != null && Mathf.Abs(currentStealthLevel - targetLevel) > 0.1f)
            {
                currentStealthLevel = Mathf.Lerp(currentStealthLevel, targetLevel, Time.deltaTime * 2f);
                
                try
                {
                    stealthAudioService.AdjustStealthAudio(currentStealthLevel);
                    LogDebug($"[PlayerStealthController] Stealth level adjusted to: {currentStealthLevel:F2}");
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[PlayerStealthController] Stealth adjustment failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 注意をそらす音の再生
        /// </summary>
        [TabGroup("Migration Settings", "Actions")]
        [Button("Create Distraction")]
        public void CreateDistraction()
        {
            if (stealthAudioService != null)
            {
                Vector3 distractionPos = transform.position + transform.forward * 5f;
                
                try
                {
                    stealthAudioService.PlayDistraction(distractionPos, 8f);
                    LogDebug($"[PlayerStealthController] Distraction created at {distractionPos}");
                }
                catch (System.Exception ex)
                {
                    EventLogger.LogErrorStatic($"[PlayerStealthController] Distraction creation failed: {ex.Message}");
                }
            }
        }

        #endregion

        #region Debug and Testing

        /// <summary>
        /// サービス切り替えテスト
        /// </summary>
        [TabGroup("Migration Settings", "Testing")]
        [Button("Toggle Service Method")]
        public void ToggleServiceMethod()
        {
            useNewServices = !useNewServices;
            LogDebug($"[PlayerStealthController] Service method toggled. Using new services: {useNewServices}");
            
            // サービス再初期化
            audioService = null;
            stealthAudioService = null;
            InitializeServices();
        }

        /// <summary>
        /// サービス状態のデバッグ表示
        /// </summary>
        [Button("Debug Service Status")]
        public void DebugServiceStatus()
        {
            LogDebug("=== PlayerStealthController Service Status ===");
            LogDebug($"Use New Services: {useNewServices}");
            LogDebug($"Service Available: {isServiceAvailable}");
            LogDebug($"Using ServiceLocator: {isUsingServiceLocator}");
            LogDebug($"Active Service Type: {activeServiceType}");
            LogDebug($"Current Stealth Level: {currentStealthLevel:F2}");
            LogDebug($"FeatureFlags.UseServiceLocator: {FeatureFlags.UseServiceLocator}");
            LogDebug($"FeatureFlags.MigrateStealthAudioCoordinator: {FeatureFlags.MigrateStealthAudioCoordinator}");
        }

        /// <summary>
        /// 機能テスト
        /// </summary>
        [Button("Test All Functions")]
        public void TestAllFunctions()
        {
            if (!isServiceAvailable)
            {
                EventLogger.LogErrorStatic("[PlayerStealthController] Services not available for testing");
                return;
            }

            LogDebug("[PlayerStealthController] Testing all functions...");

            try
            {
                // 足音テスト
                CreateFootstep();
                
                // ステルスレベル調整テスト
                stealthAudioService.AdjustStealthAudio(0.7f);
                
                // 環境ノイズ設定テスト
                stealthAudioService.SetEnvironmentNoiseLevel(0.3f);
                
                // 注意をそらす音テスト
                CreateDistraction();

                LogDebug("[PlayerStealthController] ✅ All function tests completed successfully");
            }
            catch (System.Exception ex)
            {
                EventLogger.LogErrorStatic($"[PlayerStealthController] ❌ Function test failed: {ex.Message}");
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs && FeatureFlags.EnableDebugLogging)
            {
                ServiceLocator.GetService<IEventLogger>()?.Log(message);
            }
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // ステルス範囲の可視化
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 3f);
            
            // 注意をそらす音の位置
            Gizmos.color = Color.red;
            Vector3 distractionPos = transform.position + transform.forward * 5f;
            Gizmos.DrawWireSphere(distractionPos, 1f);
            
            // 足音強度の表示
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.up * footstepIntensity * 2f);
        }
#endif

        #endregion
    }
}