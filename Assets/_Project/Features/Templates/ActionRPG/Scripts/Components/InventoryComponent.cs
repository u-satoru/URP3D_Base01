using System.Collections.Generic;
using UnityEngine;
using asterivo.Unity60.Core;
using asterivo.Unity60.Core.Events;
using asterivo.Unity60.Features.ActionRPG.Data;

namespace asterivo.Unity60.Features.ActionRPG.Components
{
    /// <summary>
    /// プレイヤーのアイテム管理を行うコンポーネント
    /// アイテムの取得、使用、管理機能を提供します
    /// </summary>
    public class InventoryComponent : MonoBehaviour
    {
        [Header("インベントリ設定")]
        [SerializeField] private int _maxSlots = 30;
        [SerializeField] private int _maxStackSize = 999;

        [Header("イベント")]
        [SerializeField] private GameEvent _onItemAdded;
        [SerializeField] private GameEvent _onItemRemoved;
        [SerializeField] private GameEvent _onItemUsed;
        [SerializeField] private GameEvent _onInventoryChanged;

        [Header("初期アイテム")]
        [SerializeField] private asterivo.Unity60.Features.ActionRPG.Data.ItemData[] _startingItems;
        [SerializeField] private int[] _startingQuantities;

        // アイテムスロット
        private List<ItemSlot> _itemSlots;
        private Dictionary<asterivo.Unity60.Features.ActionRPG.Data.ItemData, int> _itemCounts;

        // プロパティ
        public int MaxSlots => _maxSlots;
        public int UsedSlots => _itemSlots.Count;
        public int AvailableSlots => _maxSlots - _itemSlots.Count;

        void Awake()
        {
            InitializeInventory();
        }

        void Start()
        {
            AddStartingItems();
        }

        /// <summary>
        /// インベントリを初期化
        /// </summary>
        private void InitializeInventory()
        {
            _itemSlots = new List<ItemSlot>();
            _itemCounts = new Dictionary<asterivo.Unity60.Features.ActionRPG.Data.ItemData, int>();
        }

        /// <summary>
        /// 初期アイテムを追加
        /// </summary>
        private void AddStartingItems()
        {
            if (_startingItems == null || _startingQuantities == null) return;

            int count = Mathf.Min(_startingItems.Length, _startingQuantities.Length);
            for (int i = 0; i < count; i++)
            {
                if (_startingItems[i] != null && _startingQuantities[i] > 0)
                {
                    AddItem(_startingItems[i], _startingQuantities[i]);
                }
            }
        }

        /// <summary>
        /// アイテムを追加
        /// </summary>
        public bool AddItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;

            // スタック可能アイテムの場合
            if (itemData.Stackable)
            {
                return AddStackableItem(itemData, quantity);
            }
            // スタック不可能アイテムの場合
            else
            {
                return AddNonStackableItems(itemData, quantity);
            }
        }

