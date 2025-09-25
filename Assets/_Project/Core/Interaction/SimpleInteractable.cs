using UnityEngine;
using UnityEngine.Events;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Core.Interaction
{
    /// <summary>
    /// Simple concrete implementation of BaseInteractable.
    /// Provides basic interaction functionality for common use cases.
    /// Can be used directly or as a reference for custom implementations.
    /// </summary>
    public class SimpleInteractable : BaseInteractable
    {
        #region Serialized Fields

        [Header("Simple Interaction Settings")]
        [SerializeField] private bool consumeOnInteract = false;
        [SerializeField] private float cooldownTime = 0f;
        [SerializeField] private int maxInteractions = -1; // -1 = unlimited

        [Header("Unity Events")]
        [SerializeField] private UnityEvent onInteractionPerformed;
        [SerializeField] private UnityEvent onInteractionFailed;
        [SerializeField] private UnityEvent onInteractionConsumed;
        [SerializeField] private UnityEvent<IInteractor> onInteractorEntered;
        [SerializeField] private UnityEvent<IInteractor> onInteractorExited;

        [Header("Audio")]
        [SerializeField] private AudioClip interactionSound;
        [SerializeField] private AudioClip failureSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private ParticleSystem interactionEffect;

        #endregion

        #region Private Fields

        private float _lastInteractionTime;
        private int _interactionCount;
        private bool _isConsumed;

        #endregion

        #region Properties

        /// <summary>
        /// Whether this interactable has been consumed (single-use)
        /// </summary>
        public bool IsConsumed => _isConsumed;

        /// <summary>
        /// Number of times this interactable has been used
        /// </summary>
        public int InteractionCount => _interactionCount;

        /// <summary>
        /// Whether this interactable is currently on cooldown
        /// </summary>
        public bool IsOnCooldown => cooldownTime > 0f && (Time.time - _lastInteractionTime) < cooldownTime;

        /// <summary>
        /// Remaining cooldown time in seconds
        /// </summary>
        public float RemainingCooldown => Mathf.Max(0f, cooldownTime - (Time.time - _lastInteractionTime));

        /// <summary>
        /// Whether this interactable has reached its maximum interaction limit
        /// </summary>
        public bool HasReachedLimit => maxInteractions > 0 && _interactionCount >= maxInteractions;

        #endregion

        #region BaseInteractable Overrides

        /// <summary>
        /// Check if interaction is allowed considering cooldown, consumption, and limits
        /// </summary>
        public override bool CanInteract =>
            base.CanInteract &&
            !_isConsumed &&
            !IsOnCooldown &&
            !HasReachedLimit;

        /// <summary>
        /// Perform the actual interaction
        /// </summary>
        /// <param name="interactor">The interactor performing the interaction</param>
        /// <returns>True if interaction was successful</returns>
        protected override bool PerformInteraction(IInteractor interactor)
        {
            // Update interaction tracking
            _lastInteractionTime = Time.time;
            _interactionCount++;

            // Play sound effect
            PlayInteractionSound();

            // Show visual effect
            ShowInteractionEffect();

            // Fire Unity event
            onInteractionPerformed?.Invoke();

            // Check if should be consumed
            if (consumeOnInteract)
            {
                ConsumeInteraction();
            }

            // Log interaction for debugging
            Debug.Log($"SimpleInteractable: Interaction performed by {interactor} (Count: {_interactionCount})", this);

            return true;
        }

        /// <summary>
        /// Handle validation failure
        /// </summary>
        /// <param name="interactor">The interactor being denied</param>
        /// <returns>Reason for denial</returns>
        protected override string GetInteractionDenialReason(IInteractor interactor)
        {
            if (_isConsumed)
                return "This object has already been consumed";

            if (IsOnCooldown)
                return $"On cooldown (remaining: {RemainingCooldown:F1}s)";

            if (HasReachedLimit)
                return $"Maximum interactions reached ({maxInteractions})";

            return base.GetInteractionDenialReason(interactor);
        }

        /// <summary>
        /// Handle interactor entering range
        /// </summary>
        /// <param name="interactor">The interactor that entered range</param>
        protected override void OnInteractorEnteredRange(IInteractor interactor)
        {
            base.OnInteractorEnteredRange(interactor);

            // Show highlight
            if (highlightObject != null)
            {
                highlightObject.SetActive(true);
            }

            // Fire Unity event
            onInteractorEntered?.Invoke(interactor);
        }

        /// <summary>
        /// Handle interactor exiting range
        /// </summary>
        /// <param name="interactor">The interactor that exited range</param>
        protected override void OnInteractorExitedRange(IInteractor interactor)
        {
            base.OnInteractorExitedRange(interactor);

            // Hide highlight if no interactors remain
            if (GetInteractorsInRange().Count == 0 && highlightObject != null)
            {
                highlightObject.SetActive(false);
            }

            // Fire Unity event
            onInteractorExited?.Invoke(interactor);
        }

        #endregion

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            // Initialize audio source if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // Hide highlight initially
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }

        protected override void Start()
        {
            base.Start();

            // Initialize interaction tracking
            _lastInteractionTime = -cooldownTime; // Allow immediate first interaction
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset the interactable to its initial state
        /// </summary>
        [ContextMenu("Reset Interactable")]
        public virtual void ResetInteractable()
        {
            _isConsumed = false;
            _interactionCount = 0;
            _lastInteractionTime = -cooldownTime;

            // Update interaction state
            SetInteractionEnabled(true);

            Debug.Log("SimpleInteractable: Reset to initial state", this);
        }

        /// <summary>
        /// Manually consume this interactable
        /// </summary>
        [ContextMenu("Consume Interactable")]
        public virtual void ConsumeInteraction()
        {
            if (_isConsumed) return;

            _isConsumed = true;

            // Disable further interactions
            SetInteractionEnabled(false);

            // Fire Unity event
            onInteractionConsumed?.Invoke();

            Debug.Log("SimpleInteractable: Consumed", this);
        }

        /// <summary>
        /// Set the cooldown time
        /// </summary>
        /// <param name="newCooldownTime">New cooldown time in seconds</param>
        public void SetCooldownTime(float newCooldownTime)
        {
            cooldownTime = Mathf.Max(0f, newCooldownTime);
        }

        /// <summary>
        /// Set the maximum number of interactions
        /// </summary>
        /// <param name="newMaxInteractions">New maximum interactions (-1 for unlimited)</param>
        public void SetMaxInteractions(int newMaxInteractions)
        {
            maxInteractions = newMaxInteractions;
        }

        /// <summary>
        /// Set whether the interactable should be consumed on interaction
        /// </summary>
        /// <param name="shouldConsume">Whether to consume on interact</param>
        public void SetConsumeOnInteract(bool shouldConsume)
        {
            consumeOnInteract = shouldConsume;
        }

        #endregion

        #region Audio and Visual Effects

        /// <summary>
        /// Play interaction sound effect
        /// </summary>
        private void PlayInteractionSound()
        {
            if (audioSource != null && interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }
        }

        /// <summary>
        /// Play failure sound effect
        /// </summary>
        private void PlayFailureSound()
        {
            if (audioSource != null && failureSound != null)
            {
                audioSource.PlayOneShot(failureSound);
            }
        }

        /// <summary>
        /// Show interaction visual effect
        /// </summary>
        private void ShowInteractionEffect()
        {
            if (interactionEffect != null)
            {
                interactionEffect.Play();
            }
        }

        #endregion

        #region Debug Information

        /// <summary>
        /// Get extended debug information for SimpleInteractable
        /// </summary>
        /// <returns>Debug information string</returns>
        public override string GetInteractionDebugInfo()
        {
            var baseInfo = base.GetInteractionDebugInfo();
            var extendedInfo = $"Consumed: {_isConsumed}, " +
                             $"Count: {_interactionCount}" +
                             (maxInteractions > 0 ? $"/{maxInteractions}" : "") +
                             $", Cooldown: {(IsOnCooldown ? $"{RemainingCooldown:F1}s" : "Ready")}";

            return $"{baseInfo}, {extendedInfo}";
        }

        #endregion

        #region Inspector Methods

#if UNITY_EDITOR
        /// <summary>
        /// Validate configuration in the inspector
        /// </summary>
        private void OnValidate()
        {
            // Ensure positive values
            cooldownTime = Mathf.Max(0f, cooldownTime);

            // Validate max interactions
            if (maxInteractions == 0)
            {
                Debug.LogWarning("SimpleInteractable: Max interactions is 0, this interactable will never be usable. Use -1 for unlimited or a positive number.", this);
            }
        }

        /// <summary>
        /// Add context menu for testing interactions
        /// </summary>
        [ContextMenu("Test Interaction")]
        private void TestInteraction()
        {
            if (Application.isPlaying)
            {
                // Create a dummy interactor for testing
                var testInteractor = new GameObject("Test Interactor").AddComponent<Interactor>();
                testInteractor.transform.position = transform.position + Vector3.forward * (InteractionRange * 0.5f);

                bool success = OnInteract(testInteractor);
                Debug.Log($"Test interaction {(success ? "succeeded" : "failed")}", this);

                // Cleanup
                if (Application.isPlaying)
                {
                    DestroyImmediate(testInteractor.gameObject);
                }
            }
            else
            {
                Debug.Log("Test interaction can only be performed in Play mode", this);
            }
        }
#endif

        #endregion
    }
}
