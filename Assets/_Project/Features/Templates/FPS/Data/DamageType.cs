using System;

namespace asterivo.Unity60.Features.Templates.FPS.Data
{
    /// <summary>
    /// ダメージタイプ定義
    /// アーキテクチャ準拠: ScriptableObjectベースのデータ管理
    /// </summary>
    [Serializable]
    public enum DamageType
    {
        /// <summary>
        /// 物理ダメージ (銃弾、爆発など)
        /// </summary>
        Physical = 0,

        /// <summary>
        /// 炎ダメージ (火炎放射器、焼夷弾など)
        /// </summary>
        Fire = 1,

        /// <summary>
        /// 電気ダメージ (スタンガン、電撃など)
        /// </summary>
        Electric = 2,

        /// <summary>
        /// 毒ダメージ (毒ガス、毒針など)
        /// </summary>
        Poison = 3,

        /// <summary>
        /// 氷ダメージ (凍結攻撃など)
        /// </summary>
        Ice = 4,

        /// <summary>
        /// 爆発ダメージ (グレネード、RPGなど)
        /// </summary>
        Explosive = 5,

        /// <summary>
        /// 近接ダメージ (ナイフ、殴打など)
        /// </summary>
        Melee = 6,

        /// <summary>
        /// 狙撃ダメージ (スナイパーライフル専用)
        /// </summary>
        Sniper = 7,

        /// <summary>
        /// 環境ダメージ (落下、圧死など)
        /// </summary>
        Environmental = 8,

        /// <summary>
        /// 真ダメージ (防御力無視)
        /// </summary>
        True = 9
    }

    /// <summary>
    /// DamageType拡張メソッド
    /// </summary>
    public static class DamageTypeExtensions
    {
        /// <summary>
        /// ダメージタイプの説明を取得
        /// </summary>
        public static string GetDescription(this DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => "物理的な攻撃によるダメージ",
                DamageType.Fire => "火炎による継続ダメージ",
                DamageType.Electric => "電撃による麻痺効果付きダメージ",
                DamageType.Poison => "毒による継続ダメージ",
                DamageType.Ice => "凍結による移動速度低下効果付きダメージ",
                DamageType.Explosive => "爆発による範囲ダメージ",
                DamageType.Melee => "近接武器による物理ダメージ",
                DamageType.Sniper => "高威力の貫通ダメージ",
                DamageType.Environmental => "環境による致命的ダメージ",
                DamageType.True => "防御力を無視する真ダメージ",
                _ => "不明なダメージタイプ"
            };
        }

        /// <summary>
        /// ダメージタイプのダメージ倍率を取得
        /// </summary>
        public static float GetDamageMultiplier(this DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => 1.0f,
                DamageType.Fire => 0.8f,
                DamageType.Electric => 0.9f,
                DamageType.Poison => 0.6f,
                DamageType.Ice => 0.7f,
                DamageType.Explosive => 1.5f,
                DamageType.Melee => 1.2f,
                DamageType.Sniper => 2.0f,
                DamageType.Environmental => 3.0f,
                DamageType.True => 1.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// ダメージタイプがDOT（継続ダメージ）効果を持つか判定
        /// </summary>
        public static bool HasDotEffect(this DamageType damageType)
        {
            return damageType == DamageType.Fire ||
                   damageType == DamageType.Poison;
        }

        /// <summary>
        /// ダメージタイプが状態異常効果を持つか判定
        /// </summary>
        public static bool HasStatusEffect(this DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Electric => true,  // 麻痺効果
                DamageType.Ice => true,       // 凍結効果
                DamageType.Poison => true,    // 毒効果
                _ => false
            };
        }

        /// <summary>
        /// ダメージタイプが防御力を無視するか判定
        /// </summary>
        public static bool IgnoresArmor(this DamageType damageType)
        {
            return damageType == DamageType.True ||
                   damageType == DamageType.Environmental;
        }

        /// <summary>
        /// ダメージタイプのUIカラーを取得
        /// </summary>
        public static UnityEngine.Color GetUIColor(this DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Physical => UnityEngine.Color.white,
                DamageType.Fire => UnityEngine.Color.red,
                DamageType.Electric => UnityEngine.Color.yellow,
                DamageType.Poison => UnityEngine.Color.green,
                DamageType.Ice => UnityEngine.Color.cyan,
                DamageType.Explosive => UnityEngine.Color.magenta,
                DamageType.Melee => new UnityEngine.Color(0.6f, 0.3f, 0f),
                DamageType.Sniper => new UnityEngine.Color(0.8f, 0.8f, 0.8f),
                DamageType.Environmental => new UnityEngine.Color(0.5f, 0.2f, 0.2f),
                DamageType.True => new UnityEngine.Color(1f, 0f, 1f),
                _ => UnityEngine.Color.gray
            };
        }
    }
}