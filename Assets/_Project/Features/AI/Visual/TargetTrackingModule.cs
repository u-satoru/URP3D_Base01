using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.AI.States;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// NPCの目標追跡システム
    /// 複数目標同時追跡（最大5目標）と優先度管理システムを実装
    /// </summary>
    [System.Serializable]
    public class TargetTrackingModule
    {
        #region Inspector Fields
        
        [BoxGroup("Tracking Settings")]
        [PropertyRange(1, 10)]
        [LabelText("Max Targets")]
        [SerializeField] private int maxTrackedTargets = 5;
        
        [BoxGroup("Tracking Settings")]
        [PropertyRange(0.5f, 10f)]
        [LabelText("Target Expiry Time")]
        [SuffixLabel("s")]
        [SerializeField] private float targetExpiryTime = 8f;
        
        [BoxGroup("Tracking Settings")]
        [PropertyRange(0.1f, 2f)]
        [LabelText("Update Frequency")]
        [SuffixLabel("Hz")]
        [SerializeField] private float updateFrequency = 2f;
        
        [BoxGroup("Priority System")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Priority Update Rate")]
        [SuffixLabel("/s")]
        [SerializeField] private float priorityUpdateRate = 0.5f;
        
        [BoxGroup("Priority System")]
        [PropertyRange(1f, 10f)]
        [LabelText("Distance Priority Weight")]
        [SerializeField] private float distancePriorityWeight = 2f;
        
        [BoxGroup("Priority System")]
        [PropertyRange(1f, 10f)]
        [LabelText("Detection Score Weight")]
        [SerializeField] private float detectionScoreWeight = 3f;
        
        [BoxGroup("Priority System")]
        [PropertyRange(1f, 10f)]
        [LabelText("Movement Priority Weight")]
        [SerializeField] private float movementPriorityWeight = 1.5f;
        
        [BoxGroup("Priority System")]
        [PropertyRange(1f, 10f)]
        [LabelText("Threat Level Weight")]
        [SerializeField] private float threatLevelWeight = 2.5f;
        
        #endregion
        
        #region Runtime Data
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Active Targets")]
        [ListDrawerSettings(ShowIndexLabels = true, ShowFoldout = true)]
        private List<DetectedTarget> trackedTargets = new List<DetectedTarget>();
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Primary Target")]
        private DetectedTarget primaryTarget;
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Target Count")]
        private int activeTargetCount = 0;
        
        private float lastUpdateTime = 0f;
        private float lastPriorityUpdateTime = 0f;
        private Transform sensorTransform;
        
        // 除外されるタグのリスト
        private readonly HashSet<string> excludedTags = new HashSet<string> { "Untagged", "UI", "EditorOnly" };
        
        #endregion
        
        #region Properties
        
        public DetectedTarget PrimaryTarget => primaryTarget;
        public int ActiveTargetCount => activeTargetCount;
        public bool HasTargets => activeTargetCount > 0;
        public bool CanTrackMoreTargets => activeTargetCount < maxTrackedTargets;
        public IReadOnlyList<DetectedTarget> TrackedTargets => trackedTargets.AsReadOnly();
        public int MaxTargets => maxTrackedTargets;
        
        #endregion
        
        #region Events
        
        public event Action<DetectedTarget> OnTargetAdded;
        public event Action<DetectedTarget> OnTargetRemoved;
        public event Action<DetectedTarget> OnTargetUpdated;
        public event Action<DetectedTarget, DetectedTarget> OnPrimaryTargetChanged;
        public event Action<int> OnTargetCountChanged;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Target Tracking Moduleの初期化
        /// </summary>
        public void Initialize(Transform sensorTransform)
        {
            this.sensorTransform = sensorTransform;
            trackedTargets.Clear();
            primaryTarget = null;
            activeTargetCount = 0;
            lastUpdateTime = Time.time;
            lastPriorityUpdateTime = Time.time;
            
            Debug.Log("[TargetTracking] Target Tracking Module initialized");
        }
        
        #endregion
        
        #region Update Methods
        
        /// <summary>
        /// 目標追跡システムの更新（毎フレーム呼び出し）
        /// </summary>
        public void UpdateTrackingSystem(float deltaTime)
        {
            if (sensorTransform == null) return;
            
            // 定期的な更新チェック
            if (Time.time - lastUpdateTime >= (1f / updateFrequency))
            {
                UpdateTargetStates();
                CleanupExpiredTargets();
                lastUpdateTime = Time.time;
            }
            
            // 優先度の更新
            if (Time.time - lastPriorityUpdateTime >= (1f / priorityUpdateRate))
            {
                UpdateTargetPriorities();
                UpdatePrimaryTarget();
                lastPriorityUpdateTime = Time.time;
            }
        }
        
        private void UpdateTargetStates()
        {
            for (int i = 0; i < trackedTargets.Count; i++)
            {
                var target = trackedTargets[i];
                if (target.IsValid)
                {
                    // 目標の状態を更新（距離、速度など）
                    UpdateTargetDynamicData(target);
                    OnTargetUpdated?.Invoke(target);
                }
            }
        }
        
        private void UpdateTargetDynamicData(DetectedTarget target)
        {
            if (target.transform != null && sensorTransform != null)
            {
                // 現在の距離を計算
                float currentDistance = Vector3.Distance(sensorTransform.position, target.transform.position);
                
                // 速度と移動状態の更新
                Vector3 currentPosition = target.transform.position;
                float deltaTime = Time.time - target.lastSeenTime;
                
                if (deltaTime > 0f)
                {
                    Vector3 displacement = currentPosition - target.lastKnownPosition;
                    target.estimatedVelocity = displacement / deltaTime;
                    target.movementSpeed = target.estimatedVelocity.magnitude;
                    target.isMoving = target.movementSpeed > 0.1f;
                }
                
                // 位置の更新
                target.lastKnownPosition = currentPosition;
            }
        }
        
        private void CleanupExpiredTargets()
        {
            for (int i = trackedTargets.Count - 1; i >= 0; i--)
            {
                var target = trackedTargets[i];
                
                // 期限切れまたは無効な目標の削除
                if (!target.IsValid || target.TimeSinceLastSeen > targetExpiryTime)
                {
                    RemoveTarget(i);
                }
            }
        }
        
        private void UpdateTargetPriorities()
        {
            if (sensorTransform == null) return;
            
            foreach (var target in trackedTargets)
            {
                if (target.IsValid)
                {
                    float newPriority = CalculateDynamicPriority(target);
                    // 優先度の更新はDetectedTarget内部で行われる
                }
            }
        }
        
        private void UpdatePrimaryTarget()
        {
            DetectedTarget newPrimary = GetHighestPriorityTarget();
            
            if (newPrimary != primaryTarget)
            {
                DetectedTarget oldPrimary = primaryTarget;
                primaryTarget = newPrimary;
                OnPrimaryTargetChanged?.Invoke(oldPrimary, newPrimary);
                
                Debug.Log($"[TargetTracking] Primary target changed to: {(newPrimary?.transform?.name ?? "None")}");
            }
        }
        
        #endregion
        
        #region Target Management
        
        /// <summary>
        /// 新しい目標を追跡リストに追加
        /// </summary>
        public bool AddTarget(Transform targetTransform, float detectionScore)
        {
            if (targetTransform == null) return false;
            
            // 除外タグのチェック
            if (excludedTags.Contains(targetTransform.tag)) return false;
            
            // 既存の目標かチェック
            DetectedTarget existingTarget = FindTarget(targetTransform);
            if (existingTarget != null)
            {
                // 既存目標の更新
                existingTarget.UpdateTarget(detectionScore, Time.time);
                OnTargetUpdated?.Invoke(existingTarget);
                return true;
            }
            
            // 容量チェック
            if (!CanTrackMoreTargets)
            {
                // 最低優先度の目標と比較
                DetectedTarget lowestPriority = GetLowestPriorityTarget();
                if (lowestPriority != null && detectionScore > lowestPriority.detectionScore)
                {
                    RemoveTarget(lowestPriority);
                }
                else
                {
                    Debug.Log("[TargetTracking] Target capacity reached, cannot add new target");
                    return false;
                }
            }
            
            // 新しい目標を作成
            DetectedTarget newTarget = new DetectedTarget(targetTransform, detectionScore, Time.time);
            trackedTargets.Add(newTarget);
            activeTargetCount = trackedTargets.Count;
            
            OnTargetAdded?.Invoke(newTarget);
            OnTargetCountChanged?.Invoke(activeTargetCount);
            
            Debug.Log($"[TargetTracking] Target added: {targetTransform.name} (Score: {detectionScore:F2})");
            return true;
        }
        
        /// <summary>
        /// 目標を追跡リストから削除
        /// </summary>
        public bool RemoveTarget(Transform targetTransform)
        {
            DetectedTarget target = FindTarget(targetTransform);
            if (target != null)
            {
                return RemoveTarget(target);
            }
            return false;
        }
        
        private bool RemoveTarget(DetectedTarget target)
        {
            int index = trackedTargets.IndexOf(target);
            if (index >= 0)
            {
                RemoveTarget(index);
                return true;
            }
            return false;
        }
        
        private void RemoveTarget(int index)
        {
            if (index >= 0 && index < trackedTargets.Count)
            {
                DetectedTarget removedTarget = trackedTargets[index];
                trackedTargets.RemoveAt(index);
                activeTargetCount = trackedTargets.Count;
                
                // プライマリターゲットだった場合はクリア
                if (primaryTarget == removedTarget)
                {
                    primaryTarget = null;
                }
                
                OnTargetRemoved?.Invoke(removedTarget);
                OnTargetCountChanged?.Invoke(activeTargetCount);
                
                Debug.Log($"[TargetTracking] Target removed: {removedTarget.transform?.name ?? "NULL"}");
            }
        }
        
        /// <summary>
        /// 目標の検出スコアを更新
        /// </summary>
        public bool UpdateTarget(Transform targetTransform, float newDetectionScore)
        {
            DetectedTarget target = FindTarget(targetTransform);
            if (target != null)
            {
                target.UpdateTarget(newDetectionScore, Time.time);
                OnTargetUpdated?.Invoke(target);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 全目標をクリア
        /// </summary>
        public void ClearAllTargets()
        {
            var targets = new List<DetectedTarget>(trackedTargets);
            trackedTargets.Clear();
            primaryTarget = null;
            activeTargetCount = 0;
            
            foreach (var target in targets)
            {
                OnTargetRemoved?.Invoke(target);
            }
            
            OnTargetCountChanged?.Invoke(activeTargetCount);
            Debug.Log("[TargetTracking] All targets cleared");
        }
        
        #endregion
        
        #region Priority System
        
        /// <summary>
        /// 目標の動的優先度計算
        /// </summary>
        private float CalculateDynamicPriority(DetectedTarget target)
        {
            if (!target.IsValid || sensorTransform == null) return 0f;
            
            float priority = 0f;
            
            // 距離による優先度（近いほど高い）
            float distance = Vector3.Distance(sensorTransform.position, target.transform.position);
            float distanceScore = Mathf.Clamp01(1f - (distance / 30f)); // 30m基準
            priority += distanceScore * distancePriorityWeight;
            
            // 検出スコアによる優先度
            priority += target.detectionScore * detectionScoreWeight;
            
            // 移動による優先度（動いているほど高い）
            if (target.isMoving)
            {
                float movementScore = Mathf.Clamp01(target.movementSpeed / 10f); // 10m/s基準
                priority += movementScore * movementPriorityWeight;
            }
            
            // 脅威レベルによる優先度（疑念度が高いほど高い）
            if (target.wasSuspicious)
            {
                float threatScore = Mathf.Clamp01(target.suspicionDuration / 5f); // 5秒基準
                priority += threatScore * threatLevelWeight;
            }
            
            // 信頼度による修正
            priority *= target.confidenceLevel;
            
            return priority;
        }
        
        /// <summary>
        /// 最高優先度の目標を取得
        /// </summary>
        public DetectedTarget GetHighestPriorityTarget()
        {
            if (trackedTargets.Count == 0) return null;
            
            DetectedTarget highest = null;
            float highestPriority = float.MinValue;
            
            foreach (var target in trackedTargets)
            {
                if (target.IsValid)
                {
                    float priority = CalculateDynamicPriority(target);
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        highest = target;
                    }
                }
            }
            
            return highest;
        }
        
        /// <summary>
        /// 最低優先度の目標を取得
        /// </summary>
        public DetectedTarget GetLowestPriorityTarget()
        {
            if (trackedTargets.Count == 0) return null;
            
            DetectedTarget lowest = null;
            float lowestPriority = float.MaxValue;
            
            foreach (var target in trackedTargets)
            {
                if (target.IsValid)
                {
                    float priority = CalculateDynamicPriority(target);
                    if (priority < lowestPriority)
                    {
                        lowestPriority = priority;
                        lowest = target;
                    }
                }
            }
            
            return lowest;
        }
        
        /// <summary>
        /// 優先度順にソートされた目標リストを取得
        /// </summary>
        public List<DetectedTarget> GetTargetsByPriority()
        {
            var validTargets = trackedTargets.Where(t => t.IsValid).ToList();
            validTargets.Sort((a, b) => CalculateDynamicPriority(b).CompareTo(CalculateDynamicPriority(a)));
            return validTargets;
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// 指定されたTransformの目標を検索
        /// </summary>
        public DetectedTarget FindTarget(Transform targetTransform)
        {
            if (targetTransform == null) return null;
            
            return trackedTargets.FirstOrDefault(t => t.transform == targetTransform);
        }
        
        /// <summary>
        /// 指定された位置から最も近い目標を取得
        /// </summary>
        public DetectedTarget GetNearestTarget(Vector3 position)
        {
            if (trackedTargets.Count == 0) return null;
            
            DetectedTarget nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var target in trackedTargets)
            {
                if (target.IsValid)
                {
                    float distance = Vector3.Distance(position, target.lastKnownPosition);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = target;
                    }
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 指定された範囲内の目標を取得
        /// </summary>
        public List<DetectedTarget> GetTargetsInRange(Vector3 center, float range)
        {
            var targetsInRange = new List<DetectedTarget>();
            
            foreach (var target in trackedTargets)
            {
                if (target.IsValid && Vector3.Distance(center, target.lastKnownPosition) <= range)
                {
                    targetsInRange.Add(target);
                }
            }
            
            return targetsInRange;
        }
        
        /// <summary>
        /// 移動中の目標のみを取得
        /// </summary>
        public List<DetectedTarget> GetMovingTargets()
        {
            return trackedTargets.Where(t => t.IsValid && t.isMoving).ToList();
        }
        
        /// <summary>
        /// 疑わしい目標のみを取得
        /// </summary>
        public List<DetectedTarget> GetSuspiciousTargets()
        {
            return trackedTargets.Where(t => t.IsValid && t.wasSuspicious).ToList();
        }
        
        #endregion
        
        #region Debug Methods
        
        /// <summary>
        /// デバッグ情報の取得
        /// </summary>
        public string GetDebugInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Target Tracking Module Debug Info ===");
            sb.AppendLine($"Active Targets: {activeTargetCount}/{maxTrackedTargets}");
            sb.AppendLine($"Primary Target: {(primaryTarget?.transform?.name ?? "None")}");
            sb.AppendLine($"Update Frequency: {updateFrequency:F1} Hz");
            sb.AppendLine($"Target Expiry Time: {targetExpiryTime:F1}s");
            sb.AppendLine();
            
            sb.AppendLine("--- Target List ---");
            for (int i = 0; i < trackedTargets.Count; i++)
            {
                var target = trackedTargets[i];
                float priority = CalculateDynamicPriority(target);
                sb.AppendLine($"[{i}] {target.ToString()} | Priority: {priority:F2}");
            }
            
            return sb.ToString();
        }
        
        #endregion
    }
}