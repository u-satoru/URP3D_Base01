using UnityEngine;
using Unity.Cinemachine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Features.Player;
using asterivo.Unity60.Features.Player.States;
// using asterivo.Unity60.Features.Player.Stealth; // Fixed: namespace doesn't exist
using asterivo.Unity60.Features.AI.Visual;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// ステルス特化カメラコントローラー
    /// プレイヤーの隠密状態に応じた最適なカメラアングルと制御を提供
    /// </summary>
    public class StealthCameraController : MonoBehaviour
    {
        #region Stealth Camera Configuration

        [TabGroup("Stealth Camera", "Basic Settings")]
        [Title("Stealth Camera Settings", "隠密行動に最適化されたカメラシステム", TitleAlignments.Centered)]
        [SerializeField] private bool enableStealthCamera = true;
        [SerializeField] private bool debugMode = false;

        [TabGroup("Stealth Camera", "Camera References")]
        [Header("Cinemachine Virtual Cameras")]
        [SerializeField] private CinemachineCamera normalCamera;
        [SerializeField] private CinemachineCamera crouchCamera;
        [SerializeField] private CinemachineCamera proneCamera;
        [SerializeField] private CinemachineCamera coverCamera;
        [SerializeField] private CinemachineCamera peekingCamera;

        [TabGroup("Stealth Camera", "Detection Integration")]
        [Header("Detection Indicator Settings")]
        [SerializeField] private bool enableDetectionIndicators = true;
        [SerializeField] private float detectionUIDistance = 50f;
        [SerializeField] private LayerMask detectionLayerMask = -1;
        [SerializeField] private Color undetectedColor = Color.green;
        [SerializeField] private Color suspiciousColor = Color.yellow;
        [SerializeField] private Color alertColor = Color.red;

        #endregion

        #region Camera Behavior Settings

        [TabGroup("Stealth Camera", "Behavior")]
        [Header("Low-Profile Camera Angles")]
        [SerializeField, Range(0f, 45f)] private float crouchCameraAngle = 15f;
        [SerializeField, Range(0f, 30f)] private float proneCameraAngle = 8f;
        [SerializeField, Range(2f, 10f)] private float normalCameraDistance = 5f;
        [SerializeField, Range(1.5f, 8f)] private float crouchCameraDistance = 3.5f;
        [SerializeField, Range(1f, 6f)] private float proneCameraDistance = 2.5f;

        [TabGroup("Stealth Camera", "Shadow")]
        [Header("Shadow-Enhanced Visibility")]
        [SerializeField] private bool enableShadowDetection = true;
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private float shadowThreshold = 0.3f;
        [SerializeField] private float shadowCameraBoost = 1.2f;
        [SerializeField] private AnimationCurve shadowVisibilityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [TabGroup("Stealth Camera", "Transitions")]
        [Header("Camera Transition Settings")]
        [SerializeField, Range(0.1f, 2f)] private float transitionSpeed = 0.8f;
        [SerializeField, Range(0.05f, 0.5f)] private float stealthTransitionSpeed = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        #endregion

        #region Runtime State

        [TabGroup("Stealth Camera", "Runtime")]
        [Header("Current State")]
        [SerializeField, ReadOnly] private StealthCameraMode currentMode = StealthCameraMode.Normal;
        [SerializeField, ReadOnly] private float currentShadowLevel = 0f;
        [SerializeField, ReadOnly] private bool isInShadow = false;
        [SerializeField, ReadOnly] private bool isDetectionActive = false;
        [SerializeField, ReadOnly] private Vector3 lastKnownPosition;

        private StealthMechanics stealthMechanics;
        private IStealthAudioService stealthAudioService;
        private Transform playerTransform;
        private UnityEngine.Camera mainCamera;
        private CinemachineCamera activeVirtualCamera;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStealthCamera();
        }

        private void Start()
        {
            if (enableStealthCamera)
            {
                SetupStealthCameraSystem();
            }
        }

        private void Update()
        {
            if (enableStealthCamera && playerTransform != null)
            {
                UpdateStealthCamera();
            }
        }

        private void LateUpdate()
        {
            if (enableStealthCamera && enableShadowDetection)
            {
                UpdateShadowDetection();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// ステルスカメラシステムの初期化
        /// </summary>
        private void InitializeStealthCamera()
        {
            LogDebug("[StealthCamera] Initializing Stealth Camera System...");

            try
            {
                // メインカメラの取得
                mainCamera = UnityEngine.Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
                }

                // プレイヤーコンポーネントの取得
                stealthMechanics = StealthMechanics.Instance;
                if (stealthMechanics != null)
                {
                    playerTransform = stealthMechanics.transform;
                }

                // サービスの取得
                if (FeatureFlags.UseServiceLocator)
                {
                    stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
                }

                // メインライトの自動検出
                if (mainDirectionalLight == null)
                {
                    Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                    foreach (Light light in lights)
                    {
                        if (light.type == LightType.Directional)
                        {
                            mainDirectionalLight = light;
                            break;
                        }
                    }
                }

                LogDebug("[StealthCamera] ✅ Stealth Camera initialization completed");
            }
            catch (System.Exception ex)
            {
                LogError($"[StealthCamera] ❌ Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ステルスカメラシステムのセットアップ
        /// </summary>
        private void SetupStealthCameraSystem()
        {
            LogDebug("[StealthCamera] Setting up Stealth Camera System...");

            // Virtual Cameraの設定
            SetupVirtualCameras();

            // 初期カメラモードの設定
            SetCameraMode(StealthCameraMode.Normal);

            // イベントリスナーの設定
            SetupEventListeners();

            LogDebug("[StealthCamera] ✅ Stealth Camera setup completed");
        }

        /// <summary>
        /// Virtual Cameraの設定
        /// </summary>
        private void SetupVirtualCameras()
        {
            if (normalCamera != null)
            {
                ConfigureVirtualCamera(normalCamera, normalCameraDistance, 0f);
                activeVirtualCamera = normalCamera;
            }

            if (crouchCamera != null)
            {
                ConfigureVirtualCamera(crouchCamera, crouchCameraDistance, crouchCameraAngle);
            }

            if (proneCamera != null)
            {
                ConfigureVirtualCamera(proneCamera, proneCameraDistance, proneCameraAngle);
            }

            // カバーカメラとピーキングカメラの特殊設定
            if (coverCamera != null)
            {
                ConfigureCoverCamera(coverCamera);
            }

            if (peekingCamera != null)
            {
                ConfigurePeekingCamera(peekingCamera);
            }
        }

        /// <summary>
        /// Virtual Cameraの基本設定
        /// </summary>
        private void ConfigureVirtualCamera(CinemachineCamera vcam, float distance, float angle)
        {
            if (vcam == null) return;

            // Follow設定
            if (playerTransform != null)
            {
                vcam.Follow = playerTransform;
                vcam.LookAt = playerTransform;
            }

            // Composer設定
            var composer = vcam.GetComponent<CinemachinePositionComposer>();
            if (composer == null)
            {
                composer = vcam.gameObject.AddComponent<CinemachinePositionComposer>();
            }

            // Transposer設定
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer == null)
            {
                transposer = vcam.gameObject.AddComponent<CinemachineFollow>();
            }

            // 距離とアングルの設定
            transposer.FollowOffset = new Vector3(0f, distance * 0.6f, -distance);
            // composer.TrackedObjectOffset = new Vector3(0f, angle * 0.1f, 0f);
        }

        /// <summary>
        /// カバーカメラの特殊設定
        /// </summary>
        private void ConfigureCoverCamera(CinemachineCamera vcam)
        {
            // カバー時の肩越し視点設定
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                transposer.FollowOffset = new Vector3(1.2f, 1.8f, -2f);
            }

            var composer = vcam.GetComponent<CinemachinePositionComposer>();
            if (composer != null)
            {
                // composer.TrackedObjectOffset = new Vector3(0.5f, 0.2f, 0f);
            }
        }

        /// <summary>
        /// ピーキングカメラの特殊設定
        /// </summary>
        private void ConfigurePeekingCamera(CinemachineCamera vcam)
        {
            // ピーキング時の視点設定
            var transposer = vcam.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                transposer.FollowOffset = new Vector3(0.8f, 1.5f, -1.5f);
            }
        }

        /// <summary>
        /// イベントリスナーの設定
        /// </summary>
        private void SetupEventListeners()
        {
            // プレイヤー状態変更イベントのリスン
            // 実装は既存のイベントシステムに依存
        }

        #endregion

        #region Camera Control

        /// <summary>
        /// ステルスカメラの更新
        /// </summary>
        private void UpdateStealthCamera()
        {
            // プレイヤーの状態に基づくカメラモード決定
            DetermineOptimalCameraMode();

            // カメラトランジションの処理
            ProcessCameraTransition();

            // 検知インジケーターの更新
            if (enableDetectionIndicators)
            {
                UpdateDetectionIndicators();
            }

            // 位置の記録
            lastKnownPosition = playerTransform.position;
        }

        /// <summary>
        /// 最適なカメラモードの決定
        /// </summary>
        private void DetermineOptimalCameraMode()
        {
            if (stealthMechanics == null) return;

            StealthCameraMode newMode = StealthCameraMode.Normal;

            // プレイヤーの状態を検査（StealthMovementControllerから情報取得）
            var stealthMovement = stealthMechanics.GetComponent<StealthMovementController>();
            if (stealthMovement != null)
            {
                // 姿勢に基づくカメラモード決定
                switch (stealthMovement.GetCurrentStance())
                {
                    case MovementStance.Standing:
                        newMode = StealthCameraMode.Normal;
                        break;
                    case MovementStance.Crouching:
                        newMode = StealthCameraMode.Crouch;
                        break;
                    case MovementStance.Prone:
                        newMode = StealthCameraMode.Prone;
                        break;
                }

                // 影の状態による調整
                if (isInShadow && currentShadowLevel > shadowThreshold)
                {
                    ApplyShadowCameraBoost(newMode);
                }
            }

            if (newMode != currentMode)
            {
                SetCameraMode(newMode);
            }
        }

        /// <summary>
        /// カバーモードの詳細決定
        /// </summary>
        private StealthCameraMode DetermineCoverMode(StealthMovementController stealthMovement)
        {
            // ピーキング状態の確認
            // TODO: Add IsPeeking property to StealthMovementController
            if (false) // stealthMovement.IsPeeking
            {
                return StealthCameraMode.Peeking;
            }

            return StealthCameraMode.Cover;
        }

        /// <summary>
        /// カメラモードの設定
        /// </summary>
        public void SetCameraMode(StealthCameraMode newMode)
        {
            if (currentMode == newMode) return;

            var previousMode = currentMode;
            currentMode = newMode;

            // Virtual Cameraの優先度設定
            ResetAllCameraPriorities();

            CinemachineCamera targetCamera = GetCameraForMode(newMode);
            if (targetCamera != null)
            {
                targetCamera.Priority = 10;
                activeVirtualCamera = targetCamera;
            }

            LogDebug($"[StealthCamera] Camera mode changed: {previousMode} → {newMode}");

            // トランジション速度の調整
            AdjustTransitionSpeed(newMode);
        }

        /// <summary>
        /// 全カメラの優先度リセット
        /// </summary>
        private void ResetAllCameraPriorities()
        {
            if (normalCamera != null) normalCamera.Priority = 0;
            if (crouchCamera != null) crouchCamera.Priority = 0;
            if (proneCamera != null) proneCamera.Priority = 0;
            if (coverCamera != null) coverCamera.Priority = 0;
            if (peekingCamera != null) peekingCamera.Priority = 0;
        }

        /// <summary>
        /// モードに対応するカメラの取得
        /// </summary>
        private CinemachineCamera GetCameraForMode(StealthCameraMode mode)
        {
            return mode switch
            {
                StealthCameraMode.Normal => normalCamera,
                StealthCameraMode.Crouch => crouchCamera,
                StealthCameraMode.Prone => proneCamera,
                StealthCameraMode.Cover => coverCamera,
                StealthCameraMode.Peeking => peekingCamera,
                _ => normalCamera
            };
        }

        /// <summary>
        /// トランジション速度の調整
        /// </summary>
        private void AdjustTransitionSpeed(StealthCameraMode mode)
        {
            float speed = (mode == StealthCameraMode.Cover || mode == StealthCameraMode.Peeking)
                ? stealthTransitionSpeed
                : transitionSpeed;

            if (activeVirtualCamera != null)
            {
                // TODO: Update CinemachineCore API for 3.1.4
                // var brain = CinemachineCore.Instance.GetActiveBrain(0);
                // if (brain != null)
                // {
                //     brain.m_DefaultBlend.m_Time = speed;
                // }
            }
        }

        /// <summary>
        /// カメラトランジションの処理
        /// </summary>
        private void ProcessCameraTransition()
        {
            if (activeVirtualCamera == null) return;

            // スムーズなトランジション効果の適用
            ApplyTransitionEffects();
        }

        /// <summary>
        /// トランジション効果の適用
        /// </summary>
        private void ApplyTransitionEffects()
        {
            // カーブを使用したスムーズなトランジション
            if (activeVirtualCamera != null)
            {
                var transposer = activeVirtualCamera.GetComponent<CinemachineFollow>();
                if (transposer != null)
                {
                    float t = Time.time % 1f;
                    float curveValue = transitionCurve.Evaluate(t);
                    // トランジション効果の適用
                }
            }
        }

        #endregion

        #region Shadow Detection & Enhancement

        /// <summary>
        /// 影検知の更新
        /// </summary>
        private void UpdateShadowDetection()
        {
            if (mainDirectionalLight == null || playerTransform == null) return;

            // 光線による影判定
            Vector3 lightDirection = -mainDirectionalLight.transform.forward;
            Vector3 playerPosition = playerTransform.position + Vector3.up * 0.1f;

            RaycastHit hit;
            bool inShadow = Physics.Raycast(playerPosition, lightDirection, out hit, 100f);

            if (inShadow)
            {
                currentShadowLevel = CalculateShadowLevel(hit);
                isInShadow = currentShadowLevel > shadowThreshold;
            }
            else
            {
                currentShadowLevel = 0f;
                isInShadow = false;
            }

            // 影状態の変化を検知
            if (isInShadow)
            {
                ApplyShadowCameraBoost(currentMode);
            }
        }

        /// <summary>
        /// 影レベルの計算
        /// </summary>
        private float CalculateShadowLevel(RaycastHit hit)
        {
            float distance = hit.distance;
            float lightIntensity = mainDirectionalLight.intensity;

            // 距離と光の強度を考慮した影レベル計算
            float shadowLevel = Mathf.Clamp01(1f - (distance * 0.1f)) * lightIntensity;

            return shadowVisibilityCurve.Evaluate(shadowLevel);
        }

        /// <summary>
        /// 影カメラブーストの適用
        /// </summary>
        private void ApplyShadowCameraBoost(StealthCameraMode mode)
        {
            if (activeVirtualCamera == null) return;

            var transposer = activeVirtualCamera.GetComponent<CinemachineFollow>();
            if (transposer != null)
            {
                Vector3 originalOffset = transposer.FollowOffset;
                Vector3 boostedOffset = originalOffset * shadowCameraBoost;

                // スムーズな適用
                transposer.FollowOffset = Vector3.Lerp(originalOffset, boostedOffset, Time.deltaTime * 2f);
            }
        }

        #endregion

        #region Detection Indicators

        /// <summary>
        /// 検知インジケーターの更新
        /// </summary>
        private void UpdateDetectionIndicators()
        {
            if (!enableDetectionIndicators || mainCamera == null) return;

            // 範囲内のNPCを検索
            Collider[] nearbyNPCs = Physics.OverlapSphere(playerTransform.position, detectionUIDistance, detectionLayerMask);

            foreach (Collider npcCollider in nearbyNPCs)
            {
                var npcVisualSensor = npcCollider.GetComponent<NPCVisualSensor>();
                if (npcVisualSensor != null)
                {
                    UpdateNPCDetectionIndicator(npcVisualSensor);
                }
            }
        }

        /// <summary>
        /// NPC検知インジケーターの更新
        /// </summary>
        private void UpdateNPCDetectionIndicator(NPCVisualSensor npcSensor)
        {
            // NPCの警戒レベルに基づく色設定
            Color indicatorColor = GetIndicatorColor(npcSensor.GetCurrentAlertLevel());

            // 画面上のの位置計算
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(npcSensor.transform.position);

            if (screenPosition.z > 0) // カメラの前方にいる場合
            {
                // UI要素の更新（実装はUIシステムに依存）
                DisplayDetectionIndicator(screenPosition, indicatorColor, npcSensor.GetCurrentAlertLevel());
            }
        }

        /// <summary>
        /// インジケーター色の取得
        /// </summary>
        private Color GetIndicatorColor(asterivo.Unity60.Core.Data.AlertLevel alertLevel)
        {
            return alertLevel switch
            {
                asterivo.Unity60.Core.Data.AlertLevel.Relaxed => undetectedColor,
                asterivo.Unity60.Core.Data.AlertLevel.Suspicious => suspiciousColor,
                asterivo.Unity60.Core.Data.AlertLevel.Investigating => suspiciousColor,
                asterivo.Unity60.Core.Data.AlertLevel.Alert => alertColor,
                _ => undetectedColor
            };
        }

        /// <summary>
        /// 検知インジケーターの表示
        /// </summary>
        private void DisplayDetectionIndicator(Vector3 screenPosition, Color color, asterivo.Unity60.Core.Data.AlertLevel alertLevel)
        {
            // UIシステムとの連携（実装はStealthUIManagerに委譲）
            // ここでは基本的な描画処理のみ
        }

        #endregion

        #region Public API

        /// <summary>
        /// 手動カメラモード設定
        /// </summary>
        public void ForceSetCameraMode(StealthCameraMode mode)
        {
            SetCameraMode(mode);
        }

        /// <summary>
        /// カメラシェイク効果
        /// </summary>
        public void TriggerCameraShake(float intensity, float duration)
        {
            if (activeVirtualCamera != null)
            {
                var impulse = activeVirtualCamera.GetComponent<CinemachineImpulseSource>();
                if (impulse == null)
                {
                    impulse = activeVirtualCamera.gameObject.AddComponent<CinemachineImpulseSource>();
                }

                impulse.GenerateImpulse(Vector3.one * intensity);
            }
        }

        /// <summary>
        /// 検知状態の取得
        /// </summary>
        public bool IsPlayerDetected()
        {
            return isDetectionActive;
        }

        /// <summary>
        /// 影状態の取得
        /// </summary>
        public bool IsPlayerInShadow()
        {
            return isInShadow;
        }

        /// <summary>
        /// 現在のカメラモード取得
        /// </summary>
        public StealthCameraMode GetCurrentCameraMode()
        {
            return currentMode;
        }

        #endregion

        #region Editor Support

        [TabGroup("Stealth Camera", "Actions")]
        [Button("Test Normal Camera")]
        public void TestNormalCamera()
        {
            SetCameraMode(StealthCameraMode.Normal);
        }

        [Button("Test Crouch Camera")]
        public void TestCrouchCamera()
        {
            SetCameraMode(StealthCameraMode.Crouch);
        }

        [Button("Test Prone Camera")]
        public void TestProneCamera()
        {
            SetCameraMode(StealthCameraMode.Prone);
        }

        [Button("Test Cover Camera")]
        public void TestCoverCamera()
        {
            SetCameraMode(StealthCameraMode.Cover);
        }

        [Button("Test Camera Shake")]
        public void TestCameraShake()
        {
            TriggerCameraShake(1f, 0.5f);
        }

        [Button("Validate Camera Setup")]
        public void ValidateCameraSetup()
        {
            LogDebug("=== Stealth Camera Validation ===");
            LogDebug($"Normal Camera: {(normalCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Crouch Camera: {(crouchCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Prone Camera: {(proneCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Cover Camera: {(coverCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Peeking Camera: {(peekingCamera != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Main Light: {(mainDirectionalLight != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Player Controller: {(stealthMechanics != null ? "✅ Found" : "❌ Missing")}");
            LogDebug($"Current Mode: {currentMode}");
            LogDebug($"Shadow Level: {currentShadowLevel:F2}");
            LogDebug($"In Shadow: {isInShadow}");
            LogDebug("=== Validation Complete ===");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            // 検知範囲の可視化
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, detectionUIDistance);

            // 影判定レイの可視化
            if (mainDirectionalLight != null)
            {
                Gizmos.color = isInShadow ? Color.blue : Color.yellow;
                Vector3 lightDirection = -mainDirectionalLight.transform.forward;
                Gizmos.DrawRay(playerTransform.position + Vector3.up * 0.1f, lightDirection * 10f);
            }

            // カメラ位置の可視化
            if (activeVirtualCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(activeVirtualCamera.transform.position, Vector3.one * 0.5f);
            }
        }
#endif

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
    }

    /// <summary>
    /// ステルスカメラモード
    /// </summary>
    public enum StealthCameraMode
    {
        Normal,     // 通常立ち状態
        Crouch,     // しゃがみ状態
        Prone,      // 伏せ状態
        Cover,      // カバー状態
        Peeking     // ピーキング状態
    }
}