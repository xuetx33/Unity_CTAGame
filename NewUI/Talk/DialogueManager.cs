using MFrameWork;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private Dictionary<int, DialogueData> dialogueDict;
    private Dictionary<int, List<DialogueData>> dialogueGroups;

    [Header("资源配置")]
    [SerializeField] private string dialogueCSVPath = "TalkContent/TalkText";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDialogueData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadDialogueData()
    {
        dialogueDict = new Dictionary<int, DialogueData>();
        dialogueGroups = new Dictionary<int, List<DialogueData>>();

        TextAsset csvFile = Resources.Load<TextAsset>(dialogueCSVPath);
        if (csvFile == null)
        {
            Debug.LogError("对话CSV文件加载失败: " + dialogueCSVPath);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim()))
                continue;

            DialogueData data = ParseCSVLine(lines[i]);
            if (data != null)
            {
                dialogueDict[data.id] = data;

                // 按对话组组织数据
                if (!dialogueGroups.ContainsKey(data.groupId))
                {
                    dialogueGroups[data.groupId] = new List<DialogueData>();
                }
                dialogueGroups[data.groupId].Add(data);
            }
        }

        Debug.Log($"对话数据加载完成，共{dialogueDict.Count}条对话，{dialogueGroups.Count}个对话组");
    }

    private DialogueData ParseCSVLine(string line)
    {
        string[] fields = ParseCSVFields(line);

        if (fields.Length >= 9)
        {
            DialogueData data = new DialogueData();

            if (int.TryParse(fields[0], out int id))
            {
                data.id = id;
                data.type = fields[1];
                data.characterName = fields[2];
                data.content = fields[3];

                if (int.TryParse(fields[4], out int nextId))
                    data.nextId = nextId;

                data.effect = fields[5];

                if (int.TryParse(fields[6], out int characterId))
                    data.characterId = characterId;

                if (int.TryParse(fields[7], out int groupId))
                    data.groupId = groupId;

                data.condition = fields[8];

                return data;
            }
        }

        return null;
    }

    private string[] ParseCSVFields(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        result.Add(currentField);
        return result.ToArray();
    }

    public int GetSuitableDialogueGroup(int npcId)
    {
        // 如果有TaskManager实例，检查任务状态
        if (TaskManager.Instance != null)
        {
            // 1. 检查是否有可提交的任务（优先级最高）
            if (TaskManager.Instance.HasCompletableTask(npcId))
            {
                Debug.Log($"NPC {npcId} 有可提交的任务");
                return GetCompletionDialogueGroup(npcId);
            }

            // 2. 检查是否有进行中的任务
            if (TaskManager.Instance.HasActiveTaskForNPC(npcId))
            {
                Debug.Log($"NPC {npcId} 有进行中的任务");
                return GetTaskProgressDialogueGroup(npcId);
            }

            // 3. 检查是否有可接任务
            if (TaskManager.Instance.HasAvailableTask(npcId))
            {
                Debug.Log($"NPC {npcId} 有可接任务");
                return GetNewTaskDialogueGroup(npcId);
            }
        }

        // 4. 默认对话
        Debug.Log($"NPC {npcId} 使用默认对话");
        return GetDefaultDialogueGroup(npcId);
    }

    private int GetDefaultDialogueGroup(int npcId)
    {
        // 返回NPC ID作为默认对话组
        return npcId;
    }

    private int GetNewTaskDialogueGroup(int npcId)
    {
        // 有可接任务时的对话组：npcId + 10000
        return npcId + 10000;
    }

    private int GetTaskProgressDialogueGroup(int npcId)
    {
        // 任务进行中的对话组：npcId + 20000
        return npcId + 20000;
    }

    private int GetCompletionDialogueGroup(int npcId)
    {
        // 可提交任务的对话组：npcId + 30000
        return npcId + 30000;
    }

    // 新方法：获取任务完成后的对话组
    private int GetTaskCompletedDialogueGroup(int npcId)
    {
        // 任务完成后的对话组：npcId + 40000
        return npcId + 40000;
    }

    public DialogueData GetDialogueById(int id)
    {
        dialogueDict.TryGetValue(id, out DialogueData data);
        return data;
    }

    public List<DialogueData> GetDialogueGroup(int groupId)
    {
        dialogueGroups.TryGetValue(groupId, out List<DialogueData> group);
        return group;
    }

    public int GetStartDialogueId(int groupId)
    {
        var group = GetDialogueGroup(groupId);
        if (group != null && group.Count > 0)
        {
            // 找到组内ID最小的对话作为起点
            int minId = int.MaxValue;
            foreach (var dialogue in group)
            {
                if (dialogue.id < minId)
                    minId = dialogue.id;
            }
            return minId;
        }
        return -1;
    }
}