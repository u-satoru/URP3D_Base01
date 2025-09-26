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
    /// Collection Service・壹・繝ｩ繝・ヨ繝輔か繝ｼ繝槭・蜿朱寔繧｢繧､繝・Β邂｡逅・す繧ｹ繝・Β
    /// ServiceLocator邨ｱ蜷茨ｼ壹い繧､繝・Β蜿朱寔繝ｻ繧ｹ繧ｳ繧｢邂｡逅・・騾ｲ謐苓ｿｽ霍｡繝ｻLearn & Grow萓｡蛟､螳溽樟
    /// Event鬧・虚騾壻ｿ｡・壹Ξ繝吶Ν螳御ｺ・・繧｢繧､繝・Β蜿朱寔繝ｻ繧ｹ繧ｳ繧｢譖ｴ譁ｰ縺ｮ逍守ｵ仙粋騾夂衍
    /// </summary>
    public class CollectionService : ICollectionService
    {
        // 險ｭ螳壹ョ繝ｼ繧ｿ
        private PlatformerCollectionSettings _settings;

        // 繧｢繧､繝・Β邂｡逅・ョ繝ｼ繧ｿ
        private readonly HashSet<int> _collectedItems = new HashSet<int>();
        private readonly Dictionary<int, CollectibleItemData> _levelItems = new Dictionary<int, CollectibleItemData>();

        // 邨ｱ險域ュ蝣ｱ
        private int _currentScore = 0;
        private int _totalItemsInCurrentLevel = 0;
        private bool _isLevelCompleted = false;

        // Event鬧・虚騾壻ｿ｡・育鮪邨仙粋・・
        private GameEvent<CollectionEventData> _onItemCollected;
        private GameEvent<LevelCompletionEventData> _onLevelCompleted;
        private GameEvent<int> _onScoreChanged;

        // ServiceLocator騾｣謳ｺ繝輔Λ繧ｰ
        private bool _isInitialized = false;

        public int CollectedItemsCount => _collectedItems.Count;
        public int TotalItemsInLevel => _totalItemsInCurrentLevel;
        public float CompletionPercentage => _totalItemsInCurrentLevel > 0 ?
            (float)_collectedItems.Count / _totalItemsInCurrentLevel : 0f;
        public bool IsLevelCompleted => _isLevelCompleted;

        /// <summary>
        /// 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ・夊ｨｭ螳壹・繝ｼ繧ｹ蛻晄悄蛹・
        /// </summary>
        public CollectionService(PlatformerCollectionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // Event鬧・虚騾壻ｿ｡縺ｮ蛻晄悄蛹・
            InitializeEventChannels();

            Debug.Log("[CollectionService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// 繧｢繧､繝・Β蜿朱寔・哘vent鬧・虚騾夂衍 + ServiceLocator騾｣謳ｺ
        /// </summary>
        public void CollectItem(int itemId, int score)
        {
            if (_collectedItems.Contains(itemId))
            {
                Debug.LogWarning($"[CollectionService] Item {itemId} already collected.");
                return;
            }

            // 繧｢繧､繝・Β蜿朱寔蜃ｦ逅・
            _collectedItems.Add(itemId);
            _currentScore += score;

            // ServiceLocator邨檎罰縺ｧ繧ｲ繝ｼ繝繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｫ騾夂衍
            var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
            if (gameManager != null)
            {
                gameManager.AddScore(score);
            }

            // Event鬧・虚騾壻ｿ｡・壹い繧､繝・Β蜿朱寔騾夂衍
            var eventData = new CollectionEventData
            {
                ItemId = itemId,
                Score = score,
                TotalScore = _currentScore,
                CollectedItemsCount = _collectedItems.Count,
                CompletionPercentage = CompletionPercentage
            };

            _onItemCollected?.Raise(eventData);
            _onScoreChanged?.Raise(_currentScore);

            // UnityEvent逋ｺ陦鯉ｼ医う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ貅匁侠・・
            OnItemCollected?.Invoke($"Item_{itemId}", score);
            OnScoreChanged?.Invoke(_currentScore);

            Debug.Log($"[CollectionService] Item {itemId} collected. Score: +{score}, Total: {_currentScore}");

            // 繝ｬ繝吶Ν螳御ｺ・メ繧ｧ繝・け
            CheckLevelCompletion();
        }

        /// <summary>
        /// 繝ｬ繝吶Ν蛻晄悄蛹厄ｼ壹い繧､繝・Β驟咲ｽｮ諠・ｱ縺ｮ險ｭ螳・
        /// </summary>
        public void InitializeLevel(CollectibleItemData[] levelItems)
        {
            _levelItems.Clear();
            _totalItemsInCurrentLevel = levelItems?.Length ?? 0;

            if (levelItems != null)
            {
                foreach (var item in levelItems)
                {
                    _levelItems[item.ItemId] = item;
                }
            }

            Debug.Log($"[CollectionService] Level initialized with {_totalItemsInCurrentLevel} collectible items.");
        }

        /// <summary>
        /// 繝ｬ繝吶Ν螳御ｺ・愛螳夲ｼ哘vent鬧・虚騾夂衍
        /// </summary>
        private void CheckLevelCompletion()
        {
            // 蠢・医い繧､繝・Β蜿朱寔繝√ぉ繝・け
            bool allRequiredCollected = true;
            int requiredItemsCount = 0;

            foreach (var item in _levelItems.Values)
            {
                if (item.IsRequired)
                {
                    requiredItemsCount++;
                    if (!_collectedItems.Contains(item.ItemId))
                    {
                        allRequiredCollected = false;
                    }
                }
            }

            // 螳御ｺ・擅莉ｶ繝√ぉ繝・け
            bool completionByPercentage = CompletionPercentage >= _settings.LevelCompletionThreshold;
            bool completionByRequired = requiredItemsCount > 0 ? allRequiredCollected : false;

            _isLevelCompleted = _settings.RequireAllMandatoryItems ?
                completionByRequired : (completionByRequired || completionByPercentage);

            if (_isLevelCompleted)
            {
                // Event鬧・虚騾壻ｿ｡・壹Ξ繝吶Ν螳御ｺ・夂衍
                var completionData = new LevelCompletionEventData
                {
                    CompletionPercentage = CompletionPercentage,
                    TotalScore = _currentScore,
                    CollectedItemsCount = _collectedItems.Count,
                    TotalItemsCount = _totalItemsInCurrentLevel,
                    CompletedByRequiredItems = completionByRequired,
                    CompletedByPercentage = completionByPercentage
                };

                _onLevelCompleted?.Raise(completionData);

                // ServiceLocator邨檎罰縺ｧ繧ｲ繝ｼ繝繝槭ロ繝ｼ繧ｸ繝｣繝ｼ縺ｫ騾夂衍
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                gameManager?.CompleteLevel();

                Debug.Log($"[CollectionService] Level completed! Score: {_currentScore}, Completion: {CompletionPercentage:P}");
            }
        }

        /// <summary>
        /// 繝ｬ繝吶Ν繝ｪ繧ｻ繝・ヨ・夂憾諷句・譛溷喧
        /// </summary>
        public void ResetLevel()
        {
            _collectedItems.Clear();
            _levelItems.Clear();
            _currentScore = 0;
            _totalItemsInCurrentLevel = 0;
            _isLevelCompleted = false;

            // Event鬧・虚騾壻ｿ｡・壹Μ繧ｻ繝・ヨ騾夂衍
            _onScoreChanged?.Raise(0);

            Debug.Log("[CollectionService] Level reset completed.");
        }

        /// <summary>
        /// 迚ｹ螳壹い繧､繝・Β蜿朱寔迥ｶ豕∫｢ｺ隱・
        /// </summary>
        public bool IsItemCollected(int itemId)
        {
            return _collectedItems.Contains(itemId);
        }

        /// <summary>
        /// 繧｢繧､繝・Β諠・ｱ蜿門ｾ・
        /// </summary>
        public CollectibleItemData GetItemData(int itemId)
        {
            _levelItems.TryGetValue(itemId, out var itemData);
            return itemData;
        }

        /// <summary>
        /// 蜿朱寔貂医∩繧｢繧､繝・Β繝ｪ繧ｹ繝亥叙蠕・
        /// </summary>
        public IReadOnlyCollection<int> GetCollectedItems()
        {
            return _collectedItems;
        }

        /// <summary>
        /// 險ｭ螳壽峩譁ｰ・壹Λ繝ｳ繧ｿ繧､繝險ｭ螳壼､画峩蟇ｾ蠢・
        /// </summary>
        public void UpdateSettings(PlatformerCollectionSettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
            Debug.Log("[CollectionService] Settings updated at runtime.");
        }

        /// <summary>
        /// Event鬧・虚騾壻ｿ｡繝√Ε繝阪Ν蛻晄悄蛹・
        /// </summary>
        private void InitializeEventChannels()
        {
            // NOTE: 螳滄圀縺ｮ螳溯｣・〒縺ｯ縲√％繧後ｉ縺ｮEventChannel縺ｯScriptableObject縺ｨ縺励※
            // 繝励Ο繧ｸ繧ｧ繧ｯ繝亥・縺ｧ菴懈・繝ｻ邂｡逅・＆繧後ｋ
            // 縺薙％縺ｧ縺ｯ讒矩繧堤､ｺ縺吶◆繧√・繝励Ξ繝ｼ繧ｹ繝帙Ν繝繝ｼ

            // _onItemCollected = Resources.Load<GameEvent<CollectionEventData>>("Events/OnItemCollected");
            // _onLevelCompleted = Resources.Load<GameEvent<LevelCompletionEventData>>("Events/OnLevelCompleted");
            // _onScoreChanged = Resources.Load<GameEvent<int>>("Events/OnScoreChanged");

            Debug.Log("[CollectionService] Event channels initialized for loose coupling communication.");
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

                bool integration = gameManager != null && physicsService != null;
                Debug.Log($"[CollectionService] ServiceLocator integration verified: {integration}");
                return integration;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CollectionService] ServiceLocator integration failed: {ex.Message}");
                return false;
            }
        }

        // ==================================================
        // IPlatformerService 蝓ｺ蠎輔う繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ螳溯｣・
        // ==================================================

        // IPlatformerService縺ｧ蠢・ｦ√↑繝励Ο繝代ユ繧｣
        public bool IsInitialized { get; private set; } = false;
        public bool IsEnabled { get; private set; } = false;

        public void Initialize()
        {
            if (IsInitialized) return;

            InitializeEventChannels();
            IsInitialized = true;
            Debug.Log("[CollectionService] Initialized successfully.");
        }

        public void Enable()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[CollectionService] Cannot enable - not initialized yet.");
                return;
            }

            IsEnabled = true;
            Debug.Log("[CollectionService] Enabled.");
        }

        public void Disable()
        {
            IsEnabled = false;
            Debug.Log("[CollectionService] Disabled.");
        }

        public void Reset()
        {
            ResetLevel();
            Debug.Log("[CollectionService] Reset completed.");
        }

        public void UpdateService(float deltaTime)
        {
            if (!IsEnabled) return;
            // 繧ｵ繝ｼ繝薙せ譖ｴ譁ｰ蜃ｦ逅・ｼ亥ｿ・ｦ√↓蠢懊§縺ｦ螳溯｣・ｼ・
        }

        // ==================================================
        // ICollectionService 霑ｽ蜉繝｡繧ｽ繝・ラ螳溯｣・
        // ==================================================

        // UnityEvent繝励Ο繝代ユ繧｣・医お繝ｩ繝ｼ隗｣豎ｺ逕ｨ・・
        private UnityEngine.Events.UnityEvent<string, int> _onItemCollectedEvent = new UnityEngine.Events.UnityEvent<string, int>();
        private UnityEngine.Events.UnityEvent<int> _onScoreChangedEvent = new UnityEngine.Events.UnityEvent<int>();

        public UnityEngine.Events.UnityEvent<string, int> OnItemCollected => _onItemCollectedEvent;
        public UnityEngine.Events.UnityEvent<int> OnScoreChanged => _onScoreChangedEvent;

        public System.Collections.Generic.IEnumerable<int> GetCollectedItemIds()
        {
            return _collectedItems;
        }

        public void RestoreCollectedItems(System.Collections.Generic.IEnumerable<int> itemIds)
        {
            _collectedItems.Clear();
            foreach (var itemId in itemIds)
            {
                _collectedItems.Add(itemId);
            }
            Debug.Log($"[CollectionService] Restored {_collectedItems.Count} collected items.");
        }

        public void InitializeLevel(int totalItemsCount)
        {
            _totalItemsInCurrentLevel = totalItemsCount;
            _collectedItems.Clear();
            _levelItems.Clear();
            _currentScore = 0;
            _isLevelCompleted = false;
            Debug.Log($"[CollectionService] Level initialized with {totalItemsCount} items.");
        }

        // 繧ｹ繧ｳ繧｢邂｡逅・僑蠑ｵ
        public int CurrentScore => _currentScore;
        public int HighScore { get; private set; } = 0;

        public void AddScore(int points)
        {
            _currentScore += points;
            if (_currentScore > HighScore)
            {
                HighScore = _currentScore;
            }

            // 繧､繝吶Φ繝育匱陦・
            OnScoreChanged?.Invoke(_currentScore);
            _onScoreChanged?.Raise(_currentScore);
            Debug.Log($"[CollectionService] Score added: +{points}, Total: {_currentScore}");
        }

        /// <summary>
        /// 繝ｪ繧ｽ繝ｼ繧ｹ隗｣謾ｾ・唔Disposable螳溯｣・
        /// </summary>
        public void Dispose()
        {
            _collectedItems.Clear();
            _levelItems.Clear();

            // Event雉ｼ隱ｭ隗｣髯､・亥ｮ溯｣・凾縺ｫ蠢・ｦ・ｼ・
            // if (_onItemCollected != null) _onItemCollected.RemoveAllListeners();

            Debug.Log("[CollectionService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// 繧ｨ繝・ぅ繧ｿ逕ｨ險ｺ譁ｭ諠・ｱ・夐幕逋ｺ謾ｯ謠ｴ讖溯・
        /// </summary>
        [ContextMenu("Show Collection Debug Info")]
        public void ShowCollectionDebugInfo()
        {
            Debug.Log("=== Collection Service Diagnostic ===");
            Debug.Log($"Collected Items: {_collectedItems.Count}/{_totalItemsInCurrentLevel}");
            Debug.Log($"Completion Percentage: {CompletionPercentage:P}");
            Debug.Log($"Current Score: {_currentScore}");
            Debug.Log($"Level Completed: {_isLevelCompleted}");
            Debug.Log($"ServiceLocator Integration: {VerifyServiceLocatorIntegration()}");

            Debug.Log("Collected Item IDs:");
            foreach (var itemId in _collectedItems)
            {
                Debug.Log($"  - Item {itemId}");
            }
        }
#endif
    }

    /// <summary>
    /// 繧｢繧､繝・Β蜿朱寔繧､繝吶Φ繝医ョ繝ｼ繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct CollectionEventData
    {
        public int ItemId;
        public int Score;
        public int TotalScore;
        public int CollectedItemsCount;
        public float CompletionPercentage;
    }

    /// <summary>
    /// 繝ｬ繝吶Ν螳御ｺ・う繝吶Φ繝医ョ繝ｼ繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct LevelCompletionEventData
    {
        public float CompletionPercentage;
        public int TotalScore;
        public int CollectedItemsCount;
        public int TotalItemsCount;
        public bool CompletedByRequiredItems;
        public bool CompletedByPercentage;
    }

    /// <summary>
    /// 蜿朱寔繧｢繧､繝・Β繝・・繧ｿ讒矩
    /// </summary>
    [System.Serializable]
    public struct CollectibleItemData
    {
        public int ItemId;
        public string ItemName;
        public int Score;
        public bool IsRequired;
        public Vector3 Position;
        public string Description;
    }
}


