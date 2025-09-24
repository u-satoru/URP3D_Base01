using UnityEngine;

namespace asterivo.Unity60.Features.Templates.Stealth.Configuration
{
    /// <summary>
    /// Layer 1: ステルスパフォーマンス設定
    /// 50体NPC同時稼働、0.1ms/frame処理時間制限
    /// ObjectPool最適化、LODシステム統合
    /// </summary>
    [System.Serializable]
    public class StealthPerformanceConfig
    {
        [Header("NPC Performance Limits")]
        [Range(1, 100)]
        [Tooltip("最大同時稼働NPC数")]
        public int MaxSimultaneousNPCs = 50;

        [Range(0.01f, 0.2f)]
        [Tooltip("フレーム当たりの最大処理時間（ms）")]
        public float MaxFrameTimeMs = 0.1f;

        [Range(1, 20)]
        [Tooltip("フレーム分散処理のバッチサイズ")]
        public int ProcessingBatchSize = 10;

        [Header("LOD System Integration")]
        [Tooltip("距離ベースLOD有効化")]
        public bool EnableDistanceBasedLOD = true;

        [Range(5f, 50f)]
        [Tooltip("高品質処理距離")]
        public float HighQualityDistance = 20f;

        [Range(10f, 100f)]
        [Tooltip("中品質処理距離")]
        public float MediumQualityDistance = 40f;

        [Range(20f, 200f)]
        [Tooltip("低品質処理距離")]
        public float LowQualityDistance = 80f;

        [Header("Update Frequency Scaling")]
        [Tooltip("更新頻度スケーリング有効化")]
        public bool EnableUpdateFrequencyScaling = true;

        [Range(60f, 120f)]
        [Tooltip("高品質更新頻度（Hz）")]
        public float HighQualityUpdateRate = 60f;

        [Range(20f, 60f)]
        [Tooltip("中品質更新頻度（Hz）")]
        public float MediumQualityUpdateRate = 30f;

        [Range(5f, 30f)]
        [Tooltip("低品質更新頻度（Hz）")]
        public float LowQualityUpdateRate = 10f;

        [Header("ObjectPool Optimization")]
        [Tooltip("ObjectPool最適化有効化")]
        public bool EnableObjectPoolOptimization = true;

        [Range(10, 500)]
        [Tooltip("検知イベントプールサイズ")]
        public int DetectionEventPoolSize = 100;

        [Range(10, 200)]
        [Tooltip("メモリオブジェクトプールサイズ")]
        public int MemoryObjectPoolSize = 50;

        [Range(5, 100)]
        [Tooltip("コマンドオブジェクトプールサイズ")]
        public int CommandObjectPoolSize = 25;

        [Header("Memory Management")]
        [Range(0.5f, 10f)]
        [Tooltip("ガベージコレクション間隔（秒）")]
        public float GCInterval = 2f;

        [Range(1, 50)]
        [Tooltip("NPCあたりの最大メモリ使用量（KB）")]
        public int MaxMemoryPerNPCKB = 5;

        [Tooltip("自動メモリクリーンアップ")]
        public bool EnableAutoMemoryCleanup = true;

        [Header("Culling & Optimization")]
        [Tooltip("視界外カリング有効化")]
        public bool EnableFrustumCulling = true;

        [Tooltip("オクルージョンカリング有効化")]
        public bool EnableOcclusionCulling = true;

        [Range(0.1f, 5f)]
        [Tooltip("カリング更新間隔（秒）")]
        public float CullingUpdateInterval = 1f;

        [Header("Threading & Async Processing")]
        [Tooltip("非同期処理有効化")]
        public bool EnableAsyncProcessing = true;

        [Range(1, 8)]
        [Tooltip("ワーカースレッド数")]
        public int WorkerThreadCount = 2;

        [Tooltip("メインスレッドブロッキング回避")]
        public bool AvoidMainThreadBlocking = true;

        [Header("Performance Monitoring")]
        [Tooltip("リアルタイムパフォーマンス監視")]
        public bool EnableRealTimePerformanceMonitoring = true;

