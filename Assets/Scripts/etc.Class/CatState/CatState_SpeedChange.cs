//[貓行為狀態包]速度變化

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_SpeedChange : AbsCatState
{
    private float originSpeedRatio; //原速度變化值
    private AnimationCurve s_variationCurve; //變化曲線

    private float timer; //計時器

    enum Type
    {
        加速, 緩速
    }
    private Type type; //速度變化類型(加速or緩速)

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.速度變化;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[0];

        //設定拮抗強度
        this.antagonismValue = a;

        //設定參數
        originSpeedRatio = parentCat.speedRatio; //儲存原變化值

        CatAttributeData_SpeedChange _att = (CatAttributeData_SpeedChange)parentCat.Dict_attributeSetting[CatStateNames.速度變化];
        type = (bool)etc[0]? Type.加速 : Type.緩速; //true = 加速 / false = 緩速
        if (type == Type.加速) s_variationCurve = _att.speedUpVariation; //加速
        else s_variationCurve = _att.speedDownVariation;//緩速
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart");
        timer = 0; //計時器歸零
        if (type == Type.緩速) parentCat.SetEmotion(EmotionType.緩速, true); //開啟"緩速"表情
    }

    //行為實作
    public override void Action()
    {
        float endingTime = s_variationCurve.keys[s_variationCurve.keys.Length - 1].time; //取得曲線的結束時間(最後時間點)

        if (timer >= endingTime) //超過最後時間點, 則固定取最後時間點的值
        {
            parentCat.speedRatio = s_variationCurve.Evaluate(endingTime);
            return;
        }

        parentCat.speedRatio = s_variationCurve.Evaluate(timer);

        timer += Time.deltaTime; //未超過最後時間點, 計時器推進
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state) { }

    //銷毀前結束事件
    public override void ActionOver()
    {
        if(type == Type.緩速) parentCat.SetEmotion(EmotionType.緩速, false); //關閉"緩速"表情

        parentCat.speedRatio = originSpeedRatio; //恢復原速度
    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        //Debug.Log("覆蓋狀態 - 速度變化");

        if (type == Type.緩速) parentCat.SetEmotion(EmotionType.緩速, false); //關閉"緩速"表情

        CatAttributeData_SpeedChange _att = (CatAttributeData_SpeedChange)parentCat.Dict_attributeSetting[CatStateNames.速度變化];
        type = (bool)etc[0] ? Type.加速 : Type.緩速; //true = 加速 / false = 緩速
        if (type == Type.加速) s_variationCurve = _att.speedUpVariation; //加速
        else s_variationCurve = _att.speedDownVariation;//緩速

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
