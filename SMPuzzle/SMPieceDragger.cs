using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个木料拖拽、旋转、吸附逻辑
/// 挂载在动态生成的木料GameObject上
/// 职责：鼠标长按拖拽、SPACE旋转、吸附判定、自动合并
/// </summary>
public class SMPieceDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("=== 木料数据 ===")]
    private SMPieceData pieceData;
    private SMPieceCategory requiredCategory;
    private SMPieceType pieceType;

    [Header("=== UI引用 ===")]
    private RectTransform rectTransform;
    private Image imageComponent;

    [Header("=== 拖拽参数 ===")]
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private Vector2 dragStartMousePos;

    [Header("=== 旋转 ===")]
    private float currentRotation = 0f; // 0, 90, 180, 270

    [Header("=== 拼接区 ===")]
    private Transform puzzleZone;
    private RectTransform puzzleZoneRect;

    [Header("=== 吸附参数 ===")]
    [SerializeField] private float attachmentSnapDistance = 80f;  // 吸附距离阈值
    [SerializeField] private float rotationSnapAngle = 20f;       // 旋转对齐容差

    private SMPieceDragger pairedPiece = null;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        imageComponent = GetComponent<Image>();

        // 添加CanvasGroup用于透明度控制
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        dragStartPos = rectTransform.anchoredPosition;

        // 添加GraphicRaycaster用于检测
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void Update()
    {
        if (!isDragging) return;

        // 按SPACE旋转木料
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RotatePiece();
        }
    }

    /// <summary>
    /// 设置木料数据
    /// </summary>
    public void SetPieceData(SMPieceData data, SMPieceCategory category, SMPieceType type)
    {
        pieceData = data;
        requiredCategory = category;
        pieceType = type;

        if (imageComponent != null && data.sprite != null)
        {
            imageComponent.sprite = data.sprite;
        }

        Debug.Log($"? 木料已配置: {type} - {category}");
    }

    /// <summary>
    /// 设置拼接区
    /// </summary>
    public void SetPuzzleZone(Transform zone)
    {
        puzzleZone = zone;
        if (zone != null)
        {
            puzzleZoneRect = zone.GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// 鼠标按下 - 开始拖拽
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPos = rectTransform.anchoredPosition;
        dragStartMousePos = eventData.position;

        // 移到最前面
        transform.SetAsLastSibling();

        // 降低透明度表示正在拖拽
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.8f;
        }

        Debug.Log($"开始拖拽: {gameObject.name}");
    }

    /// <summary>
    /// 拖拽中 - 更新位置
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && rectTransform != null)
        {
            rectTransform.anchoredPosition += eventData.delta;
        }
    }

    /// <summary>
    /// 鼠标抬起 - 结束拖拽、判定吸附
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // 恢复透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        Debug.Log($"结束拖拽: {gameObject.name}");

        // 判定是否在拼接区且满足配对条件
        if (IsOverPuzzleZone() && IsRotationValid())
        {
            Debug.Log($"在拼接区内 + 旋转有效，尝试配对...");

            // 尝试与拼接区内已存在的木料配对
            if (TryAttachToPairedPiece())
            {
                Debug.Log($"? 吸附成功!");
                OnAttachmentSuccess();
                return;
            }
        }

        Debug.Log($"吸附失败，回到原点");
        // 吸附失败 -> 回到原点
        ResetPosition();
    }

    /// <summary>
    /// 旋转木料（仅0/90/180/270°）
    /// </summary>
    private void RotatePiece()
    {
        currentRotation = (currentRotation + 90) % 360;
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);

        Debug.Log($"?? {gameObject.name} 旋转至 {currentRotation}°");
    }

    /// <summary>
    /// 判定是否在拼接区内
    /// </summary>
    private bool IsOverPuzzleZone()
    {
        if (puzzleZoneRect == null)
        {
            Debug.LogWarning("puzzleZoneRect 为空!");
            return false;
        }

        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(
            puzzleZoneRect,
            rectTransform.position,
            null
        );

        return isInside;
    }

    /// <summary>
    /// 判定旋转是否有效（必须是0/90/180/270的整数倍）
    /// </summary>
    private bool IsRotationValid()
    {
        float angle = Mathf.Round(currentRotation) % 360;
        return (angle % 90) < Mathf.Epsilon;
    }

    /// <summary>
    /// 尝试与拼接区内已有木料配对
    /// </summary>
    private bool TryAttachToPairedPiece()
    {
        if (puzzleZone == null)
        {
            Debug.LogWarning("puzzleZone 为空!");
            return false;
        }

        // 遍历拼接区内所有拖拽器，找寻能配对的
        foreach (Transform child in puzzleZone)
        {
            SMPieceDragger otherDragger = child.GetComponent<SMPieceDragger>();
            if (otherDragger == null || otherDragger == this)
                continue;

            // 检查是否同属所需类别且类型互补（榫头+卯眼）
            if (otherDragger.requiredCategory == this.requiredCategory &&
                otherDragger.pieceType != this.pieceType)
            {
                // 检查距离和旋转对齐
                if (IsPieceAligned(otherDragger))
                {
                    pairedPiece = otherDragger;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 判定两块木料是否对齐（旋转和位置）
    /// </summary>
    private bool IsPieceAligned(SMPieceDragger other)
    {
        // 旋转对齐检查
        float rotDiff = Mathf.Abs(this.currentRotation - other.currentRotation);
        rotDiff = Mathf.Min(rotDiff, 360 - rotDiff);

        // 允许容差范围内的旋转偏差
        if (rotDiff > rotationSnapAngle && rotDiff < (180 - rotationSnapAngle))
        {
            Debug.Log($"旋转不对齐: 差异={rotDiff}°");
            return false;
        }

        // 位置对齐检查（距离足够近）
        float distance = Vector2.Distance(
            rectTransform.anchoredPosition,
            other.rectTransform.anchoredPosition
        );

        bool isAligned = distance < attachmentSnapDistance;

        Debug.Log($"对齐检查: 旋转差异={rotDiff}°, 距离={distance}px, 对齐={isAligned}");

        return isAligned;
    }

    /// <summary>
    /// 吸附成功
    /// </summary>
    private void OnAttachmentSuccess()
    {
        Debug.Log($"? 吸附成功！{pieceType} 与 {pairedPiece.pieceType} 配对完成");

        // 播放吸附音效（可选）
        if (GameManager.Instance != null && GameManager.Instance.normalClip != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.normalClip);
        }

        // 销毁当前木料
        Destroy(gameObject);

        // 销毁配对木料
        if (pairedPiece != null)
        {
            Destroy(pairedPiece.gameObject);
        }

        // 通知控制器题目完成
        if (SMPuzzleGameController.Instance != null)
        {
            SMPuzzleGameController.Instance.OnQuestionCompleted();
        }
    }

    /// <summary>
    /// 重置到原始位置
    /// </summary>
    private void ResetPosition()
    {
        rectTransform.anchoredPosition = dragStartPos;
        currentRotation = 0;
        rectTransform.localRotation = Quaternion.identity;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
}