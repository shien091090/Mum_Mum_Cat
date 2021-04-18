//控制遊戲從開始、中斷、關卡更換監聽到結束的流程
//[Partial]監聽提示跳出時機

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    /*
     [Memo]
     (遊戲開始時) "請點擊螢幕"
     (點擊螢幕後餌食落下到一半) "有飼料掉下來了"
     (貓咪吃掉飼料) "貓咪會去追逐掉下來的飼料"
     (過1秒後) "從上面的框格中可以預先看到接下來會落下的飼料"
     (立即) "但有些是不好的飼料, 貓咪吃了會有負面影響"
     (立即, 無聚光燈) "請點擊螢幕"
     (落下後貓咪吃掉飼料後再過1秒) "貓咪吃到不好的飼料後, HP減少了"
     (立即, 無聚光燈) "請盡量誘導貓咪吃該吃的, 避開不該吃的, 照顧好你的貓咪"
     (立即, 無聚光燈) "祝你好運！"
     */

    //提示監聽
    private void HintListen()
    {
        //StartCoroutine(GameStart());
    }

    //遊戲開始時
    public IEnumerator GameStart()
    {
        yield return new WaitForSeconds(3f);

        HintSystem.Instance.Hint("幹你媽~松佳冬杯瓜裡搭");

        isPlaying = false;
        yield return new WaitUntil(() => !HintSystem.Instance.GetHintingState);
        isPlaying = true;
    }
}
