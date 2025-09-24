using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// 隠蔽ゾーンの基本インターフェース
    /// 様々な隠蔽メカニズムに共通の契約を定義
    /// </summary>
    public interface IConcealmentZone
    {
        /// <summary>
        /// 隠蔽強度（0.0 = 隠蔽なし, 1.0 = 完全隠蔽）
        /// </summary>
        float ConcealmentStrength { get; }

        /// <summary>
        /// 隠蔽ゾーンの種類
        /// </summary>
        ConcealmentType ZoneType { get; }

        /// <summary>
        /// ゾーンが現在アクティブかどうか
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// 隠蔽の種類
    /// 異なる隠蔽メカニズムによる効果の差別化
    /// </summary>
    public enum ConcealmentType
    {
        // 自然環境
        TallGrass,          // 背の高い草
        Bush,               // 茂み
        Tree,               // 木陰
        Rock,               // 岩陰
        WaterReeds,         // 水辺の葦

        // 人工構造物
        Locker,             // ロッカー
        Container,          // コンテナ
        Dumpster,           // ダンプスター
        Crate,              // 木箱
        Barrel,             // バレル

        // 建築環境
        Doorway,            // 扉の陰
        Pillar,             // 柱
        Corner,             // 角
        Alcove,             // 窪み
        UnderStairs,        // 階段下

        // 光学環境
        Shadow,             // 影
        Darkness,           // 暗闇
        Fog,                // 霧
        Smoke,              // 煙

        // 動的環境
        MovingCrowd,        // 人混み
        VehicleShadow,      // 車両の陰
        Ventilation,        // 換気口
        Ceiling,            // 天井

        // 特殊環境
        WaterSubmersion,    // 水中
        SnowDrift,          // 雪だまり
        SandDune,           // 砂丘
        Custom              // カスタム（スクリプト制御）
    }

    /// <summary>
    /// 隠蔽効果の詳細情報
    /// </summary>
    [System.Serializable]
    public struct ConcealmentEffect
    {
        [Header("Visibility Reduction")]
        [Range(0.0f, 1.0f)]
        public float VisibilityReduction;

        [Header("Noise Dampening")]
        [Range(0.0f, 1.0f)]
        public float NoiseDampening;

        [Header("Movement Restriction")]
        [Range(0.0f, 1.0f)]
        public float MovementSpeedMultiplier;

        [Header("Detection Immunity")]
        public bool ImmuneToVisualDetection;
        public bool ImmuneToAudioDetection;
        public bool ImmuneToThermalDetection;
        public bool ImmuneToMotionDetection;

        [Header("Duration")]
        public float MaxConcealmentTime; // 0 = unlimited
        public float EntryTime;
        public float ExitTime;

        /// <summary>
        /// デフォルト隠蔽効果
        /// </summary>
        public static ConcealmentEffect Default => new ConcealmentEffect
        {
            VisibilityReduction = 0.7f,
            NoiseDampening = 0.5f,
            MovementSpeedMultiplier = 1.0f,
            ImmuneToVisualDetection = false,
            ImmuneToAudioDetection = false,
            ImmuneToThermalDetection = false,
            ImmuneToMotionDetection = false,
            MaxConcealmentTime = 0.0f,
            EntryTime = 0.5f,
            ExitTime = 0.5f
        };

        /// <summary>
        /// 完全隠蔽効果（ロッカーなど）
        /// </summary>
        public static ConcealmentEffect Perfect => new ConcealmentEffect
        {
            VisibilityReduction = 1.0f,
            NoiseDampening = 1.0f,
            MovementSpeedMultiplier = 0.0f,
            ImmuneToVisualDetection = true,
            ImmuneToAudioDetection = true,
            ImmuneToThermalDetection = true,
            ImmuneToMotionDetection = true,
            MaxConcealmentTime = 0.0f,
            EntryTime = 1.0f,
            ExitTime = 1.0f
        };

        /// <summary>
        /// 部分隠蔽効果（草むらなど）
        /// </summary>
        public static ConcealmentEffect Partial => new ConcealmentEffect
        {
            VisibilityReduction = 0.5f,
            NoiseDampening = 0.3f,
            MovementSpeedMultiplier = 0.8f,
            ImmuneToVisualDetection = false,
            ImmuneToAudioDetection = false,
            ImmuneToThermalDetection = false,
            ImmuneToMotionDetection = false,
            MaxConcealmentTime = 0.0f,
            EntryTime = 0.2f,
            ExitTime = 0.3f
        };
    }

    /// <summary>
    /// 隠蔽ゾーンの品質評価
    /// </summary>
    public enum ConcealmentQuality
    {
        Poor,       // 貧弱（20%以下の隠蔽）
        Fair,       // 普通（21-40%の隠蔽）
        Good,       // 良好（41-60%の隠蔽）
        Excellent,  // 優秀（61-80%の隠蔽）
        Perfect     // 完璧（81-100%の隠蔽）
    }

    /// <summary>
    /// 隠蔽ゾーンユーティリティ
    /// </summary>
    public static class ConcealmentUtility
    {
        /// <summary>
        /// 隠蔽強度から品質を判定
        /// </summary>
        public static ConcealmentQuality GetQualityFromStrength(float strength)
        {
            strength = Mathf.Clamp01(strength);

            if (strength <= 0.2f) return ConcealmentQuality.Poor;
            if (strength <= 0.4f) return ConcealmentQuality.Fair;
            if (strength <= 0.6f) return ConcealmentQuality.Good;
            if (strength <= 0.8f) return ConcealmentQuality.Excellent;
            return ConcealmentQuality.Perfect;
        }

        /// <summary>
        /// 隠蔽タイプのデフォルト効果を取得
        /// </summary>
        public static ConcealmentEffect GetDefaultEffectForType(ConcealmentType type)
        {
            return type switch
            {
                // 完全隠蔽
                ConcealmentType.Locker => ConcealmentEffect.Perfect,
                ConcealmentType.Container => ConcealmentEffect.Perfect,
                ConcealmentType.Dumpster => ConcealmentEffect.Perfect,

                // 高品質隠蔽
                ConcealmentType.Bush => new ConcealmentEffect
                {
                    VisibilityReduction = 0.8f,
                    NoiseDampening = 0.6f,
                    MovementSpeedMultiplier = 0.6f,
                    EntryTime = 0.3f,
                    ExitTime = 0.4f
                },

                ConcealmentType.Shadow => new ConcealmentEffect
                {
                    VisibilityReduction = 0.7f,
                    NoiseDampening = 0.2f,
                    MovementSpeedMultiplier = 0.9f,
                    EntryTime = 0.1f,
                    ExitTime = 0.2f
                },

                ConcealmentType.Darkness => new ConcealmentEffect
                {
                    VisibilityReduction = 0.9f,
                    NoiseDampening = 0.3f,
                    MovementSpeedMultiplier = 0.8f,
                    ImmuneToVisualDetection = true,
                    EntryTime = 0.2f,
                    ExitTime = 0.3f
                },

                // 中品質隠蔽
                ConcealmentType.TallGrass => ConcealmentEffect.Partial,
                ConcealmentType.Rock => ConcealmentEffect.Partial,

                // 低品質隠蔽
                ConcealmentType.Pillar => new ConcealmentEffect
                {
                    VisibilityReduction = 0.4f,
                    NoiseDampening = 0.1f,
                    MovementSpeedMultiplier = 1.0f,
                    EntryTime = 0.1f,
                    ExitTime = 0.1f
                },

                // デフォルト
                _ => ConcealmentEffect.Default
            };
        }

        /// <summary>
        /// 隠蔽タイプの説明を取得
        /// </summary>
        public static string GetDescriptionForType(ConcealmentType type)
        {
            return type switch
            {
                ConcealmentType.TallGrass => "背の高い草による自然な隠蔽",
                ConcealmentType.Bush => "茂みによる中程度の隠蔽",
                ConcealmentType.Tree => "木陰による部分的な隠蔽",
                ConcealmentType.Rock => "岩陰による物理的隠蔽",
                ConcealmentType.WaterReeds => "水辺の葦による隠蔽",

                ConcealmentType.Locker => "ロッカー内での完全隠蔽",
                ConcealmentType.Container => "コンテナ内での完全隠蔽",
                ConcealmentType.Dumpster => "ダンプスター内での完全隠蔽",
                ConcealmentType.Crate => "木箱による隠蔽",
                ConcealmentType.Barrel => "バレルによる隠蔽",

                ConcealmentType.Doorway => "扉の陰による隠蔽",
                ConcealmentType.Pillar => "柱による部分的隠蔽",
                ConcealmentType.Corner => "角による戦術的隠蔽",
                ConcealmentType.Alcove => "窪みによる隠蔽",
                ConcealmentType.UnderStairs => "階段下による隠蔽",

                ConcealmentType.Shadow => "影による光学的隠蔽",
                ConcealmentType.Darkness => "暗闇による高度な隠蔽",
                ConcealmentType.Fog => "霧による視界阻害隠蔽",
                ConcealmentType.Smoke => "煙による一時的隠蔽",

                ConcealmentType.MovingCrowd => "人混みによる動的隠蔽",
                ConcealmentType.VehicleShadow => "車両の陰による隠蔽",
                ConcealmentType.Ventilation => "換気口による隠蔽",
                ConcealmentType.Ceiling => "天井による隠蔽",

                ConcealmentType.WaterSubmersion => "水中による完全隠蔽",
                ConcealmentType.SnowDrift => "雪だまりによる環境隠蔽",
                ConcealmentType.SandDune => "砂丘による自然隠蔽",
                ConcealmentType.Custom => "カスタム隠蔽メカニズム",

                _ => "未定義の隠蔽タイプ"
            };
        }
    }
}