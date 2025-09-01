using UnityEngine;
using Unity.Cinemachine;
using Unity6.Core.Events;
using Unity6.Core.Player;
using System.Collections.Generic;
using System.Collections;

namespace Unity6.Camera
{
    /// <summary>
    /// Cinemachine 3.1統合カメラシステム
    /// Unity 6最適化版 - イベント駆動アーキテクチャ対応
    /// </summary>
    public class CinemachineIntegration : MonoBehaviour, 
        IGameEventListener<CameraState>, 
        IGameEventListener<Vector2>
    {
        private static CinemachineIntegration instance;
        public static CinemachineIntegration Instance => instance;
        
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
        [SerializeField] private CameraStateEvent cameraStateChangeEvent;
        [SerializeField] private Vector2GameEvent lookInputEvent;
        [SerializeField] private GameEvent aimStartedEvent;
        [SerializeField] private GameEvent aimEndedEvent;
        
        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float verticalLookLimit = 80f;
        [SerializeField] private bool invertYAxis = false;
        
        [Header("Transition Settings")]
        [SerializeField] private float defaultBlendTime = 1f;
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
            // Singleton Pattern
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCameraSystem();
            }
            else
            {
                Destroy(gameObject);
            }
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

        // IGameEventListener<CameraState> implementation
        public void OnEventRaised(CameraState newState)
        {
            SwitchToCamera(newState);
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
                Debug.LogWarning($"[CinemachineIntegration] Camera state '{targetState}' not configured!");
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
            
            Debug.Log($"[CinemachineIntegration] Switched to camera: {targetState}");
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