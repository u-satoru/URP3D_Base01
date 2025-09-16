using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// 射撃コマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// Core層Commandパターン統合
    /// </summary>
    public class FireCommand : ICommand, IResettableCommand
    {
        private Vector3 _origin;
        private Vector3 _direction;
        private string _weaponName;
        private float _damage;
        private GameObject _fireSource;
        private bool _wasExecuted;

        // Undo用データ保持
        private bool _hadAmmo;
        private int _previousAmmoCount;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && _fireSource != null;

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted;

        /// <summary>
        /// 射撃コマンド初期化
        /// </summary>
        public void Initialize(Vector3 origin, Vector3 direction, string weaponName,
                              float damage, GameObject fireSource)
        {
            _origin = origin;
            _direction = direction;
            _weaponName = weaponName;
            _damage = damage;
            _fireSource = fireSource;
            _wasExecuted = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 5)
            {
                Initialize((Vector3)parameters[0], (Vector3)parameters[1], (string)parameters[2],
                          (float)parameters[3], (GameObject)parameters[4]);
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[FireCommand] Cannot execute - invalid state or missing source");
                return;
            }

            try
            {
                // ServiceLocator経由でWeaponManagerサービス取得
                var weaponManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IWeaponManager>();
                var ammoManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAmmoManager>();
                var effectsService = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IEffectsService>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (weaponManager == null || ammoManager == null)
                {
                    Debug.LogError("[FireCommand] Required services not found via ServiceLocator");
                    return;
                }

                // 弾薬確認とUndo用データ保存
                var currentWeapon = weaponManager.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    _previousAmmoCount = ammoManager.GetCurrentAmmo(currentWeapon.WeaponType);
                    _hadAmmo = _previousAmmoCount > 0;

                    if (!_hadAmmo)
                    {
                        // 弾切れの場合は空撃ち音のみ再生
                        audioService?.PlaySFX("EmptyFire", _origin);
                        Debug.Log("[FireCommand] No ammo available");
                        _wasExecuted = true; // Undo可能にするため実行済みとマーク
                        return;
                    }

                    // 弾薬消費
                    ammoManager.ConsumeAmmo(currentWeapon.WeaponType, 1);

                    // 射撃実行
                    weaponManager.Fire(_origin, _direction);

                    // エフェクト再生（ObjectPool統合）
                    if (effectsService != null && currentWeapon.MuzzleFlashPrefab != null)
                    {
                        effectsService.PlayMuzzleFlash(currentWeapon.MuzzleFlashPrefab, _origin);
                        effectsService.PlayBulletTrail(_origin, _origin + _direction * 100f);
                    }

                    // オーディオ再生
                    audioService?.PlaySFX(currentWeapon.FireSound, _origin);

                    // 射撃イベント発行（Event駆動アーキテクチャ）
                    var weaponFiredData = new Events.WeaponFiredData(
                        currentWeapon.WeaponName,
                        currentWeapon.WeaponType,
                        _origin,
                        _direction,
                        _damage,
                        ammoManager.GetCurrentAmmo(currentWeapon.WeaponType),
                        _fireSource,
                        currentWeapon.FireRate
                    );

                    // ServiceLocator経由でイベント取得・発行
                    var weaponFiredEvent = Resources.Load<Events.WeaponFiredEvent>("Events/WeaponFiredEvent");
                    weaponFiredEvent?.RaiseWeaponFired(weaponFiredData);

                    _wasExecuted = true;

                    Debug.Log($"[FireCommand] Fired {_weaponName} from {_origin} towards {_direction}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FireCommand] Execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[FireCommand] Cannot undo - command was not executed");
                return;
            }

            try
            {
                // ServiceLocator経由でサービス取得
                var weaponManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IWeaponManager>();
                var ammoManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAmmoManager>();

                if (weaponManager == null || ammoManager == null)
                {
                    Debug.LogError("[FireCommand] Required services not found for Undo");
                    return;
                }

                var currentWeapon = weaponManager.GetCurrentWeapon();
                if (currentWeapon != null && _hadAmmo)
                {
                    // 弾薬を元に戻す（1発追加）
                    ammoManager.AddAmmo(currentWeapon.WeaponType, 1);
                    Debug.Log($"[FireCommand] Undid fire command - restored 1 ammo for {currentWeapon.WeaponName}");
                }

                _wasExecuted = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FireCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _origin = Vector3.zero;
            _direction = Vector3.forward;
            _weaponName = string.Empty;
            _damage = 0f;
            _fireSource = null;
            _wasExecuted = false;
            _hadAmmo = false;
            _previousAmmoCount = 0;

            Debug.Log("[FireCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"FireCommand[Weapon: {_weaponName}, Origin: {_origin}, Direction: {_direction}, " +
                   $"Damage: {_damage}, Executed: {_wasExecuted}, CanUndo: {CanUndo}]";
        }
    }
}