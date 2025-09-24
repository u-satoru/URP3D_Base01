using UnityEngine;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Stealth.Environment;
using asterivo.Unity60.Features.Templates.Stealth.Services;

namespace asterivo.Unity60.Features.Templates.Stealth.Events
{
    /// <summary>
    /// ステルス検出データ構造
    /// AI検出、プレイヤー行動、環境相互作用の情報を統合
    /// </summary>
    [System.Serializable]
    public struct StealthDetectionData
    {
        /// <summary>検出を行ったAIまたは関連オブジェクト</summary>
        public GameObject Detector;

        /// <summary>検出が発生した位置</summary>
        public Vector3 Position;

        /// <summary>疑心レベル (0.0 - 1.0)</summary>
        public float SuspicionLevel;

        /// <summary>音響レベル (0.0 - 1.0)</summary>
        public float NoiseLevel;

        /// <summary>視認性係数 (0.0 - 1.0)</summary>
        public float VisibilityFactor;

        /// <summary>検出の種類</summary>
        public StealthDetectionType DetectionType;

        /// <summary>検出が発生した時刻</summary>
        public float Timestamp;

        /// <summary>追加の検出情報（オプション）</summary>
        public string AdditionalInfo;
    }

    /// <summary>
    /// ステルス検出の種類
    /// </summary>
    public enum StealthDetectionType
    {
        /// <summary>疑心レベルの変化</summary>
        SuspicionChange,
        /// <summary>プレイヤーが発見された</summary>
        Spotted,
        /// <summary>プレイヤーを見失った</summary>
        Lost,
        /// <summary>陽動音の検出</summary>
        Distraction,
        /// <summary>足音の検出</summary>
        Footstep,
        /// <summary>物体の破損音</summary>
        ObjectBreak,
        /// <summary>死体の発見</summary>
        BodyFound,
        /// <summary>不審なドアの開閉</summary>
        SuspiciousDoor,
        /// <summary>光の変化</summary>
        LightChange
    }

    /// <summary>
    /// プレイヤー隠蔽状態変化イベントデータ
    /// </summary>
    [System.Serializable]
    public struct PlayerConcealmentData
    {
        /// <summary>隠蔽ゾーンの種類</summary>
        public ConcealmentType ZoneType;

        /// <summary>隠蔽効果の強度 (0.0 - 1.0)</summary>
        public float ConcealmentStrength;

        /// <summary>隠蔽ゾーンに入ったかどうか</summary>
        public bool IsEntering;

        /// <summary>隠蔽ゾーンの位置</summary>
        public Vector3 ZonePosition;

        /// <summary>隠蔽ゾーンのサイズ</summary>
        public Vector3 ZoneSize;
    }

    /// <summary>
    /// ステルス隠蔽イベントデータ (ConcealmentManagerとの互換性用)
    /// </summary>
    [System.Serializable]
    public struct StealthConcealmentEventData
    {
        /// <summary>隠蔽ゾーンの種類</summary>
        public ConcealmentType ConcealmentType;

        /// <summary>隠蔽効果の強度 (0.0 - 1.0)</summary>
        public float ConcealmentStrength;

        /// <summary>隠蔽ゾーンに入ったかどうか</summary>
        public bool IsEntering;

        /// <summary>イベントが発生した位置</summary>
        public Vector3 Position;
    }

    /// <summary>
    /// 環境相互作用イベントデータ
    /// </summary>
    [System.Serializable]
    public struct EnvironmentalInteractionData
    {
        /// <summary>相互作用対象オブジェクト</summary>
        public GameObject TargetObject;

        /// <summary>相互作用の種類</summary>
        public StealthInteractionType InteractionType;

        /// <summary>相互作用が成功したかどうか</summary>
        public bool Success;

        /// <summary>相互作用が発生した位置</summary>
        public Vector3 Position;

        /// <summary>相互作用によって発生した音のレベル</summary>
        public float GeneratedNoiseLevel;

        /// <summary>相互作用の実行時刻</summary>
        public float Timestamp;
    }

    /// <summary>
    /// ステルス統計更新イベントデータ
    /// </summary>
    [System.Serializable]
    public struct StealthStatisticsData
    {
        /// <summary>現在の統計情報</summary>
        public StealthStatistics Statistics;

        /// <summary>前回の統計情報</summary>
        public StealthStatistics PreviousStatistics;

        /// <summary>統計が更新された理由</summary>
        public string UpdateReason;
    }

    // GameEventの定義
    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Detection Event", fileName = "StealthDetectionEvent")]
    public class StealthDetectionEvent : GameEvent<StealthDetectionData> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Player Visibility Event", fileName = "PlayerVisibilityEvent")]
    public class PlayerVisibilityEvent : GameEvent<float> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Player Noise Event", fileName = "PlayerNoiseEvent")]
    public class PlayerNoiseEvent : GameEvent<float> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Player Concealment Event", fileName = "PlayerConcealmentEvent")]
    public class PlayerConcealmentEvent : GameEvent<PlayerConcealmentData> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Stealth Concealment Event", fileName = "StealthConcealmentEvent")]
    public class StealthConcealmentEvent : GameEvent<StealthConcealmentEventData> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Environmental Interaction Event", fileName = "EnvironmentalInteractionEvent")]
    public class EnvironmentalInteractionEvent : GameEvent<EnvironmentalInteractionData> { }

    [CreateAssetMenu(menuName = "asterivo/Events/Stealth/Statistics Update Event", fileName = "StealthStatisticsEvent")]
    public class StealthStatisticsEvent : GameEvent<StealthStatisticsData> { }

