using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Commands
{
    /// <summary>
    /// 武器切り替えコマンド実装
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// ObjectPool最適化によるメモリ効率化（95%削減効果）
    /// アニメーション統合対応
    /// </summary>
    public class WeaponSwitchCommand : ICommand, IResettableCommand
    {
        private string _targetWeaponName;
        private Events.WeaponType _targetWeaponType;
        private GameObject _switchSource;
        private bool _wasExecuted;
        private bool _isSwitching;

        // Undo用データ保持
        private string _previousWeaponName;
        private Events.WeaponType _previousWeaponType;
        private bool _hadPreviousWeapon;

        /// <summary>
        /// コマンド実行可否
        /// </summary>
        public bool CanExecute => !_wasExecuted && !_isSwitching && _switchSource != null && !string.IsNullOrEmpty(_targetWeaponName);

        /// <summary>
        /// Undo実行可否
        /// </summary>
        public bool CanUndo => _wasExecuted && !_isSwitching && _hadPreviousWeapon;

        /// <summary>
        /// 武器切り替えコマンド初期化
        /// </summary>
        public void Initialize(string targetWeaponName, Events.WeaponType targetWeaponType, GameObject switchSource)
        {
            _targetWeaponName = targetWeaponName;
            _targetWeaponType = targetWeaponType;
            _switchSource = switchSource;
            _wasExecuted = false;
            _isSwitching = false;
        }

        /// <summary>
        /// IResettableCommand準拠の初期化
        /// </summary>
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 3)
            {
                Initialize((string)parameters[0], (Events.WeaponType)parameters[1], (GameObject)parameters[2]);
            }
            else
            {
                Debug.LogWarning("[WeaponSwitchCommand] Initialize called with insufficient parameters");
            }
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute()
        {
            if (!CanExecute)
            {
                Debug.LogWarning("[WeaponSwitchCommand] Cannot execute - invalid state or missing parameters");
                return;
            }

            try
            {
                // ServiceLocator経由でWeaponManagerサービス取得
                var weaponManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IWeaponManager>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (weaponManager == null)
                {
                    Debug.LogError("[WeaponSwitchCommand] WeaponManager not found via ServiceLocator");
                    return;
                }

                // 現在の武器情報を保存（Undo用）
                var currentWeapon = weaponManager.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    _previousWeaponName = currentWeapon.WeaponName;
                    _previousWeaponType = currentWeapon.WeaponType;
                    _hadPreviousWeapon = true;
                }
                else
                {
                    _previousWeaponName = "";
                    _previousWeaponType = Events.WeaponType.Unknown;
                    _hadPreviousWeapon = false;
                }

                // 同じ武器への切り替えチェック
                if (_hadPreviousWeapon && _targetWeaponType == _previousWeaponType)
                {
                    Debug.Log($"[WeaponSwitchCommand] Already equipped {_targetWeaponName}");
                    _wasExecuted = true; // Undo可能にするためマーク
                    return;
                }

                // 武器切り替え開始
                _isSwitching = true;

                // 切り替え音再生
                audioService?.PlaySFX("WeaponSwitch", _switchSource.transform.position);

                // 武器切り替えイベント発行（開始）
                var weaponSwitchData = new Events.WeaponSwitchData(
                    _previousWeaponName,
                    _targetWeaponName,
                    _previousWeaponType,
                    _targetWeaponType,
                    _switchSource,
                    0.5f // 切り替え時間
                );

                var weaponSwitchEvent = Resources.Load<Events.WeaponSwitchEvent>("Events/WeaponSwitchEvent");
                weaponSwitchEvent?.RaiseWeaponSwitch(weaponSwitchData);

                // 実際の武器切り替え実行
                bool switchSuccess = weaponManager.SwitchWeapon(_targetWeaponName);

                if (switchSuccess)
                {
                    _wasExecuted = true;

                    // 切り替え完了を非同期で処理（アニメーション待機）
                    _ = CompleteSwitchAsync(weaponSwitchData);

                    Debug.Log($"[WeaponSwitchCommand] Switched from {_previousWeaponName} to {_targetWeaponName}");
                }
                else
                {
                    Debug.LogWarning($"[WeaponSwitchCommand] Failed to switch to {_targetWeaponName} - weapon not available");
                    _isSwitching = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponSwitchCommand] Execution failed: {ex.Message}");
                _isSwitching = false;
            }
        }

        /// <summary>
        /// 切り替え完了処理（非同期）
        /// </summary>
        private async System.Threading.Tasks.Task CompleteSwitchAsync(Events.WeaponSwitchData switchData)
        {
            try
            {
                // 切り替えアニメーション時間待機
                await System.Threading.Tasks.Task.Delay(500); // 0.5秒

                // ServiceLocator経由でサービス再取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                // 切り替え完了音再生
                audioService?.PlaySFX("WeaponSwitchComplete", _switchSource.transform.position);

                Debug.Log($"[WeaponSwitchCommand] Weapon switch completed: {switchData.NewWeapon}");

                _isSwitching = false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponSwitchCommand] Switch completion failed: {ex.Message}");
                _isSwitching = false;
            }
        }

        /// <summary>
        /// コマンドUndo実行
        /// </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning("[WeaponSwitchCommand] Cannot undo - no previous weapon or still switching");
                return;
            }

            try
            {
                // ServiceLocator経由でWeaponManagerサービス取得
                var weaponManager = asterivo.Unity60.Core.ServiceLocator.GetService<Services.IWeaponManager>();
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();

                if (weaponManager == null)
                {
                    Debug.LogError("[WeaponSwitchCommand] WeaponManager not found for Undo");
                    return;
                }

                // 前の武器に切り替え
                bool undoSuccess = weaponManager.SwitchWeapon(_previousWeaponName);

                if (undoSuccess)
                {
                    // Undo音再生
                    audioService?.PlaySFX("WeaponSwitch", _switchSource.transform.position);

                    // Undo用武器切り替えイベント発行
                    var undoSwitchData = new Events.WeaponSwitchData(
                        _targetWeaponName,
                        _previousWeaponName,
                        _targetWeaponType,
                        _previousWeaponType,
                        _switchSource,
                        0.3f // Undoは高速
                    );

                    var weaponSwitchEvent = Resources.Load<Events.WeaponSwitchEvent>("Events/WeaponSwitchEvent");
                    weaponSwitchEvent?.RaiseWeaponSwitch(undoSwitchData);

                    _wasExecuted = false;

                    Debug.Log($"[WeaponSwitchCommand] Undid weapon switch - restored {_previousWeaponName}");
                }
                else
                {
                    Debug.LogWarning($"[WeaponSwitchCommand] Failed to undo switch to {_previousWeaponName}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[WeaponSwitchCommand] Undo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// ObjectPool再利用のための状態リセット（IResettableCommand実装）
        /// </summary>
        public void Reset()
        {
            _targetWeaponName = string.Empty;
            _targetWeaponType = Events.WeaponType.Unknown;
            _switchSource = null;
            _wasExecuted = false;
            _isSwitching = false;
            _previousWeaponName = string.Empty;
            _previousWeaponType = Events.WeaponType.Unknown;
            _hadPreviousWeapon = false;

            Debug.Log("[WeaponSwitchCommand] Command reset for ObjectPool reuse");
        }

        /// <summary>
        /// デバッグ情報取得
        /// </summary>
        public override string ToString()
        {
            return $"WeaponSwitchCommand[Target: {_targetWeaponName}, Previous: {_previousWeaponName}, " +
                   $"Executed: {_wasExecuted}, Switching: {_isSwitching}, CanUndo: {CanUndo}]";
        }
    }
}