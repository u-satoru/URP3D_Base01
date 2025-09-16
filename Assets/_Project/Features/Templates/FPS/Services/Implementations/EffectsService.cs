using System;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// エフェクトサービス実装
    /// ObjectPool統合視覚・パーティクルエフェクトシステム + ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// </summary>
    public class EffectsService : IEffectsService
    {
        private EffectQuality _currentQuality = EffectQuality.High;
        private bool _effectsEnabled = true;
        private Camera _mainCamera;

        // イベント
        public event Action<Vector3, GameObject> OnMuzzleFlashPlayed;
        public event Action<Vector3, Vector3> OnImpactEffectPlayed;

        // 射撃エフェクト
        public void PlayMuzzleFlash(GameObject muzzleFlashPrefab, Vector3 position)
        {
            PlayMuzzleFlash(muzzleFlashPrefab, position, Quaternion.identity);
        }

        public void PlayMuzzleFlash(GameObject muzzleFlashPrefab, Vector3 position, Quaternion rotation)
        {
            if (!_effectsEnabled || muzzleFlashPrefab == null)
                return;

            // ObjectPool経由でエフェクト取得（実装予定）
            var muzzleFlash = CreatePooledEffect(muzzleFlashPrefab, position, rotation);
            if (muzzleFlash != null)
            {
                var particleSystem = muzzleFlash.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    ApplyQualitySettings(particleSystem);
                    particleSystem.Play();

                    // 一定時間後にプールに返却
                    float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                    _ = ReturnToPoolAfterDelay(muzzleFlash, duration);
                }

                OnMuzzleFlashPlayed?.Invoke(position, muzzleFlash);

                Debug.Log($"[EffectsService] Muzzle flash played at {position}");
            }
        }

        // 衝突エフェクト
        public void PlayImpactEffect(GameObject impactPrefab, Vector3 position, Vector3 normal)
        {
            PlayImpactEffect(impactPrefab, position, normal, null);
        }

        public void PlayImpactEffect(GameObject impactPrefab, Vector3 position, Vector3 normal, GameObject parent)
        {
            if (!_effectsEnabled || impactPrefab == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(normal);
            var impactEffect = CreatePooledEffect(impactPrefab, position, rotation);

            if (impactEffect != null)
            {
                if (parent != null)
                {
                    impactEffect.transform.SetParent(parent.transform, true);
                }

                var particleSystem = impactEffect.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    ApplyQualitySettings(particleSystem);
                    particleSystem.Play();

                    float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                    _ = ReturnToPoolAfterDelay(impactEffect, duration);
                }

                // ServiceLocator経由でAudioサービス取得
                var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                audioService?.PlaySFX("ImpactSound", position);

                OnImpactEffectPlayed?.Invoke(position, normal);

                Debug.Log($"[EffectsService] Impact effect played at {position}");
            }
        }

        // 弾道エフェクト
        public void PlayBulletTrail(Vector3 start, Vector3 end, float duration = 0.1f)
        {
            if (!_effectsEnabled)
                return;

            // LineRenderer を使用した弾道表示
            var trailObject = new GameObject("BulletTrail");
            var lineRenderer = trailObject.AddComponent<LineRenderer>();

            // LineRenderer設定
            lineRenderer.material = GetBulletTrailMaterial();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // 品質設定適用
            ApplyTrailQualitySettings(lineRenderer);

            // 一定時間後に削除
            _ = DestroyAfterDelay(trailObject, duration);

            Debug.Log($"[EffectsService] Bullet trail from {start} to {end}");
        }

        public void PlayLaserSight(Vector3 start, Vector3 end, Color color)
        {
            if (!_effectsEnabled)
                return;

            // レーザーサイトの表示（LineRendererまたは専用エフェクト）
            var laserObject = new GameObject("LaserSight");
            var lineRenderer = laserObject.AddComponent<LineRenderer>();

            lineRenderer.material = GetLaserSightMaterial();
            lineRenderer.color = color;
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.positionCount = 2;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            // レーザーは継続表示のため、手動で制御する必要がある
            Debug.Log($"[EffectsService] Laser sight from {start} to {end}");
        }

        // ダメージエフェクト
        public void PlayBloodEffect(Vector3 position, Vector3 direction, float intensity = 1f)
        {
            if (!_effectsEnabled)
                return;

            var bloodPrefab = GetBloodEffectPrefab();
            if (bloodPrefab != null)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                var bloodEffect = CreatePooledEffect(bloodPrefab, position, rotation);

                if (bloodEffect != null)
                {
                    var particleSystem = bloodEffect.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        // 強度に基づいてパーティクル数調整
                        var emission = particleSystem.emission;
                        emission.rateOverTime = emission.rateOverTime.constant * intensity;

                        ApplyQualitySettings(particleSystem);
                        particleSystem.Play();

                        float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                        _ = ReturnToPoolAfterDelay(bloodEffect, duration);
                    }
                }

                Debug.Log($"[EffectsService] Blood effect played at {position} with intensity {intensity}");
            }
        }

        public void PlayExplosionEffect(Vector3 position, float radius, GameObject explosionPrefab = null)
        {
            if (!_effectsEnabled)
                return;

            explosionPrefab = explosionPrefab ?? GetExplosionEffectPrefab();
            if (explosionPrefab != null)
            {
                var explosion = CreatePooledEffect(explosionPrefab, position, Quaternion.identity);
                if (explosion != null)
                {
                    // 爆発の規模に基づいてスケール調整
                    float scale = radius / 5f; // 基準半径5mとする
                    explosion.transform.localScale = Vector3.one * scale;

                    var particleSystem = explosion.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        ApplyQualitySettings(particleSystem);
                        particleSystem.Play();

                        float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
                        _ = ReturnToPoolAfterDelay(explosion, duration);
                    }

                    // ServiceLocator経由でAudioサービス取得
                    var audioService = asterivo.Unity60.Core.ServiceLocator.GetService<asterivo.Unity60.Core.Audio.Interfaces.IAudioService>();
                    audioService?.PlaySFX("Explosion", position);

                    // カメラシェイク効果
                    var cameraService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSCameraService>();
                    float shakeIntensity = radius * 0.1f;
                    cameraService?.ApplyCameraShake(shakeIntensity, 1f);

                    Debug.Log($"[EffectsService] Explosion effect played at {position} with radius {radius}");
                }
            }
        }

        // UI視覚エフェクト
        public void PlayScreenEffect(ScreenEffectType effectType, float intensity, float duration)
        {
            if (!_effectsEnabled)
                return;

            _mainCamera = _mainCamera ?? Camera.main;
            if (_mainCamera == null)
                return;

            switch (effectType)
            {
                case ScreenEffectType.DamageFlash:
                    ApplyDamageFlash(intensity, duration);
                    break;

                case ScreenEffectType.LowHealthEffect:
                    ApplyLowHealthEffect(intensity, duration);
                    break;

                case ScreenEffectType.Flash:
                    ApplyFlashEffect(intensity, duration);
                    break;

                case ScreenEffectType.Blur:
                    ApplyBlurEffect(intensity, duration);
                    break;

                case ScreenEffectType.Desaturate:
                    ApplyDesaturateEffect(intensity, duration);
                    break;
            }

            Debug.Log($"[EffectsService] Screen effect {effectType} played with intensity {intensity}");
        }

        public void PlayCameraShake(float intensity, float duration, ShakeType shakeType = ShakeType.Random)
        {
            if (!_effectsEnabled)
                return;

            var cameraService = asterivo.Unity60.Core.ServiceLocator.GetService<IFPSCameraService>();
            cameraService?.ApplyCameraShake(intensity, duration);

            Debug.Log($"[EffectsService] Camera shake {shakeType} played with intensity {intensity}");
        }

        // パーティクルシステム管理（ObjectPool統合）
        public ParticleSystem GetPooledParticleSystem(string effectName)
        {
            // ObjectPool統合実装予定
            var prefab = Resources.Load<GameObject>($"Effects/{effectName}");
            if (prefab != null)
            {
                var instance = UnityEngine.Object.Instantiate(prefab);
                return instance.GetComponent<ParticleSystem>();
            }

            Debug.LogWarning($"[EffectsService] Effect prefab not found: {effectName}");
            return null;
        }

        public void ReturnParticleSystem(ParticleSystem particleSystem)
        {
            if (particleSystem != null)
            {
                // ObjectPool統合実装予定
                UnityEngine.Object.Destroy(particleSystem.gameObject);
            }
        }

        // エフェクト設定
        public void SetEffectQuality(EffectQuality quality)
        {
            _currentQuality = quality;
            Debug.Log($"[EffectsService] Effect quality set to: {quality}");
        }

        public void SetEffectsEnabled(bool enabled)
        {
            _effectsEnabled = enabled;
            Debug.Log($"[EffectsService] Effects {(enabled ? "enabled" : "disabled")}");
        }

        // プライベートメソッド
        private GameObject CreatePooledEffect(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            // ObjectPool統合実装予定
            return UnityEngine.Object.Instantiate(prefab, position, rotation);
        }

        private async System.Threading.Tasks.Task ReturnToPoolAfterDelay(GameObject effect, float delay)
        {
            await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(delay * 1000));

            if (effect != null)
            {
                // ObjectPool統合実装予定
                UnityEngine.Object.Destroy(effect);
            }
        }

        private async System.Threading.Tasks.Task DestroyAfterDelay(GameObject obj, float delay)
        {
            await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(delay * 1000));

            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        private void ApplyQualitySettings(ParticleSystem particleSystem)
        {
            var main = particleSystem.main;

            switch (_currentQuality)
            {
                case EffectQuality.Low:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.5f);
                    break;

                case EffectQuality.Medium:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.75f);
                    break;

                case EffectQuality.High:
                    // デフォルト設定使用
                    break;

                case EffectQuality.Ultra:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 1.5f);
                    break;
            }
        }

        private void ApplyTrailQualitySettings(LineRenderer lineRenderer)
        {
            switch (_currentQuality)
            {
                case EffectQuality.Low:
                    lineRenderer.enabled = false; // 低品質では弾道表示なし
                    break;

                case EffectQuality.Medium:
                    lineRenderer.startWidth *= 0.5f;
                    lineRenderer.endWidth *= 0.5f;
                    break;

                case EffectQuality.High:
                case EffectQuality.Ultra:
                    // デフォルト設定使用
                    break;
            }
        }

        private Material GetBulletTrailMaterial()
        {
            // マテリアルリソース管理実装予定
            return Resources.Load<Material>("Materials/BulletTrail");
        }

        private Material GetLaserSightMaterial()
        {
            return Resources.Load<Material>("Materials/LaserSight");
        }

        private GameObject GetBloodEffectPrefab()
        {
            return Resources.Load<GameObject>("Effects/BloodEffect");
        }

        private GameObject GetExplosionEffectPrefab()
        {
            return Resources.Load<GameObject>("Effects/ExplosionEffect");
        }

        // スクリーンエフェクト実装
        private void ApplyDamageFlash(float intensity, float duration)
        {
            // ダメージフラッシュエフェクト実装
            // PostProcessing Volume や UI Overlay 使用
        }

        private void ApplyLowHealthEffect(float intensity, float duration)
        {
            // 低体力エフェクト実装
            // 画面端の赤いビネット効果等
        }

        private void ApplyFlashEffect(float intensity, float duration)
        {
            // フラッシュエフェクト実装
            // 画面全体の白フラッシュ
        }

        private void ApplyBlurEffect(float intensity, float duration)
        {
            // ブラーエフェクト実装
            // PostProcessing Blur使用
        }

        private void ApplyDesaturateEffect(float intensity, float duration)
        {
            // 彩度低下エフェクト実装
            // PostProcessing Color Adjustments使用
        }

        // 統計・デバッグ情報
        public EffectsStatistics GetEffectsStatistics()
        {
            return new EffectsStatistics
            {
                EffectsEnabled = _effectsEnabled,
                CurrentQuality = _currentQuality,
                ActiveEffectCount = 0 // ObjectPool統合時に実装
            };
        }
    }

    /// <summary>
    /// エフェクト統計情報
    /// </summary>
    [System.Serializable]
    public class EffectsStatistics
    {
        public bool EffectsEnabled;
        public EffectQuality CurrentQuality;
        public int ActiveEffectCount;
    }
}