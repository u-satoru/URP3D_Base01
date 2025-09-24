using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Interaction
{
    /// <summary>
    /// Core interaction system that detects and manages interactions with IInteractable objects.
    /// Integrates with Event-Driven Architecture and Command Pattern.
    /// Provides sphere-based detection with priority-based targeting.
    /// </summary>
    public class Interactor : MonoBehaviour, IInteractor
    {
        #region Serialized Fields

        [Header("Interaction Settings")]
        [SerializeField] private float maxInteractionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask = -1;
        [SerializeField] private int maxInteractablesDetected = 10;
        [SerializeField] private float detectionUpdateRate = 10f; // Hz

        [Header("Detection Origin")]
        [SerializeField] private Transform interactionOrigin;

        [Header("Event Channels")]
        [SerializeField] private GameEvent<IInteractable> onTargetChanged;
        [SerializeField] private GameEvent<IInteractable> onInteractionPerformed;
        [SerializeField] private GameEvent<IInteractable> onInteractionFailed;

        [Header("Debug")]
        [SerializeField] private bool enableDebugDrawing = true;
        [SerializeField] private Color detectionRangeColor = Color.blue;
        [SerializeField] private Color targetHighlightColor = Color.green;

        #endregion

        #region Private Fields

        private IInteractable _currentTarget;
        private readonly List<IInteractable> _interactablesInRange = new();
        private readonly Collider[] _detectionBuffer = new Collider[20];

        private float _lastDetectionUpdate;
        private bool _canPerformInteractions = true;

        // Command integration
        private ICommandPoolService _commandPool;

        #endregion

        #region IInteractor Properties

        /// <summary>
        /// Transform for interaction detection origin
        /// </summary>
        public Transform InteractionOrigin => interactionOrigin != null ? interactionOrigin : transform;

        /// <summary>
        /// Maximum interaction range
        /// </summary>
        public float MaxInteractionRange => maxInteractionRange;

        /// <summary>
        /// Current primary interaction target
        /// </summary>
        public IInteractable CurrentTarget => _currentTarget;

        /// <summary>
        /// Whether this interactor can currently perform interactions
        /// </summary>
        public bool CanPerformInteractions => _canPerformInteractions;

        /// <summary>
        /// Layer mask for interaction detection
        /// </summary>
        public LayerMask InteractionLayerMask => interactionLayerMask;

        #endregion

        #region IInteractor Events

        /// <summary>
        /// Event fired when interaction target changes
        /// </summary>
        public event System.Action<IInteractor, IInteractable, IInteractable> OnTargetChanged;

        /// <summary>
        /// Event fired when an interaction is performed
        /// </summary>
        public event System.Action<IInteractor, IInteractable, bool> OnInteractionPerformed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeCommandPool();
        }

        private void Update()
        {
            if (ShouldUpdateDetection())
            {
                UpdateInteractionDetection();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            // Use this transform if no specific interaction origin is set
            if (interactionOrigin == null)
            {
                interactionOrigin = transform;
            }
        }

        /// <summary>
        /// Initialize command pool for interaction commands
        /// </summary>
        private void InitializeCommandPool()
        {
            _commandPool = ServiceLocator.GetService<ICommandPoolService>();
        }

        #endregion

        #region IInteractor Implementation

        /// <summary>
        /// Attempt to interact with the current target
        /// </summary>
        /// <returns>True if interaction was successful</returns>
        public bool TryInteract()
        {
            if (_currentTarget == null || !_canPerformInteractions)
            {
                return false;
            }

            return TryInteractWith(_currentTarget);
        }

        /// <summary>
        /// Attempt to interact with a specific interactable
        /// </summary>
        /// <param name="interactable">The target to interact with</param>
        /// <returns>True if interaction was successful</returns>
        public bool TryInteractWith(IInteractable interactable)
        {
            if (interactable == null || !_canPerformInteractions)
            {
                return false;
            }

            // Validate interaction
            if (!interactable.CanInteract || !interactable.CanInteractWith(this))
            {
                FireInteractionFailed(interactable);
                return false;
            }

            // Check range
            float distance = Vector3.Distance(InteractionOrigin.position, interactable.InteractionTransform.position);
            if (distance > interactable.InteractionRange || distance > maxInteractionRange)
            {
                FireInteractionFailed(interactable);
                return false;
            }

            // Perform interaction
            bool success = false;
            try
            {
                success = interactable.OnInteract(this);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Interactor: Error during interaction with {interactable}: {e.Message}");
                success = false;
            }

            // Fire events
            if (success)
            {
                FireInteractionPerformed(interactable);
            }
            else
            {
                FireInteractionFailed(interactable);
            }

            return success;
        }

        /// <summary>
        /// Update interaction detection (typically called in Update)
        /// </summary>
        public void UpdateInteractionDetection()
        {
            _lastDetectionUpdate = Time.time;

            // Clear previous detections
            var previousInteractables = new List<IInteractable>(_interactablesInRange);
            _interactablesInRange.Clear();

            // Perform sphere detection
            Vector3 origin = InteractionOrigin.position;
            int hitCount = Physics.OverlapSphereNonAlloc(origin, maxInteractionRange, _detectionBuffer, interactionLayerMask);

            // Process detected colliders
            for (int i = 0; i < hitCount && _interactablesInRange.Count < maxInteractablesDetected; i++)
            {
                var collider = _detectionBuffer[i];
                if (collider == null) continue;

                var interactable = collider.GetComponent<IInteractable>();
                if (interactable != null && !_interactablesInRange.Contains(interactable))
                {
                    // Check if within specific interactable's range
                    float distance = Vector3.Distance(origin, interactable.InteractionTransform.position);
                    if (distance <= interactable.InteractionRange)
                    {
                        _interactablesInRange.Add(interactable);
                    }
                }
            }

            // Handle range enter/exit events
            ProcessRangeEvents(previousInteractables);

            // Update current target
            UpdateCurrentTarget();
        }

        /// <summary>
        /// Get all interactables currently in range
        /// </summary>
        /// <returns>List of interactables in range</returns>
        public List<IInteractable> GetInteractablesInRange()
        {
            return new List<IInteractable>(_interactablesInRange);
        }

        #endregion

        #region Target Management

        /// <summary>
        /// Update the current primary interaction target based on priority and distance
        /// </summary>
        private void UpdateCurrentTarget()
        {
            IInteractable newTarget = null;

            if (_interactablesInRange.Count > 0)
            {
                // Filter to only interactables that can currently be interacted with
                var validInteractables = _interactablesInRange
                    .Where(i => i.CanInteract && i.CanInteractWith(this))
                    .ToList();

                if (validInteractables.Count > 0)
                {
                    // Sort by priority (descending) then by distance (ascending)
                    Vector3 origin = InteractionOrigin.position;
                    newTarget = validInteractables
                        .OrderByDescending(i => i.InteractionPriority)
                        .ThenBy(i => Vector3.Distance(origin, i.InteractionTransform.position))
                        .First();
                }
            }

            // Update target if changed
            if (newTarget != _currentTarget)
            {
                var previousTarget = _currentTarget;

                // Unfocus previous target
                if (previousTarget != null)
                {
                    previousTarget.OnInteractionUnfocused(this);
                }

                // Set new target
                _currentTarget = newTarget;

                // Focus new target
                if (_currentTarget != null)
                {
                    _currentTarget.OnInteractionFocused(this);
                }

                // Fire events
                FireTargetChanged(previousTarget, _currentTarget);
            }
        }

        #endregion

        #region Range Event Processing

        /// <summary>
        /// Process range enter/exit events for interactables
        /// </summary>
        /// <param name="previousInteractables">Previous frame's interactables</param>
        private void ProcessRangeEvents(List<IInteractable> previousInteractables)
        {
            // Handle range entered
            foreach (var interactable in _interactablesInRange)
            {
                if (!previousInteractables.Contains(interactable))
                {
                    interactable.OnInteractionRangeEntered(this);
                }
            }

            // Handle range exited
            foreach (var interactable in previousInteractables)
            {
                if (!_interactablesInRange.Contains(interactable))
                {
                    interactable.OnInteractionRangeExited(this);
                }
            }
        }

        #endregion

        #region Event Firing

        /// <summary>
        /// Fire target changed event
        /// </summary>
        /// <param name="previousTarget">Previous target</param>
        /// <param name="newTarget">New target</param>
        private void FireTargetChanged(IInteractable previousTarget, IInteractable newTarget)
        {
            OnTargetChanged?.Invoke(this, previousTarget, newTarget);
            onTargetChanged?.Raise(newTarget);
        }

        /// <summary>
        /// Fire interaction performed event
        /// </summary>
        /// <param name="interactable">Interacted object</param>
        private void FireInteractionPerformed(IInteractable interactable)
        {
            OnInteractionPerformed?.Invoke(this, interactable, true);
            onInteractionPerformed?.Raise(interactable);
        }

        /// <summary>
        /// Fire interaction failed event
        /// </summary>
        /// <param name="interactable">Failed interaction target</param>
        private void FireInteractionFailed(IInteractable interactable)
        {
            OnInteractionPerformed?.Invoke(this, interactable, false);
            onInteractionFailed?.Raise(interactable);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Check if detection should be updated this frame
        /// </summary>
        /// <returns>True if detection should update</returns>
        private bool ShouldUpdateDetection()
        {
            return Time.time - _lastDetectionUpdate >= (1f / detectionUpdateRate);
        }

        /// <summary>
        /// Enable or disable interaction capabilities
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetInteractionEnabled(bool enabled)
        {
            _canPerformInteractions = enabled;

            // Clear target if interactions are disabled
            if (!enabled && _currentTarget != null)
            {
                var previousTarget = _currentTarget;
                _currentTarget = null;
                FireTargetChanged(previousTarget, null);
            }
        }

        /// <summary>
        /// Force an immediate detection update
        /// </summary>
        public void ForceDetectionUpdate()
        {
            UpdateInteractionDetection();
        }

        /// <summary>
        /// Get interaction status for debugging
        /// </summary>
        /// <returns>Debug information string</returns>
        public string GetInteractionDebugInfo()
        {
            return $"Target: {(_currentTarget != null ? "Yes" : "None")}, " +
                   $"In Range: {_interactablesInRange.Count}, " +
                   $"Can Interact: {_canPerformInteractions}";
        }

        #endregion

        #region Debug Drawing

        private void OnDrawGizmosSelected()
        {
            if (!enableDebugDrawing) return;

            Vector3 origin = InteractionOrigin != null ? InteractionOrigin.position : transform.position;

            // Draw detection range
            Gizmos.color = detectionRangeColor;
            Gizmos.DrawWireSphere(origin, maxInteractionRange);

            // Draw current target
            if (_currentTarget != null)
            {
                Gizmos.color = targetHighlightColor;
                Vector3 targetPos = _currentTarget.InteractionTransform.position;
                Gizmos.DrawLine(origin, targetPos);
                Gizmos.DrawWireSphere(targetPos, _currentTarget.InteractionRange);
            }

            // Draw all interactables in range
            Gizmos.color = Color.yellow;
            foreach (var interactable in _interactablesInRange)
            {
                if (interactable != _currentTarget)
                {
                    Gizmos.DrawWireCube(interactable.InteractionTransform.position, Vector3.one * 0.2f);
                }
            }
        }

        #endregion
    }
}