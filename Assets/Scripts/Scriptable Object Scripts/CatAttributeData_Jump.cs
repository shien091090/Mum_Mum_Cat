using UnityEngine;

//狀態屬性(Jump)
[CreateAssetMenu(fileName = "new_CatAttribute_Jump", menuName = "CatAttribute/Create CatAttribute(Jump)", order = 2)]
public class CatAttributeData_Jump : CatAttribute
{
    public float jumpForce; //跳躍力道
}
