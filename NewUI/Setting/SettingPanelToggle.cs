using UnityEngine;
using UnityEngine.UI;

public class OpenSettingAndHideButton : MonoBehaviour
{
    // 拖入设置面板
    public GameObject settingPanel;
    // 拖入当前按钮（也可以通过GetComponent获取）
    public Button selfButton;

    void Start()
    {
        // 初始隐藏面板
        if (settingPanel != null)
            settingPanel.SetActive(false);

        // 自动获取按钮组件（如果selfButton没赋值）
        if (selfButton == null)
            selfButton = GetComponent<Button>();

        // 绑定点击事件
        selfButton.onClick.AddListener(OpenPanelAndHide);
    }

    // 打开面板并隐藏按钮
    void OpenPanelAndHide()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);

        if (selfButton != null)
            selfButton.gameObject.SetActive(false); // 隐藏按钮所在的GameObject
    }
}