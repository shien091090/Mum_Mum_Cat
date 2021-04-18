using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

public class CatState_RunUp : AbsCatState
{
    private float s_coolDownTime; //跳躍冷卻時間
    private float s_cdTimeOscillation; //冷卻時間隨機震盪幅度(1=100%) ※計算正負幅度
    private float s_forceConversion; //跳躍力道(AddForce參數)➡實際速度的換算比值
    private int s_recordListLength; //紀錄列表長度(越短則貓咪改變跳躍模式時適應的時間越短)
    private float s_recordFrequency; //紀錄頻率(數值越低代表貓咪的反應越快)

    //private List<float[]> highestRecordList; //最高點Y軸位置與時間紀錄([0]=Y軸位置 / [1]=時間)
    private Queue<float[]> highestRecordQueue = new Queue<float[]>(); //最高點Y軸位置與時間紀錄([0]=Y軸位置 / [1]=時間)
    private float[] highestRecordAverage; //平均最高點位置與時間紀錄
    private float r_pos; //位置(X軸)紀錄
    private float r_speed; //速度紀錄
    [SerializeField]
    private float timer; //計時器
    [SerializeField]
    private float realCdTime; //實際冷卻時間

    public float temp_pos;
    public float temp_updatePos;
    public float temp_time;

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.助跑起跳;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[0];

        //設定拮抗強度
        this.antagonismValue = a;

        //設定初始化參數
        CatAttributeData_RunUp _att_runUp = (CatAttributeData_RunUp)parentCat.Dict_attributeSetting[CatStateNames.助跑起跳];
        CatAttributeData_Jump _att_jump = (CatAttributeData_Jump)parentCat.Dict_attributeSetting[CatStateNames.跳躍];
        s_coolDownTime = _att_runUp.coolDownTime; //設定跳躍冷卻時間
        s_cdTimeOscillation = _att_runUp.cdTimeOscillation; //設定冷卻時間隨機震盪幅度
        s_forceConversion = _att_runUp.forceConversion; //設定跳躍力道換算比
        s_recordListLength = _att_runUp.recordListLength; //設定記錄列表長度
        s_recordFrequency = _att_runUp.recordFrequency; //設定記錄頻率

        //設定最高點位置的時間
        highestRecordAverage = new float[2];
        highestRecordAverage[1] = ( _att_jump.jumpForce * s_forceConversion ) / Mathf.Abs(( Physics2D.gravity.y * GameController.c_gravityRatio )); //推算抵達最高高度時的時間
        highestRecordAverage[0] = ( ( _att_jump.jumpForce * s_forceConversion ) * highestRecordAverage[1] ) - ( Mathf.Abs(( ( Physics2D.gravity.y * GameController.c_gravityRatio ) / 2f )) * Mathf.Pow(highestRecordAverage[1], 2) ); //推算最高高度

        //Debug.Log(string.Format("h : {0}, t : {1}", highestRecordAverage[0], highestRecordAverage[1]));

