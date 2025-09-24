using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Features.ActionRPG.Data;

namespace asterivo.Unity60.Features.ActionRPG.Components
{
    /// <summary>
    /// 敵が倒された時のルートドロップを管理するコンポーネント
    /// HealthComponentの死亡イベントを受け取ってアイテムを生成します
    /// </summary>
    public class LootDropComponent : MonoBehaviour
    {
        [Header("ドロップ設定")]
        [SerializeField] private EnemyData _enemyData;
        [SerializeField] private Transform _dropPoint;
        
        [Header("ドロップエフェクト")]
        [SerializeField] private GameObject _dropEffectPrefab;
        [SerializeField] private AudioClip _dropSound;
        
        [Header("イベント発行")]
        [SerializeField] private GameEvent _onLootDropped;
        [SerializeField] private GameEvent<int> _onRuneDropped; // ルーン獲得量を通知

        // コンポーネント参照
        private asterivo.Unity60.Core.Combat.HealthComponent _healthComponent;
        private AudioSource _audioSource;
        
        // ドロップ済みフラグ
        private bool _hasDropped;

        void Awake()
        {
            _healthComponent = GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            _audioSource = GetComponent<AudioSource>();

            // ドロップポイントが設定されていない場合は自分の位置を使用
            if (_dropPoint == null)
                _dropPoint = transform;
        }

        void OnEnable()
        {
            // HealthComponentの死亡イベントを受信
            if (_healthComponent != null)
            {
                _healthComponent.OnDied += OnEnemyDeath;
            }
        }

        void OnDisable()
        {
            // イベント受信解除
            if (_healthComponent != null)
            {
                _healthComponent.OnDied -= OnEnemyDeath;
            }
        }

        /// <summary>
        /// 敵の死亡時に呼ばれる
        /// </summary>
        private void OnEnemyDeath()
        {
            if (_hasDropped || _enemyData == null) return;

            DropLoot();
            _hasDropped = true;
        }

        /// <summary>
        /// ルートをドロップする
        /// </summary>
        private void DropLoot()
        {
            // ドロップ確率チェック
            float dropRoll = Random.Range(0f, 1f);
            if (dropRoll > _enemyData.RuneDropChance) return;

            // ルーンドロップ量を計算（レベルスケーリング考慮）
            int dropAmount = CalculateDropAmount();
            
            if (dropAmount > 0)
            {
                CreateRunePickup(dropAmount);
                PlayDropEffects();
                
                // イベント発行
                if (_onLootDropped != null)
                    _onLootDropped.Raise();
                    
                if (_onRuneDropped != null)
                    _onRuneDropped.Raise(dropAmount);

                Debug.Log($"{gameObject.name}がルーン x{dropAmount} をドロップしました。");
            }
        }

        /// <summary>
        /// ドロップ量を計算
        /// </summary>
        private int CalculateDropAmount()
        {
            if (_enemyData == null) return 0;

            // 基本ドロップ量
            int baseAmount = _enemyData.RuneDropAmount;
            
            // レベルスケーリング（TODO: レベル情報を取得）
            int currentLevel = 1; // 暫定値
            int scaledAmount = _enemyData.GetScaledRuneDropAmount(currentLevel);
            
            // ランダム要素を追加（±20%）
            float randomFactor = Random.Range(0.8f, 1.2f);
            int finalAmount = Mathf.RoundToInt(scaledAmount * randomFactor);
            
            return Mathf.Max(1, finalAmount); // 最低1個は保証
        }

