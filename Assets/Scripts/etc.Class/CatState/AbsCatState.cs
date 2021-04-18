//貓咪狀態包的父抽象類別

/*
 * 增加新狀態步驟:
 * 1.在EnumClass的CatStateNames中增加新狀態的列舉值
 * 2.建立新腳本, 新的Class需繼承AbsCatState
 * 3.按照AbsCatState所規定寫出所有須override的method
 * 4.修改CatBehavior的AddState方法
 * 5.最重要的是, 請在BuildInfo方法(初始化)裡將stateName設定成新的列舉值
 */

/*
 * 拮抗值(Antagonism)筆記:
 * 走路(Movement) : 1
 * 助跑起跳(RunUp) : 1
 * 跳躍(Jump) : 10
 * 害怕(Scared) : 8
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbsCatState : MonoBehaviour
{
    [SerializeField]
    protected CatStateNames stateName; //狀態名稱
    public CatStateNames GetStateName { get { return stateName; } } //取得狀態名稱

    [SerializeField]
    protected AntagonismType antagonismReceive = AntagonismType.無; //拮抗受制狀態
    public AntagonismType GetAntagonismReceive { get { return antagonismReceive; } } //取得拮抗受制狀態

    [System.Serializable]
    public struct AntagonismStates //拮抗狀態列表(Struct)
    {
        public CatStateNames antagonismState; //拮抗對象狀態
        public AntagonismType antagonismType; //拮抗方式
    }
    protected AntagonismStates[] antagonismStates;
    public AntagonismStates[] GetAntagonismStates { get { return antagonismStates; } } //取得拮抗狀態列表

    [SerializeField]
    protected int antagonismValue; //拮抗強度
    public int GetAntagonismValue { get { return antagonismValue; } }

    [SerializeField]
    protected float duration; //持續時間

    protected CatBehavior parentCat; //父物件腳本

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        parentCat = this.gameObject.GetComponent<CatBehavior>(); //取得貓的父物件腳本
        GameController.Instance.ChangePlayingStateEvent += ChangePlayingState; //遊戲暫停事件追加
    }

    //[抽象方法]行為實作開始(開始時執行一次)
    abstract public void ActionStart();

    //[抽象方法]行為實作
    abstract public void Action();

    //[抽象方法]建立狀態資訊
    //a : 拮抗強度
    //etc : 其他自訂參數
    abstract public void BuildInfo(int a, params object[] etc);

    //[抽象方法]遊戲暫停時事件
    //[input]state : true:  進行 / false : 暫停
    abstract public void ChangePlayingState(bool state);

    //[抽象方法]取得數據(State相互取得數據的中介方法)
    //[input]dataName : 數據的變數名(可在CatState中定義)
    //[output] object : 指定變數名的數據
    abstract public object GetDate(string dataName);

    //[抽象方法]設定數據(State相互干涉的中介方法)
    //[input]dataName : 數據的變數名(可在CatState中定義) / value : 目標值
    abstract public void SetData(string dataName, object value);

    //[抽象方法]行為結束(銷毀前執行一次)
    abstract public void ActionOver();

    //[抽象方法]覆蓋狀態處理
    //[input] time : 持續時間 / isReset : true : 覆蓋 / false : 延長 / etc : 傳入參數
    abstract protected void CoverUpState(float time, bool isCoverUp, params object[] etc);

    //[預設方法]執行前初始化
    //t = 設定持續時間
    //a = 設定拮抗強度
    public void StateInitialize(float t)
    {
        this.duration = t;
        StartCoroutine(this.TimerStart());
    }

    //[預設方法]拮抗作用發生
    public void AntagonismAffect(AntagonismType type)
    {
        antagonismReceive = type;
    }

    //賦予相同狀態(重置或延長時間)
    public void DuplicateSet(float time, bool isCoverUp, params object[] etc)
    {
        if (isCoverUp) duration = time; //重置(true) : 重置作用時間
        else duration += time; //延長(false) : 延長作用時間

        CoverUpState(time, isCoverUp, etc); //特殊處理
    }

    //[預設方法]持續作用計時器(每一幀呼叫一次Action)
    public IEnumerator TimerStart()
    {
        bool actionStart = false;

        while (duration > 0 || duration < 0) //只有在計時器大於0(有限持續時間狀態)以及小於0(常駐狀態)時執行
        {
            yield return new WaitForEndOfFrame();

            if(GameController.Instance.GetPlayingState) //遊戲是否為進行中狀態
            {
                if (antagonismReceive != AntagonismType.凍結 && duration > 0) //計時器倒數
                {
                    duration = duration - Time.deltaTime < 0 ? 0 : duration - Time.deltaTime;
                }

                if (antagonismReceive == AntagonismType.無)
                {
                    if(!actionStart) //行為開始時執行一次ActionStart
                    {
                        ActionStart();
                        actionStart = true;
                    }

                    Action(); //呼叫Action(狀態行為實作程序)
                }
            }
        }

        parentCat.DestroyState(new List<AbsCatState>() { this }); //計時器倒數結束時, 銷毀狀態
    }
}
