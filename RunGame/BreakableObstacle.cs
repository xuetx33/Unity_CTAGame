using UnityEngine;

public class BreakableObstacle : MonoBehaviour
{
    [Header("破坏设置")]
    public KeyCode breakKey = KeyCode.E; // 破坏按键（可自定义为E、空格等）
    public float detectRange = 1.5f; // 检测玩家的距离范围
    public GameObject breakEffect; // 破坏特效（如粒子效果，可选）
    public float destroyDelay = 0.1f; // 特效播放后延迟销毁的时间

    private bool isPlayerNearby; // 玩家是否在范围内
    private Transform playerTransform; // 主角位置引用
  

    
    void Update()
    {
        // 检测玩家是否在范围内
        CheckPlayerDistance();

        // 玩家在范围内且按下破坏键时，执行破坏
        if (isPlayerNearby && Input.GetKeyDown(breakKey))
        {
            Break();
        }
    }

    // 检测玩家是否在可交互范围内
    void CheckPlayerDistance()
    {
        if (playerTransform == null)
        {
           
                // 自动获取场景中的主角（假设主角Tag为"Player"）
                playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
                return;
            
        }
        else
        {    // 计算与玩家的水平距离（2D跑酷主要关注X轴）
            float distance = Mathf.Abs(playerTransform.position.x - transform.position.x);
            isPlayerNearby = distance <= detectRange;
           Debug.Log("Player Distance: " + distance + " | Nearby: " + isPlayerNearby);
        }
    }

    // 破坏障碍物的逻辑
    void Break()
    {
        // 播放破坏特效（如果有）
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        //// 禁用碰撞和渲染，延迟后销毁（或回收至对象池）
        //GetComponent<SpriteRenderer>().enabled = false;
        //GetComponent<Collider2D>().enabled = false;
        //Destroy(gameObject, destroyDelay);

        gameObject.SetActive(false);
    }

    // 可视化检测范围（Gizmos辅助调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 绘制检测范围的圆形（在X轴方向扩展）
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}