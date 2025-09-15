using UnityEngine;
using UnityEngine.InputSystem;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using asterivo.Unity60.Features.Templates.Adventure.Quest;
using asterivo.Unity60.Features.Templates.Adventure.Interaction;
using asterivo.Unity60.Features.Templates.Adventure.Events;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;

namespace asterivo.Unity60.Features.Templates.Adventure.Player
{
    /// <summary>
    /// Adventure template player controller
    /// Handles adventure-specific player behavior, inventory interaction, and quest progression
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class AdventurePlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private LayerMask groundMask = 1;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayerMask = 1;
        [SerializeField] private Transform interactionPoint;

        [Header("Adventure Settings")]
        [SerializeField] private PlayerInventoryData inventoryData;
        [SerializeField] private float maxCarryWeight = 100f;
        [SerializeField] private float currentCarryWeight = 0f;

        // Components
        private CharacterController characterController;
        private Animator animator;
        private PlayerInput playerInput;
        
        // Movement
        private Vector3 velocity;
        private bool isGrounded;
        private bool isRunning;
        private Transform groundCheck;
        
        // Input
        private Vector2 movementInput;
        private bool jumpPressed;
        private bool runPressed;
        private bool interactPressed;

        // Adventure Systems
        private AdventureTemplateConfiguration templateConfig;
        private QuestManager questManager;
        private AdventureInventoryManager inventoryManager;
        private InteractionManager interactionManager;

        // Current interaction
        private BaseInteractable currentInteractable;

        // Events
        [Header("Events")]
        [SerializeField] private GameEvent<PlayerMovementEventData> onPlayerMoved;
        [SerializeField] private GameEvent<PlayerInteractionEventData> onPlayerInteracted;
        [SerializeField] private GameEvent<PlayerInventoryChangedEventData> onInventoryChanged;

        public float CurrentCarryWeight => currentCarryWeight;
        public float MaxCarryWeight => maxCarryWeight;
        public bool CanCarryMore => currentCarryWeight < maxCarryWeight;
        public BaseInteractable CurrentInteractable => currentInteractable;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            playerInput = GetComponent<PlayerInput>();
            
            if (interactionPoint == null)
                interactionPoint = transform;

