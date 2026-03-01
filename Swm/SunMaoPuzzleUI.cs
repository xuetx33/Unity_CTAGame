using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 榫卯拼接游戏 - UI 控制器
/// 负责显示计时器、进度、完成/失败提示，并提供重置按钮的绑定点。
/// 请在 Unity Inspector 中将对应 UI 组件拖入各字段。
/// </summary>
public class SunMaoPuzzleUI : MonoBehaviour
{
    [Header("计时器 UI")]
    [Tooltip("倒计时进度条（Image，FillAmount 模式）")]
    public Image timerFillImage;

    [Tooltip("倒计时文本")]
    public TextMeshProUGUI timerText;

    [Header("进度 UI")]
    [Tooltip("拼接进度文本，例如"已拼接：3 / 6"")]
    public TextMeshProUGUI progressText;

    [Tooltip("拼接进度条（Image，FillAmount 模式）")]
    public Image progressFillImage;

    [Header("结果面板")]
    [Tooltip("拼接完成时显示的面板")]
    public GameObject completionPanel;

    [Tooltip("完成面板上的分数文本")]
    public TextMeshProUGUI scoreText;

    [Tooltip("失败面板")]
    public GameObject failedPanel;

    [Header("颜色设置")]
    public Color timerFullColor  = Color.green;
    public Color timerMidColor   = Color.yellow;
    public Color timerLowColor   = Color.red;

    [Header("预警阈值")]
    [Tooltip("剩余时间比例低于此值时进度条变红并闪烁")]
    public float warningThreshold = 0.3f;

    [Tooltip("剩余时间比例高于此值时使用满格颜色，低于此值且高于 warningThreshold 时使用中间颜色")]
    public float midThreshold = 0.6f;

    [Tooltip("闪烁速度")]
    public float flashSpeed = 2f;

    // ---- 内部引用 ----
    private SunMaoPuzzleManager puzzleManager;
    private bool isWarning = false;

    // ---- Unity 生命周期 ----
    private void Start()
    {
        puzzleManager = FindObjectOfType<SunMaoPuzzleManager>();

        if (puzzleManager == null)
        {
            Debug.LogError("[榫卯拼接UI] 未找到 SunMaoPuzzleManager！");
            return;
        }

        // 注册完成/失败/重置事件
        puzzleManager.OnPuzzleComplete.AddListener(ShowCompletionPanel);
        puzzleManager.OnPuzzleFailed.AddListener(ShowFailedPanel);
        puzzleManager.OnPuzzleReset.AddListener(HideResultPanels);

        // 初始化面板状态
        HideResultPanels();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleComplete.RemoveListener(ShowCompletionPanel);
            puzzleManager.OnPuzzleFailed.RemoveListener(ShowFailedPanel);
            puzzleManager.OnPuzzleReset.RemoveListener(HideResultPanels);
        }
    }

    private void Update()
    {
        if (puzzleManager == null || puzzleManager.IsPuzzleComplete) return;

        RefreshUI();

        // 闪烁效果
        if (isWarning && timerFillImage != null)
        {
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
            Color c = timerFillImage.color;
            c.a = alpha;
            timerFillImage.color = c;
        }
    }

    // ---- 刷新 UI ----

    private void RefreshUI()
    {
        RefreshTimer();
        RefreshProgress();
    }

    private void RefreshTimer()
    {
        if (puzzleManager == null) return;

        bool hasCountdown = puzzleManager.enableTimer && puzzleManager.countdownTime > 0f;

        if (timerText != null)
        {
            if (hasCountdown)
                timerText.text = $"剩余时间：{Mathf.CeilToInt(puzzleManager.RemainingTime)} 秒";
            else
                timerText.text = $"用时：{puzzleManager.ElapsedTime:F1} 秒";
        }

        if (timerFillImage != null && hasCountdown)
        {
            float progress = puzzleManager.countdownTime > 0f
                ? puzzleManager.RemainingTime / puzzleManager.countdownTime
                : 1f;

            timerFillImage.fillAmount = progress;
            isWarning = progress <= warningThreshold;

            if (!isWarning)
            {
                Color newColor = progress > midThreshold ? timerFullColor
                               : progress > warningThreshold ? timerMidColor
                               : timerLowColor;
                newColor.a = 1f;
                timerFillImage.color = newColor;
            }
        }
    }

    private void RefreshProgress()
    {
        if (puzzleManager == null) return;

        if (progressText != null)
            progressText.text = $"已拼接：{puzzleManager.PlacedCount} / {puzzleManager.totalPieces}";

        if (progressFillImage != null)
            progressFillImage.fillAmount = puzzleManager.Progress;
    }

    // ---- 结果面板 ----

    private void ShowCompletionPanel()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);

        if (scoreText != null)
        {
            scoreText.text = $"得分：{puzzleManager.CalculateScore()}";
        }

        Debug.Log("[榫卯拼接UI] 显示完成面板");
    }

    private void ShowFailedPanel()
    {
        if (failedPanel != null)
            failedPanel.SetActive(true);

        Debug.Log("[榫卯拼接UI] 显示失败面板");
    }

    private void HideResultPanels()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (failedPanel != null)
            failedPanel.SetActive(false);
    }

    // ---- 按钮回调（在 Inspector 中绑定到 Button.OnClick） ----

    /// <summary>重置按钮点击回调。</summary>
    public void OnResetButtonClicked()
    {
        if (puzzleManager != null)
            puzzleManager.ResetPuzzle();
    }
}
