using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Components;

namespace asterivo.Unity60.Features.Templates.SurvivalHorror
{
    /// <summary>
    /// SurvivalHorrorアイテムのピックアップコンポーネント
    /// プレイヤーとの相互作用、インベントリ統合、視覚的フィードバックを管理
    /// </summary>
    public class SH_ItemPickup : MonoBehaviour
    {
        [Header("Item Configuration")]
        [SerializeField] private SH_ItemData itemData;
        [SerializeField] private int quantity = 1;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 2.0f;
        [SerializeField] private bool autoPickup = false;
        [SerializeField] private bool requireInteraction = true;
        [SerializeField] private string interactionPrompt = "Eキーで取得";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private Renderer itemRenderer;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private float highlightIntensity = 1.5f;
        [SerializeField] private bool enableGlow = true;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip cannotPickupSound;

        [Header("Animation")]
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private float hoverHeight = 0.2f;
        [SerializeField] private float hoverSpeed = 2.0f;
        [SerializeField] private bool enableRotation = true;
        [SerializeField] private float rotationSpeed = 30f;

        [Header("Events")]
        [SerializeField] private GameEvent<SH_ItemData> onItemPickedUp;
        [SerializeField] private GameEvent<string> onPickupFailed;

        // Runtime State
        private SH_ResourceManager resourceManager;
        private Transform playerTransform;
        private bool isPlayerInRange = false;
        private bool isHighlighted = false;
        private Vector3 originalPosition;
        private Material originalMaterial;
        private Collider itemCollider;

        // UI References
        private Canvas interactionUI;
        private UnityEngine.UI.Text interactionText;

        public SH_ItemData ItemData => itemData;
        public int Quantity => quantity;
        public bool IsPlayerInRange => isPlayerInRange;

        private void Awake()
        {
            CacheComponents();
            SetupInteractionUI();
            originalPosition = transform.position;
        }

        private void Start()
        {
            if (itemData != null)
            {
                SetupVisualAppearance();
            }
        }

        private void Update()
        {
            if (enableHoverAnimation)
            {
                UpdateHoverAnimation();
            }

            if (enableRotation)
            {
                UpdateRotationAnimation();
            }

            UpdatePlayerInteraction();
        }

        /// <summary>
        /// アイテムピックアップを初期化
        /// </summary>
        public void Initialize(SH_ItemData data, SH_ResourceManager manager)
        {
            itemData = data;
            resourceManager = manager;

            if (itemData != null)
            {
                SetupVisualAppearance();
                SetupAudio();

                Debug.Log($"[SH_ItemPickup] Initialized with {itemData.ItemName}");
            }
        }

        /// <summary>
        /// アイテムを手動でピックアップ試行
        /// </summary>
        public bool TryPickup(GameObject picker)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[SH_ItemPickup] Cannot pickup: no item data");
                return false;
            }

            // インベントリコンポーネントを取得
            var inventory = picker.GetComponent<LimitedInventoryComponent>();
            if (inventory == null)
            {
                Debug.LogWarning("[SH_ItemPickup] Picker has no inventory component");
                PlayCannotPickupFeedback();
                return false;
            }

            // インベントリに追加を試行
            var inventoryData = CreateInventoryItemData();
            if (!inventory.TryAddItem(inventoryData, quantity))
            {
                Debug.Log($"[SH_ItemPickup] Cannot pickup {itemData.ItemName}: inventory full");
                PlayCannotPickupFeedback();
                onPickupFailed?.Raise("インベントリが満杯です");
                return false;
            }

