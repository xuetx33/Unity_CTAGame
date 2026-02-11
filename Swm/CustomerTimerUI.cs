using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerTimerUI : MonoBehaviour
{
    [Header("UI引用")]
    public RectTransform timerFill; // 进度条填充RectTransform
    public Image timerFillImage; // 进度条填充图像
    public TextMeshProUGUI timerText; // 时间文本
    public Mask timerMask; // Mask组件（可选）

    [Header("进度条设置")]
    public float maxWidth = 400f; // 进度条最大宽度

    [Header("颜色设置")]
    public Color fullTimeColor = Color.green;
    public Color midTimeColor = Color.yellow;
    public Color lowTimeColor = Color.red;

    [Header("警告效果")]
    public bool enableWarningEffect = true;
    public float warningThreshold = 0.3f;
    public float warningFlashSpeed = 2f;

    private Customer customer;
    private bool isWarning = false;
    private float originalWidth;

    void Start()
    {
        // 保存原始宽度
        if (timerFill != null)
        {
            originalWidth = timerFill.sizeDelta.x;
        }

        // 查找顾客组件
        customer = FindObjectOfType<Customer>();

        // 如果手动没有分配UI组件，尝试自动查找
        if (timerFill == null)
        {
            Transform fillTransform = transform.Find("TimerPanel/TimerMask/TimerFill");
            if (fillTransform != null)
                timerFill = fillTransform.GetComponent<RectTransform>();
        }

        if (timerFillImage == null && timerFill != null)
            timerFillImage = timerFill.GetComponent<Image>();

        if (timerText == null)
        {
            Transform textTransform = transform.Find("TimerPanel/TimerText");
            if (textTransform != null)
                timerText = textTransform.GetComponent<TextMeshProUGUI>();
        }

        // 订阅顾客的时间变化事件
        if (customer != null)
        {
            customer.OnTimeChanged.AddListener(UpdateTimerUI);
            customer.OnTimeOut.AddListener(OnTimerExpired);

            // 初始化UI
            UpdateTimerUI(customer.GetCurrentTime());
        }
        else
        {
            Debug.LogError("未找到Customer组件！");
        }
    }

    void Update()
    {
        // 如果启用了警告效果且处于警告状态，让进度条闪烁
        if (enableWarningEffect && isWarning && timerFillImage != null)
        {
            float alpha = Mathf.PingPong(Time.time * warningFlashSpeed, 1f);
            Color color = timerFillImage.color;
            color.a = alpha;
            timerFillImage.color = color;
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (customer != null)
        {
            customer.OnTimeChanged.RemoveListener(UpdateTimerUI);
            customer.OnTimeOut.RemoveListener(OnTimerExpired);
        }
    }

    // 更新计时器UI
    void UpdateTimerUI(float currentTime)
    {
        if (timerFill == null || timerFillImage == null || timerText == null)
            return;

        // 获取时间进度（0到1的值）
        float progress = customer.GetTimeProgress();

        // 更新进度条宽度
        UpdateProgressBar(progress);

        // 更新文本显示
        timerText.text = $"剩余时间: {Mathf.CeilToInt(currentTime)}秒";

        // 根据时间比例更新颜色
        UpdateTimerColor(progress);

        // 检查是否需要显示警告
        isWarning = progress <= warningThreshold && currentTime > 0;

        // 如果不在警告状态，确保不透明度为1
        if (!isWarning)
        {
            Color color = timerFillImage.color;
            color.a = 1f;
            timerFillImage.color = color;
        }
    }

    // 更新进度条宽度
    void UpdateProgressBar(float progress)
    {
        if (timerFill == null) return;

        // 计算新宽度
        float newWidth = maxWidth * progress;

        // 设置进度条宽度
        timerFill.sizeDelta = new Vector2(newWidth, timerFill.sizeDelta.y);
    }

    // 根据时间比例更新进度条颜色
    void UpdateTimerColor(float progress)
    {
        if (timerFillImage == null) return;

        if (progress > 0.6f)
        {
            // 时间充足 - 绿色
            timerFillImage.color = fullTimeColor;
        }
        else if (progress > 0.3f)
        {
            // 时间中等 - 黄色
            timerFillImage.color = midTimeColor;
        }
        else
        {
            // 时间不足 - 红色
            timerFillImage.color = lowTimeColor;
        }
    }

    // 计时器到期处理
    void OnTimerExpired()
    {
        if (timerText != null)
        {
            timerText.text = "时间到!";
        }

        if (timerFillImage != null)
        {
            timerFillImage.color = Color.gray;
        }

        // 将进度条宽度设为0
        if (timerFill != null)
        {
            timerFill.sizeDelta = new Vector2(0, timerFill.sizeDelta.y);
        }
    }

    // 公共方法：手动设置进度（0到1）
    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        UpdateProgressBar(progress);
    }

    // 公共方法：重置UI状态
    public void ResetUI()
    {
        if (timerFill != null)
        {
            timerFill.sizeDelta = new Vector2(maxWidth, timerFill.sizeDelta.y);
        }

        if (timerFillImage != null)
        {
            timerFillImage.color = fullTimeColor;
            Color color = timerFillImage.color;
            color.a = 1f;
            timerFillImage.color = color;
        }

        if (timerText != null)
        {
            if (customer != null)
            {
                timerText.text = $"剩余时间: {Mathf.CeilToInt(customer.GetCurrentTime())}秒";
            }
            else
            {
                timerText.text = "剩余时间: 60秒";
            }
        }

        isWarning = false;
    }
}