        [Range(0.1f, 5f)]
        [Tooltip("パフォーマンス測定間隔（秒）")]
        public float PerformanceMeasurementInterval = 1f;

        [Tooltip("パフォーマンス劣化時の自動調整")]
        public bool EnableAutoPerformanceAdjustment = true;

        [Range(0.05f, 0.5f)]
        [Tooltip("パフォーマンス劣化検知閾値（ms）")]
        public float PerformanceDegradationThreshold = 0.15f;

        [Header("Quality vs Performance Trade-offs")]
        [Range(0f, 1f)]
        [Tooltip("品質重視度（0=パフォーマンス重視、1=品質重視）")]
        public float QualityPerformanceBalance = 0.7f;

        [Tooltip("動的品質調整有効化")]
        public bool EnableDynamicQualityAdjustment = true;

        [Range(30f, 120f)]
        [Tooltip("目標フレームレート")]
        public float TargetFrameRate = 60f;

        /// <summary>
        /// 50体NPC対応最適化設定
        /// 0.1ms/frame処理時間制限準拠
        /// </summary>
        public void ApplyFiftyNPCOptimizedSettings()
        {
            MaxSimultaneousNPCs = 50;
            MaxFrameTimeMs = 0.1f;
            ProcessingBatchSize = 10; // 5フレームで全NPC処理

            // LODシステム最適化
            EnableDistanceBasedLOD = true;
            HighQualityDistance = 15f;
            MediumQualityDistance = 30f;
            LowQualityDistance = 60f;

            // 更新頻度スケーリング
            EnableUpdateFrequencyScaling = true;
            HighQualityUpdateRate = 60f;
            MediumQualityUpdateRate = 20f;
            LowQualityUpdateRate = 5f;

            // ObjectPool最適化
            EnableObjectPoolOptimization = true;
            DetectionEventPoolSize = 150; // 50NPC * 3イベント
            MemoryObjectPoolSize = 75; // 50NPC * 1.5
            CommandObjectPoolSize = 50; // 50NPC * 1

            // メモリ管理
            MaxMemoryPerNPCKB = 5; // 合計250KB
            EnableAutoMemoryCleanup = true;
            GCInterval = 3f;

            // カリング最適化
            EnableFrustumCulling = true;
            EnableOcclusionCulling = true;
            CullingUpdateInterval = 0.5f;

            // 非同期処理
            EnableAsyncProcessing = true;
            WorkerThreadCount = 2;
            AvoidMainThreadBlocking = true;

            // パフォーマンス監視
            EnableRealTimePerformanceMonitoring = true;
            EnableAutoPerformanceAdjustment = true;
            PerformanceDegradationThreshold = 0.12f;

            QualityPerformanceBalance = 0.6f; // やや性能重視
            EnableDynamicQualityAdjustment = true;
            TargetFrameRate = 60f;
        }

        /// <summary>
        /// 高品質設定（NPC数制限）
        /// </summary>
        public void ApplyHighQualitySettings()
        {
            MaxSimultaneousNPCs = 25; // 品質重視で制限
            MaxFrameTimeMs = 0.2f;

            EnableDistanceBasedLOD = false; // 常に高品質
            EnableUpdateFrequencyScaling = false;

            HighQualityUpdateRate = 120f;
            MaxMemoryPerNPCKB = 10; // より多くのメモリ使用許可

            EnableFrustumCulling = false;
            EnableOcclusionCulling = false;

            QualityPerformanceBalance = 1f; // 完全品質重視
            EnableDynamicQualityAdjustment = false;
        }

        /// <summary>
        /// 最大パフォーマンス設定
        /// </summary>
        public void ApplyMaxPerformanceSettings()
        {
            MaxSimultaneousNPCs = 100; // 最大値
            MaxFrameTimeMs = 0.05f; // より厳しい制限
            ProcessingBatchSize = 20;

            // 積極的LOD
            EnableDistanceBasedLOD = true;
            HighQualityDistance = 10f;
            MediumQualityDistance = 20f;
            LowQualityDistance = 40f;

            // 低頻度更新
            HighQualityUpdateRate = 30f;
            MediumQualityUpdateRate = 15f;
            LowQualityUpdateRate = 5f;

            // メモリ制限
            MaxMemoryPerNPCKB = 3;
            GCInterval = 1f;

            // 積極的カリング
            EnableFrustumCulling = true;
            EnableOcclusionCulling = true;
            CullingUpdateInterval = 0.25f;

            QualityPerformanceBalance = 0f; // 完全性能重視
            PerformanceDegradationThreshold = 0.08f;
        }

