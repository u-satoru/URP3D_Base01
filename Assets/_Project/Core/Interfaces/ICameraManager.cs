using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 統合カメラ管理サービスインターフェース（Cinemachine統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層カメラ基盤において、
    /// Cinemachine Virtual Cameraの統合管理とTemplate層互換性を実現する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// 動的カメラ切り替え、状態管理、高度なカメラエフェクトを一元制御します。
    ///
    /// 【Cinemachine統合カメラシステム】
    /// - Virtual Camera Management: First/Third/Aim/Cover モードの動的切り替え
    /// - Brain Integration: Cinemachine Brainによる滑らかなカメラブレンド
    /// - Priority Control: カメラ優先度による自動切り替えシステム
    /// - Timeline Support: Cinemachine Timelineでのカットシーンカメラワーク
    ///
    /// 【多視点カメラ制御システム】
    /// - First Person: 一人称視点によるFPS/ホラーゲーム対応
    /// - Third Person: 三人称視点によるTPS/アクション対応
    /// - Aim Mode: 精密エイムカメラによるシューティング対応
    /// - Cover Mode: カバーシステム連動カメラによる戦術的ゲームプレイ
    ///
    /// 【ServiceLocator統合設計】
    /// - Central Camera Hub: ServiceLocator.Get&lt;ICameraManager&gt;()による統一アクセス
    /// - Cross-Layer Control: Core/Feature/Template層での統一カメラ制御
    /// - Singleton Alternative: ServiceLocatorによる依存性注入とライフサイクル管理
    /// - Mock Support: ユニットテスト用モックカメラサービス登録対応
    ///
    /// 【Template層互換性システム】
    /// - TPS Template: TPSWeaponManager連動のリコイル・シェイク効果
    /// - Stealth Template: 隠密行動に最適化されたカメラワーク
    /// - FPS Template: 高精度エイムとクイック視点切り替え
    /// - Horror Template: 恐怖演出強化のカメラエフェクト
    ///
    /// 【高度なカメラエフェクト】
    /// - Screen Shake: 爆発・衝撃による画面振動エフェクト
    /// - Recoil System: 武器発砲時のリアルなリコイル表現
    /// - Smooth Damping: 移動・回転の自然な減衰制御
    /// - FOV Control: 視野角動的変更によるズーム・広角効果
    ///
    /// 【パフォーマンス最適化】
    /// - Efficient Transitions: Cinemachine最適化による滑らかな遷移
    /// - LOD Integration: 距離に応じたカメラ品質の動的調整
    /// - Memory Management: カメラ状態の軽量管理とメモリ効率化
    /// - Update Optimization: 必要時のみのカメラパラメータ更新
    ///
    /// 【リアルタイムカメラ制御】
    /// - Dynamic Parameters: 距離・高さ・FOVのリアルタイム調整
    /// - Target Management: Follow/LookAtターゲットの動的変更
    /// - State Queries: 現在カメラモードの即座判定
    /// - Position Access: カメラ位置・回転の高速取得
    ///
    /// 【ゲームジャンル最適化】
    /// - Action Games: 高速移動対応の応答性重視設定
    /// - Stealth Games: 慎重な移動に最適な精密制御
    /// - Horror Games: 恐怖演出強化の特殊エフェクト
    /// - Shooting Games: エイム精度向上の専門設定
    ///
    /// 【開発支援機能】
    /// - Camera Debug: エディタでのカメラ状態可視化
    /// - Parameter Validation: カメラ設定値の自動検証
    /// - Performance Metrics: カメラシステムのパフォーマンス監視
    /// - Template Integration: 各ジャンルテンプレートとの自動連携
    /// </summary>
    public interface ICameraManager : IService
    {
        /// <summary>
        /// Set third person camera distance from target
        /// </summary>
        /// <param name="distance">Distance from target</param>
        void SetThirdPersonDistance(float distance);

        /// <summary>
        /// Set third person camera height offset
        /// </summary>
        /// <param name="height">Height offset from target</param>
        void SetThirdPersonHeight(float height);

        /// <summary>
        /// Set camera field of view
        /// </summary>
        /// <param name="fov">Field of view in degrees</param>
        void SetFieldOfView(float fov);

        /// <summary>
        /// Set camera transition speed
        /// </summary>
        /// <param name="speed">Transition speed</param>
        void SetTransitionSpeed(float speed);

        /// <summary>
        /// Get current third person distance
        /// </summary>
        float GetThirdPersonDistance();

        /// <summary>
        /// Get current third person height
        /// </summary>
        float GetThirdPersonHeight();

        /// <summary>
        /// Get current field of view
        /// </summary>
        float GetFieldOfView();

        /// <summary>
        /// Get current transition speed
        /// </summary>
        float GetTransitionSpeed();

        /// <summary>
        /// Set camera follow target
        /// </summary>
        /// <param name="target">Transform to follow</param>
        void SetFollowTarget(Transform target);

        /// <summary>
        /// Set camera look at target
        /// </summary>
        /// <param name="target">Transform to look at</param>
        void SetLookAtTarget(Transform target);

        /// <summary>
        /// Get current follow target
        /// </summary>
        Transform GetFollowTarget();

        /// <summary>
        /// Get current look at target
        /// </summary>
        Transform GetLookAtTarget();

        /// <summary>
        /// Switch to first person camera
        /// </summary>
        void SwitchToFirstPerson();

        /// <summary>
        /// Switch to third person camera
        /// </summary>
        void SwitchToThirdPerson();

        /// <summary>
        /// Switch to aim camera mode
        /// </summary>
        void SwitchToAimMode();

        /// <summary>
        /// Switch to cover camera mode
        /// </summary>
        void SwitchToCoverMode();

        /// <summary>
        /// Check if camera is in first person mode
        /// </summary>
        bool IsFirstPerson();

        /// <summary>
        /// Check if camera is in third person mode
        /// </summary>
        bool IsThirdPerson();

        /// <summary>
        /// Check if camera is in aim mode
        /// </summary>
        bool IsAimMode();

        /// <summary>
        /// Check if camera is in cover mode
        /// </summary>
        bool IsCoverMode();

        /// <summary>
        /// Set camera shake intensity
        /// </summary>
        /// <param name="intensity">Shake intensity</param>
        /// <param name="duration">Shake duration</param>
        void AddCameraShake(float intensity, float duration);

        /// <summary>
        /// Stop camera shake
        /// </summary>
        void StopCameraShake();

        /// <summary>
        /// Set camera movement damping
        /// </summary>
        /// <param name="damping">Damping value</param>
        void SetMovementDamping(float damping);

        /// <summary>
        /// Set camera rotation damping
        /// </summary>
        /// <param name="damping">Damping value</param>
        void SetRotationDamping(float damping);

        /// <summary>
        /// Get current camera position
        /// </summary>
        Vector3 GetCameraPosition();

        /// <summary>
        /// Get current camera rotation
        /// </summary>
        Quaternion GetCameraRotation();

        /// <summary>
        /// Get main camera component
        /// </summary>
        UnityEngine.Camera GetMainCamera();

        /// <summary>
        /// Apply screen shake effect (TPS Template compatibility)
        /// </summary>
        /// <param name="intensity">Shake intensity</param>
        /// <param name="duration">Shake duration</param>
        void ApplyScreenShake(float intensity, float duration = 0.5f);

        /// <summary>
        /// Apply recoil effect to camera (TPS Template compatibility)
        /// </summary>
        /// <param name="recoilAmount">Recoil intensity</param>
        void ApplyRecoil(float recoilAmount);

        /// <summary>
        /// Handle player death camera behavior (TPS Template compatibility)
        /// </summary>
        void OnPlayerDeath();

        /// <summary>
        /// Handle player respawn camera behavior (TPS Template compatibility)
        /// </summary>
        void OnPlayerRespawn();
    }
}