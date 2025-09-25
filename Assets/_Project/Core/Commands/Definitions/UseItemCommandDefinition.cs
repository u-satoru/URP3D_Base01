using UnityEngine;
using asterivo.Unity60.Core.Commands;

namespace asterivo.Unity60.Core.Commands.Definitions
{
    /// <summary>
    /// アイテム使用コマンドの定義。
    /// プレイヤーのインベントリアイテム使用アクションをカプセル化します。
    /// 
    /// 主な機能：
    /// - 各種アイテムタイプ（消耗品、武器、防具等）の使用
    /// - アイテム使用条件と制約の管理
    /// - 使用効果の適用と持続時間管理
    /// - アニメーションとエフェクトの制御
    /// </summary>
    [System.Serializable]
    public class UseItemCommandDefinition : ICommandDefinition
    {
        /// <summary>
        /// アイテム使用の種類を定義する列挙型
        /// </summary>
        public enum UseType
        {
            Instant,        // 瞬間使用（消耗品等）
            Equip,          // 装備（武器、防具等）
            Activate,       // 起動（道具、スキルアイテム等）
            Consume,        // 消費使用
            Toggle          // オン/オフ切り替え
        }

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
        public float usageDelay = 0f; // 効果適用までの遅延

        [Header("Cooldown")]
        public float cooldownDuration = 0f;
        public bool globalCooldown = false; // 全アイテムの使用を制限するか

        [Header("Effects")]
        public bool showUseEffect = true;
        public bool showTargetingUI = false;
        public string useEffectName = "";

        /// <summary>
        /// デフォルトコンストラクタ
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
        /// アイテム使用コマンドが実行可能かどうかを判定します
        /// </summary>
        public bool CanExecute(object context = null)
        {
            // 基本的な実行可能性チェック
            if (string.IsNullOrEmpty(targetItemId) && itemSlotIndex < 0) return false;
            
            if (requiresTargeting && maxTargetDistance <= 0f) return false;
            if (animationDuration < 0f || usageDelay < 0f) return false;

            // コンテキストがある場合の追加チェック
            if (context != null)
            {
                // インベントリ内のアイテム存在チェック
                // アイテム使用可能状態チェック
                // クールダウンチェック
                // 使用条件チェック（戦闘中、移動中等）
                // プレイヤーの状態チェック（スタン、沈黙等）
            }

            return true;
        }

        /// <summary>
        /// アイテム使用コマンドを作成します
        /// </summary>
        public ICommand CreateCommand(object context = null)
        {
            if (!CanExecute(context))
                return null;

            return new UseItemCommand(this, context);
        }
    }

    /// <summary>
    /// UseItemCommandDefinitionに対応する実際のコマンド実装
    /// </summary>
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
        /// アイテム使用コマンドの実行
        /// </summary>
        public void Execute()
        {
            if (executed) return;

            // 使用対象アイテムの取得
            targetItem = GetTargetItem();
            if (targetItem == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.LogWarning($"Target item not found: {definition.targetItemId}");
#endif
                return;
            }

            // ターゲティングが必要な場合の処理
            if (definition.requiresTargeting)
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

            // 遅延がある場合は遅延実行、そうでなければ即座に実行
            if (definition.usageDelay > 0f)
            {
                // 実際の実装では Coroutine または Timer で遅延実行
                // 現在は即座に実行
                ApplyItemEffect();
            }
            else
            {
                ApplyItemEffect();
            }

            executed = true;
        }

        /// <summary>
        /// 使用対象のアイテムを取得
        /// </summary>
        private IUsableItem GetTargetItem()
        {
            // 実際の実装では InventorySystem との連携
            
            // アイテムIDでの検索
            if (!string.IsNullOrEmpty(definition.targetItemId))
            {
                // return inventorySystem.GetItemById(definition.targetItemId);
            }
            
            // スロットインデックスでの検索
            if (definition.itemSlotIndex >= 0)
            {
                // return inventorySystem.GetItemAtSlot(definition.itemSlotIndex);
            }

            // 仮の実装
            return new MockUsableItem(definition.targetItemId);
        }

