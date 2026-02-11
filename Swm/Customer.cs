using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // 用于事件系统

public class Customer : MonoBehaviour
{
    [Header("顾客需求")]
    public int[] requiredItems = new int[8]; // 8格需求数组，对应8种物品

    [Header("倒计时设置")]
    public float initialTime = 60f; // 初始时间（秒），可在Unity中设置
    public float minTime = 15f; // 最小时间限制（秒）
    public float timeDecreasePerOrder = 3f; // 每次提交后减少的时间（秒）
    public bool useConstantTime = false; // 是否使用恒定时间（不随时间递减）

    [Header("事件")]
    public UnityEvent OnTimeOut; // 时间耗尽事件
    public UnityEvent<float> OnTimeChanged; // 时间变化事件（传递当前时间）

    private float currentTime; // 当前剩余时间
    private bool isTimerRunning = false; // 计时器是否在运行
    private int successfulOrders = 0; // 成功订单数量

    void Start()
    {
        StartCustomerCycle();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    // 开始顾客周期（生成需求并开始计时）
    void StartCustomerCycle()
    {
        GenerateRequirements();
        ShowRequirements();
        StartTimer();
    }

    // 开始计时器
    void StartTimer()
    {
        // 计算当前应该使用的时间
        if (useConstantTime)
        {
            currentTime = initialTime;
        }
        else
        {
            // 根据成功订单数量减少时间，但不小于最小值
            float calculatedTime = initialTime - (successfulOrders * timeDecreasePerOrder);
            currentTime = Mathf.Max(calculatedTime, minTime);
        }

        isTimerRunning = true;
        OnTimeChanged?.Invoke(currentTime); // 触发时间变化事件
        Debug.Log($"开始倒计时: {currentTime}秒");
    }

    // 更新计时器
    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        OnTimeChanged?.Invoke(currentTime); // 触发时间变化事件

        // 检查时间是否耗尽
        if (currentTime <= 0)
        {
            currentTime = 0;
            TimeOut();
        }
    }

    // 时间耗尽处理
    void TimeOut()
    {
        isTimerRunning = false;
        Debug.Log("时间耗尽！顾客不满意地离开了！");
        OnTimeOut?.Invoke(); // 触发时间耗尽事件

        // 这里可以添加扣分、生命值减少等逻辑

        // 生成新顾客需求
        StartCustomerCycle();
    }

    // 接收订单
    public void ReceiveOrder(List<int> submittedItems)
    {
        if (!isTimerRunning)
        {
            Debug.Log("计时器未运行，无法接收订单");
            return;
        }

        Debug.Log($"顾客收到了订单，包含物品: {GetItemListString(submittedItems)}");

        bool isCorrect = CheckOrder(submittedItems);

        if (isCorrect)
        {
            Debug.Log("✓ 订单正确！顾客很满意！");
            successfulOrders++;
            // 这里可以添加得分、金币等逻辑
        }
        else
        {
            Debug.Log("✗ 订单错误！顾客不满意！");
            // 这里可以添加扣分等逻辑
        }

        // 停止当前计时器，生成新需求并开始新计时器
        isTimerRunning = false;
        GenerateNewRequirements();
    }

    // 检查订单是否正确
    bool CheckOrder(List<int> submittedItems)
    {
        // 创建需求物品列表
        List<int> requiredKinds = new List<int>();
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == 1)
            {
                requiredKinds.Add(i + 1); // i=0对应物品1，i=1对应物品2，以此类推
            }
        }

        // 检查提交的物品是否完全匹配需求
        if (submittedItems.Count != requiredKinds.Count)
            return false;

        // 排序后比较
        List<int> sortedSubmitted = new List<int>(submittedItems);
        List<int> sortedRequired = new List<int>(requiredKinds);

        sortedSubmitted.Sort();
        sortedRequired.Sort();

        for (int i = 0; i < sortedSubmitted.Count; i++)
        {
            if (sortedSubmitted[i] != sortedRequired[i])
                return false;
        }

        return true;
    }

    // 生成新需求
    void GenerateNewRequirements()
    {
        GenerateRequirements();
        ShowRequirements();
        StartTimer(); // 开始新的计时器
    }

    // 生成需求
    void GenerateRequirements()
    {
        for (int i = 0; i < requiredItems.Length; i++)
        {
            // 70%概率需要该物品
            requiredItems[i] = Random.Range(0f, 1f) < 0.7f ? 1 : 0;
        }

        // 确保至少需要一个物品
        if (CountRequiredItems() == 0)
        {
            int randomIndex = Random.Range(0, requiredItems.Length);
            requiredItems[randomIndex] = 1;
        }

        Debug.Log($"顾客需求生成: [{requiredItems[0]}, {requiredItems[1]}, {requiredItems[2]}, {requiredItems[3]}, {requiredItems[4]}, {requiredItems[5]}, {requiredItems[6]}, {requiredItems[7]}]");
    }

    // 显示需求
    void ShowRequirements()
    {
        string requirementText = "新需求: ";
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == 1)
            {
                requirementText += GetItemName(i + 1) + " ";
            }
        }
        Debug.Log(requirementText);
    }

    // 计算需求物品数量
    int CountRequiredItems()
    {
        int count = 0;
        foreach (int req in requiredItems)
        {
            if (req == 1) count++;
        }
        return count;
    }

    // 获取物品列表字符串
    string GetItemListString(List<int> items)
    {
        string result = "";
        foreach (int item in items)
        {
            result += GetItemName(item) + " ";
        }
        return result.Trim();
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
            default: return "未知";
        }
    }

    // 公共方法：获取当前剩余时间
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // 公共方法：获取当前进度（0到1的值）
    public float GetTimeProgress()
    {
        float maxTime = useConstantTime ? initialTime : Mathf.Max(initialTime - (successfulOrders * timeDecreasePerOrder), minTime);
        return currentTime / maxTime;
    }

    // 公共方法：重置顾客状态（用于重新开始游戏等）
    public void ResetCustomer()
    {
        successfulOrders = 0;
        isTimerRunning = false;
        StartCustomerCycle();
    }
}