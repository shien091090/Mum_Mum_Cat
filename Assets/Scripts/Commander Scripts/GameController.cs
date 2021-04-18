//控制遊戲從開始、中斷、關卡更換監聽到結束的流程

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制遊戲從開始、中斷、關卡更換監聽到結束的流程
/// </summary>
public partial class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance  //單例模式
    {
        get { return _instance; }
    }

    [Header("可自訂參數")]
    //public const float c_gravityRatio = 47.36f; //重力加速度(9.8 m/s)換算成LocalPosition(像素)的轉換率
    public float jumpScoreBonus; //跳躍時的分數加成倍率
    public float jumpHpBonus; //跳躍時的血量加成倍率(也會照比例減緩血量傷害)

    [Header("遊戲進行狀態")]
    [SerializeField]
    private bool isPlaying; //遊戲進行中與否
    public bool GetPlayingState { get { return isPlaying; } } //取得遊戲進行狀態
    public bool gameOver = false; //遊戲結束(死掉)
    public string playingBgm; //撥放中BGM的名稱
    public float GameTime { private set; get; } //遊戲時間

    [Header("參考物件")]
    public CatBehavior cat; //貓咪腳本
    public Transform startPosRef; //貓咪起始位置參考(Transform)
    public StageAttributeData initialStage; //初始關卡
    public StageBoardController stageBoard; //關卡資訊記分板
    public AnimationController animationController; //動畫控制腳本

    [SerializeField]
    private StageAttributeData nowStage; //目前的關卡資訊
    public StageAttributeData GetNowStage { get { return nowStage; } } //取得目前關卡資訊

    public delegate void Del_ChangePlayingState(bool b); //(委派)遊戲進行狀態變更事件
    public Del_ChangePlayingState ChangePlayingStateEvent; //遊戲進行狀態變更事件(遊戲暫停&繼續時的事件)

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設置單例物件
    }

    void Start()
    {
        HintListen(); //提示監聽開始
        GameInitialize(); //遊戲初始化

        AudiosPack.Instance.SetVolume(0, PlayerPrefs.GetFloat("SET_BGM", 0.7f)); //載入BGM音量
        AudiosPack.Instance.SetVolume(1, PlayerPrefs.GetFloat("SET_SE", 0.7f)); //載入SE音量
        AudiosPack.Instance.SetVolume(2, PlayerPrefs.GetFloat("SET_SE", 0.7f)); //載入SE音量

        UIManager.Instance.JumpButtonScript.LoadButtonPos(); //讀取跳躍按鈕位置

        StartCoroutine(Cor_ShowOpening(false, 1.1f)); //開頭畫面
    }

    void Update()
    {
        if (isPlaying && !gameOver)
        {
            GameTime += Time.deltaTime; //遊戲時間推進
            cat.hpBarScript.HpDamage(GetNowStage.hpDamageSpeed * Time.deltaTime); //持續扣血
        }
        //Debug.Log(GameTime);
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //遊戲初始化
    private void GameInitialize()
    {
        if (HintSystem.Instance.curtain.gameObject.activeSelf) HintSystem.Instance.curtain.gameObject.SetActive(false); //初始化 : 布幕物件隱藏
        if (HintSystem.Instance.spotlight.gameObject.activeSelf) HintSystem.Instance.spotlight.gameObject.SetActive(false); //初始化 : 聚光燈物件隱藏
        if (HintSystem.Instance.textScript.gameObject.activeSelf) HintSystem.Instance.textScript.gameObject.SetActive(false); //初始化 : 提示文字物件隱藏

        stageBoard.StageDataInitialize(); //初始化關卡數值
        nowStage = initialStage; //返回初始關卡
        stageBoard.SetStageInfo(nowStage); //設定關卡資訊
        FodderController.Instance.FodderLineInitialize(); //餌食對列初始化
        cat.hpBarScript.HpBarInitialize(); //血條數據初始化

        EventConditionsInitialize(); //事件條件設定

        if (cat.CatAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tumble"))
        {
            //Debug.Log("目前撥放倒下動畫");
            cat.CatAnimator.SetTrigger("Tumble Over");
        }
        cat.transform.position = startPosRef.position; //初始化貓咪位置
        cat.ResetEmotion(); //清空表情特效

        StageEventController.Instance.ClearAllEvent(); //清除所有事件與相關物件

        gameOver = false; //遊戲結束設為false

        cat.AddState(CatStateNames.行走, -1, 1, null); //添加"走路"狀態
        cat.AddState(CatStateNames.追蹤, -1, 1, null); //添加"追蹤"狀態(額外參數:餌食追蹤更新頻率)
        //cat.AddState(CatStateNames.助跑起跳, -1, 1, null); //添加"助跑起跳"狀態(額外參數:跳躍CD、CD隨機震盪幅度)

        cat.GetRigidbody.velocity = Vector2.zero; //速度初始化

        GameTime = 0; //遊戲時間初始化

        SetPlayingState(false); //設置遊戲為非進行狀態
    }

    //設置遊戲進行狀態(進行或暫停)
    public void SetPlayingState(bool state)
    {
        ChangePlayingStateEvent?.Invoke(state); //呼叫暫停事件
        isPlaying = state;
    }

    //接收分數
    public void ScoreReceive(int score)
    {
        //Debug.Log("得分=" + score);
        stageBoard.ScoreIncrease(score); //得分版處理
    }

    //關卡推進
    public void NextStage()
    {
        if (GetNowStage.nextStage == null) return; //未設置下一關時, 直接結束程序

        //Debug.Log("下一關!");

        //判斷BGM是否換歌
        if (GetNowStage.nextStage.bgmName != playingBgm)
        {
            AudioManagerScript.Instance.CoverPlayAudioClip(GetNowStage.nextStage.bgmName); //撥放遊戲音樂
            playingBgm = GetNowStage.nextStage.bgmName; //紀錄遊戲音樂名稱
        }

        nowStage = GetNowStage.nextStage; //更換關卡腳本
        stageBoard.SetStageInfo(GetNowStage); //設定關卡資訊
    }

    //遊戲結束
    public IEnumerator GameOver()
    {
        gameOver = true; //遊戲結束
        PlayerController.Instance.canOperated = false; //玩家不可操作

        stageBoard.SaveHighScore(); //紀錄最高分數

        cat.RemainState(new List<CatStateNames>() { CatStateNames.跳躍, CatStateNames.行走 }); //只留下"跳躍"和"行走"狀態

        yield return new WaitWhile(() => cat.StateExistenceTest(CatStateNames.跳躍, out _)); //等待貓咪跳躍結束
        cat.GetRigidbody.simulated = false; //貓咪剛體模擬暫停

        AudioManagerScript.Instance.Stop(0); //停止撥放音樂
        AudioManagerScript.Instance.Stop(2); //停止撥放長音效

        AudioManagerScript.Instance.PlayAudioClip("se_gameOver"); //撥放GG音效

        cat.CatAnimator.SetTrigger("Tumble"); //倒地動畫

        yield return new WaitForSeconds(0.3f);

        cat.SetEmotion(EmotionType.倒地暈眩, true);

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(UIManager.Instance.CallGameOverPanel()); //淡入遊戲結束畫面

        StartCoroutine(Cor_ShowOpening(true, 1.1f)); //開頭畫面
    }

    //呼叫開頭畫面
    //[input] resetData : 數據是否初始化 / delay : 操作延遲時間
    private IEnumerator Cor_ShowOpening(bool resetData, float delay)
    {
        GameObject openingPanel = animationController.openingPanel.gameObject;

        AudioManagerScript.Instance.PlayAudioClip("bgm_opening"); //撥放開頭音樂

        if (GetPlayingState) SetPlayingState(false); //玩家不可操作

        if (!openingPanel.activeSelf) //若開頭畫面尚未顯示
        {
            openingPanel.SetActive(true);
            animationController.openingPanel.SetTrigger("Start"); //撥放淡入動畫

            yield return new WaitForSeconds(delay);
        }

        if (resetData) GameInitialize(); //Reset的狀況

        UIManager.Instance.hintButton.interactable = true;
        UIManager.Instance.hintPanel.interactable = true;

        bool isClicked = false;
        UIManager.Instance.startButton.onClick.AddListener(() => { isClicked = true; });

        UIManager.Instance.JumpButtonScript.TimerUpdate(UIManager.Instance.JumpButtonScript.cdTime); //跳躍按鈕CD時間條重置

        yield return new WaitUntil(() => isClicked); //按下滑鼠左鍵
        UIManager.Instance.CallHintPanel(false);
        UIManager.Instance.hintButton.interactable = false;
        UIManager.Instance.hintPanel.interactable = false;

        AudioManagerScript.Instance.Stop(0); //淡出音樂
        AudioManagerScript.Instance.PlayAudioClip("se_button1"); //撥放按鈕音效

        animationController.openingPanel.SetTrigger("End"); //撥放淡出動畫

        yield return new WaitForSeconds(delay);

        SetPlayingState(true); //玩家可操作
        openingPanel.SetActive(false);
        cat.GetRigidbody.simulated = true; //貓咪剛體正常模擬

        AudioManagerScript.Instance.CoverPlayAudioClip(nowStage.bgmName); //撥放遊戲音樂
        playingBgm = nowStage.bgmName; //紀錄遊戲音樂名稱
    }
}
