using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;

namespace asterivo.Unity60.Features.Templates.TPS.Services
{
    /// <summary>
    /// TPS Template Service Manager - Central hub for TPS-specific services
    /// Integrates with ServiceLocator for dependency management
    /// </summary>
    public class TPSServiceManager : MonoBehaviour
    {
        [Header("TPS Service Configuration")]
        [SerializeField] private bool _autoInitializeOnAwake = true;
        [SerializeField] private bool _enableDebugLogging = false;

        // ServiceLocator-managed dependencies
        private IInputManager _inputManager;
        private IPoolManager _poolManager;
        private IAudioManager _audioManager;
        private IGameEventManager _gameEventManager;
        private ICameraManager _cameraManager;

        private bool _isInitialized = false;

        private void Awake()
        {
            if (_autoInitializeOnAwake)
            {
                InitializeServices();
            }
        }

        /// <summary>
        /// Initialize all TPS services using ServiceLocator pattern
        /// </summary>
        public void InitializeServices()
        {
            if (_isInitialized)
            {
                if (_enableDebugLogging)
                    Debug.LogWarning("[TPSServiceManager] Services already initialized.");
                return;
            }

            try
            {
                // Get services from ServiceLocator
                _inputManager = ServiceLocator.GetService<IInputManager>();
                _poolManager = ServiceLocator.GetService<IPoolManager>();
                _audioManager = ServiceLocator.GetService<IAudioManager>();
                _gameEventManager = ServiceLocator.GetService<IGameEventManager>();
                _cameraManager = ServiceLocator.GetService<ICameraManager>();

                // Validate all required services are available
                ValidateServices();

                _isInitialized = true;

                if (_enableDebugLogging)
                    Debug.Log("[TPSServiceManager] All TPS services initialized successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[TPSServiceManager] Failed to initialize services: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validate that all required services are properly registered
        /// </summary>
        private void ValidateServices()
        {
            if (_inputManager == null)
                throw new System.InvalidOperationException("IInputManager not registered in ServiceLocator");

            if (_poolManager == null)
                throw new System.InvalidOperationException("IPoolManager not registered in ServiceLocator");

            if (_audioManager == null)
                throw new System.InvalidOperationException("IAudioManager not registered in ServiceLocator");

            if (_gameEventManager == null)
                throw new System.InvalidOperationException("IGameEventManager not registered in ServiceLocator");

            if (_cameraManager == null)
                throw new System.InvalidOperationException("ICameraManager not registered in ServiceLocator");
        }

        /// <summary>
        /// Get Input Manager service
        /// </summary>
        public IInputManager GetInputManager()
        {
            EnsureInitialized();
            return _inputManager;
        }

        /// <summary>
        /// Get Pool Manager service
        /// </summary>
        public IPoolManager GetPoolManager()
        {
            EnsureInitialized();
            return _poolManager;
        }

        /// <summary>
        /// Get Audio Manager service
        /// </summary>
        public IAudioManager GetAudioManager()
        {
            EnsureInitialized();
            return _audioManager;
        }

        /// <summary>
        /// Get Game Event Manager service
        /// </summary>
        public IGameEventManager GetGameEventManager()
        {
            EnsureInitialized();
            return _gameEventManager;
        }

        /// <summary>
        /// Get Camera Manager service
        /// </summary>
        public ICameraManager GetCameraManager()
        {
            EnsureInitialized();
            return _cameraManager;
        }

        /// <summary>
        /// Check if services are initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Ensure services are initialized before use
        /// </summary>
        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                InitializeServices();
            }
        }

        private void OnDestroy()
        {
            _isInitialized = false;

            if (_enableDebugLogging)
                Debug.Log("[TPSServiceManager] Services cleaned up.");
        }
    }
}


