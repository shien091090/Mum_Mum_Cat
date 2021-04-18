//關卡資訊&記分板控制腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageBoardController : MonoBehaviour
{
    [Header("可自訂參數")]
    public float scoreRate; //分數乘算倍率

    [Header("參考物件")]
    public Text stageNumberText; //關卡數(Text)
    public Text sceneNameText; //場景名稱(Text)
    public Text scoreValueText; //分數(Text)
    public Text highScoreText; //最高分數(Text)
    public Slider thresholdSlider; //下一關分數進度條(Slider)
    public Text eff_stageNumberText; //特效顯示用關卡數(Text)
    public Animation changeStageAnime; //換關動畫

    [Header("遊戲進行狀態")]
    [SerializeField]
    private int aggregateScore; //累計分數值
    [SerializeField]
    private int originScore; //此關卡分數起始值
    [SerializeField]
    private int highScore; //最高分數
    [SerializeField]
    private int surplusScore; //剩餘分數儲存值
    [SerializeField]
    private int thresholdScore; //門檻分數

    public int GetAggregateScore { get { return aggregateScore; } } //取得累計分數
    public int GetHighScore { get { return highScore; } } //取得最高分數

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //關卡數值初始化
    public void StageDataInitialize()
    {
        aggregateScore = 0; //累計分數初始化
        originScore = 0; //分數起始值初始化
        surplusScore = 0; //剩餘分數儲存值初始化
        scoreValueText.text = "0"; //分數顯示初始化
        thresholdSlider.value = 0; //下一關進度條初始化

        //讀取最高分數
        highScore = PlayerPrefs.GetInt("HIGHSCORE");
        highScoreText.text = highScore.ToString();
    }

    //設置關卡資訊
    public void SetStageInfo(StageAttributeData attribute)
    {
        stageNumberText.text = attribute.stageIndex; //設定關卡數(string)
        sceneNameText.text = attribute.sceneName; //設定場景名稱
        thresholdScore = aggregateScore + attribute.thresholdScore; //設定下一關分數門檻
        originScore = aggregateScore; //設定分數起始值
        thresholdSlider.value = 0; //進度條初始化

        if (surplusScore > 0) //若有來自上一關的剩餘分數
        {
            aggregateScore += surplusScore; //加算剩餘分數
            surplusScore = 0;
            if (aggregateScore >= thresholdScore) aggregateScore = thresholdScore; //若分數再次超過下一關門檻, 則直接設為門檻分數, 必須再次有分數增加才會換下一關(不可一次跳兩關以上)
        }
    }

    //分數增減
    public void ScoreIncrease(int v)
    {
        int realValue = Mathf.RoundToInt(v * scoreRate); //實際增減分數(四捨五入)

        if (aggregateScore + realValue >= thresholdScore && GameController.Instance.GetNowStage.nextStage != null) //分數增加後超過門檻時
        {
            surplusScore += ( aggregateScore + realValue ) - thresholdScore; //儲存剩餘分數
            aggregateScore = thresholdScore; //分數設為門檻值

            eff_stageNumberText.text = GameController.Instance.GetNowStage.nextStage.stageIndex; //特效顯示用關卡數
            changeStageAnime.Play(); //撥放換關卡特效

            AudioManagerScript.Instance.PlayAudioClip("se_stage_up"); //撥放換關卡音效

            GameController.Instance.NextStage();
        }
        else aggregateScore += realValue; //分數增加後不超過門檻則直接加分

        scoreValueText.text = aggregateScore.ToString(); //累計分數顯示
        thresholdSlider.value = (float)( aggregateScore - originScore ) / (float)( thresholdScore - originScore ); //進度條顯示
    }

    //紀錄最高分數
    public void SaveHighScore()
    {
        if (aggregateScore > highScore) //若刷新最高分數則儲存之
        {
            PlayerPrefs.SetInt("HIGHSCORE", aggregateScore);
        }
    }
}
