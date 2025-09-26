namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// 環境タイプ - オーディオシステムで使用される環境設定
    /// </summary>
    public enum EnvironmentType
    {
        /// <summary>屋外環境</summary>
        Outdoor,

        /// <summary>屋内環境</summary>
        Indoor,

        /// <summary>洞窟環境</summary>
        Cave,

        /// <summary>森林環境</summary>
        Forest,

        /// <summary>水中環境</summary>
        Underwater,

        /// <summary>都市環境</summary>
        Urban,

        /// <summary>セーフルーム（サバイバルホラー用）</summary>
        SafeRoom,

        /// <summary>廊下（サバイバルホラー用）</summary>
        Corridor,

        /// <summary>暗い部屋（サバイバルホラー用）</summary>
        DarkRoom,

        /// <summary>地下室/ダンジョン（サバイバルホラー用）</summary>
        BasementDungeon
    }

    /// <summary>
    /// 天候タイプ - 環境音響に影響を与える天候設定
    /// </summary>
    public enum WeatherType
    {
        /// <summary>晴天</summary>
        Clear,

        /// <summary>曇り</summary>
        Cloudy,

        /// <summary>雨天</summary>
        Rain,

        /// <summary>雷雨</summary>
        Thunderstorm,

        /// <summary>雪</summary>
        Snow,

        /// <summary>霧</summary>
        Fog,

        /// <summary>嵐</summary>
        Storm,

        /// <summary>風が強い</summary>
        Windy
    }

    /// <summary>
    /// 時間帯 - 環境音響とBGMに影響を与える時刻設定
    /// </summary>
    public enum TimeOfDay
    {
        /// <summary>早朝 (5:00 - 8:00)</summary>
        Dawn,

        /// <summary>朝 (8:00 - 12:00)</summary>
        Morning,

        /// <summary>日中 (12:00 - 17:00)</summary>
        Day,

        /// <summary>夕方 (17:00 - 19:00)</summary>
        Evening,

        /// <summary>夕暮れ (19:00 - 21:00)</summary>
        Dusk,

        /// <summary>夜 (21:00 - 5:00)</summary>
        Night
    }

    /// <summary>
    /// ゲーム状態 - BGMと効果音の制御に使用
    /// </summary>
    public enum GameState
    {
        /// <summary>メインメニュー</summary>
        MainMenu,

        /// <summary>ゲームプレイ中</summary>
        Gameplay,

        /// <summary>一時停止中</summary>
        Paused,

        /// <summary>カットシーン再生中</summary>
        Cutscene,

        /// <summary>ロード中</summary>
        Loading,

        /// <summary>ゲームオーバー</summary>
        GameOver,

        /// <summary>勝利/クリア</summary>
        Victory,

        /// <summary>インベントリ画面</summary>
        Inventory,

        /// <summary>会話中</summary>
        Dialogue,

        /// <summary>戦闘中</summary>
        Combat,

        /// <summary>探索中</summary>
        Exploration
    }

    /// <summary>
    /// オーディオ環境タイプ（プレイヤー状態用）
    /// </summary>
    public enum AudioEnvironmentType
    {
        /// <summary>地面にいる状態</summary>
        Grounded,

        /// <summary>空中にいる状態</summary>
        Airborne,

        /// <summary>水中にいる状態</summary>
        Swimming,

        /// <summary>しゃがみ状態</summary>
        Crouched,

        /// <summary>スプリント中</summary>
        Sprinting
    }

    /// <summary>
    /// 雰囲気状態（サバイバルホラー用）
    /// </summary>
    public enum AtmosphereState
    {
        /// <summary>通常状態</summary>
        Normal,

        /// <summary>緊張状態</summary>
        Tense,

        /// <summary>恐怖状態</summary>
        Fear,

        /// <summary>極度の恐怖状態</summary>
        Terror,

        /// <summary>パニック状態</summary>
        Panic
    }

    /// <summary>
    /// BGM状態
    /// </summary>
    public enum BGMState
    {
        /// <summary>停止中</summary>
        Stopped,

        /// <summary>再生中</summary>
        Playing,

        /// <summary>一時停止中</summary>
        Paused,

        /// <summary>フェードイン中</summary>
        FadingIn,

        /// <summary>フェードアウト中</summary>
        FadingOut,

        /// <summary>クロスフェード中</summary>
        CrossFading
    }
}