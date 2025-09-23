using UnityEngine;
// using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// アイチE��使用コマンド�E定義、E    /// プレイヤーのインベントリアイチE��使用アクションをカプセル化します、E    /// 
    /// 主な機�E�E�E    /// - 吁E��アイチE��タイプ（消耗品、武器、E��具等）�E使用
    /// - アイチE��使用条件と制紁E�E管琁E    /// - 使用効果�E適用と持続時間管琁E    /// - アニメーションとエフェクト�E制御
    /// </summary>
    [System.Serializable]
    public class UseItemCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// アイチE��使用の種類を定義する列挙垁E        /// </summary>
        public enum UseType
        {
            Instant,        // 瞬間使用�E�消耗品等！E            Equip,          // 裁E���E�武器、E��具等！E            Activate,       // 起動（道具、スキルアイチE��等！E            Consume,        // 消費使用
            Toggle          // オン/オフ�Eり替ぁE        }

        [Header("Item Usage Parameters")]
        public UseType useType = UseType.Instant;
        public string targetItemId = "";
        public int itemSlotIndex = -1; // -1 = auto-detect
        public bool consumeOnUse = true;

        [Header("Usage Conditions")]
        public bool requiresTargeting = false;
        public float maxTargetDistance = 5f;
        public LayerMask validTargetLayers = -1;
        public bool canUseInCombat = true;
        public bool canUseWhileMoving = true;

        [Header("Effect Application")]
        public bool applyToSelf = true;
        public bool canApplyToOthers = false;
        public float effectDuration = 0f; // 0 = permanent/instant
        public bool stackable = false;

        [Header("Animation & Timing")]
        public bool playUseAnimation = true;
        public string useAnimationTrigger = "UseItem";
        public float animationDuration = 1f;
        public float usageDelay = 0f; // 効果適用までの遁E��

        [Header("Cooldown")]
        public float cooldownDuration = 0f;
        public bool globalCooldown = false; // 全アイチE��の使用を制限するか

        [Header("Effects")]
        public bool showUseEffect = true;
        public bool showTargetingUI = false;
        public string useEffectName = "";

        /// <summary>
        /// チE��ォルトコンストラクタ
        /// </summary>
        public UseItemCommandDefinition()
        {
        }

        /// <summary>
        /// パラメータ付きコンストラクタ
        /// </summary>
        public UseItemCommandDefinition(UseType type, string itemId, bool consume = true)
        {
            useType = type;
            targetItemId = itemId;
            consumeOnUse = consume;
        }

        /// <summary>
        /// アイチE��使用コマンドが実行可能かどぁE��を判定しまぁE        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本皁E��実行可能性チェチE��
            if (string.IsNullOrEmpty(targetItemId) && itemSlotIndex < 0) return false;
            
            if (requiresTargeting && maxTargetDistance <= 0f) return false;
            if (animationDuration < 0f || usageDelay < 0f) return false;

            // コンチE��ストがある場合�E追加チェチE��
            if (context != null)
            {
                // インベントリ冁E�EアイチE��存在チェチE��
                // アイチE��使用可能状態チェチE��
                // クールダウンチェチE��
                // 使用条件チェチE���E�戦闘中、移動中等！E                // プレイヤーの状態チェチE���E�スタン、沈黙等！E            }

            return true;
        }

        /// <summary>
        /// アイチE��使用コマンドを作�EしまぁE        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new UseItemCommand(this, context);
        }
    }

    /// <summary>
    /// UseItemCommandDefinitionに対応する実際のコマンド実裁E    /// </summary>
    public class UseItemCommand : ICommand
    {
        private UseItemCommandDefinition definition;
        private object context;
        private bool executed = false;
        private IUsableItem targetItem;
        private GameObject targetObject;
        private bool isEffectActive = false;
        private float effectStartTime = 0f;

        public UseItemCommand(UseItemCommandDefinition useItemDefinition, object executionContext)
        {
            definition = useItemDefinition;
            context = executionContext;
        }

        /// <summary>
        /// アイチE��使用コマンド�E実衁E        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 使用対象アイチE��の取征E            targetItem = GetTargetItem();
            if (targetItem == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Target item not found: {definition.targetItemId}");
#endif
                return;
            }

            // ターゲチE��ングが忁E��な場合�E処琁E            if (definition.requiresTargeting)
            {
                targetObject = FindTarget();
                if (targetObject == null && definition.requiresTargeting)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    UnityEngine.Debug.LogWarning("No valid target found for item use");
#endif
                    return;
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Executing {definition.useType} item use: {targetItem.GetItemName()}");
#endif

            // 使用アニメーションの再生
            if (definition.playUseAnimation)
            {
                PlayUseAnimation();
            }

            // 遁E��がある場合�E遁E��実行、そぁE��なければ即座に実衁E            if (definition.usageDelay > 0f)
            {
                // 実際の実裁E��は Coroutine また�E Timer で遁E��実衁E                // 現在は即座に実衁E                ApplyItemEffect();
            }
            else
            {
                ApplyItemEffect();
            }

            executed = true;
        }

        /// <summary>
        /// 使用対象のアイチE��を取征E        /// </summary>
        private IUsableItem GetTargetItem()
        {
            // 実際の実裁E��は InventorySystem との連携
            
            // アイチE��IDでの検索
            if (!string.IsNullOrEmpty(definition.targetItemId))
            {
                // return inventorySystem.GetItemById(definition.targetItemId);
            }
            
            // スロチE��インチE��クスでの検索
            if (definition.itemSlotIndex >= 0)
            {
                // return inventorySystem.GetItemAtSlot(definition.itemSlotIndex);
            }

            // 仮の実裁E            return new MockUsableItem(definition.targetItemId);
        }

        /// <summary>
        /// ターゲチE��オブジェクト�E検索
        /// </summary>
        private GameObject FindTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // カメラまた�Eプレイヤーの前方向にRaycast
            Ray ray = new Ray(mono.transform.position, mono.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, definition.maxTargetDistance, definition.validTargetLayers))
            {
                return hit.collider.gameObject;
            }

            return null;
        }

        /// <summary>
        /// 使用アニメーションの再生
        /// </summary>
        private void PlayUseAnimation()
        {
            if (context is not MonoBehaviour mono) return;

            var animator = mono.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(definition.useAnimationTrigger))
            {
                animator.SetTrigger(definition.useAnimationTrigger);
            }
        }

        /// <summary>
        /// アイチE��効果�E適用
        /// </summary>
        private void ApplyItemEffect()
        {
            if (targetItem == null) return;

            switch (definition.useType)
            {
                case UseItemCommandDefinition.UseType.Instant:
                    ApplyInstantEffect();
                    break;
                case UseItemCommandDefinition.UseType.Equip:
                    ApplyEquipEffect();
                    break;
                case UseItemCommandDefinition.UseType.Activate:
                    ApplyActivateEffect();
                    break;
                case UseItemCommandDefinition.UseType.Consume:
                    ApplyConsumeEffect();
                    break;
                case UseItemCommandDefinition.UseType.Toggle:
                    ApplyToggleEffect();
                    break;
            }

            // アイチE��の消費処琁E            if (definition.consumeOnUse)
            {
                ConsumeItem();
            }

            // エフェクト�E表示
            if (definition.showUseEffect)
            {
                ShowUseEffect();
            }

            // クールダウンの開姁E            if (definition.cooldownDuration > 0f)
            {
                StartCooldown();
            }
        }

        /// <summary>
        /// 瞬間効果�E適用
        /// </summary>
        private void ApplyInstantEffect()
        {
            var target = definition.applyToSelf ? context : targetObject;
            targetItem.ApplyInstantEffect(target);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Applied instant effect from {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// 裁E��効果�E適用
        /// </summary>
        private void ApplyEquipEffect()
        {
            if (context is MonoBehaviour mono)
            {
                // 実際の実裁E��は EquipmentSystem との連携
                // equipmentSystem.EquipItem(targetItem);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Equipped {targetItem.GetItemName()}");
#endif
            }
        }

        /// <summary>
        /// 起動効果�E適用
        /// </summary>
        private void ApplyActivateEffect()
        {
            isEffectActive = true;
            effectStartTime = Time.time;
            
            targetItem.OnActivate(context, targetObject);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Activated {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// 消費効果�E適用
        /// </summary>
        private void ApplyConsumeEffect()
        {
            var target = definition.applyToSelf ? context : targetObject;
            targetItem.OnConsume(target);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Consumed {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// トグル効果�E適用
        /// </summary>
        private void ApplyToggleEffect()
        {
            // アイチE��の現在の状態を取得して刁E��替ぁE            bool currentState = targetItem.GetToggleState();
            targetItem.SetToggleState(!currentState);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Toggled {targetItem.GetItemName()} to {!currentState}");
#endif
        }

        /// <summary>
        /// アイチE��の消費処琁E        /// </summary>
        private void ConsumeItem()
        {
            // 実際の実裁E��は InventorySystem との連携
            // inventorySystem.ConsumeItem(targetItem);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item consumed: {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// 使用エフェクト�E表示
        /// </summary>
        private void ShowUseEffect()
        {
            if (context is not MonoBehaviour mono) return;

            // パ�EチE��クルエフェクチE            if (!string.IsNullOrEmpty(definition.useEffectName))
            {
                // effectSystem.PlayEffect(definition.useEffectName, mono.transform.position);
            }

            // サウンドエフェクチE            // audioSystem.PlaySound(targetItem.GetUseSound());

            // UI エフェクチE            // uiSystem.ShowItemUseNotification(targetItem);
        }

        /// <summary>
        /// クールダウンの開姁E        /// </summary>
        private void StartCooldown()
        {
            // 実際の実裁E��は CooldownSystem との連携
            // cooldownSystem.StartCooldown(targetItem.GetItemId(), definition.cooldownDuration);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started cooldown: {definition.cooldownDuration}s");
#endif
        }

        /// <summary>
        /// 継続効果�E更新�E�外部から定期皁E��呼び出される！E        /// </summary>
        public void UpdateItemEffect(float deltaTime)
        {
            if (!isEffectActive || definition.effectDuration <= 0f) return;

            float elapsedTime = Time.time - effectStartTime;
            
            if (elapsedTime >= definition.effectDuration)
            {
                EndItemEffect();
            }
            else
            {
                // 継続効果�E更新処琁E                targetItem?.UpdateEffect(context, deltaTime);
            }
        }

        /// <summary>
        /// アイチE��効果�E終亁E        /// </summary>
        private void EndItemEffect()
        {
            isEffectActive = false;
            targetItem?.OnEffectEnd(context);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item effect ended: {targetItem?.GetItemName()}");
#endif
        }

        /// <summary>
        /// Undo操作（アイチE��使用の取り消し�E�E        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // アイチE��効果�E取り消し
            if (isEffectActive)
            {
                EndItemEffect();
            }

            // 使用したアイチE��の復允E��消費してぁE��場合！E            if (definition.consumeOnUse && targetItem != null)
            {
                // 実際の実裁E��は InventorySystem との連携
                // inventorySystem.RestoreItem(targetItem);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Restored consumed item: {targetItem.GetItemName()}");
#endif
            }

            // 裁E��アイチE��の取り外し
            if (definition.useType == UseItemCommandDefinition.UseType.Equip)
            {
                // equipmentSystem.UnequipItem(targetItem);
            }

            executed = false;
        }

        /// <summary>
        /// こ�EコマンドがUndo可能かどぁE��
        /// </summary>
        public bool CanUndo => executed && (definition.consumeOnUse || definition.useType == UseItemCommandDefinition.UseType.Equip);

        /// <summary>
        /// アイチE��効果が現在アクチE��ブかどぁE��
        /// </summary>
        public bool IsEffectActive => isEffectActive;
    }

    /// <summary>
    /// 使用可能アイチE��のインターフェース
    /// </summary>
    public interface IUsableItem
    {
        string GetItemId();
        string GetItemName();
        void ApplyInstantEffect(object target);
        void OnActivate(object user, object target = null);
        void OnConsume(object user);
        void UpdateEffect(object user, float deltaTime);
        void OnEffectEnd(object user);
        bool GetToggleState();
        void SetToggleState(bool state);
    }

    /// <summary>
    /// チE��ト用のモチE��アイチE��実裁E    /// </summary>
    internal class MockUsableItem : IUsableItem
    {
        private string itemId;
        private bool toggleState = false;

        public MockUsableItem(string id)
        {
            itemId = id;
        }

        public string GetItemId() => itemId;
        public string GetItemName() => $"Mock Item ({itemId})";
        public void ApplyInstantEffect(object target) { }
        public void OnActivate(object user, object target = null) { }
        public void OnConsume(object user) { }
        public void UpdateEffect(object user, float deltaTime) { }
        public void OnEffectEnd(object user) { }
        public bool GetToggleState() => toggleState;
        public void SetToggleState(bool state) => toggleState = state;
    }
}