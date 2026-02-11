using UnityEngine;

public class RunPlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float runSpeed = 5f; // 由RunGameManager动态控制
    public float jumpForce = 10f;
    private bool isGrounded;
    public LayerMask groundLayer; // 地面图层掩码
    public float groundCheckRadius = 0.6f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 检查核心组件，避免遗漏配置
        if (rb == null)
            Debug.LogError("RunPlayerController：请为主角添加Rigidbody2D组件！");
    }

    void Update()
    {
        if (rb == null) return; // 组件缺失时停止执行，避免报错

        // 自动跑（速度由GameManager控制，此处保持原有赋值逻辑）
        rb.velocity = new Vector2(runSpeed, rb.velocity.y);

        // 地面检测（保留你原有的射线检测参数）
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius, groundLayer);

        // 空格跳跃（保留原有逻辑）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // 保留你原有的射线Gizmos绘制逻辑
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Vector2.down * groundCheckRadius;
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection);
    }

    // 新增：碰撞障碍物时通知GameManager（核心配套逻辑）
    private void OnCollisionEnter2D(Collision2D other)
    {
        // 仅响应"Obstacle"标签的障碍物（确保普通障碍物触发追赶，可打破障碍不触发）
        if (other.gameObject.CompareTag("Obstacle"))
        {
            // 安全调用GameManager的碰撞方法（避免GameManager未赋值报错）
            RunGameManager.Instance?.OnObstacleHit();
        }
    }
}