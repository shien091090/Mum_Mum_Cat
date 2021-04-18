//監聽玩家操作(目前只有點擊，也就是餵食行為)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 監聽玩家操作(目前只有點擊，也就是餵食行為)
/// </summary>
public class PlayerController : MonoBehaviour
{
    private static PlayerController _instance;
    public static PlayerController Instance //單例模式
    {
        get { return _instance; }
    }

    [Header("遊戲進行狀態")]
    public bool canOperated = false; //是否為可點擊狀態(主要作用於跑動畫時無法操作)

    public int testValue = 0;

    //------------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        if (_instance == null) _instance = this; //設置單例物件
        GameController.Instance.ChangePlayingStateEvent += ChangePlayingState; //遊戲暫停事件追加
    }

    void Update()
    {
        if (canOperated) KeyListen(); //若遊戲非暫停狀態, 持續監聽滑鼠點擊事件

        //測試按鍵
        if (Input.GetKeyDown(KeyCode.F1)) //暫停遊戲
        {
            GameController.Instance.SetPlayingState(!GameController.Instance.GetPlayingState);
        }

        if (Input.GetKeyDown(KeyCode.F2)) //離開遊戲
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.F3)) //下一關Test
        {
            Debug.Log("GetNowStage : " + GameController.Instance.GetNowStage.name);
            Debug.Log("NextStage : " + GameController.Instance.GetNowStage.nextStage.name);

            GameController.Instance.NextStage();
        }

        if (Input.GetKeyDown(KeyCode.F4)) //直接暴斃
        {
            GameController.Instance.cat.hpBarScript.HpDamage(-500);
        }

        if (Input.GetKeyDown(KeyCode.F5)) //跳躍
        {
            GameController.Instance.cat.AddState(CatStateNames.跳躍, -1, 10, null);
        }
    }


    //------------------------------------------------------------------------------------------------------------------

    //暫停事件
    private void ChangePlayingState(bool state)
    {
        //Debug.Log("PlayerController暫停事件 : " + state);
        canOperated = state;
    }

    //按鍵監聽
    private void KeyListen()
    {
        //按下滑鼠左鍵
        if (Input.GetMouseButtonDown(0))
        {
            if (Application.platform == RuntimePlatform.WindowsEditor) //Unity Editor調試時
            {
                if (EventSystem.current.IsPointerOverGameObject() == true) return;
            }
            else //實際遊玩時(手機)
            {
                Touch touch = Input.GetTouch(0);
                bool isTouchUIElement = EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                if (isTouchUIElement) return;
            }

            FodderController.Instance.ThrowFodder();
        }

        //手機"返回"鍵
        if (( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer ) && Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.CallExitPanel();
        }
    }
}
