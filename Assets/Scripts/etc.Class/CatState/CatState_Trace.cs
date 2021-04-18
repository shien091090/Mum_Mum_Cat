using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_Trace : AbsCatState
{
    private float s_traceFrequency; //飼料追蹤頻率
    private float timer; //計時器

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.追蹤;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[0];

        //設定拮抗強度
        this.antagonismValue = a;

        //設定參考物件或參數
        CatAttributeData_Trace _att = (CatAttributeData_Trace)parentCat.Dict_attributeSetting[CatStateNames.追蹤];
        s_traceFrequency = _att.traceFrequency;
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart - 追蹤");

        timer = 0; //計時器歸零
    }

    //行為實作
    public override void Action()
    {
        //Debug.Log("Action - 追蹤");

        if (!GameController.Instance.GetPlayingState) return; //若遊戲為暫停則結束程序

        if (FodderController.Instance.GetFallingFodders.Count <= 1) //目前只有1個餌食掉落或貓咪靜止狀態時, 將Timer設定為待觸發狀態使得下一個丟出的餌食會立即吸引貓咪
        {
            timer = s_traceFrequency;
        }
        else
        {
            timer += Time.deltaTime; //計時器運行
        }

        if (FodderController.Instance.GetFallingFodders.Count > 0 && timer >= s_traceFrequency) //達到頻率門檻時執行
        {
            timer = 0; //計時器歸零

            SetFollowTarget();
        }

        if (parentCat.tracingPosition != null && !parentCat.tracingPosition.gameObject.activeSelf) SetFollowTarget(); //若追蹤物件已消失, 則直接追蹤下一個物件
    }

    //找到最近餌食
    private void SetFollowTarget()
    {
        if (GameController.Instance.gameOver) //遊戲結束
        {
            if (parentCat.tracingPosition != null) parentCat.tracingPosition = null; //清空追蹤物件參考
            return;
        }

        float distance = float.MaxValue; //初始化最近距離
        bool flavoured = false; //香味效果
        int targetIndex = -1;

        for (int i = 0; i < FodderController.Instance.GetFallingFodders.Count; i++) //遍歷所有落下中的餌食
        {
            //取得最近距離飼料, 存下飼料物件的索引
            float d = Vector3.Distance(this.transform.localPosition, FodderController.Instance.GetFallingFodders[i].localPosition);

            //取得餌食特殊標籤
            FodderSpecialTag tag = FodderController.Instance.GetFallingFodders[i].GetComponent<FodderBehavior>().GetAttribute.specialTag;

            if (tag == FodderSpecialTag.香味) //香味效果(強制吸引貓咪)
            {
                if (d < distance || !flavoured)
                {
                    flavoured = true;
                    distance = d;
                    targetIndex = i;
                }
            }
            else if (d < distance && !flavoured)
            {
                distance = d;
                targetIndex = i;
            }
        }

        if (targetIndex == -1) parentCat.tracingPosition = null;
        else parentCat.tracingPosition = FodderController.Instance.GetFallingFodders[targetIndex];

        //[測試]最接近飼料顯示標記
        for (int i = 0; i < FodderController.Instance.GetFallingFodders.Count; i++)
        {
            bool signActive = targetIndex == i;
            FodderController.Instance.GetFallingFodders[i].gameObject.GetComponent<FodderBehavior>().testSign.SetActive(signActive);
        }
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        //Debug.Log("[CatState]追蹤暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {
        //Debug.Log("ActionOver - 追蹤");
    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        Debug.Log("覆蓋狀態 - 追蹤");
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
