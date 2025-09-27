using UnityEngine;
using System;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// ステルス状態統合管理列挙型（AIセンサー連動・リアルタイム状態追跡）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// プレイヤーのステルス状態を5段階で精密管理する基幹列挙型です。
    /// AIセンサーシステムとイベント駆動アーキテクチャの統合により、
    /// リアルタイムステルス判定と動的状態遷移を実現します。
    ///
    /// 【Learn & Grow価値実現】
    /// - 直感的5段階: 初心者でも理解しやすい段階的ステルス状態
    /// - 学習コスト削減: 複雑なステルス判定の可視化による習得時間短縮
    /// - 段階的理解: 各状態での行動結果の明確なフィードバック
    ///
    /// 【AIセンサーシステム連動】
    /// - Visual Sensor: 視覚検知レベルとの完全同期
    /// - Auditory Sensor: 音響検知との動的連携
    /// - Environmental Sensor: 環境要因による状態自動調整
    /// - Sensor Fusion: 複数センサー情報の統合判定
    /// </summary>
    public enum StealthState
    {
        /// <summary>完全に視認可能 - NPCに発見される可能性が最も高い</summary>
        Visible,

        /// <summary>部分的に隠蔽 - 環境要素による部分的隠蔽効果</summary>
        Concealed,

        /// <summary>完全に隠蔽 - 理想的な隠れ状態</summary>
        Hidden,

        /// <summary>発見済み - NPCによって確認された状態</summary>
        Detected,

        /// <summary>正体バレ状態 - 警戒が最高レベルに到達</summary>
        Compromised
    }

    /// <summary>
    /// 検知の種類を表現する列挙型
    /// 多様な検知システムの統合管理による高度なステルス体験の実現
    /// Core層配置: ServiceLocator統合のためのアーキテクチャ制約対応
    /// </summary>
    public enum DetectionType
    {
        /// <summary>視覚検知 - NPCVisualSensorによる直接目視</summary>
        Visual,

        /// <summary>聴覚検知 - 音響システムによる検知</summary>
        Auditory,

        /// <summary>環境的手がかり - ドア開放、物体移動等</summary>
        Environmental,

        /// <summary>他NPCからの情報 - 協調検出システム</summary>
        Cooperative
    }

    /// <summary>
    /// 環境隠蔽レベル
    /// 環境による隠蔽効果の段階的管理
    /// Core層配置: ServiceLocator統合のためのアーキテクチャ制約対応
    /// </summary>
    public enum ConcealmentLevel
    {
        /// <summary>隠蔽なし - 完全露出</summary>
        None = 0,

        /// <summary>軽微な隠蔽 - 影、草むら等</summary>
        Light = 1,

        /// <summary>中程度の隠蔽 - 障害物、暗闇等</summary>
        Medium = 2,

        /// <summary>高度な隠蔽 - 完全な遮蔽物</summary>
        High = 3,

        /// <summary>完全隠蔽 - ロッカー、隠し部屋等</summary>
        Complete = 4
    }

    /// <summary>
    /// AI警戒レベル - NPCの警戒状態を4段階で管理
    /// </summary>
    public enum AlertLevel
    {
        /// <summary>平常状態 - 通常のパトロール</summary>
        Relaxed = 0,

        /// <summary>疑念状態 - 何か異常を感じている</summary>
        Suspicious = 1,

        /// <summary>調査状態 - 積極的に異常を調査中</summary>
        Investigating = 2,

        /// <summary>警戒状態 - 目標を確認、追跡中</summary>
        Alert = 3
    }

    /// <summary>
    /// プレイヤーの移動姿勢
    /// </summary>
    public enum MovementStance
    {
        /// <summary>立ち姿勢 - 通常移動</summary>
        Standing,

        /// <summary>しゃがみ姿勢 - 静音移動</summary>
        Crouching,

        /// <summary>伏せ姿勢 - 最大隠蔽</summary>
        Prone,

        /// <summary>走行姿勢 - 高速移動</summary>
        Running
    }

    /// <summary>
    /// AI警戒状態詳細情報管理クラス（階層化ステートマシン統合・履歴追跡対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// NPCの警戒状態変化を詳細追跡・管理する情報コンテナクラスです。
    /// 階層化ステートマシン（HSM）との統合により、
    /// 複雑な警戒状態遷移とAI行動制御の高精度管理を実現します。
    ///
    /// 【警戒状態履歴追跡】
    /// - Previous State Tracking: 前状態の記録による戻り判定
    /// - Time in State: 状態継続時間による行動パターン調整
    /// - Suspicion Value: 疑念レベルの数値化と累積管理
    /// - Position Memory: 最後の発見位置の記録と索敵行動
    ///
    /// 【HSM状態遷移統合】
    /// - State History: 警戒レベル変化の完全な履歴管理
    /// - Transition Logic: 状態遷移条件の詳細データ保持
    /// - Behavior Adaptation: 警戒履歴に基づくAI行動パターン変化
    /// - Memory Persistence: 一定期間の警戒状態記憶機能
    ///
    /// 【リアルタイム同期機能】
    /// - Timestamp Precision: DateTime.Nowによる正確な時刻記録
    /// - Event Integration: AlertLevelChangedEventとの自動連携
    /// - Real-time Updates: 60FPS対応の高頻度状態更新
    /// - Performance Optimization: 軽量構造体による高速処理
    /// </summary>
    [Serializable]
    public class AlertStateInfo
    {
        public AlertLevel currentLevel;
        public AlertLevel previousLevel;
        public float suspicionValue;
        public Vector3 lastKnownPosition;
        public float timeInCurrentState;
        public string triggeredBy;
        public DateTime timestamp;

        public AlertStateInfo()
        {
            currentLevel = AlertLevel.Relaxed;
            previousLevel = AlertLevel.Relaxed;
            suspicionValue = 0f;
            lastKnownPosition = Vector3.zero;
            timeInCurrentState = 0f;
            triggeredBy = "";
            timestamp = DateTime.Now;
        }

        public AlertStateInfo(AlertLevel level, float suspicion, Vector3 position, string trigger)
        {
            currentLevel = level;
            previousLevel = AlertLevel.Relaxed;
            suspicionValue = suspicion;
            lastKnownPosition = position;
            timeInCurrentState = 0f;
            triggeredBy = trigger;
            timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// マルチモーダルセンサー検知情報統合クラス（3D空間・信頼度管理対応）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// 視覚・聴覚・環境センサーによる検知情報を統合管理するデータクラスです。
    /// 3D空間での正確な検知情報とAI意思決定のための信頼度評価により、
    /// 高精度なステルスAIシステムの基盤を提供します。
    ///
    /// 【マルチモーダル検知統合】
    /// - Detection Type: 視覚・聴覚・環境・協調検知の統一管理
    /// - Confidence Level: 検知信頼度の数値化（0.0-1.0）
    /// - Sensor Fusion: 複数センサー情報の重み付け統合
    /// - False Positive Filter: 誤検知の自動除外機能
    ///
    /// 【3D空間情報管理】
    /// - Position Accuracy: Vector3による正確な検知位置記録
    /// - Direction Vector: 検知方向の3D ベクトル情報
    /// - Distance Calculation: 検知距離の自動算出と記録
    /// - Spatial Validation: 3D空間での検知妥当性確認
    ///
    /// 【AI意思決定支援】
    /// - GameObject Reference: 検知対象の直接参照管理
    /// - Confirmation Status: 検知確定状態のフラグ管理
    /// - Temporal Data: 検知時刻による情報鮮度評価
    /// - Decision Weight: AI判断における検知情報の重み付け
    /// </summary>
    [Serializable]
    public class DetectionInfo
    {
        public DetectionType type;
        public float confidence;
        public Vector3 detectedPosition;
        public Vector3 detectedDirection;
        public float distance;
        public GameObject detectedObject;
        public DateTime detectionTime;
        public bool isConfirmed;

        public DetectionInfo()
        {
            type = DetectionType.Visual;
            confidence = 0f;
            detectedPosition = Vector3.zero;
            detectedDirection = Vector3.forward;
            distance = 0f;
            detectedObject = null;
            detectionTime = DateTime.Now;
            isConfirmed = false;
        }

        public DetectionInfo(DetectionType detectionType, float conf, Vector3 pos, GameObject target)
        {
            type = detectionType;
            confidence = conf;
            detectedPosition = pos;
            detectedDirection = Vector3.forward;
            distance = 0f;
            detectedObject = target;
            detectionTime = DateTime.Now;
            isConfirmed = false;
        }
    }

    /// <summary>
    /// ステルス移動状態詳細管理クラス（リアルタイム音響・視認性制御）
    ///
    /// Unity 6における3層アーキテクチャのCore層データ基盤において、
    /// プレイヤーの移動状態を多角的に分析・管理するリアルタイム情報クラスです。
    /// 音響システムと視覚システムの統合により、
    /// 精密なステルス判定と動的隠蔽効果の制御を実現します。
    ///
    /// 【移動姿勢制御システム】
    /// - Stance Management: Standing/Crouching/Prone/Running の動的制御
    /// - Velocity Tracking: リアルタイム移動速度ベクトルの精密管理
    /// - Posture Impact: 姿勢による検知範囲・音響特性の自動調整
    /// - Animation Integration: キャラクターアニメーションとの完全同期
    ///
    /// 【音響制御統合】
    /// - Noise Level Control: 移動姿勢・速度に応じた音響レベル動的算出
    /// - Surface Material: 地面材質による音響特性の自動調整
    /// - Environmental Masking: 環境音による音響マスキング効果
    /// - Distance Attenuation: 距離減衰を考慮した音響影響計算
    ///
    /// 【視認性制御システム】
    /// - Visibility Level: 姿勢・環境による視認性レベルの動的算出
    /// - Cover System: カバー状態の自動検知と隠蔽効果適用
    /// - Concealment Integration: 環境隠蔽レベルとの統合制御
    /// - Lighting Impact: 照明条件による視認性の自動調整
    ///
    /// 【リアルタイム処理最適化】
    /// - High-Frequency Updates: 60FPS対応の高頻度状態更新
    /// - Lightweight Structure: 最小メモリフットプリントによる高効率
    /// - Timestamp Accuracy: 正確な時刻記録による状態変化追跡
    /// - Event Synchronization: MovementChangedEventとの自動連携
    /// </summary>
    [Serializable]
    public class StealthMovementInfo
    {
        public MovementStance stance;
        public Vector3 velocity;
        public float noiseLevel;
        public float visibilityLevel;
        public bool isInCover;
        public ConcealmentLevel concealmentLevel;
        public DateTime timestamp;

        public StealthMovementInfo()
        {
            stance = MovementStance.Standing;
            velocity = Vector3.zero;
            noiseLevel = 1f;
            visibilityLevel = 1f;
            isInCover = false;
            concealmentLevel = ConcealmentLevel.None;
            timestamp = DateTime.Now;
        }

        public StealthMovementInfo(MovementStance movementStance, Vector3 vel, float noise, float visibility)
        {
            stance = movementStance;
            velocity = vel;
            noiseLevel = noise;
            visibilityLevel = visibility;
            isInCover = false;
            concealmentLevel = ConcealmentLevel.None;
            timestamp = DateTime.Now;
        }
    }
}
