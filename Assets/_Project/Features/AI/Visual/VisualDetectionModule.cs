using UnityEngine;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Features.AI.Configuration;
using DetectionConfig = asterivo.Unity60.Features.AI.Configuration.AIDetectionConfiguration;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// 視覚検知の多重判定システム（距離・角度・遮蔽・光量）を実装
    /// </summary>
    public class VisualDetectionModule
    {
        private NPCVisualSensor sensor;
        private VisibilityCalculator visibilityCalculator;
        private AIDetectionConfiguration config;
        
        // 検出スコアの重み付け設定
        private readonly float distanceWeight = 0.25f;
        private readonly float angleWeight = 0.25f;
        private readonly float obstructionWeight = 0.25f;
        private readonly float lightWeight = 0.25f;
        
        // 閾値ベース判定
        private readonly float minimumVisibilityForDetection = 0.1f;
        private readonly float maximumDistanceForInstantDetection = 2f;
        private readonly float optimalAngleThreshold = 30f; // degrees
        
        public VisualDetectionModule(NPCVisualSensor sensor, VisibilityCalculator calculator, AIDetectionConfiguration config)
        {
            this.sensor = sensor;
            this.visibilityCalculator = calculator;
            this.config = config;
        }
        
        /// <summary>
        /// 目標の総合検出スコアを計算
        /// 多重判定システムにより、距離・角度・遮蔽・光量を総合的に評価
        /// </summary>
        /// <param name="target">検出対象</param>
        /// <returns>0.0f～1.0f の検出スコア</returns>
        public float CalculateDetectionScore(Transform target)
        {
            if (target == null || config == null) return 0f;
            
            Transform eyePos = sensor.EyePosition;
                
            // 基本的な前提条件チェック
            if (!IsWithinDetectionRange(target, eyePos))
                return 0f;
                
            // 各要素の計算
            float distanceFactor = CalculateDistanceFactor(target, eyePos);
            float angleFactor = CalculateAngleFactor(target, eyePos);
            float obstructionFactor = CalculateObstructionFactor(target, eyePos);
            float lightFactor = CalculateLightFactor(target);
            
            // 重み付け合成
            float weightedScore = 
                distanceFactor * distanceWeight +
                angleFactor * angleWeight +
                obstructionFactor * obstructionWeight +
                lightFactor * lightWeight;
                
            // 閾値ベース調整
            float adjustedScore = ApplyThresholdAdjustments(weightedScore, target, eyePos);
            
            // 最終的なクランプ
            return Mathf.Clamp01(adjustedScore);
        }
        
        #region Individual Factor Calculations
        
        /// <summary>
        /// 距離による検出係数を計算
        /// 近距離では検出しやすく、遠距離では困難になる
        /// </summary>
        private float CalculateDistanceFactor(Transform target, Transform eye)
        {
            float distance = Vector3.Distance(target.position, eye.position);
            
            // 設定された最大検知距離を使用
            float maxRange = config?.maxDetectionRange ?? 20f;
            
            if (distance >= maxRange)
                return 0f;
                
            // 距離減衰カーブの適用
            float normalizedDistance = distance / maxRange;
            
            if (config?.distanceFalloffCurve != null)
            {
                return config.distanceFalloffCurve.Evaluate(normalizedDistance);
            }
            
            // フォールバック：線形減衰
            return 1f - normalizedDistance;
        }
        
        /// <summary>
        /// 角度による検出係数を計算
        /// 視野角の中央に近いほど検出しやすい
        /// </summary>
        private float CalculateAngleFactor(Transform target, Transform eye)
        {
            Vector3 directionToTarget = (target.position - eye.position).normalized;
            float angle = Vector3.Angle(eye.forward, directionToTarget);
            
            // 設定された視野角を使用
            float fov = config?.fieldOfView ?? 110f;
            float halfFOV = fov / 2f;
            
            if (angle > halfFOV)
                return 0f;
                
            // 角度正規化（0.0 = 真正面, 1.0 = 視野角端）
            float normalizedAngle = angle / halfFOV;
            
            // 視野角の中央により高いスコアを付与
            return 1f - (normalizedAngle * 0.8f); // 端でも20%のスコアは保持
        }
        
        /// <summary>
        /// 遮蔽による検出係数を計算
        /// 複数点チェックによる部分遮蔽の検出
        /// </summary>
        private float CalculateObstructionFactor(Transform target, Transform eye)
        {
            if (visibilityCalculator != null)
            {
                // VisibilityCalculatorの高度な遮蔽計算を使用
                float visibility = visibilityCalculator.CalculateVisibility(eye.position, target.position, target);
                
                // visibilityは総合スコアなので、遮蔽部分のみを抽出
                // ここでは簡略化して、visibilityが高いほど遮蔽が少ないとする
                return Mathf.Clamp01(visibility * 2f); // 遮蔽係数として調整
            }
            
            // フォールバック：シンプルな視線チェック
            return HasDirectLineOfSight(target, eye) ? 1f : 0f;
        }
        
        /// <summary>
        /// 光量による検出係数を計算
        /// 明るい場所ほど検出しやすい
        /// </summary>
        private float CalculateLightFactor(Transform target)
        {
            if (visibilityCalculator != null)
            {
                // VisibilityCalculatorの光量計算を利用
                // 実際の実装では、VisibilityCalculatorに光量のみを取得するメソッドを追加することを推奨
                float visibility = visibilityCalculator.CalculateVisibility(sensor.transform.position, target.position, target);
                
                // 光量係数の推定（実装では専用メソッドを使用すべき）
                return Mathf.Clamp01(visibility + 0.3f); // 暗闇でも最低限の視認性を保持
            }
            
            // フォールバック：固定値
            return 0.7f; // デフォルト光量レベル
        }
        
        #endregion
        
        #region Threshold-based Adjustments
        
        /// <summary>
        /// 閾値ベースの調整を適用
        /// ゲームプレイバランスのための特別な調整ルール
        /// </summary>
        private float ApplyThresholdAdjustments(float baseScore, Transform target, Transform eye)
        {
            float adjustedScore = baseScore;
            
            // 超近距離での即座検出
            float distance = Vector3.Distance(target.position, eye.position);
            if (distance <= maximumDistanceForInstantDetection)
            {
                adjustedScore = Mathf.Max(adjustedScore, 0.9f);
            }
            
            // 最適角度での検出ブースト
            Vector3 directionToTarget = (target.position - eye.position).normalized;
            float angle = Vector3.Angle(eye.forward, directionToTarget);
            if (angle <= optimalAngleThreshold)
            {
                adjustedScore *= 1.2f; // 20%ブースト
            }
            
            // 最小視認性チェック
            if (adjustedScore > 0f && adjustedScore < minimumVisibilityForDetection)
            {
                adjustedScore = 0f; // 閾値以下は無視
            }
            
            return adjustedScore;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 検知範囲内かどうかを判定
        /// </summary>
        private bool IsWithinDetectionRange(Transform target, Transform eye)
        {
            float distance = Vector3.Distance(target.position, eye.position);
            float maxRange = config?.maxDetectionRange ?? 20f;
            return distance <= maxRange;
        }
        
        /// <summary>
        /// 直接的な視線があるかどうかを判定
        /// </summary>
        private bool HasDirectLineOfSight(Transform target, Transform eye)
        {
            Vector3 direction = target.position - eye.position;
            LayerMask obstacles = -1; // デフォルトですべてのレイヤー
            return !Physics.Raycast(eye.position, direction.normalized, direction.magnitude, obstacles);
        }
        
        #endregion
        
        #region Public Utility Methods
        
        /// <summary>
        /// デバッグ情報用：個別係数の詳細を取得
        /// </summary>
        public DetectionScoreBreakdown GetScoreBreakdown(Transform target)
        {
            if (target == null) return null;
            
            Transform eyePos = sensor.EyePosition;
            
            return new DetectionScoreBreakdown
            {
                distanceFactor = CalculateDistanceFactor(target, eyePos),
                angleFactor = CalculateAngleFactor(target, eyePos),
                obstructionFactor = CalculateObstructionFactor(target, eyePos),
                lightFactor = CalculateLightFactor(target),
                totalScore = CalculateDetectionScore(target)
            };
        }
        
        /// <summary>
        /// 設定の動的更新
        /// </summary>
        public void UpdateConfiguration(DetectionConfig newConfig)
        {
            config = newConfig;
        }
        
        #endregion
    }
    
    #region Supporting Classes
    
    /// <summary>
    /// 検出スコアの内訳情報（デバッグ用）
    /// </summary>
    public class DetectionScoreBreakdown
    {
        public float distanceFactor;
        public float angleFactor;
        public float obstructionFactor;
        public float lightFactor;
        public float totalScore;
        
        public override string ToString()
        {
            return $"Distance: {distanceFactor:F2}, Angle: {angleFactor:F2}, " +
                   $"Obstruction: {obstructionFactor:F2}, Light: {lightFactor:F2}, " +
                   $"Total: {totalScore:F2}";
        }
    }
    
    #endregion
}