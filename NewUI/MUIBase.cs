// **************************************
//
// 文件名(MUIBase.cs):
// 功能描述("UI基类"):
// 作者(Max1993):
// 日期(2019/5/19  21:26):
//
// **************************************
//
using UnityEngine;

namespace MFrameWork
{
    /// <summary>
    /// UI层级
    /// </summary>
    public enum MUILayerType
    {
        Top,
        Upper,
        Normal,
        Hud
    }

    public abstract class MUIBase
    {
        protected bool m_isInited;
        protected string m_uiName;
        protected bool m_isCacheUI = false;
        protected GameObject m_uiGameObject;
        protected bool m_active = false;
        protected string m_uiFullPath = "";
        protected MUILayerType m_uiLayerType;

        // ========== 新增：遮罩相关配置 ==========
        /// <summary>
        /// 是否启用遮罩（默认开启，子类可关闭）
        /// </summary>
        public bool EnableMask { get; set; } = true;

        /// <summary>
        /// 遮罩透明度（默认0.7，子类可自定义）
        /// </summary>
        public float MaskAlpha { get; set; } = 0.7f;



        public string UIName
        {
            get { return m_uiName; }
            set
            {
                m_uiName = value;
                // 简化路径，假设UI预制体放在**目录下
                m_uiFullPath = "Prefabs/UI/" + m_uiName;
            }
        }

        public bool IsCacheUI
        {
            get { return m_isCacheUI; }
            set { m_isCacheUI = value; }
        }

        public GameObject UIGameObject
        {
            get { return m_uiGameObject; }
            set { m_uiGameObject = value; }
        }

        public bool Active
        {
            get { return m_active; }
            set
            {
                m_active = value;
                if (m_uiGameObject != null)
                {
                    m_uiGameObject.SetActive(value);
                    if (m_uiGameObject.activeSelf)
                    {
                        // ========== 新增：激活UI时自动显示遮罩 ==========
                        if (EnableMask)
                        {
                            MUIManager.Instance.ShowGlobalMask(MaskAlpha);
                        }
                        OnActive();
                    }
                    else
                    {
                        // ========== 新增：隐藏UI时自动隐藏遮罩 ==========
                        if (EnableMask)
                        {
                            MUIManager.Instance.HideGlobalMask();
                        }
                        OnDeActive();
                    }
                }
            }
        }

        public bool IsInited { get { return m_isInited; } }

        protected MUIBase(string uiName, MUILayerType layerType)
        {
            UIName = uiName;
            m_uiLayerType = layerType;
        }

        public virtual void Init()
        {
            // 从Resources加载预制体
            GameObject go = Resources.Load<GameObject>(m_uiFullPath);
            if (go == null)
            {
                Debug.LogError("UI加载失败，路径：" + m_uiFullPath);
                return;
            }
            m_uiGameObject = GameObject.Instantiate(go);
            m_isInited = true;
            SetPanelByLayerType(m_uiLayerType);
            m_uiGameObject.transform.localPosition = Vector3.zero;
            m_uiGameObject.transform.localScale = Vector3.one;
        }

        private void SetPanelByLayerType(MUILayerType layerType)
        {
            Transform parent = MUIManager.Instance.GetLayerTransform(layerType);
            if (parent != null)
            {
                m_uiGameObject.transform.SetParent(parent);

                // ========== 新增：强制 UI 铺满全屏 ==========
                RectTransform rect = m_uiGameObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.localScale = Vector3.one;
                }
            }
            else
            {
            }
        }
        public virtual void Uninit()
        {
            m_isInited = false;
            m_active = false;
            if (EnableMask)
            {
                MUIManager.Instance.ForceHideGlobalMask();
            }
            if (m_isCacheUI)
            {
                m_uiGameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(m_uiGameObject);
            }
        }

        protected abstract void OnActive();

        protected abstract void OnDeActive();

        public virtual void Update(float deltaTime) { }

        public virtual void LateUpdate(float deltaTime) { }

        public virtual void OnLogOut() { }
    }
}