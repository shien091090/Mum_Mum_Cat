//關卡數據資料(ScriptableObject)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//餌食設定資訊
[System.Serializable]
public class FodderInfo
{
    public FodderType fodder; //餌食種類
    public int buildFreq; //生成頻率權重
}

[CreateAssetMenu(fileName = "new_StageAttribute", menuName = "StageAttribute/Create StageAttribute", order = 1)]
public class StageAttributeData : ScriptableObject
{
    public string stageIndex; //關卡數
    public string sceneName; //場景名稱
    public string bgmName; //音樂名稱
    public int thresholdScore; //門檻分數(由起始分數或上一關終點分數再往上加)
    public List<FodderInfo> stateFodders; //關卡中可能出現的餌食資訊
    public StageAttributeData nextStage; //下一關
    public float hpDamageSpeed; //損血速度(每秒)
}
