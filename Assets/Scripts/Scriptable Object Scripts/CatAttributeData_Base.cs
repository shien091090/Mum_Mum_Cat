using UnityEngine;

//基本屬性
[CreateAssetMenu(fileName = "new_CatAttribute_Base", menuName = "CatAttribute/Create CatAttribute(Base)", order = 1)]
public class CatAttributeData_Base : ScriptableObject
{
    public float baseSpeed; //基礎移動速度
    public AnimationCurve baseMoveCurve; //基礎加速度變化曲線(縱軸:加速度0~1(1=100%加速度) / 橫軸:餌食距離0~1(0=餌食位置、1=螢幕寬度))
}