using UnityEngine;
using System.Collections.Generic;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Core.Debug;
using asterivo.Unity60.Features.Templates.Stealth.Mechanics;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Interaction
{
    /// <summary>
    /// 環境相互作用システム
    /// ステルスゲームにおける環境オブジェクトとのインタラクションを管理
    /// </summary>
    public class EnvironmentalInteraction : MonoBehaviour
    {
        #region Interactable Interface
        
        public interface IStealthInteractable
        {
            void Interact(GameObject interactor);
            bool CanInteract(GameObject interactor);
            string GetInteractionPrompt();
            float GetInteractionRange();
        }
        
        #endregion
        
        #region Properties and Fields
        
        [TabGroup("Interaction", "Settings")]
        [Header("Core Settings")]
        [SerializeField] private bool enableInteraction = true;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private float interactionCheckInterval = 0.1f;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        
        [TabGroup("Interaction", "UI")]
        [Header("Interaction UI")]
        [SerializeField] private GameObject interactionPromptPrefab;
        [SerializeField] private Transform uiCanvas;
        [SerializeField] private float promptOffset = 1.5f;
        [SerializeField] private bool showInteractionPrompts = true;
        
        [TabGroup("Interaction", "Input")]
        [Header("Input Settings")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private KeyCode quickInteractionKey = KeyCode.Q;
        [SerializeField] private float holdDuration = 0.5f;
        
        // Runtime state
        private IStealthInteractable currentInteractable;
        private GameObject currentInteractableObject;
        private List<IStealthInteractable> nearbyInteractables = new List<IStealthInteractable>();
        private GameObject currentPrompt;
        private float interactionHoldTime;
        private bool isInteracting;
        
        // Services
        private IStealthAudioService stealthAudioService;
        private IEventLogger eventLogger;
        private ICommandInvoker commandInvoker;
        
        // Events
        private GameEvent onInteractionStarted;
        private GameEvent onInteractionCompleted;
        private GameEvent onInteractionCancelled;
        
        // Caching
        private float lastCheckTime;
        private Collider[] interactableColliders = new Collider[10];
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeServices();
        }
        
        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            LoadEvents();
        }
        
        private void Update()
        {
            if (!enableInteraction || playerTransform == null) return;
            
            // Check for nearby interactables
            if (Time.time - lastCheckTime >= interactionCheckInterval)
            {
                CheckNearbyInteractables();
                lastCheckTime = Time.time;
            }
            
            // Handle interaction input
            HandleInteractionInput();
            
            // Update interaction prompt
            UpdateInteractionPrompt();
        }
        
        private void OnDestroy()
        {
            if (currentPrompt != null)
            {
                Destroy(currentPrompt);
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeServices()
        {
            stealthAudioService = ServiceLocator.GetService<IStealthAudioService>();
            eventLogger = ServiceLocator.GetService<IEventLogger>();
            commandInvoker = ServiceLocator.GetService<ICommandInvoker>();
        }
        
        private void LoadEvents()
        {
            // Load GameEvent assets from Resources or references
            // This would typically be done through ScriptableObject references
        }
        
        #endregion
        
        #region Interaction Detection
        
        private void CheckNearbyInteractables()
        {
            nearbyInteractables.Clear();
            
            // Find all interactables in range
            int count = Physics.OverlapSphereNonAlloc(
                playerTransform.position,
                interactionRange,
                interactableColliders,
                interactableLayerMask
            );
            
            IStealthInteractable closestInteractable = null;
            GameObject closestObject = null;
            float closestDistance = float.MaxValue;
            
            for (int i = 0; i < count; i++)
            {
                if (interactableColliders[i] != null)
                {
                    var interactable = interactableColliders[i].GetComponent<IStealthInteractable>();
                    if (interactable != null && interactable.CanInteract(playerTransform.gameObject))
                    {
                        nearbyInteractables.Add(interactable);
                        
                        float distance = Vector3.Distance(
                            playerTransform.position,
                            interactableColliders[i].transform.position
                        );
                        
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = interactable;
                            closestObject = interactableColliders[i].gameObject;
                        }
                    }
                }
            }
            
            // Update current interactable
            if (closestInteractable != currentInteractable)
            {
                OnInteractableChanged(closestInteractable, closestObject);
            }
        }
        
        private void OnInteractableChanged(IStealthInteractable newInteractable, GameObject newObject)
        {
            // Hide previous prompt
            if (currentPrompt != null)
            {
                currentPrompt.SetActive(false);
            }
            
            currentInteractable = newInteractable;
            currentInteractableObject = newObject;
            
            // Show new prompt
            if (currentInteractable != null && showInteractionPrompts)
            {
                ShowInteractionPrompt();
            }
        }
        
        #endregion
        
        #region Interaction Input
        
        private void HandleInteractionInput()
        {
            if (currentInteractable == null) return;
            
            // Quick interaction
            if (Input.GetKeyDown(quickInteractionKey))
            {
                PerformQuickInteraction();
                return;
            }
            
            // Normal interaction (hold)
            if (Input.GetKey(interactionKey))
            {
                interactionHoldTime += Time.deltaTime;
                
                if (!isInteracting)
                {
                    StartInteraction();
                }
                
                if (interactionHoldTime >= holdDuration)
                {
                    CompleteInteraction();
                }
            }
            else if (Input.GetKeyUp(interactionKey))
            {
                if (isInteracting && interactionHoldTime < holdDuration)
                {
                    CancelInteraction();
                }
                
                interactionHoldTime = 0f;
            }
        }
        
        private void StartInteraction()
        {
            isInteracting = true;
            onInteractionStarted?.Raise();
            
            eventLogger?.Log($"[EnvironmentalInteraction] Started interaction with {currentInteractableObject.name}");
        }
        
        private void CompleteInteraction()
        {
            if (!isInteracting) return;
            
            // Perform the interaction
            currentInteractable.Interact(playerTransform.gameObject);
            
            // Create and execute interaction command
            if (commandInvoker != null)
            {
                var interactionCommand = new InteractionCommand(currentInteractableObject, playerTransform.gameObject);
                commandInvoker.ExecuteCommand(interactionCommand);
            }
            
            onInteractionCompleted?.Raise();
            
            eventLogger?.Log($"[EnvironmentalInteraction] Completed interaction with {currentInteractableObject.name}");
            
            // Reset state
            isInteracting = false;
            interactionHoldTime = 0f;
            
            // Re-check interactables
            CheckNearbyInteractables();
        }
        
        private void CancelInteraction()
        {
            isInteracting = false;
            interactionHoldTime = 0f;
            
            onInteractionCancelled?.Raise();
            
            eventLogger?.Log($"[EnvironmentalInteraction] Cancelled interaction with {currentInteractableObject.name}");
        }
        
        private void PerformQuickInteraction()
        {
            currentInteractable.Interact(playerTransform.gameObject);
            
            onInteractionCompleted?.Raise();
            
            eventLogger?.Log($"[EnvironmentalInteraction] Quick interaction with {currentInteractableObject.name}");
            
            // Re-check interactables
            CheckNearbyInteractables();
        }
        
        #endregion
        
        #region UI Management
        
        private void ShowInteractionPrompt()
        {
            if (currentInteractable == null) return;
            
            // Create prompt if needed
            if (currentPrompt == null && interactionPromptPrefab != null)
            {
                currentPrompt = Instantiate(interactionPromptPrefab, uiCanvas);
            }
            
            if (currentPrompt != null)
            {
                currentPrompt.SetActive(true);
                
                // Update prompt text
                var promptText = currentPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (promptText != null)
                {
                    promptText.text = currentInteractable.GetInteractionPrompt();
                }
            }
        }
        
        private void UpdateInteractionPrompt()
        {
            if (currentPrompt == null || !currentPrompt.activeSelf || currentInteractableObject == null) return;
            
            // Position prompt above interactable
            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(
                currentInteractableObject.transform.position + Vector3.up * promptOffset
            );
            
            if (screenPos.z > 0)
            {
                currentPrompt.transform.position = screenPos;
                
                // Update progress bar if holding
                if (isInteracting)
                {
                    var progressBar = currentPrompt.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (progressBar != null)
                    {
                        progressBar.fillAmount = interactionHoldTime / holdDuration;
                    }
                }
            }
            else
            {
                currentPrompt.SetActive(false);
            }
        }
        
        #endregion
        
        #region Interaction Command
        
        /// <summary>
        /// コマンドパターンを使用したインタラクション
        /// </summary>
        private class InteractionCommand : ICommand
        {
            private GameObject interactable;
            private GameObject interactor;
            private bool wasExecuted;
            
            public InteractionCommand(GameObject interactable, GameObject interactor)
            {
                this.interactable = interactable;
                this.interactor = interactor;
            }
            
            public void Execute()
            {
                if (interactable != null)
                {
                    wasExecuted = true;
                    // Log interaction
                    Debug.Log($"[InteractionCommand] {interactor.name} interacted with {interactable.name}");
                }
            }
            
            public void Undo()
            {
                if (wasExecuted && interactable != null)
                {
                    // Potentially reverse the interaction
                    Debug.Log($"[InteractionCommand] Undoing interaction with {interactable.name}");
                }
            }
            
            public bool CanUndo => wasExecuted;
        }
        
        #endregion
        
        #region Stealth Interactables
        
        /// <summary>
        /// ドアインタラクション
        /// </summary>
        [System.Serializable]
        public class StealthDoor : MonoBehaviour, IStealthInteractable
        {
            [SerializeField] private bool isLocked;
            [SerializeField] private bool isOpen;
            [SerializeField] private float openSpeed = 2f;
            [SerializeField] private float openAngle = 90f;
            
            private Quaternion closedRotation;
            private Quaternion openRotation;
            
            private void Start()
            {
                closedRotation = transform.rotation;
                openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
            }
            
            public void Interact(GameObject interactor)
            {
                if (isLocked) return;
                
                isOpen = !isOpen;
                StopAllCoroutines();
                StartCoroutine(AnimateDoor());
                
                // Generate sound
                var audioService = ServiceLocator.GetService<IStealthAudioService>();
                audioService?.EmitDetectableSound(transform.position, 10f, 0.5f, "door");
            }
            
            public bool CanInteract(GameObject interactor)
            {
                return !isLocked || HasKey(interactor);
            }
            
            public string GetInteractionPrompt()
            {
                if (isLocked) return "Locked - Press [E] to unlock";
                return isOpen ? "Press [E] to close" : "Press [E] to open";
            }
            
            public float GetInteractionRange()
            {
                return 2f;
            }
            
            private bool HasKey(GameObject interactor)
            {
                // Check if interactor has required key
                return false;
            }
            
            private System.Collections.IEnumerator AnimateDoor()
            {
                Quaternion targetRotation = isOpen ? openRotation : closedRotation;
                
                while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        openSpeed * Time.deltaTime
                    );
                    yield return null;
                }
                
                transform.rotation = targetRotation;
            }
        }
        
        /// <summary>
        /// ディストラクションアイテム
        /// </summary>
        [System.Serializable]
        public class DistractionItem : MonoBehaviour, IStealthInteractable
        {
            [SerializeField] private float distractionRadius = 15f;
            [SerializeField] private float distractionDuration = 5f;
            [SerializeField] private bool singleUse = true;
            private bool used = false;
            
            public void Interact(GameObject interactor)
            {
                if (used && singleUse) return;
                
                // Create distraction
                StealthMechanics.Instance.CreateDistraction(transform.position, distractionRadius);
                
                // Disable after use
                if (singleUse)
                {
                    used = true;
                    gameObject.SetActive(false);
                }
            }
            
            public bool CanInteract(GameObject interactor)
            {
                return !used || !singleUse;
            }
            
            public string GetInteractionPrompt()
            {
                return "Press [E] to throw distraction";
            }
            
            public float GetInteractionRange()
            {
                return 1.5f;
            }
        }
        
        /// <summary>
        /// 隠れ場所
        /// </summary>
        [System.Serializable]
        public class HidingSpot : MonoBehaviour, IStealthInteractable
        {
            [SerializeField] private Transform hidingPosition;
            [SerializeField] private bool isOccupied;
            [SerializeField] private float hidingEffectiveness = 0.9f;
            
            private GameObject hiddenObject;
            
            public void Interact(GameObject interactor)
            {
                if (isOccupied)
                {
                    // Exit hiding
                    if (hiddenObject == interactor)
                    {
                        ExitHiding();
                    }
                }
                else
                {
                    // Enter hiding
                    EnterHiding(interactor);
                }
            }
            
            public bool CanInteract(GameObject interactor)
            {
                return !isOccupied || hiddenObject == interactor;
            }
            
            public string GetInteractionPrompt()
            {
                if (isOccupied && hiddenObject != null)
                {
                    return "Press [E] to exit hiding";
                }
                return "Press [E] to hide";
            }
            
            public float GetInteractionRange()
            {
                return 2f;
            }
            
            private void EnterHiding(GameObject interactor)
            {
                isOccupied = true;
                hiddenObject = interactor;
                
                // Move interactor to hiding position
                if (hidingPosition != null)
                {
                    interactor.transform.position = hidingPosition.position;
                    interactor.transform.rotation = hidingPosition.rotation;
                }
                
                // Notify stealth system
                StealthMechanics.Instance.ForceEnterStealth();
            }
            
            private void ExitHiding()
            {
                isOccupied = false;
                
                // Move out of hiding
                if (hiddenObject != null)
                {
                    hiddenObject.transform.position = transform.position + transform.forward * 1.5f;
                }
                
                hiddenObject = null;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 現在インタラクト可能なオブジェクトを取得
        /// </summary>
        public GameObject GetCurrentInteractable()
        {
            return currentInteractableObject;
        }
        
        /// <summary>
        /// 近くのインタラクト可能オブジェクトのリストを取得
        /// </summary>
        public List<IStealthInteractable> GetNearbyInteractables()
        {
            return new List<IStealthInteractable>(nearbyInteractables);
        }
        
        /// <summary>
        /// インタラクション中かどうか
        /// </summary>
        public bool IsInteracting()
        {
            return isInteracting;
        }
        
        /// <summary>
        /// 強制的にインタラクションを実行
        /// </summary>
        public void ForceInteraction(GameObject target)
        {
            var interactable = target.GetComponent<IStealthInteractable>();
            if (interactable != null && interactable.CanInteract(playerTransform.gameObject))
            {
                interactable.Interact(playerTransform.gameObject);
                onInteractionCompleted?.Raise();
            }
        }
        
        #endregion
        
        #region Editor
        
#if UNITY_EDITOR
        [TabGroup("Interaction", "Debug")]
        [Button("Test Interaction")]
        private void TestInteraction()
        {
            if (currentInteractable != null)
            {
                CompleteInteraction();
            }
        }
        
        [TabGroup("Interaction", "Debug")]
        [Button("Find All Interactables")]
        private void FindAllInteractables()
        {
            var allInteractables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int count = 0;
            
            foreach (var obj in allInteractables)
            {
                if (obj is IStealthInteractable)
                {
                    count++;
                    Debug.Log($"Found interactable: {obj.name}", obj);
                }
            }
            
            Debug.Log($"Total interactables found: {count}");
        }
        
        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;
            
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, interactionRange);
            
            // Draw current interactable
            if (currentInteractableObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerTransform.position, currentInteractableObject.transform.position);
                Gizmos.DrawWireCube(currentInteractableObject.transform.position, Vector3.one * 0.5f);
            }
            
            // Draw nearby interactables
            Gizmos.color = Color.cyan;
            foreach (var collider in interactableColliders)
            {
                if (collider != null && collider.gameObject != currentInteractableObject)
                {
                    Gizmos.DrawWireCube(collider.transform.position, Vector3.one * 0.3f);
                }
            }
        }
#endif
        
        #endregion
    }
}
