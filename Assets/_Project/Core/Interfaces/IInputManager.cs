using UnityEngine;
using UnityEngine.InputSystem;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 統合入力管理サービスインターフェース（Unity Input System統合・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層入力基盤において、
    /// Unity Input Systemと統合したアプリケーション全体の入力制御を一元管理する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// クロスプラットフォーム入力、リアルタイム設定、Template層互換性を実現します。
    ///
    /// 【Unity Input System統合アーキテクチャ】
    /// - Input Action Maps: アクションマップによる動的入力スキーム切り替え
    /// - Cross-Platform Support: マウス・キーボード・ゲームパッド・タッチの統一管理
    /// - Action-Based Input: 入力アクションの動的有効化・無効化制御
    /// - Input Binding Override: 実行時キーバインド変更とカスタマイズ対応
    ///
    /// 【ServiceLocator統合設計】
    /// - Central Input Hub: ServiceLocator.Get&lt;IInputManager&gt;()による統一アクセス
    /// - Cross-Layer Input: Core/Feature/Template層での統一入力処理
    /// - Singleton Alternative: ServiceLocatorによる依存性注入とライフサイクル管理
    /// - Mock Support: ユニットテスト用モック入力サービス登録対応
    ///
    /// 【高精度入力制御システム】
    /// - Sensitivity Control: マウス・ゲームパッド感度のリアルタイム調整
    /// - Y-Axis Inversion: Y軸反転設定によるユーザー操作性向上
    /// - Input Smoothing: 入力値の平滑化による自然な操作感実現
    /// - Dead Zone Management: アナログ入力のデッドゾーン動的調整
    ///
    /// 【包括的ゲーム入力対応】
    /// - Movement System: WASD/左スティック移動入力の統一処理
    /// - Camera Control: マウス/右スティック視点制御の精密管理
    /// - Action Inputs: ジャンプ・しゃがみ・ダッシュ・インタラクションの統合
    /// - Combat Inputs: エイム・射撃・リロード・近接攻撃の戦闘入力
    ///
    /// 【Template層互換性システム】
    /// - TPS Template: TPSゲームプレイ用入力メソッドの完全対応
    /// - FPS Template: 高精度エイム入力とクイック反応システム
    /// - Stealth Template: 慎重な移動制御と隠密アクション入力
    /// - Horror Template: 恐怖演出中の制限された入力制御
    ///
    /// 【リアルタイム入力状態管理】
    /// - Frame-Perfect Input: フレーム精度でのInputDown判定
    /// - Continuous Input: 継続入力のHold状態監視
    /// - Input History: 入力履歴による高度な操作判定
    /// - Event-Driven Input: 入力変更のGameEvent自動発行
    ///
    /// 【動的入力制御機能】
    /// - Input Toggle: ゲーム状態に応じた入力有効・無効切り替え
    /// - Context Switching: UIメニュー・ゲームプレイ間の入力文脈切り替え
    /// - Action Map Control: 動的アクションマップ変更による柔軟な入力制御
    /// - Custom Bindings: プレイヤーカスタムキーバインドの実行時適用
    ///
    /// 【パフォーマンス最適化】
    /// - Input Polling: 効率的な入力ポーリングによる CPU 負荷軽減
    /// - Event Batching: 複数入力イベントの一括処理による最適化
    /// - Memory Efficient: 軽量な入力状態管理とメモリ効率化
    /// - Update Optimization: 必要時のみの入力更新によるパフォーマンス向上
    ///
    /// 【ゲームジャンル特化対応】
    /// - Action Games: 高速反応を要求するアクション入力
    /// - Stealth Games: 慎重で精密な移動制御入力
    /// - Shooter Games: 高精度エイムと瞬間的な反応入力
    /// - Horror Games: 制限された操作による緊張感演出
    ///
    /// 【開発支援・デバッグ機能】
    /// - Input Visualization: エディタでの入力状態リアルタイム表示
    /// - Binding Validation: 入力バインディングの設定検証
    /// - Performance Metrics: 入力システムのパフォーマンス監視
    /// - Template Integration: 各ジャンルテンプレートとの自動連携
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