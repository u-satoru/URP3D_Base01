using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// Environmental elements that affect stealth gameplay (shadows, foliage, noise sources, lights)
    /// Part of Layer 5: Environment Interaction System
    /// </summary>
    public class EnvironmentalElement : MonoBehaviour
    {
        [Header("Element Configuration")]
        [SerializeField] private EnvironmentalElementType _elementType = EnvironmentalElementType.Shadow;
        [SerializeField, Range(0f, 1f)] private float _intensity = 0.5f;
        [SerializeField] private float _influenceRadius = 5f;
        [SerializeField] private bool _isActive = true;

        [Header("Dynamic Behavior")]
        [SerializeField] private bool _canToggle = false;
        [SerializeField] private float _toggleInterval = 0f; // 0 = no auto-toggle
        [SerializeField] private AnimationCurve _intensityCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Header("Audio Integration")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _activationSound;
        [SerializeField] private AudioClip _deactivationSound;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem _particleEffect;
        [SerializeField] private Light _lightComponent;
        [SerializeField] private Renderer _renderer;

        // Properties
        public EnvironmentalElementType ElementType => _elementType;
        public float Intensity => _intensity;
        public float InfluenceRadius => _influenceRadius;
        public bool IsActive => _isActive;
        public bool CanToggle => _canToggle;

        // Events
        public event Action<EnvironmentalElement> OnElementActivated;
        public event Action<EnvironmentalElement> OnElementDeactivated;
        public event Action<EnvironmentalElement> OnIntensityChanged;

        // Dynamic state
        private float _currentIntensity;
        private float _toggleTimer;
        private bool _wasActive;

        // Learn & Grow tutorial support
        [Header("Tutorial Support")]
        [SerializeField] private bool _isTutorialElement = false;
        [SerializeField] private string _tutorialDescription = "";
        [SerializeField] private bool _highlightInTutorial = false;

        #region Unity Lifecycle

        private void Awake()
        {
            _currentIntensity = _intensity;
            _wasActive = _isActive;
            
            InitializeComponents();
        }

        private void Start()
        {
            UpdateElementState();
        }

        private void Update()
        {
            UpdateDynamicBehavior();
            UpdateVisualEffects();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
                
            if (_particleEffect == null)
                _particleEffect = GetComponentInChildren<ParticleSystem>();
                
            if (_lightComponent == null)
                _lightComponent = GetComponent<Light>();
                
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            // Configure based on element type
            ConfigureElementType();
        }

        private void ConfigureElementType()
        {
            switch (_elementType)
            {
                case EnvironmentalElementType.Light:
                    ConfigureLightElement();
                    break;
                    
                case EnvironmentalElementType.Shadow:
                    ConfigureShadowElement();
                    break;
                    
                case EnvironmentalElementType.Foliage:
                    ConfigureFoliageElement();
                    break;
                    
                case EnvironmentalElementType.Noise:
                    ConfigureNoiseElement();
                    break;
            }
        }

        private void ConfigureLightElement()
        {
            if (_lightComponent != null)
            {
                _lightComponent.intensity = _currentIntensity * 2f; // Scale for Unity light
                _lightComponent.range = _influenceRadius;
            }
        }

        private void ConfigureShadowElement()
        {
            if (_renderer != null)
            {
                var material = _renderer.material;
                var color = material.color;
                color.a = _currentIntensity;
                material.color = color;
            }
        }

        private void ConfigureFoliageElement()
        {
            if (_renderer != null)
            {
                var material = _renderer.material;
                var color = Color.Lerp(new Color(0.6f, 0.4f, 0.2f), Color.green, _currentIntensity);
                material.color = color;
            }
        }

        private void ConfigureNoiseElement()
        {
            if (_audioSource != null)
            {
                _audioSource.volume = _currentIntensity;
                _audioSource.maxDistance = _influenceRadius;
            }
        }

        #endregion

        #region Dynamic Behavior

        private void UpdateDynamicBehavior()
        {
            if (_toggleInterval > 0f)
            {
                _toggleTimer += Time.deltaTime;
                if (_toggleTimer >= _toggleInterval)
                {
                    ToggleElement();
                    _toggleTimer = 0f;
                }
            }

            // Update intensity based on curve if active
            if (_isActive && _intensityCurve != null)
            {
                var normalizedTime = (_toggleTimer / _toggleInterval);
                var curveValue = _intensityCurve.Evaluate(normalizedTime);
                SetIntensity(_intensity * curveValue);
            }
        }

        private void UpdateVisualEffects()
        {
            // Update particle effects
            if (_particleEffect != null)
            {
                var emission = _particleEffect.emission;
                emission.enabled = _isActive;
                
                if (_isActive)
                {
                    var main = _particleEffect.main;
                    main.startLifetime = _currentIntensity * 2f;
                }
            }

            // Update material properties
            if (_renderer != null && _isActive)
            {
                UpdateMaterialProperties();
            }
        }

        private void UpdateMaterialProperties()
        {
            var material = _renderer.material;
            
            switch (_elementType)
            {
                case EnvironmentalElementType.Light:
                    if (material.HasProperty("_EmissionColor"))
                    {
                        var emissionColor = Color.white * _currentIntensity;
                        material.SetColor("_EmissionColor", emissionColor);
                    }
                    break;
                    
                case EnvironmentalElementType.Shadow:
                    var shadowColor = material.color;
                    shadowColor.a = _currentIntensity * 0.5f;
                    material.color = shadowColor;
                    break;
                    
                case EnvironmentalElementType.Foliage:
                    var foliageColor = Color.Lerp(new Color(0.6f, 0.4f, 0.2f), Color.green, _currentIntensity);
                    material.color = foliageColor;
                    break;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Activate or deactivate the environmental element
        /// </summary>
        public void SetActive(bool active)
        {
            if (_isActive == active) return;
            
            _isActive = active;
            
            if (_isActive)
            {
                OnElementActivated?.Invoke(this);
                PlayActivationSound();
            }
            else
            {
                OnElementDeactivated?.Invoke(this);
                PlayDeactivationSound();
            }
            
            UpdateElementState();
        }

        /// <summary>
        /// Toggle the element's active state
        /// </summary>
        public void ToggleElement()
        {
            if (_canToggle)
            {
                SetActive(!_isActive);
            }
        }

        /// <summary>
        /// Set the intensity of the element
        /// </summary>
        public void SetIntensity(float intensity)
        {
            var newIntensity = Mathf.Clamp01(intensity);
            if (Mathf.Approximately(_currentIntensity, newIntensity)) return;
            
            _currentIntensity = newIntensity;
            OnIntensityChanged?.Invoke(this);
            
            UpdateElementState();
        }

        /// <summary>
        /// Get the influence of this element at a specific position
        /// </summary>
        public float GetInfluenceAt(Vector3 position)
        {
            if (!_isActive) return 0f;
            
            var distance = Vector3.Distance(transform.position, position);
            if (distance >= _influenceRadius) return 0f;
            
            // Linear falloff with intensity modulation
            var influence = 1f - (distance / _influenceRadius);
            return influence * _currentIntensity;
        }

        /// <summary>
        /// Get the environmental effect value at a position (concealment, exposure, etc.)
        /// </summary>
        public float GetEffectValueAt(Vector3 position)
        {
            var influence = GetInfluenceAt(position);
            
            return _elementType switch
            {
                EnvironmentalElementType.Shadow => influence * 0.8f, // High concealment
                EnvironmentalElementType.Foliage => influence * 0.6f, // Medium concealment
                EnvironmentalElementType.Light => influence * -0.9f, // High exposure (negative)
                EnvironmentalElementType.Noise => influence * 0.4f, // Noise masking
                _ => 0f
            };
        }

        #endregion

        #region Audio System

        private void PlayActivationSound()
        {
            if (_audioSource != null && _activationSound != null)
            {
                _audioSource.PlayOneShot(_activationSound);
            }
        }

        private void PlayDeactivationSound()
        {
            if (_audioSource != null && _deactivationSound != null)
            {
                _audioSource.PlayOneShot(_deactivationSound);
            }
        }

        #endregion

        #region State Management

        private void UpdateElementState()
        {
            // Update component-specific configurations
            ConfigureElementType();
            
            // Handle state change detection
            if (_wasActive != _isActive)
            {
                _wasActive = _isActive;
                UpdateVisualEffects();
            }
        }

        #endregion

        #region Tutorial Support

        /// <summary>
        /// Configure this element for tutorial use
        /// </summary>
        public void SetupForTutorial(string description, bool highlight = true)
        {
            _isTutorialElement = true;
            _tutorialDescription = description;
            _highlightInTutorial = highlight;
        }

        /// <summary>
        /// Get tutorial information for this element
        /// </summary>
        public string GetTutorialInfo()
        {
            if (!_isTutorialElement) return string.Empty;
            
            var info = $"{_elementType} Element";
            if (!string.IsNullOrEmpty(_tutorialDescription))
            {
                info += $": {_tutorialDescription}";
            }
            
            return info;
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Draw influence radius
            Gizmos.color = _isActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _influenceRadius);

            // Element type-specific visualization
            switch (_elementType)
            {
                case EnvironmentalElementType.Light:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(transform.position, Vector3.up * 2f);
                    break;
                    
                case EnvironmentalElementType.Shadow:
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
                    break;
                    
                case EnvironmentalElementType.Foliage:
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, Vector3.one);
                    break;
                    
                case EnvironmentalElementType.Noise:
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, _influenceRadius * 0.3f);
                    break;
            }

            // Show intensity
            Gizmos.color = new Color(1f, 1f, 1f, _currentIntensity);
            Gizmos.DrawSphere(transform.position + Vector3.up * 1.5f, 0.2f);
        }

        private void OnDrawGizmos()
        {
            // Tutorial highlighting
            if (_isTutorialElement && _highlightInTutorial)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.3f);
            }
        }

        #endregion
    }

    /// <summary>
    /// Types of environmental elements that affect stealth gameplay
    /// </summary>
    public enum EnvironmentalElementType
    {
        Shadow,     // Provides concealment
        Light,      // Increases visibility/exposure
        Foliage,    // Provides partial concealment
        Noise       // Masks player sounds
    }
}