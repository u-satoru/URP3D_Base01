using UnityEngine;
using UnityEngine.InputSystem;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// Interface for input management service
    /// Provides centralized input handling for the application
    /// </summary>
    public interface IInputManager : IService
    {
        /// <summary>
        /// Set mouse sensitivity
        /// </summary>
        /// <param name="sensitivity">Mouse sensitivity value</param>
        void SetMouseSensitivity(float sensitivity);

        /// <summary>
        /// Set gamepad sensitivity
        /// </summary>
        /// <param name="sensitivity">Gamepad sensitivity value</param>
        void SetGamepadSensitivity(float sensitivity);

        /// <summary>
        /// Set Y-axis inversion
        /// </summary>
        /// <param name="invert">Whether to invert Y-axis</param>
        void SetInvertYAxis(bool invert);

        /// <summary>
        /// Get current mouse sensitivity
        /// </summary>
        float GetMouseSensitivity();

        /// <summary>
        /// Get current gamepad sensitivity
        /// </summary>
        float GetGamepadSensitivity();

        /// <summary>
        /// Check if Y-axis is inverted
        /// </summary>
        bool IsYAxisInverted();

        /// <summary>
        /// Enable or disable input
        /// </summary>
        /// <param name="enabled">Whether input should be enabled</param>
        void SetInputEnabled(bool enabled);

        /// <summary>
        /// Check if input is currently enabled
        /// </summary>
        bool IsInputEnabled();

        /// <summary>
        /// Get movement input vector
        /// </summary>
        Vector2 GetMovementInput();

        /// <summary>
        /// Get look input vector
        /// </summary>
        Vector2 GetLookInput();

        /// <summary>
        /// Check if jump input was pressed this frame
        /// </summary>
        bool GetJumpInputDown();

        /// <summary>
        /// Check if run input is being held
        /// </summary>
        bool GetRunInput();

        /// <summary>
        /// Check if crouch input was pressed this frame
        /// </summary>
        bool GetCrouchInputDown();

        /// <summary>
        /// Check if aim input is being held
        /// </summary>
        bool GetAimInput();

        /// <summary>
        /// Check if fire input was pressed this frame
        /// </summary>
        bool GetFireInputDown();

        /// <summary>
        /// Check if fire input is being held
        /// </summary>
        bool GetFireInput();

        /// <summary>
        /// Check if reload input was pressed this frame
        /// </summary>
        bool GetReloadInputDown();

        /// <summary>
        /// Check if melee input was pressed this frame
        /// </summary>
        bool GetMeleeInputDown();

        /// <summary>
        /// Check if interact input was pressed this frame
        /// </summary>
        bool GetInteractInputDown();

        /// <summary>
        /// Check if pause input was pressed this frame
        /// </summary>
        bool GetPauseInputDown();

        /// <summary>
        /// Check if respawn input was pressed this frame (TPS Template compatibility)
        /// </summary>
        bool GetRespawnInputDown();

        /// <summary>
        /// Check if jump input is pressed (TPS Template compatibility)
        /// </summary>
        bool IsJumpPressed();

        /// <summary>
        /// Check if sprint input is held (TPS Template compatibility)
        /// </summary>
        bool IsSprintHeld();

        /// <summary>
        /// Check if crouch input is pressed (TPS Template compatibility)
        /// </summary>
        bool IsCrouchPressed();

        /// <summary>
        /// Set input action map
        /// </summary>
        /// <param name="actionMap">Action map name</param>
        void SetActionMap(string actionMap);

        /// <summary>
        /// Enable specific input action
        /// </summary>
        /// <param name="actionName">Action name to enable</param>
        void EnableAction(string actionName);

        /// <summary>
        /// Disable specific input action
        /// </summary>
        /// <param name="actionName">Action name to disable</param>
        void DisableAction(string actionName);
    }
}
