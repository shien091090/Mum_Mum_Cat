//控制遊戲從開始、中斷、關卡更換監聽到結束的流程
//[Partial]監聽事件觸發時機

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//事件設定
public class EventSetting
{
    public string StageIndex { private set; get; } //關卡數
    public StageEventType StageEventType { private set; get; } //指定事件
    public EventConditionType ConditionType { private set; get; } //事件觸發條件
    public ArrayList InputValues { private set; get; } //傳入值(ArrayList)

    public EventSetting(string _stageIndex, StageEventType _stageEvent, EventConditionType _condition, params object[] _values)
    {
        InputValues = new ArrayList();

        StageIndex = _stageIndex;
        StageEventType = _stageEvent;
        ConditionType = _condition;

        foreach (object item in _values)
        {
            InputValues.Add(item);
        }
    }

}

public partial class GameController : MonoBehaviour
{
    public List<EventSetting> gameEventList; //遊戲事件設定列表
    private float freqTimer; //頻率計時器

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //設定事件觸發條件
    private void EventConditionsInitialize()
    {
        gameEventList = new List<EventSetting>(); //遊戲事件設定列表

        freqTimer = 0;

        gameEventList.Add(new EventSetting("1", StageEventType.watatu君現身, EventConditionType.總遊戲時間, 15f));
        gameEventList.Add(new EventSetting("1", StageEventType.watatu君現身, EventConditionType.每隔定量遊戲時間, 60f));

        //for (int i = 0; i < gameEventList.Count; i++)
        //{
        //    Debug.Log(string.Format("[事件{0}] [第{1}關] 事件名 : {2} / 條件 : {3}", i, gameEventList[i].StageIndex, gameEventList[i].StageEventType, gameEventList[i].ConditionType));
        //    for (int j = 0; j < gameEventList[i].InputValues.Count; j++)
        //    {
        //        Debug.Log(string.Format("條件[{0}] : {1}", j, gameEventList[i].InputValues[j]));
        //    }
        //}

        SetEventListener(nowStage); //設定事件監聽者
    }

    //設定事件監聽者
    private void SetEventListener(StageAttributeData attribute)
    {
        List<EventSetting> listenTargetList = new List<EventSetting>(); //監聽對象清單

        for (int i = 0; i < gameEventList.Count; i++)
        {
            if (gameEventList[i].StageIndex == attribute.stageIndex) listenTargetList.Add(gameEventList[i]); //核對目前關卡數與設定事件關卡數是否吻合, 若吻合則加入監聽對象清單
        }

        if (listenTargetList.Count == 0) return; //若此關卡無設定事件, 直接結束程序

        StartCoroutine(EventTriggerListen(listenTargetList)); //針對目前關卡開始監聽事件觸發條件
    }

    //事件觸發監聽
    private IEnumerator EventTriggerListen(List<EventSetting> eventSettings)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < eventSettings.Count; i++)
            {
                switch (eventSettings[i].ConditionType)
                {
                    case EventConditionType.每隔定量遊戲時間:

                        if (GameTime - freqTimer >= (float)eventSettings[i].InputValues[0])
                        {
                            freqTimer = GameTime;
                            StageEventController.Instance.EventStart(eventSettings[i].StageEventType); //事件觸發
                        }

                        break;

                    case EventConditionType.總遊戲時間:

                        if (GameTime >= (float)eventSettings[i].InputValues[0])
                        {
                            StageEventController.Instance.EventStart(eventSettings[i].StageEventType); //事件觸發
                            eventSettings.RemoveAt(i); //從監聽列表中銷毀指定事件, 事件觸發後不會觸發第二次
                            goto JumpOut;
                        }

                        break;

                    case EventConditionType.此關卡經過時間:
                        break;

                    case EventConditionType.總累計分數:
                        break;

                    case EventConditionType.此關卡分數進度百分比:

                        float progressRate; //分數進度百分比
                        progressRate = stageBoard.thresholdSlider.value;

                        if (progressRate >= (float)eventSettings[i].InputValues[0]) //條件達成時
                        {
                            StageEventController.Instance.EventStart(eventSettings[i].StageEventType); //事件觸發
                            eventSettings.RemoveAt(i); //從監聽列表中銷毀指定事件, 事件觸發後不會觸發第二次
                            goto JumpOut;
                        }

                        break;

                    case EventConditionType.此關卡累計分數:
                        break;
                }
            }

JumpOut:

            if (eventSettings.Count == 0) break; //若監聽列表為空, 結束監聽程序
        }

        //Debug.Log("事件監聽結束");
    }
}
