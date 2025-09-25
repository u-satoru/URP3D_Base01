using UnityEngine;
using UnityEngine.Pool;
using asterivo.Unity60.Features.Templates.FPS.Data;

namespace asterivo.Unity60.Features.Templates.FPS.WeaponSystem
{
    /// <summary>
    /// Projectile 射撃戦略
    /// 物理弾薬オブジェクトを生成する射撃システム
    /// 詳細設計書準拠：弾丸の物理挙動と ObjectPool 最適化
    /// </summary>
    public class ProjectileStrategy : IShootingStrategy
    {
        private readonly IObjectPool<Projectile> _projectilePool;

        public ProjectileStrategy()
        {
            // ObjectPool を使用した弾薬の効率的管理
            _projectilePool = new ObjectPool<Projectile>(
                CreateProjectile,
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile,
                defaultCapacity: 50,
                maxSize: 200
            );
        }

        /// <summary>
        /// Projectile射撃の実行
        /// </summary>
        public ShotResult ExecuteShot(Vector3 origin, Vector3 direction, WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogError("ProjectileStrategy: WeaponData が null です。");
                return ShotResult.Miss;
            }

            // プールから弾薬オブジェクトを取得
            var projectile = _projectilePool.Get();
            
            if (projectile != null)
            {
                // 弾薬の初期化と射出
                projectile.Initialize(origin, direction, weaponData, OnProjectileHit);
                projectile.Fire();

                Debug.Log($"Projectile射撃: {weaponData.WeaponName} - 弾薬速度: {weaponData.ProjectileSpeed}");
            }

            // Projectile の場合、即座の結果は返さない（非同期処理）
            return ShotResult.Miss; // 一時的な戻り値
        }

        /// <summary>
        /// プロジェクタイルヒット時のコールバック
        /// </summary>
        private void OnProjectileHit(Projectile projectile, ShotResult result)
        {
            // ヒット処理完了後、プールに返却
            _projectilePool.Release(projectile);
        }

        // ObjectPool 関数群
        private Projectile CreateProjectile()
        {
            var projectileObject = new GameObject("Projectile");
            var projectile = projectileObject.AddComponent<Projectile>();
            return projectile;
        }

        private void OnGetProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private void OnReleaseProjectile(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
            projectile.Reset();
        }

