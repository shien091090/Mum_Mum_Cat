using UnityEngine;

//狀態屬性(Scared)
[CreateAssetMenu(fileName = "new_CatAttribute_Scared", menuName = "CatAttribute/Create CatAttribute(Scared)", order = 6)]
public class CatAttributeData_Scared : CatAttribute
{
    public float moveSpeed; //速度
    public float randomJumpFreq; //隨機跳躍頻率(0=不跳躍 / -1=僅撞牆時跳躍 / >0 隨機跳躍(秒))
}