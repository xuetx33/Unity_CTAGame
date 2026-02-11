using UnityEngine;
using MFrameWork;

public class DialogueTester : MonoBehaviour
{
    private bool m_hasInitialized = false;

    void Start()
    {
        if (!m_hasInitialized)
        {
            MUIManager.Instance.InitUIInfo();
            m_hasInitialized = true;

            // 创建并注册对话UI
            TalkUIController dialogueUI = new TalkUIController();
            dialogueUI.Init();
        }
    }

    void Update()
    {
        MUIManager.Instance.Update(Time.deltaTime);
        MUIManager.Instance.LateUpdate(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleDialogue();
        }
    }

    void ToggleDialogue()
    {
        TalkUIController dialogue = MUIManager.Instance.GetUI("TalkPanel") as TalkUIController;

        if (dialogue == null) return;

        if (dialogue.Active)
        {
            dialogue.Active = false;
        }
        else
        {
            dialogue.Active = true;
            dialogue.StartDialogue(1);
        }
    }
}