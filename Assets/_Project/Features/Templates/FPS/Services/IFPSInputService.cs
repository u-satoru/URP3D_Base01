using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS入力サービス（ServiceLocator経由アクセス）
    /// Unity Input System統合対応
    /// </summary>
    public interface IFPSInputService
    {
        // 基本入力状態
        Vector2 MovementInput { get; }
        Vector2 LookInput { get; }
        bool IsFirePressed { get; }
        bool IsAimPressed { get; }
        bool IsReloadPressed { get; }
        bool IsSprintPressed { get; }
        bool IsJumpPressed { get; }
        bool IsCrouchPressed { get; }

        // 武器関連入力
        int WeaponSwitchInput { get; }
        bool IsWeaponSwitchPressed { get; }

        // 入力イベント
        event Action OnFirePressed;
        event Action OnFireReleased;
        event Action OnReloadPressed;
        event Action OnAimPressed;
        event Action OnAimReleased;
        event Action OnJumpPressed;
        event Action OnSprintPressed;
        event Action OnSprintReleased;
        event Action OnCrouchPressed;
        event Action OnCrouchReleased;
        event Action<int> OnWeaponSwitchPressed;

        // 入力設定
        void SetMouseSensitivity(float sensitivity);
        void EnableInput(bool enable);
        void SetInputMap(FPSInputMap inputMap);
    }

    /// <summary>
    /// FPS入力マップ定義
    /// </summary>
    public enum FPSInputMap
    {
        Gameplay,
        UI,
        Paused,
        Cutscene
    }
}