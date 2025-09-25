namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// ゲームジャンルの種類を定義するenum
    /// TASK-004: Ultimate Template Phase-1統合で使用される7ジャンル対応
    /// </summary>
    public enum GenreType
    {
        /// <summary>ステルスアクション（最優先）</summary>
        Stealth = 0,
        
        /// <summary>3Dプラットフォーマー（高優先）</summary>
        Platformer = 1,
        
        /// <summary>一人称視点シューター（高優先）</summary>
        FPS = 2,
        
        /// <summary>三人称視点シューター（高優先）</summary>
        TPS = 3,
        
        /// <summary>アクションRPG（中優先）</summary>
        ActionRPG = 4,
        
        /// <summary>アドベンチャーゲーム</summary>
        Adventure = 5,
        
        /// <summary>リアルタイム戦略ゲーム</summary>
        Strategy = 6
    }
    
    /// <summary>
    /// ジャンル優先度レベル（REQUIREMENTS.md優先度準拠）
    /// </summary>
    public enum GenrePriority
    {
        /// <summary>最優先ジャンル</summary>
        Highest = 0,
        
        /// <summary>高優先度ジャンル</summary>
        High = 1,
        
        /// <summary>中優先度ジャンル</summary>
        Medium = 2,
        
        /// <summary>標準優先度ジャンル</summary>
        Normal = 3
    }
    
    /// <summary>
    /// ジャンル関連のユーティリティ機能
    /// </summary>
    public static class GenreUtilities
    {
        /// <summary>
        /// ジャンルの優先度を取得
        /// </summary>
        /// <param name="genreType">ジャンルの種類</param>
        /// <returns>優先度レベル</returns>
        public static GenrePriority GetPriority(GenreType genreType)
        {
            return genreType switch
            {
                GenreType.Stealth => GenrePriority.Highest,
                GenreType.Platformer => GenrePriority.High,
                GenreType.FPS => GenrePriority.High,
                GenreType.TPS => GenrePriority.High,
                GenreType.ActionRPG => GenrePriority.Medium,
                GenreType.Adventure => GenrePriority.Normal,
                GenreType.Strategy => GenrePriority.Normal,
                _ => GenrePriority.Normal
            };
        }
        
        /// <summary>
        /// ジャンルの日本語表示名を取得
        /// </summary>
        /// <param name="genreType">ジャンルの種類</param>
        /// <returns>日本語表示名</returns>
        public static string GetDisplayName(GenreType genreType)
        {
            return genreType switch
            {
                GenreType.Stealth => "ステルスアクション",
                GenreType.Platformer => "3Dプラットフォーマー",
                GenreType.FPS => "一人称シューティング",
                GenreType.TPS => "三人称シューティング",
                GenreType.ActionRPG => "アクションRPG",
                GenreType.Adventure => "アドベンチャー",
                GenreType.Strategy => "リアルタイム戦略",
                _ => genreType.ToString()
            };
        }
        
        /// <summary>
        /// 学習時間の目安を取得（分）
        /// Learn & Grow価値実現：70%削減効果込み
        /// </summary>
        /// <param name="genreType">ジャンルの種類</param>
        /// <returns>学習時間（分）</returns>
        public static int GetEstimatedLearningTimeMinutes(GenreType genreType)
        {
            return genreType switch
            {
                GenreType.Stealth => 180,      // 3時間（ベースライン）
                GenreType.Platformer => 120,   // 2時間
                GenreType.FPS => 150,          // 2.5時間
                GenreType.TPS => 150,          // 2.5時間
                GenreType.ActionRPG => 240,    // 4時間（複雑なため）
                GenreType.Adventure => 90,     // 1.5時間
                GenreType.Strategy => 200,     // 3.3時間（複雑なため）
                _ => 120
            };
        }
    }
}
