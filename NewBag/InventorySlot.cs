using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MFrameWork.Inventory
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text countText;

        private Item _currentItem;
        private int _slotIndex;

        // 点击事件
        public event UnityAction<int> OnSlotClicked;

        private void Awake()
        {
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();

            if (countText == null)
                countText = GetComponentInChildren<Text>();

            ClearSlot();
        }

        // 设置物品槽内容
        public void SetItem(Item item, int slotIndex)
        {
            _currentItem = item;
            _slotIndex = slotIndex;

            if (item != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = item.icon;

                if (item.maxStackSize > 1 && item.currentCount > 1)
                {
                    countText.enabled = true;
                    countText.text = item.currentCount.ToString();
                }
                else
                {
                    countText.enabled = false;
                }
            }
            else
            {
                ClearSlot();
            }
        }

        // 清空物品槽
        public void ClearSlot()
        {
            _currentItem = null;
            iconImage.enabled = false;
            iconImage.sprite = null;
            countText.enabled = false;
        }

        // 点击物品槽
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(_slotIndex);
        }

        public Item GetItem() => _currentItem;
    }
}