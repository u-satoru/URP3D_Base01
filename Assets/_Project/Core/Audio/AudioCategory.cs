using UnityEngine;

namespace asterivo.Unity60.Core.Audio
{
    /// <summary>
    /// オーディオカテゴリの定義
    /// ステルスオーディオシステムで使用する音声カテゴリの列挙
    /// 
    /// 各カテゴリの用途:
    /// - BGM: バックグラウンドミュージック
    /// - Ambient: 環境音（風、雨、鳥の声など）
    /// - Effect: 効果音（足音、ドアの音など）
    /// - SFX: サウンドエフェクト（UI音、魔法音など）
    /// - Stealth: ステルス関連音（隠れる音、発見音など）
    /// - UI: ユーザーインターフェースの操作音
    /// </summary>
    public enum AudioCategory
    {
        /// <summary>
        /// バックグラウンドミュージック
        /// ゲーム全体の雰囲気を作るメインBGM
        /// </summary>
        BGM,
        
        /// <summary>
        /// 環境音・アンビエント
        /// 自然環境や空間の音（風、雨、鳥の声、機械音など）
        /// </summary>
        Ambient,
        
        /// <summary>
        /// 効果音・インタラクション音
        /// プレイヤーの行動に対する直接的な音響フィードバック
        /// </summary>
        Effect,
        
        /// <summary>
        /// サウンドエフェクト・UI音
        /// ユーザーインターフェースや特殊効果の音
        /// </summary>
        SFX,
        
        /// <summary>
        /// UI音
        /// ユーザーインターフェースの操作音
        /// </summary>
        UI,
        
        /// <summary>
        /// ステルス関連音
        /// ステルスゲームプレイに特化した音響効果
        /// 隠れる音、発見音、警戒レベル変化音など
        /// </summary>
        Stealth
    }
    
    /// <summary>
    /// AudioCategoryの拡張メソッド
    /// カテゴリに関連するユーティリティ機能を提供
    /// </summary>
    public static class AudioCategoryExtensions
    {
        /// <summary>
        /// オーディオカテゴリの表示名を取得
        /// UI表示やデバッグログで使用
        /// </summary>
        /// <param name="category">対象のオーディオカテゴリ</param>
        /// <returns>表示用の日本語名</returns>
        public static string GetDisplayName(this AudioCategory category)
        {
            return category switch
            {
                AudioCategory.BGM => "BGM",
                AudioCategory.Ambient => "環境音",
                AudioCategory.Effect => "効果音",
                AudioCategory.SFX => "サウンドエフェクト",
                AudioCategory.Stealth => "ステルス音",
                AudioCategory.UI => "UI音",
                _ => "未定義"
            };
        }
        
        /// <summary>
        /// デフォルトの音量レベルを取得
        /// カテゴリごとの適切な初期音量
        /// </summary>
        /// <param name="category">対象のオーディオカテゴリ</param>
        /// <returns>0.0～1.0の範囲の音量値</returns>
        public static float GetDefaultVolume(this AudioCategory category)
        {
            return category switch
            {
                AudioCategory.BGM => 0.7f,
                AudioCategory.Ambient => 0.5f,
                AudioCategory.Effect => 0.8f,
                AudioCategory.SFX => 0.9f,
                AudioCategory.UI => 0.9f,
                AudioCategory.Stealth => 0.6f,
                _ => 0.5f
            };
        }
        
        /// <summary>
        /// カテゴリが3D空間音響を使用するかを判定
        /// </summary>
        /// <param name="category">対象のオーディオカテゴリ</param>
        /// <returns>3D音響を使用する場合はtrue</returns>
        public static bool UsesSpatialAudio(this AudioCategory category)
        {
            return category switch
            {
                AudioCategory.BGM => false,      // BGMは通常2D
                AudioCategory.Ambient => true,   // 環境音は3D
                AudioCategory.Effect => true,    // 効果音は3D
                AudioCategory.SFX => false,      // UIサウンドは2D
                AudioCategory.UI => false,       // UI音は2D
                AudioCategory.Stealth => true,   // ステルス音は3D
                _ => false
            };
        }
    }
}