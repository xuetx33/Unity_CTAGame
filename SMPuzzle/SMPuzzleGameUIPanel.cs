using UnityEngine;
using MFrameWork;

/// <summary>
/// 榫卯拼接游戏UI面板（MUI框架部分）
/// 继承MUIBase，仅负责面板的初始化、激活、关闭
/// 不涉及具体UI逻辑，具体逻辑由SMPuzzleGameUIController处理
/// </summary>
public class SMPuzzleGameUIPanel : MUIBase
{
    public SMPuzzleGameUIPanel() : base("SMPuzzleGamePanel", MUILayerType.Normal)
    {
        EnableMask = true;      // 显示遮罩
        MaskAlpha = 0.3f;       // 遮罩透明度
        IsCacheUI = false;      // 不缓存UI
    }

    /// <summary>
    /// UI初始化时调用
    /// </summary>
    public override void Init()
    {
        base.Init();
        Debug.Log("? SMPuzzleGameUIPanel 已初始化");

        // 找到UIController并初始化
        if (m_uiGameObject != null)
        {
            SMPuzzleGameUIController controller = m_uiGameObject.GetComponent<SMPuzzleGameUIController>();
            if (controller != null)
            {
                controller.InitGameUI();
                Debug.Log("? UIController 已初始化");
            }
            else
            {
                Debug.LogWarning("? SMPuzzleGamePanel Prefab上未找到SMPuzzleGameUIController脚本!");
            }
        }
    }

    /// <summary>
    /// 面板激活时调用
    /// </summary>
    protected override void OnActive()
    {
        Debug.Log("? 拼接游戏面板已激活");

        if (m_uiGameObject != null)
        {
            SMPuzzleGameUIController controller = m_uiGameObject.GetComponent<SMPuzzleGameUIController>();
            if (controller != null)
            {
                controller.OnPanelActive();
            }
        }
    }

    /// <summary>
    /// 面板关闭时调用
    /// </summary>
    protected override void OnDeActive()
    {
        Debug.Log("? 拼接游戏面板已关闭");

        if (m_uiGameObject != null)
        {
            SMPuzzleGameUIController controller = m_uiGameObject.GetComponent<SMPuzzleGameUIController>();
            if (controller != null)
            {
                controller.OnPanelDeActive();
            }
        }
    }
}