using UnityEngine;
using Debug = UnityEngine.Debug;
using Unity.Cinemachine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Player; // Player moved to Features
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Helpers;
using asterivo.Unity60.Core.Debug;
using System.Collections.Generic;
using System.Collections;
using CameraState = asterivo.Unity60.Camera.CameraState;
using CoreCameraState = asterivo.Unity60.Core.Events.CameraState;
using CoreCameraStateEvent = asterivo.Unity60.Core.Events.CameraStateEvent;

namespace asterivo.Unity60.Features.Camera.Cinemachine
{
    /// <summary>
    /// Cinemachine 3.1邨ｱ蜷医き繝｡繝ｩ繧ｷ繧ｹ繝・Β
    /// Unity 6譛驕ｩ蛹也沿 - 繧､繝吶Φ繝磯ｧ・虚繧｢繝ｼ繧ｭ繝・け繝√Ε蟇ｾ蠢・
    /// ServiceLocator遘ｻ陦後ヱ繧ｿ繝ｼ繝ｳ蟇ｾ蠢・
    /// </summary>
    public class CinemachineIntegration : MonoBehaviour, 
        IGameEventListener<CoreCameraState>, 
        IGameEventListener<Vector2>
    {
        // 笨・ServiceLocator遘ｻ陦・ Legacy Singleton隴ｦ蜻翫す繧ｹ繝・Β・亥ｾ梧婿莠呈鋤諤ｧ縺ｮ縺溘ａ・・
        

        
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
        // TODO: CinemachineBrain.m_DefaultBlend縺ｮ險ｭ螳壹↓菴ｿ逕ｨ莠亥ｮ・
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
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            DontDestroyOnLoad(gameObject);
            
            // ServiceLocator縺ｸ縺ｮ逋ｻ骭ｲ
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
            // 笨・ServiceLocator蟆ら畑螳溯｣・・縺ｿ - Singleton繝代ち繝ｼ繝ｳ螳悟・蜑企勁
            // ServiceLocator縺九ｉ縺ｮ逋ｻ骭ｲ隗｣髯､
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
            // Cinemachine Brain 繧ｳ繝ｳ繝昴・繝阪Φ繝医ｒ蜿門ｾ励∪縺溘・霑ｽ蜉
            cinemachineBrain = UnityEngine.Camera.main.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                cinemachineBrain = UnityEngine.Camera.main.gameObject.AddComponent<CinemachineBrain>();
            }
            
            // 繧ｫ繝｡繝ｩ迥ｶ諷九・霎樊嶌繧呈ｧ狗ｯ・
            BuildCameraStateLookup();
            
            // 蛻晄悄繧ｫ繝｡繝ｩ迥ｶ諷九ｒ險ｭ螳・
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
            // CoreCameraState 繧・CameraState 縺ｫ螟画鋤
            CameraState localState = ConvertCoreCameraStateToLocal(newState);
            SwitchToCamera(localState);
        }

        /// <summary>
        /// CoreCameraState繧偵Ο繝ｼ繧ｫ繝ｫCameraState縺ｫ螟画鋤
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
                    return CameraState.Follow; // 縺ｾ縺溘・驕ｩ蛻・↑繝槭ャ繝斐Φ繧ｰ
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
        /// 謖・ｮ壹＆繧後◆繧ｫ繝｡繝ｩ迥ｶ諷九↓蛻・ｊ譖ｿ縺・
        /// </summary>
        public void SwitchToCamera(CameraState targetState)
        {
            if (!cameraStateLookup.ContainsKey(targetState))
            {
                ProjectDebug.LogWarning($"[CinemachineIntegration] Camera state '{targetState}' not configured!");
                return;
            }
            
            var targetConfig = cameraStateLookup[targetState];
            
            // 迴ｾ蝨ｨ縺ｮ繧ｫ繝｡繝ｩ繧帝撼繧｢繧ｯ繝・ぅ繝門喧
            if (activeCameraConfig != null && activeCameraConfig.virtualCamera != null)
            {
                activeCameraConfig.virtualCamera.Priority = 0;
                
                // 髱槭い繧ｯ繝・ぅ繝門喧繧､繝吶Φ繝医ｒ逋ｺ陦・
                if (activeCameraConfig.deactivationEvent != null)
                {
                    activeCameraConfig.deactivationEvent.Raise();
                }
            }
            
            // 譁ｰ縺励＞繧ｫ繝｡繝ｩ繧偵い繧ｯ繝・ぅ繝門喧
            activeCameraConfig = targetConfig;
            currentCameraState = targetState;
            
            if (activeCameraConfig.virtualCamera != null)
            {
                activeCameraConfig.virtualCamera.Priority = activeCameraConfig.defaultPriority;
                
                // 繧｢繧ｯ繝・ぅ繝門喧繧､繝吶Φ繝医ｒ逋ｺ陦・
                if (activeCameraConfig.activationEvent != null)
                {
                    activeCameraConfig.activationEvent.Raise();
                }
            }
            
            // 繝悶Ξ繝ｳ繝画凾髢薙ｒ險ｭ螳・
            if (cinemachineBrain != null)
            {
                cinemachineBrain.DefaultBlend.Time = activeCameraConfig.blendTime;
                cinemachineBrain.DefaultBlend.Style = activeCameraConfig.blendStyle;
            }
            
            ProjectDebug.Log($"[CinemachineIntegration] Switched to camera: {targetState}");
        }
        
        /// <summary>
        /// 繝ｫ繝・け蜈･蜉帙ｒ驕ｩ逕ｨ
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
            
            // 蝙ら峩蝗櫁ｻ｢縺ｮ蛻ｶ髯・
            currentVerticalRotation -= scaledInput.y;
            currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, -verticalLookLimit, verticalLookLimit);
            
            // Cinemachine Virtual Camera縺ｫ蝗櫁ｻ｢繧帝←逕ｨ
            // Cinemachine 3.1縺ｧ縺ｯ縲√き繝｡繝ｩ縺ｮ蝗櫁ｻ｢縺ｯTransform縺ｧ逶ｴ謗･蛻ｶ蠕｡縺吶ｋ縺薙→縺梧耳螂ｨ
            if (activeCameraConfig?.virtualCamera != null && playerTarget != null)
            {
                // 繝励Ξ繧､繝､繝ｼ縺ｮ繝ｭ繝ｼ繧ｫ繝ｫ蝗櫁ｻ｢縺ｧ邵ｦ蝗櫁ｻ｢繧貞宛蠕｡
                var currentRotation = playerTarget.localEulerAngles;
                currentRotation.x = currentVerticalRotation;
                playerTarget.localEulerAngles = currentRotation;
            }
            
            // 讓ｪ蝗櫁ｻ｢縺ｯ繝励Ξ繧､繝､繝ｼ縺ｮTransform縺ｧ蜃ｦ逅・ｼ・layerController縺ｨ騾｣謳ｺ・・
            if (playerTarget != null)
            {
                playerTarget.Rotate(Vector3.up * scaledInput.x);
            }
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繧ｿ繝ｼ繧ｲ繝・ヨ繧定ｨｭ螳・
        /// </summary>
        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
            
            // 蜈ｨ縺ｦ縺ｮVirtual Camera縺ｮ繧ｿ繝ｼ繧ｲ繝・ヨ繧呈峩譁ｰ
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
        /// FOV繧貞､画峩
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
        /// 迴ｾ蝨ｨ縺ｮ繧ｫ繝｡繝ｩ迥ｶ諷九ｒ蜿門ｾ・
        /// </summary>
        public CameraState GetCurrentCameraState()
        {
            return currentCameraState;
        }
        #endregion
    }
}

