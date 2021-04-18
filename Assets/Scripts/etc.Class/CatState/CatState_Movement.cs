//[貓行為狀態包]平行移動

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_Movement : AbsCatState
{
    public float s_moveSpeed; //移動速度
    public AnimationCurve s_moveCurve; //加速度變化曲線

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.行走;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[0];

        //設定拮抗強度
        this.antagonismValue = a;

        //設定初始化參數
        s_moveSpeed = parentCat.catAttribute_base.baseSpeed; //設定移動速度
        s_moveCurve = parentCat.catAttribute_base.baseMoveCurve; //設定加速度變化曲線
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart - 追蹤");
    }

    //行為實作
    public override void Action()
    {
        parentCat.Move(s_moveSpeed, s_moveCurve); //調用貓咪自帶的移動演算法
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        //Debug.Log("[CatState]行走暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {
        //Debug.Log("ActionOver - 追蹤");
    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        Debug.Log("覆蓋狀態 - 行走");
    }

    //取得數據(State相互取得數據的中介方法)
    public override object GetDate(string dataName)
    {
        switch (dataName)
        {
            case "MoveCurve":
                return s_moveCurve;

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
