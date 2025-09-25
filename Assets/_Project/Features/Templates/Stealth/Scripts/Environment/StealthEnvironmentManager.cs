using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.Templates.Stealth.Data;
using asterivo.Unity60.Features.Templates.Stealth.Configuration;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using asterivo.Unity60.Features.Templates.Stealth.Events;
using asterivo.Unity60.Features.Templates.Stealth.Commands;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// Layer 5: Environment Interaction System
    /// Manages hiding spots, environmental concealment, and stealth-related interactions
    /// Part of 6-layer stealth template architecture for Learn & Grow value realization
    /// </summary>
    public class StealthEnvironmentManager : MonoBehaviour
    {
        [Header("Environment Configuration")]
        [SerializeField] private StealthEnvironmentConfig _config;
        [SerializeField] private LayerMask _hidingSpotLayer = -1;
        [SerializeField] private LayerMask _environmentLayer = -1;

        [Header("Event Integration")]
        [SerializeField] private StealthDetectionEventChannel _detectionEvents;
        [SerializeField] private GameEvent _onHidingSpotEntered;
        [SerializeField] private GameEvent _onHidingSpotExited;
        [SerializeField] private GameEvent _onEnvironmentChanged;

        [Header("Debug Visualization")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private Color _hidingSpotColor = Color.green;
        [SerializeField] private Color _dangerZoneColor = Color.red;

        // Environment Management
        private readonly Dictionary<int, HidingSpot> _hidingSpots = new();
        private readonly Dictionary<int, EnvironmentalElement> _environmentElements = new();
        private readonly List<Transform> _activeHidingSpots = new();
        
        // Player interaction tracking
        private HidingSpot _currentHidingSpot;
        private Transform _playerTransform;
        private bool _isInitialized = false;

        // Performance optimization
        private readonly Dictionary<Transform, float> _lastUpdateTimes = new();
        private float _nextUpdateTime;

        // TODO: Add these properties to StealthEnvironmentConfig
        private const float DefaultDetectionRadius = 10f;
        private const float DefaultShadowConcealmentMultiplier = 0.7f;
        private const float DefaultFoliageConcealmentMultiplier = 0.8f;
        private const float DefaultLightExposureMultiplier = 1.5f;
        private const float DefaultDiscoveryRadius = 15f;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config == null)
            {
                _config = Resources.Load<StealthEnvironmentConfig>("DefaultStealthEnvironmentConfig");
            }
        }

        private void Start()
        {
            InitializeEnvironmentSystem();
        }

        private void Update()
        {
            if (!_isInitialized) return;

            if (Time.time >= _nextUpdateTime)
            {
                UpdateEnvironmentDetection();
                _nextUpdateTime = Time.time + _config.UpdateInterval;
            }

            ProcessHidingSpotInteractions();
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

        private void InitializeEnvironmentSystem()
        {
            // Find player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }

            // Discover and register hiding spots
            DiscoverHidingSpots();
            
            // Discover environmental elements
            DiscoverEnvironmentalElements();

            // Initialize performance tracking
            _nextUpdateTime = Time.time + _config.UpdateInterval;

            _isInitialized = true;

            LogDebug($"StealthEnvironmentManager initialized with {_hidingSpots.Count} hiding spots and {_environmentElements.Count} environment elements");
        }

        private void DiscoverHidingSpots()
        {
            var hidingSpotColliders = Physics.OverlapSphere(transform.position, DefaultDiscoveryRadius, _hidingSpotLayer);
            
            foreach (var collider in hidingSpotColliders)
            {
                var hidingSpot = collider.GetComponent<HidingSpot>();
                if (hidingSpot == null)
                {
                    // Auto-create hiding spot component if missing
                    hidingSpot = collider.gameObject.AddComponent<HidingSpot>();
                    hidingSpot.Initialize(_config.DefaultConcealmentLevel, _config.DefaultCapacity);
                }

                RegisterHidingSpot(hidingSpot);
            }
        }

        private void DiscoverEnvironmentalElements()
        {
            var environmentColliders = Physics.OverlapSphere(transform.position, DefaultDiscoveryRadius, _environmentLayer);
            
            foreach (var collider in environmentColliders)
            {
                var element = collider.GetComponent<EnvironmentalElement>();
                if (element == null)
                {
                    // Auto-create environmental element if missing
                    element = collider.gameObject.AddComponent<EnvironmentalElement>();
                }

                RegisterEnvironmentalElement(element);
            }
        }

        #endregion

        #region Hiding Spot Management

        public void RegisterHidingSpot(HidingSpot hidingSpot)
        {
            if (hidingSpot == null) return;

            int id = hidingSpot.GetInstanceID();
            if (!_hidingSpots.ContainsKey(id))
            {
                _hidingSpots[id] = hidingSpot;
                hidingSpot.OnPlayerEntered += OnPlayerEnteredHidingSpot;
                hidingSpot.OnPlayerExited += OnPlayerExitedHidingSpot;
                
                LogDebug($"Registered hiding spot: {hidingSpot.name} (Concealment: {hidingSpot.ConcealmentLevel})");
            }
        }

        public void UnregisterHidingSpot(HidingSpot hidingSpot)
        {
            if (hidingSpot == null) return;

            int id = hidingSpot.GetInstanceID();
            if (_hidingSpots.ContainsKey(id))
            {
                hidingSpot.OnPlayerEntered -= OnPlayerEnteredHidingSpot;
                hidingSpot.OnPlayerExited -= OnPlayerExitedHidingSpot;
                _hidingSpots.Remove(id);
                
                LogDebug($"Unregistered hiding spot: {hidingSpot.name}");
            }
        }

        private void OnPlayerEnteredHidingSpot(HidingSpot hidingSpot)
        {
            _currentHidingSpot = hidingSpot;
            
            // Create and execute hiding spot command
            if (ServiceLocator.GetService<ICommandInvoker>() != null)
            {
                var command = new HidingSpotInteractionCommand(hidingSpot, true);
                ServiceLocator.GetService<ICommandInvoker>().ExecuteCommand(command);
            }

            // Raise events
            _onHidingSpotEntered?.Raise();
            
            // Notify detection system
            if (_detectionEvents != null)
            {
                var playerPosition = _playerTransform != null ? _playerTransform.position : hidingSpot.transform.position;
                var distance = _playerTransform != null ? Vector3.Distance(hidingSpot.transform.position, _playerTransform.position) : 0f;

                var detectionData = AIDetectionData.Create(
                    "Environment",
                    asterivo.Unity60.Core.Data.DetectionType.Environmental,
                    0.0f, // Reduced suspicion in concealment
                    distance,
                    hidingSpot.transform.position,
                    playerPosition
                );

                _detectionEvents.RaiseDetectionEvent(detectionData);
            }

            LogDebug($"Player entered hiding spot: {hidingSpot.name} (Concealment: {hidingSpot.ConcealmentLevel})");
        }

        private void OnPlayerExitedHidingSpot(HidingSpot hidingSpot)
        {
            if (_currentHidingSpot == hidingSpot)
            {
                _currentHidingSpot = null;
            }
            
            // Create and execute hiding spot command
            if (ServiceLocator.GetService<ICommandInvoker>() != null)
            {
                var command = new HidingSpotInteractionCommand(hidingSpot, false);
                ServiceLocator.GetService<ICommandInvoker>().ExecuteCommand(command);
            }

            // Raise events
            _onHidingSpotExited?.Raise();

            LogDebug($"Player exited hiding spot: {hidingSpot.name}");
        }

        public HidingSpot GetCurrentHidingSpot() => _currentHidingSpot;

        public List<HidingSpot> GetNearbyHidingSpots(Vector3 position, float radius)
        {
            return _hidingSpots.Values
                .Where(spot => Vector3.Distance(spot.transform.position, position) <= radius)
                .ToList();
        }

        #endregion

        #region Environmental Element Management

        public void RegisterEnvironmentalElement(EnvironmentalElement element)
        {
            if (element == null) return;

            int id = element.GetInstanceID();
            if (!_environmentElements.ContainsKey(id))
            {
                _environmentElements[id] = element;
                element.OnElementActivated += OnEnvironmentalElementActivated;
                element.OnElementDeactivated += OnEnvironmentalElementDeactivated;
                
                LogDebug($"Registered environmental element: {element.name} (Type: {element.ElementType})");
            }
        }

        public void UnregisterEnvironmentalElement(EnvironmentalElement element)
        {
            if (element == null) return;

            int id = element.GetInstanceID();
            if (_environmentElements.ContainsKey(id))
            {
                element.OnElementActivated -= OnEnvironmentalElementActivated;
                element.OnElementDeactivated -= OnEnvironmentalElementDeactivated;
                _environmentElements.Remove(id);
                
                LogDebug($"Unregistered environmental element: {element.name}");
            }
        }

        private void OnEnvironmentalElementActivated(EnvironmentalElement element)
        {
            // Notify other systems about environmental change
            _onEnvironmentChanged?.Raise();
            
            LogDebug($"Environmental element activated: {element.name}");
        }

        private void OnEnvironmentalElementDeactivated(EnvironmentalElement element)
        {
            // Notify other systems about environmental change
            _onEnvironmentChanged?.Raise();
            
            LogDebug($"Environmental element deactivated: {element.name}");
        }

        #endregion

        #region Environment Detection

        private void UpdateEnvironmentDetection()
        {
            if (_playerTransform == null) return;

            var nearbyElements = _environmentElements.Values
                .Where(element => Vector3.Distance(element.transform.position, _playerTransform.position) <= DefaultDetectionRadius)
                .ToList();

            // Process environmental effects
            foreach (var element in nearbyElements)
            {
                ProcessEnvironmentalElement(element);
            }
        }

        private void ProcessEnvironmentalElement(EnvironmentalElement element)
        {
            if (element == null) return;

            var distance = Vector3.Distance(element.transform.position, _playerTransform.position);
            var influence = Mathf.Clamp01(1f - (distance / DefaultDetectionRadius));

            // Apply environmental effect based on type and influence
            switch (element.ElementType)
            {
                case EnvironmentalElementType.Shadow:
                    ProcessShadowElement(element, influence);
                    break;
                    
                case EnvironmentalElementType.Foliage:
                    ProcessFoliageElement(element, influence);
                    break;
                    
                case EnvironmentalElementType.Noise:
                    ProcessNoiseElement(element, influence);
                    break;
                    
                case EnvironmentalElementType.Light:
                    ProcessLightElement(element, influence);
                    break;
            }
        }

        private void ProcessShadowElement(EnvironmentalElement element, float influence)
        {
            if (influence > 0.3f) // Minimum shadow influence threshold
            {
                // Notify stealth mechanics about shadow concealment
                var stealthController = ServiceLocator.GetService<StealthMechanics>();
                if (stealthController != null)
                {
                    var concealment = DefaultShadowConcealmentMultiplier * influence;
                    stealthController.ApplyEnvironmentalConcealment(concealment, "Shadow");
                }
            }
        }

        private void ProcessFoliageElement(EnvironmentalElement element, float influence)
        {
            if (influence > 0.2f) // Minimum foliage influence threshold
            {
                // Notify stealth mechanics about foliage concealment
                var stealthController = ServiceLocator.GetService<StealthMechanics>();
                if (stealthController != null)
                {
                    var concealment = DefaultFoliageConcealmentMultiplier * influence;
                    stealthController.ApplyEnvironmentalConcealment(concealment, "Foliage");
                }
            }
        }

        private void ProcessNoiseElement(EnvironmentalElement element, float influence)
        {
            if (influence > 0.4f) // Minimum noise influence threshold
            {
                // Notify audio system about noise masking
                if (_detectionEvents != null)
                {
                    var playerPosition = _playerTransform != null ? _playerTransform.position : element.transform.position;
                    var distance = _playerTransform != null ? Vector3.Distance(element.transform.position, _playerTransform.position) : 0f;

                    var detectionData = AIDetectionData.Create(
                        "NoiseElement",
                        DetectionType.Auditory,
                        0.0f, // Masking reduces suspicion
                        distance,
                        element.transform.position,
                        playerPosition
                    );

                    _detectionEvents.RaiseDetectionEvent(detectionData);
                }
            }
        }

        private void ProcessLightElement(EnvironmentalElement element, float influence)
        {
            if (influence > 0.1f) // Any light influence is relevant
            {
                // Notify stealth mechanics about light exposure
                var stealthController = ServiceLocator.GetService<StealthMechanics>();
                if (stealthController != null)
                {
                    var exposure = DefaultLightExposureMultiplier * influence;
                    var lightDirection = Vector3.down; // Default downward light direction
                    stealthController.ApplyLightExposure(exposure, lightDirection);
                }
            }
        }

        #endregion

        #region Performance Optimization

        private void ProcessHidingSpotInteractions()
        {
            if (_playerTransform == null) return;

            // Optimize by only checking nearby hiding spots
            _activeHidingSpots.Clear();
            
            foreach (var hidingSpot in _hidingSpots.Values)
            {
                var distance = Vector3.Distance(hidingSpot.transform.position, _playerTransform.position);
                if (distance <= _config.InteractionRadius)
                {
                    _activeHidingSpots.Add(hidingSpot.transform);
                    
                    // Update last interaction time for performance tracking
                    _lastUpdateTimes[hidingSpot.transform] = Time.time;
                }
            }

            // Clean up old entries to prevent memory leaks
            // TODO: Add MaxTrackedElements property to StealthEnvironmentConfig
            const int maxTrackedElements = 1000; // Default fallback
            if (_lastUpdateTimes.Count > maxTrackedElements)
            {
                var oldestEntries = _lastUpdateTimes
                    .OrderBy(kvp => kvp.Value)
                    .Take(_lastUpdateTimes.Count - maxTrackedElements)
                    .ToList();

                foreach (var entry in oldestEntries)
                {
                    _lastUpdateTimes.Remove(entry.Key);
                }
            }
        }

        #endregion

        #region Event System Integration

        private void RegisterEventListeners()
        {
            // Register for stealth detection events if needed
            if (_detectionEvents != null)
            {
                // Could listen to detection events for environmental responses
            }
        }

        private void UnregisterEventListeners()
        {
            // Cleanup event listeners
        }

        #endregion

        #region Public API

        /// <summary>
        /// Gets the current environmental concealment level at a position
        /// </summary>
        public float GetEnvironmentalConcealmentAt(Vector3 position)
        {
            float totalConcealment = 0f;
            int concealmentSources = 0;

            // Check hiding spots
            foreach (var hidingSpot in _hidingSpots.Values)
            {
                var distance = Vector3.Distance(hidingSpot.transform.position, position);
                if (distance <= hidingSpot.InfluenceRadius)
                {
                    var influence = 1f - (distance / hidingSpot.InfluenceRadius);
                    totalConcealment += hidingSpot.ConcealmentLevel * influence;
                    concealmentSources++;
                }
            }

            // Check environmental elements
            foreach (var element in _environmentElements.Values)
            {
                if (element.ElementType == EnvironmentalElementType.Shadow || 
                    element.ElementType == EnvironmentalElementType.Foliage)
                {
                    var distance = Vector3.Distance(element.transform.position, position);
                    if (distance <= DefaultDetectionRadius)
                    {
                        var influence = 1f - (distance / DefaultDetectionRadius);
                        
                        if (element.ElementType == EnvironmentalElementType.Shadow)
                            totalConcealment += DefaultShadowConcealmentMultiplier * influence;
                        else if (element.ElementType == EnvironmentalElementType.Foliage)
                            totalConcealment += DefaultFoliageConcealmentMultiplier * influence;
                        
                        concealmentSources++;
                    }
                }
            }

            return concealmentSources > 0 ? totalConcealment / concealmentSources : 0f;
        }

        /// <summary>
        /// Gets the current light exposure level at a position
        /// </summary>
        public float GetLightExposureAt(Vector3 position)
        {
            float totalExposure = 0f;
            int lightSources = 0;

            foreach (var element in _environmentElements.Values)
            {
                if (element.ElementType == EnvironmentalElementType.Light)
                {
                    var distance = Vector3.Distance(element.transform.position, position);
                    if (distance <= DefaultDetectionRadius)
                    {
                        var influence = 1f - (distance / DefaultDetectionRadius);
                        totalExposure += DefaultLightExposureMultiplier * influence;
                        lightSources++;
                    }
                }
            }

            return lightSources > 0 ? totalExposure / lightSources : 0f;
        }

        /// <summary>
        /// Forces a refresh of all environmental elements
        /// Used for Learn & Grow tutorial scenarios
        /// </summary>
        public void RefreshEnvironment()
        {
            DiscoverHidingSpots();
            DiscoverEnvironmentalElements();
            _onEnvironmentChanged?.Raise();

            LogDebug("Environment system refreshed for tutorial scenario");
        }

        /// <summary>
        /// 設定の適用
        /// </summary>
        public void ApplyConfiguration(StealthEnvironmentConfig config)
        {
            if (config != null)
            {
                _config = config;
                RefreshEnvironment();
                UnityEngine.Debug.Log($"[StealthEnvironmentManager] Configuration applied: {config.name}");
            }
        }

        #endregion

        #region Debug and Visualization

        private void LogDebug(string message)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[StealthEnvironmentManager] {message}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugInfo) return;

            // Draw discovery radius
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _config != null ? DefaultDiscoveryRadius : 50f);

            // Draw hiding spots
            Gizmos.color = _hidingSpotColor;
            foreach (var hidingSpot in _hidingSpots.Values)
            {
                if (hidingSpot != null)
                {
                    Gizmos.DrawWireCube(hidingSpot.transform.position, Vector3.one * 2f);
                    Gizmos.DrawWireSphere(hidingSpot.transform.position, hidingSpot.InfluenceRadius);
                }
            }

            // Draw environmental elements
            foreach (var element in _environmentElements.Values)
            {
                if (element != null)
                {
                    Color elementColor = element.ElementType switch
                    {
                        EnvironmentalElementType.Shadow => Color.black,
                        EnvironmentalElementType.Light => Color.yellow,
                        EnvironmentalElementType.Foliage => Color.green,
                        EnvironmentalElementType.Noise => Color.blue,
                        _ => Color.gray
                    };
                    
                    Gizmos.color = elementColor;
                    Gizmos.DrawWireCube(element.transform.position, Vector3.one);
                }
            }

            // Draw active detection radius
            if (_playerTransform != null && _config != null)
            {
                Gizmos.color = _dangerZoneColor;
                Gizmos.DrawWireSphere(_playerTransform.position, DefaultDetectionRadius);
            }
        }

        #endregion
    }
}
