using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFrameWork; // MUIBase / MUIManager

/*
 TaskUIController.cs

 说明：
 - 面板（TaskPanel）继承自 MUIBase，通过 Resources/Prefabs/UI/TaskPanel 加载。
 - 列表项预制体：Resources/Prefabs/UI/TaskItem（根或子对象包含 TitleText、StatusText、Button）
 - 详情预制体：Resources/Prefabs/UI/TaskDetail（包含 TitleText、DescriptionText、StatusText、ProgressText、RewardsText、ButtonGroup/SubmitButton、ButtonGroup/AbandonButton）
 - 所有文本组件均使用 UnityEngine.UI.Text（你已替换）。
 - 本文件为单一控制器实现：创建、刷新任务列表、显示详情、提交/放弃、关闭面板。
 - 使用 MUIManager 来注册与激活 / 隐藏 UI。面板关闭回收由 MUIManager.DeActiveUI(UIName) 完成。
*/

public class TaskUIController : MUIBase
{
    // 预制体资源路径常量（Resources 下）
    private const string TaskItemPrefabPath = "Prefabs/UI/TaskItem";
    private const string TaskDetailPrefabPath = "Prefabs/UI/TaskDetail";

    // UI 节点引用（查找后缓存）
    private Transform leftContentTransform;
    private Transform rightDetailContainer;
    private Button closeButton; // 关闭按钮

    // 动态创建的项与详情管理
    private Dictionary<int, GameObject> taskItemObjects = new Dictionary<int, GameObject>();
    private TaskDetailUI currentDetailUI;
    private int currentTaskId = -1;

    // 构造：UIName 与 MUIBase 构造器一致（你已改为 "TaskPanel"）
    public TaskUIController() : base("TaskPanel", MUILayerType.Top)
    {
        // 自动向 MUIManager 注册（注册不初始化）
        try
        {
            if (MUIManager.Instance != null)
            {
                MUIManager.Instance.RegisterUI(UIName, this);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TaskUIController] 注册到 MUIManager 失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 初始化面板（加载预制体后第一次调用）
    /// 在此绑定各个子节点并订阅 TaskManager 事件
    /// </summary>
    public override void Init()
    {
        base.Init();
        if (!IsInited) return;

        // ---------------------------
        // 查找左侧任务列表 Content（尝试几个常见路径）
        // ---------------------------
        leftContentTransform = m_uiGameObject.transform.Find("LeftPanel/Scroll View/Viewport/Content")
                             ?? m_uiGameObject.transform.Find("LeftPanel/TaskTabGroup/Scroll View/Viewport/Content")
                             ?? m_uiGameObject.transform.Find("LeftPanel/ScrollView/Viewport/Content");

        if (leftContentTransform == null)
            Debug.LogWarning("[TaskUIController] 未找到 LeftPanel Content，请检查 TaskPanel 预制体节点路径");

        // ---------------------------
        // 查找右侧详情容器（如果面板预设中已放入 TaskDetail 节点则直接复用）
        // ---------------------------
        rightDetailContainer = m_uiGameObject.transform.Find("RightPanel/TaskDetail")
                             ?? m_uiGameObject.transform.Find("RightPanel/TaskScrollView/Viewport/Content")
                             ?? m_uiGameObject.transform.Find("RightPanel");

        if (rightDetailContainer == null)
            Debug.LogWarning("[TaskUIController] 未找到 RightPanel 节点，将使用面板根节点作为详情容器");

        // ---------------------------
        // 关闭按钮绑定（如果预制体包含 CloseButton 节点）
        // ---------------------------
        Transform closeTrans = m_uiGameObject.transform.Find("CloseButton")
                            ?? m_uiGameObject.transform.Find("TopBar/CloseButton")
                            ?? m_uiGameObject.transform.Find("Close");
        if (closeTrans != null)
        {
            closeButton = closeTrans.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }
        else
        {
            // 如果没有找到，打印提示（不是致命）
            Debug.Log("[TaskUIController] 未找到 CloseButton，如果需要关闭功能请检查预制体命名或手动绑定");
        }

        // ---------------------------
        // 订阅 TaskManager 事件（若实例已准备）
        // ---------------------------
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskListChanged += OnTaskListChanged;
            TaskManager.Instance.OnTaskProgressUpdated += OnTaskProgressUpdated;
            TaskManager.Instance.OnTaskCompleted += OnTaskCompleted;
        }

        // 填充列表
        RefreshTaskList();
    }

    /// <summary>
    /// 面板激活时刷新（MUIBase 在 Active=true 时会触发）
    /// </summary>
    protected override void OnActive()
    {
        RefreshTaskList();
    }

    protected override void OnDeActive() { }

    /// <summary>
    /// 面板卸载、销毁前的清理
    /// 取消订阅、移除监听、销毁动态对象
    /// </summary>
    public override void Uninit()
    {
        // 取消 TaskManager 事件订阅
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskListChanged -= OnTaskListChanged;
            TaskManager.Instance.OnTaskProgressUpdated -= OnTaskProgressUpdated;
            TaskManager.Instance.OnTaskCompleted -= OnTaskCompleted;
        }

        // 取消 closeButton 监听并清理引用
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseClicked);

