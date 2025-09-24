using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Stealth.Settings
{
    /// <summary>
    /// ステルスUIシステム設定（設計書準拠）
    /// Learn & Grow価値実現のための学習支援UI設定
    /// </summary>
    [CreateAssetMenu(menuName = "Templates/Stealth/UI Config", fileName = "StealthUIConfig")]
    public class StealthUIConfig : ScriptableObject
    {
        [TabGroup("Learning", "Progress")]
        [Header("Learning System Configuration")]
        [SerializeField]
        [Tooltip("学習進捗パネルを表示するかどうか（Learn & Grow価値実現）")]
        public bool ShowLearningProgress = true;

        [TabGroup("Learning", "Progress")]
        [SerializeField]
        [Tooltip("チュートリアルヒントを表示するかどうか")]
        public bool ShowTutorialHints = true;

        [TabGroup("Learning", "Progress")]
        [SerializeField]
        [Tooltip("学習段階の進捗表示を有効にするかどうか")]
        public bool EnableProgressTracking = true;

        [TabGroup("UI", "General")]
        [Header("General UI Settings")]
        [SerializeField]
        [Tooltip("UI要素のフェード時間")]
        public float UIFadeTime = 0.3f;

        [TabGroup("UI", "General")]
        [SerializeField]
        [Tooltip("警告表示の持続時間")]
        public float WarningDisplayDuration = 3f;

        [TabGroup("UI", "General")]
        [SerializeField]
        [Tooltip("通知表示の持続時間")]
        public float NotificationDisplayDuration = 2f;

        [TabGroup("UI", "Colors")]
        [Header("Color Configuration")]
        [SerializeField]
        [Tooltip("成功時の色")]
        public Color SuccessColor = Color.green;

        [TabGroup("UI", "Colors")]
        [SerializeField]
        [Tooltip("警告時の色")]
        public Color WarningColor = Color.yellow;

        [TabGroup("UI", "Colors")]
        [SerializeField]
        [Tooltip("エラー時の色")]
        public Color ErrorColor = Color.red;

        [TabGroup("Stealth", "Detection")]
        [Header("Stealth Specific UI")]
        [SerializeField]
        [Tooltip("検知レベル表示を有効にするかどうか")]
        public bool ShowDetectionLevel = true;

        [TabGroup("Stealth", "Detection")]
        [SerializeField]
        [Tooltip("警戒レベル表示を有効にするかどうか")]
        public bool ShowAlertLevel = true;

        [TabGroup("Stealth", "Detection")]
        [SerializeField]
        [Tooltip("ステルス状態インジケーターを表示するかどうか")]
        public bool ShowStealthIndicator = true;

        [TabGroup("Stealth", "Interaction")]
        [SerializeField]
        [Tooltip("相互作用可能オブジェクトのハイライトを表示するかどうか")]
        public bool ShowInteractionHighlight = true;

        [TabGroup("Stealth", "Interaction")]
        [SerializeField]
        [Tooltip("隠れ場所の候補を表示するかどうか")]
        public bool ShowHidingSpots = true;

        [TabGroup("Audio", "Feedback")]
        [Header("Audio Visual Feedback")]
        [SerializeField]
        [Tooltip("音響レベルの視覚表示を有効にするかどうか")]
        public bool ShowAudioLevelVisual = true;

        [TabGroup("Audio", "Feedback")]
        [SerializeField]
        [Tooltip("音響マスキング効果の表示を有効にするかどうか")]
        public bool ShowAudioMaskingVisual = true;

        [TabGroup("Performance")]
        [Header("Performance Settings")]
        [SerializeField]
        [Tooltip("UI更新頻度（秒）")]
        public float UIUpdateFrequency = 0.1f;

        [TabGroup("Performance")]
        [SerializeField]
        [Tooltip("重いUI要素のLOD管理を有効にするかどうか")]
        public bool EnableUILOD = true;

        #region Validation

        private void OnValidate()
        {
            // 設定値の検証
            UIFadeTime = Mathf.Max(0.1f, UIFadeTime);
            WarningDisplayDuration = Mathf.Max(1f, WarningDisplayDuration);
            NotificationDisplayDuration = Mathf.Max(1f, NotificationDisplayDuration);
            UIUpdateFrequency = Mathf.Clamp(UIUpdateFrequency, 0.01f, 1f);
        }

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        [TabGroup("Debug")]
        [Button("Test UI Colors")]
        private void TestUIColors()
        {
            Debug.Log($"Success: {SuccessColor}, Warning: {WarningColor}, Error: {ErrorColor}");
        }

        [TabGroup("Debug")]
        [Button("Validate Configuration")]
        private void ValidateConfiguration()
        {
            Debug.Log($"[StealthUIConfig] Configuration validated successfully");
            Debug.Log($"Learning Progress: {ShowLearningProgress}");
            Debug.Log($"Tutorial Hints: {ShowTutorialHints}");
            Debug.Log($"UI Update Frequency: {UIUpdateFrequency}s");
        }
#endif

        #endregion
    }
}
