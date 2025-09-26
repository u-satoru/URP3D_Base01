using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Stealth.Services;
using asterivo.Unity60.Features.Templates.Stealth.Events;

namespace asterivo.Unity60.Features.Templates.Stealth.Environment
{
    /// <summary>
    /// 迺ｰ蠅・國阡ｽ繧ｾ繝ｼ繝ｳ繧ｳ繝ｳ繝昴・繝阪Φ繝・
    /// 繝励Ξ繧､繝､繝ｼ縺碁國繧後ｋ縺薙→縺後〒縺阪ｋ迺ｰ蠅・お繝ｪ繧｢繧貞ｮ夂ｾｩ
    /// 闕峨・繧峨√Ο繝・き繝ｼ縲√さ繝ｳ繝・リ縲∝ｽｱ縺ｪ縺ｩ縺ｮ髫阡ｽ繝昴う繝ｳ繝・
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
        private bool _requiresPlayerAction = false; // 繝ｭ繝・き繝ｼ縺ｪ縺ｩ縺ｯ蜈･繧区桃菴懊′蠢・ｦ・

        [Header("Zone Behavior")]
        [SerializeField]
        private float _entryDelay = 0.0f; // 髫阡ｽ蜉ｹ譫懊′逋ｺ蜍輔☆繧九∪縺ｧ縺ｮ驕・ｻｶ

        [SerializeField]
        private float _exitDelay = 0.5f; // 髫阡ｽ蜉ｹ譫懊′蛻・ｌ繧九∪縺ｧ縺ｮ驕・ｻｶ

        [SerializeField]
        private bool _hidePlayerModel = false; // 繝励Ξ繧､繝､繝ｼ繝｢繝・Ν繧帝撼陦ｨ遉ｺ縺ｫ縺吶ｋ縺・

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

