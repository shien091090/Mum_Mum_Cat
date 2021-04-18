using UnityEngine;

//狀態屬性(Trace)
[CreateAssetMenu(fileName = "new_CatAttribute_Trace", menuName = "CatAttribute/Create CatAttribute(Trace)", order = 4)]
public class CatAttributeData_Trace : CatAttribute
{
    public float traceFrequency; //飼料追蹤頻率
}