        /// <summary>
        /// モバイル向け設定
        /// </summary>
        public void ApplyMobileSettings()
        {
            MaxSimultaneousNPCs = 20;
            MaxFrameTimeMs = 0.08f;
            ProcessingBatchSize = 5;

            EnableDistanceBasedLOD = true;
            HighQualityDistance = 8f;
            MediumQualityDistance = 15f;
            LowQualityDistance = 25f;

            // 低頻度更新
            HighQualityUpdateRate = 30f;
            MediumQualityUpdateRate = 10f;
            LowQualityUpdateRate = 3f;

            // メモリ節約
            DetectionEventPoolSize = 30;
            MemoryObjectPoolSize = 25;
            CommandObjectPoolSize = 15;
            MaxMemoryPerNPCKB = 2;

            EnableAsyncProcessing = false; // モバイルでは無効
            WorkerThreadCount = 1;

            QualityPerformanceBalance = 0.3f;
            TargetFrameRate = 30f; // モバイル向け
        }

        /// <summary>
        /// 設定の検証
        /// パフォーマンス要件との整合性確認
        /// </summary>
        public bool ValidateSettings()
        {
            bool isValid = true;

            // 50体NPC要件確認
            if (MaxSimultaneousNPCs < 50)
            {
                Debug.LogWarning("StealthPerformanceConfig: MaxSimultaneousNPCs is below 50 NPC requirement");
            }

            // 0.1ms/frame要件確認
            if (MaxFrameTimeMs > 0.1f)
            {
                Debug.LogWarning("StealthPerformanceConfig: MaxFrameTimeMs exceeds 0.1ms requirement");
            }

            // メモリ使用量確認
            float totalMemoryKB = MaxSimultaneousNPCs * MaxMemoryPerNPCKB;
            if (totalMemoryKB > 500) // 500KB上限
            {
                Debug.LogWarning($"StealthPerformanceConfig: Total memory usage ({totalMemoryKB}KB) exceeds recommended limit");
            }

            // LOD距離の論理確認
            if (HighQualityDistance >= MediumQualityDistance)
            {
                Debug.LogWarning("StealthPerformanceConfig: HighQualityDistance should be less than MediumQualityDistance");
                isValid = false;
            }

            // ObjectPool設定確認
            if (DetectionEventPoolSize < MaxSimultaneousNPCs)
            {
                Debug.LogWarning("StealthPerformanceConfig: DetectionEventPoolSize may be insufficient for max NPCs");
            }

            return isValid;
        }

        /// <summary>
        /// 推定メモリ使用量計算（KB）
        /// </summary>
        public float CalculateEstimatedMemoryUsageKB()
        {
            float npcMemory = MaxSimultaneousNPCs * MaxMemoryPerNPCKB;
            float poolMemory = (DetectionEventPoolSize * 0.5f) + (MemoryObjectPoolSize * 1f) + (CommandObjectPoolSize * 0.3f);
            
            return npcMemory + poolMemory;
        }

        /// <summary>
        /// 推定フレーム処理時間計算（ms）
        /// </summary>
        public float CalculateEstimatedFrameTimeMs()
        {
            // バッチ処理を考慮した推定値
            float baseProcessingTime = (MaxSimultaneousNPCs / (float)ProcessingBatchSize) * 0.02f;
            
            // LODによる軽減効果
            if (EnableDistanceBasedLOD)
            {
                baseProcessingTime *= 0.7f; // 30%軽減
            }

            return Mathf.Min(baseProcessingTime, MaxFrameTimeMs);
        }
    }
}