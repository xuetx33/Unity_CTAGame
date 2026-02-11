// 在游戏初始化时注册背包UI
using MFrameWork;
using MFrameWork.Inventory;
using UnityEngine;
namespace MFrameWork.Inventory
{
    public class GameInitializer : MonoBehaviour
    {
        void Start()
        {
            // 初始化UI管理器
            var uiManager = MUIManager.Instance;

            // 注册背包UI
            InventoryUI inventoryUI = new InventoryUI();
            uiManager.RegisterUI("InventoryUI", inventoryUI);

            // 初始化一些测试物品
            InitTestItems();
        }

        // 添加测试物品
        private void InitTestItems()
        {
            // 假设已有图标资源
            Sprite potionIcon = Resources.Load<Sprite>("Icons/HealthPotion");
            Sprite swordIcon = Resources.Load<Sprite>("Icons/IronSword");

            // 创建测试物品
            Item healthPotion = new Item(
                1,
                "生命药水",
                "恢复100点生命值",
                potionIcon,
                ItemType.Consumable,
                20
            );

            Item ironSword = new Item(
                2,
                "铁剑",
                "攻击力+10",
                swordIcon,
                ItemType.Equipment,
                1
            );

            // 添加到背包
            InventoryManager.Instance.AddItem(healthPotion, 5);
            InventoryManager.Instance.AddItem(ironSword);
        }

        // 打开背包按钮事件
        public void OnOpenInventoryClicked()
        {
            MUIManager.Instance.ActiveUI("InventoryUI");
        }
    }
}