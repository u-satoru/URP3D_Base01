using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// インタラクションコマンドの定義。
    /// プレイヤーの環境オブジェクトとの相互作用をカプセル化します。
    /// 
    /// 主な機能：
    /// - オブジェクトとのインタラクション（ドア、スイッチ、NPC等）
    /// - インタラクション範囲と条件の管理
    /// - インタラクション時のアニメーションとエフェクト
    /// - 複数段階のインタラクション対応
    /// </summary>
    [System.Serializable]
    public class InteractCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// インタラクションの種類を定義する列挙型
        /// </summary>
        public enum InteractionType
        {
            Instant,        // 瞬間的なインタラクション
            Hold,           // 長押しインタラクション
            Multi,          // 複数回インタラクション
            Contextual,     // 文脈依存インタラクション
            Proximity       // 近接自動インタラクション
        }

        [Header("Interaction Parameters")]
        public InteractionType interactionType = InteractionType.Instant;
        public float interactionRange = 2f;
        public LayerMask interactableLayer = -1;
        public string targetTag = "Interactable";

        [Header("Hold Interaction")]
        [Tooltip("長押しインタラクションの必要時間")]
        public float holdDuration = 1f;
        [Tooltip("長押し中にキャンセル可能か")]
        public bool canCancelHold = true;

        [Header("Multi Interaction")]
        [Tooltip("必要なインタラクション回数")]
        public int requiredInteractions = 3;
        [Tooltip("インタラクション間の最大間隔")]
        public float maxInteractionInterval = 2f;

        [Header("Requirements")]
        public bool requiresLineOfSight = true;
        public bool requiresFacing = true;
        [Tooltip("必要な向きの角度範囲（度）")]
        public float facingAngle = 90f;

        [Header("Animation")]
        public bool playAnimation = true;
        public string animationTrigger = "Interact";
        public float animationDuration = 1f;

        [Header("Effects")]
        public bool showInteractionPrompt = true;
        public string promptText = "Press E to interact";
        public bool showProgressBar = false; // 長押し時等

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public InteractCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public InteractCommandDefinition(InteractionType type, float range, string tag = "Interactable")
        {
            interactionType = type;
            interactionRange = range;
            targetTag = tag;
        }

        /// <summary>
        /// インタラクションコマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (interactionRange <= 0f) return false;
            
            if (interactionType == InteractionType.Hold && holdDuration <= 0f) return false;
            if (interactionType == InteractionType.Multi && requiredInteractions <= 0) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // 範囲内にインタラクト可能オブジェクトがあるかチェック
                // 視線チェック（requiresLineOfSight）
                // 向きチェック（requiresFacing）
                // プレイヤーの状態チェック（アニメーション中は不可等）
            }

            return true;
        }

        /// <summary>
        /// インタラクションコマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new InteractCommand(this, context);
        }
    }

    /// <summary>
    /// InteractCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
    public class InteractCommand : ICommand
    {
        private InteractCommandDefinition definition;
        private object context;
        private bool executed = false;
        private GameObject targetObject;
        private bool isInteracting = false;
        private float interactionProgress = 0f;
        private int currentInteractionCount = 0;

        public InteractCommand(InteractCommandDefinition interactDefinition, object executionContext)
        {
            definition = interactDefinition;
            context = executionContext;
        }

        /// <summary>
        /// インタラクションコマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // インタラクション対象を検索
            targetObject = FindInteractableTarget();
            if (targetObject == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning("No interactable target found within range");
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.interactionType} interaction with {targetObject.name}");
#endif

            switch (definition.interactionType)
            {
                case InteractCommandDefinition.InteractionType.Instant:
                    ExecuteInstantInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Hold:
                    StartHoldInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Multi:
                    ExecuteMultiInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Contextual:
                    ExecuteContextualInteraction();
                    break;
                case InteractCommandDefinition.InteractionType.Proximity:
                    ExecuteProximityInteraction();
                    break;
            }

            executed = true;
        }

        /// <summary>
        /// インタラクト可能な対象を検索
        /// </summary>
        private GameObject FindInteractableTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // 範囲内のオブジェクトを検索
            Collider[] nearbyObjects = Physics.OverlapSphere(mono.transform.position, definition.interactionRange, definition.interactableLayer);
            
            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var obj in nearbyObjects)
            {
                // タグチェック
                if (!string.IsNullOrEmpty(definition.targetTag) && !obj.CompareTag(definition.targetTag))
                    continue;

                // 視線チェック
                if (definition.requiresLineOfSight && !HasLineOfSight(mono.transform, obj.transform))
                    continue;

                // 向きチェック
                if (definition.requiresFacing && !IsFacing(mono.transform, obj.transform))
                    continue;

                // 最も近いオブジェクトを選択
                float distance = Vector3.Distance(mono.transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = obj.gameObject;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// 視線判定
        /// </summary>
        private bool HasLineOfSight(Transform from, Transform to)
        {
            Vector3 direction = to.position - from.position;
            RaycastHit hit;
            
            if (Physics.Raycast(from.position, direction.normalized, out hit, direction.magnitude))
            {
                return hit.collider.transform == to;
            }
            
            return true;
        }

        /// <summary>
        /// 向き判定
        /// </summary>
        private bool IsFacing(Transform from, Transform to)
        {
            Vector3 directionToTarget = (to.position - from.position).normalized;
            float angle = Vector3.Angle(from.forward, directionToTarget);
            return angle <= definition.facingAngle * 0.5f;
        }

        /// <summary>
        /// 瞬間インタラクションの実行
        /// </summary>
        private void ExecuteInstantInteraction()
        {
            if (targetObject != null)
            {
                // インタラクト可能コンポーネントの呼び出し
                var interactable = targetObject.GetComponent<IInteractable>();
                interactable?.OnInteract(context);

                PlayInteractionAnimation();
                ShowInteractionEffect();
            }
        }

        /// <summary>
        /// 長押しインタラクションの開始
        /// </summary>
        private void StartHoldInteraction()
        {
            isInteracting = true;
            interactionProgress = 0f;

            // 継続的な更新処理の開始（実際の実装では Coroutine またはUpdateLoop）
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started hold interaction: {definition.holdDuration}s required");
#endif
        }

        /// <summary>
        /// 複数回インタラクションの実行
        /// </summary>
        private void ExecuteMultiInteraction()
        {
            currentInteractionCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Multi interaction: {currentInteractionCount}/{definition.requiredInteractions}");
#endif

            if (currentInteractionCount >= definition.requiredInteractions)
            {
                // 必要回数に達した場合の処理
                CompleteMultiInteraction();
            }
            else
            {
                // まだ必要回数に達していない場合のフィードバック
                ShowProgressFeedback();
            }
        }

        /// <summary>
        /// 文脈依存インタラクションの実行
        /// </summary>
        private void ExecuteContextualInteraction()
        {
            // 現在の状況に応じて異なる処理を実行
            // 例：時間帯、アイテム所持状況、クエスト進行状況等
            
            var interactable = targetObject?.GetComponent<IContextualInteractable>();
            interactable?.OnContextualInteract(context, GetCurrentContext());
        }

        /// <summary>
        /// 近接自動インタラクションの実行
        /// </summary>
        private void ExecuteProximityInteraction()
        {
            // プレイヤーが範囲内にいる間、自動的に継続されるインタラクション
            var interactable = targetObject?.GetComponent<IProximityInteractable>();
            interactable?.OnProximityInteract(context);
        }

        /// <summary>
        /// 複数回インタラクションの完了処理
        /// </summary>
        private void CompleteMultiInteraction()
        {
            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);
            
            currentInteractionCount = 0;
            ShowInteractionEffect();
        }

        /// <summary>
        /// 長押しインタラクションの更新（外部から定期的に呼び出される）
        /// </summary>
        public void UpdateHoldInteraction(float deltaTime)
        {
            if (!isInteracting || definition.interactionType != InteractCommandDefinition.InteractionType.Hold)
                return;

            interactionProgress += deltaTime;

            // プログレスバーの更新
            if (definition.showProgressBar)
            {
                float progress = interactionProgress / definition.holdDuration;
                // UI更新処理
            }

            // 完了チェック
            if (interactionProgress >= definition.holdDuration)
            {
                CompleteHoldInteraction();
            }
        }

        /// <summary>
        /// 長押しインタラクションの完了
        /// </summary>
        private void CompleteHoldInteraction()
        {
            isInteracting = false;
            interactionProgress = 0f;

            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);

            ShowInteractionEffect();
        }

        /// <summary>
        /// インタラクションアニメーションの再生
        /// </summary>
        private void PlayInteractionAnimation()
        {
            if (!definition.playAnimation || context is not MonoBehaviour mono) return;

            var animator = mono.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(definition.animationTrigger))
            {
                animator.SetTrigger(definition.animationTrigger);
            }
        }

        /// <summary>
        /// インタラクションエフェクトの表示
        /// </summary>
        private void ShowInteractionEffect()
        {
            // パーティクルエフェクト
            // サウンドエフェクト
            // UIフィードバック

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Showing interaction effect");
#endif
        }

        /// <summary>
        /// 進行状況フィードバックの表示
        /// </summary>
        private void ShowProgressFeedback()
        {
            // 進行状況のUI表示
            // サウンドフィードバック
        }

        /// <summary>
        /// 現在のコンテキスト情報を取得
        /// </summary>
        private object GetCurrentContext()
        {
            // 時間帯、所持アイテム、クエスト状況等を含むコンテキスト情報を返す
            return new { TimeOfDay = "Day", HasKey = false };
        }

        /// <summary>
        /// インタラクションのキャンセル
        /// </summary>
        public void CancelInteraction()
        {
            if (isInteracting && definition.canCancelHold)
            {
                isInteracting = false;
                interactionProgress = 0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log("Interaction cancelled");
#endif
            }
        }

        /// <summary>
        /// Undo操作（インタラクションの取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // インタラクションの逆操作（可能な場合）
            var interactable = targetObject?.GetComponent<IUndoableInteractable>();
            interactable?.OnUndoInteract(context);

            // 進行中のインタラクションをキャンセル
            CancelInteraction();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Interaction undone");
#endif

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed && targetObject?.GetComponent<IUndoableInteractable>() != null;

        /// <summary>
        /// 現在インタラクション中かどうか
        /// </summary>
        public bool IsInteracting => isInteracting;
    }

    /// <summary>
    /// 基本的なインタラクト可能オブジェクトのインターフェース
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(object interactor);
    }

    /// <summary>
    /// 文脈依存インタラクト可能オブジェクトのインターフェース
    /// </summary>
    public interface IContextualInteractable
    {
        void OnContextualInteract(object interactor, object context);
    }

    /// <summary>
    /// 近接自動インタラクト可能オブジェクトのインターフェース
    /// </summary>
    public interface IProximityInteractable
    {
        void OnProximityInteract(object interactor);
    }

    /// <summary>
    /// Undo可能インタラクト可能オブジェクトのインターフェース
    /// </summary>
    public interface IUndoableInteractable
    {
        void OnUndoInteract(object interactor);
    }
}