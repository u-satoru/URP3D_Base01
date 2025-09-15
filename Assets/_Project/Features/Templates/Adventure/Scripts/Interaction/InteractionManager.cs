using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Features.Templates.Adventure.Quest;
using asterivo.Unity60.Features.Templates.Adventure.Dialogue;
using asterivo.Unity60.Features.Templates.Adventure.Inventory;
using asterivo.Unity60.Features.Templates.Adventure.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Interaction
{
    /// <summary>
    /// Adventure Template用インタラクション管理システム
    /// 環境オブジェクトとの相互作用、トリガー処理、プレイヤーインタラクションを管理
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [TabGroup("Config", "Configuration")]
        [Header("Interaction Settings")]
        [SerializeField]
        [Tooltip("プレイヤーとの最大インタラクション距離")]
        private float maxInteractionDistance = 3.0f;

        [TabGroup("Config", "Configuration")]
        [SerializeField]
        [Tooltip("インタラクション可能オブジェクトのレイヤーマスク")]
        private LayerMask interactionLayerMask = -1;

        [TabGroup("Config", "Configuration")]
        [SerializeField]
        [Tooltip("インタラクションのクールダウン時間")]
        private float interactionCooldown = 0.5f;

        [TabGroup("Config", "Configuration")]
        [SerializeField]
        [Tooltip("デバッグモードの有効/無効")]
        private bool debugMode = false;

        [TabGroup("Events", "Game Events")]
        [Header("Interaction Events")]
        [SerializeField]
        [Tooltip("インタラクション開始時に発行されるイベント")]
        private GameObjectGameEvent onInteractionStarted;

        [TabGroup("Events", "Game Events")]
        [SerializeField]
        [Tooltip("インタラクション完了時に発行されるイベント")]
        private GameObjectGameEvent onInteractionCompleted;

        [TabGroup("Events", "Game Events")]
        [SerializeField]
        [Tooltip("インタラクション失敗時に発行されるイベント")]
        private GameObjectGameEvent onInteractionFailed;

        [TabGroup("Events", "Game Events")]
        [SerializeField]
        [Tooltip("インタラクション可能オブジェクト検出時のイベント")]
        private GameObjectGameEvent onInteractableDetected;

        [TabGroup("Events", "Game Events")]
        [SerializeField]
        [Tooltip("インタラクション可能オブジェクト消失時のイベント")]
        private GameObjectGameEvent onInteractableLost;

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("現在インタラクション可能なオブジェクト一覧")]
        private List<IInteractable> nearbyInteractables = new List<IInteractable>();

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("現在選択中のインタラクション対象")]
        private IInteractable currentTarget;

        [TabGroup("Status", "Current Status")]
        [ShowInInspector, ReadOnly]
        [Tooltip("最後にインタラクションした時間")]
        private float lastInteractionTime;

        [TabGroup("Stats", "Statistics")]
        [ShowInInspector, ReadOnly]
        public int NearbyInteractableCount => nearbyInteractables?.Count ?? 0;

        [TabGroup("Stats", "Statistics")]
        [ShowInInspector, ReadOnly]
        public bool HasCurrentTarget => currentTarget != null;

        [TabGroup("Stats", "Statistics")]
        [ShowInInspector, ReadOnly]
        public bool CanInteract => Time.time - lastInteractionTime >= interactionCooldown;

        // 依存システム参照
        private Transform playerTransform;
        private Camera playerCamera;
        private QuestManager questManager;
        private DialogueManager dialogueManager;
        private AdventureInventoryManager inventoryManager;

        // プロパティ
        public IInteractable CurrentTarget => currentTarget;
        public IReadOnlyList<IInteractable> NearbyInteractables => nearbyInteractables.AsReadOnly();

        // イベント
        public event System.Action<IInteractable> OnInteractionStarted;
        public event System.Action<IInteractable> OnInteractionCompleted;
        public event System.Action<IInteractable> OnInteractionFailed;
        public event System.Action<IInteractable> OnInteractableDetected;
        public event System.Action<IInteractable> OnInteractableLost;

        private void Start()
        {
            InitializeInteractionManager();
        }

        private void Update()
        {
            if (Time.time - lastInteractionTime >= interactionCooldown)
            {
                UpdateNearbyInteractables();
                UpdateCurrentTarget();
            }
        }

        private void OnDestroy()
        {
            CleanupInteractionManager();
        }

        /// <summary>
        /// インタラクションマネージャーの初期化
        /// </summary>
        private void InitializeInteractionManager()
        {
            // プレイヤー参照の取得
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerCamera = player.GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
            }

            // 依存システムの取得
            questManager = asterivo.Unity60.Core.ServiceLocator.GetService<QuestManager>();
            dialogueManager = asterivo.Unity60.Core.ServiceLocator.GetService<DialogueManager>();
            inventoryManager = asterivo.Unity60.Core.ServiceLocator.GetService<AdventureInventoryManager>();

            // ServiceLocatorへの登録
            asterivo.Unity60.Core.ServiceLocator.RegisterService<InteractionManager>(this);

            LogDebug("[InteractionManager] Initialized with max distance: " + maxInteractionDistance);
        }

        /// <summary>
        /// クリーンアップ処理
        /// </summary>
        private void CleanupInteractionManager()
        {
            asterivo.Unity60.Core.ServiceLocator.UnregisterService<InteractionManager>();
            nearbyInteractables.Clear();
            currentTarget = null;
        }

        #region Interaction Detection

        /// <summary>
        /// 近くのインタラクション可能オブジェクトを更新
        /// </summary>
        private void UpdateNearbyInteractables()
        {
            if (playerTransform == null) return;

            var previousInteractables = new HashSet<IInteractable>(nearbyInteractables);
            nearbyInteractables.Clear();

            // 範囲内のコライダーを検索
            var colliders = Physics.OverlapSphere(playerTransform.position, maxInteractionDistance, interactionLayerMask);

            foreach (var collider in colliders)
            {
                var interactable = collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    nearbyInteractables.Add(interactable);

                    // 新しく検出されたオブジェクト
                    if (!previousInteractables.Contains(interactable))
                    {
                        OnInteractableDetected?.Invoke(interactable);
                        onInteractableDetected?.Raise((interactable as Component)?.gameObject);
                        LogDebug($"[InteractionManager] Detected new interactable: {interactable.GetInteractionText()}");
                    }
                }
            }

            // 消失したオブジェクトの検出
            foreach (var previousInteractable in previousInteractables)
            {
                if (!nearbyInteractables.Contains(previousInteractable))
                {
                    OnInteractableLost?.Invoke(previousInteractable);
                    onInteractableLost?.Raise((previousInteractable as Component)?.gameObject);
                    LogDebug($"[InteractionManager] Lost interactable: {previousInteractable.GetInteractionText()}");
                }
            }
        }

        /// <summary>
        /// 現在のターゲットを更新
        /// </summary>
        private void UpdateCurrentTarget()
        {
            if (nearbyInteractables.Count == 0)
            {
                currentTarget = null;
                return;
            }

            // カメラの中心に最も近いオブジェクトを選択
            IInteractable bestTarget = null;
            float bestScore = float.MaxValue;

            foreach (var interactable in nearbyInteractables)
            {
                var interactableObject = interactable as MonoBehaviour;
                if (interactableObject == null) continue;

                // カメラからの角度と距離を考慮したスコア計算
                var score = CalculateInteractionScore(interactableObject.transform.position);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = interactable;
                }
            }

            if (currentTarget != bestTarget)
            {
                // ターゲット変更の処理
                if (currentTarget != null)
                {
                    currentTarget.OnTargetLost();
                }

                currentTarget = bestTarget;

                if (currentTarget != null)
                {
                    currentTarget.OnTargetSelected();
                    LogDebug($"[InteractionManager] Target changed to: {currentTarget.GetInteractionText()}");
                }
            }
        }

        /// <summary>
        /// インタラクションスコアの計算
        /// </summary>
        private float CalculateInteractionScore(Vector3 targetPosition)
        {
            if (playerCamera == null || playerTransform == null) return float.MaxValue;

            // 距離スコア
            float distance = Vector3.Distance(playerTransform.position, targetPosition);
            float distanceScore = distance / maxInteractionDistance;

            // 角度スコア（カメラの中心からの角度）
            var directionToTarget = (targetPosition - playerCamera.transform.position).normalized;
            var cameraForward = playerCamera.transform.forward;
            float angle = Vector3.Angle(cameraForward, directionToTarget);
            float angleScore = angle / 180f; // 0-1の範囲に正規化

            // 総合スコア（距離を重視、角度も考慮）
            return distanceScore * 0.7f + angleScore * 0.3f;
        }

        #endregion

        #region Interaction Execution

        /// <summary>
        /// 現在のターゲットとインタラクション実行
        /// </summary>
        public bool TryInteract()
        {
            if (currentTarget == null)
            {
                LogWarning("[InteractionManager] No current target to interact with");
                return false;
            }

            return TryInteractWith(currentTarget);
        }

        /// <summary>
        /// 指定したオブジェクトとのインタラクション実行
        /// </summary>
        public bool TryInteractWith(IInteractable target)
        {
            if (target == null)
            {
                LogError("[InteractionManager] Cannot interact with null target");
                return false;
            }

            if (!CanInteract)
            {
                LogDebug("[InteractionManager] Interaction on cooldown");
                return false;
            }

            if (!target.CanInteract())
            {
                LogWarning($"[InteractionManager] Target cannot be interacted with: {target.GetInteractionText()}");
                OnInteractionFailed?.Invoke(target);
                onInteractionFailed?.Raise((target as Component)?.gameObject);
                return false;
            }

            // 距離チェック
            var targetTransform = (target as MonoBehaviour)?.transform;
            if (targetTransform != null && playerTransform != null)
            {
                float distance = Vector3.Distance(playerTransform.position, targetTransform.position);
                if (distance > maxInteractionDistance)
                {
                    LogWarning("[InteractionManager] Target is too far away for interaction");
                    OnInteractionFailed?.Invoke(target);
                    onInteractionFailed?.Raise((target as Component)?.gameObject);
                    return false;
                }
            }

            // インタラクション実行
            try
            {
                OnInteractionStarted?.Invoke(target);
                onInteractionStarted?.Raise((target as Component)?.gameObject);

                bool success = target.Interact();
                lastInteractionTime = Time.time;

                if (success)
                {
                    OnInteractionCompleted?.Invoke(target);
                    onInteractionCompleted?.Raise((target as Component)?.gameObject);
                    LogDebug($"[InteractionManager] Successfully interacted with: {target.GetInteractionText()}");

                    // クエスト進行チェック
                    CheckQuestProgress(target);
                }
                else
                {
                    OnInteractionFailed?.Invoke(target);
                    onInteractionFailed?.Raise((target as Component)?.gameObject);
                    LogWarning($"[InteractionManager] Interaction failed with: {target.GetInteractionText()}");
                }

                return success;
            }
            catch (System.Exception ex)
            {
                LogError($"[InteractionManager] Exception during interaction: {ex.Message}");
                OnInteractionFailed?.Invoke(target);
                onInteractionFailed?.Raise((target as Component)?.gameObject);
                return false;
            }
        }

        /// <summary>
        /// クエスト進行をチェック
        /// </summary>
        private void CheckQuestProgress(IInteractable target)
        {
            if (questManager == null) return;

            // インタラクションタイプに基づいてクエスト進行を更新
            var interactableComponent = target as MonoBehaviour;
            if (interactableComponent != null)
            {
                var interactableId = interactableComponent.name; // または固有ID
                // TODO: Fix quest objective update - needs QuestData parameter, not QuestObjectiveType
                // questManager.UpdateObjectiveProgress(QuestObjectiveType.Interact, interactableId, 1);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 指定位置から最も近いインタラクション可能オブジェクトを取得
        /// </summary>
        public IInteractable GetNearestInteractable(Vector3 position)
        {
            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var interactable in nearbyInteractables)
            {
                var interactableTransform = (interactable as MonoBehaviour)?.transform;
                if (interactableTransform != null)
                {
                    float distance = Vector3.Distance(position, interactableTransform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = interactable;
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 指定タイプのインタラクション可能オブジェクトを取得
        /// </summary>
        public List<T> GetInteractablesOfType<T>() where T : class, IInteractable
        {
            var result = new List<T>();
            foreach (var interactable in nearbyInteractables)
            {
                if (interactable is T typedInteractable)
                {
                    result.Add(typedInteractable);
                }
            }
            return result;
        }

        /// <summary>
        /// インタラクション可能オブジェクトを強制的に再スキャン
        /// </summary>
        public void ForceRescan()
        {
            UpdateNearbyInteractables();
            UpdateCurrentTarget();
        }

        /// <summary>
        /// 特定のオブジェクトをターゲットに設定
        /// </summary>
        public bool SetTarget(IInteractable target)
        {
            if (target == null || !nearbyInteractables.Contains(target))
            {
                return false;
            }

            if (currentTarget != null)
            {
                currentTarget.OnTargetLost();
            }

            currentTarget = target;
            currentTarget.OnTargetSelected();

            return true;
        }

        #endregion

        #region Debug Support

        [TabGroup("Debug", "Debug Tools")]
        [Button("Force Rescan")]
        [ShowIf("debugMode")]
        private void DebugForceRescan()
        {
            ForceRescan();
            LogDebug("[InteractionManager] Force rescan completed");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Clear Current Target")]
        [ShowIf("debugMode")]
        private void DebugClearTarget()
        {
            if (currentTarget != null)
            {
                currentTarget.OnTargetLost();
                currentTarget = null;
                LogDebug("[InteractionManager] Current target cleared");
            }
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Log Nearby Interactables")]
        [ShowIf("debugMode")]
        private void DebugLogNearbyInteractables()
        {
            LogDebug($"[InteractionManager] Found {nearbyInteractables.Count} nearby interactables:");
            for (int i = 0; i < nearbyInteractables.Count; i++)
            {
                var interactable = nearbyInteractables[i];
                LogDebug($"  {i}: {interactable.GetInteractionText()} (Can Interact: {interactable.CanInteract()})");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!debugMode || playerTransform == null) return;

            // インタラクション範囲の表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, maxInteractionDistance);

            // 現在のターゲットの表示
            if (currentTarget != null)
            {
                var targetTransform = (currentTarget as MonoBehaviour)?.transform;
                if (targetTransform != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(targetTransform.position, Vector3.one * 0.5f);
                    Gizmos.DrawLine(playerTransform.position, targetTransform.position);
                }
            }

            // その他のインタラクション可能オブジェクトの表示
            Gizmos.color = Color.cyan;
            foreach (var interactable in nearbyInteractables)
            {
                if (interactable == currentTarget) continue;

                var interactableTransform = (interactable as MonoBehaviour)?.transform;
                if (interactableTransform != null)
                {
                    Gizmos.DrawWireCube(interactableTransform.position, Vector3.one * 0.3f);
                }
            }
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        #endregion

        #region Adventure Template Integration Events

        /// <summary>
        /// Event fired when NPC interaction occurs - used by AdventureTemplateManager
        /// </summary>
        public event Action<IInteractable> OnNPCInteraction;

        /// <summary>
        /// Event fired when object interaction occurs - used by AdventureTemplateManager
        /// </summary>
        public event Action<IInteractable> OnObjectInteraction;

        /// <summary>
        /// Triggers NPC interaction event
        /// </summary>
        public void TriggerNPCInteraction(IInteractable npc)
        {
            OnNPCInteraction?.Invoke(npc);
            Debug.Log($"[InteractionManager] NPC interaction triggered: {npc?.GetInteractionText()}");
        }

        /// <summary>
        /// Triggers object interaction event
        /// </summary>
        public void TriggerObjectInteraction(IInteractable obj)
        {
            OnObjectInteraction?.Invoke(obj);
            Debug.Log($"[InteractionManager] Object interaction triggered: {obj?.GetInteractionText()}");
        }

        #endregion
    }
}