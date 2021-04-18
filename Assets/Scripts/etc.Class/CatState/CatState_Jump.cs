using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatState_Jump : AbsCatState
{
    private float s_jumpForce; //跳躍力道

    public override void BuildInfo(int a, params object[] etc)
    {
        //設定狀態名稱
        stateName = CatStateNames.跳躍;

        //設定拮抗狀態
        antagonismStates = new AntagonismStates[3];

        antagonismStates[0].antagonismState = CatStateNames.行走;
        antagonismStates[0].antagonismType = AntagonismType.凍結;

        antagonismStates[1].antagonismState = CatStateNames.助跑起跳;
        antagonismStates[1].antagonismType = AntagonismType.凍結;

        antagonismStates[2].antagonismState = CatStateNames.害怕;
        antagonismStates[2].antagonismType = AntagonismType.隱藏;

        //設定拮抗強度
        this.antagonismValue = a;

        //設定參數
        CatAttributeData_Jump _att = (CatAttributeData_Jump)parentCat.Dict_attributeSetting[CatStateNames.跳躍];
        s_jumpForce = _att.jumpForce;


    }

    //行為實作開始(開始時執行一次)
    public override void ActionStart()
    {
        //Debug.Log("ActionStart");

        parentCat.GetRigidbody.AddForce(( Vector2.up * s_jumpForce ) + ( Vector2.right * parentCat.GetRigidbody.velocity.x ), ForceMode2D.Force); //施加往上力道(跳躍)

        RectTransform _rect = parentCat.GetComponent<RectTransform>();

        ParticleEffectController.Instance.ShowParticleEffect(ParticleEffectType.跳躍, new Vector2(parentCat.transform.localPosition.x, _rect.localPosition.y - ( _rect.sizeDelta.y / 2 )), false); //粒子特效
        if (parentCat.CatAnimator != null) parentCat.CatAnimator.SetBool("Jump", true); //跳躍動畫
        AudioManagerScript.Instance.PlayAudioClip("se_jump"); //撥放跳躍音效

        StartCoroutine(Jumping()); //追蹤是否跳躍完成
    }

    public override void Action()
    {
        //Debug.Log("Action - 跳躍");
    }

    private IEnumerator Jumping()
    {
        yield return new WaitWhile(() => parentCat.isContactFloor); //等待離開地板

        yield return new WaitUntil(() => parentCat.isContactFloor); //等待接觸地板
        parentCat.DestroyState(new List<AbsCatState>() { this }); //銷毀狀態
    }

    //暫停事件實作
    public override void ChangePlayingState(bool state)
    {
        //Debug.Log("[CatState]跳躍暫停 : " + state);
    }

    //銷毀前結束事件
    public override void ActionOver()
    {
        if (parentCat.CatAnimator != null) parentCat.CatAnimator.SetBool("Jump", false);
    }

    //覆蓋狀態
    protected override void CoverUpState(float time, bool isCoverUp, params object[] etc)
    {
        //Debug.Log("覆蓋狀態 - 跳躍");
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
}
