using System.Collections.Generic;
using UnityEngine;

public class ObstaclePool : MonoBehaviour
{
    public static ObstaclePool Instance;
    public List<GameObject> obstaclePrefabs; // 所有障碍物预制体
    public int poolSizePerType = 10; // 每种障碍物的池大小
    private Dictionary<GameObject, List<GameObject>> poolDictionary = new Dictionary<GameObject, List<GameObject>>();

    void Awake()
    {
        Instance = this;
        // 初始化每种预制体的对象池
        foreach (var prefab in obstaclePrefabs)
        {
            List<GameObject> pool = new List<GameObject>();
            for (int i = 0; i < poolSizePerType; i++)
            {
                GameObject obs = Instantiate(prefab);
                obs.SetActive(false);
                pool.Add(obs);
            }
            poolDictionary.Add(prefab, pool);
        }
    }

    // 获取指定预制体的障碍物
    public GameObject GetObstacle(GameObject prefab)
    {
        foreach (var obs in poolDictionary[prefab])
        {
            if (!obs.activeInHierarchy)
            {
                return obs;
            }
        }
        // 池不足时临时创建
        GameObject newObs = Instantiate(prefab);
        poolDictionary[prefab].Add(newObs);
        return newObs;
    }
}