using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｳ繝槭Φ繝峨・螳夂ｾｩ縲・    /// 繝励Ξ繧､繝､繝ｼ縺ｮ迺ｰ蠅・が繝悶ず繧ｧ繧ｯ繝医→縺ｮ逶ｸ莠剃ｽ懃畑繧偵き繝励そ繝ｫ蛹悶＠縺ｾ縺吶・    /// 
    /// 荳ｻ縺ｪ讖溯・・・    /// - 繧ｪ繝悶ず繧ｧ繧ｯ繝医→縺ｮ繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ・医ラ繧｢縲√せ繧､繝・メ縲¨PC遲会ｼ・    /// - 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ遽・峇縺ｨ譚｡莉ｶ縺ｮ邂｡逅・    /// - 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ譎ゅ・繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｨ繧ｨ繝輔ぉ繧ｯ繝・    /// - 隍・焚谿ｵ髫弱・繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蟇ｾ蠢・    /// </summary>
    [System.Serializable]
    public class InteractCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ遞ｮ鬘槭ｒ螳夂ｾｩ縺吶ｋ蛻玲嫌蝙・        /// </summary>
        public enum InteractionType
        {
            Instant,        // 迸ｬ髢鍋噪縺ｪ繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
            Hold,           // 髟ｷ謚ｼ縺励う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
            Multi,          // 隍・焚蝗槭う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
            Contextual,     // 譁・ц萓晏ｭ倥う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
            Proximity       // 霑第磁閾ｪ蜍輔う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
        }

        [Header("Interaction Parameters")]
        public InteractionType interactionType = InteractionType.Instant;
        public float interactionRange = 2f;
        public LayerMask interactableLayer = -1;
        public string targetTag = "Interactable";

        [Header("Hold Interaction")]
        [Tooltip("髟ｷ謚ｼ縺励う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ蠢・ｦ∵凾髢・)]
        public float holdDuration = 1f;
        [Tooltip("髟ｷ謚ｼ縺嶺ｸｭ縺ｫ繧ｭ繝｣繝ｳ繧ｻ繝ｫ蜿ｯ閭ｽ縺・)]
        public bool canCancelHold = true;

        [Header("Multi Interaction")]
        [Tooltip("蠢・ｦ√↑繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蝗樊焚")]
        public int requiredInteractions = 3;
        [Tooltip("繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ髢薙・譛螟ｧ髢馴囈")]
        public float maxInteractionInterval = 2f;

        [Header("Requirements")]
        public bool requiresLineOfSight = true;
        public bool requiresFacing = true;
        [Tooltip("蠢・ｦ√↑蜷代″縺ｮ隗貞ｺｦ遽・峇・亥ｺｦ・・)]
        public float facingAngle = 90f;

        [Header("Animation")]
        public bool playAnimation = true;
        public string animationTrigger = "Interact";
        public float animationDuration = 1f;

        [Header("Effects")]
        public bool showInteractionPrompt = true;
        public string promptText = "Press E to interact";
        public bool showProgressBar = false; // 髟ｷ謚ｼ縺玲凾遲・
        /// <summary>
        /// 繝・ヵ繧ｩ繝ｫ繝医さ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public InteractCommandDefinition()
        {
        }

        /// <summary>
        /// 繝代Λ繝｡繝ｼ繧ｿ莉倥″繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ
        /// </summary>
        public InteractCommandDefinition(InteractionType type, float range, string tag = "Interactable")
        {
            interactionType = type;
            interactionRange = range;
            targetTag = tag;
        }

        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｳ繝槭Φ繝峨′螳溯｡悟庄閭ｽ縺九←縺・°繧貞愛螳壹＠縺ｾ縺・        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 蝓ｺ譛ｬ逧・↑螳溯｡悟庄閭ｽ諤ｧ繝√ぉ繝・け
            if (interactionRange <= 0f) return false;
            
            if (interactionType == InteractionType.Hold && holdDuration <= 0f) return false;
            if (interactionType == InteractionType.Multi && requiredInteractions <= 0) return false;

            // 繧ｳ繝ｳ繝・く繧ｹ繝医′縺ゅｋ蝣ｴ蜷医・霑ｽ蜉繝√ぉ繝・け
            if (context != null)
            {
                // 遽・峇蜀・↓繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医′縺ゅｋ縺九メ繧ｧ繝・け
                // 隕也ｷ壹メ繧ｧ繝・け・・equiresLineOfSight・・                // 蜷代″繝√ぉ繝・け・・equiresFacing・・                // 繝励Ξ繧､繝､繝ｼ縺ｮ迥ｶ諷九メ繧ｧ繝・け・医い繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ荳ｭ縺ｯ荳榊庄遲会ｼ・            }

            return true;
        }

        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｳ繝槭Φ繝峨ｒ菴懈・縺励∪縺・        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new InteractCommand(this, context);
        }
    }

    /// <summary>
    /// InteractCommandDefinition縺ｫ蟇ｾ蠢懊☆繧句ｮ滄圀縺ｮ繧ｳ繝槭Φ繝牙ｮ溯｣・    /// </summary>
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
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｳ繝槭Φ繝峨・螳溯｡・        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ蟇ｾ雎｡繧呈､懃ｴ｢
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
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ縺ｪ蟇ｾ雎｡繧呈､懃ｴ｢
        /// </summary>
        private GameObject FindInteractableTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // 遽・峇蜀・・繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ讀懃ｴ｢
            Collider[] nearbyObjects = Physics.OverlapSphere(mono.transform.position, definition.interactionRange, definition.interactableLayer);
            
            GameObject closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var obj in nearbyObjects)
            {
                // 繧ｿ繧ｰ繝√ぉ繝・け
                if (!string.IsNullOrEmpty(definition.targetTag) && !obj.CompareTag(definition.targetTag))
                    continue;

                // 隕也ｷ壹メ繧ｧ繝・け
                if (definition.requiresLineOfSight && !HasLineOfSight(mono.transform, obj.transform))
                    continue;

                // 蜷代″繝√ぉ繝・け
                if (definition.requiresFacing && !IsFacing(mono.transform, obj.transform))
                    continue;

                // 譛繧りｿ代＞繧ｪ繝悶ず繧ｧ繧ｯ繝医ｒ驕ｸ謚・                float distance = Vector3.Distance(mono.transform.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = obj.gameObject;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// 隕也ｷ壼愛螳・        /// </summary>
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
        /// 蜷代″蛻､螳・        /// </summary>
        private bool IsFacing(Transform from, Transform to)
        {
            Vector3 directionToTarget = (to.position - from.position).normalized;
            float angle = Vector3.Angle(from.forward, directionToTarget);
            return angle <= definition.facingAngle * 0.5f;
        }

        /// <summary>
        /// 迸ｬ髢薙う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteInstantInteraction()
        {
            if (targetObject != null)
            {
                // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｳ繝ｳ繝昴・繝阪Φ繝医・蜻ｼ縺ｳ蜃ｺ縺・                var interactable = targetObject.GetComponent<IInteractable>();
                interactable?.OnInteract(context);

                PlayInteractionAnimation();
                ShowInteractionEffect();
            }
        }

        /// <summary>
        /// 髟ｷ謚ｼ縺励う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ髢句ｧ・        /// </summary>
        private void StartHoldInteraction()
        {
            isInteracting = true;
            interactionProgress = 0f;

            // 邯咏ｶ夂噪縺ｪ譖ｴ譁ｰ蜃ｦ逅・・髢句ｧ具ｼ亥ｮ滄圀縺ｮ螳溯｣・〒縺ｯ Coroutine 縺ｾ縺溘・UpdateLoop・・#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started hold interaction: {definition.holdDuration}s required");
#endif
        }

        /// <summary>
        /// 隍・焚蝗槭う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteMultiInteraction()
        {
            currentInteractionCount++;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Multi interaction: {currentInteractionCount}/{definition.requiredInteractions}");
#endif

            if (currentInteractionCount >= definition.requiredInteractions)
            {
                // 蠢・ｦ∝屓謨ｰ縺ｫ驕斐＠縺溷ｴ蜷医・蜃ｦ逅・                CompleteMultiInteraction();
            }
            else
            {
                // 縺ｾ縺蠢・ｦ∝屓謨ｰ縺ｫ驕斐＠縺ｦ縺・↑縺・ｴ蜷医・繝輔ぅ繝ｼ繝峨ヰ繝・け
                ShowProgressFeedback();
            }
        }

        /// <summary>
        /// 譁・ц萓晏ｭ倥う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteContextualInteraction()
        {
            // 迴ｾ蝨ｨ縺ｮ迥ｶ豕√↓蠢懊§縺ｦ逡ｰ縺ｪ繧句・逅・ｒ螳溯｡・            // 萓具ｼ壽凾髢灘ｸｯ縲√い繧､繝・Β謇謖∫憾豕√√け繧ｨ繧ｹ繝磯ｲ陦檎憾豕∫ｭ・            
            var interactable = targetObject?.GetComponent<IContextualInteractable>();
            interactable?.OnContextualInteract(context, GetCurrentContext());
        }

        /// <summary>
        /// 霑第磁閾ｪ蜍輔う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳溯｡・        /// </summary>
        private void ExecuteProximityInteraction()
        {
            // 繝励Ξ繧､繝､繝ｼ縺檎ｯ・峇蜀・↓縺・ｋ髢薙∬・蜍慕噪縺ｫ邯咏ｶ壹＆繧後ｋ繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ
            var interactable = targetObject?.GetComponent<IProximityInteractable>();
            interactable?.OnProximityInteract(context);
        }

        /// <summary>
        /// 隍・焚蝗槭う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳御ｺ・・逅・        /// </summary>
        private void CompleteMultiInteraction()
        {
            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);
            
            currentInteractionCount = 0;
            ShowInteractionEffect();
        }

        /// <summary>
        /// 髟ｷ謚ｼ縺励う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ譖ｴ譁ｰ・亥､夜Κ縺九ｉ螳壽悄逧・↓蜻ｼ縺ｳ蜃ｺ縺輔ｌ繧具ｼ・        /// </summary>
        public void UpdateHoldInteraction(float deltaTime)
        {
            if (!isInteracting || definition.interactionType != InteractCommandDefinition.InteractionType.Hold)
                return;

            interactionProgress += deltaTime;

            // 繝励Ο繧ｰ繝ｬ繧ｹ繝舌・縺ｮ譖ｴ譁ｰ
            if (definition.showProgressBar)
            {
                float progress = interactionProgress / definition.holdDuration;
                // UI譖ｴ譁ｰ蜃ｦ逅・            }

            // 螳御ｺ・メ繧ｧ繝・け
            if (interactionProgress >= definition.holdDuration)
            {
                CompleteHoldInteraction();
            }
        }

        /// <summary>
        /// 髟ｷ謚ｼ縺励う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ螳御ｺ・        /// </summary>
        private void CompleteHoldInteraction()
        {
            isInteracting = false;
            interactionProgress = 0f;

            var interactable = targetObject?.GetComponent<IInteractable>();
            interactable?.OnInteract(context);

            ShowInteractionEffect();
        }

        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ縺ｮ蜀咲函
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
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧ｨ繝輔ぉ繧ｯ繝医・陦ｨ遉ｺ
        /// </summary>
        private void ShowInteractionEffect()
        {
            // 繝代・繝・ぅ繧ｯ繝ｫ繧ｨ繝輔ぉ繧ｯ繝・            // 繧ｵ繧ｦ繝ｳ繝峨お繝輔ぉ繧ｯ繝・            // UI繝輔ぅ繝ｼ繝峨ヰ繝・け

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Showing interaction effect");
#endif
        }

        /// <summary>
        /// 騾ｲ陦檎憾豕√ヵ繧｣繝ｼ繝峨ヰ繝・け縺ｮ陦ｨ遉ｺ
        /// </summary>
        private void ShowProgressFeedback()
        {
            // 騾ｲ陦檎憾豕√・UI陦ｨ遉ｺ
            // 繧ｵ繧ｦ繝ｳ繝峨ヵ繧｣繝ｼ繝峨ヰ繝・け
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｳ繝ｳ繝・く繧ｹ繝域ュ蝣ｱ繧貞叙蠕・        /// </summary>
        private object GetCurrentContext()
        {
            // 譎る俣蟶ｯ縲∵園謖√い繧､繝・Β縲√け繧ｨ繧ｹ繝育憾豕∫ｭ峨ｒ蜷ｫ繧繧ｳ繝ｳ繝・く繧ｹ繝域ュ蝣ｱ繧定ｿ斐☆
            return new { TimeOfDay = "Day", HasKey = false };
        }

        /// <summary>
        /// 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ繧ｭ繝｣繝ｳ繧ｻ繝ｫ
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
        /// Undo謫堺ｽ懶ｼ医う繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ蜿悶ｊ豸医＠・・        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // 繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ縺ｮ騾・桃菴懶ｼ亥庄閭ｽ縺ｪ蝣ｴ蜷茨ｼ・            var interactable = targetObject?.GetComponent<IUndoableInteractable>();
            interactable?.OnUndoInteract(context);

            // 騾ｲ陦御ｸｭ縺ｮ繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ繧偵く繝｣繝ｳ繧ｻ繝ｫ
            CancelInteraction();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log("Interaction undone");
#endif

            executed = false;
        }

        /// <summary>
        /// 縺薙・繧ｳ繝槭Φ繝峨′Undo蜿ｯ閭ｽ縺九←縺・°
        /// </summary>
        public bool CanUndo => executed && targetObject?.GetComponent<IUndoableInteractable>() != null;

        /// <summary>
        /// 迴ｾ蝨ｨ繧､繝ｳ繧ｿ繝ｩ繧ｯ繧ｷ繝ｧ繝ｳ荳ｭ縺九←縺・°
        /// </summary>
        public bool IsInteracting => isInteracting;
    }

    /// <summary>
    /// 蝓ｺ譛ｬ逧・↑繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(object interactor);
    }

    /// <summary>
    /// 譁・ц萓晏ｭ倥う繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IContextualInteractable
    {
        void OnContextualInteract(object interactor, object context);
    }

    /// <summary>
    /// 霑第磁閾ｪ蜍輔う繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IProximityInteractable
    {
        void OnProximityInteract(object interactor);
    }

    /// <summary>
    /// Undo蜿ｯ閭ｽ繧､繝ｳ繧ｿ繝ｩ繧ｯ繝亥庄閭ｽ繧ｪ繝悶ず繧ｧ繧ｯ繝医・繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ
    /// </summary>
    public interface IUndoableInteractable
    {
        void OnUndoInteract(object interactor);
    }
}
