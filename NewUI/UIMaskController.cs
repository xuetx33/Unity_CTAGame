using UnityEngine;
using UnityEngine.UI;
using MFrameWork;

namespace MFrameWork
{
    public class UIMaskController : MUIBase
    {
        private Image _maskImage;
        private float _targetAlpha = 0.7f;

        public UIMaskController() : base("UIMaskPanel", MUILayerType.Normal)
        {
            IsCacheUI = true;
            EnableMask = false;
        }

        public void SetMaskAlpha(float alpha)
        {
            _targetAlpha = Mathf.Clamp01(alpha);
            if (_maskImage != null)
            {
                _maskImage.color = new Color(0, 0, 0, _targetAlpha);
            }
        }

        public override void Init()
        {
            base.Init();

            if (m_uiGameObject == null)
            {
                Debug.LogError("遮罩预制体加载失败！路径：" + m_uiFullPath);
                return;
            }

            // 关键：保留预制体原有的RectTransform参数，不被框架重置
            RectTransform rectTrans = m_uiGameObject.GetComponent<RectTransform>();
            if (rectTrans != null)
            {
                rectTrans.anchorMin = new Vector2(0, 0);
                rectTrans.anchorMax = new Vector2(1, 1);
                rectTrans.offsetMin = Vector2.zero;
                rectTrans.offsetMax = Vector2.zero;
                rectTrans.pivot = new Vector2(0.5f, 0.5f);
                rectTrans.localScale = Vector3.one;
            }

            // 获取预制体自带的Image组件（不再创建，保留预制体配置）
            _maskImage = m_uiGameObject.GetComponent<Image>();
            if (_maskImage != null)
            {
                _maskImage.color = new Color(0, 0, 0, _targetAlpha);
            }

            // 强制放在NormalLayer最底层
            m_uiGameObject.transform.SetSiblingIndex(0);
            m_uiGameObject.SetActive(false);
        }

        protected override void OnActive()
        {
            m_uiGameObject.SetActive(true);
        }

        protected override void OnDeActive()
        {
            m_uiGameObject.SetActive(false);
        }

        public override void Uninit()
        {
            base.Uninit();
            _maskImage = null;
        }
    }
}