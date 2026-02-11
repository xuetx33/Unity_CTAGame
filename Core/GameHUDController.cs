using UnityEngine;
using UnityEngine.UI;
using MFrameWork;

namespace MFrameWork
{
    public class GameHUDController : MUIBase
    {
        private Button _settingButton;
        private Button _taskButton;

        public GameHUDController() : base("GameHUD", MUILayerType.Normal)
        {
            IsCacheUI = true;
            EnableMask = false;
        }

        public override void Init()
        {
            base.Init();
            
           // 查找设置按钮（根据你的层级直接查找）
           _settingButton = m_uiGameObject.transform.Find("Seting_Button").GetComponent<Button>();
            _taskButton = m_uiGameObject.transform.Find("Task_Button").GetComponent <Button>();

            // 绑定点击事件
            _settingButton.onClick.AddListener(OnSettingButtonClicked);
            _taskButton.onClick.AddListener(OnTaskButtonClicked);

            Debug.Log("GameHUD 初始化完成");
        }

        protected override void OnActive()
        {
            Debug.Log("GameHUD 显示");
        }

        protected override void OnDeActive()
        {
            Debug.Log("GameHUD 隐藏");
        }


        private void OnSettingButtonClicked()
        {
            Debug.Log("点击了设置按钮");
            // 打开设置面板
            OpenPanel("SettingsPanel", () => new SettingsController());
        }
        private void OnTaskButtonClicked()
        {
            Debug.Log("点击了任务按钮");
            // 打开设置面板
            OpenPanel("TaskPanel", () => new SettingsController());
        }
        private void OpenPanel(string panelName, System.Func<MUIBase> createController)
        {
            if (MUIManager.Instance.GetUI(panelName) == null)
            {
                MUIManager.Instance.RegisterUI(panelName, createController());
            }
            MUIManager.Instance.ActiveUI(panelName);
        }

        public override void Uninit()
        {
            base.Uninit();
           // _settingButton.onClick.RemoveListener(OnSettingButtonClicked);
        }
    }
}