using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C : MonoBehaviour
{
    //碰撞体对象标签（场景中必须唯一）
    public string confinerTag = "CameraConfinerBounds";

    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            Debug.LogError("虚拟摄像机上没有CinemachineConfiner2D组件！");
            enabled = false;
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // 场景加载完成后的回调
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 使用标签查找碰撞体（性能更优）
        GameObject collisionObject = GameObject.FindWithTag(confinerTag);
        if (collisionObject == null)
        {
            Debug.LogError($"新场景中找不到标签为 {confinerTag} 的碰撞体对象！");
            return;
        }

        Collider2D newCollider = collisionObject.GetComponent<Collider2D>();
        if (newCollider == null)
        {
            Debug.LogError($"标签为 {confinerTag} 的对象上没有挂载2D碰撞体组件！");
            return;
        }

        confiner.m_BoundingShape2D = newCollider;
        Debug.Log($"已为场景 {scene.name} 更新CinemachineConfiner2D碰撞体");
    }


    // 避免脚本销毁时内存泄漏
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
