using System;
using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Platformer.Services;

namespace asterivo.Unity60.Features.Templates.Platformer.Controllers
{
    /// <summary>
    /// Player Controller：プラットフォーマープレイヤー統合制御システム
    /// ServiceLocator + Event駆動アーキテクチャによる包括的プレイヤー制御
    /// Learn & Grow価値実現：直感的操作・流体的動作・高度プラットフォーマーメカニクス
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(JumpController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;
        [SerializeField] private float _invincibilityTime = 1.5f;
        [SerializeField] private int _lives = 3;

        [Header("Movement Settings")]
        [SerializeField] private float _baseMovementSpeed = 5f;
        [SerializeField] private float _runSpeedMultiplier = 1.5f;
        [SerializeField] private float _crouchSpeedMultiplier = 0.5f;
        [SerializeField] private float _accelerationTime = 0.1f;
        [SerializeField] private float _decelerationTime = 0.1f;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _flipSpriteWithMovement = true;

        [Header("Interaction")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayerMask = 1;
        [SerializeField] private LayerMask _collectibleLayerMask = 1;

        [Header("Effects")]
        [SerializeField] private GameObject _damageEffect;
        [SerializeField] private GameObject _healEffect;
        [SerializeField] private GameObject _invincibilityEffect;

        [Header("Audio")]
        [SerializeField] private bool _enableMovementSounds = true;
        [SerializeField] private string _walkSoundEvent = "Footstep";
        [SerializeField] private string _runSoundEvent = "RunFootstep";
        [SerializeField] private string _damageSoundEvent = "PlayerDamage";
        [SerializeField] private string _healSoundEvent = "Heal";

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        [SerializeField] private bool _drawInteractionRadius = true;

        // ServiceLocator統合
        private IPlatformerPhysicsService _physicsService;
        private IPlatformerInputService _inputService;
        private IPlatformerAudioService _audioService;
        private IPlatformerUIService _uiService;
        private ICollectionService _collectionService;
        private ICheckpointService _checkpointService;

        // コンポーネント参照
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private JumpController _jumpController;

        // プレイヤー状態
        private bool _isInvincible = false;
        private bool _isDead = false;
        private bool _isCrouching = false;
        private bool _isRunning = false;
        private bool _facingRight = true;

        // 移動制御
        private Vector2 _velocity;
        private float _horizontalInput;
        private float _targetVelocityX;
        private float _velocityXSmoothing;

        // アニメーション パラメータ ハッシュ
        private int _animSpeedHash;
        private int _animGroundedHash;
        private int _animJumpingHash;
        private int _animCrouchingHash;
        private int _animRunningHash;

        // Events
        public event Action<int, int> OnHealthChanged; // current, max
        public event Action<int> OnLivesChanged;
        public event Action OnPlayerDied;
        public event Action OnPlayerRespawned;
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            // コンポーネント取得
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _jumpController = GetComponent<JumpController>();

            // アニメーター自動取得
            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            // アニメーション パラメータ ハッシュ計算
            if (_animator != null)
            {
                _animSpeedHash = Animator.StringToHash("Speed");
                _animGroundedHash = Animator.StringToHash("Grounded");
                _animJumpingHash = Animator.StringToHash("Jumping");
                _animCrouchingHash = Animator.StringToHash("Crouching");
                _animRunningHash = Animator.StringToHash("Running");
            }

            // 初期値設定
            _currentHealth = _maxHealth;
            _velocity = Vector2.zero;
        }

        private void Start()
        {
            // ServiceLocator統合：サービス取得
            InitializeServices();

            // UI初期化
            UpdateUI();
        }

        private void InitializeServices()
        {
            try
            {
                // Physics Service：移動計算と物理演算
                _physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                if (_physicsService == null)
                {
                    Debug.LogError("[PlayerController] IPlatformerPhysicsService not found in ServiceLocator!");
                }

                // Input Service：プレイヤー入力処理
                _inputService = ServiceLocator.GetService<IPlatformerInputService>();
                if (_inputService != null)
                {
                    // Event駆動：入力イベント購読
                    _inputService.OnMovementChanged += OnMovementInput;
                    _inputService.OnCrouchPressed += OnCrouchPressed;
                    _inputService.OnCrouchReleased += OnCrouchReleased;
                    _inputService.OnRunPressed += OnRunPressed;
                    _inputService.OnRunReleased += OnRunReleased;
                    _inputService.OnInteractPressed += OnInteractPressed;
                }
                else
                {
                    Debug.LogError("[PlayerController] IPlatformerInputService not found in ServiceLocator!");
                }

                // Audio Service：移動・アクションサウンド
                if (_enableMovementSounds)
                {
                    _audioService = ServiceLocator.GetService<IPlatformerAudioService>();
                    if (_audioService == null)
                    {
                        Debug.LogWarning("[PlayerController] IPlatformerAudioService not found. Movement sounds will be disabled.");
                        _enableMovementSounds = false;
                    }
                }

                // UI Service：ヘルス・スコア・ライフ表示
                _uiService = ServiceLocator.GetService<IPlatformerUIService>();
                if (_uiService == null)
                {
                    Debug.LogWarning("[PlayerController] IPlatformerUIService not found. UI updates will be disabled.");
                }

                // Collection Service：アイテム収集
                _collectionService = ServiceLocator.GetService<ICollectionService>();
                if (_collectionService != null)
                {
                    _collectionService.OnItemCollected.AddListener(OnItemCollected);
                    _collectionService.OnScoreChanged.AddListener(OnScoreUpdated);
                }

                // Checkpoint Service：セーブ・リスポーン
                _checkpointService = ServiceLocator.GetService<ICheckpointService>();

                // JumpController連携
                if (_jumpController != null)
                {
                    _jumpController.OnJumpStarted += OnJumpStarted;
                    _jumpController.OnLanded += OnLanded;
                }

                LogDebug("PlayerController services initialized successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerController] Failed to initialize services: {ex.Message}");
            }
        }

        private void Update()
        {
            if (_isDead) return;

            // 入力処理
            ProcessInput();

            // 移動計算
            CalculateMovement();

            // アニメーション更新
            UpdateAnimation();

            // インタラクション検出
            CheckInteractions();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            // 物理移動適用
            ApplyMovement();
        }

        private void ProcessInput()
        {
            if (_inputService == null) return;

            // 水平移動入力
            _horizontalInput = _inputService.MovementInput.x;
        }

        private void CalculateMovement()
        {
            if (_physicsService == null) return;

            // 移動速度計算
            float targetSpeed = _horizontalInput * _baseMovementSpeed;

            // 走行・しゃがみ倍率適用
            if (_isRunning && !_isCrouching)
            {
                targetSpeed *= _runSpeedMultiplier;
            }
            else if (_isCrouching)
            {
                targetSpeed *= _crouchSpeedMultiplier;
            }

            // スムーズな加減速
            _targetVelocityX = targetSpeed;

            // Physics Service統合：移動計算
            _velocity = _physicsService.CalculateMovement(_velocity, _horizontalInput, _jumpController.IsGrounded);

            // スプライト反転
            UpdateSpriteDirection();
        }

        private void ApplyMovement()
        {
            // X軸移動のスムーズ化
            _velocity.x = Mathf.SmoothDamp(_velocity.x, _targetVelocityX, ref _velocityXSmoothing,
                (_horizontalInput != 0) ? _accelerationTime : _decelerationTime);

            // Physics Service統合：重力適用
            if (_physicsService != null)
            {
                bool isJumpPressed = _inputService?.JumpHeld ?? false;
                _velocity = _physicsService.ApplyGravity(_velocity, _jumpController.IsGrounded, isJumpPressed);
            }

            // Rigidbody2Dに速度適用
            _rigidbody.linearVelocity = _velocity;
        }

        private void UpdateSpriteDirection()
        {
            if (!_flipSpriteWithMovement || _spriteRenderer == null) return;

            if (_horizontalInput > 0 && !_facingRight)
            {
                Flip();
            }
            else if (_horizontalInput < 0 && _facingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            _spriteRenderer.flipX = !_facingRight;
        }

        private void UpdateAnimation()
        {
            if (_animator == null) return;

            // 移動速度
            float speed = Mathf.Abs(_velocity.x);
            _animator.SetFloat(_animSpeedHash, speed);

            // 地面状態
            _animator.SetBool(_animGroundedHash, _jumpController.IsGrounded);

            // ジャンプ状態
            _animator.SetBool(_animJumpingHash, _jumpController.IsJumping);

            // しゃがみ状態
            _animator.SetBool(_animCrouchingHash, _isCrouching);

            // 走行状態
            _animator.SetBool(_animRunningHash, _isRunning && speed > 0.1f);
        }

        private void CheckInteractions()
        {
            // 収集可能アイテムの検出
            Collider2D[] collectibles = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _collectibleLayerMask);
            foreach (var collectible in collectibles)
            {
                if (_collectionService != null)
                {
                    // Collection Service統合：アイテム収集処理
                    // _collectionService.TryCollectItem(collectible.gameObject);
                }
            }
        }

        // 入力イベントハンドラー
        private void OnMovementInput(Vector2 input)
        {
            // MovementInput プロパティを使用するため、このイベントは主にデバッグ用
            LogDebug($"Movement input: {input}");
        }

        private void OnCrouchPressed()
        {
            _isCrouching = true;
            LogDebug("Crouch started.");
        }

        private void OnCrouchReleased()
        {
            _isCrouching = false;
            LogDebug("Crouch ended.");
        }

        private void OnRunPressed()
        {
            _isRunning = true;
            LogDebug("Run started.");
        }

        private void OnRunReleased()
        {
            _isRunning = false;
            LogDebug("Run ended.");
        }

        private void OnInteractPressed()
        {
            // インタラクション処理
            Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayerMask);
            foreach (var interactable in interactables)
            {
                // インタラクション実行
                var interaction = interactable.GetComponent<IInteractable>();
                if (interaction != null)
                {
                    interaction.Interact(this);
                    LogDebug($"Interacted with: {interactable.name}");
                }
            }
        }

