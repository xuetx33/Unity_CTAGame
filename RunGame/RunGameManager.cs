using UnityEngine;
using UnityEngine.UI;
using System;

public class RunGameManager : MonoBehaviour
{
    public static RunGameManager Instance; // 单例模式

    [Header("游戏核心设置")]
    public float totalGameTime = 180f; // 总时长（3分钟=180秒）
    public float initialPlayerSpeed = 5f; // 初始速度
    public float maxPlayerSpeed = 15f; // 最大速度
    public float speedIncreaseRate = 0.1f; // 每秒速度增量

    [Header("追赶机制设置")]
    public float initialDistance = 20f; // 初始距离（主角与追赶者）
    public float minDistance = 5f; // 最小安全距离
    public float distanceAfterHit = 5f; // 碰撞后拉近的距离
    public float distanceRecoveryPerSecond = 1f; // 每秒恢复的距离
    public int maxConsecutiveHits = 2; // 连续碰撞上限（超过失败）

    [Header("UI - 状态显示（可选）")]
    public Text timeText; // 剩余时间文本
    public Text speedText; // 当前速度文本

    [Header("UI - 终点距离横条（可选）")]
    public RectTransform distanceBarContainer; // 横条容器（背景）
    public RectTransform playerMarker; // 主角标记
    public RectTransform chaserMarker; // 追赶者标记
    [Range(0.1f, 0.3f)] public float maxDistanceRatio = 0.2f; // 最大距离占横条比例

    [Header("UI - 结果面板（可选）")]
    public GameObject gameOverPanel; // 失败面板
    public GameObject successPanel; // 成功面板

    [Header("主角引用（必须）")]
    [Tooltip("必须赋值主角的移动控制器，否则无法运行")]
    public RunPlayerController playerController;

