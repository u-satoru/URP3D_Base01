using System;

namespace asterivo.Unity60.Core.Templates
{
    /// <summary>
    /// ゲームジャンルタイプの定義
    /// TASK-004.2 7ジャンルテンプレート完全実装に対応
    /// </summary>
    [Serializable]
    public enum GenreType
    {
        /// <summary>ステルスアクションゲーム（最優先）</summary>
        Stealth = 0,
        
        /// <summary>3Dプラットフォーマー（高優先）</summary>
        Platformer = 1,
        
        /// <summary>一人称シューティング（高優先）</summary>
        FPS = 2,
        
        /// <summary>三人称シューティング（高優先）</summary>
        TPS = 3,
        
        /// <summary>アクションRPG（中優先）</summary>
        ActionRPG = 4,
        
        /// <summary>アドベンチャー</summary>
        Adventure = 5,
        
        /// <summary>ストラテジー</summary>
        Strategy = 6
    }
    
    /// <summary>
    /// GenreType拡張メソッド
    /// </summary>
    public static class GenreTypeExtensions
    {
        /// <summary>
        /// ジャンルの表示名を取得
        /// </summary>
        public static string GetDisplayName(this GenreType genreType)
        {
            return genreType switch
            {
                GenreType.Stealth => "Stealth Action",
                GenreType.Platformer => "3D Platformer",
                GenreType.FPS => "First Person Shooter",
                GenreType.TPS => "Third Person Shooter",
                GenreType.ActionRPG => "Action RPG",
                GenreType.Adventure => "Adventure",
                GenreType.Strategy => "Strategy",
                _ => genreType.ToString()
            };
        }
        
        /// <summary>
        /// ジャンルの優先度を取得（REQUIREMENTS.md準拠）
        /// </summary>
        public static int GetPriority(this GenreType genreType)
        {
            return genreType switch
            {
                GenreType.Stealth => 1,      // 最優先
                GenreType.Platformer => 2,   // 高優先
                GenreType.FPS => 2,          // 高優先
                GenreType.TPS => 2,          // 高優先
                GenreType.ActionRPG => 3,    // 中優先
                GenreType.Adventure => 4,    // 対応可能
                GenreType.Strategy => 4,     // 対応可能
                _ => 5
            };
        }
        
        /// <summary>
        /// ジャンルの名前空間プレフィックスを取得
        /// </summary>
        public static string GetNamespacePrefix(this GenreType genreType)
        {
            return $"asterivo.Unity60.Features.Templates.{genreType}";
        }
    }
}
