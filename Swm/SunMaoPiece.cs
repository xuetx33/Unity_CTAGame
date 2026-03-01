using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 榫卯拼接游戏 - 单个榫卯构件
/// 支持鼠标拖拽，当放置到对应插槽附近时自动吸附。
/// </summary>
public class SunMaoPiece : MonoBehaviour
{
    [Header("构件属性")]
    [Tooltip("构件编号，需与对应 SunMaoSlot 的 slotId 一致")]
    public int pieceId;

    [Tooltip("构件名称（如"直榫"、"燕尾榫"等）")]
    public string pieceName = "榫卯构件";

    [Header("吸附设置")]
    [Tooltip("与插槽的最大吸附距离（世界单位）")]
    public float snapDistance = 1.5f;

    [Header("高亮设置")]
    [Tooltip("悬停时缩放倍率")]
    public float hoverScale = 1.07f;

    [Header("事件")]
    public UnityEvent OnPlaced;   // 成功放入插槽时触发
    public UnityEvent OnReset;    // 被重置回初始位置时触发

    // ---- 内部状态 ----
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;
    private Transform startParent;

    private bool isPlaced = false;   // 是否已正确放置
    private bool isDragging = false; // 是否正在拖拽

    private SunMaoPuzzleManager puzzleManager;

    // ---- Unity 生命周期 ----
    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale    = transform.localScale;
        startParent   = transform.parent;

        puzzleManager = FindObjectOfType<SunMaoPuzzleManager>();
    }

    private void OnMouseDown()
    {
        if (!isPlaced)
            isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isPlaced && isDragging)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = transform.position.z;
            transform.position = worldPos;
        }
    }

    private void OnMouseUp()
    {
        if (!isPlaced && isDragging)
        {
            isDragging = false;
            TrySnap();
        }
    }

    private void OnMouseEnter()
    {
        if (!isPlaced)
            transform.localScale = startScale * hoverScale;
    }

    private void OnMouseExit()
    {
        if (!isPlaced && !isDragging)
            transform.localScale = startScale;
    }

    // ---- 核心逻辑 ----

    /// <summary>
    /// 放开鼠标时，查找距离最近且匹配的插槽，尝试吸附。
    /// </summary>
    private void TrySnap()
    {
        SunMaoSlot[] slots = FindObjectsOfType<SunMaoSlot>();
        SunMaoSlot bestSlot = null;
        float bestDist = float.MaxValue;

        foreach (SunMaoSlot slot in slots)
        {
            if (slot.slotId != pieceId) continue;   // 编号不匹配
            if (slot.IsOccupied) continue;           // 已被占用

            float dist = Vector2.Distance(transform.position, slot.transform.position);
            if (dist < snapDistance && dist < bestDist)
            {
                bestSlot = slot;
                bestDist = dist;
            }
        }

        if (bestSlot != null)
        {
            PlaceInSlot(bestSlot);
        }
        else
        {
            ResetPiece();
        }
    }

    /// <summary>
    /// 将构件放入插槽。
    /// </summary>
    private void PlaceInSlot(SunMaoSlot slot)
    {
        isPlaced = true;

        transform.SetParent(slot.transform);
        transform.position = slot.transform.position;
        transform.rotation = Quaternion.identity;
        transform.localScale = startScale;

        slot.SetOccupied(this);
        OnPlaced?.Invoke();

        if (puzzleManager != null)
            puzzleManager.OnPiecePlaced();

        Debug.Log($"[榫卯拼接] 构件 "{pieceName}"（ID={pieceId}）已正确放入插槽");
    }

    // ---- 公共接口 ----

    /// <summary>
    /// 将构件重置到初始状态（从插槽中移出）。
    /// </summary>
    public void ResetPiece()
    {
        isPlaced   = false;
        isDragging = false;

        transform.SetParent(startParent);
        transform.position   = startPosition;
        transform.rotation   = startRotation;
        transform.localScale = startScale;

        OnReset?.Invoke();
    }

    /// <summary>
    /// 返回构件是否已正确放置。
    /// </summary>
    public bool IsPlaced => isPlaced;
}
