using UnityEngine;
using Sirenix.OdinInspector;

namespace asterivo.Unity60.Features.Templates.Platformer.Settings
{
    /// <summary>
    /// Platformer Level Settings：レベル生成・配置・動的調整システム
    /// ScriptableObjectベースのデータ駆動設計でノンプログラマー対応
    /// Learn & Grow価値実現：ゲームデザイナーがコード不要でレベル設計可能
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformerLevelSettings", menuName = "Platformer/Settings/Level Settings")]
    public class PlatformerLevelSettings : ScriptableObject
    {
        [Header("Level Generation Configuration")]
        [InfoBox("プラットフォーマーレベル生成システムの基本設定")]

        [TabGroup("Basic")]
        [LabelText("レベル長さ")]
        [Range(5, 100)]
        [Tooltip("レベルのセグメント数（5 = 短いレベル、100 = 長いレベル）")]
        public int LevelLength = 20;

        [TabGroup("Basic")]
        [LabelText("難易度スケール")]
        [Range(0.5f, 3.0f)]
        [Tooltip("基本難易度の倍率（0.5 = 簡単、3.0 = 非常に困難）")]
        public float DifficultyScale = 1f;

        [TabGroup("Basic")]
        [LabelText("最大高度")]
        [Range(5f, 50f)]
        [Tooltip("レベル内での最大高度制限")]
        public float MaxHeight = 20f;

        [Header("Procedural Generation Configuration")]
        [TabGroup("Generation")]
        [LabelText("プロシージャル生成設定")]
        public ProceduralGenerationSettings ProceduralGeneration = new ProceduralGenerationSettings();

        [Header("Platform Configuration")]
        [TabGroup("Platforms")]
        [LabelText("プラットフォーム設定")]
        public PlatformGenerationSettings PlatformGeneration = new PlatformGenerationSettings();

        [Header("Collectibles Configuration")]
        [TabGroup("Collectibles")]
        [LabelText("収集アイテム配置設定")]
        public CollectiblePlacementSettings CollectiblePlacement = new CollectiblePlacementSettings();

        [Header("Hazards Configuration")]
        [TabGroup("Hazards")]
        [LabelText("ハザード設定")]
        public HazardGenerationSettings HazardGeneration = new HazardGenerationSettings();

        [Header("Performance Configuration")]
        [TabGroup("Performance")]
        [LabelText("パフォーマンス設定")]
        public LevelPerformanceSettings Performance = new LevelPerformanceSettings();

        [Header("Debug Configuration")]
        [TabGroup("Debug")]
        [LabelText("デバッグ設定")]
        public LevelDebugSettings Debug = new LevelDebugSettings();

        [System.Serializable]
        public class ProceduralGenerationSettings
        {
            [LabelText("シード値")]
            [Tooltip("ランダム生成のシード値（0で毎回異なる）")]
            public int Seed = 0;

            [LabelText("チャンク生成サイズ")]
            [Range(10f, 100f)]
            public float ChunkSize = 50f;

            [LabelText("プリロード距離")]
            [Range(50f, 200f)]
            [Tooltip("プレイヤー前方にプリロードする距離")]
            public float PreloadDistance = 100f;

            [LabelText("アンロード距離")]
            [Range(100f, 300f)]
            [Tooltip("プレイヤー後方でアンロードする距離")]
            public float UnloadDistance = 150f;

            [LabelText("バリエーション設定")]
            public VariationSettings Variations = new VariationSettings();
        }

        [System.Serializable]
        public class VariationSettings
        {
            [LabelText("地形バリエーション")]
            [Range(0.1f, 1.0f)]
            public float TerrainVariation = 0.5f;

            [LabelText("高度バリエーション")]
            [Range(0.1f, 1.0f)]
            public float HeightVariation = 0.3f;

            [LabelText("密度バリエーション")]
            [Range(0.1f, 1.0f)]
            public float DensityVariation = 0.4f;
        }

        [System.Serializable]
        public class PlatformGenerationSettings
        {
            [LabelText("プラットフォーム密度")]
            [Range(0.1f, 2.0f)]
            public float PlatformDensity = 1.0f;

