using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// TTK (Time-to-Kill) バランス設定
    /// 詳細設計書準拠：「タクティカルシューター」と「カジュアル（アーケード）シューター」のスペクトラム対応
    /// TTKを中心としたコアメカニクスの調整可能な設計
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/TTK Balance Config")]
    public class TTKBalanceConfig : ScriptableObject
    {
        [Header("TTK Profile Settings")]
        [SerializeField] private TTKProfile _currentProfile = TTKProfile.Balanced;

        [Header("Tactical Profile (Low TTK)")]
        [SerializeField] private TTKSettings _tacticalSettings = TTKSettings.TacticalDefault;

        [Header("Balanced Profile")]
        [SerializeField] private TTKSettings _balancedSettings = TTKSettings.BalancedDefault;

        [Header("Casual Profile (High TTK)")]
        [SerializeField] private TTKSettings _casualSettings = TTKSettings.CasualDefault;

        [Header("Custom Profile")]
        [SerializeField] private TTKSettings _customSettings = TTKSettings.BalancedDefault;

        [Header("Global Modifiers")]
        [SerializeField] private float _globalDamageMultiplier = 1.0f;
        [SerializeField] private float _globalFireRateMultiplier = 1.0f;
        [SerializeField] private float _globalHealthMultiplier = 1.0f;

        // Properties
        public TTKProfile CurrentProfile => _currentProfile;
        public TTKSettings TacticalSettings => _tacticalSettings;
        public TTKSettings BalancedSettings => _balancedSettings;
        public TTKSettings CasualSettings => _casualSettings;
        public TTKSettings CustomSettings => _customSettings;
        public float GlobalDamageMultiplier => _globalDamageMultiplier;
        public float GlobalFireRateMultiplier => _globalFireRateMultiplier;
        public float GlobalHealthMultiplier => _globalHealthMultiplier;

        /// <summary>
        /// 現在のTTK設定を取得
        /// </summary>
        public TTKSettings GetCurrentSettings()
        {
            return _currentProfile switch
            {
                TTKProfile.Tactical => _tacticalSettings,
                TTKProfile.Balanced => _balancedSettings,
                TTKProfile.Casual => _casualSettings,
                TTKProfile.Custom => _customSettings,
                _ => _balancedSettings
            };
        }

        /// <summary>
        /// TTKプロファイルの適用
        /// 詳細設計書準拠：目指すゲームフィールに合わせた容易なバランス調整
        /// </summary>
        public void ApplyProfile(TTKProfile profile)
        {
            _currentProfile = profile;
            var settings = GetCurrentSettings();

            // グローバル乗数の適用
            _globalDamageMultiplier = settings.DamageMultiplier;
            _globalFireRateMultiplier = settings.FireRateMultiplier;
            _globalHealthMultiplier = settings.HealthMultiplier;

            Debug.Log($"TTKプロファイル '{profile}' を適用しました。" +
                     $"平均TTK: {settings.TargetTTK:F2}s, " +
                     $"ダメージ倍率: {_globalDamageMultiplier:F2}, " +
                     $"体力倍率: {_globalHealthMultiplier:F2}");
        }

        /// <summary>
        /// 武器データにTTKバランスを適用
        /// </summary>
        public WeaponStats ApplyTTKBalance(WeaponStats originalStats)
        {
            var modifiedStats = originalStats;
            modifiedStats.damage *= _globalDamageMultiplier;
            modifiedStats.fireRate *= _globalFireRateMultiplier;
            return modifiedStats;
        }

        /// <summary>
        /// プレイヤー体力にTTKバランスを適用
        /// </summary>
        public float ApplyHealthBalance(float originalHealth)
        {
            return originalHealth * _globalHealthMultiplier;
        }

        /// <summary>
        /// TTKバランスの妥当性検証
        /// </summary>
        public bool ValidateTTKBalance()
        {
            var settings = GetCurrentSettings();
            
            // TTK範囲チェック
            if (settings.TargetTTK < 0.1f || settings.TargetTTK > 10.0f)
            {
                Debug.LogWarning($"TTK目標値 {settings.TargetTTK:F2}s が推奨範囲(0.1-10.0s)外です。");
                return false;
            }

            // 乗数の妥当性チェック
            if (_globalDamageMultiplier <= 0 || _globalFireRateMultiplier <= 0 || _globalHealthMultiplier <= 0)
            {
                Debug.LogError("TTKバランス設定に無効な値があります。乗数は正の値である必要があります。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// カスタムTTK設定の作成
        /// </summary>
        public void CreateCustomProfile(float targetTTK, float damageMultiplier, float healthMultiplier)
        {
            _customSettings = new TTKSettings
            {
                ProfileName = "Custom",
                TargetTTK = targetTTK,
                DamageMultiplier = damageMultiplier,
                FireRateMultiplier = 1.0f,
                HealthMultiplier = healthMultiplier,
                Description = $"カスタム設定 - TTK: {targetTTK:F2}s"
            };

            _currentProfile = TTKProfile.Custom;
            ApplyProfile(TTKProfile.Custom);
        }

        private void OnValidate()
        {
            // Editor上での値制限
            _globalDamageMultiplier = Mathf.Clamp(_globalDamageMultiplier, 0.1f, 5.0f);
            _globalFireRateMultiplier = Mathf.Clamp(_globalFireRateMultiplier, 0.1f, 5.0f);
            _globalHealthMultiplier = Mathf.Clamp(_globalHealthMultiplier, 0.1f, 10.0f);

            ValidateTTKBalance();
        }
    }

    /// <summary>
    /// TTK設定データ構造
    /// </summary>
    [System.Serializable]
    public struct TTKSettings
    {
        [Header("Profile Identity")]
        public string ProfileName;
        [TextArea(2, 3)]
        public string Description;

        [Header("TTK Parameters")]
        [Range(0.1f, 10.0f)]
        public float TargetTTK;           // 目標TTK（秒）
        
        [Header("Balance Multipliers")]
        [Range(0.1f, 5.0f)]
        public float DamageMultiplier;    // ダメージ乗数
        [Range(0.1f, 5.0f)]
        public float FireRateMultiplier;  // 発射レート乗数
        [Range(0.1f, 10.0f)]
        public float HealthMultiplier;    // 体力乗数

        public static TTKSettings TacticalDefault => new TTKSettings
        {
            ProfileName = "Tactical",
            Description = "低TTKのタクティカルシューター。戦術的プレイと精密性を重視。",
            TargetTTK = 0.5f,
            DamageMultiplier = 1.5f,
            FireRateMultiplier = 1.0f,
            HealthMultiplier = 0.8f
        };

        public static TTKSettings BalancedDefault => new TTKSettings
        {
            ProfileName = "Balanced",
            Description = "バランス型。適度なTTKで幅広いプレイスタイルに対応。",
            TargetTTK = 1.0f,
            DamageMultiplier = 1.0f,
            FireRateMultiplier = 1.0f,
            HealthMultiplier = 1.0f
        };

        public static TTKSettings CasualDefault => new TTKSettings
        {
            ProfileName = "Casual",
            Description = "高TTKのカジュアルシューター。長期戦と機動性を重視。",
            TargetTTK = 2.0f,
            DamageMultiplier = 0.7f,
            FireRateMultiplier = 1.2f,
            HealthMultiplier = 1.5f
        };
    }
}