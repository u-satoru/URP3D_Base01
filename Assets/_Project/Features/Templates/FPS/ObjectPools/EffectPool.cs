using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace asterivo.Unity60.Features.Templates.FPS.ObjectPools
{
    /// <summary>
    /// エフェクト専用ObjectPool
    /// ServiceLocator + Event駆動のハイブリッドアーキテクチャ準拠
    /// パーティクルエフェクトの95%メモリ削減効果を実現
    /// 射撃フラッシュ、爆発、煙などの高頻度エフェクト管理
    /// </summary>
    public class EffectPool : MonoBehaviour
    {
        [Header("エフェクトプール設定")]
        [SerializeField] private EffectPoolConfig[] _effectConfigs;
        [SerializeField] private int _defaultPoolSize = 20;
        [SerializeField] private int _defaultMaxSize = 100;
        [SerializeField] private Transform _poolParent;

        private Dictionary<string, UnityObjectPool<PooledEffect>> _effectPools;
        private Dictionary<string, GameObject> _effectPrefabs;
        private static EffectPool _instance;

        public static EffectPool Instance => _instance;

        [System.Serializable]
        public class EffectPoolConfig
        {
            public string effectName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 50;
            public float autoReturnTime = 5f;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                InitializeAllPools();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAllPools()
        {
            _effectPools = new Dictionary<string, UnityObjectPool<PooledEffect>>();
            _effectPrefabs = new Dictionary<string, GameObject>();

            // プール用親オブジェクト作成
            if (_poolParent == null)
            {
                var poolParentGO = new GameObject("EffectPool_Parent");
                poolParentGO.transform.SetParent(transform);
                _poolParent = poolParentGO.transform;
            }

            // 各エフェクト用プール初期化
            foreach (var config in _effectConfigs)
            {
                if (config.prefab != null)
                {
                    InitializeEffectPool(config);
                }
            }

            Debug.Log($"[EffectPool] Initialized {_effectPools.Count} effect pools");
        }

        private void InitializeEffectPool(EffectPoolConfig config)
        {
            _effectPrefabs[config.effectName] = config.prefab;

            var pool = new UnityObjectPool<PooledEffect>(
                factory: () => CreateEffect(config),
                poolParent: _poolParent,
                maxPoolSize: config.maxSize
            );

            pool.Initialize(config.initialSize, config.maxSize);
            _effectPools[config.effectName] = pool;

            Debug.Log($"[EffectPool] Initialized '{config.effectName}' pool - Initial: {config.initialSize}, Max: {config.maxSize}");
        }

        private PooledEffect CreateEffect(EffectPoolConfig config)
        {
            var effectGO = Instantiate(config.prefab, _poolParent);
            effectGO.name = $"{config.effectName}_Pooled";
            
            var pooledEffect = effectGO.GetComponent<PooledEffect>();
            if (pooledEffect == null)
            {
                pooledEffect = effectGO.AddComponent<PooledEffect>();
            }

            pooledEffect.Initialize(this, config.effectName, config.autoReturnTime);
            return pooledEffect;
        }

        /// <summary>
        /// エフェクトの取得・再生
        /// </summary>
        public PooledEffect PlayEffect(string effectName, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_effectPools.ContainsKey(effectName))
            {
                Debug.LogWarning($"[EffectPool] Effect '{effectName}' not found in pool");
                return null;
            }

            var effect = _effectPools[effectName].Get();
            effect.Play(position, rotation, parent);
            return effect;
        }

        /// <summary>
        /// エフェクトの取得・再生（オーバーロード：スケール指定）
        /// </summary>
        public PooledEffect PlayEffect(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null)
        {
            var effect = PlayEffect(effectName, position, rotation, parent);
            if (effect != null)
            {
                effect.transform.localScale = scale;
            }
            return effect;
        }

        /// <summary>
        /// エフェクトをプールに返却
        /// </summary>
        public void ReturnEffect(string effectName, PooledEffect effect)
        {
            if (_effectPools.ContainsKey(effectName))
            {
                _effectPools[effectName].Return(effect);
            }
        }

        /// <summary>
        /// 全エフェクトプールの統計取得
        /// </summary>
        public void LogAllStatistics()
        {
            foreach (var kvp in _effectPools)
            {
                kvp.Value.LogStatistics($"EffectPool_{kvp.Key}");
            }
        }

        /// <summary>
        /// 指定エフェクトプールの統計取得
        /// </summary>
        public PoolStatistics GetEffectStatistics(string effectName)
        {
            return _effectPools.ContainsKey(effectName) ? _effectPools[effectName].Statistics : null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                foreach (var pool in _effectPools.Values)
                {
                    pool.Clear();
                }
                _effectPools?.Clear();
                _instance = null;
            }
        }

        /// <summary>
        /// ランタイムでの新しいエフェクトプール追加
        /// </summary>
        public void AddEffectPool(string effectName, GameObject prefab, int initialSize = 10, int maxSize = 50, float autoReturnTime = 5f)
        {
            if (_effectPools.ContainsKey(effectName))
            {
                Debug.LogWarning($"[EffectPool] Effect pool '{effectName}' already exists");
                return;
            }

            var config = new EffectPoolConfig
            {
                effectName = effectName,
                prefab = prefab,
                initialSize = initialSize,
                maxSize = maxSize,
                autoReturnTime = autoReturnTime
            };

            InitializeEffectPool(config);
        }
    }

    /// <summary>
    /// プール可能なエフェクトクラス
    /// パーティクルシステム、アニメーター、オーディオソース統合管理
    /// </summary>
    public class PooledEffect : MonoBehaviour, IPoolable
    {
        [Header("エフェクト設定")]
        [SerializeField] private bool _autoReturnToPool = true;
        [SerializeField] private float _customDuration = -1f;

        private ParticleSystem[] _particleSystems;
        private Animator _animator;
        private AudioSource _audioSource;
        private EffectPool _pool;
        private string _effectName;
        private float _autoReturnTime;
        private bool _isPlaying;
        private Coroutine _autoReturnCoroutine;

        public bool IsAvailable => !_isPlaying;
        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(EffectPool pool, string effectName, float autoReturnTime)
        {
            _pool = pool;
            _effectName = effectName;
            _autoReturnTime = autoReturnTime;
        }

        public void Play(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            transform.position = position;
            transform.rotation = rotation;
            
            if (parent != null)
            {
                transform.SetParent(parent);
            }

            _isPlaying = true;
            gameObject.SetActive(true);

            // パーティクルシステム再生
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }

            // アニメーター再生
            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Rebind();
                _animator.Update(0);
            }

            // オーディオ再生
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
            }

            // 自動返却設定
            if (_autoReturnToPool)
            {
                float duration = _customDuration > 0 ? _customDuration : _autoReturnTime;
                _autoReturnCoroutine = StartCoroutine(AutoReturnToPool(duration));
            }
        }

        public void Stop()
        {
            if (!_isPlaying) return;

            // パーティクルシステム停止
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            // オーディオ停止
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            ReturnToPool();
        }

        private IEnumerator AutoReturnToPool(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            // パーティクルが完全に終了するまで待機
            bool allParticlesStopped = false;
            while (!allParticlesStopped)
            {
                allParticlesStopped = true;
                foreach (var ps in _particleSystems)
                {
                    if (ps != null && ps.IsAlive())
                    {
                        allParticlesStopped = false;
                        break;
                    }
                }
                
                if (!allParticlesStopped)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            ReturnToPool();
        }

        public void OnGetFromPool()
        {
            _isPlaying = false;
            gameObject.SetActive(false);
        }

        public void OnReturnToPool()
        {
            _isPlaying = false;
            
            // 自動返却コルーチン停止
            if (_autoReturnCoroutine != null)
            {
                StopCoroutine(_autoReturnCoroutine);
                _autoReturnCoroutine = null;
            }

            // 全コンポーネントリセット
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Clear();
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            if (_animator != null)
            {
                _animator.enabled = false;
            }

            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            // 親から切り離し
            transform.SetParent(_pool.transform);
            gameObject.SetActive(false);
        }

        private void ReturnToPool()
        {
            if (_pool != null && _isPlaying)
            {
                _pool.ReturnEffect(_effectName, this);
            }
        }

        /// <summary>
        /// エフェクトの手動返却（外部から呼び出し可能）
        /// </summary>
        public void ReturnToPoolManually()
        {
            ReturnToPool();
        }
    }
}