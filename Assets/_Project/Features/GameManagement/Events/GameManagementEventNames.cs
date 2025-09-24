namespace asterivo.Unity60.Features.GameManagement.Events
{
    /// <summary>
    /// GameManagement機能で使用されるイベント名の定義
    /// タイポを防ぎ、イベント名の一元管理を実現
    /// </summary>
    public static class GameManagementEventNames
    {
        // ゲーム状態変更イベント
        public const string OnGameStateChanged = "Game_StateChanged";
        public const string OnGameStateChanging = "Game_StateChanging";
        public const string OnGameStarted = "Game_Started";
        public const string OnGamePaused = "Game_Paused";
        public const string OnGameResumed = "Game_Resumed";
        public const string OnGameOver = "Game_Over";
        public const string OnGameVictory = "Game_Victory";
        public const string OnGameRestarted = "Game_Restarted";

        // メニュー関連イベント
        public const string OnReturnToMainMenu = "Game_ReturnToMainMenu";
        public const string OnMenuOpened = "Game_MenuOpened";
        public const string OnMenuClosed = "Game_MenuClosed";

        // ローディングイベント
        public const string OnLoadingStarted = "Game_LoadingStarted";
        public const string OnLoadingProgress = "Game_LoadingProgress";
        public const string OnLoadingCompleted = "Game_LoadingCompleted";

        // コマンドシステムイベント
        public const string OnCommandExecuted = "Game_CommandExecuted";
        public const string OnCommandUndone = "Game_CommandUndone";
        public const string OnCommandRedone = "Game_CommandRedone";
        public const string OnCommandStackCleared = "Game_CommandStackCleared";

        // 時間管理イベント
        public const string OnGameTimeUpdated = "Game_TimeUpdated";
        public const string OnTimeLimitWarning = "Game_TimeLimitWarning";
        public const string OnTimeLimitExpired = "Game_TimeLimitExpired";

        // データ更新イベント
        public const string OnGameDataUpdated = "Game_DataUpdated";
        public const string OnPlayerDataUpdated = "Game_PlayerDataUpdated";
        public const string OnSettingsChanged = "Game_SettingsChanged";

        // エラー・警告イベント
        public const string OnErrorOccurred = "Game_ErrorOccurred";
        public const string OnWarningRaised = "Game_WarningRaised";

        // 初期化・終了イベント
        public const string OnGameManagerInitialized = "GameManager_Initialized";
        public const string OnGameManagerShutdown = "GameManager_Shutdown";
    }
}