        /// <summary>
        /// ターゲットオブジェクトの検索
        /// </summary>
        private GameObject FindTarget()
        {
            if (context is not MonoBehaviour mono) return null;

            // カメラまたはプレイヤーの前方向にRaycast
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
        /// アイテム効果の適用
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

            // アイテムの消費処理
            if (definition.consumeOnUse)
            {
                ConsumeItem();
            }

            // エフェクトの表示
            if (definition.showUseEffect)
            {
                ShowUseEffect();
            }

            // クールダウンの開始
            if (definition.cooldownDuration > 0f)
            {
                StartCooldown();
            }
        }

        /// <summary>
        /// 瞬間効果の適用
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
        /// 装備効果の適用
        /// </summary>
        private void ApplyEquipEffect()
        {
            if (context is MonoBehaviour mono)
            {
                // 実際の実装では EquipmentSystem との連携
                // equipmentSystem.EquipItem(targetItem);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Equipped {targetItem.GetItemName()}");
#endif
            }
        }

        /// <summary>
        /// 起動効果の適用
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
        /// 消費効果の適用
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
        /// トグル効果の適用
        /// </summary>
        private void ApplyToggleEffect()
        {
            // アイテムの現在の状態を取得して切り替え
            bool currentState = targetItem.GetToggleState();
            targetItem.SetToggleState(!currentState);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Toggled {targetItem.GetItemName()} to {!currentState}");
#endif
        }

        /// <summary>
        /// アイテムの消費処理
        /// </summary>
        private void ConsumeItem()
        {
            // 実際の実装では InventorySystem との連携
            // inventorySystem.ConsumeItem(targetItem);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item consumed: {targetItem.GetItemName()}");
#endif
        }

        /// <summary>
        /// 使用エフェクトの表示
        /// </summary>
        private void ShowUseEffect()
        {
            if (context is not MonoBehaviour mono) return;

            // パーティクルエフェクト
            if (!string.IsNullOrEmpty(definition.useEffectName))
            {
                // effectSystem.PlayEffect(definition.useEffectName, mono.transform.position);
            }

            // サウンドエフェクト
            // audioSystem.PlaySound(targetItem.GetUseSound());

            // UI エフェクト
            // uiSystem.ShowItemUseNotification(targetItem);
        }

        /// <summary>
        /// クールダウンの開始
        /// </summary>
        private void StartCooldown()
        {
            // 実際の実装では CooldownSystem との連携
            // cooldownSystem.StartCooldown(targetItem.GetItemId(), definition.cooldownDuration);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Started cooldown: {definition.cooldownDuration}s");
#endif
        }

        /// <summary>
        /// 継続効果の更新（外部から定期的に呼び出される）
        /// </summary>
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
                // 継続効果の更新処理
                targetItem?.UpdateEffect(context, deltaTime);
            }
        }

        /// <summary>
        /// アイテム効果の終了
        /// </summary>
        private void EndItemEffect()
        {
            isEffectActive = false;
            targetItem?.OnEffectEnd(context);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Item effect ended: {targetItem?.GetItemName()}");
#endif
        }

        /// <summary>
        /// Undo操作（アイテム使用の取り消し）
        /// </summary>
        public void Undo()
        {
            if (!executed) return;

            // アイテム効果の取り消し
            if (isEffectActive)
            {
                EndItemEffect();
            }

            // 使用したアイテムの復元（消費していた場合）
            if (definition.consumeOnUse && targetItem != null)
            {
                // 実際の実装では InventorySystem との連携
                // inventorySystem.RestoreItem(targetItem);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Restored consumed item: {targetItem.GetItemName()}");
#endif
            }

            // 装備アイテムの取り外し
            if (definition.useType == UseItemCommandDefinition.UseType.Equip)
            {
                // equipmentSystem.UnequipItem(targetItem);
            }

            executed = false;
        }

        /// <summary>
        /// このコマンドがUndo可能かどうか
        /// </summary>
        public bool CanUndo => executed && (definition.consumeOnUse || definition.useType == UseItemCommandDefinition.UseType.Equip);

        /// <summary>
        /// アイテム効果が現在アクティブかどうか
        /// </summary>
        public bool IsEffectActive => isEffectActive;
    }

    /// <summary>
    /// 使用可能アイテムのインターフェース
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
    /// テスト用のモックアイテム実装
    /// </summary>
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