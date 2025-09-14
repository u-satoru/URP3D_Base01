using UnityEngine;
using Cinemachine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Player.Scripts;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Camera
{
    /// <summary>
    /// Platformer特化カメラ制御システム
    /// 3Dプラットフォーマーに最適化されたカメラ追従と視点管理
    /// Cinemachine統合による高品質なカメラワーク
    /// </summary>
    public class PlatformerCameraController : MonoBehaviour
    {
        #region Camera Configuration

        [TabGroup("Camera", "Basic Settings")]
        [Title("Platformer Camera Configuration", "3Dプラットフォーマー最適化カメラシステム", TitleAlignments.Centered)]
        [SerializeField] private bool enableCamera = true;
        [SerializeField] private bool debugMode = false;

        [TabGroup("Camera", "Follow Settings")]
        [Header("Player Follow Configuration")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 8f, -10f);
        [SerializeField, Range(0.1f, 5f)] private float followSpeed = 2f;
        [SerializeField, Range(0.1f, 3f)] private float rotationSpeed = 1.5f;

        [TabGroup("Camera", "Look Settings")]
        [Header("Look Ahead System")]
        [SerializeField] private bool enableLookAhead = true;
        [SerializeField, Range(0f, 10f)] private float lookAheadDistance = 3f;
        [SerializeField, Range(0.1f, 2f)] private float lookAheadSpeed = 1f;
        [SerializeField, Range(0f, 5f)] private float lookAheadBuffer = 1f;

        [TabGroup("Camera", "Dynamic Settings")]
        [Header("Dynamic Camera Behavior")]
        [SerializeField] private bool enableDynamicHeight = true;
        [SerializeField, Range(0f, 15f)] private float jumpHeight = 12f;
        [SerializeField, Range(0f, 5f)] private float fallDistance = 8f;
        [SerializeField, Range(0.1f, 3f)] private float verticalFollowSpeed = 1.2f;

        [TabGroup("Camera", "Smoothing")]
        [Header("Camera Smoothing")]
        [SerializeField, Range(0.1f, 5f)] private float positionDamping = 1.5f;
        [SerializeField, Range(0.1f, 5f)] private float rotationDamping = 2f;
        [SerializeField] private AnimationCurve dampingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [TabGroup("Camera", "Boundaries")]
        [Header("Camera Boundaries")]
        [SerializeField] private bool enableBoundaries = false;
        [SerializeField] private Bounds cameraBounds = new Bounds(Vector3.zero, Vector3.one * 50f);
        [SerializeField] private LayerMask obstacleLayer = 1;
        [SerializeField, Range(0.1f, 5f)] private float obstacleAvoidanceDistance = 2f;

        #endregion

        #region Cinemachine Integration

        [TabGroup("Cinemachine", "Virtual Cameras")]
        [Header("Cinemachine Virtual Cameras")]
        [SerializeField] private CinemachineVirtualCamera followCamera;
        [SerializeField] private CinemachineVirtualCamera aimCamera;
        [SerializeField] private CinemachineVirtualCamera overviewCamera;

        [TabGroup("Cinemachine", "Composers")]
        [Header("Cinemachine Composers")]
        [SerializeField] private CinemachineFramingTransposer framingTransposer;
        [SerializeField] private CinemachineComposer composer;

        [TabGroup("Cinemachine", "Settings")]
        [Header("Cinemachine Configuration")]
        [SerializeField, Range(0f, 10f)] private float cameraBlendTime = 1.5f;
        [SerializeField] private CinemachineBlendDefinition.Style blendStyle = CinemachineBlendDefinition.Style.EaseInOut;

        #endregion

        #region Camera States

        [TabGroup("States", "Current State")]
        [Header("Current Camera State")]
        [SerializeField, ReadOnly] private PlatformerCameraMode currentMode = PlatformerCameraMode.Follow;
        [SerializeField, ReadOnly] private bool isTransitioning = false;
        [SerializeField, ReadOnly] private float transitionProgress = 0f;

        [TabGroup("States", "Player State")]
        [Header("Player State Tracking")]
        [SerializeField, ReadOnly] private Vector3 playerVelocity;
        [SerializeField, ReadOnly] private bool isPlayerGrounded;
        [SerializeField, ReadOnly] private bool isPlayerJumping;
        [SerializeField, ReadOnly] private float playerFallSpeed;

        private PlatformerCameraMode previousMode;
        private Vector3 currentLookAheadOffset;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float lastDirectionChangeTime;
        private Vector3 lastPlayerDirection;

        #endregion

        #region Service References

        private PlatformerPlayerController playerController;
        private Camera mainCamera;
        private bool isInitialized = false;

        #endregion

        #region Camera Modes

        public enum PlatformerCameraMode
        {
            Follow,        // 通常追従モード
            LookAhead,     // 先読みモード
            Jump,          // ジャンプ時特別モード
            Fall,          // 落下時特別モード
            Overview,      // 全体俯瞰モード
            Cinematic     // シネマティックモード
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeCamera();
        }

        private void Start()
        {
            if (enableCamera)
            {
                SetupPlatformerCamera();
            }
        }

        private void Update()
        {
            if (enableCamera && isInitialized)
            {
                UpdateCameraSystem();
            }
        }

        private void LateUpdate()
        {
            if (enableCamera && isInitialized)
            {
                ApplyCameraTransform();
            }
        }

        #endregion

        #region Camera Initialization

        /// <summary>
        /// プラットフォーマーカメラの初期化
        /// </summary>
        private void InitializeCamera()
        {
            LogDebug("[PlatformerCamera] Initializing Platformer Camera System...");

            try
            {
                // メインカメラ取得
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<Camera>();
                }

                // プレイヤーコントローラー取得
                if (playerTarget != null)
                {
                    playerController = playerTarget.GetComponent<PlatformerPlayerController>();
                }

                // Cinemachineコンポーネント初期化
                InitializeCinemachineComponents();

                // 初期状態設定
                currentLookAheadOffset = Vector3.zero;
                targetPosition = transform.position;
                targetRotation = transform.rotation;
                lastPlayerDirection = Vector3.forward;

                isInitialized = true;
                LogDebug("[PlatformerCamera] ✅ Camera initialization completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"[PlatformerCamera] ❌ Camera initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Cinemachineコンポーネントの初期化
        /// </summary>
        private void InitializeCinemachineComponents()
        {
            // Virtual Cameraの自動検出・作成
            if (followCamera == null)
            {
                CreateFollowCamera();
            }

            // Composerコンポーネントの設定
            SetupComposers();
        }

        /// <summary>
        /// Follow Virtual Cameraの作成
        /// </summary>
        private void CreateFollowCamera()
        {
            GameObject followCamObj = new GameObject("PlatformerFollowCamera");
            followCamObj.transform.SetParent(transform);

            followCamera = followCamObj.AddComponent<CinemachineVirtualCamera>();
            followCamera.Priority = 10;

            if (playerTarget != null)
            {
                followCamera.Follow = playerTarget;
                followCamera.LookAt = playerTarget;
            }

            // Transposerの設定
            var transposer = followCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = followOffset;
            }

            LogDebug("[PlatformerCamera] ✅ Follow Virtual Camera created");
        }

        /// <summary>
        /// Composerコンポーネントのセットアップ
        /// </summary>
        private void SetupComposers()
        {
            if (followCamera != null)
            {
                // Framing Transposer設定
                framingTransposer = followCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (framingTransposer != null)
                {
                    framingTransposer.m_XDamping = positionDamping;
                    framingTransposer.m_YDamping = verticalFollowSpeed;
                    framingTransposer.m_ZDamping = positionDamping;
                }

                // Composer設定
                composer = followCamera.GetCinemachineComponent<CinemachineComposer>();
                if (composer != null)
                {
                    composer.m_HorizontalDamping = rotationDamping;
                    composer.m_VerticalDamping = rotationDamping;
                }
            }
        }

        #endregion

        #region Camera Setup

        /// <summary>
        /// プラットフォーマーカメラのセットアップ
        /// </summary>
        private void SetupPlatformerCamera()
        {
            LogDebug("[PlatformerCamera] Setting up Platformer Camera System...");

            // カメラモードの初期設定
            SetCameraMode(PlatformerCameraMode.Follow);

            // イベントリスナーの登録
            RegisterEventListeners();

            // 初期位置・回転の設定
            if (playerTarget != null)
            {
                SetInitialCameraPosition();
            }

            LogDebug("[PlatformerCamera] ✅ Platformer Camera setup completed");
        }

        /// <summary>
        /// イベントリスナーの登録
        /// </summary>
        private void RegisterEventListeners()
        {
            // プレイヤーイベントリスナー登録
            if (ServiceLocator.TryGetService<IEventLogger>(out var eventLogger))
            {
                // ジャンプイベント
                // フォールイベント
                // 着地イベント
                // 方向転換イベント
            }
        }

        /// <summary>
        /// 初期カメラ位置の設定
        /// </summary>
        private void SetInitialCameraPosition()
        {
            Vector3 initialPosition = playerTarget.position + followOffset;
            transform.position = initialPosition;
            transform.LookAt(playerTarget.position);

            targetPosition = initialPosition;
            targetRotation = transform.rotation;
        }

        #endregion

        #region Camera Update System

        /// <summary>
        /// カメラシステムの更新
        /// </summary>
        private void UpdateCameraSystem()
        {
            // プレイヤー状態の更新
            UpdatePlayerState();

            // カメラモードの動的切り替え
            UpdateCameraMode();

            // Look Aheadシステムの更新
            if (enableLookAhead)
            {
                UpdateLookAheadSystem();
            }

            // 動的高さ調整
            if (enableDynamicHeight)
            {
                UpdateDynamicHeight();
            }

            // ターゲット位置・回転の計算
            CalculateTargetTransform();

            // 境界チェック
            if (enableBoundaries)
            {
                CheckCameraBoundaries();
            }

            // 障害物回避
            CheckObstacleAvoidance();
        }

        /// <summary>
        /// プレイヤー状態の更新
        /// </summary>
        private void UpdatePlayerState()
        {
            if (playerController != null)
            {
                playerVelocity = playerController.GetVelocity();
                isPlayerGrounded = playerController.IsGrounded();
                isPlayerJumping = playerController.IsJumping();
                playerFallSpeed = playerVelocity.y < 0 ? Mathf.Abs(playerVelocity.y) : 0f;
            }
            else if (playerTarget != null)
            {
                // PlayerControllerがない場合の基本状態推定
                Vector3 previousPos = targetPosition;
                Vector3 currentPos = playerTarget.position;
                playerVelocity = (currentPos - previousPos) / Time.deltaTime;
            }
        }

        /// <summary>
        /// カメラモードの動的更新
        /// </summary>
        private void UpdateCameraMode()
        {
            PlatformerCameraMode newMode = DetermineCameraMode();

            if (newMode != currentMode)
            {
                SetCameraMode(newMode);
            }
        }

        /// <summary>
        /// 適切なカメラモードの決定
        /// </summary>
        private PlatformerCameraMode DetermineCameraMode()
        {
            // ジャンプ中
            if (isPlayerJumping && playerVelocity.y > 2f)
            {
                return PlatformerCameraMode.Jump;
            }

            // 高速落下中
            if (playerFallSpeed > 5f)
            {
                return PlatformerCameraMode.Fall;
            }

            // 水平移動速度が高い場合
            Vector3 horizontalVel = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            if (horizontalVel.magnitude > 8f && enableLookAhead)
            {
                return PlatformerCameraMode.LookAhead;
            }

            // デフォルトは通常追従
            return PlatformerCameraMode.Follow;
        }

        /// <summary>
        /// Look Aheadシステムの更新
        /// </summary>
        private void UpdateLookAheadSystem()
        {
            if (playerTarget == null) return;

            Vector3 playerDirection = new Vector3(playerVelocity.x, 0, playerVelocity.z).normalized;
            float playerSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude;

            // 方向変化の検出
            if (Vector3.Dot(playerDirection, lastPlayerDirection) < 0.5f && playerSpeed > 1f)
            {
                lastDirectionChangeTime = Time.time;
            }

            // Look Ahead オフセットの計算
            if (playerSpeed > 1f && Time.time - lastDirectionChangeTime > lookAheadBuffer)
            {
                Vector3 targetLookAhead = playerDirection * lookAheadDistance;
                currentLookAheadOffset = Vector3.Slerp(currentLookAheadOffset, targetLookAhead,
                    lookAheadSpeed * Time.deltaTime);
            }
            else
            {
                // 停止時または方向転換後はLook Aheadを減少
                currentLookAheadOffset = Vector3.Slerp(currentLookAheadOffset, Vector3.zero,
                    lookAheadSpeed * Time.deltaTime);
            }

            lastPlayerDirection = playerDirection;
        }

        /// <summary>
        /// 動的高さ調整の更新
        /// </summary>
        private void UpdateDynamicHeight()
        {
            if (playerTarget == null) return;

            float heightAdjustment = 0f;

            // ジャンプ中の高さ調整
            if (isPlayerJumping)
            {
                heightAdjustment = Mathf.Lerp(0f, jumpHeight, playerVelocity.y / 15f);
            }
            // 落下中の高さ調整
            else if (playerFallSpeed > 2f)
            {
                heightAdjustment = -Mathf.Lerp(0f, fallDistance, playerFallSpeed / 20f);
            }

            // 現在のオフセットに高さ調整を適用
            Vector3 adjustedOffset = followOffset;
            adjustedOffset.y += heightAdjustment;

            // Virtual Cameraの更新
            if (followCamera != null)
            {
                var transposer = followCamera.GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    transposer.m_FollowOffset = Vector3.Slerp(transposer.m_FollowOffset, adjustedOffset,
                        verticalFollowSpeed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// ターゲット位置・回転の計算
        /// </summary>
        private void CalculateTargetTransform()
        {
            if (playerTarget == null) return;

            // 基本位置の計算
            Vector3 basePosition = playerTarget.position + followOffset + currentLookAheadOffset;

            // カメラモード別調整
            switch (currentMode)
            {
                case PlatformerCameraMode.Jump:
                    basePosition.y += 2f;
                    break;
                case PlatformerCameraMode.Fall:
                    basePosition.y -= 1f;
                    break;
                case PlatformerCameraMode.Overview:
                    basePosition += Vector3.up * 10f + Vector3.back * 5f;
                    break;
            }

            targetPosition = basePosition;

            // 回転の計算
            Vector3 lookDirection = (playerTarget.position - targetPosition).normalized;
            targetRotation = Quaternion.LookRotation(lookDirection);
        }

        /// <summary>
        /// カメラ境界チェック
        /// </summary>
        private void CheckCameraBoundaries()
        {
            if (cameraBounds.Contains(targetPosition)) return;

            // 境界内に収まるよう位置を調整
            targetPosition = ConstrainToBounds(targetPosition, cameraBounds);
        }

        /// <summary>
        /// 位置を境界内に制限
        /// </summary>
        private Vector3 ConstrainToBounds(Vector3 position, Bounds bounds)
        {
            return new Vector3(
                Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
                Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
            );
        }

        /// <summary>
        /// 障害物回避チェック
        /// </summary>
        private void CheckObstacleAvoidance()
        {
            if (playerTarget == null) return;

            Vector3 directionToPlayer = (playerTarget.position - targetPosition).normalized;
            float distanceToPlayer = Vector3.Distance(targetPosition, playerTarget.position);

            // プレイヤーまでの経路に障害物がないかチェック
            if (Physics.Raycast(targetPosition, directionToPlayer, out RaycastHit hit,
                distanceToPlayer - 0.1f, obstacleLayer))
            {
                // 障害物を避ける位置を計算
                Vector3 avoidanceDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
                targetPosition += avoidanceDirection * obstacleAvoidanceDistance;
            }
        }

        /// <summary>
        /// カメラ変換の適用
        /// </summary>
        private void ApplyCameraTransform()
        {
            if (!isTransitioning)
            {
                // 通常のスムーズ追従
                float dampingFactor = dampingCurve.Evaluate(Time.deltaTime);

                transform.position = Vector3.Slerp(transform.position, targetPosition,
                    positionDamping * dampingFactor * Time.deltaTime);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    rotationDamping * dampingFactor * Time.deltaTime);
            }
        }

        #endregion

        #region Camera Mode Management

        /// <summary>
        /// カメラモードの設定
        /// </summary>
        public void SetCameraMode(PlatformerCameraMode mode)
        {
            if (currentMode == mode) return;

            previousMode = currentMode;
            currentMode = mode;

            LogDebug($"[PlatformerCamera] Camera mode changed: {previousMode} → {currentMode}");

            // Virtual Camera優先度の更新
            UpdateVirtualCameraPriorities();

            // モード固有の処理
            OnCameraModeChanged();
        }

        /// <summary>
        /// Virtual Camera優先度の更新
        /// </summary>
        private void UpdateVirtualCameraPriorities()
        {
            switch (currentMode)
            {
                case PlatformerCameraMode.Follow:
                case PlatformerCameraMode.LookAhead:
                case PlatformerCameraMode.Jump:
                case PlatformerCameraMode.Fall:
                    if (followCamera != null) followCamera.Priority = 10;
                    if (aimCamera != null) aimCamera.Priority = 0;
                    if (overviewCamera != null) overviewCamera.Priority = 0;
                    break;

                case PlatformerCameraMode.Overview:
                    if (followCamera != null) followCamera.Priority = 0;
                    if (aimCamera != null) aimCamera.Priority = 0;
                    if (overviewCamera != null) overviewCamera.Priority = 10;
                    break;

                case PlatformerCameraMode.Cinematic:
                    // シネマティックモード時の設定
                    break;
            }
        }

        /// <summary>
        /// カメラモード変更時の処理
        /// </summary>
        private void OnCameraModeChanged()
        {
            // モード別の初期設定
            switch (currentMode)
            {
                case PlatformerCameraMode.Jump:
                    // ジャンプモード固有の設定
                    break;

                case PlatformerCameraMode.Fall:
                    // 落下モード固有の設定
                    break;

                case PlatformerCameraMode.Overview:
                    // 俯瞰モード固有の設定
                    break;
            }

            // イベント発行
            // GameEventの発行などがここに入る
        }

        /// <summary>
        /// 現在のカメラモードを取得
        /// </summary>
        public PlatformerCameraMode GetCurrentMode()
        {
            return currentMode;
        }

        /// <summary>
        /// カメラモードを強制設定
        /// </summary>
        public void ForceSetCameraMode(PlatformerCameraMode mode)
        {
            SetCameraMode(mode);
        }

        #endregion

        #region Public API

        /// <summary>
        /// プレイヤーターゲットの設定
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;

            if (target != null)
            {
                playerController = target.GetComponent<PlatformerPlayerController>();

                // Virtual Cameraターゲットの更新
                if (followCamera != null)
                {
                    followCamera.Follow = target;
                    followCamera.LookAt = target;
                }
            }
        }

        /// <summary>
        /// カメラ設定の動的更新
        /// </summary>
        public void UpdateCameraSettings(Vector3 newOffset, float newFollowSpeed)
        {
            followOffset = newOffset;
            followSpeed = newFollowSpeed;

            // Virtual Cameraの設定更新
            if (followCamera != null)
            {
                var transposer = followCamera.GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    transposer.m_FollowOffset = newOffset;
                }
            }
        }

        /// <summary>
        /// カメラシェイク効果
        /// </summary>
        public void PlayCameraShake(float intensity = 1f, float duration = 0.5f)
        {
            // カメラシェイクの実装
            LogDebug($"[PlatformerCamera] Camera shake: intensity={intensity}, duration={duration}");
        }

        #endregion

        #region Template Actions

        [TabGroup("Actions", "Camera Control")]
        [Button("Test Follow Mode")]
        public void TestFollowMode()
        {
            SetCameraMode(PlatformerCameraMode.Follow);
        }

        [Button("Test Jump Mode")]
        public void TestJumpMode()
        {
            SetCameraMode(PlatformerCameraMode.Jump);
        }

        [Button("Test Overview Mode")]
        public void TestOverviewMode()
        {
            SetCameraMode(PlatformerCameraMode.Overview);
        }

        [Button("Test Camera Shake")]
        public void TestCameraShake()
        {
            PlayCameraShake(2f, 1f);
        }

        [Button("Validate Camera Setup")]
        public void ValidateCameraSetup()
        {
            LogDebug("=== Platformer Camera Validation ===");
            LogDebug($"Camera Enabled: {enableCamera}");
            LogDebug($"Initialization Status: {isInitialized}");
            LogDebug($"Player Target: {(playerTarget != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Follow Camera: {(followCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Current Mode: {currentMode}");
            LogDebug($"Player Grounded: {isPlayerGrounded}");
            LogDebug($"Player Velocity: {playerVelocity}");
            LogDebug("=== Validation Complete ===");
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
        private void OnDrawGizmosSelected()
        {
            if (!enableCamera) return;

            // カメラ追従範囲の可視化
            Gizmos.color = Color.blue;
            if (playerTarget != null)
            {
                Vector3 cameraPos = playerTarget.position + followOffset;
                Gizmos.DrawWireSphere(cameraPos, 2f);
                Gizmos.DrawLine(playerTarget.position, cameraPos);
            }

            // Look Ahead範囲の可視化
            if (enableLookAhead)
            {
                Gizmos.color = Color.green;
                Vector3 lookAheadPos = transform.position + currentLookAheadOffset;
                Gizmos.DrawWireSphere(lookAheadPos, 1f);
            }

            // カメラ境界の可視化
            if (enableBoundaries)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(cameraBounds.center, cameraBounds.size);
            }

            // ターゲット位置の可視化
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }
#endif

        #endregion
    }
}