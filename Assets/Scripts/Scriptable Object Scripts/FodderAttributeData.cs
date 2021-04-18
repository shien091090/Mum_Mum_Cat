//餌食數據資料(ScriptableObject)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FodderEffectAction : UnityEngine.Events.UnityEvent<CatBehavior>
{

}

[CreateAssetMenu(fileName = "new_FodderAttribute", menuName = "FodderAttribute/Create FodderAttribute", order = 1)]
public class FodderAttributeData : ScriptableObject
{
    public FodderType fodderType; //餌食種類
    public FodderCollisionType fodderCollisionType; //餌食觸碰地面效果
    public Sprite imageSprite; //餌食圖像
    public Sprite frameLineSprite; //圖像對列框圖像
    public Vector2 colliderSize; //碰撞體大小
    public AnimationCurve fallCurve; //墜落曲線(X軸 = 時間, Y軸 = 0~1(0為地板,1為初始位置))
    public AnimationCurve shakeCurve; //震盪(左右方向)曲線(X軸 = 時間, Y軸 = -1~1(-1=畫面左端, 1=畫面右端))
    public AnimationCurve rotationCurve; //旋轉曲線(X軸 = 時間, Y軸 = 旋轉角度(歐拉角))
    public int score; //分數
    public float hpEffect; //血量影響值(正值=補血、負值=扣血)
    public FodderSpecialTag specialTag; //特殊標籤
    public FodderEffectAction fodderEffectAction; //餌食特殊效果
}
