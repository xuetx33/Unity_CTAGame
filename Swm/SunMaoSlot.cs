using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 榫卯拼接游戏 - 插槽（卯口）
/// 标记一个可以接收特定 SunMaoPiece 的位置。
/// </summary>
public class SunMaoSlot : MonoBehaviour
{
    [Header("插槽属性")]
    [Tooltip("插槽编号，需与对应 SunMaoPiece 的 pieceId 一致")]
    public int slotId;

    [Tooltip("插槽名称（方便在 Inspector 中识别）")]
    public string slotName = "榫卯插槽";

    [Header("视觉反馈")]
    [Tooltip("空置时渲染器颜色（可选）")]
    public Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    [Tooltip("已占用时渲染器颜色（可选）")]
    public Color occupiedColor = new Color(0.3f, 0.8f, 0.3f, 1f);

    [Header("事件")]
    public UnityEvent OnOccupied;  // 构件放入时触发
    public UnityEvent OnVacated;   // 构件移出时触发

    // ---- 内部状态 ----
    private bool isOccupied = false;
    private SunMaoPiece occupyingPiece = null;
    private SpriteRenderer spriteRenderer;

    // ---- Unity 生命周期 ----
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    // ---- 公共接口 ----

    /// <summary>
    /// 当前插槽是否已被占用。
    /// </summary>
    public bool IsOccupied => isOccupied;

    /// <summary>
    /// 占用插槽（由 SunMaoPiece 调用）。
    /// </summary>
    public void SetOccupied(SunMaoPiece piece)
    {
        isOccupied     = true;
        occupyingPiece = piece;
        UpdateVisual();
        OnOccupied?.Invoke();
        Debug.Log($"[榫卯拼接] 插槽 "{slotName}"（ID={slotId}）已被占用");
    }

    /// <summary>
    /// 清空插槽（构件被重置时调用）。
    /// </summary>
    public void Vacate()
    {
        isOccupied     = false;
        occupyingPiece = null;
        UpdateVisual();
        OnVacated?.Invoke();
        Debug.Log($"[榫卯拼接] 插槽 "{slotName}"（ID={slotId}）已清空");
    }

    /// <summary>
    /// 返回当前占用该插槽的构件（若为空则返回 null）。
    /// </summary>
    public SunMaoPiece OccupyingPiece => occupyingPiece;

    // ---- 辅助方法 ----
    private void UpdateVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = isOccupied ? occupiedColor : emptyColor;
    }

    // 在编辑器中显示插槽范围（方便调试）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOccupied ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