        // 清理创建的 UI 元素
        ClearTaskItems();
        ClearDetail();

        base.Uninit();
    }

    // ---------- 事件处理（TaskManager） ----------
    private void OnTaskListChanged() => RefreshTaskList();

    private void OnTaskProgressUpdated(int taskId)
    {
        // 更新列表项显示
        if (taskItemObjects.TryGetValue(taskId, out GameObject obj))
        {
            var ui = obj.GetComponent<TaskItemUI>();
            ui?.Refresh();
        }
        // 若详情正在显示该任务，也刷新详情
        if (currentTaskId == taskId)
            currentDetailUI?.Refresh(taskId);
    }

    private void OnTaskCompleted(int taskId)
    {
        // 任务完成后刷新列表与详情
        RefreshTaskList();
        if (currentTaskId == taskId)
            currentDetailUI?.Refresh(taskId);
    }

    // ---------- 列表刷新与项管理 ----------
    /// <summary>
    /// 刷新左侧任务列表（仅显示 InProgress / Completable 状态的任务）
    /// </summary>
    private void RefreshTaskList()
    {
        if (leftContentTransform == null) return;

        ClearTaskItems();

        List<TaskData> showTasks = new List<TaskData>();
        var all = TaskManager.Instance?.GetAllTasks();
        if (all != null)
        {
            foreach (var t in all)
            {
                TaskStatus s = TaskManager.Instance.GetTaskStatus(t.taskId);
                if (s == TaskStatus.InProgress || s == TaskStatus.Completable)
                    showTasks.Add(t);
            }
        }

        // 加载任务项预制体
        GameObject itemPrefab = Resources.Load<GameObject>(TaskItemPrefabPath);
        if (itemPrefab == null)
        {
            Debug.LogError("[TaskUIController] 无法加载 TaskItem 预制体，路径: " + TaskItemPrefabPath);
            return;
        }

        // 实例化并初始化每个任务项
        foreach (var t in showTasks)
        {
            GameObject go = GameObject.Instantiate(itemPrefab, leftContentTransform, false);
            go.name = $"TaskItem_{t.taskId}";
            var ui = go.GetComponent<TaskItemUI>();
            if (ui == null) ui = go.AddComponent<TaskItemUI>();
            ui.Setup(t.taskId, OnTaskItemClicked);
            taskItemObjects[t.taskId] = go;
        }

        // 默认选中第一个任务（如果存在）
        if (showTasks.Count > 0)
            ShowTaskDetail(showTasks[0].taskId);
        else
            ClearDetail();
    }

    /// <summary>
    /// 清除左侧动态生成的任务项
    /// </summary>
    private void ClearTaskItems()
    {
        foreach (var kv in taskItemObjects)
            if (kv.Value != null) GameObject.Destroy(kv.Value);
        taskItemObjects.Clear();
    }

    private void OnTaskItemClicked(int taskId) => ShowTaskDetail(taskId);

    // ---------- 详情展示 ----------
    /// <summary>
    /// 显示某个任务的详情：若右侧已有详情并且是同一任务则刷新，否则销毁旧详情并实例化新详情预制体
    /// </summary>
    private void ShowTaskDetail(int taskId)
    {
        currentTaskId = taskId;

        // 如果当前详情就是目标任务，直接刷新
        if (currentDetailUI != null && currentDetailUI.CurrentTaskId == taskId)
        {
            currentDetailUI.Refresh(taskId);
            return;
        }

        // 销毁旧详情
        if (currentDetailUI != null)
        {
            if (currentDetailUI.gameObject != null)
                GameObject.Destroy(currentDetailUI.gameObject);
            currentDetailUI = null;
        }

        // 先尝试在容器中找到名为 "TaskDetail" 的节点（预制体可能包含）
        Transform exist = rightDetailContainer?.Find("TaskDetail");
        if (exist != null)
        {
            currentDetailUI = exist.GetComponent<TaskDetailUI>() ?? exist.gameObject.AddComponent<TaskDetailUI>();
            currentDetailUI.Refresh(taskId);
            return;
        }

        // 否则实例化 TaskDetail 预制体到右侧容器
        GameObject detailPrefab = Resources.Load<GameObject>(TaskDetailPrefabPath);
        if (detailPrefab == null)
        {
            Debug.LogError("[TaskUIController] 无法加载 TaskDetail 预制体，路径: " + TaskDetailPrefabPath);
            return;
        }

        GameObject detailObj = GameObject.Instantiate(detailPrefab, rightDetailContainer ?? m_uiGameObject.transform, false);
        detailObj.name = "TaskDetail";
        currentDetailUI = detailObj.GetComponent<TaskDetailUI>() ?? detailObj.AddComponent<TaskDetailUI>();
        currentDetailUI.Refresh(taskId);
    }

    /// <summary>
    /// 清空/销毁当前详情
    /// </summary>
    private void ClearDetail()
    {
        if (currentDetailUI != null)
        {
            if (currentDetailUI.gameObject != null)
                GameObject.Destroy(currentDetailUI.gameObject);
            currentDetailUI = null;
            currentTaskId = -1;
        }
    }

    // ---------- Close 按钮处理 ----------
    /// <summary>
    /// 关闭面板的回调。通过 MUIManager 隐藏/卸载 UI。
    /// </summary>
    private void OnCloseClicked()
    {
        // 使用 MUIManager 隐藏当前 UI（会触发 MUIBase.Active = false 并隐藏遮罩）
        try
        {
            if (MUIManager.Instance != null)
            {
                MUIManager.Instance.DeActiveUI(UIName);
            }
            else
            {
                // 退回到直接设置 Active=false 的兜底逻辑（如果没有 MUIManager）
                this.Active = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TaskUIController] 关闭面板时出错: " + ex.Message);
            this.Active = false;
        }
    }
}

