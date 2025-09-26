using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.TPS.Data;

namespace asterivo.Unity60.Features.Templates.TPS
{
    /// <summary>
    /// TPS Template Configuration - Central ScriptableObject for TPS template settings
    /// Feature layer placement following architecture separation principles
    /// Implements data-driven design with ScriptableObject pattern
    /// </summary>
    [CreateAssetMenu(fileName = "New TPS Template Config", menuName = "Templates/TPS Template Configuration")]
    public class TPSTemplateConfig : ScriptableObject
    {
        [Header("=== TPS Template Configuration ===")]
        [Space(10)]
        
        [Header("Player Configuration")]
        [SerializeField] private TPSPlayerData _playerData;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Vector3 _defaultSpawnPosition = Vector3.zero;
        [SerializeField] private Vector3 _defaultSpawnRotation = Vector3.zero;
        
        [Header("Camera Configuration")]
        [SerializeField] private float _thirdPersonDistance = 5.0f;
        [SerializeField] private float _thirdPersonHeight = 2.0f;
        [SerializeField] private float _aimingFOV = 45.0f;
        [SerializeField] private float _normalFOV = 60.0f;
        [SerializeField] private float _cameraTransitionSpeed = 5.0f;
        
        [Header("Combat Configuration")]
        [SerializeField] private bool _enableAutoAim = false;
        [SerializeField] private float _autoAimRadius = 2.0f;
        [SerializeField] private LayerMask _enemyLayerMask = -1;
        [SerializeField] private LayerMask _coverLayerMask = -1;
        [SerializeField] private bool _enableCoverSystem = true;
        
        [Header("Audio Configuration")]
        [SerializeField] private bool _enableFootstepAudio = true;
        [SerializeField] private bool _enableCombatAudio = true;
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _sfxVolume = 0.8f;
        [SerializeField] private float _musicVolume = 0.6f;
        
        [Header("Input Configuration")]
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _gamepadSensitivity = 3.0f;
        [SerializeField] private bool _invertYAxis = false;
        [SerializeField] private bool _enableGamepadSupport = true;
        
        [Header("Performance Configuration")]
        [SerializeField] private int _maxConcurrentSounds = 10;
        [SerializeField] private float _cullingDistance = 100.0f;
        [SerializeField] private bool _enableOcclusion = true;
        [SerializeField] private int _maxPooledObjects = 50;
        
        [Header("Event Channels")]
        [SerializeField] private GameEvent _templateActivatedEvent;
        [SerializeField] private GameEvent _templateDeactivatedEvent;
        [SerializeField] private GameEvent<string> _templateStateChangedEvent;
        
        [Header("Debug Settings")]
        [SerializeField] private bool _enableDebugMode = false;
        [SerializeField] private bool _showDebugUI = false;
        [SerializeField] private bool _logPlayerActions = false;
        [SerializeField] private bool _showCoverDebugGizmos = false;

        // === Properties for easy access ===
        
        // Player Properties
        public TPSPlayerData PlayerData => _playerData;
        public GameObject PlayerPrefab => _playerPrefab;
        public Vector3 DefaultSpawnPosition => _defaultSpawnPosition;
        public Vector3 DefaultSpawnRotation => _defaultSpawnRotation;
        
        // Camera Properties
        public float ThirdPersonDistance => _thirdPersonDistance;
        public float ThirdPersonHeight => _thirdPersonHeight;
        public float AimingFOV => _aimingFOV;
        public float NormalFOV => _normalFOV;
        public float CameraTransitionSpeed => _cameraTransitionSpeed;
        
        // Combat Properties
        public bool EnableAutoAim => _enableAutoAim;
        public float AutoAimRadius => _autoAimRadius;
        public LayerMask EnemyLayerMask => _enemyLayerMask;
        public LayerMask CoverLayerMask => _coverLayerMask;
        public bool EnableCoverSystem => _enableCoverSystem;
        
        // Audio Properties
        public bool EnableFootstepAudio => _enableFootstepAudio;
        public bool EnableCombatAudio => _enableCombatAudio;
        public float MasterVolume => _masterVolume;
        public float SFXVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;
        
        // Input Properties
        public float MouseSensitivity => _mouseSensitivity;
        public float GamepadSensitivity => _gamepadSensitivity;
        public bool InvertYAxis => _invertYAxis;
        public bool EnableGamepadSupport => _enableGamepadSupport;
        
        // Performance Properties
        public int MaxConcurrentSounds => _maxConcurrentSounds;
        public float CullingDistance => _cullingDistance;
        public bool EnableOcclusion => _enableOcclusion;
        public int MaxPooledObjects => _maxPooledObjects;
        
        // Event Properties
        public GameEvent TemplateActivatedEvent => _templateActivatedEvent;
        public GameEvent TemplateDeactivatedEvent => _templateDeactivatedEvent;
        public GameEvent<string> TemplateStateChangedEvent => _templateStateChangedEvent;
        
        // Debug Properties
        public bool EnableDebugMode => _enableDebugMode;
        public bool ShowDebugUI => _showDebugUI;
        public bool LogPlayerActions => _logPlayerActions;
        public bool ShowCoverDebugGizmos => _showCoverDebugGizmos;