            // 繝医Μ繧ｬ繝ｼ縺ｨ縺励※險ｭ螳・
            _zoneCollider.isTrigger = true;
        }

        private void InitializeServices()
        {
            // ServiceLocator縺九ｉStealthService繧貞叙蠕・
            _stealthService = ServiceLocator.GetService<IStealthService>();
            if (_stealthService == null)
            {
                Debug.LogWarning($"[{gameObject.name}] StealthService not found in ServiceLocator");
            }
        }

        private void InitializeEvents()
        {
            // 繧､繝吶Φ繝医・蛻晄悄蛹・
            _concealmentEvent = ScriptableObject.CreateInstance<PlayerConcealmentEvent>();
            _concealmentEvent.name = $"ConcealmentEvent_{gameObject.name}";
        }
        #endregion

        #region Player Detection
        private bool IsPlayerObject(GameObject obj)
        {
            if (obj == null) return false;

            // 繝ｬ繧､繝､繝ｼ繝槭せ繧ｯ繝√ぉ繝・け
            if (((1 << obj.layer) & _playerLayerMask) == 0) return false;

            // 繧ｿ繧ｰ繝√ぉ繝・け
            if (!string.IsNullOrEmpty(_playerTag) && !obj.CompareTag(_playerTag)) return false;

            return true;
        }

        private void OnPlayerEntered(GameObject player)
        {
            if (_requiresPlayerAction)
            {
                // 繝励Ξ繧､繝､繝ｼ縺ｮ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺悟ｿ・ｦ√↑蝣ｴ蜷茨ｼ医Ο繝・き繝ｼ縺ｪ縺ｩ・・
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

            // 繝励Ξ繧､繝､繝ｼ繝ｬ繝ｳ繝繝ｩ繝ｼ縺ｮ蜿門ｾ・
            _playerRenderer = player.GetComponent<Renderer>();

            // StealthService縺ｫ騾夂衍
            _stealthService?.EnterConcealmentZone(this);

            // 繧ｨ繝輔ぉ繧ｯ繝医・蜀咲函
            PlayEntryEffects();

            // 繧､繝吶Φ繝育匱陦・
            RaiseConcealmentEvent(true);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Player entered concealment zone: {_zoneType}");
        }

        private void StopConcealmentProcess()
        {
            if (!_playerInZone) return;

            _playerInZone = false;

            // StealthService縺ｫ騾夂衍
            _stealthService?.ExitConcealmentZone(this);

            // 繝励Ξ繧､繝､繝ｼ繝｢繝・Ν縺ｮ陦ｨ遉ｺ繧貞ｾｩ蜈・
            if (_hidePlayerModel && _playerRenderer != null)
            {
                _playerRenderer.enabled = true;
            }

            // 繧ｨ繝輔ぉ繧ｯ繝医・蜀咲函
            PlayExitEffects();

            // 繧､繝吶Φ繝育匱陦・
            RaiseConcealmentEvent(false);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Player exited concealment zone: {_zoneType}");

            // 繝ｪ繧ｻ繝・ヨ
            _currentPlayer = null;
            _playerRenderer = null;
        }

        private void UpdateEntryDelay()
        {
            if (Time.time - _entryTime >= _entryDelay)
            {
                // 驕・ｻｶ譎る俣縺檎ｵ碁℃縺励◆繧蛾國阡ｽ蜉ｹ譫懊ｒ逋ｺ蜍・
                ActivateConcealmentEffects();
            }
        }

        private void ActivateConcealmentEffects()
        {
            // 繝励Ξ繧､繝､繝ｼ繝｢繝・Ν縺ｮ髱櫁｡ｨ遉ｺ
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
            // UI陦ｨ遉ｺ繧・・繝ｭ繝ｳ繝励ヨ陦ｨ遉ｺ縺ｮ蜃ｦ逅・
            // 縺薙ｌ縺ｯ蠕後〒UI/UX繧ｷ繧ｹ繝・Β縺ｧ螳溯｣・
            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Interaction prompt shown for {_zoneType}");
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｫ繧医ｋ髫阡ｽ髢句ｧ具ｼ医Ο繝・き繝ｼ縺ｪ縺ｩ・・
        /// </summary>
        public void ActivateByPlayerAction(GameObject player)
        {
            if (!_requiresPlayerAction) return;
            if (!IsPlayerObject(player)) return;

            StartConcealmentProcess(player);
        }

        /// <summary>
        /// 繝励Ξ繧､繝､繝ｼ繧｢繧ｯ繧ｷ繝ｧ繝ｳ縺ｫ繧医ｋ髫阡ｽ邨ゆｺ・
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
            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・
            if (_entryEffect != null)
            {
                _entryEffect.Play();
            }

            // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｨ繝輔ぉ繧ｯ繝・
            if (_entrySound != null)
            {
                AudioSource.PlayClipAtPoint(_entrySound, transform.position);
            }
        }

        private void PlayExitEffects()
        {
            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・
            if (_exitEffect != null)
            {
                _exitEffect.Play();
            }

            // 繧ｪ繝ｼ繝・ぅ繧ｪ繧ｨ繝輔ぉ繧ｯ繝・
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
        /// 髫阡ｽ繧ｾ繝ｼ繝ｳ縺ｮ譛牙柑/辟｡蜉ｹ繧貞・繧頑崛縺・
        /// </summary>
        /// <param name="active">譛牙柑縺ｫ縺吶ｋ縺九←縺・°</param>
        public void SetActive(bool active)
        {
            bool wasActive = _isActive;
            _isActive = active;

            if (wasActive && !_isActive && _playerInZone)
            {
                // 辟｡蜉ｹ蛹悶＆繧後◆蝣ｴ蜷医√・繝ｬ繧､繝､繝ｼ縺御ｸｭ縺ｫ縺・※繧る國阡ｽ蜉ｹ譫懊ｒ蛛懈ｭ｢
                StopConcealmentProcess();
            }

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Concealment zone {(_isActive ? "activated" : "deactivated")}");
        }

        /// <summary>
        /// 髫阡ｽ蠑ｷ蠎ｦ繧貞虚逧・↓螟画峩
        /// </summary>
        /// <param name="newStrength">譁ｰ縺励＞髫阡ｽ蠑ｷ蠎ｦ (0.0 - 1.0)</param>
        public void SetConcealmentStrength(float newStrength)
        {
            _concealmentStrength = Mathf.Clamp01(newStrength);

            if (_enableDebugLogs)
                Debug.Log($"[{gameObject.name}] Concealment strength changed to: {_concealmentStrength:F2}");
        }

        /// <summary>
        /// 迴ｾ蝨ｨ繝励Ξ繧､繝､繝ｼ縺後だ繝ｼ繝ｳ蜀・↓縺・ｋ縺九←縺・°
        /// </summary>
        public bool HasPlayerInside => _playerInZone;

        /// <summary>
        /// 繧ｾ繝ｼ繝ｳ蜀・・繝励Ξ繧､繝､繝ｼ繧ｪ繝悶ず繧ｧ繧ｯ繝・
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

            // 髫阡ｽ蠑ｷ蠎ｦ縺ｮ蜿ｯ隕門喧
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


