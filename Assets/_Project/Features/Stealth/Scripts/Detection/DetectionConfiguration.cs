using UnityEngine;

namespace asterivo.Unity60.Stealth.Detection
{
    /// <summary>
    /// ステルス検出システムのパラメータを定義するScriptableObject設定クラス
    /// </summary>
    /// <remarks>
    /// 設計思想：
    /// このクラスは、複雑なステルス検出システムの全てのパラメータを一元管理し、
    /// デザイナーが Unity Editor 上で直感的にゲームバランスを調整できるようにします。
    /// ScriptableObjectとして実装することで、実行時の変更なしに設定の切り替えが可能です。
    /// 
    /// 主要な設定カテゴリ：
    /// 1. 検出距離：視覚・音響検出の基本距離設定
    /// 2. 視野角：AIの視認範囲と周辺視野の定義
    /// 3. 光検出：照明による視認性への影響
    /// 4. 距離減衰：距離に応じた検出率の変化カーブ
    /// 5. 音響検出：音による検知と壁越し減衰
    /// 6. 検出速度：警戒状態の変化速度
    /// 7. 移動修正：プレイヤーの行動による検出率変化
    /// 
    /// ゲームデザインへの活用：
    /// - 難易度調整：距離や閾値の調整で段階的な難易度設定
    /// - キャラクター差別化：NPCタイプ別の検出設定
    /// - 環境対応：屋内・屋外での設定切り替え
    /// - プレイテスト対応：迅速なパラメータ調整と検証
    /// 
    /// 運用上の利点：
    /// - プログラマー以外でも調整可能
    /// - 設定変更時のリコンパイル不要
    /// - バージョン管理による設定履歴の管理
    /// - 複数の設定プリセットの作成・切り替え
    /// 
    /// パフォーマンス考慮：
    /// - 実行時の設定読み込みは一度のみ
    /// - AnimationCurveによる効率的な非線形計算
    /// - Reset()による初期値の確実な設定
    /// </remarks>
    [CreateAssetMenu(menuName = "asterivo/Stealth/Detection Config", fileName = "DetectionConfig")]
    public class DetectionConfiguration : ScriptableObject
    {
        [Header("Detection Ranges")]
        /// <summary>視覚検出の最大距離（メートル）</summary>
        /// <remarks>この距離を超えた目標は検出対象から除外され、パフォーマンス最適化に貢献します</remarks>
        public float maxDetectionRange = 30f;
        
        /// <summary>音響検出の最大距離（メートル）</summary>
        /// <remarks>音の基準検出距離。実際の検出距離は音の強度によって変動します</remarks>
        public float maxNoiseDetectionRange = 20f;
        
        /// <summary>即座に検出される距離（メートル）</summary>
        /// <remarks>この距離内では角度や遮蔽に関係なく即座に発見されます</remarks>
        public float instantDetectionRange = 3f;
        
        [Header("Field of View")]
        /// <summary>視野角（度）</summary>
        /// <remarks>AIの前方視野角。人間の場合、約110-120度が一般的です</remarks>
        public float fieldOfView = 110f;
        
        /// <summary>周辺視野での検出率乗数</summary>
        /// <remarks>視野角の端付近での検出率低下を表現。0.5 = 50%の検出率</remarks>
        public float peripheralVisionMultiplier = 0.5f;
        
        [Header("Light Detection")]
        /// <summary>光量チェックの距離（メートル）</summary>
        /// <remarks>周囲の光量を調査する際のRaycast距離</remarks>
        public float lightCheckDistance = 10f;
        
        /// <summary>デフォルトの光量レベル（0.0f～1.0f）</summary>
        /// <remarks>光量計算に失敗した場合の fallback 値</remarks>
        public float defaultLightLevel = 0.5f;
        
        /// <summary>光量と視認性の関係を定義するカーブ</summary>
        /// <remarks>X軸: 光量(0-1), Y軸: 視認性倍率(0-1). 暗闇での隠れやすさを調整</remarks>
        public AnimationCurve lightVisibilityCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);
        
        [Header("Distance Falloff")]
        /// <summary>距離による検出率減衰カーブ</summary>
        /// <remarks>X軸: 正規化距離(0-1), Y軸: 検出率(0-1). 距離感による発見されやすさ</remarks>
        public AnimationCurve distanceFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Noise Detection")]
        /// <summary>壁越しでの音響減衰率</summary>
        /// <remarks>0.3 = 壁越しの場合、音が30%まで減衰することを意味します</remarks>
        public float noiseThroughWallMultiplier = 0.3f;
        
        /// <summary>基本聴取距離（メートル）</summary>
        /// <remarks>標準的な音の聞こえる基準距離</remarks>
        public float baseHearingRange = 10f;
        
        [Header("Detection Speed")]
        /// <summary>検出レベル上昇速度（per second）</summary>
        /// <remarks>視認時の警戒度上昇スピード。高いほど素早く発見される</remarks>
        public float detectionBuildUpSpeed = 1f;
        
        /// <summary>検出レベル減少速度（per second）</summary>
        /// <remarks>視界から外れた際の警戒度低下スピード。低いほど長く警戒を続ける</remarks>
        public float detectionDecaySpeed = 0.5f;
        
        /// <summary>疑念状態への遷移閾値（0.0f～1.0f）</summary>
        /// <remarks>この値を超えるとAIが疑念状態に移行し、調査行動を開始</remarks>
        public float suspicionThreshold = 0.3f;
        
        /// <summary>警戒状態への遷移閾値（0.0f～1.0f）</summary>
        /// <remarks>この値を超えるとAIが警戒状態に移行し、積極的な捜索を開始</remarks>
        public float alertThreshold = 0.7f;
        
        [Header("Movement Modifiers")]
        /// <summary>走行時の視認性乗数</summary>
        /// <remarks>1.5 = 走行時は50%発見されやすくなることを意味します</remarks>
        public float runningVisibilityMultiplier = 1.5f;
        
        /// <summary>しゃがみ時の視認性乗数</summary>
        /// <remarks>0.6 = しゃがみ時は40%発見されにくくなることを意味します</remarks>
        public float crouchingVisibilityMultiplier = 0.6f;
        
        /// <summary>伏せ時の視認性乗数</summary>
        /// <remarks>0.3 = 伏せ時は70%発見されにくくなることを意味します</remarks>
        public float proneVisibilityMultiplier = 0.3f;
        
        /// <summary>静止時のボーナス乗数</summary>
        /// <remarks>0.8 = 静止時は20%発見されにくくなることを意味します</remarks>
        public float stillnessBonus = 0.8f;
        
        private void Reset()
        {
            maxDetectionRange = 30f;
            maxNoiseDetectionRange = 20f;
            instantDetectionRange = 3f;
            fieldOfView = 110f;
            peripheralVisionMultiplier = 0.5f;
            lightCheckDistance = 10f;
            defaultLightLevel = 0.5f;
            noiseThroughWallMultiplier = 0.3f;
            baseHearingRange = 10f;
            detectionBuildUpSpeed = 1f;
            detectionDecaySpeed = 0.5f;
            suspicionThreshold = 0.3f;
            alertThreshold = 0.7f;
            
            lightVisibilityCurve = AnimationCurve.Linear(0f, 0.3f, 1f, 1f);
            distanceFalloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }
    }
}
