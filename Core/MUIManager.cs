using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MFrameWork
{
    public class MUIManager
    {
        // 单例实例
        public static MUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MUIManager();
                    _instance.InitUIInfo();
                }
                return _instance;
            }
        }
        private static MUIManager _instance;

        private Dictionary<string, MUIBase> m_uiDict;
        public GameObject m_uiRoot;
        private Transform m_transNormal;
        private Transform m_transTop;
        private Transform m_transUpper;
        private Transform m_transHud;
        private UIMaskController _maskController; // 遮罩控制器实例
        private int _maskRefCount = 0; // 遮罩引用计数（解决多UI叠加问题）
        private MUIManager() { }

        public bool InitUIInfo()
        {
            m_uiDict = new Dictionary<string, MUIBase>();
            // 创建UI根节点
            m_uiRoot = new GameObject("UIRoot");
            m_uiRoot.AddComponent<CoroutineRunner>(); // 给它一个跑协程的身份
            GameObject.DontDestroyOnLoad(m_uiRoot);

            // 创建全局 Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(m_uiRoot.transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 添加 Canvas Scaler 组件以支持分辨率适配
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2560, 1440); // 设置你的设计分辨率

            // 创建 EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.transform.SetParent(m_uiRoot.transform);
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            // --- 以下是修改的核心部分 ---

            // 创建一个函数来生成全屏的层级节点
            System.Func<string, Transform> createFullScreenLayer = (string name) =>
            {
                GameObject layerObj = new GameObject(name);
                layerObj.transform.SetParent(canvasObj.transform);

                // 添加 Rect Transform 组件
                RectTransform rectTrans = layerObj.AddComponent<RectTransform>();

                // 设置锚点以填充整个父对象 (Canvas)
                rectTrans.anchorMin = Vector2.zero;
                rectTrans.anchorMax = Vector2.one;

                // 设置偏移量为0
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
                RegisterUI("UITransition", new UITransition()); 
                RegisterUI("GameHUD", new GameHUDController());
                ActiveUI("GameHUD");
                RegisterUI("TalkPanel", new TalkUIController());
                RegisterUI("TaskPanel", new TaskUIController());
                return layerObj.transform;
            };

            // 使用上面的函数创建各个层级
            m_transNormal = createFullScreenLayer("NormalLayer");
            m_transTop = createFullScreenLayer("TopLayer");
            m_transUpper = createFullScreenLayer("UpperLayer");
            m_transHud = createFullScreenLayer("HudLayer");
            // ========== 新增：初始化遮罩控制器 ==========
            InitMaskController();
            
            return true;
        }

        public Transform GetLayerTransform(MUILayerType layerType)
        {
            switch (layerType)
            {
                case MUILayerType.Normal: return m_transNormal;
                case MUILayerType.Top: return m_transTop;
                case MUILayerType.Upper: return m_transUpper;
                case MUILayerType.Hud: return m_transHud;
                default: return null;
            }
        }

        // 注册UI（示例：注册登录界面）
        public void RegisterUI(string uiName, MUIBase uiInstance)
        {
            if (!m_uiDict.ContainsKey(uiName))
            {
                m_uiDict.Add(uiName, uiInstance);
                Debug.Log("注册UI成功：" + uiName);
            }
        }

        // 激活UI
        public MUIBase ActiveUI(string uiName)
        {
            if (m_uiDict.TryGetValue(uiName, out MUIBase ui))
            {
                if (!ui.IsInited)
                {
                    ui.Init();
                }
                ui.Active = true;
                GameManager.Instance.canControlShenYan = false;
                return ui;
            }
            Debug.LogError("UI未注册：" + uiName);
            return null;
        }

        // 隐藏UI
        public void DeActiveUI(string uiName)
        {
            if (m_uiDict.TryGetValue(uiName, out MUIBase ui))
            {
                ui.Active = false;
                if (!ui.IsCacheUI)
                {
                    ui.Uninit();
                }
                GameManager.Instance.canControlShenYan = true;
            }
            else
            {
                Debug.LogError("UI未注册：" + uiName);
            }
        }

        // 获取UI
        public MUIBase GetUI(string uiName)
        {
            m_uiDict.TryGetValue(uiName, out MUIBase ui);
            return ui;
        }

        // 隐藏所有UI
        public void DeActiveAll()
        {
            foreach (var ui in m_uiDict.Values)
            {
                DeActiveUI(ui.UIName);
            }
        }

        // 更新逻辑
        public void Update(float deltaTime)
        {
            foreach (var ui in m_uiDict.Values)
            {
                ui.Update(deltaTime);
            }
         
        }

        // 延迟更新逻辑
        public void LateUpdate(float deltaTime)
        {
            foreach (var ui in m_uiDict.Values)
            {
                ui.LateUpdate(deltaTime);
            }
        }
        // ========== 新增：初始化遮罩控制器 ==========
        private void InitMaskController()
        {
            _maskController = new UIMaskController();
            RegisterUI("UIMaskPanel", _maskController);
            if (!_maskController.IsInited)
            {
                _maskController.Init(); // 提前初始化遮罩
            }
        }

        // ========== 新增：显示全局遮罩 ==========
        public void ShowGlobalMask(float alpha = 0.7f)
        {
            _maskRefCount++;
            if (_maskRefCount <= 0) _maskRefCount = 1;

            if (_maskController != null)
            {
                _maskController.SetMaskAlpha(alpha);
                _maskController.Active = true;
            }
        }

        // ========== 新增：隐藏全局遮罩 ==========
        public void HideGlobalMask()
        {
            _maskRefCount--;
            if (_maskRefCount <= 0)
            {
                _maskRefCount = 0;
                if (_maskController != null)
                {
                    _maskController.Active = false;
                }
            }
        }

 
        public void ForceHideGlobalMask()
        {
            _maskRefCount = 0;
            if (_maskController != null)
            {
                _maskController.Active = false;
            }
        }
       
    }


}