            [LabelText("最小プラットフォーム幅")]
            [Range(1f, 10f)]
            public float MinPlatformWidth = 2f;

            [LabelText("最大プラットフォーム幅")]
            [Range(5f, 20f)]
            public float MaxPlatformWidth = 8f;

            [LabelText("ジャンプ距離")]
            [Range(2f, 15f)]
            [Tooltip("プラットフォーム間の最大ジャンプ可能距離")]
            public float JumpDistance = 6f;

            [LabelText("移動プラットフォーム率")]
            [Range(0f, 0.5f)]
            [Tooltip("移動プラットフォームの生成確率")]
            public float MovingPlatformRate = 0.2f;

            [LabelText("落下プラットフォーム率")]
            [Range(0f, 0.3f)]
            [Tooltip("落下プラットフォームの生成確率")]
            public float FallingPlatformRate = 0.1f;
        }

        [System.Serializable]
        public class CollectiblePlacementSettings
        {
            [LabelText("アイテム密度")]
            [Range(0.1f, 3.0f)]
            public float ItemDensity = 1.0f;

            [LabelText("レアアイテム率")]
            [Range(0f, 0.3f)]
            public float RareItemRate = 0.1f;

            [LabelText("隠しアイテム率")]
            [Range(0f, 0.2f)]
            public float HiddenItemRate = 0.05f;

            [LabelText("リスク報酬バランス")]
            [Range(0.1f, 2.0f)]
            [Tooltip("リスクの高い場所への高価値アイテム配置率")]
            public float RiskRewardBalance = 1.0f;

            [LabelText("アクセス難易度調整")]
            [Range(0.5f, 2.0f)]
            [Tooltip("アイテムへのアクセス難易度調整")]
            public float AccessDifficulty = 1.0f;
        }

        [System.Serializable]
        public class HazardGenerationSettings
        {
            [LabelText("ハザード密度")]
            [Range(0f, 2.0f)]
            public float HazardDensity = 0.5f;

            [LabelText("スパイク生成率")]
            [Range(0f, 0.5f)]
            public float SpikeRate = 0.2f;

            [LabelText("溶岩生成率")]
            [Range(0f, 0.3f)]
            public float LavaRate = 0.1f;

            [LabelText("敵配置率")]
            [Range(0f, 0.4f)]
            public float EnemyRate = 0.15f;

            [LabelText("ハザード警告システム")]
            public bool EnableHazardWarnings = true;

            [LabelText("難易度連動スケーリング")]
            public bool ScaleWithDifficulty = true;
        }

        [System.Serializable]
        public class LevelPerformanceSettings
        {
            [LabelText("オクルージョンカリング")]
            public bool EnableOcclusionCulling = true;

            [LabelText("LODシステム")]
            public bool EnableLODSystem = true;

            [LabelText("オブジェクトプーリング")]
            public bool EnableObjectPooling = true;

            [LabelText("最大同時オブジェクト数")]
            [Range(50, 500)]
            public int MaxConcurrentObjects = 200;

            [LabelText("ガベージコレクション最適化")]
            public bool OptimizeGarbageCollection = true;

            [LabelText("フレーム分散処理")]
            public bool EnableFrameDistribution = true;
        }

        [System.Serializable]
        public class LevelDebugSettings
        {
            [LabelText("デバッグ表示有効")]
            public bool EnableDebugDisplay = false;

            [LabelText("生成ログ有効")]
            public bool EnableGenerationLogs = false;

            [LabelText("プラットフォームデバッグ色")]
            public Color PlatformDebugColor = Color.green;

            [LabelText("ハザードデバッグ色")]
            public Color HazardDebugColor = Color.red;

            [LabelText("チャンク境界表示")]
            public bool ShowChunkBoundaries = false;

            [LabelText("パフォーマンス統計表示")]
            public bool ShowPerformanceStats = false;
        }

