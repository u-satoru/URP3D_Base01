using UnityEngine;
using System;

namespace asterivo.Unity60.Core.Data
{
    /// <summary>
    /// プレイヤーのステルス状態を表現する列挙型
    /// Learn & Grow価値実現: 直感的な5段階状態管理による学習コスト削減
    /// Core層配置: ServiceLocator統合のためのアーキテクチャ制約対応
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
    /// 警戒状態情報 - AI警戒システムの詳細情報
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
    /// 検知情報 - センサーによる検知の詳細データ
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
    /// ステルス移動情報 - 移動に関する詳細データ
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