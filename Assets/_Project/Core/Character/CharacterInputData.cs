using UnityEngine;

namespace asterivo.Unity60.Core.Character
{
    /// <summary>
    /// Data structure for character input information.
    /// Used to pass input data between Input System, Commands, and Character Control System.
    /// </summary>
    [System.Serializable]
    public struct CharacterInputData
    {
        #region Movement Input

        /// <summary>
        /// Movement input vector (typically from WASD or analog stick)
        /// </summary>
        public Vector2 movementInput;

        /// <summary>
        /// Movement direction in world space (calculated from movement input and camera)
        /// </summary>
        public Vector3 worldMovementDirection;

        /// <summary>
        /// Raw movement magnitude (0-1 range for analog input support)
        /// </summary>
        public float movementMagnitude;

        #endregion

        #region Action Input

        /// <summary>
        /// Jump input (button press)
        /// </summary>
        public bool jumpPressed;

        /// <summary>
        /// Jump input held (button hold)
        /// </summary>
        public bool jumpHeld;

        /// <summary>
        /// Run/Sprint input
        /// </summary>
        public bool runPressed;

        /// <summary>
        /// Crouch input
        /// </summary>
        public bool crouchPressed;

        /// <summary>
        /// Crouch input held
        /// </summary>
        public bool crouchHeld;

        #endregion

        #region Camera Input

        /// <summary>
        /// Camera look input (mouse delta or right stick)
        /// </summary>
        public Vector2 lookInput;

        /// <summary>
        /// Camera reference transform for calculating world movement direction
        /// </summary>
        public Transform cameraTransform;

        #endregion

        #region Timing

        /// <summary>
        /// Time when this input data was captured
        /// </summary>
        public float captureTime;

        /// <summary>
        /// Delta time since last input capture
        /// </summary>
        public float deltaTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Create character input data with movement and camera information
        /// </summary>
        /// <param name="movementInput">Movement input vector</param>
        /// <param name="cameraTransform">Camera transform for world space calculations</param>
        public CharacterInputData(Vector2 movementInput, Transform cameraTransform)
        {
            this.movementInput = movementInput;
            this.cameraTransform = cameraTransform;
            this.captureTime = Time.time;
            this.deltaTime = Time.deltaTime;

            // Calculate world movement direction
            this.worldMovementDirection = CalculateWorldMovementDirection(movementInput, cameraTransform);
            this.movementMagnitude = movementInput.magnitude;

            // Initialize action inputs
            this.jumpPressed = false;
            this.jumpHeld = false;
            this.runPressed = false;
            this.crouchPressed = false;
            this.crouchHeld = false;
            this.lookInput = Vector2.zero;
        }

        /// <summary>
        /// Create complete character input data
        /// </summary>
        public CharacterInputData(Vector2 movementInput, bool jumpPressed, bool jumpHeld,
                                bool runPressed, bool crouchPressed, bool crouchHeld,
                                Vector2 lookInput, Transform cameraTransform)
        {
            this.movementInput = movementInput;
            this.jumpPressed = jumpPressed;
            this.jumpHeld = jumpHeld;
            this.runPressed = runPressed;
            this.crouchPressed = crouchPressed;
            this.crouchHeld = crouchHeld;
            this.lookInput = lookInput;
            this.cameraTransform = cameraTransform;
            this.captureTime = Time.time;
            this.deltaTime = Time.deltaTime;

            // Calculate derived values
            this.worldMovementDirection = CalculateWorldMovementDirection(movementInput, cameraTransform);
            this.movementMagnitude = movementInput.magnitude;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculate world space movement direction from input and camera
        /// </summary>
        /// <param name="input">2D movement input</param>
        /// <param name="cameraTransform">Camera transform</param>
        /// <returns>World space movement direction</returns>
        private static Vector3 CalculateWorldMovementDirection(Vector2 input, Transform cameraTransform)
        {
            if (cameraTransform == null)
            {
                // Fallback to world space input
                return new Vector3(input.x, 0, input.y).normalized;
            }

            // Get camera forward and right vectors (projected to horizontal plane)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Remove vertical component
            cameraForward.y = 0;
            cameraRight.y = 0;

            // Normalize
            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;

            // Calculate world space direction
            Vector3 worldDirection = (cameraForward * input.y) + (cameraRight * input.x);
            return worldDirection.normalized;
        }

        /// <summary>
        /// Check if there is any movement input
        /// </summary>
        public bool HasMovementInput => movementMagnitude > 0.1f;

        /// <summary>
        /// Check if there is any action input
        /// </summary>
        public bool HasActionInput => jumpPressed || runPressed || crouchPressed;

        /// <summary>
        /// Check if there is any input at all
        /// </summary>
        public bool HasAnyInput => HasMovementInput || HasActionInput || lookInput.magnitude > 0.1f;

        /// <summary>
        /// Create a copy with updated timing information
        /// </summary>
        /// <returns>Updated input data</returns>
        public CharacterInputData WithUpdatedTiming()
        {
            var updated = this;
            updated.deltaTime = Time.deltaTime;
            updated.captureTime = Time.time;
            return updated;
        }

        /// <summary>
        /// Create a copy with different movement input
        /// </summary>
        /// <param name="newMovementInput">New movement input</param>
        /// <returns>Updated input data</returns>
        public CharacterInputData WithMovementInput(Vector2 newMovementInput)
        {
            var updated = this;
            updated.movementInput = newMovementInput;
            updated.worldMovementDirection = CalculateWorldMovementDirection(newMovementInput, cameraTransform);
            updated.movementMagnitude = newMovementInput.magnitude;
            return updated;
        }

        #endregion

        #region Static Utility

        /// <summary>
        /// Create empty input data
        /// </summary>
        public static CharacterInputData Empty => new CharacterInputData(Vector2.zero, null);

        /// <summary>
        /// Create input data with only movement
        /// </summary>
        /// <param name="movement">Movement input</param>
        /// <param name="camera">Camera transform</param>
        /// <returns>Input data with movement</returns>
        public static CharacterInputData MovementOnly(Vector2 movement, Transform camera)
        {
            return new CharacterInputData(movement, camera);
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get debug string representation
        /// </summary>
        /// <returns>Debug string</returns>
        public override string ToString()
        {
            return $"CharacterInput[Move:{movementInput}, World:{worldMovementDirection}, Jump:{jumpPressed}, Run:{runPressed}, Crouch:{crouchPressed}]";
        }

        #endregion
    }
}
