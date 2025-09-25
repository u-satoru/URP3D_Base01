using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Services;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Level Generation Service：プラットフォーマーレベル生成・配置・動的調整システム
    /// ServiceLocator統合：レベル生成・プロシージャル配置・パフォーマンス最適化・Learn & Grow価値実現
    /// Event駆動通信：レベル生成・配置完了・チャンク管理の疎結合通知
    /// </summary>
    public class LevelGenerationService : ILevelGenerationService
    {
        // 設定データ
        private PlatformerLevelSettings _settings;

        // レベル生成状態
        private int _currentLevelNumber = 0;
        private bool _isLevelGenerated = false;
        private bool _isGenerating = false;

        // チャンク管理
        private readonly Dictionary<Vector2Int, LevelChunkData> _activeChunks = new Dictionary<Vector2Int, LevelChunkData>();
        private readonly Queue<LevelChunkData> _chunkPool = new Queue<LevelChunkData>();

        // プロシージャル生成
        private System.Random _levelRandom;
        private Vector3 _playerPosition = Vector3.zero;
        private float _generatedDistance = 0f;

        // パフォーマンス統計
        private int _chunksGenerated = 0;
        private int _chunksDestroyed = 0;
        private float _generationTime = 0f;

        // Event駆動通信（疎結合）
        private GameEvent<LevelGenerationEventData> _onLevelGenerated;
        private GameEvent<ChunkEventData> _onChunkGenerated;
        private GameEvent<ChunkEventData> _onChunkDestroyed;
        private GameEvent<LevelProgressEventData> _onLevelProgress;

        // ServiceLocator連携フラグ
        private bool _isInitialized = false;

        // ==================================================
        // IPlatformerService 基底インターフェース実装
        // ==================================================

        // IPlatformerServiceで必要なプロパティ
        public bool IsInitialized { get; private set; } = false;
        public bool IsEnabled { get; private set; } = false;

        // プロパティ公開
        public int CurrentLevelNumber => _currentLevelNumber;
        public bool IsLevelGenerated => _isLevelGenerated;
        public bool IsGenerating => _isGenerating;
        public int ActiveChunksCount => _activeChunks.Count;
        public float GenerationProgress => _generatedDistance / (_settings.LevelLength * _settings.ProceduralGeneration.ChunkSize);

        /// <summary>
        /// コンストラクタ：設定ベース初期化
        /// </summary>
        public LevelGenerationService(PlatformerLevelSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // Event駆動通信の初期化
            InitializeEventChannels();

            Debug.Log("[LevelGenerationService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// レベル生成：Event駆動通知 + ServiceLocator連携
        /// </summary>
        public void GenerateLevel(int levelNumber)
        {
            if (_isGenerating)
            {
                Debug.LogWarning($"[LevelGenerationService] Level generation already in progress.");
                return;
            }

            _isGenerating = true;
            _currentLevelNumber = levelNumber;

            try
            {
                // ランダムシード設定
                int seed = _settings.ProceduralGeneration.Seed == 0 ?
                    UnityEngine.Random.Range(1, int.MaxValue) :
                    _settings.ProceduralGeneration.Seed + levelNumber;
                _levelRandom = new System.Random(seed);

                var startTime = Time.realtimeSinceStartup;

                // 既存レベルクリア
                ClearCurrentLevel();

                // ServiceLocator経由でゲームマネージャーと連携
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                var physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();

                // 初期チャンク生成
                GenerateInitialChunks();

                // コレクタブルアイテム配置
                PlaceCollectibles();

                _generationTime = Time.realtimeSinceStartup - startTime;
                _isLevelGenerated = true;
                _isGenerating = false;

                // Event駆動通信：レベル生成完了通知
                var eventData = new LevelGenerationEventData
                {
                    LevelNumber = levelNumber,
                    GenerationTime = _generationTime,
                    ChunkCount = _activeChunks.Count,
                    Seed = seed,
                    DifficultyScale = _settings.DifficultyScale
                };

                _onLevelGenerated?.Raise(eventData);

                Debug.Log($"[LevelGenerationService] Level {levelNumber} generated successfully in {_generationTime:F3}s with {_activeChunks.Count} chunks (Seed: {seed})");
            }
            catch (Exception ex)
            {
                _isGenerating = false;
                Debug.LogError($"[LevelGenerationService] Failed to generate level {levelNumber}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 現在レベルクリア：リソース解放とプール活用
        /// </summary>
        public void ClearCurrentLevel()
        {
            foreach (var chunk in _activeChunks.Values)
            {
                DestroyChunk(chunk);
            }

            _activeChunks.Clear();
            _generatedDistance = 0f;
            _isLevelGenerated = false;

            Debug.Log("[LevelGenerationService] Current level cleared and resources freed.");
        }

        /// <summary>
        /// プレイヤー位置更新：動的チャンク管理
        /// </summary>
        public void UpdatePlayerPosition(Vector3 playerPosition)
        {
            _playerPosition = playerPosition;

            // プロシージャル生成：前方プリロード
            float preloadDistance = _playerPosition.x + _settings.ProceduralGeneration.PreloadDistance;
            if (preloadDistance > _generatedDistance)
            {
                GenerateForwardChunks(preloadDistance);
            }

            // パフォーマンス最適化：後方アンロード
            float unloadDistance = _playerPosition.x - _settings.ProceduralGeneration.UnloadDistance;
            UnloadDistantChunks(unloadDistance);

            // 進捗通知
            var progressData = new LevelProgressEventData
            {
                PlayerPosition = playerPosition,
                Progress = GenerationProgress,
                ActiveChunks = _activeChunks.Count
            };
            _onLevelProgress?.Raise(progressData);
        }

        /// <summary>
        /// 初期チャンク生成：基本レベル構造
        /// </summary>
        private void GenerateInitialChunks()
        {
            float chunkSize = _settings.ProceduralGeneration.ChunkSize;
            int initialChunks = Mathf.CeilToInt(_settings.ProceduralGeneration.PreloadDistance / chunkSize);

            for (int i = 0; i < initialChunks; i++)
            {
                Vector2Int chunkCoord = new Vector2Int(i, 0);
                GenerateChunk(chunkCoord);
            }

            _generatedDistance = initialChunks * chunkSize;
        }

        /// <summary>
        /// 前方チャンク生成：プロシージャル拡張
        /// </summary>
        private void GenerateForwardChunks(float targetDistance)
        {
            float chunkSize = _settings.ProceduralGeneration.ChunkSize;
            int chunksNeeded = Mathf.CeilToInt((targetDistance - _generatedDistance) / chunkSize);

            for (int i = 0; i < chunksNeeded; i++)
            {
                int chunkX = Mathf.FloorToInt(_generatedDistance / chunkSize);
                Vector2Int chunkCoord = new Vector2Int(chunkX + i + 1, 0);

                if (!_activeChunks.ContainsKey(chunkCoord))
                {
                    GenerateChunk(chunkCoord);
                }
            }

            _generatedDistance = targetDistance;
        }

        /// <summary>
        /// チャンク生成：プラットフォーム・ハザード・コレクタブル配置
        /// </summary>
        private void GenerateChunk(Vector2Int chunkCoord)
        {
            var chunk = GetPooledChunk();
            chunk.Coordinate = chunkCoord;
            chunk.Position = new Vector3(
                chunkCoord.x * _settings.ProceduralGeneration.ChunkSize,
                0,
                chunkCoord.y * _settings.ProceduralGeneration.ChunkSize
            );

            // プラットフォーム生成
            GeneratePlatforms(chunk);

            // ハザード配置
            if (_settings.HazardGeneration.HazardDensity > 0)
            {
                GenerateHazards(chunk);
            }

            _activeChunks[chunkCoord] = chunk;
            _chunksGenerated++;

            // Event駆動通信：チャンク生成通知
            var eventData = new ChunkEventData
            {
                ChunkCoordinate = chunkCoord,
                ChunkPosition = chunk.Position,
                PlatformCount = chunk.Platforms.Count,
                HazardCount = chunk.Hazards.Count
            };
            _onChunkGenerated?.Raise(eventData);

            if (_settings.Debug.EnableGenerationLogs)
            {
                Debug.Log($"[LevelGenerationService] Generated chunk {chunkCoord} with {chunk.Platforms.Count} platforms and {chunk.Hazards.Count} hazards");
            }
        }

        /// <summary>
        /// プラットフォーム生成：物理演算ベースの配置
        /// </summary>
        private void GeneratePlatforms(LevelChunkData chunk)
        {
            var platformSettings = _settings.PlatformGeneration;
            float chunkSize = _settings.ProceduralGeneration.ChunkSize;
            float density = platformSettings.PlatformDensity * _settings.DifficultyScale;

            int platformCount = Mathf.RoundToInt(density * (chunkSize / 10f));

            for (int i = 0; i < platformCount; i++)
            {
                var platform = new PlatformData
                {
                    Position = new Vector3(
                        chunk.Position.x + _levelRandom.Next(0, (int)chunkSize),
                        _levelRandom.Next(0, (int)_settings.MaxHeight),
                        chunk.Position.z
                    ),
                    Width = Mathf.Lerp(platformSettings.MinPlatformWidth, platformSettings.MaxPlatformWidth, (float)_levelRandom.NextDouble()),
                    PlatformType = DeterminePlatformType(platformSettings)
                };

                chunk.Platforms.Add(platform);
            }
        }

        /// <summary>
        /// ハザード生成：難易度連動配置
        /// </summary>
        private void GenerateHazards(LevelChunkData chunk)
        {
            var hazardSettings = _settings.HazardGeneration;
            float chunkSize = _settings.ProceduralGeneration.ChunkSize;

            float density = hazardSettings.HazardDensity;
            if (hazardSettings.ScaleWithDifficulty)
            {
                density *= _settings.DifficultyScale;
            }

            int hazardCount = Mathf.RoundToInt(density * (chunkSize / 15f));

            for (int i = 0; i < hazardCount; i++)
            {
                var hazard = new HazardData
                {
                    Position = new Vector3(
                        chunk.Position.x + _levelRandom.Next(0, (int)chunkSize),
                        _levelRandom.Next(0, (int)(_settings.MaxHeight * 0.7f)),
                        chunk.Position.z
                    ),
                    HazardType = DetermineHazardType(hazardSettings)
                };

                chunk.Hazards.Add(hazard);
            }
        }

        /// <summary>
        /// コレクタブルアイテム配置：CollectionServiceとの統合
        /// </summary>
        private void PlaceCollectibles()
        {
            var collectionService = ServiceLocator.GetService<ICollectionService>();
            var placementSettings = _settings.CollectiblePlacement;

            var collectibles = new List<CollectibleItemData>();
            int totalItems = Mathf.RoundToInt(_settings.LevelLength * placementSettings.ItemDensity);

            for (int i = 0; i < totalItems; i++)
            {
                var item = new CollectibleItemData
                {
                    ItemId = i,
                    ItemName = $"Item_{_currentLevelNumber}_{i}",
                    Score = CalculateItemScore(placementSettings),
                    IsRequired = _levelRandom.NextDouble() < 0.1, // 10% are required
                    Position = GenerateItemPosition(),
                    Description = "Procedurally placed collectible item"
                };

                collectibles.Add(item);
            }

            collectionService?.InitializeLevel(collectibles.ToArray());
            Debug.Log($"[LevelGenerationService] Placed {collectibles.Count} collectible items for level {_currentLevelNumber}");
        }

        /// <summary>
        /// 遠距離チャンクアンロード：メモリ最適化
        /// </summary>
        private void UnloadDistantChunks(float unloadDistance)
        {
            var chunksToRemove = new List<Vector2Int>();

            foreach (var kvp in _activeChunks)
            {
                if (kvp.Value.Position.x < unloadDistance)
                {
                    chunksToRemove.Add(kvp.Key);
                }
            }

            foreach (var chunkCoord in chunksToRemove)
            {
                var chunk = _activeChunks[chunkCoord];
                DestroyChunk(chunk);
                _activeChunks.Remove(chunkCoord);
            }

            if (chunksToRemove.Count > 0 && _settings.Debug.EnableGenerationLogs)
            {
                Debug.Log($"[LevelGenerationService] Unloaded {chunksToRemove.Count} distant chunks for performance optimization");
            }
        }

        /// <summary>
        /// チャンク破棄：オブジェクトプール活用
        /// </summary>
        private void DestroyChunk(LevelChunkData chunk)
        {
            // Event駆動通信：チャンク破棄通知
            var eventData = new ChunkEventData
            {
                ChunkCoordinate = chunk.Coordinate,
                ChunkPosition = chunk.Position,
                PlatformCount = chunk.Platforms.Count,
                HazardCount = chunk.Hazards.Count
            };
            _onChunkDestroyed?.Raise(eventData);

            // オブジェクトプール活用
            chunk.Reset();
            _chunkPool.Enqueue(chunk);
            _chunksDestroyed++;
        }

        /// <summary>
        /// プールからチャンク取得：メモリ効率化
        /// </summary>
        private LevelChunkData GetPooledChunk()
        {
            if (_chunkPool.Count > 0)
            {
                return _chunkPool.Dequeue();
            }
            return new LevelChunkData();
        }

        /// <summary>
        /// プラットフォームタイプ決定：確率ベース
        /// </summary>
        private PlatformType DeterminePlatformType(PlatformerLevelSettings.PlatformGenerationSettings settings)
        {
            double rand = _levelRandom.NextDouble();

            if (rand < settings.FallingPlatformRate)
                return PlatformType.Falling;
            else if (rand < settings.FallingPlatformRate + settings.MovingPlatformRate)
                return PlatformType.Moving;
            else
                return PlatformType.Static;
        }

        /// <summary>
        /// ハザードタイプ決定：確率ベース
        /// </summary>
        private HazardType DetermineHazardType(PlatformerLevelSettings.HazardGenerationSettings settings)
        {
            double rand = _levelRandom.NextDouble();
            double total = settings.SpikeRate + settings.LavaRate + settings.EnemyRate;

            if (total == 0) return HazardType.Spike;

            rand *= total;

            if (rand < settings.SpikeRate)
                return HazardType.Spike;
            else if (rand < settings.SpikeRate + settings.LavaRate)
                return HazardType.Lava;
            else
                return HazardType.Enemy;
        }

        /// <summary>
        /// アイテムスコア計算：リスク報酬バランス
        /// </summary>
        private int CalculateItemScore(PlatformerLevelSettings.CollectiblePlacementSettings settings)
        {
            int baseScore = 100;
            float riskMultiplier = 1f + (settings.RiskRewardBalance - 1f) * (float)_levelRandom.NextDouble();

            if (_levelRandom.NextDouble() < settings.RareItemRate)
            {
                baseScore *= 5; // レアアイテム
            }

            return Mathf.RoundToInt(baseScore * riskMultiplier * _settings.DifficultyScale);
        }

        /// <summary>
        /// アイテム位置生成：配置最適化
        /// </summary>
        private Vector3 GenerateItemPosition()
        {
            return new Vector3(
                _levelRandom.Next(0, _settings.LevelLength) * _settings.ProceduralGeneration.ChunkSize / _settings.LevelLength,
                _levelRandom.Next(1, (int)_settings.MaxHeight),
                0
            );
        }

        /// <summary>
        /// Event駆動通信チャネル初期化
        /// </summary>
        private void InitializeEventChannels()
        {
            // NOTE: 実際の実装では、これらのEventChannelはScriptableObjectとして
            // プロジェクト内で作成・管理される
            // ここでは構造を示すためのプレースホルダー

            // _onLevelGenerated = Resources.Load<GameEvent<LevelGenerationEventData>>("Events/OnLevelGenerated");
            // _onChunkGenerated = Resources.Load<GameEvent<ChunkEventData>>("Events/OnChunkGenerated");
            // _onChunkDestroyed = Resources.Load<GameEvent<ChunkEventData>>("Events/OnChunkDestroyed");
            // _onLevelProgress = Resources.Load<GameEvent<LevelProgressEventData>>("Events/OnLevelProgress");

            Debug.Log("[LevelGenerationService] Event channels initialized for loose coupling communication.");
        }

        /// <summary>
        /// ServiceLocator統合検証
        /// </summary>
        public bool VerifyServiceLocatorIntegration()
        {
            try
            {
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                var physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();
                var collectionService = ServiceLocator.GetService<ICollectionService>();

                bool integration = gameManager != null && physicsService != null && collectionService != null;
                Debug.Log($"[LevelGenerationService] ServiceLocator integration verified: {integration}");
                return integration;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LevelGenerationService] ServiceLocator integration failed: {ex.Message}");
                return false;
            }
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            InitializeEventChannels();
            IsInitialized = true;
            Debug.Log("[LevelGenerationService] Initialized successfully.");
        }

        public void Enable()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[LevelGenerationService] Cannot enable - not initialized yet.");
                return;
            }

            IsEnabled = true;
            Debug.Log("[LevelGenerationService] Enabled.");
        }

        public void Disable()
        {
            IsEnabled = false;
            Debug.Log("[LevelGenerationService] Disabled.");
        }

        public void Reset()
        {
            ClearCurrentLevel();
            _currentLevelNumber = 0;
            _generatedDistance = 0f;
            _isLevelGenerated = false;
            _isGenerating = false;
            _chunksGenerated = 0;
            _chunksDestroyed = 0;
            _generationTime = 0f;
            Debug.Log("[LevelGenerationService] Reset completed.");
        }

        public void UpdateService(float deltaTime)
        {
            if (!IsEnabled) return;
            // サービス更新処理（必要に応じて実装）
            // 例：動的レベル調整、パフォーマンス監視など
        }

        /// <summary>
        /// 設定更新：ランタイム設定変更対応
        /// </summary>
        public void UpdateSettings(PlatformerLevelSettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
            Debug.Log("[LevelGenerationService] Settings updated at runtime.");
        }

        /// <summary>
        /// リソース解放：IDisposable実装
        /// </summary>
        public void Dispose()
        {
            ClearCurrentLevel();
            _chunkPool.Clear();

            // Event購読解除（実装時に必要）
            // if (_onLevelGenerated != null) _onLevelGenerated.RemoveAllListeners();

            Debug.Log("[LevelGenerationService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用診断情報：開発支援機能
        /// </summary>
        [ContextMenu("Show Level Generation Debug Info")]
        public void ShowLevelGenerationDebugInfo()
        {
            Debug.Log("=== Level Generation Service Diagnostic ===");
            Debug.Log($"Current Level: {_currentLevelNumber}");
            Debug.Log($"Level Generated: {_isLevelGenerated}");
            Debug.Log($"Is Generating: {_isGenerating}");
            Debug.Log($"Active Chunks: {_activeChunks.Count}");
            Debug.Log($"Generated Distance: {_generatedDistance:F1}m");
            Debug.Log($"Generation Progress: {GenerationProgress:P}");
            Debug.Log($"Generation Time: {_generationTime:F3}s");
            Debug.Log($"Chunks Generated: {_chunksGenerated}");
            Debug.Log($"Chunks Destroyed: {_chunksDestroyed}");
            Debug.Log($"ServiceLocator Integration: {VerifyServiceLocatorIntegration()}");
        }
#endif
    }

    /// <summary>
    /// レベル生成イベントデータ構造
    /// </summary>
    [System.Serializable]
    public struct LevelGenerationEventData
    {
        public int LevelNumber;
        public float GenerationTime;
        public int ChunkCount;
        public int Seed;
        public float DifficultyScale;
    }

    /// <summary>
    /// チャンクイベントデータ構造
    /// </summary>
    [System.Serializable]
    public struct ChunkEventData
    {
        public Vector2Int ChunkCoordinate;
        public Vector3 ChunkPosition;
        public int PlatformCount;
        public int HazardCount;
    }

    /// <summary>
    /// レベル進捗イベントデータ構造
    /// </summary>
    [System.Serializable]
    public struct LevelProgressEventData
    {
        public Vector3 PlayerPosition;
        public float Progress;
        public int ActiveChunks;
    }

    /// <summary>
    /// レベルチャンクデータ構造
    /// </summary>
    [System.Serializable]
    public class LevelChunkData
    {
        public Vector2Int Coordinate;
        public Vector3 Position;
        public List<PlatformData> Platforms = new List<PlatformData>();
        public List<HazardData> Hazards = new List<HazardData>();
        public List<CollectibleItemData> Collectibles = new List<CollectibleItemData>();

        public void Reset()
        {
            Coordinate = Vector2Int.zero;
            Position = Vector3.zero;
            Platforms.Clear();
            Hazards.Clear();
            Collectibles.Clear();
        }
    }

    /// <summary>
    /// プラットフォームデータ構造
    /// </summary>
    [System.Serializable]
    public struct PlatformData
    {
        public Vector3 Position;
        public float Width;
        public PlatformType PlatformType;
    }

    /// <summary>
    /// ハザードデータ構造
    /// </summary>
    [System.Serializable]
    public struct HazardData
    {
        public Vector3 Position;
        public HazardType HazardType;
    }

    /// <summary>
    /// プラットフォームタイプ列挙
    /// </summary>
    public enum PlatformType
    {
        Static,     // 静的プラットフォーム
        Moving,     // 移動プラットフォーム
        Falling     // 落下プラットフォーム
    }

    /// <summary>
    /// ハザードタイプ列挙
    /// </summary>
    public enum HazardType
    {
        Spike,      // スパイク
        Lava,       // 溶岩
        Enemy       // 敵
    }
}
