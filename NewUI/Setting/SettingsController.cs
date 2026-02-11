using UnityEngine;
using UnityEngine.UI;
using MFrameWork;

namespace MFrameWork
{
    public class SettingsController : MUIBase
    {
        // 定义UI组件变量
        private Button _saveButton;
        private Button _loadButton;
        private Slider _volumeSlider;
        private Toggle _toggle;
        private Button _closeButton;

        public SettingsController() : base("SettingsPanel", MUILayerType.Normal)
        {
            IsCacheUI = true; // 缓存UI，关闭后不销毁
        }

        public override void Init()
        {
            base.Init(); // 调用基类的Init，这会实例化SettingsPanel预制体，并赋值给m_uiGameObject

            // 检查m_uiGameObject是否成功创建
            if (m_uiGameObject == null)
            {
                Debug.LogError("SettingsPanel 预制体未成功加载，无法查找UI组件！");
                return;
            }

            // 使用m_uiGameObject.transform.Find来查找子组件
            // 请确保以下路径与你在SettingsPanel预制体中的实际层级完全一致
            _saveButton = FindUIComponent<Button>("Panel/SaveButton");
            _loadButton = FindUIComponent<Button>("Panel/LoadButton");
            _volumeSlider = FindUIComponent<Slider>("Panel/text_voice/Slider");
            _toggle = FindUIComponent<Toggle>("title/Toggle");
            _closeButton = FindUIComponent<Button>("Close_button");

            // 检查是否有组件未找到
            if (_saveButton == null || _loadButton == null || _volumeSlider == null || _toggle == null || _closeButton == null)
            {
                Debug.LogError("SettingsPanel 中有UI组件未找到，请检查路径是否正确！");
                return;
            }

            // 绑定事件
            _saveButton.onClick.AddListener(OnSaveClick);
            _loadButton.onClick.AddListener(OnLoadClick);
            _volumeSlider.onValueChanged.AddListener(OnVolumeChange);
            _toggle.onValueChanged.AddListener(OnToggleChange);
            _closeButton.onClick.AddListener(OnCloseClick);

            Debug.Log("设置面板初始化完成");
        }

        /// <summary>
        /// 辅助函数，用于查找UI组件，简化代码
        /// </summary>
        private T FindUIComponent<T>(string path) where T : UnityEngine.Component
        {
            Transform trans = m_uiGameObject.transform.Find(path);
            if (trans == null)
            {
                Debug.LogError($"在 {m_uiGameObject.name} 中未找到路径为 {path} 的对象");
                return null;
            }
            T component = trans.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"在 {path} 对象上未找到 {typeof(T).Name} 组件");
            }
            return component;
        }

        protected override void OnActive()
        {
            Debug.Log("设置面板显示");
            RefreshUISettings();
        }

        protected override void OnDeActive()
        {
            Debug.Log("设置面板隐藏");
        }

        // ... [其他事件处理方法保持不变] ...

        private void OnSaveClick() { 
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Save();
                Debug.Log("点击了保存按钮");
            }
            else
            {
                Debug.LogError("SaveSystem.Instance 未初始化，无法保存！");
            }
        }
        private void OnLoadClick()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Load();
                Debug.Log("点击了加载按钮");
            }
            else
            {
                Debug.LogError("SaveSystem.Instance 未初始化，无法加载！");
            }
        }
        private void OnVolumeChange(float value) {

            GameManager.Instance.audioSource.volume = value;
            Debug.Log("音量调整为：" + value); }
        private void OnToggleChange(bool isOn)
        {

            if (isOn)
            {
                //激活声音对象自动播放
                GameManager.Instance.audioSource.gameObject.SetActive(true);
                //Volume();
            }
            else
            {
                GameManager.Instance.audioSource.gameObject.SetActive(false);

                Debug.Log("开关状态：" + isOn);
            }
        }
        private void OnCloseClick() { MUIManager.Instance.DeActiveUI("SettingsPanel"); }

        private void RefreshUISettings()
        {
            // 示例：从游戏管理器中读取当前设置并更新UI
            // if (GameManager.Instance != null)
            // {
            //     _volumeSlider.value = GameManager.Instance.Volume;
            //     _toggle.isOn = GameManager.Instance.IsFullScreen;
            // }
        }

        public override void Uninit()
        {
            base.Uninit();
            // 移除事件监听
            if (_saveButton != null) _saveButton.onClick.RemoveListener(OnSaveClick);
            if (_loadButton != null) _loadButton.onClick.RemoveListener(OnLoadClick);
            if (_volumeSlider != null) _volumeSlider.onValueChanged.RemoveListener(OnVolumeChange);
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(OnToggleChange);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClick);
        }
    }
}