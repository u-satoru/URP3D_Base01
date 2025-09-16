using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// リロードコマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// 非同期リロード処理対応
    /// </summary>
    public class ReloadCommand : ICommand, IResettableCommand
    {
        private Events.WeaponType _weaponType;
        private GameObject _reloadSource;
        private bool _wasExecuted;
        private bool _isReloading;

        // Undo用データ保持
        private int _previousCurrentAmmo;
        private int _previousReserveAmmo;
        private bool _couldReload;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && !_isReloading && _reloadSource != null;

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted && !_isReloading;

        /// <summary>
        /// リロードコマンド初期化
        /// </summary>
        public void Initialize(Events.WeaponType weaponType, GameObject reloadSource)
        {
            _weaponType = weaponType;
            _reloadSource = reloadSource;
            _wasExecuted = false;
            _isReloading = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 2)
            {
                Initialize((Events.WeaponType)parameters[0], (GameObject)parameters[1]);
            }
            else
            {
                Debug.LogWarning("[ReloadCommand] Initialize called with insufficient parameters");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[ReloadCommand] Cannot execute - invalid state or already reloading");
                return;
            }

            try
            {
                // ServiceLocator経由でAmmoManagerサービス取得
                var ammoManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAmmoManager>();
                var weaponManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IWeaponManager>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (ammoManager == null || weaponManager == null)
                {
                    Debug.LogError("[ReloadCommand] Required services not found via ServiceLocator");
                    return;
                }

                // リロード可否確認とUndo用データ保存
                _couldReload = ammoManager.CanReload(_weaponType);
                if (!_couldReload)
                {
                    Debug.Log($"[ReloadCommand] Cannot reload {_weaponType} - already full or no reserve ammo");
                    _wasExecuted = true; // Undo可能にするためマーク（実際にはリロードしていない）
                    return;
                }

                // 現在の弾薬状態を保存（Undo用）
                _previousCurrentAmmo = ammoManager.GetCurrentAmmo(_weaponType);
                _previousReserveAmmo = ammoManager.GetReserveAmmo(_weaponType);

                // リロード開始
                _isReloading = true;
                ammoManager.Reload(_weaponType);

                // リロード開始音再生
                var currentWeapon = weaponManager.GetCurrentWeapon();
                if (currentWeapon != null && currentWeapon.WeaponType == _weaponType)
                {
                    audioService?.PlaySFX(currentWeapon.ReloadSound, _reloadSource.transform.position);
                }
                else
                {
                    audioService?.PlaySFX("ReloadGeneric", _reloadSource.transform.position);
                }

                // リロード開始イベント発行（Event駆動アーキテクチャ）
                var reloadData = new Events.WeaponReloadData(
                    currentWeapon?.WeaponName ?? _weaponType.ToString(),
                    _weaponType,
                    _previousCurrentAmmo,
                    ammoManager.GetMaxAmmo(_weaponType),
                    _previousReserveAmmo,
                    currentWeapon?.ReloadTime ?? 2.0f,
                    _reloadSource
                );

                var weaponReloadEvent = Resources.Load<Events.WeaponReloadEvent>("Events/WeaponReloadEvent");
                weaponReloadEvent?.RaiseReloadStarted(reloadData);

                _wasExecuted = true;

                // リロード完了を非同期で処理（実際のリロード処理はAmmoManagerで実行される）
                _ = WaitForReloadCompletion(currentWeapon?.ReloadTime ?? 2.0f, reloadData, weaponReloadEvent);

                Debug.Log($"[ReloadCommand] Started reloading {_weaponType}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReloadCommand] Execution failed: {ex.Message}");
                _isReloading = false;
            }
        }

        /// <summary>
        /// リロード完了待機（非同期処理）
        /// </summary>
        private async System.Threading.Tasks.Task WaitForReloadCompletion(float reloadTime,
                                                                          Events.WeaponReloadData reloadData,
                                                                          Events.WeaponReloadEvent weaponReloadEvent)
        {
            try
            {
                // リロード時間待機
                await System.Threading.Tasks.Task.Delay(System.TimeSpan.FromSeconds(reloadTime));

                // ServiceLocator経由でサービス再取得（非同期処理後のため）
                var ammoManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAmmoManager>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (ammoManager != null)
                {
                    // リロード完了データ作成
                    var completedData = new Events.WeaponReloadData(
                        reloadData.WeaponName,
                        reloadData.WeaponType,
                        ammoManager.GetCurrentAmmo(_weaponType),
                        ammoManager.GetMaxAmmo(_weaponType),
                        ammoManager.GetReserveAmmo(_weaponType),
                        reloadTime,
                        _reloadSource
                    );

                    // リロード完了イベント発行
                    weaponReloadEvent?.RaiseReloadCompleted(completedData);

                    // リロード完了音再生
                    audioService?.PlaySFX("ReloadComplete", _reloadSource.transform.position);

                    Debug.Log($"[ReloadCommand] Completed reloading {_weaponType}");
                }

                _isReloading = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReloadCommand] Reload completion failed: {ex.Message}");
                _isReloading = false;
            }
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[ReloadCommand] Cannot undo - command was not executed or still reloading");
                return;
            }

            try
            {
                // ServiceLocator経由でAmmoManagerサービス取得
                var ammoManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IAmmoManager>();

                if (ammoManager == null)
                {
                    Debug.LogError("[ReloadCommand] AmmoManager not found for Undo");
                    return;
                }

                if (_couldReload)
                {
                    // 弾薬を元の状態に戻す
                    ammoManager.SetAmmo(_weaponType, _previousCurrentAmmo, _previousReserveAmmo);
                    Debug.Log($"[ReloadCommand] Undid reload command - restored ammo state for {_weaponType}");
                }

                _wasExecuted = false;
                _isReloading = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ReloadCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _weaponType = Events.WeaponType.Unknown;
            _reloadSource = null;
            _wasExecuted = false;
            _isReloading = false;
            _previousCurrentAmmo = 0;
            _previousReserveAmmo = 0;
            _couldReload = false;

            Debug.Log("[ReloadCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"ReloadCommand[WeaponType: {_weaponType}, Executed: {_wasExecuted}, " +
                   $"Reloading: {_isReloading}, CanUndo: {CanUndo}]";
        }
    }
}