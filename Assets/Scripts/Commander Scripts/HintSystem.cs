//提示系統。由Game Controller定義哪邊需要跳出提示，由提示系統實作提示效果

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 提示系統。由Game Controller定義哪邊需要跳出提示，由提示系統實作提示效果
/// </summary>
public partial class HintSystem : MonoBehaviour
{
    private static HintSystem _instance; //單例模式物件
    public static HintSystem Instance { get { return _instance; } } //取得單例物件

    [Header("參考組件(UI)")]
    public Image curtain; //提示時的背景布幕
    public Canvas spotlight; //聚光燈(Canvas) ※這邊的聚光燈效果使用了外部的ReverseMaskComponent
    [Header("參考腳本")]
    public HintTextBehavior textScript; //文字物件的行為腳本(HintTextBehavior)
    [Header("參數")]
    [SerializeField]
    private bool isHinting = false; //提示進行中與否
    public float forcedWatingTime; //略過提示前的強制等待時間

    public bool GetHintingState { get { return isHinting; } } //取得提示進行中狀態

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設置單例物件

        //建立提示點查找庫
        for (int i = 0; i < hintPointList.Count; i++)
        {
            hintPointDic.Add(hintPointList[i].hintPointName, hintPointList[i].hintPos);
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    // 提示顯示(多載1/3)
    //輸入文字內容(content), 聚光燈位置設定在已定義好的hintPoint
    public void Hint(string content, string hintPointName)
    {
        isHinting = true; //提示進行開始

        spotlight.transform.localPosition = GetHintPointPos(hintPointName); //設定聚光燈位置
        if(!curtain.gameObject.activeSelf) curtain.gameObject.SetActive(true); //布幕物件顯示
        if (!spotlight.gameObject.activeSelf) spotlight.gameObject.SetActive(true); //聚光燈物件顯示
        textScript.SetTextContent(content, true); //套用文字

        StartCoroutine(WaitingForKey()); //等待玩家按下按鍵, 結束提示
    }

    // 提示顯示(多載2/3)
    //輸入文字內容(content), 聚光燈位置設定於pos
    public void Hint(string content, Vector3 localPos)
    {
        isHinting = true; //提示進行開始

        spotlight.transform.localPosition = localPos; //設定聚光燈位置
        if (!curtain.gameObject.activeSelf) curtain.gameObject.SetActive(true); //布幕物件顯示
        if (!spotlight.gameObject.activeSelf) spotlight.gameObject.SetActive(true); //聚光燈物件顯示
        textScript.SetTextContent(content, true); //套用文字

        StartCoroutine(WaitingForKey()); //等待玩家按下按鍵, 結束提示
    }

    // 提示顯示(多載3/3)
    //輸入文字內容(content), 無聚光燈
    public void Hint(string content)
    {
        isHinting = true; //提示進行開始

        if (!curtain.gameObject.activeSelf) curtain.gameObject.SetActive(true); //布幕物件顯示
        textScript.SetTextContent(content, false); //套用文字

        StartCoroutine(WaitingForKey()); //等待玩家按下按鍵, 結束提示
    }

    private IEnumerator WaitingForKey()
    {
        PlayerController.Instance.canOperated = false; //玩家不可操作

        yield return new WaitForSeconds(forcedWatingTime); //強制等待時間

        yield return new WaitUntil( ()=> Input.GetMouseButtonDown(0) );

        if (curtain.gameObject.activeSelf) curtain.gameObject.SetActive(false); //布幕物件隱藏
        if (spotlight.gameObject.activeSelf) spotlight.gameObject.SetActive(false); //聚光燈物件隱藏
        if (textScript.gameObject.activeSelf) textScript.gameObject.SetActive(false); //文字物件隱藏

        yield return null;

        isHinting = false;
        PlayerController.Instance.canOperated = true; //玩家可操作
    }

}
