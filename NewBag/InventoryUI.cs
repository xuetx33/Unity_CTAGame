using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFrameWork;

namespace MFrameWork.Inventory
{
    public class InventoryUI : MUIBase
    {
        // 移除[SerializeField]，改为代码加载
        private GameObject slotPrefab;
        private Transform slotContainer;
        private Button closeButton;
        private List<InventorySlot> _slots = new List<InventorySlot>();

        public InventoryUI() : base("InventoryUI", MUILayerType.Normal)
        {
            IsCacheUI = true;
        }

        public override void Init()
        {
            base.Init();

            // 关键修改：从Resources加载物品槽预制体（路径需手动放置）
            slotPrefab = Resources.Load<GameObject>("Prefabs/InventorySlot");
            if (slotPrefab == null)
            {
                Debug.LogError("请将物品槽预制体放在：Resources/Prefabs/InventorySlot.prefab");
                return;
            }

            // 查找UI组件（原有逻辑不变）
            slotContainer = UIGameObject.transform.Find("SlotContainer");
            if (slotContainer == null)
            {
                Debug.LogError("InventoryUI预制体中未找到SlotContainer");
                return;
            }

            closeButton = UIGameObject.transform.Find("CloseButton").GetComponent<Button>();
            if (closeButton == null)
            {
                Debug.LogError("InventoryUI预制体中未找到CloseButton");
                return;
            }
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            InitSlots();
            InventoryManager.Instance.OnInventoryUpdated += UpdateInventoryUI;
        }

        private void InitSlots()
        {
            int capacity = InventoryManager.Instance.GetCapacity();

            // 清除现有槽位（原有逻辑不变）
            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot.gameObject);
            }
            _slots.Clear();

            // 生成新槽位（使用代码加载的slotPrefab）
            for (int i = 0; i < capacity; i++)
            {
                GameObject slotObj = GameObject.Instantiate(slotPrefab, slotContainer);
                InventorySlot slot = slotObj.GetComponent<InventorySlot>();
                if (slot == null)
                {
                    Debug.LogError("物品槽预制体未挂载InventorySlot脚本");
                    continue;
                }
                slot.OnSlotClicked += OnSlotClicked;
                _slots.Add(slot);
            }
        }

        private void UpdateInventoryUI()
        {
            List<Item> items = InventoryManager.Instance.GetAllItems();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (i < items.Count)
                {
                    _slots[i].SetItem(items[i], i);
                }
                else
                {
                    _slots[i].ClearSlot();
                }
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            Item item = _slots[slotIndex].GetItem();
            if (item != null)
            {
                Debug.Log($"点击物品：{item.name}（数量：{item.currentCount}）");
                // 可扩展：物品使用/装备逻辑
            }
        }

        private void OnCloseButtonClicked()
        {
            Active = false;
        }

        protected override void OnActive()
        {
            UpdateInventoryUI();
        }

        protected override void OnDeActive() { }

        public override void Uninit()
        {
            InventoryManager.Instance.OnInventoryUpdated -= UpdateInventoryUI;
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            foreach (var slot in _slots)
            {
                slot.OnSlotClicked -= OnSlotClicked;
            }
            base.Uninit();
        }
    }
}