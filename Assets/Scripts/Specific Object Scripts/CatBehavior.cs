//貓行為實作腳本
//[partial]Main

/*Public Func:

    //賦予貓咪狀態(stateName = 狀態名稱 / t = 持續時間 / a = 拮抗強度)
    public void AddState(CatStateNames stateName, float t, int a) 

    //銷毀指定狀態(states = 欲銷毀狀態)
    public void DestroyState(List<AbsCatState> states)

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class CatBehavior : MonoBehaviour
{
    [Header("參考物件")]
    public HpBarBehavior hpBarScript; //血條腳本
    public EmotionController emotionController; //表情特效管理腳本

    [System.Serializable]
    public struct AttributeSetting
    {
        public CatStateNames state; //狀態名
        public CatAttribute data; //屬性數據(ScriptableObject)
    }

    [Header("狀態數據物件")]
    public CatAttributeData_Base catAttribute_base; //基本數據設定
    public List<AttributeSetting> attributeSettings; //狀態數據設定

    public Dictionary<CatStateNames, CatAttribute> Dict_attributeSetting { private set; get; } //(字典)從attributeSettings中, 由狀態名(CatStateNames)查找數據資料(CatAttribute)

    [Header("遊戲進行狀態")]
    public List<AbsCatState> activeCatStates; //激活中的狀態清單
    public Transform tracingPosition; //追蹤目標
    public bool isContactFloor; //是否接觸地面
    public bool isContactWall; //是否接觸牆壁

    [Header("可自訂參數(生命相關)")]
    public float catMaxHp; //貓咪最大血量
    public float catIniHp; //貓咪初始血量

    [Header("可自訂參數(行動相關)")]
    public float speedRatio = 1; //速度變化值(用於加速或緩速)
    public float reverseBuffer; //翻轉緩衝速度(貓咪速度絕對值在此數值之下時, 不執行翻轉動畫)

    [Header("可自訂參數(動畫相關)")]
    public AnimationCurve walkSpeedVariation; //走路動畫撥放速度變化(縱軸:動畫撥放速度 / 橫軸:移動速度)
    public AnimationCurve runSpeedVariation; //跑步動畫撥放速度變化(縱軸:動畫撥放速度 / 橫軸:移動速度)
    public bool isRunning = false; //是否為跑步狀態


    public Rigidbody2D GetRigidbody { get { return this.gameObject.GetComponent<Rigidbody2D>(); } } //取得Rigidbody2D

    public Animator CatAnimator { private set; get; }
    private Animation catAnimation;

    private float edgeDistance; //畫面左右邊距離(最大距離)

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (this.GetComponent<Animator>() != null) CatAnimator = this.GetComponent<Animator>();
        if (this.GetComponent<Animation>() != null) catAnimation = this.GetComponent<Animation>();
    }

    void Start()
    {
        AttributeSettingDebug(); //數據設定偵錯與建立Dictionary(dict_attributeSetting)
        edgeDistance = UIManager.Instance.screenRect.sizeDelta.x; //取得畫面左右邊距離
        GameController.Instance.ChangePlayingStateEvent += RigidbodyPause; //動畫暫停事件訂閱

        //Debug.Log("edgeDistance : " + edgeDistance);
    }

    void Update()
    {
        //貓咪動畫
        if (CatAnimator != null) CatWalkAnime();
    }

    //碰撞偵測(Enter)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Floor")) isContactFloor = true;
        else if (collision.collider.CompareTag("Wall")) isContactWall = true;
    }

    //碰撞偵測(Exit)
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Floor")) isContactFloor = false;
        else if (collision.collider.CompareTag("Wall")) isContactWall = false;
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //吃餌食
    //[input] fodder = 餌食Script, action = 餌食特殊效果, isBonus = 是否有加成效果
    public void EatFodder(FodderBehavior fodder, FodderEffectAction effectAction, bool isBonus)
    {
        float damage = 0; //HP影響值(負數=扣血/正數=補血)
        float hpBonus = GameController.Instance.jumpHpBonus; //HP加成

        if (isBonus) //有加成效果
        {
            //加成效果=補血增強&扣血減緩 : 假設HP加成效果為1.3時, 補血*1.3 / 扣血*0.7
            float _bonus = fodder.GetAttribute.hpEffect >= 0 ? hpBonus : ( 2 - hpBonus );
            //Debug.Log("Bonus = " + _bonus);
            damage = fodder.GetAttribute.hpEffect * _bonus;
        }
        else //無加成效果
        {
            damage = fodder.GetAttribute.hpEffect;
        }

        AudioManagerScript.Instance.PlayAudioClip("se_eat"); //撥放"吃"音效

        if (this == GameController.Instance.cat) //此貓為主人公貓
        {
            GameController.Instance.ScoreReceive(Mathf.RoundToInt(fodder.GetAttribute.score * GameController.Instance.jumpScoreBonus)); //餌食得分處理
        }

        if (hpBarScript != null) //當貓咪存在血條時, 套用HP增減效果
        {
            hpBarScript.HpDamage(damage);
            //Debug.Log("Damage = " + damage);
        }

        if (effectAction.GetPersistentEventCount() > 0) effectAction.Invoke(this); //執行餌食特殊效果
    }

    //移動(多載1/3) 往指定方向移動
    public void Move(MoveDirection direction, float speed, AnimationCurve moveCurve)
    {
        switch (direction)
        {
            case MoveDirection.往左:
                _Move(UIManager.Instance.EdgeLocalPos_x.x, speed, moveCurve);
                break;

            case MoveDirection.往右:
                _Move(UIManager.Instance.EdgeLocalPos_x.y, speed, moveCurve);
                break;

            case MoveDirection.不移動:
                return;
        }
    }

    //移動(多載2/3) 往指定X軸位置移動
    public void Move(float xPos, float speed, AnimationCurve moveCurve)
    {
        _Move(xPos, speed, moveCurve);
    }

    //移動(多載3/3) 自動往TracingPosition的方向移動(自動追蹤餌食)
    public void Move(float speed, AnimationCurve moveCurve)
    {
        if (tracingPosition != null) _Move(tracingPosition.localPosition.x, speed, moveCurve);
    }

    //[private]移動
    //[input] xPos : 目標位置 / speed : 移動速度 / moveCurve : 加速度變化曲線
    private void _Move(float xPos, float speed, AnimationCurve moveCurve)
    {
        float catPos_x = this.transform.localPosition.x; //貓咪目前X軸位置
        Vector2 dir = new Vector2(); //欲移動方向
        float fodderDistance; //與餌食之間的距離
        float executeRate; //加速度作用率

        //方向判定
        if (catPos_x - xPos > 0) dir = Vector2.left;
        else if (catPos_x - xPos < 0) dir = Vector2.right;
        else if (catPos_x - xPos == 0) dir = Vector2.zero;

        //速度加成計算
        float speedRate;

        if (moveCurve == null) speedRate = 1; //若沒有定義加速度變化曲線
        else
        {
            fodderDistance = Mathf.Abs(tracingPosition.localPosition.x - this.transform.localPosition.x); //取得貓咪與餌食之間的距離
            executeRate = fodderDistance / edgeDistance; //作用率

            speedRate = moveCurve.Evaluate(executeRate); //根據貓咪與餌食之間的距離與畫面寬度之間的比值計算速度加成
        }

        //Debug.Log("speed_x * moveCurve.Evaluate(executeRate) = " + ( speed_x * moveCurve.Evaluate(executeRate) ));

        //執行移動
        //移動量變數 : dir(方向)、speed(移動速度)、speedRate(根據與目標距離的速度乘算)、speedRatio(整體速度乘算(加速或緩速用))、Time.deltaTime(每幀時間)
        GetRigidbody.AddForce(dir * ( speed * speedRate ) * speedRatio * Time.deltaTime, ForceMode2D.Impulse);
    }

    //設定表情特效
    //[input] emotion : 表情種類 / state : 開或關
    public void SetEmotion(EmotionType emotion, bool state)
    {
        if (emotionController == null) return; //若沒有設定表情特效腳本, 則直接結束程序

        emotionController.SetEmotion(emotion, state);
    }

    //清空表情特效
    public void ResetEmotion()
    {
        if (emotionController == null) return; //若沒有設定表情特效腳本, 則直接結束程序

        emotionController.ResetEmotion();
    }

    //貓咪走路動畫(走路與跑步的切換, 以及動畫撥放速度的控制)
    private void CatWalkAnime()
    {
        float speed = Mathf.Abs(GetRigidbody.velocity.x); //取得目前速度

        if (speed >= walkSpeedVariation.keys[walkSpeedVariation.keys.Length - 1].time) //速度超過曲線time最大值(橫軸), 轉為"跑步"動畫
        {
            if (!isRunning)
            {
                isRunning = true;
                CatAnimator.SetBool("Running", isRunning);
            }

            float _speed = speed;
            if (_speed >= runSpeedVariation.keys[runSpeedVariation.keys.Length - 1].time) _speed = runSpeedVariation.keys[runSpeedVariation.keys.Length - 1].time;

            //Debug.Log("TestSpeed : " + _speed);
            CatAnimator.SetFloat("RunSpeed", runSpeedVariation.Evaluate(_speed)); //改變動畫撥放速度
        }
        else
        {
            if (isRunning)
            {
                isRunning = false;
                CatAnimator.SetBool("Running", isRunning);
            }

            //Debug.Log("TestSpeed : " + speed);
            CatAnimator.SetFloat("WalkSpeed", walkSpeedVariation.Evaluate(speed)); //改變動畫撥放速度
        }

        //Direction : true = 往左 / false = 往右
        if (GetRigidbody.velocity.x > reverseBuffer && CatAnimator.GetBool("Direction"))
        {
            catAnimation.Play("cat_turn_right");
            CatAnimator.SetBool("Direction", false);
        }
        else if (GetRigidbody.velocity.x < -reverseBuffer && !CatAnimator.GetBool("Direction"))
        {
            catAnimation.Play("cat_turn_left");
            CatAnimator.SetBool("Direction", true);
        }

    }

    //數據設定偵錯與建立Dictionary(dict_attributeSetting)
    private void AttributeSettingDebug()
    {
        Dict_attributeSetting = new Dictionary<CatStateNames, CatAttribute>();

        List<CatStateNames> _stateList = new List<CatStateNames>(); //狀態列表(篩選重複用)

        for (int i = 0; i < attributeSettings.Count; i++)
        {
            bool stateDiscrepant = false; //測試狀態名是否與數據腳本不符合

            switch (attributeSettings[i].state)
            {
                case CatStateNames.跳躍:
                    if (attributeSettings[i].data.GetType() != typeof(CatAttributeData_Jump)) stateDiscrepant = true;
                    break;

                case CatStateNames.跌倒:
                    break;

                case CatStateNames.助跑起跳:
                    if (attributeSettings[i].data.GetType() != typeof(CatAttributeData_RunUp)) stateDiscrepant = true;
                    break;

                case CatStateNames.害怕:
                    if (attributeSettings[i].data.GetType() != typeof(CatAttributeData_Scared)) stateDiscrepant = true;
                    break;

                case CatStateNames.速度變化:
                    if (attributeSettings[i].data.GetType() != typeof(CatAttributeData_SpeedChange)) stateDiscrepant = true;
                    break;

                case CatStateNames.追蹤:
                    if (attributeSettings[i].data.GetType() != typeof(CatAttributeData_Trace)) stateDiscrepant = true;
                    break;

                default:
                    break;
            }

            if (stateDiscrepant) throw new System.Exception("[ERROR]狀態名是否與數據腳本不符合!!");

            if (_stateList.Count > 0)  //若非第一個執行的項目時, 執行重複檢測
            {
                for (int j = 0; j < _stateList.Count; j++)
                {
                    if (attributeSettings[i].state == _stateList[j]) throw new System.Exception("[ERROR]設定重複的狀態數據!!");
                }
            }

            Dict_attributeSetting.Add(attributeSettings[i].state, attributeSettings[i].data); //字典追加
            _stateList.Add(attributeSettings[i].state); //加入至狀態列表以利下一個迴圈開始執行重複檢測
        }
    }

    //剛體模擬暫停or繼續
    private void RigidbodyPause(bool state)
    {
        GetRigidbody.simulated = state;
    }
}
