using UnityEngine;
using asterivo.Unity60.Core.Commands;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// HitScan 射撃戦略
    /// Raycast を使用した即着弾システム
    /// 詳細設計書準拠：距離・部位別ダメージ計算機能
    /// </summary>
    public class HitScanStrategy : IShootingStrategy
    {
        private readonly LayerMask _targetLayerMask;
        private readonly float _maxRange;

        public HitScanStrategy(LayerMask targetLayerMask, float maxRange)
        {
            _targetLayerMask = targetLayerMask;
            _maxRange = maxRange;
        }

        /// <summary>
        /// HitScan射撃の実行
        /// </summary>
        public ShotResult ExecuteShot(Vector3 origin, Vector3 direction, WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogError("HitScanStrategy: WeaponData が null です。");
                return ShotResult.Miss;
            }

            // Raycast 実行
            var effectiveRange = Mathf.Min(weaponData.Stats.maxRange, _maxRange);
            
            if (Physics.Raycast(origin, direction, out RaycastHit hit, effectiveRange, _targetLayerMask))
            {
                return ProcessHit(hit, weaponData);
            }

            // ヒットしなかった場合
            return ShotResult.Miss;
        }

        /// <summary>
        /// ヒット処理
        /// </summary>
        private ShotResult ProcessHit(RaycastHit hit, WeaponData weaponData)
        {
            var hitDistance = hit.distance;
            var hitboxType = DetermineHitboxType(hit);
            var baseDamage = weaponData.GetDamageAtDistance(hitDistance);
            var actualDamage = ApplyHitboxMultiplier(baseDamage, hitboxType);

            // ダメージコマンドの実行
            ApplyDamage(hit.collider.gameObject, actualDamage, hitboxType);

            // ヒットエフェクトの生成
            CreateHitEffect(hit, weaponData);

            // 射撃結果の作成
            var result = ShotResult.CreateHit(hit, hitDistance, actualDamage, hitboxType);

            // デバッグログ
            LogHitInfo(result, weaponData);

            return result;
        }

        /// <summary>
        /// ヒットボックスタイプの判定
        /// コライダーの名前やタグから部位を特定
        /// </summary>
        private HitboxType DetermineHitboxType(RaycastHit hit)
        {
            var colliderName = hit.collider.name.ToLower();
            var colliderTag = hit.collider.tag;

            // コライダー名による判定
            if (colliderName.Contains("head") || colliderTag == "Headshot")
            {
                return HitboxType.Head;
            }
            else if (colliderName.Contains("chest") || colliderName.Contains("torso"))
            {
                return HitboxType.Chest;
            }
            else if (colliderName.Contains("stomach") || colliderName.Contains("belly"))
            {
                return HitboxType.Stomach;
            }
            else if (colliderName.Contains("arm") || colliderName.Contains("hand"))
            {
                return HitboxType.Arms;
            }
            else if (colliderName.Contains("leg") || colliderName.Contains("foot"))
            {
                return HitboxType.Legs;
            }

            // Hitbox コンポーネントによる判定
            var hitbox = hit.collider.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                return hitbox.HitboxType;
            }

            // デフォルトは胸部
            return HitboxType.Chest;
        }

        /// <summary>
        /// ヒットボックス倍率の適用
        /// </summary>
        private float ApplyHitboxMultiplier(float baseDamage, HitboxType hitboxType)
        {
            var multipliers = HitboxMultiplier.DefaultMultipliers;
            
            foreach (var multiplier in multipliers)
            {
                if (multiplier.hitboxType == hitboxType)
                {
                    return baseDamage * multiplier.damageMultiplier;
                }
            }

            return baseDamage;
        }

        /// <summary>
        /// ダメージの適用
        /// Command パターンを使用したダメージ処理
        /// </summary>
        private void ApplyDamage(GameObject target, float damage, HitboxType hitboxType)
        {
            // HealthComponent の検索
            var healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                // 親オブジェクトにある可能性をチェック
                healthComponent = target.GetComponentInParent<HealthComponent>();
            }

            if (healthComponent != null)
            {
                // DamageCommand を使用したダメージ適用（IHealthTargetにキャスト、intに変換）
                var damageCommand = new DamageCommand(healthComponent as IHealthTarget, Mathf.RoundToInt(damage));
                damageCommand.Execute();

                // ヒットボックス情報をHealthComponentに通知
                if (healthComponent is IHitboxAware hitboxAware)
                {
                    hitboxAware.OnHitboxHit(hitboxType, damage);
                }
            }
            else
            {
                Debug.LogWarning($"HitScanStrategy: {target.name} に HealthComponent が見つかりません。");
            }
        }

        /// <summary>
        /// ヒットエフェクトの生成
        /// </summary>
        private void CreateHitEffect(RaycastHit hit, WeaponData weaponData)
        {
            // 衝撃エフェクトの生成
            // TODO: weaponData から適切なエフェクトプレファブを取得
            // var hitEffect = Object.Instantiate(weaponData.HitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            // Object.Destroy(hitEffect, 2.0f);

            // デカル（弾痕）の生成
            CreateBulletDecal(hit);
        }

        /// <summary>
        /// 弾痕デカルの生成
        /// </summary>
        private void CreateBulletDecal(RaycastHit hit)
        {
            // TODO: マテリアルに応じた弾痕の生成
            // 金属、木材、コンクリートなどで異なる弾痕を表示
        }

        /// <summary>
        /// ヒット情報のログ出力
        /// </summary>
        private void LogHitInfo(ShotResult result, WeaponData weaponData)
        {
            if (Application.isEditor)
            {
                Debug.Log($"HitScan射撃ヒット: " +
                         $"武器={weaponData.WeaponName}, " +
                         $"距離={result.Distance:F1}m, " +
                         $"部位={result.HitboxType}, " +
                         $"ダメージ={result.ActualDamage:F1}, " +
                         $"対象={result.HitGameObject.name}");
            }
        }
    }

    /// <summary>
    /// ヒットボックスコンポーネント
    /// 各コライダーに設置してヒットボックスタイプを指定
    /// </summary>
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private HitboxType _hitboxType = HitboxType.Chest;
        
        public HitboxType HitboxType => _hitboxType;
    }

    /// <summary>
    /// HealthComponent インターフェース（仮定義）
    /// 実際の HealthComponent が実装すべきインターフェース
    /// </summary>
    public interface IHitboxAware
    {
        void OnHitboxHit(HitboxType hitboxType, float damage);
    }

    /// <summary>
    /// HealthComponent 基本実装（参考）
    /// 実際のプロジェクトでは Core 層の HealthComponent を使用
    /// </summary>
    public class HealthComponent : MonoBehaviour, IHitboxAware
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0f;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(float damage)
        {
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            
            if (!IsAlive)
            {
                Die();
            }
        }

        public void OnHitboxHit(HitboxType hitboxType, float damage)
        {
            // ヒットボックス別の処理
            switch (hitboxType)
            {
                case HitboxType.Head:
                    // ヘッドショット時の特別処理
                    Debug.Log("ヘッドショット!");
                    break;
                case HitboxType.Chest:
                    // 胸部ヒット時の処理
                    break;
                // その他の部位...
            }

            TakeDamage(damage);
        }

        private void Die()
        {
            Debug.Log($"{gameObject.name} が死亡しました。");
            // 死亡処理
        }
    }
}