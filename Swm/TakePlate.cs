using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("盘子设置")]
    public Transform customerTransform; // 顾客位置
    public List<int> itemsOnPlate = new List<int>(); // 盘子上的物品列表
    public List<TakeItem> itemObjectsOnPlate = new List<TakeItem>(); // 盘子上的物品对象

    void Start()
    {
        // 移除拖拽相关的初始化
    }

    // 添加物品到盘子
    public void AddItem(TakeItem item)
    {
        if (!itemsOnPlate.Contains(item.Kind))
        {
            itemsOnPlate.Add(item.Kind);
            itemObjectsOnPlate.Add(item);

            // 设置物品为盘子的子物体
            item.transform.SetParent(transform);

            Debug.Log($"盘子添加了物品: {GetItemName(item.Kind)}");
        }
    }

    // 提交给顾客（公共方法，供按钮调用）
    public void SubmitToCustomer()
    {
        if (itemsOnPlate.Count == 0)
        {
            Debug.Log("盘子上没有物品，无法提交");
            return;
        }

        Customer customer = FindObjectOfType<Customer>();
        if (customer != null)
        {
            customer.ReceiveOrder(itemsOnPlate);

            // 提交后清空盘子并重置所有物品
            ClearPlate();
        }
    }

    // 清空盘子并重置物品（公共方法，供按钮调用）
    public void ClearPlate()
    {
        // 重置所有TakeItem物品
        foreach (TakeItem item in itemObjectsOnPlate)
        {
            if (item != null)
            {
                // 解除父子关系
                item.transform.SetParent(null);
                item.ResetItem();
            }
        }

        itemsOnPlate.Clear();
        itemObjectsOnPlate.Clear();
        Debug.Log("盘子已清空，所有物品已重置");
    }

    // 获取物品名称
    string GetItemName(int itemType)
    {
        switch (itemType)
        {
            case 1: return "香囊";
            case 2: return "拨浪鼓";
            case 3: return "红糖";
            case 4: return "茶叶";
            case 5: return "针线";
            case 6: return "蒲扇";
            case 7: return "毛笔";
            case 8: return "酒";
            default: return "未知物品";
        }
    }

    // 移除可视化显示提交范围（因为不再需要拖拽检测）
    // void OnDrawGizmosSelected() 方法已移除
}