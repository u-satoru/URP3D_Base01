using System;
using System.Collections.Generic;
using UnityEngine;

namespace asterivo.Unity60.Features.Templates.FPS.Services
{
    /// <summary>
    /// FPSテンプレートインベントリサービスインターフェース
    /// アーキテクチャ準拠: ServiceLocator + Event駆動のハイブリッドアプローチ
    /// </summary>
    public interface IInventoryService
    {
        // プロパティ
        int MaxInventorySize { get; }
        int CurrentItemCount { get; }
        bool IsInventoryFull { get; }

        // イベント
        event Action<string, int> OnItemAdded;
        event Action<string, int> OnItemRemoved;
        event Action<string> OnItemUsed;
        event Action OnInventoryFull;
        event Action OnInventoryChanged;

        // アイテム管理
        bool AddItem(string itemId, int quantity = 1);
        bool RemoveItem(string itemId, int quantity = 1);
        bool UseItem(string itemId);
        bool HasItem(string itemId, int minQuantity = 1);
        int GetItemQuantity(string itemId);

        // インベントリ操作
        void ClearInventory();
        Dictionary<string, int> GetAllItems();
        bool TransferItem(string itemId, int quantity, IInventoryService targetInventory);

        // アイテム検索
        List<string> GetItemsByCategory(string category);
        List<string> FindItems(Func<string, bool> predicate);

        // 状態管理
        void Initialize(int maxSize);
        bool IsInitialized { get; }
    }
}