        SetCoolDownTime(); //設定實際冷卻時間
        timer = realCdTime; //初始化計時器(使貓咪一開始就準備好可以跳)
    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart");
        //StartCoroutine(Cor_RecordSpeed(s_recordFrequency));
    }

    //行為實作
    public override void Action()
    {
        //Debug.Log("助跑起跳");

        timer += Time.deltaTime; //計時器推進

        if (parentCat.tracingPosition != null && timer >= realCdTime) //當存在追蹤物件時
        {
            float runUpDistance = r_speed * highestRecordAverage[1]; //計算預備跳躍距離(有正負值)

            //Debug.Log("[計算預備跳躍距離] runUpDistance = " + runUpDistance);
            //Debug.Log("貓咪在餌食的" + (( parentCat.tracingPosition.localPosition.x > parentCat.transform.localPosition.x )? "左": "右") + "邊");
            Debug.Log(string.Format("與餌食之間的距離 : {0}, 跳躍距離 : {1}", Vector2.Distance(parentCat.tracingPosition.localPosition, parentCat.transform.localPosition), runUpDistance));
            Debug.Log(string.Format("餌食高度 : {0}, 最大跳躍高度 : {1}", parentCat.tracingPosition.localPosition.y, highestRecordAverage[0] + parentCat.transform.localPosition.y));

            if (runUpDistance > 0 && //往右助跑
                parentCat.tracingPosition.localPosition.x > parentCat.transform.localPosition.x && //追蹤中餌食在貓咪的右邊
                Mathf.Abs(parentCat.tracingPosition.localPosition.x - parentCat.transform.localPosition.x) <= Mathf.Abs(runUpDistance) && //判斷餌食距離是否在跳躍距離內
                parentCat.tracingPosition.localPosition.y >= highestRecordAverage[0] + parentCat.transform.localPosition.y) //餌食高度大於最大跳躍高度
            {
                parentCat.AddState(CatStateNames.跳躍, -1, 10, null);
                timer = 0; //計時器歸0
                SetCoolDownTime(); //設定實際冷卻時間
                StartCoroutine(Cor_RecordHighest());
            }
            else if (runUpDistance < 0 && //往左助跑
                parentCat.tracingPosition.localPosition.x < parentCat.transform.localPosition.x && //追蹤中餌食在貓咪的左邊
                Mathf.Abs(parentCat.tracingPosition.localPosition.x - parentCat.transform.localPosition.x) <= Mathf.Abs(runUpDistance) && //判斷餌食距離是否在跳躍距離內
                parentCat.tracingPosition.localPosition.y >= highestRecordAverage[0] + parentCat.transform.localPosition.y) //餌食高度大於最大跳躍高度
            {
                parentCat.AddState(CatStateNames.跳躍, -1, 10, null);
                timer = 0; //計時器歸0
                SetCoolDownTime(); //設定實際冷卻時間
                StartCoroutine(Cor_RecordHighest());
            }
        }
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        //Debug.Log("[CatState]助跑起跳暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {

    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        Debug.Log("覆蓋狀態 - 助跑起跳");
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

    //設定冷卻時間
    private void SetCoolDownTime()
    {
        float min = s_coolDownTime - ( s_coolDownTime * s_cdTimeOscillation );
        if (min < 0) min = 0; //限制最小值為0
        float max = s_coolDownTime + ( s_coolDownTime * s_cdTimeOscillation );

        realCdTime = Random.Range(min, max);
    }

    ////每間隔一定時間紀錄速度&加速度
    //private IEnumerator Cor_RecordSpeed(float freq)
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(freq);

    //        float displacement = parentCat.transform.localPosition.x - r_pos; //位移量
    //        r_pos = parentCat.transform.localPosition.x; //更新X軸位置紀錄

    //        if (displacement == 0) //位移量=0時
    //        {
    //            r_speed = 0;
    //            continue;
    //        }

    //        r_speed = displacement / parentCat.recordFrequency; //紀錄速度
    //    }
    //}

    //紀錄跳到最高點的位置與時間
    private IEnumerator Cor_RecordHighest()
    {
        yield return new WaitWhile(() => parentCat.isContactFloor);

        //System.Func<bool> func = () =>
        //{
        //    Debug.Log("2");
        //    return ( temp_pos <= temp_updatePos );
        //};

        float _originPos = parentCat.transform.localPosition.y;
        temp_pos = parentCat.transform.localPosition.y;
        temp_updatePos = temp_pos;

        for (temp_time = 0;
            temp_pos <= temp_updatePos;
            temp_updatePos = parentCat.transform.localPosition.y)
        {
            temp_pos = temp_updatePos;

            yield return new WaitForEndOfFrame();
            temp_time += Time.deltaTime;
        }

        if (highestRecordQueue.Count >= s_recordListLength) highestRecordQueue.Dequeue(); //若長度已超過, 則先取出
        highestRecordQueue.Enqueue(new float[2] { temp_pos - _originPos, temp_time }); //加入紀錄

        //計算平均值
        float _highestSum = 0;
        float _timeSum = 0;

        foreach (float[] queue in highestRecordQueue)
        {
            //Debug.Log(string.Format("h : {0}, t : {1}", queue[0], queue[1]));
            _highestSum += queue[0];
            _timeSum += queue[1];
        }

        Debug.Log(string.Format("Origin[ {0}, {1} ]", highestRecordAverage[0], highestRecordAverage[1]));

        highestRecordAverage[0] = _highestSum / highestRecordQueue.Count; //計算最高高度平均值
        highestRecordAverage[1] = _timeSum / highestRecordQueue.Count; //計算花費時間平均值

        Debug.Log(string.Format("After[ {0}, {1} ]", highestRecordAverage[0], highestRecordAverage[1]));
    }
}

    */
