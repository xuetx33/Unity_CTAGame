using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 榫卯拼接游戏 - 总管理器
/// 负责追踪所有构件的放置状态，判定完成条件，管理计时与分数，并提供重置接口。
/// </summary>
public class SunMaoPuzzleManager : MonoBehaviour
{
    // ---- 单例 ----
    public static SunMaoPuzzleManager Instance { get; private set; }

    [Header("谜题设置")]
    [Tooltip("本谜题需要放置的构件总数（0 = 自动统计场景中的 SunMaoPiece 数量）")]
    public int totalPieces = 0;

    [Header("计时设置")]
    [Tooltip("是否启用计时功能")]
    public bool enableTimer = true;

    [Tooltip("倒计时秒数（0 = 正向计时，不限时）")]
    public float countdownTime = 120f;

    [Header("分数设置")]
    [Tooltip("完成谜题的基础分数")]
    public int baseScore = 100;

    [Tooltip("每提前 1 秒完成额外加分")]
    public int bonusScorePerSecond = 1;

    [Header("事件")]
    public UnityEvent OnPuzzleComplete;  // 所有构件放置完毕
    public UnityEvent OnPuzzleFailed;    // 倒计时耗尽
    public UnityEvent OnPuzzleReset;     // 谜题重置

    // ---- 内部状态 ----
    private int placedCount = 0;
    private bool isPuzzleComplete = false;
    private bool isPuzzleActive = false;

    private float elapsedTime = 0f;
    private float remainingTime = 0f;

    private SunMaoPiece[] allPieces;
    private SunMaoSlot[]  allSlots;

    // ---- Unity 生命周期 ----
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializePuzzle();
    }

    private void Update()
    {
        if (!isPuzzleActive || isPuzzleComplete) return;

        elapsedTime += Time.deltaTime;

        if (enableTimer && countdownTime > 0f)
        {
            remainingTime = Mathf.Max(0f, countdownTime - elapsedTime);

            if (remainingTime <= 0f)
            {
                PuzzleFailed();
            }
        }
    }

    // ---- 初始化 ----

    /// <summary>
    /// 扫描场景，收集所有构件与插槽，开始谜题。
    /// </summary>
    public void InitializePuzzle()
    {
        allPieces = FindObjectsOfType<SunMaoPiece>();
        allSlots  = FindObjectsOfType<SunMaoSlot>();

        if (totalPieces <= 0)
            totalPieces = allPieces.Length;

        placedCount      = 0;
        isPuzzleComplete = false;
        elapsedTime      = 0f;
        remainingTime    = countdownTime;
        isPuzzleActive   = true;

        Debug.Log($"[榫卯拼接] 谜题开始，共 {totalPieces} 个构件，限时 {(countdownTime > 0 ? countdownTime + "秒" : "不限时")}");
    }

    // ---- 由 SunMaoPiece 回调 ----

    /// <summary>
    /// 每次有构件成功放入插槽时调用。
    /// </summary>
    public void OnPiecePlaced()
    {
        if (!isPuzzleActive || isPuzzleComplete) return;

        placedCount++;
        Debug.Log($"[榫卯拼接] 已放置 {placedCount} / {totalPieces} 个构件");

        if (placedCount >= totalPieces)
            PuzzleComplete();
    }

    // ---- 完成 / 失败 / 重置 ----

    private void PuzzleComplete()
    {
        isPuzzleComplete = true;
        isPuzzleActive   = false;

        int score = CalculateScore();
        Debug.Log($"[榫卯拼接] 拼接完成！耗时 {elapsedTime:F1} 秒，得分：{score}");

        OnPuzzleComplete?.Invoke();
    }

    private void PuzzleFailed()
    {
        isPuzzleActive = false;
        Debug.Log("[榫卯拼接] 时间耗尽，拼接失败！");
        OnPuzzleFailed?.Invoke();
    }

    /// <summary>
    /// 重置谜题：将所有构件恢复到初始位置并清空插槽。
    /// </summary>
    public void ResetPuzzle()
    {
        // 先清空所有插槽
        if (allSlots != null)
        {
            foreach (SunMaoSlot slot in allSlots)
            {
                if (slot != null && slot.IsOccupied)
                    slot.Vacate();
            }
        }

        // 再重置所有构件
        if (allPieces != null)
        {
            foreach (SunMaoPiece piece in allPieces)
            {
                if (piece != null)
                    piece.ResetPiece();
            }
        }

        InitializePuzzle();
        OnPuzzleReset?.Invoke();
        Debug.Log("[榫卯拼接] 谜题已重置");
    }

    // ---- 分数计算 ----

    /// <summary>计算当前得分（完成后调用最准确）。</summary>
    public int CalculateScore()
    {
        int score = baseScore;

        if (enableTimer && countdownTime > 0f)
        {
            int timeBonus = Mathf.Max(0, Mathf.FloorToInt(remainingTime)) * bonusScorePerSecond;
            score += timeBonus;
        }

        return score;
    }

    // ---- 公共查询接口 ----

    /// <summary>当前已放置的构件数量。</summary>
    public int PlacedCount => placedCount;

    /// <summary>谜题是否已完成。</summary>
    public bool IsPuzzleComplete => isPuzzleComplete;

    /// <summary>倒计时剩余秒数（正向计时模式下与已用时间相同）。</summary>
    public float RemainingTime => remainingTime;

    /// <summary>已用时间（秒）。</summary>
    public float ElapsedTime => elapsedTime;

    /// <summary>谜题进度（0 到 1）。</summary>
    public float Progress => totalPieces > 0 ? (float)placedCount / totalPieces : 0f;
}
