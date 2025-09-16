using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace asterivo.Unity60.Features.Templates.Platformer.Services
{
    /// <summary>
    /// 収集サービス インターフェース
    /// アイテム収集・スコア管理・進捗追跡
    /// </summary>
    public interface ICollectionService : IPlatformerService
    {
        // 基本収集機能
        void CollectItem(int itemId, int score);
        int CollectedItemsCount { get; }
        int TotalItemsInLevel { get; }
        float CompletionPercentage { get; }
        bool IsLevelCompleted { get; }
        void ResetLevel();

        // 不足していたメソッド（エラー解決用）
        UnityEvent<string, int> OnItemCollected { get; }
        UnityEvent<int> OnScoreChanged { get; }
        IEnumerable<int> GetCollectedItemIds();
        void RestoreCollectedItems(IEnumerable<int> itemIds);
        void InitializeLevel(int totalItemsCount);
        void InitializeLevel(CollectibleItemData[] levelItems);

        // スコア管理拡張
        int CurrentScore { get; }
        int HighScore { get; }
        void AddScore(int points);
    }
}