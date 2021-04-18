//[事件]わたる君現身

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WataruKun : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        StageEventController.Instance.ClearEvent += WataruKunDisappear;
    }

    //------------------------------------------------------------------------------------------------------------------------------------------------

    //わたる君現身
    public void WataruKunAppear()
    {
        this.gameObject.SetActive(true); //物件開啟

        GameController.Instance.cat.AddState(CatStateNames.害怕, -1, 8, this.transform); //添加"害怕"狀態

        this.GetComponent<Animator>().enabled = true; //開啟動畫

        AudioManagerScript.Instance.PlayAudioClip("lse_wataru"); //撥放わたる君現身音效
        AudioManagerScript.Instance.Stop(0); //背景音樂停止
    }

    //わたる君消失
    private void WataruKunDisappear()
    {
        //Debug.Log("わたる君消失");

        if (!this.gameObject.activeSelf) return; //若已經是關閉狀態則結束程序

        this.GetComponent<Animator>().enabled = false; //關閉動畫
        this.gameObject.SetActive(false); //關閉物件
        StageEventController.Instance.EventOver(StageEventType.watatu君現身); //從事件執行列表中將指定事件抽除

        GameController.Instance.cat.DestroyState(new List<CatStateNames>() { CatStateNames.害怕 }); //結束"害怕"狀態

        if (!GameController.Instance.gameOver) AudioManagerScript.Instance.PlayAudioClip(GameController.Instance.playingBgm); //回復音樂
    }
}
