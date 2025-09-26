using System;
using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.Templates.Platformer.Settings;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// Level Generation Service・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝槭・繝ｬ繝吶Ν逕滓・繝ｻ驟咲ｽｮ繝ｻ蜍慕噪隱ｿ謨ｴ繧ｷ繧ｹ繝・Β
    /// ServiceLocator邨ｱ蜷茨ｼ壹Ξ繝吶Ν逕滓・繝ｻ繝励Ο繧ｷ繝ｼ繧ｸ繝｣繝ｫ驟咲ｽｮ繝ｻ繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹悶・Learn & Grow萓｡蛟､螳溽樟
    /// Event鬧・虚騾壻ｿ｡・壹Ξ繝吶Ν逕滓・繝ｻ驟咲ｽｮ螳御ｺ・・繝√Ε繝ｳ繧ｯ邂｡逅・・逍守ｵ仙粋騾夂衍
    /// </summary>
    public class LevelGenerationService : ILevelGenerationService
    {
        // 險ｭ螳壹ョ繝ｼ繧ｿ
        private PlatformerLevelSettings _settings;

        // 繝ｬ繝吶Ν逕滓・迥ｶ諷・
        private int _currentLevelNumber = 0;
        private bool _isLevelGenerated = false;
        private bool _isGenerating = false;

        // 繝√Ε繝ｳ繧ｯ邂｡逅・
        private readonly Dictionary<Vector2Int, LevelChunkData> _activeChunks = new Dictionary<Vector2Int, LevelChunkData>();
        private readonly Queue<LevelChunkData> _chunkPool = new Queue<LevelChunkData>();

        // 繝励Ο繧ｷ繝ｼ繧ｸ繝｣繝ｫ逕滓・
        private System.Random _levelRandom;
        private Vector3 _playerPosition = Vector3.zero;
        private float _generatedDistance = 0f;

        // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ邨ｱ險・
        private int _chunksGenerated = 0;
        private int _chunksDestroyed = 0;
        private float _generationTime = 0f;

        // Event鬧・虚騾壻ｿ｡・育鮪邨仙粋・・
        private GameEvent<LevelGenerationEventData> _onLevelGenerated;
        private GameEvent<ChunkEventData> _onChunkGenerated;
        private GameEvent<ChunkEventData> _onChunkDestroyed;
        private GameEvent<LevelProgressEventData> _onLevelProgress;

        // ServiceLocator騾｣謳ｺ繝輔Λ繧ｰ
        private bool _isInitialized = false;

        // ==================================================
        // IPlatformerService 蝓ｺ蠎輔う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・
        // ==================================================

        // IPlatformerService縺ｧ蠢・ｦ√↑繝励Ο繝代ユ繧｣
        public bool IsInitialized { get; private set; } = false;
        public bool IsEnabled { get; private set; } = false;

        // 繝励Ο繝代ユ繧｣蜈ｬ髢・
        public int CurrentLevelNumber => _currentLevelNumber;
        public bool IsLevelGenerated => _isLevelGenerated;
        public bool IsGenerating => _isGenerating;
        public int ActiveChunksCount => _activeChunks.Count;
        public float GenerationProgress => _generatedDistance / (_settings.LevelLength * _settings.ProceduralGeneration.ChunkSize);

        /// <summary>
        /// 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・夊ｨｭ螳壹・繝ｼ繧ｹ蛻晄悄蛹・
        /// </summary>
        public LevelGenerationService(PlatformerLevelSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // Event鬧・虚騾壻ｿ｡縺ｮ蛻晄悄蛹・
            InitializeEventChannels();

            Debug.Log("[LevelGenerationService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 繝ｬ繝吶Ν逕滓・・哘vent鬧・虚騾夂衍 + ServiceLocator騾｣謳ｺ
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
                // 繝ｩ繝ｳ繝繝繧ｷ繝ｼ繝芽ｨｭ螳・
                int seed = _settings.ProceduralGeneration.Seed == 0 ?
                    UnityEngine.Random.Range(1, int.MaxValue) :
                    _settings.ProceduralGeneration.Seed + levelNumber;
                _levelRandom = new System.Random(seed);

                var startTime = Time.realtimeSinceStartup;

                // 譌｢蟄倥Ξ繝吶Ν繧ｯ繝ｪ繧｢
                ClearCurrentLevel();

                // ServiceLocator邨檎罰縺ｧ繧ｲ繝ｼ繝繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｨ騾｣謳ｺ
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                var physicsService = ServiceLocator.GetService<IPlatformerPhysicsService>();

                // 蛻晄悄繝√Ε繝ｳ繧ｯ逕滓・
                GenerateInitialChunks();

                // 繧ｳ繝ｬ繧ｯ繧ｿ繝悶Ν繧｢繧､繝・Β驟咲ｽｮ
                PlaceCollectibles();

                _generationTime = Time.realtimeSinceStartup - startTime;
                _isLevelGenerated = true;
                _isGenerating = false;

                // Event鬧・虚騾壻ｿ｡・壹Ξ繝吶Ν逕滓・螳御ｺ・夂衍
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
        /// 迴ｾ蝨ｨ繝ｬ繝吶Ν繧ｯ繝ｪ繧｢・壹Μ繧ｽ繝ｼ繧ｹ隗｣謾ｾ縺ｨ繝励・繝ｫ豢ｻ逕ｨ
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
        /// 繝励Ξ繧､繝､繝ｼ菴咲ｽｮ譖ｴ譁ｰ・壼虚逧・メ繝｣繝ｳ繧ｯ邂｡逅・
        /// </summary>
        public void UpdatePlayerPosition(Vector3 playerPosition)
        {
            _playerPosition = playerPosition;

            // 繝励Ο繧ｷ繝ｼ繧ｸ繝｣繝ｫ逕滓・・壼燕譁ｹ繝励Μ繝ｭ繝ｼ繝・
            float preloadDistance = _playerPosition.x + _settings.ProceduralGeneration.PreloadDistance;
            if (preloadDistance > _generatedDistance)
            {
                GenerateForwardChunks(preloadDistance);
            }

            // 繝代ヵ繧ｩ繝ｼ繝槭Φ繧ｹ譛驕ｩ蛹厄ｼ壼ｾ梧婿繧｢繝ｳ繝ｭ繝ｼ繝・
            float unloadDistance = _playerPosition.x - _settings.ProceduralGeneration.UnloadDistance;
            UnloadDistantChunks(unloadDistance);

            // 騾ｲ謐鈴夂衍
            var progressData = new LevelProgressEventData
            {
                PlayerPosition = playerPosition,
                Progress = GenerationProgress,
                ActiveChunks = _activeChunks.Count
            };
            _onLevelProgress?.Raise(progressData);
        }

        /// <summary>
        /// 蛻晄悄繝√Ε繝ｳ繧ｯ逕滓・・壼渕譛ｬ繝ｬ繝吶Ν讒矩
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
        /// 蜑肴婿繝√Ε繝ｳ繧ｯ逕滓・・壹・繝ｭ繧ｷ繝ｼ繧ｸ繝｣繝ｫ諡｡蠑ｵ
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
        /// 繝√Ε繝ｳ繧ｯ逕滓・・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝繝ｻ繝上じ繝ｼ繝峨・繧ｳ繝ｬ繧ｯ繧ｿ繝悶Ν驟咲ｽｮ
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

            // 繝励Λ繝・ヨ繝輔か繝ｼ繝逕滓・
            GeneratePlatforms(chunk);

            // 繝上じ繝ｼ繝蛾・鄂ｮ
            if (_settings.HazardGeneration.HazardDensity > 0)
            {
                GenerateHazards(chunk);
            }

            _activeChunks[chunkCoord] = chunk;
            _chunksGenerated++;

            // Event鬧・虚騾壻ｿ｡・壹メ繝｣繝ｳ繧ｯ逕滓・騾夂衍
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝逕滓・・夂黄逅・ｼ皮ｮ励・繝ｼ繧ｹ縺ｮ驟咲ｽｮ
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
        /// 繝上じ繝ｼ繝臥函謌撰ｼ夐屮譏灘ｺｦ騾｣蜍暮・鄂ｮ
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
        /// 繧ｳ繝ｬ繧ｯ繧ｿ繝悶Ν繧｢繧､繝・Β驟咲ｽｮ・咾ollectionService縺ｨ縺ｮ邨ｱ蜷・
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
        /// 驕霍晞屬繝√Ε繝ｳ繧ｯ繧｢繝ｳ繝ｭ繝ｼ繝会ｼ壹Γ繝｢繝ｪ譛驕ｩ蛹・
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
        /// 繝√Ε繝ｳ繧ｯ遐ｴ譽・ｼ壹が繝悶ず繧ｧ繧ｯ繝医・繝ｼ繝ｫ豢ｻ逕ｨ
        /// </summary>
        private void DestroyChunk(LevelChunkData chunk)
        {
            // Event鬧・虚騾壻ｿ｡・壹メ繝｣繝ｳ繧ｯ遐ｴ譽・夂衍
            var eventData = new ChunkEventData
            {
                ChunkCoordinate = chunk.Coordinate,
                ChunkPosition = chunk.Position,
                PlatformCount = chunk.Platforms.Count,
                HazardCount = chunk.Hazards.Count
            };
            _onChunkDestroyed?.Raise(eventData);

            // 繧ｪ繝悶ず繧ｧ繧ｯ繝医・繝ｼ繝ｫ豢ｻ逕ｨ
            chunk.Reset();
            _chunkPool.Enqueue(chunk);
            _chunksDestroyed++;
        }

        /// <summary>
        /// 繝励・繝ｫ縺九ｉ繝√Ε繝ｳ繧ｯ蜿門ｾ暦ｼ壹Γ繝｢繝ｪ蜉ｹ邇・喧
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
        /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｿ繧､繝玲ｱｺ螳夲ｼ夂｢ｺ邇・・繝ｼ繧ｹ
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
        /// 繝上じ繝ｼ繝峨ち繧､繝玲ｱｺ螳夲ｼ夂｢ｺ邇・・繝ｼ繧ｹ
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
        /// 繧｢繧､繝・Β繧ｹ繧ｳ繧｢險育ｮ暦ｼ壹Μ繧ｹ繧ｯ蝣ｱ驟ｬ繝舌Λ繝ｳ繧ｹ
        /// </summary>
        private int CalculateItemScore(PlatformerLevelSettings.CollectiblePlacementSettings settings)
        {
            int baseScore = 100;
            float riskMultiplier = 1f + (settings.RiskRewardBalance - 1f) * (float)_levelRandom.NextDouble();

            if (_levelRandom.NextDouble() < settings.RareItemRate)
            {
                baseScore *= 5; // 繝ｬ繧｢繧｢繧､繝・Β
            }

            return Mathf.RoundToInt(baseScore * riskMultiplier * _settings.DifficultyScale);
        }

        /// <summary>
        /// 繧｢繧､繝・Β菴咲ｽｮ逕滓・・夐・鄂ｮ譛驕ｩ蛹・
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
        /// Event鬧・虚騾壻ｿ｡繝√Ε繝阪Ν蛻晄悄蛹・
        /// </summary>
        private void InitializeEventChannels()
        {
            // NOTE: 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√％繧後ｉ縺ｮEventChannel縺ｯScriptableObject縺ｨ縺励※
            // 繝励Ο繧ｸ繧ｧ繧ｯ繝亥・縺ｧ菴懈・繝ｻ邂｡逅・＆繧後ｋ
            // 縺薙％縺ｧ縺ｯ讒矩繧堤､ｺ縺吶◆繧√・繝励Ξ繝ｼ繧ｹ繝帙Ν繝繝ｼ

            // _onLevelGenerated = Resources.Load<GameEvent<LevelGenerationEventData>>("Events/OnLevelGenerated");
            // _onChunkGenerated = Resources.Load<GameEvent<ChunkEventData>>("Events/OnChunkGenerated");
            // _onChunkDestroyed = Resources.Load<GameEvent<ChunkEventData>>("Events/OnChunkDestroyed");
            // _onLevelProgress = Resources.Load<GameEvent<LevelProgressEventData>>("Events/OnLevelProgress");

            Debug.Log("[LevelGenerationService] Event channels initialized for loose coupling communication.");
        }

        /// <summary>
        /// ServiceLocator邨ｱ蜷域､懆ｨｼ
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
            // 繧ｵ繝ｼ繝薙せ譖ｴ譁ｰ蜃ｦ逅・ｼ亥ｿ・ｦ√↓蠢懊§縺ｦ螳溯｣・ｼ・
            // 萓具ｼ壼虚逧・Ξ繝吶Ν隱ｿ謨ｴ縲√ヱ繝輔か繝ｼ繝槭Φ繧ｹ逶｣隕悶↑縺ｩ
        }

        /// <summary>
        /// 險ｭ螳壽峩譁ｰ・壹Λ繝ｳ繧ｿ繧､繝險ｭ螳壼､画峩蟇ｾ蠢・
        /// </summary>
        public void UpdateSettings(PlatformerLevelSettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
            Debug.Log("[LevelGenerationService] Settings updated at runtime.");
        }

        /// <summary>
        /// 繝ｪ繧ｽ繝ｼ繧ｹ隗｣謾ｾ・唔Disposable螳溯｣・
        /// </summary>
        public void Dispose()
        {
            ClearCurrentLevel();
            _chunkPool.Clear();

            // Event雉ｼ隱ｭ隗｣髯､・亥ｮ溯｣・凾縺ｫ蠢・ｦ・ｼ・
            // if (_onLevelGenerated != null) _onLevelGenerated.RemoveAllListeners();

            Debug.Log("[LevelGenerationService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ險ｺ譁ｭ諠・ｱ・夐幕逋ｺ謾ｯ謠ｴ讖溯・
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
    /// 繝ｬ繝吶Ν逕滓・繧､繝吶Φ繝医ョ繝ｼ繧ｿ讒矩
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
    /// 繝√Ε繝ｳ繧ｯ繧､繝吶Φ繝医ョ繝ｼ繧ｿ讒矩
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
    /// 繝ｬ繝吶Ν騾ｲ謐励う繝吶Φ繝医ョ繝ｼ繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct LevelProgressEventData
    {
        public Vector3 PlayerPosition;
        public float Progress;
        public int ActiveChunks;
    }

    /// <summary>
    /// 繝ｬ繝吶Ν繝√Ε繝ｳ繧ｯ繝・・繧ｿ讒矩
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
    /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繝・・繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct PlatformData
    {
        public Vector3 Position;
        public float Width;
        public PlatformType PlatformType;
    }

    /// <summary>
    /// 繝上じ繝ｼ繝峨ョ繝ｼ繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct HazardData
    {
        public Vector3 Position;
        public HazardType HazardType;
    }

    /// <summary>
    /// 繝励Λ繝・ヨ繝輔か繝ｼ繝繧ｿ繧､繝怜・謖・
    /// </summary>
    public enum PlatformType
    {
        Static,     // 髱咏噪繝励Λ繝・ヨ繝輔か繝ｼ繝
        Moving,     // 遘ｻ蜍輔・繝ｩ繝・ヨ繝輔か繝ｼ繝
        Falling     // 關ｽ荳九・繝ｩ繝・ヨ繝輔か繝ｼ繝
    }

    /// <summary>
    /// 繝上じ繝ｼ繝峨ち繧､繝怜・謖・
    /// </summary>
    public enum HazardType
    {
        Spike,      // 繧ｹ繝代う繧ｯ
        Lava,       // 貅ｶ蟯ｩ
        Enemy       // 謨ｵ
    }
}