        // ジャンプイベントハンドラー
        private void OnJumpStarted()
        {
            LogDebug("Jump started.");
        }

        private void OnLanded()
        {
            LogDebug("Player landed.");

            // 着地音効果
            PlayMovementSound(_walkSoundEvent);
        }

        // コレクションイベントハンドラー
        private void OnItemCollected(string itemType, int value)
        {
            LogDebug($"Item collected: {itemType}, Value: {value}");

            // コレクション効果音
            if (_audioService != null)
            {
                // _audioService.PlaySound("ItemCollected", transform.position);
            }

            // アイテムタイプに応じた処理
            switch (itemType.ToLower())
            {
                case "health":
                case "heal":
                    Heal(value);
                    break;
                case "coin":
                case "gem":
                case "score":
                    // スコア処理は Collection Service が担当
                    break;
            }
        }

        private void OnScoreUpdated(int newScore)
        {
            LogDebug($"Score updated: {newScore}");
            OnScoreChanged?.Invoke(newScore);
        }

        // ヘルス・ダメージシステム
        public void TakeDamage(int damage)
        {
            if (_isDead || _isInvincible) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            // イベント発行
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // ダメージエフェクト
            if (_damageEffect != null)
            {
                Instantiate(_damageEffect, transform.position, Quaternion.identity);
            }

            // ダメージサウンド
            PlaySound(_damageSoundEvent);

            // 無敵時間開始
            StartCoroutine(InvincibilityCoroutine());

            // 死亡チェック
            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // UI更新
                UpdateUI();
            }

