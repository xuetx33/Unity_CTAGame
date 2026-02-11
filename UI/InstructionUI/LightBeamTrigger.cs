using UnityEngine;

public class LightBeamWithBreath : MonoBehaviour
{
    // 引用当前物体的SpriteRenderer组件（用于显示和呼吸效果）
    private SpriteRenderer spriteRenderer;

    // 呼吸闪烁效果参数（可在Inspector面板调整）
    public float flickerSpeed = 2f; // 闪烁速度（数值越大越快）
    public float minAlpha = 0.5f;   // 最小透明度（0-1，0完全透明，1完全不透明）
    public float maxAlpha = 1f;     // 最大透明度

    void Start()
    {
        // 获取组件并初始隐藏光柱
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        // 只有光柱显示时，才执行呼吸效果
        if (spriteRenderer.enabled)
        {
            // 用PingPong和Lerp实现平滑的透明度渐变（呼吸感）
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * flickerSpeed, 1f));
            Color currentColor = spriteRenderer.color;
            currentColor.a = alpha; // 只修改透明度
            spriteRenderer.color = currentColor;
        }
    }

    // 玩家进入触发区域时调用
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检测进入的物体是否是玩家（标签需设置为"Player"）
        if (other.CompareTag("Luna"))
        {
            spriteRenderer.enabled = true; // 显示光柱并开始呼吸效果
        }
    }

    // 玩家离开触发区域时调用
    void OnTriggerExit2D(Collider2D other)
    {
        // 检测离开的物体是否是玩家
        if (other.CompareTag("Luna"))
        {
            spriteRenderer.enabled = false; // 隐藏光柱并停止呼吸效果
        }
    }
}