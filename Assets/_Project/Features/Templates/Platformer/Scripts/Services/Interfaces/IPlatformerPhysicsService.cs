using System;
using UnityEngine;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer物理サービス インターフェース
    /// Jump & Movement Physics システム・ServiceLocator統合
    /// </summary>
    public interface IPlatformerPhysicsService : IPlatformerService
    {
        // 物理設定プロパティ
        float Gravity { get; }
        float JumpVelocity { get; }
        float MoveSpeed { get; }
        bool EnableWallJump { get; }

        // ジャンプ制御
        Vector2 CalculateJumpVelocity(bool isGrounded, int jumpCount);
        bool CanJump(bool isGrounded, int currentJumpCount);
        Vector2 ApplyGravity(Vector2 velocity, bool isGrounded, bool isJumpPressed);

        // 移動制御
        Vector2 CalculateMovement(Vector2 currentVelocity, float inputAxis, bool isGrounded);
        Vector2 ApplyAirControl(Vector2 velocity, float inputAxis);

        // 地面・壁検出
        bool CheckGrounded(Vector2 position, LayerMask groundMask);
        bool CheckWall(Vector2 position, Vector2 direction, LayerMask wallMask);
        RaycastHit2D GetGroundInfo(Vector2 position, LayerMask groundMask);

        // 物理設定更新
        void UpdateSettings(PlatformerPhysicsSettings newSettings);
        void ApplyDifficultyModifiers(PlatformerGameplaySettings.GameDifficulty difficulty);

        // パフォーマンス最適化
        void EnablePhysicsOptimization(bool enable);
        void SetPhysicsUpdateRate(int fps);
    }
}
