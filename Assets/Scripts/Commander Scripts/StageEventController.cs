//關卡事件控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEventController : MonoBehaviour
{
    private static StageEventController _instance; //單例物件
    public static StageEventController Instance { get { return _instance; } } //取得單例物件

    [Header("參考物件")]
    public WataruKun wataruKunScript; //わたる君腳本

    [Header("遊戲進行狀態")]
    [SerializeField]
    private List<StageEventType> executingEventList = new List<StageEventType>(); //進行中事件

    public event System.Action ClearEvent; //清除事件

    //------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this;
    }

    //------------------------------------------------------------------------------------------------------------------

    //開始指定事件
    public void EventStart(StageEventType e)
    {
        for (int i = 0; i < executingEventList.Count; i++) //若目前指定事件正在發生中, 則直接結束程序
        {
            if (executingEventList[i] == e) return;
        }

        switch (e)
        {
            case StageEventType.watatu君現身:
                //Debug.Log("watatu君現身");

                executingEventList.Add(e);
                wataruKunScript.WataruKunAppear();

                break;

            case StageEventType.雲霄飛車:
                break;

            case StageEventType.下雨天:
                break;

            case StageEventType.小狗搶食物:
                break;
        }
    }

    //指定事件結束
    public void EventOver(StageEventType e)
    {
        executingEventList.Remove(e); //從事件執行列表中將指定事件抽除
    }

    //清空所有事件與相關物件
    public void ClearAllEvent()
    {
        executingEventList = new List<StageEventType>();
        
        ClearEvent?.Invoke(); //呼叫清空事件
    }
}
