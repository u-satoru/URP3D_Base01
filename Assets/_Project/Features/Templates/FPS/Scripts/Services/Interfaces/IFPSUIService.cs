using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS UI Service Interface：一人称シューティング特化のユーザーインターフェース管理
    /// ServiceLocator + Event駆動アーキテクチャによる高度なUI管理システム
    /// 詳細設計書準拠：HUD更新・弾薬表示・ヘルスバー・クロスヘア・スコア表示
    /// </summary>
    public interface IFPSUIService : IFPSService
    {
        // Core UI Management
        void ShowGameUI();
        void HideGameUI();
        void ShowPauseMenu();
        void HidePauseMenu();
        void ShowGameOverScreen();
        void ShowVictoryScreen();
        void HideAllScreens();

        // FPS-specific HUD Updates
        void UpdateHealthBar(int currentHealth, int maxHealth);
        void UpdateAmmoCount(int currentAmmo, int totalAmmo);
        void UpdateWeaponInfo(string weaponName, string fireMode);
        void UpdateScore(int score);
        void UpdateKillCount(int kills, int deaths);
        void UpdateTimer(float timeInSeconds);

        // Crosshair & Aiming UI
        void ShowCrosshair();
        void HideCrosshair();
        void SetCrosshairStyle(CrosshairStyle style);
        void UpdateCrosshairAccuracy(float accuracy);

        // Hit Feedback UI
        void ShowHitMarker(bool isHeadshot = false);
        void ShowDamageNumber(float damage, Vector3 worldPosition);
        void FlashDamageIndicator(Vector3 damageDirection);

        // Weapon & Reload UI
        void ShowReloadIndicator();
        void HideReloadIndicator();
        void UpdateReloadProgress(float progress);
        void ShowWeaponSwitchIndicator(string weaponName);

        // Objective & Alert UI
        void ShowObjectiveUpdate(string objective);
        void ShowAlertMessage(string message, AlertType alertType);
        void UpdateObjectiveProgress(string objective, float progress);

        // Debug & Diagnostics
        void ShowUIDebugInfo();
        void ShowPerformanceMetrics(bool show);

        // Events - Core UI
        event Action OnGameUIShown;
        event Action OnGameUIHidden;
        event Action OnPauseMenuShown;
        event Action OnPauseMenuHidden;
        event Action OnGameOverShown;
        event Action OnVictoryShown;

        // Events - HUD Updates
        event Action<int, int> OnHealthChanged;
        event Action<int, int> OnAmmoChanged;
        event Action<string, string> OnWeaponChanged;
        event Action<int> OnScoreChanged;
        event Action<int, int> OnKillCountChanged;

        // Events - Button Actions
        event Action OnResumeRequested;
        event Action OnRestartRequested;
        event Action OnMainMenuRequested;
        event Action OnSettingsRequested;
        event Action OnQuitRequested;
    }

    /// <summary>
    /// クロスヘアスタイル定義
    /// </summary>
    public enum CrosshairStyle
    {
        Default,        // デフォルトクロスヘア
        Sniper,         // スナイパー用精密照準
        Shotgun,        // ショットガン用拡散表示
        AssaultRifle,   // アサルトライフル用
        Pistol,         // ピストル用
        Hidden          // 非表示
    }

    /// <summary>
    /// アラートタイプ定義
    /// </summary>
    public enum AlertType
    {
        Info,           // 情報メッセージ
        Warning,        // 警告メッセージ
        Danger,         // 危険メッセージ
        Success,        // 成功メッセージ
        Objective       // 目標関連メッセージ
    }
}
