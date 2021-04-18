//動畫管理腳本

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animation fodderFrame; //餌食對列框
    public Animator wataruKun; //わたる君
    public Animator scared; //表情:害怕
    public Animator openingPanel; //開頭畫面
    public Animator cat; //貓咪

    void Start()
    {
        GameController.Instance.ChangePlayingStateEvent += AnimationPause; //動畫暫停事件訂閱
    }

    //動畫暫停與繼續
    private void AnimationPause(bool state)
    {
        foreach (AnimationState s in fodderFrame)
        {
            s.speed = state ? 1 : 0;
        }

        scared.speed = state ? 1 : 0;
        wataruKun.speed = state ? 1 : 0;
        cat.speed = state ? 1 : 0;

    }
}
