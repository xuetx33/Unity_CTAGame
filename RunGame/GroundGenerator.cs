using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    public GameObject groundSegmentPrefab; // 地面片段预制体
    public int initialSegments = 5; // 初始生成的片段数量
    public float segmentLength = 15f; // 每个片段的长度（需和预制体实际长度一致）
    public Transform player; // 主角位置（用于判断何时生成新片段）
    private List<GameObject> activeSegments = new List<GameObject>();
    private float lastSpawnX; // 上一个生成的片段的X坐标
    public float ylongth;

    void Start()
    {
        // 初始化地面（从原点开始生成初始片段）
        lastSpawnX = 0;
        for (int i = 0; i < initialSegments; i++)
        {
            SpawnGroundSegment();
        }
    }

    void Update()
    {
        // 当主角靠近最后一个地面片段时，生成新片段
        if (player.position.x > lastSpawnX - segmentLength * 2)
        {
            SpawnGroundSegment();
        }

        // 回收超出视野左侧的地面片段（复用）
        RecycleOldSegments();
    }

    // 生成新的地面片段
    void SpawnGroundSegment()
    {
        GameObject segment = Instantiate(groundSegmentPrefab);
        segment.transform.position = new Vector3(lastSpawnX, ylongth, 0); // 沿X轴拼接
        activeSegments.Add(segment);
        lastSpawnX += segmentLength; // 更新下一个生成位置
    }

    // 回收左侧超出视野的片段（移到右侧继续使用）
    void RecycleOldSegments()
    {
        if (activeSegments.Count == 0) return;

        GameObject oldestSegment = activeSegments[0];
        // 当片段在主角左侧超过2个片段长度时，回收
        if (oldestSegment.transform.position.x < player.position.x - segmentLength * 2)
        {
            // 将旧片段移到最右侧继续使用
            oldestSegment.transform.position = new Vector3(lastSpawnX, ylongth, 0);
            lastSpawnX += segmentLength;
            // 从列表移除并重新加入末尾（保持顺序）
            activeSegments.RemoveAt(0);
            activeSegments.Add(oldestSegment);
        }
    }
}