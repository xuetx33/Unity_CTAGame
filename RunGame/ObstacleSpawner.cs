using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;
    private float nextSpawnTime;
    public float spawnXPosOffset = 15f; // 相对于主角前方的生成距离
    public Transform player; // 主角位置（用于动态计算生成X坐标）
    public float groundY = 0; // 地面Y坐标（确保障碍物生成在地面上）
    public List<GameObject> obstaclePrefabs; // 多个障碍物预制体（随机选择）

    void Update()
    {
        if (player == null) return;

        // 动态计算生成位置（主角前方15单位）
        float currentSpawnX = player.position.x + spawnXPosOffset;

        if (Time.time >= nextSpawnTime)
        {
            SpawnObstacle(currentSpawnX);
            nextSpawnTime = Time.time + Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    void SpawnObstacle(float xPos)
    {
        // 随机选择一个障碍物预制体
        GameObject randomPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
        GameObject obstacle = ObstaclePool.Instance.GetObstacle(randomPrefab); // 需修改对象池支持多预制体

        // 生成在地面上（Y坐标固定为地面高度）
        obstacle.transform.position = new Vector2(xPos, groundY);
        obstacle.SetActive(true);
    }
}