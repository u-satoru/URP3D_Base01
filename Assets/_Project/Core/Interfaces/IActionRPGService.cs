using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Core.Services
{
    /// <summary>
    /// ActionRPG機能のコアサービスインターフェース
    /// Core層で定義し、Feature層で実装される
    /// </summary>
    public interface IActionRPGService : IService
    {
        /// <summary>
        /// 経験値を追加
        /// </summary>
        void AddExperience(int amount);

        /// <summary>
        /// 現在の経験値情報を取得
        /// </summary>
        (int currentExp, int currentLevel, int expToNext) GetExperienceInfo();

        /// <summary>
        /// ルーン収集イベントを通知
        /// </summary>
        void NotifyResourceCollected(int amount);

        /// <summary>
        /// プレイヤー参照を取得
        /// </summary>
        GameObject GetPlayerGameObject();

        /// <summary>
        /// レベルアップイベントの通知
        /// </summary>
        event Action<int> OnLevelUp;

        /// <summary>
        /// 経験値更新イベントの通知
        /// </summary>
        event Action<int> OnExperienceGained;
    }

    /// <summary>
    /// ステータス管理インターフェース
    /// </summary>
    public interface IStatSystem
    {
        /// <summary>
        /// 現在のレベルを取得
        /// </summary>
        int CurrentLevel { get; }

        /// <summary>
        /// 現在の経験値を取得
        /// </summary>
        int CurrentExperience { get; }

        /// <summary>
        /// 派生ステータスを取得
        /// </summary>
        int GetDerivedStat(string statName);

        /// <summary>
        /// ステータスポイントを割り当て
        /// </summary>
        bool AllocateStatPoint(string statType, int points = 1);
    }

    /// <summary>
    /// インベントリ管理インターフェース
    /// </summary>
    public interface IInventorySystem
    {
        /// <summary>
        /// アイテムを追加
        /// </summary>
        bool AddItem(object itemData, int amount);

        /// <summary>
        /// アイテムを削除
        /// </summary>
        bool RemoveItem(object itemData, int amount);

        /// <summary>
        /// アイテム所持数を取得
        /// </summary>
        int GetItemCount(object itemData);

        /// <summary>
        /// インベントリがいっぱいかどうか
        /// </summary>
        bool IsFull { get; }
    }
}
