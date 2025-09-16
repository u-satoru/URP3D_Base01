using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 武器管理サービス（ServiceLocator経由アクセス）
    /// FPS Template専用武器システムの核心インターフェース
    /// </summary>
    public interface IWeaponManager
    {
        IWeaponData CurrentWeapon { get; }
        bool CanShoot { get; }
        bool CanReload { get; }

        void SwitchWeapon(int weaponIndex);
        void StartReload();
        void Fire(Vector3 origin, Vector3 direction);
        void InitializeWeapons(IWeaponData[] weapons);

        event Action<IWeaponData> OnWeaponSwitched;
        event Action<int> OnAmmoChanged;
    }

    /// <summary>
    /// 武器データインターフェース
    /// ScriptableObjectベースの武器データアセット統合用
    /// </summary>
    public interface IWeaponData
    {
        string WeaponName { get; }
        WeaponType Type { get; }
        int WeaponIndex { get; }

        int MaxAmmo { get; }
        int ReserveAmmo { get; }
        int CurrentAmmo { get; }
        float ReloadTime { get; }

        float Damage { get; }
        float FireRate { get; }
        float Range { get; }
        float Accuracy { get; }

        AudioClip FireSound { get; }
        AudioClip ReloadSound { get; }
        AudioClip EmptySound { get; }

        GameObject MuzzleFlashPrefab { get; }
        GameObject ImpactEffectPrefab { get; }

        void SetAmmo(int ammo);
        void FireWeapon(Vector3 origin, Vector3 direction);
    }

    /// <summary>
    /// 武器タイプ定義
    /// </summary>
    public enum WeaponType
    {
        AssaultRifle,
        Pistol,
        Shotgun,
        Sniper,
        SMG,
        LMG,
        Melee
    }
}