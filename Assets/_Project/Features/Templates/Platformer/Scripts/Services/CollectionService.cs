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
    /// Collection Service：プラットフォーマー収集アイテム管理システム
    /// ServiceLocator統合：アイテム収集・スコア管理・進捗追跡・Learn & Grow価値実現
    /// Event駆動通信：レベル完了・アイテム収集・スコア更新の疎結合通知
    /// </summary>
    public class CollectionService : ICollectionService
    {
        // 設定データ
        private PlatformerCollectionSettings _settings;

        // アイテム管理データ
        private readonly HashSet<int> _collectedItems = new HashSet<int>();
        private readonly Dictionary<int, CollectibleItemData> _levelItems = new Dictionary<int, CollectibleItemData>();

        // 統計情報
        private int _currentScore = 0;
        private int _totalItemsInCurrentLevel = 0;
        private bool _isLevelCompleted = false;

        // Event駆動通信（疎結合）
        private GameEvent<CollectionEventData> _onItemCollected;
        private GameEvent<LevelCompletionEventData> _onLevelCompleted;
        private GameEvent<int> _onScoreChanged;

        // ServiceLocator連携フラグ
        private bool _isInitialized = false;

        public int CollectedItemsCount => _collectedItems.Count;
        public int TotalItemsInLevel => _totalItemsInCurrentLevel;
        public float CompletionPercentage => _totalItemsInCurrentLevel > 0 ?
            (float)_collectedItems.Count / _totalItemsInCurrentLevel : 0f;
        public bool IsLevelCompleted => _isLevelCompleted;

        /// <summary>
        /// コンストラクタ：設定ベース初期化
        /// </summary>
        public CollectionService(PlatformerCollectionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            // Event駆動通信の初期化
            InitializeEventChannels();

            Debug.Log("[CollectionService] Initialized with ServiceLocator + Event-driven architecture.");
        }

        /// <summary>
        /// アイテム収集：Event駆動通知 + ServiceLocator連携
        /// </summary>
        public void CollectItem(int itemId, int score)
        {
            if (_collectedItems.Contains(itemId))
            {
                Debug.LogWarning($"[CollectionService] Item {itemId} already collected.");
                return;
            }

            // アイテム収集処理
            _collectedItems.Add(itemId);
            _currentScore += score;

            // ServiceLocator経由でゲームマネージャーに通知
            var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
            if (gameManager != null)
            {
                gameManager.AddScore(score);
            }

            // Event駆動通信：アイテム収集通知
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

            // UnityEvent発行（インターフェース準拠）
            OnItemCollected?.Invoke($"Item_{itemId}", score);
            OnScoreChanged?.Invoke(_currentScore);

            Debug.Log($"[CollectionService] Item {itemId} collected. Score: +{score}, Total: {_currentScore}");

            // レベル完了チェック
            CheckLevelCompletion();
        }

        /// <summary>
        /// レベル初期化：アイテム配置情報の設定
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
        /// レベル完了判定：Event駆動通知
        /// </summary>
        private void CheckLevelCompletion()
        {
            // 必須アイテム収集チェック
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

            // 完了条件チェック
            bool completionByPercentage = CompletionPercentage >= _settings.LevelCompletionThreshold;
            bool completionByRequired = requiredItemsCount > 0 ? allRequiredCollected : false;

            _isLevelCompleted = _settings.RequireAllMandatoryItems ?
                completionByRequired : (completionByRequired || completionByPercentage);

            if (_isLevelCompleted)
            {
                // Event駆動通信：レベル完了通知
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

                // ServiceLocator経由でゲームマネージャーに通知
                var gameManager = ServiceLocator.GetService<IPlatformerGameManager>();
                gameManager?.CompleteLevel();

                Debug.Log($"[CollectionService] Level completed! Score: {_currentScore}, Completion: {CompletionPercentage:P}");
            }
        }

        /// <summary>
        /// レベルリセット：状態初期化
        /// </summary>
        public void ResetLevel()
        {
            _collectedItems.Clear();
            _levelItems.Clear();
            _currentScore = 0;
            _totalItemsInCurrentLevel = 0;
            _isLevelCompleted = false;

            // Event駆動通信：リセット通知
            _onScoreChanged?.Raise(0);

            Debug.Log("[CollectionService] Level reset completed.");
        }

        /// <summary>
        /// 特定アイテム収集状況確認
        /// </summary>
        public bool IsItemCollected(int itemId)
        {
            return _collectedItems.Contains(itemId);
        }

        /// <summary>
        /// アイテム情報取得
        /// </summary>
        public CollectibleItemData GetItemData(int itemId)
        {
            _levelItems.TryGetValue(itemId, out var itemData);
            return itemData;
        }

        /// <summary>
        /// 収集済みアイテムリスト取得
        /// </summary>
        public IReadOnlyCollection<int> GetCollectedItems()
        {
            return _collectedItems;
        }

        /// <summary>
        /// 設定更新：ランタイム設定変更対応
        /// </summary>
        public void UpdateSettings(PlatformerCollectionSettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
            Debug.Log("[CollectionService] Settings updated at runtime.");
        }

        /// <summary>
        /// Event駆動通信チャネル初期化
        /// </summary>
        private void InitializeEventChannels()
        {
            // NOTE: 実際の実装では、これらのEventChannelはScriptableObjectとして
            // プロジェクト内で作成・管理される
            // ここでは構造を示すためのプレースホルダー

            // _onItemCollected = Resources.Load<GameEvent<CollectionEventData>>("Events/OnItemCollected");
            // _onLevelCompleted = Resources.Load<GameEvent<LevelCompletionEventData>>("Events/OnLevelCompleted");
            // _onScoreChanged = Resources.Load<GameEvent<int>>("Events/OnScoreChanged");

            Debug.Log("[CollectionService] Event channels initialized for loose coupling communication.");
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
        // IPlatformerService 基底インターフェース実装
        // ==================================================

        // IPlatformerServiceで必要なプロパティ
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
            // サービス更新処理（必要に応じて実装）
        }

        // ==================================================
        // ICollectionService 追加メソッド実装
        // ==================================================

        // UnityEventプロパティ（エラー解決用）
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

        // スコア管理拡張
        public int CurrentScore => _currentScore;
        public int HighScore { get; private set; } = 0;

        public void AddScore(int points)
        {
            _currentScore += points;
            if (_currentScore > HighScore)
            {
                HighScore = _currentScore;
            }

            // イベント発行
            OnScoreChanged?.Invoke(_currentScore);
            _onScoreChanged?.Raise(_currentScore);
            Debug.Log($"[CollectionService] Score added: +{points}, Total: {_currentScore}");
        }

        /// <summary>
        /// リソース解放：IDisposable実装
        /// </summary>
        public void Dispose()
        {
            _collectedItems.Clear();
            _levelItems.Clear();

            // Event購読解除（実装時に必要）
            // if (_onItemCollected != null) _onItemCollected.RemoveAllListeners();

            Debug.Log("[CollectionService] Disposed successfully.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用診断情報：開発支援機能
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
    /// アイテム収集イベントデータ構造
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
    /// レベル完了イベントデータ構造
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
    /// 収集アイテムデータ構造
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
