using System;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Platformer UI Service Interface：ユーザーインターフェース・メニュー・HUD管理
    /// ServiceLocator + Event駆動アーキテクチャによる高度なUI管理システム
    /// Learn & Grow価値実現：直感的なUI・ゲーム状態の視覚化・プラットフォーマー特化HUD
    /// </summary>
    public interface IPlatformerUIService : IPlatformerService
    {
        // Core UI Management
        void ShowGameUI();
        void HideGameUI();
        void ShowPauseMenu();
        void HidePauseMenu();
        void ShowGameOverScreen();
        void ShowLevelCompleteScreen();
        void HideAllScreens();

        // HUD Updates
        void UpdateHealthBar(int currentHealth, int maxHealth);
        void UpdateScore(int score);
        void UpdateLives(int lives);
        void UpdateTimer(float timeInSeconds);

        // Debug & Diagnostics
        void ShowUIDebugInfo();

        // Events
        event Action OnGameUIShown;
        event Action OnGameUIHidden;
        event Action OnPauseMenuShown;
        event Action OnPauseMenuHidden;
        event Action OnGameOverShown;
        event Action OnLevelCompleteShown;
        event Action<int, int> OnHealthChanged;
        event Action<int> OnScoreChanged;
        event Action<int> OnLivesChanged;

        // Button Events
        event Action OnResumeRequested;
        event Action OnRestartRequested;
        event Action OnMainMenuRequested;
        event Action OnNextLevelRequested;
    }
}
