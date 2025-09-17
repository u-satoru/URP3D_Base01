using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// 環境隠蔽ゾーンコンポーネント
    /// プレイヤーが隠れることができる環境エリアを定義
    /// 草むら、ロッカー、コンテナ、影などの隠蔽ポイント
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnvironmentConcealmentZone : MonoBehaviour, IConcealmentZone
    {
        [Header("Concealment Configuration")]
        [SerializeField, Range(0.0f, 1.0f)]
        private float _concealmentStrength = 0.8f;

        [SerializeField]
        private ConcealmentType _zoneType = ConcealmentType.TallGrass;

        [SerializeField]
        private bool _isActive = true;

        [SerializeField]
        private bool _requiresPlayerAction = false; // ロッカーなどは入る操作が必要

        [Header("Zone Behavior")]
        [SerializeField]
        private float _entryDelay = 0.0f; // 隠蔽効果が発動するまでの遅延

        [SerializeField]
        private float _exitDelay = 0.5f; // 隠蔽効果が切れるまでの遅延

        [SerializeField]
        private bool _hidePlayerModel = false; // プレイヤーモデルを非表示にするか

        [Header("Visual and Audio Effects")]
        [SerializeField]
        private ParticleSystem _entryEffect;

        [SerializeField]
        private ParticleSystem _exitEffect;

        [SerializeField]
        private AudioClip _entrySound;

        [SerializeField]
        private AudioClip _exitSound;

        [Header("Detection")]
        [SerializeField]
        private LayerMask _playerLayerMask = -1;

        [SerializeField]
        private string _playerTag = "Player";

        [Header("Debug")]
        [SerializeField]
        private bool _enableDebugLogs = true;

        [SerializeField]
        private bool _showGizmos = true;

        [SerializeField]
        private Color _gizmoColor = Color.blue;

        // Private fields
        private Collider _zoneCollider;
        private IStealthService _stealthService;
        private bool _playerInZone = false;
        private float _entryTime = 0.0f;
        private GameObject _currentPlayer;
        private Renderer _playerRenderer;

        // Events
        private PlayerConcealmentEvent _concealmentEvent;

        #region IConcealmentZone Implementation
        public float ConcealmentStrength => _isActive ? _concealmentStrength : 0.0f;
        public ConcealmentType ZoneType => _zoneType;
        public bool IsActive => _isActive && _playerInZone;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeServices();
            InitializeEvents();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerObject(other.gameObject))
            {
                OnPlayerEntered(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayerObject(other.gameObject) && other.gameObject == _currentPlayer)
            {
                OnPlayerExited(other.gameObject);
            }
        }

        private void Update()
        {
            if (_playerInZone && _entryDelay > 0.0f)
            {
                UpdateEntryDelay();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showGizmos) return;

            DrawDebugGizmos();
        }
        #endregion

        #region Initialization
        private void InitializeComponents()
        {
            _zoneCollider = GetComponent<Collider>();
            if (_zoneCollider == null)
            {
                Debug.LogError($"[{gameObject.name}] Collider component required for EnvironmentConcealmentZone!");
                return;
            }

            // トリガーとして設定
            _zoneCollider.isTrigger = true;
        }

        private void InitializeServices()
        {
            // ServiceLocatorからStealthServiceを取得
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning($"[{gameObject.name}] StealthService not found in ServiceLocator");
            }
        }

        private void InitializeEvents()
        {
            // イベントの初期化
            _concealmentEvent = ScriptableObject.CreateInstance<PlayerConcealmentEvent>();
            _concealmentEvent.name = $"ConcealmentEvent_{gameObject.name}";
        }
        #endregion

        #region Player Detection
        private bool IsPlayerObject(GameObject obj)
        {
            if (obj == null) return false;

            // レイヤーマスクチェック
            if (((1 << obj.layer) & _playerLayerMask) == 0) return false;

            // タグチェック
            if (!string.IsNullOrEmpty(_playerTag) && !obj.CompareTag(_playerTag)) return false;

            return true;
        }

        private void OnPlayerEntered(GameObject player)
        {
            if (_requiresPlayerAction)
            {
                // プレイヤーのアクションが必要な場合（ロッカーなど）
                ShowInteractionPrompt();
                return;
            }

            StartConcealmentProcess(player);
        }

        private void OnPlayerExited(GameObject player)
        {
            if (_currentPlayer == player)
            {
                StopConcealmentProcess();
            }
        }

        private void StartConcealmentProcess(GameObject player)
        {
            _playerInZone = true;
            _currentPlayer = player;
            _entryTime = Time.time;

            // プレイヤーレンダラーの取得
            _playerRenderer = player.GetComponent<Renderer>();

            // StealthServiceに通知
            _stealthService?.EnterConcealmentZone(this);

            // エフェクトの再生
            PlayEntryEffects();

            // イベント発行
            RaiseConcealmentEvent(true);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Player entered concealment zone: {_zoneType}");
        }

        private void StopConcealmentProcess()
        {
            if (!_playerInZone) return;

            _playerInZone = false;

            // StealthServiceに通知
            _stealthService?.ExitConcealmentZone(this);

            // プレイヤーモデルの表示を復元
            if (_hidePlayerModel && _playerRenderer != null)
            {
                _playerRenderer.enabled = true;
            }

            // エフェクトの再生
            PlayExitEffects();

            // イベント発行
            RaiseConcealmentEvent(false);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Player exited concealment zone: {_zoneType}");

            // リセット
            _currentPlayer = null;
            _playerRenderer = null;
        }

        private void UpdateEntryDelay()
        {
            if (Time.time - _entryTime >= _entryDelay)
            {
                // 遅延時間が経過したら隠蔽効果を発動
                ActivateConcealmentEffects();
            }
        }

        private void ActivateConcealmentEffects()
        {
            // プレイヤーモデルの非表示
            if (_hidePlayerModel && _playerRenderer != null)
            {
                _playerRenderer.enabled = false;
            }

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Concealment effects activated");
        }
        #endregion

        #region Player Interaction (for zones requiring action)
        private void ShowInteractionPrompt()
        {
            // UI表示やプロンプト表示の処理
            // これは後でUI/UXシステムで実装
            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Interaction prompt shown for {_zoneType}");
        }

        /// <summary>
        /// プレイヤーアクションによる隠蔽開始（ロッカーなど）
        /// </summary>
        public void ActivateByPlayerAction(GameObject player)
        {
            if (!_requiresPlayerAction) return;
            if (!IsPlayerObject(player)) return;

            StartConcealmentProcess(player);
        }

        /// <summary>
        /// プレイヤーアクションによる隠蔽終了
        /// </summary>
        public void DeactivateByPlayerAction()
        {
            if (!_requiresPlayerAction) return;

            StopConcealmentProcess();
        }
        #endregion

        #region Effects and Audio
        private void PlayEntryEffects()
        {
            // パーティクルエフェクト
            if (_entryEffect != null)
            {
                _entryEffect.Play();
            }

            // オーディオエフェクト
            if (_entrySound != null)
            {
                AudioSource.PlayClipAtPoint(_entrySound, transform.position);
            }
        }

        private void PlayExitEffects()
        {
            // パーティクルエフェクト
            if (_exitEffect != null)
            {
                _exitEffect.Play();
            }

            // オーディオエフェクト
            if (_exitSound != null)
            {
                AudioSource.PlayClipAtPoint(_exitSound, transform.position);
            }
        }
        #endregion

        #region Events
        private void RaiseConcealmentEvent(bool isEntering)
        {
            if (_concealmentEvent == null) return;

            var data = new PlayerConcealmentData
            {
                ZoneType = _zoneType,
                ConcealmentStrength = _concealmentStrength,
                IsEntering = isEntering,
                ZonePosition = transform.position,
                ZoneSize = _zoneCollider.bounds.size
            };

            _concealmentEvent.Raise(data);
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// 隠蔽ゾーンの有効/無効を切り替え
        /// </summary>
        /// <param name="active">有効にするかどうか</param>
        public void SetActive(bool active)
        {
            bool wasActive = _isActive;
            _isActive = active;

            if (wasActive && !_isActive && _playerInZone)
            {
                // 無効化された場合、プレイヤーが中にいても隠蔽効果を停止
                StopConcealmentProcess();
            }

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Concealment zone {(_isActive ? "activated" : "deactivated")}");
        }

        /// <summary>
        /// 隠蔽強度を動的に変更
        /// </summary>
        /// <param name="newStrength">新しい隠蔽強度 (0.0 - 1.0)</param>
        public void SetConcealmentStrength(float newStrength)
        {
            _concealmentStrength = Mathf.Clamp01(newStrength);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Concealment strength changed to: {_concealmentStrength:F2}");
        }

        /// <summary>
        /// 現在プレイヤーがゾーン内にいるかどうか
        /// </summary>
        public bool HasPlayerInside => _playerInZone;

        /// <summary>
        /// ゾーン内のプレイヤーオブジェクト
        /// </summary>
        public GameObject CurrentPlayer => _currentPlayer;
        #endregion

        #region Debug
        private void DrawDebugGizmos()
        {
            Color originalColor = Gizmos.color;
            Gizmos.color = _gizmoColor;

            if (_zoneCollider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 size = boxCollider.size;
                Vector3 center = boxCollider.center;

                if (_isActive)
                {
                    Gizmos.DrawCube(center, size);
                }
                else
                {
                    Gizmos.DrawWireCube(center, size);
                }
            }
            else if (_zoneCollider is SphereCollider sphereCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 center = sphereCollider.center;
                float radius = sphereCollider.radius;

                if (_isActive)
                {
                    Gizmos.DrawSphere(center, radius);
                }
                else
                {
                    Gizmos.DrawWireSphere(center, radius);
                }
            }

            // 隠蔽強度の可視化
            if (_playerInZone)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, _concealmentStrength * 2.0f);
            }

            Gizmos.color = originalColor;
            Gizmos.matrix = Matrix4x4.identity;
        }
        #endregion
    }
}