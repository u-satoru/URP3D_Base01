using UnityEngine;
using UnityEngine.UI;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using asterivo.Unity60.Features.Templates.Adventure.Events;
using asterivo.Unity60.Features.Templates.Adventure.Player;

namespace asterivo.Unity60.Features.Templates.Adventure.Interaction
{
    /// <summary>
    /// Adventure template interaction system
    /// Handles player interaction with objects in the adventure template
    /// </summary>
    public class AdventureInteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("UI References")]
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private Text interactionText;
        [SerializeField] private Image interactionIcon;

        [Header("Interaction Effects")]
        [SerializeField] private GameObject interactionEffectPrefab;
        [SerializeField] private AudioClip interactionSound;
        [SerializeField] private AudioClip cantInteractSound;

        // Current interaction state
        private BaseInteractable currentInteractable;
        private bool isInteracting = false;
        private AdventurePlayerController playerController;
        private AdventureTemplateConfiguration templateConfig;
        private InteractionManager interactionManager;

        // Components
        private AudioSource audioSource;
        private Camera playerCamera;

        // Events
        [Header("Events")]
        [SerializeField] private GameEvent<InteractionStartedEventData> onInteractionStarted;
        [SerializeField] private GameEvent<InteractionEndedEventData> onInteractionEnded;
        [SerializeField] private GameEvent<InteractionAttemptEventData> onInteractionAttempt;

        public BaseInteractable CurrentInteractable => currentInteractable;
        public bool IsInteracting => isInteracting;
        public float InteractionRange => interactionRange;

        private void Awake()
        {
            playerController = GetComponent<AdventurePlayerController>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindFirstObjectByType<Camera>();
        }

        private void Start()
        {
            InitializeServices();
            SetupUI();
        }

        private void Update()
        {
            if (!isInteracting)
            {
                CheckForInteractables();
                HandleInteractionInput();
            }

            UpdateUI();
        }

        public void Initialize(AdventureTemplateConfiguration config)
        {
            templateConfig = config;
            Debug.Log("[AdventureInteractionSystem] Adventure interaction system initialized");
        }

        #region Interaction Detection

        private void CheckForInteractables()
        {
            BaseInteractable nearestInteractable = FindNearestInteractable();
            
            if (nearestInteractable != currentInteractable)
            {
                // Exit previous interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionExit(playerController.gameObject);
                }

                // Enter new interactable
                currentInteractable = nearestInteractable;

                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionEnter(playerController.gameObject);
                }
            }
        }

        private BaseInteractable FindNearestInteractable()
        {
            Vector3 playerPosition = transform.position;
            Collider[] nearbyColliders = Physics.OverlapSphere(playerPosition, interactionRange, interactableLayerMask);
            
            BaseInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var collider in nearbyColliders)
            {
                var interactable = collider.GetComponent<BaseInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    // Check if interactable is in front of player
                    Vector3 directionToInteractable = (collider.transform.position - playerPosition).normalized;
                    Vector3 playerForward = transform.forward;
                    
                    float dot = Vector3.Dot(playerForward, directionToInteractable);
                    if (dot > 0.3f) // Only consider interactables in front of player
                    {
                        float distance = Vector3.Distance(playerPosition, collider.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearest = interactable;
                        }
                    }
                }
            }

            return nearest;
        }

        #endregion

        #region Interaction Input

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            if (currentInteractable == null)
            {
                PlayCantInteractFeedback();
                return;
            }

            if (!currentInteractable.CanInteract())
            {
                PlayCantInteractFeedback();
                return;
            }

            StartInteraction();
        }

        public void StartInteraction()
        {
            if (currentInteractable == null || isInteracting) return;

            isInteracting = true;
            
            // Raise interaction started event
            onInteractionStarted?.Raise(new InteractionStartedEventData
            {
                interactable = currentInteractable,
                player = playerController,
                interactionType = currentInteractable.InteractionType,
                position = currentInteractable.transform.position
            });

            // Play interaction effects
            PlayInteractionEffects();

            // Perform the interaction
            currentInteractable.Interact();

            // For most interactions, end immediately. Some might require manual ending.
            if (currentInteractable.InteractionType != InteractionType.Dialogue &&
                currentInteractable.InteractionType != InteractionType.Container)
            {
                EndInteraction();
            }

            Debug.Log($"[AdventureInteractionSystem] Started interaction with {currentInteractable.name}");
        }

        public void EndInteraction()
        {
            if (!isInteracting) return;

            var previousInteractable = currentInteractable;
            isInteracting = false;

            // Raise interaction ended event
            onInteractionEnded?.Raise(new InteractionEndedEventData
            {
                interactable = previousInteractable,
                player = playerController,
                interactionType = previousInteractable?.InteractionType ?? InteractionType.Generic,
                duration = Time.time // Could track actual duration if needed
            });

            Debug.Log($"[AdventureInteractionSystem] Ended interaction with {previousInteractable?.name ?? "unknown"}");
        }

        #endregion

        #region UI Management

        private void SetupUI()
        {
            if (interactionUI == null)
            {
                CreateInteractionUI();
            }

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            bool shouldShowUI = currentInteractable != null && !isInteracting;
            
            if (interactionUI != null)
            {
                interactionUI.SetActive(shouldShowUI);
                
                if (shouldShowUI && interactionText != null)
                {
                    string promptText = string.IsNullOrEmpty(currentInteractable.InteractionPrompt) 
                        ? interactionPrompt 
                        : currentInteractable.InteractionPrompt;
                    
                    interactionText.text = promptText;
                }

                if (shouldShowUI && interactionIcon != null && currentInteractable.InteractionIcon != null)
                {
                    interactionIcon.sprite = currentInteractable.InteractionIcon;
                    interactionIcon.gameObject.SetActive(true);
                }
                else if (interactionIcon != null)
                {
                    interactionIcon.gameObject.SetActive(false);
                }
            }
        }

        private void CreateInteractionUI()
        {
            // Find Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Create interaction UI
            GameObject uiObject = new GameObject("InteractionUI");
            uiObject.transform.SetParent(canvas.transform, false);

            // Add RectTransform
            RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(200, 50);

            // Add background panel
            Image background = uiObject.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f);

            // Add text
            GameObject textObject = new GameObject("InteractionText");
            textObject.transform.SetParent(uiObject.transform, false);
            
            Text text = textObject.AddComponent<Text>();
            text.text = interactionPrompt;
            text.color = Color.white;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            interactionUI = uiObject;
            interactionText = text;

            Debug.Log("[AdventureInteractionSystem] Created interaction UI");
        }

        #endregion

        #region Effects and Feedback

        private void PlayInteractionEffects()
        {
            // Play sound effect
            if (audioSource != null && interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }

            // Spawn visual effect
            if (interactionEffectPrefab != null && currentInteractable != null)
            {
                Vector3 effectPosition = currentInteractable.transform.position;
                GameObject effect = Instantiate(interactionEffectPrefab, effectPosition, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        private void PlayCantInteractFeedback()
        {
            // Play negative feedback sound
            if (audioSource != null && cantInteractSound != null)
            {
                audioSource.PlayOneShot(cantInteractSound);
            }

            // Raise interaction attempt event
            onInteractionAttempt?.Raise(new InteractionAttemptEventData
            {
                interactable = currentInteractable,
                player = playerController,
                wasSuccessful = false,
                failureReason = currentInteractable != null ? "Cannot interact" : "No interactable"
            });

            Debug.Log("[AdventureInteractionSystem] Cannot interact - no valid interactable or conditions not met");
        }

        #endregion

        #region Service Integration

        private void InitializeServices()
        {
            // Find interaction manager
            interactionManager = FindObjectOfType<InteractionManager>();

            // Register with ServiceLocator if available
            try
            {
                asterivo.Unity60.Core.ServiceLocator.RegisterService<AdventureInteractionSystem>(this);
            }
            catch
            {
                // ServiceLocator not available or already registered
            }
        }

        #endregion

        #region Public Interface

        public void SetInteractionRange(float range)
        {
            interactionRange = range;
        }

        public void SetInteractionKey(KeyCode key)
        {
            interactionKey = key;
        }

        public void EnableInteractions()
        {
            enabled = true;
        }

        public void DisableInteractions()
        {
            enabled = false;
            
            if (currentInteractable != null)
            {
                currentInteractable.OnInteractionExit(playerController.gameObject);
                currentInteractable = null;
            }

            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }

        public bool HasInteractableInRange()
        {
            return currentInteractable != null;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Draw line to current interactable
            if (currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentInteractable.transform.position);
            }
        }
    }

    // Event data structures
    [System.Serializable]
    public class InteractionStartedEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public InteractionType interactionType;
        public Vector3 position;
    }

    [System.Serializable]
    public class InteractionEndedEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public InteractionType interactionType;
        public float duration;
    }

    [System.Serializable]
    public class InteractionAttemptEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public bool wasSuccessful;
        public string failureReason;
    }
}