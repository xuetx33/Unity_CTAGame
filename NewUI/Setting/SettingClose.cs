using UnityEngine;
using UnityEngine.UI;

public class ClosePanelAndShowButton : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject originalButton;

    void Start()
    {
        Button closeBtn = GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseAndShow);
        }
    }

    // 关闭面板并显示原按钮
    void CloseAndShow()
    {
        // 关闭设置面板
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }

        // 显示原按钮
        if (originalButton != null)
        {
            originalButton.SetActive(true);
        }
    }
}