using MFrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
/// <summary>
/// 游戏总管理
/// </summary>
[System.Serializable]
public class CameraBinding
{
    [Tooltip("摄像机组件")]
    public Camera camera;

    [Tooltip("与该摄像机绑定的游戏物体，主摄像机（index 0）可留空")]
    public GameObject boundObject;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

 
    public bool canControlShenYan;
    public bool hasPetTheDog;
    public int candleNum;

    public AudioSource audioSource;
    public AudioClip normalClip;

    public List<Item> itemList = new List<Item>();
    public UnityEngine.UI.Slider audioSlider;
    public UnityEngine.UI.Toggle toggle;
    //NPC对话阶段
    public int NaladialogInfoIndex;
    public int ShangSiDialogInfoIndex;
    public int ZhiXianDialogInfoIndex;
    public int DiZhuDialogInfoIndex;
    public int GuanJiaDialogInfoIndex;
    public int FangShiDialogInfoIndex;
    public int HouChuDialogInfoIndex;
    public int LiuDialogInfoIndex;
    public int BuDialogInfoIndex;
    public int HanDialogInfoIndex;
    //任务判断
    public bool LiuTalk;
    public bool BuTalk;
    public bool HanTalk;
    // 存储每个NPC的对话阶段
    private Dictionary<int, int> npcDialogStages = new Dictionary<int, int>();

    // 问题答案存储变量
    public string QuestionAnswer { get; set; }

    [Header("摄像机设置")]
    [Tooltip("摄像机及其绑定物体的列表，index 0 为主摄像机")]
    public List<CameraBinding> cameraBindings = new List<CameraBinding>();

    [Tooltip("摄像机切换时的平滑过渡时间")]
    public float transitionTime = 0.5f;

    // 当前激活的摄像机索引
    private int currentCameraIndex = 0;
    // 用于平滑过渡的变量
    private float transitionTimer = 0f;
    private Camera currentCamera;
    private Camera targetCamera;
    private GameObject currentBoundObject;
    public GameObject MainCanvas;



   
    // 文字识别结果（新增）
    public string RecognizedText { get; set; } // 存储文字识别结果
    // 书写状态标记（新增）
    public bool isWritingInProgress { get; private set; }
    // 书写提交事件（新增）
    public event Action OnWritingSubmit;
    public HandwritingRecognition handwritingRecognition; // 书写识别组件引用

    // 新增：触发识别并等待完成
    public void StartRecognition()
    {
        // 先取消之前的订阅（避免重复触发）
        handwritingRecognition.OnRecognitionComplete -= OnRecognitionFinished;
        // 订阅识别完成事件
        handwritingRecognition.OnRecognitionComplete += OnRecognitionFinished;
        // 开始识别
        handwritingRecognition.OnButtonClick();
    }

    // 识别完全完成后调用
    private void OnRecognitionFinished()
    {
        // 取消订阅（避免重复触发）
        handwritingRecognition.OnRecognitionComplete -= OnRecognitionFinished;
        // 触发书写提交事件（此时识别结果已完全准备好）
        OnWritingSubmit?.Invoke();
    }


