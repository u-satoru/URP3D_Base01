using UnityEngine;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// 視覚センサーによって検出された目標の情報を保持するデータクラス
    /// </summary>
    [System.Serializable]
    public class DetectedTarget
    {
        [Header("Target Information")]
        public Transform transform;
        
        [Header("Detection Data")]
        public float detectionScore;
        public float firstDetectedTime;
        public float lastSeenTime;
        
        [Header("Position Tracking")]
        public Vector3 firstDetectedPosition;
        public Vector3 lastKnownPosition;
        public Vector3 estimatedVelocity;
        
        [Header("Confidence & Priority")]
        [Range(0f, 1f)]
        public float confidenceLevel;
        public int priority;
        
        [Header("Behavioral Data")]
        public bool isMoving;
        public float movementSpeed;
        public bool wasSuspicious;
        public float suspicionDuration;
        
        /// <summary>
        /// 新しい検出目標を作成
        /// </summary>
        public DetectedTarget(Transform target, float score, float time)
        {
            transform = target;
            detectionScore = score;
            firstDetectedTime = time;
            lastSeenTime = time;
            
            if (target != null)
            {
                firstDetectedPosition = target.position;
                lastKnownPosition = target.position;
            }
            
            confidenceLevel = Mathf.Clamp01(score);
            priority = CalculateInitialPriority(score);
            
            // 初期状態
            isMoving = false;
            movementSpeed = 0f;
            wasSuspicious = score >= 0.5f;
            suspicionDuration = 0f;
            estimatedVelocity = Vector3.zero;
        }
        
        /// <summary>
        /// 目標をリセット（メモリプール用）
        /// </summary>
        public void ResetTarget(Transform newTarget, float newScore, float currentTime)
        {
            transform = newTarget;
            detectionScore = newScore;
            firstDetectedTime = currentTime;
            lastSeenTime = currentTime;
            
            if (newTarget != null)
            {
                firstDetectedPosition = newTarget.position;
                lastKnownPosition = newTarget.position;
            }
            else
            {
                firstDetectedPosition = Vector3.zero;
                lastKnownPosition = Vector3.zero;
            }
            
            confidenceLevel = Mathf.Clamp01(newScore);
            priority = CalculateInitialPriority(newScore);
            
            // 初期状態にリセット
            isMoving = false;
            movementSpeed = 0f;
            wasSuspicious = newScore >= 0.5f;
            suspicionDuration = 0f;
            estimatedVelocity = Vector3.zero;
        }

        /// <summary>
        /// 目標情報を更新
        /// </summary>
        public void UpdateTarget(float newScore, float currentTime)
        {
            float deltaTime = currentTime - lastSeenTime;
            
            // スコアと信頼度の更新
            detectionScore = newScore;
            confidenceLevel = Mathf.Lerp(confidenceLevel, newScore, deltaTime * 2f);
            
            // 位置と速度の更新
            if (transform != null)
            {
                Vector3 newPosition = transform.position;
                
                if (deltaTime > 0f)
                {
                    estimatedVelocity = (newPosition - lastKnownPosition) / deltaTime;
                    movementSpeed = estimatedVelocity.magnitude;
                    isMoving = movementSpeed > 0.1f; // 閾値以上で移動中とみなす
                }
                
                lastKnownPosition = newPosition;
            }
            
            // 疑念レベルの追跡
            if (newScore >= 0.5f)
            {
                if (!wasSuspicious)
                {
                    wasSuspicious = true;
                    suspicionDuration = 0f;
                }
                suspicionDuration += deltaTime;
            }
            else
            {
                wasSuspicious = false;
            }
            
            // 優先度の更新
            priority = CalculateDynamicPriority();
            
            // 最終更新時刻
            lastSeenTime = currentTime;
        }
        
        /// <summary>
        /// 目標の予測位置を計算
        /// </summary>
        /// <param name="futureTime">何秒先を予測するか</param>
        /// <returns>予測位置</returns>
        public Vector3 GetPredictedPosition(float futureTime)
        {
            if (!isMoving || estimatedVelocity.magnitude < 0.01f)
                return lastKnownPosition;
                
            return lastKnownPosition + estimatedVelocity * futureTime;
        }
        
        /// <summary>
        /// 目標を見失ってからの時間
        /// </summary>
        public float TimeSinceLastSeen => Time.time - lastSeenTime;
        
        /// <summary>
        /// 検出してからの総時間
        /// </summary>
        public float TotalDetectionTime => Time.time - firstDetectedTime;
        
        /// <summary>
        /// 目標が有効かどうか
        /// </summary>
        public bool IsValid => transform != null;
        
        /// <summary>
        /// 初期優先度を計算
        /// </summary>
        private int CalculateInitialPriority(float score)
        {
            if (score >= 0.9f) return 5; // 最高優先度
            if (score >= 0.7f) return 4; // 高優先度
            if (score >= 0.5f) return 3; // 中優先度
            if (score >= 0.3f) return 2; // 低優先度
            return 1; // 最低優先度
        }
        
        /// <summary>
        /// 動的優先度を計算（行動パターンも考慮）
        /// </summary>
        private int CalculateDynamicPriority()
        {
            int basePriority = CalculateInitialPriority(detectionScore);
            
            // 移動している目標は優先度を上げる
            if (isMoving && movementSpeed > 2f)
                basePriority += 1;
                
            // 長時間疑念を持たれている目標は優先度を上げる
            if (wasSuspicious && suspicionDuration > 3f)
                basePriority += 1;
                
            // 信頼度が高い目標は優先度を上げる
            if (confidenceLevel > 0.8f)
                basePriority += 1;
                
            return Mathf.Clamp(basePriority, 1, 5);
        }
        
        /// <summary>
        /// 検出目標の状態を文字列で表現
        /// </summary>
        public override string ToString()
        {
            string targetName = transform != null ? transform.name : "NULL";
            return $"Target: {targetName}, Score: {detectionScore:F2}, Priority: {priority}, " +
                   $"Confidence: {confidenceLevel:F2}, Moving: {isMoving}, Speed: {movementSpeed:F1}";
        }
        
        /// <summary>
        /// デバッグ情報の詳細版
        /// </summary>
        public string GetDetailedInfo()
        {
            string targetName = transform != null ? transform.name : "NULL";
            return $"=== Detected Target Info ===\n" +
                   $"Name: {targetName}\n" +
                   $"Detection Score: {detectionScore:F3}\n" +
                   $"Confidence Level: {confidenceLevel:F3}\n" +
                   $"Priority: {priority}\n" +
                   $"First Detected: {firstDetectedTime:F1}s\n" +
                   $"Last Seen: {lastSeenTime:F1}s\n" +
                   $"Time Since Last Seen: {TimeSinceLastSeen:F1}s\n" +
                   $"Total Detection Time: {TotalDetectionTime:F1}s\n" +
                   $"Position: {lastKnownPosition}\n" +
                   $"Velocity: {estimatedVelocity}\n" +
                   $"Speed: {movementSpeed:F2} m/s\n" +
                   $"Is Moving: {isMoving}\n" +
                   $"Was Suspicious: {wasSuspicious}\n" +
                   $"Suspicion Duration: {suspicionDuration:F1}s";
        }
    }
}