using UnityEngine;
using TMPro;
using MFrameWork;

[RequireComponent(typeof(CircleCollider2D))]
public class NPCInteractable : MonoBehaviour
{
    [Header("NPC基础设置")]
    [SerializeField] private int npcId = 1001;
    [SerializeField] private string npcName = "NPC";

    [Header("交互设置")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.K;

    [Header("对话组配置")]
    [SerializeField] private int defaultDialogueGroup = 1001;

    [Header("UI引用")]
    [SerializeField] private Canvas interactCanvas;
    [SerializeField] private TextMeshProUGUI interactHintText;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 1.5f, 0);

    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private Camera mainCamera;
    private CircleCollider2D interactionCollider;

    private void Start()
    {
        // 获取碰撞器组件
        interactionCollider = GetComponent<CircleCollider2D>();
        if (interactionCollider != null)
        {
            interactionCollider.isTrigger = true;
            interactionCollider.radius = interactDistance;
        }
        else
        {
            Debug.LogError("NPCInteractable: 缺少CircleCollider2D组件");
        }

        // 自动查找玩家
        FindPlayer();

        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
            if (mainCamera == null)
                Debug.LogError("NPCInteractable: 未找到摄像机");
        }

        // 自动设置UI组件
        SetupUIComponents();

        // 初始隐藏提示
        if (interactCanvas != null)
        {
            interactCanvas.gameObject.SetActive(false);
        }

        Debug.Log($"NPCInteractable初始化完成: {npcName} (ID: {npcId})");
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"NPCInteractable: 找到玩家对象: {player.name}");
        }
        else
        {
            Debug.LogError("NPCInteractable: 未找到玩家对象，请确保玩家有'Player'标签");
            // 尝试再次查找
            Invoke("FindPlayer", 1f);
        }
    }

    private void Update()
    {
        // 如果玩家对象丢失，重新查找
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // 使用碰撞器检测，这里保留距离检测作为备用
        if (isPlayerInRange)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= interactDistance)
            {
                // 更新UI位置
                UpdateUIPosition();

                // 显示交互提示
                if (interactCanvas != null && !interactCanvas.gameObject.activeSelf)
                {
                    interactCanvas.gameObject.SetActive(true);
                    Debug.Log($"显示交互提示: {npcName}");
                }

                // 检测按键输入
                if (Input.GetKeyDown(interactKey))
                {
                    Debug.Log($"按下交互键: {interactKey}");
                    StartDialogue();
                }
            }
            else
            {
                // 玩家离开交互范围
                if (interactCanvas != null && interactCanvas.gameObject.activeSelf)
                {
                    interactCanvas.gameObject.SetActive(false);
                    Debug.Log($"隐藏交互提示: {npcName}");
                }
                isPlayerInRange = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerTransform = other.transform;
            Debug.Log($"玩家进入交互范围: {npcName}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactCanvas != null)
            {
                interactCanvas.gameObject.SetActive(false);
            }
            Debug.Log($"玩家离开交互范围: {npcName}");
        }
    }

    private void SetupUIComponents()
    {
        // 如果Canvas不存在，自动创建
        if (interactCanvas == null)
        {
            CreateUIComponents();
        }
        else
        {
            // 确保文本内容正确
            if (interactHintText != null)
            {
                interactHintText.text = $"按{interactKey}键对话";
            }
        }
    }

    private void CreateUIComponents()
    {
        GameObject canvasObj = new GameObject("InteractCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = uiOffset;

        interactCanvas = canvasObj.AddComponent<Canvas>();
        interactCanvas.renderMode = RenderMode.WorldSpace;

        // 设置Canvas大小
        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(3, 1);

        // 创建文本组件
        GameObject textObj = new GameObject("InteractHintText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one * 0.02f;

        interactHintText = textObj.AddComponent<TextMeshProUGUI>();
        interactHintText.text = $"按{interactKey}键对话";
        interactHintText.fontSize = 24;
        interactHintText.alignment = TextAlignmentOptions.Center;
        interactHintText.color = Color.white;

        // 添加背景
        AddTextBackground(textObj);

        Debug.Log($"已为NPC {npcName} 创建交互UI");
    }

    private void AddTextBackground(GameObject textObj)
    {
        GameObject backgroundObj = new GameObject("TextBackground");
        backgroundObj.transform.SetParent(textObj.transform);
        backgroundObj.transform.SetAsFirstSibling();
        backgroundObj.transform.localPosition = Vector3.zero;
        backgroundObj.transform.localScale = Vector3.one;

        UnityEngine.UI.Image bgImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(200, 50);
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void UpdateUIPosition()
    {
        if (interactCanvas != null && mainCamera != null)
        {
            // 让Canvas始终面向摄像机
            interactCanvas.transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void StartDialogue()
    {
        Debug.Log($"开始对话: NPC {npcName} (ID: {npcId})");

        // 检查DialogueManager是否存在
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager实例不存在！");
            return;
        }

        // 获取合适的对话组
        int dialogueGroupId = DialogueManager.Instance.GetSuitableDialogueGroup(npcId);
        Debug.Log($"获取对话组: {dialogueGroupId}");

        if (dialogueGroupId > 0)
        {
            TalkUIController dialogueUI = MUIManager.Instance.GetUI("TalkPanel") as TalkUIController;
            if (dialogueUI != null)
            {
                dialogueUI.StartDialogueByGroup(dialogueGroupId);
                Debug.Log($"成功启动对话组: {dialogueGroupId}");
            }
            else
            {
                Debug.LogError("对话UI未找到或未注册");
            }
        }
        else
        {
            Debug.LogWarning($"NPC {npcId} 没有找到合适的对话组");
        }

        // 隐藏提示
        if (interactCanvas != null)
        {
            interactCanvas.gameObject.SetActive(false);
        }
    }

    [ContextMenu("设置默认交互提示")]
    private void SetupDefaultHint()
    {
        SetupUIComponents();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + uiOffset, 0.2f);
    }
}