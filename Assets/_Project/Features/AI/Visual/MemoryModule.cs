using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using asterivo.Unity60.Core.Data;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// NPCの記憶システム
    /// 短期記憶（5秒）→長期記憶（30秒）階層管理
    /// 位置履歴管理と目標予測位置計算を実装
    /// </summary>
    [System.Serializable]
    public class MemoryModule
    {
        #region Inspector Fields
        
        [BoxGroup("Memory Settings")]
        [PropertyRange(3f, 10f)]
        [LabelText("Short Term Memory Duration")]
        [SuffixLabel("s")]
        [SerializeField] private float shortTermMemoryDuration = 5f;
        
        [BoxGroup("Memory Settings")]
        [PropertyRange(15f, 60f)]
        [LabelText("Long Term Memory Duration")]
        [SuffixLabel("s")]
        [SerializeField] private float longTermMemoryDuration = 30f;
        
        [BoxGroup("Memory Settings")]
        [PropertyRange(5, 50)]
        [LabelText("Max Memory Entries")]
        [SerializeField] private int maxMemoryEntries = 20;
        
        [BoxGroup("Position Prediction")]
        [PropertyRange(0.5f, 5f)]
        [LabelText("Prediction Time")]
        [SuffixLabel("s")]
        [SerializeField] private float predictionTime = 2f;
        
        [BoxGroup("Position Prediction")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Velocity Sample Rate")]
        [SuffixLabel("s")]
        // TODO: Future enhancement - advanced velocity sampling
#pragma warning disable CS0414 // Field assigned but never used - planned for velocity sampling enhancement
        [SerializeField] private float velocitySampleRate = 0.2f;
#pragma warning restore CS0414
        
        [BoxGroup("Position Prediction")]
        [PropertyRange(1f, 10f)]
        [LabelText("Max Prediction Distance")]
        [SuffixLabel("m")]
        [SerializeField] private float maxPredictionDistance = 5f;
        
        [BoxGroup("Confidence")]
        [PropertyRange(0.1f, 1f)]
        [LabelText("Memory Confidence Decay")]
        [SuffixLabel("/s")]
        [SerializeField] private float memoryConfidenceDecay = 0.1f;
        
        [BoxGroup("Confidence")]
        [PropertyRange(0f, 1f)]
        [LabelText("Min Confidence Threshold")]
        [SerializeField] private float minConfidenceThreshold = 0.1f;
        
        #endregion
        
        #region Runtime Data
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Short Term Memories")]
        private List<MemoryEntry> shortTermMemories = new List<MemoryEntry>();
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Long Term Memories")]
        private List<MemoryEntry> longTermMemories = new List<MemoryEntry>();
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Position History")]
        private Dictionary<int, List<PositionRecord>> positionHistories = new Dictionary<int, List<PositionRecord>>();
        
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Active Targets")]
        private Dictionary<int, TargetMemoryInfo> activeTargets = new Dictionary<int, TargetMemoryInfo>();
        
        private float lastCleanupTime = 0f;
        private const float cleanupInterval = 1f;
        
        #endregion
        
        #region Properties
        
        public int ShortTermMemoryCount => shortTermMemories.Count;
        public int LongTermMemoryCount => longTermMemories.Count;
        public int TotalMemoryCount => shortTermMemories.Count + longTermMemories.Count;
        public bool HasMemories => TotalMemoryCount > 0;
        
        #endregion
        
        #region Events
        
        public event Action<MemoryEntry> OnMemoryAdded;
        public event Action<MemoryEntry> OnMemoryMoved;
        public event Action<MemoryEntry> OnMemoryRemoved;
        public event Action<int, Vector3> OnTargetPositionPredicted;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Memory Moduleの初期化
        /// </summary>
        public void Initialize()
        {
            shortTermMemories.Clear();
            longTermMemories.Clear();
            positionHistories.Clear();
            activeTargets.Clear();
            lastCleanupTime = Time.time;
            
            Debug.Log("[MemorySystem] Memory Module initialized");
        }
        
        #endregion
        
        #region Update Methods
        
        /// <summary>
        /// 記憶システムの更新（毎フレーム呼び出し）
        /// </summary>
        public void UpdateMemorySystem(float deltaTime)
        {
            // 定期的なクリーンアップ
            if (Time.time - lastCleanupTime >= cleanupInterval)
            {
                CleanupExpiredMemories();
                UpdateMemoryConfidence(deltaTime);
                ProcessMemoryTransitions();
                lastCleanupTime = Time.time;
            }
            
            // 位置予測の更新
            UpdatePositionPredictions(deltaTime);
        }
        
        private void CleanupExpiredMemories()
        {
            float currentTime = Time.time;
            
            // 短期記憶の期限切れチェック
            for (int i = shortTermMemories.Count - 1; i >= 0; i--)
            {
                var memory = shortTermMemories[i];
                if (currentTime - memory.timestamp > shortTermMemoryDuration)
                {
                    // 長期記憶への移行チェック
                    if (memory.confidence >= minConfidenceThreshold && longTermMemories.Count < maxMemoryEntries)
                    {
                        MoveToLongTermMemory(memory);
                    }
                    
                    RemoveShortTermMemory(i);
                }
            }
            
            // 長期記憶の期限切れチェック
            for (int i = longTermMemories.Count - 1; i >= 0; i--)
            {
                var memory = longTermMemories[i];
                if (currentTime - memory.timestamp > longTermMemoryDuration || memory.confidence < minConfidenceThreshold)
                {
                    RemoveLongTermMemory(i);
                }
            }
        }
        
        private void UpdateMemoryConfidence(float deltaTime)
        {
            // 短期記憶の信頼度減衰
            for (int i = 0; i < shortTermMemories.Count; i++)
            {
                var memory = shortTermMemories[i];
                memory.confidence -= memoryConfidenceDecay * deltaTime;
                memory.confidence = Mathf.Max(0f, memory.confidence);
                shortTermMemories[i] = memory;
            }
            
            // 長期記憶の信頼度減衰
            for (int i = 0; i < longTermMemories.Count; i++)
            {
                var memory = longTermMemories[i];
                memory.confidence -= memoryConfidenceDecay * deltaTime * 0.5f; // 長期記憶は減衰が遅い
                memory.confidence = Mathf.Max(0f, memory.confidence);
                longTermMemories[i] = memory;
            }
        }
        
        private void ProcessMemoryTransitions()
        {
            // 信頼度の高い短期記憶の長期記憶への移行処理は CleanupExpiredMemories で実行
        }
        
        private void UpdatePositionPredictions(float deltaTime)
        {
            foreach (var kvp in activeTargets.ToList())
            {
                int targetId = kvp.Key;
                var targetInfo = kvp.Value;
                
                if (positionHistories.ContainsKey(targetId))
                {
                    Vector3 predictedPosition = CalculatePredictedPosition(targetId);
                    OnTargetPositionPredicted?.Invoke(targetId, predictedPosition);
                }
            }
        }
        
        #endregion
        
        #region Memory Management
        
        /// <summary>
        /// 新しい記憶の追加
        /// </summary>
        public void AddMemory(int targetId, Vector3 position, float confidence, string description = "", MemoryType type = MemoryType.Visual)
        {
            var memory = new MemoryEntry
            {
                targetId = targetId,
                position = position,
                timestamp = Time.time,
                confidence = confidence,
                description = description,
                memoryType = type,
                isReinforced = false
            };
            
            shortTermMemories.Add(memory);
            OnMemoryAdded?.Invoke(memory);
            
            // 位置履歴への追加
            AddPositionRecord(targetId, position, confidence);
            
            // アクティブターゲット情報の更新
            UpdateActiveTarget(targetId, position, confidence);
            
            Debug.Log($"[MemorySystem] Memory added: Target {targetId} at {position} (confidence: {confidence:F2})");
        }
        
        /// <summary>
        /// 記憶の強化（同じ対象を再発見した場合）
        /// </summary>
        public void ReinforceMemory(int targetId, Vector3 position, float additionalConfidence)
        {
            // 短期記憶から対象を検索
            for (int i = 0; i < shortTermMemories.Count; i++)
            {
                var memory = shortTermMemories[i];
                if (memory.targetId == targetId && Vector3.Distance(memory.position, position) < 2f)
                {
                    memory.confidence = Mathf.Clamp01(memory.confidence + additionalConfidence);
                    memory.isReinforced = true;
                    memory.timestamp = Time.time; // タイムスタンプ更新
                    shortTermMemories[i] = memory;
                    
                    Debug.Log($"[MemorySystem] Memory reinforced: Target {targetId} (new confidence: {memory.confidence:F2})");
                    return;
                }
            }
            
            // 見つからなければ新しい記憶として追加
            AddMemory(targetId, position, additionalConfidence);
        }
        
        private void MoveToLongTermMemory(MemoryEntry memory)
        {
            if (longTermMemories.Count >= maxMemoryEntries)
            {
                // 最も信頼度の低い記憶を削除
                var lowestConfidence = longTermMemories.OrderBy(m => m.confidence).First();
                longTermMemories.Remove(lowestConfidence);
                OnMemoryRemoved?.Invoke(lowestConfidence);
            }
            
            longTermMemories.Add(memory);
            OnMemoryMoved?.Invoke(memory);
            
            Debug.Log($"[MemorySystem] Memory moved to long-term: Target {memory.targetId}");
        }
        
        private void RemoveShortTermMemory(int index)
        {
            var memory = shortTermMemories[index];
            shortTermMemories.RemoveAt(index);
            OnMemoryRemoved?.Invoke(memory);
        }
        
        private void RemoveLongTermMemory(int index)
        {
            var memory = longTermMemories[index];
            longTermMemories.RemoveAt(index);
            OnMemoryRemoved?.Invoke(memory);
        }
        
        #endregion
        
        #region Position Tracking
        
        private void AddPositionRecord(int targetId, Vector3 position, float confidence)
        {
            if (!positionHistories.ContainsKey(targetId))
            {
                positionHistories[targetId] = new List<PositionRecord>();
            }
            
            var record = new PositionRecord
            {
                position = position,
                timestamp = Time.time,
                confidence = confidence
            };
            
            positionHistories[targetId].Add(record);
            
            // 古い記録の削除（最大20個まで保持）
            if (positionHistories[targetId].Count > 20)
            {
                positionHistories[targetId].RemoveAt(0);
            }
        }
        
        private void UpdateActiveTarget(int targetId, Vector3 position, float confidence)
        {
            if (!activeTargets.ContainsKey(targetId))
            {
                activeTargets[targetId] = new TargetMemoryInfo();
            }
            
            var targetInfo = activeTargets[targetId];
            targetInfo.lastKnownPosition = position;
            targetInfo.lastSeenTime = Time.time;
            targetInfo.confidence = confidence;
            targetInfo.totalSightings++;
            
            activeTargets[targetId] = targetInfo;
        }
        
        #endregion
        
        #region Position Prediction
        
        /// <summary>
        /// 目標の予測位置計算
        /// </summary>
        public Vector3 CalculatePredictedPosition(int targetId)
        {
            if (!positionHistories.ContainsKey(targetId) || positionHistories[targetId].Count < 2)
            {
                return GetLastKnownPosition(targetId);
            }
            
            var history = positionHistories[targetId];
            var recentHistory = history.Where(r => Time.time - r.timestamp <= predictionTime * 2f).OrderBy(r => r.timestamp).ToList();
            
            if (recentHistory.Count < 2)
            {
                return GetLastKnownPosition(targetId);
            }
            
            // 線形予測
            Vector3 velocity = CalculateAverageVelocity(recentHistory);
            Vector3 lastPosition = recentHistory.Last().position;
            Vector3 predictedPosition = lastPosition + velocity * predictionTime;
            
            // 予測距離の制限
            float predictionDistance = Vector3.Distance(lastPosition, predictedPosition);
            if (predictionDistance > maxPredictionDistance)
            {
                Vector3 direction = (predictedPosition - lastPosition).normalized;
                predictedPosition = lastPosition + direction * maxPredictionDistance;
            }
            
            return predictedPosition;
        }
        
        private Vector3 CalculateAverageVelocity(List<PositionRecord> history)
        {
            if (history.Count < 2) return Vector3.zero;
            
            Vector3 totalVelocity = Vector3.zero;
            int velocityCount = 0;
            
            for (int i = 1; i < history.Count; i++)
            {
                float deltaTime = history[i].timestamp - history[i - 1].timestamp;
                if (deltaTime > 0f)
                {
                    Vector3 velocity = (history[i].position - history[i - 1].position) / deltaTime;
                    totalVelocity += velocity;
                    velocityCount++;
                }
            }
            
            return velocityCount > 0 ? totalVelocity / velocityCount : Vector3.zero;
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// 最後に確認された位置を取得
        /// </summary>
        public Vector3 GetLastKnownPosition(int targetId)
        {
            // 短期記憶から検索
            var shortTermMemory = shortTermMemories.FindLast(m => m.targetId == targetId);
            if (shortTermMemory.targetId == targetId)
            {
                return shortTermMemory.position;
            }
            
            // 長期記憶から検索
            var longTermMemory = longTermMemories.FindLast(m => m.targetId == targetId);
            if (longTermMemory.targetId == targetId)
            {
                return longTermMemory.position;
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// 対象に関する記憶があるかチェック
        /// </summary>
        public bool HasMemoryOf(int targetId)
        {
            return shortTermMemories.Any(m => m.targetId == targetId) || 
                   longTermMemories.Any(m => m.targetId == targetId);
        }
        
        /// <summary>
        /// 対象の記憶の信頼度を取得
        /// </summary>
        public float GetMemoryConfidence(int targetId)
        {
            var shortTermMemory = shortTermMemories.FindLast(m => m.targetId == targetId);
            if (shortTermMemory.targetId == targetId)
            {
                return shortTermMemory.confidence;
            }
            
            var longTermMemory = longTermMemories.FindLast(m => m.targetId == targetId);
            if (longTermMemory.targetId == targetId)
            {
                return longTermMemory.confidence;
            }
            
            return 0f;
        }
        
        /// <summary>
        /// 全記憶のクリア
        /// </summary>
        public void ClearAllMemories()
        {
            shortTermMemories.Clear();
            longTermMemories.Clear();
            positionHistories.Clear();
            activeTargets.Clear();
            
            Debug.Log("[MemorySystem] All memories cleared");
        }
        
        #endregion
    }
    
    #region Support Structures
    
    /// <summary>
    /// 記憶エントリ
    /// </summary>
    [System.Serializable]
    public struct MemoryEntry
    {
        public int targetId;
        public Vector3 position;
        public float timestamp;
        public float confidence;
        public string description;
        public MemoryType memoryType;
        public bool isReinforced;
    }
    
    /// <summary>
    /// 位置記録
    /// </summary>
    [System.Serializable]
    public struct PositionRecord
    {
        public Vector3 position;
        public float timestamp;
        public float confidence;
    }
    
    /// <summary>
    /// 目標記憶情報
    /// </summary>
    [System.Serializable]
    public struct TargetMemoryInfo
    {
        public Vector3 lastKnownPosition;
        public float lastSeenTime;
        public float confidence;
        public int totalSightings;
    }
    
    /// <summary>
    /// 記憶タイプ
    /// </summary>
    public enum MemoryType
    {
        Visual,
        Audio,
        Investigation,
        Communication
    }
    
    #endregion
}