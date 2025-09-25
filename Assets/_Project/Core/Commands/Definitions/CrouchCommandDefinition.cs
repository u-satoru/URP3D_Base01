using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// Crouch command definition.
    /// Encapsulates player or AI crouch actions.
    ///
    /// Main features:
    /// - Initiate and end crouch state
    /// - Modify movement speed and stealth effectiveness
    /// - Adjust collision size
    /// - Control animation and camera
    /// </summary>
    [System.Serializable]
    public class CrouchCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// Enum defining types of crouch
        /// </summary>
        public enum CrouchType
        {
            Normal,     // Normal crouch
            Sneak,      // Stealth-focused crouch
            Cover,      // Crouch for cover usage
            Slide       // Sliding
        }

        [Header("Crouch Parameters")]
        public CrouchType crouchType = CrouchType.Normal;
        public bool toggleMode = true; // true: toggle mode, false: hold-to-crouch mode
        public float speedMultiplier = 0.5f;
        public float heightReduction = 0.5f;

        [Header("Stealth Effects")]
        public float noiseReduction = 0.7f; // Sound reduction rate
        public float visibilityReduction = 0.3f; // Visibility reduction rate
        public bool canHideInTallGrass = true;

        [Header("Movement Constraints")]
        public bool canSprint = false;
        public bool canJump = false;
        public float maxSlopeAngle = 30f;

        [Header("Animation")]
        public float transitionDuration = 0.3f;
        public bool adjustCameraHeight = true;
        public float cameraHeightOffset = -0.5f;

        [Header("Physics")]
        public bool adjustColliderHeight = true;
        public bool maintainGroundContact = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CrouchCommandDefinition()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        public CrouchCommandDefinition(CrouchType type, bool isToggle, float speedMult = 0.5f)
        {
            crouchType = type;
            toggleMode = isToggle;
            speedMultiplier = speedMult;
        }

        /// <summary>
        /// Determine if crouch command can be executed
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // Basic executability check
            if (speedMultiplier < 0f || heightReduction < 0f || heightReduction > 1f)
                return false;

            if (transitionDuration < 0f)
                return false;

            // Additional checks if context exists
            if (context != null)
            {
                // Check current terrain (impossible on steep slopes)
                // Check ceiling height (restriction where standing up is impossible)
                // State exceptions (foot injuries, etc.)
                // Animation state check (impossible during jump)
            }

            return true;
        }

        /// <summary>
        /// Create crouch command
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new CrouchCommand(this, context);
        }
    }

    /// <summary>
    /// Actual command implementation corresponding to CrouchCommandDefinition
    /// </summary>
    public class CrouchCommand : ICommand
    {
        private CrouchCommandDefinition definition;
        private object context;
        private bool executed = false;
        private bool isCrouching = false;
        private float originalHeight;
        private float originalSpeed;
        private Vector3 originalCameraPosition;

        public CrouchCommand(CrouchCommandDefinition crouchDefinition, object executionContext)
        {
            definition = crouchDefinition;
            context = executionContext;
        }

        /// <summary>
        /// Execute crouch command
        /// </summary>
        public void Execute()
        {
            if (executed) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.crouchType} crouch: toggle={definition.toggleMode}");
#endif

            // Toggle mode switches state
            if (definition.toggleMode)
            {
                if (isCrouching)
                {
                    StandUp();
                }
                else
                {
                    StartCrouch();
                }
            }
            else
            {
                // Hold mode always starts crouch
                StartCrouch();
            }

            executed = true;
        }

        /// <summary>
        /// Start crouch state
        /// </summary>
        private void StartCrouch()
        {
            if (isCrouching) return;

            // Save state before execution (for Undo)
            SaveOriginalState();

            isCrouching = true;

            // Implement actual crouch processing here
            if (context is MonoBehaviour mono)
            {
                // Adjust collider height
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height *= (1f - definition.heightReduction);
                    collider.center = new Vector3(collider.center.x, collider.center.y - (originalHeight * definition.heightReduction * 0.5f), collider.center.z);
                }

                // Adjust movement speed (link with PlayerController)
                // Animation control
                // Adjust camera position
                // Apply stealth state
                // Sound effects
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Started crouching");
#endif
        }

        /// <summary>
        /// Stand up processing
        /// </summary>
        private void StandUp()
        {
            if (!isCrouching) return;

            // Check ceiling (whether standing up is possible)
            if (!CanStandUp())
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("Cannot stand up - ceiling too low");
#endif
                return;
            }

            isCrouching = false;

            // Restore state
            RestoreOriginalState();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Stood up from crouch");
#endif
        }

        /// <summary>
        /// Check if standing up is possible
        /// </summary>
        private bool CanStandUp()
        {
            // In actual implementation, check with Raycast if there are no obstacles above
            // Currently always returns true
            return true;
        }

        /// <summary>
        /// Save original state
        /// </summary>
        private void SaveOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // Save collider height
                if (mono.GetComponent<CapsuleCollider>() != null)
                {
                    originalHeight = mono.GetComponent<CapsuleCollider>().height;
                }

                // Save other states
                // originalSpeed = playerController.moveSpeed;
                // originalCameraPosition = camera.localPosition;
            }
        }

        /// <summary>
        /// Restore original state
        /// </summary>
        private void RestoreOriginalState()
        {
            if (context is MonoBehaviour mono)
            {
                // Restore collider
                if (definition.adjustColliderHeight && mono.GetComponent<CapsuleCollider>() != null)
                {
                    var collider = mono.GetComponent<CapsuleCollider>();
                    collider.height = originalHeight;
                    collider.center = new Vector3(collider.center.x, 0f, collider.center.z);
                }

                // Restore other states
                // playerController.moveSpeed = originalSpeed;
                // camera.localPosition = originalCameraPosition;
            }
        }

        /// <summary>
        /// End crouch in hold mode
        /// </summary>
        public void EndCrouch()
        {
            if (!definition.toggleMode && isCrouching)
            {
                StandUp();
            }
        }

        /// <summary>
        /// Undo operation (force release crouch state)
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            if (isCrouching)
            {
                StandUp();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Crouch command undone");
#endif

            executed = false;
        }

        /// <summary>
        /// Whether this command can be undone
        /// </summary>
        public bool CanUndo => executed;

        /// <summary>
        /// Whether currently crouching
        /// </summary>
        public bool IsCrouching => isCrouching;
    }
}