using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// エフェクトサービス（ServiceLocator経由アクセス）
    /// FPS Template専用視覚・パーティクルエフェクトシステム
    /// ObjectPool統合対応
    /// </summary>
    public interface IEffectsService
    {
        // 射撃エフェクト
        void PlayMuzzleFlash(GameObject muzzleFlashPrefab, Vector3 position);
        void PlayMuzzleFlash(GameObject muzzleFlashPrefab, Vector3 position, Quaternion rotation);

        // 衝突エフェクト
        void PlayImpactEffect(GameObject impactPrefab, Vector3 position, Vector3 normal);
        void PlayImpactEffect(GameObject impactPrefab, Vector3 position, Vector3 normal, GameObject parent);

        // 弾道エフェクト
        void PlayBulletTrail(Vector3 start, Vector3 end, float duration = 0.1f);
        void PlayLaserSight(Vector3 start, Vector3 end, Color color);

        // ダメージエフェクト
        void PlayBloodEffect(Vector3 position, Vector3 direction, float intensity = 1f);
        void PlayExplosionEffect(Vector3 position, float radius, GameObject explosionPrefab = null);

        // UI視覚エフェクト
        void PlayScreenEffect(ScreenEffectType effectType, float intensity, float duration);
        void PlayCameraShake(float intensity, float duration, ShakeType shakeType = ShakeType.Random);

        // パーティクルシステム管理（ObjectPool統合）
        ParticleSystem GetPooledParticleSystem(string effectName);
        void ReturnParticleSystem(ParticleSystem particleSystem);

        // エフェクト設定
        void SetEffectQuality(EffectQuality quality);
        void SetEffectsEnabled(bool enabled);

        // エフェクトイベント
        event Action<Vector3, GameObject> OnMuzzleFlashPlayed;
        event Action<Vector3, Vector3> OnImpactEffectPlayed;
    }

    /// <summary>
    /// スクリーンエフェクトタイプ
    /// </summary>
    public enum ScreenEffectType
    {
        DamageFlash,      // ダメージ時の赤フラッシュ
        LowHealthEffect,  // 低体力時のエフェクト
        Flash,            // 一般的なフラッシュ
        Blur,             // ブラー効果
        Desaturate        // 彩度低下
    }

    /// <summary>
    /// カメラシェイクタイプ
    /// </summary>
    public enum ShakeType
    {
        Random,           // ランダムシェイク
        Explosion,        // 爆発シェイク
        Recoil,          // 反動シェイク
        Impact           // 衝撃シェイク
    }

    /// <summary>
    /// エフェクト品質設定
    /// </summary>
    public enum EffectQuality
    {
        Low,     // 低品質（パフォーマンス優先）
        Medium,  // 中品質（バランス）
        High,    // 高品質（品質優先）
        Ultra    // 最高品質
    }
}