using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    public float speed = 0.5f;      // 移动速度
    public float distance = 0.0001f;   // 移动距离
    private Vector3 startPos;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        startPos = transform.position;
    }

    void Update()
    {
        // 用 PingPong 实现来回移动
        float step = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + Vector3.up * step;
    }
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