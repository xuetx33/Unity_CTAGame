using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeItem : MonoBehaviour
{
    [Header("物品设置")]
    private Vector2 startPos; // 初始位置
    private Quaternion startRot; // 初始旋转
    private Vector3 startScale; // 初始缩放
    private Transform startParent; // 初始父物体
    public Transform correctTrans; // 正确位置（盘子位置）
    private bool isCorrectTrans = false; // 是否在正确位置
    public int Kind; // 识别物品类型，1香囊，2拨浪鼓，3红糖,4茶叶，5针线，6蒲扇，7毛笔，8酒

    private Plate plate; // 盘子引用
    private bool hasBeenPlaced = false; // 是否已经被放置过

    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        startScale = transform.localScale;
        startParent = transform.parent;
        plate = FindObjectOfType<Plate>(); // 确保能找到盘子
    }

    private void OnMouseDrag()
    {
        if (!isCorrectTrans && !hasBeenPlaced)
        {
            transform.position = new Vector2(
                Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                Camera.main.ScreenToWorldPoint(Input.mousePosition).y
            );
        }
    }

    private void OnMouseUp()
    {
        if (!hasBeenPlaced &&
            Mathf.Abs(transform.position.x - correctTrans.position.x) <= 2 &&
            Mathf.Abs(transform.position.y - correctTrans.position.y) <= 2)
        {
            // 放置到盘子位置
            transform.position = correctTrans.position;
            transform.rotation = Quaternion.identity; // 重置旋转
            isCorrectTrans = true;
            hasBeenPlaced = true;

            // 设置为盘子的子物体
            if (plate != null)
            {
                transform.SetParent(plate.transform);
                plate.AddItem(this);
            }
            else
            {
                Debug.LogError("盘子引用为空，请确保场景中有Plate对象");
            }
        }
        else if (!hasBeenPlaced)
        {
            // 回到初始位置
            ResetToStart();
        }
    }

    private void OnMouseEnter()
    {
        if (!isCorrectTrans && !hasBeenPlaced)
        {
            // 使用临时变量计算缩放，避免直接修改localScale
            transform.localScale = startScale * 1.07f;
        }
    }

    private void OnMouseExit()
    {
        if (!isCorrectTrans && !hasBeenPlaced)
        {
            // 直接恢复为初始缩放
            transform.localScale = startScale;
        }
    }

    // 重置物品状态
    public void ResetItem()
    {
        ResetToStart();
        isCorrectTrans = false;
        hasBeenPlaced = false;
    }

    // 重置到初始状态
    private void ResetToStart()
    {
        // 解除父子关系，回到初始父物体
        transform.SetParent(startParent);

        // 恢复位置、旋转和缩放
        transform.position = startPos;
        transform.rotation = startRot;
        transform.localScale = startScale; // 确保使用初始缩放
    }
}