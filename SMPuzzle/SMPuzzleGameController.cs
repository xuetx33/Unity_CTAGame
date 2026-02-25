using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MFrameWork;

/// <summary>
/// 榫卯拼接游戏核心控制器
/// 挂载在场景中的某个GameObject上（如GameManager等）
/// 职责：难度管理、题目切换、计时、进度、与MUIManager集成
/// </summary>
public class SMPuzzleGameController : MonoBehaviour
{
    public static SMPuzzleGameController Instance;

    [Header("=== 游戏核心参数 ===")]
    [Tooltip("难度: 0=简单(3题120秒), 1=中等(6题180秒), 2=困难(12题240秒)")]
    public int difficulty = 1;
    public int totalQuestions = 6;
    public float timeLimit = 180f;

    [Header("=== 木料数据 ===")]
    public List<SMPieceData> allTenonPieces = new List<SMPieceData>();      // 所有榫头
    public List<SMPieceData> allMortisePieces = new List<SMPieceData>();    // 所有卯眼

    [Header("=== UI面板引用 ===")]
    [Tooltip("不需要手动赋值，游戏会自动寻找")]
    public SMPuzzleGameUIController uiController;

    [Header("=== 游戏状态 ===")]
    private float gameTimer = 0;
    private int currentQuestionIndex = 0;
    private bool gameActive = false;
    private bool canvasActive = false;
    private SMPieceCategory currentRequiredCategory;
    private int questionsCompleted = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("? SMPuzzleGameController 单例已创建");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitDifficulty();
    }
    private void Start()
    {
        // 在MUIManager初始化后注册UI面板
        if (MUIManager.Instance != null)
        {
            // ? 只在这里注册一次
            if (MUIManager.Instance.GetUI("SMPuzzleGamePanel") == null)
            {
                var uiPanel = new SMPuzzleGameUIPanel();
                MUIManager.Instance.RegisterUI("SMPuzzleGamePanel", uiPanel);
                Debug.Log("? SMPuzzleGamePanel 已注册到MUIManager");
            }
        }
        else
        {
            Debug.LogError("? MUIManager 未初始化!");
        }

        // 验证木料数据
        ValidatePieceData();
    }

    private void Update()
    {
        // F5打开游戏Canvas
        if (Input.GetKeyDown(KeyCode.F5))
        {
            OpenGameCanvas();
        }

        // 游戏进行中更新计时器
        if (!gameActive || uiController == null) return;

        gameTimer += Time.deltaTime;
        float timePercent = gameTimer / timeLimit;

        uiController.UpdateTimer(gameTimer, timeLimit);
        uiController.UpdateTimeProgress(timePercent);

        // 时间超出
        if (gameTimer >= timeLimit)
        {
            OnGameTimeout();
        }
    }

    /// <summary>
    /// 验证木料数据是否完整
    /// </summary>
    private void ValidatePieceData()
    {
        if (allTenonPieces.Count == 0)
        {
            Debug.LogWarning("? 榫头木料列表为空!");
        }
        if (allMortisePieces.Count == 0)
        {
            Debug.LogWarning("? 卯眼木料列表为空!");
        }

        Debug.Log($"? 木料数据: {allTenonPieces.Count} 个榫头 + {allMortisePieces.Count} 个卯眼");
    }

    /// <summary>
    /// 初始化难度相关参数
    /// </summary>
    private void InitDifficulty()
    {
        switch (difficulty)
        {
            case 0: // 简单
                totalQuestions = 3;
                timeLimit = 120f;
                Debug.Log("? 难度: 简单 (3题, 120秒)");
                break;
            case 1: // 中等
                totalQuestions = 6;
                timeLimit = 180f;
                Debug.Log("? 难度: 中等 (6题, 180秒)");
                break;
            case 2: // 困难
                totalQuestions = 12;
                timeLimit = 240f;
                Debug.Log("? 难度: 困难 (12题, 240秒)");
                break;
        }
    }

    /// <summary>
    /// 打开游戏Canvas（F5触发或程序调用）
    /// </summary>
    public void OpenGameCanvas()
    {
        if (!canvasActive && MUIManager.Instance != null)
        {
            Debug.Log("? 打开游戏面板");
            MUIManager.Instance.ActiveUI("SMPuzzleGamePanel");
            canvasActive = true;

            // ? 重要！延迟一帧让Prefab加载完成，然后寻找UIController
            StartCoroutine(FindUIControllerAndStartGame());
        }
    }

    /// <summary>
    /// 延迟一帧后寻找UIController并开始游戏
    /// </summary>
    private System.Collections.IEnumerator FindUIControllerAndStartGame()
    {
        yield return null; // 等一帧，让Prefab加载完成

        // 如果还没有引用，则从场景中寻找
        if (uiController == null)
        {
            // 使用新的API：FindFirstObjectByType (Unity 2023.1+)
            // 或者用 FindObjectOfType<T>(FindObjectsInactive.Include) 支持旧版本

            // 方案1：新版本Unity (推荐)
#if UNITY_2023_1_OR_NEWER
            uiController = UnityEngine.Object.FindFirstObjectByType<SMPuzzleGameUIController>();
#else
        // 方案2：旧版本Unity
        uiController = FindObjectOfType<SMPuzzleGameUIController>(true);
#endif

            if (uiController != null)
            {
                Debug.Log("? 自动找到 SMPuzzleGameUIController");
            }
            else
            {
                Debug.LogError("? 无法找到 SMPuzzleGameUIController 组件! 请检查Prefab是否配置正确");
                yield break;
            }
        }

        StartGame();
    }

    /// <summary>
    /// 关闭游戏Canvas
    /// </summary>
    public void CloseGameCanvas()
    {
        if (canvasActive && MUIManager.Instance != null)
        {
            Debug.Log("? 关闭游戏面板");
            MUIManager.Instance.DeActiveUI("SMPuzzleGamePanel");
            canvasActive = false;
            gameActive = false;
        }
    }

    /// <summary>
    /// 游戏开始初始化
    /// </summary>
    private void StartGame()
    {
        gameTimer = 0;
        currentQuestionIndex = 0;
        questionsCompleted = 0;
        gameActive = true;

        Debug.Log("?? 游戏开始!");

        if (uiController != null)
        {
            uiController.InitGameUI();
            uiController.SetProgressBar(0);
            uiController.UpdateTimer(0, timeLimit);
        }
        else
        {
            Debug.LogError("? StartGame: uiController 仍为空!");
            return;
        }

        GenerateNextQuestion();
    }

    /// <summary>
    /// 生成下一题（随机需求类型）
    /// </summary>
    private void GenerateNextQuestion()
    {
        if (questionsCompleted >= totalQuestions)
        {
            OnGameComplete();
            return;
        }

        currentQuestionIndex++;

        // 随机选择需求类别
        SMPieceCategory[] categories = {
            SMPieceCategory.Beam,
            SMPieceCategory.Short,
            SMPieceCategory.Corner
        };

        currentRequiredCategory = categories[Random.Range(0, categories.Length)];

        Debug.Log($"?? 生成第 {currentQuestionIndex}/{totalQuestions} 题: {currentRequiredCategory}");

        if (uiController != null)
        {
            uiController.SetQuestionText(currentRequiredCategory);
            uiController.SetProgressBar((float)questionsCompleted / totalQuestions);
        }

        RefreshPieceSelection();
    }

    /// <summary>
    /// 刷新左右木料选择区
    /// </summary>
    private void RefreshPieceSelection()
    {
        if (uiController == null) return;

        // 获取对应类别的榫头和卯眼
        var relevantTenons = allTenonPieces
            .Where(p => p.category == currentRequiredCategory)
            .ToList();

        var relevantMortises = allMortisePieces
            .Where(p => p.category == currentRequiredCategory)
            .ToList();

        if (relevantTenons.Count == 0 || relevantMortises.Count == 0)
        {
            Debug.LogError($"? 类别 {currentRequiredCategory} 的木料不完整!");
            return;
        }

        uiController.PopulatePieceUI(relevantTenons, relevantMortises, currentRequiredCategory);
    }

    /// <summary>
    /// 当题目完成时调用（由SMPieceDragger调用）
    /// </summary>
    public void OnQuestionCompleted()
    {
        if (!gameActive) return;

        questionsCompleted++;
        Debug.Log($"? 完成第 {questionsCompleted}/{totalQuestions} 题");

        // 播放音效反馈（可选）
        if (GameManager.Instance != null && GameManager.Instance.normalClip != null)
        {
            GameManager.Instance.PlaySound(GameManager.Instance.normalClip);
        }

        // 短暂延迟后切换到下一题
        Invoke(nameof(GenerateNextQuestion), 0.8f);
    }

    /// <summary>
    /// 游戏完成（所有题目完成）
    /// </summary>
    private void OnGameComplete()
    {
        gameActive = false;

        if (uiController != null)
        {
            uiController.OnGameComplete(true);
        }

        Debug.Log("?? 游戏完成！");
    }

    /// <summary>
    /// 游戏超时
    /// </summary>
    private void OnGameTimeout()
    {
        gameActive = false;

        if (uiController != null)
        {
            uiController.OnGameComplete(false);
        }

        Debug.Log("? 游戏超时!");
    }

    public SMPieceCategory GetCurrentCategory() => currentRequiredCategory;
    public bool IsGameActive() => gameActive;
}