            LogDebug($"Player took {damage} damage. Health: {_currentHealth}/{_maxHealth}");
        }

        public void Heal(int amount)
        {
            if (_isDead) return;

            int oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            if (_currentHealth != oldHealth)
            {
                // イベント発行
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

                // 回復エフェクト
                if (_healEffect != null)
                {
                    Instantiate(_healEffect, transform.position, Quaternion.identity);
                }

                // 回復サウンド
                PlaySound(_healSoundEvent);

                // UI更新
                UpdateUI();

                LogDebug($"Player healed {amount}. Health: {_currentHealth}/{_maxHealth}");
            }
        }

        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _lives--;

            // イベント発行
            OnPlayerDied?.Invoke();
            OnLivesChanged?.Invoke(_lives);

            // アニメーション停止
            if (_animator != null)
            {
                _animator.SetBool("Dead", true);
            }

            // リスポーン処理
            if (_lives > 0)
            {
                StartCoroutine(RespawnCoroutine());
            }
            else
            {
                // ゲームオーバー
                GameOver();
            }

            LogDebug($"Player died. Lives remaining: {_lives}");
        }

        private void GameOver()
        {
            LogDebug("Game Over!");

            // UI Service統合：ゲームオーバー画面表示
            if (_uiService != null)
            {
                _uiService.ShowGameOverScreen();
            }
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(2f);

            // Checkpoint Service統合：最後のチェックポイントからリスポーン
            if (_checkpointService != null)
            {
                var spawnPoint = _checkpointService.GetLastCheckpointPosition();
                transform.position = spawnPoint;
            }

            // ステータスリセット
            _currentHealth = _maxHealth;
            _isDead = false;
            _isInvincible = false;

            // アニメーションリセット
            if (_animator != null)
            {
                _animator.SetBool("Dead", false);
            }

            // イベント発行
            OnPlayerRespawned?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // UI更新
            UpdateUI();

            LogDebug("Player respawned.");
        }

        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;

            // 無敵エフェクト
            if (_invincibilityEffect != null)
            {
                _invincibilityEffect.SetActive(true);
            }

            // スプライト点滅
            if (_spriteRenderer != null)
            {
                float flashInterval = 0.1f;
                float elapsed = 0f;

                while (elapsed < _invincibilityTime)
                {
                    _spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                    yield return new WaitForSeconds(flashInterval);
                    _spriteRenderer.color = Color.white;
                    yield return new WaitForSeconds(flashInterval);
                    elapsed += flashInterval * 2;
                }

                _spriteRenderer.color = Color.white;
            }
            else
            {
                yield return new WaitForSeconds(_invincibilityTime);
            }

            // 無敵解除
            _isInvincible = false;

            if (_invincibilityEffect != null)
            {
                _invincibilityEffect.SetActive(false);
            }
        }

        private void UpdateUI()
        {
            if (_uiService != null)
            {
                _uiService.UpdateHealthBar(_currentHealth, _maxHealth);
                _uiService.UpdateLives(_lives);
            }
        }

        private void PlayMovementSound(string soundEvent)
        {
            if (_enableMovementSounds && _audioService != null && !string.IsNullOrEmpty(soundEvent))
            {
                // _audioService.PlaySound(soundEvent, transform.position);
                LogDebug($"Movement sound triggered: {soundEvent}");
            }
        }

        private void PlaySound(string soundEvent)
        {
            if (_audioService != null && !string.IsNullOrEmpty(soundEvent))
            {
                // _audioService.PlaySound(soundEvent, transform.position);
                LogDebug($"Sound triggered: {soundEvent}");
            }
        }

        private void OnDestroy()
        {
            // Event駆動：イベント購読解除
            if (_inputService != null)
            {
                _inputService.OnMovementChanged -= OnMovementInput;
                _inputService.OnCrouchPressed -= OnCrouchPressed;
                _inputService.OnCrouchReleased -= OnCrouchReleased;
                _inputService.OnRunPressed -= OnRunPressed;
                _inputService.OnRunReleased -= OnRunReleased;
                _inputService.OnInteractPressed -= OnInteractPressed;
            }

            if (_collectionService != null)
            {
                _collectionService.OnItemCollected.RemoveListener(OnItemCollected);
                _collectionService.OnScoreChanged.RemoveListener(OnScoreUpdated);
            }

            if (_jumpController != null)
            {
                _jumpController.OnJumpStarted -= OnJumpStarted;
                _jumpController.OnLanded -= OnLanded;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawInteractionRadius) return;

            // インタラクション範囲の可視化
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);

            // デバッグ情報表示
            if (_showDebugInfo)
            {
                var labelPos = transform.position + Vector3.up * 3f;
                UnityEditor.Handles.Label(labelPos,
                    $"Health: {_currentHealth}/{_maxHealth}\n" +
                    $"Lives: {_lives}\n" +
                    $"Speed: {_velocity.magnitude:F2}\n" +
                    $"Grounded: {(_jumpController ? _jumpController.IsGrounded : false)}\n" +
                    $"Invincible: {_isInvincible}");
            }
        }

        // 外部制御API
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            _velocity = Vector2.zero;
            _rigidbody.linearVelocity = Vector2.zero;
        }

        public void AddLife(int amount = 1)
        {
            _lives += amount;
            OnLivesChanged?.Invoke(_lives);
            UpdateUI();
            LogDebug($"Life added. Lives: {_lives}");
        }

        public void SetMaxHealth(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            UpdateUI();
        }

        // プロパティ
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public int Lives => _lives;
        public bool IsAlive => !_isDead;
        public bool IsInvincible => _isInvincible;
        public bool IsGrounded => _jumpController ? _jumpController.IsGrounded : false;
        public bool IsMoving => Mathf.Abs(_velocity.x) > 0.1f;
        public bool IsCrouching => _isCrouching;
        public bool IsRunning => _isRunning;
        public Vector2 Velocity => _velocity;

        private void LogDebug(string message)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[PlayerController] {message}");
            }
        }
    }

    /// <summary>
    /// インタラクション可能オブジェクトのインターフェース
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerController player);
    }
}
