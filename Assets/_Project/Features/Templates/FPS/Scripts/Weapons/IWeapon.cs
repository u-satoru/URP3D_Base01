using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons
{
    /// <summary>
    /// FPS武器システムの基底インターフェース
    /// 射撃、リロード、エイムなどの基本機能を定義
    /// </summary>
    public interface IWeapon
    {
        /// <summary>武器データ</summary>
        WeaponData WeaponData { get; }
        
        /// <summary>現在の弾薬数</summary>
        int CurrentAmmo { get; }
        
        /// <summary>最大弾薬数</summary>
        int MaxAmmo { get; }
        
        /// <summary>リザーブ弾薬数</summary>
        int ReserveAmmo { get; }
        
        /// <summary>射撃可能かどうか</summary>
        bool CanShoot { get; }
        
        /// <summary>リロード中かどうか</summary>
        bool IsReloading { get; }
        
        /// <summary>エイム中かどうか</summary>
        bool IsAiming { get; }
        
        /// <summary>
        /// 射撃を実行
        /// </summary>
        /// <param name="origin">射撃開始位置</param>
        /// <param name="direction">射撃方向</param>
        /// <returns>射撃が実行されたかどうか</returns>
        bool Shoot(Vector3 origin, Vector3 direction);
        
        /// <summary>
        /// リロードを開始
        /// </summary>
        /// <returns>リロードが開始されたかどうか</returns>
        bool StartReload();
        
        /// <summary>
        /// エイム開始
        /// </summary>
        void StartAiming();
        
        /// <summary>
        /// エイム終了
        /// </summary>
        void StopAiming();
        
        /// <summary>
        /// 武器の更新処理
        /// </summary>
        void UpdateWeapon();
    }
}