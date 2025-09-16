using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPSテンプレート移動サービスインターフェース
    /// アーキテクチャ準拠: ServiceLocator + Event駆動のハイブリッドアプローチ
    /// </summary>
    public interface IMovementService
    {
        // プロパティ
        Vector3 Position { get; set; }
        Vector3 Velocity { get; }
        bool IsGrounded { get; }
        bool IsMoving { get; }
        bool IsSprinting { get; }
        bool IsCrouching { get; }
        bool IsJumping { get; }

        // 移動設定
        float WalkSpeed { get; set; }
        float SprintSpeed { get; set; }
        float CrouchSpeed { get; set; }
        float JumpForce { get; set; }
        float Gravity { get; set; }

        // イベント
        event Action<Vector3> OnPositionChanged;
        event Action<Vector3> OnVelocityChanged;
        event Action OnGroundedStateChanged;
        event Action OnMovementStarted;
        event Action OnMovementStopped;
        event Action OnJump;
        event Action OnLand;
        event Action OnSprintStarted;
        event Action OnSprintStopped;
        event Action OnCrouchStarted;
        event Action OnCrouchStopped;

        // 移動制御
        void Move(Vector3 direction);
        void MoveToPosition(Vector3 targetPosition, float speed = -1f);
        void Jump();
        void StartSprint();
        void StopSprint();
        void StartCrouch();
        void StopCrouch();
        void Stop();

        // 状態制御
        void SetMovementEnabled(bool enabled);
        bool IsMovementEnabled { get; }

        // 物理制御
        void ApplyForce(Vector3 force, ForceMode forceMode = ForceMode.Force);
        void SetGravityEnabled(bool enabled);
        bool IsGravityEnabled { get; }

        // 初期化
        void Initialize(Transform target, CharacterController controller = null);
        bool IsInitialized { get; }

        // 地面検知
        void CheckGroundStatus();
        float GetGroundDistance();
    }
}