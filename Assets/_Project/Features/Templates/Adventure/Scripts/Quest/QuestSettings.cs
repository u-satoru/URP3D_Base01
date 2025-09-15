using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Adventure.Quest
{
    /// <summary>
    /// Adventure Template用クエスト設定
    /// クエストシステムの全体設定と調整パラメータを管理
    /// </summary>
    [CreateAssetMenu(fileName = "QuestSettings", menuName = "Templates/Adventure/Quest Settings")]
    public class QuestSettings : ScriptableObject
    {
        [TabGroup("General", "General Settings")]
        [Header("Quest Limits")]
        [SerializeField]
        [Tooltip("同時に進行可能なクエスト数の上限")]
        private int maxActiveQuests = 10;

        [TabGroup("General", "General Settings")]
        [SerializeField]
        [Tooltip("自動セーブでクエスト進捗を保存するかどうか")]
        private bool autoSaveProgress = true;

        [TabGroup("General", "General Settings")]
        [SerializeField]
        [Tooltip("クエスト完了時のデバッグログを表示するかどうか")]
        private bool debugLogging = true;

        [TabGroup("Rewards", "Reward Settings")]
        [Header("Reward Settings")]
        [SerializeField]
        [Tooltip("クエスト報酬システムを有効にする")]
        private bool enableRewards = true;

        [TabGroup("Rewards", "Reward Settings")]
        [SerializeField]
        [Tooltip("経験値報酬の基本倍率")]
        [Range(0.1f, 5.0f)]
        private float experienceMultiplier = 1.0f;

        [TabGroup("Rewards", "Reward Settings")]
        [SerializeField]
        [Tooltip("金貨報酬の基本倍率")]
        [Range(0.1f, 5.0f)]
        private float goldMultiplier = 1.0f;

        [TabGroup("Timing", "Time Settings")]
        [Header("Time Settings")]
        [SerializeField]
        [Tooltip("クエストのタイマー機能を有効にする")]
        private bool enableQuestTimers = true;

        [TabGroup("Timing", "Time Settings")]
        [SerializeField]
        [Tooltip("制限時間のあるクエストのデフォルト時間（秒）")]
        [Min(60)]
        private float defaultTimeLimit = 600f; // 10 minutes

        [TabGroup("Timing", "Time Settings")]
        [SerializeField]
        [Tooltip("制限時間切れの警告を表示する残り時間（秒）")]
        [Min(10)]
        private float timeLimitWarningThreshold = 60f; // 1 minute

        [TabGroup("UI", "UI Settings")]
        [Header("Quest Display")]
        [SerializeField]
        [Tooltip("クエスト完了時の通知表示時間（秒）")]
        [Min(1)]
        private float questCompleteNotificationDuration = 3.0f;

        [TabGroup("UI", "UI Settings")]
        [SerializeField]
        [Tooltip("新しいクエスト開始時の通知表示時間（秒）")]
        [Min(1)]
        private float questStartNotificationDuration = 2.5f;

        [TabGroup("Validation", "Validation Settings")]
        [Header("Validation")]
        [SerializeField]
        [Tooltip("クエスト前提条件の厳密チェックを有効にする")]
        private bool strictPrerequisiteValidation = true;

        [TabGroup("Validation", "Validation Settings")]
        [SerializeField]
        [Tooltip("不正な目標進捗の自動修正を有効にする")]
        private bool autoFixInvalidObjectiveProgress = true;

        // プロパティ
        public int MaxActiveQuests => maxActiveQuests;
        public bool AutoSaveProgress => autoSaveProgress;
        public bool DebugLogging => debugLogging;
        public bool EnableRewards => enableRewards;
        public bool EnableQuestTimers => enableQuestTimers;
        public float ExperienceMultiplier => experienceMultiplier;
        public float GoldMultiplier => goldMultiplier;
        public float DefaultTimeLimit => defaultTimeLimit;
        public float TimeLimitWarningThreshold => timeLimitWarningThreshold;
        public float QuestCompleteNotificationDuration => questCompleteNotificationDuration;
        public float QuestStartNotificationDuration => questStartNotificationDuration;
        public bool StrictPrerequisiteValidation => strictPrerequisiteValidation;
        public bool AutoFixInvalidObjectiveProgress => autoFixInvalidObjectiveProgress;

        [TabGroup("Debug", "Debug Tools")]
        [Button("Validate Settings")]
        private void ValidateSettings()
        {
            bool isValid = true;
            System.Text.StringBuilder errors = new System.Text.StringBuilder();

            if (maxActiveQuests <= 0)
            {
                errors.AppendLine("Max Active Quests must be greater than 0");
                isValid = false;
            }

            if (experienceMultiplier <= 0)
            {
                errors.AppendLine("Experience Multiplier must be greater than 0");
                isValid = false;
            }

            if (goldMultiplier <= 0)
            {
                errors.AppendLine("Gold Multiplier must be greater than 0");
                isValid = false;
            }

            if (defaultTimeLimit < 60)
            {
                errors.AppendLine("Default Time Limit should be at least 60 seconds");
                isValid = false;
            }

            if (timeLimitWarningThreshold >= defaultTimeLimit)
            {
                errors.AppendLine("Time Limit Warning Threshold should be less than Default Time Limit");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log("[QuestSettings] All settings are valid!");
            }
            else
            {
                Debug.LogError($"[QuestSettings] Validation errors:\n{errors}");
            }
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Log Current Settings")]
        private void LogCurrentSettings()
        {
            Debug.Log($"[QuestSettings] Current Configuration:\n" +
                     $"Max Active Quests: {maxActiveQuests}\n" +
                     $"Auto Save Progress: {autoSaveProgress}\n" +
                     $"Debug Logging: {debugLogging}\n" +
                     $"Experience Multiplier: {experienceMultiplier}\n" +
                     $"Gold Multiplier: {goldMultiplier}\n" +
                     $"Default Time Limit: {defaultTimeLimit}s\n" +
                     $"Time Limit Warning: {timeLimitWarningThreshold}s\n" +
                     $"Quest Complete Notification: {questCompleteNotificationDuration}s\n" +
                     $"Quest Start Notification: {questStartNotificationDuration}s\n" +
                     $"Strict Prerequisites: {strictPrerequisiteValidation}\n" +
                     $"Auto Fix Progress: {autoFixInvalidObjectiveProgress}");
        }

        [TabGroup("Debug", "Debug Tools")]
        [Button("Reset to Defaults")]
        private void ResetToDefaults()
        {
            maxActiveQuests = 10;
            autoSaveProgress = true;
            debugLogging = true;
            enableRewards = true;
            enableQuestTimers = true;
            experienceMultiplier = 1.0f;
            goldMultiplier = 1.0f;
            defaultTimeLimit = 600f;
            timeLimitWarningThreshold = 60f;
            questCompleteNotificationDuration = 3.0f;
            questStartNotificationDuration = 2.5f;
            strictPrerequisiteValidation = true;
            autoFixInvalidObjectiveProgress = true;

            Debug.Log("[QuestSettings] Settings reset to default values");
        }

        private void OnValidate()
        {
            // 実行時の値制限
            maxActiveQuests = Mathf.Max(1, maxActiveQuests);
            experienceMultiplier = Mathf.Max(0.1f, experienceMultiplier);
            goldMultiplier = Mathf.Max(0.1f, goldMultiplier);
            defaultTimeLimit = Mathf.Max(60f, defaultTimeLimit);
            timeLimitWarningThreshold = Mathf.Min(timeLimitWarningThreshold, defaultTimeLimit - 10f);
            questCompleteNotificationDuration = Mathf.Max(1f, questCompleteNotificationDuration);
            questStartNotificationDuration = Mathf.Max(1f, questStartNotificationDuration);
        }
    }
}