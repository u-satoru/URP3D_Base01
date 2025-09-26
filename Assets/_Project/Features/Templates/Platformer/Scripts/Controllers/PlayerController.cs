using System;
using System.Collections;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Features.Templates.Platformer.Services;

namespace asterivo.Unity60.Features.Templates.Platformer.Controllers
{
    /// <summary>
    /// Player Controller・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝槭・繝励Ξ繧､繝､繝ｼ邨ｱ蜷亥宛蠕｡繧ｷ繧ｹ繝・Β
    /// ServiceLocator + Event鬧・虚繧｢繝ｼ繧ｭ繝・け繝√Ε縺ｫ繧医ｋ蛹・峡逧・・繝ｬ繧､繝､繝ｼ蛻ｶ蠕｡
    /// Learn & Grow萓｡蛟､螳溽樟・夂峩諢溽噪謫堺ｽ懊・豬∽ｽ鍋噪蜍穂ｽ懊・鬮伜ｺｦ繝励Λ繝・ヨ繝輔か繝ｼ繝槭・繝｡繧ｫ繝九け繧ｹ
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

        // ServiceLocator邨ｱ蜷・
        private IPlatformerPhysicsService _physicsService;
        private IPlatformerInputService _inputService;
        private IPlatformerAudioService _audioService;
        private IPlatformerUIService _uiService;
        private ICollectionService _collectionService;
        private ICheckpointService _checkpointService;

        // 繧ｳ繝ｳ繝昴・繝阪Φ繝亥盾辣ｧ
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private JumpController _jumpController;

        // 繝励Ξ繧､繝､繝ｼ迥ｶ諷・
        private bool _isInvincible = false;
        private bool _isDead = false;
        private bool _isCrouching = false;
        private bool _isRunning = false;
        private bool _facingRight = true;

        // 遘ｻ蜍募宛蠕｡
        private Vector2 _velocity;
        private float _horizontalInput;
        private float _targetVelocityX;
        private float _velocityXSmoothing;

        // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ 繝代Λ繝｡繝ｼ繧ｿ 繝上ャ繧ｷ繝･
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
            // 繧ｳ繝ｳ繝昴・繝阪Φ繝亥叙蠕・
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _jumpController = GetComponent<JumpController>();

            // 繧｢繝九Γ繝ｼ繧ｿ繝ｼ閾ｪ蜍募叙蠕・
            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ 繝代Λ繝｡繝ｼ繧ｿ 繝上ャ繧ｷ繝･險育ｮ・
            if (_animator != null)
            {
                _animSpeedHash = Animator.StringToHash("Speed");
                _animGroundedHash = Animator.StringToHash("Grounded");
                _animJumpingHash = Animator.StringToHash("Jumping");
                _animCrouchingHash = Animator.StringToHash("Crouching");
                _animRunningHash = Animator.StringToHash("Running");
            }

