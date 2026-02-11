using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFrameWork.Inventory
{
    public class InventoryManager
    {
        // 单例实例
        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InventoryManager();
                }
                return _instance;
            }
        }
        private static InventoryManager _instance;

        private List<Item> _items = new List<Item>(); // 物品列表
        private int _capacity = 20;                  // 背包容量

        // 事件：物品更新时触发
        public event Action OnInventoryUpdated;

        private InventoryManager() { }

        // 添加物品
        public bool AddItem(Item item, int count = 1)
        {
            if (item == null || count <= 0) return false;

            // 检查是否可以堆叠
            if (item.maxStackSize > 1)
            {
                var existingItem = _items.Find(i => i.id == item.id);
                if (existingItem != null)
                {
                    int remainingSpace = existingItem.maxStackSize - existingItem.currentCount;
                    if (remainingSpace >= count)
                    {
                        existingItem.currentCount += count;
                        OnInventoryUpdated?.Invoke();
                        return true;
                    }
                    else
                    {
                        existingItem.currentCount = existingItem.maxStackSize;
                        count -= remainingSpace;
                    }
                }
            }

            // 检查背包是否已满
            if (_items.Count >= _capacity)
            {
                Debug.LogWarning("背包已满，无法添加物品");
                return false;
            }

            // 添加新物品
            Item newItem = item.Clone();
            newItem.currentCount = count;
            _items.Add(newItem);
            OnInventoryUpdated?.Invoke();
            return true;
        }

        // 移除物品
        public bool RemoveItem(int itemId, int count = 1)
        {
            var item = _items.Find(i => i.id == itemId);
            if (item == null) return false;

            if (item.currentCount > count)
            {
                item.currentCount -= count;
                OnInventoryUpdated?.Invoke();
                return true;
            }
            else
            {
                _items.Remove(item);
                OnInventoryUpdated?.Invoke();
                return true;
            }
        }

        // 获取物品列表
        public List<Item> GetAllItems()
        {
            return new List<Item>(_items);
        }

        // 清空背包
        public void ClearInventory()
        {
            _items.Clear();
            OnInventoryUpdated?.Invoke();
        }

        // 检查背包是否有空位
        public bool HasEmptySlot()
        {
            return _items.Count < _capacity;
        }

        // 设置背包容量
        public void SetCapacity(int capacity)
        {
            if (capacity > 0)
            {
                _capacity = capacity;
                OnInventoryUpdated?.Invoke();
            }
        }

        // 获取背包容量
        public int GetCapacity() => _capacity;
    }
}