        private void OnDestroyProjectile(Projectile projectile)
        {
            Object.Destroy(projectile.gameObject);
        }
    }

    /// <summary>
    /// 弾薬プロジェクタイルクラス
    /// 物理的な弾薬オブジェクトの挙動を管理
    /// </summary>
    public class Projectile : MonoBehaviour, IResettablePoolObject
    {
        [Header("Projectile Components")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collider;
        [SerializeField] private TrailRenderer _trail;

        [Header("Projectile Settings")]
        [SerializeField] private float _lifeTime = 5.0f;
        [SerializeField] private float _gravityScale = 1.0f;

        // State
        private WeaponData _weaponData;
        private float _damage;
        private float _speed;
        private Vector3 _initialVelocity;
        private System.Action<Projectile, ShotResult> _onHitCallback;
        private float _startTime;
        private bool _hasHit = false;

        // Properties
        public bool HasHit => _hasHit;
        public float DistanceTraveled => Vector3.Distance(transform.position, _initialPosition);
        private Vector3 _initialPosition;

        private void Awake()
        {
            // コンポーネントの取得・作成
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }
            }

            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
                if (_collider == null)
                {
                    var sphereCollider = gameObject.AddComponent<SphereCollider>();
                    sphereCollider.radius = 0.01f; // 小さな弾薬コライダー
                    sphereCollider.isTrigger = true;
                    _collider = sphereCollider;
                }
            }

            if (_trail == null)
            {
                _trail = GetComponent<TrailRenderer>();
                if (_trail == null)
                {
                    _trail = gameObject.AddComponent<TrailRenderer>();
                    SetupTrailRenderer();
                }
            }

            // 物理設定
            _rigidbody.useGravity = true;
            _rigidbody.linearDamping = 0.1f;
        }

        /// <summary>
        /// 弾薬の初期化
        /// </summary>
        public void Initialize(Vector3 origin, Vector3 direction, WeaponData weaponData, System.Action<Projectile, ShotResult> onHitCallback)
        {
            transform.position = origin;
            transform.rotation = Quaternion.LookRotation(direction);
            
            _weaponData = weaponData;
            _speed = weaponData.ProjectileSpeed;
            _damage = weaponData.Stats.damage;
            _initialVelocity = direction.normalized * _speed;
            _onHitCallback = onHitCallback;
            _initialPosition = origin;
            _startTime = Time.time;
            _hasHit = false;

            // 重力の設定（3D物理では useGravity を使用）
            _rigidbody.useGravity = _gravityScale > 0f;
        }

        /// <summary>
        /// 弾薬の発射
        /// </summary>
        public void Fire()
        {
            _rigidbody.linearVelocity = _initialVelocity;
            
            // 寿命管理
            Invoke(nameof(OnLifeTimeExpired), _lifeTime);
        }

        private void Update()
        {
            // 進行方向への回転更新
            if (_rigidbody.linearVelocity.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;

            // 射手自身との衝突を無視
            if (other.transform.IsChildOf(transform.root)) return;

            ProcessHit(other);
        }

        /// <summary>
        /// ヒット処理
        /// </summary>
        private void ProcessHit(Collider hitCollider)
        {
            _hasHit = true;
            CancelInvoke(nameof(OnLifeTimeExpired));

            // ヒット位置の計算
            var hitPoint = transform.position;
            var hitNormal = (transform.position - hitCollider.transform.position).normalized;

            // ヒットボックスタイプの判定
            var hitboxType = DetermineHitboxType(hitCollider);

            // 距離による減衰ダメージの計算
            var distance = DistanceTraveled;
            var actualDamage = _weaponData.GetDamageAtDistance(distance);
            actualDamage = ApplyHitboxMultiplier(actualDamage, hitboxType);

            // RaycastHit 相当の情報を作成
            var hitInfo = new RaycastHit();
            // Note: RaycastHit は構造体で直接設定できないため、
            // 実際の実装では別の方法でヒット情報を作成する必要があります

            var result = new ShotResult
            {
                Hit = true,
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                HitCollider = hitCollider,
                Distance = distance,
                ActualDamage = actualDamage,
                HitboxType = hitboxType,
                HitGameObject = hitCollider.gameObject
            };

            // ダメージの適用
            ApplyDamage(hitCollider.gameObject, actualDamage, hitboxType);

            // ヒットエフェクトの生成
            CreateHitEffect(hitPoint, hitNormal);

            // コールバック実行
            _onHitCallback?.Invoke(this, result);

            Debug.Log($"Projectile Hit: {hitCollider.name}, Distance: {distance:F1}m, Damage: {actualDamage:F1}");
        }

        /// <summary>
        /// ヒットボックスタイプの判定
        /// </summary>
        private HitboxType DetermineHitboxType(Collider hitCollider)
        {
            var hitbox = hitCollider.GetComponent<Hitbox>();
            if (hitbox != null)
            {
                return hitbox.HitboxType;
            }

            // 名前による判定
            var colliderName = hitCollider.name.ToLower();
            if (colliderName.Contains("head")) return HitboxType.Head;
            if (colliderName.Contains("chest")) return HitboxType.Chest;
            if (colliderName.Contains("arm")) return HitboxType.Arms;
            if (colliderName.Contains("leg")) return HitboxType.Legs;

            return HitboxType.Chest; // デフォルト
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
        /// </summary>
        private void ApplyDamage(GameObject target, float damage, HitboxType hitboxType)
        {
            var healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                healthComponent = target.GetComponentInParent<HealthComponent>();
            }

            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);

                if (healthComponent is IHitboxAware hitboxAware)
                {
                    hitboxAware.OnHitboxHit(hitboxType, damage);
                }
            }
        }

        /// <summary>
        /// ヒットエフェクトの生成
        /// </summary>
        private void CreateHitEffect(Vector3 hitPoint, Vector3 hitNormal)
        {
            // TODO: ヒットエフェクトの実装
            // パーティクルシステムやデカルの生成
        }

        /// <summary>
        /// 寿命切れ時の処理
        /// </summary>
        private void OnLifeTimeExpired()
        {
            if (!_hasHit)
            {
                _onHitCallback?.Invoke(this, ShotResult.Miss);
            }
        }

        /// <summary>
        /// Trail Renderer の設定
        /// </summary>
        private void SetupTrailRenderer()
        {
            if (_trail != null)
            {
                _trail.time = 0.5f;
                _trail.startWidth = 0.01f;
                _trail.endWidth = 0.001f;
                _trail.material = new Material(Shader.Find("Sprites/Default"));
                _trail.material.color = Color.yellow;
            }
        }

        /// <summary>
        /// ObjectPool用のリセット処理
        /// </summary>
        public void Reset()
        {
            _hasHit = false;
            _weaponData = null;
            _onHitCallback = null;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            if (_trail != null)
            {
                _trail.Clear();
            }

            CancelInvoke();
        }
    }

    /// <summary>
    /// ObjectPool用のリセット可能インターフェース
    /// </summary>
    public interface IResettablePoolObject
    {
        void Reset();
    }
}
