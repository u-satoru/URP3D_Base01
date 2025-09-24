using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Audio.Interfaces;
using asterivo.Unity60.Features.Templates.FPS.Services;
using asterivo.Unity60.Features.Templates.FPS.Data;
using asterivo.Unity60.Features.Templates.FPS.Configuration;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// 武器制御の中心クラス
    /// プレイヤーにアタッチし、武器の切り替え・発射入力・リロード命令を管理
    /// 詳細設計書準拠：WeaponDataベースの柔軟な武器システム
    /// </summary>
    [RequireComponent(typeof(AmmoManager), typeof(RecoilSystem))]
    public class WeaponController : MonoBehaviour
    {
        [Header("Current Weapon")]
        [SerializeField] private WeaponData _currentWeapon;
        [SerializeField] private GameObject _currentWeaponModel;

        [Header("Weapon Slots")]
        [SerializeField] private WeaponData _primaryWeapon;
        [SerializeField] private WeaponData _secondaryWeapon;
        [SerializeField] private int _currentWeaponSlot = 0; // 0: Primary, 1: Secondary

        [Header("Shooting Configuration")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LayerMask _targetLayerMask = -1;
        [SerializeField] private float _maxShootingRange = 100.0f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        // Components
        private AmmoManager _ammoManager;
        private RecoilSystem _recoilSystem;
        private IShootingStrategy _currentShootingStrategy;
        private Dictionary<ShotType, IShootingStrategy> _shootingStrategies;

        // ServiceLocator Services
        private IFPSInputService _inputService;

        // State
        private bool _isShooting = false;
        private bool _canShoot = true;
        private float _lastShotTime = 0f;
        private float _timeBetweenShots = 0f;

        // Event Channels
        [Header("Event Channels")]
        [SerializeField] private GameEvent _onWeaponFired;
        [SerializeField] private GameEvent _onWeaponReloaded;
        [SerializeField] private GameEvent _onWeaponSwitched;
        [SerializeField] private GameEvent _onAmmoChanged;

        // Properties
        public WeaponData CurrentWeapon => _currentWeapon;
        public bool IsShooting => _isShooting;
        public bool CanShoot => _canShoot && _ammoManager.HasAmmo && !_ammoManager.IsReloading;
        public float TimeBetweenShots => _timeBetweenShots;
        public AmmoManager AmmoManager => _ammoManager;

        private void Awake()
        {
            _ammoManager = GetComponent<AmmoManager>();
            _recoilSystem = GetComponent<RecoilSystem>();

            InitializeShootingStrategies();
        }

        private void Start()
        {
            // 初期武器のセットアップ
            if (_primaryWeapon != null)
            {
                EquipWeapon(_primaryWeapon);
            }

            // ServiceLocator経由でInputServiceを取得・統合
            SetupInputServiceIntegration();
        }

        private void Update()
        {
            UpdateShootingCooldown();
        }

        /// <summary>
        /// 射撃戦略の初期化
        /// Strategy パターンによる HitScan と Projectile の切り替え実装
        /// </summary>
        private void InitializeShootingStrategies()
        {
            _shootingStrategies = new Dictionary<ShotType, IShootingStrategy>
            {
                { ShotType.HitScan, new HitScanStrategy(_targetLayerMask, _maxShootingRange) },
                { ShotType.Projectile, new ProjectileStrategy() }
            };
        }

        /// <summary>
        /// 武器の装備
        /// WeaponData を基に武器の性能と見た目を変更
        /// </summary>
        public void EquipWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("WeaponController: 無効な武器データです。");
                return;
            }

            // 前の武器モデルの削除
            if (_currentWeaponModel != null)
            {
                DestroyImmediate(_currentWeaponModel);
            }

            _currentWeapon = weaponData;
            _timeBetweenShots = weaponData.GetTimeBetweenShots();

            // 射撃戦略の設定
            if (_shootingStrategies.ContainsKey(weaponData.ShotType))
            {
                _currentShootingStrategy = _shootingStrategies[weaponData.ShotType];
            }

            // 武器モデルの生成
            if (weaponData.WeaponPrefab != null)
            {
                _currentWeaponModel = Instantiate(weaponData.WeaponPrefab, transform);
            }

            // 弾薬の初期化
            _ammoManager.Initialize(weaponData.AmmoConfig);

            // 反動システムの設定
            _recoilSystem.SetRecoilPattern(weaponData.RecoilPattern);

            // ServiceLocator経由でUI更新
            var uiService = ServiceLocator.GetService<IFPSUIService>();
            if (uiService != null)
            {
                string fireMode = weaponData.FireMode.ToString();
                uiService.UpdateWeaponInfo(weaponData.WeaponName, fireMode);
                uiService.UpdateAmmoCount(_ammoManager.CurrentAmmo, _ammoManager.TotalAmmo);
            }

            // イベント発行
            _onWeaponSwitched?.Raise();

            Debug.Log($"武器を装備しました: {weaponData.WeaponName}");
        }

        /// <summary>
        /// 武器の切り替え（プライマリ ⇔ セカンダリ）
        /// </summary>
        public void SwitchWeapon()
        {
            if (!CanSwitchWeapon()) return;

            _currentWeaponSlot = _currentWeaponSlot == 0 ? 1 : 0;
            WeaponData targetWeapon = _currentWeaponSlot == 0 ? _primaryWeapon : _secondaryWeapon;

            if (targetWeapon != null)
            {
                StartCoroutine(SwitchWeaponCoroutine(targetWeapon));
            }
        }

        /// <summary>
        /// 射撃処理
        /// 入力システムから呼び出される
        /// </summary>
        public void StartShooting()
        {
            if (!CanShoot) return;

            _isShooting = true;

            if (_currentWeapon.IsFullAuto())
            {
                // フルオート射撃の場合は継続的に射撃
                InvokeRepeating(nameof(FireSingleShot), 0f, _timeBetweenShots);
            }
            else
            {
                // セミオート・バーストの場合は1回のみ
                FireSingleShot();
            }
        }

        /// <summary>
        /// 射撃停止
        /// </summary>
        public void StopShooting()
        {
            _isShooting = false;
            CancelInvoke(nameof(FireSingleShot));
        }

        /// <summary>
        /// 単発射撃処理
        /// </summary>
        private void FireSingleShot()
        {
            if (!CanShoot)
            {
                StopShooting();
                return;
            }

            // 弾薬消費
            _ammoManager.ConsumeAmmo(1);

            // 射撃戦略による実際の射撃処理
            var shotResult = _currentShootingStrategy.ExecuteShot(
                _firePoint.position,
                _firePoint.forward,
                _currentWeapon
            );

            // 反動適用
            _recoilSystem.ApplyRecoil();

            // 音響効果
            PlayFireSound();

            // エフェクト
            ShowMuzzleFlash();

            // ServiceLocator経由でUI更新
            var uiService = ServiceLocator.GetService<IFPSUIService>();
            if (uiService != null)
            {
                uiService.UpdateAmmoCount(_ammoManager.CurrentAmmo, _ammoManager.TotalAmmo);
            }

            // イベント発行
            _onWeaponFired?.Raise();
            _onAmmoChanged?.Raise();

            // 射撃クールダウン
            _lastShotTime = Time.time;
            _canShoot = false;

            Debug.Log($"射撃: {_currentWeapon.WeaponName} - 残弾: {_ammoManager.CurrentAmmo}");
        }

        /// <summary>
        /// リロード処理
        /// </summary>
        public void Reload()
        {
            if (!_ammoManager.CanReload) return;

            // ServiceLocator経由でUI更新（リロード開始）
            var uiService = ServiceLocator.GetService<IFPSUIService>();
            if (uiService != null)
            {
                uiService.ShowReloadIndicator();
            }

            StartCoroutine(_ammoManager.ReloadCoroutine());
            PlayReloadSound();
            _onWeaponReloaded?.Raise();
        }

        /// <summary>
        /// 武器切り替え可能チェック
        /// </summary>
        private bool CanSwitchWeapon()
        {
            return !_isShooting && !_ammoManager.IsReloading;
        }

        /// <summary>
        /// 射撃クールダウンの更新
        /// </summary>
        private void UpdateShootingCooldown()
        {
            if (!_canShoot && Time.time - _lastShotTime >= _timeBetweenShots)
            {
                _canShoot = true;
            }
        }

        /// <summary>
        /// 武器切り替えコルーチン
        /// </summary>
        private System.Collections.IEnumerator SwitchWeaponCoroutine(WeaponData newWeapon)
        {
            _canShoot = false;
            
            // 切り替えアニメーション待機
            yield return new WaitForSeconds(0.3f);

            EquipWeapon(newWeapon);
            
            yield return new WaitForSeconds(0.2f);
            _canShoot = true;
        }

        /// <summary>
        /// 射撃音の再生
        /// ServiceLocator統合による音響システム連携
        /// </summary>
        private void PlayFireSound()
        {
            if (_currentWeapon.FireSound != null)
            {
                // ServiceLocator経由でオーディオサービスを取得
                var audioService = ServiceLocator.GetService<IAudioService>();
                if (audioService != null)
                {
                    audioService.PlaySound(_currentWeapon.FireSound.name, transform.position);
                }
                else
                {
                    // フォールバック: 直接AudioSourceを使用
                    if (_audioSource != null)
                    {
                        _audioSource.PlayOneShot(_currentWeapon.FireSound);
                    }
                }
            }
        }

        /// <summary>
        /// リロード音の再生
        /// ServiceLocator統合による音響システム連携
        /// </summary>
        private void PlayReloadSound()
        {
            if (_currentWeapon.ReloadSound != null)
            {
                // ServiceLocator経由でオーディオサービスを取得
                var audioService = ServiceLocator.GetService<IAudioService>();
                if (audioService != null)
                {
                    audioService.PlaySound(_currentWeapon.ReloadSound.name, transform.position);
                }
                else
                {
                    // フォールバック: 直接AudioSourceを使用
                    if (_audioSource != null)
                    {
                        _audioSource.PlayOneShot(_currentWeapon.ReloadSound);
                    }
                }
            }
        }

        /// <summary>
        /// マズルフラッシュの表示
        /// </summary>
        private void ShowMuzzleFlash()
        {
            if (_currentWeapon.MuzzleFlashPrefab != null && _firePoint != null)
            {
                var flash = Instantiate(_currentWeapon.MuzzleFlashPrefab, _firePoint.position, _firePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }

        /// <summary>
        /// プライマリ武器の設定
        /// </summary>
        public void SetPrimaryWeapon(WeaponData weapon)
        {
            _primaryWeapon = weapon;
            if (_currentWeaponSlot == 0)
            {
                EquipWeapon(weapon);
            }
        }

        /// <summary>
        /// セカンダリ武器の設定
        /// </summary>
        public void SetSecondaryWeapon(WeaponData weapon)
        {
            _secondaryWeapon = weapon;
            if (_currentWeaponSlot == 1)
            {
                EquipWeapon(weapon);
            }
        }

        /// <summary>
        /// ServiceLocator経由でInputServiceを取得・統合
        /// Event駆動システムとの統合により統一入力管理を実現
        /// </summary>
        private void SetupInputServiceIntegration()
        {
            // ServiceLocator経由でInputServiceを取得
            _inputService = ServiceLocator.GetService<IFPSInputService>();

            if (_inputService != null)
            {
                // 射撃関連イベントの購読
                _inputService.OnFirePressed += HandleFirePressed;
                _inputService.OnFireReleased += HandleFireReleased;
                _inputService.OnReloadPressed += HandleReloadPressed;
                _inputService.OnWeaponSwitchPressed += HandleWeaponSwitchPressed;

                Debug.Log("[WeaponController] InputService integration completed successfully");
            }
            else
            {
                Debug.LogWarning("[WeaponController] FPSInputService not found in ServiceLocator. Input integration disabled.");
            }
        }

        /// <summary>
        /// InputService購読解除（GameObject破棄時）
        /// </summary>
        private void OnDestroy()
        {
            if (_inputService != null)
            {
                // イベント購読解除でメモリリーク防止
                _inputService.OnFirePressed -= HandleFirePressed;
                _inputService.OnFireReleased -= HandleFireReleased;
                _inputService.OnReloadPressed -= HandleReloadPressed;
                _inputService.OnWeaponSwitchPressed -= HandleWeaponSwitchPressed;
            }
        }

        #region Input Event Handlers

        /// <summary>
        /// 射撃開始入力ハンドラー
        /// InputServiceからのイベント受信でServiceLocator統合を実現
        /// </summary>
        private void HandleFirePressed()
        {
            StartShooting();
        }

        /// <summary>
        /// 射撃停止入力ハンドラー
        /// </summary>
        private void HandleFireReleased()
        {
            StopShooting();
        }

        /// <summary>
        /// リロード入力ハンドラー
        /// </summary>
        private void HandleReloadPressed()
        {
            Reload();
        }

        /// <summary>
        /// 武器切り替え入力ハンドラー
        /// </summary>
        private void HandleWeaponSwitchPressed()
        {
            SwitchWeapon();
        }

        #endregion

        /// <summary>
        /// デバッグ情報の表示
        /// </summary>
        private void OnGUI()
        {
            if (Application.isEditor && _currentWeapon != null)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 200));
                GUILayout.Label($"武器: {_currentWeapon.WeaponName}");
                GUILayout.Label($"弾薬: {_ammoManager.CurrentAmmo}/{_ammoManager.TotalAmmo}");
                GUILayout.Label($"TTK: {_currentWeapon.GetTTK():F2}s");
                GUILayout.Label($"射撃可能: {CanShoot}");
                GUILayout.Label($"リロード中: {_ammoManager.IsReloading}");

                // ServiceLocator統合状況表示
                var inputServiceStatus = _inputService != null ? "Connected" : "Disconnected";
                GUILayout.Label($"InputService: {inputServiceStatus}");
                GUILayout.EndArea();
            }
        }
    }
}
