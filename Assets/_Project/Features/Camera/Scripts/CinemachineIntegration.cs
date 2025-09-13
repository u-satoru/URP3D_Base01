using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Cinemachine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Player; // Player moved to Features
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core.Debug;
using System.Collections.Generic;
using System.Collections;
using CameraState = asterivo.Unity60.Camera.CameraState;
using CoreCameraState = asterivo.Unity60.Core.Events.CameraState;
using CoreCameraStateEvent = asterivo.Unity60.Core.Events.CameraStateEvent;

namespace asterivo.Unity60.Camera
{
    /// <summary>
    /// Cinemachine 3.1統合カメラシステム
    /// Unity 6最適化版 - イベント駆動アーキテクチャ対応
    /// ServiceLocator移行パターン対応
    /// </summary>
    public class CinemachineIntegration : MonoBehaviour, 
        IGameEventListener<CoreCameraState>, 
        IGameEventListener<Vector2>
    {
        // ✅ ServiceLocator移行: Legacy Singleton警告システム（後方互換性のため）
        

        
        #region Camera Configuration
        [System.Serializable]
        public class CameraConfig
        {
            [Header("Basic Settings")]
            public string cameraName;
            public CinemachineCamera virtualCamera;
            public CameraState cameraState;
            public int defaultPriority = 10;
            
            [Header("Transition Settings")]
            public float blendTime = 1f;
            public CinemachineBlendDefinition.Styles blendStyle = 
                CinemachineBlendDefinition.Styles.EaseInOut;
            
            [Header("Events")]
            public GameEvent activationEvent;
            public GameEvent deactivationEvent;
            
            [Header("FOV Settings")]
            public float fieldOfView = 60f;
            public float minFOV = 30f;
            public float maxFOV = 120f;
        }
        #endregion
        
        #region Inspector Fields
        [Header("Camera System Configuration")]
        [SerializeField] private List<CameraConfig> cameraConfigs = new List<CameraConfig>();
        [SerializeField] private CameraState defaultCameraState = CameraState.Follow;
        
        [Header("Event Channels")]
        [SerializeField] private CoreCameraStateEvent cameraStateChangeEvent;
        [SerializeField] private Vector2GameEvent lookInputEvent;
        [SerializeField] private GameEvent aimStartedEvent;
        [SerializeField] private GameEvent aimEndedEvent;
        
        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 80f;
        [SerializeField] private bool invertYAxis = false;
        
        [Header("Transition Settings")]
        // TODO: CinemachineBrain.m_DefaultBlendの設定に使用予定
#pragma warning disable CS0414 // Field assigned but never used - planned for camera blend configuration
        [SerializeField] private float defaultBlendTime = 1f;
#pragma warning restore CS0414
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #endregion
        
        #region Private Fields
        private CameraState currentCameraState;
        private CameraConfig activeCameraConfig;
        private Dictionary<CameraState, CameraConfig> cameraStateLookup;
        
        // Look Input
        private Vector2 currentLookInput;
        private float currentVerticalRotation;
        
        // Cinemachine Components
        private CinemachineBrain cinemachineBrain;
        private Transform playerTarget;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocatorへの登録
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.RegisterService<CinemachineIntegration>(this);
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) eventLogger.Log("[CinemachineIntegration] Successfully registered to ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.LogError($"[CinemachineIntegration] Failed to register to ServiceLocator: {ex.Message}");
                }
            }
            else
            {
                if (FeatureFlags.EnableDebugLogging)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.LogWarning("[CinemachineIntegration] ServiceLocator is disabled - service not registered");
                }
            }
            
            InitializeCameraSystem();
        }
        
        private void Start()
        {
            SetupDefaultCamera();
        }
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        private void OnDisable()
        {
            UnregisterEventListeners();
        }
        
        private void OnDestroy()
        {
            // ✅ ServiceLocator専用実装のみ - Singletonパターン完全削除
            // ServiceLocatorからの登録解除
            if (FeatureFlags.UseServiceLocator)
            {
                try
                {
                    ServiceLocator.UnregisterService<CinemachineIntegration>();
                    
                    if (FeatureFlags.EnableDebugLogging)
                    {
                        var eventLogger = ServiceLocator.GetService<IEventLogger>();
                        if (eventLogger != null) eventLogger.Log("[CinemachineIntegration] Unregistered from ServiceLocator");
                    }
                }
                catch (System.Exception ex)
                {
                    var eventLogger = ServiceLocator.GetService<IEventLogger>();
                    if (eventLogger != null) eventLogger.LogError($"[CinemachineIntegration] Failed to unregister from ServiceLocator: {ex.Message}");
                }
            }
        }
        #endregion
        
        #region Initialization
        private void InitializeCameraSystem()
        {
            // Cinemachine Brain コンポーネントを取得または追加
            cinemachineBrain = UnityEngine.Camera.main.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                cinemachineBrain = UnityEngine.Camera.main.gameObject.AddComponent<CinemachineBrain>();
            }
            
            // カメラ状態の辞書を構築
            BuildCameraStateLookup();
            
            // 初期カメラ状態を設定
            currentCameraState = defaultCameraState;
        }
        
        private void BuildCameraStateLookup()
        {
            cameraStateLookup = new Dictionary<CameraState, CameraConfig>();
            
            foreach (var config in cameraConfigs)
            {
                if (config.virtualCamera != null && !cameraStateLookup.ContainsKey(config.cameraState))
                {
                    cameraStateLookup[config.cameraState] = config;
                }
            }
        }
        
        private void SetupDefaultCamera()
        {
            if (cameraStateLookup.ContainsKey(defaultCameraState))
            {
                SwitchToCamera(defaultCameraState);
            }
        }
        #endregion
        
        #region Event System
        private void RegisterEventListeners()
        {
            if (cameraStateChangeEvent != null)
            {
                cameraStateChangeEvent.RegisterListener(this);
            }
            
            if (lookInputEvent != null)
            {
                lookInputEvent.RegisterListener(this);
            }
        }
        
        private void UnregisterEventListeners()
        {
            if (cameraStateChangeEvent != null)
            {
                cameraStateChangeEvent.UnregisterListener(this);
            }
            
            if (lookInputEvent != null)
            {
                lookInputEvent.UnregisterListener(this);
            }
        }

        // IGameEventListener<CoreCameraState> implementation
        public void OnEventRaised(CoreCameraState newState)
        {
            // CoreCameraState を CameraState に変換
            CameraState localState = ConvertCoreCameraStateToLocal(newState);
            SwitchToCamera(localState);
        }

        /// <summary>
        /// CoreCameraStateをローカルCameraStateに変換
        /// </summary>
        private CameraState ConvertCoreCameraStateToLocal(CoreCameraState coreState)
        {
            switch (coreState)
            {
                case CoreCameraState.Follow:
                    return CameraState.Follow;
                case CoreCameraState.Aim:
                    return CameraState.Aim;
                case CoreCameraState.Combat:
                    return CameraState.Combat;
                case CoreCameraState.Cinematic:
                    return CameraState.Cutscene;
                case CoreCameraState.FreeLook:
                    return CameraState.Free;
                case CoreCameraState.FirstPerson:
                    return CameraState.Follow; // または適切なマッピング
                case CoreCameraState.ThirdPerson:
                    return CameraState.Follow;
                default:
                    return CameraState.Follow;
            }
        }

        // IGameEventListener<Vector2> implementation  
        void IGameEventListener<Vector2>.OnEventRaised(Vector2 lookInput)
        {
            OnLookInputReceived(lookInput);
        }
        
        private void OnCameraStateChangeRequested(CameraState newState)
        {
            SwitchToCamera(newState);
        }
        
        private void OnLookInputReceived(Vector2 lookInput)
        {
            currentLookInput = lookInput;
            ApplyLookInput();
        }
        #endregion
        
        #region Camera Control
        /// <summary>
        /// 指定されたカメラ状態に切り替え
        /// </summary>
        public void SwitchToCamera(CameraState targetState)
        {
            if (!cameraStateLookup.ContainsKey(targetState))
            {
                ProjectDebug.LogWarning($"[CinemachineIntegration] Camera state '{targetState}' not configured!");
                return;
            }
            
            var targetConfig = cameraStateLookup[targetState];
            
            // 現在のカメラを非アクティブ化
            if (activeCameraConfig != null && activeCameraConfig.virtualCamera != null)
            {
                activeCameraConfig.virtualCamera.Priority = 0;
                
                // 非アクティブ化イベントを発行
                if (activeCameraConfig.deactivationEvent != null)
                {
                    activeCameraConfig.deactivationEvent.Raise();
                }
            }
            
            // 新しいカメラをアクティブ化
            activeCameraConfig = targetConfig;
            currentCameraState = targetState;
            
            if (activeCameraConfig.virtualCamera != null)
            {
                activeCameraConfig.virtualCamera.Priority = activeCameraConfig.defaultPriority;
                
                // アクティブ化イベントを発行
                if (activeCameraConfig.activationEvent != null)
                {
                    activeCameraConfig.activationEvent.Raise();
                }
            }
            
            // ブレンド時間を設定
            if (cinemachineBrain != null)
            {
                cinemachineBrain.DefaultBlend.Time = activeCameraConfig.blendTime;
                cinemachineBrain.DefaultBlend.Style = activeCameraConfig.blendStyle;
            }
            
            ProjectDebug.Log($"[CinemachineIntegration] Switched to camera: {targetState}");
        }
        
        /// <summary>
        /// ルック入力を適用
        /// </summary>
        private void ApplyLookInput()
        {
            if (activeCameraConfig?.virtualCamera == null)
                return;
            
            Vector2 scaledInput = currentLookInput * lookSensitivity * Time.deltaTime;
            
            if (invertYAxis)
            {
                scaledInput.y = -scaledInput.y;
            }
            
            // 垂直回転の制限
            currentVerticalRotation -= scaledInput.y;
            currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, -verticalLookLimit, verticalLookLimit);
            
            // Cinemachine Virtual Cameraに回転を適用
            // Cinemachine 3.1では、カメラの回転はTransformで直接制御することが推奨
            if (activeCameraConfig?.virtualCamera != null && playerTarget != null)
            {
                // プレイヤーのローカル回転で縦回転を制御
                var currentRotation = playerTarget.localEulerAngles;
                currentRotation.x = currentVerticalRotation;
                playerTarget.localEulerAngles = currentRotation;
            }
            
            // 横回転はプレイヤーのTransformで処理（PlayerControllerと連携）
            if (playerTarget != null)
            {
                playerTarget.Rotate(Vector3.up * scaledInput.x);
            }
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// プレイヤーターゲットを設定
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
            
            // 全てのVirtual Cameraのターゲットを更新
            foreach (var config in cameraConfigs)
            {
                if (config.virtualCamera != null)
                {
                    config.virtualCamera.Follow = target;
                    config.virtualCamera.LookAt = target;
                }
            }
        }
        
        /// <summary>
        /// FOVを変更
        /// </summary>
        public void SetFieldOfView(float fov)
        {
            if (activeCameraConfig?.virtualCamera != null)
            {
                fov = Mathf.Clamp(fov, activeCameraConfig.minFOV, activeCameraConfig.maxFOV);
                activeCameraConfig.virtualCamera.Lens.FieldOfView = fov;
            }
        }
        
        /// <summary>
        /// 現在のカメラ状態を取得
        /// </summary>
        public CameraState GetCurrentCameraState()
        {
            return currentCameraState;
        }
        #endregion
    }
}