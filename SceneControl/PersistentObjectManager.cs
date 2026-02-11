using UnityEngine;
using System.Collections.Generic;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager instance;

    // 存储所有已注册的持久化物体
    private Dictionary<string, GameObject> persistentObjects = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // 确保管理器本身是单例
        if (instance == null)
        {
            instance = this;
      
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 注册物体并检查重复
    public void RegisterPersistentObject(GameObject obj, string id)
    {
        //Debug.Log($"检查重持久化物体: {id}", obj);
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("物体ID不能为空！", obj);
            return;
        }

        // 如果已存在相同ID的物体，则销毁新的重复物体
        if (persistentObjects.ContainsKey(id))
        {
            Debug.Log($"销毁重复的持久化物体: {id}", obj);
            Destroy(obj);
        }
        else
        {
            // 否则添加到字典中
            persistentObjects[id] = obj;
            Debug.Log($"注册持久化物体: {id}", obj);
        }
    }

    // 取消注册物体（需要时调用）
    public void UnregisterPersistentObject(string id)
    {
        if (persistentObjects.ContainsKey(id))
        {
            persistentObjects.Remove(id);
        }
    }

    // 获取指定ID的持久化物体
    public GameObject GetPersistentObject(string id)
    {
        if (persistentObjects.TryGetValue(id, out GameObject obj))
        {
            return obj;
        }
        return null;
    }
}
