// TaskData.cs
using System;

[System.Serializable]
public class TaskData
{
    public int taskId;                  // 任务ID
    public string taskName;             // 任务名称
    public string description;          // 任务描述
    public int startNpcId;              // 起始NPC ID
    public int endNpcId;                // 结束NPC ID
    public TaskStatus status;           // 任务状态
    public string objectives;           // 任务目标描述
    public string rewards;              // 任务奖励
    public int currentProgress;         // 当前进度
    public int requiredProgress;        // 需要进度
    public string progressText;         // 进度文本（如：清理哥布林:{0}/{1}）

    // 扩展字段，用于存储条件、前置任务等
    public int prerequisiteTaskId = -1; // 前置任务ID
    public int minLevel = 1;           // 最低等级要求

    // 检查任务是否可接
    public bool CanAccept()
    {
        return status == TaskStatus.NotAccepted;
    }

    // 检查任务是否可提交
    public bool CanComplete()
    {
        return status == TaskStatus.Completable && currentProgress >= requiredProgress;
    }

    // 获取进度文本（替换占位符）
    public string GetProgressText()
    {
        if (string.IsNullOrEmpty(progressText))
            return $"进度: {currentProgress}/{requiredProgress}";

        return string.Format(progressText, currentProgress, requiredProgress);
    }
}