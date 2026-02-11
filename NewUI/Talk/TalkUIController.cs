using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace MFrameWork
{
    public class TalkUIController : MUIBase
    {
        // 现有UI组件引用保持不变
        private Image m_character;
        private Text m_talkName;
        private Text m_talkContent;
        private GameObject m_background;
        private GameObject m_optionPanel;
        private Button[] m_optionButtons;
        private Text[] m_optionTexts;

        // 对话数据
        private Dictionary<int, DialogueData> m_dialogueDict;
        private int m_currentDialogueId;
        private bool m_isWaitingForChoice;
        private List<DialogueData> m_currentOptions;
        private bool m_shouldShowOptionsAfterClick = false;

        // 选项选择后的状态
        private bool m_isShowingSelectedOption = false;
        private DialogueData m_selectedOptionData;
        private int m_selectedOptionNextId;

        // 资源路径
        private string m_csvPath = "TalkContent/TalkText";
        private string m_characterSpritesPath = "CharacterSprites/";

        public TalkUIController() : base("TalkPanel", MUILayerType.Top)
        {
            m_isCacheUI = true;
        }

        public override void Init()
        {
            base.Init();

            if (m_uiGameObject == null)
            {
                Debug.LogError("UI GameObject 加载失败: " + m_uiFullPath);
                return;
            }

            // 确保UI初始状态为关闭
            m_uiGameObject.SetActive(false);
            m_active = false;

            // 获取UI组件引用
            Transform transform = m_uiGameObject.transform;

            m_character = transform.Find("Character")?.GetComponent<Image>();
            m_talkName = transform.Find("TalkName")?.GetComponent<Text>();
            m_talkContent = transform.Find("TalkContent")?.GetComponent<Text>();
            m_background = transform.Find("BackGround")?.gameObject;
            m_optionPanel = transform.Find("Option")?.gameObject;

            // 获取选项按钮
            if (m_optionPanel != null)
            {
                m_optionButtons = new Button[3];
                m_optionTexts = new Text[3];

                for (int i = 0; i < 3; i++)
                {
                    string buttonName = "OptionButton" + (i + 1);
                    Transform buttonTransform = m_optionPanel.transform.Find(buttonName);

                    if (buttonTransform != null)
                    {
                        m_optionButtons[i] = buttonTransform.GetComponent<Button>();

                        Transform textTransform = buttonTransform.Find("Text");
                        if (textTransform != null)
                        {
                            m_optionTexts[i] = textTransform.GetComponent<Text>();
                        }
                        else
                        {
                            m_optionTexts[i] = buttonTransform.GetComponent<Text>();
                        }
                    }
                }

                m_optionPanel.SetActive(false);
            }

            // 不再在这里加载对话数据，使用DialogueManager
            // LoadDialogueData();

            MUIManager.Instance.RegisterUI("TalkPanel", this);
        }

        protected override void OnActive()
        {
            if (m_background != null)
                m_background.SetActive(true);
        }

        protected override void OnDeActive()
        {
            if (m_background != null)
                m_background.SetActive(false);

            if (m_optionPanel != null)
                m_optionPanel.SetActive(false);
        }

        // 通过对话组开始对话（新增方法）
        public void StartDialogueByGroup(int groupId)
        {
            int startId = DialogueManager.Instance.GetStartDialogueId(groupId);
            if (startId > 0)
            {
                StartDialogue(startId);
            }
            else
            {
                Debug.LogError($"对话组 {groupId} 没有找到起始对话");
            }
        }

        // 开始对话（原有方法）
        public void StartDialogue(int startId = 1)
        {
            m_currentDialogueId = startId;
            m_isWaitingForChoice = false;
            m_shouldShowOptionsAfterClick = false;
            m_isShowingSelectedOption = false;
            Active = true;
            ShowCurrentDialogue();
        }

        // 显示当前对话（修改方法，使用DialogueManager）
        private void ShowCurrentDialogue()
        {
            DialogueData data = DialogueManager.Instance.GetDialogueById(m_currentDialogueId);
            if (data != null)
            {
                // 更新UI显示
                if (m_talkName != null)
                    m_talkName.text = data.characterName;

                if (m_talkContent != null)
                    m_talkContent.text = data.content;

                // 更新角色立绘
                UpdateCharacterSprite(data.characterId);

                // 处理对话类型
                if (data.type == "#")
                {
                    m_isWaitingForChoice = false;
                    HideOptionsPanel();
                    CheckIfNextIsOption(data.nextId);
                }
                else if (data.type == "@")
                {
                    Debug.LogWarning("直接遇到了选项对话，这不应该发生");
                }

                // 触发效果（增强版）
                if (!string.IsNullOrEmpty(data.effect))
                {
                    TriggerEffect(data.effect);
                }
            }
            else
            {
                EndDialogue();
            }
        }

        // 更新角色立绘（保持不变）
        private void UpdateCharacterSprite(int characterId)
        {
            if (m_character == null)
            {
                Debug.LogError("Character组件为空，无法更新立绘");
                return;
            }

            if (characterId == 0)
            {
                m_character.gameObject.SetActive(false);
                return;
            }

            m_character.gameObject.SetActive(true);

            string spritePath = $"{m_characterSpritesPath}{characterId}";
            Sprite sprite = Resources.Load<Sprite>(spritePath);

            if (sprite != null)
            {
                m_character.sprite = sprite;
            }
            else
            {
                Debug.LogError($"无法加载角色立绘: {spritePath}");
                m_character.gameObject.SetActive(false);
            }
        }

        // 检查下一个对话是否是选项（修改方法，使用DialogueManager）
        private void CheckIfNextIsOption(int nextId)
        {
            if (nextId <= 0) return;

            m_currentOptions = new List<DialogueData>();
            int currentOptionId = nextId;

            while (true)
            {
                DialogueData optionData = DialogueManager.Instance.GetDialogueById(currentOptionId);
                if (optionData != null && optionData.type == "@")
                {
                    m_currentOptions.Add(optionData);
                    currentOptionId++;
                }
                else
                {
                    break;
                }
            }

            if (m_currentOptions.Count > 0)
            {
                m_shouldShowOptionsAfterClick = true;
            }
        }

        // 显示选项（保持不变）
        private void ShowOptionsForCurrentDialogue()
        {
            HideAllOptionButtons();

            for (int i = 0; i < m_currentOptions.Count && i < m_optionButtons.Length; i++)
            {
                if (m_optionButtons[i] != null)
                {
                    if (m_optionTexts[i] != null)
                    {
                        m_optionTexts[i].text = m_currentOptions[i].content;
                    }

                    int optionIndex = i;
                    m_optionButtons[i].onClick.RemoveAllListeners();
                    m_optionButtons[i].onClick.AddListener(() => OnOptionSelected(optionIndex));

                    m_optionButtons[i].gameObject.SetActive(true);
                }
            }

            ShowOptionsPanel();
        }

        // 选项按钮点击事件处理（保持不变）
        private void OnOptionSelected(int optionIndex)
        {
            if (m_currentOptions == null || optionIndex < 0 || optionIndex >= m_currentOptions.Count)
                return;

            DialogueData selectedOption = m_currentOptions[optionIndex];

            HideOptionsPanel();

            ShowSelectedOption(selectedOption);
        }

        // 显示选择的选项内容（保持不变）
        private void ShowSelectedOption(DialogueData selectedOption)
        {
            m_selectedOptionData = selectedOption;
            m_selectedOptionNextId = selectedOption.nextId;
            m_isShowingSelectedOption = true;
            m_isWaitingForChoice = false;

            if (m_talkName != null)
                m_talkName.text = selectedOption.characterName;

            if (m_talkContent != null)
                m_talkContent.text = selectedOption.content;

            UpdateCharacterSprite(selectedOption.characterId);

            if (!string.IsNullOrEmpty(selectedOption.effect))
            {
                TriggerEffect(selectedOption.effect);
            }
        }

        // 继续选项内容后的对话（保持不变）
        private void ContinueAfterSelectedOption()
        {
            if (m_selectedOptionNextId > 0)
            {
                m_currentDialogueId = m_selectedOptionNextId;
                m_isShowingSelectedOption = false;
                m_isWaitingForChoice = false;
                m_shouldShowOptionsAfterClick = false;
                ShowCurrentDialogue();
            }
            else
            {
                EndDialogue();
            }
        }

        // 隐藏所有选项按钮（保持不变）
        private void HideAllOptionButtons()
        {
            if (m_optionButtons != null)
            {
                for (int i = 0; i < m_optionButtons.Length; i++)
                {
                    if (m_optionButtons[i] != null)
                    {
                        m_optionButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        // 显示选项面板（保持不变）
        private void ShowOptionsPanel()
        {
            if (m_optionPanel != null)
            {
                m_optionPanel.SetActive(true);
            }
        }

        // 隐藏选项面板（保持不变）
        private void HideOptionsPanel()
        {
            if (m_optionPanel != null)
            {
                m_optionPanel.SetActive(false);
            }
        }

        private void TriggerEffect(string effect)
        {
            if (string.IsNullOrEmpty(effect)) return;

            string[] effectParts = effect.Split(':');
            if (effectParts.Length == 0) return;

            string effectType = effectParts[0];

            switch (effectType)
            {
                case "accept_task":
                    if (effectParts.Length >= 2 && int.TryParse(effectParts[1], out int taskId))
                    {
                        if (TaskManager.Instance != null)
                        {
                            bool success = TaskManager.Instance.AcceptTask(taskId);
                            if (success)
                            {
                                Debug.Log($"成功接受任务: {taskId}");

                                // 更新对话内容中的进度占位符
                                ReplaceProgressPlaceholders(taskId);
                            }
                        }
                    }
                    break;

                case "complete_task":
                    if (effectParts.Length >= 2 && int.TryParse(effectParts[1], out int completeTaskId))
                    {
                        if (TaskManager.Instance != null)
                        {
                            bool success = TaskManager.Instance.CompleteTask(completeTaskId);
                            if (success)
                            {
                                Debug.Log($"成功完成任务: {completeTaskId}");
                            }
                        }
                    }
                    break;

                case "update_task":
                    if (effectParts.Length >= 3 &&
                        int.TryParse(effectParts[1], out int updateTaskId) &&
                        int.TryParse(effectParts[2], out int progress))
                    {
                        if (TaskManager.Instance != null)
                        {
                            TaskManager.Instance.UpdateTaskProgress(updateTaskId, progress);
                            Debug.Log($"更新任务进度: {updateTaskId} -> {progress}");
                        }
                    }
                    break;

                case "abandon_task":
                    if (effectParts.Length >= 2 && int.TryParse(effectParts[1], out int abandonTaskId))
                    {
                        if (TaskManager.Instance != null)
                        {
                            TaskManager.Instance.AbandonTask(abandonTaskId);
                            Debug.Log($"放弃任务: {abandonTaskId}");
                        }
                    }
                    break;

                case "give_rewards":
                    if (effectParts.Length >= 2)
                    {
                        // 格式: give_rewards:300:金币:5
                        for (int i = 1; i < effectParts.Length; i += 2)
                        {
                            if (i + 1 < effectParts.Length)
                            {
                                string amount = effectParts[i];
                                string type = effectParts[i + 1];
                                Debug.Log($"获得奖励: {type} x{amount}");
                            }
                        }
                    }
                    break;

                case "add_item":
                    if (effectParts.Length >= 3 &&
                        int.TryParse(effectParts[1], out int itemId) &&
                        int.TryParse(effectParts[2], out int count))
                    {
                        // 调用物品添加逻辑
                        Debug.Log($"添加物品: {itemId} x{count}");
                    }
                    break;

                default:
                    // 原有的好感度等效果处理
                    if (effect.Contains("好感"))
                    {
                        Debug.Log($"触发好感度效果: {effect}");
                    }
                    break;
            }
        }

        // 新方法：替换对话中的进度占位符
        private void ReplaceProgressPlaceholders(int taskId)
        {
            if (TaskManager.Instance == null) return;

            TaskData task = TaskManager.Instance.GetTaskData(taskId);
            if (task == null) return;

            // 获取当前对话
            DialogueData currentDialogue = DialogueManager.Instance.GetDialogueById(m_currentDialogueId);
            if (currentDialogue != null && currentDialogue.content.Contains("{progress}"))
            {
                // 替换进度占位符
                string progressText = task.GetProgressText();
                currentDialogue.content = currentDialogue.content.Replace("{progress}", progressText);

                // 更新UI显示
                if (m_talkContent != null)
                    m_talkContent.text = currentDialogue.content;
            }
        }

        // 继续下一句对话（保持不变）
        public void NextDialogue()
        {
            if (m_isShowingSelectedOption)
            {
                ContinueAfterSelectedOption();
                return;
            }

            if (m_isWaitingForChoice)
                return;

            if (m_shouldShowOptionsAfterClick)
            {
                m_isWaitingForChoice = true;
                m_shouldShowOptionsAfterClick = false;
                ShowOptionsForCurrentDialogue();
                return;
            }

            DialogueData data = DialogueManager.Instance.GetDialogueById(m_currentDialogueId);
            if (data != null)
            {
                if (data.nextId > 0)
                {
                    m_currentDialogueId = data.nextId;
                    ShowCurrentDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
            else
            {
                EndDialogue();
            }
        }

        // 结束对话（保持不变）
        private void EndDialogue()
        {
            Active = false;
        }

        // 更新方法，用于检测点击继续（保持不变）
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (m_active && !m_isWaitingForChoice && Input.GetMouseButtonDown(0))
            {
                NextDialogue();
            }
        }
    }
}
// 对话数据结构（更新版）
[System.Serializable]
public class DialogueData
{
    public int id;
    public string type;
    public string characterName;
    public string content;
    public int nextId;
    public string effect;
    public int characterId;
    public int groupId;     // 新增：对话组ID
    public string condition; // 新增：触发条件
}