using System;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// 弾薬管理サービス（ServiceLocator経由アクセス）
    /// FPS Template専用弾薬システム
    /// </summary>
    public interface IAmmoManager
    {
        // 弾薬状態
        int GetCurrentAmmo(WeaponType weaponType);
        int GetMaxAmmo(WeaponType weaponType);
        int GetReserveAmmo(WeaponType weaponType);
        bool CanReload(WeaponType weaponType);

        // 弾薬操作
        void ConsumeAmmo(WeaponType weaponType, int amount = 1);
        void Reload(WeaponType weaponType);
        void AddAmmo(WeaponType weaponType, int amount);
        void SetAmmo(WeaponType weaponType, int current, int reserve);

        // 弾薬設定
        void InitializeAmmo(AmmoConfiguration[] configurations);
        void ResetAllAmmo();

        // 弾薬イベント
        event Action<WeaponType, int, int> OnAmmoChanged; // weaponType, current, reserve
        event Action<WeaponType> OnAmmoEmpty;
        event Action<WeaponType> OnReloadStarted;
        event Action<WeaponType> OnReloadCompleted;
    }

    /// <summary>
    /// 弾薬設定データ
    /// </summary>
    [System.Serializable]
    public class AmmoConfiguration
    {
        public WeaponType weaponType;
        public int maxAmmo;
        public int startingAmmo;
        public int maxReserveAmmo;
        public int startingReserveAmmo;
        public float reloadTime;
    }
}