        /// <summary>
        /// Apply template configuration to the scene
        /// Called when TPS template is activated
        /// </summary>
        public void ApplyConfiguration()
        {
            if (_enableDebugMode)
            {
                Debug.Log("[TPSTemplateConfig] Applying TPS template configuration...");
            }
            
            // Apply camera settings
            ApplyCameraSettings();
            
            // Apply audio settings
            ApplyAudioSettings();
            
            // Apply input settings
            ApplyInputSettings();
            
            // Apply performance settings
            ApplyPerformanceSettings();
            
            // Raise template activated event
            if (_templateActivatedEvent != null)
            {
                _templateActivatedEvent.Raise();
            }
            
            if (_templateStateChangedEvent != null)
            {
                _templateStateChangedEvent.Raise("TPS_Template_Active");
            }
            
            if (_enableDebugMode)
            {
                Debug.Log("[TPSTemplateConfig] TPS template configuration applied successfully.");
            }
        }
        
        /// <summary>
        /// Apply camera settings to the active camera system
        /// </summary>
        private void ApplyCameraSettings()
        {
            // Implementation will integrate with CameraManager service via ServiceLocator
            var cameraManager = ServiceLocator.GetService<ICameraManager>();
            if (cameraManager != null)
            {
                // Configure TPS camera settings
                cameraManager.SetThirdPersonDistance(_thirdPersonDistance);
                cameraManager.SetThirdPersonHeight(_thirdPersonHeight);
                cameraManager.SetFieldOfView(_normalFOV);
                cameraManager.SetTransitionSpeed(_cameraTransitionSpeed);
                
                if (_enableDebugMode)
                {
                    Debug.Log($"[TPSTemplateConfig] Camera settings applied: Distance={_thirdPersonDistance}, Height={_thirdPersonHeight}, FOV={_normalFOV}");
                }
            }
        }
        
        /// <summary>
        /// Apply audio settings to the audio system
        /// </summary>
        private void ApplyAudioSettings()
        {
            var audioManager = ServiceLocator.GetService<IAudioManager>();
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(_masterVolume);
                audioManager.SetSFXVolume(_sfxVolume);
                audioManager.SetMusicVolume(_musicVolume);
                
                if (_enableDebugMode)
                {
                    Debug.Log($"[TPSTemplateConfig] Audio settings applied: Master={_masterVolume}, SFX={_sfxVolume}, Music={_musicVolume}");
                }
            }
        }
        
        /// <summary>
        /// Apply input settings to the input system
        /// </summary>
        private void ApplyInputSettings()
        {
            var inputManager = ServiceLocator.GetService<IInputManager>();
            if (inputManager != null)
            {
                inputManager.SetMouseSensitivity(_mouseSensitivity);
                inputManager.SetGamepadSensitivity(_gamepadSensitivity);
                inputManager.SetInvertYAxis(_invertYAxis);
                
                if (_enableDebugMode)
                {
                    Debug.Log($"[TPSTemplateConfig] Input settings applied: Mouse={_mouseSensitivity}, Gamepad={_gamepadSensitivity}, InvertY={_invertYAxis}");
                }
            }
        }
        
        /// <summary>
        /// Apply performance settings to relevant systems
        /// </summary>
        private void ApplyPerformanceSettings()
        {
            var poolManager = ServiceLocator.GetService<IPoolManager>();
            if (poolManager != null)
            {
                poolManager.SetMaxPoolSize(_maxPooledObjects);
                
                if (_enableDebugMode)
                {
                    Debug.Log($"[TPSTemplateConfig] Performance settings applied: MaxPooled={_maxPooledObjects}, Culling={_cullingDistance}");
                }
            }
        }
        
        /// <summary>
        /// Deactivate template configuration
        /// Called when switching away from TPS template
        /// </summary>
        public void DeactivateConfiguration()
        {
            if (_enableDebugMode)
            {
                Debug.Log("[TPSTemplateConfig] Deactivating TPS template configuration...");
            }
            
            // Raise template deactivated event
            if (_templateDeactivatedEvent != null)
            {
                _templateDeactivatedEvent.Raise();
            }
            
            if (_templateStateChangedEvent != null)
            {
                _templateStateChangedEvent.Raise("TPS_Template_Inactive");
            }
            
            if (_enableDebugMode)
            {
                Debug.Log("[TPSTemplateConfig] TPS template configuration deactivated.");
            }
        }
        
        /// <summary>
        /// Validate configuration values in the editor
        /// </summary>
        private void OnValidate()
        {
            // Ensure camera values are reasonable
            _thirdPersonDistance = Mathf.Max(1.0f, _thirdPersonDistance);
            _thirdPersonHeight = Mathf.Max(0.1f, _thirdPersonHeight);
            _aimingFOV = Mathf.Clamp(_aimingFOV, 20f, 120f);
            _normalFOV = Mathf.Clamp(_normalFOV, 20f, 120f);
            _cameraTransitionSpeed = Mathf.Max(0.1f, _cameraTransitionSpeed);
            
            // Ensure combat values are reasonable
            _autoAimRadius = Mathf.Max(0.5f, _autoAimRadius);
            
            // Ensure audio values are valid
            _masterVolume = Mathf.Clamp01(_masterVolume);
            _sfxVolume = Mathf.Clamp01(_sfxVolume);
            _musicVolume = Mathf.Clamp01(_musicVolume);
            
            // Ensure input values are reasonable
            _mouseSensitivity = Mathf.Max(0.1f, _mouseSensitivity);
            _gamepadSensitivity = Mathf.Max(0.1f, _gamepadSensitivity);
            
            // Ensure performance values are positive
            _maxConcurrentSounds = Mathf.Max(1, _maxConcurrentSounds);
            _cullingDistance = Mathf.Max(10f, _cullingDistance);
            _maxPooledObjects = Mathf.Max(1, _maxPooledObjects);
        }
    }
}


