using System;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Quest;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Interaction
{
    /// <summary>
    /// インタラクション可能オブジェクトの基底クラス
    /// 共通機能を提供し、具体的なインタラクションロジックは継承先で実装
    /// </summary>
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        [TabGroup("Basic", "Basic Settings")]
        [Header("Interaction Properties")]
        [SerializeField]
        [Tooltip("インタラクションに表示されるテキスト")]
        protected string interactionText = "Interact";

        [TabGroup("Basic", "Basic Settings")]
        [SerializeField]
        [Tooltip("インタラクションの優先度（高いほど優先）")]
        [Range(0, 10)]
        protected int interactionPriority = 1;

        [TabGroup("Basic", "Basic Settings")]
        [SerializeField]
        [Tooltip("インタラクション可能な範囲")]
        [Min(0.1f)]
        protected float interactionRange = 2.0f;

        [TabGroup("Basic", "Basic Settings")]
        [SerializeField]
        [Tooltip("インタラクション後にオブジェクトが無効になるか")]
        protected bool consumeOnInteraction = false;

        [TabGroup("Basic", "Basic Settings")]
        [SerializeField]
        [Tooltip("インタラクション可能な状態")]
        protected bool isInteractable = true;

        [TabGroup("Requirements", "Requirements")]
        [Header("Interaction Requirements")]
        [SerializeField]
        [Tooltip("インタラクションに必要なアイテムのID一覧")]
        protected string[] requiredItemIds = new string[0];

        [TabGroup("Requirements", "Requirements")]
        [SerializeField]
        [Tooltip("必要なクエスト状態")]
        protected QuestState requiredQuestState = QuestState.None;

        [TabGroup("Requirements", "Requirements")]
        [SerializeField]
        [Tooltip("必要なクエストID")]
        protected string requiredQuestId = "";

        [TabGroup("Visual", "Visual Feedback")]
        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("ターゲット時のハイライト色")]
        protected Color highlightColor = Color.yellow;

        [TabGroup("Visual", "Visual Feedback")]
        [SerializeField]
        [Tooltip("ハイライト用のレンダラー")]
        protected Renderer targetRenderer;

        [TabGroup("Visual", "Visual Feedback")]
        [SerializeField]
        [Tooltip("デバッグモードの有効/無効")]
        protected bool debugMode = false;

        [TabGroup("Events", "Game Events")]
        [Header("Custom Events")]
        [SerializeField]
        [Tooltip("インタラクション成功時のイベント")]
        protected GameEvent onInteractionSuccess;

        [TabGroup("Events", "Game Events")]
        [SerializeField]
        [Tooltip("インタラクション失敗時のイベント")]
        protected GameEvent onInteractionFailed;

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("現在ターゲット中かどうか")]
        protected bool isTargeted = false;

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("元のマテリアル色")]
        protected Color originalColor;

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("最後にインタラクションした時間")]
        protected float lastInteractionTime;

        // 依存システム参照
        protected QuestManager questManager;
        protected AdventureInventoryManager inventoryManager;

        // プロパティ
        public virtual bool IsInteractable => isInteractable;
        public virtual bool IsTargeted => isTargeted;
        public virtual string InteractionText => interactionText;

        // 追加のプロパティ（Adventure Template互換性のため）
        public virtual InteractionType InteractionType => InteractionType.Custom;
        public virtual string InteractionPrompt => interactionText;
        public virtual Sprite InteractionIcon => null;

        // イベント
        public event System.Action<IInteractable> OnInteractionSucceeded;
        public event System.Action<IInteractable> OnInteractionFailed;

        protected virtual void Awake()
        {
            InitializeInteractable();
        }

        protected virtual void Start()
        {
            InitializeDependencies();
        }

        /// <summary>
        /// インタラクション可能オブジェクトの初期化
        /// </summary>
        protected virtual void InitializeInteractable()
        {
            // レンダラーが指定されていない場合は自動取得
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            // 元の色を記録
            if (targetRenderer != null && targetRenderer.material != null)
            {
                originalColor = targetRenderer.material.color;
            }

            LogDebug($"[{GetType().Name}] Initialized interactable: {interactionText}");
        }

        /// <summary>
        /// 依存システムの初期化
        /// </summary>
        protected virtual void InitializeDependencies()
        {
            questManager = asterivo.Unity60.Core.ServiceLocator.GetService<QuestManager>();
            inventoryManager = asterivo.Unity60.Core.ServiceLocator.GetService<AdventureInventoryManager>();
        }

        #region IInteractable Implementation

        /// <summary>
        /// インタラクション可能かどうかを判定
        /// </summary>
        public virtual bool CanInteract()
        {
            if (!isInteractable)
            {
                return false;
            }

            // 必要なアイテムのチェック
            if (!HasRequiredItems())
            {
                return false;
            }

            // クエスト要件のチェック
            if (!CheckQuestRequirements())
            {
                return false;
            }

            // カスタム条件のチェック
            return CanInteractCustom();
        }

        /// <summary>
        /// インタラクションを実行
        /// </summary>
        public virtual bool Interact()
        {
            if (!CanInteract())
            {
                LogWarning($"[{GetType().Name}] Cannot interact with {gameObject.name}");
                OnInteractionFailed?.Invoke(this);
                onInteractionFailed?.Raise();
                return false;
            }

            try
            {
                lastInteractionTime = Time.time;

                // 具体的なインタラクション処理を実行
                bool success = PerformInteraction();

                if (success)
                {
                    OnInteractionSucceeded?.Invoke(this);
                    onInteractionSuccess?.Raise();
                    LogDebug($"[{GetType().Name}] Interaction successful with {gameObject.name}");

                    // インタラクション後の処理
                    OnInteractionComplete();

                    // 消費型の場合は無効化
                    if (consumeOnInteraction)
                    {
                        ConsumeInteractable();
                    }
                }
                else
                {
                    OnInteractionFailed?.Invoke(this);
                    onInteractionFailed?.Raise();
                    LogWarning($"[{GetType().Name}] Interaction failed with {gameObject.name}");
                }

                return success;
            }
            catch (System.Exception ex)
            {
                LogError($"[{GetType().Name}] Exception during interaction: {ex.Message}");
                OnInteractionFailed?.Invoke(this);
                onInteractionFailed?.Raise();
                return false;
            }
        }

        /// <summary>
        /// インタラクション時に表示するテキストを取得
        /// </summary>
        public virtual string GetInteractionText()
        {
            if (!CanInteract())
            {
                return GetUnavailableInteractionText();
            }

            return interactionText;
        }

        /// <summary>
        /// インタラクションの優先度を取得
        /// </summary>
        public virtual int GetInteractionPriority()
        {
            return interactionPriority;
        }

        /// <summary>
        /// プレイヤーがこのオブジェクトをターゲットした時の処理
        /// </summary>
        public virtual void OnTargetSelected()
        {
            if (isTargeted) return;

            isTargeted = true;

            // ビジュアルフィードバック
            ApplyHighlight(true);

            // カスタム処理
            OnTargetSelectedCustom();

            LogDebug($"[{GetType().Name}] Target selected: {gameObject.name}");
        }

        /// <summary>
        /// プレイヤーがこのオブジェクトのターゲットを外した時の処理
        /// </summary>
        public virtual void OnTargetLost()
        {
            if (!isTargeted) return;

            isTargeted = false;

            // ビジュアルフィードバック
            ApplyHighlight(false);

            // カスタム処理
            OnTargetLostCustom();

            LogDebug($"[{GetType().Name}] Target lost: {gameObject.name}");
        }

        /// <summary>
        /// インタラクション範囲を取得
        /// </summary>
        public virtual float GetInteractionRange()
        {
            return interactionRange;
        }

        /// <summary>
        /// インタラクションに必要なアイテムがあるかチェック
        /// </summary>
        public virtual bool HasRequiredItems()
        {
            if (inventoryManager == null || requiredItemIds.Length == 0)
            {
                return true;
            }

            foreach (var itemId in requiredItemIds)
            {
                if (!inventoryManager.CheckItemAvailability(itemId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// インタラクション後にオブジェクトが無効になるかどうか
        /// </summary>
        public virtual bool IsConsumedOnInteraction()
        {
            return consumeOnInteraction;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 具体的なインタラクション処理（継承先で実装）
        /// </summary>
        protected abstract bool PerformInteraction();

        /// <summary>
        /// カスタムインタラクション可能条件（継承先でオーバーライド可能）
        /// </summary>
        protected virtual bool CanInteractCustom()
        {
            return true;
        }

        /// <summary>
        /// インタラクション不可時のテキスト（継承先でオーバーライド可能）
        /// </summary>
        protected virtual string GetUnavailableInteractionText()
        {
            if (!HasRequiredItems())
            {
                return "Required item missing";
            }

            if (!CheckQuestRequirements())
            {
                return "Quest requirement not met";
            }

            return "Cannot interact";
        }

        /// <summary>
        /// ターゲット選択時のカスタム処理（継承先でオーバーライド可能）
        /// </summary>
        protected virtual void OnTargetSelectedCustom()
        {
            // 継承先で実装
        }

        /// <summary>
        /// ターゲット消失時のカスタム処理（継承先でオーバーライド可能）
        /// </summary>
        protected virtual void OnTargetLostCustom()
        {
            // 継承先で実装
        }

        /// <summary>
        /// インタラクション完了後の処理（継承先でオーバーライド可能）
        /// </summary>
        protected virtual void OnInteractionComplete()
        {
            // 継承先で実装
        }

        /// <summary>
        /// クエスト要件のチェック
        /// </summary>
        protected virtual bool CheckQuestRequirements()
        {
            if (questManager == null || string.IsNullOrEmpty(requiredQuestId) || requiredQuestState == QuestState.None)
            {
                return true;
            }

            var questState = questManager.GetQuestState(requiredQuestId);
            return questState == requiredQuestState;
        }

        /// <summary>
        /// ハイライト効果の適用/解除
        /// </summary>
        protected virtual void ApplyHighlight(bool highlight)
        {
            if (targetRenderer == null || targetRenderer.material == null) return;

            if (highlight)
            {
                targetRenderer.material.color = highlightColor;
            }
            else
            {
                targetRenderer.material.color = originalColor;
            }
        }

        /// <summary>
        /// インタラクション可能オブジェクトの消費処理
        /// </summary>
        protected virtual void ConsumeInteractable()
        {
            isInteractable = false;
            gameObject.SetActive(false);
        }

        #endregion

        #region Debug Support

        [TabGroup("Debug", "Debug Tools")]
        [Button("Force Interact")]
        [ShowIf("debugMode")]
        private void DebugForceInteract()
        {
            bool success = Interact();
            LogDebug($"[{GetType().Name}] Debug force interact result: {success}");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Test Requirements")]
        [ShowIf("debugMode")]
        private void DebugTestRequirements()
        {
            LogDebug($"[{GetType().Name}] Can Interact: {CanInteract()}");
            LogDebug($"[{GetType().Name}] Has Required Items: {HasRequiredItems()}");
            LogDebug($"[{GetType().Name}] Quest Requirements Met: {CheckQuestRequirements()}");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Toggle Highlight")]
        [ShowIf("debugMode")]
        private void DebugToggleHighlight()
        {
            ApplyHighlight(!isTargeted);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!debugMode) return;

            // インタラクション範囲の表示
            Gizmos.color = isInteractable ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // インタラクション可能な状態の表示
            if (isTargeted)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2, Vector3.one * 0.2f);
            }
        }

        protected virtual void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        protected virtual void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        protected virtual void LogError(string message)
        {
            Debug.LogError(message);
        }

        /// <summary>
        /// プレイヤーがインタラクション範囲に入った時の処理
        /// </summary>
        public virtual void OnInteractionEnter(GameObject player)
        {
            // 基本実装: ハイライト効果を適用
            ApplyHighlight(true);

            // 継承先で追加処理を実装可能
        }

        /// <summary>
        /// プレイヤーがインタラクション範囲を出た時の処理
        /// </summary>
        public virtual void OnInteractionExit(GameObject player)
        {
            // 基本実装: ハイライト効果を解除
            ApplyHighlight(false);

            // 継承先で追加処理を実装可能
        }

        #endregion
    }
}