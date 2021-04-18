using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JumpButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("參考物件")]
    public Transform jumpButtonParent; //跳躍按鈕父物件
    public RectTransform parentRect; //父物件範圍(Rect)
    private Button jumpButton; //跳躍按鈕
    public Image cdTimeCircle; //CD時間環狀條

    [Header("可自訂參數")]
    public float cdTime; //CD時間
    public float dragTime; //滑鼠點超過多久後開始拖曳
    public Vector2 buttonAlphaTrans; //按鈕透明度變化(x=尚未激活, y=已激活)

    [Header("遊戲進行狀態")]
    public bool standByJumping = false; //準備跳躍(CD結束)
    public bool isClicking = false; //左鍵點擊狀態
    public bool isCdTimeWorking; //CD進行中

    private float timer; //計時器
    private float clickTimer; //滑鼠點擊計時器(判斷點擊or拖曳)

    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        //初始化Button
        jumpButton = this.gameObject.AddComponent<Button>();
        jumpButton.onClick.AddListener(Jump);
    }

    void Start()
    {
        StartCoroutine(Cor_MouseEvent()); //滑鼠事件處理
    }

    void Update()
    {
        if (isCdTimeWorking && !standByJumping) TimerUpdate(timer + Time.deltaTime); //計時器更新

        //按鈕凍結判斷
        if (GameController.Instance.gameOver || //若遊戲結束
            !GameController.Instance.GetPlayingState || //若遊戲非進行
            clickTimer >= dragTime || //若點擊超過拖曳時間
            GameController.Instance.cat.StateExistenceTest(CatStateNames.害怕, out _)) //若貓咪處於"害怕"狀態
        {
            if (jumpButton.interactable) jumpButton.interactable = false; //按鈕凍結
        }
        else
        {
            if (!jumpButton.interactable) jumpButton.interactable = true; //解除按鈕凍結
        }

        //CD條凍結
        if (GameController.Instance.gameOver || //若遊戲結束
        !GameController.Instance.GetPlayingState || //若遊戲非進行
        clickTimer >= dragTime) //若點擊超過拖曳時間
        {
            if (isCdTimeWorking) isCdTimeWorking = false; //CD條凍結
        }
        else
        {
            if (!isCdTimeWorking) isCdTimeWorking = true; //解除CD條凍結
        }
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //紀錄目前按鈕位置
    public void SaveButtonPos(Vector2 pos)
    {
        PlayerPrefs.SetFloat("UI_JUMP_X", pos.x);
        PlayerPrefs.SetFloat("UI_JUMP_Y", pos.y);
    }

    //讀取按鈕位置
    public void LoadButtonPos()
    {
        jumpButtonParent.localPosition = new Vector2(PlayerPrefs.GetFloat("UI_JUMP_X", jumpButtonParent.localPosition.x), PlayerPrefs.GetFloat("UI_JUMP_Y", jumpButtonParent.localPosition.y));
    }

    //更新計時器
    public void TimerUpdate(float t)
    {
        if (t >= cdTime) //若計時器到底(CD時間結束)
        {
            timer = cdTime;
            standByJumping = true; //準備跳躍
            jumpButton.image.color = new Color(jumpButton.image.color.r, jumpButton.image.color.g, jumpButton.image.color.b, buttonAlphaTrans.y); //按鈕透明度變化(已激活)
        }
        else
        {
            timer = t; //計時器推進中
            jumpButton.image.color = new Color(jumpButton.image.color.r, jumpButton.image.color.g, jumpButton.image.color.b, buttonAlphaTrans.x); //按鈕透明度變化(未激活)
        }

        cdTimeCircle.fillAmount = timer / cdTime; //CD條顯示
    }

    //跳躍
    public void Jump()
    {
        if (!standByJumping || GameController.Instance.cat.StateExistenceTest(CatStateNames.跳躍, out _)) //CD完成前 或 貓咪未在跳躍狀態
        {
            AudioManagerScript.Instance.PlayAudioClip("se_error"); //撥放"錯誤"音效
            return;
        }

        GameController.Instance.cat.AddState(CatStateNames.跳躍, -1, 10, null);
        TimerUpdate(0); //計時器歸零
        standByJumping = false;
    }

    //滑鼠左鍵點擊
    public void OnPointerDown(PointerEventData d)
    {
        isClicking = true;
    }

    //滑鼠左鍵放開
    public void OnPointerUp(PointerEventData d)
    {
        isClicking = false;
    }

    //滑鼠事件監聽協程
    private IEnumerator Cor_MouseEvent()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (isClicking) //滑鼠左鍵按下時
            {
                clickTimer += Time.deltaTime;
            }
            else //滑鼠左鍵放開時
            {
                clickTimer = 0;
            }

            if (clickTimer >= dragTime) //開始拖曳
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, Input.mousePosition, Camera.main, out Vector2 mousePos); //滑鼠位置轉換(從螢幕座標到本地座標)

                float pos_x = Mathf.Clamp(mousePos.x, -( parentRect.sizeDelta.x / 2 ) + ( jumpButtonParent.GetComponent<RectTransform>().sizeDelta.x / 2 ), ( parentRect.sizeDelta.x / 2 ) - ( jumpButtonParent.GetComponent<RectTransform>().sizeDelta.x / 2 ));
                float pos_y = Mathf.Clamp(mousePos.y, -( parentRect.sizeDelta.y / 2 ) + ( jumpButtonParent.GetComponent<RectTransform>().sizeDelta.y / 2 ), ( parentRect.sizeDelta.y / 2 ) - ( jumpButtonParent.GetComponent<RectTransform>().sizeDelta.y / 2 ));

                jumpButtonParent.localPosition = new Vector2(pos_x, pos_y); //按鈕跟隨滑鼠移動
                SaveButtonPos(jumpButtonParent.localPosition); //紀錄按鈕位置
            }
        }
    }
}
