using UnityEngine;
using asterivo.Unity60.Core.Data;
using asterivo.Unity60.Core.Events;

namespace asterivo.Unity60.Stealth.Detection
{
    /// <summary>
    /// ステルスゲームにおける視認性計算を行うコンポーネント
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このクラスは、高度なステルスゲームシステムにおける視認性の計算を担当します。
    /// 単純な距離や角度だけでなく、複数の要素を総合的に評価することで、
    /// リアルで戦略的な隠密プレイを実現します。
    /// 
    /// 主要な計算要素：
    /// 1. 距離係数：観察者からの距離による検出難易度
    /// 2. 角度係数：観察者の視野角内での位置による検出率
    /// 3. 遮蔽係数：障害物による視線遮蔽の評価
    /// 4. 光量係数：照明状況による視認性の変化
    /// 
    /// 高度な機能：
    /// - 複数チェックポイントによる部分遮蔽の検出
    /// - 360度光量サンプリングによる照明評価
    /// - 音響検知システム（壁を通した減衰含む）
    /// - デバッグ可視化システム
    /// 
    /// ゲームプレイへの影響：
    /// プレイヤーは以下の戦略的要素を考慮する必要があります：
    /// - 光の当たらない場所での待機
    /// - 障害物を利用した視線遮断
    /// - 敵の視野角を意識した移動
    /// - 距離を活用した潜行
    /// 
    /// パフォーマンス最適化：
    /// - 距離による早期カリング
    /// - 必要時のみのRaycast実行
    /// - 設定可能なサンプル数
    /// - デバッグ機能のビルド時除外
    /// 
    /// 連携システム：
    /// - DetectionConfiguration：検出パラメータの外部設定
    /// - AIStateMachine：検出結果による状態遷移
    /// - PlayerStealthController：プレイヤーの隠密行動
    /// </remarks>
    public class VisibilityCalculator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DetectionConfiguration config;
        
        [Header("Light Detection")]
        [SerializeField] private LayerMask obstacleLayerMask = -1;
        [SerializeField] private int lightSampleCount = 8;
        
        [Header("Debug")]
        [SerializeField] private bool debugDrawRays = false;
        
        /// <summary>
        /// 目標と観察者間の総合的な視認性を計算します
        /// </summary>
        /// <param name="target">視認される目標のTransform</param>
        /// <param name="observer">観察者のTransform</param>
        /// <returns>0.0f（完全に隠れている）から1.0f（完全に見えている）までの視認性</returns>
        /// <remarks>
        /// 計算式：
        /// Visibility = Distance × Angle × Obstruction × Light
        /// 
        /// 各係数の詳細：
        /// - Distance（距離係数）：設定されたカーブに基づく距離減衰
        /// - Angle（角度係数）：観察者の視野角内での位置評価
        /// - Obstruction（遮蔽係数）：複数点での視線チェック結果
        /// - Light（光量係数）：周囲の照明状況による視認性変化
        /// 
        /// 戻り値の解釈：
        /// - 0.0f - 0.2f：発見困難（安全な隠匿状態）
        /// - 0.2f - 0.5f：発見可能性低（注意深い移動推奨）
        /// - 0.5f - 0.8f：発見リスク高（迅速な隠蔽行動必要）
        /// - 0.8f - 1.0f：発見ほぼ確実（即座の退避必要）
        /// 
        /// パフォーマンス考慮：
        /// - maxDetectionRangeを超えた場合の早期終了
        /// - null チェックによる例外防止
        /// - 各計算の順次実行による最適化
        /// 
        /// 使用例：
        /// <code>
        /// float visibility = calculator.CalculateVisibility(player, guard);
        /// if (visibility > 0.7f) 
        /// {
        ///     // 警戒状態に遷移
        ///     ai.OnTargetSpotted(player);
        /// }
        /// </code>
        /// </remarks>
        public float CalculateVisibility(Transform target, Transform observer)
        {
            if (target == null || observer == null) return 0f;
            
            float distance = Vector3.Distance(target.position, observer.position);
            
            if (distance > config.maxDetectionRange)
                return 0f;
                
            float distanceFactor = config.distanceFalloffCurve.Evaluate(
                distance / config.maxDetectionRange);
                
            float angleFactor = CalculateAngleFactor(target, observer);
            
            float obstructionFactor = CalculateObstructionFactor(target, observer);
            
            float lightFactor = CalculateLightFactor(target.position);
            
            float visibility = distanceFactor * angleFactor * obstructionFactor * lightFactor;
            
            return Mathf.Clamp01(visibility);
        }
        
