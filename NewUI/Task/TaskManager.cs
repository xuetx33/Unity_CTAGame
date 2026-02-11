// TaskManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }

    // 任务数据存储
    private Dictionary<int, TaskData> allTasks = new Dictionary<int, TaskData>();
    private Dictionary<int, TaskStatus> taskStatus = new Dictionary<int, TaskStatus>();
    private Dictionary<int, int> taskProgress = new Dictionary<int, int>();

    // 事件系统
    public event Action<int> OnTaskAccepted;
    public event Action<int> OnTaskProgressUpdated;
    public event Action<int> OnTaskCompleted;
    public event Action OnTaskListChanged;

    // 当前激活的任务列表
    public List<int> ActiveTaskIds { get; private set; } = new List<int>();

    [Header("任务配置")]
    [SerializeField] private string taskDataPath = "TaskData/Tasks";


    private void Awake()
    {
        //Debug.Log("TaskManager-TEST");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            Debug.Log("任务管理器已创建并初始化");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
        

    private void Initialize()
    {
        LoadTaskData();
        Debug.Log($"任务管理器初始化完成，加载了{allTasks.Count}个任务");
    }

    private void LoadTaskData()
    {
        // 示例任务数据
        CreateSampleTasks();

        // 初始化状态
        foreach (var taskId in allTasks.Keys)
        {
            taskStatus[taskId] = TaskStatus.NotAccepted;
            taskProgress[taskId] = 0;
        }
    }

    private void CreateSampleTasks()
    {
        // 任务1：清理哥布林
        TaskData task1 = new TaskData
        {
            taskId = 1001,
            taskName = "清理哥布林",
            description = "东边森林的哥布林最近很猖獗，去清理10只哥布林。",
            startNpcId = 1001,
            endNpcId = 1001,
            status = TaskStatus.NotAccepted,
            currentProgress = 0,
            requiredProgress = 10,
            objectives = "清理森林中的哥布林",
            rewards = "金币:300,经验值:100,物品:治疗药水x5",
            progressText = "清理哥布林:{0}/{1}"
        };

        allTasks[1001] = task1;

        // 任务2：采集月光草
        TaskData task2 = new TaskData
        {
            taskId = 1002,
            taskName = "采集月光草",
            description = "帮草药师刘娅菲采集3株月光草。",
            startNpcId = 1002,
            endNpcId = 1002,
            status = TaskStatus.NotAccepted,
            currentProgress = 0,
            requiredProgress = 3,
            objectives = "在夜晚采集月光草",
            rewards = "金币:100,经验值:80,物品:月光药剂x1",
            progressText = "采集月光草:{0}/{3}"
        };

        allTasks[1002] = task2;
      //  AcceptTask(1001);
    }

    // 获取任务数据
    public TaskData GetTaskData(int taskId)
    {
        allTasks.TryGetValue(taskId, out TaskData task);
        return task;
    }

    // 获取任务状态
    public TaskStatus GetTaskStatus(int taskId)
    {
        if (taskStatus.TryGetValue(taskId, out TaskStatus status))
            return status;
        return TaskStatus.NotAccepted;
    }

    // 获取任务进度
    public int GetTaskProgress(int taskId)
    {
        if (taskProgress.TryGetValue(taskId, out int progress))
            return progress;
        return 0;
    }

    // 接受任务
    public bool AcceptTask(int taskId)
    {
        if (!allTasks.ContainsKey(taskId))
        {
            Debug.LogWarning($"任务不存在: {taskId}");
            return false;
        }

        TaskData task = allTasks[taskId];

        // 检查前置任务
        if (task.prerequisiteTaskId > 0)
        {
            TaskStatus prerequisiteStatus = GetTaskStatus(task.prerequisiteTaskId);
            if (prerequisiteStatus != TaskStatus.Completed)
            {
                Debug.LogWarning($"无法接受任务 {taskId}，需要先完成前置任务 {task.prerequisiteTaskId}");
                return false;
            }
        }

        // 检查是否已接受
        if (taskStatus[taskId] != TaskStatus.NotAccepted)
        {
            Debug.LogWarning($"任务 {taskId} 已经接受过");
            return false;
        }

        // 更新状态
        taskStatus[taskId] = TaskStatus.InProgress;
        taskProgress[taskId] = 0;

        // 添加到激活任务列表
        if (!ActiveTaskIds.Contains(taskId))
            ActiveTaskIds.Add(taskId);

        // 触发事件
        OnTaskAccepted?.Invoke(taskId);
        OnTaskListChanged?.Invoke();

        Debug.Log($"接受任务: {task.taskName} (ID: {taskId})");
        return true;
    }

    // 更新任务进度
    public void UpdateTaskProgress(int taskId, int amount)
    {
        if (!taskStatus.ContainsKey(taskId) || taskStatus[taskId] != TaskStatus.InProgress)
            return;

        int oldProgress = taskProgress[taskId];
        taskProgress[taskId] += amount;

        TaskData task = allTasks[taskId];
        task.currentProgress = taskProgress[taskId];

        // 检查是否完成任务要求
        if (taskProgress[taskId] >= task.requiredProgress && taskStatus[taskId] == TaskStatus.InProgress)
        {
            taskStatus[taskId] = TaskStatus.Completable;
            task.status = TaskStatus.Completable;
            Debug.Log($"任务 {taskId} 已完成，可提交");
        }

        OnTaskProgressUpdated?.Invoke(taskId);

        if (oldProgress != taskProgress[taskId])
        {
            Debug.Log($"任务 {taskId} 进度更新: {oldProgress} -> {taskProgress[taskId]}");
        }
    }

    // 完成任务（提交任务）
    public bool CompleteTask(int taskId)
    {
        if (!allTasks.ContainsKey(taskId))
        {
            Debug.LogWarning($"任务不存在: {taskId}");
            return false;
        }

        if (taskStatus[taskId] != TaskStatus.Completable)
        {
            Debug.LogWarning($"任务 {taskId} 还不能提交");
            return false;
        }

        TaskData task = allTasks[taskId];
        taskStatus[taskId] = TaskStatus.Completed;
        task.status = TaskStatus.Completed;

        // 从激活任务列表移除
        ActiveTaskIds.Remove(taskId);

        // 发放奖励
        GiveRewards(task);

        // 触发事件
        OnTaskCompleted?.Invoke(taskId);
        OnTaskListChanged?.Invoke();

        Debug.Log($"完成任务: {task.taskName} (ID: {taskId})");
        return true;
    }

    // 放弃任务
    public bool AbandonTask(int taskId)
    {
        if (!allTasks.ContainsKey(taskId) || taskStatus[taskId] == TaskStatus.NotAccepted)
            return false;

        taskStatus[taskId] = TaskStatus.NotAccepted;
        taskProgress[taskId] = 0;
        ActiveTaskIds.Remove(taskId);

        OnTaskListChanged?.Invoke();
        Debug.Log($"放弃任务: {taskId}");
        return true;
    }

    // 发放奖励
    private void GiveRewards(TaskData task)
    {
        if (string.IsNullOrEmpty(task.rewards))
            return;

        string[] rewardEntries = task.rewards.Split(',');
        foreach (string entry in rewardEntries)
        {
            string[] parts = entry.Trim().Split(':');
            if (parts.Length < 2) continue;

            string type = parts[0].Trim();
            string value = parts[1].Trim();

            switch (type)
            {
                case "金币":
                    if (int.TryParse(value, out int gold))
                    {
                        Debug.Log($"获得金币: {gold}");
                        // 给玩家金币
                    }
                    break;

                case "经验值":
                    if (int.TryParse(value, out int exp))
                    {
                        Debug.Log($"获得经验值: {exp}");
                        // 给玩家经验值
                    }
                    break;

                case "物品":
                    Debug.Log($"获得物品: {value}");
                    // 给玩家物品
                    break;
            }
        }
    }

    // NPC相关查询方法
    public bool HasAvailableTask(int npcId)
    {
        foreach (var task in allTasks.Values)
        {
            if (task.startNpcId == npcId && taskStatus[task.taskId] == TaskStatus.NotAccepted)
                return true;
        }
        return false;
    }

    public bool HasActiveTaskForNPC(int npcId)
    {
        foreach (var task in allTasks.Values)
        {
            TaskStatus status = GetTaskStatus(task.taskId);
            if (task.startNpcId == npcId && (status == TaskStatus.InProgress || status == TaskStatus.Completable))
                return true;
        }
        return false;
    }

    public bool HasCompletableTask(int npcId)
    {
        foreach (var task in allTasks.Values)
        {
            if (task.endNpcId == npcId && taskStatus[task.taskId] == TaskStatus.Completable)
                return true;
        }
        return false;
    }

    public int GetTaskForNPC(int npcId)
    {
        foreach (var task in allTasks.Values)
        {
            if (task.startNpcId == npcId || task.endNpcId == npcId)
                return task.taskId;
        }
        return -1;
    }

    // 获取所有任务
    public List<TaskData> GetAllTasks()
    {
        return allTasks.Values.ToList();
    }

    // 获取激活的任务
    public List<TaskData> GetActiveTasks()
    {
        List<TaskData> activeTasks = new List<TaskData>();
        foreach (int taskId in ActiveTaskIds)
        {
            if (allTasks.ContainsKey(taskId))
                activeTasks.Add(allTasks[taskId]);
        }
        return activeTasks;
    }
}