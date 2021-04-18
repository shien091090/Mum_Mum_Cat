//餌食事件(可丟進FodderBehavior的fodderEffectAction中觸發吃掉餌食時的事件)

/*
 * 使用方式 : 
 * 拉進fodderEffectAction後, 可依序追加事件
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FodderEvents : MonoBehaviour
{
    public string StateName { get; set; } //狀態

    public float Duration { get; set; } //持續時間

    public int AntagonismValue { get; set; } //拮抗值

    private ArrayList inputParameters = new ArrayList(); //傳入參數

    public void AddInput(string s) { inputParameters.Add(s); } //追加傳入變數(字串)
    public void AddInput(float f) { inputParameters.Add(f); } //追加傳入變數(浮點數)
    public void AddInput(int i) { inputParameters.Add(i); } //追加傳入變數(整數)
    public void AddInput(bool b) { inputParameters.Add(b); } //追加傳入變數(布林值)

    public void AddBuff(CatBehavior target)
    {
        CatStateNames state = (CatStateNames)System.Enum.Parse(typeof(CatStateNames), StateName);

        switch (inputParameters.Count)
        {
            case 0:
                target.AddState(state, Duration, AntagonismValue, null); //追加狀態
                break;

            case 1:
                target.AddState(state, Duration, AntagonismValue, inputParameters[0]); //追加狀態(傳入參數數量=1)
                break;

            case 2:
                target.AddState(state, Duration, AntagonismValue, inputParameters[0], inputParameters[1]); //追加狀態(傳入參數數量=2)
                break;

            case 3:
                target.AddState(state, Duration, AntagonismValue, inputParameters[0], inputParameters[1], inputParameters[2]); //追加狀態(傳入參數數量=3)
                break;

            case 4:
                target.AddState(state, Duration, AntagonismValue, inputParameters[0], inputParameters[1], inputParameters[2], inputParameters[3]); //追加狀態(傳入參數數量=4)
                break;
        }

        //清空參數
        Duration = 0;
        AntagonismValue = 0;
        inputParameters = new ArrayList();
    }

    //針對CatState賦予狀態時的字串拆解分析(應用於組件Event的輸入值, 因Event無法輸入多參數)
    private static void StateStringAnalyze(string content, out CatStateNames catState, out float time, out int antagonismValue)
    {
        string _content = content; //Copy
        List<string> result = new List<string>(); //輸出結果

        List<string> itemString = new List<string>() { "s:", "t:", "a:" }; //分析項目列表
        string split_s = ","; //分隔符號

        for (int i = 0; i < itemString.Count; i++)
        {
            int itemIndex = content.IndexOf(itemString[i]); //項目所在位置(Index)
            int splitIndex = content.IndexOf(split_s); //分隔符號所在位置(index)
            string subString = content.Substring(itemIndex + itemString[i].Length, ( splitIndex < 0 ? content.Length : splitIndex ) - ( itemIndex + itemString[i].Length )); //取出項目與分隔符號間的字串

            result.Add(subString); //將其加入分析結果

            //Debug.Log("Replace預備字串 : " + itemString[i] + subString + split_s);
            content = content.Replace(itemString[i] + subString + split_s, string.Empty); //取出後, 從原字串拔除並繼續分析
            //Debug.Log("Replace結果 : " + content);
        }

        try
        {
            catState = (CatStateNames)System.Enum.Parse(typeof(CatStateNames), result[0]);
            time = float.Parse(result[1]);
            antagonismValue = int.Parse(result[2]);
            return;
        }
        catch (System.Exception)
        {
            Debug.Log("[ERROR]字串轉換失敗");
        }

        //不運行
        catState = CatStateNames.行走;
        time = 0;
        antagonismValue = 0;
    }
}