            // ピックアップ成功処理
            ExecuteSuccessfulPickup(picker);
            return true;
        }

        /// <summary>
        /// アイテムの数量を設定
        /// </summary>
        public void SetQuantity(int newQuantity)
        {
            quantity = Mathf.Max(1, newQuantity);
        }

        /// <summary>
        /// ハイライト効果を設定
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (isHighlighted == highlighted) return;

            isHighlighted = highlighted;

            if (highlightEffect != null)
            {
                highlightEffect.SetActive(highlighted);
            }

            if (itemRenderer != null && highlightMaterial != null)
            {
                itemRenderer.material = highlighted ? highlightMaterial : originalMaterial;
            }

            if (enableGlow && itemRenderer != null)
            {
                var emission = itemRenderer.material.GetColor("_EmissionColor");
                itemRenderer.material.SetColor("_EmissionColor", highlighted ? emission * highlightIntensity : Color.black);
            }

            UpdateInteractionUI(highlighted);
        }

        /// <summary>
        /// ピックアップの有効性をチェック
        /// </summary>
        public bool CanBePickedUp(GameObject picker)
        {
            if (itemData == null) return false;

            var inventory = picker.GetComponent<LimitedInventoryComponent>();
            if (inventory == null) return false;

            var inventoryData = CreateInventoryItemData();
            // TODO: Implement proper inventory space checking
            // For now, assume pickup is possible - TryAddItem will validate during actual pickup
            return true;
        }

        /// <summary>
        /// コンポーネントをキャッシュ
        /// </summary>
        private void CacheComponents()
        {
            itemCollider = GetComponent<Collider>();
            if (itemCollider == null)
            {
                itemCollider = gameObject.AddComponent<SphereCollider>();
                ((SphereCollider)itemCollider).radius = 0.5f;
                itemCollider.isTrigger = true;
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // 3D audio
            }

            if (itemRenderer == null)
            {
                itemRenderer = GetComponentInChildren<Renderer>();
            }

            // プレイヤーを検索
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }

        /// <summary>
        /// インタラクションUIを設定
        /// </summary>
        private void SetupInteractionUI()
        {
            if (requireInteraction)
            {
                var uiObject = new GameObject("InteractionUI");
                uiObject.transform.SetParent(transform);
                uiObject.transform.localPosition = Vector3.up;

                interactionUI = uiObject.AddComponent<Canvas>();
                interactionUI.renderMode = RenderMode.WorldSpace;
                interactionUI.worldCamera = UnityEngine.Camera.main;
                interactionUI.sortingOrder = 100;

                var textObject = new GameObject("InteractionText");
                textObject.transform.SetParent(uiObject.transform);
                textObject.transform.localPosition = Vector3.zero;
                textObject.transform.localScale = Vector3.one * 0.01f;

                interactionText = textObject.AddComponent<UnityEngine.UI.Text>();
                interactionText.text = interactionPrompt;
                interactionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                interactionText.fontSize = 14;
                interactionText.color = Color.white;
                interactionText.alignment = TextAnchor.MiddleCenter;

                var rectTransform = textObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50);

                interactionUI.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 視覚的外観を設定
        /// </summary>
        private void SetupVisualAppearance()
        {
            if (itemRenderer != null)
            {
                originalMaterial = itemRenderer.material;

                // アイテムアイコンがある場合はスプライトレンダラーとして表示
                if (itemData.Icon != null && itemRenderer is SpriteRenderer spriteRenderer)
                {
                    spriteRenderer.sprite = itemData.Icon;
                }

                // レアリティに基づく色調整
                ApplyRarityVisualEffects();
            }
        }

        /// <summary>
        /// レアリティベースの視覚効果を適用
        /// </summary>
        private void ApplyRarityVisualEffects()
        {
            if (itemRenderer == null) return;

            Color rarityColor = itemData.Rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Legendary => Color.magenta,
                _ => Color.white
            };

            itemRenderer.material.color = rarityColor;

            // レジェンダリーアイテムには追加のパーティクル効果
            if (itemData.Rarity == ItemRarity.Legendary && highlightEffect == null)
            {
                CreateLegendaryEffect();
            }
        }

        /// <summary>
        /// レジェンダリーアイテム用特殊効果を作成
        /// </summary>
        private void CreateLegendaryEffect()
        {
            var effectObject = new GameObject("LegendaryEffect");
            effectObject.transform.SetParent(transform);
            effectObject.transform.localPosition = Vector3.zero;

            var particles = effectObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = Color.magenta;
            main.startSize = 0.1f;
            main.startLifetime = 2.0f;
            main.maxParticles = 20;

            var emission = particles.emission;
            emission.rateOverTime = 10;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            highlightEffect = effectObject;
        }

        /// <summary>
        /// オーディオを設定
        /// </summary>
        private void SetupAudio()
        {
            if (itemData != null && audioSource != null)
            {
                // アイテムデータからピックアップサウンドを取得
                if (itemData.UseSound != null)
                {
                    pickupSound = itemData.UseSound;
                }
            }
        }

        /// <summary>
        /// ホバーアニメーションを更新
        /// </summary>
        private void UpdateHoverAnimation()
        {
            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = originalPosition + Vector3.up * hoverOffset;
        }

        /// <summary>
        /// 回転アニメーションを更新
        /// </summary>
        private void UpdateRotationAnimation()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// プレイヤーとのインタラクションを更新
        /// </summary>
        private void UpdatePlayerInteraction()
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distance <= interactionDistance;

            if (isPlayerInRange != wasInRange)
            {
                SetHighlight(isPlayerInRange);
            }

            // 自動ピックアップ処理
            if (autoPickup && isPlayerInRange && playerTransform.gameObject != null)
            {
                TryPickup(playerTransform.gameObject);
            }

            // インタラクションキー入力チェック
            if (requireInteraction && isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                TryPickup(playerTransform.gameObject);
            }
        }

        /// <summary>
        /// インタラクションUIを更新
        /// </summary>
        private void UpdateInteractionUI(bool show)
        {
            if (interactionUI != null)
            {
                interactionUI.gameObject.SetActive(show && requireInteraction);

                if (show && interactionText != null)
                {
                    // インベントリ状況に応じてプロンプトを変更
                    bool canPickup = playerTransform != null && CanBePickedUp(playerTransform.gameObject);
                    interactionText.text = canPickup ? interactionPrompt : "インベントリが満杯";
                    interactionText.color = canPickup ? Color.white : Color.red;
                }
            }
        }

        /// <summary>
        /// 成功ピックアップを実行
        /// </summary>
        private void ExecuteSuccessfulPickup(GameObject picker)
        {
            // アイテムピックアップイベントを発行
            itemData.OnPickup(picker);
            onItemPickedUp?.Raise(itemData);

            // リソースマネージャーに通知
            if (resourceManager != null)
            {
                resourceManager.OnItemCollected(itemData, gameObject);
            }

            // ピックアップフィードバック
            PlayPickupFeedback();

            // オブジェクトを削除
            Destroy(gameObject);

            Debug.Log($"[SH_ItemPickup] Successfully picked up {itemData.ItemName} x{quantity}");
        }

        /// <summary>
        /// ピックアップ成功フィードバックを再生
        /// </summary>
        private void PlayPickupFeedback()
        {
            if (pickupSound != null && audioSource != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // 視覚的フィードバック（パーティクル等）
            if (highlightEffect != null)
            {
                var feedback = Instantiate(highlightEffect, transform.position, transform.rotation);
                Destroy(feedback, 2f);
            }
        }

        /// <summary>
        /// ピックアップ失敗フィードバックを再生
        /// </summary>
        private void PlayCannotPickupFeedback()
        {
            if (cannotPickupSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(cannotPickupSound);
            }

            // 視覚的失敗フィードバック（赤い点滅等）
            if (itemRenderer != null)
            {
                StartCoroutine(FlashRed());
            }
        }

        /// <summary>
        /// 赤い点滅効果
        /// </summary>
        private System.Collections.IEnumerator FlashRed()
        {
            var originalColor = itemRenderer.material.color;
            itemRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            itemRenderer.material.color = originalColor;
        }

        /// <summary>
        /// インベントリアイテムデータを作成
        /// </summary>
        private InventoryItemData CreateInventoryItemData()
        {
            // Convert SurvivalHorror.ItemType to Core.Components.ItemType
            var coreItemType = itemData.ItemType switch
            {
                ItemType.Consumable => asterivo.Unity60.Core.Components.ItemType.Consumable,
                ItemType.KeyItem => asterivo.Unity60.Core.Components.ItemType.Key,
                ItemType.Equipment => asterivo.Unity60.Core.Components.ItemType.Tool,
                ItemType.Document => asterivo.Unity60.Core.Components.ItemType.Document,
                _ => asterivo.Unity60.Core.Components.ItemType.Misc
            };
            
            return new InventoryItemData(
                itemData.ItemId,
                itemData.ItemName,
                itemData.Description,
                coreItemType
            );
        }

        // Unity Events
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && autoPickup)
            {
                TryPickup(other.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}
