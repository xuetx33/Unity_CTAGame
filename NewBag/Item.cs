using UnityEngine;

namespace MFrameWork.Inventory
{
    // 物品类型
    public enum ItemType
    {
        Consumable,   // 消耗品
        Equipment,    // 装备
        Material,     // 材料
        QuestItem     // 任务物品
    }

    // 物品数据类
    [System.Serializable]
    public class Item
    {
        public int id;               // 物品ID
        public string name;          // 物品名称
        public string description;   // 物品描述
        public Sprite icon;          // 物品图标
        public ItemType type;        // 物品类型
        public int maxStackSize = 1; // 最大堆叠数量
        public int currentCount = 1; // 当前数量

        // 构造函数
        public Item(int id, string name, string description, Sprite icon, ItemType type, int maxStackSize)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.icon = icon;
            this.type = type;
            this.maxStackSize = maxStackSize;
        }

        // 复制物品（用于堆叠和拆分）
        public Item Clone()
        {
            return new Item(id, name, description, icon, type, maxStackSize)
            {
                currentCount = currentCount
            };
        }
    }
}