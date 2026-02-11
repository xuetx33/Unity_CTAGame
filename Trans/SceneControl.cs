using System.Collections.Generic;
using UnityEngine;

//场景控制器，用于控制场景中的一些通用信息的类

public class SceneControl : MonoBehaviour
{


    public List<WayPoint> list = new List<WayPoint>();//保存的当前场景中的所有传送点触发器的对象
    public Transform player;

    public static int state = 0;//0正常，1改变场景，2交互
    public static int pathPoint;//角色传送到当前场景时对应通过的路径点标记

    private void Start()
    {
        // 如果未手动绑定玩家，则自动查找标签为"Player"的对象
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("ShenYan");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("场景B中未找到标签为Player的对象！");
                return;
            }
        }
        //场景初始加载，遍历当前所有触发点，找到角色在该场景的传送位置
        foreach (WayPoint point in list)
        {
            if (point.pathPoint == pathPoint && point.PointAnchor && player)
            {
                player.position = point.PointAnchor.position;
                Debug.Log("传送成功！");
                break;
            }
        }

    }
    }