    // 私有变量
    private float currentGameTime;
    private float currentPlayerSpeed;
    private float currentDistance;
    private int consecutiveHitCount;
    private float lastHitTime;
    private bool isGameOver;
    private float totalBarLength; // 横条总长度（像素）

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 检查核心组件
        if (playerController == null)
            Debug.LogError("【错误】RunGameManager：请为playerController赋值主角的RunPlayerController组件！");
    }

    void Start()
    {
        // 核心组件缺失时停止初始化
        if (playerController == null)
            return;

        // 初始化游戏状态
        currentGameTime = totalGameTime;
        currentPlayerSpeed = initialPlayerSpeed;
        currentDistance = initialDistance;
        consecutiveHitCount = 0;
        lastHitTime = -Mathf.Infinity;
        isGameOver = false;

        // 初始化主角速度
        playerController.runSpeed = currentPlayerSpeed;

        // 初始化横条（若UI已赋值）
        if (distanceBarContainer != null && playerMarker != null && chaserMarker != null)
        {
            totalBarLength = distanceBarContainer.rect.width;
            UpdateMarkerPositions(); // 初始位置
        }

        // 隐藏结果面板（若UI已赋值）
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(false);
    }

    void Update()
    {
        // 游戏结束或核心组件缺失时停止更新
        if (isGameOver || playerController == null)
            return;

        // 更新游戏时间
        UpdateGameTime();

        // 递增主角速度
        UpdatePlayerSpeed();

        // 恢复追赶距离（未碰撞时）
        RecoverDistanceOverTime();

        // 更新UI显示
        UpdateUI();

        // 更新横条标记位置（若UI已赋值）
        if (distanceBarContainer != null && playerMarker != null && chaserMarker != null)
            UpdateMarkerPositions();
    }

    #region 游戏逻辑核心方法
    // 更新剩余时间
    void UpdateGameTime()
    {
        currentGameTime -= Time.deltaTime;
        if (currentGameTime <= 0)
        {
            currentGameTime = 0;
            GameSuccess(); // 时间到，游戏成功
        }
    }

    // 随时间增加主角速度
    void UpdatePlayerSpeed()
    {
        if (currentPlayerSpeed < maxPlayerSpeed)
        {
            currentPlayerSpeed += speedIncreaseRate * Time.deltaTime;
            currentPlayerSpeed = Mathf.Clamp(currentPlayerSpeed, initialPlayerSpeed, maxPlayerSpeed);
            playerController.runSpeed = currentPlayerSpeed;
        }
    }

    // 未碰撞时恢复距离
    void RecoverDistanceOverTime()
    {
        if (Time.time - lastHitTime > 1f && currentDistance < initialDistance)
        {
            currentDistance += distanceRecoveryPerSecond * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, initialDistance);
        }
    }

    // 处理障碍物碰撞事件（由主角控制器调用）
    public void OnObstacleHit()
    {
        if (isGameOver || playerController == null)
            return;

        // 拉近距离
        currentDistance -= distanceAfterHit;
        currentDistance = Mathf.Max(currentDistance, minDistance);

        // 更新连续碰撞计数
        consecutiveHitCount++;
        lastHitTime = Time.time;

        // 检查失败条件
        if (consecutiveHitCount >= maxConsecutiveHits)
            GameOver();
    }

    // 重置连续碰撞计数（距离恢复到安全值时）
    void ResetConsecutiveHits()
    {
        if (currentDistance >= initialDistance * 0.8f)
            consecutiveHitCount = 0;
    }
    #endregion

    #region UI更新方法
    // 更新状态文本UI
    void UpdateUI()
    {
        // 更新剩余时间文本
        if (timeText != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentGameTime);
            timeText.text = $"剩余时间：{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        // 更新速度文本
        if (speedText != null)
            speedText.text = $"速度：{currentPlayerSpeed:F1}";

        // 重置连续碰撞计数
        ResetConsecutiveHits();
    }

    // 更新横条上的标记位置
    // 更新横条上的标记位置（修改后）
    // 更新横条上的标记位置（修改后）
    void UpdateMarkerPositions()
    {
        if (distanceBarContainer == null || playerMarker == null || chaserMarker == null)
            return;

        // 1. 主角位置（随时间从左到右移动）
        float playerProgress = 1 - (currentGameTime / totalGameTime);
        float playerX = playerProgress * totalBarLength;

        // 2. 追赶者与主角的距离比例（0~1，碰撞时变小）
        float distanceRatio = currentDistance / initialDistance;
        float maxDistanceOnBar = totalBarLength * maxDistanceRatio; // 横条上的最大距离

        // 3. 追赶者位置 = 主角位置 - 动态距离（直接跟随主角移动）
        float chaserX = playerX - (maxDistanceOnBar * distanceRatio);

        // 4. 限制在横条内（不超出左边缘，不超过主角）
        chaserX = Mathf.Clamp(chaserX, 0, playerX - 1f);

        // 应用位置
        playerMarker.anchoredPosition = new Vector2(playerX, 0);
        chaserMarker.anchoredPosition = new Vector2(chaserX, 0);
    }
    #endregion

    #region 游戏结果处理
    // 游戏成功（时间到）
    void GameSuccess()
    {
        isGameOver = true;
        if (successPanel != null)
            successPanel.SetActive(true);
        else
            Debug.Log("【游戏结果】成功到达终点！");

        playerController.runSpeed = 0;
        Time.timeScale = 0; // 暂停游戏
    }

    // 游戏失败（被追上）
    void GameOver()
    {
        isGameOver = true;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.Log("【游戏结果】被追上了，失败！");

        playerController.runSpeed = 0;
        Time.timeScale = 0; // 暂停游戏
    }

    // 重新开始游戏（绑定到UI按钮）
    public void RestartGame()
    {
        Time.timeScale = 1; // 恢复时间流速
        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        catch (Exception e)
        {
            Debug.LogError("重启场景失败：" + e.Message);
        }
    }
    #endregion
}