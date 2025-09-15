using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth
{
    /// <summary>
    /// Layer 1: ステルスUI設定
    /// HUD要素、インジケーター、フィードバックシステム
    /// Learn & Grow価値実現のためのUI学習支援
    /// </summary>
    [System.Serializable]
    public class StealthUIConfig
    {
        [Header("HUD Elements")]
        [Tooltip("ステルスメーター表示")]
        public bool ShowStealthMeter = true;

        [Tooltip("検知レベルインジケーター表示")]
        public bool ShowDetectionIndicator = true;

        [Tooltip("相互作用プロンプト表示")]
        public bool ShowInteractionPrompts = true;

        [Tooltip("隠蔽状態インジケーター")]
        public bool ShowConcealmentIndicator = true;

        [Header("Visual Feedback")]
        [Tooltip("画面エッジでの警告効果")]
        public bool EnableScreenEdgeWarning = true;

        [Range(0f, 1f)]
        [Tooltip("検知時の画面赤色化強度")]
        public float DetectionAlertIntensity = 0.3f;

        [Range(0.1f, 3f)]
        [Tooltip("アラート表示持続時間")]
        public float AlertDuration = 2f;

        [Tooltip("カメラ振動効果")]
        public bool EnableCameraShake = true;

        [Range(0.1f, 2f)]
        [Tooltip("カメラ振動の強度")]
        public float CameraShakeIntensity = 0.5f;

        [Header("Audio Visual Indicators")]
        [Tooltip("音響可視化インジケーター")]
        public bool ShowAudioVisualizer = false;

        [Tooltip("NPC視界可視化（デバッグ用）")]
        public bool ShowNPCVisionDebug = false;

        [Tooltip("足音レベル表示")]
        public bool ShowFootstepLevelIndicator = true;

        [Header("Tutorial & Learning UI")]
        [Tooltip("チュートリアルヒント表示")]
        public bool ShowTutorialHints = true;

        [Tooltip("プログレス追跡UI")]
        public bool ShowProgressTracking = true;

        [Tooltip("コンテキストヘルプ表示")]
        public bool ShowContextualHelp = true;

        [Range(1f, 10f)]
        [Tooltip("チュートリアルヒント表示時間")]
        public float TutorialHintDuration = 5f;

        [Header("HUD Positioning")]
        [Range(0f, 1f)]
        [Tooltip("ステルスメーターのX位置（画面比率）")]
        public float StealthMeterPositionX = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("ステルスメーターのY位置（画面比率）")]
        public float StealthMeterPositionY = 0.9f;

        [Range(0f, 1f)]
        [Tooltip("検知インジケーターのX位置")]
        public float DetectionIndicatorPositionX = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("検知インジケーターのY位置")]
        public float DetectionIndicatorPositionY = 0.1f;

        [Header("Color Schemes")]
        [Tooltip("ステルス状態時のUI色")]
        public Color StealthedColor = new Color(0f, 1f, 0f, 0.7f);

        [Tooltip("警戒状態時のUI色")]
        public Color AlertColor = new Color(1f, 0f, 0f, 0.8f);

        [Tooltip("隠蔽状態時のUI色")]
        public Color ConcealedColor = new Color(0f, 0f, 1f, 0.6f);

        [Tooltip("中立状態時のUI色")]
        public Color NeutralColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("Performance Settings")]
        [Range(10f, 60f)]
        [Tooltip("UI更新頻度（FPS）")]
        public float UIUpdateRate = 30f;

        [Tooltip("最適化されたUI描画")]
        public bool EnableOptimizedRendering = true;

        [Range(0.01f, 0.05f)]
        [Tooltip("UI処理の最大フレーム時間")]
        public float MaxUIProcessingTime = 0.02f;

        [Header("Accessibility")]
        [Tooltip("色覚異常対応")]
        public bool EnableColorBlindSupport = false;

        [Tooltip("高コントラストモード")]
        public bool EnableHighContrastMode = false;

        [Range(0.8f, 2f)]
        [Tooltip("UI要素のスケール")]
        public float UIElementScale = 1f;

        /// <summary>
        /// デフォルト設定の適用
        /// 最適なユーザーエクスペリエンス
        /// </summary>
        public void ApplyDefaultSettings()
        {
            ShowStealthMeter = true;
            ShowDetectionIndicator = true;
            ShowInteractionPrompts = true;
            ShowConcealmentIndicator = true;
            EnableScreenEdgeWarning = true;
            DetectionAlertIntensity = 0.3f;
            AlertDuration = 2f;
            EnableCameraShake = true;
            CameraShakeIntensity = 0.5f;
            ShowAudioVisualizer = false;
            ShowNPCVisionDebug = false;
            ShowFootstepLevelIndicator = true;
            ShowTutorialHints = true;
            ShowProgressTracking = true;
            ShowContextualHelp = true;
            TutorialHintDuration = 5f;
            StealthMeterPositionX = 0.1f;
            StealthMeterPositionY = 0.9f;
            DetectionIndicatorPositionX = 0.5f;
            DetectionIndicatorPositionY = 0.1f;
            StealthedColor = new Color(0f, 1f, 0f, 0.7f);
            AlertColor = new Color(1f, 0f, 0f, 0.8f);
            ConcealedColor = new Color(0f, 0f, 1f, 0.6f);
            NeutralColor = new Color(1f, 1f, 1f, 0.5f);
            UIUpdateRate = 30f;
            EnableOptimizedRendering = true;
            MaxUIProcessingTime = 0.02f;
            EnableColorBlindSupport = false;
            EnableHighContrastMode = false;
            UIElementScale = 1f;
        }

        /// <summary>
        /// ミニマルUI設定（上級者向け）
        /// </summary>
        public void ApplyMinimalSettings()
        {
            ShowStealthMeter = false;
            ShowDetectionIndicator = true; // 最低限必要
            ShowInteractionPrompts = false;
            ShowConcealmentIndicator = false;
            EnableScreenEdgeWarning = false;
            ShowTutorialHints = false;
            ShowProgressTracking = false;
            ShowContextualHelp = false;
            ShowFootstepLevelIndicator = false;
        }

        /// <summary>
        /// 学習支援UI設定
        /// Learn & Grow価値実現特化
        /// </summary>
        public void ApplyLearningSupportSettings()
        {
            ShowStealthMeter = true;
            ShowDetectionIndicator = true;
            ShowInteractionPrompts = true;
            ShowConcealmentIndicator = true;
            ShowTutorialHints = true;
            ShowProgressTracking = true;
            ShowContextualHelp = true;
            TutorialHintDuration = 8f; // 長めの表示
            ShowFootstepLevelIndicator = true;
            ShowAudioVisualizer = true; // 学習用に音響可視化
        }

        /// <summary>
        /// パフォーマンス最適化設定
        /// </summary>
        public void ApplyPerformanceSettings()
        {
            UIUpdateRate = 20f; // 低頻度更新
            EnableOptimizedRendering = true;
            MaxUIProcessingTime = 0.01f;
            ShowAudioVisualizer = false;
            ShowNPCVisionDebug = false;
            DetectionAlertIntensity = 0.2f; // エフェクト軽減
            EnableCameraShake = false;
        }

        /// <summary>
        /// アクセシビリティ対応設定
        /// </summary>
        public void ApplyAccessibilitySettings()
        {
            EnableColorBlindSupport = true;
            EnableHighContrastMode = true;
            UIElementScale = 1.2f;
            TutorialHintDuration = 10f;
            ShowContextualHelp = true;
            
            // よりコントラストの高い色設定
            StealthedColor = new Color(0f, 1f, 0f, 1f);
            AlertColor = new Color(1f, 0f, 0f, 1f);
            ConcealedColor = new Color(0f, 0.5f, 1f, 1f);
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            if (UIUpdateRate <= 0f)
            {
                Debug.LogWarning("StealthUIConfig: UIUpdateRate must be greater than 0");
                isValid = false;
            }

            if (TutorialHintDuration <= 0f)
            {
                Debug.LogWarning("StealthUIConfig: TutorialHintDuration must be greater than 0");
                isValid = false;
            }

            if (UIElementScale <= 0f)
            {
                Debug.LogWarning("StealthUIConfig: UIElementScale must be greater than 0");
                isValid = false;
            }

            return isValid;
        }
    }
}