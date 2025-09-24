using UnityEngine;
using UnityEngine.InputSystem;

namespace asterivo.Unity60.Core.Input
{
    /// <summary>
    /// Core input data structures for the generic input management system.
    /// Provides type-safe input communication between Input System and game systems.
    /// </summary>

    /// <summary>
    /// Basic input action data containing action name and value
    /// </summary>
    [System.Serializable]
    public struct InputActionData
    {
        public string actionName;
        public float value;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public bool boolValue;
        public InputActionPhase phase;
        public double time;

        public static InputActionData Create(string name, float val, InputActionPhase actionPhase = InputActionPhase.Performed)
        {
            return new InputActionData
            {
                actionName = name,
                value = val,
                boolValue = val > 0.5f,
                phase = actionPhase,
                time = Time.timeAsDouble
            };
        }

        public static InputActionData Create(string name, Vector2 val, InputActionPhase actionPhase = InputActionPhase.Performed)
        {
            return new InputActionData
            {
                actionName = name,
                vector2Value = val,
                value = val.magnitude,
                phase = actionPhase,
                time = Time.timeAsDouble
            };
        }

        public static InputActionData Create(string name, bool val, InputActionPhase actionPhase = InputActionPhase.Performed)
        {
            return new InputActionData
            {
                actionName = name,
                boolValue = val,
                value = val ? 1f : 0f,
                phase = actionPhase,
                time = Time.timeAsDouble
            };
        }

        public bool IsPressed => boolValue && phase == InputActionPhase.Started;
        public bool IsHeld => boolValue && phase == InputActionPhase.Performed;
        public bool IsReleased => !boolValue && phase == InputActionPhase.Canceled;
    }

    /// <summary>
    /// Movement input data with processed values
    /// </summary>
    [System.Serializable]
    public struct MovementInputData
    {
        public Vector2 moveVector;
        public Vector2 lookVector;
        public bool isRunning;
        public bool isCrouching;
        public bool isJumping;
        public float moveSpeed;
        public float deltaTime;

        public static MovementInputData Create(Vector2 move, Vector2 look, bool run = false, bool crouch = false, bool jump = false)
        {
            return new MovementInputData
            {
                moveVector = move,
                lookVector = look,
                isRunning = run,
                isCrouching = crouch,
                isJumping = jump,
                moveSpeed = move.magnitude,
                deltaTime = Time.deltaTime
            };
        }

        public Vector3 GetWorldMoveDirection(Transform relativeTo)
        {
            if (relativeTo == null) return Vector3.zero;

            Vector3 forward = relativeTo.forward;
            Vector3 right = relativeTo.right;

            return (forward * moveVector.y + right * moveVector.x).normalized;
        }

        public bool HasMovementInput => moveVector.sqrMagnitude > 0.001f;
        public bool HasLookInput => lookVector.sqrMagnitude > 0.001f;
    }

    /// <summary>
    /// Combat/action input data
    /// </summary>
    [System.Serializable]
    public struct CombatInputData
    {
        public bool firePressed;
        public bool fireHeld;
        public bool fireReleased;
        public bool aimPressed;
        public bool aimHeld;
        public bool aimReleased;
        public bool reloadPressed;
        public bool interactPressed;
        public Vector2 aimDirection;
        public float deltaTime;

        public static CombatInputData Create(bool fire, bool aim, bool reload = false, bool interact = false, Vector2 aimDir = default)
        {
            return new CombatInputData
            {
                fireHeld = fire,
                aimHeld = aim,
                reloadPressed = reload,
                interactPressed = interact,
                aimDirection = aimDir,
                deltaTime = Time.deltaTime
            };
        }

        public bool HasAnyInput => firePressed || aimPressed || reloadPressed || interactPressed;
    }

    /// <summary>
    /// UI input data for menu navigation
    /// </summary>
    [System.Serializable]
    public struct UIInputData
    {
        public Vector2 navigate;
        public bool submit;
        public bool cancel;
        public bool pause;
        public bool point;
        public Vector2 scrollDelta;
        public float deltaTime;

        public static UIInputData Create(Vector2 nav, bool sub = false, bool can = false, bool ps = false)
        {
            return new UIInputData
            {
                navigate = nav,
                submit = sub,
                cancel = can,
                pause = ps,
                deltaTime = Time.deltaTime
            };
        }

        public bool HasNavigationInput => navigate.sqrMagnitude > 0.001f;
        public bool HasAnyInput => HasNavigationInput || submit || cancel || pause;
    }

    /// <summary>
    /// Input context for different game states
    /// </summary>
    public enum InputContext
    {
        Gameplay,       // Normal gameplay input
        Menu,           // UI/Menu navigation
        Cutscene,       // Limited input during cutscenes
        Pause,          // Paused state input
        Loading,        // Loading screen input
        Debug           // Debug/developer input
    }

    /// <summary>
    /// Input priority for handling conflicts
    /// </summary>
    public enum InputPriority
    {
        VeryLow = 0,
        Low = 10,
        Normal = 50,
        High = 100,
        VeryHigh = 200,
        Critical = 1000
    }

    /// <summary>
    /// Comprehensive input state data
    /// </summary>
    [System.Serializable]
    public struct InputStateData
    {
        public MovementInputData movement;
        public CombatInputData combat;
        public UIInputData ui;
        public InputContext context;
        public InputPriority priority;
        public float timestamp;
        public bool isValid;

        public static InputStateData Create(InputContext ctx = InputContext.Gameplay, InputPriority prio = InputPriority.Normal)
        {
            return new InputStateData
            {
                context = ctx,
                priority = prio,
                timestamp = Time.unscaledTime,
                isValid = true
            };
        }

        public void Invalidate()
        {
            isValid = false;
        }

        public bool ShouldProcessInput(InputContext requiredContext)
        {
            return isValid && context == requiredContext;
        }
    }
}
