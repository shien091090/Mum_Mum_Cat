//HintSystem的Partial部分
//在調用HintSystem的Hint(彈出提示)方法時, 可以輸入已經定義好的提示點字串例如"貓", 返回相對應的聚光燈位置
//實作方式是透過HintPointPack類別儲存資訊(在Unity Inspector上可編輯), 在遊戲初始化時建立Dictionary將類別的字串和對應位置儲存起來已供Hint方法方便調用

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//儲存提示點與對應位置的類別
[System.Serializable]
public class HintPointPack
{
    public string hintPointName; //提示點名稱
    public Transform hintPos; //提示點位置(Transform)
}

public partial class HintSystem : MonoBehaviour
{
    public List<HintPointPack> hintPointList = new List<HintPointPack>(); //提示點資訊集
    private Dictionary<string, Transform> hintPointDic = new Dictionary<string, Transform>(); //提示點查找庫

    //取得提示點位置
    private Vector3 GetHintPointPos(string s)
    {
        if (hintPointDic.ContainsKey(s))
        {
            return hintPointDic[s].localPosition;
        }

        return Vector3.zero; //若查無key值則返回{0,0,0}
    }
}