/* -----------------------
   TaskItemUI: 使用 UnityEngine.UI.Text
   - 负责显示任务名称和状态，响应点击事件（回调到 TaskUIController）
------------------------*/
public class TaskItemUI : MonoBehaviour
{
    public Text TitleText;
    public Text StatusText;
    public Button RootButton;

    private int taskId;
    private Action<int> onClick;

    private void Awake()
    {
        // 优先使用 inspector 绑定的引用；若为空则尝试通过常见子节点名查找
        if (TitleText == null) TitleText = transform.Find("TitleText")?.GetComponent<Text>();
        if (StatusText == null) StatusText = transform.Find("StatusText")?.GetComponent<Text>();

        if (RootButton == null) RootButton = GetComponent<Button>() ?? transform.GetComponentInChildren<Button>();
        if (RootButton != null) RootButton.onClick.AddListener(OnButtonClicked);
    }

    public void Setup(int taskId, Action<int> onClick)
    {
        this.taskId = taskId;
        this.onClick = onClick;
        Refresh();
    }

    public void Refresh()
    {
        if (TaskManager.Instance == null) return;
        TaskData data = TaskManager.Instance.GetTaskData(taskId);
        TaskStatus status = TaskManager.Instance.GetTaskStatus(taskId);

        string title = data != null ? data.taskName : $"Task {taskId}";
        string statusStr = status.ToString();

        if (TitleText != null) TitleText.text = title;
        if (StatusText != null) StatusText.text = statusStr;
    }

    private void OnButtonClicked() => onClick?.Invoke(taskId);

    private void OnDestroy()
    {
        if (RootButton != null) RootButton.onClick.RemoveListener(OnButtonClicked);
    }
}

/* -----------------------
   TaskDetailUI: 使用 UnityEngine.UI.Text
   - 负责显示详情与控制 提交/放弃 操作
------------------------*/
public class TaskDetailUI : MonoBehaviour
{
    public Text TitleText;
    public Text DescriptionText;
    public Text StatusText;
    public Text ProgressText;
    public Text RewardsText;

