namespace asterivo.Unity60.Core.Types
{
    /// <summary>
    /// ゲーム状態の定義
    /// プロジェクト全体で使用される標準的なゲーム状態を定義
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// メインメニュー状態
        /// </summary>
        MainMenu,

        /// <summary>
        /// ローディング中
        /// </summary>
        Loading,

        /// <summary>
        /// ゲームプレイ中（実際のゲーム進行中）
        /// </summary>
        Gameplay,

        /// <summary>
        /// プレイ中（Gameplayのエイリアス、後方互換性のため）
        /// </summary>
        Playing = Gameplay,

        /// <summary>
        /// 一時停止中
        /// </summary>
        Paused,

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        GameOver,

        /// <summary>
        /// 勝利/クリア
        /// </summary>
        Victory,

        /// <summary>
        /// カットシーン再生中
        /// </summary>
        Cutscene,

        /// <summary>
        /// ゲーム内（Gameplayのエイリアス、オーディオシステム用）
        /// </summary>
        InGame = Gameplay
    }
}
