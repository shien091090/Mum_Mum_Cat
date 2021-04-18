using UnityEngine;

//狀態屬性(SpeedChange)
[CreateAssetMenu(fileName = "new_CatAttribute_SpeedChange", menuName = "CatAttribute/Create CatAttribute(SpeedChange)", order = 5)]
public class CatAttributeData_SpeedChange : CatAttribute
{
    public AnimationCurve speedUpVariation; //加速變化曲線
    public AnimationCurve speedDownVariation; //緩速變化曲線
}
