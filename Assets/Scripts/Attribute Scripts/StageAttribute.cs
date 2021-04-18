//在關卡物件中掛載此腳本，可快速設定關卡中的各參數。其他腳本得以從此腳本中取得關卡資訊

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 在關卡物件中掛載此腳本，可快速設定關卡中的各參數。其他腳本得以從此腳本中取得關卡資訊
/// </summary>
public class StageAttribute : MonoBehaviour
{
    [Header("可自訂參數")]
    public string stageIndex; //關卡數
    public string sceneName; //場景名稱
    public string bgmName; //音樂名稱
    public int thresholdScore; //門檻分數(由起始分數或上一關終點分數再往上加)
    public List<FodderInfo> stateFodders; //關卡中可能出現的餌食資訊
    public StageAttribute nextStage; //下一關
    public float hpDamageSpeed; //損血速度(每秒)
}
