using UnityEngine;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Player
{
    public class StealthMovementController : MonoBehaviour
    {
        [Header("Movement Modes")]
        [SerializeField] private MovementMode[] movementModes;
        [SerializeField] private int currentModeIndex = 1;
        
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerTransform;
        
        [Header("Current State")]
        [SerializeField] private MovementStance currentStance = MovementStance.Standing;
        [SerializeField] private float currentNoiseLevel = 0.5f;
        [SerializeField] private float currentVisibility = 1.0f;
        [SerializeField] private bool isInShadow = false;
        
        [Header("Events")]
        [SerializeField] private MovementStanceEvent onStanceChanged;
        [SerializeField] private MovementInfoEvent onMovementInfoChanged;
        
        [Header("Settings")]
        [SerializeField] private float stanceTransitionSpeed = 2f;
        [SerializeField] private LayerMask shadowCheckLayers = -1;
        [SerializeField] private float shadowCheckRadius = 0.5f;
        
        private StealthMovementInfo currentMovementInfo;
        private float currentHeight;
        private float targetHeight;
        private Vector3 currentVelocity;
        
        [System.Serializable]
        public class MovementMode
        {
            public string name = "Movement Mode";
            public MovementStance stance = MovementStance.Standing;
            public float moveSpeed = 4f;
            public float noiseLevel = 0.5f;
            public float visibilityMultiplier = 1f;
            public float characterHeight = 2f;
            public Vector3 cameraOffset = Vector3.zero;
        }
        
        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
                
            if (playerTransform == null)
                playerTransform = transform;
                
            InitializeMovementModes();
        }
        
        private void InitializeMovementModes()
        {
            if (movementModes == null || movementModes.Length == 0)
            {
                movementModes = new MovementMode[]
                {
                    new MovementMode 
                    { 
                        name = "Prone", 
                        stance = MovementStance.Prone,
                        moveSpeed = 1f, 
                        noiseLevel = 0.1f, 
                        visibilityMultiplier = 0.3f,
                        characterHeight = 0.5f
                    },
                    new MovementMode 
                    { 
                        name = "Crouch", 
                        stance = MovementStance.Crouching,
                        moveSpeed = 2.5f, 
                        noiseLevel = 0.3f, 
                        visibilityMultiplier = 0.6f,
                        characterHeight = 1.2f
                    },
                    new MovementMode 
                    { 
                        name = "Walk", 
                        stance = MovementStance.Standing,
                        moveSpeed = 4f, 
                        noiseLevel = 0.5f, 
                        visibilityMultiplier = 1f,
                        characterHeight = 2f
                    },
                    new MovementMode 
                    { 
                        name = "Run", 
                        stance = MovementStance.Standing,
                        moveSpeed = 7f, 
                        noiseLevel = 1.0f, 
                        visibilityMultiplier = 1.2f,
                        characterHeight = 2f
                    }
                };
            }
            
            currentHeight = targetHeight = movementModes[currentModeIndex].characterHeight;
        }
        
        private void Start()
        {
            ApplyMovementMode(currentModeIndex);
        }
        
        private void Update()
        {
            UpdateCharacterHeight();
            UpdateShadowDetection();
            UpdateMovementInfo();
        }
        
        public void ToggleCrouch()
        {
            if (currentStance == MovementStance.Crouching)
            {
                SetStance(MovementStance.Standing);
            }
            else
            {
                SetStance(MovementStance.Crouching);
            }
        }
        
        public void ToggleProne()
        {
            if (currentStance == MovementStance.Prone)
            {
                SetStance(MovementStance.Standing);
            }
            else
            {
                SetStance(MovementStance.Prone);
            }
        }
        
        public void SetStance(MovementStance newStance)
        {
            if (currentStance == newStance) return;
            
            int modeIndex = FindModeIndexForStance(newStance);
            if (modeIndex >= 0)
            {
                ApplyMovementMode(modeIndex);
            }
        }
        
        private int FindModeIndexForStance(MovementStance stance)
        {
            for (int i = 0; i < movementModes.Length; i++)
            {
                if (movementModes[i].stance == stance)
                    return i;
            }
            return -1;
        }
        
        private void ApplyMovementMode(int modeIndex)
        {
            if (modeIndex < 0 || modeIndex >= movementModes.Length) return;
            
            currentModeIndex = modeIndex;
            MovementMode mode = movementModes[modeIndex];
            
            currentStance = mode.stance;
            targetHeight = mode.characterHeight;
            
            onStanceChanged?.Raise(currentStance);
        }
        
        private void UpdateCharacterHeight()
        {
            if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
            {
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, 
                    Time.deltaTime * stanceTransitionSpeed);
                    
                if (characterController != null)
                {
                    characterController.height = currentHeight;
                    Vector3 center = characterController.center;
                    center.y = currentHeight / 2f;
                    characterController.center = center;
                }
            }
        }
        
        private void UpdateShadowDetection()
        {
            RaycastHit hit;
            Vector3 checkPosition = transform.position + Vector3.up * 2f;
            
            isInShadow = !Physics.SphereCast(checkPosition, shadowCheckRadius, 
                Vector3.down, out hit, 10f, shadowCheckLayers);
        }
        
        private void UpdateMovementInfo()
        {
            MovementMode currentMode = movementModes[currentModeIndex];
            
            float lightLevel = CalculateLightLevel();
            float finalVisibility = currentMode.visibilityMultiplier * lightLevel;
            
            if (isInShadow)
                finalVisibility *= 0.5f;
                
            currentNoiseLevel = currentMode.noiseLevel * GetVelocityMagnitude();
            currentVisibility = finalVisibility;
            
            currentMovementInfo = new StealthMovementInfo
            {
                stance = currentStance,
                noiseRadius = currentNoiseLevel * 10f,
                visibilityModifier = currentVisibility,
                moveSpeedModifier = currentMode.moveSpeed / 4f,
                isInShadow = isInShadow,
                lightLevel = lightLevel
            };
            
            onMovementInfoChanged?.Raise(currentMovementInfo);
        }
        
        private float CalculateLightLevel()
        {
            return 0.7f;
        }
        
        private float GetVelocityMagnitude()
        {
            if (characterController != null)
                return characterController.velocity.magnitude / movementModes[currentModeIndex].moveSpeed;
            return 0f;
        }
        
        public float GetCurrentMoveSpeed()
        {
            return movementModes[currentModeIndex].moveSpeed;
        }
        
        public MovementStance GetCurrentStance() => currentStance;
        public float GetNoiseLevel() => currentNoiseLevel;
        public float GetVisibility() => currentVisibility;
        public bool IsInShadow() => isInShadow;
        public StealthMovementInfo GetMovementInfo() => currentMovementInfo;
    }
}