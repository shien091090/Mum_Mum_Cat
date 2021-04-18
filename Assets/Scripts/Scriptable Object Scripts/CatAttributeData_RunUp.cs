using UnityEngine;

//狀態屬性(RunUp)
[CreateAssetMenu(fileName = "new_CatAttribute_RunUp", menuName = "CatAttribute/Create CatAttribute(RunUp)", order = 3)]
public class CatAttributeData_RunUp : CatAttribute
{
    public float coolDownTime; //跳躍冷卻時間
    public float cdTimeOscillation; //冷卻時間隨機震盪幅度(1=100%) ※計算正負幅度
    public float forceConversion; //跳躍力道(AddForce參數)➡實際速度的換算比值
    public int recordListLength; //紀錄列表長度(越短則貓咪改變跳躍模式時適應的時間越短)
    public float recordFrequency; //紀錄頻率(數值越低代表貓咪的反應越快)
}