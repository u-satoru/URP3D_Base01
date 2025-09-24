using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer Input Service Interface：入力処理・マッピング・コントローラー対応
    /// ServiceLocator + Event駆動アーキテクチャによる高度な入力システム
    /// Learn & Grow価値実現：直感的な入力設定・プラットフォーマー特化機能
    /// </summary>
    public interface IPlatformerInputService : IPlatformerService
    {
        // Core Input Properties
        Vector2 MovementInput { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool JumpReleased { get; }
        bool PausePressed { get; }

        // Extended Input Properties
        bool CrouchPressed { get; }
        bool CrouchHeld { get; }
        bool RunPressed { get; }
        bool RunHeld { get; }
        bool InteractPressed { get; }

        // Input Management
        void EnableInput(bool enable);
        void SetInputSensitivity(float horizontal, float vertical);
        void SwitchActionMap(string actionMapName);

        // Input Buffering
        bool IsInputBuffered(string inputName);
        bool ConsumeBufferedInput(string inputName);

        // Update Method (should be called from MonoBehaviour)
        void Update();

        // Debug & Diagnostics
        void ShowInputDebugInfo();

        // Events
        event Action<Vector2> OnMovementChanged;
        event Action OnJumpPressed;
        event Action OnJumpReleased;
        event Action OnCrouchPressed;
        event Action OnCrouchReleased;
        event Action OnRunPressed;
        event Action OnRunReleased;
        event Action OnInteractPressed;
        event Action OnPausePressed;
        event Action<bool> OnInputEnabledChanged;
    }
}
