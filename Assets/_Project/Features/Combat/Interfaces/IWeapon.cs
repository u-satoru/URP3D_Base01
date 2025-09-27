using UnityEngine;

namespace asterivo.Unity60.Features.Combat.Interfaces
{
    /// <summary>
    /// 武器の基本インターフェース
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// 武器ID
        /// </summary>
        string WeaponId { get; }

        /// <summary>
        /// 武器名
        /// </summary>
        string WeaponName { get; }

        /// <summary>
        /// 武器タイプ
        /// </summary>
        WeaponType WeaponType { get; }

        /// <summary>
        /// 基本ダメージ量
        /// </summary>
        float BaseDamage { get; }

        /// <summary>
        /// 攻撃範囲
        /// </summary>
        float Range { get; }

        /// <summary>
        /// 攻撃可能かどうか
        /// </summary>
        bool CanFire { get; }

        /// <summary>
        /// 主攻撃
        /// </summary>
        void Fire();

        /// <summary>
        /// 副攻撃（オプション）
        /// </summary>
        void SecondaryFire();

        /// <summary>
        /// 武器をリロード
        /// </summary>
        void Reload();

        /// <summary>
        /// 武器を装備
        /// </summary>
        void Equip();

        /// <summary>
        /// 武器を収納
        /// </summary>
        void Holster();
    }

    /// <summary>
    /// 弾薬を使用する武器のインターフェース
    /// </summary>
    public interface IAmmoWeapon : IWeapon
    {
        /// <summary>
        /// 現在の弾薬数
        /// </summary>
        int CurrentAmmo { get; }

        /// <summary>
        /// 最大弾薬数
        /// </summary>
        int MaxAmmo { get; }

        /// <summary>
        /// 予備弾薬数
        /// </summary>
        int ReserveAmmo { get; }

        /// <summary>
        /// リロード中かどうか
        /// </summary>
        bool IsReloading { get; }

        /// <summary>
        /// 弾薬を追加
        /// </summary>
        /// <param name="amount">追加する弾薬数</param>
        /// <returns>実際に追加された弾薬数</returns>
        int AddAmmo(int amount);

        /// <summary>
        /// 弾薬を消費
        /// </summary>
        /// <param name="amount">消費する弾薬数</param>
        /// <returns>実際に消費された弾薬数</returns>
        int ConsumeAmmo(int amount);
    }

    /// <summary>
    /// 近接武器のインターフェース
    /// </summary>
    public interface IMeleeWeapon : IWeapon
    {
        /// <summary>
        /// 攻撃角度
        /// </summary>
        float AttackAngle { get; }

        /// <summary>
        /// ノックバック力
        /// </summary>
        float KnockbackForce { get; }

        /// <summary>
        /// コンボ攻撃可能かどうか
        /// </summary>
        bool CanCombo { get; }

        /// <summary>
        /// コンボ攻撃を実行
        /// </summary>
        void ComboAttack();
    }

    /// <summary>
    /// 武器タイプ
    /// </summary>
    public enum WeaponType
    {
        None,
        Pistol,        // ピストル
        Rifle,         // ライフル
        Shotgun,       // ショットガン
        Sniper,        // スナイパーライフル
        Melee,         // 近接武器
        Explosive,     // 爆発物
        Special        // 特殊武器
    }
}
