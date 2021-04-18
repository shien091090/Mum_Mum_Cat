//儲存所有UI參考物件的腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance; //單例物件
    public static UIManager Instance { get { return _instance; } } //取得單例物件

    [Header("畫面寬度")]
    public RectTransform screenRect; //用於計算畫面寬度的RectTransform
    public Vector2 EdgeLocalPos_x { private set; get; } //畫面邊緣X軸位置([x]=左 [y]=右) ※Local座標
    public Vector2 EdgePos_x { private set; get; } //畫面邊緣X軸位置([x]=左 [y]=右) ※World座標

    [Header("遊戲進行狀態")]
    [SerializeField]
    private bool onSettingMenu = false; //設定選單開啟中

    [Header("遊戲開始畫面")]
    public Button startButton; //遊戲開始按鈕
    public Button hintButton; //顯示提示按鈕
    public Button hintPanel; //提示畫面(按鈕)

    [Header("遊戲結束畫面")]
    public GameObject gameOverPanel; //遊戲結束畫面
    public Text finalScore; //結算分數(Text)
    public Text highScore; //最高分數(Text)
    public GameObject newHighScoreEffect; //"刷新最高分數"效果物件
    public Button resetButton; //"重新開始"按鈕

    [Header("設定選單")]
    public GameObject settingPanel; //設定選單
    public Slider bgmVolumeSlider; //BGM音量(Slider)
    public Text bgmVolumeValue; //BGM音量值(Text)
    public Slider seVolumeSlider; //SE音量(Slider)
    public Text seVolumeValue; //SE音量值(Text)

    [Header("離開遊戲畫面")]
    public GameObject exitPanel; //離開遊戲畫面
    public Button btn_y; //"是"按鈕
    public Button btn_n; //"否"按鈕

    [Header("跳躍按鈕")]
    public JumpButtonBehavior JumpButtonScript; //跳躍按鈕控制腳本

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設定單例模式
    }

    void Start()
    {
        EdgeLocalPos_x = new Vector2(screenRect.localPosition.x - ( screenRect.sizeDelta.x / 2 ), screenRect.localPosition.x + ( screenRect.sizeDelta.x / 2 )); //畫面邊界X軸Local位置
        EdgePos_x = new Vector2(screenRect.TransformPoint(new Vector2(EdgeLocalPos_x.x, 0)).x, screenRect.TransformPoint(new Vector2(EdgeLocalPos_x.y, 0)).x); //畫面邊界X軸World位置

        //Debug.Log(string.Format("EdgeLocalPos_x = ({0}, {1}) / EdgePos_x = ({2}, {3})", EdgeLocalPos_x.x, EdgeLocalPos_x.y, EdgePos_x.x, EdgePos_x.y));
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //呼叫遊戲結束畫面
    public IEnumerator CallGameOverPanel()
    {
        CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0; //設遊戲結束畫面為透明

        gameOverPanel.SetActive(true); //設物件為開啟
        finalScore.text = GameController.Instance.stageBoard.GetAggregateScore.ToString(); //設置結算分數

        if (GameController.Instance.stageBoard.GetAggregateScore > GameController.Instance.stageBoard.GetHighScore) //若刷新最高分數
        {
            highScore.text = GameController.Instance.stageBoard.GetAggregateScore.ToString(); //設置最高分數
            newHighScoreEffect.SetActive(true);
        }
        else
        {
            highScore.text = GameController.Instance.stageBoard.GetHighScore.ToString(); //設置最高分數
        }

        //淡入
        while (cg.alpha < 1)
        {
            yield return new WaitForEndOfFrame();
            cg.alpha += Time.deltaTime;
        }

        bool isClicked = false;
        resetButton.onClick.AddListener(() => { isClicked = true; }); //增設按鈕事件

        yield return new WaitUntil(() => isClicked); //等待玩家按下"重新開始"

        //淡出
        while (cg.alpha > 0)
        {
            yield return new WaitForEndOfFrame();
            cg.alpha -= Time.deltaTime * 1.4f;
        }

        gameOverPanel.SetActive(false); //設物件為關閉
        yield return new WaitForSeconds(0.5f);

        //Debug.Log("遊戲結束");
    }

    //呼叫設定選單
    public void CallSettingMenu()
    {
        if (GameController.Instance.gameOver) return; //正在執行遊戲結束過場時, 不可打開設定選單

        onSettingMenu = !onSettingMenu;
        AudioManagerScript.Instance.PlayAudioClip("se_button2"); //撥放按鈕音效

        if (onSettingMenu) //開啟設定選單時, 載入設定值
        {
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("SET_BGM", 0.7f); //載入BGM音量(拉條)
            bgmVolumeValue.text = ( bgmVolumeSlider.value * 100 ).ToString("0"); //載入BGM音量(值顯示)
            seVolumeSlider.value = PlayerPrefs.GetFloat("SET_SE", 0.7f); //載入SE音量(拉條)
            seVolumeValue.text = ( seVolumeSlider.value * 100 ).ToString("0"); //載入SE音量(值顯示)
        }

        settingPanel.SetActive(onSettingMenu); //選單介面開關
        GameController.Instance.SetPlayingState(!onSettingMenu); //遊戲暫停或繼續
    }

    //呼叫離開遊戲畫面
    public void CallExitPanel()
    {
        if (!exitPanel.activeSelf) StartCoroutine(Cor_CallExitPanel(GameController.Instance.GetPlayingState)); //尚未打開離開遊戲畫面時, 開啟之
    }

    //(協程)呼叫離開遊戲畫面
    private IEnumerator Cor_CallExitPanel(bool playingState)
    {
        if (playingState) GameController.Instance.SetPlayingState(false); //若在遊戲進行中打開, 則遊戲暫停

        exitPanel.SetActive(true);

        bool isClicked = false;

        Debug.Log("btn_y.onClick == null : " + btn_y.onClick == null);
        btn_y.onClick.AddListener(() => { Application.Quit(); }); //按下"是", 結束遊戲
        Debug.Log("btn_n.onClick == null : " + btn_n.onClick == null);
        btn_n.onClick.AddListener(() => { isClicked = true; }); //按下"否", 關閉畫面

        yield return new WaitUntil(() => isClicked); //按下按鈕

        exitPanel.SetActive(false);

        if (playingState) GameController.Instance.SetPlayingState(true); //若在遊戲進行中打開, 則程序結束時恢復進行狀態
    }

    //設定BGM音量
    public void SetBgmVolume(float v)
    {
        AudiosPack.Instance.SetVolume(0, v);

        bgmVolumeValue.text = ( v * 100 ).ToString("0"); //改變顯示值

        PlayerPrefs.SetFloat("SET_BGM", v); //儲存設定值
    }

    //設定SE音量
    public void SetSeVolume(float v)
    {
        AudiosPack.Instance.SetVolume(1, v);
        AudiosPack.Instance.SetVolume(2, v);

        seVolumeValue.text = ( v * 100 ).ToString("0"); //改變顯示值

        PlayerPrefs.SetFloat("SET_SE", v); //儲存設定值
    }

    //開關提示畫面(多載1/2)
    public void CallHintPanel()
    {
        AudioManagerScript.Instance.PlayAudioClip("se_button2"); //撥放按鈕音效
        hintPanel.gameObject.SetActive(!hintPanel.gameObject.activeSelf);
    }

    //開關提示畫面(多載2/2)
    public void CallHintPanel(bool b)
    {
        AudioManagerScript.Instance.PlayAudioClip("se_button2"); //撥放按鈕音效
        hintPanel.gameObject.SetActive(b);
    }
}
