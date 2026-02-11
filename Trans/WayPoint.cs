using UnityEditor;
using UnityEngine;
//传送点脚本
[System.Serializable] // 使类可在Inspector面板中显示和编辑
public class WayPoint
{
    [Tooltip("路径点编号，需与传送器的pathPoint匹配")]
    public int pathPoint; // 路径点唯一编号，用于场景间传送关联

    [Tooltip("玩家传送到此路径点时的目标位置锚点")]
    public Transform PointAnchor; // 标记玩家传送后的位置（通常为空物体的Transform）
}



