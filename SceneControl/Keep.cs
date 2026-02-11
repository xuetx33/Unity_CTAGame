using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//此脚本用于在切换场景时保留游戏物体，只需要给游戏物体挂载上该脚本即可
public class Keep : MonoBehaviour
{

    [Tooltip("为该物体设置唯一标识符，防止重复创建")]
    public string uniqueId;

    // 可以在这里保留你原有的其他参数
    [Tooltip("是否在场景切换时保留此物体")]
    public bool keepOnSceneChange = true;

    private void Awake()
    {
        // 保留你原有的DontDestroyOnLoad逻辑
        if (keepOnSceneChange)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // 直接使用单例实例，无需查找
        if (PersistentObjectManager.instance == null)
        {
            Debug.LogError("场景中没有PersistentObjectManager实例！请先创建并挂载管理器脚本。");
            return;
        }

        // 根据开关决定是否注册为持久化物体
        if (keepOnSceneChange)
        {
            PersistentObjectManager.instance.RegisterPersistentObject(gameObject, uniqueId);
        }
    }

    // 可以在这里添加你原有的其他方法
    public void SetKeepOnSceneChange(bool keep)
    {
        keepOnSceneChange = keep;
        if (keep)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
