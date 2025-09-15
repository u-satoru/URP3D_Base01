using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルスAI設定
    /// NPCの検知システム、学習機能、パフォーマンス設定
    /// 50体NPC同時稼働、0.1ms/frame要件対応
    /// </summary>
    [System.Serializable]
    public class StealthAIConfig
    {
        [Header("Detection Integration")]
        [Tooltip("NPCの視覚センサー設定")]
        public NPCVisualSensorSettings VisualSensorSettings;

        [Tooltip("NPCの聴覚センサー設定")]
        public NPCAuditorySensorSettings AuditorySensorSettings;

        [Header("Alert Level Progression")]
        [Range(0f, 1f)]
        [Tooltip("疑念状態に移行する閾値")]
        public float SuspiciousThreshold = 0.3f;

        [Range(0f, 1f)]
        [Tooltip("調査状態に移行する閾値")]
        public float InvestigatingThreshold = 0.7f;

        [Range(0f, 1f)]
        [Tooltip("警戒状態に移行する閾値")]
        public float AlertThreshold = 0.9f;

        [Header("Memory & Learning")]
        [Range(5f, 60f)]
        [Tooltip("記憶保持時間（秒）")]
        public float MemoryRetentionTime = 30f;

        [Range(1f, 10f)]
        [Tooltip("学習速度倍率")]
        public float LearningRate = 2f;

        [Tooltip("協調検出機能の有効化")]
        public bool EnableCooperativeDetection = true;

        [Header("Performance Scaling")]
        [Range(1, 100)]
        [Tooltip("最大同時稼働NPC数")]
        public int MaxSimultaneousNPCs = 50;

        [Range(0.01f, 0.1f)]
        [Tooltip("フレーム当たりの最大処理時間（ms）")]
        public float MaxFrameTimeMs = 0.1f;

        [Header("Detection Types")]
        [Tooltip("視覚検出の有効化")]
        public bool EnableVisualDetection = true;

        [Tooltip("聴覚検出の有効化")]
        public bool EnableAuditoryDetection = true;

        [Tooltip("協調AIによる情報共有の有効化")]
        public bool EnableInformationSharing = true;

        [Header("Learn & Grow Settings")]
        [Tooltip("初心者向け検出猶予時間")]
        [Range(0f, 5f)]
        public float BeginnerDetectionDelay = 1.5f;

        [Tooltip("チュートリアルモード（検出感度低下）")]
        public bool TutorialMode = false;

        [Range(0.1f, 1f)]
        [Tooltip("チュートリアルモード時の検出感度倍率")]
        public float TutorialDetectionSensitivity = 0.6f;

        /// <summary>
        /// デフォルト設定の適用
        /// パフォーマンス要件と学習支援を両立
        /// </summary>
        public void ApplyDefaultSettings()
        {
            SuspiciousThreshold = 0.3f;
            InvestigatingThreshold = 0.7f;
            AlertThreshold = 0.9f;
            MemoryRetentionTime = 30f;
            LearningRate = 2f;
            EnableCooperativeDetection = true;
            MaxSimultaneousNPCs = 50;
            MaxFrameTimeMs = 0.1f;
            EnableVisualDetection = true;
            EnableAuditoryDetection = true;
            EnableInformationSharing = true;
            BeginnerDetectionDelay = 1.5f;
            TutorialMode = false;
            TutorialDetectionSensitivity = 0.6f;
        }

        /// <summary>
        /// 高パフォーマンス設定
        /// より多くのNPCとより高速な処理
        /// </summary>
        public void ApplyHighPerformanceSettings()
        {
            MaxSimultaneousNPCs = 75; // 上限を上げる
            MaxFrameTimeMs = 0.05f; // より厳しい時間制限
            EnableCooperativeDetection = false; // 処理負荷軽減
            EnableInformationSharing = false;
            LearningRate = 1f; // 学習頻度を下げる
            MemoryRetentionTime = 15f; // メモリ使用量削減
        }

        /// <summary>
        /// 学習支援設定
        /// Learn & Grow価値実現のための設定
        /// </summary>
        public void ApplyLearningSupportSettings()
        {
            TutorialMode = true;
            TutorialDetectionSensitivity = 0.6f;
            BeginnerDetectionDelay = 2f;
            SuspiciousThreshold = 0.4f; // やや高めに設定
            InvestigatingThreshold = 0.8f;
            AlertThreshold = 0.95f;
            MemoryRetentionTime = 20f; // 少し短めで失敗から回復しやすく
        }

        /// <summary>
        /// 上級者向け設定
        /// より厳しい検出とリアリスティックな反応
        /// </summary>
        public void ApplyExpertSettings()
        {
            TutorialMode = false;
            BeginnerDetectionDelay = 0f;
            SuspiciousThreshold = 0.2f; // より敏感
            InvestigatingThreshold = 0.5f;
            AlertThreshold = 0.8f;
            MemoryRetentionTime = 45f; // より長い記憶
            LearningRate = 3f; // より高速学習
            EnableCooperativeDetection = true;
            EnableInformationSharing = true;
        }

        /// <summary>
        /// 設定の検証
        /// パフォーマンス要件との整合性確認
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // 閾値の論理的順序確認
            if (SuspiciousThreshold >= InvestigatingThreshold)
            {
                Debug.LogWarning("StealthAIConfig: SuspiciousThreshold should be less than InvestigatingThreshold");
                isValid = false;
            }

            if (InvestigatingThreshold >= AlertThreshold)
            {
                Debug.LogWarning("StealthAIConfig: InvestigatingThreshold should be less than AlertThreshold");
                isValid = false;
            }

            // パフォーマンス制限確認
            if (MaxSimultaneousNPCs > 100)
            {
                Debug.LogWarning("StealthAIConfig: MaxSimultaneousNPCs exceeds recommended maximum (100)");
                isValid = false;
            }

            if (MaxFrameTimeMs > 0.2f)
            {
                Debug.LogWarning("StealthAIConfig: MaxFrameTimeMs exceeds performance target (0.2ms)");
                isValid = false;
            }

            return isValid;
        }
    }

    /// <summary>
    /// NPCの視覚センサー設定
    /// 既存のNPCVisualSensorとの統合用
    /// </summary>
    [System.Serializable]
    public class NPCVisualSensorSettings
    {
        [Range(1f, 20f)]
        public float DetectionRange = 10f;

        [Range(30f, 180f)]
        public float FieldOfView = 90f;

        [Range(0.1f, 2f)]
        public float DetectionSensitivity = 1f;

        public LayerMask DetectionLayers = -1;
    }

    /// <summary>
    /// NPCの聴覚センサー設定
    /// 既存のNPCAuditorySensorとの統合用
    /// </summary>
    [System.Serializable]
    public class NPCAuditorySensorSettings
    {
        [Range(1f, 20f)]
        public float HearingRange = 8f;

        [Range(0.1f, 2f)]
        public float HearingSensitivity = 1f;

        [Range(0f, 1f)]
        public float NoiseThreshold = 0.3f;

        public LayerMask SoundLayers = -1;
    }
}