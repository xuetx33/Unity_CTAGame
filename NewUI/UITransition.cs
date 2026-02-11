using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MFrameWork;

namespace MFrameWork
{
    public class UITransition : MUIBase
    {
        // 组件引用
        private Image maskImage;
        private float fadeDuration = 0.8f;

        // 构造函数：必须调用 base 并传入 UI 名字和层级
        public UITransition() : base("UITransition", MUILayerType.Top)
        {
            // 是否启用全局遮罩（由于黑屏本身就是遮罩，这里可以设为 false 避免双重遮罩）
            this.EnableMask = false;
            this.IsCacheUI = true;  // 建议缓存，避免频繁创建销毁
        }

        // 必须实现：初始化逻辑（在这里获取 Prefab 里的组件）
        public override void Init()
        {
            base.Init(); // 先调用基类生成物体

            // 在生成的 m_uiGameObject 中寻找 Image 组件
            // 假设你的预制体根节点或子节点有一个 Image
            maskImage = m_uiGameObject.GetComponentInChildren<Image>();

            if (maskImage == null)
            {
                Debug.LogError("UITransition: 预制体中未找到 Image 组件！");
            }
        }

        // 必须实现：UI 激活时的回调
        protected override void OnActive()
        {
            Debug.Log("黑屏系统已激活");
        }

        // 必须实现：UI 隐藏时的回调
        protected override void OnDeActive()
        {
            Debug.Log("黑屏系统已隐藏");
        }

        // 自定义功能：淡入（变黑）
        public void FadeIn(System.Action onComplete = null)
        {
            // 修改这一行，确保获取的是 CoroutineRunner
            var runner = MUIManager.Instance.m_uiRoot.GetComponent<CoroutineRunner>();
            if (runner != null)
            {
                runner.StartCoroutine(Fade(1, onComplete));
            }
        }

        // 自定义功能：淡出（变透明）
        public void FadeOut(System.Action onComplete = null)
        {
            MUIManager.Instance.m_uiRoot.GetComponent<CoroutineRunner>().StartCoroutine(Fade(0, onComplete));
        }

        private IEnumerator Fade(float targetAlpha, System.Action onComplete)
        {
            if (maskImage == null) yield break;

            float startAlpha = maskImage.color.a;
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
                maskImage.color = new Color(0, 0, 0, newAlpha);
                yield return null;
            }
            maskImage.color = new Color(0, 0, 0, targetAlpha);
            onComplete?.Invoke();
        }
    }
}