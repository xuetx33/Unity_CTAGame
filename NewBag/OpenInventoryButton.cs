using UnityEngine;
using UnityEngine.UI;
using MFrameWork; // 引用UI框架命名空间

public class OpenBagButton : MonoBehaviour
{
    private Button _button;

    void Awake()
    {
        // 获取按钮组件
        _button = GetComponent<Button>();
        if (_button != null)
        {
            // 绑定点击事件：点击时打开背包
            _button.onClick.AddListener(OpenInventory);
        }
    }

    // 打开背包逻辑
    private void OpenInventory()
    {
        // 激活背包UI（假设背包UI的注册名称是"InventoryUI"）
        var inventory = MUIManager.Instance.ActiveUI("InventoryUI");
        if (inventory == null)
        {
            Debug.LogWarning("背包未注册，请先注册InventoryUI");
        }
    }
}