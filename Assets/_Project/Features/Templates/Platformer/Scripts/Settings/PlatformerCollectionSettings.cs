using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer Collection Settings：収集システム設定
    /// ScriptableObjectベースのデータ駆動設計でノンプログラマー対応
    /// Learn & Grow価値実現：ゲームデザイナーがコード不要でバランス調整可能
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerCollectionSettings", menuName = "Platformer/Settings/Collection Settings")]
    public class PlatformerCollectionSettings : ScriptableObject
    {
        [Header("Collection Configuration")]
        [InfoBox("プラットフォーマーアイテム収集システムの基本設定")]

        [TabGroup("Basic")]
        [LabelText("レベル完了閾値")]
        [Range(0.5f, 1.0f)]
        [Tooltip("レベル完了に必要なアイテム収集率（0.5 = 50%、1.0 = 100%）")]
        public float LevelCompletionThreshold = 0.8f;

        [TabGroup("Basic")]
        [LabelText("必須アイテム収集を要求")]
        [Tooltip("必須アイテムをすべて収集しないとレベル完了不可にするか")]
        public bool RequireAllMandatoryItems = true;

        [TabGroup("Basic")]
        [LabelText("最大同時表示アイテム数")]
        [Range(10, 100)]
        [Tooltip("同時に表示可能なアイテム数（パフォーマンス調整）")]
        public int MaxVisibleItems = 50;

        [Header("Score Configuration")]
        [TabGroup("Score")]
        [LabelText("スコア設定")]
        public ScoreSettings ScoreConfiguration = new ScoreSettings();

        [Header("Visual & Audio Configuration")]
        [TabGroup("Visual")]
        [LabelText("視覚効果設定")]
        public VisualEffectsSettings VisualEffects = new VisualEffectsSettings();

        [TabGroup("Audio")]
        [LabelText("音響効果設定")]
        public AudioEffectsSettings AudioEffects = new AudioEffectsSettings();

        [Header("Performance Configuration")]
        [TabGroup("Performance")]
        [LabelText("パフォーマンス設定")]
        public PerformanceSettings Performance = new PerformanceSettings();

        [Header("Debug Configuration")]
        [TabGroup("Debug")]
        [LabelText("デバッグ設定")]
        public DebugSettings Debug = new DebugSettings();

        [System.Serializable]
        public class ScoreSettings
        {
            [LabelText("基本アイテムスコア")]
            [Range(10, 1000)]
            public int BaseItemScore = 100;

            [LabelText("レアアイテムスコア")]
            [Range(100, 5000)]
            public int RareItemScore = 500;

            [LabelText("必須アイテムスコア")]
            [Range(500, 10000)]
            public int MandatoryItemScore = 1000;

            [LabelText("スコア倍率設定")]
            public ScoreMultiplierSettings Multipliers = new ScoreMultiplierSettings();
        }

        [System.Serializable]
        public class ScoreMultiplierSettings
        {
            [LabelText("完璧収集ボーナス")]
            [Range(1.0f, 3.0f)]
            [Tooltip("100%収集時のスコア倍率")]
            public float PerfectCollectionBonus = 2.0f;

            [LabelText("速度ボーナス")]
            [Range(1.0f, 2.0f)]
            [Tooltip("高速収集時のスコア倍率")]
            public float SpeedBonus = 1.5f;

            [LabelText("コンボボーナス")]
            [Range(1.0f, 3.0f)]
            [Tooltip("連続収集時のスコア倍率")]
            public float ComboBonus = 1.8f;
        }

        [System.Serializable]
        public class VisualEffectsSettings
        {
            [LabelText("収集エフェクト有効")]
            public bool EnableCollectionEffects = true;

            [LabelText("収集エフェクト持続時間")]
            [Range(0.1f, 2.0f)]
            public float EffectDuration = 0.5f;

            [LabelText("アイテム回転速度")]
            [Range(0f, 360f)]
            public float ItemRotationSpeed = 90f;

            [LabelText("アイテム浮遊アニメーション")]
            public bool EnableFloatingAnimation = true;

            [LabelText("浮遊振幅")]
            [Range(0.1f, 1.0f)]
            public float FloatingAmplitude = 0.2f;

            [LabelText("浮遊速度")]
            [Range(0.5f, 3.0f)]
            public float FloatingSpeed = 1.0f;
        }

        [System.Serializable]
        public class AudioEffectsSettings
        {
            [LabelText("音響効果有効")]
            public bool EnableAudioEffects = true;

            [LabelText("アイテム収集音量")]
            [Range(0f, 1f)]
            public float CollectionVolume = 0.7f;

            [LabelText("レベル完了音量")]
            [Range(0f, 1f)]
            public float LevelCompleteVolume = 0.8f;

            [LabelText("音響フィードバック有効")]
            [Tooltip("収集時の音響フィードバック（ピッチ変更等）")]
            public bool EnableAudioFeedback = true;

            [LabelText("ピッチ変更範囲")]
            [Range(0.1f, 2.0f)]
            public float PitchVariationRange = 0.2f;
        }

        [System.Serializable]
        public class PerformanceSettings
        {
            [LabelText("LOD システム有効")]
            [Tooltip("距離に応じたアイテム詳細度制御")]
            public bool EnableLODSystem = true;

            [LabelText("LOD 距離")]
            [Range(10f, 100f)]
            public float LODDistance = 30f;

            [LabelText("カリング有効")]
            [Tooltip("視界外アイテムの処理停止")]
            public bool EnableFrustumCulling = true;

            [LabelText("更新頻度（Hz）")]
            [Range(10, 60)]
            [Tooltip("アイテム状態更新頻度")]
            public int UpdateFrequency = 30;

            [LabelText("バッチ処理サイズ")]
            [Range(1, 20)]
            [Tooltip("1フレームあたりの処理アイテム数")]
            public int BatchProcessingSize = 5;
        }

        [System.Serializable]
        public class DebugSettings
        {
            [LabelText("デバッグ表示有効")]
            public bool EnableDebugDisplay = false;

            [LabelText("アイテムデバッグ色")]
            public Color ItemDebugColor = Color.cyan;

            [LabelText("収集範囲表示")]
            public bool ShowCollectionRange = false;

            [LabelText("統計情報表示")]
            public bool ShowStatistics = false;

            [LabelText("パフォーマンス監視")]
            public bool EnablePerformanceMonitoring = false;
        }

        /// <summary>
        /// 設定値検証
        /// </summary>
        [Button("設定値検証")]
        [TabGroup("Validation")]
        public void ValidateSettings()
        {
            bool isValid = true;

            if (LevelCompletionThreshold < 0.1f || LevelCompletionThreshold > 1.0f)
            {
                UnityEngine.Debug.LogError("[PlatformerCollectionSettings] Invalid LevelCompletionThreshold. Must be between 0.1 and 1.0");
                isValid = false;
            }

            if (MaxVisibleItems < 1)
            {
                UnityEngine.Debug.LogError("[PlatformerCollectionSettings] MaxVisibleItems must be at least 1");
                isValid = false;
            }

            if (ScoreConfiguration.BaseItemScore <= 0)
            {
                UnityEngine.Debug.LogError("[PlatformerCollectionSettings] BaseItemScore must be positive");
                isValid = false;
            }

            if (isValid)
            {
                UnityEngine.Debug.Log("[PlatformerCollectionSettings] All settings are valid ✓");
            }
        }

        /// <summary>
        /// デフォルト設定復元
        /// </summary>
        [Button("デフォルト設定に復元")]
        [TabGroup("Validation")]
        public void ResetToDefaults()
        {
            LevelCompletionThreshold = 0.8f;
            RequireAllMandatoryItems = true;
            MaxVisibleItems = 50;

            ScoreConfiguration = new ScoreSettings();
            VisualEffects = new VisualEffectsSettings();
            AudioEffects = new AudioEffectsSettings();
            Performance = new PerformanceSettings();
            Debug = new DebugSettings();

            UnityEngine.Debug.Log("[PlatformerCollectionSettings] Settings reset to defaults");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// 難易度別プリセット適用
        /// </summary>
        [Button("Easy")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyEasyPreset()
        {
            LevelCompletionThreshold = 0.6f;
            RequireAllMandatoryItems = false;
            ScoreConfiguration.Multipliers.PerfectCollectionBonus = 1.5f;
            UnityEngine.Debug.Log("[PlatformerCollectionSettings] Easy preset applied");
        }

        [Button("Normal")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyNormalPreset()
        {
            LevelCompletionThreshold = 0.8f;
            RequireAllMandatoryItems = true;
            ScoreConfiguration.Multipliers.PerfectCollectionBonus = 2.0f;
            UnityEngine.Debug.Log("[PlatformerCollectionSettings] Normal preset applied");
        }

        [Button("Hard")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyHardPreset()
        {
            LevelCompletionThreshold = 1.0f;
            RequireAllMandatoryItems = true;
            ScoreConfiguration.Multipliers.PerfectCollectionBonus = 3.0f;
            UnityEngine.Debug.Log("[PlatformerCollectionSettings] Hard preset applied");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ専用：設定プレビュー
        /// </summary>
        [Button("設定プレビュー")]
        [TabGroup("Debug")]
        public void PreviewSettings()
        {
            UnityEngine.Debug.Log("=== Platformer Collection Settings Preview ===");
            UnityEngine.Debug.Log($"Level Completion Threshold: {LevelCompletionThreshold:P}");
            UnityEngine.Debug.Log($"Require All Mandatory Items: {RequireAllMandatoryItems}");
            UnityEngine.Debug.Log($"Max Visible Items: {MaxVisibleItems}");
            UnityEngine.Debug.Log($"Base Item Score: {ScoreConfiguration.BaseItemScore}");
            UnityEngine.Debug.Log($"Perfect Collection Bonus: {ScoreConfiguration.Multipliers.PerfectCollectionBonus:F1}x");
            UnityEngine.Debug.Log($"Visual Effects Enabled: {VisualEffects.EnableCollectionEffects}");
            UnityEngine.Debug.Log($"Audio Effects Enabled: {AudioEffects.EnableAudioEffects}");
            UnityEngine.Debug.Log($"LOD System Enabled: {Performance.EnableLODSystem}");
        }
#endif

        private void OnValidate()
        {
            // Unity Inspector変更時の自動検証
            LevelCompletionThreshold = Mathf.Clamp(LevelCompletionThreshold, 0.1f, 1.0f);
            MaxVisibleItems = Mathf.Max(1, MaxVisibleItems);

            if (ScoreConfiguration != null)
            {
                ScoreConfiguration.BaseItemScore = Mathf.Max(1, ScoreConfiguration.BaseItemScore);
                ScoreConfiguration.RareItemScore = Mathf.Max(ScoreConfiguration.BaseItemScore, ScoreConfiguration.RareItemScore);
                ScoreConfiguration.MandatoryItemScore = Mathf.Max(ScoreConfiguration.RareItemScore, ScoreConfiguration.MandatoryItemScore);
            }
        }

        // 下位互換性のためのレガシープロパティ
        public int TotalCollectibles => MaxVisibleItems;
        public int BaseScore => ScoreConfiguration?.BaseItemScore ?? 100;
        public float ScoreMultiplier => ScoreConfiguration?.Multipliers?.PerfectCollectionBonus ?? 1f;

        public void SetToDefault() => ResetToDefaults();
    }
}
