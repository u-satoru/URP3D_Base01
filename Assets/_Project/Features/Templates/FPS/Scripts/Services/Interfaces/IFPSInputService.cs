using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS Input Service Interface：一人称シューティング特化の入力管理システム
    /// ServiceLocator + Event駆動アーキテクチャによる統一入力管理
    /// 詳細設計書準拠：武器操作・移動・カメラ・インタラクション入力の統合管理
    /// </summary>
    public interface IFPSInputService : IFPSService
    {
        // Input State Properties
        bool IsEnabled { get; }
        bool IsMovementEnabled { get; }
        bool IsCombatEnabled { get; }
        bool IsCameraEnabled { get; }

        // Movement Input
        Vector2 MovementInput { get; }
        bool IsRunning { get; }
        bool IsCrouching { get; }
        bool IsJumping { get; }

        // Combat Input
        bool IsFirePressed { get; }
        bool IsFireHeld { get; }
        bool IsReloadPressed { get; }
        bool IsAimPressed { get; }
        bool IsAimHeld { get; }
        bool IsWeaponSwitchPressed { get; }

        // Camera Input
        Vector2 CameraInput { get; }
        bool IsCameraResetPressed { get; }

        // Interaction Input
        bool IsInteractPressed { get; }

        // UI Input
        bool IsPausePressed { get; }
        bool IsInventoryPressed { get; }

        // Input State Control
        void EnableAllInput();
        void DisableAllInput();
        void EnableMovementInput();
        void DisableMovementInput();
        void EnableCombatInput();
        void DisableCombatInput();
        void EnableCameraInput();
        void DisableCameraInput();

        // Input Sensitivity Management
        void SetMouseSensitivity(float sensitivity);
        void SetControllerSensitivity(float sensitivity);
        float GetMouseSensitivity();
        float GetControllerSensitivity();

        // Input Device Detection
        bool IsKeyboardActive { get; }
        bool IsGamepadActive { get; }
        string GetActiveInputDevice();

        // Events - Input State Changes
        event Action OnInputEnabled;
        event Action OnInputDisabled;
        event Action<string> OnInputDeviceChanged;

        // Events - Movement
        event Action<Vector2> OnMovementInput;
        event Action OnRunStarted;
        event Action OnRunStopped;
        event Action OnCrouchStarted;
        event Action OnCrouchStopped;
        event Action OnJumpPressed;

        // Events - Combat
        event Action OnFirePressed;
        event Action OnFireReleased;
        event Action OnReloadPressed;
        event Action OnAimStarted;
        event Action OnAimStopped;
        event Action OnWeaponSwitchPressed;

        // Events - Camera
        event Action<Vector2> OnCameraInput;
        event Action OnCameraReset;

        // Events - Interaction
        event Action OnInteractPressed;

        // Events - UI
        event Action OnPausePressed;
        event Action OnInventoryPressed;

        // Input History & Analytics
        void ClearInputHistory();
        float GetInputResponseTime();
        int GetInputActionsPerSecond();
    }
}