        private float CalculateAngleFactor(Transform target, Transform observer)
        {
            Vector3 directionToTarget = (target.position - observer.position).normalized;
            float angle = Vector3.Angle(observer.forward, directionToTarget);
            
            if (angle > config.fieldOfView / 2f)
                return 0f;
                
            return 1f - (angle / (config.fieldOfView / 2f));
        }
        
        private float CalculateObstructionFactor(Transform target, Transform observer)
        {
            Vector3[] checkPoints = GetTargetCheckPoints(target);
            int visiblePoints = 0;
            
            foreach (Vector3 point in checkPoints)
            {
                Vector3 direction = point - observer.position;
                float distance = direction.magnitude;
                
                if (!Physics.Raycast(observer.position, direction.normalized, 
                    distance, obstacleLayerMask))
                {
                    visiblePoints++;
                }
                
                if (debugDrawRays)
                {
                    Debug.DrawRay(observer.position, direction, 
                        visiblePoints > 0 ? Color.green : Color.red, 0.1f);
                }
            }
            
            return (float)visiblePoints / checkPoints.Length;
        }
        
        private Vector3[] GetTargetCheckPoints(Transform target)
        {
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null)
            {
                return new Vector3[] { target.position };
            }
            
            Bounds bounds = targetCollider.bounds;
            return new Vector3[]
            {
                bounds.center,
                bounds.center + Vector3.up * bounds.extents.y,
                bounds.center - Vector3.up * bounds.extents.y,
                bounds.center + Vector3.right * bounds.extents.x,
                bounds.center - Vector3.right * bounds.extents.x
            };
        }
        
        private float CalculateLightFactor(Vector3 position)
        {
            float totalLight = 0f;
            int validSamples = 0;
            
            for (int i = 0; i < lightSampleCount; i++)
            {
                float angle = (360f / lightSampleCount) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 0.5f, direction, 
                    out hit, config.lightCheckDistance, obstacleLayerMask))
                {
                    float distance = hit.distance;
                    float lightContribution = 1f - (distance / config.lightCheckDistance);
                    totalLight += lightContribution;
                    validSamples++;
                }
            }
            
            if (validSamples > 0)
            {
                float averageLight = totalLight / validSamples;
                return config.lightVisibilityCurve.Evaluate(averageLight);
            }
            
            return config.defaultLightLevel;
        }
        
        /// <summary>
        /// 音響による検知レベルを計算します
        /// </summary>
        /// <param name="noiseSource">音源の位置</param>
        /// <param name="observer">観察者のTransform</param>
        /// <param name="noiseLevel">音の強度レベル（0.0f～1.0f）</param>
        /// <returns>0.0f（聞こえない）から1.0f（完全に聞こえる）までの検知レベル</returns>
        /// <remarks>
        /// 計算要素：
        /// 1. 距離減衰：音源からの距離に応じた音量減衰
        /// 2. 遮蔽効果：壁などの障害物による音の減衰
        /// 3. 音源強度：入力されたnoiseLevelによる基準音量
        /// 
        /// 遮蔽計算：
        /// - 直接経路：音が障害物に遮られていない場合
        /// - 間接経路：壁越しの場合、noiseThroughWallMultiplierを適用
        /// 
        /// ゲームプレイへの活用：
        /// - 足音による発見システム
        /// - 物音による警戒レベル上昇
        /// - 環境音によるマスキング効果
        /// - 戦術的な音を利用した陽動
        /// 
        /// 設定パラメータ：
        /// - maxNoiseDetectionRange：基準検知距離
        /// - noiseThroughWallMultiplier：壁越し減衰率
        /// 
        /// 使用例：
        /// <code>
        /// float noiseDetection = calculator.CalculateNoiseDetection(
        ///     playerPos, guard, playerMovementNoise);
        /// if (noiseDetection > 0.3f) 
        /// {
        ///     ai.InvestigateSound(playerPos, noiseDetection);
        /// }
        /// </code>
        /// </remarks>
        public float CalculateNoiseDetection(Vector3 noiseSource, Transform observer, 
            float noiseLevel)
        {
            if (observer == null) return 0f;
            
            float distance = Vector3.Distance(noiseSource, observer.position);
            float maxNoiseRange = config.maxNoiseDetectionRange * noiseLevel;
            
            if (distance > maxNoiseRange)
                return 0f;
                
            float detectionLevel = 1f - (distance / maxNoiseRange);
            
            float obstructionFactor = 1f;
            if (Physics.Linecast(noiseSource, observer.position, obstacleLayerMask))
            {
                obstructionFactor = config.noiseThroughWallMultiplier;
            }
            
            return detectionLevel * obstructionFactor;
        }
        
        /// <summary>
        /// 指定された位置が観察者の視野角内にあるかどうかを判定します
        /// </summary>
        /// <param name="position">判定したい位置</param>
        /// <param name="observer">観察者のTransform</param>
        /// <returns>視野角内の場合true、外の場合false</returns>
        /// <remarks>
        /// 判定方法：
        /// 観察者の前方向と目標位置への方向のなす角度を計算し、
        /// 設定されたfieldOfViewの半分以下かどうかで判定します。
        /// 
        /// 計算の詳細：
        /// - Vector3.Angleを使用した正確な角度計算
        /// - fieldOfView / 2fとの比較（視野角は通常全体角度で設定）
        /// - nullチェックによる安全性確保
        /// 
        /// 使用場面：
        /// - AIの視認判定の前段階チェック
        /// - UI要素の表示/非表示制御
        /// - パフォーマンス最適化（角度チェック後に詳細計算）
        /// - デバッグ・可視化システム
        /// 
        /// 注意事項：
        /// この関数は角度のみを判定し、障害物による視線遮蔽は考慮しません。
        /// 完全な視認判定にはHasLineOfSightと組み合わせて使用してください。
        /// </remarks>
        public bool IsInFieldOfView(Vector3 position, Transform observer)
        {
            if (observer == null) return false;
            
            Vector3 direction = (position - observer.position).normalized;
            float angle = Vector3.Angle(observer.forward, direction);
            
            return angle <= config.fieldOfView / 2f;
        }
        
        /// <summary>
        /// 観察者から指定位置への視線が通っているかどうかを判定します
        /// </summary>
        /// <param name="position">視線の目標位置</param>
        /// <param name="observer">観察者のTransform</param>
        /// <returns>視線が通っている場合true、遮られている場合false</returns>
        /// <remarks>
        /// 判定方法：
        /// Physics.Raycastを使用して、観察者位置から目標位置への直線上に
        /// 障害物レイヤーのオブジェクトが存在するかどうかを判定します。
        /// 
        /// 技術的詳細：
        /// - obstacleLayerMaskで指定されたレイヤーのみを判定対象とする
        /// - 距離は自動計算（direction.magnitude）
        /// - 戻り値は!Physics.Raycast（遮蔽なし = true）
        /// 
        /// パフォーマンス考慮：
        /// - 単一のRaycast呼び出しで高速判定
        /// - レイヤーマスクによる不要オブジェクトの除外
        /// - 距離制限なし（呼び出し側で制御）
        /// 
        /// 使用場面：
        /// - 視認性計算の基礎判定
        /// - UIでの視線表示システム
        /// - AIの行動判定（攻撃可能性等）
        /// - プレイヤーの隠蔽状況確認
        /// 
        /// 制限事項：
        /// - 単純な直線判定（複雑な形状の考慮なし）
        /// - 部分的な遮蔽は未対応（完全遮蔽のみ）
        /// - 動的障害物の移動は瞬時反映
        /// 
        /// より高度な判定が必要な場合は、CalculateVisibilityメソッドを使用してください。
        /// </remarks>
        public bool HasLineOfSight(Vector3 position, Transform observer)
        {
            if (observer == null) return false;
            
            Vector3 direction = position - observer.position;
            return !Physics.Raycast(observer.position, direction.normalized, 
                direction.magnitude, obstacleLayerMask);
        }
    }
}
