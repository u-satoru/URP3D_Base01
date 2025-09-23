using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// インタラクションコマンド�E定義、E    /// プレイヤーの環墁E��ブジェクトとの相互作用をカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - オブジェクトとのインタラクション�E�ドア、スイチE��、NPC等！E    /// - インタラクション篁E��と条件の管琁E    /// - インタラクション時�EアニメーションとエフェクチE    /// - 褁E��段階�Eインタラクション対忁E    /// </summary>
    [System.Serializable]
    public class InteractCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// インタラクションの種類を定義する列挙垁E        /// </summary>
        public enum InteractionType
        {
            Instant,        // 瞬間的なインタラクション
            Hold,           // 長押しインタラクション
            Multi,          // 褁E��回インタラクション
            Contextual,     // 斁E��依存インタラクション
            Proximity       // 近接自動インタラクション
        }

        [Header("Interaction Parameters")]
        public InteractionType interactionType = InteractionType.Instant;
        public float interactionRange = 2f;
        public LayerMask interactableLayer = -1;
        public string targetTag = "Interactable";

        [Header("Hold Interaction")]
        [Tooltip("長押しインタラクションの忁E��時閁E)]
        public float holdDuration = 1f;
        [Tooltip("長押し中にキャンセル可能ぁE)]
        public bool canCancelHold = true;

        [Header("Multi Interaction")]
        [Tooltip("忁E��なインタラクション回数")]
        public int requiredInteractions = 3;
        [Tooltip("インタラクション間�E最大間隔")]
        public float maxInteractionInterval = 2f;

        [Header("Requirements")]
        public bool requiresLineOfSight = true;
        public bool requiresFacing = true;
        [Tooltip("忁E��な向きの角度篁E���E�度�E�E)]
        public float facingAngle = 90f;

        [Header("Animation")]
        public bool playAnimation = true;
        public string animationTrigger = "Interact";
        public float animationDuration = 1f;

        [Header("Effects")]
        public bool showInteractionPrompt = true;
        public string promptText = "Press E to interact";
        public bool showProgressBar = false; // 長押し時筁E
        /// <summary>
        /// チE��ォルトコンストラクタ
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
        /// インタラクションコマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (interactionRange <= 0f) return false;
            
            if (interactionType == InteractionType.Hold && holdDuration <= 0f) return false;
            if (interactionType == InteractionType.Multi && requiredInteractions <= 0) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // 篁E��冁E��インタラクト可能オブジェクトがあるかチェチE��
                // 視線チェチE���E�EequiresLineOfSight�E�E                // 向きチェチE���E�EequiresFacing�E�E                // プレイヤーの状態チェチE���E�アニメーション中は不可等！E            }

            return true;
        }

        /// <summary>
        /// インタラクションコマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new InteractCommand(this, context);
        }
    }

    /// <summary>
    /// InteractCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
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
        /// インタラクションコマンド�E実衁E        /// </summary>
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

            // 篁E��冁E�Eオブジェクトを検索
            Collider[] nearbyObjects = Physics.OverlapSphere(mono.transform.position, definition.interactionRange, definition.interactableLayer);
            
            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var obj in nearbyObjects)
            {
                // タグチェチE��
                if (!string.IsNullOrEmpty(definition.targetTag) && !obj.CompareTag(definition.targetTag))
                    continue;

                // 視線チェチE��
                if (definition.requiresLineOfSight && !HasLineOfSight(mono.transform, obj.transform))
                    continue;

                // 向きチェチE��
                if (definition.requiresFacing && !IsFacing(mono.transform, obj.transform))
                    continue;

                // 最も近いオブジェクトを選抁E                float distance = Vector3.Distance(mono.transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = obj.gameObject;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// 視線判宁E        /// </summary>
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
        /// 向き判宁E        /// </summary>
        private bool IsFacing(Transform from, Transform to)
        {
            Vector3 directionToTarget = (to.position - from.position).normalized;
            float angle = Vector3.Angle(from.forward, directionToTarget);
            return angle <= definition.facingAngle * 0.5f;
        }

        /// <summary>
        /// 瞬間インタラクションの実衁E        /// </summary>
        private void ExecuteInstantInteraction()
        {
            if (targetObject != null)
            {
                // インタラクト可能コンポ�Eネント�E呼び出ぁE                var interactable = targetObject.GetComponent<IInteractable>();
                interactable?.OnInteract(context);

                PlayInteractionAnimation();
                ShowInteractionEffect();
            }
        }

        /// <summary>
        /// 長押しインタラクションの開姁E        /// </summary>
        private void StartHoldInteraction()
        {
            isInteracting = true;
            interactionProgress = 0f;

            // 継続的な更新処琁E�E開始（実際の実裁E��は Coroutine また�EUpdateLoop�E�E#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started hold interaction: {definition.holdDuration}s required");
#endif
        }

        /// <summary>
        /// 褁E��回インタラクションの実衁E        /// </summary>
        private void ExecuteMultiInteraction()
        {
            currentInteractionCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Multi interaction: {currentInteractionCount}/{definition.requiredInteractions}");
#endif

            if (currentInteractionCount >= definition.requiredInteractions)
            {
                // 忁E��回数に達した場合�E処琁E                CompleteMultiInteraction();
            }
            else
            {
                // まだ忁E��回数に達してぁE��ぁE��合�EフィードバチE��
                ShowProgressFeedback();
            }
        }

        /// <summary>
        /// 斁E��依存インタラクションの実衁E        /// </summary>
        private void ExecuteContextualInteraction()
        {
            // 現在の状況に応じて異なる�E琁E��実衁E            // 例：時間帯、アイチE��所持状況、クエスト進行状況筁E            
            var interactable = targetObject?.GetComponent<IContextualInteractable>();
            interactable?.OnContextualInteract(context, GetCurrentContext());
        }

        /// <summary>
        /// 近接自動インタラクションの実衁E        /// </summary>
        private void ExecuteProximityInteraction()
        {
            // プレイヤーが篁E��冁E��ぁE��間、�E動的に継続されるインタラクション
            var interactable = targetObject?.GetComponent<IProximityInteractable>();
            interactable?.OnProximityInteract(context);
        }

        /// <summary>
        /// 褁E��回インタラクションの完亁E�E琁E        /// </summary>
        private void CompleteMultiInteraction()
        {
            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);
            
            currentInteractionCount = 0;
            ShowInteractionEffect();
        }

        /// <summary>
        /// 長押しインタラクションの更新�E�外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateHoldInteraction(float deltaTime)
        {
            if (!isInteracting || definition.interactionType != InteractCommandDefinition.InteractionType.Hold)
                return;

            interactionProgress += deltaTime;

            // プログレスバ�Eの更新
            if (definition.showProgressBar)
            {
                float progress = interactionProgress / definition.holdDuration;
                // UI更新処琁E            }

            // 完亁E��ェチE��
            if (interactionProgress >= definition.holdDuration)
            {
                CompleteHoldInteraction();
            }
        }

        /// <summary>
        /// 長押しインタラクションの完亁E        /// </summary>
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
        /// インタラクションエフェクト�E表示
        /// </summary>
        private void ShowInteractionEffect()
        {
            // パ�EチE��クルエフェクチE            // サウンドエフェクチE            // UIフィードバチE��

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Showing interaction effect");
#endif
        }

        /// <summary>
        /// 進行状況フィードバチE��の表示
        /// </summary>
        private void ShowProgressFeedback()
        {
            // 進行状況�EUI表示
            // サウンドフィードバチE��
        }

        /// <summary>
        /// 現在のコンチE��スト情報を取征E        /// </summary>
        private object GetCurrentContext()
        {
            // 時間帯、所持アイチE��、クエスト状況等を含むコンチE��スト情報を返す
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
        /// Undo操作（インタラクションの取り消し�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // インタラクションの送E��作（可能な場合！E            var interactable = targetObject?.GetComponent<IUndoableInteractable>();
            interactable?.OnUndoInteract(context);

            // 進行中のインタラクションをキャンセル
            CancelInteraction();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Interaction undone");
#endif

            executed = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && targetObject?.GetComponent<IUndoableInteractable>() != null;

        /// <summary>
        /// 現在インタラクション中かどぁE��
        /// </summary>
        public bool IsInteracting => isInteracting;
    }

    /// <summary>
    /// 基本皁E��インタラクト可能オブジェクト�Eインターフェース
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(object interactor);
    }

    /// <summary>
    /// 斁E��依存インタラクト可能オブジェクト�Eインターフェース
    /// </summary>
    public interface IContextualInteractable
    {
        void OnContextualInteract(object interactor, object context);
    }

    /// <summary>
    /// 近接自動インタラクト可能オブジェクト�Eインターフェース
    /// </summary>
    public interface IProximityInteractable
    {
        void OnProximityInteract(object interactor);
    }

    /// <summary>
    /// Undo可能インタラクト可能オブジェクト�Eインターフェース
    /// </summary>
    public interface IUndoableInteractable
    {
        void OnUndoInteract(object interactor);
    }
}