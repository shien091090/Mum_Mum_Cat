//[貓行為狀態包]害怕狀態

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_Scared : AbsCatState
{
    public Transform runOutTarget; //逃離對象
    public float s_moveSpeed; //移動速度
    public float s_randomJumpFreq; //隨機跳躍頻率(0=不跳躍 / -1=僅撞牆時跳躍 / >0 隨機跳躍(秒))

    private float timer; //計時器

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.害怕;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[2];

        antagonismStates[0].antagonismState = CatStateNames.行走;
        antagonismStates[0].antagonismType = AntagonismType.凍結;

        antagonismStates[1].antagonismState = CatStateNames.助跑起跳;
        antagonismStates[1].antagonismType = AntagonismType.凍結;

        //設定拮抗強度
        this.antagonismValue = a;

        //設定參數
        CatAttributeData_Scared _att = (CatAttributeData_Scared)parentCat.Dict_attributeSetting[CatStateNames.害怕];
        s_moveSpeed = _att.moveSpeed; //設定速度
        s_randomJumpFreq = _att.randomJumpFreq; //隨機跳躍頻率
        runOutTarget = (Transform)etc[0]; //設定逃離對象
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart");
        parentCat.SetEmotion(EmotionType.害怕, true); //開啟"害怕"表情
        UIManager.Instance.JumpButtonScript.isCdTimeWorking = false; //凍結跳躍按鈕CD條
    }

    //行為實作
    public override void Action()
    {
        //Debug.Log("害怕");

        MoveDirection direction; //逃離方向
        Vector2 offset; //位移量

        if (runOutTarget.transform.position.x >= parentCat.transform.position.x) direction = MoveDirection.往左; //若逃離對象在貓咪的右邊, 則往左逃離
        else direction = MoveDirection.往右; //若逃離對象在貓咪的左邊, 則往右逃離

        parentCat.Move(direction, s_moveSpeed, null); //調用貓咪自帶的移動演算法

        if (s_randomJumpFreq == -1) //撞到牆時跳躍
        {
            if (parentCat.isContactWall) parentCat.AddState(CatStateNames.跳躍, -1, 10, null);
        }
        else if (s_randomJumpFreq > 0) //固定頻率跳躍
        {
            timer += Time.deltaTime; //計時器推進
            if(timer >= s_randomJumpFreq) //到達頻率時間
            {
                parentCat.AddState(CatStateNames.跳躍, -1, 10, null);
                timer = 0;
            }
        }
        //if (!isCollider) parentCat.transform.Translate(offset);
        //else parentCat.AddState(CatStateNames.跳躍, -1, 10, null); //如果撞牆的會一直跳跳
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        Debug.Log("[CatState]害怕暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {
        parentCat.SetEmotion(EmotionType.害怕, false); //關閉"害怕"表情
        UIManager.Instance.JumpButtonScript.isCdTimeWorking = true; //解除凍結跳躍按鈕CD條
    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        //Debug.Log("覆蓋狀態 - 害怕");

        runOutTarget = (Transform)etc[0]; //設定逃離對象
        timer = 0; //重置計時器

        ActionStart();
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
