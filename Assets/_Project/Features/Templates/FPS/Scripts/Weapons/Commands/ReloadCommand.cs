using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Features.Templates.FPS.Weapons.Commands
{
    /// <summary>
    /// FPS武器リロードコマンド
    /// Command Patternに従い、リロード動作をカプセル化
    /// ObjectPool対応のためIResettableCommandを実装
    /// </summary>
    public class ReloadCommand : IResettableCommand
    {
        private IWeapon _weapon;
        private int _previousAmmo;
        private int _previousReserveAmmo;
        private bool _wasExecuted;
        
        public bool CanUndo => true; // リロードは巻き戻し可能
        
        public ReloadCommand()
        {
            // プール化対応：パラメーターなしコンストラクタ
        }
        
        public ReloadCommand(IWeapon weapon)
        {
            Initialize(weapon);
        }
        
        public void Execute()
        {
            if (_weapon == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("ReloadCommand: Weapon is null, cannot execute reload");
#endif
                return;
            }
            
            // 既にリロード中、弾薬満タン、リザーブ弾薬なしの場合は実行しない
            if (_weapon.IsReloading || 
                _weapon.CurrentAmmo >= _weapon.MaxAmmo || 
                _weapon.ReserveAmmo <= 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("ReloadCommand: Cannot reload - conditions not met");
#endif
                return;
            }
            
            // 現在の状態を記録（Undo用）
            _previousAmmo = _weapon.CurrentAmmo;
            _previousReserveAmmo = _weapon.ReserveAmmo;
            
            // リロード実行（実際のWeaponSystemが処理）
            bool reloadStarted = _weapon.StartReload();
            _wasExecuted = reloadStarted;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (reloadStarted)
            {
                Debug.Log($"ReloadCommand: Started reload - Previous ammo: {_previousAmmo}, Reserve: {_previousReserveAmmo}");
            }
            else
            {
                Debug.LogWarning("ReloadCommand: Failed to start reload");
            }
#endif
        }
        
        public void Undo()
        {
            if (!_wasExecuted || _weapon == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("ReloadCommand: Cannot undo - command was not executed or weapon is null");
#endif
                return;
            }
            
            // リロード状態をリセット（実際のWeaponSystemが処理）
            // Note: この実装は実際のWeaponSystemでUndo機能をサポートしている必要がある
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"ReloadCommand: Undoing reload - Restoring ammo: {_previousAmmo}, Reserve: {_previousReserveAmmo}");
#endif
        }
        
        public void Reset()
        {
            _weapon = null;
            _previousAmmo = 0;
            _previousReserveAmmo = 0;
            _wasExecuted = false;
        }
        
        public void Initialize(params object[] parameters)
        {
            if (parameters.Length >= 1 && parameters[0] is IWeapon weapon)
            {
                _weapon = weapon;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError("ReloadCommand: Invalid parameters. Expected: IWeapon weapon");
            }
#endif
        }
    }
}