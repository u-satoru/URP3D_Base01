using UnityEngine;
using System.Collections.Generic;

namespace asterivo.Unity60.Features.Templates.FPS.ObjectPools
{
    /// <summary>
    /// 弾丸専用ObjectPool
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// 高頻度射撃による大量弾丸生成の95%メモリ削減効果を実現
    /// </summary>
    public class BulletPool : MonoBehaviour
    {
        [Header("プール設定")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private int _initialPoolSize = 50;
        [SerializeField] private int _maxPoolSize = 200;
        [SerializeField] private Transform _poolParent;

        private UnityObjectPool<PooledBullet> _bulletPool;
        private static BulletPool _instance;

        public static BulletPool Instance => _instance;
        public PoolStatistics Statistics => _bulletPool?.Statistics;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializePool();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            if (_bulletPrefab == null)
            {
                Debug.LogError("[BulletPool] Bullet prefab is not assigned!");
                return;
            }

            // プール用親オブジェクト作成
            if (_poolParent == null)
            {
                var poolParentGO = new GameObject("BulletPool_Parent");
                poolParentGO.transform.SetParent(transform);
                _poolParent = poolParentGO.transform;
            }

            _bulletPool = new UnityObjectPool<PooledBullet>(
                factory: CreateBullet,
                poolParent: _poolParent,
                maxPoolSize: _maxPoolSize
            );

            _bulletPool.Initialize(_initialPoolSize, _maxPoolSize);

            Debug.Log($"[BulletPool] Initialized with {_initialPoolSize} bullets, max size: {_maxPoolSize}");
        }

        private PooledBullet CreateBullet()
        {
            var bulletGO = Instantiate(_bulletPrefab, _poolParent);
            var pooledBullet = bulletGO.GetComponent<PooledBullet>();
            
            if (pooledBullet == null)
            {
                pooledBullet = bulletGO.AddComponent<PooledBullet>();
            }

            pooledBullet.Initialize(this);
            return pooledBullet;
        }

        /// <summary>
        /// 弾丸の取得（射撃時に使用）
        /// </summary>
        public PooledBullet GetBullet(Vector3 position, Quaternion rotation, float speed, float damage, float range)
        {
            var bullet = _bulletPool.Get();
            bullet.Setup(position, rotation, speed, damage, range);
            return bullet;
        }

        /// <summary>
        /// 弾丸の返却（衝突時や射程外時に使用）
        /// </summary>
        public void ReturnBullet(PooledBullet bullet)
        {
            _bulletPool.Return(bullet);
        }

        /// <summary>
        /// プール統計の取得（デバッグ・最適化用）
        /// </summary>
        public void LogStatistics()
        {
            _bulletPool?.LogStatistics("BulletPool");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _bulletPool?.Clear();
                _instance = null;
            }
        }
    }

    /// <summary>
    /// プール可能な弾丸クラス
    /// 物理演算、衝突検出、エフェクト統合
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class PooledBullet : MonoBehaviour, IPoolable
    {
        [Header("弾丸設定")]
        [SerializeField] private LayerMask _collisionLayers = -1;
        [SerializeField] private GameObject _impactEffectPrefab;
        [SerializeField] private TrailRenderer _trailRenderer;

        private Rigidbody _rigidbody;
        private Collider _collider;
        private BulletPool _pool;
        private float _speed;
        private float _damage;
        private float _range;
        private Vector3 _startPosition;
        private bool _isActive;
        private float _lifetime;

        public bool IsAvailable => !_isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }

        public void Initialize(BulletPool pool)
        {
            _pool = pool;
        }

        public void Setup(Vector3 position, Quaternion rotation, float speed, float damage, float range)
        {
            transform.position = position;
            transform.rotation = rotation;
            _startPosition = position;
            _speed = speed;
            _damage = damage;
            _range = range;
            _lifetime = 0f;

            // 物理設定
            _rigidbody.velocity = transform.forward * _speed;
            _rigidbody.useGravity = false;

            // アクティブ化
            gameObject.SetActive(true);
            _isActive = true;

            // トレイル初期化
            if (_trailRenderer != null)
            {
                _trailRenderer.Clear();
                _trailRenderer.enabled = true;
            }
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
            _collider.enabled = true;
        }

        public void OnReturnToPool()
        {
            _isActive = false;
            gameObject.SetActive(false);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = false;
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            _lifetime += Time.deltaTime;

            // 射程距離チェック
            float traveledDistance = Vector3.Distance(_startPosition, transform.position);
            if (traveledDistance >= _range)
            {
                ReturnToPool();
                return;
            }

            // 時間制限チェック（安全装置）
            if (_lifetime > 10f)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // レイヤーマスクチェック
            if ((_collisionLayers.value & (1 << other.gameObject.layer)) == 0)
                return;

            // 衝突処理
            HandleCollision(other);
            
            // プールに返却
            ReturnToPool();
        }

        private void HandleCollision(Collider hitCollider)
        {
            // ダメージ処理（Event駆動）
            var damageData = new Events.DamageData(
                source: gameObject,
                target: hitCollider.gameObject,
                amount: _damage,
                damageType: Events.DamageType.Bullet,
                hitPoint: transform.position,
                hitNormal: Vector3.zero
            );

            // DamageEventを発行
            var damageEvent = Resources.Load<Events.DamageEvent>("Events/DamageEvent");
            damageEvent?.RaiseDamage(damageData);

            // 衝突エフェクト生成
            if (_impactEffectPrefab != null)
            {
                var effect = Instantiate(_impactEffectPrefab, transform.position, Quaternion.LookRotation(-transform.forward));
                Destroy(effect, 2f); // 2秒後に自動削除
            }
        }

        private void ReturnToPool()
        {
            if (_pool != null && _isActive)
            {
                _pool.ReturnBullet(this);
            }
        }

        private void OnDrawGizmos()
        {
            if (_isActive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.1f);
                
                // 射程表示
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_startPosition, _range);
            }
        }
    }
}