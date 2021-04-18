//貓行為實作腳本
//[partial]各種CatState(行為狀態)的處理方法都放在這邊

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CatBehavior : MonoBehaviour
{
    //賦予貓咪狀態
    //stateName = 狀態名稱
    //t = 持續時間
    //a = 拮抗強度
    //etc = 其他參數(依照各狀態腳本自定義)
    public void AddState(CatStateNames stateName, float t, int a, params object[] etc)
    {
        if (GameController.Instance.gameOver) return; //狀態鎖上鎖時, 無法賦予任何狀態

        //判斷貓咪是否已經存在此狀態
        for (int i = 0; i < activeCatStates.Count; i++)
        {
            if (activeCatStates[i].GetStateName == stateName) //若有存在, 則執行重複狀態程序(程序內容在各CatState中定義)
            {
                activeCatStates[i].DuplicateSet(t, true, etc);
                return;
            }
        }

        AbsCatState script = null; //貓咪狀態腳本

        switch (stateName) //貓咪狀態腳本賦值
        {
            case CatStateNames.行走:
                script = this.gameObject.AddComponent<CatState_Movement>();
                break;

            case CatStateNames.跳躍:
                script = this.gameObject.AddComponent<CatState_Jump>();
                break;

            case CatStateNames.跌倒:
                script = this.gameObject.AddComponent<CatState_FallDown>();
                break;

            case CatStateNames.助跑起跳:
                //script = this.gameObject.AddComponent<CatState_RunUp>();
                break;

            case CatStateNames.害怕:
                script = this.gameObject.AddComponent<CatState_Scared>();
                break;

            case CatStateNames.速度變化:
                script = this.gameObject.AddComponent<CatState_SpeedChange>();
                break;

            case CatStateNames.追蹤:
                script = this.gameObject.AddComponent<CatState_Trace>();
                break;

            default:
                break;
        }
        script.BuildInfo(a, etc); //狀態腳本建立資訊初始化

        //拮抗處理 => 欲加進來的狀態會被如何對待 如果被銷毀則不會有後續 若沒有被銷毀則加進狀態佇列中
        Antagonism(ref script, out bool isDestroyed);

        if (isDestroyed) //若拮抗處理判斷為未被銷毀
        {
            activeCatStates.Add(script); //加入激活中狀態
            AntagonismTraverse(script); //遍歷激活中狀態重新判斷拮抗結果
            script.StateInitialize(t); //狀態執行初始化
        }
        else DestroyState(new List<AbsCatState>() { script }); //若拮抗處理判斷為會被銷毀
    }

    //銷毀指定狀態(可複數)
    //(多載1/2) 傳入AbsCatState物件
    public void DestroyState(List<AbsCatState> states)
    {
        List<CatStateNames> stateName = new List<CatStateNames>();

        for (int i = 0; i < states.Count; i++)
        {
            stateName.Add(states[i].GetStateName);
        }

        DestroyState(stateName);
    }

    //銷毀指定狀態(可複數)
    //(多載2/2) 傳入CatStateNames列舉值
    public void DestroyState(List<CatStateNames> stateName)
    {
        bool isAcitveState = false; //是否原本有在激活中狀態佇列中?
        List<AbsCatState> stateObj = new List<AbsCatState>();

        while (stateName.Count > 0)
        {
            for (int i = 0; i < activeCatStates.Count; i++) //遍歷激活中狀態
            {
                if (activeCatStates[i].GetStateName == stateName[0]) //欲銷毀狀態若有在激活中狀態中
                {
                    stateObj.Add(activeCatStates[i]); //加入銷毀清單
                    activeCatStates.RemoveAt(i); //從激活中狀態列表移除
                    isAcitveState = true; //由於會影響激活中狀態, 故更動後要再重新檢查一次狀態拮抗
                    break;
                }
            }

            if (isAcitveState) AntagonismTraverse(); //逐一對狀態佇列再一次執行拮抗處理

            stateName.RemoveAt(0); //已判斷stateName第一項, 抽除後繼續下一輪判斷
        }

        //for (int i = 0; i < stateObj.Count; i++)
        //{
        //    Debug.Log(string.Format("[{0}] : {1}", i, stateObj[i].GetStateName)) ;
        //}

        _DestroyState(stateObj); //銷毀狀態
    }

    //[private]銷毀指定狀態
    private void _DestroyState(List<AbsCatState> states)
    {
        if (states == null || states.Count == 0) return;

        GameController.Instance.ChangePlayingStateEvent -= states[0].ChangePlayingState; //遊戲進行狀態變更事件取消訂閱

        states[0].ActionOver(); //銷毀前執行方法
        Destroy(states[0]); //銷毀

        if (states.Count > 1)
        {
            states.RemoveAt(0);
            _DestroyState(states);
        }
    }

    //只留下指定狀態(可複數)
    public void RemainState(List<CatStateNames> states)
    {
        List<CatStateNames> destroyItems = new List<CatStateNames>();

        for (int j = 0; j < activeCatStates.Count; j++) //遍歷激活中狀態列表
        {
            bool match = false; //是否為指定狀態
            for (int i = 0; i < states.Count; i++) //遍歷指定的狀態
            {
                if (activeCatStates[j].GetStateName == states[i]) match = true; //判斷結果為吻合
            }

            if (!match) destroyItems.Add(activeCatStates[j].GetStateName); //若狀態判斷為"不是指定要留下的"狀態, 加入待銷毀狀態
        }

        //for (int i = 0; i < destroyItems.Count; i++)
        //{
        //    Debug.Log(string.Format("銷毀狀態[{0}] : {1}", i, destroyItems[i]));
        //}

        DestroyState(destroyItems); //銷毀狀態
    }

    //取得指定CatState內的某項數據(State相互取得數據的中介方法)
    public object GetStateDate(CatStateNames catState, string dataName)
    {
        int index = -1; //指定狀態所在索引
        bool existence = StateExistenceTest(catState, out index); //測試狀態是否存在於已激活狀態列表中

        if (!existence) return null; //若沒有指定狀態存在, 直接結束程序

        return activeCatStates[index].GetDate(dataName); //從指定的CatState中依據dataName取得指定數據並返回
    }

    //指定CatState內的某項數據(State相互干涉的中介方法)
    public void SetStateDate(CatStateNames catState, string dataName, object value)
    {
        int index = -1; //指定狀態所在索引
        bool existence = StateExistenceTest(catState, out index); //測試狀態是否存在於已激活狀態列表中

        if (!existence) return; //若沒有指定狀態存在, 直接結束程序

        activeCatStates[index].SetData(dataName, value); //設定數據
    }

    //測試已激活狀態列表中是否有指定狀態
    public bool StateExistenceTest(CatStateNames catState, out int index)
    {
        bool b = false;
        int _index = -1;

        for (int i = 0; i < activeCatStates.Count; i++) //偵測已激活狀態中是否有指定狀態
        {
            if (activeCatStates[i].GetStateName == catState)
            {
                b = true;
                _index = i;
                break;
            }
        }

        index = _index; //返回所在索引

        return b;
    }

    //拮抗處理
    //script : 受制處理的狀態腳本
    //[output] isDestroyed : 判斷為被銷毀時回傳false
    private void Antagonism(ref AbsCatState script, out bool isDestroyed)
    {
        if (activeCatStates.Count == 0) //貓咪身上無任何狀態時, 不發生拮抗
        {
            isDestroyed = true;
            return;
        }

        script.AntagonismAffect(AntagonismType.無); //預設受制狀態為"無"
        int a_receive = 0; //紀錄拮抗受制時的拮抗強度

        //遍歷所有激活中狀態
        for (int i = 0; i < activeCatStates.Count; i++)
        {
            if (activeCatStates[i].GetStateName == script.GetStateName) continue; //若被判斷的狀態名在激活中狀態中, 則略過不判斷拮抗(自己不跟自己拮抗)

            for (int j = 0; j < activeCatStates[i].GetAntagonismStates.Length; j++) //遍歷所有激活中狀態的拮抗對象狀態
            {
                if (activeCatStates[i].GetAntagonismStates[j].antagonismState == script.GetStateName && //激活中狀態中的拮抗對象狀態為this
                    activeCatStates[i].GetAntagonismValue > script.GetAntagonismValue)//激活中狀態中的拮抗強度大於this
                {
                    if (activeCatStates[i].GetAntagonismStates[j].antagonismType == AntagonismType.銷毀) //受制狀態為銷毀時直接返回結果並結束程序
                    {
                        isDestroyed = false;
                        return;
                    }

                    if (activeCatStates[i].GetAntagonismValue > a_receive) //同時有多個拮抗受制時, 相互比較其來源狀態的拮抗強度, 取最強者保留受制狀態
                    {
                        a_receive = activeCatStates[i].GetAntagonismValue;
                        script.AntagonismAffect(activeCatStates[i].GetAntagonismStates[j].antagonismType);
                    }
                }
            }
        }

        isDestroyed = true;
    }

    //遍歷激活中狀態重新判斷拮抗結果(多載1/2)
    private void AntagonismTraverse()
    {
        AntagonismTraverse(null);
    }

    //遍歷激活中狀態重新判斷拮抗結果(多載2/2)
    //passState = 略過不判斷的狀態
    private void AntagonismTraverse(AbsCatState passState)
    {
        List<AbsCatState> destroyStates = new List<AbsCatState>(); //待銷毀狀態

        for (int i = 0; i < activeCatStates.Count; i++)
        {
            if (passState != null)
            {
                if (activeCatStates[i].GetStateName == passState.GetStateName) continue; //若判斷為Pass狀態則直接continue
            }

            AbsCatState script = activeCatStates[i];
            Antagonism(ref script, out bool isDestroyed);
            if (!isDestroyed) destroyStates.Add(script);
        }

        DestroyState(destroyStates);
    }
}