    public Button SubmitButton;
    public Button AbandonButton;

    public int CurrentTaskId { get; private set; } = -1;

    private void Awake()
    {
        // 尝试通过 inspector 绑定或查找
        TitleText = TitleText ?? transform.Find("TitleText")?.GetComponent<Text>();
        DescriptionText = DescriptionText ?? transform.Find("DescriptionText")?.GetComponent<Text>();
        StatusText = StatusText ?? transform.Find("StatusText")?.GetComponent<Text>();
        ProgressText = ProgressText ?? transform.Find("ProgressText")?.GetComponent<Text>();
        RewardsText = RewardsText ?? transform.Find("RewardsText")?.GetComponent<Text>();

        SubmitButton = SubmitButton ?? transform.Find("ButtonGroup/SubmitButton")?.GetComponent<Button>();
        AbandonButton = AbandonButton ?? transform.Find("ButtonGroup/AbandonButton")?.GetComponent<Button>();

        if (SubmitButton != null) SubmitButton.onClick.AddListener(OnSubmitClicked);
        if (AbandonButton != null) AbandonButton.onClick.AddListener(OnAbandonClicked);
    }

    /// <summary>
    /// 根据任务 id 填充界面显示内容并根据状态控制按钮显示/隐藏
    /// </summary>
    public void Refresh(int taskId)
    {
        CurrentTaskId = taskId;
        if (TaskManager.Instance == null)
        {
            Clear();
            return;
        }

        TaskData data = TaskManager.Instance.GetTaskData(taskId);
        if (data == null)
        {
            Clear();
            return;
        }

        TaskStatus status = TaskManager.Instance.GetTaskStatus(taskId);

        SetText(TitleText, data.taskName);
        SetText(DescriptionText, data.description);
        SetText(StatusText, status.ToString());
       // string progressStr = string.Format(data.progressText ?? "{0}/{1}", data.currentProgress, data.requiredProgress);
        //SetText(ProgressText, progressStr);
        SetText(RewardsText, data.rewards);

        if (SubmitButton != null) SubmitButton.gameObject.SetActive(status == TaskStatus.Completable);
        if (AbandonButton != null) AbandonButton.gameObject.SetActive(status == TaskStatus.InProgress || status == TaskStatus.Completable);
    }

    private void SetText(Text txt, string value)
    {
        if (txt != null) txt.text = value ?? "";
    }

    private void Clear()
    {
        CurrentTaskId = -1;
        SetText(TitleText, "");
        SetText(DescriptionText, "");
        SetText(StatusText, "");
        SetText(ProgressText, "");
        SetText(RewardsText, "");
        if (SubmitButton != null) SubmitButton.gameObject.SetActive(false);
        if (AbandonButton != null) AbandonButton.gameObject.SetActive(false);
    }

    private void OnSubmitClicked()
    {
        if (CurrentTaskId <= 0 || TaskManager.Instance == null) return;

        bool ok = TaskManager.Instance.CompleteTask(CurrentTaskId);
        if (ok)
        {
            Refresh(CurrentTaskId);
        }
    }

    private void OnAbandonClicked()
    {
        if (CurrentTaskId <= 0 || TaskManager.Instance == null) return;

        bool ok = TaskManager.Instance.AbandonTask(CurrentTaskId);
        if (ok)
        {
            Clear();
        }
    }

    private void OnDestroy()
    {
        if (SubmitButton != null) SubmitButton.onClick.RemoveListener(OnSubmitClicked);
        if (AbandonButton != null) AbandonButton.onClick.RemoveListener(OnAbandonClicked);
    }
}

/* -----------------------
   TaskUIStarter: 测试用（通过 MUIManager 激活）
   在没有你自定义按键打开逻辑的情况下可用来快速测试面板
------------------------*/
public class TaskUIStarter : MonoBehaviour
{
    private void Start()
    {
        // 延迟确保 TaskManager、MUIManager 已初始化
        Invoke(nameof(OpenPanelViaMUI), 0.1f);
    }

    private void OpenPanelViaMUI()
    {
        var panel = new TaskUIController();
        // 注册（构造已经尝试注册一次，但双保险）
        if (MUIManager.Instance != null)
            MUIManager.Instance.RegisterUI(panel.UIName, panel);

        // 激活面板（MUIManager 会调用 Init 并设置 Active）
        MUIManager.Instance.ActiveUI(panel.UIName);
    }
}