            CreateGroundCheck();
        }

        private void Start()
        {
            InitializeServices();
            SetupInputCallbacks();
        }

        private void Update()
        {
            GroundCheck();
            HandleMovement();
            HandleInteraction();
            
            if (animator != null)
            {
                UpdateAnimations();
            }
        }

        public void Initialize(AdventureTemplateConfiguration config)
        {
            templateConfig = config;
            
            // Initialize inventory data if not assigned
            if (inventoryData == null)
            {
                inventoryData = ScriptableObject.CreateInstance<PlayerInventoryData>();
            }

            Debug.Log("[AdventurePlayerController] Adventure player controller initialized");
        }

        #region Movement System

        private void HandleMovement()
        {
            // Apply gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Handle jumping
            if (jumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpPressed = false;
            }

            // Calculate movement
            Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            
            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Raise movement event
            if (move.magnitude > 0.1f)
            {
                onPlayerMoved?.Raise(new PlayerMovementEventData
                {
                    position = transform.position,
                    velocity = move * currentSpeed,
                    isRunning = isRunning,
                    isGrounded = isGrounded
                });
            }
        }

        private void GroundCheck()
        {
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
            }
            else
            {
                isGrounded = characterController.isGrounded;
            }
        }

        private void CreateGroundCheck()
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            float speed = characterController.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsRunning", isRunning && speed > 0.1f);
        }

        #endregion

        #region Interaction System

        private void HandleInteraction()
        {
            // Check for nearby interactables
            CheckForInteractables();

            // Handle interaction input
            if (interactPressed)
            {
                TryInteract();
                interactPressed = false;
            }
        }

        private void CheckForInteractables()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(interactionPoint.position, interactionRange, interactableLayerMask);
            BaseInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in nearbyObjects)
            {
                var interactable = collider.GetComponent<BaseInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    float distance = Vector3.Distance(interactionPoint.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Update current interactable
            if (currentInteractable != closestInteractable)
            {
                // Exit previous interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionExit(this.gameObject);
                }

                currentInteractable = closestInteractable;

                // Enter new interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionEnter(this.gameObject);
                }
            }
        }

        private void TryInteract()
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
                
                onPlayerInteracted?.Raise(new PlayerInteractionEventData
                {
                    interactable = currentInteractable,
                    player = this,
                    interactionType = currentInteractable.InteractionType,
                    position = currentInteractable.transform.position
                });

                Debug.Log($"[AdventurePlayerController] Interacted with {currentInteractable.name}");
            }
        }

        #endregion

        #region Inventory Management

        public bool TryPickupItem(AdventureItemData item, int quantity = 1)
        {
            if (!CanPickupItem(item, quantity))
                return false;

            // Check carry weight
            float itemWeight = item.weight * quantity;
            if (currentCarryWeight + itemWeight > maxCarryWeight)
            {
                Debug.Log($"[AdventurePlayerController] Cannot carry more items. Weight limit exceeded.");
                return false;
            }

            // Add to inventory
            if (inventoryManager != null)
            {
                int addedQuantity = inventoryManager.AddItem(item, quantity);
                if (addedQuantity > 0)
                {
                    currentCarryWeight += itemWeight;
                    
                    onInventoryChanged?.Raise(new PlayerInventoryChangedEventData
                    {
                        player = this,
                        item = item,
                        quantityChanged = addedQuantity,
                        newTotalQuantity = addedQuantity,
                        wasAdded = true,
                        newCarryWeight = currentCarryWeight
                    });

                    Debug.Log($"[AdventurePlayerController] Picked up {quantity}x {item.itemName}");
                    return true;
                }
            }

            return false;
        }

        public bool TryUseItem(AdventureItemData item, int quantity = 1)
        {
            if (inventoryManager != null && inventoryManager.HasItem(item, quantity))
            {
                int removedQuantity = inventoryManager.RemoveItem(item, quantity);
                if (removedQuantity > 0)
                {
                    float itemWeight = item.weight * quantity;
                    currentCarryWeight = Mathf.Max(0, currentCarryWeight - itemWeight);
                    
                    onInventoryChanged?.Raise(new PlayerInventoryChangedEventData
                    {
                        player = this,
                        item = item,
                        quantityChanged = quantity,
                        newTotalQuantity = inventoryManager.GetItemCount(item),
                        wasAdded = false,
                        newCarryWeight = currentCarryWeight
                    });

                    // Apply item effects
                    ApplyItemEffects(item, removedQuantity);

                    Debug.Log($"[AdventurePlayerController] Used {removedQuantity}x {item.itemName}");
                    return true;
                }
            }

            return false;
        }

        private bool CanPickupItem(AdventureItemData item, int quantity)
        {
            if (item == null) return false;
            if (inventoryManager == null) return false;

            // Check if inventory has space
            return inventoryManager.CanAddItem(item, quantity);
        }

        private void ApplyItemEffects(AdventureItemData item, int quantity)
        {
            // Apply item-specific effects (healing, buffs, etc.)
            switch (item.itemType)
            {
                case AdventureItemType.Consumable:
                    // Apply consumable effects
                    break;
                case AdventureItemType.QuestItem:
                    // Trigger quest progression if needed
                    if (questManager != null)
                    {
                        // questManager.OnItemUsed(item);
                    }
                    break;
            }
        }

        #endregion

        #region Input Handling

        private void SetupInputCallbacks()
        {
            if (playerInput == null) return;

            // Note: In a real implementation, you would set up input action callbacks here
            // For now, we'll handle input in Update for simplicity
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                jumpPressed = true;
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            isRunning = context.ReadValue<float>() > 0.5f;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                interactPressed = true;
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // Toggle inventory UI
                if (inventoryManager != null)
                {
                    // inventoryManager.ToggleInventoryUI();
                }
            }
        }

        #endregion

        #region Service Integration

        private void InitializeServices()
        {
            // Get quest manager
            questManager = FindObjectOfType<QuestManager>();
            
            // Get inventory manager
            inventoryManager = FindObjectOfType<AdventureInventoryManager>();
            
            // Get interaction manager
            interactionManager = FindObjectOfType<InteractionManager>();

            // Register with ServiceLocator if available
            try
            {
                asterivo.Unity60.Core.ServiceLocator.RegisterService<AdventurePlayerController>(this);
            }
            catch
            {
                // ServiceLocator not available or already registered
            }
        }

        #endregion

        #region Utility Methods

        public Vector3 GetInteractionPoint()
        {
            return interactionPoint != null ? interactionPoint.position : transform.position;
        }

        public bool IsMoving()
        {
            return characterController.velocity.magnitude > 0.1f;
        }

        public bool IsNearPosition(Vector3 position, float threshold = 1f)
        {
            return Vector3.Distance(transform.position, position) <= threshold;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.blue;
            Vector3 interactionPos = interactionPoint != null ? interactionPoint.position : transform.position;
            Gizmos.DrawWireSphere(interactionPos, interactionRange);

            // Draw ground check
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }

    // Event data structures
    [System.Serializable]
    public class PlayerMovementEventData
    {
        public Vector3 position;
        public Vector3 velocity;
        public bool isRunning;
        public bool isGrounded;
    }

    [System.Serializable]
    public class PlayerInteractionEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public InteractionType interactionType;
        public Vector3 position;
    }

    
}