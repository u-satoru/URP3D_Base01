using UnityEngine;

namespace asterivo.Unity60.Features.AI.Visual
{
    /// <summary>
    /// 視覚的な可視性を計算するためのユーティリティクラス
    /// 光量、遮蔽物、距離などの要因を考慮して総合的な可視性スコアを計算
    /// </summary>
    public class VisibilityCalculator
    {
        private LayerMask obstructionMask;
        private float maxVisualRange;

        public VisibilityCalculator(LayerMask obstructionMask, float maxVisualRange = 50f)
        {
            this.obstructionMask = obstructionMask;
            this.maxVisualRange = maxVisualRange;
        }

        /// <summary>
        /// ターゲットの可視性スコアを計算（0.0 = 見えない, 1.0 = 完全に見える）
        /// </summary>
        public float CalculateVisibility(Vector3 observerPosition, Vector3 targetPosition, Transform target = null)
        {
            float distance = Vector3.Distance(observerPosition, targetPosition);

            // 距離による減衰
            float distanceScore = 1f - Mathf.Clamp01(distance / maxVisualRange);

            // 遮蔽物チェック
            bool hasLineOfSight = CheckLineOfSight(observerPosition, targetPosition);
            if (!hasLineOfSight)
                return 0f;

            // 光量による影響（簡易的な実装）
            float lightScore = CalculateLightScore(targetPosition);

            // 総合スコア
            return distanceScore * lightScore;
        }

        /// <summary>
        /// 視線が通っているかチェック
        /// </summary>
        public bool CheckLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.Raycast(from, direction.normalized, out RaycastHit hit, distance, obstructionMask))
            {
                // 遮蔽物がある場合はfalse
                return false;
            }

            return true;
        }

        /// <summary>
        /// ターゲット位置の光量スコアを計算
        /// </summary>
        private float CalculateLightScore(Vector3 position)
        {
            // 簡易実装：環境光の強度を使用
            float ambientIntensity = RenderSettings.ambientIntensity;

            // ポイントライトやスポットライトの影響を追加で計算可能
            Light[] nearbyLights = GetNearbyLights(position, 10f);
            float additionalLight = 0f;

            foreach (var light in nearbyLights)
            {
                if (!light.enabled) continue;

                float distance = Vector3.Distance(light.transform.position, position);
                float attenuation = 1f - Mathf.Clamp01(distance / light.range);
                additionalLight += light.intensity * attenuation;
            }

            // 正規化して0-1の範囲に収める
            float totalLight = ambientIntensity + additionalLight * 0.5f;
            return Mathf.Clamp01(totalLight);
        }

        /// <summary>
        /// 指定位置の近くにあるライトを取得
        /// </summary>
        private Light[] GetNearbyLights(Vector3 position, float radius)
        {
            // パフォーマンスのため、事前にキャッシュするか、
            // Light管理システムを使用することを推奨
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            System.Collections.Generic.List<Light> lights = new System.Collections.Generic.List<Light>();

            foreach (var collider in colliders)
            {
                Light light = collider.GetComponent<Light>();
                if (light != null)
                {
                    lights.Add(light);
                }
            }

            return lights.ToArray();
        }

        /// <summary>
        /// 角度による可視性スコアを計算
        /// </summary>
        public float CalculateAngleScore(Vector3 observerForward, Vector3 directionToTarget, float maxAngle)
        {
            float angle = Vector3.Angle(observerForward, directionToTarget);
            if (angle > maxAngle)
                return 0f;

            return 1f - (angle / maxAngle);
        }

        /// <summary>
        /// 移動による検出しやすさを計算
        /// </summary>
        public float CalculateMovementScore(Vector3 targetVelocity, float maxDetectableSpeed = 10f)
        {
            float speed = targetVelocity.magnitude;
            return Mathf.Clamp01(speed / maxDetectableSpeed);
        }

        /// <summary>
        /// カバー状態による可視性の減衰を計算
        /// </summary>
        public float CalculateCoverScore(bool isInCover, bool isCrouching)
        {
            if (isInCover)
                return 0.3f; // カバー中は30%の可視性
            if (isCrouching)
                return 0.6f; // しゃがみ中は60%の可視性
            return 1.0f; // 通常時は100%
        }

        /// <summary>
        /// 総合的な検出スコアを計算
        /// </summary>
        public float CalculateTotalDetectionScore(
            Vector3 observerPosition,
            Vector3 observerForward,
            Vector3 targetPosition,
            Vector3 targetVelocity,
            float viewAngle,
            bool targetInCover = false,
            bool targetCrouching = false)
        {
            // 基本的な可視性
            float visibilityScore = CalculateVisibility(observerPosition, targetPosition);
            if (visibilityScore <= 0f)
                return 0f;

            // 角度スコア
            Vector3 directionToTarget = (targetPosition - observerPosition).normalized;
            float angleScore = CalculateAngleScore(observerForward, directionToTarget, viewAngle);
            if (angleScore <= 0f)
                return 0f;

            // 動きスコア
            float movementScore = CalculateMovementScore(targetVelocity);

            // カバースコア
            float coverScore = CalculateCoverScore(targetInCover, targetCrouching);

            // 総合スコア（重み付け平均）
            float totalScore = visibilityScore * 0.3f +
                              angleScore * 0.3f +
                              movementScore * 0.2f +
                              coverScore * 0.2f;

            return Mathf.Clamp01(totalScore);
        }
    }
}
