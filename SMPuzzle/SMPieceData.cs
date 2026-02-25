using UnityEngine;

/// <summary>
/// 木料类型：榫头 or 卯眼
/// </summary>
public enum SMPieceType
{
    Tenon,   // 榫头（凸）
    Mortise  // 卯眼（凹）
}

/// <summary>
/// 木料配对类别
/// </summary>
public enum SMPieceCategory
{
    Beam,    // 横枋-立柱（配对1）
    Short,   // 短枋-短枋（配对2）
    Corner   // 转角件-转角件（配对3）
}

/// <summary>
/// 单个木料数据
/// </summary>
[System.Serializable]
public class SMPieceData
{
    public int id;                      // 唯一ID
    public SMPieceType type;            // 类型（榫头/卯眼）
    public SMPieceCategory category;    // 类别（横枋/短枋/转角）
    public Sprite sprite;               // 对应Sprite
    public string displayName;          // 显示名称

    public SMPieceData() { }

    public SMPieceData(int id, SMPieceType type, SMPieceCategory category, Sprite sprite, string name)
    {
        this.id = id;
        this.type = type;
        this.category = category;
        this.sprite = sprite;
        this.displayName = name;
    }
}