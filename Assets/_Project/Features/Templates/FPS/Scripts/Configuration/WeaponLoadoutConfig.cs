using UnityEngine;
using asterivo.Unity60.Features.Templates.FPS.Data;
using System.Collections.Generic;
using System.Linq;

namespace asterivo.Unity60.Features.Templates.FPS.Configuration
{
    /// <summary>
    /// 武器ロードアウト設定
    /// 新規武器の追加が容易になる設計
    /// データ駆動設計によりデザイナーがコード非依存で武器構成を調整可能
    /// </summary>
    [CreateAssetMenu(menuName = "FPS/Configuration/Weapon Loadout Config")]
    public class WeaponLoadoutConfig : ScriptableObject
    {
        [Header("Primary Weapons")]
        [SerializeField] private WeaponData[] _primaryWeapons;

        [Header("Secondary Weapons")]
        [SerializeField] private WeaponData[] _secondaryWeapons;

        [Header("Default Loadout")]
        [SerializeField] private WeaponData _defaultPrimary;
        [SerializeField] private WeaponData _defaultSecondary;

        [Header("Loadout Constraints")]
        [SerializeField] private int _maxPrimarySlots = 2;
        [SerializeField] private int _maxSecondarySlots = 1;
        [SerializeField] private bool _allowDuplicateWeapons = false;

        // Properties
        public WeaponData[] PrimaryWeapons => _primaryWeapons;
        public WeaponData[] SecondaryWeapons => _secondaryWeapons;
        public WeaponData DefaultPrimary => _defaultPrimary;
        public WeaponData DefaultSecondary => _defaultSecondary;
        public int MaxPrimarySlots => _maxPrimarySlots;
        public int MaxSecondarySlots => _maxSecondarySlots;
        public bool AllowDuplicateWeapons => _allowDuplicateWeapons;

        /// <summary>
        /// 全武器リストの取得
        /// </summary>
        public WeaponData[] GetAllWeapons()
        {
            var allWeapons = new List<WeaponData>();
            if (_primaryWeapons != null) allWeapons.AddRange(_primaryWeapons);
            if (_secondaryWeapons != null) allWeapons.AddRange(_secondaryWeapons);
            return allWeapons.Where(w => w != null).ToArray();
        }

        /// <summary>
        /// 武器タイプ別の武器取得
        /// </summary>
        public WeaponData[] GetWeaponsByType(WeaponType weaponType)
        {
            return GetAllWeapons().Where(w => w.WeaponType == weaponType).ToArray();
        }

        /// <summary>
        /// 武器ロードアウトの動的更新
        /// 詳細設計書準拠：新規武器追加が容易になる設計
        /// </summary>
        public void UpdateLoadout(WeaponData[] newWeapons)
        {
            if (newWeapons == null) return;

            var primaries = new List<WeaponData>();
            var secondaries = new List<WeaponData>();

            foreach (var weapon in newWeapons)
            {
                if (weapon == null) continue;

                // 武器タイプに基づく自動分類
                switch (weapon.WeaponType)
                {
                    case WeaponType.AssaultRifle:
                    case WeaponType.SMG:
                    case WeaponType.Sniper:
                    case WeaponType.Shotgun:
                    case WeaponType.LMG:
                        primaries.Add(weapon);
                        break;
                    case WeaponType.Pistol:
                        secondaries.Add(weapon);
                        break;
                }
            }

            _primaryWeapons = primaries.ToArray();
            _secondaryWeapons = secondaries.ToArray();

            // デフォルト武器の自動設定
            if (_defaultPrimary == null && _primaryWeapons.Length > 0)
            {
                _defaultPrimary = _primaryWeapons[0];
            }

            if (_defaultSecondary == null && _secondaryWeapons.Length > 0)
            {
                _defaultSecondary = _secondaryWeapons[0];
            }
        }

        /// <summary>
        /// 武器の有効性検証
        /// </summary>
        public bool ValidateWeapon(WeaponData weapon)
        {
            if (weapon == null) return false;

            // TTK範囲チェック（0.2秒〜10秒の範囲内）
            float ttk = weapon.GetTTK();
            if (ttk < 0.2f || ttk > 10.0f)
            {
                Debug.LogWarning($"武器 '{weapon.WeaponName}' のTTK値 ({ttk:F2}s) が推奨範囲外です。");
                return false;
            }

            // 基本パラメータチェック
            if (weapon.Stats.damage <= 0 || weapon.Stats.fireRate <= 0)
            {
                Debug.LogError($"武器 '{weapon.WeaponName}' に無効なパラメータがあります。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// ロードアウトバランス分析
        /// </summary>
        public LoadoutAnalysis AnalyzeBalance()
        {
            var analysis = new LoadoutAnalysis();
            var allWeapons = GetAllWeapons();

            if (allWeapons.Length == 0)
            {
                analysis.IsBalanced = false;
                analysis.Issues.Add("武器が設定されていません。");
                return analysis;
            }

            // TTK分布分析
            var ttks = allWeapons.Select(w => w.GetTTK()).ToArray();
            analysis.AverageTTK = ttks.Average();
            analysis.TTKRange = ttks.Max() - ttks.Min();

            // バランス判定
            analysis.IsBalanced = analysis.TTKRange <= 2.0f; // TTK差が2秒以内

            if (!analysis.IsBalanced)
            {
                analysis.Issues.Add($"TTK範囲が広すぎます ({analysis.TTKRange:F2}s)。武器バランスの調整が必要です。");
            }

            // 武器タイプ多様性チェック
            var weaponTypes = allWeapons.Select(w => w.WeaponType).Distinct().Count();
            if (weaponTypes < 3)
            {
                analysis.Issues.Add("武器タイプの多様性が不足しています。最低3種類の武器タイプを推奨します。");
            }

            return analysis;
        }

        private void OnValidate()
        {
            // Editor上での自動検証
            if (_primaryWeapons != null)
            {
                foreach (var weapon in _primaryWeapons)
                {
                    if (weapon != null && !ValidateWeapon(weapon))
                    {
                        Debug.LogWarning($"プライマリ武器 '{weapon.WeaponName}' に問題があります。");
                    }
                }
            }

            if (_secondaryWeapons != null)
            {
                foreach (var weapon in _secondaryWeapons)
                {
                    if (weapon != null && !ValidateWeapon(weapon))
                    {
                        Debug.LogWarning($"セカンダリ武器 '{weapon.WeaponName}' に問題があります。");
                    }
                }
            }
        }

        [System.Serializable]
        public class LoadoutAnalysis
        {
            public bool IsBalanced;
            public float AverageTTK;
            public float TTKRange;
            public List<string> Issues = new List<string>();
        }
    }
}