    private void Awake()
    {

        // 单例逻辑：确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 手动标记持久化（或通过PersistentObjectManager）
        }
        else
        {
            Destroy(gameObject); // 销毁重复实例
            return;
        }
        // 初始化摄像机状态
        InitializeCameras();
   
    }
    private void Start()
    {
        // 注册并激活游戏主界面 HUD
        //MUIManager.Instance.RegisterUI("GameHUD", new GameHUDController());
        //MUIManager.Instance.ActiveUI("GameHUD");
        //MUIManager.Instance.RegisterUI("TalkPanel", new TalkUIController());
        //MUIManager.Instance.RegisterUI("TaskPanel", new TaskUIController());

        canControlShenYan = true;

    }
    private void Update()
    {
        
        // 检测F1键按下，切换到索引为1的摄像机
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // 检查索引1是否存在
            if (cameraBindings[0].camera != null)
            {
                //MainCanvas.SetActive(true);
                SwitchToCamera(0);
            }
            else
            {
                Debug.LogWarning("索引为0的摄像机不存在或未设置");
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            // 检查索引1是否存在
            if (cameraBindings[1].camera != null)
            {
               // Debug.Log("关闭UI");
                //MainCanvas.SetActive(false);
                SwitchToCamera(1);
               
            }
            else
            {
                Debug.LogWarning("索引为1的摄像机不存在或未设置");
            }
        }

        // 处理摄像机过渡
        if (targetCamera != null && transitionTimer < transitionTime)
        {
            transitionTimer += Time.deltaTime;
            float t = transitionTimer / transitionTime;
            t = Mathf.SmoothStep(0, 1, t); // 平滑过渡曲线

            // 调整摄像机的深度实现过渡效果
            if (currentCamera != null && targetCamera != null)
            {
                targetCamera.depth = currentCamera.depth + 1;
            }

            // 过渡完成
            if (transitionTimer >= transitionTime)
            {
                // 禁用当前摄像机及其绑定物体
                currentCamera.gameObject.SetActive(false);
                if (currentBoundObject != null)
                {
                    currentBoundObject.SetActive(false);
                }

                // 更新当前摄像机信息
                currentCamera = targetCamera;
                currentCameraIndex = cameraBindings.FindIndex(b => b.camera == targetCamera);
                currentBoundObject = cameraBindings[currentCameraIndex].boundObject;

                // 激活新的绑定物体（如果有）
                if (currentBoundObject != null)
                {
                    currentBoundObject.SetActive(true);
                }

                targetCamera = null;
            }
        }



        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("按下T键，正在打开任务面板UI");
            TaskManager.Instance.AcceptTask(1001);
            TaskManager.Instance.AcceptTask(1002);

            MUIManager.Instance.ActiveUI("TaskPanel");
            
        }

    }
    // 初始化摄像机，确保只有第一个摄像机是激活的
    // 初始化摄像机，确保只有第一个摄像机是激活的
    private void InitializeCameras()
    {
        if (cameraBindings.Count == 0)
        {
            Debug.LogWarning("没有添加任何摄像机绑定到GameManager");
            return;
        }

        // 禁用所有摄像机和绑定物体
        foreach (var binding in cameraBindings)
        {
            if (binding.camera != null)
                binding.camera.gameObject.SetActive(false);

            if (binding.boundObject != null)
                binding.boundObject.SetActive(false);
        }

        // 激活第一个摄像机（主摄像机）
        currentCamera = cameraBindings[0].camera;
        if (currentCamera != null)
        {
            currentCamera.gameObject.SetActive(true);
            currentBoundObject = cameraBindings[0].boundObject;
            // 主摄像机不激活绑定物体（按需求）
        }
        else
        {
            Debug.LogError("主摄像机（index 0）未设置！");
        }

        currentCameraIndex = 0;
    }


    // 根据索引切换到指定摄像机
    public void SwitchToCamera(int index)
    {
        if(index==1)
        {
           
        }
        if(index==0)
        {
          
        }
        if (index < 0 || index >= cameraBindings.Count)
        {
            Debug.LogError("无效的摄像机索引");
            return;
        }

        if (index == currentCameraIndex) return; // 已经是目标摄像机

        var targetBinding = cameraBindings[index];
        if (targetBinding.camera == null)
        {
            Debug.LogError($"索引 {index} 的摄像机未设置！");
            return;
        }

        // 开始过渡
        transitionTimer = 0f;
        targetCamera = targetBinding.camera;
        targetCamera.gameObject.SetActive(true);
        targetCamera.clearFlags = CameraClearFlags.Depth;
    }

    private void OnEnable()
    {
        UIManager.Instance.RefreshItem();
        UIManager.Instance.itemInfromation.text = "123";
    }
    public void UpdateItemInfo(string itemDescription)
    {
        UIManager.Instance.itemInfromation.text = itemDescription;
        Debug.Log(itemDescription);
    }
   
    public void SetNPCDialogStage(int npcID, int stage)
    {
        if (npcDialogStages.ContainsKey(npcID))
        {
            npcDialogStages[npcID] = stage;
        }
        else
        {
            npcDialogStages.Add(npcID, stage);
        }
    }
    /// <summary>
    /// 获取NPC的当前对话阶段
    /// </summary>
    public int GetNPCDialogStage(int npcID)
    {
        if (npcDialogStages.TryGetValue(npcID, out int stage))
        {
            return stage;
        }
        // 默认返回0表示初始阶段
        return 0;
    }
    public void PlayMusic(AudioClip audioClip)
    {
        if (audioSource.clip != audioClip)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (audioClip)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
    public void AddItemToBag(Item itemToAdd)
    {
        if (itemToAdd == null)
        {
            Debug.LogWarning("尝试添加空物品！");
            return;
        }

        // 检查是否已存在该物品
        Item existingItem = itemList.Find(item => item.itemName == itemToAdd.itemName);

        if (existingItem != null)
        {
            itemToAdd.itemHeld++;
            //UIManager.Instance.RefreshItem();
        }
        else
        {
          
            itemList.Add(itemToAdd);
            //UIManager.Instance.CreateNewItem(itemToAdd);
        }
        UIManager.Instance.RefreshItem();
        // 打印当前背包所有物品
        PrintBagItems();
    }

    // 辅助方法：打印背包所有物品
    private void PrintBagItems()
    {
        Debug.Log("===== 背包物品列表 =====");
        if (itemList.Count == 0)
        {
            Debug.Log("背包为空");
            return;
        }

        foreach (var item in itemList)
        {
            Debug.Log($"- {item.itemName} x{item.itemHeld}");
        }
        Debug.Log("======================");
    }
  
    

}
