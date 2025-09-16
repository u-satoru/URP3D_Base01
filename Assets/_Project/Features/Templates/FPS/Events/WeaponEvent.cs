using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Events
{
    /// <summary>
    /// 武器関連イベント
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// Core層のGameEventシステム統合
    /// </summary>

    [CreateAssetMenu(fileName = "WeaponFiredEvent", menuName = "FPS Template/Events/Weapon Fired Event")]
    public class WeaponFiredEvent : ScriptableObject
    {
        // Event-driven architecture integration
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<WeaponFiredData> _weaponFiredEvent;

        /// <summary>
        /// 武器発砲イベントを発行
        /// </summary>
        public void RaiseWeaponFired(WeaponFiredData data)
        {
            _weaponFiredEvent?.Raise(data);
            Debug.Log($"[WeaponEvent] Weapon fired: {data.WeaponName} at {data.Position}");
        }

        /// <summary>
        /// 武器発砲イベントリスナー登録
        /// </summary>
        public void RegisterListener(Action<WeaponFiredData> callback)
        {
            if (_weaponFiredEvent != null)
            {
                _weaponFiredEvent.AddListener(callback);
            }
        }

        /// <summary>
        /// 武器発砲イベントリスナー解除
        /// </summary>
        public void UnregisterListener(Action<WeaponFiredData> callback)
        {
            if (_weaponFiredEvent != null)
            {
                _weaponFiredEvent.RemoveListener(callback);
            }
        }
    }

    [CreateAssetMenu(fileName = "WeaponReloadEvent", menuName = "FPS Template/Events/Weapon Reload Event")]
    public class WeaponReloadEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<WeaponReloadData> _weaponReloadStartedEvent;
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<WeaponReloadData> _weaponReloadCompletedEvent;

        /// <summary>
        /// 武器リロード開始イベント発行
        /// </summary>
        public void RaiseReloadStarted(WeaponReloadData data)
        {
            _weaponReloadStartedEvent?.Raise(data);
            Debug.Log($"[WeaponEvent] Reload started: {data.WeaponName} ({data.ReloadTime}s)");
        }

        /// <summary>
        /// 武器リロード完了イベント発行
        /// </summary>
        public void RaiseReloadCompleted(WeaponReloadData data)
        {
            _weaponReloadCompletedEvent?.Raise(data);
            Debug.Log($"[WeaponEvent] Reload completed: {data.WeaponName} - {data.CurrentAmmo}/{data.MaxAmmo}");
        }

        /// <summary>
        /// リロード開始リスナー登録
        /// </summary>
        public void RegisterReloadStartedListener(Action<WeaponReloadData> callback)
        {
            _weaponReloadStartedEvent?.AddListener(callback);
        }

        /// <summary>
        /// リロード完了リスナー登録
        /// </summary>
        public void RegisterReloadCompletedListener(Action<WeaponReloadData> callback)
        {
            _weaponReloadCompletedEvent?.AddListener(callback);
        }

        /// <summary>
        /// リロード開始リスナー解除
        /// </summary>
        public void UnregisterReloadStartedListener(Action<WeaponReloadData> callback)
        {
            _weaponReloadStartedEvent?.RemoveListener(callback);
        }

        /// <summary>
        /// リロード完了リスナー解除
        /// </summary>
        public void UnregisterReloadCompletedListener(Action<WeaponReloadData> callback)
        {
            _weaponReloadCompletedEvent?.RemoveListener(callback);
        }
    }

    [CreateAssetMenu(fileName = "WeaponSwitchEvent", menuName = "FPS Template/Events/Weapon Switch Event")]
    public class WeaponSwitchEvent : ScriptableObject
    {
        [SerializeField] private asterivo.Unity60.Core.Events.GameEvent<WeaponSwitchData> _weaponSwitchEvent;

        /// <summary>
        /// 武器切り替えイベント発行
        /// </summary>
        public void RaiseWeaponSwitch(WeaponSwitchData data)
        {
            _weaponSwitchEvent?.Raise(data);
            Debug.Log($"[WeaponEvent] Weapon switched: {data.PreviousWeapon} → {data.NewWeapon}");
        }

        /// <summary>
        /// 武器切り替えリスナー登録
        /// </summary>
        public void RegisterListener(Action<WeaponSwitchData> callback)
        {
            _weaponSwitchEvent?.AddListener(callback);
        }

        /// <summary>
        /// 武器切り替えリスナー解除
        /// </summary>
        public void UnregisterListener(Action<WeaponSwitchData> callback)
        {
            _weaponSwitchEvent?.RemoveListener(callback);
        }
    }

    /// <summary>
    /// 武器発砲イベントデータ
    /// </summary>
    [System.Serializable]
    public class WeaponFiredData
    {
        public string WeaponName;
        public WeaponType WeaponType;
        public Vector3 Position;
        public Vector3 Direction;
        public float Damage;
        public int RemainingAmmo;
        public GameObject FiredBy;
        public float FireRate;

        public WeaponFiredData(string weaponName, WeaponType weaponType, Vector3 position, Vector3 direction, float damage, int remainingAmmo, GameObject firedBy, float fireRate = 0f)
        {
            WeaponName = weaponName;
            WeaponType = weaponType;
            Position = position;
            Direction = direction;
            Damage = damage;
            RemainingAmmo = remainingAmmo;
            FiredBy = firedBy;
            FireRate = fireRate;
        }
    }

    /// <summary>
    /// 武器リロードイベントデータ
    /// </summary>
    [System.Serializable]
    public class WeaponReloadData
    {
        public string WeaponName;
        public WeaponType WeaponType;
        public int CurrentAmmo;
        public int MaxAmmo;
        public int ReserveAmmo;
        public float ReloadTime;
        public GameObject ReloadedBy;

        public WeaponReloadData(string weaponName, WeaponType weaponType, int currentAmmo, int maxAmmo, int reserveAmmo, float reloadTime, GameObject reloadedBy)
        {
            WeaponName = weaponName;
            WeaponType = weaponType;
            CurrentAmmo = currentAmmo;
            MaxAmmo = maxAmmo;
            ReserveAmmo = reserveAmmo;
            ReloadTime = reloadTime;
            ReloadedBy = reloadedBy;
        }
    }

    /// <summary>
    /// 武器切り替えイベントデータ
    /// </summary>
    [System.Serializable]
    public class WeaponSwitchData
    {
        public string PreviousWeapon;
        public string NewWeapon;
        public WeaponType PreviousWeaponType;
        public WeaponType NewWeaponType;
        public GameObject SwitchedBy;
        public float SwitchTime;

        public WeaponSwitchData(string previousWeapon, string newWeapon, WeaponType previousWeaponType, WeaponType newWeaponType, GameObject switchedBy, float switchTime = 0f)
        {
            PreviousWeapon = previousWeapon;
            NewWeapon = newWeapon;
            PreviousWeaponType = previousWeaponType;
            NewWeaponType = newWeaponType;
            SwitchedBy = switchedBy;
            SwitchTime = switchTime;
        }
    }
}