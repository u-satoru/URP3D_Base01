using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.FPS.Data;
using System.Collections;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// 弾薬管理システム
    /// 弾薬の消費、リロード、弾薬タイプ管理を担当
    /// 詳細設計書準拠：マガジンサイズ、総弾薬数、弾薬タイプの管理
    /// </summary>
    public class AmmoManager : MonoBehaviour
    {
        [Header("Current Ammo Status")]
        [SerializeField] private int _currentAmmo = 30;
        [SerializeField] private int _totalAmmo = 210;
        [SerializeField] private int _magazineSize = 30;

        [Header("Ammo Configuration")]
        [SerializeField] private AmmoConfig _ammoConfig;
        [SerializeField] private bool _infiniteAmmo = false;

        [Header("Reload Settings")]
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private float _reloadProgress = 0f;

        // Event Channels
        [Header("Event Channels")]
        [SerializeField] private GameEvent _onAmmoChanged;
        [SerializeField] private GameEvent _onReloadStarted;
        [SerializeField] private GameEvent _onReloadCompleted;
        [SerializeField] private GameEvent _onAmmoEmpty;

        // Properties
        public int CurrentAmmo => _currentAmmo;
        public int TotalAmmo => _totalAmmo;
        public int MagazineSize => _magazineSize;
        public bool HasAmmo => _currentAmmo > 0;
        public bool CanReload => !_isReloading && _currentAmmo < _magazineSize && (_totalAmmo > 0 || _infiniteAmmo);
        public bool IsReloading => _isReloading;
        public float ReloadProgress => _reloadProgress;
        public AmmoType AmmoType => _ammoConfig.ammoType;
        public bool InfiniteAmmo => _infiniteAmmo;

        /// <summary>
        /// 弾薬システムの初期化
        /// </summary>
        public void Initialize(AmmoConfig ammoConfig)
        {
            _ammoConfig = ammoConfig;
            _magazineSize = ammoConfig.magazineSize;
            _currentAmmo = _magazineSize;
            _totalAmmo = ammoConfig.totalAmmo;
            _isReloading = false;
            _reloadProgress = 0f;

            // イベント発行
            _onAmmoChanged?.Raise();

            Debug.Log($"AmmoManager初期化: {ammoConfig.ammoType} - {_currentAmmo}/{_totalAmmo}");
        }

        /// <summary>
        /// 弾薬の消費
        /// </summary>
        public bool ConsumeAmmo(int amount = 1)
        {
            if (!HasAmmo)
            {
                _onAmmoEmpty?.Raise();
                return false;
            }

            _currentAmmo = Mathf.Max(0, _currentAmmo - amount);

            // イベント発行
            _onAmmoChanged?.Raise();

            // 弾薬が尽きた場合
            if (_currentAmmo <= 0)
            {
                _onAmmoEmpty?.Raise();
            }

            return true;
        }

        /// <summary>
        /// リロード処理（コルーチン）
        /// </summary>
        public IEnumerator ReloadCoroutine()
        {
            if (!CanReload) yield break;

            _isReloading = true;
            _reloadProgress = 0f;

            // リロード開始イベント
            _onReloadStarted?.Raise();

            // リロード時間の決定（戦術的リロードか完全リロード）
            float reloadTime = _currentAmmo > 0 ? _ammoConfig.tacticalReloadTime : _ammoConfig.reloadTime;
            float startTime = Time.time;

            Debug.Log($"リロード開始: {reloadTime:F1}秒 - タイプ:{(_currentAmmo > 0 ? "戦術的" : "完全")}");

            // リロード進行
            while (_reloadProgress < 1f)
            {
                float elapsed = Time.time - startTime;
                _reloadProgress = Mathf.Clamp01(elapsed / reloadTime);
                yield return null;
            }

            // リロード完了処理
            CompleteReload();
        }

        /// <summary>
        /// リロード完了処理
        /// </summary>
        private void CompleteReload()
        {
            if (!_infiniteAmmo)
            {
                // 必要な弾薬数を計算
                int ammoNeeded = _magazineSize - _currentAmmo;
                int ammoToAdd = Mathf.Min(ammoNeeded, _totalAmmo);

                _currentAmmo += ammoToAdd;
                _totalAmmo -= ammoToAdd;
            }
            else
            {
                // 無限弾薬の場合はマガジンをフル装填
                _currentAmmo = _magazineSize;
            }

            _isReloading = false;
            _reloadProgress = 0f;

            // イベント発行
            _onReloadCompleted?.Raise();
            _onAmmoChanged?.Raise();

            Debug.Log($"リロード完了: {_currentAmmo}/{_totalAmmo}");
        }

        /// <summary>
        /// リロードの中断
        /// </summary>
        public void CancelReload()
        {
            if (_isReloading)
            {
                StopAllCoroutines();
                _isReloading = false;
                _reloadProgress = 0f;
                
                Debug.Log("リロードが中断されました");
            }
        }

        /// <summary>
        /// 弾薬の追加（アイテム取得時など）
        /// </summary>
        public void AddAmmo(int amount)
        {
            if (_infiniteAmmo) return;

            _totalAmmo += amount;
            _onAmmoChanged?.Raise();

            Debug.Log($"弾薬を{amount}発取得しました。総弾薬数: {_totalAmmo}");
        }

        /// <summary>
        /// 弾薬の設定
        /// </summary>
        public void SetAmmo(int currentAmmo, int totalAmmo)
        {
            _currentAmmo = Mathf.Clamp(currentAmmo, 0, _magazineSize);
            _totalAmmo = Mathf.Max(0, totalAmmo);
            _onAmmoChanged?.Raise();
        }

        /// <summary>
        /// 弾薬満タン補充
        /// </summary>
        public void RefillAmmo()
        {
            _currentAmmo = _magazineSize;
            _totalAmmo = _ammoConfig.totalAmmo;
            _onAmmoChanged?.Raise();

            Debug.Log("弾薬を満タンにしました");
        }

        /// <summary>
        /// 弾薬情報の取得
        /// </summary>
        public AmmoInfo GetAmmoInfo()
        {
            return new AmmoInfo
            {
                CurrentAmmo = _currentAmmo,
                TotalAmmo = _totalAmmo,
                MagazineSize = _magazineSize,
                AmmoType = _ammoConfig.ammoType,
                IsReloading = _isReloading,
                ReloadProgress = _reloadProgress,
                CanReload = CanReload
            };
        }

        /// <summary>
        /// 弾薬タイプの互換性チェック
        /// </summary>
        public bool IsCompatibleAmmoType(AmmoType ammoType)
        {
            return _ammoConfig.ammoType == ammoType;
        }

        /// <summary>
        /// リロード時間の取得
        /// </summary>
        public float GetReloadTime(bool tactical = false)
        {
            return tactical ? _ammoConfig.tacticalReloadTime : _ammoConfig.reloadTime;
        }

        /// <summary>
        /// デバッグ表示用
        /// </summary>
        public string GetAmmoDisplayText()
        {
            if (_infiniteAmmo)
            {
                return $"{_currentAmmo}/∞";
            }
            return $"{_currentAmmo}/{_totalAmmo}";
        }

        private void OnValidate()
        {
            // Editor上での値制限
            _currentAmmo = Mathf.Clamp(_currentAmmo, 0, _magazineSize);
            _totalAmmo = Mathf.Max(0, _totalAmmo);
            _reloadProgress = Mathf.Clamp01(_reloadProgress);
        }

        /// <summary>
        /// デバッグ情報の表示
        /// </summary>
        [ContextMenu("Show Ammo Status")]
        private void ShowAmmoStatus()
        {
            Debug.Log($"=== Ammo Status ===\n" +
                     $"Current: {_currentAmmo}/{_magazineSize}\n" +
                     $"Total: {_totalAmmo}\n" +
                     $"Type: {_ammoConfig.ammoType}\n" +
                     $"Can Reload: {CanReload}\n" +
                     $"Is Reloading: {_isReloading}\n" +
                     $"Infinite: {_infiniteAmmo}");
        }
    }

    /// <summary>
    /// 弾薬情報データ構造
    /// </summary>
    [System.Serializable]
    public struct AmmoInfo
    {
        public int CurrentAmmo;
        public int TotalAmmo;
        public int MagazineSize;
        public AmmoType AmmoType;
        public bool IsReloading;
        public float ReloadProgress;
        public bool CanReload;

        public float AmmoPercentage => MagazineSize > 0 ? (float)CurrentAmmo / MagazineSize : 0f;
        public bool IsLowAmmo => AmmoPercentage <= 0.3f;
        public bool IsEmpty => CurrentAmmo <= 0;
    }
}
