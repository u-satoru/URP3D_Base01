using UnityEngine;
using asterivo.Unity60.Core.Data;

namespace asterivo.Unity60.Features.Templates.Stealth.Data
{
    /// <summary>
    /// ステルステンプレート専用型定義
    /// コンパイルエラー解消のための統合型定義ファイル
    /// </summary>

    /// <summary>
    /// AI警戒レベル（ステルステンプレート専用）
    /// 既存のAlertLevel型との競合を回避
    /// </summary>
    public enum AIAlertLevel
    {
        Unaware = 0,      // 認識していない
        Suspicious = 1,   // 疑念
        Investigating = 2, // 調査中
        Searching = 3,    // 捜索中
        Alert = 4,        // 警戒
        Combat = 5        // 戦闘
    }

    /// <summary>
    /// ノイズゾーン種別
    /// 環境音響システム用
    /// </summary>
    public enum NoiseZoneType
    {
        Silent = 0,       // 静寂ゾーン
        Ambient = 1,      // 環境音ゾーン
        Machinery = 2,    // 機械音ゾーン
        WaterSource = 3,  // 水音ゾーン
        WindZone = 4,     // 風音ゾーン
        CrowdNoise = 5    // 群衆音ゾーン
    }

    /// <summary>
    /// オーディオエフェクト種別
    /// ステルスオーディオシステム用
    /// </summary>
    public enum AudioEffectType
    {
        None = 0,            // エフェクトなし
        Reverb = 1,          // リバーブ
        LowPass = 2,         // ローパスフィルタ
        HighPass = 3,        // ハイパスフィルタ
        Distortion = 4,      // ディストーション
        Echo = 5,            // エコー
        Masking = 6,         // マスキング
        DynamicRange = 7,    // ダイナミックレンジ圧縮
        Footstep = 8,        // 足音
        Movement = 9,        // 移動音
        Voice = 10           // 音声
    }

    /// <summary>
    /// プレイヤー移動状態
    /// ステルスメカニクス用
    /// </summary>
    public enum PlayerMovementState
    {
        Idle = 0,         // 待機
        Walking = 1,      // 歩行
        Running = 2,      // 走行
        Crouching = 3,    // しゃがみ
        Prone = 4,        // 伏せ
        Climbing = 5,     // 登攀
        Swimming = 6,     // 水泳
        Hiding = 7        // 隠れ中
    }

    /// <summary>
    /// ステルスUI要素種別
    /// UI管理用
    /// </summary>
    public enum StealthUIElement
    {
        DetectionMeter = 0,    // 検知メーター
        NoiseIndicator = 1,    // ノイズインジケータ
        AlertStatus = 2,       // 警戒状況表示
        Minimap = 3,          // ミニマップ
        InteractionPrompt = 4, // インタラクションプロンプト
        StealthTutorial = 5,   // ステルスチュートリアル
        ProgressHUD = 6,       // 進捗HUD
        StealthIndicator = 7   // ステルスインジケーター
    }

    /// <summary>
    /// ミニマップオブジェクト種別
    /// マップ表示用
    /// </summary>
    public enum MinimapObjectType
    {
        Player = 0,           // プレイヤー
        Enemy = 1,            // 敵
        NPC = 2,             // NPC
        Objective = 3,        // 目標
        HidingSpot = 4,      // 隠れ場所
        NoiseSource = 5,     // ノイズ発生源
        InteractableItem = 6, // インタラクト可能アイテム
        ExitPoint = 7         // 脱出ポイント
    }

    /// <summary>
    /// チュートリアルステップ種別
    /// Learn & Grow統合用
    /// </summary>
    public enum TutorialStepType
    {
        Basic = 0,            // 基本操作
        Movement = 1,         // 移動
        Stealth = 2,          // ステルス
        Interaction = 3,      // インタラクション
        AudioCues = 4,        // 音響手がかり
        Advanced = 5,         // 上級技術
        Combat = 6,           // 戦闘回避
        Completion = 7        // 完了
    }

    /// <summary>
    /// ステルス検知データ
    /// AI検知システム用
    /// </summary>
    [System.Serializable]
    public struct StealthDetectionData
    {
        public string DetectorId;
        public AIAlertLevel AlertLevel;
        public float SuspicionLevel;
        public Vector3 LastKnownPosition;
        public float DetectionTime;
        public asterivo.Unity60.Core.Data.DetectionType DetectionType;
        public bool IsPlayerVisible;
        public float NoiseLevel;
        public float DistanceToPlayer;

        public static StealthDetectionData Create(string detectorId, AIAlertLevel alertLevel, float suspicion, Vector3 lastPos)
        {
            return new StealthDetectionData
            {
                DetectorId = detectorId,
                AlertLevel = alertLevel,
                SuspicionLevel = suspicion,
                LastKnownPosition = lastPos,
                DetectionTime = Time.time,
                DetectionType = asterivo.Unity60.Core.Data.DetectionType.Visual,
                IsPlayerVisible = false,
                NoiseLevel = 0f,
                DistanceToPlayer = 0f
            };
        }
    }

}