        /// <summary>
        /// ルーンピックアップオブジェクトを生成
        /// </summary>
        private void CreateRunePickup(int amount)
        {
            if (_enemyData.DropItemPrefab == null) return;

            // ドロップ位置を少しランダムにする
            Vector3 dropPosition = _dropPoint.position;
            dropPosition += Random.insideUnitSphere * 1.5f;
            dropPosition.y = _dropPoint.position.y; // Y座標は固定

            // ピックアップオブジェクト生成
            GameObject pickup = Instantiate(_enemyData.DropItemPrefab, dropPosition, Quaternion.identity);
            
            // ルーン数を設定（RunePickupコンポーネントがあると仮定）
            var runePickup = pickup.GetComponent<RunePickup>();
            if (runePickup != null)
            {
                runePickup.SetRuneAmount(amount);
            }

            // 軽い上方向の力を加える（視覚的効果）
            var rigidbody = pickup.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Vector3 force = Vector3.up * Random.Range(2f, 4f) + Random.insideUnitSphere * 1f;
                rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// ドロップエフェクトとサウンドを再生
        /// </summary>
        private void PlayDropEffects()
        {
            // エフェクト生成
            if (_dropEffectPrefab != null)
            {
                GameObject effect = Instantiate(_dropEffectPrefab, _dropPoint.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // ドロップ音再生
            if (_audioSource != null && _dropSound != null)
            {
                _audioSource.PlayOneShot(_dropSound);
            }
        }

        /// <summary>
        /// 外部から敵データを設定
        /// </summary>
        public void SetEnemyData(EnemyData enemyData)
        {
            _enemyData = enemyData;
        }

        /// <summary>
        /// 手動でドロップを実行（テスト用）
        /// </summary>
        [ContextMenu("Force Drop Loot")]
        public void ForceDropLoot()
        {
            if (!_hasDropped)
            {
                DropLoot();
                _hasDropped = true;
            }
        }
    }

    /// <summary>
    /// ルーンピックアップオブジェクト用コンポーネント
    /// プレイヤーとのインタラクションを処理します
    /// </summary>
    public class RunePickup : MonoBehaviour
    {
        [Header("ルーン設定")]
        [SerializeField] private int _runeAmount = 100;
        [SerializeField] private asterivo.Unity60.Features.ActionRPG.Data.ItemData _runeItemData;
        
        [Header("ピックアップ設定")]
        [SerializeField] private float _pickupRange = 2.0f;
        [SerializeField] private bool _autoPickup = true;
        
        [Header("視覚効果")]
        [SerializeField] private float _rotateSpeed = 45f;
        [SerializeField] private float _bobHeight = 0.2f;
        [SerializeField] private float _bobSpeed = 2f;
        
        [Header("エフェクト")]
        [SerializeField] private GameObject _pickupEffectPrefab;
        [SerializeField] private AudioClip _pickupSound;
        
        [Header("イベント")]
        [SerializeField] private GameEvent<int> _onResourceCollected;

        // 内部状態
        private Vector3 _startPosition;
        private float _bobTimer;
        private bool _isPickedUp;
        private AudioSource _audioSource;

        void Start()
        {
            _startPosition = transform.position;
            _audioSource = GetComponent<AudioSource>();
            
            // 自動消滅タイマー（30秒後）
            Destroy(gameObject, 30f);
        }

        void Update()
        {
            if (_isPickedUp) return;

            // 回転アニメーション
            transform.Rotate(Vector3.up, _rotateSpeed * Time.deltaTime);
            
            // 上下移動アニメーション
            _bobTimer += Time.deltaTime * _bobSpeed;
            Vector3 bobOffset = Vector3.up * Mathf.Sin(_bobTimer) * _bobHeight;
            transform.position = _startPosition + bobOffset;

            // 自動ピックアップ判定
            if (_autoPickup)
            {
                CheckForPlayer();
            }
        }

        /// <summary>
        /// プレイヤーの接近をチェック
        /// </summary>
        private void CheckForPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _pickupRange);
            
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    TryPickup(col.gameObject);
                    break;
                }
            }
        }

        /// <summary>
        /// ピックアップを試行
        /// </summary>
        public void TryPickup(GameObject player)
        {
            if (_isPickedUp) return;

            var inventory = player.GetComponent<InventoryComponent>();
            if (inventory != null && _runeItemData != null)
            {
                // インベントリにアイテムを追加
                if (inventory.AddItem(_runeItemData, _runeAmount))
                {
                    PerformPickup();
                }
                else
                {
                    Debug.Log("インベントリがいっぱいです！");
                }
            }
        }

        /// <summary>
        /// ピックアップを実行
        /// </summary>
        private void PerformPickup()
        {
            _isPickedUp = true;

            // エフェクト再生
            PlayPickupEffects();

            // イベント発行
            if (_onResourceCollected != null)
                _onResourceCollected.Raise(_runeAmount);

            // オブジェクト削除
            Destroy(gameObject, 0.1f);
        }

        /// <summary>
        /// ピックアップエフェクトとサウンドを再生
        /// </summary>
        private void PlayPickupEffects()
        {
            // エフェクト生成
            if (_pickupEffectPrefab != null)
            {
                GameObject effect = Instantiate(_pickupEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // ピックアップ音再生
            if (_audioSource != null && _pickupSound != null)
            {
                _audioSource.PlayOneShot(_pickupSound);
            }
        }

        /// <summary>
        /// ルーン量を設定
        /// </summary>
        public void SetRuneAmount(int amount)
        {
            _runeAmount = amount;
        }

        /// <summary>
        /// ルーンItemDataを設定
        /// </summary>
        public void SetRuneItemData(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData)
        {
            _runeItemData = itemData;
        }

        void OnTriggerEnter(Collider other)
        {
            // 手動ピックアップモードの場合
            if (!_autoPickup && other.CompareTag("Player"))
            {
                TryPickup(other.gameObject);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _pickupRange);
        }
    }
}