            // 蛻晄悄蛟､險ｭ螳・
            _currentHealth = _maxHealth;
            _velocity = Vector2.zero;
        }

        private void Start()
        {
            // ServiceLocator邨ｱ蜷茨ｼ壹し繝ｼ繝薙せ蜿門ｾ・
            InitializeServices();

            // UI蛻晄悄蛹・
            UpdateUI();
        }

        private void InitializeServices()
        {
            try
            {
                // Physics Service・夂ｧｻ蜍戊ｨ育ｮ励→迚ｩ逅・ｼ皮ｮ・
                _physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                if (_physicsService == null)
                {
                    Debug.LogError("[PlayerController] IPlatformerPhysicsService not found in ServiceLocator!");
                }

                // Input Service・壹・繝ｬ繧､繝､繝ｼ蜈･蜉帛・逅・
                _inputService = ServiceLocator.GetService<IPlatformerInputService>();
                if (_inputService != null)
                {
                    // Event鬧・虚・壼・蜉帙う繝吶Φ繝郁ｳｼ隱ｭ
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

                // Audio Service・夂ｧｻ蜍輔・繧｢繧ｯ繧ｷ繝ｧ繝ｳ繧ｵ繧ｦ繝ｳ繝・
                if (_enableMovementSounds)
                {
                    _audioService = ServiceLocator.GetService<IPlatformerAudioService>();
                    if (_audioService == null)
                    {
                        Debug.LogWarning("[PlayerController] IPlatformerAudioService not found. Movement sounds will be disabled.");
                        _enableMovementSounds = false;
                    }
                }

                // UI Service・壹・繝ｫ繧ｹ繝ｻ繧ｹ繧ｳ繧｢繝ｻ繝ｩ繧､繝戊｡ｨ遉ｺ
                _uiService = ServiceLocator.GetService<IPlatformerUIService>();
                if (_uiService == null)
                {
                    Debug.LogWarning("[PlayerController] IPlatformerUIService not found. UI updates will be disabled.");
                }

                // Collection Service・壹い繧､繝・Β蜿朱寔
                _collectionService = ServiceLocator.GetService<ICollectionService>();
                if (_collectionService != null)
                {
                    _collectionService.OnItemCollected.AddListener(OnItemCollected);
                    _collectionService.OnScoreChanged.AddListener(OnScoreUpdated);
                }

                // Checkpoint Service・壹そ繝ｼ繝悶・繝ｪ繧ｹ繝昴・繝ｳ
                _checkpointService = ServiceLocator.GetService<ICheckpointService>();

                // JumpController騾｣謳ｺ
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

            // 蜈･蜉帛・逅・
            ProcessInput();

            // 遘ｻ蜍戊ｨ育ｮ・
            CalculateMovement();

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ譖ｴ譁ｰ
            UpdateAnimation();

            // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ讀懷・
            CheckInteractions();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            // 迚ｩ逅・ｧｻ蜍暮←逕ｨ
            ApplyMovement();
        }

        private void ProcessInput()
        {
            if (_inputService == null) return;

            // 豌ｴ蟷ｳ遘ｻ蜍募・蜉・
            _horizontalInput = _inputService.MovementInput.x;
        }

        private void CalculateMovement()
        {
            if (_physicsService == null) return;

            // 遘ｻ蜍暮溷ｺｦ險育ｮ・
            float targetSpeed = _horizontalInput * _baseMovementSpeed;

            // 襍ｰ陦後・縺励ｃ縺後∩蛟咲紫驕ｩ逕ｨ
            if (_isRunning && !_isCrouching)
            {
                targetSpeed *= _runSpeedMultiplier;
            }
            else if (_isCrouching)
            {
                targetSpeed *= _crouchSpeedMultiplier;
            }

            // 繧ｹ繝繝ｼ繧ｺ縺ｪ蜉貂幃・
            _targetVelocityX = targetSpeed;

            // Physics Service邨ｱ蜷茨ｼ夂ｧｻ蜍戊ｨ育ｮ・
            _velocity = _physicsService.CalculateMovement(_velocity, _horizontalInput, _jumpController.IsGrounded);

            // 繧ｹ繝励Λ繧､繝亥渚霆｢
            UpdateSpriteDirection();
        }

        private void ApplyMovement()
        {
            // X霆ｸ遘ｻ蜍輔・繧ｹ繝繝ｼ繧ｺ蛹・
            _velocity.x = Mathf.SmoothDamp(_velocity.x, _targetVelocityX, ref _velocityXSmoothing,
                (_horizontalInput != 0) ? _accelerationTime : _decelerationTime);

            // Physics Service邨ｱ蜷茨ｼ夐㍾蜉幃←逕ｨ
            if (_physicsService != null)
            {
                bool isJumpPressed = _inputService?.JumpHeld ?? false;
                _velocity = _physicsService.ApplyGravity(_velocity, _jumpController.IsGrounded, isJumpPressed);
            }

            // Rigidbody2D縺ｫ騾溷ｺｦ驕ｩ逕ｨ
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

            // 遘ｻ蜍暮溷ｺｦ
            float speed = Mathf.Abs(_velocity.x);
            _animator.SetFloat(_animSpeedHash, speed);

            // 蝨ｰ髱｢迥ｶ諷・
            _animator.SetBool(_animGroundedHash, _jumpController.IsGrounded);

            // 繧ｸ繝｣繝ｳ繝礼憾諷・
            _animator.SetBool(_animJumpingHash, _jumpController.IsJumping);

            // 縺励ｃ縺後∩迥ｶ諷・
            _animator.SetBool(_animCrouchingHash, _isCrouching);

            // 襍ｰ陦檎憾諷・
            _animator.SetBool(_animRunningHash, _isRunning && speed > 0.1f);
        }

        private void CheckInteractions()
        {
            // 蜿朱寔蜿ｯ閭ｽ繧｢繧､繝・Β縺ｮ讀懷・
            Collider2D[] collectibles = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _collectibleLayerMask);
            foreach (var collectible in collectibles)
            {
                if (_collectionService != null)
                {
                    // Collection Service邨ｱ蜷茨ｼ壹い繧､繝・Β蜿朱寔蜃ｦ逅・
                    // _collectionService.TryCollectItem(collectible.gameObject);
                }
            }
        }

        // 蜈･蜉帙う繝吶Φ繝医ワ繝ｳ繝峨Λ繝ｼ
        private void OnMovementInput(Vector2 input)
        {
            // MovementInput 繝励Ο繝代ユ繧｣繧剃ｽｿ逕ｨ縺吶ｋ縺溘ａ縲√％縺ｮ繧､繝吶Φ繝医・荳ｻ縺ｫ繝・ヰ繝・げ逕ｨ
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
            // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蜃ｦ逅・
            Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayerMask);
            foreach (var interactable in interactables)
            {
                // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ螳溯｡・
                var interaction = interactable.GetComponent<IInteractable>();
                if (interaction != null)
                {
                    interaction.Interact(this);
                    LogDebug($"Interacted with: {interactable.name}");
                }
            }
        }

        // 繧ｸ繝｣繝ｳ繝励う繝吶Φ繝医ワ繝ｳ繝峨Λ繝ｼ
        private void OnJumpStarted()
        {
            LogDebug("Jump started.");
        }

        private void OnLanded()
        {
            LogDebug("Player landed.");

            // 逹蝨ｰ髻ｳ蜉ｹ譫・
            PlayMovementSound(_walkSoundEvent);
        }

        // 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ繧､繝吶Φ繝医ワ繝ｳ繝峨Λ繝ｼ
        private void OnItemCollected(string itemType, int value)
        {
            LogDebug($"Item collected: {itemType}, Value: {value}");

            // 繧ｳ繝ｬ繧ｯ繧ｷ繝ｧ繝ｳ蜉ｹ譫憺浹
            if (_audioService != null)
            {
                // _audioService.PlaySound("ItemCollected", transform.position);
            }

            // 繧｢繧､繝・Β繧ｿ繧､繝励↓蠢懊§縺溷・逅・
            switch (itemType.ToLower())
            {
                case "health":
                case "heal":
                    Heal(value);
                    break;
                case "coin":
                case "gem":
                case "score":
                    // 繧ｹ繧ｳ繧｢蜃ｦ逅・・ Collection Service 縺梧球蠖・
                    break;
            }
        }

        private void OnScoreUpdated(int newScore)
        {
            LogDebug($"Score updated: {newScore}");
            OnScoreChanged?.Invoke(newScore);
        }

        // 繝倥Ν繧ｹ繝ｻ繝繝｡繝ｼ繧ｸ繧ｷ繧ｹ繝・Β
        public void TakeDamage(int damage)
        {
            if (_isDead || _isInvincible) return;

            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            // 繧､繝吶Φ繝育匱陦・
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // 繝繝｡繝ｼ繧ｸ繧ｨ繝輔ぉ繧ｯ繝・
            if (_damageEffect != null)
            {
                Instantiate(_damageEffect, transform.position, Quaternion.identity);
            }

            // 繝繝｡繝ｼ繧ｸ繧ｵ繧ｦ繝ｳ繝・
            PlaySound(_damageSoundEvent);

            // 辟｡謨ｵ譎る俣髢句ｧ・
            StartCoroutine(InvincibilityCoroutine());

            // 豁ｻ莠｡繝√ぉ繝・け
            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // UI譖ｴ譁ｰ
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
                // 繧､繝吶Φ繝育匱陦・
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

                // 蝗槫ｾｩ繧ｨ繝輔ぉ繧ｯ繝・
                if (_healEffect != null)
                {
                    Instantiate(_healEffect, transform.position, Quaternion.identity);
                }

                // 蝗槫ｾｩ繧ｵ繧ｦ繝ｳ繝・
                PlaySound(_healSoundEvent);

                // UI譖ｴ譁ｰ
                UpdateUI();

                LogDebug($"Player healed {amount}. Health: {_currentHealth}/{_maxHealth}");
            }
        }

        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _lives--;

            // 繧､繝吶Φ繝育匱陦・
            OnPlayerDied?.Invoke();
            OnLivesChanged?.Invoke(_lives);

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ蛛懈ｭ｢
            if (_animator != null)
            {
                _animator.SetBool("Dead", true);
            }

            // 繝ｪ繧ｹ繝昴・繝ｳ蜃ｦ逅・
            if (_lives > 0)
            {
                StartCoroutine(RespawnCoroutine());
            }
            else
            {
                // 繧ｲ繝ｼ繝繧ｪ繝ｼ繝舌・
                GameOver();
            }

            LogDebug($"Player died. Lives remaining: {_lives}");
        }

        private void GameOver()
        {
            LogDebug("Game Over!");

            // UI Service邨ｱ蜷茨ｼ壹ご繝ｼ繝繧ｪ繝ｼ繝舌・逕ｻ髱｢陦ｨ遉ｺ
            if (_uiService != null)
            {
                _uiService.ShowGameOverScreen();
            }
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(2f);

            // Checkpoint Service邨ｱ蜷茨ｼ壽怙蠕後・繝√ぉ繝・け繝昴う繝ｳ繝医°繧峨Μ繧ｹ繝昴・繝ｳ
            if (_checkpointService != null)
            {
                var spawnPoint = _checkpointService.GetLastCheckpointPosition();
                transform.position = spawnPoint;
            }

            // 繧ｹ繝・・繧ｿ繧ｹ繝ｪ繧ｻ繝・ヨ
            _currentHealth = _maxHealth;
            _isDead = false;
            _isInvincible = false;

            // 繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繝ｪ繧ｻ繝・ヨ
            if (_animator != null)
            {
                _animator.SetBool("Dead", false);
            }

            // 繧､繝吶Φ繝育匱陦・
            OnPlayerRespawned?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // UI譖ｴ譁ｰ
            UpdateUI();

            LogDebug("Player respawned.");
        }

        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;

            // 辟｡謨ｵ繧ｨ繝輔ぉ繧ｯ繝・
            if (_invincibilityEffect != null)
            {
                _invincibilityEffect.SetActive(true);
            }

            // 繧ｹ繝励Λ繧､繝育せ貊・
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

            // 辟｡謨ｵ隗｣髯､
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
            // Event鬧・虚・壹う繝吶Φ繝郁ｳｼ隱ｭ隗｣髯､
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

            // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ遽・峇縺ｮ蜿ｯ隕門喧
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);

            // 繝・ヰ繝・げ諠・ｱ陦ｨ遉ｺ
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

        // 螟夜Κ蛻ｶ蠕｡API
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

        // 繝励Ο繝代ユ繧｣
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
    /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蜿ｯ閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerController player);
    }
}