        /// <summary>
        /// スタック可能アイテムを追加
        /// </summary>
        private bool AddStackableItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity)
        {
            int remainingQuantity = quantity;

            // 既存スロットに追加
            foreach (var slot in _itemSlots)
            {
                if (slot.ItemData == itemData)
                {
                    int canAdd = Mathf.Min(remainingQuantity, itemData.MaxStackSize - slot.Quantity);
                    if (canAdd > 0)
                    {
                        slot.Quantity += canAdd;
                        remainingQuantity -= canAdd;
                        
                        if (remainingQuantity <= 0) break;
                    }
                }
            }

            // 新しいスロットが必要な場合
            while (remainingQuantity > 0 && _itemSlots.Count < _maxSlots)
            {
                int addQuantity = Mathf.Min(remainingQuantity, itemData.MaxStackSize);
                _itemSlots.Add(new ItemSlot(itemData, addQuantity));
                remainingQuantity -= addQuantity;
            }

            // カウント更新
            if (!_itemCounts.ContainsKey(itemData))
                _itemCounts[itemData] = 0;
            
            int actuallyAdded = quantity - remainingQuantity;
            _itemCounts[itemData] += actuallyAdded;

            if (actuallyAdded > 0)
            {
                NotifyInventoryChanged();
                
                if (_onItemAdded != null)
                    _onItemAdded.Raise();

                Debug.Log($"{itemData.ItemName} x{actuallyAdded}を取得しました。");
            }

            return remainingQuantity == 0;
        }

        /// <summary>
        /// スタック不可能アイテムを追加
        /// </summary>
        private bool AddNonStackableItems(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity)
        {
            int added = 0;
            
            for (int i = 0; i < quantity && _itemSlots.Count < _maxSlots; i++)
            {
                _itemSlots.Add(new ItemSlot(itemData, 1));
                added++;
            }

            if (added > 0)
            {
                if (!_itemCounts.ContainsKey(itemData))
                    _itemCounts[itemData] = 0;
                    
                _itemCounts[itemData] += added;
                NotifyInventoryChanged();
                
                if (_onItemAdded != null)
                    _onItemAdded.Raise();
            }

            return added == quantity;
        }

        /// <summary>
        /// アイテムを削除
        /// </summary>
        public bool RemoveItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;
            if (!HasItem(itemData, quantity)) return false;

            int remainingToRemove = quantity;

            for (int i = _itemSlots.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                var slot = _itemSlots[i];
                if (slot.ItemData == itemData)
                {
                    int removeFromSlot = Mathf.Min(remainingToRemove, slot.Quantity);
                    slot.Quantity -= removeFromSlot;
                    remainingToRemove -= removeFromSlot;

                    if (slot.Quantity <= 0)
                    {
                        _itemSlots.RemoveAt(i);
                    }
                }
            }

            // カウント更新
            _itemCounts[itemData] -= (quantity - remainingToRemove);
            if (_itemCounts[itemData] <= 0)
            {
                _itemCounts.Remove(itemData);
            }

            NotifyInventoryChanged();
            
            if (_onItemRemoved != null)
                _onItemRemoved.Raise();

            return remainingToRemove == 0;
        }

        /// <summary>
        /// アイテムを使用
        /// </summary>
        public bool UseItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity = 1)
        {
            if (!CanUseItem(itemData)) return false;
            if (!HasItem(itemData, quantity)) return false;

            // アイテム効果を適用
            ApplyItemEffects(itemData, quantity);

            // 消耗品の場合は削除
            if (itemData.ConsumeOnUse)
            {
                RemoveItem(itemData, quantity);
            }

            if (_onItemUsed != null)
                _onItemUsed.Raise();

            return true;
        }

        /// <summary>
        /// アイテムを使用可能かチェック
        /// </summary>
        public bool CanUseItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData)
        {
            return itemData != null && itemData.CanUse();
        }

        /// <summary>
        /// アイテムを所持しているかチェック
        /// </summary>
        public bool HasItem(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity = 1)
        {
            if (itemData == null) return false;
            return GetItemCount(itemData) >= quantity;
        }

        /// <summary>
        /// アイテムの所持数を取得
        /// </summary>
        public int GetItemCount(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData)
        {
            if (itemData == null) return 0;
            return _itemCounts.TryGetValue(itemData, out int count) ? count : 0;
        }

        /// <summary>
        /// アイテム効果を適用
        /// </summary>
        private void ApplyItemEffects(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity)
        {
            var healthComponent = GetComponent<asterivo.Unity60.Core.Combat.HealthComponent>();
            var statComponent = GetComponent<StatComponent>();

            // ヘルス回復
            if (itemData.HealthRestore > 0 && healthComponent != null)
            {
                int totalHeal = itemData.HealthRestore * quantity;
                healthComponent.Heal(totalHeal);
                Debug.Log($"HP {totalHeal} 回復しました。");
            }

            // フォーカス回復（実装予定）
            if (itemData.FocusRestore > 0)
            {
                Debug.Log($"フォーカス {itemData.FocusRestore * quantity} 回復しました。");
            }

            // 経験値獲得
            if (itemData.ExperienceValue > 0 && statComponent != null)
            {
                int totalExp = itemData.ExperienceValue * quantity;
                statComponent.AddExperience(totalExp);
                Debug.Log($"経験値 {totalExp} を獲得しました。");
            }
        }

        /// <summary>
        /// インベントリ変更通知
        /// </summary>
        private void NotifyInventoryChanged()
        {
            if (_onInventoryChanged != null)
                _onInventoryChanged.Raise();
        }

        /// <summary>
        /// 全アイテムスロットを取得
        /// </summary>
        public ItemSlot[] GetAllItems()
        {
            return _itemSlots.ToArray();
        }

        /// <summary>
        /// インベントリをクリア
        /// </summary>
        public void ClearInventory()
        {
            _itemSlots.Clear();
            _itemCounts.Clear();
            NotifyInventoryChanged();
        }
    }

    /// <summary>
    /// アイテムスロットのデータ構造
    /// </summary>
    [System.Serializable]
    public class ItemSlot
    {
        [SerializeField] private asterivo.Unity60.Features.ActionRPG.Data.ItemData _itemData;
        [SerializeField] private int _quantity;

        public asterivo.Unity60.Features.ActionRPG.Data.ItemData ItemData => _itemData;
        public int Quantity 
        { 
            get => _quantity;
            set => _quantity = Mathf.Max(0, value);
        }

        public ItemSlot(asterivo.Unity60.Features.ActionRPG.Data.ItemData itemData, int quantity)
        {
            _itemData = itemData;
            _quantity = quantity;
        }

        public bool IsEmpty => _itemData == null || _quantity <= 0;
        public bool CanStackWith(asterivo.Unity60.Features.ActionRPG.Data.ItemData other) => _itemData == other && _itemData.Stackable;
    }
}
