using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;
using asterivo.Unity60.Core.Combat;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorテンプレート用プレイヤーコントローラー
    /// 恐怖感、リソース管理、脆弱性を重視した操作体験を提供
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SanityComponent))]
    [RequireComponent(typeof(LimitedInventoryComponent))]
    [RequireComponent(typeof(HealthComponent))]
    public class SH_PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float runSpeed = 6.0f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private bool canRun = true;
        [SerializeField] private bool canCrouch = true;

        [Header("Survival Horror Mechanics")]
        [SerializeField] private float fearSlowdownMultiplier = 0.7f;
        [SerializeField] private float lowSanityShakeIntensity = 0.1f;
        [SerializeField] private float breathingHeavyThreshold = 0.3f;
        [SerializeField] private float staminaDrainRate = 20f;
        [SerializeField] private float staminaRecoveryRate = 15f;
        [SerializeField] private float maxStamina = 100f;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 2.0f;
        [SerializeField] private LayerMask interactionLayerMask = -1;
        [SerializeField] private Transform interactionPoint;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioSource breathingAudioSource;
        [SerializeField] private AudioSource heartbeatAudioSource;
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private AudioClip[] breathingSounds;
        [SerializeField] private AudioClip heartbeatSound;

        [Header("Visual Effects")]
        [SerializeField] private UnityEngine.Camera playerCamera;
        [SerializeField] private Transform cameraContainer;
        [SerializeField] private float headBobIntensity = 0.02f;
        [SerializeField] private float headBobFrequency = 10f;
        [SerializeField] private GameObject flashlight;

        [Header("Events")]
        [SerializeField] private GameEvent<Vector3> onPlayerMoved;
        [SerializeField] private GameEvent<float> onStaminaChanged;
        [SerializeField] private GameEvent<bool> onPlayerRunning;
        [SerializeField] private GameEvent<bool> onPlayerCrouching;

        // Core Components
        private CharacterController characterController;
        private SanityComponent sanityComponent;
        private LimitedInventoryComponent inventoryComponent;
        private HealthComponent healthComponent;

        // Runtime State
        private Vector3 moveDirection;
        private float verticalLookRotation = 0f;
        private bool isRunning = false;
        private bool isCrouching = false;
        private float currentStamina;
        private float originalHeight;
        private Vector3 originalCameraPosition;
        private bool isMoving = false;
        private float footstepTimer = 0f;

        // Survival Horror State
        private float fearLevel = 0f;
        private bool isInDanger = false;
        private Vector3 cameraShakeOffset;
        private float lastHeartbeatTime;

        // Input State
        private bool flashlightOn = false;
        private GameObject currentInteractable;

        public float CurrentStamina => currentStamina;
        public float StaminaNormalized => currentStamina / maxStamina;
        public bool IsRunning => isRunning;
        public bool IsCrouching => isCrouching;
        public bool IsMoving => isMoving;
        public float FearLevel => fearLevel;

        private void Awake()
        {
            InitializeComponents();
            CacheOriginalValues();
        }

        private void Start()
        {
            InitializePlayerState();
            SetupAudioSources();
            LockCursor();
        }

        private void Update()
        {
            HandleInput();
            HandleMovement();
            HandleLook();
            HandleInteraction();
            UpdateSurvivalHorrorEffects();
            UpdateAudio();
            UpdateVisualEffects();
        }

        /// <summary>
        /// コンポーネントを初期化
        /// </summary>
        private void InitializeComponents()
        {
            characterController = GetComponent<CharacterController>();
            sanityComponent = GetComponent<SanityComponent>();
            inventoryComponent = GetComponent<LimitedInventoryComponent>();
            healthComponent = GetComponent<HealthComponent>();

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (cameraContainer == null)
                cameraContainer = playerCamera?.transform.parent ?? playerCamera?.transform;

            if (interactionPoint == null)
                interactionPoint = playerCamera?.transform ?? transform;
        }

        /// <summary>
        /// 初期値をキャッシュ
        /// </summary>
        private void CacheOriginalValues()
        {
            originalHeight = characterController.height;
            originalCameraPosition = cameraContainer?.localPosition ?? Vector3.zero;
            currentStamina = maxStamina;
        }

        /// <summary>
        /// プレイヤー状態を初期化
        /// </summary>
        private void InitializePlayerState()
        {
            // コンポーネントの初期設定
            if (sanityComponent != null)
            {
                // maxSanity is set via Inspector - no SetMaxSanity method available
                // SanityComponent events are GameEvent ScriptableObjects, configured in Inspector
            }

            if (healthComponent != null)
            {
                // Health event handlers can be implemented as needed
                // healthComponent.OnHealthChanged += (current, max) => { /* Handle health change */ };
                // healthComponent.OnDied += () => { /* Handle death */ };
            }

            Debug.Log("[SH_PlayerController] Player initialized successfully");
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            // 移動入力
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

            // 走行入力
            bool runInput = Input.GetKey(KeyCode.LeftShift) && canRun;
            bool hasStamina = currentStamina > 10f;
            isRunning = runInput && hasStamina && moveDirection.magnitude > 0.1f && !isCrouching;

            // しゃがみ入力
            if (Input.GetKeyDown(KeyCode.LeftControl) && canCrouch)
            {
                ToggleCrouch();
            }

            // フラッシュライト切り替え
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();
            }

            // インタラクション入力
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryInteract();
            }

            // インベントリ切り替え（デバッグ用）
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventoryDebug();
            }
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        private void HandleMovement()
        {
            if (moveDirection.magnitude > 0.1f)
            {
                // 速度計算
                float targetSpeed = GetCurrentMovementSpeed();

                // 恐怖状態による速度補正
                targetSpeed *= GetFearSpeedMultiplier();

                // 正気度による速度補正
                targetSpeed *= GetSanitySpeedMultiplier();

                // 移動ベクトル計算
                Vector3 move = transform.TransformDirection(moveDirection) * targetSpeed;

                // 重力適用
                if (!characterController.isGrounded)
                {
                    move.y -= 9.81f * Time.deltaTime;
                }

                // 移動実行
                characterController.Move(move * Time.deltaTime);

                // スタミナ消費
                if (isRunning)
                {
                    ConsumeStamina(staminaDrainRate * Time.deltaTime);
                }

                isMoving = true;
                onPlayerMoved?.Raise(transform.position);
            }
            else
            {
                isMoving = false;

                // スタミナ回復
                RecoverStamina(staminaRecoveryRate * Time.deltaTime);
            }

            // 移動状態イベント
            if (isRunning != (Input.GetKey(KeyCode.LeftShift) && moveDirection.magnitude > 0.1f))
            {
                onPlayerRunning?.Raise(isRunning);
            }
        }

        /// <summary>
        /// 視点操作処理
        /// </summary>
        private void HandleLook()
        {
            if (playerCamera == null) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 正気度による視点揺れ
            if (sanityComponent != null && sanityComponent.SanityNormalized < 0.5f)
            {
                float shakeAmount = (1f - sanityComponent.SanityNormalized) * lowSanityShakeIntensity;
                mouseX += Random.Range(-shakeAmount, shakeAmount);
                mouseY += Random.Range(-shakeAmount, shakeAmount);
            }

            // 水平回転（Y軸）
            transform.Rotate(Vector3.up * mouseX);

            // 垂直回転（X軸）
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
        }

        /// <summary>
        /// インタラクション処理
        /// </summary>
        private void HandleInteraction()
        {
            // インタラクション対象の検出
            var previousInteractable = currentInteractable;
            currentInteractable = GetInteractableInRange();

            // インタラクション対象が変更された場合のハイライト制御
            if (previousInteractable != currentInteractable)
            {
                if (previousInteractable != null)
                {
                    var pickup = previousInteractable.GetComponent<SH_ItemPickup>();
                    pickup?.SetHighlight(false);
                }

                if (currentInteractable != null)
                {
                    var pickup = currentInteractable.GetComponent<SH_ItemPickup>();
                    pickup?.SetHighlight(true);
                }
            }
        }

        /// <summary>
        /// サバイバルホラー効果を更新
        /// </summary>
        private void UpdateSurvivalHorrorEffects()
        {
            // 恐怖レベルの計算
            fearLevel = CalculateFearLevel();

            // 心拍数の制御
            UpdateHeartbeat();

            // カメラシェイクの更新
            UpdateCameraShake();
        }

        /// <summary>
        /// オーディオを更新
        /// </summary>
        private void UpdateAudio()
        {
            UpdateFootsteps();
            UpdateBreathing();
        }

        /// <summary>
        /// 視覚効果を更新
        /// </summary>
        private void UpdateVisualEffects()
        {
            UpdateHeadBob();
            ApplyCameraShake();
        }

        /// <summary>
        /// 現在の移動速度を取得
        /// </summary>
        private float GetCurrentMovementSpeed()
        {
            if (isCrouching) return crouchSpeed;
            if (isRunning) return runSpeed;
            return walkSpeed;
        }

        /// <summary>
        /// 恐怖による速度補正を取得
        /// </summary>
        private float GetFearSpeedMultiplier()
        {
            return isInDanger ? fearSlowdownMultiplier : 1.0f;
        }

        /// <summary>
        /// 正気度による速度補正を取得
        /// </summary>
        private float GetSanitySpeedMultiplier()
        {
            if (sanityComponent == null) return 1.0f;
            return Mathf.Lerp(0.6f, 1.0f, sanityComponent.SanityNormalized);
        }

        /// <summary>
        /// しゃがみ状態を切り替え
        /// </summary>
        private void ToggleCrouch()
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                characterController.height = originalHeight * 0.6f;
                cameraContainer.localPosition = originalCameraPosition + Vector3.down * 0.5f;
            }
            else
            {
                characterController.height = originalHeight;
                cameraContainer.localPosition = originalCameraPosition;
            }

            onPlayerCrouching?.Raise(isCrouching);
        }

        /// <summary>
        /// フラッシュライトを切り替え
        /// </summary>
        private void ToggleFlashlight()
        {
            if (flashlight != null)
            {
                flashlightOn = !flashlightOn;
                flashlight.SetActive(flashlightOn);

                Debug.Log($"[SH_PlayerController] Flashlight {(flashlightOn ? "ON" : "OFF")}");
            }
        }

        /// <summary>
        /// インタラクションを試行
        /// </summary>
        private void TryInteract()
        {
            if (currentInteractable != null)
            {
                var pickup = currentInteractable.GetComponent<SH_ItemPickup>();
                if (pickup != null)
                {
                    pickup.TryPickup(gameObject);
                }
            }
        }

        /// <summary>
        /// 範囲内のインタラクション対象を取得
        /// </summary>
        private GameObject GetInteractableInRange()
        {
            var colliders = Physics.OverlapSphere(interactionPoint.position, interactionRange, interactionLayerMask);

            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.GetComponent<SH_ItemPickup>() != null)
                {
                    return collider.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// スタミナを消費
        /// </summary>
        private void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            onStaminaChanged?.Raise(StaminaNormalized);

            if (currentStamina <= 0f && isRunning)
            {
                isRunning = false;
                onPlayerRunning?.Raise(false);
            }
        }

        /// <summary>
        /// スタミナを回復
        /// </summary>
        private void RecoverStamina(float amount)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
            onStaminaChanged?.Raise(StaminaNormalized);
        }

        /// <summary>
        /// 恐怖レベルを計算
        /// </summary>
        private float CalculateFearLevel()
        {
            float fear = 0f;

            // 正気度ベースの恐怖
            if (sanityComponent != null)
            {
                fear += (1f - sanityComponent.SanityNormalized) * 0.5f;
            }

            // 体力ベースの恐怖
            if (healthComponent != null)
            {
                fear += (1f - healthComponent.HealthPercentage) * 0.3f;
            }

            // 環境ベースの恐怖（暗闇等）
            fear += flashlightOn ? 0f : 0.2f;

            return Mathf.Clamp01(fear);
        }

        /// <summary>
        /// 心拍音を更新
        /// </summary>
        private void UpdateHeartbeat()
        {
            if (heartbeatAudioSource == null || heartbeatSound == null) return;

            float heartbeatRate = 60f + (fearLevel * 40f); // 60-100 BPM
            float heartbeatInterval = 60f / heartbeatRate;

            if (Time.time - lastHeartbeatTime > heartbeatInterval)
            {
                heartbeatAudioSource.volume = 0.3f + (fearLevel * 0.7f);
                heartbeatAudioSource.PlayOneShot(heartbeatSound);
                lastHeartbeatTime = Time.time;
            }
        }

        /// <summary>
        /// カメラシェイクを更新
        /// </summary>
        private void UpdateCameraShake()
        {
            if (sanityComponent == null) return;

            float shakeIntensity = (1f - sanityComponent.SanityNormalized) * lowSanityShakeIntensity;
            cameraShakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );
        }

        /// <summary>
        /// 足音を更新
        /// </summary>
        private void UpdateFootsteps()
        {
            if (!isMoving || footstepAudioSource == null || footstepSounds.Length == 0) return;

            float stepInterval = GetCurrentMovementSpeed() > walkSpeed ? 0.4f : 0.6f;
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= stepInterval)
            {
                var randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                footstepAudioSource.PlayOneShot(randomFootstep, isCrouching ? 0.3f : 1.0f);
                footstepTimer = 0f;
            }
        }

        /// <summary>
        /// 呼吸音を更新
        /// </summary>
        private void UpdateBreathing()
        {
            if (breathingAudioSource == null) return;

            bool shouldPlayHeavyBreathing = sanityComponent.SanityNormalized < breathingHeavyThreshold ||
                                          currentStamina < 30f ||
                                          fearLevel > 0.6f;

            if (shouldPlayHeavyBreathing && !breathingAudioSource.isPlaying)
            {
                if (breathingSounds.Length > 0)
                {
                    var breathingClip = breathingSounds[Random.Range(0, breathingSounds.Length)];
                    breathingAudioSource.clip = breathingClip;
                    breathingAudioSource.loop = true;
                    breathingAudioSource.volume = 0.5f + (fearLevel * 0.5f);
                    breathingAudioSource.Play();
                }
            }
            else if (!shouldPlayHeavyBreathing && breathingAudioSource.isPlaying)
            {
                breathingAudioSource.Stop();
            }
        }

        /// <summary>
        /// ヘッドボブを更新
        /// </summary>
        private void UpdateHeadBob()
        {
            if (!isMoving || cameraContainer == null) return;

            float bobAmount = headBobIntensity * (isRunning ? 2f : 1f);
            float bobSpeed = headBobFrequency * GetCurrentMovementSpeed();

            float bobOffsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            cameraContainer.localPosition = originalCameraPosition + Vector3.up * bobOffsetY;
        }

        /// <summary>
        /// カメラシェイクを適用
        /// </summary>
        private void ApplyCameraShake()
        {
            if (cameraContainer != null)
            {
                cameraContainer.localPosition += cameraShakeOffset;
            }
        }

        /// <summary>
        /// オーディオソースを設定
        /// </summary>
        private void SetupAudioSources()
        {
            if (footstepAudioSource != null)
            {
                footstepAudioSource.spatialBlend = 0f; // 2D audio
                footstepAudioSource.volume = 0.7f;
            }

            if (breathingAudioSource != null)
            {
                breathingAudioSource.spatialBlend = 0f; // 2D audio
                breathingAudioSource.volume = 0.5f;
            }

            if (heartbeatAudioSource != null)
            {
                heartbeatAudioSource.spatialBlend = 0f; // 2D audio
                heartbeatAudioSource.volume = 0.3f;
            }
        }

        /// <summary>
        /// カーソルをロック
        /// </summary>
        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// インベントリデバッグを切り替え
        /// </summary>
        private void ToggleInventoryDebug()
        {
            if (inventoryComponent != null)
            {
                Debug.Log($"[SH_PlayerController] Inventory: {inventoryComponent.UsedSlots}/{inventoryComponent.MaxSlots} slots used");
            }
        }

        // イベントハンドラー
        private void OnSanityChanged(float sanity)
        {
            Debug.Log($"[SH_PlayerController] Sanity changed: {sanity:F1}");
        }

        private void OnHealthChanged(float health)
        {
            Debug.Log($"[SH_PlayerController] Health changed: {health:F1}");
        }

        private void OnPlayerDeath()
        {
            Debug.Log("[SH_PlayerController] Player died!");
            // 死亡処理の実装
            enabled = false;
        }

        // Public API
        public void SetFearState(bool inDanger)
        {
            isInDanger = inDanger;
        }

        public void AddFear(float amount)
        {
            if (sanityComponent != null)
            {
                sanityComponent.DecreaseSanity(amount);
            }
        }

        // Debug Gizmos
        private void OnDrawGizmosSelected()
        {
            if (interactionPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(interactionPoint.position, interactionRange);
            }
        }
    }
}
