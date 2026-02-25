using MFrameWork;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 榫卯拼接游戏UI控制器（MonoBehaviour部分）
/// 挂载在SMPuzzleGamePanel Prefab根节点上
/// 职责：进度条、计时器、需求显示、木料区、拼接区管理
/// </summary>
public class SMPuzzleGameUIController : MonoBehaviour
{
    [Header("=== UI元素引用 ===")]
    [SerializeField] private Text questionText;           // 需求题目
    [SerializeField] private Slider progressBar;          // 进度条
    [SerializeField] private Text progressText;           // 进度百分比
    [SerializeField] private Text timerText;              // 倒计时文本
    [SerializeField] private Image timerFillImage;        // 倒计时填充图
    [SerializeField] private Text tipText;                // 提示文本

    [Header("=== 木料区域父节点 ===")]
    [SerializeField] private Transform leftPieceParent;   // 榫头木料区（左）
    [SerializeField] private Transform rightPieceParent;  // 卯眼木料区（右）
    [SerializeField] private Transform puzzleZoneParent;  // 拼接工作区（中央）

    [Header("=== 预制体 ===")]
    [SerializeField] private GameObject smPiecePrefab;    // 木料预制体

    private List<SMPieceDragger> activeDraggers = new List<SMPieceDragger>();
    private bool isUIInitialized = false;

    private void OnEnable()
    {
        // 面板激活时
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // 面板关闭时清理资源
        ClearPieces();
    }

    /// <summary>
    /// 游戏UI初始化（由SMPuzzleGameUIPanel调用）
    /// </summary>
    public void InitGameUI()
    {
        // 检查所有必要的组件
        if (ValidateUIComponents())
        {
            SetProgressBar(0);
            UpdateTimer(0, SMPuzzleGameController.Instance.timeLimit);
            questionText.text = "准备开始游戏...";
            tipText.text = "拖动木料到中间拼接区，按 SPACE 旋转 (0/90/180/270°)";
            ClearPieces();
            isUIInitialized = true;
            Debug.Log("? UI初始化完成");
        }
        else
        {
            Debug.LogError("? UI组件不完整，请检查Prefab配置!");
        }
    }

