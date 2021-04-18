//餌食管理腳本

/*
 * 追加新餌食方式:
 * 1.在EnumClass.cs中的FodderType內增添新的餌食種類
 * 2.在Game Scene/FodderAttribute追加新的GameObject, 掛載FodderAttribute(Script)
 * 3.在FodderAttribute中設定新餌食的各種屬性
 * 4.完成(記得Stage Attribute中要設定該餌食的出現機率)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FodderController : MonoBehaviour
{
    private static FodderController _instance; //單例物件
    public static FodderController Instance { get { return _instance; } } //取得單例物件

    [Header("可自訂參數")]
    public List<FodderAttributeData> fodderAttributes = new List<FodderAttributeData>(); //餌食資料庫
    public int fodderQueueLength; //餌食預置體佇列長度

    [Header("參考物件")]
    public GameObject fodderPrefab; //餌食預置體
    public RectTransform fodderRect; //餌食父物件(RectTransform)
    public Transform Anchor_Vertex; //錨定點:餌食投擲起始點
    public Transform Anchor_Bottom; //錨定點:畫面底部
    public FoddersFrameBehavior foddersFrameBehavior; //腳本參考:準備投擲餌食框對列行為

    [Header("遊戲進行狀態")]
    [SerializeField]
    private List<FodderType> fodderQueue; //接下來要丟的餌食隊伍
    public List<FodderType> GetFodderQueue { get { return fodderQueue; } } //取得預備投擲餌食對列(fodderQueue)

    [SerializeField]
    private List<Transform> fallingFodders; //落下中餌食列表
    public List<Transform> GetFallingFodders { get { return fallingFodders; } } //取得落下中餌食列表

    [SerializeField]
    private List<GameObject> fodderObjectPool; //餌食物件池
    public List<GameObject> GetFodderObjectPool { get { return fodderObjectPool; } } //取得物件池

    public Dictionary<FodderType, FodderAttributeData> dic_attributeList = new Dictionary<FodderType, FodderAttributeData>(); //FodderAttribute對應Dictionary

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例物件
        fodderObjectPool = new List<GameObject>(); //物件池初始化
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //餌食對列初始化
    public void FodderLineInitialize()
    {
        InsertFodderQueue(fodderQueueLength); //生成餌食預置體佇列
        ClearFallingFodder(); //銷毀所有掉落中餌食

        if (dic_attributeList.Count == 0) //建立餌食數據對應Dictionary
        {
            //Debug.Log("AttributeSetting.childCount = " + AttributeSetting.childCount);

            //for (int i = 0; i < AttributeSetting.childCount; i++) //遍歷AttributeSetting底下的所有子物件
            //{
            //    GameObject go = AttributeSetting.GetChild(i).gameObject;

            //    if (go.GetComponent<FodderAttribute>() == null) throw new System.Exception("[ERROR]此物件不存在FodderAttribute");
            //    FodderAttribute newAttribute = go.GetComponent<FodderAttribute>();

            //    if (attributeList.Count > 0) //若attributeList已經有Attribute存入其中
            //    {
            //        for (int j = 0; j < attributeList.Count; j++) //尋找已經存入的Attribute跟欲加入的Attribute是否發生重複
            //        {
            //            if (attributeList[j].fodderType == newAttribute.fodderType) throw new System.Exception("[ERROR]設定重複的FodderType");
            //        }

            //    }

            //    attributeList.Add(newAttribute); //將新增的Attribute加入至attributeList
            //    dic_attributeList.Add(newAttribute.fodderType, newAttribute); //設定對應fodderType與Attribute的Dictionary

            //    //Debug.Log("dic_attributeList.Count = " + dic_attributeList.Count);
            //}

            for (int i = 0; i < fodderAttributes.Count; i++) //遍歷餌食資料庫, 逐一加入字典
            {
                if(dic_attributeList.ContainsKey(fodderAttributes[i].fodderType)) throw new System.Exception("[ERROR]設定重複的FodderType");
                dic_attributeList.Add(fodderAttributes[i].fodderType, fodderAttributes[i]);
            }
        }

        foddersFrameBehavior.ImageInitialize(); //餌食對列框圖像初始化
    }

    //從物件池中抽取物件
    public GameObject ExtractFromPool()
    {
        GameObject go = null;

        UnityEngine.Events.UnityAction CreateNew = new UnityEngine.Events.UnityAction(() => //Lambda:創立新物件&加長物件池陣列
      {
          go = Instantiate(fodderPrefab, fodderRect); //建立新物件
          fodderObjectPool.Add(go);
      });

        if (fodderObjectPool.Count == 0) CreateNew(); //若物件池中沒有任何物件
        else //若物件中存在物件
        {
            int index = -1; //物件池陣列索引
            for (int i = 0; i < fodderObjectPool.Count; i++)
            {
                if (!fodderObjectPool[i].activeSelf) //若物件池中有找到未激活(睡眠中)物件, 則設定其物件所在索引
                {
                    index = i;
                    break;
                }
            }

            if (index < 0) CreateNew(); //若物件池中的物件都在激活狀態, 則創立新物件
            else go = fodderObjectPool[index]; //若物件池中有物件處於未激活(睡眠中), 則設定回傳該物件
        }

        return go;
    }

    //插入輸入值數量之餌食預置體佇列(超過時替換)
    private void InsertFodderQueue(int count)
    {
        StageAttributeData nowStage = GameController.Instance.GetNowStage; //取得目前關卡資訊

        if (count > fodderQueueLength) count = fodderQueueLength; //插入數量不可超過佇列長度

        for (int i = 0; i < count; i++) //運行輸入值次數的插入佇列動作
        {
            //GameObject newPrefab = null;
            FodderType fodderType = FodderType.無;
            int totalWeight = 0; //權重加總
            int randomValue; //隨機值

            for (int j = 0; j < nowStage.stateFodders.Count; j++) //計算所有項目之權重加總
            {
                totalWeight += nowStage.stateFodders[j].buildFreq;
            }
            randomValue = Random.Range(1, totalWeight + 1); //在加總權重值中隨機取數

            for (int j = 0; j < nowStage.stateFodders.Count; j++) //計算隨機數值落在哪個區間, 以判斷抽取之餌食預置體
            {
                if (nowStage.stateFodders[j].buildFreq >= randomValue) //落於該區間時
                {
                    fodderType = nowStage.stateFodders[j].fodder;
                    break;
                }
                randomValue -= nowStage.stateFodders[j].buildFreq; //未落於該區間時, 減去該項目權重值以進行下一個項目的權重值判斷
            }

            if (fodderType == FodderType.無) //若下一個欲新增的餌食預置體為空值則報錯
            {
                Debug.Log("[ERROR]未設定下一個欲新增的餌食");
                return;
            }

            fodderQueue.Add(fodderType); //增添下一個餌食預置體於佇列
        }

        if (fodderQueue.Count > fodderQueueLength) fodderQueue.RemoveRange(0, fodderQueue.Count - fodderQueueLength); //佇列長度溢出時縮減
    }

    //增添落下中餌食列表
    //isIncrease : true = 增加, false = 減去
    public void FallingFodderIncrease(Transform fodder, bool isIncrease)
    {
        if (isIncrease) //增加飼料Transform於列表中
        {
            fallingFodders.Add(fodder);
        }
        else //從列表中消除指定飼料Transform
        {
            fallingFodders.Remove(fodder);
        }
    }

    //丟餌食
    public void ThrowFodder()
    {
        if (foddersFrameBehavior.throwing) return; //若正在投擲中則禁止投擲

        AudioManagerScript.Instance.PlayAudioClip("se_throw"); //撥放"投擲"音效

        //餌食落下
        RectTransformUtility.ScreenPointToWorldPointInRectangle(fodderRect, Input.mousePosition, Camera.main, out Vector3 mousePos); //滑鼠位置轉換(從螢幕座標到世界座標)

        //GameObject fodder = Instantiate(GetNextFodder(), new Vector2(mousePos.x, fodderRect.transform.position.y), Quaternion.identity, fodderRect); //生成餌食物件
        //GameObject fodder = CreateFodder(new Vector2(mousePos.x, fodderRect.transform.position.y));

        GameObject fodder = ExtractFromPool(); //從物件池中取得物件
        if (fodder == null) throw new System.Exception("[ERROR]餌食物件為空物件");

        FodderBehavior fodderBehavior = fodder.GetComponent<FodderBehavior>();
        if (fodderBehavior == null) throw new System.Exception("[ERROR]餌食物件中不存在FodderBehavior腳本");

        FodderType fodderType = fodderQueue[0]; //取得要拋出的餌食種類(佇列第一項)

        if (!dic_attributeList.ContainsKey(fodderType)) throw new System.Exception("[ERROR]尚未設定此種類餌食數據");
        FodderAttributeData attribute = dic_attributeList[fodderType]; //取得指定種類餌食數據

        fodder.transform.position = new Vector2(mousePos.x, fodderRect.transform.position.y); //設定餌食物件位置
        fodder.SetActive(true);

        fodderBehavior.SetAttribute(attribute); //設定餌食數據

        InsertFodderQueue(1); //插入新的餌食預置體於佇列

        foddersFrameBehavior.PlayAnimation(); //撥放投擲餌食後的往前替補動畫

        FallingFodderIncrease(fodder.transform, true); //添加至落下中餌食列表

        //if (GetFallingFodders.Count <= 1) GameController.Instance.cat.SetFollowTarget(); //若目前沒有餌食正在掉落(貓咪處於靜置狀態), 則拋餌瞬間馬上吸引貓咪
    }

    int a = 0;

    //清空所有掉落中餌食
    public void ClearFallingFodder()
    {
        if (GetFallingFodders.Count < 1) return;
        a += 1;
        GetFallingFodders[0].GetComponent<FodderBehavior>().FodderDestroy();
        if (a >= 200) return;
        ClearFallingFodder();
    }
}
