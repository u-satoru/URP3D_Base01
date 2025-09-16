using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPS UI管理サービス（ServiceLocator経由アクセス）
    /// ObjectPool統合UI要素管理対応
    /// </summary>
    public interface IFPSUIService
    {
        // UI要素の表示/非表示制御
        void ShowCrosshair(bool show);
        void ShowAmmoCounter(bool show);
        void ShowHealthBar(bool show);
        void ShowWeaponInfo(bool show);

        // 動的UI要素更新
        void UpdateAmmoDisplay(int currentAmmo, int maxAmmo, int reserveAmmo);
        void UpdateHealthDisplay(float currentHealth, float maxHealth);
        void UpdateWeaponDisplay(string weaponName, WeaponType weaponType);
        void UpdateCrosshair(CrosshairState state);

        // ダメージ・エフェクトUI（ObjectPool活用）
        void ShowDamageIndicator(Vector3 damageSource, float damageAmount);
        void ShowHitMarker();
        void ShowKillMarker();

        // UI要素生成（ObjectPool統合）
        T CreateUIElement<T>() where T : Component;
        void ReturnUIElement<T>(T element) where T : Component;

        // UI設定適用
        void ApplyUIConfiguration(FPSUIConfiguration config);

        // UIイベント
        event Action<bool> OnCrosshairVisibilityChanged;
        event Action<float> OnHealthBarUpdated;
    }

    /// <summary>
    /// クロスヘア状態定義
    /// </summary>
    public enum CrosshairState
    {
        Normal,
        Aiming,
        Firing,
        Reloading,
        Hidden
    }

    /// <summary>
    /// FPS UI設定（ScriptableObject連携用）
    /// </summary>
    [System.Serializable]
    public class FPSUIConfiguration
    {
        public bool showCrosshair = true;
        public bool showAmmoCounter = true;
        public bool showHealthBar = true;
        public bool showWeaponInfo = true;

        public Color crosshairColor = Color.white;
        public Color healthBarColor = Color.green;
        public Color ammoCounterColor = Color.white;
    }
}