using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_FallDown : AbsCatState
{
    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.跌倒;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[1];

        antagonismStates[0].antagonismState = CatStateNames.行走;
        antagonismStates[0].antagonismType = AntagonismType.凍結;

        //設定拮抗強度
        this.antagonismValue = a;
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart");
    }

    //行為實作
    public override void Action()
    {
        Debug.Log("跌倒");
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        Debug.Log("[CatState]跌倒暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {

    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        Debug.Log("覆蓋狀態 - 跌倒");
    }

    //取得數據(State相互取得數據的中介方法)
    public override object GetDate(string dataName)
    {
        switch (dataName)
        {
            default:
                return null;
        }
    }

    //設定數據(State相互干涉的中介方法)
    public override void SetData(string dataName, object value)
    {
        switch (dataName)
        {
            default:
                break;
        }
    }
}