    /// <summary>
    /// ステルスシステムの静的イベントアクセスポイント
    /// ScriptableObjectベースのイベントシステムへの統一アクセスを提供
    /// </summary>
    public static class StealthEvents
    {
        private static StealthConcealmentEvent _onConcealmentChanged;
        private static StealthDetectionEvent _onDetectionChanged;
        private static PlayerVisibilityEvent _onVisibilityChanged;
        private static PlayerNoiseEvent _onNoiseChanged;
        private static EnvironmentalInteractionEvent _onEnvironmentalInteraction;
        private static StealthStatisticsEvent _onStatisticsUpdated;

        /// <summary>
        /// プレイヤーの隠蔽状態変化イベント
        /// </summary>
        public static StealthConcealmentEvent OnConcealmentChanged
        {
            get
            {
                if (_onConcealmentChanged == null)
                {
                    _onConcealmentChanged = ScriptableObject.CreateInstance<StealthConcealmentEvent>();
                }
                return _onConcealmentChanged;
            }
        }

        /// <summary>
        /// AI検出状態変化イベント
        /// </summary>
        public static StealthDetectionEvent OnDetectionChanged
        {
            get
            {
                if (_onDetectionChanged == null)
                {
                    _onDetectionChanged = ScriptableObject.CreateInstance<StealthDetectionEvent>();
                }
                return _onDetectionChanged;
            }
        }

        /// <summary>
        /// プレイヤー視認性変化イベント
        /// </summary>
        public static PlayerVisibilityEvent OnVisibilityChanged
        {
            get
            {
                if (_onVisibilityChanged == null)
                {
                    _onVisibilityChanged = ScriptableObject.CreateInstance<PlayerVisibilityEvent>();
                }
                return _onVisibilityChanged;
            }
        }

        /// <summary>
        /// プレイヤー音響レベル変化イベント
        /// </summary>
        public static PlayerNoiseEvent OnNoiseChanged
        {
            get
            {
                if (_onNoiseChanged == null)
                {
                    _onNoiseChanged = ScriptableObject.CreateInstance<PlayerNoiseEvent>();
                }
                return _onNoiseChanged;
            }
        }

        /// <summary>
        /// 環境相互作用イベント
        /// </summary>
        public static EnvironmentalInteractionEvent OnEnvironmentalInteraction
        {
            get
            {
                if (_onEnvironmentalInteraction == null)
                {
                    _onEnvironmentalInteraction = ScriptableObject.CreateInstance<EnvironmentalInteractionEvent>();
                }
                return _onEnvironmentalInteraction;
            }
        }

        /// <summary>
        /// ステルス統計更新イベント
        /// </summary>
        public static StealthStatisticsEvent OnStatisticsUpdated
        {
            get
            {
                if (_onStatisticsUpdated == null)
                {
                    _onStatisticsUpdated = ScriptableObject.CreateInstance<StealthStatisticsEvent>();
                }
                return _onStatisticsUpdated;
            }
        }

        /// <summary>
        /// 全イベントインスタンスをクリーンアップ
        /// </summary>
        public static void Cleanup()
        {
            if (_onConcealmentChanged != null)
            {
                UnityEngine.Object.DestroyImmediate(_onConcealmentChanged);
                _onConcealmentChanged = null;
            }
            if (_onDetectionChanged != null)
            {
                UnityEngine.Object.DestroyImmediate(_onDetectionChanged);
                _onDetectionChanged = null;
            }
            if (_onVisibilityChanged != null)
            {
                UnityEngine.Object.DestroyImmediate(_onVisibilityChanged);
                _onVisibilityChanged = null;
            }
            if (_onNoiseChanged != null)
            {
                UnityEngine.Object.DestroyImmediate(_onNoiseChanged);
                _onNoiseChanged = null;
            }
            if (_onEnvironmentalInteraction != null)
            {
                UnityEngine.Object.DestroyImmediate(_onEnvironmentalInteraction);
                _onEnvironmentalInteraction = null;
            }
            if (_onStatisticsUpdated != null)
            {
                UnityEngine.Object.DestroyImmediate(_onStatisticsUpdated);
                _onStatisticsUpdated = null;
            }
        }
    }
}

namespace asterivo.Unity60.Features.Templates.Stealth.Services
{
    /// <summary>
    /// 他の名前空間の型をServices名前空間で使用するため
    /// </summary>
    using asterivo.Unity60.Features.Templates.Stealth.Environment;
    using asterivo.Unity60.Features.Templates.Stealth.Events;

    /// <summary>
    /// ステルス相互作用の種類（Services名前空間でも使用可能）
    /// </summary>
    public enum StealthInteractionType
    {
        /// <summary>オブジェクトを投擲して陽動</summary>
        ThrowObject,
        /// <summary>死体を隠す</summary>
        HideBody,
        /// <summary>光源を破壊</summary>
        SabotageLight,
        /// <summary>セキュリティカメラを無効化</summary>
        DisableCamera,
        /// <summary>ドアを開閉</summary>
        OperateDoor,
        /// <summary>スイッチを操作</summary>
        OperateSwitch
    }

    /// <summary>
    /// ステルス統計情報（Services名前空間でも使用可能）
    /// </summary>
    [System.Serializable]
    public struct StealthStatistics
    {
        /// <summary>発見された回数</summary>
        public int TimesSpotted;
        /// <summary>隠蔽ゾーン使用回数</summary>
        public int ConcealmentZonesUsed;
        /// <summary>陽動作戦実行回数</summary>
        public int DistrictionsCreated;
        /// <summary>環境相互作用回数</summary>
        public int EnvironmentalInteractions;
        /// <summary>平均視認性係数</summary>
        public float AverageVisibilityFactor;
        /// <summary>総ステルス時間</summary>
        public float TotalStealthTime;
    }
}