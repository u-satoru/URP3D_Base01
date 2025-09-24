using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer Checkpoint Settings：セーブ・リスポーンシステム設定
    /// ScriptableObjectベースのデータ駆動設計でノンプログラマー対応
    /// Learn & Grow価値実現：ゲームデザイナーがコード不要でセーブシステム調整可能
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerCheckpointSettings", menuName = "Platformer/Settings/Checkpoint Settings")]
    public class PlatformerCheckpointSettings : ScriptableObject
    {
        [Header("Checkpoint Configuration")]
        [InfoBox("プラットフォーマーチェックポイント・セーブシステムの基本設定")]

        [TabGroup("Basic")]
        [LabelText("自動保存有効")]
        [Tooltip("チェックポイント到達時の自動保存を有効にするか")]
        public bool EnableAutoSave = true;

        [TabGroup("Basic")]
        [LabelText("自動保存間隔")]
        [Range(5f, 300f)]
        [Tooltip("自動保存の間隔（秒）。0で無効")]
        public float AutoSaveInterval = 30f;

        [TabGroup("Basic")]
        [LabelText("最大保存スロット数")]
        [Range(1, 10)]
        [Tooltip("同時に管理できるセーブスロットの最大数")]
        public int MaxSaveSlots = 3;

        [Header("Respawn Configuration")]
        [TabGroup("Respawn")]
        [LabelText("リスポーン設定")]
        public RespawnSettings RespawnConfiguration = new RespawnSettings();

        [Header("Save Data Configuration")]
        [TabGroup("SaveData")]
        [LabelText("セーブデータ設定")]
        public SaveDataSettings SaveDataConfiguration = new SaveDataSettings();

        [Header("Checkpoint Placement")]
        [TabGroup("Placement")]
        [LabelText("チェックポイント配置設定")]
        public CheckpointPlacementSettings PlacementSettings = new CheckpointPlacementSettings();

        [Header("Performance Configuration")]
        [TabGroup("Performance")]
        [LabelText("パフォーマンス設定")]
        public CheckpointPerformanceSettings Performance = new CheckpointPerformanceSettings();

        [Header("Debug Configuration")]
        [TabGroup("Debug")]
        [LabelText("デバッグ設定")]
        public CheckpointDebugSettings Debug = new CheckpointDebugSettings();

        [System.Serializable]
        public class RespawnSettings
        {
            [LabelText("リスポーン遅延")]
            [Range(0f, 5f)]
            [Tooltip("死亡からリスポーンまでの遅延時間")]
            public float RespawnDelay = 1.5f;

            [LabelText("リスポーンエフェクト有効")]
            [Tooltip("リスポーン時の視覚効果を表示するか")]
            public bool EnableRespawnEffects = true;

            [LabelText("リスポーン時体力回復")]
            [Tooltip("リスポーン時にプレイヤーの体力を回復するか")]
            public bool RestoreHealthOnRespawn = true;

            [LabelText("リスポーン時体力回復量")]
            [Range(0f, 1f)]
            [Tooltip("リスポーン時の体力回復割合（0.0-1.0）")]
            public float HealthRestoreAmount = 1.0f;

            [LabelText("リスポーン時無敵時間")]
            [Range(0f, 10f)]
            [Tooltip("リスポーン後の無敵時間（秒）")]
            public float InvulnerabilityTime = 2.0f;

            [LabelText("エフェクト持続時間")]
            [Range(0.1f, 3.0f)]
            [Tooltip("リスポーンエフェクトの持続時間")]
            public float EffectDuration = 1.0f;
        }

        [System.Serializable]
        public class SaveDataSettings
        {
            [LabelText("データ暗号化有効")]
            [Tooltip("セーブデータの暗号化を有効にするか")]
            public bool EnableEncryption = true;

            [LabelText("データ圧縮有効")]
            [Tooltip("セーブデータの圧縮を有効にするか")]
            public bool EnableCompression = true;

            [LabelText("データ整合性チェック")]
            [Tooltip("セーブデータの整合性チェックを有効にするか")]
            public bool EnableIntegrityCheck = true;

            [LabelText("バックアップ作成")]
            [Tooltip("セーブ時にバックアップファイルを作成するか")]
            public bool CreateBackups = true;

            [LabelText("最大バックアップ数")]
            [Range(1, 5)]
            [Tooltip("保持するバックアップファイルの最大数")]
            public int MaxBackupCount = 2;

            [LabelText("保存するデータ項目")]
            public SaveDataItems SaveItems = new SaveDataItems();
        }

        [System.Serializable]
        public class SaveDataItems
        {
            [LabelText("プレイヤー位置")]
            public bool SavePlayerPosition = true;

            [LabelText("プレイヤーステータス")]
            public bool SavePlayerStats = true;

            [LabelText("収集アイテム")]
            public bool SaveCollectedItems = true;

            [LabelText("レベル進行状況")]
            public bool SaveLevelProgress = true;

            [LabelText("ゲーム設定")]
            public bool SaveGameSettings = true;

            [LabelText("実績・統計")]
            public bool SaveAchievements = false;
        }

        [System.Serializable]
        public class CheckpointPlacementSettings
        {
            [LabelText("自動配置有効")]
            [Tooltip("レベル生成時にチェックポイントを自動配置するか")]
            public bool EnableAutoPlacement = true;

            [LabelText("チェックポイント間隔")]
            [Range(50f, 500f)]
            [Tooltip("チェックポイント間の推奨距離")]
            public float CheckpointSpacing = 200f;

            [LabelText("難所前配置")]
            [Tooltip("難しいセクション前に自動配置するか")]
            public bool PlaceBeforeDifficultSections = true;

            [LabelText("収集アイテム後配置")]
            [Tooltip("重要な収集アイテム後に配置するか")]
            public bool PlaceAfterCollectibles = true;

            [LabelText("最小高度差")]
            [Range(10f, 100f)]
            [Tooltip("チェックポイント配置の最小高度差")]
            public float MinimumHeightDifference = 30f;

            [LabelText("配置バリエーション")]
            public PlacementVariationSettings Variations = new PlacementVariationSettings();
        }

        [System.Serializable]
        public class PlacementVariationSettings
        {
            [LabelText("距離バリエーション")]
            [Range(0.1f, 0.5f)]
            [Tooltip("配置間隔のランダムバリエーション（0.1 = ±10%）")]
            public float DistanceVariation = 0.2f;

            [LabelText("高度オフセット")]
            [Range(0f, 10f)]
            [Tooltip("配置位置のランダム高度オフセット")]
            public float HeightOffset = 5f;

            [LabelText("配置タイミング調整")]
            [Range(0.5f, 2.0f)]
            [Tooltip("難易度に応じた配置タイミング調整")]
            public float DifficultyModifier = 1.0f;
        }

        [System.Serializable]
        public class CheckpointPerformanceSettings
        {
            [LabelText("非同期保存有効")]
            [Tooltip("セーブ処理を非同期で実行するか")]
            public bool EnableAsyncSaving = true;

            [LabelText("保存処理タイムアウト")]
            [Range(1f, 30f)]
            [Tooltip("セーブ処理のタイムアウト時間（秒）")]
            public float SaveTimeout = 10f;

            [LabelText("チェックポイントプーリング")]
            [Tooltip("チェックポイントオブジェクトのプーリングを有効にするか")]
            public bool EnableCheckpointPooling = true;

            [LabelText("最大同時チェックポイント数")]
            [Range(5, 50)]
            [Tooltip("同時にアクティブにできるチェックポイントの最大数")]
            public int MaxActiveCheckpoints = 20;

            [LabelText("ガベージコレクション最適化")]
            [Tooltip("セーブ処理でのGC最適化を有効にするか")]
            public bool OptimizeGarbageCollection = true;
        }

        [System.Serializable]
        public class CheckpointDebugSettings
        {
            [LabelText("デバッグ表示有効")]
            public bool EnableDebugDisplay = false;

            [LabelText("チェックポイントデバッグ色")]
            public Color CheckpointDebugColor = Color.green;

            [LabelText("リスポーン範囲表示")]
            public bool ShowRespawnRange = false;

            [LabelText("セーブデータログ")]
            public bool LogSaveData = false;

            [LabelText("パフォーマンス監視")]
            public bool EnablePerformanceMonitoring = false;

            [LabelText("詳細ログ出力")]
            public bool EnableVerboseLogging = false;
        }

        /// <summary>
        /// 設定値検証
        /// </summary>
        [Button("設定値検証")]
        [TabGroup("Validation")]
        public void ValidateSettings()
        {
            bool isValid = true;

            if (AutoSaveInterval < 5f && AutoSaveInterval > 0f)
            {
                UnityEngine.Debug.LogError("[PlatformerCheckpointSettings] AutoSaveInterval too short. Minimum 5 seconds recommended");
                isValid = false;
            }

            if (MaxSaveSlots < 1 || MaxSaveSlots > 10)
            {
                UnityEngine.Debug.LogError("[PlatformerCheckpointSettings] Invalid MaxSaveSlots. Must be between 1 and 10");
                isValid = false;
            }

            if (PlacementSettings.CheckpointSpacing < 50f)
            {
                UnityEngine.Debug.LogError("[PlatformerCheckpointSettings] CheckpointSpacing too small. Minimum 50 units recommended");
                isValid = false;
            }

            if (isValid)
            {
                UnityEngine.Debug.Log("[PlatformerCheckpointSettings] All settings are valid ✓");
            }
        }

        /// <summary>
        /// デフォルト設定復元
        /// </summary>
        [Button("デフォルト設定に復元")]
        [TabGroup("Validation")]
        public void ResetToDefaults()
        {
            EnableAutoSave = true;
            AutoSaveInterval = 30f;
            MaxSaveSlots = 3;

            RespawnConfiguration = new RespawnSettings();
            SaveDataConfiguration = new SaveDataSettings();
            PlacementSettings = new CheckpointPlacementSettings();
            Performance = new CheckpointPerformanceSettings();
            Debug = new CheckpointDebugSettings();

            UnityEngine.Debug.Log("[PlatformerCheckpointSettings] Settings reset to defaults");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// 難易度別プリセット適用
        /// </summary>
        [Button("Casual")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyCasualPreset()
        {
            AutoSaveInterval = 15f;
            RespawnConfiguration.RespawnDelay = 1.0f;
            RespawnConfiguration.RestoreHealthOnRespawn = true;
            RespawnConfiguration.HealthRestoreAmount = 1.0f;
            PlacementSettings.CheckpointSpacing = 150f;
            UnityEngine.Debug.Log("[PlatformerCheckpointSettings] Casual preset applied");
        }

        [Button("Normal")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyNormalPreset()
        {
            AutoSaveInterval = 30f;
            RespawnConfiguration.RespawnDelay = 1.5f;
            RespawnConfiguration.RestoreHealthOnRespawn = true;
            RespawnConfiguration.HealthRestoreAmount = 0.8f;
            PlacementSettings.CheckpointSpacing = 200f;
            UnityEngine.Debug.Log("[PlatformerCheckpointSettings] Normal preset applied");
        }

        [Button("Hardcore")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyHardcorePreset()
        {
            AutoSaveInterval = 60f;
            RespawnConfiguration.RespawnDelay = 2.0f;
            RespawnConfiguration.RestoreHealthOnRespawn = false;
            RespawnConfiguration.HealthRestoreAmount = 0.5f;
            PlacementSettings.CheckpointSpacing = 300f;
            UnityEngine.Debug.Log("[PlatformerCheckpointSettings] Hardcore preset applied");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ専用：設定プレビュー
        /// </summary>
        [Button("設定プレビュー")]
        [TabGroup("Debug")]
        public void PreviewSettings()
        {
            UnityEngine.Debug.Log("=== Platformer Checkpoint Settings Preview ===");
            UnityEngine.Debug.Log($"Auto Save: {EnableAutoSave} (Interval: {AutoSaveInterval}s)");
            UnityEngine.Debug.Log($"Max Save Slots: {MaxSaveSlots}");
            UnityEngine.Debug.Log($"Respawn Delay: {RespawnConfiguration.RespawnDelay}s");
            UnityEngine.Debug.Log($"Health Restore: {RespawnConfiguration.RestoreHealthOnRespawn} ({RespawnConfiguration.HealthRestoreAmount:P})");
            UnityEngine.Debug.Log($"Checkpoint Spacing: {PlacementSettings.CheckpointSpacing}m");
            UnityEngine.Debug.Log($"Data Encryption: {SaveDataConfiguration.EnableEncryption}");
            UnityEngine.Debug.Log($"Async Saving: {Performance.EnableAsyncSaving}");
        }
#endif

        private void OnValidate()
        {
            // Unity Inspector変更時の自動検証
            AutoSaveInterval = Mathf.Max(0f, AutoSaveInterval);
            MaxSaveSlots = Mathf.Clamp(MaxSaveSlots, 1, 10);

            if (RespawnConfiguration != null)
            {
                RespawnConfiguration.RespawnDelay = Mathf.Max(0f, RespawnConfiguration.RespawnDelay);
                RespawnConfiguration.HealthRestoreAmount = Mathf.Clamp01(RespawnConfiguration.HealthRestoreAmount);
                RespawnConfiguration.InvulnerabilityTime = Mathf.Max(0f, RespawnConfiguration.InvulnerabilityTime);
            }

            if (PlacementSettings != null)
            {
                PlacementSettings.CheckpointSpacing = Mathf.Max(10f, PlacementSettings.CheckpointSpacing);
                PlacementSettings.MinimumHeightDifference = Mathf.Max(5f, PlacementSettings.MinimumHeightDifference);
            }

            if (Performance != null)
            {
                Performance.SaveTimeout = Mathf.Clamp(Performance.SaveTimeout, 1f, 30f);
                Performance.MaxActiveCheckpoints = Mathf.Clamp(Performance.MaxActiveCheckpoints, 1, 50);
            }
        }

        // 下位互換性のためのレガシープロパティ
        public float GetRespawnDelay() => RespawnConfiguration?.RespawnDelay ?? 1.5f;
        public float GetSaveInterval() => AutoSaveInterval;
        public bool ShouldAutoSave() => EnableAutoSave;
        public void SetToDefault() => ResetToDefaults();
    }
}
