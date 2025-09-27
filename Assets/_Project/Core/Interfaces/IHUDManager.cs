using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// 統合HUD/UI管理サービスインターフェース（イベント駆動UI・3層アーキテクチャ対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層UI基盤において、
    /// ゲーム内HUD/UIシステムを一元管理する中核インターフェースです。
    /// ServiceLocatorパターンとIService統合により、
    /// リアルタイムUI更新、Template層互換性、高性能UI制御を実現します。
    ///
    /// 【包括的HUD管理システム】
    /// - Health System UI: ヘルス・ダメージ表示とローヘルス警告システム
    /// - Combat UI: 弾薬・武器・クロスヘア・ヒットマーカーの統合管理
    /// - Interactive UI: インタラクション・リロード進捗・カバー表示
    /// - Navigation UI: ミニマップ・プレイヤー位置追跡システム
    ///
    /// 【ServiceLocator統合設計】
    /// - Central UI Hub: ServiceLocator.Get&lt;IHUDManager&gt;()による統一アクセス
    /// - Cross-Layer UI: Core/Feature/Template層での統一UI制御
    /// - Singleton Alternative: ServiceLocatorによる依存性注入とライフサイクル管理
    /// - Mock Support: ユニットテスト用モックUIサービス登録対応
    ///
    /// 【リアルタイムUI更新システム】
    /// - Event-Driven Updates: GameEventによる自動UI同期
    /// - Frame-Rate Optimization: 60FPS安定動作のためのUI更新最適化
    /// - State Synchronization: ゲーム状態とUIの完全同期
    /// - Animation Integration: DOTween ProによるUI過渡効果
    ///
    /// 【Template層互換性システム】
    /// - TPS Template: 三人称シューティング用HUDレイアウト
    /// - FPS Template: 一人称シューティング用クロスヘア・ヒットマーカー
    /// - Stealth Template: ステルス特化UI（隠蔽インジケーター等）
    /// - Horror Template: 恐怖演出強化UI（ヘルス警告・画面歪み等）
    ///
    /// 【戦闘UI統合機能】
    /// - Weapon Display: 武器名・アイコン・弾薬数のリアルタイム表示
    /// - Crosshair System: 武器タイプ別クロスヘア・精度反映リティクル
    /// - Hit Feedback: ダメージインジケーター・ヘッドショットマーカー
    /// - Reload System: 進捗バー・アニメーション連動のリロードUI
    ///
    /// 【ゲーム状態UI制御】
    /// - Game Flow UI: ポーズメニュー・ゲームオーバー画面の管理
    /// - Player Status: ヘルス・ステート・インタラクション状態表示
    /// - Scoring System: スコア・メッセージ表示による成績フィードバック
    /// - Panel Management: 個別パネルの動的表示・非表示制御
    ///
    /// 【高度なUI演出機能】
    /// - Damage Visualization: 3D空間ダメージインジケーターの2D変換
    /// - Health Warning: ローヘルス時の画面エフェクト・警告表示
    /// - Cover Integration: カバーシステム連動のUI表示制御
    /// - Minimap System: プレイヤー位置・回転のリアルタイム追跡
    ///
    /// 【パフォーマンス最適化】
    /// - UI Pooling: 頻繁表示UI要素のオブジェクトプール管理
    /// - Canvas Optimization: UI階層とCanvasの効率的管理
    /// - Event Batching: 複数UI更新の一括処理による最適化
    /// - Memory Management: UI要素の軽量化とメモリ効率化
    ///
    /// 【ゲームジャンル特化対応】
    /// - Action Games: 高速戦闘対応のクイック反応UI
    /// - Stealth Games: 隠密性重視の控えめなUIデザイン
    /// - Shooter Games: 精密エイム支援の高精度クロスヘア
    /// - Horror Games: 恐怖演出強化の特殊UI効果
    ///
    /// 【開発支援・デバッグ機能】
    /// - UI Debug Mode: エディタでのUI状態可視化
    /// - Layout Validation: UI要素配置の自動検証
    /// - Performance Metrics: UIシステムのパフォーマンス監視
    /// - Template Integration: 各ジャンルテンプレートとの自動連携
    ///
    /// 【アクセシビリティ対応】
    /// - Text Scaling: 動的テキストサイズ調整
    /// - Color Blind Support: 色覚バリアフリー対応
    /// - High Contrast: 高コントラストモード対応
    /// - UI Navigation: ゲームパッドによるUI操作支援
    /// </summary>
    public interface IHUDManager : IService
    {
        /// <summary>
        /// Update player health display
        /// </summary>
        /// <param name="currentHealth">Current health value</param>
        /// <param name="maxHealth">Maximum health value</param>
        void UpdateHealthDisplay(float currentHealth, float maxHealth);

        /// <summary>
        /// Update ammo display
        /// </summary>
        /// <param name="currentAmmo">Current ammo in magazine</param>
        /// <param name="totalAmmo">Total ammo available</param>
        void UpdateAmmoDisplay(int currentAmmo, int totalAmmo);

        /// <summary>
        /// Update weapon display with generic weapon information
        /// </summary>
        /// <param name="weaponName">Name of the current weapon</param>
        /// <param name="weaponIconId">Icon identifier for the weapon</param>
        void UpdateWeaponDisplay(string weaponName, string weaponIconId = "");

        /// <summary>
        /// Update player state display
        /// </summary>
        /// <param name="stateName">Name of the current player state</param>
        void UpdatePlayerStateDisplay(string stateName);

        /// <summary>
        /// Show or hide crosshair
        /// </summary>
        /// <param name="show">Whether to show crosshair</param>
        void ShowCrosshair(bool show);

        /// <summary>
        /// Update crosshair style based on weapon type
        /// </summary>
        /// <param name="weaponTypeId">Identifier for weapon type (e.g., "rifle", "pistol", "shotgun")</param>
        void UpdateCrosshairStyle(string weaponTypeId);

        /// <summary>
        /// Show damage indicator
        /// </summary>
        /// <param name="damage">Damage amount</param>
        /// <param name="position">World position where damage occurred</param>
        void ShowDamageIndicator(float damage, Vector3 position);

        /// <summary>
        /// Show hit marker
        /// </summary>
        /// <param name="isHeadshot">Whether it was a headshot</param>
        void ShowHitMarker(bool isHeadshot = false);

        /// <summary>
        /// Update reticle spread based on accuracy
        /// </summary>
        /// <param name="spread">Spread value (0-1)</param>
        void UpdateReticleSpread(float spread);

        /// <summary>
        /// Show reload progress
        /// </summary>
        /// <param name="progress">Reload progress (0-1)</param>
        void ShowReloadProgress(float progress);

        /// <summary>
        /// Hide reload progress
        /// </summary>
        void HideReloadProgress();

        /// <summary>
        /// Show low health warning
        /// </summary>
        /// <param name="intensity">Warning intensity (0-1)</param>
        void ShowLowHealthWarning(float intensity);

        /// <summary>
        /// Hide low health warning
        /// </summary>
        void HideLowHealthWarning();

        /// <summary>
        /// Show interaction prompt
        /// </summary>
        /// <param name="text">Prompt text</param>
        void ShowInteractionPrompt(string text);

        /// <summary>
        /// Hide interaction prompt
        /// </summary>
        void HideInteractionPrompt();

        /// <summary>
        /// Show cover indicator
        /// </summary>
        /// <param name="show">Whether to show cover indicator</param>
        void ShowCoverIndicator(bool show);

        /// <summary>
        /// Update minimap player position
        /// </summary>
        /// <param name="position">Player world position</param>
        /// <param name="rotation">Player rotation</param>
        void UpdateMinimapPlayer(Vector3 position, float rotation);

        /// <summary>
        /// Show or hide HUD
        /// </summary>
        /// <param name="show">Whether to show HUD</param>
        void ShowHUD(bool show);

        /// <summary>
        /// Set HUD visibility for specific panels
        /// </summary>
        /// <param name="panelName">Name of the panel</param>
        /// <param name="visible">Whether panel should be visible</param>
        void SetPanelVisibility(string panelName, bool visible);

        /// <summary>
        /// Show game over screen
        /// </summary>
        void ShowGameOverScreen();

        /// <summary>
        /// Hide game over screen
        /// </summary>
        void HideGameOverScreen();

        /// <summary>
        /// Show pause menu
        /// </summary>
        void ShowPauseMenu();

        /// <summary>
        /// Hide pause menu
        /// </summary>
        void HidePauseMenu();

        /// <summary>
        /// Update score display
        /// </summary>
        /// <param name="score">Current score</param>
        void UpdateScoreDisplay(int score);

        /// <summary>
        /// Show message to player
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="duration">Display duration in seconds</param>
        void ShowMessage(string message, float duration = 3.0f);
    }
}