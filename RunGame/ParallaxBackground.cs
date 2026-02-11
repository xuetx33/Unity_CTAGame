using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform target; // 主角的Transform
    public float parallaxSpeed = 0.5f; // 背景移动速度（小于主角速度）
    private float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        float distance = target.position.x * (1 - parallaxSpeed);
        float movement = target.position.x * parallaxSpeed;
        transform.position = new Vector3(startX + movement, transform.position.y, transform.position.z);
    }
}