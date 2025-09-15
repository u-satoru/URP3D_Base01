using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Adventure.Player;
using asterivo.Unity60.Features.Templates.Adventure.Interaction;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;

namespace asterivo.Unity60.Features.Templates.Adventure.Events
{
    // Player Movement Events
    [System.Serializable]
    public class PlayerMovementEventData
    {
        public AdventurePlayerController player;
        public Vector3 previousPosition;
        public Vector3 newPosition;
        public Vector3 movementDelta;
        public float movementSpeed;
        public bool isGrounded;
        public bool isRunning;
        public bool isCrouching;
    }

    [CreateAssetMenu(fileName = "PlayerMovementEvent", menuName = "Adventure Template/Events/Player Movement Event")]
    public class PlayerMovementGameEvent : GameEvent<PlayerMovementEventData> { }

    // Player Interaction Events
    [System.Serializable]
    public class PlayerInteractionEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public InteractionType interactionType;
        public Vector3 position;
        public bool successful;
    }

    [CreateAssetMenu(fileName = "PlayerInteractionEvent", menuName = "Adventure Template/Events/Player Interaction Event")]
    public class PlayerInteractionGameEvent : GameEvent<PlayerInteractionEventData> { }

    // Player Inventory Events
    [System.Serializable]
    public class PlayerInventoryChangedEventData
    {
        public AdventurePlayerController player;
        public AdventureItemData item;
        public int quantityChanged;
        public int newTotalQuantity;
        public bool wasAdded; // true for add, false for remove
        public float newCarryWeight;
    }

    [CreateAssetMenu(fileName = "PlayerInventoryChangedEvent", menuName = "Adventure Template/Events/Player Inventory Changed Event")]
    public class PlayerInventoryChangedGameEvent : GameEvent<PlayerInventoryChangedEventData> { }

    // Interaction System Events
    [System.Serializable]
    public class InteractionStartedEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public float interactionStartTime;
        public string interactionPrompt;
    }

    [CreateAssetMenu(fileName = "InteractionStartedEvent", menuName = "Adventure Template/Events/Interaction Started Event")]
    public class InteractionStartedGameEvent : GameEvent<InteractionStartedEventData> { }

    [System.Serializable]
    public class InteractionEndedEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public float interactionDuration;
        public bool wasSuccessful;
        public string endReason;
    }

    [CreateAssetMenu(fileName = "InteractionEndedEvent", menuName = "Adventure Template/Events/Interaction Ended Event")]
    public class InteractionEndedGameEvent : GameEvent<InteractionEndedEventData> { }

    [System.Serializable]
    public class InteractionAttemptEventData
    {
        public BaseInteractable interactable;
        public AdventurePlayerController player;
        public bool canInteract;
        public string failureReason;
        public Vector3 attemptPosition;
    }

    [CreateAssetMenu(fileName = "InteractionAttemptEvent", menuName = "Adventure Template/Events/Interaction Attempt Event")]
    public class InteractionAttemptGameEvent : GameEvent<InteractionAttemptEventData> { }
}