    /// <summary>
    /// 验证所有UI组件是否已赋值
    /// </summary>
    private bool ValidateUIComponents()
    {
        if (questionText == null)
        {
            Debug.LogError("? questionText 未赋值");
            return false;
        }
        if (progressBar == null)
        {
            Debug.LogError("? progressBar 未赋值");
            return false;
        }
        if (timerText == null)
        {
            Debug.LogError("? timerText 未赋值");
            return false;
        }
        if (leftPieceParent == null)
        {
            Debug.LogError("? leftPieceParent 未赋值");
            return false;
        }
        if (rightPieceParent == null)
        {
            Debug.LogError("? rightPieceParent 未赋值");
            return false;
        }
        if (puzzleZoneParent == null)
        {
            Debug.LogError("? puzzleZoneParent 未赋值");
            return false;
        }
        if (smPiecePrefab == null)
        {
            Debug.LogError("? smPiecePrefab 未赋值");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 面板激活时的回调
    /// </summary>
    public void OnPanelActive()
    {
        Debug.Log("? UI面板已显示");
    }

    /// <summary>
    /// 面板关闭时的回调
    /// </summary>
    public void OnPanelDeActive()
    {
        Debug.Log("? UI面板已隐藏");
        ClearPieces();
    }

    /// <summary>
    /// 设置需求题目文本
    /// </summary>
    public void SetQuestionText(SMPieceCategory category)
    {
        string questionMsg = category switch
        {
            SMPieceCategory.Beam => "?? 需求：拼接 【横枋】 + 【立柱】",
            SMPieceCategory.Short => "?? 需求：拼接 【短枋】 + 【短枋】",
            SMPieceCategory.Corner => "?? 需求：拼接 【转角件】 + 【转角件】",
            _ => "未知需求"
        };
        questionText.text = questionMsg;
    }

    /// <summary>
    /// 设置进度条（0-1）
    /// </summary>
    public void SetProgressBar(float progress)
    {
        if (progressBar == null) return;

        progress = Mathf.Clamp01(progress);
        progressBar.value = progress;

        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }

    /// <summary>
    /// 更新计时器显示
    /// </summary>
    public void UpdateTimer(float currentTime, float timeLimit)
    {
        if (timerText == null) return;

        float remainingTime = Mathf.Max(0, timeLimit - currentTime);
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = $"? {minutes:D2}:{seconds:D2}";
    }

    /// <summary>
    /// 更新时间进度条显示（用于可视化）
    /// </summary>
    public void UpdateTimeProgress(float timePercent)
    {
        if (timerFillImage == null) return;

        timerFillImage.fillAmount = Mathf.Clamp01(1 - timePercent);

        // 根据时间比例改变颜色
        if (timePercent > 0.8f)
        {
            timerFillImage.color = Color.red;
        }
        else if (timePercent > 0.5f)
        {
            timerFillImage.color = Color.yellow;
        }
        else
        {
            timerFillImage.color = Color.green;
        }
    }

    /// <summary>
    /// 填充左右木料区UI
    /// </summary>
    public void PopulatePieceUI(List<SMPieceData> tenonPieces, List<SMPieceData> mortisePieces, SMPieceCategory category)
    {
        ClearPieces();
        activeDraggers.Clear();

        Debug.Log($"生成木料：{tenonPieces.Count} 个榫头 + {mortisePieces.Count} 个卯眼");

        // 添加榫头到左侧
        foreach (var piece in tenonPieces)
        {
            var pieceObj = CreatePieceUI(piece, category, leftPieceParent, SMPieceType.Tenon);
            if (pieceObj != null)
            {
                var dragger = pieceObj.GetComponent<SMPieceDragger>();
                if (dragger != null) activeDraggers.Add(dragger);
            }
        }

        // 添加卯眼到右侧
        foreach (var piece in mortisePieces)
        {
            var pieceObj = CreatePieceUI(piece, category, rightPieceParent, SMPieceType.Mortise);
            if (pieceObj != null)
            {
                var dragger = pieceObj.GetComponent<SMPieceDragger>();
                if (dragger != null) activeDraggers.Add(dragger);
            }
        }
    }

    /// <summary>
    /// 创建单个木料UI元素
    /// </summary>
    private GameObject CreatePieceUI(SMPieceData data, SMPieceCategory category, Transform parent, SMPieceType type)
    {
        if (smPiecePrefab == null)
        {
            Debug.LogError("? smPiecePrefab 预制体未赋值!");
            return null;
        }

        if (parent == null)
        {
            Debug.LogError($"? 木料父节点为空！");
            Debug.LogError($"  leftPieceParent: {(leftPieceParent != null ? leftPieceParent.name : "null")}");
            Debug.LogError($"  rightPieceParent: {(rightPieceParent != null ? rightPieceParent.name : "null")}");
            return null;
        }

        // ? 关键改动：不用Instantiate + SetParent
        // ? 改成：直接在parent下创建新GameObject

        GameObject pieceObj = new GameObject($"{type}_{data.category}_{data.id}");
        pieceObj.transform.SetParent(parent, false);

        // 添加RectTransform
        RectTransform pieceRect = pieceObj.AddComponent<RectTransform>();
        pieceRect.anchoredPosition = Vector2.zero;
        pieceRect.sizeDelta = new Vector2(100, 100);
        pieceRect.localScale = Vector3.one;

        Debug.Log($"? 已生成木料: {pieceObj.name}");
        Debug.Log($"  父节点: {pieceObj.transform.parent.name}");

        // 添加Image组件
        Image img = pieceObj.AddComponent<Image>();
        if (data.sprite != null)
        {
            img.sprite = data.sprite;
        }

        // 添加Dragger组件
        SMPieceDragger dragger = pieceObj.AddComponent<SMPieceDragger>();
        dragger.SetPieceData(data, category, type);
        dragger.SetPuzzleZone(puzzleZoneParent);

        return pieceObj;
    }

    /// <summary>
    /// 检查物体是否在UIRoot下
    /// </summary>
    private bool IsInUIRoot(Transform t)
    {
        while (t != null)
        {
            if (t.name == "UIRoot")
                return true;
            t = t.parent;
        }
        return false;
    }

    /// <summary>
    /// 清空所有木料UI
    /// </summary>
    public void ClearPieces()
    {
        if (leftPieceParent != null)
        {
            foreach (Transform child in leftPieceParent)
            {
                Destroy(child.gameObject);
            }
        }

        if (rightPieceParent != null)
        {
            foreach (Transform child in rightPieceParent)
            {
                Destroy(child.gameObject);
            }
        }

        if (puzzleZoneParent != null)
        {
            foreach (Transform child in puzzleZoneParent)
            {
                Destroy(child.gameObject);
            }
        }

        activeDraggers.Clear();
    }

    /// <summary>
    /// 游戏完成或失败
    /// </summary>
    public void OnGameComplete(bool success)
    {
        ClearPieces();

        if (success)
        {
            questionText.text = "? 恭喜通关！";
            questionText.color = Color.green;
            tipText.text = "按 F5 重新开始游戏";
            Debug.Log("? 游戏成功完成!");
        }
        else
        {
            questionText.text = "? 超时失败！";
            questionText.color = Color.red;
            tipText.text = "按 F5 重新开始游戏";
            Debug.Log("? 游戏超时失败!");
        }

        // 3秒后自动关闭面板
        Invoke(nameof(AutoClosePanel), 3f);
    }

    private void AutoClosePanel()
    {
        if (MUIManager.Instance != null)
        {
            MUIManager.Instance.DeActiveUI("SMPuzzleGamePanel");
        }
    }
}