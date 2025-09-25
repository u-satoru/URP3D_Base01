using System;
using UnityEngine;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// Component representing a hiding spot for stealth gameplay
    /// Provides concealment for the player from AI detection
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HidingSpot : MonoBehaviour, IHidingSpot
    {
        [Header("Concealment Properties")]
        [SerializeField, Range(0f, 1f)] private float _concealmentLevel = 0.8f;
        [SerializeField] private int _capacity = 1;
        [SerializeField] private float _influenceRadius = 2f;
        [SerializeField] private string _spotName = "Hiding Spot";

        [Header("Interaction Settings")]
        [SerializeField] private bool _requiresInteraction = false;
        [SerializeField] private KeyCode _interactionKey = KeyCode.E;
        [SerializeField] private float _interactionDistance = 2f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject _visualIndicator;
        [SerializeField] private Color _availableColor = Color.green;
        [SerializeField] private Color _occupiedColor = Color.red;

        // Properties
        public float ConcealmentLevel => _concealmentLevel;
        public int Capacity => _capacity;
        public int CurrentOccupants => _currentOccupants;
        public bool IsAvailable => _currentOccupants < _capacity;
        public float InfluenceRadius => _influenceRadius;
        public bool RequiresInteraction => _requiresInteraction;
        
        // IHidingSpot インターフェース実装
        public string SpotName => _spotName;
        public Transform HidingTransform => transform;

        // Events
        public event Action<HidingSpot> OnPlayerEntered;
        public event Action<HidingSpot> OnPlayerExited;
        public event Action<HidingSpot> OnOccupancyChanged;

        // State
        private int _currentOccupants = 0;
        private bool _playerIsInside = false;
        private Collider _triggerCollider;
        private Renderer _renderer;

        // Learn & Grow tutorial support
        [Header("Tutorial Support")]
        [SerializeField] private bool _isTutorialSpot = false;
        [SerializeField] private string _tutorialMessage = "";

        #region Unity Lifecycle

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider>();
            _triggerCollider.isTrigger = true;
            
            _renderer = GetComponent<Renderer>();
            
            UpdateVisualIndicator();
        }

        private void Update()
        {
            if (_requiresInteraction && _playerIsInside)
            {
                HandleInteractionInput();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Initialize the hiding spot with specific parameters
        /// </summary>
        public void Initialize(float concealmentLevel, int capacity)
        {
            _concealmentLevel = Mathf.Clamp01(concealmentLevel);
            _capacity = Mathf.Max(1, capacity);
            
            UpdateVisualIndicator();
        }

        /// <summary>
        /// Manually enter the hiding spot (for command pattern)
        /// </summary>
        public bool TryEnter(Transform occupant)
        {
            if (!IsAvailable) return false;

            _currentOccupants++;
            
            if (occupant.CompareTag("Player"))
            {
                _playerIsInside = true;
                OnPlayerEntered?.Invoke(this);
                
                // Show tutorial message if this is a tutorial spot
                if (_isTutorialSpot && !string.IsNullOrEmpty(_tutorialMessage))
                {
                    ShowTutorialMessage(_tutorialMessage);
                }
            }

            OnOccupancyChanged?.Invoke(this);
            UpdateVisualIndicator();
            
            return true;
        }

        /// <summary>
        /// Manually exit the hiding spot (for command pattern)
        /// </summary>
        public bool TryExit(Transform occupant)
        {
            if (_currentOccupants <= 0) return false;

            _currentOccupants--;
            
            if (occupant.CompareTag("Player") && _playerIsInside)
            {
                _playerIsInside = false;
                OnPlayerExited?.Invoke(this);
            }

            OnOccupancyChanged?.Invoke(this);
            UpdateVisualIndicator();
            
            return true;
        }

        /// <summary>
        /// Get the effective concealment at a specific position within this hiding spot
        /// </summary>
        public float GetConcealmentAt(Vector3 position)
        {
            var distance = Vector3.Distance(transform.position, position);
            if (distance >= _influenceRadius) return 0f;

            // Linear falloff based on distance
            var influence = 1f - (distance / _influenceRadius);
            return _concealmentLevel * influence;
        }

        #endregion

        #region Collision Detection

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                HandlePlayerApproach(other.transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                HandlePlayerLeave(other.transform);
            }
        }

        private void HandlePlayerApproach(Transform player)
        {
            if (!_requiresInteraction)
            {
                // Automatically enter if no interaction required
                TryEnter(player);
            }
            else
            {
                // Just mark player as nearby for interaction
                _playerIsInside = true;
                ShowInteractionPrompt(true);
            }
        }

        private void HandlePlayerLeave(Transform player)
        {
            if (_playerIsInside)
            {
                if (_requiresInteraction)
                {
                    ShowInteractionPrompt(false);
                }
                
                TryExit(player);
            }
        }

        #endregion

        #region Interaction System

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(_interactionKey))
            {
                if (_playerIsInside && _currentOccupants == 0)
                {
                    // Enter hiding spot
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        TryEnter(player.transform);
                    }
                }
                else if (_playerIsInside && _currentOccupants > 0)
                {
                    // Exit hiding spot
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        TryExit(player.transform);
                    }
                }
            }
        }

        private void ShowInteractionPrompt(bool show)
        {
            // TODO: Integration with UI system
            if (show)
            {
                Debug.Log($"Press {_interactionKey} to {(_currentOccupants > 0 ? "exit" : "enter")} hiding spot");
            }
        }

        #endregion

        #region Visual Feedback

        private void UpdateVisualIndicator()
        {
            if (_visualIndicator != null)
            {
                _visualIndicator.SetActive(_playerIsInside || _currentOccupants > 0);
            }

            if (_renderer != null)
            {
                var material = _renderer.material;
                material.color = IsAvailable ? _availableColor : _occupiedColor;
            }
        }

        #endregion

        #region Tutorial Support

        private void ShowTutorialMessage(string message)
        {
            // TODO: Integration with tutorial UI system
            Debug.Log($"[Tutorial] {message}");
        }

        /// <summary>
        /// Configure this hiding spot for tutorial use
        /// </summary>
        public void SetupForTutorial(string message, float concealmentLevel = 0.9f)
        {
            _isTutorialSpot = true;
            _tutorialMessage = message;
            _concealmentLevel = concealmentLevel;
            
            UpdateVisualIndicator();
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Draw influence radius
            Gizmos.color = IsAvailable ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _influenceRadius);

            // Draw interaction distance if applicable
            if (_requiresInteraction)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _interactionDistance);
            }

            // Draw concealment level visualization
            Gizmos.color = new Color(0, 1, 0, _concealmentLevel);
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);

            // Show capacity
            if (_capacity > 1)
            {
                for (int i = 0; i < _capacity; i++)
                {
                    Vector3 position = transform.position + Vector3.right * (i * 0.3f - (_capacity - 1) * 0.15f);
                    Gizmos.color = i < _currentOccupants ? Color.red : Color.white;
                    Gizmos.DrawWireCube(position + Vector3.up * 0.8f, Vector3.one * 0.2f);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (_isTutorialSpot)
            {
                // Highlight tutorial spots
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.3f);
            }
        }

        #endregion
    }
}