        /// <summary>
        /// 設定値検証
        /// </summary>
        [Button("設定値検証")]
        [TabGroup("Validation")]
        public void ValidateSettings()
        {
            bool isValid = true;

            if (LevelLength < 5 || LevelLength > 100)
            {
                UnityEngine.Debug.LogError("[PlatformerLevelSettings] Invalid LevelLength. Must be between 5 and 100");
                isValid = false;
            }

            if (DifficultyScale < 0.1f || DifficultyScale > 5.0f)
            {
                UnityEngine.Debug.LogError("[PlatformerLevelSettings] Invalid DifficultyScale. Must be between 0.1 and 5.0");
                isValid = false;
            }

            if (ProceduralGeneration.ChunkSize < 10f)
            {
                UnityEngine.Debug.LogError("[PlatformerLevelSettings] ChunkSize too small. Minimum 10 units required");
                isValid = false;
            }

            if (isValid)
            {
                UnityEngine.Debug.Log("[PlatformerLevelSettings] All settings are valid ✓");
            }
        }

        /// <summary>
        /// デフォルト設定復元
        /// </summary>
        [Button("デフォルト設定に復元")]
        [TabGroup("Validation")]
        public void ResetToDefaults()
        {
            LevelLength = 20;
            DifficultyScale = 1f;
            MaxHeight = 20f;

            ProceduralGeneration = new ProceduralGenerationSettings();
            PlatformGeneration = new PlatformGenerationSettings();
            CollectiblePlacement = new CollectiblePlacementSettings();
            HazardGeneration = new HazardGenerationSettings();
            Performance = new LevelPerformanceSettings();
            Debug = new LevelDebugSettings();

            UnityEngine.Debug.Log("[PlatformerLevelSettings] Settings reset to defaults");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// 難易度別プリセット適用
        /// </summary>
        [Button("Beginner")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyBeginnerPreset()
        {
            DifficultyScale = 0.6f;
            LevelLength = 15;
            HazardGeneration.HazardDensity = 0.2f;
            PlatformGeneration.JumpDistance = 4f;
            UnityEngine.Debug.Log("[PlatformerLevelSettings] Beginner preset applied");
        }

        [Button("Normal")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyNormalPreset()
        {
            DifficultyScale = 1.0f;
            LevelLength = 20;
            HazardGeneration.HazardDensity = 0.5f;
            PlatformGeneration.JumpDistance = 6f;
            UnityEngine.Debug.Log("[PlatformerLevelSettings] Normal preset applied");
        }

        [Button("Expert")]
        [TabGroup("Presets")]
        [HorizontalGroup("Presets/Difficulty")]
        public void ApplyExpertPreset()
        {
            DifficultyScale = 1.8f;
            LevelLength = 30;
            HazardGeneration.HazardDensity = 1.0f;
            PlatformGeneration.JumpDistance = 8f;
            UnityEngine.Debug.Log("[PlatformerLevelSettings] Expert preset applied");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ専用：設定プレビュー
        /// </summary>
        [Button("設定プレビュー")]
        [TabGroup("Debug")]
        public void PreviewSettings()
        {
            UnityEngine.Debug.Log("=== Platformer Level Settings Preview ===");
            UnityEngine.Debug.Log($"Level Length: {LevelLength} segments");
            UnityEngine.Debug.Log($"Difficulty Scale: {DifficultyScale:F1}x");
            UnityEngine.Debug.Log($"Max Height: {MaxHeight}m");
            UnityEngine.Debug.Log($"Platform Density: {PlatformGeneration.PlatformDensity:F1}");
            UnityEngine.Debug.Log($"Hazard Density: {HazardGeneration.HazardDensity:F1}");
            UnityEngine.Debug.Log($"Object Pooling: {Performance.EnableObjectPooling}");
        }
#endif

        private void OnValidate()
        {
            // Unity Inspector変更時の自動検証
            LevelLength = Mathf.Clamp(LevelLength, 5, 100);
            DifficultyScale = Mathf.Clamp(DifficultyScale, 0.1f, 5.0f);
            MaxHeight = Mathf.Max(5f, MaxHeight);

            if (ProceduralGeneration != null)
            {
                ProceduralGeneration.ChunkSize = Mathf.Max(10f, ProceduralGeneration.ChunkSize);
                ProceduralGeneration.PreloadDistance = Mathf.Max(ProceduralGeneration.ChunkSize, ProceduralGeneration.PreloadDistance);
            }
        }

        // 下位互換性のためのレガシープロパティ
        public void SetToDefault() => ResetToDefaults();
    }
}
