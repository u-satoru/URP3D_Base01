using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPSカメラサービス（ServiceLocator経由アクセス）
    /// Cinemachine 3.1統合対応
    /// </summary>
    public interface IFPSCameraService
    {
        bool IsFirstPerson { get; }
        float MouseSensitivity { get; set; }
        float FieldOfView { get; set; }
        Transform CameraTransform { get; }

        void SetFirstPersonMode(bool firstPerson);
        void ApplyCameraShake(float intensity, float duration);
        void SetAimingMode(bool aiming);
        void SetCameraPosition(Vector3 position);
        void SetCameraRotation(Quaternion rotation);

        // カメラ状態管理（DESIGN.mdのCameraStateMachine統合用）
        CameraState CurrentState { get; }
        void SwitchCameraState(CameraState newState);

        event Action<bool> OnViewModeChanged;
        event Action<CameraState> OnCameraStateChanged;
    }

    /// <summary>
    /// カメラ状態定義（DESIGN.mdのCameraStateMachine準拠）
    /// </summary>
    public enum CameraState
    {
        FirstPerson,    // 一人称視点
        ThirdPerson,    // 三人称視点
        Aim,           // 照準モード
        Cover          // カバー状態
    }
}